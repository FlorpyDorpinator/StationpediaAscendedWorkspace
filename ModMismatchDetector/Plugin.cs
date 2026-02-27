using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using Assets.Scripts;
using Assets.Scripts.Networking;
using Assets.Scripts.Objects;
using Assets.Scripts.UI;
using Assets.Scripts.Util;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LaunchPadBooster;
using UI;
using UnityEngine;
using UnityEngine.Events;

namespace ModMismatchDetector
{
    [BepInPlugin("com.florpydorp.modmismatchdetector", "Mod Mismatch Detector", "2.0.0")]
    [BepInDependency("launchpad.booster")]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        internal static Plugin Instance;
        private Harmony _harmony;

        // cached mod list for server use (loaded once ModsConfig is available)
        private static List<ModEntry> _cachedServerMods;
        private static bool _cachedServerModsLoaded;

        /// <summary>
        /// Server-side cache: client mod lists received during VerifyPlayer deserialization.
        /// Keyed by ClientId (Steam ID).
        /// </summary>
        internal static readonly Dictionary<ulong, List<ModEntry>> ClientModLists =
            new Dictionary<ulong, List<ModEntry>>();

        /// <summary>Marker prefix injected into rejection messages so the client can identify our custom messages.</summary>
        internal const string MISMATCH_MARKER = "[MODMISMATCH]";

        /// <summary>Protocol version for the appended mod data.</summary>
        internal const byte PROTOCOL_VERSION = 3;

        /// <summary>
        /// Max bytes we can safely append/send. The game's network buffer is only 1024 bytes.
        /// Vanilla VerifyPlayer fields use ~60-100 bytes, Handshake fields ~34 bytes.
        /// We leave generous margin.
        /// </summary>
        internal const int MAX_PAYLOAD_BYTES = 880;
        internal const int MAX_REJECTION_BYTES = 900;

        /// <summary>Set to true when Handshake_Prefix handles a rejection, so ConfirmationPanel patch doesn't double-fire.</summary>
        internal static bool HandshakeHandled;

        // --- IMGUI overlay state ---
        private bool _showOverlay;
        private string _overlayTitle = "";
        private string _overlayBody = "";
        private Vector2 _scrollPos;
        private Action _overlayCloseAction;
        private GUIStyle _boxStyle;
        private GUIStyle _titleStyle;
        private GUIStyle _bodyStyle;
        private GUIStyle _buttonStyle;
        private bool _stylesInitialized;

        /// <summary>Show the scrollable mod-report overlay from any thread.</summary>
        internal static void ShowOverlay(string title, string body, Action onClose = null)
        {
            if (Instance == null) return;
            Instance._overlayTitle = title ?? "";
            Instance._overlayBody = StripRichTextTags(body ?? "");
            Instance._scrollPos = Vector2.zero;
            Instance._overlayCloseAction = onClose;
            Instance._showOverlay = true;
        }

        internal static void HideOverlay()
        {
            if (Instance == null) return;
            Instance._showOverlay = false;
        }

        private static string StripRichTextTags(string input)
        {
            // Remove Unity rich text tags like <color=#FF6666>, </color>, <b>, </b> etc.
            var sb = new StringBuilder(input.Length);
            int i = 0;
            while (i < input.Length)
            {
                if (input[i] == '<')
                {
                    int close = input.IndexOf('>', i);
                    if (close > i)
                    {
                        i = close + 1;
                        continue;
                    }
                }
                sb.Append(input[i]);
                i++;
            }
            return sb.ToString();
        }

