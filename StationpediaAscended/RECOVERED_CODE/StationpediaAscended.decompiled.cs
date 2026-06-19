using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Text;
using System.Text.RegularExpressions;
using Assets.Scripts;
using Assets.Scripts.Inventory;
using Assets.Scripts.Networking;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Serialization;
using Assets.Scripts.UI;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LeTai.Asset.TranslucentImage;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StationpediaAscended.Data;
using StationpediaAscended.Diagnostics;
using StationpediaAscended.Patches;
using StationpediaAscended.Tooltips;
using StationpediaAscended.UI;
using StationpediaAscended.UI.StationPlanner;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Video;
using Util.Commands;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.Default | DebuggableAttribute.DebuggingModes.DisableOptimizations | DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints | DebuggableAttribute.DebuggingModes.EnableEditAndContinue)]
[assembly: TargetFramework(".NETFramework,Version=v4.8", FrameworkDisplayName = ".NET Framework 4.8")]
[assembly: AssemblyCompany("StationpediaAscended")]
[assembly: AssemblyConfiguration("Debug")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0+92b728dc3028625afe75dcb2bcd4707b3613eb53")]
[assembly: AssemblyProduct("StationpediaAscended")]
[assembly: AssemblyTitle("StationpediaAscended")]
[assembly: AssemblyVersion("1.0.0.0")]
[module: RefSafetyRules(11)]
namespace Microsoft.CodeAnalysis
{
	[CompilerGenerated]
	[Embedded]
	internal sealed class EmbeddedAttribute : Attribute
	{
	}
}
namespace System.Runtime.CompilerServices
{
	[CompilerGenerated]
	[Embedded]
	[AttributeUsage(AttributeTargets.Module, AllowMultiple = false, Inherited = false)]
	internal sealed class RefSafetyRulesAttribute : Attribute
	{
		public readonly int Version;

		public RefSafetyRulesAttribute(int P_0)
		{
			Version = P_0;
		}
	}
}
namespace StationpediaAscended
{
	[BepInPlugin("com.florpydorp.stationpediaascended", "Stationpedia Ascended", "0.3.0")]
	public class StationpediaAscendedMod : BaseUnityPlugin
	{
		[Serializable]
		[CompilerGenerated]
		private sealed class <>c
		{
			public static readonly <>c <>9 = new <>c();

			public static UnityAction <>9__66_0;

			public static Comparison<StationpediaPage> <>9__84_0;

			internal void <SetupStationPlannerButton>b__66_0()
			{
				StationPlannerWindow.Toggle();
			}

			internal int <DumpPageKeysCommand>b__84_0(StationpediaPage a, StationpediaPage b)
			{
				return string.Compare(a.Key, b.Key, StringComparison.OrdinalIgnoreCase);
			}
		}

		public const string PluginGuid = "com.florpydorp.stationpediaascended";

		public const string PluginName = "Stationpedia Ascended";

		public const string PluginVersion = "0.3.0";

		public const string HarmonyId = "com.stationpediaascended.mod";

		internal static ManualLogSource Log;

		private string _lastPageKey = "";

		private bool _stationpediaFound = false;

		private const float CHECK_INTERVAL = 0.1f;

		private Coroutine _monitorCoroutine;

		private GUIStyle _tooltipStyle;

		private bool _stylesInitialized = false;

		private Texture2D _tooltipBackground;

		private Harmony _harmony;

		internal static Sprite _customIconSprite;

		internal static Sprite _iconExpanded;

		internal static Sprite _iconCollapsed;

		internal static List<GameObject> _createdGameObjects = new List<GameObject>();

		internal static List<Component> _addedComponents = new List<Component>();

		private bool _initialized = false;

		private static MonoBehaviour _scriptEngineHost;

		private static bool _hiddenItemsPopulated = false;

		private static Button _stationPlannerButton = null;

		private static Sprite _stationPlannerIconSprite = null;

		private static Harmony _harmonyStatic;

		private static GameObject _headerTitleObject = null;

		private static TextMeshProUGUI _headerTitleText = null;

		private static Image _headerIconImage = null;

		private static Button _headerButton = null;

		private static Sprite _originalHeaderIconSprite = null;

		private static bool _stationPlannerButtonCreated = false;

		private static Sprite _roundedBgSprite = null;

		private static Sprite _windowBgSprite = null;

		private static string _lastPageKeyStatic = "";

		private static bool _stationpediaFoundStatic = false;

		public static StationpediaAscendedMod Instance { get; private set; }

		public static Dictionary<string, DeviceDescriptions> DeviceDatabase { get; private set; }

		public static GenericDescriptionsData GenericDescriptions { get; private set; }

		public static string CurrentTooltipText { get; set; } = "";

		public static bool ShowTooltip { get; set; } = false;

		public static void InitializeFromScriptEngine(MonoBehaviour host, ManualLogSource log)
		{
			Log = log;
			_scriptEngineHost = host;
			CleanupExistingTooltips();
			LoadDescriptionsStatic();
			ApplyHarmonyPatchesStatic();
			_lastPageKeyStatic = "";
			_stationpediaFoundStatic = false;
			if ((Object)(object)Stationpedia.Instance != (Object)null)
			{
				try
				{
					Stationpedia.Regenerate();
				}
				catch (Exception arg)
				{
					Log.LogError((object)$"Failed to regenerate Stationpedia: {arg}");
				}
			}
			host.StartCoroutine(MonitorStationpediaCoroutineStatic());
			ConsoleWindow.Print("[Stationpedia Ascended] v0.3.0 initialized via ScriptEngine!", ConsoleColor.White, false, true);
		}

		private static void CleanupExistingTooltips()
		{
			int num = 0;
			GameObject[] array = Object.FindObjectsOfType<GameObject>();
			foreach (GameObject val in array)
			{
				Component[] components = val.GetComponents<Component>();
				Component[] array2 = components;
				foreach (Component val2 in array2)
				{
					if (!((Object)(object)val2 != (Object)null))
					{
						continue;
					}
					string name = ((object)val2).GetType().Name;
					switch (name)
					{
					default:
						if (!(name == "SELogicTooltip"))
						{
							continue;
						}
						break;
					case "SPDALogicTooltip":
					case "SPDASlotTooltip":
					case "SPDAVersionTooltip":
					case "SPDAMemoryTooltip":
						break;
					}
					try
					{
						Object.Destroy((Object)(object)val2);
						num++;
					}
					catch
					{
					}
				}
			}
			_addedComponents.Clear();
			_createdGameObjects.Clear();
		}

		public static void CleanupFromScriptEngine()
		{
			if (_harmonyStatic != null)
			{
				_harmonyStatic.UnpatchSelf();
				_harmonyStatic = null;
			}
			foreach (Component addedComponent in _addedComponents)
			{
				if ((Object)(object)addedComponent != (Object)null)
				{
					try
					{
						Object.Destroy((Object)(object)addedComponent);
					}
					catch
					{
					}
				}
			}
			_addedComponents.Clear();
			foreach (GameObject createdGameObject in _createdGameObjects)
			{
				if ((Object)(object)createdGameObject != (Object)null)
				{
					try
					{
						Object.Destroy((Object)(object)createdGameObject);
					}
					catch
					{
					}
				}
			}
			_createdGameObjects.Clear();
			ShowTooltip = false;
			CurrentTooltipText = "";
			DeviceDatabase = null;
			GenericDescriptions = null;
			_scriptEngineHost = null;
			ConsoleWindow.Print("[Stationpedia Ascended] Cleaned up", ConsoleColor.White, false, true);
		}

		public void TriggerReload()
		{
			CleanupForReload();
			_initialized = false;
			Initialize();
		}

		private void CleanupForReload()
		{
			if (_monitorCoroutine != null)
			{
				((MonoBehaviour)this).StopCoroutine(_monitorCoroutine);
				_monitorCoroutine = null;
			}
			if (_harmony != null)
			{
				_harmony.UnpatchSelf();
				_harmony = null;
			}
			foreach (Component addedComponent in _addedComponents)
			{
				if ((Object)(object)addedComponent != (Object)null)
				{
					try
					{
						Object.Destroy((Object)(object)addedComponent);
					}
					catch
					{
					}
				}
			}
			_addedComponents.Clear();
			foreach (GameObject createdGameObject in _createdGameObjects)
			{
				if ((Object)(object)createdGameObject != (Object)null)
				{
					try
					{
						Object.Destroy((Object)(object)createdGameObject);
					}
					catch
					{
					}
				}
			}
			_createdGameObjects.Clear();
			_lastPageKey = "";
			_stationpediaFound = false;
			ShowTooltip = false;
			CurrentTooltipText = "";
		}

		private void Awake()
		{
			try
			{
				Log = Logger.CreateLogSource("Stationpedia Ascended");
				ConsoleWindow.Print("[Stationpedia Ascended] v0.3.0 loading...", ConsoleColor.White, false, true);
				CleanupExistingTooltips();
				_initialized = false;
				Instance = this;
				Initialize();
			}
			catch (Exception ex)
			{
				ManualLogSource log = Log;
				if (log != null)
				{
					log.LogError((object)$"ERROR in Awake: {ex}");
				}
				ConsoleWindow.Print("[Stationpedia Ascended] ERROR: " + ex.Message, ConsoleColor.White, false, true);
			}
		}

		private void Initialize()
		{
			if (!_initialized)
			{
				LoadCustomIcon();
				LoadCustomIcons();
				LoadDescriptions();
				ApplyHarmonyPatches();
				_monitorCoroutine = ((MonoBehaviour)this).StartCoroutine(MonitorStationpediaCoroutine());
				UIAssetInspector.Initialize();
				_initialized = true;
				ConsoleWindow.Print("[Stationpedia Ascended] v0.3.0 initialized successfully!", ConsoleColor.White, false, true);
			}
		}

		private IEnumerator MonitorStationpediaCoroutine()
		{
			while (true)
			{
				yield return (object)new WaitForSeconds(0.1f);
				try
				{
					if ((Object)(object)Stationpedia.Instance == (Object)null)
					{
						continue;
					}
					if (!_stationpediaFound)
					{
						_stationpediaFound = true;
						PopulateHiddenItems();
						try
						{
							GameObject titleGO = Stationpedia.Instance.StationpediaTitleText;
							if ((Object)(object)titleGO != (Object)null)
							{
								_headerTitleObject = titleGO;
								TextMeshProUGUI titleText = titleGO.GetComponentInChildren<TextMeshProUGUI>();
								if ((Object)(object)titleText != (Object)null)
								{
									_headerTitleText = titleText;
									UpdateHeaderAppearance();
								}
								Transform headerParent = titleGO.transform.parent;
								if ((Object)(object)headerParent != (Object)null)
								{
									for (int i = 0; i < headerParent.childCount; i++)
									{
										Transform child = headerParent.GetChild(i);
										Image img = ((Component)child).GetComponent<Image>();
										if ((Object)(object)img != (Object)null && (Object)(object)((Component)child).gameObject != (Object)(object)titleGO && (Object)(object)img.sprite != (Object)null && !((Object)img.sprite).name.ToLower().Contains("background"))
										{
											_headerIconImage = img;
											if ((Object)(object)_originalHeaderIconSprite == (Object)null)
											{
												_originalHeaderIconSprite = img.sprite;
											}
											if (!VanillaModeManager.IsVanillaMode && (Object)(object)_customIconSprite != (Object)null)
											{
												img.sprite = _customIconSprite;
												img.preserveAspect = true;
											}
											LayoutElement layoutElement = ((Component)child).GetComponent<LayoutElement>();
											if ((Object)(object)layoutElement == (Object)null)
											{
												layoutElement = ((Component)child).gameObject.AddComponent<LayoutElement>();
											}
											layoutElement.preferredWidth = 32f;
											layoutElement.preferredHeight = 32f;
											layoutElement.minWidth = 32f;
											layoutElement.minHeight = 32f;
											layoutElement.flexibleWidth = 0f;
											layoutElement.flexibleHeight = 0f;
											RectTransform rt = ((Component)child).GetComponent<RectTransform>();
											if ((Object)(object)rt != (Object)null)
											{
												rt.sizeDelta = new Vector2(32f, 32f);
												rt.SetSizeWithCurrentAnchors((Axis)0, 32f);
												rt.SetSizeWithCurrentAnchors((Axis)1, 32f);
											}
											break;
										}
									}
								}
							}
						}
						catch (Exception ex)
						{
							Exception ex2 = ex;
							ManualLogSource log = Log;
							if (log != null)
							{
								log.LogWarning((object)("Error setting title/icon: " + ex2.Message));
							}
						}
						try
						{
							SetupHeaderToggle();
						}
						catch (Exception ex)
						{
							Exception ex3 = ex;
							ManualLogSource log2 = Log;
							if (log2 != null)
							{
								log2.LogWarning((object)("Error setting up header toggle: " + ex3.Message));
							}
						}
						try
						{
							SetupStationPlannerButton();
						}
						catch (Exception ex)
						{
							Exception ex4 = ex;
							ManualLogSource log3 = Log;
							if (log3 != null)
							{
								log3.LogWarning((object)("Error setting up Station Planner button: " + ex4.Message));
							}
						}
						try
						{
							StationPlannerWindow.Initialize();
						}
						catch (Exception ex)
						{
							Exception ex5 = ex;
							ManualLogSource log4 = Log;
							if (log4 != null)
							{
								log4.LogWarning((object)("Error initializing Station Planner: " + ex5.Message));
							}
						}
						try
						{
							SearchPatches.InitializeSearchSystem(Stationpedia.Instance);
						}
						catch (Exception ex)
						{
							Exception ex6 = ex;
							ManualLogSource log5 = Log;
							if (log5 != null)
							{
								log5.LogWarning((object)("Error initializing search system: " + ex6.Message));
							}
						}
						try
						{
							HomePageLayoutManager.Initialize();
						}
						catch (Exception ex)
						{
							Exception ex7 = ex;
							ManualLogSource log6 = Log;
							if (log6 != null)
							{
								log6.LogWarning((object)("Error initializing home page layout: " + ex7.Message));
							}
						}
						try
						{
							SurvivalManualLoader.RegisterSurvivalManualPage();
						}
						catch (Exception ex)
						{
							Exception ex8 = ex;
							ManualLogSource log7 = Log;
							if (log7 != null)
							{
								log7.LogWarning((object)("Error registering Survival Manual: " + ex8.Message));
							}
						}
						try
						{
							DaylightSensorGuideLoader.RegisterDaylightSensorGuidePage();
						}
						catch (Exception ex)
						{
							Exception ex9 = ex;
							ManualLogSource log8 = Log;
							if (log8 != null)
							{
								log8.LogWarning((object)("Error registering Daylight Sensor Guide: " + ex9.Message));
							}
						}
					}
					if (!((Component)Stationpedia.Instance).gameObject.activeInHierarchy)
					{
						_lastPageKey = "";
						continue;
					}
					string currentPageKey = Stationpedia.CurrentPageKey;
					if (!string.IsNullOrEmpty(currentPageKey) && currentPageKey != _lastPageKey)
					{
						_lastPageKey = currentPageKey;
						((MonoBehaviour)this).StartCoroutine(AddTooltipsAfterDelay(currentPageKey));
					}
				}
				catch (Exception ex)
				{
					Exception ex10 = ex;
					ManualLogSource log9 = Log;
					if (log9 != null)
					{
						log9.LogError((object)("Error in monitor: " + ex10.Message));
					}
				}
			}
		}

		private void Start()
		{
		}

		private void InitializeStyles()
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Expected O, but got Unknown
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Expected O, but got Unknown
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Expected O, but got Unknown
			if (!_stylesInitialized)
			{
				_tooltipBackground = new Texture2D(1, 1);
				_tooltipBackground.SetPixel(0, 0, new Color(0.05f, 0.1f, 0.15f, 0.95f));
				_tooltipBackground.Apply();
				_tooltipStyle = new GUIStyle(GUI.skin.box);
				_tooltipStyle.normal.background = _tooltipBackground;
				_tooltipStyle.normal.textColor = Color.white;
				_tooltipStyle.padding = new RectOffset(10, 10, 8, 8);
				_tooltipStyle.wordWrap = true;
				_tooltipStyle.richText = true;
				_tooltipStyle.fontSize = 14;
				_tooltipStyle.alignment = (TextAnchor)0;
				_stylesInitialized = true;
			}
		}

		private void OnGUI()
		{
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Expected O, but got Unknown
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0143: Unknown result type (might be due to invalid IL or missing references)
			//IL_0149: Unknown result type (might be due to invalid IL or missing references)
			//IL_0168: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_010c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0112: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				if (ShowTooltip && !string.IsNullOrEmpty(CurrentTooltipText))
				{
					if (!_stylesInitialized)
					{
						InitializeStyles();
					}
					Vector2 mousePosition = Event.current.mousePosition;
					GUIContent val = new GUIContent(CurrentTooltipText);
					float num = 350f;
					Vector2 val2 = _tooltipStyle.CalcSize(val);
					if (val2.x > num)
					{
						val2.x = num;
						val2.y = _tooltipStyle.CalcHeight(val, num);
					}
					val2.x += 10f;
					val2.y += 10f;
					float num2 = mousePosition.x + 15f;
					float num3 = mousePosition.y + 15f;
					if (num2 + val2.x > (float)Screen.width)
					{
						num2 = mousePosition.x - val2.x - 10f;
					}
					if (num3 + val2.y > (float)Screen.height)
					{
						num3 = mousePosition.y - val2.y - 10f;
					}
					num2 = Mathf.Max(5f, num2);
					num3 = Mathf.Max(5f, num3);
					Rect val3 = default(Rect);
					((Rect)(ref val3))..ctor(num2, num3, val2.x, val2.y);
					GUI.color = new Color(1f, 0.5f, 0f, 0.8f);
					GUI.Box(new Rect(((Rect)(ref val3)).x - 2f, ((Rect)(ref val3)).y - 2f, ((Rect)(ref val3)).width + 4f, ((Rect)(ref val3)).height + 4f), "");
					GUI.color = Color.white;
					GUI.Box(val3, CurrentTooltipText, _tooltipStyle);
				}
			}
			catch (Exception)
			{
			}
		}

		private void LoadCustomIcon()
		{
			//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00df: Expected O, but got Unknown
			//IL_010e: Unknown result type (might be due to invalid IL or missing references)
			//IL_011d: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				byte[] array = null;
				if (array == null)
				{
					List<string> list = new List<string>();
					list.Add("C:\\Dev\\12-17-25 Stationeers Respawn Update Code\\StationpediaAscended\\mod\\images\\phoenix-icon.png");
					list.Add(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", "images", "phoenix-icon.png"));
					list.Add(Path.Combine(Paths.BepInExRootPath, "scripts", "images", "phoenix-icon.png"));
					foreach (string item in list)
					{
						if (!string.IsNullOrEmpty(item) && File.Exists(item))
						{
							array = File.ReadAllBytes(item);
							break;
						}
					}
				}
				if (array == null)
				{
					return;
				}
				Texture2D val = new Texture2D(2, 2, (TextureFormat)4, false);
				((Texture)val).filterMode = (FilterMode)1;
				if (ImageConversion.LoadImage(val, array))
				{
					_customIconSprite = Sprite.Create(val, new Rect(0f, 0f, (float)((Texture)val).width, (float)((Texture)val).height), new Vector2(0.5f, 0.5f), 100f);
					return;
				}
				ManualLogSource log = Log;
				if (log != null)
				{
					log.LogWarning((object)"Failed to load phoenix-icon.png - invalid image format");
				}
			}
			catch (Exception ex)
			{
				ManualLogSource log2 = Log;
				if (log2 != null)
				{
					log2.LogError((object)("Error loading custom icon: " + ex.Message));
				}
			}
		}

		private void LoadCustomIcons()
		{
			try
			{
				List<string> list = new List<string>();
				list.Add("C:\\Dev\\12-17-25 Stationeers Respawn Update Code\\StationpediaAscended\\mod\\images");
				list.Add(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", "images"));
				list.Add(Path.Combine(Paths.BepInExRootPath, "scripts", "images"));
				foreach (string item in list)
				{
					if (string.IsNullOrEmpty(item) || !Directory.Exists(item))
					{
						continue;
					}
					string path = Path.Combine(item, "icon_expanded.png");
					string path2 = Path.Combine(item, "icon_collapsed.png");
					string path3 = Path.Combine(item, "phoenix-icon.png");
					string path4 = Path.Combine(item, "Book-Closed.png");
					if ((Object)(object)_iconExpanded == (Object)null)
					{
						if (File.Exists(path))
						{
							_iconExpanded = LoadSpriteFromFile(path);
						}
						else if (File.Exists(path3))
						{
							_iconExpanded = LoadSpriteFromFile(path3);
						}
					}
					if ((Object)(object)_iconCollapsed == (Object)null)
					{
						if (File.Exists(path2))
						{
							_iconCollapsed = LoadSpriteFromFile(path2);
						}
						else if (File.Exists(path4))
						{
							_iconCollapsed = LoadSpriteFromFile(path4);
						}
					}
					if (!((Object)(object)_iconExpanded != (Object)null) || !((Object)(object)_iconCollapsed != (Object)null))
					{
						continue;
					}
					break;
				}
				if ((Object)(object)_iconExpanded == (Object)null && (Object)(object)_customIconSprite != (Object)null)
				{
					_iconExpanded = _customIconSprite;
				}
				if ((Object)(object)_iconCollapsed == (Object)null && (Object)(object)_customIconSprite != (Object)null)
				{
					_iconCollapsed = _customIconSprite;
				}
			}
			catch (Exception ex)
			{
				ManualLogSource log = Log;
				if (log != null)
				{
					log.LogError((object)("Error loading custom icons: " + ex.Message));
				}
			}
		}

		private void SetupHeaderToggle()
		{
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Expected O, but got Unknown
			//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_011f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0140: Unknown result type (might be due to invalid IL or missing references)
			//IL_014d: Unknown result type (might be due to invalid IL or missing references)
			//IL_015d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0175: Unknown result type (might be due to invalid IL or missing references)
			//IL_017f: Expected O, but got Unknown
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)_headerTitleObject == (Object)null)
			{
				return;
			}
			Transform parent = _headerTitleObject.transform.parent;
			if ((Object)(object)parent == (Object)null)
			{
				return;
			}
			Button component = ((Component)parent).GetComponent<Button>();
			if ((Object)(object)component != (Object)null)
			{
				_headerButton = component;
				((UnityEventBase)_headerButton.onClick).RemoveAllListeners();
				((UnityEvent)_headerButton.onClick).AddListener(new UnityAction(OnHeaderClicked));
				return;
			}
			Graphic val = ((Component)parent).GetComponent<Graphic>();
			if ((Object)(object)val == (Object)null)
			{
				Image val2 = ((Component)parent).gameObject.AddComponent<Image>();
				((Graphic)val2).color = new Color(0f, 0f, 0f, 0f);
				((Graphic)val2).raycastTarget = true;
				val = (Graphic)(object)val2;
			}
			_headerButton = ((Component)parent).gameObject.AddComponent<Button>();
			((Selectable)_headerButton).targetGraphic = val;
			ColorBlock colors = ((Selectable)_headerButton).colors;
			((ColorBlock)(ref colors)).normalColor = Color.white;
			((ColorBlock)(ref colors)).highlightedColor = new Color(1f, 0.9f, 0.8f, 1f);
			((ColorBlock)(ref colors)).pressedColor = new Color(1f, 0.7f, 0.5f, 1f);
			((ColorBlock)(ref colors)).selectedColor = Color.white;
			((Selectable)_headerButton).colors = colors;
			((UnityEvent)_headerButton.onClick).AddListener(new UnityAction(OnHeaderClicked));
		}

		private void OnHeaderClicked()
		{
			VanillaModeManager.Toggle();
			UpdateHeaderAppearance();
			RefreshCurrentPage();
		}

		private void UpdateHeaderAppearance()
		{
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)_headerTitleText != (Object)null)
			{
				((TMP_Text)_headerTitleText).text = "Stationpedia <color=#FF7A18>Ascended</color>";
			}
			if ((Object)(object)_headerIconImage != (Object)null && (Object)(object)_customIconSprite != (Object)null)
			{
				_headerIconImage.sprite = _customIconSprite;
				_headerIconImage.preserveAspect = true;
				((Component)_headerIconImage).transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
				if (VanillaModeManager.IsVanillaMode)
				{
					((Graphic)_headerIconImage).color = Color.white;
				}
				else
				{
					((Graphic)_headerIconImage).color = new Color(1f, 0.6f, 0.2f, 1f);
				}
			}
		}

		private void SetupStationPlannerButton()
		{
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e5: Expected O, but got Unknown
			//IL_017c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0161: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0202: Unknown result type (might be due to invalid IL or missing references)
			//IL_0206: Unknown result type (might be due to invalid IL or missing references)
			//IL_0227: Unknown result type (might be due to invalid IL or missing references)
			//IL_0248: Unknown result type (might be due to invalid IL or missing references)
			//IL_0255: Unknown result type (might be due to invalid IL or missing references)
			//IL_0265: Unknown result type (might be due to invalid IL or missing references)
			//IL_028b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0290: Unknown result type (might be due to invalid IL or missing references)
			//IL_0296: Expected O, but got Unknown
			Stationpedia instance = Stationpedia.Instance;
			if ((Object)(object)instance == (Object)null)
			{
				return;
			}
			Toggle toggleMouse = instance.ToggleMouse;
			if ((Object)(object)toggleMouse == (Object)null)
			{
				ManualLogSource log = Log;
				if (log != null)
				{
					log.LogWarning((object)"ToggleMouse not found, cannot create Station Planner button");
				}
				return;
			}
			Transform parent = ((Component)toggleMouse).transform.parent;
			if ((Object)(object)parent == (Object)null)
			{
				return;
			}
			Transform val = parent.Find("StationPlannerButton");
			if ((Object)(object)val != (Object)null)
			{
				_stationPlannerButton = ((Component)val).GetComponent<Button>();
				_stationPlannerButtonCreated = true;
			}
			else
			{
				if ((Object)(object)_stationPlannerButton != (Object)null && _stationPlannerButtonCreated)
				{
					return;
				}
				if ((Object)(object)_stationPlannerIconSprite == (Object)null)
				{
					_stationPlannerIconSprite = LoadImageFromModFolder("Book-ClosedONE.png");
				}
				GameObject val2 = new GameObject("StationPlannerButton");
				val2.transform.SetParent(parent, false);
				int siblingIndex = ((Component)toggleMouse).transform.GetSiblingIndex();
				val2.transform.SetSiblingIndex(siblingIndex + 1);
				ManualLogSource log2 = Log;
				if (log2 != null)
				{
					log2.LogInfo((object)$"Station Planner button: mouseIndex={siblingIndex}, set to {siblingIndex + 1}");
				}
				RectTransform val3 = val2.AddComponent<RectTransform>();
				RectTransform component = ((Component)toggleMouse).GetComponent<RectTransform>();
				if ((Object)(object)component != (Object)null)
				{
					val3.sizeDelta = component.sizeDelta;
				}
				else
				{
					val3.sizeDelta = new Vector2(32f, 32f);
				}
				Image val4 = val2.AddComponent<Image>();
				if ((Object)(object)_stationPlannerIconSprite != (Object)null)
				{
					val4.sprite = _stationPlannerIconSprite;
					val4.preserveAspect = true;
				}
				else
				{
					((Graphic)val4).color = new Color(0.9f, 0.8f, 0.6f, 1f);
				}
				_stationPlannerButton = val2.AddComponent<Button>();
				((Selectable)_stationPlannerButton).targetGraphic = (Graphic)(object)val4;
				ColorBlock colors = ((Selectable)_stationPlannerButton).colors;
				((ColorBlock)(ref colors)).normalColor = Color.white;
				((ColorBlock)(ref colors)).highlightedColor = new Color(1f, 0.9f, 0.7f, 1f);
				((ColorBlock)(ref colors)).pressedColor = new Color(0.9f, 0.7f, 0.5f, 1f);
				((ColorBlock)(ref colors)).selectedColor = Color.white;
				((Selectable)_stationPlannerButton).colors = colors;
				ButtonClickedEvent onClick = _stationPlannerButton.onClick;
				object obj = <>c.<>9__66_0;
				if (obj == null)
				{
					UnityAction val5 = delegate
					{
						StationPlannerWindow.Toggle();
					};
					<>c.<>9__66_0 = val5;
					obj = (object)val5;
				}
				((UnityEvent)onClick).AddListener((UnityAction)obj);
				_stationPlannerButtonCreated = true;
				ManualLogSource log3 = Log;
				if (log3 != null)
				{
					log3.LogInfo((object)"Station Planner button added to Stationpedia header (to LEFT of mouse toggle)");
				}
			}
		}

		private void RefreshCurrentPage()
		{
			try
			{
				if ((Object)(object)Stationpedia.Instance != (Object)null && !string.IsNullOrEmpty(Stationpedia.CurrentPageKey))
				{
					string currentPageKey = Stationpedia.CurrentPageKey;
					_lastPageKey = "";
					Stationpedia.Instance.SetPage("Home", false);
					((MonoBehaviour)this).StartCoroutine(DelayedSetPage(currentPageKey));
				}
			}
			catch (Exception ex)
			{
				ManualLogSource log = Log;
				if (log != null)
				{
					log.LogWarning((object)("Error refreshing page: " + ex.Message));
				}
			}
		}

		private IEnumerator DelayedSetPage(string pageKey)
		{
			yield return null;
			if ((Object)(object)Stationpedia.Instance != (Object)null)
			{
				Stationpedia.Instance.SetPage(pageKey, false);
			}
		}

		private static Sprite LoadSpriteFromFile(string path)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Expected O, but got Unknown
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				byte[] array = File.ReadAllBytes(path);
				Texture2D val = new Texture2D(2, 2, (TextureFormat)4, false);
				((Texture)val).filterMode = (FilterMode)1;
				if (ImageConversion.LoadImage(val, array))
				{
					return Sprite.Create(val, new Rect(0f, 0f, (float)((Texture)val).width, (float)((Texture)val).height), new Vector2(0.5f, 0.5f), 100f);
				}
			}
			catch (Exception ex)
			{
				ManualLogSource log = Log;
				if (log != null)
				{
					log.LogWarning((object)("Failed to load sprite from " + path + ": " + ex.Message));
				}
			}
			return null;
		}

		public static Sprite LoadSlicedSprite(string relativePath, int borderSize = 10)
		{
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Expected O, but got Unknown
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0109: Unknown result type (might be due to invalid IL or missing references)
			List<string> list = new List<string>();
			list.Add("C:\\Dev\\12-17-25 Stationeers Respawn Update Code\\StationpediaAscended\\mod\\images");
			list.Add(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", "images"));
			list.Add(Path.Combine(Paths.BepInExRootPath, "scripts", "images"));
			Vector4 val2 = default(Vector4);
			foreach (string item in list)
			{
				if (string.IsNullOrEmpty(item))
				{
					continue;
				}
				string text = Path.Combine(item, relativePath);
				if (!File.Exists(text))
				{
					continue;
				}
				try
				{
					byte[] array = File.ReadAllBytes(text);
					Texture2D val = new Texture2D(2, 2, (TextureFormat)4, false);
					((Texture)val).filterMode = (FilterMode)1;
					if (ImageConversion.LoadImage(val, array))
					{
						((Vector4)(ref val2))..ctor((float)borderSize, (float)borderSize, (float)borderSize, (float)borderSize);
						return Sprite.Create(val, new Rect(0f, 0f, (float)((Texture)val).width, (float)((Texture)val).height), new Vector2(0.5f, 0.5f), 100f, 0u, (SpriteMeshType)0, val2);
					}
				}
				catch (Exception ex)
				{
					ManualLogSource log = Log;
					if (log != null)
					{
						log.LogWarning((object)("Failed to load sliced sprite from " + text + ": " + ex.Message));
					}
				}
			}
			ManualLogSource log2 = Log;
			if (log2 != null)
			{
				log2.LogWarning((object)("Sliced sprite not found: " + relativePath));
			}
			return null;
		}

		public static Sprite GetRoundedBackgroundSprite()
		{
			if ((Object)(object)_roundedBgSprite == (Object)null)
			{
				_roundedBgSprite = LoadSlicedSprite("rounded-bg.png", 12);
			}
			return _roundedBgSprite;
		}

		public static Sprite GetWindowBackgroundSprite()
		{
			if ((Object)(object)_windowBgSprite == (Object)null)
			{
				_windowBgSprite = LoadSlicedSprite("Inv-window-bg.png");
			}
			return _windowBgSprite;
		}

		public static Sprite LoadImageFromModFolder(string relativePath)
		{
			List<string> list = new List<string>();
			list.Add("C:\\Dev\\12-17-25 Stationeers Respawn Update Code\\StationpediaAscended\\mod\\images");
			list.Add(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", "images"));
			list.Add(Path.Combine(Paths.BepInExRootPath, "scripts", "images"));
			foreach (string item in list)
			{
				if (!string.IsNullOrEmpty(item))
				{
					string path = Path.Combine(item, relativePath);
					if (File.Exists(path))
					{
						return LoadSpriteFromFile(path);
					}
				}
			}
			ManualLogSource log = Log;
			if (log != null)
			{
				log.LogWarning((object)("Image not found: " + relativePath));
			}
			return null;
		}

		public static string GetImageFilePath(string relativePath)
		{
			List<string> list = new List<string>();
			list.Add("C:\\Dev\\12-17-25 Stationeers Respawn Update Code\\StationpediaAscended\\mod\\images");
			list.Add(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", "images"));
			list.Add(Path.Combine(Paths.BepInExRootPath, "scripts", "images"));
			foreach (string item in list)
			{
				if (!string.IsNullOrEmpty(item))
				{
					string text = Path.Combine(item, relativePath);
					if (File.Exists(text))
					{
						return text;
					}
				}
			}
			ManualLogSource log = Log;
			if (log != null)
			{
				log.LogWarning((object)("File not found: " + relativePath));
			}
			return null;
		}

		private void LoadDescriptions()
		{
			DeviceDatabase = new Dictionary<string, DeviceDescriptions>();
			GenericDescriptions = new GenericDescriptionsData();
			try
			{
				string text = null;
				if (text == null)
				{
					List<string> list = new List<string>();
					list.Add("C:\\Dev\\12-17-25 Stationeers Respawn Update Code\\StationpediaAscended\\mod\\descriptions.json");
					list.Add(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", "descriptions.json"));
					list.Add(Path.Combine(Application.dataPath, "..", "BepInEx", "scripts", "descriptions.json"));
					foreach (string item in list)
					{
						if (!string.IsNullOrEmpty(item) && File.Exists(item))
						{
							text = File.ReadAllText(item);
							break;
						}
					}
				}
				if (text != null)
				{
					DescriptionsRoot descriptionsRoot = JsonConvert.DeserializeObject<DescriptionsRoot>(text);
					if (descriptionsRoot?.devices != null)
					{
						foreach (DeviceDescriptions device in descriptionsRoot.devices)
						{
							DeviceDatabase[device.deviceKey] = device;
						}
					}
					if (descriptionsRoot?.genericDescriptions != null)
					{
						GenericDescriptions = descriptionsRoot.genericDescriptions;
					}
					if (descriptionsRoot?.guides != null && descriptionsRoot.guides.Count > 0)
					{
						JsonGuideLoader.LoadGuides(descriptionsRoot);
					}
					if (descriptionsRoot?.mechanics != null && descriptionsRoot.mechanics.Count > 0)
					{
						JsonMechanicsLoader.LoadMechanics(descriptionsRoot);
					}
				}
				else
				{
					ManualLogSource log = Log;
					if (log != null)
					{
						log.LogWarning((object)"descriptions.json not found");
					}
				}
			}
			catch (Exception ex)
			{
				ManualLogSource log2 = Log;
				if (log2 != null)
				{
					log2.LogError((object)("Error loading descriptions: " + ex.Message));
				}
			}
		}

		private void ApplyHarmonyPatches()
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Expected O, but got Unknown
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Expected O, but got Unknown
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cf: Expected O, but got Unknown
			//IL_0136: Unknown result type (might be due to invalid IL or missing references)
			//IL_0144: Expected O, but got Unknown
			//IL_0186: Unknown result type (might be due to invalid IL or missing references)
			//IL_0194: Expected O, but got Unknown
			//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e4: Expected O, but got Unknown
			//IL_0226: Unknown result type (might be due to invalid IL or missing references)
			//IL_0234: Expected O, but got Unknown
			//IL_0277: Unknown result type (might be due to invalid IL or missing references)
			//IL_0284: Expected O, but got Unknown
			//IL_02de: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f2: Expected O, but got Unknown
			//IL_02f2: Expected O, but got Unknown
			try
			{
				_harmony = new Harmony("com.stationpediaascended.mod");
				Type typeFromHandle = typeof(UniversalPage);
				MethodInfo method = typeFromHandle.GetMethod("PopulateLogicSlotInserts", BindingFlags.Instance | BindingFlags.Public);
				if (method != null)
				{
					MethodInfo method2 = typeof(HarmonyPatches).GetMethod("PopulateLogicSlotInserts_Postfix", BindingFlags.Static | BindingFlags.Public);
					_harmony.Patch((MethodBase)method, (HarmonyMethod)null, new HarmonyMethod(method2), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
				}
				else
				{
					ManualLogSource log = Log;
					if (log != null)
					{
						log.LogError((object)"Could not find PopulateLogicSlotInserts method");
					}
				}
				MethodInfo method3 = typeFromHandle.GetMethod("ChangeDisplay", BindingFlags.Instance | BindingFlags.Public);
				if (method3 != null)
				{
					MethodInfo method4 = typeof(HarmonyPatches).GetMethod("ChangeDisplay_Postfix", BindingFlags.Static | BindingFlags.Public);
					_harmony.Patch((MethodBase)method3, (HarmonyMethod)null, new HarmonyMethod(method4), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
				}
				else
				{
					ManualLogSource log2 = Log;
					if (log2 != null)
					{
						log2.LogError((object)"Could not find ChangeDisplay method");
					}
				}
				Type typeFromHandle2 = typeof(Stationpedia);
				MethodInfo method5 = typeFromHandle2.GetMethod("OnDrag", BindingFlags.Instance | BindingFlags.Public);
				if (method5 != null)
				{
					MethodInfo method6 = typeof(HarmonyPatches).GetMethod("Stationpedia_OnDrag_Prefix", BindingFlags.Static | BindingFlags.Public);
					_harmony.Patch((MethodBase)method5, new HarmonyMethod(method6), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
				}
				MethodInfo method7 = typeFromHandle2.GetMethod("OnBeginDrag", BindingFlags.Instance | BindingFlags.Public);
				if (method7 != null)
				{
					MethodInfo method8 = typeof(HarmonyPatches).GetMethod("Stationpedia_OnBeginDrag_Prefix", BindingFlags.Static | BindingFlags.Public);
					_harmony.Patch((MethodBase)method7, new HarmonyMethod(method8), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
				}
				MethodInfo method9 = typeFromHandle2.GetMethod("ClearPreviousSearch", BindingFlags.Instance | BindingFlags.NonPublic);
				if (method9 != null)
				{
					MethodInfo method10 = typeof(SearchPatches).GetMethod("ClearPreviousSearch_Postfix", BindingFlags.Static | BindingFlags.Public);
					_harmony.Patch((MethodBase)method9, (HarmonyMethod)null, new HarmonyMethod(method10), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
				}
				MethodInfo method11 = typeFromHandle2.GetMethod("SetPage", BindingFlags.Instance | BindingFlags.Public);
				if (method11 != null)
				{
					MethodInfo method12 = typeof(HarmonyPatches).GetMethod("Stationpedia_SetPage_Prefix", BindingFlags.Static | BindingFlags.Public);
					_harmony.Patch((MethodBase)method11, new HarmonyMethod(method12), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
				}
				MethodInfo method13 = typeFromHandle2.GetMethod("SetPageGuides", BindingFlags.Instance | BindingFlags.NonPublic);
				if (method13 != null)
				{
					MethodInfo method14 = typeof(HarmonyPatches).GetMethod("Stationpedia_SetPageGuides_Postfix", BindingFlags.Static | BindingFlags.Public);
					_harmony.Patch((MethodBase)method13, (HarmonyMethod)null, new HarmonyMethod(method14), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
				}
				MethodInfo method15 = typeFromHandle2.GetMethod("SetPageLore", BindingFlags.Instance | BindingFlags.Public);
				if (method15 != null)
				{
					MethodInfo method16 = typeof(HarmonyPatches).GetMethod("Stationpedia_SetPageLore_Prefix", BindingFlags.Static | BindingFlags.Public);
					MethodInfo method17 = typeof(HarmonyPatches).GetMethod("Stationpedia_SetPageLore_Postfix", BindingFlags.Static | BindingFlags.Public);
					_harmony.Patch((MethodBase)method15, new HarmonyMethod(method16), new HarmonyMethod(method17), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
				}
				RegisterConsoleCommands();
			}
			catch (Exception ex)
			{
				ManualLogSource log3 = Log;
				if (log3 != null)
				{
					log3.LogError((object)("Error applying Harmony patches: " + ex.Message));
				}
			}
		}

		private static void PopulateHiddenItems()
		{
			if (_hiddenItemsPopulated)
			{
				return;
			}
			_hiddenItemsPopulated = true;
			try
			{
				Dictionary<string, bool> dictionary = Stationpedia.DataHandler?.HiddenInPedia;
				if (dictionary == null)
				{
					ManualLogSource log = Log;
					if (log != null)
					{
						log.LogWarning((object)"HiddenInPedia dictionary not available");
					}
					return;
				}
				int num = 0;
				int num2 = 0;
				List<string> list = new List<string>();
				foreach (Thing allPrefab in Prefab.AllPrefabs)
				{
					if (!((Object)(object)allPrefab == (Object)null))
					{
						num2++;
						string text = allPrefab.PrefabName ?? "";
						string text2 = allPrefab.DisplayName ?? "";
						string text3 = text2.ToLowerInvariant();
						string text4 = text.ToLowerInvariant();
						bool flag = false;
						if (text3.StartsWith("burnt") || text4.Contains("burnt"))
						{
							flag = true;
						}
						if (text3.Contains("ruptured") || text4.Contains("ruptured"))
						{
							flag = true;
						}
						if (allPrefab is CableRuptured)
						{
							flag = true;
						}
						if (text3.Contains("wreckage") || text4.Contains("wreckage"))
						{
							flag = true;
						}
						if (flag)
						{
							dictionary[text] = true;
							allPrefab.HideInStationpedia = true;
							num++;
							list.Add("Thing" + text);
						}
					}
				}
				int num3 = 0;
				foreach (string pageKey in list)
				{
					StationpediaPage val = Stationpedia.StationpediaPages.Find((StationpediaPage p) => p.Key == pageKey);
					if (val != null)
					{
						Stationpedia.StationpediaPages.Remove(val);
						num3++;
					}
				}
			}
			catch (Exception ex)
			{
				ManualLogSource log2 = Log;
				if (log2 != null)
				{
					log2.LogError((object)("Error populating hidden items: " + ex.Message));
				}
			}
		}

		private void RegisterConsoleCommands()
		{
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Expected O, but got Unknown
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Expected O, but got Unknown
			//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d8: Expected O, but got Unknown
			//IL_0116: Unknown result type (might be due to invalid IL or missing references)
			//IL_0120: Expected O, but got Unknown
			try
			{
				IReadOnlyDictionary<string, CommandBase> commandsMap = CommandLine.CommandsMap;
				if (!commandsMap.ContainsKey("stationpediacenter"))
				{
					CommandLine.AddCommand("stationpediacenter", (CommandBase)new BasicCommand((Func<string[], string>)CenterStationpediaCommand, "Centers the Stationpedia window on screen", (string[])null, false));
				}
				if (!commandsMap.ContainsKey("spda_dumpkeys"))
				{
					CommandLine.AddCommand("spda_dumpkeys", (CommandBase)new BasicCommand((Func<string[], string>)DumpPageKeysCommand, "Exports all Stationpedia page keys to a file for use in descriptions.json", (string[])null, false));
				}
				if (!commandsMap.ContainsKey("spda_currentkey"))
				{
					CommandLine.AddCommand("spda_currentkey", (CommandBase)new BasicCommand((Func<string[], string>)CurrentPageKeyCommand, "Shows the deviceKey of the currently open Stationpedia page", (string[])null, false));
				}
				if (!commandsMap.ContainsKey("assetdisplay"))
				{
					CommandLine.AddCommand("assetdisplay", (CommandBase)new BasicCommand((Func<string[], string>)AssetDisplayCommand, "Toggles the UI Asset Inspector to show asset names under mouse cursor", (string[])null, false));
				}
			}
			catch (Exception ex)
			{
				ManualLogSource log = Log;
				if (log != null)
				{
					log.LogError((object)("Error registering console commands: " + ex.Message));
				}
			}
		}

		private static string CenterStationpediaCommand(string[] args)
		{
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				Stationpedia instance = Stationpedia.Instance;
				if ((Object)(object)instance == (Object)null)
				{
					return "Stationpedia instance not found";
				}
				RectTransform rectTransform = ((UserInterfaceBase)instance).RectTransform;
				if ((Object)(object)rectTransform == (Object)null)
				{
					return "Stationpedia RectTransform not found";
				}
				((Transform)rectTransform).localPosition = Vector3.zero;
				return "Stationpedia centered on screen";
			}
			catch (Exception ex)
			{
				return "Error centering Stationpedia: " + ex.Message;
			}
		}

		private static string CurrentPageKeyCommand(string[] args)
		{
			try
			{
				string currentPageKey = Stationpedia.CurrentPageKey;
				if (string.IsNullOrEmpty(currentPageKey))
				{
					return "No page currently open";
				}
				return "Current page deviceKey: " + currentPageKey;
			}
			catch (Exception ex)
			{
				return "Error: " + ex.Message;
			}
		}

		private static string AssetDisplayCommand(string[] args)
		{
			try
			{
				UIAssetInspector.Toggle();
				bool isEnabled = UIAssetInspector.IsEnabled;
				return "UI Asset Inspector: " + (isEnabled ? "ENABLED - hover over UI elements to see asset names" : "DISABLED");
			}
			catch (Exception ex)
			{
				return "Error toggling UI Asset Inspector: " + ex.Message;
			}
		}

		private static string DumpPageKeysCommand(string[] args)
		{
			try
			{
				List<StationpediaPage> stationpediaPages = Stationpedia.StationpediaPages;
				if (stationpediaPages == null || stationpediaPages.Count == 0)
				{
					return "No Stationpedia pages found. Open Stationpedia first.";
				}
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("# Stationpedia Page Keys");
				stringBuilder.AppendLine($"# Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
				stringBuilder.AppendLine($"# Total pages: {stationpediaPages.Count}");
				stringBuilder.AppendLine("#");
				stringBuilder.AppendLine("# Use these keys as 'deviceKey' in descriptions.json");
				stringBuilder.AppendLine("# Format: deviceKey | Title");
				stringBuilder.AppendLine("#");
				stringBuilder.AppendLine();
				List<StationpediaPage> list = new List<StationpediaPage>(stationpediaPages);
				list.Sort((StationpediaPage a, StationpediaPage b) => string.Compare(a.Key, b.Key, StringComparison.OrdinalIgnoreCase));
				foreach (StationpediaPage item in list)
				{
					stringBuilder.AppendLine(item.Key + " | " + item.Title);
				}
				string text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "My Games", "Stationeers", "stationpedia_keys.txt");
				File.WriteAllText(text, stringBuilder.ToString());
				return $"Exported {stationpediaPages.Count} page keys to:\n{text}";
			}
			catch (Exception ex)
			{
				return "Error dumping keys: " + ex.Message;
			}
		}

		private static void LoadDescriptionsStatic()
		{
			DeviceDatabase = new Dictionary<string, DeviceDescriptions>();
			GenericDescriptions = new GenericDescriptionsData();
			try
			{
				string text = null;
				if (text == null)
				{
					List<string> list = new List<string>();
					list.Add("C:\\Dev\\12-17-25 Stationeers Respawn Update Code\\StationpediaAscended\\mod\\descriptions.json");
					list.Add(Path.Combine(Application.dataPath, "..", "BepInEx", "scripts", "descriptions.json"));
					list.Add(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", "descriptions.json"));
					list.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "My Games", "Stationeers", "mods", "StationpediaAscended", "descriptions.json"));
					foreach (string item in list)
					{
						if (!string.IsNullOrEmpty(item))
						{
							string fullPath = Path.GetFullPath(item);
							if (File.Exists(fullPath))
							{
								text = File.ReadAllText(fullPath);
								break;
							}
						}
					}
				}
				if (text != null)
				{
					DescriptionsRoot descriptionsRoot = JsonConvert.DeserializeObject<DescriptionsRoot>(text);
					if (descriptionsRoot?.devices != null)
					{
						foreach (DeviceDescriptions device in descriptionsRoot.devices)
						{
							DeviceDatabase[device.deviceKey] = device;
						}
					}
					if (descriptionsRoot?.genericDescriptions != null)
					{
						GenericDescriptions = descriptionsRoot.genericDescriptions;
					}
					if (descriptionsRoot?.guides != null && descriptionsRoot.guides.Count > 0)
					{
						JsonGuideLoader.LoadGuides(descriptionsRoot);
					}
					if (descriptionsRoot?.mechanics != null && descriptionsRoot.mechanics.Count > 0)
					{
						JsonMechanicsLoader.LoadMechanics(descriptionsRoot);
					}
				}
				else
				{
					ManualLogSource log = Log;
					if (log != null)
					{
						log.LogWarning((object)"descriptions.json not found");
					}
				}
			}
			catch (Exception arg)
			{
				ManualLogSource log2 = Log;
				if (log2 != null)
				{
					log2.LogError((object)$"Error loading descriptions: {arg}");
				}
			}
		}

		private static void ApplyHarmonyPatchesStatic()
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Expected O, but got Unknown
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Expected O, but got Unknown
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Expected O, but got Unknown
			//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_010c: Expected O, but got Unknown
			try
			{
				_harmonyStatic = new Harmony("com.stationpediaascended.mod.scriptengine");
				Type typeFromHandle = typeof(UniversalPage);
				MethodInfo method = typeFromHandle.GetMethod("PopulateLogicSlotInserts", BindingFlags.Instance | BindingFlags.Public);
				if (method != null)
				{
					MethodInfo method2 = typeof(HarmonyPatches).GetMethod("PopulateLogicSlotInserts_Postfix", BindingFlags.Static | BindingFlags.Public);
					_harmonyStatic.Patch((MethodBase)method, (HarmonyMethod)null, new HarmonyMethod(method2), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
				}
				MethodInfo method3 = typeFromHandle.GetMethod("ChangeDisplay", BindingFlags.Instance | BindingFlags.Public);
				if (method3 != null)
				{
					MethodInfo method4 = typeof(HarmonyPatches).GetMethod("ChangeDisplay_Postfix", BindingFlags.Static | BindingFlags.Public);
					_harmonyStatic.Patch((MethodBase)method3, (HarmonyMethod)null, new HarmonyMethod(method4), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
				}
				Type typeFromHandle2 = typeof(Stationpedia);
				MethodInfo method5 = typeFromHandle2.GetMethod("ClearPreviousSearch", BindingFlags.Instance | BindingFlags.NonPublic);
				if (method5 != null)
				{
					MethodInfo method6 = typeof(SearchPatches).GetMethod("ClearPreviousSearch_Postfix", BindingFlags.Static | BindingFlags.Public);
					_harmonyStatic.Patch((MethodBase)method5, (HarmonyMethod)null, new HarmonyMethod(method6), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
				}
			}
			catch (Exception arg)
			{
				ManualLogSource log = Log;
				if (log != null)
				{
					log.LogError((object)$"Error applying Harmony patches: {arg}");
				}
			}
		}

		private static IEnumerator MonitorStationpediaCoroutineStatic()
		{
			while (true)
			{
				yield return (object)new WaitForSeconds(0.1f);
				try
				{
					if ((Object)(object)Stationpedia.Instance == (Object)null)
					{
						continue;
					}
					if (!_stationpediaFoundStatic)
					{
						_stationpediaFoundStatic = true;
						PopulateHiddenItems();
					}
					if (!((Component)Stationpedia.Instance).gameObject.activeInHierarchy)
					{
						_lastPageKeyStatic = "";
						continue;
					}
					string currentPageKey = Stationpedia.CurrentPageKey;
					if (!string.IsNullOrEmpty(currentPageKey) && currentPageKey != _lastPageKeyStatic)
					{
						_lastPageKeyStatic = currentPageKey;
						if ((Object)(object)_scriptEngineHost != (Object)null)
						{
							_scriptEngineHost.StartCoroutine(AddTooltipsAfterDelayStatic(currentPageKey));
						}
					}
				}
				catch (Exception ex)
				{
					Exception ex2 = ex;
					ManualLogSource log = Log;
					if (log != null)
					{
						log.LogError((object)("Error in monitor: " + ex2.Message));
					}
				}
			}
		}

		private static IEnumerator AddTooltipsAfterDelayStatic(string pageKey)
		{
			yield return (object)new WaitForEndOfFrame();
			yield return (object)new WaitForEndOfFrame();
			AddTooltipsToLogicItemsStatic(pageKey);
		}

		private static void AddTooltipsToLogicItemsStatic(string pageKey)
		{
			try
			{
				UniversalPage val = Stationpedia.Instance?.UniversalPageRef;
				if ((Object)(object)val == (Object)null)
				{
					return;
				}
				int num = 0;
				num += AddTooltipsToCategoryStatic(val.LogicContents, pageKey, "Logic");
				num += AddTooltipsToCategoryStatic(val.LogicSlotContents, pageKey, "LogicSlot");
				num += AddTooltipsToCategoryStatic(val.ModeContents, pageKey, "Mode");
				num += AddTooltipsToCategoryStatic(val.ConnectionContents, pageKey, "Connection");
				try
				{
					num += AddTooltipsToPropertiesStatic(val);
				}
				catch
				{
				}
			}
			catch (Exception ex)
			{
				ManualLogSource log = Log;
				if (log != null)
				{
					log.LogError((object)("Error adding tooltips: " + ex.Message));
				}
			}
		}

		private static int AddTooltipsToCategoryStatic(StationpediaCategory category, string pageKey, string categoryName)
		{
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Expected O, but got Unknown
			if ((Object)(object)category == (Object)null || (Object)(object)category.Contents == (Object)null)
			{
				return 0;
			}
			int num = 0;
			foreach (Transform item in (Transform)category.Contents)
			{
				Transform val = item;
				SPDALogic component = ((Component)val).GetComponent<SPDALogic>();
				if ((Object)(object)component != (Object)null && (Object)(object)component.InfoValue != (Object)null)
				{
					SPDALogicTooltip component2 = ((Component)component).GetComponent<SPDALogicTooltip>();
					if ((Object)(object)component2 == (Object)null)
					{
						string text = ((TMP_Text)component.InfoValue).text;
						SPDALogicTooltip sPDALogicTooltip = ((Component)component).gameObject.AddComponent<SPDALogicTooltip>();
						sPDALogicTooltip.Initialize(pageKey, text, categoryName);
						_addedComponents.Add((Component)(object)sPDALogicTooltip);
						num++;
					}
				}
			}
			return num;
		}

		private static int AddTooltipsToPropertiesStatic(UniversalPage universalPage)
		{
			if ((Object)(object)universalPage == (Object)null)
			{
				return 0;
			}
			int added = 0;
			AddPropertyTooltip(universalPage.FlashPointText, "Flashpoint");
			AddPropertyTooltip(universalPage.AutoIgniteText, "Autoignition");
			AddPropertyTooltip(universalPage.HeatTransferConvectionText, "Thermal Convection");
			AddPropertyTooltip(universalPage.HeatTransferRadiationText, "Thermal Radiation");
			AddPropertyTooltip(universalPage.SolarHeatingFactorText, "Solar Heating");
			AddPropertyTooltip(universalPage.SpecificHeat, "Specific Heat");
			AddPropertyTooltip(universalPage.FreezeTemperature, "Freeze Temperature");
			AddPropertyTooltip(universalPage.BoilingTemperature, "Boiling Temperature");
			AddPropertyTooltip(universalPage.MaxLiquidTemperature, "Max Liquid Temperature");
			AddPropertyTooltip(universalPage.MinLiquidPressure, "Min Liquid Pressure");
			AddPropertyTooltip(universalPage.LatentHeat, "Latent Heat");
			AddPropertyTooltip(universalPage.MolesPerLitre, "Moles Per Litre");
			AddPropertyTooltip(universalPage.MaxPressure, "Max Pressure");
			AddPropertyTooltip(universalPage.Volume, "Volume");
			AddPropertyTooltip(universalPage.DeviceBasePower, "Base Power");
			AddPropertyTooltip(universalPage.DevicePowerStorage, "Power Storage");
			AddPropertyTooltip(universalPage.DevicePowerGeneration, "Power Generation");
			AddPropertyTooltip(universalPage.GrowthTime, "Growth Time");
			AddPropertyTooltip(universalPage.Nutrition, "Nutrition");
			AddPropertyTooltip(universalPage.NutritionQuality, "Nutrition Quality");
			AddPropertyTooltip(universalPage.MoodBonus, "Mood Bonus");
			AddPropertyTooltip(universalPage.PlaceableInRocket, "Placeable In Rocket");
			AddPropertyTooltip(universalPage.RocketMass, "Rocket Mass");
			return added;
			void AddPropertyTooltip(TextMeshProUGUI textElement, string propertyName)
			{
				//IL_008b: Unknown result type (might be due to invalid IL or missing references)
				if ((Object)(object)textElement != (Object)null && (Object)(object)((Component)textElement).gameObject != (Object)null && !string.IsNullOrEmpty(((TMP_Text)textElement).text))
				{
					Transform parent = ((TMP_Text)textElement).transform.parent;
					GameObject val = ((parent != null) ? ((Component)parent).gameObject : null);
					if ((Object)(object)val != (Object)null)
					{
						Graphic component = val.GetComponent<Graphic>();
						if ((Object)(object)component == (Object)null)
						{
							Image val2 = val.AddComponent<Image>();
							((Graphic)val2).color = new Color(0f, 0f, 0f, 0f);
							((Graphic)val2).raycastTarget = true;
						}
						SPDAPropertyTooltip component2 = val.GetComponent<SPDAPropertyTooltip>();
						if ((Object)(object)component2 == (Object)null)
						{
							SPDAPropertyTooltip sPDAPropertyTooltip = val.AddComponent<SPDAPropertyTooltip>();
							sPDAPropertyTooltip.Initialize(propertyName);
							_addedComponents.Add((Component)(object)sPDAPropertyTooltip);
							added++;
						}
					}
				}
			}
		}

		private IEnumerator AddTooltipsAfterDelay(string pageKey)
		{
			yield return (object)new WaitForEndOfFrame();
			yield return (object)new WaitForEndOfFrame();
			AddTooltipsToLogicItems(pageKey);
		}

		private void AddTooltipsToLogicItems(string pageKey)
		{
			try
			{
				UniversalPage universalPageRef = Stationpedia.Instance.UniversalPageRef;
				if ((Object)(object)universalPageRef == (Object)null)
				{
					return;
				}
				int num = 0;
				num += AddTooltipsToCategory(universalPageRef.LogicContents, pageKey, "Logic");
				num += AddTooltipsToCategory(universalPageRef.LogicSlotContents, pageKey, "LogicSlot");
				num += AddTooltipsToCategory(universalPageRef.ModeContents, pageKey, "Mode");
				num += AddTooltipsToCategory(universalPageRef.ConnectionContents, pageKey, "Connection");
				try
				{
					num += AddTooltipsToSlots(universalPageRef.SlotContents, pageKey);
				}
				catch
				{
				}
				try
				{
					num += AddTooltipsToVersions(universalPageRef.StructureVersionContents, pageKey);
				}
				catch
				{
				}
				try
				{
					num += AddTooltipsToMemory(universalPageRef.LogicInstructions, pageKey);
				}
				catch
				{
				}
				try
				{
					num += AddTooltipsToProperties(universalPageRef);
				}
				catch
				{
				}
			}
			catch (Exception ex)
			{
				ManualLogSource log = Log;
				if (log != null)
				{
					log.LogError((object)("Error adding tooltips: " + ex.Message));
				}
			}
		}

		private int AddTooltipsToCategory(StationpediaCategory category, string pageKey, string categoryName)
		{
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Expected O, but got Unknown
			if ((Object)(object)category == (Object)null || (Object)(object)category.Contents == (Object)null)
			{
				return 0;
			}
			int num = 0;
			foreach (Transform item in (Transform)category.Contents)
			{
				Transform val = item;
				SPDALogic component = ((Component)val).GetComponent<SPDALogic>();
				if ((Object)(object)component != (Object)null && (Object)(object)component.InfoValue != (Object)null)
				{
					SPDALogicTooltip component2 = ((Component)component).GetComponent<SPDALogicTooltip>();
					if ((Object)(object)component2 == (Object)null)
					{
						string text = ((TMP_Text)component.InfoValue).text;
						SPDALogicTooltip sPDALogicTooltip = ((Component)component).gameObject.AddComponent<SPDALogicTooltip>();
						sPDALogicTooltip.Initialize(pageKey, text, categoryName);
						_addedComponents.Add((Component)(object)sPDALogicTooltip);
						num++;
					}
				}
			}
			return num;
		}

		private int AddTooltipsToSlots(StationpediaCategory category, string pageKey)
		{
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Expected O, but got Unknown
			if ((Object)(object)category == (Object)null || (Object)(object)category.Contents == (Object)null)
			{
				return 0;
			}
			int num = 0;
			foreach (Transform item in (Transform)category.Contents)
			{
				Transform val = item;
				SPDASlot component = ((Component)val).GetComponent<SPDASlot>();
				if ((Object)(object)component != (Object)null && (Object)(object)component.SlotTitle != (Object)null && !string.IsNullOrEmpty(((TMP_Text)component.SlotTitle).text))
				{
					SPDASlotTooltip component2 = ((Component)component).GetComponent<SPDASlotTooltip>();
					if ((Object)(object)component2 == (Object)null)
					{
						string text = ((TMP_Text)component.SlotTitle).text;
						SPDASlotTooltip sPDASlotTooltip = ((Component)component).gameObject.AddComponent<SPDASlotTooltip>();
						sPDASlotTooltip.Initialize(pageKey, text);
						_addedComponents.Add((Component)(object)sPDASlotTooltip);
						num++;
					}
				}
			}
			return num;
		}

		private int AddTooltipsToVersions(StationpediaCategory category, string pageKey)
		{
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Expected O, but got Unknown
			if ((Object)(object)category == (Object)null || (Object)(object)category.Contents == (Object)null)
			{
				return 0;
			}
			int num = 0;
			foreach (Transform item in (Transform)category.Contents)
			{
				Transform val = item;
				SPDAVersion component = ((Component)val).GetComponent<SPDAVersion>();
				if ((Object)(object)component != (Object)null && (Object)(object)component.BuildTitle != (Object)null && !string.IsNullOrEmpty(((TMP_Text)component.BuildTitle).text))
				{
					SPDAVersionTooltip component2 = ((Component)component).GetComponent<SPDAVersionTooltip>();
					if ((Object)(object)component2 == (Object)null)
					{
						string text = ((TMP_Text)component.BuildTitle).text;
						SPDAVersionTooltip sPDAVersionTooltip = ((Component)component).gameObject.AddComponent<SPDAVersionTooltip>();
						sPDAVersionTooltip.Initialize(pageKey, text);
						_addedComponents.Add((Component)(object)sPDAVersionTooltip);
						num++;
					}
				}
			}
			return num;
		}

		private int AddTooltipsToMemory(StationpediaCategory category, string pageKey)
		{
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Expected O, but got Unknown
			if ((Object)(object)category == (Object)null || (Object)(object)category.Contents == (Object)null)
			{
				return 0;
			}
			int num = 0;
			foreach (Transform item in (Transform)category.Contents)
			{
				Transform val = item;
				SPDAGeneric component = ((Component)val).GetComponent<SPDAGeneric>();
				if ((Object)(object)component != (Object)null && (Object)(object)component.Header != (Object)null && !string.IsNullOrEmpty(((TMP_Text)component.Header).text))
				{
					SPDAMemoryTooltip component2 = ((Component)component).GetComponent<SPDAMemoryTooltip>();
					if ((Object)(object)component2 == (Object)null)
					{
						string text = ((TMP_Text)component.Header).text;
						SPDAMemoryTooltip sPDAMemoryTooltip = ((Component)component).gameObject.AddComponent<SPDAMemoryTooltip>();
						sPDAMemoryTooltip.Initialize(pageKey, text);
						_addedComponents.Add((Component)(object)sPDAMemoryTooltip);
						num++;
					}
				}
			}
			return num;
		}

		private int AddTooltipsToProperties(UniversalPage universalPage)
		{
			if ((Object)(object)universalPage == (Object)null)
			{
				return 0;
			}
			int added = 0;
			AddPropertyTooltip(universalPage.FlashPointText, "Flashpoint");
			AddPropertyTooltip(universalPage.AutoIgniteText, "Autoignition");
			AddPropertyTooltip(universalPage.HeatTransferConvectionText, "Thermal Convection");
			AddPropertyTooltip(universalPage.HeatTransferRadiationText, "Thermal Radiation");
			AddPropertyTooltip(universalPage.SolarHeatingFactorText, "Solar Heating");
			AddPropertyTooltip(universalPage.SpecificHeat, "Specific Heat");
			AddPropertyTooltip(universalPage.FreezeTemperature, "Freeze Temperature");
			AddPropertyTooltip(universalPage.BoilingTemperature, "Boiling Temperature");
			AddPropertyTooltip(universalPage.MaxLiquidTemperature, "Max Liquid Temperature");
			AddPropertyTooltip(universalPage.MinLiquidPressure, "Min Liquid Pressure");
			AddPropertyTooltip(universalPage.LatentHeat, "Latent Heat");
			AddPropertyTooltip(universalPage.MolesPerLitre, "Moles Per Litre");
			AddPropertyTooltip(universalPage.MaxPressure, "Max Pressure");
			AddPropertyTooltip(universalPage.Volume, "Volume");
			AddPropertyTooltip(universalPage.DeviceBasePower, "Base Power");
			AddPropertyTooltip(universalPage.DevicePowerStorage, "Power Storage");
			AddPropertyTooltip(universalPage.DevicePowerGeneration, "Power Generation");
			AddPropertyTooltip(universalPage.GrowthTime, "Growth Time");
			AddPropertyTooltip(universalPage.Nutrition, "Nutrition");
			AddPropertyTooltip(universalPage.NutritionQuality, "Nutrition Quality");
			AddPropertyTooltip(universalPage.MoodBonus, "Mood Bonus");
			AddPropertyTooltip(universalPage.PlaceableInRocket, "Placeable In Rocket");
			AddPropertyTooltip(universalPage.RocketMass, "Rocket Mass");
			return added;
			void AddPropertyTooltip(TextMeshProUGUI textElement, string propertyName)
			{
				//IL_008b: Unknown result type (might be due to invalid IL or missing references)
				if ((Object)(object)textElement != (Object)null && (Object)(object)((Component)textElement).gameObject != (Object)null && !string.IsNullOrEmpty(((TMP_Text)textElement).text))
				{
					Transform parent = ((TMP_Text)textElement).transform.parent;
					GameObject val = ((parent != null) ? ((Component)parent).gameObject : null);
					if ((Object)(object)val != (Object)null)
					{
						Graphic component = val.GetComponent<Graphic>();
						if ((Object)(object)component == (Object)null)
						{
							Image val2 = val.AddComponent<Image>();
							((Graphic)val2).color = new Color(0f, 0f, 0f, 0f);
							((Graphic)val2).raycastTarget = true;
						}
						SPDAPropertyTooltip component2 = val.GetComponent<SPDAPropertyTooltip>();
						if ((Object)(object)component2 == (Object)null)
						{
							SPDAPropertyTooltip sPDAPropertyTooltip = val.AddComponent<SPDAPropertyTooltip>();
							sPDAPropertyTooltip.Initialize(propertyName);
							_addedComponents.Add((Component)(object)sPDAPropertyTooltip);
							added++;
						}
					}
				}
			}
		}

		public static LogicDescription GetLogicDescription(string deviceKey, string logicTypeName)
		{
			string key = CleanLogicTypeName(logicTypeName);
			if (!string.IsNullOrEmpty(deviceKey) && DeviceDatabase != null && DeviceDatabase.TryGetValue(deviceKey, out var value) && value.logicDescriptions != null && value.logicDescriptions.TryGetValue(key, out var value2))
			{
				return value2;
			}
			if (GenericDescriptions?.logic != null && GenericDescriptions.logic.TryGetValue(key, out var value3))
			{
				return new LogicDescription
				{
					dataType = "Varies",
					range = "Device-specific",
					description = value3
				};
			}
			return null;
		}

		public static SlotDescription GetSlotDescription(string deviceKey, string slotName)
		{
			string text = CleanLogicTypeName(slotName);
			if (!string.IsNullOrEmpty(deviceKey) && DeviceDatabase != null && DeviceDatabase.TryGetValue(deviceKey, out var value) && value.slotDescriptions != null && value.slotDescriptions.TryGetValue(text, out var value2))
			{
				return value2;
			}
			if (GenericDescriptions?.slotTypes != null && GenericDescriptions.slotTypes.TryGetValue(text, out var value3))
			{
				return new SlotDescription
				{
					slotType = text,
					description = value3
				};
			}
			if (GenericDescriptions?.slots != null && GenericDescriptions.slots.TryGetValue(text, out var value4))
			{
				return new SlotDescription
				{
					slotType = text,
					description = value4
				};
			}
			return null;
		}

		public static ModeDescription GetModeDescription(string deviceKey, string modeValue)
		{
			string text = CleanLogicTypeName(modeValue);
			if (!string.IsNullOrEmpty(deviceKey) && DeviceDatabase != null && DeviceDatabase.TryGetValue(deviceKey, out var value) && value.modeDescriptions != null && value.modeDescriptions.TryGetValue(text, out var value2))
			{
				return value2;
			}
			if (GenericDescriptions?.modes != null && GenericDescriptions.modes.TryGetValue(text, out var value3))
			{
				return new ModeDescription
				{
					modeValue = text,
					description = value3
				};
			}
			return null;
		}

		public static LogicDescription GetConnectionDescription(string deviceKey, string connectionType)
		{
			string key = CleanLogicTypeName(connectionType);
			if (GenericDescriptions?.connections != null && GenericDescriptions.connections.TryGetValue(key, out var value))
			{
				return new LogicDescription
				{
					dataType = "Connection",
					range = "N/A",
					description = value
				};
			}
			return null;
		}

		public static string GetSlotLogicDescription(string slotLogicType)
		{
			string key = CleanLogicTypeName(slotLogicType);
			if (GenericDescriptions?.slots != null && GenericDescriptions.slots.TryGetValue(key, out var value))
			{
				return value;
			}
			return null;
		}

		public static PropertyDescription GetPropertyDescription(string propertyName)
		{
			string key = CleanLogicTypeName(propertyName);
			if (GenericDescriptions?.properties != null && GenericDescriptions.properties.TryGetValue(key, out var value))
			{
				return value;
			}
			if (GenericDescriptions?.AdditionalData != null && GenericDescriptions.AdditionalData.TryGetValue(key, out var value2))
			{
				string text = ((object)value2)?.ToString();
				if (!string.IsNullOrEmpty(text))
				{
					return new PropertyDescription
					{
						description = text
					};
				}
			}
			return null;
		}

		public static VersionDescription GetVersionDescription(string deviceKey, string versionName)
		{
			string key = CleanLogicTypeName(versionName);
			if (!string.IsNullOrEmpty(deviceKey) && DeviceDatabase != null && DeviceDatabase.TryGetValue(deviceKey, out var value) && value.versionDescriptions != null && value.versionDescriptions.TryGetValue(key, out var value2))
			{
				return value2;
			}
			if (GenericDescriptions?.versions != null && GenericDescriptions.versions.TryGetValue(key, out var value3))
			{
				return new VersionDescription
				{
					description = value3
				};
			}
			return null;
		}

		public static MemoryDescription GetMemoryDescription(string deviceKey, string instructionName)
		{
			string key = CleanLogicTypeName(instructionName);
			if (!string.IsNullOrEmpty(deviceKey) && DeviceDatabase != null && DeviceDatabase.TryGetValue(deviceKey, out var value) && value.memoryDescriptions != null && value.memoryDescriptions.TryGetValue(key, out var value2))
			{
				return value2;
			}
			if (GenericDescriptions?.memory != null && GenericDescriptions.memory.TryGetValue(key, out var value3))
			{
				return value3;
			}
			return null;
		}

		private static string CleanLogicTypeName(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				return name;
			}
			string text = Regex.Replace(name, "<[^>]+>", "");
			return text.Trim();
		}

		private void Update()
		{
			if (Input.GetKeyDown((KeyCode)285))
			{
				StationPlannerWindow.Toggle();
			}
			StationPlannerWindow.UpdateWindow();
		}

		private void OnDestroy()
		{
			try
			{
				StationPlannerWindow.Cleanup();
			}
			catch (Exception ex)
			{
				ManualLogSource log = Log;
				if (log != null)
				{
					log.LogWarning((object)("Error cleaning up Station Planner: " + ex.Message));
				}
			}
			try
			{
				UIAssetInspector.Cleanup();
			}
			catch (Exception ex2)
			{
				ManualLogSource log2 = Log;
				if (log2 != null)
				{
					log2.LogWarning((object)("Error cleaning up UI Asset Inspector: " + ex2.Message));
				}
			}
			((MonoBehaviour)this).StopAllCoroutines();
			_monitorCoroutine = null;
			if (_harmony != null)
			{
				_harmony.UnpatchSelf();
				_harmony = null;
			}
			if (_addedComponents != null)
			{
				foreach (Component addedComponent in _addedComponents)
				{
					if ((Object)(object)addedComponent != (Object)null)
					{
						try
						{
							Object.Destroy((Object)(object)addedComponent);
						}
						catch
						{
						}
					}
				}
				_addedComponents.Clear();
			}
			_addedComponents = new List<Component>();
			if (_createdGameObjects != null)
			{
				foreach (GameObject createdGameObject in _createdGameObjects)
				{
					if ((Object)(object)createdGameObject != (Object)null)
					{
						try
						{
							Object.Destroy((Object)(object)createdGameObject);
						}
						catch
						{
						}
					}
				}
				_createdGameObjects.Clear();
			}
			_createdGameObjects = new List<GameObject>();
			ShowTooltip = false;
			CurrentTooltipText = "";
			if ((Object)(object)_tooltipBackground != (Object)null)
			{
				Object.Destroy((Object)(object)_tooltipBackground);
				_tooltipBackground = null;
			}
			_tooltipStyle = null;
			_stylesInitialized = false;
			_lastPageKey = "";
			_stationpediaFound = false;
			_initialized = false;
			Instance = null;
			DeviceDatabase = null;
			GenericDescriptions = null;
			ConsoleWindow.Print("[Stationpedia Ascended] Cleaned up", ConsoleColor.White, false, true);
		}
	}
}
namespace StationpediaAscended.UI
{
	public class CategoryHeaderHandler : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler
	{
		public StationpediaCategory Category;

		public TextMeshProUGUI TitleText;

		public IconAnimator IconAnimator;

		private string _originalTitleMarkup;

		private bool _isHovering = false;

		private static readonly Color DimMultiplier = new Color(0.7f, 0.7f, 0.7f, 1f);

		public void Initialize(StationpediaCategory category, IconAnimator iconAnimator)
		{
			Category = category;
			TitleText = category.Title;
			IconAnimator = iconAnimator;
			if ((Object)(object)TitleText != (Object)null)
			{
				_originalTitleMarkup = ((TMP_Text)TitleText).text;
			}
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if ((Object)(object)Category != (Object)null)
			{
				Category.ToggleContentVisibility();
			}
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			_isHovering = true;
			if ((Object)(object)TitleText != (Object)null)
			{
				_originalTitleMarkup = ((TMP_Text)TitleText).text;
				ApplyDimmedTitle();
			}
			if ((Object)(object)IconAnimator != (Object)null && (Object)(object)IconAnimator.TargetImage != (Object)null)
			{
				((MonoBehaviour)this).StartCoroutine(AnimateIconHover(entering: true));
			}
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			_isHovering = false;
			if ((Object)(object)TitleText != (Object)null && !string.IsNullOrEmpty(_originalTitleMarkup))
			{
				((TMP_Text)TitleText).text = _originalTitleMarkup;
			}
			if ((Object)(object)IconAnimator != (Object)null && (Object)(object)IconAnimator.TargetImage != (Object)null)
			{
				((MonoBehaviour)this).StartCoroutine(AnimateIconHover(entering: false));
			}
		}

		private void ApplyDimmedTitle()
		{
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)TitleText == (Object)null)
			{
				return;
			}
			string originalTitleMarkup = _originalTitleMarkup;
			int num = originalTitleMarkup.IndexOf("<color=#");
			if (num < 0)
			{
				return;
			}
			int num2 = num + 7;
			int num3 = originalTitleMarkup.IndexOf(">", num2);
			if (num3 > num2)
			{
				string text = originalTitleMarkup.Substring(num2 + 1, num3 - num2 - 1);
				Color val = default(Color);
				if (ColorUtility.TryParseHtmlString("#" + text, ref val))
				{
					Color val2 = default(Color);
					((Color)(ref val2))..ctor(val.r * DimMultiplier.r, val.g * DimMultiplier.g, val.b * DimMultiplier.b, val.a);
					string text2 = ColorUtility.ToHtmlStringRGB(val2);
					((TMP_Text)TitleText).text = originalTitleMarkup.Substring(0, num2 + 1) + text2 + originalTitleMarkup.Substring(num3);
				}
			}
		}

		private IEnumerator AnimateIconHover(bool entering)
		{
			if (!((Object)(object)IconAnimator == (Object)null) && !((Object)(object)IconAnimator.TargetImage == (Object)null))
			{
				Image targetImage = IconAnimator.TargetImage;
				Vector3 startScale = ((Component)targetImage).transform.localScale;
				Vector3 targetScale = (entering ? (Vector3.one * 1.15f) : Vector3.one);
				float duration = 0.1f;
				float elapsed = 0f;
				while (elapsed < duration)
				{
					elapsed += Time.unscaledDeltaTime;
					float t = elapsed / duration;
					float eased = 1f - (1f - t) * (1f - t);
					((Component)targetImage).transform.localScale = Vector3.Lerp(startScale, targetScale, eased);
					yield return null;
				}
				((Component)targetImage).transform.localScale = targetScale;
			}
		}

		private void OnDisable()
		{
			if (_isHovering && (Object)(object)TitleText != (Object)null && !string.IsNullOrEmpty(_originalTitleMarkup))
			{
				((TMP_Text)TitleText).text = _originalTitleMarkup;
			}
			_isHovering = false;
		}
	}
	public static class HomePageLayoutManager
	{
		[CompilerGenerated]
		private static class <>O
		{
			public static UnityAction <0>__OnSurvivalManualClick;

			public static UnityAction <1>__OnGameMechanicsClick;
		}

		private static GameObject _customButtonContainer;

		private static GameObject _survivalManualButton;

		private static GameObject _gameMechanicsButton;

		public static void Initialize()
		{
			//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fc: Expected O, but got Unknown
			//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c4: Expected O, but got Unknown
			//IL_020d: Unknown result type (might be due to invalid IL or missing references)
			//IL_021c: Unknown result type (might be due to invalid IL or missing references)
			//IL_022b: Unknown result type (might be due to invalid IL or missing references)
			//IL_023a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0249: Unknown result type (might be due to invalid IL or missing references)
			//IL_0270: Unknown result type (might be due to invalid IL or missing references)
			//IL_028e: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_048a: Unknown result type (might be due to invalid IL or missing references)
			//IL_048f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0495: Expected O, but got Unknown
			//IL_04dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_04e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_04e8: Expected O, but got Unknown
			//IL_055a: Unknown result type (might be due to invalid IL or missing references)
			//IL_057b: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				Stationpedia instance = Stationpedia.Instance;
				if ((Object)(object)instance == (Object)null)
				{
					return;
				}
				FieldInfo field = typeof(Stationpedia).GetField("_homeGuideButtonContainer", BindingFlags.Instance | BindingFlags.NonPublic);
				if (field == null)
				{
					ManualLogSource log = StationpediaAscendedMod.Log;
					if (log != null)
					{
						log.LogWarning((object)"Could not find _homeGuideButtonContainer field");
					}
					return;
				}
				object value = field.GetValue(instance);
				GameObject val = (GameObject)((value is GameObject) ? value : null);
				if ((Object)(object)val == (Object)null)
				{
					ManualLogSource log2 = StationpediaAscendedMod.Log;
					if (log2 != null)
					{
						log2.LogWarning((object)"_homeGuideButtonContainer is null");
					}
					return;
				}
				Transform parent = val.transform.parent;
				if ((Object)(object)parent == (Object)null)
				{
					ManualLogSource log3 = StationpediaAscendedMod.Log;
					if (log3 != null)
					{
						log3.LogWarning((object)"Guide button container has no parent");
					}
					return;
				}
				CleanupExistingButtons(parent, (Transform)(object)instance.HomePageContent);
				Button val2 = null;
				Button val3 = null;
				TextMeshProUGUI val4 = null;
				TextMeshProUGUI textTemplate = null;
				foreach (Transform item in val.transform)
				{
					Transform val5 = item;
					Button component = ((Component)val5).GetComponent<Button>();
					if ((Object)(object)component != (Object)null)
					{
						TextMeshProUGUI componentInChildren = ((Component)val5).GetComponentInChildren<TextMeshProUGUI>();
						if ((Object)(object)val2 == (Object)null)
						{
							val2 = component;
							val4 = componentInChildren;
						}
						else if ((Object)(object)val3 == (Object)null)
						{
							val3 = component;
							textTemplate = componentInChildren;
						}
					}
				}
				if ((Object)(object)val2 == (Object)null)
				{
					ManualLogSource log4 = StationpediaAscendedMod.Log;
					if (log4 != null)
					{
						log4.LogWarning((object)"Could not find Guide button in guide container");
					}
					return;
				}
				if ((Object)(object)val3 == (Object)null)
				{
					val3 = val2;
					textTemplate = val4;
				}
				_customButtonContainer = new GameObject("CustomButtonContainer_SPA");
				_customButtonContainer.transform.SetParent(parent, false);
				int siblingIndex = val.transform.GetSiblingIndex();
				_customButtonContainer.transform.SetSiblingIndex(siblingIndex);
				RectTransform component2 = val.GetComponent<RectTransform>();
				RectTransform val6 = _customButtonContainer.AddComponent<RectTransform>();
				val6.anchorMin = component2.anchorMin;
				val6.anchorMax = component2.anchorMax;
				val6.pivot = component2.pivot;
				val6.sizeDelta = component2.sizeDelta;
				val6.anchoredPosition = component2.anchoredPosition;
				RectTransform component3 = ((Component)val2).GetComponent<RectTransform>();
				float num = (((Object)(object)component3 != (Object)null) ? component3.sizeDelta.y : 108f);
				ManualLogSource log5 = StationpediaAscendedMod.Log;
				if (log5 != null)
				{
					log5.LogInfo((object)$"Container template size: {component2.sizeDelta}, Button height: {num}");
				}
				HorizontalLayoutGroup component4 = val.GetComponent<HorizontalLayoutGroup>();
				if ((Object)(object)component4 != (Object)null)
				{
					HorizontalLayoutGroup val7 = _customButtonContainer.AddComponent<HorizontalLayoutGroup>();
					((HorizontalOrVerticalLayoutGroup)val7).spacing = ((HorizontalOrVerticalLayoutGroup)component4).spacing;
					((LayoutGroup)val7).padding = ((LayoutGroup)component4).padding;
					((LayoutGroup)val7).childAlignment = ((LayoutGroup)component4).childAlignment;
					((HorizontalOrVerticalLayoutGroup)val7).childControlWidth = ((HorizontalOrVerticalLayoutGroup)component4).childControlWidth;
					((HorizontalOrVerticalLayoutGroup)val7).childControlHeight = ((HorizontalOrVerticalLayoutGroup)component4).childControlHeight;
					((HorizontalOrVerticalLayoutGroup)val7).childForceExpandWidth = ((HorizontalOrVerticalLayoutGroup)component4).childForceExpandWidth;
					((HorizontalOrVerticalLayoutGroup)val7).childForceExpandHeight = ((HorizontalOrVerticalLayoutGroup)component4).childForceExpandHeight;
					ManualLogSource log6 = StationpediaAscendedMod.Log;
					if (log6 != null)
					{
						log6.LogInfo((object)$"Container HLG: childControlHeight={((HorizontalOrVerticalLayoutGroup)component4).childControlHeight}, childForceExpandHeight={((HorizontalOrVerticalLayoutGroup)component4).childForceExpandHeight}");
					}
				}
				else
				{
					HorizontalLayoutGroup val8 = _customButtonContainer.AddComponent<HorizontalLayoutGroup>();
					((HorizontalOrVerticalLayoutGroup)val8).spacing = 10f;
					((LayoutGroup)val8).childAlignment = (TextAnchor)4;
					((HorizontalOrVerticalLayoutGroup)val8).childControlWidth = true;
					((HorizontalOrVerticalLayoutGroup)val8).childControlHeight = false;
					((HorizontalOrVerticalLayoutGroup)val8).childForceExpandWidth = true;
					((HorizontalOrVerticalLayoutGroup)val8).childForceExpandHeight = false;
				}
				LayoutElement component5 = val.GetComponent<LayoutElement>();
				if ((Object)(object)component5 != (Object)null)
				{
					LayoutElement val9 = _customButtonContainer.AddComponent<LayoutElement>();
					val9.preferredHeight = component5.preferredHeight;
					val9.flexibleWidth = component5.flexibleWidth;
					ManualLogSource log7 = StationpediaAscendedMod.Log;
					if (log7 != null)
					{
						log7.LogInfo((object)$"Container LE from template: preferredHeight={component5.preferredHeight}");
					}
				}
				else
				{
					LayoutElement val10 = _customButtonContainer.AddComponent<LayoutElement>();
					val10.preferredHeight = num;
					val10.flexibleWidth = 1f;
					ManualLogSource log8 = StationpediaAscendedMod.Log;
					if (log8 != null)
					{
						log8.LogInfo((object)$"Container LE created: preferredHeight={num}");
					}
				}
				GameObject gameObject = ((Component)val2).gameObject;
				object obj = <>O.<0>__OnSurvivalManualClick;
				if (obj == null)
				{
					UnityAction val11 = OnSurvivalManualClick;
					<>O.<0>__OnSurvivalManualClick = val11;
					obj = (object)val11;
				}
				_survivalManualButton = CloneButton(gameObject, "SurvivalManualButton_SPA", "Stationeers Survival Manual", (UnityAction)obj, val4);
				_survivalManualButton.transform.SetParent(_customButtonContainer.transform, false);
				GameObject gameObject2 = ((Component)val3).gameObject;
				object obj2 = <>O.<1>__OnGameMechanicsClick;
				if (obj2 == null)
				{
					UnityAction val12 = OnGameMechanicsClick;
					<>O.<1>__OnGameMechanicsClick = val12;
					obj2 = (object)val12;
				}
				_gameMechanicsButton = CloneButton(gameObject2, "GameMechanicsButton_SPA", "Game Mechanics", (UnityAction)obj2, textTemplate);
				_gameMechanicsButton.transform.SetParent(_customButtonContainer.transform, false);
				LayoutRebuilder.ForceRebuildLayoutImmediate(val6);
				LayoutRebuilder.ForceRebuildLayoutImmediate(((Component)parent).GetComponent<RectTransform>());
				RectTransform component6 = ((Component)val2).GetComponent<RectTransform>();
				RectTransform component7 = _survivalManualButton.GetComponent<RectTransform>();
				ManualLogSource log9 = StationpediaAscendedMod.Log;
				if (log9 != null)
				{
					log9.LogInfo((object)$"Guide button size: {((component6 != null) ? new Vector2?(component6.sizeDelta) : ((Vector2?)null))}, Survival button size: {((component7 != null) ? new Vector2?(component7.sizeDelta) : ((Vector2?)null))}");
				}
				LayoutElement component8 = ((Component)val2).GetComponent<LayoutElement>();
				LayoutElement component9 = _survivalManualButton.GetComponent<LayoutElement>();
				if ((Object)(object)component8 != (Object)null)
				{
					ManualLogSource log10 = StationpediaAscendedMod.Log;
					if (log10 != null)
					{
						log10.LogInfo((object)$"Guide LE: pref={component8.preferredWidth}x{component8.preferredHeight}, flex={component8.flexibleWidth}x{component8.flexibleHeight}");
					}
				}
				if ((Object)(object)component9 != (Object)null)
				{
					ManualLogSource log11 = StationpediaAscendedMod.Log;
					if (log11 != null)
					{
						log11.LogInfo((object)$"Survival LE: pref={component9.preferredWidth}x{component9.preferredHeight}, flex={component9.flexibleWidth}x{component9.flexibleHeight}");
					}
				}
				ManualLogSource log12 = StationpediaAscendedMod.Log;
				if (log12 != null)
				{
					log12.LogInfo((object)"Home page custom buttons created successfully");
				}
			}
			catch (Exception ex)
			{
				ManualLogSource log13 = StationpediaAscendedMod.Log;
				if (log13 != null)
				{
					log13.LogError((object)("Error initializing home page layout: " + ex.Message + "\n" + ex.StackTrace));
				}
			}
		}

		private static void CleanupExistingButtons(Transform guideParent, Transform homePageContent)
		{
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Expected O, but got Unknown
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Expected O, but got Unknown
			List<GameObject> list = new List<GameObject>();
			if ((Object)(object)guideParent != (Object)null)
			{
				foreach (Transform item in guideParent)
				{
					Transform val = item;
					if (((Object)val).name.Contains("CustomButtonContainer"))
					{
						list.Add(((Component)val).gameObject);
					}
				}
			}
			if ((Object)(object)homePageContent != (Object)null)
			{
				foreach (Transform item2 in homePageContent)
				{
					Transform val2 = item2;
					string name = ((Object)val2).name;
					if (!(name == "SurvivalManualButton") && !(name == "GameMechanicsButton") && !name.Contains("SurvivalManual") && !name.Contains("GameMechanics"))
					{
						continue;
					}
					Button component = ((Component)val2).GetComponent<Button>();
					if ((Object)(object)component != (Object)null)
					{
						TextMeshProUGUI componentInChildren = ((Component)val2).GetComponentInChildren<TextMeshProUGUI>();
						if ((Object)(object)componentInChildren != (Object)null && (((TMP_Text)componentInChildren).text.Contains("Survival Manual") || ((TMP_Text)componentInChildren).text.Contains("Game Mechanics")))
						{
							list.Add(((Component)val2).gameObject);
						}
					}
				}
			}
			foreach (GameObject item3 in list)
			{
				ManualLogSource log = StationpediaAscendedMod.Log;
				if (log != null)
				{
					log.LogInfo((object)("Removing duplicate button: " + ((Object)item3).name));
				}
				Object.DestroyImmediate((Object)(object)item3);
			}
			_customButtonContainer = null;
			_survivalManualButton = null;
			_gameMechanicsButton = null;
		}

		private static GameObject CloneButton(GameObject template, string name, string buttonText, UnityAction onClick, TextMeshProUGUI textTemplate)
		{
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Expected O, but got Unknown
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_0108: Unknown result type (might be due to invalid IL or missing references)
			//IL_0116: Unknown result type (might be due to invalid IL or missing references)
			//IL_0124: Unknown result type (might be due to invalid IL or missing references)
			//IL_0132: Unknown result type (might be due to invalid IL or missing references)
			//IL_021a: Unknown result type (might be due to invalid IL or missing references)
			//IL_023b: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = Object.Instantiate<GameObject>(template);
			((Object)val).name = name;
			TextMeshProUGUI componentInChildren = val.GetComponentInChildren<TextMeshProUGUI>();
			if ((Object)(object)componentInChildren != (Object)null)
			{
				((TMP_Text)componentInChildren).text = buttonText;
				((TMP_Text)componentInChildren).enableWordWrapping = true;
				((TMP_Text)componentInChildren).alignment = (TextAlignmentOptions)514;
				((TMP_Text)componentInChildren).overflowMode = (TextOverflowModes)3;
				if ((Object)(object)textTemplate != (Object)null)
				{
					((TMP_Text)componentInChildren).font = ((TMP_Text)textTemplate).font;
					((TMP_Text)componentInChildren).fontSize = ((TMP_Text)textTemplate).fontSize;
					((TMP_Text)componentInChildren).fontStyle = ((TMP_Text)textTemplate).fontStyle;
				}
			}
			Button component = val.GetComponent<Button>();
			if ((Object)(object)component != (Object)null)
			{
				component.onClick = new ButtonClickedEvent();
				((UnityEvent)component.onClick).AddListener(onClick);
				Navigation navigation = ((Selectable)component).navigation;
				((Navigation)(ref navigation)).mode = (Mode)0;
				((Selectable)component).navigation = navigation;
			}
			RectTransform component2 = template.GetComponent<RectTransform>();
			RectTransform component3 = val.GetComponent<RectTransform>();
			if ((Object)(object)component2 != (Object)null && (Object)(object)component3 != (Object)null)
			{
				component3.anchorMin = component2.anchorMin;
				component3.anchorMax = component2.anchorMax;
				component3.pivot = component2.pivot;
				component3.sizeDelta = component2.sizeDelta;
				component3.anchoredPosition = component2.anchoredPosition;
			}
			LayoutElement component4 = template.GetComponent<LayoutElement>();
			LayoutElement val2 = val.GetComponent<LayoutElement>();
			if ((Object)(object)component4 != (Object)null)
			{
				if ((Object)(object)val2 == (Object)null)
				{
					val2 = val.AddComponent<LayoutElement>();
				}
				val2.minWidth = component4.minWidth;
				val2.minHeight = component4.minHeight;
				val2.preferredWidth = component4.preferredWidth;
				val2.preferredHeight = component4.preferredHeight;
				val2.flexibleWidth = component4.flexibleWidth;
				val2.flexibleHeight = component4.flexibleHeight;
				val2.layoutPriority = component4.layoutPriority;
			}
			else if ((Object)(object)component2 != (Object)null)
			{
				if ((Object)(object)val2 == (Object)null)
				{
					val2 = val.AddComponent<LayoutElement>();
				}
				val2.flexibleWidth = 1f;
				val2.preferredHeight = component2.sizeDelta.y;
				ManualLogSource log = StationpediaAscendedMod.Log;
				if (log != null)
				{
					log.LogInfo((object)$"Added LayoutElement with preferredHeight={component2.sizeDelta.y} from RectTransform");
				}
			}
			return val;
		}

		private static void OnSurvivalManualClick()
		{
			ManualLogSource log = StationpediaAscendedMod.Log;
			if (log != null)
			{
				log.LogInfo((object)("Survival Manual clicked. CurrentPageKey: " + Stationpedia.CurrentPageKey));
			}
			Stationpedia instance = Stationpedia.Instance;
			if (instance != null)
			{
				instance.SetPage("SurvivalManual", true);
			}
		}

		private static void OnGameMechanicsClick()
		{
			ManualLogSource log = StationpediaAscendedMod.Log;
			if (log != null)
			{
				log.LogInfo((object)("Game Mechanics clicked. CurrentPageKey: " + Stationpedia.CurrentPageKey));
			}
			Stationpedia instance = Stationpedia.Instance;
			if (instance != null)
			{
				instance.SetPage("GameMechanics", true);
			}
		}

		public static void ModifyGuideLoreLayout(RectTransform loreGuideContents)
		{
		}

		public static void Cleanup()
		{
			if ((Object)(object)_survivalManualButton != (Object)null)
			{
				Object.Destroy((Object)(object)_survivalManualButton);
				_survivalManualButton = null;
			}
			if ((Object)(object)_gameMechanicsButton != (Object)null)
			{
				Object.Destroy((Object)(object)_gameMechanicsButton);
				_gameMechanicsButton = null;
			}
			if ((Object)(object)_customButtonContainer != (Object)null)
			{
				Object.Destroy((Object)(object)_customButtonContainer);
				_customButtonContainer = null;
			}
		}
	}
	public class IconAnimator : MonoBehaviour
	{
		public Image TargetImage;

		public Sprite ExpandedSprite;

		public Sprite CollapsedSprite;

		public float AnimationDuration = 0.15f;

		private bool _isExpanded = false;

		private Coroutine _currentAnimation;

		public void SetState(bool expanded, bool animate = true)
		{
			if (_isExpanded != expanded)
			{
				_isExpanded = expanded;
				if (_currentAnimation != null)
				{
					((MonoBehaviour)this).StopCoroutine(_currentAnimation);
				}
				if (animate && ((Component)this).gameObject.activeInHierarchy)
				{
					_currentAnimation = ((MonoBehaviour)this).StartCoroutine(AnimateTransition());
				}
				else
				{
					ApplyState();
				}
			}
		}

		public void Initialize(bool expanded)
		{
			_isExpanded = expanded;
			ApplyState();
		}

		private void ApplyState()
		{
			if (!((Object)(object)TargetImage == (Object)null))
			{
				if (_isExpanded && (Object)(object)ExpandedSprite != (Object)null)
				{
					TargetImage.sprite = ExpandedSprite;
				}
				else if (!_isExpanded && (Object)(object)CollapsedSprite != (Object)null)
				{
					TargetImage.sprite = CollapsedSprite;
				}
			}
		}

		private IEnumerator AnimateTransition()
		{
			if (!((Object)(object)TargetImage == (Object)null))
			{
				Vector3 originalScale = ((Component)TargetImage).transform.localScale;
				float halfDuration = AnimationDuration / 2f;
				float elapsed = 0f;
				while (elapsed < halfDuration)
				{
					elapsed += Time.unscaledDeltaTime;
					float t = elapsed / halfDuration;
					float eased = 1f - (1f - t) * (1f - t);
					((Component)TargetImage).transform.localScale = Vector3.Lerp(originalScale, originalScale * 0.7f, eased);
					yield return null;
				}
				ApplyState();
				elapsed = 0f;
				while (elapsed < halfDuration)
				{
					elapsed += Time.unscaledDeltaTime;
					float t2 = elapsed / halfDuration;
					float eased2 = ((t2 < 0.5f) ? (2f * t2 * t2) : (1f - Mathf.Pow(-2f * t2 + 2f, 2f) / 2f));
					((Component)TargetImage).transform.localScale = Vector3.Lerp(originalScale * 0.7f, originalScale, eased2);
					yield return null;
				}
				((Component)TargetImage).transform.localScale = originalScale;
				_currentAnimation = null;
			}
		}

		private void OnDisable()
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)TargetImage != (Object)null)
			{
				((Component)TargetImage).transform.localScale = Vector3.one;
			}
			_currentAnimation = null;
		}
	}
	public class TocLinkHandler : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerMoveHandler, IPointerExitHandler
	{
		public TextMeshProUGUI TextComponent;

		private static Dictionary<string, RectTransform> _sectionRegistry = new Dictionary<string, RectTransform>();

		private static Dictionary<string, StationpediaCategory> _categoryRegistry = new Dictionary<string, StationpediaCategory>();

		private static Dictionary<string, string> _parentRegistry = new Dictionary<string, string>();

		private int _lastHoveredLinkIndex = -1;

		private static readonly Color NormalLinkColor = new Color(1f, 1f, 1f, 1f);

		private static readonly Color HoverLinkColor = new Color(0.6f, 0.6f, 0.6f, 1f);

		public static void RegisterSection(string tocId, RectTransform rectTransform, StationpediaCategory category = null, string parentTocId = null)
		{
			if (!string.IsNullOrEmpty(tocId))
			{
				_sectionRegistry[tocId] = rectTransform;
				if ((Object)(object)category != (Object)null)
				{
					_categoryRegistry[tocId] = category;
				}
				if (!string.IsNullOrEmpty(parentTocId))
				{
					_parentRegistry[tocId] = parentTocId;
				}
			}
		}

		public static void ClearRegistry()
		{
			_sectionRegistry.Clear();
			_categoryRegistry.Clear();
			_parentRegistry.Clear();
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)TextComponent == (Object)null)
			{
				return;
			}
			int num = TMP_TextUtilities.FindIntersectingLink((TMP_Text)(object)TextComponent, Vector2.op_Implicit(eventData.position), (Camera)null);
			if (num == -1)
			{
				return;
			}
			TMP_LinkInfo val = ((TMP_Text)TextComponent).textInfo.linkInfo[num];
			string linkID = ((TMP_LinkInfo)(ref val)).GetLinkID();
			if (linkID.StartsWith("toc_"))
			{
				string sectionId = linkID.Substring(4);
				ScrollToSection(sectionId);
			}
			else
			{
				if (string.IsNullOrEmpty(linkID) || !(linkID != "Clipboard"))
				{
					return;
				}
				try
				{
					Stationpedia instance = Stationpedia.Instance;
					if (instance != null)
					{
						instance.SetPage(linkID, true);
					}
				}
				catch (Exception ex)
				{
					ManualLogSource log = StationpediaAscendedMod.Log;
					if (log != null)
					{
						log.LogWarning((object)("Failed to navigate to page '" + linkID + "': " + ex.Message));
					}
				}
			}
		}

		public void OnPointerMove(PointerEventData eventData)
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)TextComponent == (Object)null)
			{
				return;
			}
			int num = TMP_TextUtilities.FindIntersectingLink((TMP_Text)(object)TextComponent, Vector2.op_Implicit(eventData.position), (Camera)null);
			if (num != _lastHoveredLinkIndex)
			{
				if (_lastHoveredLinkIndex >= 0)
				{
					ClearLinkHighlight();
				}
				if (num >= 0)
				{
					ApplyLinkHighlight(num);
				}
				_lastHoveredLinkIndex = num;
			}
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (_lastHoveredLinkIndex >= 0)
			{
				ClearLinkHighlight();
				_lastHoveredLinkIndex = -1;
			}
		}

		private void ApplyLinkHighlight(int linkIndex)
		{
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_0123: Unknown result type (might be due to invalid IL or missing references)
			//IL_0129: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00df: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0109: Unknown result type (might be due to invalid IL or missing references)
			//IL_010b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0116: Unknown result type (might be due to invalid IL or missing references)
			//IL_0118: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)TextComponent == (Object)null || linkIndex < 0 || linkIndex >= ((TMP_Text)TextComponent).textInfo.linkCount)
			{
				return;
			}
			try
			{
				TMP_LinkInfo val = ((TMP_Text)TextComponent).textInfo.linkInfo[linkIndex];
				for (int i = val.linkTextfirstCharacterIndex; i < val.linkTextfirstCharacterIndex + val.linkTextLength && i < ((TMP_Text)TextComponent).textInfo.characterCount; i++)
				{
					TMP_CharacterInfo val2 = ((TMP_Text)TextComponent).textInfo.characterInfo[i];
					if (val2.isVisible)
					{
						int materialReferenceIndex = val2.materialReferenceIndex;
						int vertexIndex = val2.vertexIndex;
						Color32[] colors = ((TMP_Text)TextComponent).textInfo.meshInfo[materialReferenceIndex].colors32;
						colors[vertexIndex + 3] = (colors[vertexIndex + 2] = (colors[vertexIndex + 1] = (colors[vertexIndex] = Color32.op_Implicit(HoverLinkColor))));
					}
				}
				((TMP_Text)TextComponent).UpdateVertexData((TMP_VertexDataUpdateFlags)16);
			}
			catch
			{
			}
		}

		private void ClearLinkHighlight()
		{
			if ((Object)(object)TextComponent != (Object)null)
			{
				((TMP_Text)TextComponent).ForceMeshUpdate(false, false);
			}
		}

		public static void ScrollToSectionStatic(string sectionId)
		{
			if (!_sectionRegistry.TryGetValue(sectionId, out var value))
			{
				ConsoleWindow.Print("[Stationpedia Ascended] TOC target not found: " + sectionId, ConsoleColor.White, false, true);
				return;
			}
			ExpandParentChainStatic(sectionId);
			if (_categoryRegistry.TryGetValue(sectionId, out var value2) && (Object)(object)value2.Contents != (Object)null && !((Component)value2.Contents).gameObject.activeSelf)
			{
				value2.ToggleContentVisibility();
			}
			if ((Object)(object)StationpediaAscendedMod.Instance != (Object)null)
			{
				((MonoBehaviour)StationpediaAscendedMod.Instance).StartCoroutine(ScrollToTargetCoroutineStatic(value));
			}
		}

		private void ScrollToSection(string sectionId)
		{
			ScrollToSectionStatic(sectionId);
		}

		private static void ExpandParentChainStatic(string tocId)
		{
			List<string> list = new List<string>();
			string key = tocId;
			string value;
			while (_parentRegistry.TryGetValue(key, out value))
			{
				list.Insert(0, value);
				key = value;
			}
			foreach (string item in list)
			{
				if (_categoryRegistry.TryGetValue(item, out var value2) && (Object)(object)value2.Contents != (Object)null && !((Component)value2.Contents).gameObject.activeSelf)
				{
					value2.ToggleContentVisibility();
				}
			}
		}

		private void ExpandParentChain(string tocId)
		{
			ExpandParentChainStatic(tocId);
		}

		private static IEnumerator ScrollToTargetCoroutineStatic(RectTransform target)
		{
			yield return null;
			yield return null;
			yield return null;
			Canvas.ForceUpdateCanvases();
			Stationpedia stationpedia = Stationpedia.Instance;
			if ((Object)(object)stationpedia == (Object)null)
			{
				yield break;
			}
			ScrollRect scrollRect = ((Component)stationpedia).GetComponentInChildren<ScrollRect>();
			if ((Object)(object)scrollRect == (Object)null || (Object)(object)scrollRect.content == (Object)null || (Object)(object)scrollRect.viewport == (Object)null)
			{
				yield break;
			}
			LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
			yield return null;
			Rect rect = scrollRect.content.rect;
			float contentHeight = ((Rect)(ref rect)).height;
			rect = scrollRect.viewport.rect;
			float viewportHeight = ((Rect)(ref rect)).height;
			if (!(contentHeight <= viewportHeight))
			{
				Vector3[] targetCorners = (Vector3[])(object)new Vector3[4];
				target.GetWorldCorners(targetCorners);
				Vector3 targetTopWorld = targetCorners[1];
				float targetFromTop = 0f - ((Transform)scrollRect.content).InverseTransformPoint(targetTopWorld).y;
				float topMargin = 10f;
				targetFromTop -= topMargin;
				targetFromTop = Mathf.Max(0f, targetFromTop);
				float scrollableHeight = contentHeight - viewportHeight;
				float targetNormalizedPos = 1f - targetFromTop / scrollableHeight;
				targetNormalizedPos = Mathf.Clamp01(targetNormalizedPos);
				float startPos = scrollRect.verticalNormalizedPosition;
				float duration = 0.3f;
				float elapsed = 0f;
				while (elapsed < duration)
				{
					elapsed += Time.unscaledDeltaTime;
					float t = elapsed / duration;
					float eased = 1f - Mathf.Pow(1f - t, 3f);
					scrollRect.verticalNormalizedPosition = Mathf.Lerp(startPos, targetNormalizedPos, eased);
					yield return null;
				}
				scrollRect.verticalNormalizedPosition = targetNormalizedPos;
			}
		}
	}
	public static class VanillaModeManager
	{
		public static bool IsVanillaMode { get; private set; } = true;

		public static event Action<bool> OnVanillaModeChanged;

		public static void Toggle()
		{
			IsVanillaMode = !IsVanillaMode;
			VanillaModeManager.OnVanillaModeChanged?.Invoke(IsVanillaMode);
			ConsoleWindow.Print("[Stationpedia Ascended] Mode: " + (IsVanillaMode ? "Vanilla" : "Ascended"), ConsoleColor.White, false, true);
		}

		public static void SetVanillaMode(bool vanilla)
		{
			if (IsVanillaMode != vanilla)
			{
				IsVanillaMode = vanilla;
				VanillaModeManager.OnVanillaModeChanged?.Invoke(IsVanillaMode);
			}
		}

		public static string GetTitleColor(int depth)
		{
			if (IsVanillaMode)
			{
				return "#FFFFFF";
			}
			return depth switch
			{
				1 => "#E09030", 
				0 => "#FF7A18", 
				_ => "#C08040", 
			};
		}

		public static Color GetBackgroundColor(string customColor = null)
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			if (IsVanillaMode)
			{
				return new Color(0.1f, 0.1f, 0.1f, 0.9f);
			}
			Color result = default(Color);
			if (!string.IsNullOrEmpty(customColor) && ColorUtility.TryParseHtmlString(customColor, ref result))
			{
				return result;
			}
			return new Color(0.06f, 0.12f, 0.22f, 0.92f);
		}
	}
}
namespace StationpediaAscended.UI.StationPlanner
{
	public enum LineStyle
	{
		Normal,
		H1,
		H2,
		H3,
		Bullet,
		Checkbox,
		CheckboxChecked,
		Strikethrough
	}
	public class EditorLine
	{
		public string Text { get; set; } = "";

		public LineStyle Style { get; set; } = LineStyle.Normal;
	}
	[Serializable]
	public class EditorLineData
	{
		public string text;

		public int style;
	}
	[Serializable]
	public class BlockEditorDocument
	{
		public List<EditorLineData> lines = new List<EditorLineData>();
	}
	public class BlockEditor : MonoBehaviour
	{
		private const string BUNDLE_NAME = "stationpediaascended_ui";

		private const string PREFAB_NAME = "SA_TMPInputField_Base";

		private const string H1_OPEN = "<size=150%><b>";

		private const string H1_CLOSE = "</b></size>";

		private const string H2_OPEN = "<size=130%><b>";

		private const string H2_CLOSE = "</b></size>";

		private const string H3_OPEN = "<size=115%><b>";

		private const string H3_CLOSE = "</b></size>";

		private const string BULLET_PREFIX = "• ";

		private const string CHECKBOX_UNCHECKED = "  <color=#FBB03B>☐</color> ";

		private const string CHECKBOX_CHECKED_OPEN = "  <color=#888888>☑</color> <s><color=#888888>";

		private const string CHECKBOX_CHECKED_CLOSE = "</color></s>";

		private const string STRIKETHROUGH_OPEN = "<s><color=#888888>";

		private const string STRIKETHROUGH_CLOSE = "</color></s>";

		private TMP_InputField _inputField;

		private RectTransform _inputFieldRect;

		private GameObject _prefabInstance;

		private List<LineStyle> _lineStyles = new List<LineStyle>();

		private string _previousPlainText = "";

		private int _previousLineCount = 0;

		private bool _isUpdatingDisplay = false;

		private static AssetBundle _loadedBundle;

		private const char BULLET_CHAR = '•';

		public TMP_InputField InputField => _inputField;

		public bool IsInitialized => (Object)(object)_inputField != (Object)null;

		public int CurrentLineIndex
		{
			get
			{
				if ((Object)(object)_inputField == (Object)null || (Object)(object)_inputField.textComponent == (Object)null)
				{
					return 0;
				}
				_inputField.textComponent.ForceMeshUpdate(false, false);
				string parsedText = _inputField.textComponent.GetParsedText();
				if (string.IsNullOrEmpty(parsedText))
				{
					return 0;
				}
				int num = _inputField.caretPosition;
				if (num > parsedText.Length)
				{
					num = parsedText.Length;
				}
				if (num < 0)
				{
					num = 0;
				}
				int num2 = 0;
				for (int i = 0; i < num; i++)
				{
					if (parsedText[i] == '\n')
					{
						num2++;
					}
				}
				return num2;
			}
		}

		public event Action OnContentChanged;

		public bool Initialize(Transform parent)
		{
			try
			{
				_prefabInstance = LoadPrefabFromBundle(parent);
				if ((Object)(object)_prefabInstance == (Object)null)
				{
					ManualLogSource log = StationpediaAscendedMod.Log;
					if (log != null)
					{
						log.LogError((object)"[BlockEditor] Failed to load prefab from AssetBundle");
					}
					return false;
				}
				_inputField = _prefabInstance.GetComponent<TMP_InputField>();
				if ((Object)(object)_inputField == (Object)null)
				{
					ManualLogSource log2 = StationpediaAscendedMod.Log;
					if (log2 != null)
					{
						log2.LogError((object)"[BlockEditor] Prefab does not have TMP_InputField component");
					}
					Object.Destroy((Object)(object)_prefabInstance);
					return false;
				}
				_inputFieldRect = _prefabInstance.GetComponent<RectTransform>();
				ConfigureInputField();
				((UnityEvent<string>)(object)_inputField.onValueChanged).AddListener((UnityAction<string>)OnTextChanged);
				CheckboxClickHandler checkboxClickHandler = _prefabInstance.AddComponent<CheckboxClickHandler>();
				checkboxClickHandler.BlockEditor = this;
				_lineStyles.Add(LineStyle.Normal);
				_previousPlainText = "";
				_previousLineCount = 1;
				ManualLogSource log3 = StationpediaAscendedMod.Log;
				if (log3 != null)
				{
					log3.LogInfo((object)"[BlockEditor] Successfully initialized from prefab");
				}
				return true;
			}
			catch (Exception ex)
			{
				ManualLogSource log4 = StationpediaAscendedMod.Log;
				if (log4 != null)
				{
					log4.LogError((object)("[BlockEditor] Initialization failed: " + ex.Message + "\n" + ex.StackTrace));
				}
				return false;
			}
		}

		private GameObject LoadPrefabFromBundle(Transform parent)
		{
			string bundlePath = GetBundlePath();
			if (string.IsNullOrEmpty(bundlePath))
			{
				ManualLogSource log = StationpediaAscendedMod.Log;
				if (log != null)
				{
					log.LogError((object)"[BlockEditor] Could not find AssetBundle file");
				}
				return null;
			}
			ManualLogSource log2 = StationpediaAscendedMod.Log;
			if (log2 != null)
			{
				log2.LogInfo((object)("[BlockEditor] Loading AssetBundle from: " + bundlePath));
			}
			if ((Object)(object)_loadedBundle == (Object)null)
			{
				IEnumerable<AssetBundle> allLoadedAssetBundles = AssetBundle.GetAllLoadedAssetBundles();
				foreach (AssetBundle item in allLoadedAssetBundles)
				{
					if (((Object)item).name == "stationpediaascended_ui" || ((Object)item).name.Contains("stationpediaascended"))
					{
						ManualLogSource log3 = StationpediaAscendedMod.Log;
						if (log3 != null)
						{
							log3.LogInfo((object)("[BlockEditor] Found already-loaded bundle: " + ((Object)item).name));
						}
						_loadedBundle = item;
						break;
					}
				}
				if ((Object)(object)_loadedBundle == (Object)null)
				{
					_loadedBundle = AssetBundle.LoadFromFile(bundlePath);
					if ((Object)(object)_loadedBundle == (Object)null)
					{
						ManualLogSource log4 = StationpediaAscendedMod.Log;
						if (log4 != null)
						{
							log4.LogError((object)"[BlockEditor] Failed to load AssetBundle from file");
						}
						return null;
					}
				}
				ManualLogSource log5 = StationpediaAscendedMod.Log;
				if (log5 != null)
				{
					log5.LogInfo((object)"[BlockEditor] Bundle loaded. Contents:");
				}
				string[] allAssetNames = _loadedBundle.GetAllAssetNames();
				foreach (string text in allAssetNames)
				{
					ManualLogSource log6 = StationpediaAscendedMod.Log;
					if (log6 != null)
					{
						log6.LogInfo((object)("  - " + text));
					}
				}
			}
			GameObject val = _loadedBundle.LoadAsset<GameObject>("SA_TMPInputField_Base");
			if ((Object)(object)val == (Object)null)
			{
				val = _loadedBundle.LoadAsset<GameObject>("SA_TMPInputField_Base.prefab");
			}
			if ((Object)(object)val == (Object)null)
			{
				val = _loadedBundle.LoadAsset<GameObject>("assets/prefabs/ui/" + "SA_TMPInputField_Base".ToLower() + ".prefab");
			}
			if ((Object)(object)val == (Object)null)
			{
				ManualLogSource log7 = StationpediaAscendedMod.Log;
				if (log7 != null)
				{
					log7.LogError((object)"[BlockEditor] Prefab 'SA_TMPInputField_Base' not found in bundle. Available assets logged above.");
				}
				return null;
			}
			ManualLogSource log8 = StationpediaAscendedMod.Log;
			if (log8 != null)
			{
				log8.LogInfo((object)("[BlockEditor] Successfully loaded prefab: " + ((Object)val).name));
			}
			GameObject val2 = Object.Instantiate<GameObject>(val, parent);
			((Object)val2).name = "BlockEditor_InputField";
			return val2;
		}

		private string GetBundlePath()
		{
			string[] array = new string[4]
			{
				Path.Combine(Path.GetDirectoryName(typeof(BlockEditor).Assembly.Location), "stationpediaascended_ui"),
				Path.Combine(Paths.BepInExRootPath, "scripts", "stationpediaascended_ui"),
				Path.Combine(Paths.PluginPath, "StationpediaAscended", "stationpediaascended_ui"),
				Path.Combine(Paths.BepInExRootPath, "scripts", "assets", "stationpediaascended_ui")
			};
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (File.Exists(text))
				{
					ManualLogSource log = StationpediaAscendedMod.Log;
					if (log != null)
					{
						log.LogInfo((object)("[BlockEditor] Found bundle at: " + text));
					}
					return text;
				}
			}
			ManualLogSource log2 = StationpediaAscendedMod.Log;
			if (log2 != null)
			{
				log2.LogWarning((object)"[BlockEditor] Bundle not found in any of these locations:");
			}
			string[] array3 = array;
			foreach (string text2 in array3)
			{
				ManualLogSource log3 = StationpediaAscendedMod.Log;
				if (log3 != null)
				{
					log3.LogWarning((object)("  - " + text2));
				}
			}
			return null;
		}

		private void ConfigureInputField()
		{
			_inputField.lineType = (LineType)2;
			_inputField.richText = true;
			_inputField.characterLimit = 0;
			((Selectable)_inputField).interactable = true;
			_inputField.readOnly = false;
		}

		private void OnTextChanged(string newText)
		{
			if (_isUpdatingDisplay)
			{
				return;
			}
			try
			{
				string text = StripRichTextTags(newText);
				string[] array = text.Split('\n');
				string[] array2 = _previousPlainText.Split('\n');
				while (_lineStyles.Count < array.Length)
				{
					_lineStyles.Add(LineStyle.Normal);
				}
				while (_lineStyles.Count > array.Length)
				{
					_lineStyles.RemoveAt(_lineStyles.Count - 1);
				}
				for (int i = 0; i < array.Length; i++)
				{
					string text2 = array[i];
					while (text2.Length > 0 && (text2[0] == '•' || text2.StartsWith("• ") || text2.StartsWith(" •")))
					{
						text2 = text2.TrimStart('•', ' ');
					}
					if (text2.StartsWith("•"))
					{
						text2 = text2.TrimStart('•', ' ');
					}
					array[i] = text2;
				}
				text = string.Join("\n", array);
				array = text.Split('\n');
				for (int j = 0; j < array.Length; j++)
				{
					bool flag = j >= array2.Length || string.IsNullOrEmpty(array2[j]);
					bool flag2 = string.IsNullOrEmpty(array[j]);
					if ((j < array2.Length && !(array[j] != array2[j])) || flag || flag2 || j >= _lineStyles.Count)
					{
						continue;
					}
					LineStyle lineStyle = _lineStyles[j];
					if (lineStyle == LineStyle.H1 || lineStyle == LineStyle.H2 || lineStyle == LineStyle.H3)
					{
						ManualLogSource log = StationpediaAscendedMod.Log;
						if (log != null)
						{
							log.LogInfo((object)$"[BlockEditor] Line {j} edited, downgrading from {lineStyle} to Normal");
						}
						_lineStyles[j] = LineStyle.Normal;
					}
				}
				_previousPlainText = text;
				_previousLineCount = array.Length;
				RefreshDisplay();
				this.OnContentChanged?.Invoke();
			}
			catch (Exception ex)
			{
				ManualLogSource log2 = StationpediaAscendedMod.Log;
				if (log2 != null)
				{
					log2.LogError((object)("[BlockEditor] OnTextChanged error: " + ex.Message));
				}
			}
		}

		private string StripRichTextTags(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return text;
			}
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = false;
			foreach (char c in text)
			{
				switch (c)
				{
				case '<':
					flag = true;
					continue;
				case '>':
					flag = false;
					continue;
				}
				if (!flag)
				{
					stringBuilder.Append(c);
				}
			}
			return stringBuilder.ToString();
		}

		public void RefreshDisplay()
		{
			if ((Object)(object)_inputField == (Object)null)
			{
				return;
			}
			_isUpdatingDisplay = true;
			try
			{
				int caretPosition = _inputField.caretPosition;
				string text = GenerateRichText();
				_inputField.text = text;
				_inputField.caretPosition = Mathf.Min(caretPosition, text.Length);
			}
			finally
			{
				_isUpdatingDisplay = false;
			}
		}

		private string GenerateRichText()
		{
			string[] array = _previousPlainText.Split('\n');
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < array.Length; i++)
			{
				if (i > 0)
				{
					stringBuilder.Append('\n');
				}
				LineStyle lineStyle = ((i < _lineStyles.Count) ? _lineStyles[i] : LineStyle.Normal);
				string value = array[i];
				switch (lineStyle)
				{
				case LineStyle.H1:
					stringBuilder.Append("<size=150%><b>");
					stringBuilder.Append(value);
					stringBuilder.Append("</b></size>");
					break;
				case LineStyle.H2:
					stringBuilder.Append("<size=130%><b>");
					stringBuilder.Append(value);
					stringBuilder.Append("</b></size>");
					break;
				case LineStyle.H3:
					stringBuilder.Append("<size=115%><b>");
					stringBuilder.Append(value);
					stringBuilder.Append("</b></size>");
					break;
				case LineStyle.Bullet:
					stringBuilder.Append("• ");
					stringBuilder.Append(value);
					break;
				case LineStyle.Checkbox:
					stringBuilder.Append("  <color=#FBB03B>☐</color> ");
					stringBuilder.Append(value);
					break;
				case LineStyle.CheckboxChecked:
					stringBuilder.Append("  <color=#888888>☑</color> <s><color=#888888>");
					stringBuilder.Append(value);
					stringBuilder.Append("</color></s>");
					break;
				case LineStyle.Strikethrough:
					stringBuilder.Append("<s><color=#888888>");
					stringBuilder.Append(value);
					stringBuilder.Append("</color></s>");
					break;
				default:
					stringBuilder.Append(value);
					break;
				}
			}
			return stringBuilder.ToString();
		}

		public void ApplyStyleToCurrentLine(LineStyle style)
		{
			int currentLineIndex = CurrentLineIndex;
			ApplyStyleToLine(currentLineIndex, style);
		}

		public void InsertText(string text)
		{
			if (!((Object)(object)_inputField == (Object)null) && !string.IsNullOrEmpty(text))
			{
				int caretPosition = _inputField.caretPosition;
				string text2 = _inputField.text;
				string text3 = text2.Insert(caretPosition, text);
				_inputField.text = text3;
				_inputField.caretPosition = caretPosition + text.Length;
				_inputField.ActivateInputField();
			}
		}

		public void ApplyStyleToLine(int lineIndex, LineStyle style)
		{
			while (_lineStyles.Count <= lineIndex)
			{
				_lineStyles.Add(LineStyle.Normal);
			}
			_lineStyles[lineIndex] = style;
			ManualLogSource log = StationpediaAscendedMod.Log;
			if (log != null)
			{
				log.LogInfo((object)$"[BlockEditor] Applied {style} to line {lineIndex}");
			}
			RefreshDisplay();
			if (style == LineStyle.Bullet)
			{
				PositionCaretAfterPrefix(lineIndex);
			}
		}

		private void PositionCaretAfterPrefix(int lineIndex)
		{
			if ((Object)(object)_inputField == (Object)null || (Object)(object)_inputField.textComponent == (Object)null)
			{
				return;
			}
			_inputField.textComponent.ForceMeshUpdate(false, false);
			string parsedText = _inputField.textComponent.GetParsedText();
			if (!string.IsNullOrEmpty(parsedText))
			{
				string[] array = parsedText.Split('\n');
				int num = 0;
				for (int i = 0; i < lineIndex && i < array.Length; i++)
				{
					num += array[i].Length + 1;
				}
				if (lineIndex < array.Length && array[lineIndex].StartsWith("• "))
				{
					num += 2;
				}
				_inputField.caretPosition = num;
				_inputField.selectionAnchorPosition = num;
				_inputField.selectionFocusPosition = num;
				_inputField.ActivateInputField();
				_inputField.selectionFocusPosition = num;
			}
		}

		public void ToggleStyleOnCurrentLine(LineStyle style)
		{
			int currentLineIndex = CurrentLineIndex;
			if (currentLineIndex < _lineStyles.Count && _lineStyles[currentLineIndex] == style)
			{
				ApplyStyleToLine(currentLineIndex, LineStyle.Normal);
			}
			else
			{
				ApplyStyleToLine(currentLineIndex, style);
			}
		}

		public void ToggleCheckboxOnLine(int lineIndex)
		{
			if (lineIndex < 0 || lineIndex >= _lineStyles.Count)
			{
				return;
			}
			switch (_lineStyles[lineIndex])
			{
			case LineStyle.Checkbox:
			{
				_lineStyles[lineIndex] = LineStyle.CheckboxChecked;
				ManualLogSource log2 = StationpediaAscendedMod.Log;
				if (log2 != null)
				{
					log2.LogInfo((object)$"[BlockEditor] Checked checkbox on line {lineIndex}");
				}
				break;
			}
			case LineStyle.CheckboxChecked:
			{
				_lineStyles[lineIndex] = LineStyle.Checkbox;
				ManualLogSource log = StationpediaAscendedMod.Log;
				if (log != null)
				{
					log.LogInfo((object)$"[BlockEditor] Unchecked checkbox on line {lineIndex}");
				}
				break;
			}
			}
			RefreshDisplay();
		}

		public int GetCheckboxLineAtPosition(Vector2 localPoint)
		{
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)_inputField == (Object)null || (Object)(object)_inputField.textComponent == (Object)null)
			{
				return -1;
			}
			TMP_Text textComponent = _inputField.textComponent;
			TMP_TextInfo textInfo = textComponent.textInfo;
			if (textInfo == null || textInfo.characterCount == 0)
			{
				return -1;
			}
			int num = TMP_TextUtilities.FindIntersectingCharacter(textComponent, Vector2.op_Implicit(localPoint), (Camera)null, true);
			if (num < 0)
			{
				return -1;
			}
			int lineIndexFromDisplayCharIndex = GetLineIndexFromDisplayCharIndex(num);
			if (lineIndexFromDisplayCharIndex < 0 || lineIndexFromDisplayCharIndex >= _lineStyles.Count)
			{
				return -1;
			}
			LineStyle lineStyle = _lineStyles[lineIndexFromDisplayCharIndex];
			if (lineStyle != LineStyle.Checkbox && lineStyle != LineStyle.CheckboxChecked)
			{
				return -1;
			}
			int displayCharIndexForLineStart = GetDisplayCharIndexForLineStart(lineIndexFromDisplayCharIndex);
			if (num >= displayCharIndexForLineStart && num < displayCharIndexForLineStart + 6)
			{
				return lineIndexFromDisplayCharIndex;
			}
			return -1;
		}

		private int GetLineIndexFromDisplayCharIndex(int displayCharIndex)
		{
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)_inputField == (Object)null || (Object)(object)_inputField.textComponent == (Object)null)
			{
				return -1;
			}
			TMP_Text textComponent = _inputField.textComponent;
			TMP_TextInfo textInfo = textComponent.textInfo;
			if (textInfo == null)
			{
				return -1;
			}
			for (int i = 0; i < textInfo.lineCount; i++)
			{
				TMP_LineInfo val = textInfo.lineInfo[i];
				if (displayCharIndex >= val.firstCharacterIndex && displayCharIndex <= val.lastCharacterIndex)
				{
					return i;
				}
			}
			return -1;
		}

		private int GetDisplayCharIndexForLineStart(int lineIndex)
		{
			if ((Object)(object)_inputField == (Object)null || (Object)(object)_inputField.textComponent == (Object)null)
			{
				return 0;
			}
			TMP_TextInfo textInfo = _inputField.textComponent.textInfo;
			if (textInfo == null || lineIndex >= textInfo.lineCount)
			{
				return 0;
			}
			return textInfo.lineInfo[lineIndex].firstCharacterIndex;
		}

		public LineStyle GetCurrentLineStyle()
		{
			int currentLineIndex = CurrentLineIndex;
			if (currentLineIndex < _lineStyles.Count)
			{
				return _lineStyles[currentLineIndex];
			}
			return LineStyle.Normal;
		}

		public string GetPlainText()
		{
			return _previousPlainText;
		}

		public void SetContent(string plainText, List<LineStyle> styles)
		{
			_previousPlainText = plainText ?? "";
			_lineStyles = styles ?? new List<LineStyle>();
			if (_lineStyles.Count == 0)
			{
				_lineStyles.Add(LineStyle.Normal);
			}
			string[] array = _previousPlainText.Split('\n');
			while (_lineStyles.Count < array.Length)
			{
				_lineStyles.Add(LineStyle.Normal);
			}
			while (_lineStyles.Count > array.Length)
			{
				_lineStyles.RemoveAt(_lineStyles.Count - 1);
			}
			_previousLineCount = array.Length;
			RefreshDisplay();
		}

		public List<LineStyle> GetLineStyles()
		{
			return new List<LineStyle>(_lineStyles);
		}

		public string SerializeDocument()
		{
			BlockEditorDocument blockEditorDocument = new BlockEditorDocument();
			string[] array = _previousPlainText.Split('\n');
			for (int i = 0; i < array.Length; i++)
			{
				blockEditorDocument.lines.Add(new EditorLineData
				{
					text = array[i],
					style = (int)((i < _lineStyles.Count) ? _lineStyles[i] : LineStyle.Normal)
				});
			}
			return JsonConvert.SerializeObject((object)blockEditorDocument, (Formatting)1);
		}

		public void DeserializeDocument(string json)
		{
			if (string.IsNullOrEmpty(json))
			{
				SetContent("", new List<LineStyle>());
				return;
			}
			try
			{
				BlockEditorDocument blockEditorDocument = JsonConvert.DeserializeObject<BlockEditorDocument>(json);
				if (blockEditorDocument == null || blockEditorDocument.lines == null || blockEditorDocument.lines.Count == 0)
				{
					ManualLogSource log = StationpediaAscendedMod.Log;
					if (log != null)
					{
						log.LogWarning((object)"[BlockEditor] JSON parsed but no lines found, loading as plain text");
					}
					LoadFromPlainText(json);
					return;
				}
				StringBuilder stringBuilder = new StringBuilder();
				List<LineStyle> list = new List<LineStyle>();
				for (int i = 0; i < blockEditorDocument.lines.Count; i++)
				{
					if (i > 0)
					{
						stringBuilder.Append('\n');
					}
					stringBuilder.Append(blockEditorDocument.lines[i].text);
					list.Add((LineStyle)blockEditorDocument.lines[i].style);
				}
				SetContent(stringBuilder.ToString(), list);
			}
			catch (Exception ex)
			{
				ManualLogSource log2 = StationpediaAscendedMod.Log;
				if (log2 != null)
				{
					log2.LogError((object)("[BlockEditor] Failed to deserialize document: " + ex.Message));
				}
				LoadFromPlainText(json);
			}
		}

		public void LoadFromPlainText(string plainText)
		{
			if (string.IsNullOrEmpty(plainText))
			{
				SetContent("", new List<LineStyle>());
				return;
			}
			string[] array = plainText.Split('\n');
			List<LineStyle> list = new List<LineStyle>();
			for (int i = 0; i < array.Length; i++)
			{
				list.Add(LineStyle.Normal);
			}
			SetContent(plainText, list);
		}

		private void OnDestroy()
		{
			if ((Object)(object)_inputField != (Object)null)
			{
				((UnityEvent<string>)(object)_inputField.onValueChanged).RemoveListener((UnityAction<string>)OnTextChanged);
			}
			if ((Object)(object)_prefabInstance != (Object)null)
			{
				Object.Destroy((Object)(object)_prefabInstance);
			}
		}

		public static void UnloadBundle()
		{
			if ((Object)(object)_loadedBundle != (Object)null)
			{
				_loadedBundle.Unload(true);
				_loadedBundle = null;
			}
		}

		public static TMP_InputField CreateDialogInputField(Transform parent)
		{
			try
			{
				string bundlePathStatic = GetBundlePathStatic();
				if (string.IsNullOrEmpty(bundlePathStatic))
				{
					ManualLogSource log = StationpediaAscendedMod.Log;
					if (log != null)
					{
						log.LogWarning((object)"[BlockEditor] Could not find AssetBundle for dialog input field");
					}
					return null;
				}
				if ((Object)(object)_loadedBundle == (Object)null)
				{
					IEnumerable<AssetBundle> allLoadedAssetBundles = AssetBundle.GetAllLoadedAssetBundles();
					foreach (AssetBundle item in allLoadedAssetBundles)
					{
						if (((Object)item).name == "stationpediaascended_ui" || ((Object)item).name.Contains("stationpediaascended"))
						{
							_loadedBundle = item;
							break;
						}
					}
					if ((Object)(object)_loadedBundle == (Object)null)
					{
						_loadedBundle = AssetBundle.LoadFromFile(bundlePathStatic);
					}
				}
				if ((Object)(object)_loadedBundle == (Object)null)
				{
					ManualLogSource log2 = StationpediaAscendedMod.Log;
					if (log2 != null)
					{
						log2.LogWarning((object)"[BlockEditor] Failed to load AssetBundle for dialog");
					}
					return null;
				}
				GameObject val = _loadedBundle.LoadAsset<GameObject>("SA_TMPInputField_Base");
				if ((Object)(object)val == (Object)null)
				{
					val = _loadedBundle.LoadAsset<GameObject>("SA_TMPInputField_Base.prefab");
				}
				if ((Object)(object)val == (Object)null)
				{
					val = _loadedBundle.LoadAsset<GameObject>("assets/prefabs/ui/" + "SA_TMPInputField_Base".ToLower() + ".prefab");
				}
				if ((Object)(object)val == (Object)null)
				{
					ManualLogSource log3 = StationpediaAscendedMod.Log;
					if (log3 != null)
					{
						log3.LogWarning((object)"[BlockEditor] Prefab not found for dialog input field");
					}
					return null;
				}
				GameObject val2 = Object.Instantiate<GameObject>(val, parent);
				((Object)val2).name = "Dialog_InputField";
				TMP_InputField component = val2.GetComponent<TMP_InputField>();
				if ((Object)(object)component != (Object)null)
				{
					component.lineType = (LineType)0;
					component.characterLimit = 0;
					((Selectable)component).interactable = true;
					ManualLogSource log4 = StationpediaAscendedMod.Log;
					if (log4 != null)
					{
						log4.LogInfo((object)"[BlockEditor] Created dialog input field from prefab");
					}
				}
				return component;
			}
			catch (Exception ex)
			{
				ManualLogSource log5 = StationpediaAscendedMod.Log;
				if (log5 != null)
				{
					log5.LogError((object)("[BlockEditor] Failed to create dialog input field: " + ex.Message));
				}
				return null;
			}
		}

		private static string GetBundlePathStatic()
		{
			string[] array = new string[4]
			{
				Path.Combine(Path.GetDirectoryName(typeof(BlockEditor).Assembly.Location), "stationpediaascended_ui"),
				Path.Combine(Paths.BepInExRootPath, "scripts", "stationpediaascended_ui"),
				Path.Combine(Paths.PluginPath, "StationpediaAscended", "stationpediaascended_ui"),
				Path.Combine(Paths.BepInExRootPath, "scripts", "assets", "stationpediaascended_ui")
			};
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (File.Exists(text))
				{
					return text;
				}
			}
			return null;
		}
	}
	public class CheckboxClickHandler : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
	{
		public BlockEditor BlockEditor;

		public void OnPointerClick(PointerEventData eventData)
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Invalid comparison between Unknown and I4
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)BlockEditor == (Object)null || !BlockEditor.IsInitialized || (int)eventData.button > 0)
			{
				return;
			}
			TMP_InputField inputField = BlockEditor.InputField;
			if ((Object)(object)inputField == (Object)null || (Object)(object)inputField.textComponent == (Object)null)
			{
				return;
			}
			RectTransform component = ((Component)inputField.textComponent).GetComponent<RectTransform>();
			Vector2 localPoint = default(Vector2);
			if (RectTransformUtility.ScreenPointToLocalPointInRectangle(component, eventData.position, eventData.pressEventCamera, ref localPoint))
			{
				int checkboxLineAtPosition = BlockEditor.GetCheckboxLineAtPosition(localPoint);
				if (checkboxLineAtPosition >= 0)
				{
					BlockEditor.ToggleCheckboxOnLine(checkboxLineAtPosition);
					((AbstractEventData)eventData).Use();
				}
			}
		}
	}
	public class CustomTextEditor : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerDownHandler, IScrollHandler
	{
		private TextMeshProUGUI _textDisplay;

		private TextMeshProUGUI _placeholder;

		private Image _cursorImage;

		private RectTransform _cursorRect;

		private RectTransform _textRect;

		private RectTransform _viewportRect;

		private ScrollRect _scrollRect;

		private StringBuilder _text = new StringBuilder();

		private int _cursorPosition = 0;

		private int _selectionStart = -1;

		private int _selectionEnd = -1;

		private bool _isFocused = false;

		private float _cursorBlinkTimer = 0f;

		private bool _cursorVisible = true;

		public Color CursorColor = new Color(0.984f, 0.69f, 0.231f, 1f);

		public Color SelectionColor = new Color(0.984f, 0.69f, 0.231f, 0.25f);

		public Color TextColor = new Color(0.9f, 0.9f, 0.9f, 1f);

		public Color PlaceholderColor = new Color(0.5f, 0.55f, 0.6f, 1f);

		public float CursorBlinkRate = 0.53f;

		public int CursorWidth = 2;

		public string PlaceholderText = "Click here to type...";

		public int FontSize = 14;

		public string Text
		{
			get
			{
				return _text.ToString();
			}
			set
			{
				_text.Clear();
				if (!string.IsNullOrEmpty(value))
				{
					_text.Append(value);
				}
				_cursorPosition = Mathf.Clamp(_cursorPosition, 0, _text.Length);
				ClearSelection();
				UpdateDisplay();
			}
		}

		public int CursorPos
		{
			get
			{
				return _cursorPosition;
			}
			set
			{
				_cursorPosition = Mathf.Clamp(value, 0, _text.Length);
				UpdateCursorPosition();
			}
		}

		public bool IsFocused => _isFocused;

		public event Action<string> OnTextChanged;

		public event Action OnFocusGained;

		public event Action OnFocusLost;

		public void Initialize(RectTransform viewport, TMP_FontAsset font = null, Material fontMaterial = null)
		{
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Expected O, but got Unknown
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0110: Unknown result type (might be due to invalid IL or missing references)
			//IL_0116: Expected O, but got Unknown
			//IL_014f: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0202: Unknown result type (might be due to invalid IL or missing references)
			//IL_021d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0238: Unknown result type (might be due to invalid IL or missing references)
			//IL_0253: Unknown result type (might be due to invalid IL or missing references)
			//IL_0263: Unknown result type (might be due to invalid IL or missing references)
			//IL_026a: Expected O, but got Unknown
			//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_033b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0352: Unknown result type (might be due to invalid IL or missing references)
			//IL_0369: Unknown result type (might be due to invalid IL or missing references)
			//IL_0380: Unknown result type (might be due to invalid IL or missing references)
			//IL_0397: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ae: Expected O, but got Unknown
			//IL_03d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_040b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0426: Unknown result type (might be due to invalid IL or missing references)
			//IL_0441: Unknown result type (might be due to invalid IL or missing references)
			//IL_0462: Unknown result type (might be due to invalid IL or missing references)
			_viewportRect = viewport;
			_scrollRect = ((Component)this).gameObject.GetComponent<ScrollRect>();
			if ((Object)(object)_scrollRect == (Object)null)
			{
				_scrollRect = ((Component)this).gameObject.AddComponent<ScrollRect>();
			}
			_scrollRect.horizontal = false;
			_scrollRect.vertical = true;
			_scrollRect.movementType = (MovementType)2;
			_scrollRect.scrollSensitivity = 25f;
			_scrollRect.viewport = viewport;
			GameObject val = new GameObject("Content");
			val.transform.SetParent((Transform)(object)viewport, false);
			RectTransform val2 = val.AddComponent<RectTransform>();
			val2.anchorMin = new Vector2(0f, 1f);
			val2.anchorMax = new Vector2(1f, 1f);
			val2.pivot = new Vector2(0f, 1f);
			val2.anchoredPosition = Vector2.zero;
			ContentSizeFitter val3 = val.AddComponent<ContentSizeFitter>();
			val3.verticalFit = (FitMode)2;
			_scrollRect.content = val2;
			GameObject val4 = new GameObject("Text");
			val4.transform.SetParent(val.transform, false);
			_textDisplay = val4.AddComponent<TextMeshProUGUI>();
			((TMP_Text)_textDisplay).fontSize = FontSize;
			((Graphic)_textDisplay).color = TextColor;
			((TMP_Text)_textDisplay).alignment = (TextAlignmentOptions)257;
			((TMP_Text)_textDisplay).enableWordWrapping = true;
			((TMP_Text)_textDisplay).overflowMode = (TextOverflowModes)0;
			((Graphic)_textDisplay).raycastTarget = false;
			((TMP_Text)_textDisplay).richText = false;
			if ((Object)(object)font != (Object)null)
			{
				((TMP_Text)_textDisplay).font = font;
				if ((Object)(object)fontMaterial != (Object)null)
				{
					((TMP_Text)_textDisplay).fontSharedMaterial = fontMaterial;
				}
			}
			_textRect = val4.GetComponent<RectTransform>();
			_textRect.anchorMin = Vector2.zero;
			_textRect.anchorMax = new Vector2(1f, 1f);
			_textRect.pivot = new Vector2(0f, 1f);
			_textRect.offsetMin = new Vector2(4f, 0f);
			_textRect.offsetMax = new Vector2(-4f, 0f);
			GameObject val5 = new GameObject("Placeholder");
			val5.transform.SetParent(val.transform, false);
			_placeholder = val5.AddComponent<TextMeshProUGUI>();
			((TMP_Text)_placeholder).text = PlaceholderText;
			((TMP_Text)_placeholder).fontSize = FontSize;
			((Graphic)_placeholder).color = PlaceholderColor;
			((TMP_Text)_placeholder).fontStyle = (FontStyles)2;
			((TMP_Text)_placeholder).alignment = (TextAlignmentOptions)257;
			((TMP_Text)_placeholder).enableWordWrapping = true;
			((Graphic)_placeholder).raycastTarget = false;
			if ((Object)(object)font != (Object)null)
			{
				((TMP_Text)_placeholder).font = font;
				if ((Object)(object)fontMaterial != (Object)null)
				{
					((TMP_Text)_placeholder).fontSharedMaterial = fontMaterial;
				}
			}
			RectTransform component = val5.GetComponent<RectTransform>();
			component.anchorMin = Vector2.zero;
			component.anchorMax = new Vector2(1f, 1f);
			component.pivot = new Vector2(0f, 1f);
			component.offsetMin = new Vector2(4f, 0f);
			component.offsetMax = new Vector2(-4f, 0f);
			GameObject val6 = new GameObject("Cursor");
			val6.transform.SetParent(val.transform, false);
			_cursorImage = val6.AddComponent<Image>();
			((Graphic)_cursorImage).color = CursorColor;
			((Graphic)_cursorImage).raycastTarget = false;
			_cursorRect = val6.GetComponent<RectTransform>();
			_cursorRect.anchorMin = new Vector2(0f, 1f);
			_cursorRect.anchorMax = new Vector2(0f, 1f);
			_cursorRect.pivot = new Vector2(0f, 1f);
			_cursorRect.sizeDelta = new Vector2((float)CursorWidth, (float)(FontSize + 4));
			UpdateDisplay();
		}

		private void Update()
		{
			if (!_isFocused)
			{
				return;
			}
			_cursorBlinkTimer += Time.deltaTime;
			if (_cursorBlinkTimer >= CursorBlinkRate)
			{
				_cursorBlinkTimer = 0f;
				_cursorVisible = !_cursorVisible;
				if ((Object)(object)_cursorImage != (Object)null)
				{
					((Behaviour)_cursorImage).enabled = _cursorVisible;
				}
			}
			HandleTextInput();
			HandleSpecialKeys();
		}

		private void OnDisable()
		{
			if (_isFocused)
			{
				Unfocus();
			}
		}

		private void HandleTextInput()
		{
			string inputString = Input.inputString;
			if (string.IsNullOrEmpty(inputString))
			{
				return;
			}
			string text = inputString;
			foreach (char c in text)
			{
				if (c != '\b')
				{
					if (c == '\n' || c == '\r')
					{
						InsertCharacter('\n');
					}
					else if (c >= ' ' || c == '\t')
					{
						InsertCharacter(c);
					}
				}
			}
		}

		private void HandleSpecialKeys()
		{
			bool flag = Input.GetKey((KeyCode)304) || Input.GetKey((KeyCode)303);
			bool flag2 = Input.GetKey((KeyCode)306) || Input.GetKey((KeyCode)305);
			if (Input.GetKeyDown((KeyCode)8))
			{
				if (HasSelection())
				{
					DeleteSelection();
				}
				else if (_cursorPosition > 0)
				{
					if (flag2)
					{
						int num = FindWordStart(_cursorPosition - 1);
						_text.Remove(num, _cursorPosition - num);
						_cursorPosition = num;
					}
					else
					{
						_text.Remove(_cursorPosition - 1, 1);
						_cursorPosition--;
					}
					OnTextModified();
				}
			}
			if (Input.GetKeyDown((KeyCode)127))
			{
				if (HasSelection())
				{
					DeleteSelection();
				}
				else if (_cursorPosition < _text.Length)
				{
					if (flag2)
					{
						int num2 = FindWordEnd(_cursorPosition);
						_text.Remove(_cursorPosition, num2 - _cursorPosition);
					}
					else
					{
						_text.Remove(_cursorPosition, 1);
					}
					OnTextModified();
				}
			}
			if (Input.GetKeyDown((KeyCode)276))
			{
				if (flag)
				{
					ExtendSelection(-1, flag2);
				}
				else
				{
					ClearSelection();
					if (flag2)
					{
						_cursorPosition = FindWordStart(_cursorPosition - 1);
					}
					else if (_cursorPosition > 0)
					{
						_cursorPosition--;
					}
				}
				UpdateCursorPosition();
				ResetBlink();
			}
			if (Input.GetKeyDown((KeyCode)275))
			{
				if (flag)
				{
					ExtendSelection(1, flag2);
				}
				else
				{
					ClearSelection();
					if (flag2)
					{
						_cursorPosition = FindWordEnd(_cursorPosition);
					}
					else if (_cursorPosition < _text.Length)
					{
						_cursorPosition++;
					}
				}
				UpdateCursorPosition();
				ResetBlink();
			}
			if (Input.GetKeyDown((KeyCode)273))
			{
				MoveToLine(-1, flag);
			}
			if (Input.GetKeyDown((KeyCode)274))
			{
				MoveToLine(1, flag);
			}
			if (Input.GetKeyDown((KeyCode)278))
			{
				if (flag)
				{
					StartOrExtendSelection();
				}
				else
				{
					ClearSelection();
				}
				if (flag2)
				{
					_cursorPosition = 0;
				}
				else
				{
					_cursorPosition = FindLineStart(_cursorPosition);
				}
				if (flag)
				{
					_selectionEnd = _cursorPosition;
				}
				UpdateCursorPosition();
				ResetBlink();
			}
			if (Input.GetKeyDown((KeyCode)279))
			{
				if (flag)
				{
					StartOrExtendSelection();
				}
				else
				{
					ClearSelection();
				}
				if (flag2)
				{
					_cursorPosition = _text.Length;
				}
				else
				{
					_cursorPosition = FindLineEnd(_cursorPosition);
				}
				if (flag)
				{
					_selectionEnd = _cursorPosition;
				}
				UpdateCursorPosition();
				ResetBlink();
			}
			if (flag2 && Input.GetKeyDown((KeyCode)97))
			{
				SelectAll();
			}
			if (flag2 && Input.GetKeyDown((KeyCode)99))
			{
				CopyToClipboard();
			}
			if (flag2 && Input.GetKeyDown((KeyCode)120))
			{
				CutToClipboard();
			}
			if (flag2 && Input.GetKeyDown((KeyCode)118))
			{
				PasteFromClipboard();
			}
			if (flag2 && !Input.GetKeyDown((KeyCode)122))
			{
			}
		}

		private void InsertCharacter(char c)
		{
			if (HasSelection())
			{
				DeleteSelection();
			}
			_text.Insert(_cursorPosition, c);
			_cursorPosition++;
			OnTextModified();
		}

		public void InsertText(string text)
		{
			if (!string.IsNullOrEmpty(text))
			{
				if (HasSelection())
				{
					DeleteSelection();
				}
				_text.Insert(_cursorPosition, text);
				_cursorPosition += text.Length;
				OnTextModified();
			}
		}

		private bool HasSelection()
		{
			return _selectionStart >= 0 && _selectionEnd >= 0 && _selectionStart != _selectionEnd;
		}

		private void ClearSelection()
		{
			_selectionStart = -1;
			_selectionEnd = -1;
			UpdateDisplay();
		}

		private void StartOrExtendSelection()
		{
			if (_selectionStart < 0)
			{
				_selectionStart = _cursorPosition;
			}
		}

		private void ExtendSelection(int direction, bool byWord)
		{
			StartOrExtendSelection();
			if (byWord)
			{
				if (direction < 0)
				{
					_cursorPosition = FindWordStart(_cursorPosition - 1);
				}
				else
				{
					_cursorPosition = FindWordEnd(_cursorPosition);
				}
			}
			else
			{
				_cursorPosition = Mathf.Clamp(_cursorPosition + direction, 0, _text.Length);
			}
			_selectionEnd = _cursorPosition;
			UpdateDisplay();
		}

		private void DeleteSelection()
		{
			if (HasSelection())
			{
				int num = Mathf.Min(_selectionStart, _selectionEnd);
				int num2 = Mathf.Max(_selectionStart, _selectionEnd);
				_text.Remove(num, num2 - num);
				_cursorPosition = num;
				ClearSelection();
				OnTextModified();
			}
		}

		public void SelectAll()
		{
			_selectionStart = 0;
			_selectionEnd = _text.Length;
			_cursorPosition = _text.Length;
			UpdateDisplay();
		}

		private string GetSelectedText()
		{
			if (!HasSelection())
			{
				return "";
			}
			int num = Mathf.Min(_selectionStart, _selectionEnd);
			int num2 = Mathf.Max(_selectionStart, _selectionEnd);
			return _text.ToString(num, num2 - num);
		}

		private void CopyToClipboard()
		{
			string selectedText = GetSelectedText();
			if (!string.IsNullOrEmpty(selectedText))
			{
				GUIUtility.systemCopyBuffer = selectedText;
			}
		}

		private void CutToClipboard()
		{
			CopyToClipboard();
			if (HasSelection())
			{
				DeleteSelection();
			}
		}

		private void PasteFromClipboard()
		{
			string systemCopyBuffer = GUIUtility.systemCopyBuffer;
			if (!string.IsNullOrEmpty(systemCopyBuffer))
			{
				systemCopyBuffer = systemCopyBuffer.Replace("\r\n", "\n").Replace("\r", "\n");
				InsertText(systemCopyBuffer);
			}
		}

		private int FindWordStart(int pos)
		{
			if (pos <= 0)
			{
				return 0;
			}
			pos = Mathf.Min(pos, _text.Length - 1);
			while (pos > 0 && char.IsWhiteSpace(_text[pos]))
			{
				pos--;
			}
			while (pos > 0 && !char.IsWhiteSpace(_text[pos - 1]))
			{
				pos--;
			}
			return pos;
		}

		private int FindWordEnd(int pos)
		{
			if (pos >= _text.Length)
			{
				return _text.Length;
			}
			while (pos < _text.Length && !char.IsWhiteSpace(_text[pos]))
			{
				pos++;
			}
			while (pos < _text.Length && char.IsWhiteSpace(_text[pos]))
			{
				pos++;
			}
			return pos;
		}

		private int FindLineStart(int pos)
		{
			while (pos > 0 && _text[pos - 1] != '\n')
			{
				pos--;
			}
			return pos;
		}

		private int FindLineEnd(int pos)
		{
			while (pos < _text.Length && _text[pos] != '\n')
			{
				pos++;
			}
			return pos;
		}

		private void MoveToLine(int direction, bool extendSelection)
		{
			//IL_0119: Unknown result type (might be due to invalid IL or missing references)
			//IL_011e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0120: Unknown result type (might be due to invalid IL or missing references)
			//IL_0130: Unknown result type (might be due to invalid IL or missing references)
			//IL_0177: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)_textDisplay == (Object)null)
			{
				return;
			}
			((TMP_Text)_textDisplay).ForceMeshUpdate(false, false);
			TMP_TextInfo textInfo = ((TMP_Text)_textDisplay).textInfo;
			if (textInfo.characterCount == 0)
			{
				if (!extendSelection)
				{
					ClearSelection();
				}
				return;
			}
			int num = 0;
			int num2 = Mathf.Min(_cursorPosition, textInfo.characterCount - 1);
			if (num2 >= 0 && num2 < textInfo.characterCount)
			{
				num = textInfo.characterInfo[num2].lineNumber;
			}
			int num3 = Mathf.Clamp(num + direction, 0, textInfo.lineCount - 1);
			if (num3 == num)
			{
				if (direction < 0)
				{
					_cursorPosition = 0;
				}
				else
				{
					_cursorPosition = _text.Length;
				}
			}
			else
			{
				float num4 = 0f;
				if (num2 >= 0 && num2 < textInfo.characterCount)
				{
					num4 = textInfo.characterInfo[num2].origin;
				}
				TMP_LineInfo val = textInfo.lineInfo[num3];
				int num5 = val.firstCharacterIndex;
				float num6 = float.MaxValue;
				for (int i = val.firstCharacterIndex; i <= val.lastCharacterIndex && i < textInfo.characterCount; i++)
				{
					float num7 = Mathf.Abs(textInfo.characterInfo[i].origin - num4);
					if (num7 < num6)
					{
						num6 = num7;
						num5 = i;
					}
				}
				_cursorPosition = Mathf.Min(num5, _text.Length);
			}
			if (extendSelection)
			{
				StartOrExtendSelection();
				_selectionEnd = _cursorPosition;
			}
			else
			{
				ClearSelection();
			}
			UpdateCursorPosition();
			ResetBlink();
		}

		private void OnTextModified()
		{
			UpdateDisplay();
			this.OnTextChanged?.Invoke(_text.ToString());
		}

		private void UpdateDisplay()
		{
			if (!((Object)(object)_textDisplay == (Object)null))
			{
				string text = _text.ToString();
				if ((Object)(object)_placeholder != (Object)null)
				{
					((Component)_placeholder).gameObject.SetActive(string.IsNullOrEmpty(text) && !_isFocused);
				}
				((TMP_Text)_textDisplay).text = text;
				((TMP_Text)_textDisplay).ForceMeshUpdate(false, false);
				UpdateCursorPosition();
			}
		}

		private void UpdateCursorPosition()
		{
			//IL_0141: Unknown result type (might be due to invalid IL or missing references)
			//IL_0147: Unknown result type (might be due to invalid IL or missing references)
			//IL_014e: Unknown result type (might be due to invalid IL or missing references)
			//IL_011c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0121: Unknown result type (might be due to invalid IL or missing references)
			//IL_0125: Unknown result type (might be due to invalid IL or missing references)
			//IL_012d: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)_cursorRect == (Object)null || (Object)(object)_textDisplay == (Object)null)
			{
				return;
			}
			((TMP_Text)_textDisplay).ForceMeshUpdate(false, false);
			TMP_TextInfo textInfo = ((TMP_Text)_textDisplay).textInfo;
			Vector2 val = default(Vector2);
			((Vector2)(ref val))..ctor(4f, 0f);
			if (_cursorPosition > 0 && textInfo.characterCount > 0)
			{
				int num = Mathf.Min(_cursorPosition - 1, textInfo.characterCount - 1);
				if (num >= 0)
				{
					TMP_CharacterInfo val2 = textInfo.characterInfo[num];
					val.x = val2.origin + val2.xAdvance + 4f;
					int lineNumber = val2.lineNumber;
					if (lineNumber < textInfo.lineCount)
					{
						TMP_LineInfo val3 = textInfo.lineInfo[lineNumber];
						val.y = 0f - val3.baseline + val3.ascender;
					}
				}
			}
			else if (textInfo.lineCount > 0)
			{
				TMP_LineInfo val4 = textInfo.lineInfo[0];
				val.y = 0f - val4.baseline + val4.ascender;
			}
			_cursorRect.anchoredPosition = new Vector2(val.x, 0f - val.y);
			EnsureCursorVisible();
		}

		private void EnsureCursorVisible()
		{
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)_scrollRect == (Object)null || (Object)(object)_cursorRect == (Object)null || (Object)(object)_viewportRect == (Object)null)
			{
				return;
			}
			float num = 0f - _cursorRect.anchoredPosition.y;
			Rect rect = _viewportRect.rect;
			float height = ((Rect)(ref rect)).height;
			rect = _scrollRect.content.rect;
			float height2 = ((Rect)(ref rect)).height;
			if (!(height2 <= height))
			{
				float num2 = height2 - height;
				float num3 = (1f - _scrollRect.verticalNormalizedPosition) * num2;
				float num4 = num3;
				float num5 = num3 + height;
				float y = _cursorRect.sizeDelta.y;
				if (num < num4 + 10f)
				{
					float num6 = Mathf.Max(0f, num - 10f);
					_scrollRect.verticalNormalizedPosition = 1f - num6 / num2;
				}
				else if (num + y > num5 - 10f)
				{
					float num7 = Mathf.Min(num2, num + y - height + 10f);
					_scrollRect.verticalNormalizedPosition = 1f - num7 / num2;
				}
			}
		}

		private void ResetBlink()
		{
			_cursorBlinkTimer = 0f;
			_cursorVisible = true;
			if ((Object)(object)_cursorImage != (Object)null)
			{
				((Behaviour)_cursorImage).enabled = true;
			}
		}

		public void Focus()
		{
			if (!_isFocused)
			{
				_isFocused = true;
				ResetBlink();
				if ((Object)(object)_placeholder != (Object)null)
				{
					((Component)_placeholder).gameObject.SetActive(false);
				}
				if ((Object)(object)_cursorImage != (Object)null)
				{
					((Behaviour)_cursorImage).enabled = true;
				}
				this.OnFocusGained?.Invoke();
			}
		}

		public void Unfocus()
		{
			if (_isFocused)
			{
				_isFocused = false;
				ClearSelection();
				if ((Object)(object)_placeholder != (Object)null)
				{
					((Component)_placeholder).gameObject.SetActive(_text.Length == 0);
				}
				if ((Object)(object)_cursorImage != (Object)null)
				{
					((Behaviour)_cursorImage).enabled = false;
				}
				this.OnFocusLost?.Invoke();
			}
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			Focus();
			PositionCursorFromClick(eventData);
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			Focus();
			PositionCursorFromClick(eventData);
		}

		public void OnScroll(PointerEventData eventData)
		{
		}

		private void PositionCursorFromClick(PointerEventData eventData)
		{
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_010b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0110: Unknown result type (might be due to invalid IL or missing references)
			//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_012b: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)_textDisplay == (Object)null)
			{
				return;
			}
			((TMP_Text)_textDisplay).ForceMeshUpdate(false, false);
			TMP_TextInfo textInfo = ((TMP_Text)_textDisplay).textInfo;
			if (textInfo.characterCount == 0)
			{
				_cursorPosition = 0;
				ClearSelection();
				UpdateCursorPosition();
				return;
			}
			Vector2 val = default(Vector2);
			RectTransformUtility.ScreenPointToLocalPointInRectangle(_textRect, eventData.position, eventData.pressEventCamera, ref val);
			int num = 0;
			float num2 = float.MaxValue;
			for (int i = 0; i < textInfo.characterCount; i++)
			{
				TMP_CharacterInfo val2 = textInfo.characterInfo[i];
				if (val2.isVisible)
				{
					float num3 = val2.origin + val2.xAdvance * 0.5f;
					int lineNumber = val2.lineNumber;
					float num4 = 0f;
					if (lineNumber < textInfo.lineCount)
					{
						TMP_LineInfo val3 = textInfo.lineInfo[lineNumber];
						num4 = 0f - (val3.baseline - val3.ascender * 0.5f);
					}
					float num5 = Vector2.Distance(val, new Vector2(num3, num4));
					if (num5 < num2)
					{
						num2 = num5;
						num = ((!(val.x < num3)) ? (i + 1) : i);
					}
				}
			}
			_cursorPosition = Mathf.Clamp(num, 0, _text.Length);
			ClearSelection();
			UpdateCursorPosition();
			ResetBlink();
		}
	}
	[Serializable]
	public class PlannerFileSystem
	{
		[JsonProperty("version")]
		public string Version { get; set; } = "1.0";

		[JsonProperty("rootFolder")]
		public PlannerFolder RootFolder { get; set; } = new PlannerFolder
		{
			Id = "root",
			Name = "Notes",
			IsExpanded = true
		};

		[JsonProperty("lastOpenedFileId")]
		public string LastOpenedFileId { get; set; }

		[JsonProperty("windowPosition")]
		public PlannerWindowPosition WindowPosition { get; set; } = new PlannerWindowPosition();
	}
	[Serializable]
	public class PlannerFolder
	{
		[JsonProperty("id")]
		public string Id { get; set; } = Guid.NewGuid().ToString();

		[JsonProperty("name")]
		public string Name { get; set; } = "New Folder";

		[JsonProperty("isExpanded")]
		public bool IsExpanded { get; set; } = false;

		[JsonProperty("subFolders")]
		public List<PlannerFolder> SubFolders { get; set; } = new List<PlannerFolder>();

		[JsonProperty("files")]
		public List<PlannerFile> Files { get; set; } = new List<PlannerFile>();

		[JsonProperty("sortOrder")]
		public int SortOrder { get; set; } = 0;

		[JsonProperty("persistAcrossSaves")]
		public bool PersistAcrossSaves { get; set; } = false;

		[JsonProperty("saveName")]
		public string SaveName { get; set; } = "";

		public PlannerFile FindFile(string fileId)
		{
			foreach (PlannerFile file in Files)
			{
				if (file.Id == fileId)
				{
					return file;
				}
			}
			foreach (PlannerFolder subFolder in SubFolders)
			{
				PlannerFile plannerFile = subFolder.FindFile(fileId);
				if (plannerFile != null)
				{
					return plannerFile;
				}
			}
			return null;
		}

		public PlannerFolder FindFolder(string folderId)
		{
			if (Id == folderId)
			{
				return this;
			}
			foreach (PlannerFolder subFolder in SubFolders)
			{
				PlannerFolder plannerFolder = subFolder.FindFolder(folderId);
				if (plannerFolder != null)
				{
					return plannerFolder;
				}
			}
			return null;
		}

		public bool RemoveFile(string fileId)
		{
			PlannerFile plannerFile = Files.Find((PlannerFile f) => f.Id == fileId);
			if (plannerFile != null)
			{
				Files.Remove(plannerFile);
				return true;
			}
			foreach (PlannerFolder subFolder in SubFolders)
			{
				if (subFolder.RemoveFile(fileId))
				{
					return true;
				}
			}
			return false;
		}

		public bool RemoveFolder(string folderId)
		{
			PlannerFolder plannerFolder = SubFolders.Find((PlannerFolder f) => f.Id == folderId);
			if (plannerFolder != null)
			{
				SubFolders.Remove(plannerFolder);
				return true;
			}
			foreach (PlannerFolder subFolder in SubFolders)
			{
				if (subFolder.RemoveFolder(folderId))
				{
					return true;
				}
			}
			return false;
		}

		public List<PlannerFile> GetAllFiles()
		{
			List<PlannerFile> list = new List<PlannerFile>(Files);
			foreach (PlannerFolder subFolder in SubFolders)
			{
				list.AddRange(subFolder.GetAllFiles());
			}
			return list;
		}
	}
	[Serializable]
	public class PlannerFile
	{
		[JsonProperty("id")]
		public string Id { get; set; } = Guid.NewGuid().ToString();

		[JsonProperty("name")]
		public string Name { get; set; } = "New Note";

		[JsonProperty("content")]
		public string Content { get; set; } = "";

		[JsonProperty("created")]
		public DateTime Created { get; set; } = DateTime.Now;

		[JsonProperty("lastModified")]
		public DateTime LastModified { get; set; } = DateTime.Now;

		[JsonProperty("sortOrder")]
		public int SortOrder { get; set; } = 0;

		[JsonProperty("tags")]
		public List<string> Tags { get; set; } = new List<string>();

		[JsonProperty("pinned")]
		public bool Pinned { get; set; } = false;
	}
	[Serializable]
	public class PlannerWindowPosition
	{
		[JsonProperty("x")]
		public float X { get; set; } = 0f;

		[JsonProperty("y")]
		public float Y { get; set; } = 0f;

		[JsonProperty("width")]
		public float Width { get; set; } = 700f;

		[JsonProperty("height")]
		public float Height { get; set; } = 500f;
	}
	public class StationPlannerWindow : MonoBehaviour
	{
		private Canvas _windowCanvas;

		private GameObject _windowPanel;

		private RectTransform _windowRect;

		private static Material _translucentMaterial;

		private BlockEditor _blockEditor;

		private ScrollRect _fileTreeScrollRect;

		private RectTransform _fileTreeContent;

		private TextMeshProUGUI _currentFileLabel;

		private Button _btnH1;

		private Button _btnH2;

		private Button _btnH3;

		private Button _btnBullet;

		private Button _btnStrike;

		private Button _pauseButton;

		private TextMeshProUGUI _pauseButtonText;

		private Image _pauseButtonBg;

		private bool _isPaused = false;

		private static TMP_FontAsset _cachedFont;

		private static Material _cachedFontMaterial;

		private PlannerFileSystem _fileSystem;

		private PlannerFile _currentFile;

		private PlannerFolder _currentFolder;

		private HashSet<PlannerFolder> _selectedFolders = new HashSet<PlannerFolder>();

		private string _savePath;

		private bool _hasUnsavedChanges = false;

		private string _lastKnownSaveName = null;

		private const float MIN_WIDTH = 600f;

		private const float MIN_HEIGHT = 300f;

		private const float DEFAULT_WIDTH = 700f;

		private const float DEFAULT_HEIGHT = 480f;

		private const float TITLE_BAR_HEIGHT = 50f;

		private const float TOOLBAR_HEIGHT = 40f;

		private const float FILE_TREE_WIDTH = 180f;

		private static readonly Color BG_DARK = new Color(0.035f, 0.055f, 0.082f, 0.98f);

		private static readonly Color BG_PANEL = new Color(0.055f, 0.082f, 0.118f, 0.95f);

		private static readonly Color BG_INPUT = new Color(0.025f, 0.04f, 0.065f, 1f);

		private static readonly Color ACCENT = new Color(0.984f, 0.69f, 0.231f, 1f);

		private static readonly Color ACCENT_ORANGE = new Color(0.996f, 0.353f, 0.086f, 1f);

		private static readonly Color TEXT_NORMAL = new Color(0.9f, 0.9f, 0.9f, 1f);

		private static readonly Color TEXT_DIM = new Color(0.5f, 0.55f, 0.6f, 1f);

		private static readonly Color BORDER = new Color(0.984f, 0.69f, 0.231f, 0.8f);

		private static readonly Color BTN_NORMAL = new Color(0f, 0f, 0f, 0.5f);

		private static readonly Color BTN_HOVER = new Color(0f, 0f, 0f, 0.65f);

		private static readonly Color BTN_PRESS = new Color(0f, 0f, 0f, 0.8f);

		private static readonly Color SELECTED = new Color(0.984f, 0.69f, 0.231f, 0.3f);

		private static readonly Color FOLDER_PERSISTENT = new Color(0.4f, 0.7f, 1f, 1f);

		private GameObject _toolbarContainer;

		private List<Button> _toolbarButtons = new List<Button>();

		public static StationPlannerWindow Instance { get; private set; }

		public static bool IsOpen => (Object)(object)Instance != (Object)null && (Object)(object)Instance._windowCanvas != (Object)null && ((Behaviour)Instance._windowCanvas).enabled;

		public static void Initialize()
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Expected O, but got Unknown
			if (!((Object)(object)Instance != (Object)null))
			{
				GameObject val = new GameObject("StationPlannerWindow");
				Instance = val.AddComponent<StationPlannerWindow>();
				Object.DontDestroyOnLoad((Object)(object)val);
				Instance.CreateWindow();
				Instance.LoadFileSystem();
				Instance.Hide();
			}
		}

		public static void Cleanup()
		{
			if ((Object)(object)Instance != (Object)null)
			{
				Instance.SaveCurrentFile();
				Instance.SaveFileSystem();
				if ((Object)(object)Instance._windowPanel != (Object)null)
				{
					Object.Destroy((Object)(object)Instance._windowPanel);
				}
				if ((Object)(object)Instance._windowCanvas != (Object)null)
				{
					Object.Destroy((Object)(object)((Component)Instance._windowCanvas).gameObject);
				}
				Object.Destroy((Object)(object)((Component)Instance).gameObject);
				Instance = null;
			}
		}

		private void OnDestroy()
		{
			SaveCurrentFile();
			SaveFileSystem();
		}

		private void CreateWindow()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0100: Unknown result type (might be due to invalid IL or missing references)
			//IL_0111: Unknown result type (might be due to invalid IL or missing references)
			//IL_0121: Unknown result type (might be due to invalid IL or missing references)
			//IL_0127: Expected O, but got Unknown
			//IL_0147: Unknown result type (might be due to invalid IL or missing references)
			//IL_0153: Unknown result type (might be due to invalid IL or missing references)
			//IL_0169: Unknown result type (might be due to invalid IL or missing references)
			//IL_017f: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01df: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e6: Expected O, but got Unknown
			//IL_020a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0217: Unknown result type (might be due to invalid IL or missing references)
			//IL_0224: Unknown result type (might be due to invalid IL or missing references)
			//IL_0231: Unknown result type (might be due to invalid IL or missing references)
			//IL_0279: Unknown result type (might be due to invalid IL or missing references)
			//IL_0292: Unknown result type (might be due to invalid IL or missing references)
			//IL_0299: Expected O, but got Unknown
			//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_032c: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = new GameObject("StationPlannerCanvas");
			val.transform.SetParent(((Component)this).transform);
			_windowCanvas = val.AddComponent<Canvas>();
			_windowCanvas.renderMode = (RenderMode)0;
			_windowCanvas.sortingOrder = 900;
			Canvas windowCanvas = _windowCanvas;
			windowCanvas.additionalShaderChannels = (AdditionalCanvasShaderChannels)(windowCanvas.additionalShaderChannels | 1);
			CanvasScaler val2 = val.AddComponent<CanvasScaler>();
			val2.uiScaleMode = (ScaleMode)1;
			val2.referenceResolution = new Vector2(1920f, 1080f);
			val.AddComponent<GraphicRaycaster>();
			_windowPanel = CreatePanel(val.transform, "Window", 700f, 480f);
			_windowRect = _windowPanel.GetComponent<RectTransform>();
			_windowRect.anchorMin = new Vector2(0.5f, 0.5f);
			_windowRect.anchorMax = new Vector2(0.5f, 0.5f);
			_windowRect.pivot = new Vector2(0.5f, 0.5f);
			_windowRect.anchoredPosition = Vector2.zero;
			GameObject val3 = new GameObject("WindowShadow");
			val3.transform.SetParent(_windowPanel.transform, false);
			RectTransform val4 = val3.AddComponent<RectTransform>();
			val4.anchorMin = Vector2.zero;
			val4.anchorMax = Vector2.one;
			val4.offsetMin = new Vector2(-8f, -8f);
			val4.offsetMax = new Vector2(8f, 8f);
			Image val5 = val3.AddComponent<Image>();
			val5.sprite = LoadSlicedSprite("window-shadow.png", 16);
			val5.type = (Type)1;
			((Graphic)val5).color = new Color(1f, 1f, 1f, 0.5f);
			((Graphic)val5).raycastTarget = false;
			GameObject val6 = new GameObject("DialogBg");
			val6.transform.SetParent(_windowPanel.transform, false);
			RectTransform val7 = val6.AddComponent<RectTransform>();
			val7.anchorMin = Vector2.zero;
			val7.anchorMax = Vector2.one;
			val7.offsetMin = Vector2.zero;
			val7.offsetMax = Vector2.zero;
			Image val8 = val6.AddComponent<Image>();
			val8.sprite = LoadSlicedSprite("dialog-bg.png", 18);
			val8.type = (Type)1;
			((Graphic)val8).color = new Color(0f, 0.165f, 0.278f, 1f);
			((Graphic)val8).raycastTarget = true;
			GameObject val9 = new GameObject("DialogOutline");
			val9.transform.SetParent(_windowPanel.transform, false);
			RectTransform val10 = val9.AddComponent<RectTransform>();
			val10.anchorMin = Vector2.zero;
			val10.anchorMax = Vector2.one;
			val10.offsetMin = Vector2.zero;
			val10.offsetMax = Vector2.zero;
			Image val11 = val9.AddComponent<Image>();
			val11.sprite = LoadSlicedSprite("dialog-outline.png", 18);
			val11.type = (Type)1;
			((Graphic)val11).color = new Color(0.996f, 0.353f, 0.086f, 1f);
			((Graphic)val11).raycastTarget = false;
			CreateTitleBar(_windowPanel.transform);
			CreateToolbar(_windowPanel.transform);
			CreateMainContent(_windowPanel.transform);
			CreateResizeHandle(_windowPanel.transform);
		}

		private Sprite LoadSlicedSprite(string filename, int borderSize)
		{
			return StationpediaAscendedMod.LoadSlicedSprite(filename, borderSize);
		}

		private Sprite LoadSprite(string filename)
		{
			return StationpediaAscendedMod.LoadImageFromModFolder(filename);
		}

		private TMP_FontAsset GetCachedFont()
		{
			if ((Object)(object)_cachedFont != (Object)null)
			{
				return _cachedFont;
			}
			try
			{
				TextMeshProUGUI val = Object.FindObjectOfType<TextMeshProUGUI>();
				if ((Object)(object)val != (Object)null)
				{
					_cachedFont = ((TMP_Text)val).font;
					_cachedFontMaterial = ((TMP_Text)val).fontSharedMaterial;
					ManualLogSource log = StationpediaAscendedMod.Log;
					if (log != null)
					{
						TMP_FontAsset cachedFont = _cachedFont;
						log.LogInfo((object)("[StationPlanner] Cached font: " + ((cachedFont != null) ? ((Object)cachedFont).name : null)));
					}
				}
			}
			catch (Exception ex)
			{
				ManualLogSource log2 = StationpediaAscendedMod.Log;
				if (log2 != null)
				{
					log2.LogError((object)("[StationPlanner] Failed to cache font: " + ex.Message));
				}
			}
			return _cachedFont;
		}

		private void ApplyGameFont(TextMeshProUGUI textComp)
		{
			TMP_FontAsset cachedFont = GetCachedFont();
			if ((Object)(object)cachedFont != (Object)null)
			{
				((TMP_Text)textComp).font = cachedFont;
				if ((Object)(object)_cachedFontMaterial != (Object)null)
				{
					((TMP_Text)textComp).fontSharedMaterial = _cachedFontMaterial;
				}
			}
		}

		private Material GetTranslucentMaterial()
		{
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Expected O, but got Unknown
			if ((Object)(object)_translucentMaterial != (Object)null)
			{
				return _translucentMaterial;
			}
			Shader val = Shader.Find("UI/TranslucentImage");
			if ((Object)(object)val == (Object)null)
			{
				ManualLogSource log = StationpediaAscendedMod.Log;
				if (log != null)
				{
					log.LogError((object)"[StationPlanner] UI/TranslucentImage shader not found!");
				}
				return null;
			}
			_translucentMaterial = new Material(val);
			((Object)_translucentMaterial).name = "StationPlanner-Translucent";
			ManualLogSource log2 = StationpediaAscendedMod.Log;
			if (log2 != null)
			{
				log2.LogInfo((object)"[StationPlanner] Created TranslucentImage material from shader");
			}
			return _translucentMaterial;
		}

		private Image CreatePanelBackground(GameObject go, Sprite sprite, Color color)
		{
			//IL_0100: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			Material translucentMaterial = GetTranslucentMaterial();
			if ((Object)(object)translucentMaterial != (Object)null)
			{
				TranslucentImage val = go.AddComponent<TranslucentImage>();
				if ((Object)(object)sprite != (Object)null)
				{
					((Image)val).sprite = sprite;
					((Image)val).type = (Type)1;
				}
				((Graphic)val).color = color;
				val.vibrancy = 2f;
				val.brightness = -0.257f;
				val.flatten = 0.2f;
				val.spriteBlending = 1f;
				((Graphic)val).material = translucentMaterial;
				ManualLogSource log = StationpediaAscendedMod.Log;
				if (log != null)
				{
					string[] obj = new string[6]
					{
						"[StationPlanner] Created TranslucentImage on ",
						((Object)go).name,
						" - material=",
						((Object)translucentMaterial).name,
						", shader=",
						null
					};
					Shader shader = translucentMaterial.shader;
					obj[5] = ((shader != null) ? ((Object)shader).name : null);
					log.LogInfo((object)string.Concat(obj));
				}
				return (Image)(object)val;
			}
			Image val2 = go.AddComponent<Image>();
			if ((Object)(object)sprite != (Object)null)
			{
				val2.sprite = sprite;
				val2.type = (Type)1;
			}
			((Graphic)val2).color = color;
			return val2;
		}

		private GameObject CreatePanel(Transform parent, string name, float width, float height)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Expected O, but got Unknown
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = new GameObject(name);
			val.transform.SetParent(parent, false);
			RectTransform val2 = val.AddComponent<RectTransform>();
			val2.sizeDelta = new Vector2(width, height);
			return val;
		}

		private void CreateTitleBar(Transform parent)
		{
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a1: Expected O, but got Unknown
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0110: Unknown result type (might be due to invalid IL or missing references)
			//IL_011d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0134: Unknown result type (might be due to invalid IL or missing references)
			//IL_014b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0188: Unknown result type (might be due to invalid IL or missing references)
			//IL_019f: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0201: Expected O, but got Unknown
			//IL_025c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0273: Unknown result type (might be due to invalid IL or missing references)
			//IL_028a: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c9: Expected O, but got Unknown
			//IL_030a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0321: Unknown result type (might be due to invalid IL or missing references)
			//IL_0338: Unknown result type (might be due to invalid IL or missing references)
			//IL_034f: Unknown result type (might be due to invalid IL or missing references)
			//IL_036d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0377: Expected O, but got Unknown
			GameObject val = CreatePanel(parent, "TitleBar", 0f, 50f);
			RectTransform component = val.GetComponent<RectTransform>();
			component.anchorMin = new Vector2(0f, 1f);
			component.anchorMax = new Vector2(1f, 1f);
			component.pivot = new Vector2(0.5f, 1f);
			component.anchoredPosition = Vector2.zero;
			component.sizeDelta = new Vector2(0f, 50f);
			WindowDragHandler windowDragHandler = val.AddComponent<WindowDragHandler>();
			windowDragHandler.WindowRect = _windowRect;
			GameObject val2 = new GameObject("Title");
			val2.transform.SetParent(val.transform, false);
			TextMeshProUGUI val3 = val2.AddComponent<TextMeshProUGUI>();
			((TMP_Text)val3).text = "STATION NOTEPAD";
			((TMP_Text)val3).fontSize = 20f;
			((TMP_Text)val3).fontStyle = (FontStyles)1;
			((Graphic)val3).color = TEXT_NORMAL;
			((TMP_Text)val3).alignment = (TextAlignmentOptions)4097;
			((TMP_Text)val3).characterSpacing = 4f;
			RectTransform component2 = val2.GetComponent<RectTransform>();
			component2.anchorMin = Vector2.zero;
			component2.anchorMax = Vector2.one;
			component2.offsetMin = new Vector2(18f, 4f);
			component2.offsetMax = new Vector2(-110f, -10f);
			GameObject val4 = CreateTitleBarButton(val.transform, "||", 28f, 28f);
			RectTransform component3 = val4.GetComponent<RectTransform>();
			component3.anchorMin = new Vector2(1f, 0.5f);
			component3.anchorMax = new Vector2(1f, 0.5f);
			component3.pivot = new Vector2(1f, 0.5f);
			component3.anchoredPosition = new Vector2(-84f, 0f);
			_pauseButton = val4.GetComponent<Button>();
			((UnityEvent)_pauseButton.onClick).AddListener(new UnityAction(TogglePause));
			_pauseButtonText = val4.GetComponentInChildren<TextMeshProUGUI>();
			_pauseButtonBg = val4.GetComponent<Image>();
			AddSimpleTooltip(val4, "Pause/Resume game");
			GameObject val5 = CreateTitleBarButton(val.transform, "?", 28f, 28f);
			RectTransform component4 = val5.GetComponent<RectTransform>();
			component4.anchorMin = new Vector2(1f, 0.5f);
			component4.anchorMax = new Vector2(1f, 0.5f);
			component4.pivot = new Vector2(1f, 0.5f);
			component4.anchoredPosition = new Vector2(-50f, 0f);
			((UnityEvent)val5.GetComponent<Button>().onClick).AddListener(new UnityAction(ShowHelpDialog));
			AddSimpleTooltip(val5, "Show help guide");
			GameObject val6 = CreateTitleBarButton(val.transform, "X", 28f, 28f);
			RectTransform component5 = val6.GetComponent<RectTransform>();
			component5.anchorMin = new Vector2(1f, 0.5f);
			component5.anchorMax = new Vector2(1f, 0.5f);
			component5.pivot = new Vector2(1f, 0.5f);
			component5.anchoredPosition = new Vector2(-16f, 0f);
			((UnityEvent)val6.GetComponent<Button>().onClick).AddListener(new UnityAction(Hide));
			AddSimpleTooltip(val6, "Close Station Notepad");
		}

		private void CreateToolbar(Transform parent)
		{
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00be: Expected O, but got Unknown
			//IL_011c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Expected O, but got Unknown
			//IL_0162: Unknown result type (might be due to invalid IL or missing references)
			//IL_016c: Expected O, but got Unknown
			//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b4: Expected O, but got Unknown
			//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fd: Expected O, but got Unknown
			//IL_0268: Unknown result type (might be due to invalid IL or missing references)
			//IL_0272: Expected O, but got Unknown
			//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d9: Expected O, but got Unknown
			//IL_0336: Unknown result type (might be due to invalid IL or missing references)
			//IL_0340: Expected O, but got Unknown
			//IL_038b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0395: Expected O, but got Unknown
			//IL_03e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ea: Expected O, but got Unknown
			//IL_042d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0437: Expected O, but got Unknown
			_toolbarContainer = CreatePanel(parent, "Toolbar", 0f, 40f);
			RectTransform component = _toolbarContainer.GetComponent<RectTransform>();
			component.anchorMin = new Vector2(0f, 1f);
			component.anchorMax = new Vector2(1f, 1f);
			component.pivot = new Vector2(0.5f, 1f);
			component.anchoredPosition = new Vector2(0f, -50f);
			component.sizeDelta = new Vector2(0f, 40f);
			HorizontalLayoutGroup val = _toolbarContainer.AddComponent<HorizontalLayoutGroup>();
			((HorizontalOrVerticalLayoutGroup)val).spacing = 5f;
			((LayoutGroup)val).padding = new RectOffset(8, 8, 4, 4);
			((LayoutGroup)val).childAlignment = (TextAnchor)3;
			((HorizontalOrVerticalLayoutGroup)val).childControlWidth = false;
			((HorizontalOrVerticalLayoutGroup)val).childControlHeight = false;
			((HorizontalOrVerticalLayoutGroup)val).childForceExpandWidth = false;
			((HorizontalOrVerticalLayoutGroup)val).childForceExpandHeight = false;
			_toolbarButtons.Clear();
			Button val2 = CreateToolbarBtn(_toolbarContainer.transform, "Note", 55f);
			((UnityEvent)val2.onClick).AddListener(new UnityAction(OnNewFile));
			AddSimpleTooltip(((Component)val2).gameObject, "Create a new note file");
			Button val3 = CreateToolbarBtn(_toolbarContainer.transform, "Folder", 70f);
			((UnityEvent)val3.onClick).AddListener(new UnityAction(OnNewFolder));
			AddSimpleTooltip(((Component)val3).gameObject, "Create a new folder");
			Button val4 = CreateToolbarBtn(_toolbarContainer.transform, "Rename", 75f);
			((UnityEvent)val4.onClick).AddListener(new UnityAction(OnRename));
			AddSimpleTooltip(((Component)val4).gameObject, "Rename selected file or folder");
			Button val5 = CreateToolbarBtn(_toolbarContainer.transform, "Delete", 70f);
			((UnityEvent)val5.onClick).AddListener(new UnityAction(OnDelete));
			AddSimpleTooltip(((Component)val5).gameObject, "Delete selected file or folder");
			CreateToolbarSeparator(_toolbarContainer.transform);
			_btnH1 = CreateToolbarBtn(_toolbarContainer.transform, "H1", 36f);
			((TMP_Text)((Component)_btnH1).GetComponentInChildren<TextMeshProUGUI>()).fontStyle = (FontStyles)1;
			((UnityEvent)_btnH1.onClick).AddListener((UnityAction)delegate
			{
				ApplyLineStyle(LineStyle.H1);
			});
			AddSimpleTooltip(((Component)_btnH1).gameObject, "Heading 1 - Large title");
			_btnH2 = CreateToolbarBtn(_toolbarContainer.transform, "H2", 36f);
			((TMP_Text)((Component)_btnH2).GetComponentInChildren<TextMeshProUGUI>()).fontStyle = (FontStyles)1;
			((UnityEvent)_btnH2.onClick).AddListener((UnityAction)delegate
			{
				ApplyLineStyle(LineStyle.H2);
			});
			AddSimpleTooltip(((Component)_btnH2).gameObject, "Heading 2 - Medium title");
			_btnH3 = CreateToolbarBtn(_toolbarContainer.transform, "H3", 36f);
			((TMP_Text)((Component)_btnH3).GetComponentInChildren<TextMeshProUGUI>()).fontStyle = (FontStyles)1;
			((UnityEvent)_btnH3.onClick).AddListener((UnityAction)delegate
			{
				ApplyLineStyle(LineStyle.H3);
			});
			AddSimpleTooltip(((Component)_btnH3).gameObject, "Heading 3 - Small title");
			_btnBullet = CreateToolbarBtn(_toolbarContainer.transform, "•", 28f);
			((UnityEvent)_btnBullet.onClick).AddListener((UnityAction)delegate
			{
				ApplyLineStyle(LineStyle.Bullet);
			});
			AddSimpleTooltip(((Component)_btnBullet).gameObject, "Bullet point - Toggle bullet style on current line");
			_btnStrike = CreateToolbarBtn(_toolbarContainer.transform, "/", 28f);
			((UnityEvent)_btnStrike.onClick).AddListener((UnityAction)delegate
			{
				ApplyLineStyle(LineStyle.Strikethrough);
			});
			AddSimpleTooltip(((Component)_btnStrike).gameObject, "Strikethrough - Grey crossed-out text");
			Button val6 = CreateToolbarBtn(_toolbarContainer.transform, "¶", 28f);
			((UnityEvent)val6.onClick).AddListener((UnityAction)delegate
			{
				ApplyLineStyle(LineStyle.Normal);
			});
			AddSimpleTooltip(((Component)val6).gameObject, "Reset to normal text");
		}

		private void CreateMainContent(Transform parent)
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = CreatePanel(parent, "ContentArea", 0f, 0f);
			RectTransform component = val.GetComponent<RectTransform>();
			component.anchorMin = Vector2.zero;
			component.anchorMax = Vector2.one;
			component.offsetMin = new Vector2(16f, 16f);
			component.offsetMax = new Vector2(-16f, -102f);
			CreateFileTreePanel(val.transform);
			CreateEditorPanel(val.transform);
		}

		private void CreateFileTreePanel(Transform parent)
		{
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0108: Unknown result type (might be due to invalid IL or missing references)
			//IL_0115: Unknown result type (might be due to invalid IL or missing references)
			//IL_0122: Unknown result type (might be due to invalid IL or missing references)
			//IL_0147: Unknown result type (might be due to invalid IL or missing references)
			//IL_015d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0167: Expected O, but got Unknown
			//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_020f: Unknown result type (might be due to invalid IL or missing references)
			//IL_026b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0286: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e3: Expected O, but got Unknown
			//IL_0331: Unknown result type (might be due to invalid IL or missing references)
			//IL_0338: Expected O, but got Unknown
			//IL_0357: Unknown result type (might be due to invalid IL or missing references)
			//IL_0364: Unknown result type (might be due to invalid IL or missing references)
			//IL_0371: Unknown result type (might be due to invalid IL or missing references)
			//IL_037e: Unknown result type (might be due to invalid IL or missing references)
			//IL_03e2: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = CreatePanel(parent, "FileTree", 180f, 0f);
			RectTransform component = val.GetComponent<RectTransform>();
			component.anchorMin = new Vector2(0f, 0f);
			component.anchorMax = new Vector2(0f, 1f);
			component.pivot = new Vector2(0f, 0.5f);
			component.anchoredPosition = Vector2.zero;
			component.sizeDelta = new Vector2(180f, 0f);
			Image val2 = val.AddComponent<Image>();
			Sprite val3 = LoadSlicedSprite("button-bg.png", 7);
			if ((Object)(object)val3 != (Object)null)
			{
				val2.sprite = val3;
				val2.type = (Type)1;
			}
			((Graphic)val2).color = new Color(0f, 0f, 0f, 0.75f);
			((Graphic)val2).raycastTarget = true;
			Button val4 = val.AddComponent<Button>();
			((Selectable)val4).targetGraphic = (Graphic)(object)val2;
			ColorBlock colors = default(ColorBlock);
			((ColorBlock)(ref colors)).normalColor = Color.clear;
			((ColorBlock)(ref colors)).highlightedColor = Color.clear;
			((ColorBlock)(ref colors)).pressedColor = Color.clear;
			((ColorBlock)(ref colors)).selectedColor = Color.clear;
			((ColorBlock)(ref colors)).colorMultiplier = 1f;
			((ColorBlock)(ref colors)).fadeDuration = 0f;
			((Selectable)val4).colors = colors;
			((UnityEvent)val4.onClick).AddListener(new UnityAction(DeselectAll));
			_fileTreeScrollRect = val.AddComponent<ScrollRect>();
			_fileTreeScrollRect.horizontal = false;
			_fileTreeScrollRect.vertical = true;
			_fileTreeScrollRect.movementType = (MovementType)2;
			_fileTreeScrollRect.scrollSensitivity = 25f;
			GameObject val5 = CreatePanel(val.transform, "Viewport", 0f, 0f);
			RectTransform component2 = val5.GetComponent<RectTransform>();
			component2.anchorMin = Vector2.zero;
			component2.anchorMax = Vector2.one;
			component2.offsetMin = new Vector2(4f, 4f);
			component2.offsetMax = new Vector2(-4f, -4f);
			val5.AddComponent<RectMask2D>();
			_fileTreeScrollRect.viewport = component2;
			GameObject val6 = CreatePanel(val5.transform, "Content", 0f, 0f);
			_fileTreeContent = val6.GetComponent<RectTransform>();
			_fileTreeContent.anchorMin = new Vector2(0f, 1f);
			_fileTreeContent.anchorMax = new Vector2(1f, 1f);
			_fileTreeContent.pivot = new Vector2(0f, 1f);
			_fileTreeContent.anchoredPosition = Vector2.zero;
			VerticalLayoutGroup val7 = val6.AddComponent<VerticalLayoutGroup>();
			((HorizontalOrVerticalLayoutGroup)val7).spacing = 1f;
			((LayoutGroup)val7).padding = new RectOffset(2, 2, 2, 2);
			((HorizontalOrVerticalLayoutGroup)val7).childControlWidth = true;
			((HorizontalOrVerticalLayoutGroup)val7).childControlHeight = false;
			((HorizontalOrVerticalLayoutGroup)val7).childForceExpandWidth = true;
			((HorizontalOrVerticalLayoutGroup)val7).childForceExpandHeight = false;
			ContentSizeFitter val8 = val6.AddComponent<ContentSizeFitter>();
			val8.verticalFit = (FitMode)2;
			_fileTreeScrollRect.content = _fileTreeContent;
			GameObject val9 = new GameObject("Outline");
			val9.transform.SetParent(val.transform, false);
			RectTransform val10 = val9.AddComponent<RectTransform>();
			val10.anchorMin = Vector2.zero;
			val10.anchorMax = Vector2.one;
			val10.offsetMin = Vector2.zero;
			val10.offsetMax = Vector2.zero;
			Image val11 = val9.AddComponent<Image>();
			Sprite val12 = LoadSlicedSprite("slot-outline.png", 8);
			if ((Object)(object)val12 != (Object)null)
			{
				val11.sprite = val12;
				val11.type = (Type)1;
				val11.fillCenter = false;
			}
			((Graphic)val11).color = new Color(0f, 0f, 0f, 0.5f);
			((Graphic)val11).raycastTarget = false;
		}

		private void CreateEditorPanel(Transform parent)
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Expected O, but got Unknown
			//IL_0109: Unknown result type (might be due to invalid IL or missing references)
			//IL_0147: Unknown result type (might be due to invalid IL or missing references)
			//IL_015e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0175: Unknown result type (might be due to invalid IL or missing references)
			//IL_018c: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ba: Expected O, but got Unknown
			//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0214: Unknown result type (might be due to invalid IL or missing references)
			//IL_035e: Unknown result type (might be due to invalid IL or missing references)
			//IL_036b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0378: Unknown result type (might be due to invalid IL or missing references)
			//IL_0385: Unknown result type (might be due to invalid IL or missing references)
			//IL_039b: Unknown result type (might be due to invalid IL or missing references)
			//IL_03d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_041f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0426: Expected O, but got Unknown
			//IL_0445: Unknown result type (might be due to invalid IL or missing references)
			//IL_0452: Unknown result type (might be due to invalid IL or missing references)
			//IL_045f: Unknown result type (might be due to invalid IL or missing references)
			//IL_046c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0266: Unknown result type (might be due to invalid IL or missing references)
			//IL_026d: Expected O, but got Unknown
			//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_02de: Unknown result type (might be due to invalid IL or missing references)
			//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0305: Unknown result type (might be due to invalid IL or missing references)
			//IL_04d0: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = CreatePanel(parent, "Editor", 0f, 0f);
			RectTransform component = val.GetComponent<RectTransform>();
			component.anchorMin = Vector2.zero;
			component.anchorMax = Vector2.one;
			component.offsetMin = new Vector2(190f, 0f);
			component.offsetMax = Vector2.zero;
			Image val2 = val.AddComponent<Image>();
			Sprite val3 = LoadSlicedSprite("button-bg.png", 7);
			if ((Object)(object)val3 != (Object)null)
			{
				val2.sprite = val3;
				val2.type = (Type)1;
			}
			((Graphic)val2).color = new Color(0f, 0f, 0f, 0.75f);
			((Graphic)val2).raycastTarget = true;
			GameObject val4 = new GameObject("FileLabel");
			val4.transform.SetParent(val.transform, false);
			_currentFileLabel = val4.AddComponent<TextMeshProUGUI>();
			((TMP_Text)_currentFileLabel).text = "No file selected";
			((TMP_Text)_currentFileLabel).fontSize = 11f;
			((Graphic)_currentFileLabel).color = TEXT_DIM;
			((TMP_Text)_currentFileLabel).alignment = (TextAlignmentOptions)260;
			((Graphic)_currentFileLabel).raycastTarget = false;
			RectTransform component2 = val4.GetComponent<RectTransform>();
			component2.anchorMin = new Vector2(0f, 1f);
			component2.anchorMax = new Vector2(1f, 1f);
			component2.pivot = new Vector2(1f, 1f);
			component2.anchoredPosition = new Vector2(-8f, -4f);
			component2.sizeDelta = new Vector2(0f, 16f);
			GameObject val5 = new GameObject("EditorContainer");
			val5.transform.SetParent(val.transform, false);
			RectTransform val6 = val5.AddComponent<RectTransform>();
			val6.anchorMin = Vector2.zero;
			val6.anchorMax = Vector2.one;
			val6.offsetMin = new Vector2(8f, 8f);
			val6.offsetMax = new Vector2(-8f, -22f);
			_blockEditor = val5.AddComponent<BlockEditor>();
			if (!_blockEditor.Initialize(val5.transform))
			{
				ManualLogSource log = StationpediaAscendedMod.Log;
				if (log != null)
				{
					log.LogError((object)"[StationPlanner] Failed to initialize BlockEditor - AssetBundle may be missing");
				}
				GameObject val7 = new GameObject("ErrorText");
				val7.transform.SetParent(val5.transform, false);
				TextMeshProUGUI val8 = val7.AddComponent<TextMeshProUGUI>();
				((TMP_Text)val8).text = "ERROR: Could not load editor prefab.\n\nPlease ensure 'stationpediaascended_ui' AssetBundle\nis in the BepInEx/scripts folder.";
				((TMP_Text)val8).fontSize = 14f;
				((Graphic)val8).color = new Color(1f, 0.4f, 0.4f, 1f);
				((TMP_Text)val8).alignment = (TextAlignmentOptions)514;
				RectTransform component3 = val7.GetComponent<RectTransform>();
				component3.anchorMin = Vector2.zero;
				component3.anchorMax = Vector2.one;
				component3.offsetMin = Vector2.zero;
				component3.offsetMax = Vector2.zero;
			}
			else
			{
				_blockEditor.OnContentChanged += OnEditorContentChanged;
				if ((Object)(object)_blockEditor.InputField != (Object)null)
				{
					RectTransform component4 = ((Component)_blockEditor.InputField).GetComponent<RectTransform>();
					component4.anchorMin = Vector2.zero;
					component4.anchorMax = Vector2.one;
					component4.offsetMin = Vector2.zero;
					component4.offsetMax = Vector2.zero;
					_blockEditor.InputField.caretColor = ACCENT;
					_blockEditor.InputField.customCaretColor = true;
					_blockEditor.InputField.caretWidth = 2;
					_blockEditor.InputField.selectionColor = SELECTED;
					((UnityEvent<string>)(object)_blockEditor.InputField.onValueChanged).AddListener((UnityAction<string>)delegate
					{
						_hasUnsavedChanges = true;
					});
					ManualLogSource log2 = StationpediaAscendedMod.Log;
					if (log2 != null)
					{
						log2.LogInfo((object)"[StationPlanner] BlockEditor initialized successfully");
					}
				}
			}
			GameObject val9 = new GameObject("Outline");
			val9.transform.SetParent(val.transform, false);
			RectTransform val10 = val9.AddComponent<RectTransform>();
			val10.anchorMin = Vector2.zero;
			val10.anchorMax = Vector2.one;
			val10.offsetMin = Vector2.zero;
			val10.offsetMax = Vector2.zero;
			Image val11 = val9.AddComponent<Image>();
			Sprite val12 = LoadSlicedSprite("slot-outline.png", 8);
			if ((Object)(object)val12 != (Object)null)
			{
				val11.sprite = val12;
				val11.type = (Type)1;
				val11.fillCenter = false;
			}
			((Graphic)val11).color = new Color(0f, 0f, 0f, 0.5f);
			((Graphic)val11).raycastTarget = false;
		}

		private void CreateIC10Scrollbar(Transform parent, ScrollRect scrollRect)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Expected O, but got Unknown
			//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0113: Unknown result type (might be due to invalid IL or missing references)
			//IL_012a: Unknown result type (might be due to invalid IL or missing references)
			//IL_013a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0141: Expected O, but got Unknown
			//IL_0161: Unknown result type (might be due to invalid IL or missing references)
			//IL_016e: Unknown result type (might be due to invalid IL or missing references)
			//IL_017b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0188: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_0201: Unknown result type (might be due to invalid IL or missing references)
			//IL_0206: Unknown result type (might be due to invalid IL or missing references)
			//IL_021e: Unknown result type (might be due to invalid IL or missing references)
			//IL_023f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0260: Unknown result type (might be due to invalid IL or missing references)
			//IL_026d: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = new GameObject("Scrollbar");
			val.transform.SetParent(parent, false);
			RectTransform val2 = val.AddComponent<RectTransform>();
			val2.anchorMin = new Vector2(1f, 0f);
			val2.anchorMax = new Vector2(1f, 1f);
			val2.pivot = new Vector2(1f, 0.5f);
			val2.anchoredPosition = Vector2.zero;
			val2.sizeDelta = new Vector2(12f, 0f);
			Image val3 = val.AddComponent<Image>();
			Sprite val4 = LoadSlicedSprite("scrollbar-bg.png", 4);
			if ((Object)(object)val4 != (Object)null)
			{
				val3.sprite = val4;
				val3.type = (Type)1;
			}
			((Graphic)val3).color = Color.white;
			GameObject val5 = new GameObject("Sliding Area");
			val5.transform.SetParent(val.transform, false);
			RectTransform val6 = val5.AddComponent<RectTransform>();
			val6.anchorMin = Vector2.zero;
			val6.anchorMax = Vector2.one;
			val6.offsetMin = new Vector2(2f, 2f);
			val6.offsetMax = new Vector2(-2f, -2f);
			GameObject val7 = new GameObject("Handle");
			val7.transform.SetParent(val5.transform, false);
			RectTransform val8 = val7.AddComponent<RectTransform>();
			val8.anchorMin = Vector2.zero;
			val8.anchorMax = Vector2.one;
			val8.offsetMin = Vector2.zero;
			val8.offsetMax = Vector2.zero;
			Image val9 = val7.AddComponent<Image>();
			Sprite val10 = LoadSlicedSprite("scrollbar-handle.png", 4);
			if ((Object)(object)val10 != (Object)null)
			{
				val9.sprite = val10;
				val9.type = (Type)1;
			}
			((Graphic)val9).color = Color.white;
			Scrollbar val11 = val.AddComponent<Scrollbar>();
			val11.handleRect = val8;
			val11.direction = (Direction)2;
			((Selectable)val11).targetGraphic = (Graphic)(object)val9;
			ColorBlock colors = ((Selectable)val11).colors;
			((ColorBlock)(ref colors)).normalColor = new Color(0.8f, 0.8f, 0.8f, 1f);
			((ColorBlock)(ref colors)).highlightedColor = new Color(1f, 1f, 1f, 1f);
			((ColorBlock)(ref colors)).pressedColor = new Color(0.6f, 0.6f, 0.6f, 1f);
			((Selectable)val11).colors = colors;
			scrollRect.verticalScrollbar = val11;
			scrollRect.verticalScrollbarVisibility = (ScrollbarVisibility)2;
			scrollRect.verticalScrollbarSpacing = 2f;
		}

		private void CreateResizeHandle(Transform parent)
		{
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Expected O, but got Unknown
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0113: Unknown result type (might be due to invalid IL or missing references)
			//IL_012d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0147: Unknown result type (might be due to invalid IL or missing references)
			//IL_0163: Unknown result type (might be due to invalid IL or missing references)
			//IL_019c: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = CreatePanel(parent, "ResizeHandle", 22f, 22f);
			RectTransform component = val.GetComponent<RectTransform>();
			component.anchorMin = new Vector2(1f, 0f);
			component.anchorMax = new Vector2(1f, 0f);
			component.pivot = new Vector2(1f, 0f);
			component.anchoredPosition = new Vector2(-1f, 1f);
			Image val2 = val.AddComponent<Image>();
			((Graphic)val2).color = Color.clear;
			for (int i = 0; i < 3; i++)
			{
				GameObject val3 = new GameObject($"GripLine{i}");
				val3.transform.SetParent(val.transform, false);
				RectTransform val4 = val3.AddComponent<RectTransform>();
				float num = 12.5f + (float)i * 4.5f;
				val4.anchorMin = new Vector2(1f, 0f);
				val4.anchorMax = new Vector2(1f, 0f);
				val4.pivot = new Vector2(1f, 0f);
				val4.sizeDelta = new Vector2(num + 1.8f, 1.8f);
				val4.anchoredPosition = new Vector2(-5f, num + 1.8f);
				((Transform)val4).localRotation = Quaternion.Euler(0f, 0f, 45f);
				Image val5 = val3.AddComponent<Image>();
				((Graphic)val5).color = new Color(TEXT_DIM.r, TEXT_DIM.g, TEXT_DIM.b, 0.5f);
				((Graphic)val5).raycastTarget = false;
			}
			WindowResizeHandler windowResizeHandler = val.AddComponent<WindowResizeHandler>();
			windowResizeHandler.WindowRect = _windowRect;
			windowResizeHandler.MinSize = new Vector2(600f, 300f);
		}

		private Button CreateToolbarBtn(Transform parent, string label, float width)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Expected O, but got Unknown
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0116: Unknown result type (might be due to invalid IL or missing references)
			//IL_0123: Unknown result type (might be due to invalid IL or missing references)
			//IL_014a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0157: Unknown result type (might be due to invalid IL or missing references)
			//IL_015e: Expected O, but got Unknown
			//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0207: Unknown result type (might be due to invalid IL or missing references)
			//IL_021e: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = new GameObject(label);
			val.transform.SetParent(parent, false);
			RectTransform val2 = val.AddComponent<RectTransform>();
			val2.sizeDelta = new Vector2(width, 26f);
			LayoutElement val3 = val.AddComponent<LayoutElement>();
			val3.preferredWidth = width;
			val3.preferredHeight = 26f;
			val3.minWidth = width;
			val3.minHeight = 26f;
			val3.flexibleWidth = 0f;
			val3.flexibleHeight = 0f;
			Image val4 = val.AddComponent<Image>();
			Sprite val5 = LoadSlicedSprite("button-bg.png", 7);
			if ((Object)(object)val5 != (Object)null)
			{
				val4.sprite = val5;
				val4.type = (Type)1;
			}
			((Graphic)val4).color = Color.white;
			Button val6 = val.AddComponent<Button>();
			((Selectable)val6).targetGraphic = (Graphic)(object)val4;
			ColorBlock colors = ((Selectable)val6).colors;
			((ColorBlock)(ref colors)).normalColor = BTN_NORMAL;
			((ColorBlock)(ref colors)).highlightedColor = new Color(1f, 1f, 1f, 0.5f);
			((ColorBlock)(ref colors)).pressedColor = new Color(1f, 0.396f, 0.125f, 0.125f);
			((ColorBlock)(ref colors)).selectedColor = BTN_NORMAL;
			((ColorBlock)(ref colors)).colorMultiplier = 1f;
			((ColorBlock)(ref colors)).fadeDuration = 0.1f;
			((Selectable)val6).colors = colors;
			GameObject val7 = new GameObject("Text");
			val7.transform.SetParent(val.transform, false);
			TextMeshProUGUI val8 = val7.AddComponent<TextMeshProUGUI>();
			((TMP_Text)val8).text = label.ToUpper();
			((TMP_Text)val8).fontSize = 11f;
			((TMP_Text)val8).fontStyle = (FontStyles)1;
			((TMP_Text)val8).characterSpacing = 2f;
			((Graphic)val8).color = TEXT_NORMAL;
			((TMP_Text)val8).alignment = (TextAlignmentOptions)514;
			((Graphic)val8).raycastTarget = false;
			ApplyGameFont(val8);
			RectTransform component = val7.GetComponent<RectTransform>();
			component.anchorMin = Vector2.zero;
			component.anchorMax = Vector2.one;
			component.offsetMin = new Vector2(2f, 0f);
			component.offsetMax = new Vector2(-2f, 0f);
			_toolbarButtons.Add(val6);
			return val6;
		}

		private GameObject CreateTextButton(Transform parent, string label, float w, float h)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Expected O, but got Unknown
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Expected O, but got Unknown
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0100: Unknown result type (might be due to invalid IL or missing references)
			//IL_010d: Unknown result type (might be due to invalid IL or missing references)
			//IL_011a: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = new GameObject(label);
			val.transform.SetParent(parent, false);
			RectTransform val2 = val.AddComponent<RectTransform>();
			val2.sizeDelta = new Vector2(w, h);
			Image val3 = val.AddComponent<Image>();
			((Graphic)val3).color = BTN_NORMAL;
			Button val4 = val.AddComponent<Button>();
			((Selectable)val4).targetGraphic = (Graphic)(object)val3;
			ColorBlock colors = ((Selectable)val4).colors;
			((ColorBlock)(ref colors)).normalColor = BTN_NORMAL;
			((ColorBlock)(ref colors)).highlightedColor = BTN_HOVER;
			((ColorBlock)(ref colors)).pressedColor = BTN_PRESS;
			((Selectable)val4).colors = colors;
			GameObject val5 = new GameObject("Text");
			val5.transform.SetParent(val.transform, false);
			TextMeshProUGUI val6 = val5.AddComponent<TextMeshProUGUI>();
			((TMP_Text)val6).text = label;
			((TMP_Text)val6).fontSize = 13f;
			((Graphic)val6).color = TEXT_NORMAL;
			((TMP_Text)val6).alignment = (TextAlignmentOptions)514;
			ApplyGameFont(val6);
			RectTransform component = val5.GetComponent<RectTransform>();
			component.anchorMin = Vector2.zero;
			component.anchorMax = Vector2.one;
			component.offsetMin = Vector2.zero;
			component.offsetMax = Vector2.zero;
			return val;
		}

		private GameObject CreateTitleBarButton(Transform parent, string label, float w, float h)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Expected O, but got Unknown
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0111: Expected O, but got Unknown
			//IL_015c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0191: Unknown result type (might be due to invalid IL or missing references)
			//IL_019e: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = new GameObject(label);
			val.transform.SetParent(parent, false);
			RectTransform val2 = val.AddComponent<RectTransform>();
			val2.sizeDelta = new Vector2(w, h);
			Image val3 = val.AddComponent<Image>();
			Sprite val4 = LoadSlicedSprite("button-bg.png", 7);
			if ((Object)(object)val4 != (Object)null)
			{
				val3.sprite = val4;
				val3.type = (Type)1;
			}
			((Graphic)val3).color = Color.white;
			Button val5 = val.AddComponent<Button>();
			((Selectable)val5).targetGraphic = (Graphic)(object)val3;
			ColorBlock colors = ((Selectable)val5).colors;
			((ColorBlock)(ref colors)).normalColor = BTN_NORMAL;
			((ColorBlock)(ref colors)).highlightedColor = new Color(1f, 1f, 1f, 0.5f);
			((ColorBlock)(ref colors)).pressedColor = new Color(1f, 0.396f, 0.125f, 0.125f);
			((ColorBlock)(ref colors)).selectedColor = BTN_NORMAL;
			((ColorBlock)(ref colors)).colorMultiplier = 1f;
			((ColorBlock)(ref colors)).fadeDuration = 0.1f;
			((Selectable)val5).colors = colors;
			GameObject val6 = new GameObject("Text");
			val6.transform.SetParent(val.transform, false);
			TextMeshProUGUI val7 = val6.AddComponent<TextMeshProUGUI>();
			((TMP_Text)val7).text = label;
			((TMP_Text)val7).fontSize = 11f;
			((TMP_Text)val7).fontStyle = (FontStyles)1;
			((TMP_Text)val7).characterSpacing = 2f;
			((Graphic)val7).color = TEXT_NORMAL;
			((TMP_Text)val7).alignment = (TextAlignmentOptions)514;
			((Graphic)val7).raycastTarget = false;
			ApplyGameFont(val7);
			RectTransform component = val6.GetComponent<RectTransform>();
			component.anchorMin = Vector2.zero;
			component.anchorMax = Vector2.one;
			component.offsetMin = new Vector2(2f, 0f);
			component.offsetMax = new Vector2(-2f, 0f);
			return val;
		}

		private void CreateToolbarSeparator(Transform parent)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = new GameObject("Sep");
			val.transform.SetParent(parent, false);
			RectTransform val2 = val.AddComponent<RectTransform>();
			val2.sizeDelta = new Vector2(2f, 20f);
			LayoutElement val3 = val.AddComponent<LayoutElement>();
			val3.preferredWidth = 2f;
			val3.preferredHeight = 20f;
			Image val4 = val.AddComponent<Image>();
			((Graphic)val4).color = new Color(1f, 1f, 1f, 0.3f);
			((Graphic)val4).raycastTarget = false;
		}

		private void AddSimpleTooltip(GameObject target, string text)
		{
			TooltipTrigger tooltipTrigger = target.AddComponent<TooltipTrigger>();
			tooltipTrigger.TooltipText = text;
		}

		public void Show()
		{
			if ((Object)(object)_windowCanvas != (Object)null)
			{
				((Behaviour)_windowCanvas).enabled = true;
				CursorManager.SetCursor(false);
				if ((Object)(object)_blockEditor != (Object)null && (Object)(object)_blockEditor.InputField != (Object)null)
				{
					_blockEditor.InputField.ActivateInputField();
				}
			}
		}

		public void Hide()
		{
			if ((Object)(object)_windowCanvas != (Object)null)
			{
				SaveCurrentFile();
				((Behaviour)_windowCanvas).enabled = false;
			}
		}

		public static void Toggle()
		{
			if ((Object)(object)Instance == (Object)null)
			{
				Initialize();
			}
			if (IsOpen)
			{
				Instance.Hide();
			}
			else
			{
				Instance.Show();
			}
		}

		public static void UpdateWindow()
		{
			if ((Object)(object)Instance == (Object)null)
			{
				return;
			}
			Instance.CheckForSaveChange();
			if (IsOpen)
			{
				if (Input.GetKey((KeyCode)306) && Input.GetKeyDown((KeyCode)115))
				{
					Instance.SaveCurrentFile();
					Instance.SaveFileSystem();
				}
				if (Input.GetKeyDown((KeyCode)27))
				{
					Instance.Hide();
				}
			}
		}

		private void CheckForSaveChange()
		{
			string currentSaveName = GetCurrentSaveName();
			string text = currentSaveName ?? "";
			string text2 = _lastKnownSaveName ?? "";
			if (text != text2)
			{
				Debug.Log((object)("[Station Notepad] Save changed from '" + text2 + "' to '" + text + "' - refreshing folder display"));
				_lastKnownSaveName = currentSaveName;
				_selectedFolders.Clear();
				_currentFolder = null;
				_currentFile = null;
				if ((Object)(object)_fileTreeContent != (Object)null)
				{
					RefreshFileTree();
				}
				if ((Object)(object)_currentFileLabel != (Object)null)
				{
					((TMP_Text)_currentFileLabel).text = "No file";
				}
			}
		}

		private string GetCurrentSaveName()
		{
			try
			{
				return XmlSaveLoad.Instance?.CurrentStationName;
			}
			catch
			{
				return null;
			}
		}

		private bool IsInMainMenu()
		{
			return string.IsNullOrEmpty(GetCurrentSaveName());
		}

		private void LoadFileSystem()
		{
			_savePath = GetSavePath();
			string text = Path.Combine(_savePath, "notepad_files.json");
			if (File.Exists(text))
			{
				try
				{
					_fileSystem = JsonConvert.DeserializeObject<PlannerFileSystem>(File.ReadAllText(text));
					Debug.Log((object)("[Station Notepad] Loaded file system from " + text));
				}
				catch (Exception ex)
				{
					Debug.LogError((object)("[Station Notepad] Failed to load file system: " + ex.Message));
					_fileSystem = new PlannerFileSystem();
				}
			}
			else
			{
				_fileSystem = new PlannerFileSystem();
				PlannerFolder plannerFolder = new PlannerFolder
				{
					Id = Guid.NewGuid().ToString(),
					Name = "Global Notes",
					PersistAcrossSaves = true,
					IsExpanded = true
				};
				plannerFolder.Files.Add(new PlannerFile
				{
					Id = Guid.NewGuid().ToString(),
					Name = "Welcome to Station Notepad",
					Content = "THANK YOU FOR SUBSCRIBING!\n=========================\n\nThank you for using Stationpedia Ascended!\n\nThis notepad is your personal space for notes,\nrecipes, and plans. Feel free to delete this\nnote once you've read it.\n\n-------------------------------------------\n\nHOW TO USE STATION NOTEPAD\n--------------------------\n\nCreating Notes & Folders:\n  * Click NOTE to create a new note\n  * Click FOLDER to create a new folder\n  * Right-click items to rename or delete\n  * Drag and drop to reorganize\n\nSaving:\n  * Notes auto-save when you switch files\n  * Notes auto-save when you close the window\n\n-------------------------------------------\n\nFOLDER TYPES\n------------\n\nGlobal Notes (marked with G):\n  * Visible in ALL save files\n  * Great for recipes and reference info\n\nPer-Save Folders:\n  * Only visible in that specific save\n  * Perfect for base plans and to-do lists\n  * Right-click a folder to make it per-save\n\n-------------------------------------------\n\nKEYBOARD SHORTCUTS\n------------------\n\n  F4      - Toggle notepad window\n  Escape  - Close window\n\n-------------------------------------------\n\nHappy note-taking, Stationeer!\n\nReport bugs or suggestions on the\nStationpedia Discord or to FlorpyDorp."
				});
				_fileSystem.RootFolder.SubFolders.Add(plannerFolder);
				Debug.Log((object)"[Station Notepad] Created new file system with welcome note");
			}
			RefreshFileTree();
			PlannerFile plannerFile = FindFirstFile(_fileSystem.RootFolder);
			if (plannerFile != null)
			{
				SelectFile(plannerFile);
			}
		}

		private bool ShouldShowFolder(PlannerFolder folder, string currentSave)
		{
			if (folder.PersistAcrossSaves)
			{
				return true;
			}
			if (string.IsNullOrEmpty(currentSave))
			{
				return false;
			}
			return folder.SaveName == currentSave;
		}

		private void SaveFileSystem()
		{
			if (_fileSystem == null)
			{
				Debug.Log((object)"[Station Notepad] SaveFileSystem: _fileSystem is null");
				return;
			}
			try
			{
				Debug.Log((object)("[Station Notepad] Saving to: " + _savePath));
				if (!Directory.Exists(_savePath))
				{
					Directory.CreateDirectory(_savePath);
					Debug.Log((object)("[Station Notepad] Created directory: " + _savePath));
				}
				string text = Path.Combine(_savePath, "notepad_files.json");
				string contents = JsonConvert.SerializeObject((object)_fileSystem, (Formatting)1);
				File.WriteAllText(text, contents);
				Debug.Log((object)("[Station Notepad] Saved file system to " + text));
			}
			catch (Exception ex)
			{
				Debug.LogError((object)("[Station Notepad] Save failed: " + ex.Message + "\n" + ex.StackTrace));
			}
		}

		private string GetSavePath()
		{
			try
			{
				string text = Path.Combine(Paths.BepInExRootPath, "scripts", "StationNotepadData");
				Debug.Log((object)("[Station Notepad] Save path: " + text));
				return text;
			}
			catch
			{
				string text2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "My Games", "Stationeers", "StationNotepadData");
				Debug.Log((object)("[Station Notepad] Fallback save path: " + text2));
				return text2;
			}
		}

		private void RefreshFileTree()
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Expected O, but got Unknown
			foreach (Transform item in (Transform)_fileTreeContent)
			{
				Transform val = item;
				Object.Destroy((Object)(object)((Component)val).gameObject);
			}
			BuildFolder(_fileSystem.RootFolder, 0);
		}

		private void BuildFolder(PlannerFolder folder, int depth)
		{
			string currentSaveName = GetCurrentSaveName();
			foreach (PlannerFolder subFolder in folder.SubFolders)
			{
				if (ShouldShowFolder(subFolder, currentSaveName))
				{
					CreateFolderRow(subFolder, depth);
					if (subFolder.IsExpanded)
					{
						BuildFolder(subFolder, depth + 1);
					}
				}
			}
			foreach (PlannerFile file in folder.Files)
			{
				CreateFileRow(file, depth);
			}
		}

		private void CreateFolderRow(PlannerFolder folder, int depth)
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Expected O, but got Unknown
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d7: Expected O, but got Unknown
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0111: Expected O, but got Unknown
			//IL_013a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0184: Unknown result type (might be due to invalid IL or missing references)
			//IL_017d: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a8: Expected O, but got Unknown
			//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0234: Unknown result type (might be due to invalid IL or missing references)
			//IL_025a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0261: Expected O, but got Unknown
			//IL_028a: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_02fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_030a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0317: Unknown result type (might be due to invalid IL or missing references)
			//IL_031e: Expected O, but got Unknown
			//IL_033e: Unknown result type (might be due to invalid IL or missing references)
			//IL_034b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0358: Unknown result type (might be due to invalid IL or missing references)
			//IL_0365: Unknown result type (might be due to invalid IL or missing references)
			//IL_022d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0226: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_040d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0417: Expected O, but got Unknown
			//IL_0449: Unknown result type (might be due to invalid IL or missing references)
			//IL_044e: Unknown result type (might be due to invalid IL or missing references)
			//IL_045d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0456: Unknown result type (might be due to invalid IL or missing references)
			//IL_0489: Unknown result type (might be due to invalid IL or missing references)
			//IL_0482: Unknown result type (might be due to invalid IL or missing references)
			//IL_0496: Unknown result type (might be due to invalid IL or missing references)
			//IL_04a3: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = new GameObject("F_" + folder.Name);
			val.transform.SetParent((Transform)(object)_fileTreeContent, false);
			RectTransform val2 = val.AddComponent<RectTransform>();
			val2.sizeDelta = new Vector2(0f, 24f);
			LayoutElement val3 = val.AddComponent<LayoutElement>();
			val3.minHeight = 24f;
			val3.preferredHeight = 24f;
			bool flag = _currentFolder == folder || _selectedFolders.Contains(folder);
			Image val4 = val.AddComponent<Image>();
			((Graphic)val4).color = (flag ? SELECTED : Color.clear);
			HorizontalLayoutGroup val5 = val.AddComponent<HorizontalLayoutGroup>();
			((HorizontalOrVerticalLayoutGroup)val5).spacing = 2f;
			((LayoutGroup)val5).padding = new RectOffset(depth * 12 + 2, 4, 2, 2);
			((LayoutGroup)val5).childAlignment = (TextAnchor)3;
			((HorizontalOrVerticalLayoutGroup)val5).childControlWidth = false;
			((HorizontalOrVerticalLayoutGroup)val5).childControlHeight = false;
			((HorizontalOrVerticalLayoutGroup)val5).childForceExpandWidth = false;
			((HorizontalOrVerticalLayoutGroup)val5).childForceExpandHeight = false;
			GameObject val6 = new GameObject("Arrow");
			val6.transform.SetParent(val.transform, false);
			RectTransform val7 = val6.AddComponent<RectTransform>();
			val7.sizeDelta = new Vector2(14f, 20f);
			TextMeshProUGUI val8 = val6.AddComponent<TextMeshProUGUI>();
			((TMP_Text)val8).text = (folder.IsExpanded ? "v" : ">");
			((TMP_Text)val8).fontSize = 12f;
			((Graphic)val8).color = (flag ? ACCENT : TEXT_DIM);
			((TMP_Text)val8).alignment = (TextAlignmentOptions)4097;
			GameObject val9 = new GameObject("Name");
			val9.transform.SetParent(val.transform, false);
			RectTransform val10 = val9.AddComponent<RectTransform>();
			val10.sizeDelta = new Vector2(110f, 20f);
			TextMeshProUGUI val11 = val9.AddComponent<TextMeshProUGUI>();
			((TMP_Text)val11).text = "[" + folder.Name + "]";
			((TMP_Text)val11).fontSize = 12f;
			((TMP_Text)val11).fontStyle = (FontStyles)1;
			((Graphic)val11).color = (folder.PersistAcrossSaves ? FOLDER_PERSISTENT : (flag ? ACCENT : TEXT_NORMAL));
			((TMP_Text)val11).alignment = (TextAlignmentOptions)4097;
			((TMP_Text)val11).overflowMode = (TextOverflowModes)1;
			GameObject val12 = new GameObject("Global");
			val12.transform.SetParent(val.transform, false);
			RectTransform val13 = val12.AddComponent<RectTransform>();
			val13.sizeDelta = new Vector2(20f, 20f);
			Image val14 = val12.AddComponent<Image>();
			((Graphic)val14).color = BTN_NORMAL;
			Button val15 = val12.AddComponent<Button>();
			((Selectable)val15).targetGraphic = (Graphic)(object)val14;
			ColorBlock colors = ((Selectable)val15).colors;
			((ColorBlock)(ref colors)).normalColor = BTN_NORMAL;
			((ColorBlock)(ref colors)).highlightedColor = BTN_HOVER;
			((ColorBlock)(ref colors)).pressedColor = BTN_PRESS;
			((ColorBlock)(ref colors)).selectedColor = BTN_NORMAL;
			((ColorBlock)(ref colors)).disabledColor = BTN_NORMAL;
			((Selectable)val15).colors = colors;
			GameObject val16 = new GameObject("Text");
			val16.transform.SetParent(val12.transform, false);
			RectTransform val17 = val16.AddComponent<RectTransform>();
			val17.anchorMin = Vector2.zero;
			val17.anchorMax = Vector2.one;
			val17.offsetMin = Vector2.zero;
			val17.offsetMax = Vector2.zero;
			TextMeshProUGUI val18 = val16.AddComponent<TextMeshProUGUI>();
			((TMP_Text)val18).text = "G";
			((TMP_Text)val18).fontSize = 11f;
			((TMP_Text)val18).fontStyle = (FontStyles)1;
			((TMP_Text)val18).characterSpacing = 2f;
			((Graphic)val18).color = (Color)(folder.PersistAcrossSaves ? new Color(0.4f, 0.7f, 1f, 1f) : TEXT_DIM);
			((TMP_Text)val18).alignment = (TextAlignmentOptions)514;
			((Graphic)val18).raycastTarget = false;
			ApplyGameFont(val18);
			PlannerFolder folderRef = folder;
			((UnityEvent)val15.onClick).AddListener((UnityAction)delegate
			{
				ToggleFolderPersistence(folderRef);
			});
			AddSimpleTooltip(val12, folder.PersistAcrossSaves ? "Global folder (persists across all saves). Click to make per-save." : "Per-save folder. Click to make global (persists across all saves).");
			Button val19 = val.AddComponent<Button>();
			((Selectable)val19).targetGraphic = (Graphic)(object)val4;
			ColorBlock colors2 = ((Selectable)val19).colors;
			((ColorBlock)(ref colors2)).normalColor = (flag ? SELECTED : Color.clear);
			((ColorBlock)(ref colors2)).highlightedColor = (Color)(flag ? SELECTED : new Color(0.2f, 0.3f, 0.4f, 0.5f));
			((ColorBlock)(ref colors2)).pressedColor = SELECTED;
			((Selectable)val19).colors = colors2;
			FolderDropHandler folderDropHandler = val.AddComponent<FolderDropHandler>();
			folderDropHandler.Folder = folder;
			folderDropHandler.Window = this;
			folderDropHandler.Background = val4;
			FolderDragHandler folderDragHandler = val.AddComponent<FolderDragHandler>();
			folderDragHandler.Folder = folder;
			folderDragHandler.Window = this;
			FolderShiftClickHandler folderShiftClickHandler = val.AddComponent<FolderShiftClickHandler>();
			folderShiftClickHandler.Folder = folder;
			folderShiftClickHandler.Window = this;
		}

		private void DeselectAll()
		{
			_currentFolder = null;
			_currentFile = null;
			_selectedFolders.Clear();
			if ((Object)(object)_currentFileLabel != (Object)null)
			{
				((TMP_Text)_currentFileLabel).text = "No file";
			}
			RefreshFileTree();
		}

		private void SelectFolder(PlannerFolder folder)
		{
			SelectFolder(folder, shiftHeld: false);
		}

		private void SelectFolder(PlannerFolder folder, bool shiftHeld)
		{
			if (shiftHeld)
			{
				if (_selectedFolders.Contains(folder))
				{
					_selectedFolders.Remove(folder);
					if (_currentFolder == folder)
					{
						_currentFolder = ((_selectedFolders.Count > 0) ? new List<PlannerFolder>(_selectedFolders)[0] : null);
					}
				}
				else
				{
					_selectedFolders.Add(folder);
					_currentFolder = folder;
				}
			}
			else
			{
				folder.IsExpanded = !folder.IsExpanded;
				_selectedFolders.Clear();
				_selectedFolders.Add(folder);
				_currentFolder = folder;
			}
			_currentFile = null;
			if ((Object)(object)_currentFileLabel != (Object)null)
			{
				if (_selectedFolders.Count > 1)
				{
					((TMP_Text)_currentFileLabel).text = $"[{_selectedFolders.Count} folders]";
				}
				else
				{
					((TMP_Text)_currentFileLabel).text = "[" + folder.Name + "]";
				}
			}
			RefreshFileTree();
		}

		private void CreateFileRow(PlannerFile file, int depth)
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Expected O, but got Unknown
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Expected O, but got Unknown
			//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0103: Expected O, but got Unknown
			//IL_012c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0168: Unknown result type (might be due to invalid IL or missing references)
			//IL_0161: Unknown result type (might be due to invalid IL or missing references)
			//IL_018e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0195: Expected O, but got Unknown
			//IL_01be: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_020a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0217: Unknown result type (might be due to invalid IL or missing references)
			//IL_0224: Unknown result type (might be due to invalid IL or missing references)
			//IL_0231: Unknown result type (might be due to invalid IL or missing references)
			//IL_0238: Expected O, but got Unknown
			//IL_0258: Unknown result type (might be due to invalid IL or missing references)
			//IL_0265: Unknown result type (might be due to invalid IL or missing references)
			//IL_0272: Unknown result type (might be due to invalid IL or missing references)
			//IL_027f: Unknown result type (might be due to invalid IL or missing references)
			//IL_02af: Unknown result type (might be due to invalid IL or missing references)
			//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e6: Expected O, but got Unknown
			//IL_02ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f3: Expected O, but got Unknown
			//IL_031c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0332: Unknown result type (might be due to invalid IL or missing references)
			//IL_0352: Unknown result type (might be due to invalid IL or missing references)
			//IL_0357: Unknown result type (might be due to invalid IL or missing references)
			//IL_035b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0368: Unknown result type (might be due to invalid IL or missing references)
			//IL_0375: Unknown result type (might be due to invalid IL or missing references)
			//IL_0382: Unknown result type (might be due to invalid IL or missing references)
			//IL_038f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0396: Expected O, but got Unknown
			//IL_03b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_03c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_03d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_03dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_040d: Unknown result type (might be due to invalid IL or missing references)
			//IL_043a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0444: Expected O, but got Unknown
			//IL_0459: Unknown result type (might be due to invalid IL or missing references)
			//IL_045e: Unknown result type (might be due to invalid IL or missing references)
			//IL_046d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0466: Unknown result type (might be due to invalid IL or missing references)
			//IL_0499: Unknown result type (might be due to invalid IL or missing references)
			//IL_0492: Unknown result type (might be due to invalid IL or missing references)
			//IL_04a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_04b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_04d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_04da: Expected O, but got Unknown
			GameObject val = new GameObject("f_" + file.Name);
			val.transform.SetParent((Transform)(object)_fileTreeContent, false);
			RectTransform val2 = val.AddComponent<RectTransform>();
			val2.sizeDelta = new Vector2(0f, 24f);
			LayoutElement val3 = val.AddComponent<LayoutElement>();
			val3.minHeight = 24f;
			val3.preferredHeight = 24f;
			bool flag = _currentFile == file;
			Image val4 = val.AddComponent<Image>();
			((Graphic)val4).color = (flag ? SELECTED : Color.clear);
			HorizontalLayoutGroup val5 = val.AddComponent<HorizontalLayoutGroup>();
			((HorizontalOrVerticalLayoutGroup)val5).spacing = 2f;
			((LayoutGroup)val5).padding = new RectOffset(depth * 12 + 14, 2, 2, 2);
			((LayoutGroup)val5).childAlignment = (TextAnchor)3;
			((HorizontalOrVerticalLayoutGroup)val5).childControlWidth = false;
			((HorizontalOrVerticalLayoutGroup)val5).childControlHeight = false;
			((HorizontalOrVerticalLayoutGroup)val5).childForceExpandWidth = false;
			((HorizontalOrVerticalLayoutGroup)val5).childForceExpandHeight = false;
			GameObject val6 = new GameObject("Name");
			val6.transform.SetParent(val.transform, false);
			RectTransform val7 = val6.AddComponent<RectTransform>();
			val7.sizeDelta = new Vector2(100f, 20f);
			TextMeshProUGUI val8 = val6.AddComponent<TextMeshProUGUI>();
			((TMP_Text)val8).text = file.Name;
			((TMP_Text)val8).fontSize = 12f;
			((Graphic)val8).color = (flag ? ACCENT : TEXT_NORMAL);
			((TMP_Text)val8).alignment = (TextAlignmentOptions)4097;
			((TMP_Text)val8).overflowMode = (TextOverflowModes)1;
			GameObject val9 = new GameObject("MoveUp");
			val9.transform.SetParent(val.transform, false);
			RectTransform val10 = val9.AddComponent<RectTransform>();
			val10.sizeDelta = new Vector2(16f, 16f);
			Image val11 = val9.AddComponent<Image>();
			((Graphic)val11).color = BTN_NORMAL;
			Button val12 = val9.AddComponent<Button>();
			((Selectable)val12).targetGraphic = (Graphic)(object)val11;
			ColorBlock colors = ((Selectable)val12).colors;
			((ColorBlock)(ref colors)).normalColor = BTN_NORMAL;
			((ColorBlock)(ref colors)).highlightedColor = BTN_HOVER;
			((ColorBlock)(ref colors)).pressedColor = BTN_PRESS;
			((Selectable)val12).colors = colors;
			GameObject val13 = new GameObject("Text");
			val13.transform.SetParent(val9.transform, false);
			RectTransform val14 = val13.AddComponent<RectTransform>();
			val14.anchorMin = Vector2.zero;
			val14.anchorMax = Vector2.one;
			val14.offsetMin = Vector2.zero;
			val14.offsetMax = Vector2.zero;
			TextMeshProUGUI val15 = val13.AddComponent<TextMeshProUGUI>();
			((TMP_Text)val15).text = "^";
			((TMP_Text)val15).fontSize = 10f;
			((Graphic)val15).color = TEXT_NORMAL;
			((TMP_Text)val15).alignment = (TextAlignmentOptions)514;
			PlannerFile f1 = file;
			((UnityEvent)val12.onClick).AddListener((UnityAction)delegate
			{
				MoveFileUp(f1);
			});
			GameObject val16 = new GameObject("MoveDown");
			val16.transform.SetParent(val.transform, false);
			RectTransform val17 = val16.AddComponent<RectTransform>();
			val17.sizeDelta = new Vector2(16f, 16f);
			Image val18 = val16.AddComponent<Image>();
			((Graphic)val18).color = BTN_NORMAL;
			Button val19 = val16.AddComponent<Button>();
			((Selectable)val19).targetGraphic = (Graphic)(object)val18;
			ColorBlock colors2 = ((Selectable)val19).colors;
			((ColorBlock)(ref colors2)).normalColor = BTN_NORMAL;
			((ColorBlock)(ref colors2)).highlightedColor = BTN_HOVER;
			((ColorBlock)(ref colors2)).pressedColor = BTN_PRESS;
			((Selectable)val19).colors = colors2;
			GameObject val20 = new GameObject("Text");
			val20.transform.SetParent(val16.transform, false);
			RectTransform val21 = val20.AddComponent<RectTransform>();
			val21.anchorMin = Vector2.zero;
			val21.anchorMax = Vector2.one;
			val21.offsetMin = Vector2.zero;
			val21.offsetMax = Vector2.zero;
			TextMeshProUGUI val22 = val20.AddComponent<TextMeshProUGUI>();
			((TMP_Text)val22).text = "v";
			((TMP_Text)val22).fontSize = 10f;
			((Graphic)val22).color = TEXT_NORMAL;
			((TMP_Text)val22).alignment = (TextAlignmentOptions)514;
			PlannerFile f2 = file;
			((UnityEvent)val19.onClick).AddListener((UnityAction)delegate
			{
				MoveFileDown(f2);
			});
			Button val23 = val.AddComponent<Button>();
			((Selectable)val23).targetGraphic = (Graphic)(object)val4;
			ColorBlock colors3 = ((Selectable)val23).colors;
			((ColorBlock)(ref colors3)).normalColor = (flag ? SELECTED : Color.clear);
			((ColorBlock)(ref colors3)).highlightedColor = (Color)(flag ? SELECTED : new Color(0.2f, 0.3f, 0.4f, 0.4f));
			((ColorBlock)(ref colors3)).pressedColor = SELECTED;
			((Selectable)val23).colors = colors3;
			PlannerFile f3 = file;
			((UnityEvent)val23.onClick).AddListener((UnityAction)delegate
			{
				SelectFile(f3);
			});
			FileDragHandler fileDragHandler = val.AddComponent<FileDragHandler>();
			fileDragHandler.File = file;
			fileDragHandler.Window = this;
		}

		private void SelectFile(PlannerFile file)
		{
			SaveCurrentFile();
			_currentFile = file;
			_currentFolder = null;
			if ((Object)(object)_blockEditor != (Object)null && _blockEditor.IsInitialized && file != null)
			{
				string text = file.Content?.TrimStart() ?? "";
				if (!string.IsNullOrEmpty(text) && text.StartsWith("{") && text.Contains("\"lines\""))
				{
					_blockEditor.DeserializeDocument(file.Content);
				}
				else
				{
					_blockEditor.LoadFromPlainText(file.Content ?? "");
				}
				_blockEditor.InputField.ActivateInputField();
			}
			if ((Object)(object)_currentFileLabel != (Object)null)
			{
				((TMP_Text)_currentFileLabel).text = file?.Name ?? "No file";
			}
			_hasUnsavedChanges = false;
			RefreshFileTree();
		}

		private void SaveCurrentFile()
		{
			if (_currentFile != null && (Object)(object)_blockEditor != (Object)null && _blockEditor.IsInitialized && _hasUnsavedChanges)
			{
				_currentFile.Content = _blockEditor.SerializeDocument();
				_currentFile.LastModified = DateTime.Now;
				_hasUnsavedChanges = false;
			}
		}

		private void OnEditorContentChanged()
		{
			_hasUnsavedChanges = true;
			SaveCurrentFile();
			SaveFileSystem();
		}

		private void OnNewFile()
		{
			Debug.Log((object)"[Station Planner] OnNewFile called");
			if (_fileSystem == null)
			{
				Debug.LogError((object)"[Station Planner] _fileSystem is null!");
				_fileSystem = new PlannerFileSystem();
			}
			PlannerFolder plannerFolder = _currentFolder ?? _fileSystem.RootFolder;
			PlannerFile plannerFile = new PlannerFile
			{
				Id = Guid.NewGuid().ToString(),
				Name = "Note " + (CountAllFiles(_fileSystem.RootFolder) + 1),
				Content = ""
			};
			Debug.Log((object)("[Station Planner] Created file: " + plannerFile.Name + " in folder: " + plannerFolder.Name));
			plannerFolder.Files.Add(plannerFile);
			plannerFolder.IsExpanded = true;
			SaveFileSystem();
			RefreshFileTree();
			SelectFile(plannerFile);
		}

		private int CountAllFiles(PlannerFolder folder)
		{
			int num = folder.Files.Count;
			foreach (PlannerFolder subFolder in folder.SubFolders)
			{
				num += CountAllFiles(subFolder);
			}
			return num;
		}

		private void OnNewFolder()
		{
			Debug.Log((object)"[Station Notepad] OnNewFolder called");
			if (_fileSystem == null)
			{
				Debug.LogError((object)"[Station Notepad] _fileSystem is null!");
				_fileSystem = new PlannerFileSystem();
			}
			PlannerFolder plannerFolder = _currentFolder ?? _fileSystem.RootFolder;
			string currentSaveName = GetCurrentSaveName();
			bool flag = string.IsNullOrEmpty(currentSaveName);
			PlannerFolder plannerFolder2 = new PlannerFolder
			{
				Id = Guid.NewGuid().ToString(),
				Name = "Folder " + (CountAllFolders(_fileSystem.RootFolder) + 1),
				IsExpanded = true,
				PersistAcrossSaves = flag,
				SaveName = (flag ? "" : currentSaveName)
			};
			Debug.Log((object)$"[Station Notepad] Created folder: {plannerFolder2.Name} in: {plannerFolder.Name}, Global: {plannerFolder2.PersistAcrossSaves}, SaveName: {plannerFolder2.SaveName}");
			plannerFolder.SubFolders.Add(plannerFolder2);
			plannerFolder.IsExpanded = true;
			SaveFileSystem();
			RefreshFileTree();
			SelectFolder(plannerFolder2);
		}

		private int CountAllFolders(PlannerFolder folder)
		{
			int num = folder.SubFolders.Count;
			foreach (PlannerFolder subFolder in folder.SubFolders)
			{
				num += CountAllFolders(subFolder);
			}
			return num;
		}

		private void OnRename()
		{
			if (_currentFolder != null)
			{
				ShowInputDialog("Rename Folder", _currentFolder.Name, delegate(string newName)
				{
					if (!string.IsNullOrWhiteSpace(newName))
					{
						_currentFolder.Name = newName.Trim();
						SaveFileSystem();
						RefreshFileTree();
						if ((Object)(object)_currentFileLabel != (Object)null)
						{
							((TMP_Text)_currentFileLabel).text = "[" + _currentFolder.Name + "]";
						}
					}
				});
			}
			else
			{
				if (_currentFile == null)
				{
					return;
				}
				ShowInputDialog("Rename", _currentFile.Name, delegate(string newName)
				{
					if (!string.IsNullOrWhiteSpace(newName))
					{
						_currentFile.Name = newName.Trim();
						SaveFileSystem();
						RefreshFileTree();
						if ((Object)(object)_currentFileLabel != (Object)null)
						{
							((TMP_Text)_currentFileLabel).text = _currentFile.Name;
						}
					}
				});
			}
		}

		private void OnDelete()
		{
			if (_selectedFolders.Count > 1)
			{
				foreach (PlannerFolder item in new List<PlannerFolder>(_selectedFolders))
				{
					FindParentFolder(_fileSystem.RootFolder, item)?.SubFolders.Remove(item);
				}
				_selectedFolders.Clear();
				_currentFolder = null;
				if ((Object)(object)_currentFileLabel != (Object)null)
				{
					((TMP_Text)_currentFileLabel).text = "No selection";
				}
				SaveFileSystem();
				RefreshFileTree();
			}
			else if (_currentFolder != null)
			{
				PlannerFolder plannerFolder = FindParentFolder(_fileSystem.RootFolder, _currentFolder);
				if (plannerFolder != null)
				{
					plannerFolder.SubFolders.Remove(_currentFolder);
					_selectedFolders.Remove(_currentFolder);
					_currentFolder = null;
					if ((Object)(object)_currentFileLabel != (Object)null)
					{
						((TMP_Text)_currentFileLabel).text = "No selection";
					}
					SaveFileSystem();
					RefreshFileTree();
				}
			}
			else
			{
				if (_currentFile == null)
				{
					return;
				}
				RemoveFileFromAllFolders(_fileSystem.RootFolder, _currentFile);
				PlannerFile plannerFile = FindFirstFile(_fileSystem.RootFolder);
				if (plannerFile != null)
				{
					SelectFile(plannerFile);
				}
				else
				{
					_currentFile = null;
					if ((Object)(object)_blockEditor != (Object)null && _blockEditor.IsInitialized)
					{
						_blockEditor.LoadFromPlainText("");
					}
					if ((Object)(object)_currentFileLabel != (Object)null)
					{
						((TMP_Text)_currentFileLabel).text = "No file";
					}
				}
				SaveFileSystem();
				RefreshFileTree();
			}
		}

		private PlannerFolder FindParentFolder(PlannerFolder searchIn, PlannerFolder target)
		{
			if (searchIn.SubFolders.Contains(target))
			{
				return searchIn;
			}
			foreach (PlannerFolder subFolder in searchIn.SubFolders)
			{
				PlannerFolder plannerFolder = FindParentFolder(subFolder, target);
				if (plannerFolder != null)
				{
					return plannerFolder;
				}
			}
			return null;
		}

		private PlannerFolder FindParentFolderOfFile(PlannerFolder searchIn, PlannerFile target)
		{
			if (searchIn.Files.Contains(target))
			{
				return searchIn;
			}
			foreach (PlannerFolder subFolder in searchIn.SubFolders)
			{
				PlannerFolder plannerFolder = FindParentFolderOfFile(subFolder, target);
				if (plannerFolder != null)
				{
					return plannerFolder;
				}
			}
			return null;
		}

		private void RemoveFileFromAllFolders(PlannerFolder folder, PlannerFile file)
		{
			folder.Files.Remove(file);
			foreach (PlannerFolder subFolder in folder.SubFolders)
			{
				RemoveFileFromAllFolders(subFolder, file);
			}
		}

		private PlannerFile FindFirstFile(PlannerFolder folder)
		{
			if (folder.Files.Count > 0)
			{
				return folder.Files[0];
			}
			foreach (PlannerFolder subFolder in folder.SubFolders)
			{
				PlannerFile plannerFile = FindFirstFile(subFolder);
				if (plannerFile != null)
				{
					return plannerFile;
				}
			}
			return null;
		}

		private void MoveFileUp(PlannerFile file)
		{
			PlannerFolder plannerFolder = FindParentFolderOfFile(_fileSystem.RootFolder, file);
			if (plannerFolder != null)
			{
				int num = plannerFolder.Files.IndexOf(file);
				if (num > 0)
				{
					plannerFolder.Files.RemoveAt(num);
					plannerFolder.Files.Insert(num - 1, file);
					SaveFileSystem();
					RefreshFileTree();
				}
			}
		}

		private void MoveFileDown(PlannerFile file)
		{
			PlannerFolder plannerFolder = FindParentFolderOfFile(_fileSystem.RootFolder, file);
			if (plannerFolder != null)
			{
				int num = plannerFolder.Files.IndexOf(file);
				if (num < plannerFolder.Files.Count - 1)
				{
					plannerFolder.Files.RemoveAt(num);
					plannerFolder.Files.Insert(num + 1, file);
					SaveFileSystem();
					RefreshFileTree();
				}
			}
		}

		public void MoveFileToFolder(PlannerFile file, PlannerFolder targetFolder)
		{
			if (file != null && targetFolder != null)
			{
				PlannerFolder plannerFolder = FindParentFolderOfFile(_fileSystem.RootFolder, file);
				if (plannerFolder != null && plannerFolder != targetFolder)
				{
					plannerFolder.Files.Remove(file);
					targetFolder.Files.Add(file);
					targetFolder.IsExpanded = true;
					Debug.Log((object)("[Station Notepad] Moved '" + file.Name + "' from '" + plannerFolder.Name + "' to '" + targetFolder.Name + "'"));
					SaveFileSystem();
					RefreshFileTree();
				}
			}
		}

		public void MoveFolderToFolder(PlannerFolder folder, PlannerFolder targetFolder)
		{
			if (folder != null && targetFolder != null && !IsDescendantOf(targetFolder, folder))
			{
				PlannerFolder plannerFolder = FindParentFolder(_fileSystem.RootFolder, folder);
				if (plannerFolder != null && plannerFolder != targetFolder)
				{
					plannerFolder.SubFolders.Remove(folder);
					targetFolder.SubFolders.Add(folder);
					targetFolder.IsExpanded = true;
					Debug.Log((object)("[Station Notepad] Moved folder '" + folder.Name + "' from '" + plannerFolder.Name + "' to '" + targetFolder.Name + "'"));
					SaveFileSystem();
					RefreshFileTree();
				}
			}
		}

		public void ReorderFolder(PlannerFolder folder, PlannerFolder referenceFolder, bool insertBefore)
		{
			if (folder == null || referenceFolder == null || folder == referenceFolder)
			{
				return;
			}
			PlannerFolder plannerFolder = FindParentFolder(_fileSystem.RootFolder, referenceFolder);
			if (plannerFolder == null)
			{
				return;
			}
			PlannerFolder plannerFolder2 = FindParentFolder(_fileSystem.RootFolder, folder);
			if (plannerFolder2 != null && !IsDescendantOf(plannerFolder, folder))
			{
				plannerFolder2.SubFolders.Remove(folder);
				int num = plannerFolder.SubFolders.IndexOf(referenceFolder);
				if (num < 0)
				{
					plannerFolder.SubFolders.Add(folder);
				}
				else
				{
					int index = (insertBefore ? num : (num + 1));
					plannerFolder.SubFolders.Insert(index, folder);
				}
				Debug.Log((object)("[Station Notepad] Reordered folder '" + folder.Name + "' " + (insertBefore ? "before" : "after") + " '" + referenceFolder.Name + "'"));
				SaveFileSystem();
				RefreshFileTree();
			}
		}

		private bool IsDescendantOf(PlannerFolder potentialDescendant, PlannerFolder potentialAncestor)
		{
			if (potentialDescendant == potentialAncestor)
			{
				return true;
			}
			foreach (PlannerFolder subFolder in potentialAncestor.SubFolders)
			{
				if (IsDescendantOf(potentialDescendant, subFolder))
				{
					return true;
				}
			}
			return false;
		}

		private void ShowInputDialog(string title, string defaultValue, Action<string> onConfirm)
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Expected O, but got Unknown
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fa: Expected O, but got Unknown
			//IL_011e: Unknown result type (might be due to invalid IL or missing references)
			//IL_012b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0138: Unknown result type (might be due to invalid IL or missing references)
			//IL_0145: Unknown result type (might be due to invalid IL or missing references)
			//IL_0196: Unknown result type (might be due to invalid IL or missing references)
			//IL_01af: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b6: Expected O, but got Unknown
			//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0238: Unknown result type (might be due to invalid IL or missing references)
			//IL_024f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0266: Unknown result type (might be due to invalid IL or missing references)
			//IL_027d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0294: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ab: Expected O, but got Unknown
			//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0307: Unknown result type (might be due to invalid IL or missing references)
			//IL_031e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0382: Unknown result type (might be due to invalid IL or missing references)
			//IL_0392: Unknown result type (might be due to invalid IL or missing references)
			//IL_0399: Expected O, but got Unknown
			//IL_03b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_03c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_03d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_03e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0444: Unknown result type (might be due to invalid IL or missing references)
			//IL_0603: Unknown result type (might be due to invalid IL or missing references)
			//IL_060a: Expected O, but got Unknown
			//IL_062a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0637: Unknown result type (might be due to invalid IL or missing references)
			//IL_064e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0665: Unknown result type (might be due to invalid IL or missing references)
			//IL_067d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0684: Expected O, but got Unknown
			//IL_06b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_06dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_06ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_06f7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0704: Unknown result type (might be due to invalid IL or missing references)
			//IL_074a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0776: Unknown result type (might be due to invalid IL or missing references)
			//IL_0483: Unknown result type (might be due to invalid IL or missing references)
			//IL_0490: Unknown result type (might be due to invalid IL or missing references)
			//IL_049d: Unknown result type (might be due to invalid IL or missing references)
			//IL_04aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_04b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0787: Unknown result type (might be due to invalid IL or missing references)
			//IL_078e: Expected O, but got Unknown
			//IL_07bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_07d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_07ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_0801: Unknown result type (might be due to invalid IL or missing references)
			//IL_0818: Unknown result type (might be due to invalid IL or missing references)
			//IL_0883: Unknown result type (might be due to invalid IL or missing references)
			//IL_088d: Expected O, but got Unknown
			//IL_08b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_08bf: Expected O, but got Unknown
			//IL_04e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_04ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_0505: Unknown result type (might be due to invalid IL or missing references)
			//IL_051c: Unknown result type (might be due to invalid IL or missing references)
			//IL_05c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_05ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_054c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0559: Unknown result type (might be due to invalid IL or missing references)
			//IL_0566: Unknown result type (might be due to invalid IL or missing references)
			//IL_0573: Unknown result type (might be due to invalid IL or missing references)
			//IL_0597: Unknown result type (might be due to invalid IL or missing references)
			GameObject dialog = new GameObject("InputDialog");
			dialog.transform.SetParent(_windowPanel.transform, false);
			RectTransform val = dialog.AddComponent<RectTransform>();
			val.anchorMin = new Vector2(0.5f, 0.5f);
			val.anchorMax = new Vector2(0.5f, 0.5f);
			val.sizeDelta = new Vector2(300f, 130f);
			val.anchoredPosition = Vector2.zero;
			Image val2 = dialog.AddComponent<Image>();
			Sprite val3 = LoadSlicedSprite("dialog-bg.png", 18);
			if ((Object)(object)val3 != (Object)null)
			{
				val2.sprite = val3;
				val2.type = (Type)1;
			}
			((Graphic)val2).color = new Color(0f, 0.165f, 0.278f, 0.98f);
			GameObject val4 = new GameObject("Border");
			val4.transform.SetParent(dialog.transform, false);
			RectTransform val5 = val4.AddComponent<RectTransform>();
			val5.anchorMin = Vector2.zero;
			val5.anchorMax = Vector2.one;
			val5.offsetMin = Vector2.zero;
			val5.offsetMax = Vector2.zero;
			Image val6 = val4.AddComponent<Image>();
			Sprite val7 = LoadSlicedSprite("dialog-outline.png", 18);
			if ((Object)(object)val7 != (Object)null)
			{
				val6.sprite = val7;
				val6.type = (Type)1;
				val6.fillCenter = false;
			}
			((Graphic)val6).color = ACCENT_ORANGE;
			((Graphic)val6).raycastTarget = false;
			GameObject val8 = new GameObject("Title");
			val8.transform.SetParent(dialog.transform, false);
			TextMeshProUGUI val9 = val8.AddComponent<TextMeshProUGUI>();
			((TMP_Text)val9).text = title.ToUpper();
			((TMP_Text)val9).fontSize = 16f;
			((TMP_Text)val9).fontStyle = (FontStyles)1;
			((Graphic)val9).color = TEXT_NORMAL;
			((TMP_Text)val9).characterSpacing = 2f;
			((TMP_Text)val9).alignment = (TextAlignmentOptions)514;
			RectTransform component = val8.GetComponent<RectTransform>();
			component.anchorMin = new Vector2(0f, 1f);
			component.anchorMax = new Vector2(1f, 1f);
			component.pivot = new Vector2(0.5f, 1f);
			component.anchoredPosition = new Vector2(0f, -12f);
			component.sizeDelta = new Vector2(0f, 24f);
			GameObject val10 = new GameObject("InputContainer");
			val10.transform.SetParent(dialog.transform, false);
			RectTransform val11 = val10.AddComponent<RectTransform>();
			val11.anchorMin = new Vector2(0.5f, 0.5f);
			val11.anchorMax = new Vector2(0.5f, 0.5f);
			val11.sizeDelta = new Vector2(260f, 32f);
			val11.anchoredPosition = new Vector2(0f, 8f);
			Image val12 = val10.AddComponent<Image>();
			Sprite val13 = LoadSlicedSprite("button-bg.png", 7);
			if ((Object)(object)val13 != (Object)null)
			{
				val12.sprite = val13;
				val12.type = (Type)1;
				val12.fillCenter = true;
			}
			((Graphic)val12).color = new Color(0f, 0f, 0f, 0.75f);
			GameObject val14 = new GameObject("Outline");
			val14.transform.SetParent(val10.transform, false);
			RectTransform val15 = val14.AddComponent<RectTransform>();
			val15.anchorMin = Vector2.zero;
			val15.anchorMax = Vector2.one;
			val15.offsetMin = Vector2.zero;
			val15.offsetMax = Vector2.zero;
			Image val16 = val14.AddComponent<Image>();
			Sprite val17 = LoadSlicedSprite("slot-outline.png", 8);
			if ((Object)(object)val17 != (Object)null)
			{
				val16.sprite = val17;
				val16.type = (Type)1;
				val16.fillCenter = false;
			}
			((Graphic)val16).color = new Color(0f, 0f, 0f, 0.5f);
			((Graphic)val16).raycastTarget = false;
			TMP_InputField val18 = BlockEditor.CreateDialogInputField(val10.transform);
			if ((Object)(object)val18 != (Object)null)
			{
				RectTransform component2 = ((Component)val18).GetComponent<RectTransform>();
				component2.anchorMin = Vector2.zero;
				component2.anchorMax = Vector2.one;
				component2.offsetMin = Vector2.zero;
				component2.offsetMax = Vector2.zero;
				component2.sizeDelta = Vector2.zero;
				if ((Object)(object)val18.textViewport != (Object)null)
				{
					RectTransform textViewport = val18.textViewport;
					textViewport.anchorMin = Vector2.zero;
					textViewport.anchorMax = Vector2.one;
					textViewport.offsetMin = new Vector2(8f, 4f);
					textViewport.offsetMax = new Vector2(-8f, -4f);
				}
				if ((Object)(object)val18.textComponent != (Object)null)
				{
					RectTransform component3 = ((Component)val18.textComponent).GetComponent<RectTransform>();
					component3.anchorMin = Vector2.zero;
					component3.anchorMax = Vector2.one;
					component3.offsetMin = Vector2.zero;
					component3.offsetMax = Vector2.zero;
					val18.textComponent.fontSize = 14f;
					((Graphic)val18.textComponent).color = TEXT_NORMAL;
					val18.textComponent.alignment = (TextAlignmentOptions)4097;
				}
				val18.text = defaultValue;
				val18.caretColor = ACCENT;
				val18.customCaretColor = true;
				val18.caretWidth = 2;
				val18.caretBlinkRate = 0.85f;
				val18.selectionColor = SELECTED;
			}
			else
			{
				GameObject val19 = new GameObject("TextArea");
				val19.transform.SetParent(val10.transform, false);
				RectTransform val20 = val19.AddComponent<RectTransform>();
				val20.anchorMin = Vector2.zero;
				val20.anchorMax = Vector2.one;
				val20.offsetMin = new Vector2(8f, 4f);
				val20.offsetMax = new Vector2(-8f, -4f);
				val19.AddComponent<RectMask2D>();
				GameObject val21 = new GameObject("Text");
				val21.transform.SetParent(val19.transform, false);
				TextMeshProUGUI val22 = val21.AddComponent<TextMeshProUGUI>();
				((TMP_Text)val22).fontSize = 14f;
				((Graphic)val22).color = TEXT_NORMAL;
				((TMP_Text)val22).alignment = (TextAlignmentOptions)4097;
				((Graphic)val22).raycastTarget = false;
				RectTransform component4 = val21.GetComponent<RectTransform>();
				component4.anchorMin = Vector2.zero;
				component4.anchorMax = Vector2.one;
				component4.offsetMin = Vector2.zero;
				component4.offsetMax = Vector2.zero;
				val18 = val10.AddComponent<TMP_InputField>();
				val18.textComponent = (TMP_Text)(object)val22;
				val18.textViewport = val20;
				val18.text = defaultValue;
				((Selectable)val18).targetGraphic = (Graphic)(object)val12;
				((Selectable)val18).interactable = true;
				val18.caretColor = ACCENT;
				val18.customCaretColor = true;
				val18.caretWidth = 2;
				val18.caretBlinkRate = 0.85f;
				val18.selectionColor = SELECTED;
			}
			GameObject val23 = new GameObject("Buttons");
			val23.transform.SetParent(dialog.transform, false);
			RectTransform val24 = val23.AddComponent<RectTransform>();
			val24.anchorMin = new Vector2(0.5f, 0f);
			val24.anchorMax = new Vector2(0.5f, 0f);
			val24.pivot = new Vector2(0.5f, 0f);
			val24.anchoredPosition = new Vector2(0f, 10f);
			val24.sizeDelta = new Vector2(180f, 26f);
			HorizontalLayoutGroup val25 = val23.AddComponent<HorizontalLayoutGroup>();
			((HorizontalOrVerticalLayoutGroup)val25).spacing = 10f;
			((LayoutGroup)val25).childAlignment = (TextAnchor)4;
			((HorizontalOrVerticalLayoutGroup)val25).childControlWidth = false;
			((HorizontalOrVerticalLayoutGroup)val25).childControlHeight = true;
			TMP_InputField dialogInputField = val18;
			Button val26 = CreateToolbarBtn(val23.transform, "OK", 70f);
			((UnityEvent)val26.onClick).AddListener((UnityAction)delegate
			{
				onConfirm?.Invoke(dialogInputField.text);
				Object.Destroy((Object)(object)dialog);
			});
			Button val27 = CreateToolbarBtn(val23.transform, "Cancel", 70f);
			((UnityEvent)val27.onClick).AddListener((UnityAction)delegate
			{
				Object.Destroy((Object)(object)dialog);
			});
			((MonoBehaviour)this).StartCoroutine(ActivateInputFieldDelayed(val18));
		}

		private IEnumerator ActivateInputFieldDelayed(TMP_InputField inputField)
		{
			yield return null;
			if ((Object)(object)inputField != (Object)null)
			{
				((Selectable)inputField).Select();
				inputField.ActivateInputField();
			}
		}

		private void ApplyLineStyle(LineStyle style)
		{
			if (!((Object)(object)_blockEditor == (Object)null) && _blockEditor.IsInitialized)
			{
				_blockEditor.ToggleStyleOnCurrentLine(style);
				_hasUnsavedChanges = true;
				_blockEditor.InputField.ActivateInputField();
			}
		}

		private void TogglePause()
		{
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			if (!NetworkManager.IsClient && NetworkBase.Clients.Count == 0 && !InventoryManager.Instance.InGameMenuOpen)
			{
				_isPaused = !_isPaused;
				WorldManager.SetGamePause(_isPaused);
				if ((Object)(object)_pauseButtonText != (Object)null)
				{
					((Graphic)_pauseButtonText).color = (_isPaused ? ACCENT : TEXT_NORMAL);
				}
				if ((Object)(object)_pauseButtonBg != (Object)null)
				{
					((Graphic)_pauseButtonBg).color = (Color)(_isPaused ? new Color(0.4f, 0.4f, 0.4f, 0.7f) : BTN_NORMAL);
				}
				ManualLogSource log = StationpediaAscendedMod.Log;
				if (log != null)
				{
					log.LogInfo((object)$"[StationPlanner] Game pause toggled: {_isPaused}");
				}
			}
		}

		private void ToggleFolderPersistence(PlannerFolder folder)
		{
			if (folder == null)
			{
				return;
			}
			folder.PersistAcrossSaves = !folder.PersistAcrossSaves;
			if (folder.PersistAcrossSaves)
			{
				folder.SaveName = "";
			}
			else
			{
				string currentSaveName = GetCurrentSaveName();
				if (!string.IsNullOrEmpty(currentSaveName))
				{
					folder.SaveName = currentSaveName;
				}
				else
				{
					folder.PersistAcrossSaves = true;
					Debug.Log((object)"[Station Notepad] Can't make folder per-save in main menu - keeping global");
				}
			}
			Debug.Log((object)("[Station Notepad] Folder '" + folder.Name + "' persistence: " + (folder.PersistAcrossSaves ? "Global" : ("Per-save (" + folder.SaveName + ")"))));
			SaveFileSystem();
			RefreshFileTree();
		}

		private void UpdateFolderRowAppearance(PlannerFolder folder)
		{
			RefreshFileTree();
		}

		private void OnToggleFolderGlobal()
		{
			if (_currentFolder == null)
			{
				Debug.Log((object)"[Station Planner] No folder selected - cannot toggle global/per-save");
			}
			else
			{
				ToggleFolderPersistence(_currentFolder);
			}
		}

		private void ShowHelpDialog()
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Expected O, but got Unknown
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f3: Expected O, but got Unknown
			//IL_0117: Unknown result type (might be due to invalid IL or missing references)
			//IL_0124: Unknown result type (might be due to invalid IL or missing references)
			//IL_0131: Unknown result type (might be due to invalid IL or missing references)
			//IL_013e: Unknown result type (might be due to invalid IL or missing references)
			//IL_018f: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01af: Expected O, but got Unknown
			//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0230: Unknown result type (might be due to invalid IL or missing references)
			//IL_0247: Unknown result type (might be due to invalid IL or missing references)
			//IL_025e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0275: Unknown result type (might be due to invalid IL or missing references)
			//IL_028c: Unknown result type (might be due to invalid IL or missing references)
			//IL_029c: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a3: Expected O, but got Unknown
			//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0316: Unknown result type (might be due to invalid IL or missing references)
			//IL_0371: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b9: Expected O, but got Unknown
			//IL_03d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_03e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_03fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0414: Unknown result type (might be due to invalid IL or missing references)
			//IL_0436: Unknown result type (might be due to invalid IL or missing references)
			//IL_043d: Expected O, but got Unknown
			//IL_0477: Unknown result type (might be due to invalid IL or missing references)
			//IL_04b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_04c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_04df: Unknown result type (might be due to invalid IL or missing references)
			//IL_04f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_050d: Unknown result type (might be due to invalid IL or missing references)
			//IL_053d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0544: Expected O, but got Unknown
			//IL_0564: Unknown result type (might be due to invalid IL or missing references)
			//IL_0571: Unknown result type (might be due to invalid IL or missing references)
			//IL_057e: Unknown result type (might be due to invalid IL or missing references)
			//IL_058b: Unknown result type (might be due to invalid IL or missing references)
			//IL_05ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_0635: Unknown result type (might be due to invalid IL or missing references)
			//IL_064c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0663: Unknown result type (might be due to invalid IL or missing references)
			//IL_067a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0693: Unknown result type (might be due to invalid IL or missing references)
			//IL_069d: Expected O, but got Unknown
			GameObject dialog = new GameObject("HelpDialog");
			dialog.transform.SetParent(_windowPanel.transform, false);
			RectTransform val = dialog.AddComponent<RectTransform>();
			val.anchorMin = new Vector2(0.5f, 0.5f);
			val.anchorMax = new Vector2(0.5f, 0.5f);
			val.sizeDelta = new Vector2(400f, 340f);
			val.anchoredPosition = Vector2.zero;
			Image val2 = dialog.AddComponent<Image>();
			Sprite val3 = LoadSlicedSprite("dialog-bg.png", 18);
			if ((Object)(object)val3 != (Object)null)
			{
				val2.sprite = val3;
				val2.type = (Type)1;
			}
			((Graphic)val2).color = new Color(0f, 0.165f, 0.278f, 0.98f);
			GameObject val4 = new GameObject("Border");
			val4.transform.SetParent(dialog.transform, false);
			RectTransform val5 = val4.AddComponent<RectTransform>();
			val5.anchorMin = Vector2.zero;
			val5.anchorMax = Vector2.one;
			val5.offsetMin = Vector2.zero;
			val5.offsetMax = Vector2.zero;
			Image val6 = val4.AddComponent<Image>();
			Sprite val7 = LoadSlicedSprite("dialog-outline.png", 18);
			if ((Object)(object)val7 != (Object)null)
			{
				val6.sprite = val7;
				val6.type = (Type)1;
				val6.fillCenter = false;
			}
			((Graphic)val6).color = ACCENT_ORANGE;
			((Graphic)val6).raycastTarget = false;
			GameObject val8 = new GameObject("Title");
			val8.transform.SetParent(dialog.transform, false);
			TextMeshProUGUI val9 = val8.AddComponent<TextMeshProUGUI>();
			((TMP_Text)val9).text = "STATION PLANNER GUIDE";
			((TMP_Text)val9).fontSize = 16f;
			((TMP_Text)val9).fontStyle = (FontStyles)1;
			((Graphic)val9).color = TEXT_NORMAL;
			((TMP_Text)val9).characterSpacing = 2f;
			((TMP_Text)val9).alignment = (TextAlignmentOptions)514;
			RectTransform component = val8.GetComponent<RectTransform>();
			component.anchorMin = new Vector2(0f, 1f);
			component.anchorMax = new Vector2(1f, 1f);
			component.pivot = new Vector2(0.5f, 1f);
			component.anchoredPosition = new Vector2(0f, -12f);
			component.sizeDelta = new Vector2(0f, 24f);
			GameObject val10 = new GameObject("Content");
			val10.transform.SetParent(dialog.transform, false);
			RectTransform val11 = val10.AddComponent<RectTransform>();
			val11.anchorMin = new Vector2(0.5f, 0.5f);
			val11.anchorMax = new Vector2(0.5f, 0.5f);
			val11.sizeDelta = new Vector2(360f, 240f);
			val11.anchoredPosition = new Vector2(0f, 8f);
			Image val12 = val10.AddComponent<Image>();
			Sprite val13 = LoadSlicedSprite("button-bg.png", 7);
			if ((Object)(object)val13 != (Object)null)
			{
				val12.sprite = val13;
				val12.type = (Type)1;
			}
			((Graphic)val12).color = new Color(0f, 0f, 0f, 0.75f);
			ScrollRect val14 = val10.AddComponent<ScrollRect>();
			val14.horizontal = false;
			val14.vertical = true;
			val14.movementType = (MovementType)2;
			val14.scrollSensitivity = 20f;
			GameObject val15 = new GameObject("Viewport");
			val15.transform.SetParent(val10.transform, false);
			RectTransform val16 = val15.AddComponent<RectTransform>();
			val16.anchorMin = Vector2.zero;
			val16.anchorMax = Vector2.one;
			val16.offsetMin = new Vector2(8f, 8f);
			val16.offsetMax = new Vector2(-8f, -8f);
			val15.AddComponent<RectMask2D>();
			val14.viewport = val16;
			GameObject val17 = new GameObject("Text");
			val17.transform.SetParent(val15.transform, false);
			TextMeshProUGUI val18 = val17.AddComponent<TextMeshProUGUI>();
			((TMP_Text)val18).text = "QUICK START\n\nClick 'New' to create a note\nClick 'Folder' to organize\nUse toolbar to format text\n\nFORMATTING\n\nH1, H2, H3 - Headings\n• - Bullet point\n/ - Strikethrough (grey)\n¶ - Normal paragraph\n\nSHORTCUTS\n\nF4 - Toggle window\nEscape - Close\n|| - Pause/Resume game\n\nFEATURES\n\nAuto-saves as you type\nDrag notes between folders\nMove notes up/down\n\nFOLDER PERSISTENCE\n\nClick G button on folder to\ntoggle folder persistence.\nBlue G = Global (across saves)\nEmpty = Per-save folder";
			((TMP_Text)val18).fontSize = 12f;
			((Graphic)val18).color = TEXT_NORMAL;
			((TMP_Text)val18).alignment = (TextAlignmentOptions)257;
			((TMP_Text)val18).wordWrappingRatios = 0.8f;
			RectTransform component2 = val17.GetComponent<RectTransform>();
			component2.anchorMin = new Vector2(0f, 1f);
			component2.anchorMax = new Vector2(1f, 1f);
			component2.pivot = new Vector2(0f, 1f);
			component2.anchoredPosition = new Vector2(0f, 0f);
			component2.sizeDelta = new Vector2(0f, 800f);
			LayoutElement val19 = val17.AddComponent<LayoutElement>();
			val19.minHeight = 100f;
			val14.content = component2;
			GameObject val20 = new GameObject("Outline");
			val20.transform.SetParent(val10.transform, false);
			RectTransform val21 = val20.AddComponent<RectTransform>();
			val21.anchorMin = Vector2.zero;
			val21.anchorMax = Vector2.one;
			val21.offsetMin = Vector2.zero;
			val21.offsetMax = Vector2.zero;
			Image val22 = val20.AddComponent<Image>();
			Sprite val23 = LoadSlicedSprite("slot-outline.png", 8);
			if ((Object)(object)val23 != (Object)null)
			{
				val22.sprite = val23;
				val22.type = (Type)1;
				val22.fillCenter = false;
			}
			((Graphic)val22).color = new Color(0f, 0f, 0f, 0.5f);
			((Graphic)val22).raycastTarget = false;
			Button val24 = CreateToolbarBtn(dialog.transform, "Close", 70f);
			RectTransform component3 = ((Component)val24).GetComponent<RectTransform>();
			component3.anchorMin = new Vector2(0.5f, 0f);
			component3.anchorMax = new Vector2(0.5f, 0f);
			component3.pivot = new Vector2(0.5f, 0f);
			component3.anchoredPosition = new Vector2(0f, 10f);
			((UnityEvent)val24.onClick).AddListener((UnityAction)delegate
			{
				Object.Destroy((Object)(object)dialog);
			});
		}
	}
	public class WindowDragHandler : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IDragHandler
	{
		public RectTransform WindowRect;

		private Vector2 _offset;

		public void OnBeginDrag(PointerEventData e)
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			if (!((Object)(object)WindowRect == (Object)null))
			{
				Transform parent = ((Transform)WindowRect).parent;
				RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)(object)((parent is RectTransform) ? parent : null), e.position, e.pressEventCamera, ref _offset);
				_offset = WindowRect.anchoredPosition - _offset;
			}
		}

		public void OnDrag(PointerEventData e)
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			Vector2 val = default(Vector2);
			if (!((Object)(object)WindowRect == (Object)null) && RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)/*isinst with value type is only supported in some contexts*/, e.position, e.pressEventCamera, ref val))
			{
				WindowRect.anchoredPosition = val + _offset;
			}
		}
	}
	public class WindowResizeHandler : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IDragHandler
	{
		public RectTransform WindowRect;

		public Vector2 MinSize = new Vector2(400f, 300f);

		private Vector2 _startSize;

		private Vector2 _startMouse;

		public void OnBeginDrag(PointerEventData e)
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			if (!((Object)(object)WindowRect == (Object)null))
			{
				_startSize = WindowRect.sizeDelta;
				_startMouse = e.position;
			}
		}

		public void OnDrag(PointerEventData e)
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			if (!((Object)(object)WindowRect == (Object)null))
			{
				Vector2 val = e.position - _startMouse;
				Vector2 sizeDelta = default(Vector2);
				((Vector2)(ref sizeDelta))..ctor(Mathf.Max(MinSize.x, _startSize.x + val.x), Mathf.Max(MinSize.y, _startSize.y - val.y));
				WindowRect.sizeDelta = sizeDelta;
			}
		}
	}
	public class EditorClickHandler : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerDownHandler
	{
		public TMP_InputField InputField;

		public void OnPointerClick(PointerEventData eventData)
		{
			ActivateField();
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			ActivateField();
		}

		private void ActivateField()
		{
			if (!((Object)(object)InputField == (Object)null))
			{
				InputField.ActivateInputField();
				Debug.Log((object)$"[Station Planner] ActivateInputField called - isFocused: {InputField.isFocused}");
			}
		}
	}
	public class InputFieldDebugger : MonoBehaviour
	{
		public TMP_InputField InputField;

		private float _logTimer = 0f;

		private bool _lastFocusState = false;

		private void Update()
		{
			if ((Object)(object)InputField == (Object)null)
			{
				return;
			}
			if (InputField.isFocused != _lastFocusState)
			{
				_lastFocusState = InputField.isFocused;
				LogCaretState($"Focus changed to {_lastFocusState}");
			}
			if (InputField.isFocused)
			{
				_logTimer += Time.deltaTime;
				if (_logTimer >= 2f)
				{
					_logTimer = 0f;
					LogCaretState("Periodic check");
				}
			}
		}

		private void LogCaretState(string trigger)
		{
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0230: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)InputField == (Object)null)
			{
				return;
			}
			RectTransform textViewport = InputField.textViewport;
			TMP_Text textComponent = InputField.textComponent;
			Debug.Log((object)("[Station Planner DEBUG] " + trigger));
			Debug.Log((object)$"  isFocused: {InputField.isFocused}");
			Debug.Log((object)$"  interactable: {((Selectable)InputField).interactable}");
			Debug.Log((object)$"  caretPosition: {InputField.caretPosition}");
			Debug.Log((object)$"  caretColor: {InputField.caretColor}");
			Debug.Log((object)$"  caretWidth: {InputField.caretWidth}");
			Debug.Log((object)$"  customCaretColor: {InputField.customCaretColor}");
			Debug.Log((object)$"  selectionColor: {InputField.selectionColor}");
			Debug.Log((object)("  textViewport: " + (((Object)(object)textViewport != (Object)null) ? ((Object)textViewport).name : "NULL")));
			Debug.Log((object)("  textComponent: " + (((Object)(object)textComponent != (Object)null) ? ((Object)textComponent).name : "NULL")));
			if ((Object)(object)textViewport != (Object)null)
			{
				Debug.Log((object)$"  textViewport children: {((Transform)textViewport).childCount}");
				for (int i = 0; i < ((Transform)textViewport).childCount; i++)
				{
					Transform child = ((Transform)textViewport).GetChild(i);
					Image component = ((Component)child).GetComponent<Image>();
					CanvasRenderer component2 = ((Component)child).GetComponent<CanvasRenderer>();
					Debug.Log((object)$"    [{i}] {((Object)child).name} - Image: {(Object)(object)component != (Object)null}, CanvasRenderer: {(Object)(object)component2 != (Object)null}, Active: {((Component)child).gameObject.activeSelf}");
					if ((Object)(object)component != (Object)null)
					{
						Debug.Log((object)$"         Image color: {((Graphic)component).color}, enabled: {((Behaviour)component).enabled}, raycastTarget: {((Graphic)component).raycastTarget}");
					}
				}
			}
			Transform val = ((Component)InputField).transform;
			while ((Object)(object)val != (Object)null)
			{
				Canvas component3 = ((Component)val).GetComponent<Canvas>();
				CanvasGroup component4 = ((Component)val).GetComponent<CanvasGroup>();
				if ((Object)(object)component3 != (Object)null)
				{
					Debug.Log((object)$"  Found Canvas on {((Object)val).name}: enabled={((Behaviour)component3).enabled}");
				}
				if ((Object)(object)component4 != (Object)null)
				{
					Debug.Log((object)$"  Found CanvasGroup on {((Object)val).name}: alpha={component4.alpha}, interactable={component4.interactable}");
				}
				val = val.parent;
			}
		}
	}
	public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
	{
		public string TooltipText;

		private static GameObject _tooltip;

		private static TextMeshProUGUI _tooltipLabel;

		private static TooltipTrigger _activeTooltip;

		public void OnPointerEnter(PointerEventData e)
		{
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Expected O, but got Unknown
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_012f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0139: Expected O, but got Unknown
			//IL_013f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0146: Expected O, but got Unknown
			//IL_0193: Unknown result type (might be due to invalid IL or missing references)
			if (string.IsNullOrEmpty(TooltipText))
			{
				return;
			}
			_activeTooltip = this;
			if ((Object)(object)_tooltip == (Object)null)
			{
				Canvas componentInParent = ((Component)this).GetComponentInParent<Canvas>();
				if ((Object)(object)componentInParent == (Object)null)
				{
					return;
				}
				_tooltip = new GameObject("Tooltip");
				_tooltip.transform.SetParent(((Component)componentInParent).transform, false);
				RectTransform val = _tooltip.AddComponent<RectTransform>();
				val.pivot = new Vector2(0f, 1f);
				Image val2 = _tooltip.AddComponent<Image>();
				((Graphic)val2).color = new Color(0.1f, 0.14f, 0.18f, 0.95f);
				Outline val3 = _tooltip.AddComponent<Outline>();
				((Shadow)val3).effectColor = new Color(1f, 0.478f, 0.094f, 0.8f);
				((Shadow)val3).effectDistance = new Vector2(1f, 1f);
				ContentSizeFitter val4 = _tooltip.AddComponent<ContentSizeFitter>();
				val4.horizontalFit = (FitMode)2;
				val4.verticalFit = (FitMode)2;
				HorizontalLayoutGroup val5 = _tooltip.AddComponent<HorizontalLayoutGroup>();
				((LayoutGroup)val5).padding = new RectOffset(6, 6, 3, 3);
				GameObject val6 = new GameObject("Text");
				val6.transform.SetParent(_tooltip.transform, false);
				_tooltipLabel = val6.AddComponent<TextMeshProUGUI>();
				((TMP_Text)_tooltipLabel).fontSize = 11f;
				((Graphic)_tooltipLabel).color = new Color(0.9f, 0.9f, 0.9f, 1f);
			}
			((TMP_Text)_tooltipLabel).text = TooltipText;
			_tooltip.SetActive(true);
			UpdateTooltipPosition();
		}

		private void Update()
		{
			if ((Object)(object)_activeTooltip == (Object)(object)this && (Object)(object)_tooltip != (Object)null && _tooltip.activeSelf)
			{
				UpdateTooltipPosition();
			}
		}

		private void UpdateTooltipPosition()
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			if (!((Object)(object)_tooltip == (Object)null))
			{
				RectTransform component = _tooltip.GetComponent<RectTransform>();
				((Transform)component).position = Input.mousePosition + new Vector3(12f, -18f, 0f);
				LayoutRebuilder.ForceRebuildLayoutImmediate(component);
			}
		}

		public void OnPointerExit(PointerEventData e)
		{
			if ((Object)(object)_activeTooltip == (Object)(object)this)
			{
				_activeTooltip = null;
			}
			if ((Object)(object)_tooltip != (Object)null)
			{
				_tooltip.SetActive(false);
			}
		}
	}
	public class EditorHoverHandler : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
	{
		public Image Background;

		public Color NormalColor;

		public Color HoverColor;

		public void OnPointerEnter(PointerEventData eventData)
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)Background != (Object)null)
			{
				((Graphic)Background).color = HoverColor;
			}
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)Background != (Object)null)
			{
				((Graphic)Background).color = NormalColor;
			}
		}
	}
	public class FileDragHandler : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler
	{
		public PlannerFile File;

		public StationPlannerWindow Window;

		private GameObject _dragGhost;

		private Canvas _canvas;

		private static FileDragHandler _currentDrag;

		public static bool IsDragging => (Object)(object)_currentDrag != (Object)null;

		public static PlannerFile DraggedFile => _currentDrag?.File;

		private void OnDisable()
		{
			CleanupDrag();
		}

		private void OnDestroy()
		{
			CleanupDrag();
		}

		private void CleanupDrag()
		{
			if ((Object)(object)_dragGhost != (Object)null)
			{
				Object.Destroy((Object)(object)_dragGhost);
				_dragGhost = null;
			}
			if ((Object)(object)_currentDrag == (Object)(object)this)
			{
				_currentDrag = null;
			}
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Expected O, but got Unknown
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Expected O, but got Unknown
			//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0113: Unknown result type (might be due to invalid IL or missing references)
			//IL_0129: Unknown result type (might be due to invalid IL or missing references)
			//IL_0172: Unknown result type (might be due to invalid IL or missing references)
			if (File != null && !((Object)(object)Window == (Object)null))
			{
				_currentDrag = this;
				_canvas = ((Component)this).GetComponentInParent<Canvas>();
				_dragGhost = new GameObject("DragGhost");
				_dragGhost.transform.SetParent(((Component)_canvas).transform, false);
				RectTransform val = _dragGhost.AddComponent<RectTransform>();
				val.pivot = new Vector2(0f, 0.5f);
				val.sizeDelta = new Vector2(120f, 20f);
				Image val2 = _dragGhost.AddComponent<Image>();
				((Graphic)val2).color = new Color(0.984f, 0.69f, 0.231f, 0.3f);
				GameObject val3 = new GameObject("Text");
				val3.transform.SetParent(_dragGhost.transform, false);
				RectTransform val4 = val3.AddComponent<RectTransform>();
				val4.anchorMin = Vector2.zero;
				val4.anchorMax = Vector2.one;
				val4.offsetMin = new Vector2(4f, 0f);
				val4.offsetMax = new Vector2(-4f, 0f);
				TextMeshProUGUI val5 = val3.AddComponent<TextMeshProUGUI>();
				((TMP_Text)val5).text = File.Name;
				((TMP_Text)val5).fontSize = 11f;
				((Graphic)val5).color = new Color(0.9f, 0.9f, 0.9f, 1f);
				((TMP_Text)val5).alignment = (TextAlignmentOptions)4097;
				UpdateGhostPosition(eventData);
			}
		}

		public void OnDrag(PointerEventData eventData)
		{
			UpdateGhostPosition(eventData);
		}

		private void UpdateGhostPosition(PointerEventData eventData)
		{
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			if (!((Object)(object)_dragGhost == (Object)null) && !((Object)(object)_canvas == (Object)null))
			{
				RectTransform component = _dragGhost.GetComponent<RectTransform>();
				Vector2 val = default(Vector2);
				RectTransformUtility.ScreenPointToLocalPointInRectangle(((Component)_canvas).GetComponent<RectTransform>(), eventData.position, _canvas.worldCamera, ref val);
				component.anchoredPosition = val + new Vector2(10f, 0f);
			}
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			CleanupDrag();
		}
	}
	public class FolderDropHandler : MonoBehaviour, IDropHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler
	{
		public PlannerFolder Folder;

		public StationPlannerWindow Window;

		public Image Background;

		private Color _normalColor;

		private static readonly Color DROP_HIGHLIGHT = new Color(0.984f, 0.69f, 0.231f, 0.5f);

		private static readonly Color INSERT_HIGHLIGHT = new Color(0.984f, 0.69f, 0.231f, 1f);

		private GameObject _insertIndicator;

		private RectTransform _myRect;

		private void Start()
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)Background != (Object)null)
			{
				_normalColor = ((Graphic)Background).color;
			}
			_myRect = ((Component)this).GetComponent<RectTransform>();
		}

		private void OnDestroy()
		{
			DestroyInsertIndicator();
		}

		private void DestroyInsertIndicator()
		{
			if ((Object)(object)_insertIndicator != (Object)null)
			{
				Object.Destroy((Object)(object)_insertIndicator);
				_insertIndicator = null;
			}
		}

		private void CreateInsertIndicator(bool above)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Expected O, but got Unknown
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			DestroyInsertIndicator();
			_insertIndicator = new GameObject("InsertIndicator");
			_insertIndicator.transform.SetParent(((Component)this).transform, false);
			RectTransform val = _insertIndicator.AddComponent<RectTransform>();
			val.anchorMin = new Vector2(0f, (float)(above ? 1 : 0));
			val.anchorMax = new Vector2(1f, (float)(above ? 1 : 0));
			val.pivot = new Vector2(0.5f, 0.5f);
			val.sizeDelta = new Vector2(0f, 3f);
			val.anchoredPosition = Vector2.zero;
			Image val2 = _insertIndicator.AddComponent<Image>();
			((Graphic)val2).color = INSERT_HIGHLIGHT;
			((Graphic)val2).raycastTarget = false;
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			if (FileDragHandler.IsDragging && (Object)(object)Background != (Object)null)
			{
				((Graphic)Background).color = DROP_HIGHLIGHT;
			}
		}

		private void Update()
		{
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			if (FolderDragHandler.IsDragging && FolderDragHandler.DraggedFolder != Folder && (Object)(object)_myRect != (Object)null && RectTransformUtility.RectangleContainsScreenPoint(_myRect, Vector2.op_Implicit(Input.mousePosition), (Camera)null))
			{
				Vector2 val = default(Vector2);
				RectTransformUtility.ScreenPointToLocalPointInRectangle(_myRect, Vector2.op_Implicit(Input.mousePosition), (Camera)null, ref val);
				bool above = val.y > 0f;
				CreateInsertIndicator(above);
				((Graphic)Background).color = _normalColor;
			}
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)Background != (Object)null)
			{
				((Graphic)Background).color = _normalColor;
			}
			DestroyInsertIndicator();
		}

		public void OnDrop(PointerEventData eventData)
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			DestroyInsertIndicator();
			if ((Object)(object)Background != (Object)null)
			{
				((Graphic)Background).color = _normalColor;
			}
			PlannerFile draggedFile = FileDragHandler.DraggedFile;
			PlannerFolder draggedFolder = FolderDragHandler.DraggedFolder;
			if (draggedFile != null && Folder != null && (Object)(object)Window != (Object)null)
			{
				Window.MoveFileToFolder(draggedFile, Folder);
			}
			else if (draggedFolder != null && Folder != null && (Object)(object)Window != (Object)null && draggedFolder != Folder)
			{
				Vector2 val = default(Vector2);
				RectTransformUtility.ScreenPointToLocalPointInRectangle(_myRect, eventData.position, (Camera)null, ref val);
				bool insertBefore = val.y > 0f;
				Window.ReorderFolder(draggedFolder, Folder, insertBefore);
			}
		}
	}
	public class FolderShiftClickHandler : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
	{
		public PlannerFolder Folder;

		public StationPlannerWindow Window;

		public void OnPointerClick(PointerEventData eventData)
		{
			if (Folder != null && !((Object)(object)Window == (Object)null))
			{
				bool flag = Input.GetKey((KeyCode)304) || Input.GetKey((KeyCode)303);
				typeof(StationPlannerWindow).GetMethod("SelectFolder", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[2]
				{
					typeof(PlannerFolder),
					typeof(bool)
				}, null)?.Invoke(Window, new object[2] { Folder, flag });
			}
		}
	}
	public class FolderDragHandler : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler
	{
		public PlannerFolder Folder;

		public StationPlannerWindow Window;

		private GameObject _dragGhost;

		private Canvas _canvas;

		private static FolderDragHandler _currentDrag;

		public static bool IsDragging => (Object)(object)_currentDrag != (Object)null;

		public static PlannerFolder DraggedFolder => _currentDrag?.Folder;

		private void OnDisable()
		{
			CleanupDrag();
		}

		private void OnDestroy()
		{
			CleanupDrag();
		}

		private void CleanupDrag()
		{
			if ((Object)(object)_dragGhost != (Object)null)
			{
				Object.Destroy((Object)(object)_dragGhost);
				_dragGhost = null;
			}
			if ((Object)(object)_currentDrag == (Object)(object)this)
			{
				_currentDrag = null;
			}
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Expected O, but got Unknown
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Expected O, but got Unknown
			//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0113: Unknown result type (might be due to invalid IL or missing references)
			//IL_0129: Unknown result type (might be due to invalid IL or missing references)
			//IL_018a: Unknown result type (might be due to invalid IL or missing references)
			if (Folder != null && !((Object)(object)Window == (Object)null))
			{
				_currentDrag = this;
				_canvas = ((Component)this).GetComponentInParent<Canvas>();
				_dragGhost = new GameObject("FolderDragGhost");
				_dragGhost.transform.SetParent(((Component)_canvas).transform, false);
				RectTransform val = _dragGhost.AddComponent<RectTransform>();
				val.pivot = new Vector2(0f, 0.5f);
				val.sizeDelta = new Vector2(120f, 20f);
				Image val2 = _dragGhost.AddComponent<Image>();
				((Graphic)val2).color = new Color(0.4f, 0.7f, 1f, 0.3f);
				GameObject val3 = new GameObject("Text");
				val3.transform.SetParent(_dragGhost.transform, false);
				RectTransform val4 = val3.AddComponent<RectTransform>();
				val4.anchorMin = Vector2.zero;
				val4.anchorMax = Vector2.one;
				val4.offsetMin = new Vector2(4f, 0f);
				val4.offsetMax = new Vector2(-4f, 0f);
				TextMeshProUGUI val5 = val3.AddComponent<TextMeshProUGUI>();
				((TMP_Text)val5).text = "[" + Folder.Name + "]";
				((TMP_Text)val5).fontSize = 11f;
				((TMP_Text)val5).fontStyle = (FontStyles)1;
				((Graphic)val5).color = new Color(0.9f, 0.9f, 0.9f, 1f);
				((TMP_Text)val5).alignment = (TextAlignmentOptions)4097;
				UpdateGhostPosition(eventData);
			}
		}

		public void OnDrag(PointerEventData eventData)
		{
			UpdateGhostPosition(eventData);
		}

		private void UpdateGhostPosition(PointerEventData eventData)
		{
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			if (!((Object)(object)_dragGhost == (Object)null) && !((Object)(object)_canvas == (Object)null))
			{
				RectTransform component = _dragGhost.GetComponent<RectTransform>();
				Vector2 val = default(Vector2);
				RectTransformUtility.ScreenPointToLocalPointInRectangle(((Component)_canvas).GetComponent<RectTransform>(), eventData.position, _canvas.worldCamera, ref val);
				component.anchoredPosition = val + new Vector2(10f, 0f);
			}
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			CleanupDrag();
		}
	}
}
namespace StationpediaAscended.Tooltips
{
	public abstract class SPDABaseTooltip : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
	{
		protected string _deviceKey;

		protected string _cachedTooltipText;

		protected Coroutine _showCoroutine;

		protected bool _isHovering;

		protected const float HOVER_DELAY = 0.3f;

		public void OnPointerEnter(PointerEventData eventData)
		{
			_isHovering = true;
			if (_showCoroutine != null)
			{
				((MonoBehaviour)this).StopCoroutine(_showCoroutine);
			}
			_showCoroutine = ((MonoBehaviour)this).StartCoroutine(ShowTooltipAfterDelay());
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			_isHovering = false;
			if (_showCoroutine != null)
			{
				((MonoBehaviour)this).StopCoroutine(_showCoroutine);
				_showCoroutine = null;
			}
			HideTooltip();
		}

		private IEnumerator ShowTooltipAfterDelay()
		{
			yield return (object)new WaitForSecondsRealtime(0.3f);
			if (_isHovering)
			{
				string tooltipText = GetTooltipText();
				if (!string.IsNullOrEmpty(tooltipText))
				{
					StationpediaAscendedMod.CurrentTooltipText = tooltipText;
					StationpediaAscendedMod.ShowTooltip = true;
				}
			}
		}

		protected abstract string GetTooltipText();

		public void ClearCache()
		{
			_cachedTooltipText = null;
		}

		protected void OnDisable()
		{
			_isHovering = false;
			if (_showCoroutine != null)
			{
				((MonoBehaviour)this).StopCoroutine(_showCoroutine);
				_showCoroutine = null;
			}
			HideTooltip();
		}

		protected void HideTooltip()
		{
			StationpediaAscendedMod.ShowTooltip = false;
			StationpediaAscendedMod.CurrentTooltipText = "";
		}

		protected static string CleanName(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				return name;
			}
			return Regex.Replace(name, "<[^>]+>", "").Trim();
		}
	}
	public class SPDALogicTooltip : SPDABaseTooltip
	{
		private string _logicTypeName;

		private string _categoryName;

		public void Initialize(string deviceKey, string logicTypeName, string categoryName)
		{
			_deviceKey = deviceKey;
			_logicTypeName = logicTypeName;
			_categoryName = categoryName;
			_cachedTooltipText = null;
		}

		protected override string GetTooltipText()
		{
			if (_cachedTooltipText != null)
			{
				return _cachedTooltipText;
			}
			string text = SPDABaseTooltip.CleanName(_logicTypeName);
			if (_categoryName == "Mode")
			{
				ModeDescription modeDescription = StationpediaAscendedMod.GetModeDescription(_deviceKey, text);
				if (modeDescription != null)
				{
					_cachedTooltipText = FormatModeTooltip(text, modeDescription);
					return _cachedTooltipText;
				}
			}
			else if (_categoryName == "Connection")
			{
				LogicDescription connectionDescription = StationpediaAscendedMod.GetConnectionDescription(_deviceKey, text);
				if (connectionDescription != null)
				{
					_cachedTooltipText = FormatLogicTooltip(text, connectionDescription);
					return _cachedTooltipText;
				}
			}
			else if (_categoryName == "LogicSlot")
			{
				string slotLogicDescription = StationpediaAscendedMod.GetSlotLogicDescription(text);
				if (slotLogicDescription != null)
				{
					_cachedTooltipText = FormatSlotLogicTooltip(text, slotLogicDescription);
					return _cachedTooltipText;
				}
				LogicDescription logicDescription = StationpediaAscendedMod.GetLogicDescription(_deviceKey, text);
				if (logicDescription != null)
				{
					_cachedTooltipText = FormatLogicTooltip(text, logicDescription);
					return _cachedTooltipText;
				}
			}
			else
			{
				LogicDescription logicDescription2 = StationpediaAscendedMod.GetLogicDescription(_deviceKey, text);
				if (logicDescription2 != null)
				{
					_cachedTooltipText = FormatLogicTooltip(text, logicDescription2);
					return _cachedTooltipText;
				}
			}
			_cachedTooltipText = "<color=#FFA500><b>" + text + "</b></color>\n\n<color=#AAAAAA>No detailed description available yet.</color>\n<color=#666666>Device: " + _deviceKey + "</color>";
			return _cachedTooltipText;
		}

		private static string FormatLogicTooltip(string cleanName, LogicDescription desc)
		{
			return "<color=#FFA500><b>" + cleanName + "</b></color>\n<color=#888888>─────────────────────</color>\n<b>Type:</b> " + desc.dataType + "   <b>Range:</b> " + desc.range + "\n<color=#888888>─────────────────────</color>\n" + desc.description;
		}

		private static string FormatModeTooltip(string cleanName, ModeDescription desc)
		{
			return "<color=#9932CC><b>Mode: " + cleanName + "</b></color>\n<color=#888888>─────────────────────</color>\n" + desc.description;
		}

		private static string FormatSlotLogicTooltip(string cleanName, string description)
		{
			return "<color=#FFA500><b>Slot: " + cleanName + "</b></color>\n<color=#888888>─────────────────────</color>\n<b>Type:</b> LogicSlot   <b>Slots:</b> All readable slots\n<color=#888888>─────────────────────</color>\n" + description;
		}
	}
	public class SPDAMemoryTooltip : SPDABaseTooltip
	{
		private string _instructionName;

		public void Initialize(string deviceKey, string instructionName)
		{
			_deviceKey = deviceKey;
			_instructionName = instructionName;
			_cachedTooltipText = null;
		}

		protected override string GetTooltipText()
		{
			if (_cachedTooltipText != null)
			{
				return _cachedTooltipText;
			}
			string text = CleanInstructionName(_instructionName);
			MemoryDescription memoryDescription = StationpediaAscendedMod.GetMemoryDescription(_deviceKey, text);
			if (memoryDescription != null)
			{
				_cachedTooltipText = FormatTooltip(text, memoryDescription);
				return _cachedTooltipText;
			}
			_cachedTooltipText = "<color=#FF69B4><b>" + text + "</b></color>\n\n<color=#AAAAAA>No instruction description available yet.</color>";
			return _cachedTooltipText;
		}

		private static string FormatTooltip(string cleanName, MemoryDescription desc)
		{
			string text = (string.IsNullOrEmpty(desc.parameters) ? "none" : desc.parameters);
			string text2 = "<color=#FF69B4><b>Instruction: " + cleanName + "</b></color>\n<color=#888888>─────────────────────</color>\n<b>OpCode:</b> " + desc.opCode + "\n<b>Parameters:</b> " + text + "\n<color=#888888>─────────────────────</color>\n" + desc.description;
			if (!string.IsNullOrEmpty(desc.byteLayout))
			{
				text2 = text2 + "\n<color=#888888>─────────────────────</color>\n<color=#AAAAAA><b>Byte Layout:</b></color>\n" + desc.byteLayout;
			}
			return text2;
		}

		private static string CleanInstructionName(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				return name;
			}
			string text = SPDABaseTooltip.CleanName(name);
			int num = text.IndexOf(" OP_CODE:");
			if (num > 0)
			{
				text = text.Substring(0, num).Trim();
			}
			return text;
		}
	}
	public class SPDAPrefabInfoTooltip : SPDABaseTooltip
	{
		private string _fullValue;

		private bool _isPrefabHash;

		public void Initialize(string deviceKey, string fullValue, bool isPrefabHash)
		{
			_deviceKey = deviceKey;
			_fullValue = fullValue;
			_isPrefabHash = isPrefabHash;
			_cachedTooltipText = null;
		}

		protected override string GetTooltipText()
		{
			if (_cachedTooltipText != null)
			{
				return _cachedTooltipText;
			}
			if (_isPrefabHash)
			{
				_cachedTooltipText = FormatPrefabHashTooltip(_fullValue);
			}
			else
			{
				_cachedTooltipText = FormatPrefabNameTooltip(_fullValue);
			}
			return _cachedTooltipText;
		}

		private static string FormatPrefabNameTooltip(string prefabName)
		{
			return "<color=#FFA500><b>Prefab Name</b></color>\n<color=#888888>─────────────────────</color>\n<color=#00BFFF>" + prefabName + "</color>\n\n<color=#888888>─────────────────────</color>\n<color=#90EE90><b>Click to copy</b></color>\n\n<color=#AAAAAA>Used in IC10 programming to identify this device type.\nCan be used interchangeably with Prefab Hash.\n\nExample: <color=#FFA500>lb r0 " + TruncateForExample(prefabName) + " Setting</color></color>";
		}

		private static string FormatPrefabHashTooltip(string prefabHash)
		{
			return "<color=#FFA500><b>Prefab Hash</b></color>\n<color=#888888>─────────────────────</color>\n<color=#00BFFF>" + prefabHash + "</color>\n\n<color=#888888>─────────────────────</color>\n<color=#90EE90><b>Click to copy</b></color>\n\n<color=#AAAAAA>Numeric identifier for this device type in IC10.\nCan be used interchangeably with Prefab Name.\n\nExample: <color=#FFA500>lb r0 " + prefabHash + " Setting</color></color>";
		}

		private static string TruncateForExample(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				return name;
			}
			name = SPDABaseTooltip.CleanName(name);
			if (name.Length > 25)
			{
				return name.Substring(0, 22) + "...";
			}
			return name;
		}
	}
	public class SPDAPropertyTooltip : SPDABaseTooltip
	{
		private string _propertyName;

		public void Initialize(string propertyName)
		{
			_propertyName = propertyName;
			_cachedTooltipText = null;
		}

		protected override string GetTooltipText()
		{
			if (_cachedTooltipText != null)
			{
				return _cachedTooltipText;
			}
			PropertyDescription propertyDescription = StationpediaAscendedMod.GetPropertyDescription(_propertyName);
			if (propertyDescription != null)
			{
				_cachedTooltipText = FormatPropertyTooltip(_propertyName, propertyDescription);
				return _cachedTooltipText;
			}
			_cachedTooltipText = "<color=#FFA500><b>" + _propertyName + "</b></color>\n\n<color=#AAAAAA>No detailed description available yet.</color>";
			return _cachedTooltipText;
		}

		private static string FormatPropertyTooltip(string propertyName, PropertyDescription desc)
		{
			string text = "<color=#FFA500><b>" + propertyName + "</b></color>\n<color=#888888>─────────────────────</color>\n";
			if (!string.IsNullOrEmpty(desc.type))
			{
				text = text + "<b>Type:</b> " + desc.type + "\n";
			}
			if (!string.IsNullOrEmpty(desc.threshold))
			{
				text = text + "<b>Threshold:</b> " + desc.threshold + "\n";
			}
			text = text + "<color=#888888>─────────────────────</color>\n" + desc.description;
			if (!string.IsNullOrEmpty(desc.formula))
			{
				text = text + "\n\n<color=#888888><i>Formula: " + desc.formula + "</i></color>";
			}
			return text;
		}
	}
	public class SPDASlotTooltip : SPDABaseTooltip
	{
		private string _slotName;

		public void Initialize(string deviceKey, string slotName)
		{
			_deviceKey = deviceKey;
			_slotName = slotName;
			_cachedTooltipText = null;
		}

		protected override string GetTooltipText()
		{
			if (_cachedTooltipText != null)
			{
				return _cachedTooltipText;
			}
			string text = SPDABaseTooltip.CleanName(_slotName);
			SlotDescription slotDescription = StationpediaAscendedMod.GetSlotDescription(_deviceKey, text);
			if (slotDescription != null)
			{
				_cachedTooltipText = FormatTooltip(text, slotDescription);
				return _cachedTooltipText;
			}
			_cachedTooltipText = "<color=#00BFFF><b>" + text + "</b></color>\n\n<color=#AAAAAA>No slot description available yet.</color>";
			return _cachedTooltipText;
		}

		private static string FormatTooltip(string cleanName, SlotDescription desc)
		{
			return "<color=#00BFFF><b>Slot: " + cleanName + "</b></color>\n<color=#888888>─────────────────────</color>\n<b>Type:</b> " + desc.slotType + "\n<color=#888888>─────────────────────</color>\n" + desc.description;
		}
	}
	public class SPDAVersionTooltip : SPDABaseTooltip
	{
		private string _versionName;

		public void Initialize(string deviceKey, string versionName)
		{
			_deviceKey = deviceKey;
			_versionName = versionName;
			_cachedTooltipText = null;
		}

		protected override string GetTooltipText()
		{
			if (_cachedTooltipText != null)
			{
				return _cachedTooltipText;
			}
			string text = SPDABaseTooltip.CleanName(_versionName);
			VersionDescription versionDescription = StationpediaAscendedMod.GetVersionDescription(_deviceKey, text);
			if (versionDescription != null)
			{
				_cachedTooltipText = FormatTooltip(text, versionDescription);
				return _cachedTooltipText;
			}
			_cachedTooltipText = "<color=#90EE90><b>" + text + "</b></color>\n\n<color=#AAAAAA>No version description available yet.</color>";
			return _cachedTooltipText;
		}

		private static string FormatTooltip(string cleanName, VersionDescription desc)
		{
			return "<color=#90EE90><b>Version: " + cleanName + "</b></color>\n<color=#888888>─────────────────────</color>\n" + desc.description;
		}
	}
}
namespace StationpediaAscended.Patches
{
	public static class HarmonyPatches
	{
		private static Vector3 _dragOffset;

		private static bool _scrollbarVisibilityFixed = false;

		private static readonly Color StationeersBlue = new Color(0.06f, 0.12f, 0.22f, 0.92f);

		private static readonly Color StationeersBlueBorder = new Color(0.15f, 0.3f, 0.45f, 0.7f);

		private static Sprite _nativePanelSprite = null;

		private static Material _nativePanelMaterial = null;

		private static Type _nativePanelType = (Type)1;

		private static AssetBundle _tableBundle = null;

		private static GameObject _tableCellPrefab = null;

		private static GameObject _tableRowPrefab = null;

		private static GameObject _tableHeaderRowPrefab = null;

		private static GameObject _tableContainerPrefab = null;

		private static GameObject _tableSeparatorPrefab = null;

		private const string TABLE_BUNDLE_NAME = "stationpediaascended_table";

		public static void PopulateLogicSlotInserts_Postfix(UniversalPage __instance)
		{
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Expected O, but got Unknown
			try
			{
				StationpediaCategory logicSlotContents = __instance.LogicSlotContents;
				if ((Object)(object)logicSlotContents == (Object)null || (Object)(object)logicSlotContents.Contents == (Object)null)
				{
					return;
				}
				foreach (Transform item in (Transform)logicSlotContents.Contents)
				{
					Transform val = item;
					SPDALogic component = ((Component)val).GetComponent<SPDALogic>();
					if ((Object)(object)component == (Object)null || (Object)(object)component.InfoReadWrite == (Object)null)
					{
						continue;
					}
					string text = ((TMP_Text)component.InfoReadWrite).text;
					if (!string.IsNullOrEmpty(text) && text.Contains(",") && IsSlotNumberList(text))
					{
						string text2 = CondenseSlotNumbers(text);
						if (text2 != text)
						{
							((TMP_Text)component.InfoReadWrite).text = text2;
						}
					}
				}
			}
			catch (Exception)
			{
			}
		}

		public static bool Stationpedia_OnDrag_Prefix(Stationpedia __instance, PointerEventData eventData)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				Vector3 val = default(Vector3);
				((Vector3)(ref val))..ctor(eventData.position.x, eventData.position.y, 0f);
				((Transform)((UserInterfaceBase)__instance).RectTransform).position = val - _dragOffset;
			}
			catch
			{
			}
			return false;
		}

		public static bool Stationpedia_OnBeginDrag_Prefix(Stationpedia __instance, PointerEventData eventData)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				Vector3 val = default(Vector3);
				((Vector3)(ref val))..ctor(eventData.position.x, eventData.position.y, 0f);
				_dragOffset = val - ((Transform)((UserInterfaceBase)__instance).RectTransform).position;
			}
			catch
			{
			}
			return false;
		}

		public static void ChangeDisplay_Postfix(UniversalPage __instance, StationpediaPage page)
		{
			StartDelayedScrollbarFix();
			try
			{
				if (__instance == null || !Object.op_Implicit((Object)(object)__instance) || page == null)
				{
					return;
				}
				Transform content;
				try
				{
					content = (Transform)(object)__instance.Content;
					if (content == null || !Object.op_Implicit((Object)(object)content))
					{
						return;
					}
					GameObject gameObject = ((Component)content).gameObject;
					if (gameObject == null || !Object.op_Implicit((Object)(object)gameObject))
					{
						return;
					}
				}
				catch
				{
					return;
				}
				string key = page.Key;
				if (string.IsNullOrEmpty(key))
				{
					return;
				}
				Transform val = content.Find("GuideSectionsContent");
				if ((Object)(object)val != (Object)null)
				{
					Object.DestroyImmediate((Object)(object)((Component)val).gameObject);
				}
				Transform val2 = content.Find("SurvivalManualContent");
				if ((Object)(object)val2 != (Object)null)
				{
					Object.DestroyImmediate((Object)(object)((Component)val2).gameObject);
				}
				Transform val3 = content.Find("OperationalDetailsCategory");
				if ((Object)(object)val3 != (Object)null)
				{
					Object.DestroyImmediate((Object)(object)((Component)val3).gameObject);
				}
				TruncateLongPrefabName(__instance);
				AddPrefabTooltips(__instance);
				if (key == "SurvivalManual")
				{
					RenderSurvivalManualPage(__instance, content);
					return;
				}
				if (JsonGuideLoader.HasGuide(key))
				{
					RenderJsonGuidePage(__instance, content, key);
					return;
				}
				if (JsonMechanicsLoader.HasMechanic(key))
				{
					RenderJsonMechanicPage(__instance, content, key);
					return;
				}
				if (key == "DaylightSensorGuide")
				{
					RenderDaylightSensorGuidePage(__instance, content);
					return;
				}
				DeviceDescriptions value = null;
				StationpediaAscendedMod.DeviceDatabase.TryGetValue(key, out value);
				if (value != null)
				{
					HandlePageDescriptionModifications(__instance, value);
					if (value.operationalDetails != null && value.operationalDetails.Count != 0)
					{
						CreateOperationalDetailsCategory(__instance, content, value);
					}
				}
			}
			catch (Exception)
			{
			}
		}

		private static void TruncateLongPrefabName(UniversalPage page)
		{
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				if ((Object)(object)page.PrefabNameText == (Object)null || string.IsNullOrEmpty(((TMP_Text)page.PrefabNameText).text))
				{
					return;
				}
				((TMP_Text)page.PrefabNameText).overflowMode = (TextOverflowModes)1;
				((TMP_Text)page.PrefabNameText).enableWordWrapping = false;
				LayoutElement val = ((Component)page.PrefabNameText).gameObject.GetComponent<LayoutElement>();
				if ((Object)(object)val == (Object)null)
				{
					val = ((Component)page.PrefabNameText).gameObject.AddComponent<LayoutElement>();
				}
				val.preferredWidth = 300f;
				val.flexibleWidth = 0f;
				RectTransform rectTransform = ((TMP_Text)page.PrefabNameText).rectTransform;
				if ((Object)(object)rectTransform != (Object)null)
				{
					Vector2 sizeDelta = rectTransform.sizeDelta;
					if (sizeDelta.x > 300f || sizeDelta.x == 0f)
					{
						rectTransform.sizeDelta = new Vector2(300f, sizeDelta.y);
					}
				}
			}
			catch
			{
			}
		}

		private static void AddPrefabTooltips(UniversalPage page)
		{
			try
			{
				string currentPageKey = Stationpedia.CurrentPageKey;
				if ((Object)(object)page.PrefabNameText != (Object)null && !string.IsNullOrEmpty(((TMP_Text)page.PrefabNameText).text))
				{
					Transform parent = ((TMP_Text)page.PrefabNameText).transform.parent;
					if ((Object)(object)parent != (Object)null)
					{
						SPDAPrefabInfoTooltip sPDAPrefabInfoTooltip = ((Component)parent).gameObject.GetComponent<SPDAPrefabInfoTooltip>();
						if ((Object)(object)sPDAPrefabInfoTooltip == (Object)null)
						{
							sPDAPrefabInfoTooltip = ((Component)parent).gameObject.AddComponent<SPDAPrefabInfoTooltip>();
						}
						string fullValue = ExtractPrefabValue(((TMP_Text)page.PrefabNameText).text);
						sPDAPrefabInfoTooltip.Initialize(currentPageKey ?? "", fullValue, isPrefabHash: false);
					}
				}
				if (!((Object)(object)page.PrefabHashText != (Object)null) || string.IsNullOrEmpty(((TMP_Text)page.PrefabHashText).text))
				{
					return;
				}
				Transform parent2 = ((TMP_Text)page.PrefabHashText).transform.parent;
				if ((Object)(object)parent2 != (Object)null)
				{
					SPDAPrefabInfoTooltip sPDAPrefabInfoTooltip2 = ((Component)parent2).gameObject.GetComponent<SPDAPrefabInfoTooltip>();
					if ((Object)(object)sPDAPrefabInfoTooltip2 == (Object)null)
					{
						sPDAPrefabInfoTooltip2 = ((Component)parent2).gameObject.AddComponent<SPDAPrefabInfoTooltip>();
					}
					string fullValue2 = ExtractPrefabValue(((TMP_Text)page.PrefabHashText).text);
					sPDAPrefabInfoTooltip2.Initialize(currentPageKey ?? "", fullValue2, isPrefabHash: true);
				}
			}
			catch
			{
			}
		}

		private static string ExtractPrefabValue(string formattedText)
		{
			if (string.IsNullOrEmpty(formattedText))
			{
				return formattedText;
			}
			string text = Regex.Replace(formattedText, "<[^>]+>", "");
			return text.Trim();
		}

		private static void HandlePageDescriptionModifications(UniversalPage page, DeviceDescriptions deviceDesc)
		{
			TextMeshProUGUI pageDescription = page.PageDescription;
			if (pageDescription == null || !Object.op_Implicit((Object)(object)pageDescription))
			{
				return;
			}
			if (!string.IsNullOrEmpty(deviceDesc.pageDescription))
			{
				((TMP_Text)pageDescription).text = deviceDesc.pageDescription;
				return;
			}
			string text = ((TMP_Text)pageDescription).text ?? "";
			if (!string.IsNullOrEmpty(deviceDesc.pageDescriptionPrepend))
			{
				text = deviceDesc.pageDescriptionPrepend + "\n\n" + text;
			}
			if (!string.IsNullOrEmpty(deviceDesc.pageDescriptionAppend))
			{
				text = text + "\n\n" + deviceDesc.pageDescriptionAppend;
			}
			if (!string.IsNullOrEmpty(deviceDesc.pageDescriptionPrepend) || !string.IsNullOrEmpty(deviceDesc.pageDescriptionAppend))
			{
				((TMP_Text)pageDescription).text = text;
			}
		}

		private static void RenderSurvivalManualPage(UniversalPage page, Transform contentTransform)
		{
			//IL_0156: Unknown result type (might be due to invalid IL or missing references)
			//IL_015d: Expected O, but got Unknown
			//IL_0190: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01be: Unknown result type (might be due to invalid IL or missing references)
			//IL_0145: Unknown result type (might be due to invalid IL or missing references)
			//IL_014a: Unknown result type (might be due to invalid IL or missing references)
			TocLinkHandler.ClearRegistry();
			Transform val = contentTransform.Find("SurvivalManualContent");
			if ((Object)(object)val != (Object)null)
			{
				Object.DestroyImmediate((Object)(object)((Component)val).gameObject);
			}
			Stationpedia instance = Stationpedia.Instance;
			if ((Object)(object)instance == (Object)null)
			{
				return;
			}
			StationpediaCategory categoryPrefab = instance.CategoryPrefab;
			if ((Object)(object)categoryPrefab == (Object)null)
			{
				return;
			}
			TextMeshProUGUI pageDescription = page.PageDescription;
			if ((Object)(object)pageDescription == (Object)null)
			{
				return;
			}
			GuideDescription guide = JsonGuideLoader.GetGuide("SurvivalManual");
			if (guide == null)
			{
				ConsoleWindow.Print("[Stationpedia Ascended] SurvivalManual guide not found in descriptions.json", ConsoleColor.White, false, true);
				return;
			}
			DeviceDescriptions deviceDescriptions = JsonGuideLoader.ToDeviceDescriptions(guide);
			if (deviceDescriptions?.operationalDetails == null || deviceDescriptions.operationalDetails.Count == 0)
			{
				ConsoleWindow.Print("[Stationpedia Ascended] SurvivalManual has no sections", ConsoleColor.White, false, true);
				return;
			}
			if ((Object)(object)_nativePanelSprite == (Object)null)
			{
				StationpediaCategory logicSlotContents = page.LogicSlotContents;
				object obj;
				if (logicSlotContents == null)
				{
					obj = null;
				}
				else
				{
					RectTransform contents = logicSlotContents.Contents;
					obj = ((contents != null) ? ((Component)contents).GetComponent<Image>() : null);
				}
				Image val2 = (Image)obj;
				if ((Object)(object)val2 != (Object)null)
				{
					_nativePanelSprite = val2.sprite;
					_nativePanelMaterial = ((Graphic)val2).material;
					_nativePanelType = val2.type;
				}
			}
			GameObject val3 = new GameObject("SurvivalManualContent");
			val3.transform.SetParent(contentTransform, false);
			val3.transform.SetSiblingIndex(20);
			RectTransform val4 = val3.AddComponent<RectTransform>();
			val4.anchorMin = new Vector2(0f, 1f);
			val4.anchorMax = new Vector2(1f, 1f);
			val4.pivot = new Vector2(0.5f, 1f);
			VerticalLayoutGroup val5 = val3.AddComponent<VerticalLayoutGroup>();
			((HorizontalOrVerticalLayoutGroup)val5).spacing = 10f;
			((HorizontalOrVerticalLayoutGroup)val5).childForceExpandWidth = true;
			((HorizontalOrVerticalLayoutGroup)val5).childForceExpandHeight = false;
			((HorizontalOrVerticalLayoutGroup)val5).childControlWidth = true;
			((HorizontalOrVerticalLayoutGroup)val5).childControlHeight = true;
			ContentSizeFitter val6 = val3.AddComponent<ContentSizeFitter>();
			val6.horizontalFit = (FitMode)0;
			val6.verticalFit = (FitMode)2;
			if (!string.IsNullOrEmpty(deviceDescriptions.pageImage))
			{
				CreateGuideTopImage(val4, deviceDescriptions.pageImage);
			}
			string text = deviceDescriptions.pageDescription ?? "";
			if (!string.IsNullOrEmpty(deviceDescriptions.pageDescriptionPrepend))
			{
				text = deviceDescriptions.pageDescriptionPrepend + "\n\n" + text;
			}
			if (!string.IsNullOrEmpty(deviceDescriptions.pageDescriptionAppend))
			{
				text = text + "\n\n" + deviceDescriptions.pageDescriptionAppend;
			}
			if (!string.IsNullOrEmpty(text.Trim()))
			{
				CreateTextElement(val4, pageDescription, text.Trim());
			}
			foreach (OperationalDetail operationalDetail in deviceDescriptions.operationalDetails)
			{
				CreateSurvivalManualPart(val4, operationalDetail, categoryPrefab, pageDescription, page);
			}
			if ((Object)(object)Stationpedia.Instance?.ContentRectTransform != (Object)null)
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(Stationpedia.Instance.ContentRectTransform);
			}
			ConsoleWindow.Print("[Stationpedia Ascended] Rendered Survival Manual from JSON with per-Part TOCs", ConsoleColor.White, false, true);
		}

		private static void RenderJsonGuidePage(UniversalPage page, Transform contentTransform, string guideKey)
		{
			try
			{
				Stationpedia instance = Stationpedia.Instance;
				if ((Object)(object)instance == (Object)null)
				{
					return;
				}
				StationpediaCategory categoryPrefab = instance.CategoryPrefab;
				if ((Object)(object)categoryPrefab == (Object)null)
				{
					return;
				}
				GuideDescription guide = JsonGuideLoader.GetGuide(guideKey);
				if (guide == null)
				{
					ConsoleWindow.Print("[Stationpedia Ascended] Guide not found: " + guideKey, ConsoleColor.White, false, true);
					return;
				}
				DeviceDescriptions deviceDescriptions = JsonGuideLoader.ToDeviceDescriptions(guide);
				if (deviceDescriptions?.operationalDetails == null || deviceDescriptions.operationalDetails.Count == 0)
				{
					ConsoleWindow.Print("[Stationpedia Ascended] Guide " + guideKey + " has no sections", ConsoleColor.White, false, true);
					return;
				}
				RenderGuideSections(page, contentTransform, deviceDescriptions, guide.generateToc, guide.tocTitle);
				ConsoleWindow.Print("[Stationpedia Ascended] Rendered JSON guide: " + guideKey, ConsoleColor.White, false, true);
			}
			catch (Exception ex)
			{
				ConsoleWindow.Print("[Stationpedia Ascended] Error rendering JSON guide " + guideKey + ": " + ex.Message, ConsoleColor.White, false, true);
			}
		}

		private static void RenderJsonMechanicPage(UniversalPage page, Transform contentTransform, string mechanicKey)
		{
			try
			{
				Stationpedia instance = Stationpedia.Instance;
				if ((Object)(object)instance == (Object)null)
				{
					return;
				}
				StationpediaCategory categoryPrefab = instance.CategoryPrefab;
				if ((Object)(object)categoryPrefab == (Object)null)
				{
					return;
				}
				GuideDescription mechanic = JsonMechanicsLoader.GetMechanic(mechanicKey);
				if (mechanic == null)
				{
					ConsoleWindow.Print("[Stationpedia Ascended] Mechanic not found: " + mechanicKey, ConsoleColor.White, false, true);
					return;
				}
				DeviceDescriptions deviceDescriptions = JsonMechanicsLoader.ToDeviceDescriptions(mechanic);
				if (deviceDescriptions?.operationalDetails == null || deviceDescriptions.operationalDetails.Count == 0)
				{
					ConsoleWindow.Print("[Stationpedia Ascended] Mechanic " + mechanicKey + " has no sections", ConsoleColor.White, false, true);
					return;
				}
				RenderGuideSections(page, contentTransform, deviceDescriptions, mechanic.generateToc, mechanic.tocTitle);
				ConsoleWindow.Print("[Stationpedia Ascended] Rendered JSON mechanic: " + mechanicKey, ConsoleColor.White, false, true);
			}
			catch (Exception ex)
			{
				ConsoleWindow.Print("[Stationpedia Ascended] Error rendering JSON mechanic " + mechanicKey + ": " + ex.Message, ConsoleColor.White, false, true);
			}
		}

		private static void RenderDaylightSensorGuidePage(UniversalPage page, Transform contentTransform)
		{
			try
			{
				Stationpedia instance = Stationpedia.Instance;
				if ((Object)(object)instance == (Object)null)
				{
					return;
				}
				StationpediaCategory categoryPrefab = instance.CategoryPrefab;
				if ((Object)(object)categoryPrefab == (Object)null)
				{
					return;
				}
				TextMeshProUGUI pageDescription = page.PageDescription;
				if (!((Object)(object)pageDescription == (Object)null))
				{
					DeviceDescriptions daylightSensorGuideDescriptions = DaylightSensorGuideLoader.GetDaylightSensorGuideDescriptions();
					if (daylightSensorGuideDescriptions?.operationalDetails == null || daylightSensorGuideDescriptions.operationalDetails.Count == 0)
					{
						ConsoleWindow.Print("[Stationpedia Ascended] No Daylight Sensor Guide data found", ConsoleColor.White, false, true);
					}
					else
					{
						RenderGuideSections(page, contentTransform, daylightSensorGuideDescriptions, daylightSensorGuideDescriptions.generateToc, daylightSensorGuideDescriptions.tocTitle);
					}
				}
			}
			catch (Exception ex)
			{
				ConsoleWindow.Print("[Stationpedia Ascended] Error rendering Daylight Sensor Guide: " + ex.Message, ConsoleColor.White, false, true);
			}
		}

		private static void RenderGuideSections(UniversalPage page, Transform contentTransform, DeviceDescriptions guideData, bool generateToc = false, string tocTitle = null)
		{
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Expected O, but got Unknown
			//IL_011f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0136: Unknown result type (might be due to invalid IL or missing references)
			//IL_014d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				TocLinkHandler.ClearRegistry();
				Stationpedia instance = Stationpedia.Instance;
				if ((Object)(object)instance == (Object)null)
				{
					return;
				}
				StationpediaCategory categoryPrefab = instance.CategoryPrefab;
				if ((Object)(object)categoryPrefab == (Object)null)
				{
					return;
				}
				TextMeshProUGUI pageDescription = page.PageDescription;
				if ((Object)(object)pageDescription == (Object)null)
				{
					return;
				}
				if ((Object)(object)_nativePanelSprite == (Object)null)
				{
					StationpediaCategory logicSlotContents = page.LogicSlotContents;
					object obj;
					if (logicSlotContents == null)
					{
						obj = null;
					}
					else
					{
						RectTransform contents = logicSlotContents.Contents;
						obj = ((contents != null) ? ((Component)contents).GetComponent<Image>() : null);
					}
					Image val = (Image)obj;
					if ((Object)(object)val != (Object)null)
					{
						_nativePanelSprite = val.sprite;
						_nativePanelMaterial = ((Graphic)val).material;
						_nativePanelType = val.type;
					}
				}
				Transform val2 = contentTransform.Find("GuideSectionsContent");
				if ((Object)(object)val2 != (Object)null)
				{
					Object.DestroyImmediate((Object)(object)((Component)val2).gameObject);
				}
				GameObject val3 = new GameObject("GuideSectionsContent");
				val3.transform.SetParent(contentTransform, false);
				val3.transform.SetSiblingIndex(20);
				RectTransform val4 = val3.AddComponent<RectTransform>();
				val4.anchorMin = new Vector2(0f, 1f);
				val4.anchorMax = new Vector2(1f, 1f);
				val4.pivot = new Vector2(0.5f, 1f);
				VerticalLayoutGroup val5 = val3.AddComponent<VerticalLayoutGroup>();
				((HorizontalOrVerticalLayoutGroup)val5).spacing = 10f;
				((HorizontalOrVerticalLayoutGroup)val5).childForceExpandWidth = true;
				((HorizontalOrVerticalLayoutGroup)val5).childForceExpandHeight = false;
				((HorizontalOrVerticalLayoutGroup)val5).childControlWidth = true;
				((HorizontalOrVerticalLayoutGroup)val5).childControlHeight = true;
				ContentSizeFitter val6 = val3.AddComponent<ContentSizeFitter>();
				val6.horizontalFit = (FitMode)0;
				val6.verticalFit = (FitMode)2;
				if (!string.IsNullOrEmpty(guideData.pageImage))
				{
					CreateGuideTopImage(val4, guideData.pageImage);
				}
				string text = guideData.pageDescription ?? "";
				if (!string.IsNullOrEmpty(guideData.pageDescriptionPrepend))
				{
					text = guideData.pageDescriptionPrepend + "\n\n" + text;
				}
				if (!string.IsNullOrEmpty(guideData.pageDescriptionAppend))
				{
					text = text + "\n\n" + guideData.pageDescriptionAppend;
				}
				if (!string.IsNullOrEmpty(text.Trim()))
				{
					CreateTextElement(val4, pageDescription, text.Trim());
				}
				if (generateToc && guideData.operationalDetails != null && guideData.operationalDetails.Count > 0)
				{
					List<(string, string, int)> list = new List<(string, string, int)>();
					foreach (OperationalDetail operationalDetail in guideData.operationalDetails)
					{
						CollectTocEntries(list, operationalDetail, 0, guideData.tocFlat);
					}
					string title = (string.IsNullOrEmpty(guideData.tocTitle) ? "Contents" : guideData.tocTitle);
					CreateUnifiedTableOfContents(val4, pageDescription, list, title, centerColumns: true, placeAtTop: false);
				}
				foreach (OperationalDetail operationalDetail2 in guideData.operationalDetails)
				{
					RenderGuideSectionElement(val4, pageDescription, operationalDetail2, categoryPrefab, page, 0, null);
				}
				if ((Object)(object)instance.ContentRectTransform != (Object)null)
				{
					LayoutRebuilder.ForceRebuildLayoutImmediate(instance.ContentRectTransform);
				}
			}
			catch (Exception ex)
			{
				ConsoleWindow.Print("[Stationpedia Ascended] Error rendering guide sections: " + ex.Message, ConsoleColor.White, false, true);
			}
		}

		private static void CreateGuideTopImage(RectTransform parent, string imageFile)
		{
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Expected O, but got Unknown
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_010e: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				Sprite val = StationpediaAscendedMod.LoadImageFromModFolder(imageFile);
				if ((Object)(object)val == (Object)null)
				{
					ConsoleWindow.Print("[Stationpedia Ascended] Guide image not found: " + imageFile, ConsoleColor.White, false, true);
					return;
				}
				GameObject val2 = new GameObject("GuideTopImage");
				val2.transform.SetParent((Transform)(object)parent, false);
				Image val3 = val2.AddComponent<Image>();
				val3.sprite = val;
				val3.preserveAspect = true;
				RectTransform component = val2.GetComponent<RectTransform>();
				component.anchorMin = new Vector2(0f, 1f);
				component.anchorMax = new Vector2(1f, 1f);
				component.pivot = new Vector2(0.5f, 1f);
				float num = 100f;
				float num2 = (float)((Texture)val.texture).width / (float)((Texture)val.texture).height;
				float num3 = num / num2;
				LayoutElement val4 = val2.AddComponent<LayoutElement>();
				val4.preferredWidth = num;
				val4.preferredHeight = num3;
				val4.flexibleWidth = 0f;
				val4.flexibleHeight = 0f;
				component.sizeDelta = new Vector2(num, num3);
			}
			catch (Exception ex)
			{
				ConsoleWindow.Print("[Stationpedia Ascended] Error creating guide top image: " + ex.Message, ConsoleColor.White, false, true);
			}
		}

		private static void RenderGuideSectionElement(RectTransform parent, TextMeshProUGUI sourceText, OperationalDetail detail, StationpediaCategory categoryPrefab, UniversalPage page, int depth, string parentTocId)
		{
			Action<RectTransform, OperationalDetail, int, string> recurseCallback = delegate(RectTransform p, OperationalDetail d, int dep, string pid)
			{
				RenderGuideSectionElement(p, sourceText, d, categoryPrefab, page, dep, pid);
			};
			if (detail.collapsible && !string.IsNullOrEmpty(detail.title))
			{
				CreateUnifiedCollapsibleSection(parent, sourceText, detail, categoryPrefab, page, depth, parentTocId, recurseCallback);
			}
			else
			{
				CreateUnifiedInlineContent(parent, sourceText, detail, categoryPrefab, page, depth, parentTocId, recurseCallback);
			}
		}

		private static void CreateGuideCollapsibleSection(RectTransform parent, TextMeshProUGUI sourceText, OperationalDetail detail, StationpediaCategory categoryPrefab, UniversalPage page, int depth, string parentTocId)
		{
			try
			{
				StationpediaCategory val = Object.Instantiate<StationpediaCategory>(categoryPrefab, (Transform)(object)parent);
				if ((Object)(object)val == (Object)null)
				{
					return;
				}
				string text = detail.tocId ?? detail.title?.Replace(" ", "_") ?? $"section_{depth}";
				((Object)((Component)val).gameObject).name = "GuideSection_" + text;
				string titleColor = VanillaModeManager.GetTitleColor(depth);
				((TMP_Text)val.Title).text = "<color=" + titleColor + ">" + detail.title + "</color>";
				ApplyCustomCategoryIcons(val);
				ConfigureNestedCategoryLayout(val, detail, depth);
				if (!string.IsNullOrEmpty(detail.tocId))
				{
					TocLinkHandler.RegisterSection(detail.tocId, ((Component)val).GetComponent<RectTransform>(), val, parentTocId);
				}
				if (!string.IsNullOrEmpty(detail.imageFile))
				{
					CreateInlineImage(val.Contents, detail.imageFile);
				}
				if (!string.IsNullOrEmpty(detail.description))
				{
					CreateTextElement(val.Contents, sourceText, detail.description);
				}
				if (detail.items != null && detail.items.Count > 0)
				{
					StringBuilder stringBuilder = new StringBuilder();
					foreach (string item in detail.items)
					{
						stringBuilder.AppendLine("  • " + item);
					}
					CreateTextElement(val.Contents, sourceText, stringBuilder.ToString().TrimEnd());
				}
				if (detail.steps != null && detail.steps.Count > 0)
				{
					StringBuilder stringBuilder2 = new StringBuilder();
					int num = 1;
					string arg = (VanillaModeManager.IsVanillaMode ? "#FFFFFF" : "#FFA500");
					foreach (string step in detail.steps)
					{
						stringBuilder2.AppendLine($"  <color={arg}>{num}.</color> {step}");
						num++;
					}
					CreateTextElement(val.Contents, sourceText, stringBuilder2.ToString().TrimEnd());
				}
				if (!string.IsNullOrEmpty(detail.youtubeUrl))
				{
					CreateYouTubeLink(val.Contents, sourceText, detail.youtubeUrl, detail.youtubeLabel);
				}
				if (!string.IsNullOrEmpty(detail.videoFile))
				{
					CreateInlineVideo(val.Contents, detail.videoFile);
				}
				if (detail.children != null)
				{
					foreach (OperationalDetail child in detail.children)
					{
						RenderGuideSectionElement(val.Contents, sourceText, child, categoryPrefab, page, depth + 1, detail.tocId);
					}
				}
				((Component)val.Contents).gameObject.SetActive(false);
				if ((Object)(object)val.CollapseImage != (Object)null && (Object)(object)val.NotVisibleImage != (Object)null)
				{
					val.CollapseImage.sprite = val.NotVisibleImage;
				}
				IconAnimator component = ((Component)val).GetComponent<IconAnimator>();
				if ((Object)(object)component != (Object)null)
				{
					component.Initialize(expanded: false);
				}
				page.CreatedCategories.Add(val);
			}
			catch (Exception ex)
			{
				ConsoleWindow.Print("[Stationpedia Ascended] Error creating guide section: " + ex.Message, ConsoleColor.White, false, true);
			}
		}

		private static void CreateGuideInlineContent(RectTransform parent, TextMeshProUGUI sourceText, OperationalDetail detail, StationpediaCategory categoryPrefab, UniversalPage page, int depth, string parentTocId)
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Expected O, but got Unknown
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = new GameObject("InlineContent_" + (detail.title ?? "text"));
			val.transform.SetParent((Transform)(object)parent, false);
			RectTransform val2 = val.AddComponent<RectTransform>();
			val2.anchorMin = new Vector2(0f, 1f);
			val2.anchorMax = new Vector2(1f, 1f);
			val2.pivot = new Vector2(0.5f, 1f);
			VerticalLayoutGroup val3 = val.AddComponent<VerticalLayoutGroup>();
			((HorizontalOrVerticalLayoutGroup)val3).childForceExpandWidth = true;
			((HorizontalOrVerticalLayoutGroup)val3).childForceExpandHeight = false;
			((HorizontalOrVerticalLayoutGroup)val3).childControlWidth = true;
			((HorizontalOrVerticalLayoutGroup)val3).childControlHeight = true;
			((HorizontalOrVerticalLayoutGroup)val3).spacing = 5f;
			ContentSizeFitter val4 = val.AddComponent<ContentSizeFitter>();
			val4.horizontalFit = (FitMode)0;
			val4.verticalFit = (FitMode)2;
			if (!string.IsNullOrEmpty(detail.title))
			{
				string titleColor = VanillaModeManager.GetTitleColor(depth);
				CreateTextElement(val2, sourceText, "<b><color=" + titleColor + ">" + detail.title + "</color></b>");
			}
			if (!string.IsNullOrEmpty(detail.description))
			{
				CreateTextElement(val2, sourceText, detail.description);
			}
			if (detail.items != null && detail.items.Count > 0)
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (string item in detail.items)
				{
					stringBuilder.AppendLine("  • " + item);
				}
				CreateTextElement(val2, sourceText, stringBuilder.ToString().TrimEnd());
			}
			if (detail.steps != null && detail.steps.Count > 0)
			{
				StringBuilder stringBuilder2 = new StringBuilder();
				int num = 1;
				string arg = (VanillaModeManager.IsVanillaMode ? "#FFFFFF" : "#FFA500");
				foreach (string step in detail.steps)
				{
					stringBuilder2.AppendLine($"  <color={arg}>{num}.</color> {step}");
					num++;
				}
				CreateTextElement(val2, sourceText, stringBuilder2.ToString().TrimEnd());
			}
			if (detail.children == null)
			{
				return;
			}
			foreach (OperationalDetail child in detail.children)
			{
				RenderGuideSectionElement(val2, sourceText, child, categoryPrefab, page, depth + 1, detail.tocId);
			}
		}

		private static void CreateGuideTableOfContents(RectTransform parent, TextMeshProUGUI sourceText, DeviceDescriptions guideData)
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Expected O, but got Unknown
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Expected O, but got Unknown
			//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0106: Expected O, but got Unknown
			//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_017f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0150: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f7: Expected O, but got Unknown
			//IL_026c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0273: Expected O, but got Unknown
			//IL_02e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_02de: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b8: Expected O, but got Unknown
			//IL_0448: Unknown result type (might be due to invalid IL or missing references)
			//IL_044f: Expected O, but got Unknown
			//IL_04a0: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				if (guideData.operationalDetails == null || guideData.operationalDetails.Count == 0)
				{
					return;
				}
				GameObject val = new GameObject("GuideTOCOuter");
				RectTransform val2 = val.AddComponent<RectTransform>();
				val.transform.SetParent((Transform)(object)parent, false);
				val2.anchorMin = new Vector2(0f, 1f);
				val2.anchorMax = new Vector2(1f, 1f);
				val2.pivot = new Vector2(0.5f, 1f);
				LayoutElement val3 = val.AddComponent<LayoutElement>();
				val3.flexibleWidth = 1f;
				ContentSizeFitter val4 = val.AddComponent<ContentSizeFitter>();
				val4.horizontalFit = (FitMode)0;
				val4.verticalFit = (FitMode)2;
				HorizontalLayoutGroup val5 = val.AddComponent<HorizontalLayoutGroup>();
				((HorizontalOrVerticalLayoutGroup)val5).childForceExpandWidth = false;
				((HorizontalOrVerticalLayoutGroup)val5).childForceExpandHeight = false;
				((HorizontalOrVerticalLayoutGroup)val5).childControlWidth = true;
				((HorizontalOrVerticalLayoutGroup)val5).childControlHeight = true;
				((LayoutGroup)val5).childAlignment = (TextAnchor)0;
				((LayoutGroup)val5).padding = new RectOffset(0, 0, 5, 10);
				GameObject val6 = new GameObject("GuideTOC");
				val6.transform.SetParent(val.transform, false);
				Image val7 = val6.AddComponent<Image>();
				if (VanillaModeManager.IsVanillaMode)
				{
					if ((Object)(object)_nativePanelSprite != (Object)null)
					{
						val7.sprite = _nativePanelSprite;
						val7.type = _nativePanelType;
						((Graphic)val7).material = _nativePanelMaterial;
					}
					((Graphic)val7).color = new Color(0.15f, 0.15f, 0.15f, 0.95f);
				}
				else
				{
					Sprite windowBackgroundSprite = StationpediaAscendedMod.GetWindowBackgroundSprite();
					if ((Object)(object)windowBackgroundSprite != (Object)null)
					{
						val7.sprite = windowBackgroundSprite;
						val7.type = (Type)1;
					}
					((Graphic)val7).color = new Color(0.05f, 0.1f, 0.2f, 0.95f);
				}
				VerticalLayoutGroup val8 = val6.AddComponent<VerticalLayoutGroup>();
				((LayoutGroup)val8).padding = new RectOffset(16, 16, 12, 12);
				((HorizontalOrVerticalLayoutGroup)val8).spacing = 8f;
				((HorizontalOrVerticalLayoutGroup)val8).childForceExpandWidth = false;
				((HorizontalOrVerticalLayoutGroup)val8).childForceExpandHeight = false;
				((HorizontalOrVerticalLayoutGroup)val8).childControlWidth = true;
				((HorizontalOrVerticalLayoutGroup)val8).childControlHeight = true;
				LayoutElement val9 = val6.AddComponent<LayoutElement>();
				val9.flexibleWidth = 0f;
				val9.minWidth = 150f;
				ContentSizeFitter val10 = val6.AddComponent<ContentSizeFitter>();
				val10.horizontalFit = (FitMode)2;
				val10.verticalFit = (FitMode)2;
				GameObject val11 = new GameObject("TOCTitle");
				val11.transform.SetParent(val6.transform, false);
				TextMeshProUGUI val12 = val11.AddComponent<TextMeshProUGUI>();
				((TMP_Text)val12).font = ((TMP_Text)sourceText).font;
				((TMP_Text)val12).fontSharedMaterial = ((TMP_Text)sourceText).fontSharedMaterial;
				((TMP_Text)val12).fontSize = ((TMP_Text)sourceText).fontSize * 0.9f;
				((Graphic)val12).color = (Color)(VanillaModeManager.IsVanillaMode ? Color.white : new Color(1f, 0.6f, 0.2f, 1f));
				((TMP_Text)val12).text = (string.IsNullOrEmpty(guideData.tocTitle) ? "<b>Contents</b>" : ("<b>" + guideData.tocTitle + "</b>"));
				((TMP_Text)val12).enableWordWrapping = false;
				ContentSizeFitter val13 = val11.AddComponent<ContentSizeFitter>();
				val13.verticalFit = (FitMode)2;
				List<(string, string, int)> list = new List<(string, string, int)>();
				foreach (OperationalDetail operationalDetail in guideData.operationalDetails)
				{
					CollectTocEntries(list, operationalDetail, 0, guideData.tocFlat);
				}
				int count = list.Count;
				int val14 = (count + 8 - 1) / 8;
				val14 = Math.Max(1, Math.Min(3, val14));
				GameObject val15 = new GameObject("TOCColumns");
				val15.transform.SetParent(val6.transform, false);
				HorizontalLayoutGroup val16 = val15.AddComponent<HorizontalLayoutGroup>();
				((HorizontalOrVerticalLayoutGroup)val16).spacing = 24f;
				((HorizontalOrVerticalLayoutGroup)val16).childForceExpandWidth = false;
				((HorizontalOrVerticalLayoutGroup)val16).childForceExpandHeight = false;
				((HorizontalOrVerticalLayoutGroup)val16).childControlWidth = true;
				((HorizontalOrVerticalLayoutGroup)val16).childControlHeight = true;
				((LayoutGroup)val16).childAlignment = (TextAnchor)0;
				ContentSizeFitter val17 = val15.AddComponent<ContentSizeFitter>();
				val17.horizontalFit = (FitMode)2;
				val17.verticalFit = (FitMode)2;
				int num = 0;
				for (int i = 0; i < val14; i++)
				{
					if (num >= count)
					{
						break;
					}
					GameObject val18 = new GameObject($"TOCColumn{i}");
					val18.transform.SetParent(val15.transform, false);
					TextMeshProUGUI val19 = val18.AddComponent<TextMeshProUGUI>();
					((TMP_Text)val19).font = ((TMP_Text)sourceText).font;
					((TMP_Text)val19).fontSharedMaterial = ((TMP_Text)sourceText).fontSharedMaterial;
					((TMP_Text)val19).fontSize = ((TMP_Text)sourceText).fontSize * 0.85f;
					((Graphic)val19).color = ((Graphic)sourceText).color;
					((TMP_Text)val19).enableWordWrapping = false;
					((TMP_Text)val19).richText = true;
					((TMP_Text)val19).overflowMode = (TextOverflowModes)0;
					((TMP_Text)val19).lineSpacing = 8f;
					StringBuilder stringBuilder = new StringBuilder();
					int num2 = 0;
					while (num < count && num2 < 8)
					{
						(string, string, int) tuple = list[num];
						string text = ((tuple.Item3 > 0) ? new string(' ', tuple.Item3 * 2) : "");
						string text2 = ((tuple.Item3 > 0) ? "- " : "");
						string text3 = (VanillaModeManager.IsVanillaMode ? "#888888" : "#CC6600");
						string text4 = "#FFFFFF";
						if (tuple.Item3 > 0)
						{
							stringBuilder.AppendLine(text + "<color=" + text3 + ">" + text2 + "</color><link=toc_" + tuple.Item1 + "><color=" + text4 + ">" + tuple.Item2 + "</color></link>");
						}
						else
						{
							stringBuilder.AppendLine("<link=toc_" + tuple.Item1 + "><color=" + text4 + ">" + tuple.Item2 + "</color></link>");
						}
						num++;
						num2++;
					}
					((TMP_Text)val19).text = stringBuilder.ToString().TrimEnd();
					LayoutElement val20 = val18.AddComponent<LayoutElement>();
					val20.preferredWidth = 180f;
					ContentSizeFitter val21 = val18.AddComponent<ContentSizeFitter>();
					val21.horizontalFit = (FitMode)2;
					val21.verticalFit = (FitMode)2;
					TocLinkHandler tocLinkHandler = val18.AddComponent<TocLinkHandler>();
					tocLinkHandler.TextComponent = val19;
				}
			}
			catch (Exception ex)
			{
				ConsoleWindow.Print("[Stationpedia Ascended] Error creating guide TOC: " + ex.Message, ConsoleColor.White, false, true);
			}
		}

		private static void CreateSurvivalManualPart(RectTransform parent, OperationalDetail partDetail, StationpediaCategory categoryPrefab, TextMeshProUGUI sourceText, UniversalPage page)
		{
			StationpediaCategory val = Object.Instantiate<StationpediaCategory>(categoryPrefab, (Transform)(object)parent);
			((Object)((Component)val).gameObject).name = "Part_" + partDetail.tocId;
			string text = (VanillaModeManager.IsVanillaMode ? "#FFFFFF" : "#FF7A18");
			((TMP_Text)val.Title).text = "<color=" + text + ">" + partDetail.title + "</color>";
			ApplyCustomCategoryIcons(val);
			ConfigureSurvivalManualPartLayout(val);
			if (!string.IsNullOrEmpty(partDetail.tocId))
			{
				TocLinkHandler.RegisterSection(partDetail.tocId, ((Component)val).GetComponent<RectTransform>(), val);
			}
			if (partDetail.children != null && partDetail.children.Count > 0)
			{
				List<(string, string, int)> list = new List<(string, string, int)>();
				foreach (OperationalDetail child in partDetail.children)
				{
					string tocId = child.tocId;
					if (tocId == null || !tocId.EndsWith("_toc"))
					{
						CollectTocEntries(list, child);
					}
				}
				CreateUnifiedTableOfContents(val.Contents, sourceText, list, "Table Of Contents");
			}
			if (partDetail.children != null)
			{
				foreach (OperationalDetail child2 in partDetail.children)
				{
					string tocId2 = child2.tocId;
					if (tocId2 == null || !tocId2.EndsWith("_toc"))
					{
						CreateSurvivalManualSection(val.Contents, child2, categoryPrefab, sourceText, page, partDetail.tocId);
					}
				}
			}
			((Component)val.Contents).gameObject.SetActive(true);
			if ((Object)(object)val.CollapseImage != (Object)null && (Object)(object)val.VisibleImage != (Object)null)
			{
				val.CollapseImage.sprite = val.VisibleImage;
			}
			IconAnimator component = ((Component)val).GetComponent<IconAnimator>();
			if ((Object)(object)component != (Object)null)
			{
				component.Initialize(expanded: true);
			}
			page.CreatedCategories.Add(val);
		}

		private static void CreatePartTableOfContents(RectTransform parent, OperationalDetail partDetail, TextMeshProUGUI sourceText)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Expected O, but got Unknown
			//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00df: Expected O, but got Unknown
			//IL_0150: Unknown result type (might be due to invalid IL or missing references)
			//IL_0149: Unknown result type (might be due to invalid IL or missing references)
			//IL_0195: Unknown result type (might be due to invalid IL or missing references)
			//IL_019c: Expected O, but got Unknown
			//IL_02e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ef: Expected O, but got Unknown
			//IL_0340: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = new GameObject("TableOfContents");
			val.transform.SetParent((Transform)(object)parent, false);
			val.transform.SetAsFirstSibling();
			Image val2 = val.AddComponent<Image>();
			Sprite windowBackgroundSprite = StationpediaAscendedMod.GetWindowBackgroundSprite();
			if ((Object)(object)windowBackgroundSprite != (Object)null)
			{
				val2.sprite = windowBackgroundSprite;
				val2.type = (Type)1;
			}
			((Graphic)val2).color = new Color(0.3f, 0.3f, 0.3f, 0.15f);
			VerticalLayoutGroup val3 = val.AddComponent<VerticalLayoutGroup>();
			((LayoutGroup)val3).padding = new RectOffset(16, 16, 12, 12);
			((HorizontalOrVerticalLayoutGroup)val3).spacing = 8f;
			((HorizontalOrVerticalLayoutGroup)val3).childForceExpandWidth = true;
			((HorizontalOrVerticalLayoutGroup)val3).childForceExpandHeight = false;
			((HorizontalOrVerticalLayoutGroup)val3).childControlWidth = true;
			((HorizontalOrVerticalLayoutGroup)val3).childControlHeight = true;
			ContentSizeFitter val4 = val.AddComponent<ContentSizeFitter>();
			val4.horizontalFit = (FitMode)0;
			val4.verticalFit = (FitMode)2;
			GameObject val5 = new GameObject("TOCTitle");
			val5.transform.SetParent(val.transform, false);
			TextMeshProUGUI val6 = val5.AddComponent<TextMeshProUGUI>();
			((TMP_Text)val6).font = ((TMP_Text)sourceText).font;
			((TMP_Text)val6).fontSharedMaterial = ((TMP_Text)sourceText).fontSharedMaterial;
			((TMP_Text)val6).fontSize = ((TMP_Text)sourceText).fontSize * 0.95f;
			((Graphic)val6).color = (Color)(VanillaModeManager.IsVanillaMode ? Color.white : new Color(1f, 0.6f, 0.2f, 1f));
			((TMP_Text)val6).text = "<b>Table Of Contents</b>";
			((TMP_Text)val6).alignment = (TextAlignmentOptions)514;
			((TMP_Text)val6).enableWordWrapping = false;
			ContentSizeFitter val7 = val5.AddComponent<ContentSizeFitter>();
			val7.verticalFit = (FitMode)2;
			GameObject val8 = new GameObject("TOCColumns");
			val8.transform.SetParent(val.transform, false);
			HorizontalLayoutGroup val9 = val8.AddComponent<HorizontalLayoutGroup>();
			((HorizontalOrVerticalLayoutGroup)val9).spacing = 30f;
			((HorizontalOrVerticalLayoutGroup)val9).childForceExpandWidth = false;
			((HorizontalOrVerticalLayoutGroup)val9).childForceExpandHeight = false;
			((HorizontalOrVerticalLayoutGroup)val9).childControlWidth = true;
			((HorizontalOrVerticalLayoutGroup)val9).childControlHeight = true;
			((LayoutGroup)val9).childAlignment = (TextAnchor)1;
			ContentSizeFitter val10 = val8.AddComponent<ContentSizeFitter>();
			val10.horizontalFit = (FitMode)2;
			val10.verticalFit = (FitMode)2;
			LayoutElement val11 = val8.AddComponent<LayoutElement>();
			val11.flexibleWidth = 1f;
			List<(string, string, int)> list = new List<(string, string, int)>();
			if (partDetail.children != null)
			{
				foreach (OperationalDetail child in partDetail.children)
				{
					string tocId = child.tocId;
					if (tocId == null || !tocId.EndsWith("_toc"))
					{
						CollectTocEntriesFromSection(list, child, 0);
					}
				}
			}
			int count = list.Count;
			int val12 = Math.Min(3, (count + 8 - 1) / 8);
			val12 = Math.Max(1, val12);
			int num = (count + val12 - 1) / val12;
			int num2 = 0;
			for (int i = 0; i < val12; i++)
			{
				if (num2 >= count)
				{
					break;
				}
				GameObject val13 = new GameObject($"TOCColumn{i}");
				val13.transform.SetParent(val8.transform, false);
				TextMeshProUGUI val14 = val13.AddComponent<TextMeshProUGUI>();
				((TMP_Text)val14).font = ((TMP_Text)sourceText).font;
				((TMP_Text)val14).fontSharedMaterial = ((TMP_Text)sourceText).fontSharedMaterial;
				((TMP_Text)val14).fontSize = ((TMP_Text)sourceText).fontSize * 0.8f;
				((Graphic)val14).color = ((Graphic)sourceText).color;
				((TMP_Text)val14).enableWordWrapping = true;
				((TMP_Text)val14).overflowMode = (TextOverflowModes)1;
				((TMP_Text)val14).richText = true;
				LayoutElement val15 = val13.AddComponent<LayoutElement>();
				val15.preferredWidth = 200f;
				val15.flexibleWidth = 0f;
				StringBuilder stringBuilder = new StringBuilder();
				int num3 = 0;
				while (num2 < count && num3 < num)
				{
					(string, string, int) tuple = list[num2];
					string text = ((tuple.Item3 > 0) ? "  " : "");
					string text2 = ((tuple.Item3 > 0) ? "• " : "");
					if (tuple.Item3 == 0)
					{
						if (VanillaModeManager.IsVanillaMode)
						{
							stringBuilder.AppendLine("<link=toc_" + tuple.Item1 + "><u><b><color=#FFFFFF>" + tuple.Item2 + "</color></b></u></link>");
						}
						else
						{
							stringBuilder.AppendLine("<link=toc_" + tuple.Item1 + "><b><color=#FFA500>" + tuple.Item2 + "</color></b></link>");
						}
					}
					else
					{
						string text3 = (VanillaModeManager.IsVanillaMode ? "#888888" : "#CC6600");
						stringBuilder.AppendLine(text + "<color=" + text3 + ">" + text2 + "</color><link=toc_" + tuple.Item1 + "><color=#FFFFFF>" + tuple.Item2 + "</color></link>");
					}
					num2++;
					num3++;
				}
				((TMP_Text)val14).text = stringBuilder.ToString().TrimEnd();
				ContentSizeFitter val16 = val13.AddComponent<ContentSizeFitter>();
				val16.horizontalFit = (FitMode)0;
				val16.verticalFit = (FitMode)2;
				TocLinkHandler tocLinkHandler = val13.AddComponent<TocLinkHandler>();
				tocLinkHandler.TextComponent = val14;
			}
		}

		private static void CollectTocEntriesFromSection(List<(string tocId, string title, int depth)> entries, OperationalDetail detail, int depth)
		{
			if (!string.IsNullOrEmpty(detail.tocId) && !string.IsNullOrEmpty(detail.title))
			{
				entries.Add((detail.tocId, detail.title, depth));
			}
			if (detail.children == null)
			{
				return;
			}
			foreach (OperationalDetail child in detail.children)
			{
				CollectTocEntriesFromSection(entries, child, depth + 1);
			}
		}

		private static void CreateSurvivalManualSection(RectTransform parent, OperationalDetail sectionDetail, StationpediaCategory categoryPrefab, TextMeshProUGUI sourceText, UniversalPage page, string parentTocId)
		{
			Action<RectTransform, OperationalDetail, int, string> recurseCallback = delegate(RectTransform p, OperationalDetail d, int dep, string pid)
			{
				CreateSurvivalManualSection(p, d, categoryPrefab, sourceText, page, pid);
			};
			if (!sectionDetail.collapsible || string.IsNullOrEmpty(sectionDetail.title))
			{
				CreateUnifiedInlineContent(parent, sourceText, sectionDetail, categoryPrefab, page, 1, parentTocId, recurseCallback);
			}
			else
			{
				CreateUnifiedCollapsibleSection(parent, sourceText, sectionDetail, categoryPrefab, page, 1, parentTocId, recurseCallback);
			}
		}

		private static void CreateSurvivalManualSectionOld(RectTransform parent, OperationalDetail sectionDetail, StationpediaCategory categoryPrefab, TextMeshProUGUI sourceText, UniversalPage page, string parentTocId)
		{
			if (!sectionDetail.collapsible || string.IsNullOrEmpty(sectionDetail.title))
			{
				if (!string.IsNullOrEmpty(sectionDetail.description))
				{
					CreateTextElement(parent, sourceText, sectionDetail.description);
				}
				return;
			}
			StationpediaCategory val = Object.Instantiate<StationpediaCategory>(categoryPrefab, (Transform)(object)parent);
			((Object)((Component)val).gameObject).name = "Section_" + sectionDetail.tocId;
			string titleColor = VanillaModeManager.GetTitleColor(1);
			((TMP_Text)val.Title).text = "<color=" + titleColor + ">" + sectionDetail.title + "</color>";
			ApplyCustomCategoryIcons(val);
			ConfigureNestedCategoryLayout(val, sectionDetail);
			if (!string.IsNullOrEmpty(sectionDetail.tocId))
			{
				TocLinkHandler.RegisterSection(sectionDetail.tocId, ((Component)val).GetComponent<RectTransform>(), val, parentTocId);
			}
			if (!string.IsNullOrEmpty(sectionDetail.description))
			{
				CreateTextElement(val.Contents, sourceText, sectionDetail.description);
			}
			if (sectionDetail.items != null && sectionDetail.items.Count > 0)
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (string item in sectionDetail.items)
				{
					stringBuilder.AppendLine("  • " + item);
				}
				CreateTextElement(val.Contents, sourceText, stringBuilder.ToString().TrimEnd());
			}
			if (sectionDetail.steps != null && sectionDetail.steps.Count > 0)
			{
				StringBuilder stringBuilder2 = new StringBuilder();
				int num = 1;
				string arg = (VanillaModeManager.IsVanillaMode ? "#FFFFFF" : "#FFA500");
				foreach (string step in sectionDetail.steps)
				{
					stringBuilder2.AppendLine($"  <color={arg}>{num}.</color> {step}");
					num++;
				}
				CreateTextElement(val.Contents, sourceText, stringBuilder2.ToString().TrimEnd());
			}
			if (sectionDetail.children != null)
			{
				foreach (OperationalDetail child in sectionDetail.children)
				{
					CreateSurvivalManualSection(val.Contents, child, categoryPrefab, sourceText, page, sectionDetail.tocId);
				}
			}
			((Component)val.Contents).gameObject.SetActive(false);
			if ((Object)(object)val.CollapseImage != (Object)null && (Object)(object)val.NotVisibleImage != (Object)null)
			{
				val.CollapseImage.sprite = val.NotVisibleImage;
			}
			IconAnimator component = ((Component)val).GetComponent<IconAnimator>();
			if ((Object)(object)component != (Object)null)
			{
				component.Initialize(expanded: false);
			}
			page.CreatedCategories.Add(val);
		}

		private static void ConfigureSurvivalManualPartLayout(StationpediaCategory category)
		{
			//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0145: Unknown result type (might be due to invalid IL or missing references)
			//IL_014f: Expected O, but got Unknown
			GridLayoutGroup component = ((Component)category.Contents).GetComponent<GridLayoutGroup>();
			if ((Object)(object)component != (Object)null)
			{
				Object.DestroyImmediate((Object)(object)component);
			}
			Image val = ((Component)category.Contents).gameObject.GetComponent<Image>();
			if ((Object)(object)val == (Object)null)
			{
				val = ((Component)category.Contents).gameObject.AddComponent<Image>();
			}
			if (VanillaModeManager.IsVanillaMode)
			{
				if ((Object)(object)_nativePanelSprite != (Object)null)
				{
					val.sprite = _nativePanelSprite;
					val.type = _nativePanelType;
					((Graphic)val).material = _nativePanelMaterial;
				}
				((Graphic)val).color = Color.white;
			}
			else
			{
				Sprite windowBackgroundSprite = StationpediaAscendedMod.GetWindowBackgroundSprite();
				if ((Object)(object)windowBackgroundSprite != (Object)null)
				{
					val.sprite = windowBackgroundSprite;
					val.type = (Type)1;
					((Graphic)val).material = null;
				}
				((Graphic)val).color = StationeersBlue;
			}
			VerticalLayoutGroup val2 = ((Component)category.Contents).GetComponent<VerticalLayoutGroup>();
			if ((Object)(object)val2 == (Object)null)
			{
				val2 = ((Component)category.Contents).gameObject.AddComponent<VerticalLayoutGroup>();
			}
			((HorizontalOrVerticalLayoutGroup)val2).childForceExpandWidth = true;
			((HorizontalOrVerticalLayoutGroup)val2).childForceExpandHeight = false;
			((HorizontalOrVerticalLayoutGroup)val2).childControlWidth = true;
			((HorizontalOrVerticalLayoutGroup)val2).childControlHeight = true;
			((HorizontalOrVerticalLayoutGroup)val2).spacing = 10f;
			((LayoutGroup)val2).padding = new RectOffset(12, 12, 12, 12);
			ContentSizeFitter val3 = ((Component)category.Contents).GetComponent<ContentSizeFitter>();
			if ((Object)(object)val3 == (Object)null)
			{
				val3 = ((Component)category.Contents).gameObject.AddComponent<ContentSizeFitter>();
			}
			val3.verticalFit = (FitMode)2;
			val3.horizontalFit = (FitMode)0;
		}

		private static void CreateOperationalDetailsCategory(UniversalPage page, Transform contentTransform, DeviceDescriptions deviceDesc)
		{
			TocLinkHandler.ClearRegistry();
			Transform val = contentTransform.Find("OperationalDetailsCategory");
			if ((Object)(object)val != (Object)null)
			{
				Object.DestroyImmediate((Object)(object)((Component)val).gameObject);
			}
			Stationpedia instance = Stationpedia.Instance;
			if (instance == null || !Object.op_Implicit((Object)(object)instance))
			{
				return;
			}
			StationpediaCategory categoryPrefab = instance.CategoryPrefab;
			if (categoryPrefab == null || !Object.op_Implicit((Object)(object)categoryPrefab))
			{
				return;
			}
			TextMeshProUGUI pageDescription = page.PageDescription;
			if (pageDescription == null || !Object.op_Implicit((Object)(object)pageDescription))
			{
				return;
			}
			StationpediaCategory val2 = Object.Instantiate<StationpediaCategory>(categoryPrefab, contentTransform);
			if (val2 == null || !Object.op_Implicit((Object)(object)val2))
			{
				return;
			}
			((Object)((Component)val2).gameObject).name = "OperationalDetailsCategory";
			if (val2.Title == null || !Object.op_Implicit((Object)(object)val2.Title) || val2.Contents == null || !Object.op_Implicit((Object)(object)val2.Contents))
			{
				return;
			}
			string text = (VanillaModeManager.IsVanillaMode ? "#FFFFFF" : (string.IsNullOrEmpty(deviceDesc.operationalDetailsTitleColor) ? "#FF7A18" : deviceDesc.operationalDetailsTitleColor));
			((TMP_Text)val2.Title).text = "<color=" + text + ">Operational Details</color>";
			ApplyCustomCategoryIcons(val2);
			ConfigureCategoryLayout(val2, page, deviceDesc);
			if (deviceDesc.generateToc && deviceDesc.operationalDetails != null && deviceDesc.operationalDetails.Count > 0)
			{
				List<(string, string, int)> list = new List<(string, string, int)>();
				foreach (OperationalDetail operationalDetail in deviceDesc.operationalDetails)
				{
					CollectTocEntries(list, operationalDetail, 0, deviceDesc.tocFlat);
				}
				string title = (string.IsNullOrEmpty(deviceDesc.tocTitle) ? "Contents" : deviceDesc.tocTitle);
				CreateUnifiedTableOfContents(val2.Contents, pageDescription, list, title);
			}
			RenderAllOperationalDetails(val2, pageDescription, deviceDesc, categoryPrefab, page);
			((Component)val2).transform.SetSiblingIndex(20);
			((Component)val2.Contents).gameObject.SetActive(false);
			if ((Object)(object)val2.CollapseImage != (Object)null && (Object)(object)val2.NotVisibleImage != (Object)null)
			{
				val2.CollapseImage.sprite = val2.NotVisibleImage;
			}
			IconAnimator component = ((Component)val2).GetComponent<IconAnimator>();
			if ((Object)(object)component != (Object)null)
			{
				component.Initialize(expanded: false);
			}
			if ((Object)(object)Stationpedia.Instance?.ContentRectTransform != (Object)null)
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(Stationpedia.Instance.ContentRectTransform);
			}
			page.CreatedCategories.Add(val2);
		}

		private static void ApplyCustomCategoryIcons(StationpediaCategory category)
		{
			//IL_016a: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				if ((Object)(object)category.CollapseImage == (Object)null)
				{
					return;
				}
				if (!VanillaModeManager.IsVanillaMode)
				{
					if ((Object)(object)StationpediaAscendedMod._iconExpanded != (Object)null)
					{
						category.VisibleImage = StationpediaAscendedMod._iconExpanded;
					}
					if ((Object)(object)StationpediaAscendedMod._iconCollapsed != (Object)null)
					{
						category.NotVisibleImage = StationpediaAscendedMod._iconCollapsed;
					}
				}
				IconAnimator iconAnimator = ((Component)category).gameObject.GetComponent<IconAnimator>();
				if ((Object)(object)iconAnimator == (Object)null)
				{
					iconAnimator = ((Component)category).gameObject.AddComponent<IconAnimator>();
				}
				iconAnimator.TargetImage = category.CollapseImage;
				iconAnimator.ExpandedSprite = category.VisibleImage;
				iconAnimator.CollapsedSprite = category.NotVisibleImage;
				TextMeshProUGUI title = category.Title;
				Transform val = ((title != null) ? ((TMP_Text)title).transform.parent : null);
				if ((Object)(object)val == (Object)null)
				{
					Image collapseImage = category.CollapseImage;
					val = ((collapseImage != null) ? ((Component)collapseImage).transform.parent : null);
				}
				if ((Object)(object)val != (Object)null)
				{
					CategoryHeaderHandler categoryHeaderHandler = ((Component)val).gameObject.GetComponent<CategoryHeaderHandler>();
					if ((Object)(object)categoryHeaderHandler == (Object)null)
					{
						categoryHeaderHandler = ((Component)val).gameObject.AddComponent<CategoryHeaderHandler>();
					}
					categoryHeaderHandler.Initialize(category, iconAnimator);
					Graphic component = ((Component)val).GetComponent<Graphic>();
					if ((Object)(object)component == (Object)null)
					{
						Image val2 = ((Component)val).gameObject.AddComponent<Image>();
						((Graphic)val2).color = new Color(0f, 0f, 0f, 0f);
						((Graphic)val2).raycastTarget = true;
					}
					else
					{
						component.raycastTarget = true;
					}
					if ((Object)(object)category.Title != (Object)null)
					{
						((Graphic)category.Title).raycastTarget = false;
					}
					if ((Object)(object)category.CollapseImage != (Object)null)
					{
						((Graphic)category.CollapseImage).raycastTarget = false;
					}
				}
				if ((Object)(object)StationpediaAscendedMod.Instance != (Object)null)
				{
					((MonoBehaviour)StationpediaAscendedMod.Instance).StartCoroutine(MonitorCategoryState(category, iconAnimator));
				}
			}
			catch (Exception ex)
			{
				ConsoleWindow.Print("[Stationpedia Ascended] Error applying custom icons: " + ex.Message, ConsoleColor.White, false, true);
			}
		}

		private static IEnumerator MonitorCategoryState(StationpediaCategory category, IconAnimator animator)
		{
			if ((Object)(object)category == (Object)null || (Object)(object)animator == (Object)null)
			{
				yield break;
			}
			bool? lastState = null;
			while ((Object)(object)category != (Object)null && (Object)(object)category.Contents != (Object)null)
			{
				bool isExpanded = ((Component)category.Contents).gameObject.activeSelf;
				if (lastState != isExpanded)
				{
					lastState = isExpanded;
					animator.SetState(isExpanded, lastState.HasValue);
				}
				yield return (object)new WaitForSeconds(0.05f);
			}
		}

		private static void CreateTableOfContents(StationpediaCategory parentCategory, TextMeshProUGUI sourceText, DeviceDescriptions deviceDesc)
		{
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Expected O, but got Unknown
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0100: Unknown result type (might be due to invalid IL or missing references)
			//IL_0107: Expected O, but got Unknown
			//IL_0164: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bc: Expected O, but got Unknown
			//IL_020e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0215: Expected O, but got Unknown
			//IL_0287: Unknown result type (might be due to invalid IL or missing references)
			//IL_0280: Unknown result type (might be due to invalid IL or missing references)
			//IL_034d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0354: Expected O, but got Unknown
			//IL_03e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_03eb: Expected O, but got Unknown
			//IL_043c: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				if (deviceDesc.operationalDetails == null || deviceDesc.operationalDetails.Count == 0)
				{
					ConsoleWindow.Print("[Stationpedia Ascended] TOC: No operational details found", ConsoleColor.White, false, true);
					return;
				}
				GameObject val = new GameObject("TableOfContentsOuter");
				RectTransform val2 = val.AddComponent<RectTransform>();
				val.transform.SetParent((Transform)(object)parentCategory.Contents, false);
				val.transform.SetAsFirstSibling();
				val2.anchorMin = new Vector2(0f, 1f);
				val2.anchorMax = new Vector2(1f, 1f);
				val2.pivot = new Vector2(0.5f, 1f);
				HorizontalLayoutGroup val3 = val.AddComponent<HorizontalLayoutGroup>();
				((HorizontalOrVerticalLayoutGroup)val3).spacing = 10f;
				((HorizontalOrVerticalLayoutGroup)val3).childForceExpandWidth = false;
				((HorizontalOrVerticalLayoutGroup)val3).childForceExpandHeight = true;
				((HorizontalOrVerticalLayoutGroup)val3).childControlWidth = true;
				((HorizontalOrVerticalLayoutGroup)val3).childControlHeight = true;
				((LayoutGroup)val3).childAlignment = (TextAnchor)0;
				ContentSizeFitter val4 = val.AddComponent<ContentSizeFitter>();
				val4.horizontalFit = (FitMode)0;
				val4.verticalFit = (FitMode)2;
				GameObject val5 = new GameObject("TableOfContents");
				val5.transform.SetParent(val.transform, false);
				Image val6 = val5.AddComponent<Image>();
				Sprite windowBackgroundSprite = StationpediaAscendedMod.GetWindowBackgroundSprite();
				if ((Object)(object)windowBackgroundSprite != (Object)null)
				{
					val6.sprite = windowBackgroundSprite;
					val6.type = (Type)1;
				}
				((Graphic)val6).color = new Color(0.4f, 0.4f, 0.4f, 0.08f);
				LayoutElement val7 = val5.AddComponent<LayoutElement>();
				val7.flexibleWidth = 0f;
				val7.minWidth = 120f;
				val7.preferredWidth = 620f;
				VerticalLayoutGroup val8 = val5.AddComponent<VerticalLayoutGroup>();
				((LayoutGroup)val8).padding = new RectOffset(16, 16, 12, 12);
				((HorizontalOrVerticalLayoutGroup)val8).spacing = 6f;
				((HorizontalOrVerticalLayoutGroup)val8).childForceExpandWidth = true;
				((HorizontalOrVerticalLayoutGroup)val8).childForceExpandHeight = false;
				((HorizontalOrVerticalLayoutGroup)val8).childControlWidth = true;
				((HorizontalOrVerticalLayoutGroup)val8).childControlHeight = true;
				ContentSizeFitter val9 = val5.AddComponent<ContentSizeFitter>();
				val9.horizontalFit = (FitMode)2;
				val9.verticalFit = (FitMode)2;
				GameObject val10 = new GameObject("TOCTitle");
				val10.transform.SetParent(val5.transform, false);
				TextMeshProUGUI val11 = val10.AddComponent<TextMeshProUGUI>();
				((TMP_Text)val11).font = ((TMP_Text)sourceText).font;
				((TMP_Text)val11).fontSharedMaterial = ((TMP_Text)sourceText).fontSharedMaterial;
				((TMP_Text)val11).fontSize = ((TMP_Text)sourceText).fontSize * 0.9f;
				((Graphic)val11).color = (Color)(VanillaModeManager.IsVanillaMode ? Color.white : new Color(1f, 0.6f, 0.2f, 1f));
				((TMP_Text)val11).text = (string.IsNullOrEmpty(deviceDesc.tocTitle) ? "<b>Contents</b>" : ("<b>" + deviceDesc.tocTitle + "</b>"));
				((TMP_Text)val11).enableWordWrapping = false;
				ContentSizeFitter val12 = val10.AddComponent<ContentSizeFitter>();
				val12.verticalFit = (FitMode)2;
				List<(string, string, int)> list = new List<(string, string, int)>();
				foreach (OperationalDetail operationalDetail in deviceDesc.operationalDetails)
				{
					CollectTocEntries(list, operationalDetail, 0, deviceDesc.tocFlat);
				}
				int count = list.Count;
				int val13 = (count + 8 - 1) / 8;
				val13 = Math.Max(1, val13);
				GameObject val14 = new GameObject("TOCColumns");
				val14.transform.SetParent(val5.transform, false);
				HorizontalLayoutGroup val15 = val14.AddComponent<HorizontalLayoutGroup>();
				((HorizontalOrVerticalLayoutGroup)val15).spacing = 20f;
				((HorizontalOrVerticalLayoutGroup)val15).childForceExpandWidth = false;
				((HorizontalOrVerticalLayoutGroup)val15).childForceExpandHeight = false;
				((HorizontalOrVerticalLayoutGroup)val15).childControlWidth = true;
				((HorizontalOrVerticalLayoutGroup)val15).childControlHeight = true;
				((LayoutGroup)val15).childAlignment = (TextAnchor)0;
				ContentSizeFitter val16 = val14.AddComponent<ContentSizeFitter>();
				val16.horizontalFit = (FitMode)2;
				val16.verticalFit = (FitMode)2;
				int num = 0;
				for (int i = 0; i < val13; i++)
				{
					if (num >= count)
					{
						break;
					}
					GameObject val17 = new GameObject($"TOCColumn{i}");
					val17.transform.SetParent(val14.transform, false);
					TextMeshProUGUI val18 = val17.AddComponent<TextMeshProUGUI>();
					((TMP_Text)val18).font = ((TMP_Text)sourceText).font;
					((TMP_Text)val18).fontSharedMaterial = ((TMP_Text)sourceText).fontSharedMaterial;
					((TMP_Text)val18).fontSize = ((TMP_Text)sourceText).fontSize * 0.85f;
					((Graphic)val18).color = ((Graphic)sourceText).color;
					((TMP_Text)val18).enableWordWrapping = true;
					((TMP_Text)val18).richText = true;
					((TMP_Text)val18).overflowMode = (TextOverflowModes)1;
					((TMP_Text)val18).lineSpacing = 8f;
					LayoutElement val19 = val17.AddComponent<LayoutElement>();
					val19.preferredWidth = 200f;
					val19.flexibleWidth = 0f;
					StringBuilder stringBuilder = new StringBuilder();
					int num2 = 0;
					while (num < count && num2 < 8)
					{
						(string, string, int) tuple = list[num];
						string text = ((tuple.Item3 > 0) ? new string(' ', tuple.Item3 * 2) : "");
						string text2 = ((tuple.Item3 > 0) ? "- " : "");
						string text3 = (VanillaModeManager.IsVanillaMode ? "#888888" : "#CC6600");
						string text4 = "#FFFFFF";
						if (tuple.Item3 > 0)
						{
							stringBuilder.AppendLine(text + "<color=" + text3 + ">" + text2 + "</color><link=toc_" + tuple.Item1 + "><color=" + text4 + ">" + tuple.Item2 + "</color></link>");
						}
						else
						{
							stringBuilder.AppendLine("<link=toc_" + tuple.Item1 + "><color=" + text4 + ">" + tuple.Item2 + "</color></link>");
						}
						num++;
						num2++;
					}
					((TMP_Text)val18).text = stringBuilder.ToString().TrimEnd();
					ContentSizeFitter val20 = val17.AddComponent<ContentSizeFitter>();
					val20.horizontalFit = (FitMode)2;
					val20.verticalFit = (FitMode)2;
					TocLinkHandler tocLinkHandler = val17.AddComponent<TocLinkHandler>();
					tocLinkHandler.TextComponent = val18;
				}
			}
			catch (Exception ex)
			{
				ConsoleWindow.Print("[Stationpedia Ascended] Error creating TOC: " + ex.Message, ConsoleColor.White, false, true);
			}
		}

		private static void BuildTocLinks(StringBuilder sb, OperationalDetail detail, ref int index, int depth = 0)
		{
			if (!string.IsNullOrEmpty(detail.tocId) && !string.IsNullOrEmpty(detail.title))
			{
				string text = ((depth > 0) ? new string(' ', depth * 3) : "");
				string text2 = ((depth > 0) ? "- " : "");
				string text3 = (VanillaModeManager.IsVanillaMode ? "#888888" : "#CC6600");
				string text4 = "#FFFFFF";
				if (depth > 0)
				{
					sb.AppendLine(text + "<color=" + text3 + ">" + text2 + "</color><link=toc_" + detail.tocId + "><color=" + text4 + ">" + detail.title + "</color></link>");
				}
				else
				{
					sb.AppendLine("<link=toc_" + detail.tocId + "><color=" + text4 + ">" + detail.title + "</color></link>");
				}
				index++;
			}
			if (detail.children == null)
			{
				return;
			}
			foreach (OperationalDetail child in detail.children)
			{
				BuildTocLinks(sb, child, ref index, depth + 1);
			}
		}

		private static void CollectTocEntries(List<(string tocId, string title, int depth)> entries, OperationalDetail detail, int depth = 0, bool flatMode = false)
		{
			if (!string.IsNullOrEmpty(detail.tocId) && !string.IsNullOrEmpty(detail.title))
			{
				int item = ((!flatMode) ? depth : 0);
				entries.Add((detail.tocId, detail.title, item));
			}
			if (detail.children == null)
			{
				return;
			}
			foreach (OperationalDetail child in detail.children)
			{
				CollectTocEntries(entries, child, depth + 1, flatMode);
			}
		}

		private static void CreateUnifiedTableOfContents(RectTransform parent, TextMeshProUGUI sourceText, List<(string tocId, string title, int depth)> tocEntries, string title = "Contents", bool centerColumns = true, bool placeAtTop = true)
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Expected O, but got Unknown
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Expected O, but got Unknown
			//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0102: Expected O, but got Unknown
			//IL_0173: Unknown result type (might be due to invalid IL or missing references)
			//IL_016c: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d5: Expected O, but got Unknown
			//IL_02af: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b6: Expected O, but got Unknown
			//IL_0307: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				if (tocEntries == null || tocEntries.Count == 0)
				{
					return;
				}
				GameObject val = new GameObject("TableOfContents");
				val.transform.SetParent((Transform)(object)parent, false);
				if (placeAtTop)
				{
					val.transform.SetAsFirstSibling();
				}
				Image val2 = val.AddComponent<Image>();
				Sprite windowBackgroundSprite = StationpediaAscendedMod.GetWindowBackgroundSprite();
				if ((Object)(object)windowBackgroundSprite != (Object)null)
				{
					val2.sprite = windowBackgroundSprite;
					val2.type = (Type)1;
				}
				((Graphic)val2).color = new Color(0.3f, 0.3f, 0.3f, 0.15f);
				VerticalLayoutGroup val3 = val.AddComponent<VerticalLayoutGroup>();
				((LayoutGroup)val3).padding = new RectOffset(16, 16, 12, 12);
				((HorizontalOrVerticalLayoutGroup)val3).spacing = 8f;
				((HorizontalOrVerticalLayoutGroup)val3).childForceExpandWidth = true;
				((HorizontalOrVerticalLayoutGroup)val3).childForceExpandHeight = false;
				((HorizontalOrVerticalLayoutGroup)val3).childControlWidth = true;
				((HorizontalOrVerticalLayoutGroup)val3).childControlHeight = true;
				ContentSizeFitter val4 = val.AddComponent<ContentSizeFitter>();
				val4.horizontalFit = (FitMode)0;
				val4.verticalFit = (FitMode)2;
				GameObject val5 = new GameObject("TOCTitle");
				val5.transform.SetParent(val.transform, false);
				TextMeshProUGUI val6 = val5.AddComponent<TextMeshProUGUI>();
				((TMP_Text)val6).font = ((TMP_Text)sourceText).font;
				((TMP_Text)val6).fontSharedMaterial = ((TMP_Text)sourceText).fontSharedMaterial;
				((TMP_Text)val6).fontSize = ((TMP_Text)sourceText).fontSize * 0.95f;
				((Graphic)val6).color = (Color)(VanillaModeManager.IsVanillaMode ? Color.white : new Color(1f, 0.6f, 0.2f, 1f));
				((TMP_Text)val6).text = "<b>" + title + "</b>";
				((TMP_Text)val6).alignment = (TextAlignmentOptions)(centerColumns ? 514 : 513);
				((TMP_Text)val6).enableWordWrapping = false;
				ContentSizeFitter val7 = val5.AddComponent<ContentSizeFitter>();
				val7.verticalFit = (FitMode)2;
				GameObject val8 = new GameObject("TOCColumns");
				val8.transform.SetParent(val.transform, false);
				HorizontalLayoutGroup val9 = val8.AddComponent<HorizontalLayoutGroup>();
				((HorizontalOrVerticalLayoutGroup)val9).spacing = 30f;
				((HorizontalOrVerticalLayoutGroup)val9).childForceExpandWidth = false;
				((HorizontalOrVerticalLayoutGroup)val9).childForceExpandHeight = false;
				((HorizontalOrVerticalLayoutGroup)val9).childControlWidth = true;
				((HorizontalOrVerticalLayoutGroup)val9).childControlHeight = true;
				((LayoutGroup)val9).childAlignment = (TextAnchor)(centerColumns ? 1 : 0);
				ContentSizeFitter val10 = val8.AddComponent<ContentSizeFitter>();
				val10.horizontalFit = (FitMode)2;
				val10.verticalFit = (FitMode)2;
				LayoutElement val11 = val8.AddComponent<LayoutElement>();
				val11.flexibleWidth = 1f;
				int count = tocEntries.Count;
				int val12 = Math.Min(3, (count + 8 - 1) / 8);
				val12 = Math.Max(1, val12);
				int num = (count + val12 - 1) / val12;
				int num2 = 0;
				for (int i = 0; i < val12; i++)
				{
					if (num2 >= count)
					{
						break;
					}
					GameObject val13 = new GameObject($"TOCColumn{i}");
					val13.transform.SetParent(val8.transform, false);
					TextMeshProUGUI val14 = val13.AddComponent<TextMeshProUGUI>();
					((TMP_Text)val14).font = ((TMP_Text)sourceText).font;
					((TMP_Text)val14).fontSharedMaterial = ((TMP_Text)sourceText).fontSharedMaterial;
					((TMP_Text)val14).fontSize = ((TMP_Text)sourceText).fontSize * 0.8f;
					((Graphic)val14).color = ((Graphic)sourceText).color;
					((TMP_Text)val14).enableWordWrapping = true;
					((TMP_Text)val14).overflowMode = (TextOverflowModes)1;
					((TMP_Text)val14).richText = true;
					((TMP_Text)val14).lineSpacing = 8f;
					LayoutElement val15 = val13.AddComponent<LayoutElement>();
					val15.preferredWidth = 200f;
					val15.flexibleWidth = 0f;
					StringBuilder stringBuilder = new StringBuilder();
					int num3 = 0;
					while (num2 < count && num3 < num)
					{
						(string, string, int) tuple = tocEntries[num2];
						string text = ((tuple.Item3 > 0) ? "  " : "");
						string text2 = ((tuple.Item3 > 0) ? "• " : "");
						if (tuple.Item3 == 0)
						{
							if (VanillaModeManager.IsVanillaMode)
							{
								stringBuilder.AppendLine("<link=toc_" + tuple.Item1 + "><u><b><color=#FFFFFF>" + tuple.Item2 + "</color></b></u></link>");
							}
							else
							{
								stringBuilder.AppendLine("<link=toc_" + tuple.Item1 + "><b><color=#FFA500>" + tuple.Item2 + "</color></b></link>");
							}
						}
						else
						{
							string text3 = (VanillaModeManager.IsVanillaMode ? "#888888" : "#CC6600");
							stringBuilder.AppendLine(text + "<color=" + text3 + ">" + text2 + "</color><link=toc_" + tuple.Item1 + "><color=#FFFFFF>" + tuple.Item2 + "</color></link>");
						}
						num2++;
						num3++;
					}
					((TMP_Text)val14).text = stringBuilder.ToString().TrimEnd();
					ContentSizeFitter val16 = val13.AddComponent<ContentSizeFitter>();
					val16.horizontalFit = (FitMode)0;
					val16.verticalFit = (FitMode)2;
					TocLinkHandler tocLinkHandler = val13.AddComponent<TocLinkHandler>();
					tocLinkHandler.TextComponent = val14;
				}
			}
			catch (Exception ex)
			{
				ConsoleWindow.Print("[Stationpedia Ascended] Error creating unified TOC: " + ex.Message, ConsoleColor.White, false, true);
			}
		}

		private static void CreateUnifiedCollapsibleSection(RectTransform parent, TextMeshProUGUI sourceText, OperationalDetail detail, StationpediaCategory categoryPrefab, UniversalPage page, int depth, string parentTocId, Action<RectTransform, OperationalDetail, int, string> recurseCallback)
		{
			try
			{
				StationpediaCategory val = Object.Instantiate<StationpediaCategory>(categoryPrefab, (Transform)(object)parent);
				if ((Object)(object)val == (Object)null)
				{
					return;
				}
				string text = detail.tocId ?? detail.title?.Replace(" ", "_") ?? $"section_{depth}";
				((Object)((Component)val).gameObject).name = "Section_" + text;
				string titleColor = VanillaModeManager.GetTitleColor(depth);
				((TMP_Text)val.Title).text = "<color=" + titleColor + ">" + detail.title + "</color>";
				ApplyCustomCategoryIcons(val);
				ConfigureNestedCategoryLayout(val, detail, depth);
				if (!string.IsNullOrEmpty(detail.tocId))
				{
					TocLinkHandler.RegisterSection(detail.tocId, ((Component)val).GetComponent<RectTransform>(), val, parentTocId);
				}
				if (!string.IsNullOrEmpty(detail.imageFile))
				{
					CreateInlineImage(val.Contents, detail.imageFile);
				}
				if (!string.IsNullOrEmpty(detail.description))
				{
					CreateTextElement(val.Contents, sourceText, detail.description);
				}
				if (detail.items != null && detail.items.Count > 0)
				{
					StringBuilder stringBuilder = new StringBuilder();
					foreach (string item in detail.items)
					{
						stringBuilder.AppendLine("  • " + item);
					}
					CreateTextElement(val.Contents, sourceText, stringBuilder.ToString().TrimEnd());
				}
				if (detail.steps != null && detail.steps.Count > 0)
				{
					StringBuilder stringBuilder2 = new StringBuilder();
					int num = 1;
					string arg = (VanillaModeManager.IsVanillaMode ? "#FFFFFF" : "#FFA500");
					foreach (string step in detail.steps)
					{
						stringBuilder2.AppendLine($"  <color={arg}>{num}.</color> {step}");
						num++;
					}
					CreateTextElement(val.Contents, sourceText, stringBuilder2.ToString().TrimEnd());
				}
				if (!string.IsNullOrEmpty(detail.youtubeUrl))
				{
					CreateYouTubeLink(val.Contents, sourceText, detail.youtubeUrl, detail.youtubeLabel);
				}
				if (!string.IsNullOrEmpty(detail.videoFile))
				{
					CreateInlineVideo(val.Contents, detail.videoFile);
				}
				if (detail.table != null && detail.table.Count > 0)
				{
					CreateTableElement(val.Contents, sourceText, detail.table);
				}
				if (detail.children != null && recurseCallback != null)
				{
					foreach (OperationalDetail child in detail.children)
					{
						recurseCallback(val.Contents, child, depth + 1, detail.tocId);
					}
				}
				((Component)val.Contents).gameObject.SetActive(false);
				if ((Object)(object)val.CollapseImage != (Object)null && (Object)(object)val.NotVisibleImage != (Object)null)
				{
					val.CollapseImage.sprite = val.NotVisibleImage;
				}
				IconAnimator component = ((Component)val).GetComponent<IconAnimator>();
				if ((Object)(object)component != (Object)null)
				{
					component.Initialize(expanded: false);
				}
				page.CreatedCategories.Add(val);
			}
			catch (Exception ex)
			{
				ConsoleWindow.Print("[Stationpedia Ascended] Error creating unified section: " + ex.Message, ConsoleColor.White, false, true);
			}
		}

		private static void CreateUnifiedInlineContent(RectTransform parent, TextMeshProUGUI sourceText, OperationalDetail detail, StationpediaCategory categoryPrefab, UniversalPage page, int depth, string parentTocId, Action<RectTransform, OperationalDetail, int, string> recurseCallback)
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Expected O, but got Unknown
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = new GameObject("InlineContent_" + (detail.title ?? "text"));
			val.transform.SetParent((Transform)(object)parent, false);
			RectTransform val2 = val.AddComponent<RectTransform>();
			val2.anchorMin = new Vector2(0f, 1f);
			val2.anchorMax = new Vector2(1f, 1f);
			val2.pivot = new Vector2(0.5f, 1f);
			VerticalLayoutGroup val3 = val.AddComponent<VerticalLayoutGroup>();
			((HorizontalOrVerticalLayoutGroup)val3).childForceExpandWidth = true;
			((HorizontalOrVerticalLayoutGroup)val3).childForceExpandHeight = false;
			((HorizontalOrVerticalLayoutGroup)val3).childControlWidth = true;
			((HorizontalOrVerticalLayoutGroup)val3).childControlHeight = true;
			((HorizontalOrVerticalLayoutGroup)val3).spacing = 5f;
			ContentSizeFitter val4 = val.AddComponent<ContentSizeFitter>();
			val4.horizontalFit = (FitMode)0;
			val4.verticalFit = (FitMode)2;
			if (!string.IsNullOrEmpty(detail.tocId))
			{
				TocLinkHandler.RegisterSection(detail.tocId, val2, null, parentTocId);
			}
			if (!string.IsNullOrEmpty(detail.title))
			{
				string titleColor = VanillaModeManager.GetTitleColor(depth);
				CreateTextElement(val2, sourceText, "<b><color=" + titleColor + ">" + detail.title + "</color></b>");
			}
			if (!string.IsNullOrEmpty(detail.imageFile))
			{
				CreateInlineImage(val2, detail.imageFile);
			}
			if (!string.IsNullOrEmpty(detail.description))
			{
				CreateTextElement(val2, sourceText, detail.description);
			}
			if (detail.items != null && detail.items.Count > 0)
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (string item in detail.items)
				{
					stringBuilder.AppendLine("  • " + item);
				}
				CreateTextElement(val2, sourceText, stringBuilder.ToString().TrimEnd());
			}
			if (detail.steps != null && detail.steps.Count > 0)
			{
				StringBuilder stringBuilder2 = new StringBuilder();
				int num = 1;
				string arg = (VanillaModeManager.IsVanillaMode ? "#FFFFFF" : "#FFA500");
				foreach (string step in detail.steps)
				{
					stringBuilder2.AppendLine($"  <color={arg}>{num}.</color> {step}");
					num++;
				}
				CreateTextElement(val2, sourceText, stringBuilder2.ToString().TrimEnd());
			}
			if (!string.IsNullOrEmpty(detail.youtubeUrl))
			{
				CreateYouTubeLink(val2, sourceText, detail.youtubeUrl, detail.youtubeLabel);
			}
			if (!string.IsNullOrEmpty(detail.videoFile))
			{
				CreateInlineVideo(val2, detail.videoFile);
			}
			if (detail.table != null && detail.table.Count > 0)
			{
				CreateTableElement(val2, sourceText, detail.table);
			}
			if (detail.children == null || recurseCallback == null)
			{
				return;
			}
			foreach (OperationalDetail child in detail.children)
			{
				recurseCallback(val2, child, depth + 1, detail.tocId);
			}
		}

		private static void RenderAllOperationalDetails(StationpediaCategory parentCategory, TextMeshProUGUI sourceText, DeviceDescriptions deviceDesc, StationpediaCategory categoryPrefab, UniversalPage page)
		{
			foreach (OperationalDetail operationalDetail in deviceDesc.operationalDetails)
			{
				RenderOperationalDetailElement(parentCategory, sourceText, operationalDetail, categoryPrefab, page, 0);
			}
		}

		private static void RenderOperationalDetailElement(StationpediaCategory parentCategory, TextMeshProUGUI sourceText, OperationalDetail detail, StationpediaCategory categoryPrefab, UniversalPage page, int depth, string parentTocId = null)
		{
			Action<RectTransform, OperationalDetail, int, string> recurseCallback = delegate(RectTransform p, OperationalDetail d, int dep, string pid)
			{
				StationpediaCategory componentInParent = ((Component)p).GetComponentInParent<StationpediaCategory>();
				if ((Object)(object)componentInParent != (Object)null)
				{
					RenderOperationalDetailElement(componentInParent, sourceText, d, categoryPrefab, page, dep, pid);
				}
				else
				{
					RenderGuideSectionElement(p, sourceText, d, categoryPrefab, page, dep, pid);
				}
			};
			if (detail.collapsible && !string.IsNullOrEmpty(detail.title))
			{
				CreateUnifiedCollapsibleSection(parentCategory.Contents, sourceText, detail, categoryPrefab, page, depth, parentTocId, recurseCallback);
			}
			else
			{
				CreateUnifiedInlineContent(parentCategory.Contents, sourceText, detail, categoryPrefab, page, depth, parentTocId, recurseCallback);
			}
		}

		private static void CreateNestedCollapsibleCategory(StationpediaCategory parentCategory, TextMeshProUGUI sourceText, OperationalDetail detail, StationpediaCategory categoryPrefab, UniversalPage page, int depth, string parentTocId = null)
		{
			try
			{
				StationpediaCategory val = Object.Instantiate<StationpediaCategory>(categoryPrefab, (Transform)(object)parentCategory.Contents);
				if ((Object)(object)val == (Object)null)
				{
					return;
				}
				string text = detail.tocId ?? detail.title?.Replace(" ", "_") ?? $"section_{depth}";
				((Object)((Component)val).gameObject).name = "NestedCategory_" + text;
				string titleColor = VanillaModeManager.GetTitleColor(depth);
				((TMP_Text)val.Title).text = "<color=" + titleColor + ">" + detail.title + "</color>";
				ApplyCustomCategoryIcons(val);
				ConfigureNestedCategoryLayout(val, detail, depth);
				if (!string.IsNullOrEmpty(detail.tocId))
				{
					TocLinkHandler.RegisterSection(detail.tocId, ((Component)val).GetComponent<RectTransform>(), val, parentTocId);
				}
				if (!string.IsNullOrEmpty(detail.imageFile))
				{
					CreateInlineImage(val.Contents, detail.imageFile);
				}
				if (!string.IsNullOrEmpty(detail.description))
				{
					CreateTextElement(val.Contents, sourceText, detail.description);
				}
				if (detail.items != null && detail.items.Count > 0)
				{
					StringBuilder stringBuilder = new StringBuilder();
					foreach (string item in detail.items)
					{
						stringBuilder.AppendLine("  • " + item);
					}
					CreateTextElement(val.Contents, sourceText, stringBuilder.ToString().TrimEnd());
				}
				if (detail.steps != null && detail.steps.Count > 0)
				{
					StringBuilder stringBuilder2 = new StringBuilder();
					int num = 1;
					string arg = (VanillaModeManager.IsVanillaMode ? "#FFFFFF" : "#FFA500");
					foreach (string step in detail.steps)
					{
						stringBuilder2.AppendLine($"  <color={arg}>{num}.</color> {step}");
						num++;
					}
					CreateTextElement(val.Contents, sourceText, stringBuilder2.ToString().TrimEnd());
				}
				if (!string.IsNullOrEmpty(detail.youtubeUrl))
				{
					CreateYouTubeLink(val.Contents, sourceText, detail.youtubeUrl, detail.youtubeLabel);
				}
				if (!string.IsNullOrEmpty(detail.videoFile))
				{
					CreateInlineVideo(val.Contents, detail.videoFile);
				}
				if (detail.children != null)
				{
					foreach (OperationalDetail child in detail.children)
					{
						RenderOperationalDetailElement(val, sourceText, child, categoryPrefab, page, depth + 1, detail.tocId);
					}
				}
				((Component)val.Contents).gameObject.SetActive(false);
				if ((Object)(object)val.CollapseImage != (Object)null && (Object)(object)val.NotVisibleImage != (Object)null)
				{
					val.CollapseImage.sprite = val.NotVisibleImage;
				}
				IconAnimator component = ((Component)val).GetComponent<IconAnimator>();
				if ((Object)(object)component != (Object)null)
				{
					component.Initialize(expanded: false);
				}
				page.CreatedCategories.Add(val);
			}
			catch (Exception ex)
			{
				ConsoleWindow.Print("[Stationpedia Ascended] Error creating nested category: " + ex.Message, ConsoleColor.White, false, true);
			}
		}

		private static void ConfigureNestedCategoryLayout(StationpediaCategory category, OperationalDetail detail, int depth = 0)
		{
			//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d7: Expected O, but got Unknown
			//IL_0134: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			GridLayoutGroup component = ((Component)category.Contents).GetComponent<GridLayoutGroup>();
			if ((Object)(object)component != (Object)null)
			{
				Object.DestroyImmediate((Object)(object)component);
			}
			Transform val = ((Component)category.Contents).gameObject.transform.Find("BorderLayer");
			if ((Object)(object)val != (Object)null)
			{
				Object.DestroyImmediate((Object)(object)((Component)val).gameObject);
			}
			if (depth == 0)
			{
				Image val2 = ((Component)category.Contents).gameObject.GetComponent<Image>();
				if ((Object)(object)val2 == (Object)null)
				{
					val2 = ((Component)category.Contents).gameObject.AddComponent<Image>();
				}
				if (VanillaModeManager.IsVanillaMode)
				{
					if ((Object)(object)_nativePanelSprite != (Object)null)
					{
						val2.sprite = _nativePanelSprite;
						val2.type = _nativePanelType;
						((Graphic)val2).material = _nativePanelMaterial;
					}
					((Graphic)val2).color = Color.white;
				}
				else
				{
					Color backgroundColor = VanillaModeManager.GetBackgroundColor(detail.backgroundColor);
					Sprite windowBackgroundSprite = StationpediaAscendedMod.GetWindowBackgroundSprite();
					if ((Object)(object)windowBackgroundSprite != (Object)null)
					{
						val2.sprite = windowBackgroundSprite;
						val2.type = (Type)1;
						((Graphic)val2).material = null;
					}
					((Graphic)val2).color = backgroundColor;
				}
			}
			else
			{
				Image component2 = ((Component)category.Contents).gameObject.GetComponent<Image>();
				if ((Object)(object)component2 != (Object)null)
				{
					Object.DestroyImmediate((Object)(object)component2);
				}
			}
			VerticalLayoutGroup val3 = ((Component)category.Contents).GetComponent<VerticalLayoutGroup>();
			if ((Object)(object)val3 == (Object)null)
			{
				val3 = ((Component)category.Contents).gameObject.AddComponent<VerticalLayoutGroup>();
			}
			((HorizontalOrVerticalLayoutGroup)val3).childForceExpandWidth = true;
			((HorizontalOrVerticalLayoutGroup)val3).childForceExpandHeight = false;
			((HorizontalOrVerticalLayoutGroup)val3).childControlWidth = true;
			((HorizontalOrVerticalLayoutGroup)val3).childControlHeight = true;
			((HorizontalOrVerticalLayoutGroup)val3).spacing = 8f;
			((LayoutGroup)val3).padding = new RectOffset(16, 16, 14, 14);
			ContentSizeFitter val4 = ((Component)category.Contents).GetComponent<ContentSizeFitter>();
			if ((Object)(object)val4 == (Object)null)
			{
				val4 = ((Component)category.Contents).gameObject.AddComponent<ContentSizeFitter>();
			}
			val4.verticalFit = (FitMode)2;
			val4.horizontalFit = (FitMode)0;
		}

		private static void CreateInlineContent(RectTransform parent, TextMeshProUGUI sourceText, OperationalDetail detail, int depth)
		{
			string value = ((depth > 0) ? $"<indent={depth * 5}%>" : "");
			string value2 = ((depth > 0) ? "</indent>" : "");
			string titleColor = VanillaModeManager.GetTitleColor(depth);
			string arg = (VanillaModeManager.IsVanillaMode ? "#FFFFFF" : "#FFA500");
			StringBuilder stringBuilder = new StringBuilder();
			if (!string.IsNullOrEmpty(detail.title))
			{
				stringBuilder.Append(value);
				stringBuilder.AppendLine("<color=" + titleColor + "><b>" + detail.title + "</b></color>");
				stringBuilder.Append(value2);
			}
			if (!string.IsNullOrEmpty(detail.description))
			{
				stringBuilder.Append(value);
				stringBuilder.AppendLine(detail.description);
				stringBuilder.Append(value2);
			}
			if (detail.items != null && detail.items.Count > 0)
			{
				foreach (string item in detail.items)
				{
					stringBuilder.Append(value);
					stringBuilder.AppendLine("  • " + item);
					stringBuilder.Append(value2);
				}
			}
			if (detail.steps != null && detail.steps.Count > 0)
			{
				int num = 1;
				foreach (string step in detail.steps)
				{
					stringBuilder.Append(value);
					stringBuilder.AppendLine($"  <color={arg}>{num}.</color> {step}");
					stringBuilder.Append(value2);
					num++;
				}
			}
			if (stringBuilder.Length > 0)
			{
				CreateTextElement(parent, sourceText, stringBuilder.ToString().TrimEnd());
			}
			if (!string.IsNullOrEmpty(detail.imageFile))
			{
				CreateInlineImage(parent, detail.imageFile);
			}
			if (!string.IsNullOrEmpty(detail.videoFile))
			{
				CreateInlineVideo(parent, detail.videoFile);
			}
		}

		private static void CreateTextElement(RectTransform parent, TextMeshProUGUI sourceText, string text)
		{
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Expected O, but got Unknown
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0114: Unknown result type (might be due to invalid IL or missing references)
			text = Localization.ParseHelpText(text);
			text = Regex.Replace(text, "\\{HEADER:([^}]+)\\}", delegate(Match match)
			{
				string value = match.Groups[1].Value;
				return "<b><color=#FF7A18>" + value + "</color></b>";
			});
			GameObject val = new GameObject("DetailText");
			val.transform.SetParent((Transform)(object)parent, false);
			TextMeshProUGUI val2 = val.AddComponent<TextMeshProUGUI>();
			((TMP_Text)val2).font = ((TMP_Text)sourceText).font;
			((TMP_Text)val2).fontSharedMaterial = ((TMP_Text)sourceText).fontSharedMaterial;
			((TMP_Text)val2).fontSize = ((TMP_Text)sourceText).fontSize;
			((Graphic)val2).color = ((Graphic)sourceText).color;
			((TMP_Text)val2).enableWordWrapping = true;
			((TMP_Text)val2).overflowMode = (TextOverflowModes)0;
			((TMP_Text)val2).richText = true;
			((TMP_Text)val2).lineSpacing = ((TMP_Text)sourceText).lineSpacing;
			((TMP_Text)val2).margin = new Vector4(5f, 5f, 5f, 5f);
			((TMP_Text)val2).text = text;
			RectTransform component = val.GetComponent<RectTransform>();
			component.anchorMin = new Vector2(0f, 1f);
			component.anchorMax = new Vector2(1f, 1f);
			component.pivot = new Vector2(0.5f, 1f);
			ContentSizeFitter val3 = val.AddComponent<ContentSizeFitter>();
			val3.horizontalFit = (FitMode)0;
			val3.verticalFit = (FitMode)2;
			TocLinkHandler tocLinkHandler = val.AddComponent<TocLinkHandler>();
			tocLinkHandler.TextComponent = val2;
		}

		private static void CreateInlineImage(RectTransform parent, string imageFile)
		{
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Expected O, but got Unknown
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_010e: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				Sprite val = StationpediaAscendedMod.LoadImageFromModFolder(imageFile);
				if ((Object)(object)val == (Object)null)
				{
					ConsoleWindow.Print("[Stationpedia Ascended] Image not found: " + imageFile, ConsoleColor.White, false, true);
					return;
				}
				GameObject val2 = new GameObject("InlineImage");
				val2.transform.SetParent((Transform)(object)parent, false);
				Image val3 = val2.AddComponent<Image>();
				val3.sprite = val;
				val3.preserveAspect = true;
				RectTransform component = val2.GetComponent<RectTransform>();
				component.anchorMin = new Vector2(0f, 1f);
				component.anchorMax = new Vector2(1f, 1f);
				component.pivot = new Vector2(0.5f, 1f);
				float num = 100f;
				float num2 = (float)((Texture)val.texture).width / (float)((Texture)val.texture).height;
				float num3 = num / num2;
				LayoutElement val4 = val2.AddComponent<LayoutElement>();
				val4.preferredWidth = num;
				val4.preferredHeight = num3;
				val4.flexibleWidth = 0f;
				val4.flexibleHeight = 0f;
				component.sizeDelta = new Vector2(num, num3);
			}
			catch (Exception ex)
			{
				ConsoleWindow.Print("[Stationpedia Ascended] Error creating inline image: " + ex.Message, ConsoleColor.White, false, true);
			}
		}

		private static void CreateYouTubeLink(RectTransform parent, TextMeshProUGUI sourceText, string youtubeUrl, string customLabel = null)
		{
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Expected O, but got Unknown
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Expected O, but got Unknown
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e9: Expected O, but got Unknown
			//IL_0132: Unknown result type (might be due to invalid IL or missing references)
			//IL_018e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0193: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_0219: Unknown result type (might be due to invalid IL or missing references)
			//IL_0223: Expected O, but got Unknown
			try
			{
				string text = (string.IsNullOrEmpty(customLabel) ? "Watch on YouTube" : customLabel);
				GameObject val = new GameObject("YouTubeLink");
				val.transform.SetParent((Transform)(object)parent, false);
				Image val2 = val.AddComponent<Image>();
				((Graphic)val2).color = new Color(0.8f, 0.1f, 0.1f, 0.9f);
				Sprite roundedBackgroundSprite = StationpediaAscendedMod.GetRoundedBackgroundSprite();
				if ((Object)(object)roundedBackgroundSprite != (Object)null)
				{
					val2.sprite = roundedBackgroundSprite;
					val2.type = (Type)1;
				}
				HorizontalLayoutGroup val3 = val.AddComponent<HorizontalLayoutGroup>();
				((LayoutGroup)val3).padding = new RectOffset(12, 12, 8, 8);
				((HorizontalOrVerticalLayoutGroup)val3).spacing = 8f;
				((HorizontalOrVerticalLayoutGroup)val3).childForceExpandWidth = false;
				((HorizontalOrVerticalLayoutGroup)val3).childForceExpandHeight = false;
				((HorizontalOrVerticalLayoutGroup)val3).childControlWidth = true;
				((HorizontalOrVerticalLayoutGroup)val3).childControlHeight = true;
				((LayoutGroup)val3).childAlignment = (TextAnchor)3;
				GameObject val4 = new GameObject("LinkText");
				val4.transform.SetParent(val.transform, false);
				TextMeshProUGUI val5 = val4.AddComponent<TextMeshProUGUI>();
				((TMP_Text)val5).font = ((TMP_Text)sourceText).font;
				((TMP_Text)val5).fontSharedMaterial = ((TMP_Text)sourceText).fontSharedMaterial;
				((TMP_Text)val5).fontSize = ((TMP_Text)sourceText).fontSize;
				((Graphic)val5).color = Color.white;
				((TMP_Text)val5).text = text;
				((TMP_Text)val5).enableWordWrapping = false;
				ContentSizeFitter val6 = val4.AddComponent<ContentSizeFitter>();
				val6.horizontalFit = (FitMode)2;
				val6.verticalFit = (FitMode)2;
				ContentSizeFitter val7 = val.AddComponent<ContentSizeFitter>();
				val7.horizontalFit = (FitMode)2;
				val7.verticalFit = (FitMode)2;
				Button val8 = val.AddComponent<Button>();
				ColorBlock colors = ((Selectable)val8).colors;
				((ColorBlock)(ref colors)).normalColor = new Color(0.8f, 0.1f, 0.1f, 0.9f);
				((ColorBlock)(ref colors)).highlightedColor = new Color(1f, 0.2f, 0.2f, 1f);
				((ColorBlock)(ref colors)).pressedColor = new Color(0.6f, 0.05f, 0.05f, 1f);
				((Selectable)val8).colors = colors;
				((Selectable)val8).targetGraphic = (Graphic)(object)val2;
				((UnityEvent)val8.onClick).AddListener((UnityAction)delegate
				{
					Application.OpenURL(youtubeUrl);
				});
			}
			catch (Exception ex)
			{
				ConsoleWindow.Print("[Stationpedia Ascended] Error creating YouTube link: " + ex.Message, ConsoleColor.White, false, true);
			}
		}

		private static void CreateInlineVideo(RectTransform parent, string videoFile)
		{
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Expected O, but got Unknown
			//IL_012c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0133: Expected O, but got Unknown
			//IL_0176: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_022d: Unknown result type (might be due to invalid IL or missing references)
			//IL_023d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0244: Expected O, but got Unknown
			//IL_0285: Unknown result type (might be due to invalid IL or missing references)
			//IL_029c: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_02dd: Expected O, but got Unknown
			//IL_02fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b0: Expected O, but got Unknown
			//IL_03c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_03c9: Expected O, but got Unknown
			//IL_0407: Unknown result type (might be due to invalid IL or missing references)
			//IL_0468: Unknown result type (might be due to invalid IL or missing references)
			//IL_04c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_04d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_04db: Expected O, but got Unknown
			//IL_0512: Unknown result type (might be due to invalid IL or missing references)
			//IL_051f: Unknown result type (might be due to invalid IL or missing references)
			//IL_052c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0539: Unknown result type (might be due to invalid IL or missing references)
			//IL_0579: Unknown result type (might be due to invalid IL or missing references)
			//IL_05bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_05c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_05d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_05f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_061a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0627: Unknown result type (might be due to invalid IL or missing references)
			//IL_0644: Unknown result type (might be due to invalid IL or missing references)
			//IL_064e: Expected O, but got Unknown
			try
			{
				string imageFilePath = StationpediaAscendedMod.GetImageFilePath(videoFile);
				if (string.IsNullOrEmpty(imageFilePath))
				{
					ConsoleWindow.Print("[Stationpedia Ascended] Video file not found: " + videoFile, ConsoleColor.White, false, true);
					return;
				}
				float num = 400f;
				float num2 = 225f;
				float num3 = 6f;
				float num4 = 28f;
				float num5 = 6f;
				float preferredHeight = num2 + num3 * 2f + num5 + num4;
				float num6 = num + num3 * 2f;
				GameObject val = new GameObject("VideoOuterContainer");
				val.transform.SetParent((Transform)(object)parent, false);
				RectTransform component = val.GetComponent<RectTransform>();
				if ((Object)(object)component == (Object)null)
				{
					component = val.AddComponent<RectTransform>();
				}
				LayoutElement val2 = val.AddComponent<LayoutElement>();
				val2.preferredWidth = num6;
				val2.preferredHeight = preferredHeight;
				val2.flexibleWidth = 0f;
				val2.flexibleHeight = 0f;
				VerticalLayoutGroup val3 = val.AddComponent<VerticalLayoutGroup>();
				((HorizontalOrVerticalLayoutGroup)val3).spacing = num5;
				((HorizontalOrVerticalLayoutGroup)val3).childForceExpandWidth = false;
				((HorizontalOrVerticalLayoutGroup)val3).childForceExpandHeight = false;
				((HorizontalOrVerticalLayoutGroup)val3).childControlWidth = false;
				((HorizontalOrVerticalLayoutGroup)val3).childControlHeight = false;
				((LayoutGroup)val3).childAlignment = (TextAnchor)1;
				GameObject val4 = new GameObject("VideoFrameContainer");
				val4.transform.SetParent(val.transform, false);
				RectTransform val5 = val4.GetComponent<RectTransform>();
				if ((Object)(object)val5 == (Object)null)
				{
					val5 = val4.AddComponent<RectTransform>();
				}
				val5.sizeDelta = new Vector2(num6, num2 + num3 * 2f);
				LayoutElement val6 = val4.AddComponent<LayoutElement>();
				val6.preferredWidth = num6;
				val6.preferredHeight = num2 + num3 * 2f;
				Image val7 = val4.AddComponent<Image>();
				if ((Object)(object)_nativePanelSprite != (Object)null)
				{
					val7.sprite = _nativePanelSprite;
					val7.type = _nativePanelType;
					((Graphic)val7).material = _nativePanelMaterial;
				}
				else
				{
					Sprite roundedBackgroundSprite = StationpediaAscendedMod.GetRoundedBackgroundSprite();
					if ((Object)(object)roundedBackgroundSprite != (Object)null)
					{
						val7.sprite = roundedBackgroundSprite;
						val7.type = (Type)1;
					}
				}
				((Graphic)val7).color = new Color(0.02f, 0.04f, 0.08f, 1f);
				GameObject val8 = new GameObject("VideoContainer");
				val8.transform.SetParent(val4.transform, false);
				RectTransform val9 = val8.GetComponent<RectTransform>();
				if ((Object)(object)val9 == (Object)null)
				{
					val9 = val8.AddComponent<RectTransform>();
				}
				val9.anchorMin = new Vector2(0.5f, 0.5f);
				val9.anchorMax = new Vector2(0.5f, 0.5f);
				val9.pivot = new Vector2(0.5f, 0.5f);
				val9.sizeDelta = new Vector2(num, num2);
				RenderTexture val10 = new RenderTexture((int)num * 2, (int)num2 * 2, 0);
				val10.Create();
				RawImage val11 = val8.AddComponent<RawImage>();
				val11.texture = (Texture)(object)val10;
				((Graphic)val11).color = Color.white;
				VideoPlayer videoPlayer = val8.AddComponent<VideoPlayer>();
				videoPlayer.playOnAwake = false;
				videoPlayer.isLooping = true;
				videoPlayer.renderMode = (VideoRenderMode)2;
				videoPlayer.targetTexture = val10;
				videoPlayer.audioOutputMode = (VideoAudioOutputMode)2;
				videoPlayer.url = "file://" + imageFilePath.Replace("\\", "/");
				videoPlayer.skipOnDrop = true;
				videoPlayer.SetDirectAudioVolume((ushort)0, 0.5f);
				videoPlayer.prepareCompleted += (EventHandler)delegate
				{
					videoPlayer.frame = 0L;
				};
				videoPlayer.Prepare();
				GameObject val12 = new GameObject("PlayButton");
				val12.transform.SetParent(val.transform, false);
				RectTransform val13 = val12.GetComponent<RectTransform>();
				if ((Object)(object)val13 == (Object)null)
				{
					val13 = val12.AddComponent<RectTransform>();
				}
				val13.sizeDelta = new Vector2(80f, num4);
				LayoutElement val14 = val12.AddComponent<LayoutElement>();
				val14.preferredWidth = 80f;
				val14.preferredHeight = num4;
				Image playButtonBg = val12.AddComponent<Image>();
				if ((Object)(object)_nativePanelSprite != (Object)null)
				{
					playButtonBg.sprite = _nativePanelSprite;
					playButtonBg.type = _nativePanelType;
				}
				else
				{
					Sprite roundedBackgroundSprite2 = StationpediaAscendedMod.GetRoundedBackgroundSprite();
					if ((Object)(object)roundedBackgroundSprite2 != (Object)null)
					{
						playButtonBg.sprite = roundedBackgroundSprite2;
						playButtonBg.type = (Type)1;
					}
				}
				((Graphic)playButtonBg).color = new Color(0.1f, 0.2f, 0.35f, 1f);
				GameObject val15 = new GameObject("PlaySymbol");
				val15.transform.SetParent(val12.transform, false);
				RectTransform val16 = val15.GetComponent<RectTransform>();
				if ((Object)(object)val16 == (Object)null)
				{
					val16 = val15.AddComponent<RectTransform>();
				}
				val16.anchorMin = Vector2.zero;
				val16.anchorMax = Vector2.one;
				val16.offsetMin = Vector2.zero;
				val16.offsetMax = Vector2.zero;
				TextMeshProUGUI playText = val15.AddComponent<TextMeshProUGUI>();
				((TMP_Text)playText).text = "PLAY";
				((TMP_Text)playText).fontSize = 14f;
				((Graphic)playText).color = Color.white;
				((TMP_Text)playText).alignment = (TextAlignmentOptions)514;
				((TMP_Text)playText).enableWordWrapping = false;
				Button val17 = val12.AddComponent<Button>();
				((Selectable)val17).targetGraphic = (Graphic)(object)playButtonBg;
				ColorBlock colors = ((Selectable)val17).colors;
				((ColorBlock)(ref colors)).normalColor = new Color(0.1f, 0.2f, 0.35f, 1f);
				((ColorBlock)(ref colors)).highlightedColor = new Color(0.15f, 0.3f, 0.5f, 1f);
				((ColorBlock)(ref colors)).pressedColor = new Color(0.05f, 0.15f, 0.25f, 1f);
				((Selectable)val17).colors = colors;
				bool isPlaying = false;
				((UnityEvent)val17.onClick).AddListener((UnityAction)delegate
				{
					//IL_0090: Unknown result type (might be due to invalid IL or missing references)
					//IL_0043: Unknown result type (might be due to invalid IL or missing references)
					if (isPlaying)
					{
						videoPlayer.Pause();
						((TMP_Text)playText).text = "PLAY";
						((Graphic)playButtonBg).color = new Color(0.1f, 0.2f, 0.35f, 1f);
						isPlaying = false;
					}
					else
					{
						videoPlayer.Play();
						((TMP_Text)playText).text = "STOP";
						((Graphic)playButtonBg).color = new Color(0.1f, 0.4f, 0.2f, 1f);
						isPlaying = true;
					}
				});
				ConsoleWindow.Print("[Stationpedia Ascended] Video player created for: " + videoFile, ConsoleColor.White, false, true);
			}
			catch (Exception ex)
			{
				ConsoleWindow.Print("[Stationpedia Ascended] Error creating inline video: " + ex.Message, ConsoleColor.White, false, true);
			}
		}

		private static bool LoadTablePrefabs()
		{
			try
			{
				string[] array = new string[3]
				{
					Path.Combine(Path.GetDirectoryName(typeof(HarmonyPatches).Assembly.Location), "stationpediaascended_table"),
					Path.Combine(Paths.BepInExRootPath, "scripts", "stationpediaascended_table"),
					Path.Combine(Paths.PluginPath, "StationpediaAscended", "stationpediaascended_table")
				};
				string text = null;
				string[] array2 = array;
				foreach (string text2 in array2)
				{
					if (File.Exists(text2))
					{
						text = text2;
						break;
					}
				}
				if (string.IsNullOrEmpty(text))
				{
					ConsoleWindow.Print("[Stationpedia Ascended] Table bundle not found. Searched paths:", ConsoleColor.White, false, true);
					string[] array3 = array;
					foreach (string text3 in array3)
					{
						ConsoleWindow.Print("  - " + text3, ConsoleColor.White, false, true);
					}
					return false;
				}
				ConsoleWindow.Print("[Stationpedia Ascended] Loading table bundle from: " + text, ConsoleColor.White, false, true);
				IEnumerable<AssetBundle> allLoadedAssetBundles = AssetBundle.GetAllLoadedAssetBundles();
				foreach (AssetBundle item in allLoadedAssetBundles)
				{
					if (((Object)item).name.Contains("table"))
					{
						ConsoleWindow.Print("[Stationpedia Ascended] UNLOADING cached bundle: " + ((Object)item).name, ConsoleColor.White, false, true);
						item.Unload(true);
						break;
					}
				}
				_tableBundle = null;
				_tableContainerPrefab = null;
				_tableRowPrefab = null;
				_tableHeaderRowPrefab = null;
				_tableCellPrefab = null;
				_tableSeparatorPrefab = null;
				_tableBundle = AssetBundle.LoadFromFile(text);
				if ((Object)(object)_tableBundle == (Object)null)
				{
					ConsoleWindow.Print("[Stationpedia Ascended] Failed to load table bundle", ConsoleColor.White, false, true);
					return false;
				}
				ConsoleWindow.Print("[Stationpedia Ascended] Table bundle contents:", ConsoleColor.White, false, true);
				string[] allAssetNames = _tableBundle.GetAllAssetNames();
				foreach (string text4 in allAssetNames)
				{
					ConsoleWindow.Print("  - " + text4, ConsoleColor.White, false, true);
				}
				_tableContainerPrefab = _tableBundle.LoadAsset<GameObject>("assets/prefabs/tables/tablecontainer.prefab");
				_tableRowPrefab = _tableBundle.LoadAsset<GameObject>("assets/prefabs/tables/tablerow.prefab");
				_tableHeaderRowPrefab = _tableBundle.LoadAsset<GameObject>("assets/prefabs/tables/tableheaderrow.prefab");
				_tableCellPrefab = _tableBundle.LoadAsset<GameObject>("assets/prefabs/tables/tablecell.prefab");
				_tableSeparatorPrefab = _tableBundle.LoadAsset<GameObject>("assets/prefabs/tables/tableseparator.prefab");
				bool flag = (Object)(object)_tableContainerPrefab != (Object)null && (Object)(object)_tableRowPrefab != (Object)null && (Object)(object)_tableCellPrefab != (Object)null;
				if (flag)
				{
					ConsoleWindow.Print("[Stationpedia Ascended] Table prefabs loaded successfully", ConsoleColor.White, false, true);
				}
				else
				{
					ConsoleWindow.Print($"[Stationpedia Ascended] Some prefabs failed to load: Container={(Object)(object)_tableContainerPrefab != (Object)null}, Row={(Object)(object)_tableRowPrefab != (Object)null}, Cell={(Object)(object)_tableCellPrefab != (Object)null}", ConsoleColor.White, false, true);
				}
				return flag;
			}
			catch (Exception ex)
			{
				ConsoleWindow.Print("[Stationpedia Ascended] Error loading table prefabs: " + ex.Message, ConsoleColor.White, false, true);
				return false;
			}
		}

		private static void CreateTableElement(RectTransform parent, TextMeshProUGUI sourceText, List<TableRow> tableData)
		{
			//IL_0189: Unknown result type (might be due to invalid IL or missing references)
			//IL_0182: Unknown result type (might be due to invalid IL or missing references)
			//IL_018e: Unknown result type (might be due to invalid IL or missing references)
			//IL_01df: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				if (tableData == null || tableData.Count == 0)
				{
					return;
				}
				int num = tableData[0].cells?.Count ?? 0;
				if (num == 0)
				{
					return;
				}
				float[] array = new float[num];
				float num2 = 700f;
				float num3 = 12f;
				float num4 = 4f;
				float num5 = 8f;
				foreach (TableRow tableDatum in tableData)
				{
					if (tableDatum?.cells == null)
					{
						continue;
					}
					for (int i = 0; i < num && i < tableDatum.cells.Count; i++)
					{
						float num6 = (float)(tableDatum.cells[i]?.Length ?? 0) * num5 + num3;
						if (num6 > array[i])
						{
							array[i] = num6;
						}
					}
				}
				float num7 = 60f;
				float num8 = (num2 - (float)(num - 1) * num4) / (float)num;
				for (int j = 0; j < array.Length; j++)
				{
					if (array[j] < num7)
					{
						array[j] = num7;
					}
					if (array[j] > num8)
					{
						array[j] = num8;
					}
				}
				Color headerColor = (Color)(VanillaModeManager.IsVanillaMode ? Color.white : new Color(1f, 0.65f, 0f));
				Color separatorColor = default(Color);
				((Color)(ref separatorColor))..ctor(0.4f, 0.4f, 0.4f, 1f);
				if (LoadTablePrefabs() && (Object)(object)_tableContainerPrefab != (Object)null)
				{
					CreateTableFromPrefabs(parent, sourceText, tableData, num, array, headerColor, separatorColor);
				}
				else
				{
					CreateTableProgrammatically(parent, sourceText, tableData, num, array, headerColor, Color.clear, separatorColor, Color.clear, num4, num2);
				}
			}
			catch (Exception ex)
			{
				ConsoleWindow.Print("[Stationpedia Ascended] Error creating table: " + ex.Message, ConsoleColor.White, false, true);
			}
		}

		private static void CreateTableFromPrefabs(RectTransform parent, TextMeshProUGUI sourceText, List<TableRow> tableData, int columnCount, float[] columnWidths, Color headerColor, Color separatorColor)
		{
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
			TextMeshProUGUI componentInChildren = _tableCellPrefab.GetComponentInChildren<TextMeshProUGUI>();
			if ((Object)(object)componentInChildren != (Object)null)
			{
				ManualLogSource log = StationpediaAscendedMod.Log;
				if (log != null)
				{
					log.LogInfo((object)$"[PREFAB DEBUG] Cell prefab text color: {((Graphic)componentInChildren).color}");
				}
			}
			else
			{
				ManualLogSource log2 = StationpediaAscendedMod.Log;
				if (log2 != null)
				{
					log2.LogInfo((object)"[PREFAB DEBUG] Cell prefab has NO TextMeshProUGUI!");
				}
			}
			GameObject val = Object.Instantiate<GameObject>(_tableContainerPrefab);
			((Object)val).name = "TableContainer";
			val.transform.SetParent((Transform)(object)parent, false);
			for (int i = 0; i < tableData.Count; i++)
			{
				TableRow tableRow = tableData[i];
				if (tableRow?.cells == null)
				{
					continue;
				}
				bool flag = i == 0;
				GameObject val2 = (flag ? _tableHeaderRowPrefab : _tableRowPrefab);
				if ((Object)(object)val2 == (Object)null)
				{
					val2 = _tableRowPrefab;
				}
				GameObject val3 = Object.Instantiate<GameObject>(val2);
				((Object)val3).name = (flag ? "TableHeaderRow" : $"TableRow_{i}");
				val3.transform.SetParent(val.transform, false);
				for (int j = 0; j < columnCount; j++)
				{
					string text = ((j < tableRow.cells.Count) ? tableRow.cells[j] : "") ?? "";
					GameObject val4 = Object.Instantiate<GameObject>(_tableCellPrefab);
					((Object)val4).name = $"TableCell_{j}";
					val4.transform.SetParent(val3.transform, false);
					TextMeshProUGUI componentInChildren2 = val4.GetComponentInChildren<TextMeshProUGUI>();
					if (!((Object)(object)componentInChildren2 != (Object)null))
					{
						continue;
					}
					if (i == 0 && j == 0)
					{
						ManualLogSource log3 = StationpediaAscendedMod.Log;
						if (log3 != null)
						{
							log3.LogInfo((object)$"[PREFAB DEBUG] Instantiated cell text color: {((Graphic)componentInChildren2).color}");
						}
					}
					((TMP_Text)componentInChildren2).text = (flag ? ("<b>" + text + "</b>") : text);
				}
				if (flag && (Object)(object)_tableSeparatorPrefab != (Object)null)
				{
					GameObject val5 = Object.Instantiate<GameObject>(_tableSeparatorPrefab);
					((Object)val5).name = "TableHeaderSeparator";
					val5.transform.SetParent(val.transform, false);
				}
			}
		}

		private static void CreateTableProgrammatically(RectTransform parent, TextMeshProUGUI sourceText, List<TableRow> tableData, int columnCount, float[] columnWidths, Color headerColor, Color headerBgColor, Color separatorColor, Color cellBgColor, float cellSpacing, float maxTotalWidth)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Expected O, but got Unknown
			//IL_017d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0184: Expected O, but got Unknown
			//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ba: Expected O, but got Unknown
			//IL_040b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0412: Expected O, but got Unknown
			//IL_043a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0257: Unknown result type (might be due to invalid IL or missing references)
			//IL_025e: Expected O, but got Unknown
			//IL_0287: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_02db: Expected O, but got Unknown
			//IL_02fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0308: Unknown result type (might be due to invalid IL or missing references)
			//IL_031f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0336: Unknown result type (might be due to invalid IL or missing references)
			//IL_03d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_03c0: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = new GameObject("TableContainer");
			val.transform.SetParent((Transform)(object)parent, false);
			RectTransform val2 = val.AddComponent<RectTransform>();
			val2.anchorMin = new Vector2(0f, 1f);
			val2.anchorMax = new Vector2(1f, 1f);
			val2.pivot = new Vector2(0f, 1f);
			Image val3 = val.AddComponent<Image>();
			((Graphic)val3).color = Color.clear;
			VerticalLayoutGroup val4 = val.AddComponent<VerticalLayoutGroup>();
			((LayoutGroup)val4).padding = new RectOffset(8, 8, 6, 6);
			((HorizontalOrVerticalLayoutGroup)val4).spacing = 3f;
			((LayoutGroup)val4).childAlignment = (TextAnchor)0;
			((HorizontalOrVerticalLayoutGroup)val4).childForceExpandWidth = false;
			((HorizontalOrVerticalLayoutGroup)val4).childForceExpandHeight = false;
			((HorizontalOrVerticalLayoutGroup)val4).childControlWidth = true;
			((HorizontalOrVerticalLayoutGroup)val4).childControlHeight = true;
			ContentSizeFitter val5 = val.AddComponent<ContentSizeFitter>();
			val5.horizontalFit = (FitMode)2;
			val5.verticalFit = (FitMode)2;
			LayoutElement val6 = val.AddComponent<LayoutElement>();
			val6.preferredWidth = maxTotalWidth;
			float num = 0f;
			for (int i = 0; i < columnWidths.Length; i++)
			{
				num += columnWidths[i];
			}
			num += (float)(columnCount - 1) * cellSpacing + 12f;
			for (int j = 0; j < tableData.Count; j++)
			{
				TableRow tableRow = tableData[j];
				if (tableRow?.cells == null)
				{
					continue;
				}
				bool flag = j == 0;
				GameObject val7 = new GameObject(flag ? "HeaderRow" : $"Row{j}");
				val7.transform.SetParent(val.transform, false);
				RectTransform val8 = val7.AddComponent<RectTransform>();
				HorizontalLayoutGroup val9 = val7.AddComponent<HorizontalLayoutGroup>();
				((LayoutGroup)val9).padding = new RectOffset(6, 6, 2, 2);
				((HorizontalOrVerticalLayoutGroup)val9).spacing = cellSpacing;
				((LayoutGroup)val9).childAlignment = (TextAnchor)3;
				((HorizontalOrVerticalLayoutGroup)val9).childForceExpandWidth = false;
				((HorizontalOrVerticalLayoutGroup)val9).childForceExpandHeight = false;
				((HorizontalOrVerticalLayoutGroup)val9).childControlWidth = true;
				((HorizontalOrVerticalLayoutGroup)val9).childControlHeight = true;
				ContentSizeFitter val10 = val7.AddComponent<ContentSizeFitter>();
				val10.horizontalFit = (FitMode)2;
				val10.verticalFit = (FitMode)2;
				for (int k = 0; k < columnCount; k++)
				{
					string text = ((k < tableRow.cells.Count) ? tableRow.cells[k] : "") ?? "";
					GameObject val11 = new GameObject($"Cell{k}");
					val11.transform.SetParent(val7.transform, false);
					RectTransform val12 = val11.AddComponent<RectTransform>();
					Image val13 = val11.AddComponent<Image>();
					((Graphic)val13).color = Color.clear;
					LayoutElement val14 = val11.AddComponent<LayoutElement>();
					val14.minWidth = 50f;
					val14.preferredWidth = columnWidths[k];
					val14.flexibleWidth = 0f;
					val14.minHeight = 20f;
					GameObject val15 = new GameObject("Text");
					val15.transform.SetParent(val11.transform, false);
					RectTransform val16 = val15.AddComponent<RectTransform>();
					val16.anchorMin = Vector2.zero;
					val16.anchorMax = Vector2.one;
					val16.offsetMin = new Vector2(6f, 3f);
					val16.offsetMax = new Vector2(-6f, -3f);
					TextMeshProUGUI val17 = val15.AddComponent<TextMeshProUGUI>();
					((TMP_Text)val17).font = ((TMP_Text)sourceText).font;
					((TMP_Text)val17).fontSharedMaterial = ((TMP_Text)sourceText).fontSharedMaterial;
					((TMP_Text)val17).fontSize = ((TMP_Text)sourceText).fontSize;
					((TMP_Text)val17).enableWordWrapping = true;
					((TMP_Text)val17).overflowMode = (TextOverflowModes)0;
					((TMP_Text)val17).alignment = (TextAlignmentOptions)513;
					((TMP_Text)val17).richText = true;
					if (flag)
					{
						((TMP_Text)val17).text = "<b>" + text + "</b>";
						((Graphic)val17).color = headerColor;
					}
					else
					{
						((TMP_Text)val17).text = text;
						((Graphic)val17).color = ((Graphic)sourceText).color;
					}
				}
				if (flag)
				{
					GameObject val18 = new GameObject("Separator");
					val18.transform.SetParent(val.transform, false);
					RectTransform val19 = val18.AddComponent<RectTransform>();
					Image val20 = val18.AddComponent<Image>();
					((Graphic)val20).color = separatorColor;
					LayoutElement val21 = val18.AddComponent<LayoutElement>();
					val21.minHeight = 1f;
					val21.preferredHeight = 1f;
					val21.flexibleHeight = 0f;
					val21.preferredWidth = num;
					val21.flexibleWidth = 0f;
				}
			}
		}

		private static void AddCategoryIcon(StationpediaCategory category)
		{
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Expected O, but got Unknown
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				if ((Object)(object)StationpediaAscendedMod._customIconSprite != (Object)null)
				{
					Transform parent = ((TMP_Text)category.Title).transform.parent;
					if ((Object)(object)parent != (Object)null)
					{
						GameObject val = new GameObject("OperationalDetailsIcon");
						val.transform.SetParent(parent, false);
						val.transform.SetAsFirstSibling();
						Image val2 = val.AddComponent<Image>();
						val2.sprite = StationpediaAscendedMod._customIconSprite;
						val2.preserveAspect = true;
						RectTransform component = val.GetComponent<RectTransform>();
						component.sizeDelta = new Vector2(20f, 20f);
						LayoutElement val3 = val.AddComponent<LayoutElement>();
						val3.preferredWidth = 20f;
						val3.preferredHeight = 20f;
						val3.minWidth = 20f;
						val3.minHeight = 20f;
					}
				}
			}
			catch (Exception ex)
			{
				ConsoleWindow.Print("[Stationpedia Ascended] Could not add icon to category: " + ex.Message, ConsoleColor.White, false, true);
			}
		}

		private static void ConfigureCategoryLayout(StationpediaCategory category, UniversalPage page, DeviceDescriptions deviceDesc = null)
		{
			//IL_0215: Unknown result type (might be due to invalid IL or missing references)
			//IL_021f: Expected O, but got Unknown
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_0140: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0196: Unknown result type (might be due to invalid IL or missing references)
			//IL_0189: Unknown result type (might be due to invalid IL or missing references)
			GridLayoutGroup component = ((Component)category.Contents).GetComponent<GridLayoutGroup>();
			if ((Object)(object)component != (Object)null)
			{
				Object.DestroyImmediate((Object)(object)component);
			}
			Image val = ((Component)category.Contents).gameObject.GetComponent<Image>();
			StationpediaCategory logicSlotContents = page.LogicSlotContents;
			object obj;
			if (logicSlotContents == null)
			{
				obj = null;
			}
			else
			{
				RectTransform contents = logicSlotContents.Contents;
				obj = ((contents != null) ? ((Component)contents).GetComponent<Image>() : null);
			}
			Image val2 = (Image)obj;
			if ((Object)(object)val2 != (Object)null)
			{
				if ((Object)(object)_nativePanelSprite == (Object)null)
				{
					_nativePanelSprite = val2.sprite;
					_nativePanelMaterial = ((Graphic)val2).material;
					_nativePanelType = val2.type;
				}
				if ((Object)(object)val == (Object)null)
				{
					val = ((Component)category.Contents).gameObject.AddComponent<Image>();
				}
				if (VanillaModeManager.IsVanillaMode)
				{
					val.sprite = val2.sprite;
					val.type = val2.type;
					((Graphic)val).material = ((Graphic)val2).material;
					((Graphic)val).color = Color.white;
				}
				else
				{
					Sprite windowBackgroundSprite = StationpediaAscendedMod.GetWindowBackgroundSprite();
					if ((Object)(object)windowBackgroundSprite != (Object)null)
					{
						val.sprite = windowBackgroundSprite;
						val.type = (Type)1;
						((Graphic)val).material = null;
					}
					else
					{
						val.sprite = val2.sprite;
						val.type = val2.type;
						((Graphic)val).material = ((Graphic)val2).material;
					}
					if (deviceDesc != null && !string.IsNullOrEmpty(deviceDesc.operationalDetailsBackgroundColor))
					{
						Color color = default(Color);
						if (ColorUtility.TryParseHtmlString(deviceDesc.operationalDetailsBackgroundColor, ref color))
						{
							((Graphic)val).color = color;
						}
						else
						{
							((Graphic)val).color = StationeersBlue;
						}
					}
					else
					{
						((Graphic)val).color = StationeersBlue;
					}
				}
			}
			VerticalLayoutGroup component2 = ((Component)category.Contents).GetComponent<VerticalLayoutGroup>();
			if ((Object)(object)component2 == (Object)null)
			{
				component2 = ((Component)category.Contents).gameObject.AddComponent<VerticalLayoutGroup>();
				((HorizontalOrVerticalLayoutGroup)component2).childForceExpandWidth = true;
				((HorizontalOrVerticalLayoutGroup)component2).childForceExpandHeight = false;
				((HorizontalOrVerticalLayoutGroup)component2).childControlWidth = true;
				((HorizontalOrVerticalLayoutGroup)component2).childControlHeight = true;
				((HorizontalOrVerticalLayoutGroup)component2).spacing = 5f;
				((LayoutGroup)component2).padding = new RectOffset(10, 10, 10, 10);
			}
			ContentSizeFitter component3 = ((Component)category.Contents).GetComponent<ContentSizeFitter>();
			if ((Object)(object)component3 == (Object)null)
			{
				component3 = ((Component)category.Contents).gameObject.AddComponent<ContentSizeFitter>();
				component3.verticalFit = (FitMode)2;
				component3.horizontalFit = (FitMode)0;
			}
			((Component)category.Contents).gameObject.SetActive(true);
			LayoutRebuilder.ForceRebuildLayoutImmediate(category.Contents);
		}

		private static void StartDelayedScrollbarFix()
		{
			try
			{
				if ((Object)(object)StationpediaAscendedMod.Instance != (Object)null)
				{
					((MonoBehaviour)StationpediaAscendedMod.Instance).StartCoroutine(DelayedScrollbarFix());
				}
			}
			catch
			{
			}
		}

		private static IEnumerator DelayedScrollbarFix()
		{
			yield return (object)new WaitForEndOfFrame();
			yield return null;
			yield return null;
			yield return null;
			Stationpedia stationpedia = Stationpedia.Instance;
			if ((Object)(object)stationpedia == (Object)null)
			{
				yield break;
			}
			ScrollRect scrollRect = ((Component)stationpedia).GetComponentInChildren<ScrollRect>();
			if ((Object)(object)scrollRect == (Object)null)
			{
				yield break;
			}
			if (!_scrollbarVisibilityFixed)
			{
				scrollRect.verticalScrollbarVisibility = (ScrollbarVisibility)0;
				_scrollbarVisibilityFixed = true;
			}
			Scrollbar scrollbar = scrollRect.verticalScrollbar;
			if (!((Object)(object)scrollbar == (Object)null) && !((Object)(object)scrollbar.handleRect == (Object)null))
			{
				for (int i = 0; i < 5; i++)
				{
					FixHandleLocalPosition(scrollbar.handleRect, i == 0);
					yield return null;
				}
			}
		}

		private static void FixHandleLocalPosition(RectTransform handleRect, bool logFirst)
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)handleRect == (Object)null)
			{
				return;
			}
			try
			{
				Vector3 localPosition = ((Transform)handleRect).localPosition;
				if (float.IsNaN(localPosition.y) || float.IsNaN(localPosition.x) || Mathf.Abs(localPosition.x) > 0.01f || Mathf.Abs(localPosition.y) > 0.01f || Mathf.Abs(localPosition.z) > 0.01f)
				{
					((Transform)handleRect).localPosition = Vector3.zero;
					handleRect.anchoredPosition = Vector2.zero;
				}
			}
			catch
			{
			}
		}

		public static void ResetScrollbarState()
		{
			_scrollbarVisibilityFixed = false;
		}

		private static bool IsSlotNumberList(string text)
		{
			foreach (char c in text)
			{
				if (!char.IsDigit(c) && c != ',' && c != ' ')
				{
					return false;
				}
			}
			return true;
		}

		private static string CondenseSlotNumbers(string input)
		{
			string[] array = input.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			List<int> list = new List<int>();
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (int.TryParse(text.Trim(), out var result))
				{
					list.Add(result);
				}
			}
			if (list.Count <= 5)
			{
				return input;
			}
			list.Sort();
			List<string> list2 = new List<string>();
			int start = list[0];
			int num = list[0];
			for (int j = 1; j < list.Count; j++)
			{
				if (list[j] == num + 1)
				{
					num = list[j];
					continue;
				}
				list2.Add(FormatRange(start, num));
				start = list[j];
				num = list[j];
			}
			list2.Add(FormatRange(start, num));
			return string.Join(", ", list2);
		}

		private static string FormatRange(int start, int end)
		{
			if (start == end)
			{
				return start.ToString();
			}
			if (end == start + 1)
			{
				return $"{start}, {end}";
			}
			return $"{start}-{end}";
		}

		public static bool Stationpedia_SetPage_Prefix(Stationpedia __instance, string key, bool newPage)
		{
			ManualLogSource log = StationpediaAscendedMod.Log;
			if (log != null)
			{
				log.LogInfo((object)$"SetPage called: key={key}, newPage={newPage}, CurrentPageKey={Stationpedia.CurrentPageKey}");
			}
			if (key == "GameMechanics")
			{
				try
				{
					SetPageGameMechanics(__instance, newPage);
					return false;
				}
				catch (Exception ex)
				{
					ConsoleWindow.Print("[Stationpedia Ascended] Error in SetPageGameMechanics: " + ex.Message, ConsoleColor.White, false, true);
				}
			}
			return true;
		}

		private static void SetPageGameMechanics(Stationpedia stationpedia, bool newPage)
		{
			if (newPage && Stationpedia.CurrentPageKey != "GameMechanics")
			{
				FieldInfo field = typeof(Stationpedia).GetField("_pageHistory", BindingFlags.Static | BindingFlags.NonPublic);
				FieldInfo field2 = typeof(Stationpedia).GetField("_currentHistoryIndex", BindingFlags.Instance | BindingFlags.NonPublic);
				if (field != null && field2 != null)
				{
					List<string> list = field.GetValue(null) as List<string>;
					int num = (int)field2.GetValue(stationpedia);
					if (list != null)
					{
						if (num < list.Count - 1)
						{
							int count = list.Count - 1 - num;
							list.RemoveRange(num + 1, count);
						}
						list.Add("GameMechanics");
						field2.SetValue(stationpedia, list.Count - 1);
					}
				}
				Stationpedia.CurrentPageKey = "GameMechanics";
			}
			stationpedia.HomePage.SetActive(false);
			((Component)stationpedia.UniversalPageRef).gameObject.SetActive(false);
			stationpedia.LoreGuideHolder.gameObject.SetActive(true);
			PopulateGameMechanicsContents(stationpedia);
			((TMP_Text)stationpedia.LoreGuideTitle).SetText("Game Mechanics", true);
			((Selectable)stationpedia.HomePageButton).interactable = true;
			Stationpedia.UpdateNavigationInteractButtons();
		}

		private static void PopulateGameMechanicsContents(Stationpedia stationpedia)
		{
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Expected O, but got Unknown
			//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cb: Expected O, but got Unknown
			if (typeof(Stationpedia).GetField("_SPDAGuideLoreInserts", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(stationpedia) is IList list)
			{
				list.Clear();
			}
			foreach (Transform item in (Transform)stationpedia.LoreGuideContents)
			{
				Transform val = item;
				Object.Destroy((Object)(object)((Component)val).gameObject);
			}
			JsonMechanicsLoader.RegisterMechanicsPages();
			List<GuideDescription> allMechanics = JsonMechanicsLoader.GetAllMechanics();
			if (allMechanics == null || allMechanics.Count == 0)
			{
				ConsoleWindow.Print("[Stationpedia Ascended] No game mechanics pages to display", ConsoleColor.White, false, true);
				return;
			}
			foreach (GuideDescription item2 in allMechanics)
			{
				string pageKey = item2.guideKey;
				if (!Stationpedia.StationpediaPages.Exists((StationpediaPage p) => p.Key == pageKey))
				{
					continue;
				}
				StationpediaPage val2 = Stationpedia.StationpediaPages.Find((StationpediaPage p) => p.Key == pageKey);
				if (val2 != null)
				{
					SPDAListItem val3 = Object.Instantiate<SPDAListItem>(stationpedia.ListSearchPrefab, (Transform)(object)stationpedia.LoreGuideContents);
					val3.Apply(item2.displayName ?? val2.Title);
					string capturedKey = pageKey;
					((UnityEvent)val3.InsertsButton.onClick).AddListener((UnityAction)delegate
					{
						stationpedia.SetPage(capturedKey, true);
					});
					if ((Object)(object)val2.CustomSpriteToUse != (Object)null)
					{
						val3.InsertImage.sprite = val2.CustomSpriteToUse;
					}
					else
					{
						val3.InsertImage.sprite = stationpedia.ImportantSearchImage;
					}
					val3.SetSpecial();
				}
			}
			HomePageLayoutManager.ModifyGuideLoreLayout(stationpedia.LoreGuideContents);
		}

		public static void Stationpedia_SetPageGuides_Postfix(Stationpedia __instance)
		{
			try
			{
				CreateJsonGuideButtons(__instance);
				AddVanillaGuidesHeader(__instance);
				HomePageLayoutManager.ModifyGuideLoreLayout(__instance.LoreGuideContents);
			}
			catch (Exception ex)
			{
				ConsoleWindow.Print("[Stationpedia Ascended] Error modifying guide layout: " + ex.Message, ConsoleColor.White, false, true);
			}
		}

		private static void CreateJsonGuideButtons(Stationpedia stationpedia)
		{
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Expected O, but got Unknown
			try
			{
				List<GuideDescription> allGuides = JsonGuideLoader.GetAllGuides();
				if (allGuides == null || allGuides.Count == 0)
				{
					return;
				}
				JsonGuideLoader.RegisterGuidePages();
				int num = 0;
				foreach (GuideDescription item in allGuides)
				{
					if (item.guideKey == "SurvivalManual")
					{
						continue;
					}
					string text = "CustomGuide_" + item.guideKey;
					bool flag = false;
					foreach (Transform item2 in (Transform)stationpedia.LoreGuideContents)
					{
						Transform val = item2;
						if (((Object)((Component)val).gameObject).name == text)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						CreateGuideButton(stationpedia, item);
						num++;
					}
				}
				if (num > 0)
				{
					ConsoleWindow.Print($"[Stationpedia Ascended] Created {num} JSON guide buttons", ConsoleColor.White, false, true);
				}
			}
			catch (Exception ex)
			{
				ConsoleWindow.Print("[Stationpedia Ascended] Error creating JSON guide buttons: " + ex.Message, ConsoleColor.White, false, true);
			}
		}

		private static void CreateGuideButton(Stationpedia stationpedia, GuideDescription guide)
		{
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0114: Expected O, but got Unknown
			try
			{
				if ((Object)(object)stationpedia?.ListSearchPrefab == (Object)null || (Object)(object)stationpedia.LoreGuideContents == (Object)null)
				{
					return;
				}
				SPDAListItem val = Object.Instantiate<SPDAListItem>(stationpedia.ListSearchPrefab, (Transform)(object)stationpedia.LoreGuideContents);
				if ((Object)(object)val == (Object)null)
				{
					return;
				}
				((Object)((Component)val).gameObject).name = "CustomGuide_" + guide.guideKey;
				val.Apply(guide.displayName ?? guide.guideKey);
				string guideKey = guide.guideKey;
				if ((Object)(object)val.InsertsButton != (Object)null)
				{
					((UnityEventBase)val.InsertsButton.onClick).RemoveAllListeners();
					((UnityEvent)val.InsertsButton.onClick).AddListener((UnityAction)delegate
					{
						stationpedia.SetPage(guideKey, true);
					});
				}
				val.SetSpecial();
				if ((Object)(object)val.InsertImage != (Object)null && (Object)(object)stationpedia.ImportantSearchImage != (Object)null)
				{
					val.InsertImage.sprite = stationpedia.ImportantSearchImage;
					((Component)val.InsertImage).gameObject.SetActive(true);
				}
				((Component)val).transform.SetAsFirstSibling();
			}
			catch (Exception ex)
			{
				ConsoleWindow.Print("[Stationpedia Ascended] Error creating guide button for " + guide?.guideKey + ": " + ex.Message, ConsoleColor.White, false, true);
			}
		}

		private static void AddVanillaGuidesHeader(Stationpedia stationpedia)
		{
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Expected O, but got Unknown
			//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f7: Expected O, but got Unknown
			//IL_011b: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				if ((Object)(object)stationpedia == (Object)null || (Object)(object)stationpedia.LoreGuideContents == (Object)null)
				{
					return;
				}
				RectTransform loreGuideContents = stationpedia.LoreGuideContents;
				foreach (Transform item in (Transform)loreGuideContents)
				{
					Transform val = item;
					if (((Object)((Component)val).gameObject).name == "VanillaGuidesHeader")
					{
						return;
					}
				}
				int num = 0;
				for (int i = 0; i < ((Transform)loreGuideContents).childCount; i++)
				{
					Transform child = ((Transform)loreGuideContents).GetChild(i);
					if (((Object)((Component)child).gameObject).name.StartsWith("CustomGuide_"))
					{
						num++;
					}
				}
				int num2 = ((Transform)loreGuideContents).childCount - num;
				if (num2 > 0)
				{
					GameObject val2 = new GameObject("VanillaGuidesHeader");
					val2.transform.SetParent((Transform)(object)loreGuideContents, false);
					RectTransform val3 = val2.AddComponent<RectTransform>();
					val3.sizeDelta = new Vector2(0f, 40f);
					LayoutElement val4 = val2.AddComponent<LayoutElement>();
					val4.preferredHeight = 40f;
					val4.flexibleWidth = 1f;
					TextMeshProUGUI val5 = val2.AddComponent<TextMeshProUGUI>();
					((TMP_Text)val5).text = "Vanilla Guides";
					if ((Object)(object)stationpedia.LoreGuideTitle != (Object)null)
					{
						((TMP_Text)val5).font = ((TMP_Text)stationpedia.LoreGuideTitle).font;
						((TMP_Text)val5).fontSharedMaterial = ((TMP_Text)stationpedia.LoreGuideTitle).fontSharedMaterial;
						((TMP_Text)val5).fontSize = ((TMP_Text)stationpedia.LoreGuideTitle).fontSize;
						((TMP_Text)val5).fontStyle = ((TMP_Text)stationpedia.LoreGuideTitle).fontStyle;
					}
					else
					{
						((TMP_Text)val5).fontSize = 32f;
						((TMP_Text)val5).fontStyle = (FontStyles)0;
					}
					((TMP_Text)val5).alignment = (TextAlignmentOptions)514;
					((Graphic)val5).color = new Color(1f, 1f, 1f, 1f);
					val2.transform.SetSiblingIndex(num);
					ConsoleWindow.Print($"[Stationpedia Ascended] Added Vanilla Guides header (Custom: {num}, Vanilla: {num2})", ConsoleColor.White, false, true);
				}
			}
			catch (Exception ex)
			{
				ConsoleWindow.Print("[Stationpedia Ascended] Error adding vanilla guides header: " + ex.Message, ConsoleColor.White, false, true);
			}
		}

		public static void Stationpedia_SetPageLore_Prefix(Stationpedia __instance)
		{
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Expected O, but got Unknown
			try
			{
				if ((Object)(object)__instance?.LoreGuideContents == (Object)null)
				{
					return;
				}
				List<GameObject> list = new List<GameObject>();
				foreach (Transform item in (Transform)__instance.LoreGuideContents)
				{
					Transform val = item;
					list.Add(((Component)val).gameObject);
				}
				foreach (GameObject item2 in list)
				{
					Object.Destroy((Object)(object)item2);
				}
			}
			catch (Exception ex)
			{
				ConsoleWindow.Print("[Stationpedia Ascended] Error in SetPageLore prefix: " + ex.Message, ConsoleColor.White, false, true);
			}
		}

		public static void Stationpedia_SetPageLore_Postfix(Stationpedia __instance)
		{
			try
			{
				HomePageLayoutManager.ModifyGuideLoreLayout(__instance.LoreGuideContents);
			}
			catch (Exception ex)
			{
				ConsoleWindow.Print("[Stationpedia Ascended] Error modifying lore layout: " + ex.Message, ConsoleColor.White, false, true);
			}
		}
	}
	public static class SearchPatches
	{
		private enum MatchPriority
		{
			ExactTitle,
			TitleStartsWith,
			TitleContains,
			DescriptionContains
		}

		private class ScoredResult
		{
			public StationpediaPage Page { get; set; }

			public MatchPriority Priority { get; set; }

			public string Category { get; set; }

			public Transform ItemTransform { get; set; }
		}

		private static List<GameObject> _searchCategoryHeaders = new List<GameObject>();

		private static List<GameObject> _injectedItems = new List<GameObject>();

		private static string _lastSearchText = "";

		private static int _lastResultCount = -1;

		private static bool _searchFieldHooked = false;

		private static Coroutine _reorganizeCoroutine = null;

		private static Dictionary<string, List<StationpediaPage>> _pageTitleIndex = null;

		private static Dictionary<string, List<StationpediaPage>> _pageWordIndex = null;

		private static Dictionary<string, string> _cleanedTitleCache = new Dictionary<string, string>();

		private static Dictionary<string, string> _categoryCache = new Dictionary<string, string>();

		private static bool _categoryCacheBuilt = false;

		private static Sprite _cachedHeaderSprite = null;

		private static TMP_FontAsset _cachedFont = null;

		private static Material _cachedFontMaterial = null;

		private static ColorBlock _cachedButtonColors;

		private static readonly Dictionary<string, string> CategoryIcons = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
		{
			{ "AtmosDevices", "[A]" },
			{ "AirContitioningAtmos", "[AC]" },
			{ "PipesCategory", "[P]" },
			{ "GasCanisterCategory", "[GC]" },
			{ "BatteryCategory", "[B]" },
			{ "CableCategory", "[C]" },
			{ "LightCategory", "[L]" },
			{ "LogicIntegratedCircuitsCategory", "[IC]" },
			{ "LogicInputCategory", "[IN]" },
			{ "LogicProcessorsCategory", "[CPU]" },
			{ "LogicReadersCategory", "[RD]" },
			{ "LogicWriterCategory", "[WR]" },
			{ "MotherboardCategory", "[MB]" },
			{ "Logic Variables", "[VAR]" },
			{ "Logic Slot Variables", "[SLOT]" },
			{ "Fabricators", "[FAB]" },
			{ "KitCategory", "[KIT]" },
			{ "FoodCategory", "[F]" },
			{ "Edibles", "[E]" },
			{ "Plants", "[PL]" },
			{ "PersonalSuits", "[SUIT]" },
			{ "PersonalBackpacks", "[BP]" },
			{ "PersonalClothing", "[CL]" },
			{ "PersonalHelmets", "[HLM]" },
			{ "PersonalEyeWear", "[EYE]" },
			{ "PersonalToolbelt", "[TB]" },
			{ "ManualTools", "[T]" },
			{ "FireArm", "[W]" },
			{ "WallFloorCategory", "[WF]" },
			{ "DoorCategory", "[DR]" },
			{ "ChairTableCategory", "[FUR]" },
			{ "SafetyCategory", "[!]" },
			{ "ChuteCategory", "[CH]" },
			{ "CargoCategory", "[CRG]" },
			{ "CartridgeCategory", "[CRT]" },
			{ "RocketEngineCategory", "[RKT]" },
			{ "RocketPayloadCategory", "[PAY]" },
			{ "UmbilicalCategory", "[UMB]" },
			{ "GeneticDevices", "[GEN]" },
			{ "TradingDevices", "[TRD]" },
			{ "Genetics", "[DNA]" },
			{ "ApplianceCategory", "[APP]" },
			{ "Gases", "[GAS]" },
			{ "Reagents", "[REA]" },
			{ "OreHeader", "[ORE]" },
			{ "FrozenOreHeader", "[FRZ]" },
			{ "PureIceHeader", "[ICE]" },
			{ "IngotHeader", "[ING]" },
			{ "SensorCategory", "[SEN]" },
			{ "ConsoleCategory", "[CON]" },
			{ "Other", "[-]" }
		};

		public static void ClearPreviousSearch_Postfix(Stationpedia __instance)
		{
			try
			{
				CleanupCategoryHeaders();
				if (!_searchFieldHooked)
				{
					HookSearchField(__instance);
				}
				if (_pageTitleIndex == null && Stationpedia.StationpediaPages != null && Stationpedia.StationpediaPages.Count > 0)
				{
					BuildPageIndexes();
					BuildCategoryCache();
				}
			}
			catch (Exception ex)
			{
				Debug.LogError((object)("[Stationpedia Ascended] Error in ClearPreviousSearch_Postfix: " + ex.Message));
			}
		}

		public static void InitializeSearchSystem(Stationpedia stationpedia)
		{
			try
			{
				if (!_searchFieldHooked && (Object)(object)stationpedia != (Object)null)
				{
					HookSearchField(stationpedia);
				}
				if (_pageTitleIndex == null && Stationpedia.StationpediaPages != null && Stationpedia.StationpediaPages.Count > 0)
				{
					BuildPageIndexes();
					BuildCategoryCache();
				}
			}
			catch (Exception ex)
			{
				Debug.LogError((object)("[Stationpedia Ascended] Error initializing search system: " + ex.Message));
			}
		}

		private static void HookSearchField(Stationpedia stationpedia)
		{
			try
			{
				if (!((Object)(object)stationpedia?.SearchField == (Object)null))
				{
					((UnityEvent<string>)(object)stationpedia.SearchField.onSubmit).AddListener((UnityAction<string>)OnSearchSubmit);
					((UnityEvent<string>)(object)stationpedia.SearchField.onValueChanged).AddListener((UnityAction<string>)OnSearchValueChanged);
					_searchFieldHooked = true;
				}
			}
			catch (Exception ex)
			{
				Debug.LogError((object)("[Stationpedia Ascended] Error hooking search field: " + ex.Message));
			}
		}

		private static void OnSearchSubmit(string searchText)
		{
			if ((Object)(object)StationpediaAscendedMod.Instance != (Object)null)
			{
				if (_reorganizeCoroutine != null)
				{
					((MonoBehaviour)StationpediaAscendedMod.Instance).StopCoroutine(_reorganizeCoroutine);
				}
				_reorganizeCoroutine = ((MonoBehaviour)StationpediaAscendedMod.Instance).StartCoroutine(DelayedReorganize(searchText, 0.3f));
			}
		}

		private static void OnSearchValueChanged(string searchText)
		{
			if (string.IsNullOrEmpty(searchText))
			{
				CleanupCategoryHeaders();
			}
			else if ((Object)(object)StationpediaAscendedMod.Instance != (Object)null && searchText.Length >= 3)
			{
				if (_reorganizeCoroutine != null)
				{
					((MonoBehaviour)StationpediaAscendedMod.Instance).StopCoroutine(_reorganizeCoroutine);
				}
				_reorganizeCoroutine = ((MonoBehaviour)StationpediaAscendedMod.Instance).StartCoroutine(DelayedReorganize(searchText, 0.8f));
			}
		}

		private static IEnumerator DelayedReorganize(string searchText, float delay)
		{
			yield return (object)new WaitForSeconds(delay);
			yield return (object)new WaitForEndOfFrame();
			yield return (object)new WaitForEndOfFrame();
			ReorganizeSearchResults(Stationpedia.Instance, searchText);
		}

		private static int CountVisibleSearchResults(RectTransform searchContents)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Expected O, but got Unknown
			int num = 0;
			foreach (Transform item in (Transform)searchContents)
			{
				Transform val = item;
				if (((Component)val).gameObject.activeSelf && (Object)(object)((Component)val).GetComponent<SPDAListItem>() != (Object)null)
				{
					num++;
				}
			}
			return num;
		}

		public static void ReorganizeSearchResults(Stationpedia stationpedia, string searchText)
		{
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Expected O, but got Unknown
			try
			{
				if ((Object)(object)stationpedia == (Object)null || string.IsNullOrEmpty(searchText) || (Object)(object)stationpedia.SearchContents == (Object)null)
				{
					return;
				}
				int num = CountVisibleSearchResults(stationpedia.SearchContents);
				if (searchText == _lastSearchText && num == _lastResultCount)
				{
					return;
				}
				_lastSearchText = searchText;
				_lastResultCount = num;
				CleanupCategoryHeaders();
				List<(SPDAListItem, StationpediaPage, Transform)> list = new List<(SPDAListItem, StationpediaPage, Transform)>();
				HashSet<string> hashSet = new HashSet<string>();
				List<Transform> list2 = new List<Transform>();
				foreach (Transform item in (Transform)stationpedia.SearchContents)
				{
					Transform val = item;
					if (!((Component)val).gameObject.activeSelf)
					{
						continue;
					}
					SPDAListItem component = ((Component)val).GetComponent<SPDAListItem>();
					if ((Object)(object)component == (Object)null)
					{
						continue;
					}
					TextMeshProUGUI insertTitle = component.InsertTitle;
					string title = ((insertTitle != null) ? ((TMP_Text)insertTitle).text : null) ?? "";
					StationpediaPage val2 = FindPageByTitle(title);
					if (val2 != null)
					{
						if (ShouldHideFromSearch(val2))
						{
							list2.Add(val);
							continue;
						}
						FixMissingSprite(component, val2, stationpedia);
						list.Add((component, val2, val));
						hashSet.Add(val2.Key);
					}
				}
				foreach (Transform item2 in list2)
				{
					((Component)item2).gameObject.SetActive(false);
				}
				List<StationpediaPage> list3 = FindMissingMatches(searchText, hashSet);
				if (list3.Count > 0)
				{
					InjectMissingResults(stationpedia, list3, list, hashSet);
				}
				if (list.Count != 0)
				{
					List<ScoredResult> scoredResults = ScoreResults(list, searchText);
					ReorderSearchUI(stationpedia, scoredResults);
				}
			}
			catch (Exception ex)
			{
				Debug.LogError((object)("[Stationpedia Ascended] Error in ReorganizeSearchResults: " + ex.Message + "\n" + ex.StackTrace));
			}
		}

		private static List<StationpediaPage> FindMissingMatches(string searchText, HashSet<string> existingPageKeys)
		{
			BuildPageIndexes();
			List<StationpediaPage> list = new List<StationpediaPage>();
			string key = searchText.ToLowerInvariant().Trim();
			if (_pageTitleIndex.TryGetValue(key, out var value))
			{
				foreach (StationpediaPage item in value)
				{
					if (!existingPageKeys.Contains(item.Key) && !ShouldHideFromSearch(item))
					{
						list.Add(item);
					}
				}
			}
			if (_pageWordIndex.TryGetValue(key, out var value2))
			{
				foreach (StationpediaPage item2 in value2)
				{
					if (!list.Contains(item2) && !existingPageKeys.Contains(item2.Key) && !ShouldHideFromSearch(item2))
					{
						list.Add(item2);
					}
				}
			}
			return list;
		}

		private static void BuildPageIndexes()
		{
			if (_pageTitleIndex != null)
			{
				return;
			}
			Stopwatch stopwatch = Stopwatch.StartNew();
			_pageTitleIndex = new Dictionary<string, List<StationpediaPage>>();
			_pageWordIndex = new Dictionary<string, List<StationpediaPage>>();
			foreach (StationpediaPage stationpediaPage in Stationpedia.StationpediaPages)
			{
				if (ShouldHideFromSearch(stationpediaPage))
				{
					continue;
				}
				string text = CleanTitle(stationpediaPage.Title).ToLowerInvariant();
				if (string.IsNullOrEmpty(text))
				{
					continue;
				}
				if (!_pageTitleIndex.ContainsKey(text))
				{
					_pageTitleIndex[text] = new List<StationpediaPage>();
				}
				_pageTitleIndex[text].Add(stationpediaPage);
				string[] array = text.Split(new char[6] { ' ', '-', '(', ')', '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
				string[] array2 = array;
				foreach (string text2 in array2)
				{
					if (text2.Length >= 2)
					{
						if (!_pageWordIndex.ContainsKey(text2))
						{
							_pageWordIndex[text2] = new List<StationpediaPage>();
						}
						if (!_pageWordIndex[text2].Contains(stationpediaPage))
						{
							_pageWordIndex[text2].Add(stationpediaPage);
						}
					}
				}
			}
			stopwatch.Stop();
			ConsoleWindow.Print($"[Stationpedia Ascended] Built page indexes: {_pageTitleIndex.Count} titles, {_pageWordIndex.Count} words in {stopwatch.ElapsedMilliseconds}ms", ConsoleColor.White, false, true);
		}

		private static string CleanTitle(string title)
		{
			if (string.IsNullOrEmpty(title))
			{
				return "";
			}
			if (_cleanedTitleCache.TryGetValue(title, out var value))
			{
				return value;
			}
			value = Regex.Replace(title, "<[^>]+>", "").Trim();
			_cleanedTitleCache[title] = value;
			return value;
		}

		private static void InjectMissingResults(Stationpedia stationpedia, List<StationpediaPage> pages, List<(SPDAListItem item, StationpediaPage page, Transform transform)> items, HashSet<string> existingPageKeys)
		{
			//IL_0142: Unknown result type (might be due to invalid IL or missing references)
			//IL_014c: Expected O, but got Unknown
			try
			{
				if (items.Count == 0 || (Object)(object)stationpedia.SearchContents == (Object)null)
				{
					return;
				}
				SPDAListItem item = items[0].item;
				if ((Object)(object)item == (Object)null)
				{
					return;
				}
				foreach (StationpediaPage page in pages)
				{
					if (existingPageKeys.Contains(page.Key))
					{
						continue;
					}
					GameObject val = Object.Instantiate<GameObject>(((Component)item).gameObject, (Transform)(object)stationpedia.SearchContents);
					((Object)val).name = "InjectedResult_" + page.Key;
					val.SetActive(true);
					SPDAListItem component = val.GetComponent<SPDAListItem>();
					if (!((Object)(object)component != (Object)null))
					{
						continue;
					}
					if ((Object)(object)component.InsertTitle != (Object)null)
					{
						((TMP_Text)component.InsertTitle).text = page.Title;
					}
					string pageKey = page.Key;
					((UnityEventBase)component.InsertsButton.onClick).RemoveAllListeners();
					((UnityEvent)component.InsertsButton.onClick).AddListener((UnityAction)delegate
					{
						stationpedia.OpenPageByKey(pageKey);
					});
					if ((Object)(object)component.InsertImage != (Object)null)
					{
						Sprite val2 = page.CustomSpriteToUse;
						if ((Object)(object)val2 == (Object)null)
						{
							val2 = TryGetSpriteFromPrefab(page.Key);
						}
						if ((Object)(object)val2 == (Object)null)
						{
							val2 = stationpedia.ImportantSearchImage;
						}
						component.InsertImage.sprite = val2;
						((Component)component.InsertImage).gameObject.SetActive((Object)(object)val2 != (Object)null);
					}
					items.Add((component, page, val.transform));
					existingPageKeys.Add(page.Key);
					_injectedItems.Add(val);
				}
			}
			catch (Exception ex)
			{
				Debug.LogError((object)("[Stationpedia Ascended] Error injecting results: " + ex.Message));
			}
		}

		private static StationpediaPage FindPageByTitle(string title)
		{
			if (string.IsNullOrEmpty(title))
			{
				return null;
			}
			BuildPageIndexes();
			string key = CleanTitle(title).ToLowerInvariant();
			if (_pageTitleIndex.TryGetValue(key, out var value) && value.Count > 0)
			{
				return value[0];
			}
			return null;
		}

		private static Sprite TryGetSpriteFromPrefab(string pageKey)
		{
			if (string.IsNullOrEmpty(pageKey))
			{
				return null;
			}
			try
			{
				string text = null;
				text = (pageKey.StartsWith("Thing") ? pageKey.Substring(5) : ((!pageKey.StartsWith("Item")) ? pageKey : pageKey));
				Thing val = Prefab.Find(text);
				if ((Object)(object)val != (Object)null)
				{
					return val.GetThumbnail();
				}
				if (!text.StartsWith("Item"))
				{
					val = Prefab.Find("Item" + text);
					if ((Object)(object)val != (Object)null)
					{
						return val.GetThumbnail();
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning((object)("[Stationpedia Ascended] Could not load sprite for " + pageKey + ": " + ex.Message));
			}
			return null;
		}

		private static bool ShouldHideFromSearch(StationpediaPage page)
		{
			if (page == null)
			{
				return false;
			}
			string text = page.Key ?? "";
			string input = page.Title ?? "";
			string text2 = Regex.Replace(input, "<[^>]+>", "").Trim();
			string text3 = text2.ToLowerInvariant();
			if (text.StartsWith("Thing"))
			{
				string text4 = text.Substring(5);
				Thing val = Prefab.Find(text4);
				if ((Object)(object)val != (Object)null && val.HideInStationpedia)
				{
					return true;
				}
				bool value = false;
				SPDADataHandler dataHandler = Stationpedia.DataHandler;
				if (dataHandler != null && dataHandler.HiddenInPedia?.TryGetValue(text4, out value) == true && value)
				{
					return true;
				}
			}
			if (text.Contains("Ruptured") || text3.Contains("ruptured"))
			{
				return true;
			}
			if (text.Contains("CableRuptured"))
			{
				return true;
			}
			if (text3.StartsWith("burnt ") || text3.StartsWith("burnt") || text.Contains("Burnt"))
			{
				return true;
			}
			if (text3.Contains("wreckage") || text.Contains("Wreckage"))
			{
				return true;
			}
			return false;
		}

		private static void FixMissingSprite(SPDAListItem listItem, StationpediaPage page, Stationpedia stationpedia)
		{
			if (!((Object)(object)listItem?.InsertImage == (Object)null) && !((Object)(object)listItem.InsertImage.sprite != (Object)null))
			{
				Sprite val = page.CustomSpriteToUse;
				if ((Object)(object)val == (Object)null)
				{
					val = TryGetSpriteFromPrefab(page.Key);
				}
				if ((Object)(object)val == (Object)null)
				{
					val = stationpedia.ImportantSearchImage;
				}
				listItem.InsertImage.sprite = val;
				((Component)listItem.InsertImage).gameObject.SetActive((Object)(object)val != (Object)null);
			}
		}

		private static List<ScoredResult> ScoreResults(List<(SPDAListItem item, StationpediaPage page, Transform transform)> items, string searchText)
		{
			List<ScoredResult> list = new List<ScoredResult>(items.Count);
			string text = searchText.ToLowerInvariant().Trim();
			string text2 = Regex.Escape(text);
			Regex regex = new Regex("\\b" + text2 + "\\b", RegexOptions.IgnoreCase);
			foreach (var item4 in items)
			{
				SPDAListItem item = item4.item;
				StationpediaPage item2 = item4.page;
				Transform item3 = item4.transform;
				TextMeshProUGUI insertTitle = item.InsertTitle;
				string title = ((insertTitle != null) ? ((TMP_Text)insertTitle).text : null) ?? "";
				string text3 = CleanTitle(title).ToLowerInvariant();
				bool flag = regex.IsMatch(text3);
				MatchPriority priority = ((!(text3 == text)) ? ((text3.StartsWith(text + " ") || (text3.StartsWith(text) && flag)) ? MatchPriority.TitleStartsWith : ((!flag) ? ((!text3.Contains(text)) ? MatchPriority.DescriptionContains : MatchPriority.DescriptionContains) : MatchPriority.TitleContains)) : MatchPriority.ExactTitle);
				string pageCategory = GetPageCategory(item2);
				list.Add(new ScoredResult
				{
					Page = item2,
					Priority = priority,
					Category = pageCategory,
					ItemTransform = item3
				});
			}
			return list;
		}

		private static string GetCategoryIcon(string category)
		{
			if (string.IsNullOrEmpty(category))
			{
				return "[-]";
			}
			if (CategoryIcons.TryGetValue(category, out var value))
			{
				return value;
			}
			string text = category.ToLowerInvariant();
			if (text.Contains("atmos") || text.Contains("air"))
			{
				return "[A]";
			}
			if (text.Contains("pipe"))
			{
				return "[P]";
			}
			if (text.Contains("gas") && text.Contains("can"))
			{
				return "[GC]";
			}
			if (text.Contains("battery") || text.Contains("power"))
			{
				return "[B]";
			}
			if (text.Contains("cable") || text.Contains("wire"))
			{
				return "[C]";
			}
			if (text.Contains("light"))
			{
				return "[L]";
			}
			if (text.Contains("logic") || text.Contains("circuit"))
			{
				return "[IC]";
			}
			if (text.Contains("processor") || text.Contains("computer"))
			{
				return "[CPU]";
			}
			if (text.Contains("fabricat") || text.Contains("printer"))
			{
				return "[FAB]";
			}
			if (text.Contains("kit"))
			{
				return "[KIT]";
			}
			if (text.Contains("food") || text.Contains("edible"))
			{
				return "[F]";
			}
			if (text.Contains("plant") || text.Contains("seed"))
			{
				return "[PL]";
			}
			if (text.Contains("suit") || text.Contains("armor"))
			{
				return "[SUIT]";
			}
			if (text.Contains("helmet"))
			{
				return "[HLM]";
			}
			if (text.Contains("cloth"))
			{
				return "[CL]";
			}
			if (text.Contains("tool"))
			{
				return "[T]";
			}
			if (text.Contains("weapon") || text.Contains("gun"))
			{
				return "[W]";
			}
			if (text.Contains("wall") || text.Contains("floor") || text.Contains("frame"))
			{
				return "[WF]";
			}
			if (text.Contains("door") || text.Contains("airlock"))
			{
				return "[DR]";
			}
			if (text.Contains("chair") || text.Contains("table") || text.Contains("furniture"))
			{
				return "[FUR]";
			}
			if (text.Contains("chute") || text.Contains("conveyor"))
			{
				return "[CH]";
			}
			if (text.Contains("cargo") || text.Contains("storage") || text.Contains("locker"))
			{
				return "[CRG]";
			}
			if (text.Contains("rocket") || text.Contains("engine"))
			{
				return "[RKT]";
			}
			if (text.Contains("genetic") || text.Contains("dna"))
			{
				return "[DNA]";
			}
			if (text.Contains("trade") || text.Contains("vend"))
			{
				return "[TRD]";
			}
			if (text.Contains("reagent") || text.Contains("chemical"))
			{
				return "[REA]";
			}
			if (text.Contains("ore") || text.Contains("mineral"))
			{
				return "[ORE]";
			}
			if (text.Contains("ice") || text.Contains("frozen"))
			{
				return "[ICE]";
			}
			if (text.Contains("ingot") || text.Contains("metal"))
			{
				return "[ING]";
			}
			if (text.Contains("sensor"))
			{
				return "[SEN]";
			}
			if (text.Contains("console") || text.Contains("screen"))
			{
				return "[CON]";
			}
			if (text.Contains("tank") || text.Contains("canister"))
			{
				return "[GC]";
			}
			if (text.Contains("gas"))
			{
				return "[GAS]";
			}
			if (text.Contains("backpack"))
			{
				return "[BP]";
			}
			if (text.Contains("eyewear") || text.Contains("glass"))
			{
				return "[EYE]";
			}
			return "[-]";
		}

		private static string GetPageCategory(StationpediaPage page)
		{
			if (page == null)
			{
				return "Other";
			}
			string key = page.Key ?? "";
			if (_categoryCache.TryGetValue(key, out var value))
			{
				return value;
			}
			if (!_categoryCacheBuilt)
			{
				BuildCategoryCache();
				if (_categoryCache.TryGetValue(key, out value))
				{
					return value;
				}
			}
			string text = ComputePageCategory(page);
			_categoryCache[key] = text;
			return text;
		}

		private static void BuildCategoryCache()
		{
			if (_categoryCacheBuilt)
			{
				return;
			}
			Stopwatch stopwatch = Stopwatch.StartNew();
			try
			{
				foreach (KeyValuePair<string, Dictionary<string, List<StationCategoryInsert>>> item in Stationpedia.DataHandler._listDictionary)
				{
					foreach (KeyValuePair<string, List<StationCategoryInsert>> item2 in item.Value)
					{
						string key = item2.Key;
						foreach (StationCategoryInsert item3 in item2.Value)
						{
							if (!string.IsNullOrEmpty(item3.PageLink))
							{
								_categoryCache[item3.PageLink] = key;
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				ConsoleWindow.Print("[Stationpedia Ascended] Error building category cache: " + ex.Message, ConsoleColor.White, false, true);
			}
			_categoryCacheBuilt = true;
			stopwatch.Stop();
			ConsoleWindow.Print($"[Stationpedia Ascended] Built category cache: {_categoryCache.Count} entries in {stopwatch.ElapsedMilliseconds}ms", ConsoleColor.White, false, true);
		}

		private static string ComputePageCategory(StationpediaPage page)
		{
			if (page == null)
			{
				return "Other";
			}
			if (page.PageCustomCategories != null && page.PageCustomCategories.Count > 0)
			{
				return page.PageCustomCategories[0];
			}
			string text = page.Key ?? "";
			if (text.StartsWith("Gas"))
			{
				return "Gases";
			}
			if (text.StartsWith("Reagent"))
			{
				return "Reagents";
			}
			if (text.StartsWith("Gene"))
			{
				return "Genetics";
			}
			if (text.StartsWith("LogicType"))
			{
				return "Logic Variables";
			}
			if (text.StartsWith("LogicSlotType"))
			{
				return "Logic Slot Variables";
			}
			return "Other";
		}

		private static void ReorderSearchUI(Stationpedia stationpedia, List<ScoredResult> scoredResults)
		{
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_019b: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
			RectTransform searchContents = stationpedia.SearchContents;
			if ((Object)(object)searchContents == (Object)null)
			{
				return;
			}
			List<ScoredResult> list = scoredResults.Where((ScoredResult r) => r.Priority == MatchPriority.ExactTitle).ToList();
			List<ScoredResult> list2 = scoredResults.Where((ScoredResult r) => r.Priority == MatchPriority.TitleStartsWith).ToList();
			List<ScoredResult> first = scoredResults.Where((ScoredResult r) => r.Priority == MatchPriority.TitleContains).ToList();
			List<ScoredResult> second = scoredResults.Where((ScoredResult r) => r.Priority == MatchPriority.DescriptionContains).ToList();
			int siblingIndex = 0;
			if (list.Count > 0)
			{
				siblingIndex = AddCategoryHeader(searchContents, "Exact Matches", siblingIndex, new Color(0.95f, 0.6f, 0.25f, 1f));
				foreach (ScoredResult item in list.OrderBy((ScoredResult r) => r.Page.Title))
				{
					item.ItemTransform.SetSiblingIndex(siblingIndex++);
				}
			}
			if (list2.Count > 0)
			{
				siblingIndex = AddCategoryHeader(searchContents, "Starts With", siblingIndex, new Color(0.9f, 0.8f, 0.4f, 1f));
				foreach (ScoredResult item2 in list2.OrderBy((ScoredResult r) => r.Page.Title))
				{
					item2.ItemTransform.SetSiblingIndex(siblingIndex++);
				}
			}
			List<ScoredResult> list3 = first.Concat(second).ToList();
			if (list3.Count > 0)
			{
				IOrderedEnumerable<IGrouping<string, ScoredResult>> orderedEnumerable = from r in list3
					group r by r.Category into g
					orderby g.Key
					select g;
				foreach (IGrouping<string, ScoredResult> item3 in orderedEnumerable)
				{
					siblingIndex = AddCategoryHeader(searchContents, item3.Key, siblingIndex, Color.white);
					foreach (ScoredResult item4 in item3.OrderBy((ScoredResult r) => r.Page.Title))
					{
						item4.ItemTransform.SetSiblingIndex(siblingIndex++);
					}
				}
			}
			LayoutRebuilder.ForceRebuildLayoutImmediate(searchContents);
		}

		private static int AddCategoryHeader(RectTransform parent, string text, int siblingIndex, Color textColor)
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = CreateNewCategoryHeader(parent, text, textColor);
			_searchCategoryHeaders.Add(val);
			val.transform.SetSiblingIndex(siblingIndex);
			return siblingIndex + 1;
		}

		private static GameObject CreateNewCategoryHeader(RectTransform parent, string text, Color textColor)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0276: Unknown result type (might be due to invalid IL or missing references)
			//IL_027c: Expected O, but got Unknown
			//IL_0299: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0308: Unknown result type (might be due to invalid IL or missing references)
			//IL_0219: Unknown result type (might be due to invalid IL or missing references)
			//IL_0240: Unknown result type (might be due to invalid IL or missing references)
			//IL_0256: Unknown result type (might be due to invalid IL or missing references)
			//IL_025b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0268: Unknown result type (might be due to invalid IL or missing references)
			//IL_0117: Unknown result type (might be due to invalid IL or missing references)
			//IL_011c: Unknown result type (might be due to invalid IL or missing references)
			//IL_013a: Unknown result type (might be due to invalid IL or missing references)
			//IL_015e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0182: Unknown result type (might be due to invalid IL or missing references)
			//IL_0197: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = new GameObject("SearchCategoryHeader");
			val.transform.SetParent((Transform)(object)parent, false);
			RectTransform val2 = val.AddComponent<RectTransform>();
			val2.anchorMin = new Vector2(0f, 1f);
			val2.anchorMax = new Vector2(1f, 1f);
			val2.pivot = new Vector2(0.5f, 1f);
			val2.sizeDelta = new Vector2(0f, 54f);
			Image val3 = val.AddComponent<Image>();
			if ((Object)(object)_cachedHeaderSprite == (Object)null)
			{
				try
				{
					SPDAListItem componentInChildren = ((Component)parent).GetComponentInChildren<SPDAListItem>();
					if ((Object)(object)componentInChildren != (Object)null)
					{
						if ((Object)(object)componentInChildren.SpecialButton != (Object)null)
						{
							_cachedHeaderSprite = componentInChildren.SpecialButton;
						}
						else if ((Object)(object)componentInChildren.BackImage != (Object)null)
						{
							_cachedHeaderSprite = componentInChildren.BackImage.sprite;
						}
						if ((Object)(object)componentInChildren.InsertsButton != (Object)null)
						{
							_cachedButtonColors = ((Selectable)componentInChildren.InsertsButton).colors;
							((ColorBlock)(ref _cachedButtonColors)).normalColor = new Color(0.7f, 0.7f, 0.7f, 1f);
							((ColorBlock)(ref _cachedButtonColors)).highlightedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
							((ColorBlock)(ref _cachedButtonColors)).pressedColor = new Color(0.6f, 0.6f, 0.6f, 1f);
							((ColorBlock)(ref _cachedButtonColors)).selectedColor = ((ColorBlock)(ref _cachedButtonColors)).normalColor;
							((ColorBlock)(ref _cachedButtonColors)).disabledColor = ((ColorBlock)(ref _cachedButtonColors)).normalColor;
						}
					}
					TextMeshProUGUI componentInChildren2 = ((Component)parent).GetComponentInChildren<TextMeshProUGUI>();
					if ((Object)(object)componentInChildren2 != (Object)null)
					{
						_cachedFont = ((TMP_Text)componentInChildren2).font;
						_cachedFontMaterial = ((TMP_Text)componentInChildren2).fontSharedMaterial;
					}
				}
				catch
				{
				}
			}
			if ((Object)(object)_cachedHeaderSprite != (Object)null)
			{
				val3.sprite = _cachedHeaderSprite;
				val3.type = (Type)1;
				((Graphic)val3).color = Color.white;
				Button val4 = val.AddComponent<Button>();
				((Selectable)val4).targetGraphic = (Graphic)(object)val3;
				((Selectable)val4).transition = (Transition)1;
				((Selectable)val4).colors = _cachedButtonColors;
				((Selectable)val4).interactable = true;
				Navigation navigation = ((Selectable)val4).navigation;
				((Navigation)(ref navigation)).mode = (Mode)0;
				((Selectable)val4).navigation = navigation;
			}
			GameObject val5 = new GameObject("HeaderText");
			val5.transform.SetParent(val.transform, false);
			RectTransform val6 = val5.AddComponent<RectTransform>();
			val6.anchorMin = Vector2.zero;
			val6.anchorMax = Vector2.one;
			val6.offsetMin = new Vector2(15f, 0f);
			val6.offsetMax = new Vector2(-10f, 0f);
			TextMeshProUGUI val7 = val5.AddComponent<TextMeshProUGUI>();
			((TMP_Text)val7).text = text;
			((TMP_Text)val7).fontSize = 19f;
			((TMP_Text)val7).fontStyle = (FontStyles)1;
			((Graphic)val7).color = textColor;
			((TMP_Text)val7).alignment = (TextAlignmentOptions)513;
			((TMP_Text)val7).verticalAlignment = (VerticalAlignmentOptions)512;
			if ((Object)(object)_cachedFont != (Object)null)
			{
				((TMP_Text)val7).font = _cachedFont;
				((TMP_Text)val7).fontSharedMaterial = _cachedFontMaterial;
			}
			LayoutElement val8 = val.AddComponent<LayoutElement>();
			val8.preferredHeight = 54f;
			val8.flexibleWidth = 1f;
			return val;
		}

		private static void CleanupCategoryHeaders()
		{
			foreach (GameObject searchCategoryHeader in _searchCategoryHeaders)
			{
				if ((Object)(object)searchCategoryHeader != (Object)null)
				{
					Object.Destroy((Object)(object)searchCategoryHeader);
				}
			}
			_searchCategoryHeaders.Clear();
			foreach (GameObject injectedItem in _injectedItems)
			{
				if ((Object)(object)injectedItem != (Object)null)
				{
					Object.Destroy((Object)(object)injectedItem);
				}
			}
			_injectedItems.Clear();
			_lastSearchText = "";
			_lastResultCount = -1;
		}

		public static void Reset()
		{
			CleanupCategoryHeaders();
			_searchFieldHooked = false;
			_reorganizeCoroutine = null;
			_pageTitleIndex = null;
			_pageWordIndex = null;
			_cleanedTitleCache.Clear();
			_categoryCache.Clear();
			_categoryCacheBuilt = false;
			_cachedHeaderSprite = null;
			_cachedFont = null;
			_cachedFontMaterial = null;
		}
	}
}
namespace StationpediaAscended.Diagnostics
{
	public class UIAssetInspector : MonoBehaviour
	{
		private Canvas _tooltipCanvas;

		private GameObject _tooltipPanel;

		private List<TextMeshProUGUI> _columnTexts = new List<TextMeshProUGUI>();

		private RectTransform _tooltipRect;

		private LayoutElement _panelLayoutElement;

		private const int ITEMS_PER_COLUMN = 1;

		private const int MAX_COLUMNS = 6;

		private const float COLUMN_WIDTH = 300f;

		private const float COLUMN_SPACING = 10f;

		private const float PADDING = 10f;

		private const float MAX_HEIGHT_PERCENT = 0.9f;

		private PointerEventData _pointerData;

		private List<RaycastResult> _raycastResults = new List<RaycastResult>();

		private StringBuilder _sb = new StringBuilder();

		private List<StringBuilder> _columnBuilders = new List<StringBuilder>();

		private string _lastPlainTextContent = "";

		private List<LayoutElement> _columnLayoutElements = new List<LayoutElement>();

		private static readonly Color BG_COLOR = new Color(0.05f, 0.08f, 0.12f, 0.95f);

		private static readonly Color BORDER_COLOR = new Color(0.98431f, 0.6902f, 0.23137f, 1f);

		private static readonly Color TEXT_COLOR = new Color(0.9f, 0.9f, 0.9f, 1f);

		public static UIAssetInspector Instance { get; private set; }

		public static bool IsEnabled { get; private set; } = false;

		public static void Initialize()
		{
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Expected O, but got Unknown
			GameObject val = GameObject.Find("UIAssetInspector");
			if ((Object)(object)val != (Object)null)
			{
				Object.Destroy((Object)(object)val);
				Debug.Log((object)"[UIAssetInspector] Destroyed old instance for hot reload");
			}
			Instance = null;
			GameObject val2 = new GameObject("UIAssetInspector");
			Instance = val2.AddComponent<UIAssetInspector>();
			Object.DontDestroyOnLoad((Object)(object)val2);
			Instance.CreateTooltip();
			Instance.SetEnabled(enabled: false);
			Debug.Log((object)"[UIAssetInspector] Initialized. Use 'assetdisplay' command to toggle.");
		}

		public static void Toggle()
		{
			if ((Object)(object)Instance == (Object)null)
			{
				Initialize();
			}
			Instance.SetEnabled(!IsEnabled);
		}

		public static void Cleanup()
		{
			if ((Object)(object)Instance != (Object)null)
			{
				if ((Object)(object)Instance._tooltipCanvas != (Object)null)
				{
					Object.Destroy((Object)(object)((Component)Instance._tooltipCanvas).gameObject);
				}
				Object.Destroy((Object)(object)((Component)Instance).gameObject);
				Instance = null;
			}
		}

		private void SetEnabled(bool enabled)
		{
			IsEnabled = enabled;
			if ((Object)(object)_tooltipCanvas != (Object)null)
			{
				((Behaviour)_tooltipCanvas).enabled = enabled;
			}
			string text = (enabled ? "ENABLED - Right-click to copy to clipboard" : "DISABLED");
			Debug.Log((object)("[UIAssetInspector] " + text));
		}

		private void CreateTooltip()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected O, but got Unknown
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Expected O, but got Unknown
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
			//IL_0104: Unknown result type (might be due to invalid IL or missing references)
			//IL_0124: Unknown result type (might be due to invalid IL or missing references)
			//IL_013a: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ac: Expected O, but got Unknown
			//IL_0219: Unknown result type (might be due to invalid IL or missing references)
			//IL_0220: Expected O, but got Unknown
			//IL_0251: Unknown result type (might be due to invalid IL or missing references)
			GameObject val = new GameObject("AssetInspectorCanvas");
			val.transform.SetParent(((Component)this).transform);
			_tooltipCanvas = val.AddComponent<Canvas>();
			_tooltipCanvas.renderMode = (RenderMode)0;
			_tooltipCanvas.sortingOrder = 32000;
			CanvasScaler val2 = val.AddComponent<CanvasScaler>();
			val2.uiScaleMode = (ScaleMode)1;
			val2.referenceResolution = new Vector2(1920f, 1080f);
			_tooltipPanel = new GameObject("TooltipPanel");
			_tooltipPanel.transform.SetParent(val.transform, false);
			_tooltipRect = _tooltipPanel.AddComponent<RectTransform>();
			_tooltipRect.pivot = new Vector2(0f, 1f);
			_tooltipRect.anchorMin = new Vector2(0f, 0f);
			_tooltipRect.anchorMax = new Vector2(0f, 0f);
			Image val3 = _tooltipPanel.AddComponent<Image>();
			((Graphic)val3).color = BG_COLOR;
			((Graphic)val3).raycastTarget = false;
			Outline val4 = _tooltipPanel.AddComponent<Outline>();
			((Shadow)val4).effectColor = BORDER_COLOR;
			((Shadow)val4).effectDistance = new Vector2(2f, 2f);
			ContentSizeFitter val5 = _tooltipPanel.AddComponent<ContentSizeFitter>();
			val5.horizontalFit = (FitMode)2;
			val5.verticalFit = (FitMode)0;
			_panelLayoutElement = _tooltipPanel.AddComponent<LayoutElement>();
			_panelLayoutElement.preferredHeight = (float)Screen.height * 0.9f;
			HorizontalLayoutGroup val6 = _tooltipPanel.AddComponent<HorizontalLayoutGroup>();
			((LayoutGroup)val6).padding = new RectOffset(10, 10, 8, 8);
			((HorizontalOrVerticalLayoutGroup)val6).spacing = 20f;
			((LayoutGroup)val6).childAlignment = (TextAnchor)0;
			((HorizontalOrVerticalLayoutGroup)val6).childControlWidth = true;
			((HorizontalOrVerticalLayoutGroup)val6).childControlHeight = true;
			((HorizontalOrVerticalLayoutGroup)val6).childForceExpandWidth = false;
			((HorizontalOrVerticalLayoutGroup)val6).childForceExpandHeight = false;
			_columnTexts.Clear();
			_columnBuilders.Clear();
			for (int i = 0; i < 6; i++)
			{
				GameObject val7 = new GameObject($"Column{i}");
				val7.transform.SetParent(_tooltipPanel.transform, false);
				TextMeshProUGUI val8 = val7.AddComponent<TextMeshProUGUI>();
				((TMP_Text)val8).fontSize = 11f;
				((Graphic)val8).color = TEXT_COLOR;
				((TMP_Text)val8).alignment = (TextAlignmentOptions)257;
				((TMP_Text)val8).enableWordWrapping = true;
				((TMP_Text)val8).overflowMode = (TextOverflowModes)3;
				((Graphic)val8).raycastTarget = false;
				((TMP_Text)val8).richText = true;
				LayoutElement val9 = val7.AddComponent<LayoutElement>();
				val9.minWidth = 300f;
				val9.preferredWidth = 300f;
				val9.minHeight = 50f;
				val9.preferredHeight = (float)Screen.height * 0.9f - 50f;
				val9.flexibleWidth = 0f;
				val9.flexibleHeight = 0f;
				_columnTexts.Add(val8);
				_columnBuilders.Add(new StringBuilder());
				_columnLayoutElements.Add(val9);
				val7.SetActive(false);
			}
		}

		private void Update()
		{
			if (IsEnabled && !((Object)(object)_tooltipCanvas == (Object)null))
			{
				if (Input.GetMouseButtonDown(1))
				{
					CopyToClipboard();
				}
				UpdateTooltipContent();
				LayoutRebuilder.ForceRebuildLayoutImmediate(_tooltipRect);
				PositionTooltip();
			}
		}

		private void CopyToClipboard()
		{
			if (!string.IsNullOrEmpty(_lastPlainTextContent))
			{
				GUIUtility.systemCopyBuffer = _lastPlainTextContent;
				Debug.Log((object)"[UIAssetInspector] Copied to clipboard!");
			}
		}

		private void PositionTooltip()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_0107: Unknown result type (might be due to invalid IL or missing references)
			//IL_013f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0144: Unknown result type (might be due to invalid IL or missing references)
			Vector2 val = Vector2.op_Implicit(Input.mousePosition);
			float num = (float)Screen.height * 0.9f;
			_panelLayoutElement.preferredHeight = num;
			foreach (LayoutElement columnLayoutElement in _columnLayoutElements)
			{
				columnLayoutElement.preferredHeight = num - 20f;
			}
			_tooltipRect.SetSizeWithCurrentAnchors((Axis)1, num);
			Vector2 val2 = default(Vector2);
			((Vector2)(ref val2))..ctor(_tooltipRect.sizeDelta.x, num);
			float num2 = val.x + 20f;
			float num3 = val.y - 10f;
			if (num2 + val2.x > (float)(Screen.width - 10))
			{
				num2 = val.x - val2.x - 20f;
			}
			if (num2 < 10f)
			{
				num2 = 10f;
			}
			if (num3 - val2.y < 10f)
			{
				num3 = val2.y + 10f;
			}
			if (num3 > (float)(Screen.height - 10))
			{
				num3 = Screen.height - 10;
			}
			((Transform)_tooltipRect).position = Vector2.op_Implicit(new Vector2(num2, num3));
		}

		private void UpdateTooltipContent()
		{
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Expected O, but got Unknown
			//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_012e: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
			foreach (StringBuilder columnBuilder in _columnBuilders)
			{
				columnBuilder.Clear();
			}
			EventSystem current2 = EventSystem.current;
			if ((Object)(object)current2 == (Object)null)
			{
				_columnBuilders[0].AppendLine("<color=#FBB03B><b>== UI ASSET INSPECTOR ==</b></color>");
				_columnBuilders[0].AppendLine("<color=#FF8080>No EventSystem found</color>");
				ApplyColumnsToUI(1);
				return;
			}
			if (_pointerData == null)
			{
				_pointerData = new PointerEventData(current2);
			}
			_pointerData.position = Vector2.op_Implicit(Input.mousePosition);
			_raycastResults.Clear();
			current2.RaycastAll(_pointerData, _raycastResults);
			List<RaycastResult> list = new List<RaycastResult>();
			foreach (RaycastResult raycastResult in _raycastResults)
			{
				RaycastResult current3 = raycastResult;
				if ((Object)(object)((RaycastResult)(ref current3)).gameObject != (Object)null && !((RaycastResult)(ref current3)).gameObject.transform.IsChildOf(((Component)this).transform))
				{
					list.Add(current3);
				}
			}
			TranslucentImageSource[] array = Object.FindObjectsOfType<TranslucentImageSource>();
			_columnBuilders[0].AppendLine("<color=#FBB03B><b>== UI ASSET INSPECTOR ==</b></color>");
			_columnBuilders[0].AppendLine("<color=#808080>Right-click to copy</color>");
			_columnBuilders[0].AppendLine("<color=#606060>------------------------</color>");
			if (array.Length != 0)
			{
				_columnBuilders[0].AppendLine($"<color=#FF80FF>TranslucentImageSources: {array.Length}</color>");
				TranslucentImageSource[] array2 = array;
				foreach (TranslucentImageSource val in array2)
				{
					_columnBuilders[0].AppendLine("  -> " + ((Object)((Component)val).gameObject).name);
				}
			}
			else
			{
				_columnBuilders[0].AppendLine("<color=#FF8080>No TranslucentImageSource!</color>");
			}
			_columnBuilders[0].AppendLine("<color=#606060>------------------------</color>");
			if (list.Count == 0)
			{
				_columnBuilders[0].AppendLine("<color=#FFFF80>No UI elements under cursor</color>");
				ApplyColumnsToUI(1);
				return;
			}
			_columnBuilders[0].AppendLine($"<color=#80FFFF>Found {list.Count} element(s):</color>\n");
			int num = Mathf.Min(list.Count, 6);
			int num2 = Mathf.CeilToInt((float)num / 1f);
			num2 = Mathf.Max(1, Mathf.Min(num2, 6));
			int num3 = 0;
			for (int j = 0; j < num2; j++)
			{
				if (num3 >= num)
				{
					break;
				}
				int num4 = ((j != 0) ? 1 : 0);
				for (int k = 0; k < num4; k++)
				{
					if (num3 >= num)
					{
						break;
					}
					RaycastResult val2 = list[num3];
					GameObject gameObject = ((RaycastResult)(ref val2)).gameObject;
					AppendGameObjectInfo(_columnBuilders[j], gameObject, num3 + 1);
					num3++;
				}
			}
			if (list.Count > num)
			{
				int index = num2 - 1;
				_columnBuilders[index].AppendLine($"<color=#808080>... and {list.Count - num} more</color>");
			}
			ApplyColumnsToUI(num2);
			BuildPlainTextContent(num2);
		}

		private void BuildPlainTextContent(int columnsNeeded)
		{
			_sb.Clear();
			for (int i = 0; i < columnsNeeded; i++)
			{
				string input = _columnBuilders[i].ToString();
				input = Regex.Replace(input, "<[^>]+>", "");
				_sb.Append(input);
			}
			_lastPlainTextContent = _sb.ToString();
		}

		private void ApplyColumnsToUI(int columnsNeeded)
		{
			float num = (float)Screen.width - 20f;
			float num2 = (float)Screen.height * 0.9f;
			float num3 = (float)columnsNeeded * 300f + (float)(columnsNeeded - 1) * 10f + 20f;
			int num4 = columnsNeeded;
			float num5 = 300f;
			if (num3 > num)
			{
				while (num4 > 1)
				{
					float num6 = (float)num4 * 300f + (float)(num4 - 1) * 10f + 20f;
					if (num6 <= num)
					{
						break;
					}
					num4--;
				}
				if (num4 == 1 && 320f > num)
				{
					num5 = num - 20f - 20f;
				}
			}
			for (int i = 0; i < _columnTexts.Count; i++)
			{
				bool flag = i < num4;
				((Component)_columnTexts[i]).gameObject.SetActive(flag);
				if (flag)
				{
					((TMP_Text)_columnTexts[i]).text = _columnBuilders[i].ToString();
					_columnLayoutElements[i].minWidth = num5;
					_columnLayoutElements[i].preferredWidth = num5;
					_columnLayoutElements[i].preferredHeight = num2 - 50f;
				}
			}
		}

		private void AppendGameObjectInfo(StringBuilder sb, GameObject go, int index)
		{
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_024b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0250: Unknown result type (might be due to invalid IL or missing references)
			//IL_0252: Unknown result type (might be due to invalid IL or missing references)
			//IL_0254: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0312: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0272: Unknown result type (might be due to invalid IL or missing references)
			//IL_0281: Unknown result type (might be due to invalid IL or missing references)
			//IL_0290: Unknown result type (might be due to invalid IL or missing references)
			//IL_029f: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0206: Unknown result type (might be due to invalid IL or missing references)
			//IL_0224: Unknown result type (might be due to invalid IL or missing references)
			//IL_06f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_05f2: Unknown result type (might be due to invalid IL or missing references)
			//IL_05a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_05c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_07bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0aab: Unknown result type (might be due to invalid IL or missing references)
			//IL_08cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b7b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0b87: Unknown result type (might be due to invalid IL or missing references)
			//IL_0bbd: Unknown result type (might be due to invalid IL or missing references)
			//IL_0bc9: Unknown result type (might be due to invalid IL or missing references)
			sb.AppendLine($"<color=#FFD700>[{index}] {((Object)go).name}</color>");
			string hierarchyPath = GetHierarchyPath(go.transform, 5);
			sb.AppendLine("  <color=#A0A0A0>Path: " + hierarchyPath + "</color>");
			RectTransform component = go.GetComponent<RectTransform>();
			if ((Object)(object)component != (Object)null)
			{
				sb.AppendLine("  <color=#C080C0>RectTransform:</color>");
				sb.AppendLine($"    <color=#808080>sizeDelta: {component.sizeDelta}</color>");
				sb.AppendLine($"    <color=#808080>anchorMin: {component.anchorMin} anchorMax: {component.anchorMax}</color>");
				sb.AppendLine($"    <color=#808080>pivot: {component.pivot}</color>");
				sb.AppendLine($"    <color=#808080>offsetMin: {component.offsetMin} offsetMax: {component.offsetMax}</color>");
			}
			Image component2 = go.GetComponent<Image>();
			if ((Object)(object)component2 != (Object)null)
			{
				sb.Append("  <color=#80FF80>Image:</color> ");
				if ((Object)(object)component2.sprite != (Object)null)
				{
					sb.AppendLine("<color=#FFFFFF>" + ((Object)component2.sprite).name + "</color>");
					if ((Object)(object)component2.sprite.texture != (Object)null)
					{
						Texture2D texture = component2.sprite.texture;
						sb.AppendLine($"    <color=#808080>Texture: {((Object)texture).name} ({((Texture)texture).width}x{((Texture)texture).height})</color>");
						sb.AppendLine($"    <color=#808080>TexFormat: {texture.format}, isReadable: {((Texture)texture).isReadable}</color>");
						if (((Texture)texture).isReadable)
						{
							try
							{
								Color pixel = texture.GetPixel(((Texture)texture).width / 2, ((Texture)texture).height / 2);
								Color pixel2 = texture.GetPixel(2, 2);
								sb.AppendLine("    <color=#AAFFAA>CenterPixel: " + ColorToHex(pixel) + "</color>");
								sb.AppendLine("    <color=#AAFFAA>CornerPixel: " + ColorToHex(pixel2) + "</color>");
							}
							catch
							{
							}
						}
					}
					Vector4 border = component2.sprite.border;
					if (border != Vector4.zero)
					{
						sb.AppendLine($"    <color=#808080>Border(9-slice): L={border.x} B={border.y} R={border.z} T={border.w}</color>");
					}
					sb.AppendLine($"    <color=#808080>SpriteRect: {component2.sprite.rect}</color>");
				}
				else
				{
					sb.AppendLine("<color=#808080>(no sprite)</color>");
				}
				sb.AppendLine("    <color=#808080>Color: " + ColorToHex(((Graphic)component2).color) + "</color>");
				sb.AppendLine($"    <color=#808080>Type: {component2.type}, RaycastTarget: {((Graphic)component2).raycastTarget}</color>");
				sb.AppendLine("    <color=#808080>Material: " + (((Object)(object)((Graphic)component2).material != (Object)null) ? ((Object)((Graphic)component2).material).name : "null") + "</color>");
				sb.AppendLine($"    <color=#808080>maskable: {((MaskableGraphic)component2).maskable}, preserveAspect: {component2.preserveAspect}</color>");
				TranslucentImage component3 = go.GetComponent<TranslucentImage>();
				if ((Object)(object)component3 != (Object)null)
				{
					sb.AppendLine("    <color=#FF80FF>== TranslucentImage ==</color>");
					sb.AppendLine($"    <color=#FF80FF>vibrancy: {component3.vibrancy:F3}</color>");
					sb.AppendLine($"    <color=#FF80FF>brightness: {component3.brightness:F3}</color>");
					sb.AppendLine($"    <color=#FF80FF>flatten: {component3.flatten:F3}</color>");
					sb.AppendLine($"    <color=#FF80FF>spriteBlending: {component3.spriteBlending:F3}</color>");
					Material material = ((Graphic)component3).material;
					sb.AppendLine("    <color=#FF80FF>TI.material: " + (((Object)(object)material != (Object)null) ? ((Object)material).name : "null") + "</color>");
					if ((Object)(object)material != (Object)null && (Object)(object)material.shader != (Object)null)
					{
						sb.AppendLine("    <color=#FF80FF>TI.shader: " + ((Object)material.shader).name + "</color>");
					}
					sb.AppendLine("    <color=#FF80FF>TI.materialForRendering: " + (((Object)(object)((Graphic)component3).materialForRendering != (Object)null) ? ((Object)((Graphic)component3).materialForRendering).name : "null") + "</color>");
				}
			}
			int childCount = go.transform.childCount;
			if (childCount > 0)
			{
				sb.AppendLine($"  <color=#FFAA00>Children ({childCount}):</color>");
				for (int i = 0; i < Mathf.Min(childCount, 8); i++)
				{
					Transform child = go.transform.GetChild(i);
					Image component4 = ((Component)child).GetComponent<Image>();
					RectTransform component5 = ((Component)child).GetComponent<RectTransform>();
					sb.Append("    <color=#D0D000>" + ((Object)child).name + "</color>");
					if ((Object)(object)component4 != (Object)null)
					{
						string text = (((Object)(object)component4.sprite != (Object)null) ? ((Object)component4.sprite).name : "(no sprite)");
						sb.Append(" -> <color=#80FF80>" + text + "</color>");
						sb.Append(" color:" + ColorToHex(((Graphic)component4).color));
						sb.Append($" type:{component4.type}");
					}
					if ((Object)(object)component5 != (Object)null)
					{
						sb.Append($" size:{component5.sizeDelta}");
					}
					sb.AppendLine();
				}
				if (childCount > 8)
				{
					sb.AppendLine($"    <color=#808080>... and {childCount - 8} more children</color>");
				}
			}
			if ((Object)(object)go.transform.parent != (Object)null)
			{
				GameObject gameObject = ((Component)go.transform.parent).gameObject;
				Image component6 = gameObject.GetComponent<Image>();
				sb.AppendLine("  <color=#60A060>Parent: " + ((Object)gameObject).name + "</color>");
				if ((Object)(object)component6 != (Object)null)
				{
					string text2 = (((Object)(object)component6.sprite != (Object)null) ? ((Object)component6.sprite).name : "(no sprite)");
					sb.AppendLine("    <color=#60A060>Parent Image: " + text2 + " color:" + ColorToHex(((Graphic)component6).color) + "</color>");
				}
				if ((Object)(object)go.transform.parent.parent != (Object)null)
				{
					GameObject gameObject2 = ((Component)go.transform.parent.parent).gameObject;
					sb.AppendLine("    <color=#508050>Grandparent: " + ((Object)gameObject2).name + "</color>");
					Image component7 = gameObject2.GetComponent<Image>();
					if ((Object)(object)component7 != (Object)null && (Object)(object)component7.sprite != (Object)null)
					{
						sb.AppendLine("    <color=#508050>GP Image: " + ((Object)component7.sprite).name + " color:" + ColorToHex(((Graphic)component7).color) + "</color>");
					}
				}
			}
			if ((Object)(object)go.transform.parent != (Object)null)
			{
				int siblingIndex = go.transform.GetSiblingIndex();
				int childCount2 = go.transform.parent.childCount;
				sb.AppendLine($"  <color=#6060A0>Sibling index: {siblingIndex}/{childCount2}</color>");
				for (int j = 0; j < Mathf.Min(childCount2, 6); j++)
				{
					if (j != siblingIndex)
					{
						Transform child2 = go.transform.parent.GetChild(j);
						Image component8 = ((Component)child2).GetComponent<Image>();
						if ((Object)(object)component8 != (Object)null)
						{
							string text3 = (((Object)(object)component8.sprite != (Object)null) ? ((Object)component8.sprite).name : "(no sprite)");
							sb.AppendLine($"    <color=#6060A0>Sib[{j}] {((Object)child2).name}: {text3} {ColorToHex(((Graphic)component8).color)}</color>");
						}
					}
				}
			}
			RawImage component9 = go.GetComponent<RawImage>();
			if ((Object)(object)component9 != (Object)null)
			{
				sb.Append("  <color=#80FF80>RawImage:</color> ");
				if ((Object)(object)component9.texture != (Object)null)
				{
					sb.AppendLine("<color=#FFFFFF>" + ((Object)component9.texture).name + "</color>");
				}
				else
				{
					sb.AppendLine("<color=#808080>(no texture)</color>");
				}
			}
			TextMeshProUGUI component10 = go.GetComponent<TextMeshProUGUI>();
			if ((Object)(object)component10 != (Object)null)
			{
				string text4 = ((TMP_Text)component10).text ?? "";
				if (text4.Length > 30)
				{
					text4 = text4.Substring(0, 30) + "...";
				}
				text4 = text4.Replace("\n", "\\n").Replace("<", "[").Replace(">", "]");
				sb.AppendLine("  <color=#80FFFF>TMP:</color> \"" + text4 + "\"");
				if ((Object)(object)((TMP_Text)component10).font != (Object)null)
				{
					sb.AppendLine("    <color=#808080>Font: " + ((Object)((TMP_Text)component10).font).name + "</color>");
				}
			}
			Button component11 = go.GetComponent<Button>();
			if ((Object)(object)component11 != (Object)null)
			{
				sb.AppendLine("  <color=#FFFF80>Button</color>");
			}
			UiComponentRenderer component12 = go.GetComponent<UiComponentRenderer>();
			if ((Object)(object)component12 != (Object)null)
			{
				sb.AppendLine("  <color=#FF8080>UiComponentRenderer</color>");
			}
			Canvas component13 = go.GetComponent<Canvas>();
			if ((Object)(object)component13 != (Object)null)
			{
				sb.AppendLine($"  <color=#8080FF>Canvas: sortOrder={component13.sortingOrder}, renderMode={component13.renderMode}</color>");
			}
			CanvasGroup component14 = go.GetComponent<CanvasGroup>();
			if ((Object)(object)component14 != (Object)null)
			{
				sb.AppendLine($"  <color=#8080FF>CanvasGroup: alpha={component14.alpha}, blocksRaycasts={component14.blocksRaycasts}</color>");
			}
			Mask component15 = go.GetComponent<Mask>();
			if ((Object)(object)component15 != (Object)null)
			{
				sb.AppendLine($"  <color=#FF80FF>Mask present, enabled={((Behaviour)component15).enabled}</color>");
			}
			RectMask2D component16 = go.GetComponent<RectMask2D>();
			if ((Object)(object)component16 != (Object)null)
			{
				sb.AppendLine("  <color=#FF80FF>RectMask2D present</color>");
			}
			Shadow component17 = go.GetComponent<Shadow>();
			if ((Object)(object)component17 != (Object)null)
			{
				sb.AppendLine($"  <color=#FFA0A0>Shadow: color={ColorToHex(component17.effectColor)}, dist={component17.effectDistance}</color>");
			}
			Outline component18 = go.GetComponent<Outline>();
			if ((Object)(object)component18 != (Object)null)
			{
				sb.AppendLine($"  <color=#FFA0A0>Outline: color={ColorToHex(((Shadow)component18).effectColor)}, dist={((Shadow)component18).effectDistance}</color>");
			}
			Component[] components = go.GetComponents<Component>();
			Component[] array = components;
			foreach (Component val in array)
			{
				if ((Object)(object)val == (Object)null)
				{
					continue;
				}
				string name = ((object)val).GetType().Name;
				switch (name)
				{
				case "RectTransform":
				case "CanvasRenderer":
				case "Image":
				case "RawImage":
				case "Button":
				case "TextMeshProUGUI":
				case "Canvas":
				case "UiComponentRenderer":
				case "TranslucentImage":
				case "CanvasGroup":
				case "Mask":
				case "RectMask2D":
				case "Shadow":
					continue;
				}
				if (!(name == "Outline") && (name.Contains("Scroll") || name.Contains("Layout") || name.Contains("Mask") || name.Contains("Input") || name.Contains("Toggle") || name.Contains("Slider") || name.Contains("Dropdown") || name.Contains("Editor") || name.Contains("Panel") || name.Contains("Window") || name.Contains("Fitter") || name.Contains("Anim")))
				{
					sb.AppendLine("  <color=#C0C0C0>" + name + "</color>");
				}
			}
			sb.AppendLine();
		}

		private string GetHierarchyPath(Transform t, int maxDepth)
		{
			if ((Object)(object)t == (Object)null || maxDepth <= 0)
			{
				return "";
			}
			List<string> list = new List<string>();
			Transform val = t;
			int num = 0;
			while ((Object)(object)val != (Object)null && num < maxDepth)
			{
				list.Insert(0, ((Object)val).name);
				val = val.parent;
				num++;
			}
			if ((Object)(object)val != (Object)null)
			{
				list.Insert(0, "...");
			}
			return string.Join("/", list);
		}

		private string ColorToHex(Color c)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return "#" + ColorUtility.ToHtmlStringRGBA(c);
		}
	}
}
namespace StationpediaAscended.Data
{
	public static class DaylightSensorGuideLoader
	{
		private static DeviceDescriptions _guideDescriptions;

		private static bool _isRegistered;

		public static void RegisterDaylightSensorGuidePage()
		{
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Expected O, but got Unknown
			if (_isRegistered)
			{
				return;
			}
			try
			{
				DeviceDescriptions deviceDescriptions = GuideLoader.LoadGuide("daylight-sensor-guide.md", "DaylightSensorGuide", "Daylight Sensor Guide");
				if (deviceDescriptions == null)
				{
					ManualLogSource log = StationpediaAscendedMod.Log;
					if (log != null)
					{
						log.LogWarning((object)"[DaylightSensorGuideLoader] Failed to load daylight sensor guide");
					}
					return;
				}
				_guideDescriptions = deviceDescriptions;
				StationpediaPage val = new StationpediaPage
				{
					Key = "DaylightSensorGuide",
					Title = "Daylight Sensor Guide"
				};
				val.Text = "A comprehensive guide to the Daylight Sensor, covering solar tracking, logic integration, and automation setups.\n\nThis guide includes modes, logic variables, panel tracking setups, and advanced automation examples.\n\n<i>Expand the sections below to learn more.</i>";
				Stationpedia.Register(val, false);
				_isRegistered = true;
				ConsoleWindow.Print("[Stationpedia Ascended] Daylight Sensor Guide page registered", ConsoleColor.White, false, true);
			}
			catch (Exception ex)
			{
				ManualLogSource log2 = StationpediaAscendedMod.Log;
				if (log2 != null)
				{
					log2.LogError((object)("Error registering Daylight Sensor Guide: " + ex.Message));
				}
			}
		}

		public static DeviceDescriptions GetDaylightSensorGuideDescriptions()
		{
			if (_guideDescriptions == null)
			{
				DeviceDescriptions guideDescriptions = GuideLoader.LoadGuide("daylight-sensor-guide.md", "DaylightSensorGuide", "Daylight Sensor Guide");
				_guideDescriptions = guideDescriptions;
			}
			return _guideDescriptions;
		}

		public static void Clear()
		{
			_guideDescriptions = null;
			_isRegistered = false;
		}
	}
	public static class GameMechanicsRegistry
	{
		public static List<string> GameMechanicsPages { get; } = new List<string>();

		public static void RegisterPage(string pageKey)
		{
			if (!string.IsNullOrEmpty(pageKey) && !GameMechanicsPages.Contains(pageKey))
			{
				GameMechanicsPages.Add(pageKey);
			}
		}

		public static void Clear()
		{
			GameMechanicsPages.Clear();
		}

		public static bool IsGameMechanicsPage(string pageKey)
		{
			return GameMechanicsPages.Contains(pageKey);
		}
	}
	public static class GuideLoader
	{
		private static readonly Dictionary<string, string> DeviceNameMapping = new Dictionary<string, string>
		{
			{ "Daylight Sensor", "ThingStructureDaylightSensor" },
			{ "Solar Panel", "ThingStructureSolarPanel" },
			{ "Solar Panel (Dual)", "ThingStructureSolarPanelDual" },
			{ "StructureSolarPanelDual", "ThingStructureSolarPanelDual" },
			{ "Logic Writer", "ThingStructureLogicWriter" },
			{ "Batch Writer", "ThingStructureLogicBatchWriter" },
			{ "Logic Memory", "ThingStructureLogicMemory" },
			{ "Battery", "ThingStructureBatterySmall" },
			{ "Programmable Chip", "ThingMotherboardProgrammableChip" },
			{ "IC10", "ThingMotherboardProgrammableChip" },
			{ "Air Conditioner", "ThingStructureAirConditioner" },
			{ "Tablet", "ThingItemAdvancedTablet" }
		};

		public static DeviceDescriptions LoadGuide(string filename, string pageKey, string displayTitle)
		{
			if (string.IsNullOrEmpty(filename) || string.IsNullOrEmpty(pageKey))
			{
				ManualLogSource log = StationpediaAscendedMod.Log;
				if (log != null)
				{
					log.LogWarning((object)("[GuideLoader] Invalid parameters: filename='" + filename + "', pageKey='" + pageKey + "'"));
				}
				return null;
			}
			string text = LoadMarkdownFile(filename);
			if (string.IsNullOrEmpty(text))
			{
				ManualLogSource log2 = StationpediaAscendedMod.Log;
				if (log2 != null)
				{
					log2.LogWarning((object)("[GuideLoader] Failed to load guide file: " + filename));
				}
				return null;
			}
			return ParseMarkdownToGuide(text, pageKey, displayTitle);
		}

		private static string LoadMarkdownFile(string filename)
		{
			List<string> list = new List<string>();
			list.Add("C:\\Dev\\12-17-25 Stationeers Respawn Update Code\\StationpediaAscended\\mod\\Guides\\" + filename);
			string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			if (!string.IsNullOrEmpty(directoryName))
			{
				list.Add(Path.Combine(directoryName, "Guides", filename));
			}
			if (!string.IsNullOrEmpty(Paths.BepInExRootPath))
			{
				list.Add(Path.Combine(Paths.BepInExRootPath, "scripts", "Guides", filename));
			}
			foreach (string item in list)
			{
				if (string.IsNullOrEmpty(item) || !File.Exists(item))
				{
					continue;
				}
				try
				{
					return File.ReadAllText(item, Encoding.UTF8);
				}
				catch (Exception ex)
				{
					ManualLogSource log = StationpediaAscendedMod.Log;
					if (log != null)
					{
						log.LogWarning((object)("[GuideLoader] Failed to read file " + item + ": " + ex.Message));
					}
				}
			}
			return null;
		}

		public static DeviceDescriptions ParseMarkdownToGuide(string content, string pageKey, string displayTitle)
		{
			if (string.IsNullOrEmpty(content) || string.IsNullOrEmpty(pageKey))
			{
				return null;
			}
			DeviceDescriptions deviceDescriptions = new DeviceDescriptions
			{
				deviceKey = pageKey,
				displayName = (displayTitle ?? pageKey),
				generateToc = true,
				operationalDetails = new List<OperationalDetail>()
			};
			List<OperationalDetail> operationalDetails = ParseMarkdownSections(content);
			deviceDescriptions.operationalDetails = operationalDetails;
			return deviceDescriptions;
		}

		private static List<OperationalDetail> ParseMarkdownSections(string content)
		{
			List<OperationalDetail> list = new List<OperationalDetail>();
			if (string.IsNullOrEmpty(content))
			{
				return list;
			}
			string[] array = content.Split(new string[3] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
			OperationalDetail operationalDetail = null;
			OperationalDetail operationalDetail2 = null;
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = false;
			string text = string.Empty;
			for (int i = 0; i < array.Length; i++)
			{
				string text2 = array[i];
				if (text2.TrimStart().StartsWith("```"))
				{
					if (!flag)
					{
						flag = true;
						text = text2.TrimStart().Substring(3).Trim();
						stringBuilder.AppendLine("\n<mspace=0.5em>");
					}
					else
					{
						flag = false;
						stringBuilder.AppendLine("</mspace>\n");
						text = string.Empty;
					}
					continue;
				}
				if (flag)
				{
					string text3 = text2;
					if (text == "mips")
					{
						text3 = FormatMipsCode(text2);
					}
					stringBuilder.AppendLine("  " + text3);
					continue;
				}
				if (IsTableLine(text2))
				{
					if (stringBuilder.Length > 0)
					{
						if (operationalDetail2 != null)
						{
							SaveCurrentContent(operationalDetail2, stringBuilder);
						}
						else if (operationalDetail != null)
						{
							SaveCurrentContent(operationalDetail, stringBuilder);
						}
						stringBuilder.Clear();
					}
					List<string> list2 = new List<string> { text2 };
					for (i++; i < array.Length && IsTableLine(array[i]); i++)
					{
						list2.Add(array[i]);
					}
					i--;
					string value = ParseMarkdownTable(list2);
					if (!string.IsNullOrEmpty(value))
					{
						stringBuilder.AppendLine(value);
					}
					continue;
				}
				Match match = Regex.Match(text2, "^#\\s+(.+)$");
				if (match.Success)
				{
					if (operationalDetail != null)
					{
						if (operationalDetail2 != null)
						{
							SaveCurrentContent(operationalDetail2, stringBuilder);
							operationalDetail2 = null;
						}
						else
						{
							SaveCurrentContent(operationalDetail, stringBuilder);
						}
					}
					string title = match.Groups[1].Value.Trim();
					operationalDetail = new OperationalDetail
					{
						title = title,
						tocId = GenerateTocId(title),
						collapsible = true,
						children = new List<OperationalDetail>()
					};
					list.Add(operationalDetail);
					operationalDetail2 = null;
					stringBuilder.Clear();
					continue;
				}
				Match match2 = Regex.Match(text2, "^##\\s+(.+)$");
				if (match2.Success && operationalDetail != null)
				{
					if (operationalDetail2 != null)
					{
						SaveCurrentContent(operationalDetail2, stringBuilder);
					}
					string title2 = match2.Groups[1].Value.Trim();
					operationalDetail2 = new OperationalDetail
					{
						title = title2,
						tocId = GenerateTocId(title2),
						collapsible = true
					};
					operationalDetail.children.Add(operationalDetail2);
					stringBuilder.Clear();
				}
				else if (!(text2.Trim() == "---") && !(text2.Trim() == "***") && !(text2.Trim() == "___"))
				{
					string value2 = ConvertMarkdownLine(text2);
					if (!string.IsNullOrWhiteSpace(value2) || stringBuilder.Length > 0)
					{
						stringBuilder.AppendLine(value2);
					}
				}
			}
			if (operationalDetail != null)
			{
				if (operationalDetail2 != null)
				{
					SaveCurrentContent(operationalDetail2, stringBuilder);
				}
				else
				{
					SaveCurrentContent(operationalDetail, stringBuilder);
				}
			}
			return list;
		}

		private static void SaveCurrentContent(OperationalDetail detail, StringBuilder content)
		{
			if (detail != null && content.Length != 0)
			{
				string text = content.ToString().Trim();
				if (!string.IsNullOrEmpty(text))
				{
					detail.description = text;
				}
			}
		}

		private static string ConvertMarkdownLine(string line)
		{
			if (string.IsNullOrEmpty(line))
			{
				return line;
			}
			line = line.TrimEnd();
			line = Regex.Replace(line, "\\*\\*([^*]+)\\*\\*", "<b>$1</b>");
			line = Regex.Replace(line, "\\*([^*]+)\\*", "<i>$1</i>");
			line = Regex.Replace(line, "`([^`]+)`", "<color=#88CCFF>$1</color>");
			line = ConvertDeviceReferences(line);
			if (line.TrimStart().StartsWith("- "))
			{
				int count = line.Length - line.TrimStart().Length;
				string text = new string(' ', count);
				line = text + "• " + line.TrimStart().Substring(2);
			}
			line = Regex.Replace(line, "\\[([^\\]]+)\\]\\([^)]+\\)", "$1");
			return line;
		}

		public static string ConvertDeviceReferences(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				return text;
			}
			foreach (KeyValuePair<string, string> item in DeviceNameMapping)
			{
				Regex regex = new Regex(Regex.Escape(item.Key), RegexOptions.IgnoreCase);
				text = regex.Replace(text, "{THING:" + item.Value + "}");
			}
			return text;
		}

		public static string FormatMipsCode(string code)
		{
			if (string.IsNullOrEmpty(code))
			{
				return code;
			}
			string[] array = new string[6] { "alias", "define", "yield", "\\bj\\b", "jal", "jr" };
			string[] array2 = array;
			foreach (string text in array2)
			{
				code = Regex.Replace(code, "\\b" + text + "\\b", "<color=#569CD6>" + text + "</color>", RegexOptions.IgnoreCase);
			}
			string[] array3 = new string[5] { "r\\d{1,2}", "sp", "ra", "db", "d[0-5]" };
			string[] array4 = array3;
			foreach (string text2 in array4)
			{
				code = Regex.Replace(code, "\\b" + text2 + "\\b", "<color=#9CDCFE>${register}</color>", RegexOptions.IgnoreCase);
			}
			string[] array5 = new string[23]
			{
				"\\bl\\b", "\\bs\\b", "add", "sub", "mul", "div", "mod", "beq", "bne", "bgt",
				"blt", "bgtz", "bltz", "addi", "subi", "muli", "divi", "modi", "slt", "slti",
				"max", "min", "move"
			};
			string[] array6 = array5;
			foreach (string text3 in array6)
			{
				code = Regex.Replace(code, "\\b" + text3 + "\\b", "<color=#C586C0>" + text3 + "</color>", RegexOptions.IgnoreCase);
			}
			code = Regex.Replace(code, "^(\\s*)(\\w+):", "$1<color=#DCDCAA>$2:</color>", RegexOptions.Multiline);
			code = Regex.Replace(code, "#.*$", (Match m) => "<color=#6A9955>" + m.Value + "</color>", RegexOptions.Multiline);
			code = Regex.Replace(code, "\\b(\\d+|0x[0-9A-Fa-f]+)\\b", "<color=#B5CEA8>$1</color>");
			return code;
		}

		private static string GenerateTocId(string title)
		{
			if (string.IsNullOrEmpty(title))
			{
				return "section";
			}
			string input = title.ToLowerInvariant();
			input = Regex.Replace(input, "[^a-z0-9]+", "_");
			input = input.Trim('_');
			return string.IsNullOrEmpty(input) ? "section" : input;
		}

		private static bool IsTableLine(string line)
		{
			if (string.IsNullOrEmpty(line))
			{
				return false;
			}
			return line.TrimStart().StartsWith("|") && line.Contains("|");
		}

		private static bool IsSeparatorLine(string line)
		{
			if (string.IsNullOrEmpty(line))
			{
				return false;
			}
			string text = line.Trim();
			if (!text.StartsWith("|") || !text.EndsWith("|"))
			{
				return false;
			}
			string[] array = text.Split('|');
			string[] array2 = array;
			foreach (string text2 in array2)
			{
				string text3 = text2.Trim();
				if (!string.IsNullOrEmpty(text3) && !Regex.IsMatch(text3, "^-+(\\s*:-?|:-?\\s*)?$"))
				{
					return false;
				}
			}
			return true;
		}

		private static List<string> ExtractTableCells(string line)
		{
			List<string> list = new List<string>();
			if (string.IsNullOrEmpty(line))
			{
				return list;
			}
			string[] array = line.Split('|');
			for (int i = 1; i < array.Length - 1; i++)
			{
				string input = array[i].Trim();
				input = Regex.Replace(input, "\\*\\*([^*]+)\\*\\*", "$1");
				input = Regex.Replace(input, "\\*([^*]+)\\*", "$1");
				input = Regex.Replace(input, "`([^`]+)`", "$1");
				list.Add(input);
			}
			return list;
		}

		private static string ParseMarkdownTable(List<string> tableLines)
		{
			if (tableLines == null || tableLines.Count < 2)
			{
				return string.Empty;
			}
			StringBuilder stringBuilder = new StringBuilder();
			List<string> list = null;
			list = ExtractTableCells(tableLines[0]);
			if (list.Count == 0)
			{
				return string.Empty;
			}
			int num = 1;
			for (int i = 1; i < tableLines.Count; i++)
			{
				if (IsSeparatorLine(tableLines[i]))
				{
					num = i + 1;
					break;
				}
			}
			bool flag = true;
			for (int j = num; j < tableLines.Count; j++)
			{
				List<string> list2 = ExtractTableCells(tableLines[j]);
				if (list2.Count != 0)
				{
					if (!flag)
					{
						stringBuilder.AppendLine();
					}
					flag = false;
					string text = list2[0];
					stringBuilder.AppendLine("<b>" + text + "</b>");
					for (int k = 1; k < list2.Count && k < list.Count; k++)
					{
						string text2 = list[k];
						string text3 = list2[k];
						stringBuilder.AppendLine("    " + text2 + ": " + text3);
					}
				}
			}
			return stringBuilder.ToString().TrimEnd();
		}
	}
	public static class JsonGuideLoader
	{
		private static Dictionary<string, GuideDescription> _loadedGuides = new Dictionary<string, GuideDescription>();

		private static bool _guidesRegistered = false;

		public static void LoadGuides(DescriptionsRoot data)
		{
			_loadedGuides.Clear();
			if (data?.guides == null || data.guides.Count == 0)
			{
				ConsoleWindow.Print("[Stationpedia Ascended] No custom guides found in descriptions.json", ConsoleColor.White, false, true);
				return;
			}
			foreach (GuideDescription guide in data.guides)
			{
				if (string.IsNullOrEmpty(guide.guideKey))
				{
					ConsoleWindow.Print("[Stationpedia Ascended] Skipping guide with no guideKey", ConsoleColor.White, false, true);
					continue;
				}
				_loadedGuides[guide.guideKey] = guide;
				ConsoleWindow.Print("[Stationpedia Ascended] Loaded guide: " + (guide.displayName ?? guide.guideKey), ConsoleColor.White, false, true);
			}
			ConsoleWindow.Print($"[Stationpedia Ascended] Loaded {_loadedGuides.Count} custom guides from JSON", ConsoleColor.White, false, true);
		}

		public static void RegisterGuidePages()
		{
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Expected O, but got Unknown
			if (_guidesRegistered)
			{
				return;
			}
			foreach (KeyValuePair<string, GuideDescription> loadedGuide in _loadedGuides)
			{
				try
				{
					GuideDescription value = loadedGuide.Value;
					StationpediaPage val = new StationpediaPage
					{
						Key = value.guideKey,
						Title = (value.displayName ?? value.guideKey)
					};
					val.Text = value.pageDescription ?? "";
					Stationpedia.Register(val, false);
					ConsoleWindow.Print("[Stationpedia Ascended] Registered guide page: " + value.guideKey, ConsoleColor.White, false, true);
				}
				catch (Exception ex)
				{
					ManualLogSource log = StationpediaAscendedMod.Log;
					if (log != null)
					{
						log.LogError((object)("Error registering guide " + loadedGuide.Key + ": " + ex.Message));
					}
				}
			}
			_guidesRegistered = true;
		}

		public static List<GuideDescription> GetAllGuides()
		{
			return (from g in _loadedGuides.Values
				orderby g.sortOrder, g.displayName ?? g.guideKey
				select g).ToList();
		}

		public static GuideDescription GetGuide(string guideKey)
		{
			if (_loadedGuides.TryGetValue(guideKey, out var value))
			{
				return value;
			}
			return null;
		}

		public static bool HasGuide(string guideKey)
		{
			return _loadedGuides.ContainsKey(guideKey);
		}

		public static DeviceDescriptions ToDeviceDescriptions(GuideDescription guide)
		{
			if (guide == null)
			{
				return null;
			}
			return new DeviceDescriptions
			{
				deviceKey = guide.guideKey,
				displayName = guide.displayName,
				pageDescription = guide.pageDescription,
				pageDescriptionPrepend = guide.pageDescriptionPrepend,
				pageDescriptionAppend = guide.pageDescriptionAppend,
				pageImage = guide.pageImage,
				operationalDetails = guide.operationalDetails,
				operationalDetailsTitleColor = guide.operationalDetailsTitleColor,
				generateToc = guide.generateToc,
				tocTitle = guide.tocTitle,
				tocFlat = guide.tocFlat,
				operationalDetailsBackgroundColor = guide.operationalDetailsBackgroundColor
			};
		}

		public static void Clear()
		{
			_loadedGuides.Clear();
			_guidesRegistered = false;
		}

		public static Color GetButtonColor(GuideDescription guide)
		{
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0110: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_0108: Unknown result type (might be due to invalid IL or missing references)
			//IL_010d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
			if (guide == null || string.IsNullOrEmpty(guide.buttonColor))
			{
				return new Color(0f, 0.54f, 0.9f, 1f);
			}
			string text = guide.buttonColor.ToLower();
			switch (text)
			{
			case "blue":
				return new Color(0f, 0.54f, 0.9f, 1f);
			case "orange":
				return new Color(1f, 0.42f, 0.09f, 1f);
			case "green":
				return new Color(0.27f, 0.68f, 0.51f, 1f);
			default:
			{
				Color result = default(Color);
				if (text.StartsWith("#") && ColorUtility.TryParseHtmlString(text, ref result))
				{
					return result;
				}
				return new Color(0f, 0.54f, 0.9f, 1f);
			}
			}
		}
	}
	public static class JsonMechanicsLoader
	{
		private static Dictionary<string, GuideDescription> _loadedMechanics = new Dictionary<string, GuideDescription>();

		private static bool _mechanicsRegistered = false;

		public static void LoadMechanics(DescriptionsRoot data)
		{
			_loadedMechanics.Clear();
			if (data?.mechanics == null || data.mechanics.Count == 0)
			{
				ConsoleWindow.Print("[Stationpedia Ascended] No game mechanics found in descriptions.json", ConsoleColor.White, false, true);
				return;
			}
			foreach (GuideDescription mechanic in data.mechanics)
			{
				if (string.IsNullOrEmpty(mechanic.guideKey))
				{
					ConsoleWindow.Print("[Stationpedia Ascended] Skipping mechanic with no guideKey", ConsoleColor.White, false, true);
					continue;
				}
				_loadedMechanics[mechanic.guideKey] = mechanic;
				ConsoleWindow.Print("[Stationpedia Ascended] Loaded mechanic: " + (mechanic.displayName ?? mechanic.guideKey), ConsoleColor.White, false, true);
			}
			ConsoleWindow.Print($"[Stationpedia Ascended] Loaded {_loadedMechanics.Count} game mechanics from JSON", ConsoleColor.White, false, true);
		}

		public static void RegisterMechanicsPages()
		{
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Expected O, but got Unknown
			if (_mechanicsRegistered)
			{
				return;
			}
			foreach (KeyValuePair<string, GuideDescription> loadedMechanic in _loadedMechanics)
			{
				try
				{
					GuideDescription value = loadedMechanic.Value;
					StationpediaPage val = new StationpediaPage
					{
						Key = value.guideKey,
						Title = (value.displayName ?? value.guideKey)
					};
					val.Text = value.pageDescription ?? "";
					Stationpedia.Register(val, false);
					GameMechanicsRegistry.RegisterPage(value.guideKey);
					ConsoleWindow.Print("[Stationpedia Ascended] Registered mechanic page: " + value.guideKey, ConsoleColor.White, false, true);
				}
				catch (Exception ex)
				{
					ManualLogSource log = StationpediaAscendedMod.Log;
					if (log != null)
					{
						log.LogError((object)("Error registering mechanic " + loadedMechanic.Key + ": " + ex.Message));
					}
				}
			}
			_mechanicsRegistered = true;
		}

		public static List<GuideDescription> GetAllMechanics()
		{
			return (from m in _loadedMechanics.Values
				orderby m.sortOrder, m.displayName ?? m.guideKey
				select m).ToList();
		}

		public static GuideDescription GetMechanic(string mechanicKey)
		{
			if (_loadedMechanics.TryGetValue(mechanicKey, out var value))
			{
				return value;
			}
			return null;
		}

		public static bool HasMechanic(string mechanicKey)
		{
			return _loadedMechanics.ContainsKey(mechanicKey);
		}

		public static DeviceDescriptions ToDeviceDescriptions(GuideDescription mechanic)
		{
			if (mechanic == null)
			{
				return null;
			}
			return new DeviceDescriptions
			{
				deviceKey = mechanic.guideKey,
				displayName = mechanic.displayName,
				pageDescription = mechanic.pageDescription,
				pageDescriptionPrepend = mechanic.pageDescriptionPrepend,
				pageDescriptionAppend = mechanic.pageDescriptionAppend,
				pageImage = mechanic.pageImage,
				operationalDetails = mechanic.operationalDetails,
				operationalDetailsTitleColor = mechanic.operationalDetailsTitleColor,
				generateToc = mechanic.generateToc,
				tocTitle = mechanic.tocTitle,
				tocFlat = mechanic.tocFlat,
				operationalDetailsBackgroundColor = mechanic.operationalDetailsBackgroundColor
			};
		}

		public static void Clear()
		{
			_loadedMechanics.Clear();
			_mechanicsRegistered = false;
		}

		public static Color GetButtonColor(GuideDescription mechanic)
		{
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0110: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
			//IL_0108: Unknown result type (might be due to invalid IL or missing references)
			//IL_010d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
			if (mechanic == null || string.IsNullOrEmpty(mechanic.buttonColor))
			{
				return new Color(0f, 0.54f, 0.9f, 1f);
			}
			string text = mechanic.buttonColor.ToLower();
			switch (text)
			{
			case "blue":
				return new Color(0f, 0.54f, 0.9f, 1f);
			case "orange":
				return new Color(1f, 0.42f, 0.09f, 1f);
			case "green":
				return new Color(0.27f, 0.68f, 0.51f, 1f);
			default:
			{
				Color result = default(Color);
				if (text.StartsWith("#") && ColorUtility.TryParseHtmlString(text, ref result))
				{
					return result;
				}
				return new Color(0f, 0.54f, 0.9f, 1f);
			}
			}
		}
	}
	[Serializable]
	public class DescriptionsRoot
	{
		public string version;

		public List<DeviceDescriptions> devices;

		public List<GuideDescription> guides;

		public List<GuideDescription> mechanics;

		public GenericDescriptionsData genericDescriptions;
	}
	[Serializable]
	public class GuideDescription
	{
		public string guideKey;

		public string displayName;

		public string pageDescription;

		public string pageDescriptionPrepend;

		public string pageDescriptionAppend;

		public string pageImage;

		[JsonProperty("operationalDetails")]
		public List<OperationalDetail> operationalDetails;

		public string operationalDetailsTitleColor;

		[JsonProperty("OperationalDetails")]
		public List<OperationalDetail> OperationalDetailsAlt
		{
			set
			{
				operationalDetails = value;
			}
		}

		public bool generateToc { get; set; } = false;

		public string tocTitle { get; set; }

		public bool tocFlat { get; set; } = false;

		public string operationalDetailsBackgroundColor { get; set; }

		public string buttonColor { get; set; } = "blue";

		public int sortOrder { get; set; } = 100;
	}
	[Serializable]
	public class DeviceDescriptions
	{
		public string deviceKey;

		public string displayName;

		public string pageDescription;

		public string pageDescriptionAppend;

		public string pageDescriptionPrepend;

		public string pageImage;

		public Dictionary<string, LogicDescription> logicDescriptions;

		public Dictionary<string, ModeDescription> modeDescriptions;

		public Dictionary<string, SlotDescription> slotDescriptions;

		public Dictionary<string, VersionDescription> versionDescriptions;

		public Dictionary<string, MemoryDescription> memoryDescriptions;

		[JsonProperty("operationalDetails")]
		public List<OperationalDetail> operationalDetails;

		public string operationalDetailsTitleColor;

		[JsonProperty("OperationalDetails")]
		public List<OperationalDetail> OperationalDetailsAlt
		{
			set
			{
				operationalDetails = value;
			}
		}

		public bool generateToc { get; set; } = false;

		public string tocTitle { get; set; }

		public bool tocFlat { get; set; } = false;

		public string operationalDetailsBackgroundColor { get; set; }
	}
	[Serializable]
	public class LogicDescription
	{
		public string dataType;

		public string range;

		public string description;
	}
	[Serializable]
	public class ModeDescription
	{
		public string modeValue;

		public string description;
	}
	[Serializable]
	public class SlotDescription
	{
		public string slotType;

		public string description;
	}
	[Serializable]
	public class VersionDescription
	{
		public string description;
	}
	[Serializable]
	public class MemoryDescription
	{
		public string opCode;

		public string parameters;

		public string description;

		public string byteLayout;
	}
	[Serializable]
	public class TableRow
	{
		public List<string> cells;
	}
	[Serializable]
	public class OperationalDetail
	{
		public string title;

		public string description;

		public List<OperationalDetail> children;

		public List<string> items;

		public List<string> steps;

		public bool collapsible { get; set; } = true;

		public string tocId { get; set; }

		public string imageFile { get; set; }

		public string backgroundColor { get; set; }

		public string youtubeUrl { get; set; }

		public string youtubeLabel { get; set; }

		public string videoFile { get; set; }

		public List<TableRow> table { get; set; }
	}
	[Serializable]
	public class PropertyDescription
	{
		public string type;

		public string threshold;

		public string description;

		public string formula;
	}
	[Serializable]
	public class GenericDescriptionsData
	{
		public Dictionary<string, string> logic;

		public Dictionary<string, string> slotTypes;

		public Dictionary<string, string> slots;

		public Dictionary<string, string> modes;

		public Dictionary<string, string> versions;

		public Dictionary<string, string> connections;

		public Dictionary<string, MemoryDescription> memory;

		public Dictionary<string, PropertyDescription> properties;

		[JsonExtensionData]
		public Dictionary<string, JToken> AdditionalData { get; set; }
	}
	public static class SurvivalManualLoader
	{
		private class ParsedPart
		{
			public string Title { get; set; }

			public string TocId { get; set; }

			public List<ParsedSection> Sections { get; set; } = new List<ParsedSection>();
		}

		private class ParsedSection
		{
			public string Title { get; set; }

			public string TocId { get; set; }

			public List<string> Content { get; set; } = new List<string>();

			public List<ParsedSection> SubSections { get; set; } = new List<ParsedSection>();
		}

		private static DeviceDescriptions _survivalManualDescriptions;

		private static bool _isRegistered;

		public static void RegisterSurvivalManualPage()
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Expected O, but got Unknown
			if (_isRegistered)
			{
				return;
			}
			try
			{
				StationpediaPage val = new StationpediaPage
				{
					Key = "SurvivalManual",
					Title = "Stationeers Survival Manual"
				};
				val.Text = "Welcome to the Stationeers Survival Manual - your comprehensive guide to surviving your first hours on an alien world.\n\nThis manual covers everything from your initial spawn to building a sustainable base.\n\n<i>Expand the sections below to learn how to survive.</i>";
				Stationpedia.Register(val, false);
				_isRegistered = true;
				ConsoleWindow.Print("[Stationpedia Ascended] Survival Manual page registered", ConsoleColor.White, false, true);
			}
			catch (Exception ex)
			{
				ManualLogSource log = StationpediaAscendedMod.Log;
				if (log != null)
				{
					log.LogError((object)("Error registering Survival Manual: " + ex.Message));
				}
			}
		}

		public static DeviceDescriptions GetSurvivalManualDescriptions()
		{
			if (_survivalManualDescriptions == null)
			{
				_survivalManualDescriptions = LoadAndConvertManual();
			}
			return _survivalManualDescriptions;
		}

		private static DeviceDescriptions LoadAndConvertManual()
		{
			string text = LoadMarkdownFile();
			if (string.IsNullOrEmpty(text))
			{
				return CreateFallbackManual();
			}
			return ConvertMarkdownToDescriptions(text);
		}

		private static string LoadMarkdownFile()
		{
			List<string> list = new List<string>();
			list.Add("C:\\Dev\\12-17-25 Stationeers Respawn Update Code\\StationpediaAscended\\mod\\Guides\\Stationeers Survival Manual.md");
			list.Add(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "", "Guides", "Stationeers Survival Manual.md"));
			list.Add(Path.Combine(Paths.BepInExRootPath, "scripts", "Guides", "Stationeers Survival Manual.md"));
			foreach (string item in list)
			{
				if (string.IsNullOrEmpty(item) || !File.Exists(item))
				{
					continue;
				}
				try
				{
					return File.ReadAllText(item);
				}
				catch (Exception ex)
				{
					ManualLogSource log = StationpediaAscendedMod.Log;
					if (log != null)
					{
						log.LogWarning((object)("Failed to read Survival Manual from " + item + ": " + ex.Message));
					}
				}
			}
			ManualLogSource log2 = StationpediaAscendedMod.Log;
			if (log2 != null)
			{
				log2.LogWarning((object)"Survival Manual markdown file not found");
			}
			return null;
		}

		private static DeviceDescriptions ConvertMarkdownToDescriptions(string markdown)
		{
			DeviceDescriptions deviceDescriptions = new DeviceDescriptions
			{
				deviceKey = "SurvivalManual",
				generateToc = false,
				operationalDetails = new List<OperationalDetail>()
			};
			List<ParsedPart> list = ParseMarkdownIntoParts(markdown);
			foreach (ParsedPart item2 in list)
			{
				OperationalDetail item = ConvertPartToOperationalDetail(item2);
				deviceDescriptions.operationalDetails.Add(item);
			}
			return deviceDescriptions;
		}

		private static List<ParsedPart> ParseMarkdownIntoParts(string markdown)
		{
			List<ParsedPart> list = new List<ParsedPart>();
			string[] array = markdown.Split(new string[3] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
			ParsedPart parsedPart = null;
			ParsedSection parsedSection = null;
			ParsedSection parsedSection2 = null;
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = false;
			foreach (string text in array)
			{
				if (text.TrimStart().StartsWith("```"))
				{
					flag = !flag;
					if (flag)
					{
						stringBuilder.AppendLine("<mspace=0.5em>");
					}
					else
					{
						stringBuilder.AppendLine("</mspace>");
					}
					continue;
				}
				if (flag)
				{
					stringBuilder.AppendLine("  " + text);
					continue;
				}
				Match match = Regex.Match(text, "^#\\s+Part\\s+(\\d+):\\s*(.+)$");
				if (match.Success)
				{
					SaveCurrentContent(parsedPart, parsedSection, parsedSection2, stringBuilder);
					parsedPart = new ParsedPart
					{
						Title = "Part " + match.Groups[1].Value + ": " + match.Groups[2].Value.Trim(),
						TocId = "part" + match.Groups[1].Value
					};
					list.Add(parsedPart);
					parsedSection = null;
					parsedSection2 = null;
					stringBuilder.Clear();
					continue;
				}
				Match match2 = Regex.Match(text, "^##\\s+(.+)$");
				if (match2.Success && parsedPart != null)
				{
					SaveCurrentContent(parsedPart, parsedSection, parsedSection2, stringBuilder);
					string title = match2.Groups[1].Value.Trim();
					parsedSection = new ParsedSection
					{
						Title = title,
						TocId = GenerateTocId(title)
					};
					parsedPart.Sections.Add(parsedSection);
					parsedSection2 = null;
					stringBuilder.Clear();
					continue;
				}
				Match match3 = Regex.Match(text, "^###\\s+(.+)$");
				if (match3.Success && parsedSection != null)
				{
					SaveCurrentContent(parsedPart, parsedSection, parsedSection2, stringBuilder);
					string title2 = match3.Groups[1].Value.Trim();
					parsedSection2 = new ParsedSection
					{
						Title = title2,
						TocId = GenerateTocId(title2)
					};
					parsedSection.SubSections.Add(parsedSection2);
					stringBuilder.Clear();
				}
				else if (!text.Trim().StartsWith("---") && !(text.Trim() == "```markdown") && !(text.Trim() == "````markdown"))
				{
					string value = ConvertMarkdownLine(text);
					if (!string.IsNullOrWhiteSpace(value) || stringBuilder.Length > 0)
					{
						stringBuilder.AppendLine(value);
					}
				}
			}
			SaveCurrentContent(parsedPart, parsedSection, parsedSection2, stringBuilder);
			return list;
		}

		private static void SaveCurrentContent(ParsedPart part, ParsedSection section, ParsedSection subSection, StringBuilder content)
		{
			if (content.Length == 0)
			{
				return;
			}
			string text = content.ToString().Trim();
			if (!string.IsNullOrEmpty(text))
			{
				if (subSection != null)
				{
					subSection.Content.Add(text);
				}
				else
				{
					section?.Content.Add(text);
				}
			}
		}

		private static string ConvertMarkdownLine(string line)
		{
			if (string.IsNullOrEmpty(line))
			{
				return line;
			}
			line = Regex.Replace(line, "\\*\\*([^*]+)\\*\\*", "<b>$1</b>");
			line = Regex.Replace(line, "(?<!\\*)\\*([^*]+)\\*(?!\\*)", "<i>$1</i>");
			line = Regex.Replace(line, "`([^`]+)`", "<color=#88CCFF>$1</color>");
			if (line.TrimStart().StartsWith("- "))
			{
				int count = line.Length - line.TrimStart().Length;
				string text = new string(' ', count);
				line = text + "• " + line.TrimStart().Substring(2);
			}
			Match match = Regex.Match(line, "^(\\s*)(\\d+)\\.\\s+(.+)$");
			if (match.Success)
			{
				string value = match.Groups[1].Value;
				string value2 = match.Groups[2].Value;
				string value3 = match.Groups[3].Value;
				line = value + "<color=#FFA500>" + value2 + ".</color> " + value3;
			}
			line = Regex.Replace(line, "\\[([^\\]]+)\\]\\([^)]+\\)", "$1");
			return line;
		}

		private static string GenerateTocId(string title)
		{
			string input = title.ToLowerInvariant();
			input = Regex.Replace(input, "[^a-z0-9]+", "_");
			return input.Trim('_');
		}

		private static OperationalDetail ConvertPartToOperationalDetail(ParsedPart part)
		{
			OperationalDetail operationalDetail = new OperationalDetail
			{
				title = part.Title,
				tocId = part.TocId,
				collapsible = true,
				children = new List<OperationalDetail>()
			};
			OperationalDetail operationalDetail2 = CreateTableOfContents(part);
			if (operationalDetail2 != null)
			{
				operationalDetail.children.Add(operationalDetail2);
			}
			foreach (ParsedSection section in part.Sections)
			{
				OperationalDetail item = ConvertSectionToOperationalDetail(section);
				operationalDetail.children.Add(item);
			}
			return operationalDetail;
		}

		private static OperationalDetail CreateTableOfContents(ParsedPart part)
		{
			if (part.Sections.Count == 0)
			{
				return null;
			}
			List<string> list = new List<string>();
			foreach (ParsedSection section in part.Sections)
			{
				list.Add("<link=toc_" + section.TocId + "><color=#FFFFFF>" + section.Title + "</color></link>");
				foreach (ParsedSection subSection in section.SubSections)
				{
					list.Add("  <color=#888888>-</color> <link=toc_" + subSection.TocId + "><color=#CCCCCC>" + subSection.Title + "</color></link>");
				}
			}
			return new OperationalDetail
			{
				title = "Contents",
				tocId = part.TocId + "_toc",
				collapsible = false,
				description = string.Join("\n", list)
			};
		}

		private static OperationalDetail ConvertSectionToOperationalDetail(ParsedSection section)
		{
			OperationalDetail operationalDetail = new OperationalDetail
			{
				title = section.Title,
				tocId = section.TocId,
				collapsible = true,
				children = new List<OperationalDetail>()
			};
			if (section.Content.Count > 0)
			{
				operationalDetail.description = string.Join("\n\n", section.Content);
			}
			foreach (ParsedSection subSection in section.SubSections)
			{
				OperationalDetail operationalDetail2 = new OperationalDetail
				{
					title = subSection.Title,
					tocId = subSection.TocId,
					collapsible = true
				};
				if (subSection.Content.Count > 0)
				{
					operationalDetail2.description = string.Join("\n\n", subSection.Content);
				}
				operationalDetail.children.Add(operationalDetail2);
			}
			return operationalDetail;
		}

		private static DeviceDescriptions CreateFallbackManual()
		{
			return new DeviceDescriptions
			{
				deviceKey = "SurvivalManual",
				generateToc = true,
				operationalDetails = new List<OperationalDetail>
				{
					new OperationalDetail
					{
						title = "Part 1: Getting Started",
						tocId = "part1",
						collapsible = true,
						description = "The Stationeers Survival Manual file was not found.\n\nPlease ensure 'Stationeers Survival Manual.md' is in the Guides folder."
					}
				}
			};
		}

		public static void Clear()
		{
			_survivalManualDescriptions = null;
			_isRegistered = false;
		}
	}
}
namespace StationpediaAscended.Core
{
	public static class TooltipState
	{
		public static string CurrentText { get; set; } = "";

		public static bool IsVisible { get; set; } = false;

		public static void Show(string text)
		{
			CurrentText = text;
			IsVisible = true;
		}

		public static void Hide()
		{
			CurrentText = "";
			IsVisible = false;
		}

		public static void Reset()
		{
			CurrentText = "";
			IsVisible = false;
		}
	}
}