        void Awake()
        {
            Instance = this;
            Log = Logger;
            Log.LogInfo("Mod Mismatch Detector v2.0.0 loading...");
            Log.LogInfo(string.Format("IsServer: {0}, IsClient: {1}, IsBatchMode: {2}, NetworkRole: {3}",
                NetworkManager.IsServer, NetworkManager.IsClient, GameManager.IsBatchMode,
                Enum.GetName(typeof(Assets.Scripts.Networking.NetworkRole), Assets.Scripts.Networking.NetworkManager.NetworkRole) ?? "unknown"));

            // Register with Stationeers Launchpad (SLP) as a multiplayer-required mod.
            // This ensures both sides have the mod before VerifyPlayer fires, and applies
            // SLP's Harmony patches on VerifyPlayer/VerifyPlayerRequest so our own patches run reliably.
            var slpMod = new Mod("ModMismatchDetector", "2.0.0");
            slpMod.SetMultiplayerRequired();
            Log.LogInfo("Registered with LaunchPadBooster as MultiplayerRequired mod");

            _harmony = new Harmony("com.florpydorp.modmismatchdetector");

            int success = 0;
            int fail = 0;

            // CLIENT: Append local mod list to VerifyPlayer when connecting
            TryPatch(ref success, ref fail, "VerifyPlayer.Serialize",
                AccessTools.Method(typeof(NetworkMessages.VerifyPlayer), "Serialize"),
                postfix: new HarmonyMethod(typeof(ClientPatches), nameof(ClientPatches.VerifyPlayer_Serialize_Postfix)));

            // SERVER: Read client mod list from VerifyPlayer
            TryPatch(ref success, ref fail, "VerifyPlayer.Deserialize",
                AccessTools.Method(typeof(NetworkMessages.VerifyPlayer), "Deserialize"),
                postfix: new HarmonyMethod(typeof(ServerPatches), nameof(ServerPatches.VerifyPlayer_Deserialize_Postfix)));

            // SERVER: Compare mod lists during verification
            TryPatch(ref success, ref fail, "VerifyConnection",
                AccessTools.Method(typeof(NetworkServer), "VerifyConnection",
                    new Type[] { typeof(long), typeof(NetworkMessages.VerifyPlayer) }),
                prefix: new HarmonyMethod(typeof(ServerPatches), nameof(ServerPatches.VerifyConnection_Prefix)));

            // CLIENT: Intercept rejections to display detailed mismatch info
            TryPatch(ref success, ref fail, "Client.Handshake",
                AccessTools.Method(typeof(NetworkClient), "Handshake",
                    new Type[] { typeof(NetworkMessages.Handshake) }),
                prefix: new HarmonyMethod(typeof(ClientPatches), nameof(ClientPatches.Handshake_Prefix)));

            // CLIENT: Intercept timeout/connection error dialogs to show mod diagnostics
            TryPatch(ref success, ref fail, "ConfirmationPanel.Show",
                AccessTools.Method(typeof(ConfirmationPanel), "Show",
                    new Type[] { typeof(string), typeof(string), typeof(string), typeof(UnityAction),
                                 typeof(string), typeof(UnityAction), typeof(string), typeof(UnityAction), typeof(bool) }),
                prefix: new HarmonyMethod(typeof(ClientPatches), nameof(ClientPatches.ConfirmationPanel_Show_Prefix)));

            Log.LogInfo(string.Format("Mod Mismatch Detector: {0} patches applied, {1} failed", success, fail));

            // Log full local mod list on startup for diagnostics
            LogLocalModList();
        }

        private void LogLocalModList()
        {
            try
            {
                var mods = GetLocalModList();
                Log.LogInfo(string.Format("Game version: {0}", GameManager.GetGameVersion()));
                Log.LogInfo(string.Format("Local prefab checksum: {0}", GetLocalChecksum()));
                Log.LogInfo(string.Format("Enabled mods ({0}):", mods.Count));
                foreach (var mod in mods)
                {
                    string verStr = string.IsNullOrEmpty(mod.Version) ? "" : string.Format(" v{0}", mod.Version);
                    if (mod.WorkshopId != 0)
                        Log.LogInfo(string.Format("  - {0}{1} [Workshop:{2}]", mod.Name, verStr, mod.WorkshopId));
                    else
                        Log.LogInfo(string.Format("  - {0}{1} [Local]", mod.Name, verStr));
                }
            }
            catch (Exception ex)
            {
                Log.LogWarning(string.Format("Could not log mod list on startup: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Compute the local prefab checksum the same way the game does for GameSession.
        /// </summary>
        internal static string GetLocalChecksum()
        {
            try
            {
                int num = Prefab.AllPrefabs.Count.GetHashCode();
                foreach (var thing in Prefab.AllPrefabs)
                    num ^= thing.GetHashCode();
                return num.ToString("X");
            }
            catch
            {
                return "?";
            }
        }

        /// <summary>
        /// Build a snapshot of locally enabled mods from the game's mod config (includes version).
        /// Falls back to reading modconfig.xml directly if WorkshopMenu.ModsConfig is null (common on dedicated servers).
        /// </summary>
        internal static List<ModEntry> GetLocalModList()
        {
            // Return cached list if available (server optimization)
            if (_cachedServerModsLoaded && _cachedServerMods != null)
                return _cachedServerMods;

            var result = new List<ModEntry>();
            try
            {
                var modsConfig = WorkshopMenu.ModsConfig;
                if (modsConfig == null)
                {
                    Log.LogWarning("WorkshopMenu.ModsConfig is null - trying to load modconfig.xml directly...");
                    modsConfig = TryLoadModConfigFromFile();
                }
                if (modsConfig == null)
                {
                    Log.LogError("Could not load mod config from any source!");
                    return result;
                }

                foreach (var mod in modsConfig.GetEnabledMods())
                {
                    if (mod is CoreModData)
                        continue;

                    ulong workshopId = 0;
                    var workshopMod = mod as WorkshopModData;
                    if (workshopMod != null && workshopMod.WorkshopId != null)
                        workshopId = workshopMod.WorkshopId.Value;

                    string name = "Unknown";
                    string version = "";
                    try
                    {
                        var about = mod.GetAboutData();
                        if (about != null)
                        {
                            if (!string.IsNullOrEmpty(about.Name))
                                name = about.Name;
                            if (!string.IsNullOrEmpty(about.Version))
                                version = about.Version;
                        }
                    }
                    catch { }

                    result.Add(new ModEntry { WorkshopId = workshopId, Name = name, Version = version });
                }

                // Cache the result for server-side use
                _cachedServerMods = result;
                _cachedServerModsLoaded = true;
            }
            catch (Exception ex)
            {
                Log.LogError(string.Format("Failed to get mod list: {0}\n{1}", ex.Message, ex.StackTrace));
            }
            return result;
        }

        /// <summary>
        /// Fallback: directly deserialize modconfig.xml when WorkshopMenu.ModsConfig is unavailable.
        /// Tries multiple paths where the config could live.
        /// </summary>
        private static ModConfig TryLoadModConfigFromFile()
        {
            var paths = new List<string>();
            try
            {
                // Primary: StationSaveUtils.DefaultPath (handles batch mode vs client)
                paths.Add(Path.Combine(StationSaveUtils.DefaultPath, "modconfig.xml"));
            }
            catch { }
            try
            {
                // Fallback: game exe directory
                var exeDir = new DirectoryInfo(UnityEngine.Application.dataPath).Parent;
                if (exeDir != null)
                    paths.Add(Path.Combine(exeDir.FullName, "modconfig.xml"));
            }
            catch { }
            try
            {
                // Fallback: user documents
                paths.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "My Games", "Stationeers", "modconfig.xml"));
            }
            catch { }

            foreach (var path in paths)
            {
                try
                {
                    if (File.Exists(path))
                    {
                        Log.LogInfo(string.Format("Found modconfig.xml at: {0}", path));
                        var serializer = new XmlSerializer(typeof(ModConfig));
                        using (var stream = File.OpenRead(path))
                        {
                            var config = serializer.Deserialize(stream) as ModConfig;
                            if (config != null)
                            {
                                Log.LogInfo(string.Format("Loaded {0} mods from modconfig.xml", config.Mods.Count));
                                return config;
                            }
                        }
                    }
                    else
                    {
                        Log.LogInfo(string.Format("No modconfig.xml at: {0}", path));
                    }
                }
                catch (Exception ex)
                {
                    Log.LogWarning(string.Format("Failed to load modconfig.xml from {0}: {1}", path, ex.Message));
                }
            }
            return null;
        }

        /// <summary>
        /// Build a formatted text block listing all local mods (for client-only diagnostics).
        /// </summary>
        internal static string FormatLocalModList()
        {
            var mods = GetLocalModList();
            var sb = new StringBuilder();
            sb.AppendFormat("<color=#AAAAAA>Your game version: {0}</color>\n", GameManager.GetGameVersion());
            sb.AppendFormat("<color=#AAAAAA>Your prefab checksum: {0}</color>\n", GetLocalChecksum());
            sb.AppendFormat("\n<b>YOUR ENABLED MODS ({0}):</b>\n", mods.Count);
            if (mods.Count == 0)
            {
                sb.Append("  (none)\n");
            }
            else
            {
                foreach (var mod in mods)
                {
                    string verStr = string.IsNullOrEmpty(mod.Version) ? "" : string.Format(" v{0}", mod.Version);
                    if (mod.WorkshopId != 0)
                        sb.AppendFormat("  \u2022 {0}{1}  <color=#888888>[{2}]</color>\n",
                            mod.Name, verStr, mod.WorkshopId);
                    else
                        sb.AppendFormat("  \u2022 {0}{1}  <color=#888888>[Local]</color>\n",
                            mod.Name, verStr);
                }
            }
            return sb.ToString();
        }

        private void TryPatch(ref int success, ref int fail, string name,
            MethodInfo original, HarmonyMethod prefix = null, HarmonyMethod postfix = null)
        {
            try
            {
                if (original == null)
                {
                    Log.LogError(string.Format("  [{0}] FAILED - target method not found", name));
                    fail++;
                    return;
                }
                _harmony.Patch(original, prefix: prefix, postfix: postfix);
                Log.LogInfo(string.Format("  [{0}] OK", name));
                success++;
            }
            catch (Exception ex)
            {
                Log.LogError(string.Format("  [{0}] FAILED - {1}: {2}", name, ex.GetType().Name, ex.Message));
                fail++;
            }
        }

        void OnDestroy()
        {
            _harmony?.UnpatchSelf();
            if (Instance == this) Instance = null;
        }

        private void InitStyles()
        {
            if (_stylesInitialized) return;
            _stylesInitialized = true;

            _boxStyle = new GUIStyle(GUI.skin.box);
            _boxStyle.normal.background = MakeTex(2, 2, new Color(0.08f, 0.08f, 0.12f, 0.97f));

            _titleStyle = new GUIStyle(GUI.skin.label);
            _titleStyle.fontSize = 22;
            _titleStyle.fontStyle = FontStyle.Bold;
            _titleStyle.normal.textColor = new Color(1f, 0.48f, 0.1f); // orange
            _titleStyle.alignment = TextAnchor.MiddleCenter;
            _titleStyle.wordWrap = true;

            _bodyStyle = new GUIStyle(GUI.skin.label);
            _bodyStyle.fontSize = 15;
            _bodyStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f);
            _bodyStyle.wordWrap = true;
            _bodyStyle.richText = false;

            _buttonStyle = new GUIStyle(GUI.skin.button);
            _buttonStyle.fontSize = 18;
            _buttonStyle.fixedHeight = 40;
            _buttonStyle.normal.textColor = Color.white;
        }

        private static Texture2D MakeTex(int w, int h, Color col)
        {
            var pix = new Color[w * h];
            for (int i = 0; i < pix.Length; i++) pix[i] = col;
            var tex = new Texture2D(w, h);
            tex.SetPixels(pix);
            tex.Apply();
            return tex;
        }

        void OnGUI()
        {
            if (!_showOverlay) return;
            InitStyles();

            float pad = 40f;
            float winW = Screen.width - pad * 2;
            float winH = Screen.height - pad * 2;
            Rect windowRect = new Rect(pad, pad, winW, winH);

            // Dim background
            GUI.color = new Color(0, 0, 0, 0.6f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUI.Box(windowRect, "", _boxStyle);

            float innerPad = 20f;
            float btnH = 44f;
            float btnAreaH = btnH + 10f;

            // Title
            Rect titleRect = new Rect(windowRect.x + innerPad, windowRect.y + innerPad,
                winW - innerPad * 2, 30f);
            GUI.Label(titleRect, _overlayTitle, _titleStyle);

            // Scrollable body
            float bodyTop = titleRect.yMax + 10f;
            float bodyH = winH - (bodyTop - windowRect.y) - btnAreaH - innerPad;
            Rect bodyOuterRect = new Rect(windowRect.x + innerPad, bodyTop, winW - innerPad * 2, bodyH);

            GUIContent bodyContent = new GUIContent(_overlayBody);
            float textHeight = _bodyStyle.CalcHeight(bodyContent, bodyOuterRect.width - 20f);
            Rect bodyInnerRect = new Rect(0, 0, bodyOuterRect.width - 20f, Mathf.Max(textHeight, bodyH));

            _scrollPos = GUI.BeginScrollView(bodyOuterRect, _scrollPos, bodyInnerRect);
            GUI.Label(bodyInnerRect, _overlayBody, _bodyStyle);
            GUI.EndScrollView();

            // Close button
            float btnW = 200f;
            Rect btnRect = new Rect(windowRect.x + (winW - btnW) / 2f,
                windowRect.yMax - btnAreaH - innerPad + 10f, btnW, btnH);
            if (GUI.Button(btnRect, "Close", _buttonStyle))
            {
                _showOverlay = false;
                var closeAction = _overlayCloseAction;
                _overlayCloseAction = null;
                if (closeAction != null) closeAction();
            }
        }
    }

    /// <summary>Lightweight struct representing a single mod for comparison.</summary>
    public struct ModEntry
    {
        public ulong WorkshopId;
        public string Name;
        public string Version;
    }

    // =========================================================================
    // CLIENT-SIDE PATCHES
    // =========================================================================
    public static class ClientPatches
    {
        /// <summary>
        /// Postfix on VerifyPlayer.Serialize: appends our mod list after the
        /// vanilla fields so the server can read it.
        /// Protocol v3: uses a single compact string instead of per-mod binary fields
        /// to stay within the game's 1024-byte network buffer limit.
        /// </summary>
        public static void VerifyPlayer_Serialize_Postfix(RocketBinaryWriter writer)
        {
            try
            {
                var mods = Plugin.GetLocalModList();
                Plugin.Log.LogInfo(string.Format("[CLIENT] Appending {0} mods to VerifyPlayer (compact)...", mods.Count));

                // Write magic marker so server knows we appended data
                writer.WriteString(Plugin.MISMATCH_MARKER);
                writer.WriteByte(Plugin.PROTOCOL_VERSION);

                // Build compact payload: "name\tversion\nname\tversion\n..."
                // The entire network message must fit in 1024 bytes, so we
                // measure as we go and truncate if needed.
                var sb = new StringBuilder();
                int included = 0;
                foreach (var mod in mods)
                {
                    string entry = string.Format("{0}\t{1}\n", mod.Name ?? "", mod.Version ?? "");
                    if (Encoding.UTF8.GetByteCount(sb.ToString()) + Encoding.UTF8.GetByteCount(entry) > Plugin.MAX_PAYLOAD_BYTES - 20)
                    {
                        int remaining = mods.Count - included;
                        if (remaining > 0)
                            sb.AppendFormat("+{0}\n", remaining);
                        break;
                    }
                    sb.Append(entry);
                    included++;
                }

                string payload = sb.ToString();
                writer.WriteString(payload);

                Plugin.Log.LogInfo(string.Format("Sent {0}/{1} mod entries ({2} bytes) to server",
                    included, mods.Count, Encoding.UTF8.GetByteCount(payload)));
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError(string.Format("Failed to append mod list: {0}", ex.Message));
            }
        }

        /// <summary>
        /// Prefix on NetworkClient.Handshake: intercepts rejection messages.
        /// - If the server has our mod: parses compact mismatch format and shows detailed comparison.
        /// - If the server does NOT have our mod: enhances the vanilla rejection
        ///   with your local mod list for manual troubleshooting.
        /// </summary>
        public static bool Handshake_Prefix(NetworkMessages.Handshake handshake)
        {
            try
            {
                if (handshake.HandshakeState != HandshakeType.Rejected)
                    return true;

                // Flag so ConfirmationPanel.Show prefix doesn't double-fire
                Plugin.HandshakeHandled = true;

                // --- Case 1: Server has our mod and sent compact mismatch data ---
                if (!string.IsNullOrEmpty(handshake.Message) &&
                    handshake.Message.StartsWith(Plugin.MISMATCH_MARKER))
                {
                    string data = handshake.Message.Substring(Plugin.MISMATCH_MARKER.Length);
                    Plugin.Log.LogWarning(string.Format("Mod mismatch detected (compact):\n{0}", data));

                    // Parse compact format: sections separated by \n
                    // M:name1|name2  (missing on client)
                    // E:name1|name2  (extra on client)
                    // V:name=sVer/cVer|name=sVer/cVer  (version mismatches)
                    // C:serverCount,clientCount
                    var missing = new List<string>();
                    var extra = new List<string>();
                    var verMismatch = new List<string>();
                    int serverCount = 0, clientCount = 0;
                    bool truncated = false;

                    foreach (var line in data.Split('\n'))
                    {
                        if (string.IsNullOrEmpty(line)) continue;
                        if (line.StartsWith("M:"))
                            missing.AddRange(line.Substring(2).Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries));
                        else if (line.StartsWith("E:"))
                            extra.AddRange(line.Substring(2).Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries));
                        else if (line.StartsWith("V:"))
                            verMismatch.AddRange(line.Substring(2).Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries));
                        else if (line.StartsWith("C:"))
                        {
                            var counts = line.Substring(2).Split(',');
                            if (counts.Length >= 2)
                            {
                                int.TryParse(counts[0], out serverCount);
                                int.TryParse(counts[1], out clientCount);
                            }
                        }
                        else if (line.StartsWith("T:"))
                            truncated = true;
                    }

                    // Build formatted display
                    var displaySb = new StringBuilder();
                    if (missing.Count > 0)
                    {
                        displaySb.Append("YOU ARE MISSING (server requires):\n");
                        foreach (var name in missing)
                            displaySb.AppendFormat("  \u2022 {0}\n", name);
                    }
                    if (extra.Count > 0)
                    {
                        displaySb.Append("\nYOU HAVE EXTRA (server doesn't have):\n");
                        foreach (var name in extra)
                            displaySb.AppendFormat("  \u2022 {0}\n", name);
                    }
                    if (verMismatch.Count > 0)
                    {
                        displaySb.Append("\nVERSION MISMATCH:\n");
                        foreach (var vm in verMismatch)
                        {
                            // Format: "name=serverVer/clientVer"
                            int eq = vm.IndexOf('=');
                            if (eq > 0)
                            {
                                string modName = vm.Substring(0, eq);
                                string vers = vm.Substring(eq + 1);
                                int slash = vers.IndexOf('/');
                                if (slash > 0)
                                    displaySb.AppendFormat("  \u2022 {0}: server v{1}, you have v{2}\n",
                                        modName, vers.Substring(0, slash), vers.Substring(slash + 1));
                                else
                                    displaySb.AppendFormat("  \u2022 {0}\n", vm);
                            }
                            else
                                displaySb.AppendFormat("  \u2022 {0}\n", vm);
                        }
                    }
                    displaySb.AppendFormat("\nServer has {0} mods, you have {1} mods", serverCount, clientCount);
                    if (truncated)
                        displaySb.Append("\n(List was truncated due to message size limits)");

                    NetworkClient.StopConnectionTimer();
                    Plugin.ShowOverlay("MOD MISMATCH DETECTED", displaySb.ToString(), () => NetworkClient.Cancel());
                    NetworkManager.EndConnection();
                    return false;
                }

                // --- Case 2: Vanilla rejection - enhance with local mod diagnostics ---
                string reason = handshake.Message ?? "";
                var sb = new StringBuilder();

                // Translate common rejection keys to human-readable text
                if (reason == "MultiplayerIncorrectVersion" || reason == NetworkClient.MultiplayerIncorrectVersionKey)
                {
                    sb.AppendFormat("<color=#FF6666><b>GAME VERSION MISMATCH</b></color>\n");
                    sb.AppendFormat("Your version: <b>{0}</b>\n", GameManager.GetGameVersion());
                    sb.Append("The server is running a different game version.\n");
                }
                else if (reason == "MultiplayerBanned" || reason == NetworkClient.MultiplayerBannedKey)
                {
                    sb.Append("<color=#FF6666><b>YOU ARE BANNED</b></color>\n");
                    sb.Append("You have been banned from this server.\n");
                }
                else if (reason == "MultiplayerPassword" || reason == NetworkClient.MultiplayerPasswordKey)
                {
                    sb.Append("<color=#FFAA44><b>INCORRECT PASSWORD</b></color>\n");
                }
                else
                {
                    sb.AppendFormat("<color=#FF6666><b>CONNECTION REJECTED</b></color>\n");
                    sb.AppendFormat("Reason: {0}\n", reason);
                }

                // Always append local mod list so the user can share it
                sb.Append("\n");
                sb.Append(Plugin.FormatLocalModList());
                sb.Append("\n<color=#888888>Share this info with the server admin to diagnose issues.</color>");
                sb.Append("\n<color=#888888>(Server does not have Mod Mismatch Detector - install it on both sides for exact comparison)</color>");

                string enhanced = sb.ToString();
                Plugin.Log.LogWarning(string.Format("Connection rejected. Enhanced info:\n{0}", enhanced));

                NetworkClient.StopConnectionTimer();
                Plugin.ShowOverlay("CONNECTION REJECTED", enhanced, () => NetworkClient.Cancel());
                NetworkManager.EndConnection();
                return false;
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError(string.Format("Handshake intercept error: {0}", ex.Message));
                return true;
            }
        }

        /// <summary>
        /// Prefix on ConfirmationPanel.Show: intercepts multiplayer connection error dialogs
        /// (timeouts, address errors) and enhances them with mod diagnostic information.
        /// This covers the case where the connection times out rather than being explicitly rejected.
        /// </summary>
        public static bool ConfirmationPanel_Show_Prefix(
            ConfirmationPanel __instance,
            string titleKey, string messageKey,
            string button1Key, UnityAction button1OnClick,
            string button2Key, UnityAction button2OnClick,
            string button3Key, UnityAction button3OnClick,
            bool closeOnEscape)
        {
            try
            {
                // Don't intercept if Handshake_Prefix already handled this connection attempt
                if (Plugin.HandshakeHandled)
                {
                    Plugin.HandshakeHandled = false;
                    return true;
                }

                // Only intercept multiplayer connection error dialogs
                if (titleKey != "MultiplayerCouldNotConnect" &&
                    titleKey != "MultiplayerIncorrectVersion")
                    return true;

                var sb = new StringBuilder();
                sb.Append("<color=#FF6666><b>CONNECTION FAILED</b></color>\n\n");
                sb.Append("The server did not explicitly reject you. Possible causes:\n");
                sb.Append("  \u2022 Mod mismatch (different mods or versions than server)\n");
                sb.Append("  \u2022 Network/firewall issue\n");
                sb.Append("  \u2022 Server not responding or full\n\n");
                sb.Append(Plugin.FormatLocalModList());
                sb.Append("\n<color=#888888>Compare your mod list with the server's to find differences.</color>");
                sb.Append("\n<color=#888888>(Install Mod Mismatch Detector on the server for automatic comparison)</color>");

                string enhanced = sb.ToString();
                Plugin.Log.LogWarning("Connection failed (timeout/error). Showing mod diagnostics.");

                Plugin.ShowOverlay("CONNECTION FAILED", enhanced, () =>
                {
                    if (button1OnClick != null) button1OnClick.Invoke();
                });
                return false;
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError(string.Format("ConfirmationPanel intercept error: {0}", ex.Message));
                return true;
            }
        }
    }

    // =========================================================================
    // SERVER-SIDE PATCHES
    // =========================================================================
    public static class ServerPatches
    {
        /// <summary>
        /// Postfix on VerifyPlayer.Deserialize: tries to read the appended mod
        /// list. Supports protocol v2 (binary) and v3 (compact string).
        /// If the client doesn't have our mod the extra bytes won't exist
        /// and we silently skip.
        /// </summary>
        public static void VerifyPlayer_Deserialize_Postfix(
            NetworkMessages.VerifyPlayer __instance, RocketBinaryReader reader)
        {
            try
            {
                Plugin.Log.LogInfo(string.Format("[SERVER] VerifyPlayer deserialized for client {0}, checking for mod data...", __instance.ClientId));
                string marker = reader.ReadString();
                if (marker != Plugin.MISMATCH_MARKER)
                    return;

                byte protocolVer = reader.ReadByte();
                List<ModEntry> clientMods;

                if (protocolVer >= 3)
                {
                    // v3 compact format: single payload string "name\tversion\nname\tversion\n..."
                    string payload = reader.ReadString();
                    clientMods = ParseModPayload(payload);
                }
                else
                {
                    // v2 binary format: count + per-entry fields
                    int count = reader.ReadInt32();
                    clientMods = new List<ModEntry>(count);
                    for (int i = 0; i < count; i++)
                    {
                        var entry = new ModEntry
                        {
                            WorkshopId = reader.ReadUInt64(),
                            Name = reader.ReadString()
                        };
                        if (protocolVer >= 2)
                            entry.Version = reader.ReadString();
                        clientMods.Add(entry);
                    }
                }

                Plugin.ClientModLists[__instance.ClientId] = clientMods;
                Plugin.Log.LogInfo(string.Format(
                    "Received {0} mod entries (protocol v{1}) from client {2}",
                    clientMods.Count, protocolVer, __instance.ClientId));
            }
            catch (EndOfStreamException)
            {
                // Client doesn't have our mod - no appended data. This is expected and normal.
                Plugin.Log.LogInfo(string.Format("[SERVER] Client {0} has no mod data appended (mod not installed on client)", __instance.ClientId));
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError(string.Format("[SERVER] Error reading mod data from client {0}: {1}\n{2}", __instance.ClientId, ex.Message, ex.StackTrace));
            }
        }

        /// <summary>Parse compact mod payload: "name\tversion\nname\tversion\n..."</summary>
        private static List<ModEntry> ParseModPayload(string payload)
        {
            var result = new List<ModEntry>();
            if (string.IsNullOrEmpty(payload)) return result;

            foreach (var line in payload.Split('\n'))
            {
                if (string.IsNullOrEmpty(line)) continue;
                if (line.StartsWith("+")) continue; // truncation marker

                var parts = line.Split(new[] { '\t' }, 2);
                result.Add(new ModEntry
                {
                    Name = parts[0],
                    Version = parts.Length > 1 ? parts[1] : ""
                });
            }
            return result;
        }

        /// <summary>
        /// Prefix on NetworkServer.VerifyConnection: if we have the client's
        /// mod list, compare it against the server's mods (including versions).
        /// On mismatch, send a compact rejection that fits within the game's
        /// 1024-byte network buffer limit.
        /// </summary>
        public static bool VerifyConnection_Prefix(long hostId, NetworkMessages.VerifyPlayer msg)
        {
            try
            {
                Plugin.Log.LogInfo(string.Format("[SERVER] VerifyConnection called for client {0} ({1})", msg.ClientId, msg.Name));

                // Only do the mod-list check if the client sent us their mods
                List<ModEntry> clientMods;
                if (!Plugin.ClientModLists.TryGetValue(msg.ClientId, out clientMods))
                {
                    Plugin.Log.LogInfo(string.Format("[SERVER] Client {0} did not send mod list (mod not installed on client), proceeding normally", msg.ClientId));
                    return true; // client doesn't have our mod, proceed normally
                }

                Plugin.ClientModLists.Remove(msg.ClientId);
                Plugin.Log.LogInfo(string.Format("[SERVER] Client {0} sent {1} mods, comparing...", msg.ClientId, clientMods.Count));

                var serverMods = Plugin.GetLocalModList();
                Plugin.Log.LogInfo(string.Format("[SERVER] Server has {0} mods", serverMods.Count));

                // Log both lists for debugging
                foreach (var m in serverMods)
                    Plugin.Log.LogInfo(string.Format("[SERVER]   Server mod: {0} v{1} [ws:{2}]", m.Name, m.Version, m.WorkshopId));
                foreach (var m in clientMods)
                    Plugin.Log.LogInfo(string.Format("[SERVER]   Client mod: {0} v{1}", m.Name, m.Version));

                // --- Name-based matching (primary strategy) ---
                var serverByName = new Dictionary<string, ModEntry>(StringComparer.OrdinalIgnoreCase);
                foreach (var m in serverMods)
                    if (!string.IsNullOrEmpty(m.Name) && m.Name != "Unknown")
                        serverByName[m.Name] = m;

                var clientByName = new Dictionary<string, ModEntry>(StringComparer.OrdinalIgnoreCase);
                foreach (var m in clientMods)
                    if (!string.IsNullOrEmpty(m.Name) && m.Name != "Unknown")
                        clientByName[m.Name] = m;

                var missingOnClient = new List<ModEntry>();
                var extraOnClient = new List<ModEntry>();
                var versionMismatches = new List<VersionMismatch>();

                // Find server mods missing on client, and version mismatches
                foreach (var kvp in serverByName)
                {
                    ModEntry clientEntry;
                    if (!clientByName.TryGetValue(kvp.Key, out clientEntry))
                    {
                        missingOnClient.Add(kvp.Value);
                        Plugin.Log.LogInfo(string.Format("[SERVER]   MISSING on client: {0}", kvp.Key));
                    }
                    else if (!string.IsNullOrEmpty(kvp.Value.Version) &&
                             !string.IsNullOrEmpty(clientEntry.Version) &&
                             kvp.Value.Version != clientEntry.Version)
                    {
                        versionMismatches.Add(new VersionMismatch
                        {
                            Mod = kvp.Value,
                            ClientVersion = clientEntry.Version
                        });
                        Plugin.Log.LogInfo(string.Format("[SERVER]   VERSION MISMATCH: {0} server={1} client={2}", kvp.Key, kvp.Value.Version, clientEntry.Version));
                    }
                    else
                    {
                        Plugin.Log.LogInfo(string.Format("[SERVER]   MATCH: {0}", kvp.Key));
                    }
                }

                // Find client mods not on server
                foreach (var kvp in clientByName)
                {
                    if (!serverByName.ContainsKey(kvp.Key))
                    {
                        extraOnClient.Add(kvp.Value);
                        Plugin.Log.LogInfo(string.Format("[SERVER]   EXTRA on client: {0}", kvp.Key));
                    }
                }

                if (missingOnClient.Count == 0 && extraOnClient.Count == 0 && versionMismatches.Count == 0)
                {
                    Plugin.Log.LogInfo(string.Format(
                        "Mod lists match for client {0} ({1} mods each, versions OK)",
                        msg.ClientId, serverMods.Count));
                    return true; // all good, let normal verification proceed
                }

                // --- Build compact rejection message ---
                // Format: [MODMISMATCH]\nM:name1|name2\nE:name3\nV:name=sv/cv\nC:sCount,cCount
                // Must stay under MAX_REJECTION_BYTES to fit in the 1024-byte network buffer.
                string fullMessage = BuildCompactRejection(
                    missingOnClient, extraOnClient, versionMismatches,
                    serverMods.Count, clientMods.Count);

                Plugin.Log.LogWarning(string.Format(
                    "Mod mismatch for client {0}: {1} missing, {2} extra, {3} ver mismatch. Message={4} bytes",
                    msg.ClientId, missingOnClient.Count, extraOnClient.Count,
                    versionMismatches.Count, Encoding.UTF8.GetByteCount(fullMessage)));

                // Send rejection
                var client = new Client(hostId, msg.OwnerConnectionId, msg.ClientId,
                    msg.Name, msg.ClientConnectionMethod);

                NetworkServer.SendToClient<NetworkMessages.Handshake>(
                    new NetworkMessages.Handshake
                    {
                        HandshakeState = HandshakeType.Rejected,
                        Message = fullMessage
                    },
                    NetworkChannel.GeneralTraffic, client);

                return false; // skip original VerifyConnection
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError(string.Format(
                    "Mod check error (allowing connection): {0}\n{1}", ex.Message, ex.StackTrace));
                return true; // on error, don't block the connection
            }
        }

        /// <summary>
        /// Build a compact rejection message that stays under the byte limit.
        /// Format: [MODMISMATCH]\nM:name1|name2\nE:name3\nV:name=sv/cv\nC:s,c
        /// Progressively truncates entries if the message would exceed the limit.
        /// </summary>
        private static string BuildCompactRejection(
            List<ModEntry> missing, List<ModEntry> extra,
            List<VersionMismatch> verMismatches,
            int serverCount, int clientCount)
        {
            var sb = new StringBuilder();
            sb.Append(Plugin.MISMATCH_MARKER);
            bool wasTruncated = false;

            // Missing mods section
            if (missing.Count > 0)
            {
                sb.Append("\nM:");
                int added = 0;
                foreach (var mod in missing)
                {
                    string entry = (added > 0 ? "|" : "") + mod.Name;
                    if (Encoding.UTF8.GetByteCount(sb.ToString()) + Encoding.UTF8.GetByteCount(entry) > Plugin.MAX_REJECTION_BYTES - 100)
                    {
                        sb.AppendFormat("|+{0}", missing.Count - added);
                        wasTruncated = true;
                        break;
                    }
                    sb.Append(entry);
                    added++;
                }
            }

            // Extra mods section
            if (extra.Count > 0)
            {
                sb.Append("\nE:");
                int added = 0;
                foreach (var mod in extra)
                {
                    string entry = (added > 0 ? "|" : "") + mod.Name;
                    if (Encoding.UTF8.GetByteCount(sb.ToString()) + Encoding.UTF8.GetByteCount(entry) > Plugin.MAX_REJECTION_BYTES - 60)
                    {
                        sb.AppendFormat("|+{0}", extra.Count - added);
                        wasTruncated = true;
                        break;
                    }
                    sb.Append(entry);
                    added++;
                }
            }

            // Version mismatch section
            if (verMismatches.Count > 0)
            {
                sb.Append("\nV:");
                int added = 0;
                foreach (var vm in verMismatches)
                {
                    string entry = string.Format("{0}{1}={2}/{3}",
                        added > 0 ? "|" : "", vm.Mod.Name, vm.Mod.Version, vm.ClientVersion);
                    if (Encoding.UTF8.GetByteCount(sb.ToString()) + Encoding.UTF8.GetByteCount(entry) > Plugin.MAX_REJECTION_BYTES - 30)
                    {
                        sb.AppendFormat("|+{0}", verMismatches.Count - added);
                        wasTruncated = true;
                        break;
                    }
                    sb.Append(entry);
                    added++;
                }
            }

            // Counts
            sb.AppendFormat("\nC:{0},{1}", serverCount, clientCount);
            if (wasTruncated)
                sb.Append("\nT:1");

            return sb.ToString();
        }
    }

    internal struct VersionMismatch
    {
        public ModEntry Mod;        // server-side entry (has server version)
        public string ClientVersion; // client's version of same mod
    }
}
