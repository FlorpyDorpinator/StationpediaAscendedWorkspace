using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Xml.Serialization;
using Assets.Scripts;
using Assets.Scripts.GridSystem;
using Assets.Scripts.Inventory;
using Assets.Scripts.Localization2;
using Assets.Scripts.Networking;
using Assets.Scripts.Networks;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.Objects.Motherboards;
using Assets.Scripts.Objects.Pipes;
using Assets.Scripts.Objects.Structures;
using Assets.Scripts.Serialization;
using Assets.Scripts.UI;
using Assets.Scripts.Util;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using HarmonyLib;
using LibConstruct;
using Objects;
using StationeersMods.Interface;
using TMPro;
using Trading;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using modularconsolemod;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: AssemblyVersion("0.0.0.0")]
public class ModularConsoleMod : ModBehaviour
{
	public override void OnLoaded(ContentHandler contentHandler)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		Debug.Log((object)"Hello World!");
		Materials.Initialize(contentHandler.prefabs);
		Harmony val = new Harmony("ModularConsoleMod");
		PrefabPatch.prefabs = contentHandler.prefabs;
		val.PatchAll();
		Debug.Log((object)("ModularConsoleMod Loaded with " + contentHandler.prefabs.Count + " prefab(s)"));
		((MonoBehaviour)this).StartCoroutine(TextQueue.RenderCoroutine());
	}
}
[CompilerGenerated]
[EditorBrowsable(EditorBrowsableState.Never)]
[GeneratedCode("Unity.MonoScriptGenerator.MonoScriptInfoGenerator", null)]
internal class UnitySourceGeneratedAssemblyMonoScriptTypes_v1
{
	private struct MonoScriptData
	{
		public byte[] FilePathsData;

		public byte[] TypesData;

		public int TotalTypes;

		public int TotalFiles;

		public bool IsEditorOnly;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static MonoScriptData Get()
	{
		return new MonoScriptData
		{
			FilePathsData = new byte[1987]
			{
				0, 0, 0, 3, 0, 0, 0, 31, 92, 65,
				115, 115, 101, 116, 115, 92, 83, 99, 114, 105,
				112, 116, 115, 92, 67, 111, 110, 115, 111, 108,
				101, 66, 111, 97, 114, 100, 46, 99, 115, 0,
				0, 0, 3, 0, 0, 0, 39, 92, 65, 115,
				115, 101, 116, 115, 92, 83, 99, 114, 105, 112,
				116, 115, 92, 67, 111, 110, 115, 111, 108, 101,
				66, 111, 97, 114, 100, 83, 97, 118, 101, 68,
				97, 116, 97, 46, 99, 115, 0, 0, 0, 1,
				0, 0, 0, 31, 92, 65, 115, 115, 101, 116,
				115, 92, 83, 99, 114, 105, 112, 116, 115, 92,
				67, 111, 110, 115, 111, 108, 101, 68, 105, 103,
				105, 116, 46, 99, 115, 0, 0, 0, 1, 0,
				0, 0, 29, 92, 65, 115, 115, 101, 116, 115,
				92, 83, 99, 114, 105, 112, 116, 115, 92, 68,
				114, 97, 103, 67, 117, 114, 115, 111, 114, 46,
				99, 115, 0, 0, 0, 1, 0, 0, 0, 34,
				92, 65, 115, 115, 101, 116, 115, 92, 83, 99,
				114, 105, 112, 116, 115, 92, 73, 110, 87, 111,
				114, 108, 100, 85, 73, 83, 99, 114, 101, 101,
				110, 46, 99, 115, 0, 0, 0, 1, 0, 0,
				0, 28, 92, 65, 115, 115, 101, 116, 115, 92,
				83, 99, 114, 105, 112, 116, 115, 92, 77, 97,
				116, 101, 114, 105, 97, 108, 115, 46, 99, 115,
				0, 0, 0, 1, 0, 0, 0, 30, 92, 65,
				115, 115, 101, 116, 115, 92, 83, 99, 114, 105,
				112, 116, 115, 92, 77, 97, 116, 104, 72, 101,
				108, 112, 101, 114, 115, 46, 99, 115, 0, 0,
				0, 1, 0, 0, 0, 36, 92, 65, 115, 115,
				101, 116, 115, 92, 83, 99, 114, 105, 112, 116,
				115, 92, 77, 111, 100, 117, 108, 97, 114, 67,
				111, 110, 115, 111, 108, 101, 77, 111, 100, 46,
				99, 115, 0, 0, 0, 1, 0, 0, 0, 35,
				92, 65, 115, 115, 101, 116, 115, 92, 83, 99,
				114, 105, 112, 116, 115, 92, 77, 117, 108, 116,
				105, 67, 111, 110, 115, 116, 114, 117, 99, 116,
				111, 114, 46, 99, 115, 0, 0, 0, 1, 0,
				0, 0, 38, 92, 65, 115, 115, 101, 116, 115,
				92, 83, 99, 114, 105, 112, 116, 115, 92, 77,
				121, 71, 114, 97, 112, 104, 105, 99, 115, 82,
				97, 121, 99, 97, 115, 116, 101, 114, 46, 99,
				115, 0, 0, 0, 1, 0, 0, 0, 44, 92,
				65, 115, 115, 101, 116, 115, 92, 83, 99, 114,
				105, 112, 116, 115, 92, 112, 97, 116, 99, 104,
				101, 115, 92, 67, 97, 98, 108, 101, 78, 101,
				116, 119, 111, 114, 107, 80, 97, 116, 99, 104,
				46, 99, 115, 0, 0, 0, 7, 0, 0, 0,
				46, 92, 65, 115, 115, 101, 116, 115, 92, 83,
				99, 114, 105, 112, 116, 115, 92, 112, 97, 116,
				99, 104, 101, 115, 92, 67, 105, 114, 99, 117,
				105, 116, 98, 111, 97, 114, 100, 80, 97, 116,
				99, 104, 101, 115, 46, 99, 115, 0, 0, 0,
				2, 0, 0, 0, 44, 92, 65, 115, 115, 101,
				116, 115, 92, 83, 99, 114, 105, 112, 116, 115,
				92, 112, 97, 116, 99, 104, 101, 115, 92, 68,
				114, 97, 103, 67, 117, 114, 115, 111, 114, 80,
				97, 116, 99, 104, 101, 115, 46, 99, 115, 0,
				0, 0, 3, 0, 0, 0, 38, 92, 65, 115,
				115, 101, 116, 115, 92, 83, 99, 114, 105, 112,
				116, 115, 92, 112, 97, 116, 99, 104, 101, 115,
				92, 80, 114, 101, 102, 97, 98, 80, 97, 116,
				99, 104, 46, 99, 115, 0, 0, 0, 1, 0,
				0, 0, 40, 92, 65, 115, 115, 101, 116, 115,
				92, 83, 99, 114, 105, 112, 116, 115, 92, 112,
				97, 116, 99, 104, 101, 115, 92, 83, 97, 118,
				101, 68, 97, 116, 97, 80, 97, 116, 99, 104,
				46, 99, 115, 0, 0, 0, 1, 0, 0, 0,
				46, 92, 65, 115, 115, 101, 116, 115, 92, 83,
				99, 114, 105, 112, 116, 115, 92, 112, 97, 116,
				99, 104, 101, 115, 92, 83, 116, 97, 116, 105,
				111, 110, 112, 101, 100, 105, 97, 80, 97, 116,
				99, 104, 101, 115, 46, 99, 115, 0, 0, 0,
				1, 0, 0, 0, 41, 92, 65, 115, 115, 101,
				116, 115, 92, 83, 99, 114, 105, 112, 116, 115,
				92, 112, 97, 116, 99, 104, 101, 115, 92, 83,
				116, 114, 117, 99, 116, 117, 114, 101, 80, 97,
				116, 99, 104, 46, 99, 115, 0, 0, 0, 4,
				0, 0, 0, 36, 92, 65, 115, 115, 101, 116,
				115, 92, 83, 99, 114, 105, 112, 116, 115, 92,
				112, 97, 116, 99, 104, 101, 115, 92, 84, 101,
				120, 116, 81, 117, 101, 117, 101, 46, 99, 115,
				0, 0, 0, 1, 0, 0, 0, 40, 92, 65,
				115, 115, 101, 116, 115, 92, 83, 99, 114, 105,
				112, 116, 115, 92, 83, 116, 114, 117, 99, 116,
				117, 114, 101, 67, 111, 110, 115, 111, 108, 101,
				66, 111, 97, 114, 100, 46, 99, 115, 0, 0,
				0, 1, 0, 0, 0, 41, 92, 65, 115, 115,
				101, 116, 115, 92, 83, 99, 114, 105, 112, 116,
				115, 92, 83, 116, 114, 117, 99, 116, 117, 114,
				101, 67, 111, 110, 115, 111, 108, 101, 66, 117,
				116, 116, 111, 110, 46, 99, 115, 0, 0, 0,
				1, 0, 0, 0, 45, 92, 65, 115, 115, 101,
				116, 115, 92, 83, 99, 114, 105, 112, 116, 115,
				92, 83, 116, 114, 117, 99, 116, 117, 114, 101,
				67, 111, 110, 115, 111, 108, 101, 67, 97, 114,
				100, 82, 101, 97, 100, 101, 114, 46, 99, 115,
				0, 0, 0, 1, 0, 0, 0, 43, 92, 65,
				115, 115, 101, 116, 115, 92, 83, 99, 114, 105,
				112, 116, 115, 92, 83, 116, 114, 117, 99, 116,
				117, 114, 101, 67, 111, 110, 115, 111, 108, 101,
				67, 111, 109, 112, 117, 116, 101, 114, 46, 99,
				115, 0, 0, 0, 1, 0, 0, 0, 42, 92,
				65, 115, 115, 101, 116, 115, 92, 83, 99, 114,
				105, 112, 116, 115, 92, 83, 116, 114, 117, 99,
				116, 117, 114, 101, 67, 111, 110, 115, 111, 108,
				101, 67, 111, 110, 115, 111, 108, 101, 46, 99,
				115, 0, 0, 0, 1, 0, 0, 0, 42, 92,
				65, 115, 115, 101, 116, 115, 92, 83, 99, 114,
				105, 112, 116, 115, 92, 83, 116, 114, 117, 99,
				116, 117, 114, 101, 67, 111, 110, 115, 111, 108,
				101, 68, 105, 97, 103, 114, 97, 109, 46, 99,
				115, 0, 0, 0, 1, 0, 0, 0, 39, 92,
				65, 115, 115, 101, 116, 115, 92, 83, 99, 114,
				105, 112, 116, 115, 92, 83, 116, 114, 117, 99,
				116, 117, 114, 101, 67, 111, 110, 115, 111, 108,
				101, 68, 105, 97, 108, 46, 99, 115, 0, 0,
				0, 1, 0, 0, 0, 40, 92, 65, 115, 115,
				101, 116, 115, 92, 83, 99, 114, 105, 112, 116,
				115, 92, 83, 116, 114, 117, 99, 116, 117, 114,
				101, 67, 111, 110, 115, 111, 108, 101, 68, 105,
				111, 100, 101, 46, 99, 115, 0, 0, 0, 1,
				0, 0, 0, 46, 92, 65, 115, 115, 101, 116,
				115, 92, 83, 99, 114, 105, 112, 116, 115, 92,
				83, 116, 114, 117, 99, 116, 117, 114, 101, 67,
				111, 110, 115, 111, 108, 101, 68, 105, 111, 100,
				101, 83, 108, 105, 100, 101, 114, 46, 99, 115,
				0, 0, 0, 1, 0, 0, 0, 42, 92, 65,
				115, 115, 101, 116, 115, 92, 83, 99, 114, 105,
				112, 116, 115, 92, 83, 116, 114, 117, 99, 116,
				117, 114, 101, 67, 111, 110, 115, 111, 108, 101,
				68, 105, 115, 112, 108, 97, 121, 46, 99, 115,
				0, 0, 0, 1, 0, 0, 0, 44, 92, 65,
				115, 115, 101, 116, 115, 92, 83, 99, 114, 105,
				112, 116, 115, 92, 83, 116, 114, 117, 99, 116,
				117, 114, 101, 67, 111, 110, 115, 111, 108, 101,
				68, 114, 97, 103, 103, 97, 98, 108, 101, 46,
				99, 115, 0, 0, 0, 1, 0, 0, 0, 40,
				92, 65, 115, 115, 101, 116, 115, 92, 83, 99,
				114, 105, 112, 116, 115, 92, 83, 116, 114, 117,
				99, 116, 117, 114, 101, 67, 111, 110, 115, 111,
				108, 101, 71, 97, 117, 103, 101, 46, 99, 115,
				0, 0, 0, 1, 0, 0, 0, 46, 92, 65,
				115, 115, 101, 116, 115, 92, 83, 99, 114, 105,
				112, 116, 115, 92, 83, 116, 114, 117, 99, 116,
				117, 114, 101, 67, 111, 110, 115, 111, 108, 101,
				71, 97, 117, 103, 101, 77, 97, 110, 117, 97,
				108, 46, 99, 115, 0, 0, 0, 1, 0, 0,
				0, 40, 92, 65, 115, 115, 101, 116, 115, 92,
				83, 99, 114, 105, 112, 116, 115, 92, 83, 116,
				114, 117, 99, 116, 117, 114, 101, 67, 111, 110,
				115, 111, 108, 101, 76, 97, 98, 101, 108, 46,
				99, 115, 0, 0, 0, 1, 0, 0, 0, 45,
				92, 65, 115, 115, 101, 116, 115, 92, 83, 99,
				114, 105, 112, 116, 115, 92, 83, 116, 114, 117,
				99, 116, 117, 114, 101, 67, 111, 110, 115, 111,
				108, 101, 76, 97, 98, 101, 108, 68, 105, 111,
				100, 101, 46, 99, 115, 0, 0, 0, 1, 0,
				0, 0, 40, 92, 65, 115, 115, 101, 116, 115,
				92, 83, 99, 114, 105, 112, 116, 115, 92, 83,
				116, 114, 117, 99, 116, 117, 114, 101, 67, 111,
				110, 115, 111, 108, 101, 76, 101, 118, 101, 114,
				46, 99, 115, 0, 0, 0, 1, 0, 0, 0,
				41, 92, 65, 115, 115, 101, 116, 115, 92, 83,
				99, 114, 105, 112, 116, 115, 92, 83, 116, 114,
				117, 99, 116, 117, 114, 101, 67, 111, 110, 115,
				111, 108, 101, 78, 117, 109, 112, 97, 100, 46,
				99, 115, 0, 0, 0, 1, 0, 0, 0, 41,
				92, 65, 115, 115, 101, 116, 115, 92, 83, 99,
				114, 105, 112, 116, 115, 92, 83, 116, 114, 117,
				99, 116, 117, 114, 101, 67, 111, 110, 115, 111,
				108, 101, 83, 108, 105, 100, 101, 114, 46, 99,
				115, 0, 0, 0, 1, 0, 0, 0, 41, 92,
				65, 115, 115, 101, 116, 115, 92, 83, 99, 114,
				105, 112, 116, 115, 92, 83, 116, 114, 117, 99,
				116, 117, 114, 101, 67, 111, 110, 115, 111, 108,
				101, 83, 119, 105, 116, 99, 104, 46, 99, 115,
				0, 0, 0, 1, 0, 0, 0, 43, 92, 65,
				115, 115, 101, 116, 115, 92, 83, 99, 114, 105,
				112, 116, 115, 92, 83, 116, 114, 117, 99, 116,
				117, 114, 101, 67, 111, 110, 115, 111, 108, 101,
				84, 104, 114, 111, 116, 116, 108, 101, 46, 99,
				115, 0, 0, 0, 1, 0, 0, 0, 48, 92,
				65, 115, 115, 101, 116, 115, 92, 83, 99, 114,
				105, 112, 116, 115, 92, 83, 116, 114, 117, 99,
				116, 117, 114, 101, 67, 111, 110, 115, 111, 108,
				101, 85, 116, 105, 108, 105, 116, 121, 66, 117,
				116, 116, 111, 110, 46, 99, 115, 0, 0, 0,
				1, 0, 0, 0, 38, 92, 65, 115, 115, 101,
				116, 115, 92, 83, 99, 114, 105, 112, 116, 115,
				92, 83, 116, 114, 117, 99, 116, 117, 114, 101,
				69, 120, 116, 101, 110, 115, 105, 111, 110, 115,
				46, 99, 115, 0, 0, 0, 1, 0, 0, 0,
				30, 92, 65, 115, 115, 101, 116, 115, 92, 83,
				99, 114, 105, 112, 116, 115, 92, 83, 119, 105,
				116, 99, 104, 79, 110, 79, 102, 102, 46, 99,
				115, 0, 0, 0, 1, 0, 0, 0, 28, 92,
				65, 115, 115, 101, 116, 115, 92, 83, 99, 114,
				105, 112, 116, 115, 92, 87, 105, 114, 101, 102,
				114, 97, 109, 101, 46, 99, 115
			},
			TypesData = new byte[2379]
			{
				0, 0, 0, 0, 30, 109, 111, 100, 117, 108,
				97, 114, 99, 111, 110, 115, 111, 108, 101, 109,
				111, 100, 124, 67, 111, 110, 115, 111, 108, 101,
				66, 111, 97, 114, 100, 0, 0, 0, 0, 31,
				109, 111, 100, 117, 108, 97, 114, 99, 111, 110,
				115, 111, 108, 101, 109, 111, 100, 124, 73, 80,
				111, 119, 101, 114, 101, 100, 66, 111, 97, 114,
				100, 0, 0, 0, 0, 39, 109, 111, 100, 117,
				108, 97, 114, 99, 111, 110, 115, 111, 108, 101,
				109, 111, 100, 124, 73, 66, 111, 97, 114, 100,
				80, 111, 119, 101, 114, 101, 100, 76, 105, 115,
				116, 101, 110, 101, 114, 0, 0, 0, 0, 38,
				109, 111, 100, 117, 108, 97, 114, 99, 111, 110,
				115, 111, 108, 101, 109, 111, 100, 124, 67, 111,
				110, 115, 111, 108, 101, 66, 111, 97, 114, 100,
				83, 97, 118, 101, 68, 97, 116, 97, 0, 0,
				0, 0, 39, 109, 111, 100, 117, 108, 97, 114,
				99, 111, 110, 115, 111, 108, 101, 109, 111, 100,
				124, 67, 111, 110, 115, 111, 108, 101, 68, 101,
				118, 105, 99, 101, 83, 97, 118, 101, 68, 97,
				116, 97, 0, 0, 0, 0, 38, 109, 111, 100,
				117, 108, 97, 114, 99, 111, 110, 115, 111, 108,
				101, 109, 111, 100, 124, 67, 111, 110, 115, 111,
				108, 101, 76, 111, 103, 105, 99, 83, 97, 118,
				101, 68, 97, 116, 97, 0, 0, 0, 0, 30,
				109, 111, 100, 117, 108, 97, 114, 99, 111, 110,
				115, 111, 108, 101, 109, 111, 100, 124, 67, 111,
				110, 115, 111, 108, 101, 68, 105, 103, 105, 116,
				0, 0, 0, 0, 28, 109, 111, 100, 117, 108,
				97, 114, 99, 111, 110, 115, 111, 108, 101, 109,
				111, 100, 124, 68, 114, 97, 103, 67, 117, 114,
				115, 111, 114, 0, 0, 0, 0, 33, 109, 111,
				100, 117, 108, 97, 114, 99, 111, 110, 115, 111,
				108, 101, 109, 111, 100, 124, 73, 110, 87, 111,
				114, 108, 100, 85, 73, 83, 99, 114, 101, 101,
				110, 0, 0, 0, 0, 27, 109, 111, 100, 117,
				108, 97, 114, 99, 111, 110, 115, 111, 108, 101,
				109, 111, 100, 124, 77, 97, 116, 101, 114, 105,
				97, 108, 115, 0, 0, 0, 0, 29, 109, 111,
				100, 117, 108, 97, 114, 99, 111, 110, 115, 111,
				108, 101, 109, 111, 100, 124, 77, 97, 116, 104,
				72, 101, 108, 112, 101, 114, 115, 0, 0, 0,
				0, 18, 124, 77, 111, 100, 117, 108, 97, 114,
				67, 111, 110, 115, 111, 108, 101, 77, 111, 100,
				0, 0, 0, 0, 34, 109, 111, 100, 117, 108,
				97, 114, 99, 111, 110, 115, 111, 108, 101, 109,
				111, 100, 124, 77, 117, 108, 116, 105, 67, 111,
				110, 115, 116, 114, 117, 99, 116, 111, 114, 0,
				0, 0, 0, 37, 109, 111, 100, 117, 108, 97,
				114, 99, 111, 110, 115, 111, 108, 101, 109, 111,
				100, 124, 77, 121, 71, 114, 97, 112, 104, 105,
				99, 115, 82, 97, 121, 99, 97, 115, 116, 101,
				114, 0, 0, 0, 0, 35, 109, 111, 100, 117,
				108, 97, 114, 99, 111, 110, 115, 111, 108, 101,
				109, 111, 100, 124, 67, 97, 98, 108, 101, 78,
				101, 116, 119, 111, 114, 107, 80, 97, 116, 99,
				104, 0, 0, 0, 0, 55, 109, 111, 100, 117,
				108, 97, 114, 99, 111, 110, 115, 111, 108, 101,
				109, 111, 100, 124, 67, 105, 114, 99, 117, 105,
				116, 98, 111, 97, 114, 100, 68, 101, 118, 105,
				99, 101, 76, 105, 115, 116, 67, 104, 97, 110,
				103, 101, 84, 97, 115, 107, 80, 97, 116, 99,
				104, 0, 0, 0, 0, 35, 109, 111, 100, 117,
				108, 97, 114, 99, 111, 110, 115, 111, 108, 101,
				109, 111, 100, 124, 67, 105, 114, 99, 117, 105,
				116, 98, 111, 97, 114, 100, 80, 97, 116, 99,
				104, 0, 0, 0, 0, 37, 109, 111, 100, 117,
				108, 97, 114, 99, 111, 110, 115, 111, 108, 101,
				109, 111, 100, 124, 67, 111, 109, 112, 117, 116,
				101, 114, 66, 117, 116, 116, 111, 110, 80, 97,
				116, 99, 104, 0, 0, 0, 0, 34, 109, 111,
				100, 117, 108, 97, 114, 99, 111, 110, 115, 111,
				108, 101, 109, 111, 100, 124, 77, 111, 116, 104,
				101, 114, 98, 111, 97, 114, 100, 80, 97, 116,
				99, 104, 0, 0, 0, 0, 33, 109, 111, 100,
				117, 108, 97, 114, 99, 111, 110, 115, 111, 108,
				101, 109, 111, 100, 124, 71, 97, 115, 68, 105,
				115, 112, 108, 97, 121, 80, 97, 116, 99, 104,
				0, 0, 0, 0, 31, 109, 111, 100, 117, 108,
				97, 114, 99, 111, 110, 115, 111, 108, 101, 109,
				111, 100, 124, 67, 111, 109, 112, 117, 116, 101,
				114, 80, 97, 116, 99, 104, 0, 0, 0, 0,
				28, 109, 111, 100, 117, 108, 97, 114, 99, 111,
				110, 115, 111, 108, 101, 109, 111, 100, 124, 80,
				97, 116, 99, 104, 85, 116, 105, 108, 115, 0,
				0, 0, 0, 36, 109, 111, 100, 117, 108, 97,
				114, 99, 111, 110, 115, 111, 108, 101, 109, 111,
				100, 124, 67, 117, 114, 115, 111, 114, 77, 97,
				110, 97, 103, 101, 114, 80, 97, 116, 99, 104,
				0, 0, 0, 0, 45, 109, 111, 100, 117, 108,
				97, 114, 99, 111, 110, 115, 111, 108, 101, 109,
				111, 100, 124, 67, 97, 109, 101, 114, 97, 67,
				111, 108, 108, 105, 115, 105, 111, 110, 72, 97,
				110, 100, 108, 101, 114, 80, 97, 116, 99, 104,
				0, 0, 0, 0, 29, 109, 111, 100, 117, 108,
				97, 114, 99, 111, 110, 115, 111, 108, 101, 109,
				111, 100, 124, 80, 114, 101, 102, 97, 98, 80,
				97, 116, 99, 104, 0, 0, 0, 0, 34, 109,
				111, 100, 117, 108, 97, 114, 99, 111, 110, 115,
				111, 108, 101, 109, 111, 100, 124, 80, 114, 101,
				102, 97, 98, 69, 120, 116, 101, 110, 115, 105,
				111, 110, 115, 0, 0, 0, 0, 30, 109, 111,
				100, 117, 108, 97, 114, 99, 111, 110, 115, 111,
				108, 101, 109, 111, 100, 124, 73, 80, 97, 116,
				99, 104, 79, 110, 76, 111, 97, 100, 0, 0,
				0, 0, 31, 109, 111, 100, 117, 108, 97, 114,
				99, 111, 110, 115, 111, 108, 101, 109, 111, 100,
				124, 83, 97, 118, 101, 68, 97, 116, 97, 80,
				97, 116, 99, 104, 0, 0, 0, 0, 37, 109,
				111, 100, 117, 108, 97, 114, 99, 111, 110, 115,
				111, 108, 101, 109, 111, 100, 124, 83, 116, 97,
				116, 105, 111, 110, 112, 101, 100, 105, 97, 80,
				97, 116, 99, 104, 101, 115, 0, 0, 0, 0,
				32, 109, 111, 100, 117, 108, 97, 114, 99, 111,
				110, 115, 111, 108, 101, 109, 111, 100, 124, 83,
				116, 114, 117, 99, 116, 117, 114, 101, 80, 97,
				116, 99, 104, 0, 0, 0, 0, 27, 109, 111,
				100, 117, 108, 97, 114, 99, 111, 110, 115, 111,
				108, 101, 109, 111, 100, 124, 84, 101, 120, 116,
				81, 117, 101, 117, 101, 0, 0, 0, 0, 33,
				109, 111, 100, 117, 108, 97, 114, 99, 111, 110,
				115, 111, 108, 101, 109, 111, 100, 46, 84, 101,
				120, 116, 81, 117, 101, 117, 101, 124, 69, 110,
				116, 114, 121, 0, 0, 0, 0, 27, 109, 111,
				100, 117, 108, 97, 114, 99, 111, 110, 115, 111,
				108, 101, 109, 111, 100, 124, 83, 105, 103, 110,
				80, 97, 116, 99, 104, 0, 0, 0, 0, 28,
				109, 111, 100, 117, 108, 97, 114, 99, 111, 110,
				115, 111, 108, 101, 109, 111, 100, 124, 76, 97,
				98, 101, 108, 80, 97, 116, 99, 104, 0, 0,
				0, 0, 39, 109, 111, 100, 117, 108, 97, 114,
				99, 111, 110, 115, 111, 108, 101, 109, 111, 100,
				124, 83, 116, 114, 117, 99, 116, 117, 114, 101,
				67, 111, 110, 115, 111, 108, 101, 66, 111, 97,
				114, 100, 0, 0, 0, 0, 40, 109, 111, 100,
				117, 108, 97, 114, 99, 111, 110, 115, 111, 108,
				101, 109, 111, 100, 124, 83, 116, 114, 117, 99,
				116, 117, 114, 101, 67, 111, 110, 115, 111, 108,
				101, 66, 117, 116, 116, 111, 110, 0, 0, 0,
				0, 44, 109, 111, 100, 117, 108, 97, 114, 99,
				111, 110, 115, 111, 108, 101, 109, 111, 100, 124,
				83, 116, 114, 117, 99, 116, 117, 114, 101, 67,
				111, 110, 115, 111, 108, 101, 67, 97, 114, 100,
				82, 101, 97, 100, 101, 114, 0, 0, 0, 0,
				42, 109, 111, 100, 117, 108, 97, 114, 99, 111,
				110, 115, 111, 108, 101, 109, 111, 100, 124, 83,
				116, 114, 117, 99, 116, 117, 114, 101, 67, 111,
				110, 115, 111, 108, 101, 67, 111, 109, 112, 117,
				116, 101, 114, 0, 0, 0, 0, 41, 109, 111,
				100, 117, 108, 97, 114, 99, 111, 110, 115, 111,
				108, 101, 109, 111, 100, 124, 83, 116, 114, 117,
				99, 116, 117, 114, 101, 67, 111, 110, 115, 111,
				108, 101, 67, 111, 110, 115, 111, 108, 101, 0,
				0, 0, 0, 41, 109, 111, 100, 117, 108, 97,
				114, 99, 111, 110, 115, 111, 108, 101, 109, 111,
				100, 124, 83, 116, 114, 117, 99, 116, 117, 114,
				101, 67, 111, 110, 115, 111, 108, 101, 68, 105,
				97, 103, 114, 97, 109, 0, 0, 0, 0, 38,
				109, 111, 100, 117, 108, 97, 114, 99, 111, 110,
				115, 111, 108, 101, 109, 111, 100, 124, 83, 116,
				114, 117, 99, 116, 117, 114, 101, 67, 111, 110,
				115, 111, 108, 101, 68, 105, 97, 108, 0, 0,
				0, 0, 39, 109, 111, 100, 117, 108, 97, 114,
				99, 111, 110, 115, 111, 108, 101, 109, 111, 100,
				124, 83, 116, 114, 117, 99, 116, 117, 114, 101,
				67, 111, 110, 115, 111, 108, 101, 68, 105, 111,
				100, 101, 0, 0, 0, 0, 45, 109, 111, 100,
				117, 108, 97, 114, 99, 111, 110, 115, 111, 108,
				101, 109, 111, 100, 124, 83, 116, 114, 117, 99,
				116, 117, 114, 101, 67, 111, 110, 115, 111, 108,
				101, 68, 105, 111, 100, 101, 83, 108, 105, 100,
				101, 114, 0, 0, 0, 0, 41, 109, 111, 100,
				117, 108, 97, 114, 99, 111, 110, 115, 111, 108,
				101, 109, 111, 100, 124, 83, 116, 114, 117, 99,
				116, 117, 114, 101, 67, 111, 110, 115, 111, 108,
				101, 68, 105, 115, 112, 108, 97, 121, 0, 0,
				0, 0, 43, 109, 111, 100, 117, 108, 97, 114,
				99, 111, 110, 115, 111, 108, 101, 109, 111, 100,
				124, 83, 116, 114, 117, 99, 116, 117, 114, 101,
				67, 111, 110, 115, 111, 108, 101, 68, 114, 97,
				103, 103, 97, 98, 108, 101, 0, 0, 0, 0,
				39, 109, 111, 100, 117, 108, 97, 114, 99, 111,
				110, 115, 111, 108, 101, 109, 111, 100, 124, 83,
				116, 114, 117, 99, 116, 117, 114, 101, 67, 111,
				110, 115, 111, 108, 101, 71, 97, 117, 103, 101,
				0, 0, 0, 0, 45, 109, 111, 100, 117, 108,
				97, 114, 99, 111, 110, 115, 111, 108, 101, 109,
				111, 100, 124, 83, 116, 114, 117, 99, 116, 117,
				114, 101, 67, 111, 110, 115, 111, 108, 101, 71,
				97, 117, 103, 101, 77, 97, 110, 117, 97, 108,
				0, 0, 0, 0, 39, 109, 111, 100, 117, 108,
				97, 114, 99, 111, 110, 115, 111, 108, 101, 109,
				111, 100, 124, 83, 116, 114, 117, 99, 116, 117,
				114, 101, 67, 111, 110, 115, 111, 108, 101, 76,
				97, 98, 101, 108, 0, 0, 0, 0, 44, 109,
				111, 100, 117, 108, 97, 114, 99, 111, 110, 115,
				111, 108, 101, 109, 111, 100, 124, 83, 116, 114,
				117, 99, 116, 117, 114, 101, 67, 111, 110, 115,
				111, 108, 101, 76, 97, 98, 101, 108, 68, 105,
				111, 100, 101, 0, 0, 0, 0, 39, 109, 111,
				100, 117, 108, 97, 114, 99, 111, 110, 115, 111,
				108, 101, 109, 111, 100, 124, 83, 116, 114, 117,
				99, 116, 117, 114, 101, 67, 111, 110, 115, 111,
				108, 101, 76, 101, 118, 101, 114, 0, 0, 0,
				0, 40, 109, 111, 100, 117, 108, 97, 114, 99,
				111, 110, 115, 111, 108, 101, 109, 111, 100, 124,
				83, 116, 114, 117, 99, 116, 117, 114, 101, 67,
				111, 110, 115, 111, 108, 101, 78, 117, 109, 112,
				97, 100, 0, 0, 0, 0, 40, 109, 111, 100,
				117, 108, 97, 114, 99, 111, 110, 115, 111, 108,
				101, 109, 111, 100, 124, 83, 116, 114, 117, 99,
				116, 117, 114, 101, 67, 111, 110, 115, 111, 108,
				101, 83, 108, 105, 100, 101, 114, 0, 0, 0,
				0, 40, 109, 111, 100, 117, 108, 97, 114, 99,
				111, 110, 115, 111, 108, 101, 109, 111, 100, 124,
				83, 116, 114, 117, 99, 116, 117, 114, 101, 67,
				111, 110, 115, 111, 108, 101, 83, 119, 105, 116,
				99, 104, 0, 0, 0, 0, 42, 109, 111, 100,
				117, 108, 97, 114, 99, 111, 110, 115, 111, 108,
				101, 109, 111, 100, 124, 83, 116, 114, 117, 99,
				116, 117, 114, 101, 67, 111, 110, 115, 111, 108,
				101, 84, 104, 114, 111, 116, 116, 108, 101, 0,
				0, 0, 0, 47, 109, 111, 100, 117, 108, 97,
				114, 99, 111, 110, 115, 111, 108, 101, 109, 111,
				100, 124, 83, 116, 114, 117, 99, 116, 117, 114,
				101, 67, 111, 110, 115, 111, 108, 101, 85, 116,
				105, 108, 105, 116, 121, 66, 117, 116, 116, 111,
				110, 0, 0, 0, 0, 37, 109, 111, 100, 117,
				108, 97, 114, 99, 111, 110, 115, 111, 108, 101,
				109, 111, 100, 124, 83, 116, 114, 117, 99, 116,
				117, 114, 101, 69, 120, 116, 101, 110, 115, 105,
				111, 110, 115, 0, 0, 0, 0, 29, 109, 111,
				100, 117, 108, 97, 114, 99, 111, 110, 115, 111,
				108, 101, 109, 111, 100, 124, 83, 119, 105, 116,
				99, 104, 79, 110, 79, 102, 102, 0, 0, 0,
				0, 27, 109, 111, 100, 117, 108, 97, 114, 99,
				111, 110, 115, 111, 108, 101, 109, 111, 100, 124,
				87, 105, 114, 101, 102, 114, 97, 109, 101
			},
			TotalFiles = 42,
			TotalTypes = 58,
			IsEditorOnly = false
		};
	}
}
namespace modularconsolemod;

public class ConsoleBoard : PlacementBoard, IPoweredBoard
{
	private bool _powered;

	public override float GridSize => 0.0625f;

	public bool Powered
	{
		get
		{
			return _powered;
		}
		set
		{
			if (_powered == value)
			{
				return;
			}
			_powered = value;
			foreach (IPlacementBoardStructure structure in base.Structures)
			{
				if (structure is IBoardPoweredListener boardPoweredListener)
				{
					boardPoweredListener.OnBoardPowerChanged();
				}
			}
		}
	}

	public override IPlacementBoardStructure EquivalentStructure(Structure structure)
	{
		return (IPlacementBoardStructure)(object)((structure is IPlacementBoardStructure) ? structure : null);
	}

	public override void OnStructureRegistered(IPlacementBoardStructure structure)
	{
		((PlacementBoard)this).OnStructureRegistered(structure);
		if (structure is IBoardPoweredListener boardPoweredListener)
		{
			boardPoweredListener.OnBoardPowerChanged();
		}
	}
}
public interface IPoweredBoard
{
	bool Powered { get; }
}
public interface IBoardPoweredListener
{
	void OnBoardPowerChanged();
}
[XmlInclude(typeof(ConsoleBoardSaveData))]
public class ConsoleBoardSaveData : StructureSaveData
{
	[XmlElement]
	public PlacementBoardHostSaveData Board;

	[XmlElement]
	public PlacementBoardHostSaveData Board2;

	[XmlElement]
	public PlacementBoardHostSaveData Board3;

	[XmlElement]
	public PlacementBoardHostSaveData Board4;
}
[XmlInclude(typeof(ConsoleDeviceSaveData))]
public class ConsoleDeviceSaveData : StructureSaveData
{
	[XmlElement]
	public PlacementBoardStructureSaveData Board;
}
[XmlInclude(typeof(ConsoleLogicSaveData))]
public class ConsoleLogicSaveData : LogicBaseSaveData
{
	[XmlElement]
	public PlacementBoardStructureSaveData Board;
}
public class ConsoleDigit : Digit
{
}
public static class DragCursor
{
	public static GameObject Object;

	public static BoxCollider Collider;

	private static Thing _currentOwner;

	private static Dictionary<Collider, Interactable> _currentLookup;

	public static void Initialize()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Expected O, but got Unknown
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		Object = new GameObject();
		Collider = Object.AddComponent<BoxCollider>();
		Collider.size = new Vector3(1E+09f, 1E+09f, 1E-09f);
		((Collider)Collider).isTrigger = true;
		Object.DontDestroyOnLoad((Object)(object)Object);
		Object.SetActive(false);
	}

	public static void TakeDragCursor(Thing owner, Interactable interactable, Dictionary<Collider, Interactable> lookup)
	{
		if (!Object.op_Implicit((Object)(object)Object))
		{
			Initialize();
		}
		if ((Object)(object)_currentOwner != (Object)null)
		{
			throw new Exception("drag cursor already in use by " + _currentOwner.DisplayName);
		}
		_currentOwner = owner;
		_currentLookup = lookup;
		Thing._colliderLookup.Add((Collider)(object)Collider, owner);
		lookup.Add((Collider)(object)Collider, interactable);
		Object.transform.SetParent(((Component)owner).transform);
		Object.SetActive(true);
		UpdateDragCursor();
		CursorManager.Instance.CursorTargetCollider = (Collider)(object)Collider;
	}

	public static void ReleaseDragCursor()
	{
		if (Object.op_Implicit((Object)(object)Object))
		{
			Object.transform.SetParent((Transform)null);
			Object.SetActive(false);
		}
		Thing._colliderLookup.Remove((Collider)(object)Collider);
		_currentLookup.Remove((Collider)(object)Collider);
		_currentOwner = null;
		_currentLookup = null;
	}

	public static void UpdateDragCursor()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)Object) && Object.activeInHierarchy)
		{
			Ray val = ((!Cursor.visible) ? InputHelpers.GetCameraRay() : CameraController.CurrentCamera.ScreenPointToRay(Input.mousePosition));
			Object.transform.SetPositionAndRotation(((Ray)(ref val)).origin + ((Ray)(ref val)).direction * 0.01f, Quaternion.LookRotation(((Ray)(ref val)).direction));
		}
	}
}
public class InWorldUIScreen : InWorldUIScreen
{
}
public static class Materials
{
	public const string EMISSION = "_EMISSION";

	public const string EMISSION_COLOR = "_EmissionColor";

	public static void Initialize(IEnumerable<GameObject> prefabs)
	{
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		HashSet<Material> hashSet = new HashSet<Material>();
		foreach (GameObject prefab in prefabs)
		{
			MeshRenderer[] componentsInChildren = prefab.GetComponentsInChildren<MeshRenderer>(true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				Material[] sharedMaterials = ((Renderer)componentsInChildren[i]).sharedMaterials;
				foreach (Material item in sharedMaterials)
				{
					hashSet.Add(item);
				}
			}
		}
		Shader shader = Shader.Find("Standard");
		foreach (Material item2 in hashSet)
		{
			if (!((Object)item2).name.StartsWith("Glass") && ((Object)item2.shader).name == "Standard")
			{
				item2.shader = shader;
			}
			if (!((Object)item2).name.StartsWith("Light") && !((Object)item2).name.StartsWith("ScreenDigit"))
			{
				item2.DisableKeyword("_EMISSION");
				item2.SetColor("_EmissionColor", Color.black);
				item2.globalIlluminationFlags = (MaterialGlobalIlluminationFlags)4;
				Debug.Log((object)("Disabled SEGI emission for " + ((Object)item2).name));
			}
		}
	}
}
public static class MathHelpers
{
	public static float RayCylinderAngle(Ray ray, Vector3 center, Vector3 axis, Vector3 forward, float sqrRadius)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.Cross(axis, forward);
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		Vector3 val2 = ((Ray)(ref ray)).origin - center;
		Vector2 val3 = default(Vector2);
		((Vector2)(ref val3))..ctor(Vector3.Dot(forward, val2), Vector3.Dot(normalized, val2));
		Vector2 val4 = new Vector2(Vector3.Dot(forward, ((Ray)(ref ray)).direction), Vector3.Dot(normalized, ((Ray)(ref ray)).direction));
		Vector2 normalized2 = ((Vector2)(ref val4)).normalized;
		float num = 2f * (normalized2.x * val3.x + normalized2.y * val3.y);
		float num2 = ((Vector2)(ref val3)).sqrMagnitude - sqrRadius;
		float num3 = num * num - 4f * num2;
		if (num3 < 0f)
		{
			float num4 = (0f - val3.x) / normalized2.x;
			if (!(val3.y + normalized2.y * num4 > 0f))
			{
				return -90f;
			}
			return 90f;
		}
		float num5 = (0f - num - Mathf.Sqrt(num3)) / 2f;
		float num6 = (0f - num + Mathf.Sqrt(num3)) / 2f;
		float num7 = ((Mathf.Abs(num5) < Mathf.Abs(num6)) ? num5 : num6);
		Vector2 val5 = val3 + normalized2 * num7;
		return Mathf.Atan2(val5.y, val5.x) * 57.29578f;
	}

	public static float RayPlaneOffset(Ray ray, Vector3 origin, Vector3 normal, Vector3 up)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		float num = Vector3.Dot(origin - ((Ray)(ref ray)).origin, normal);
		float num2 = Vector3.Dot(((Ray)(ref ray)).direction, normal);
		float num3 = num / num2;
		return Vector3.Dot(((Ray)(ref ray)).origin + num3 * ((Ray)(ref ray)).direction - origin, up);
	}
}
public class MultiConstructor : MultiConstructor, IPatchOnLoad
{
	public void PatchOnLoad()
	{
		((Thing)(object)this).ApplyPaintableParent((ColorType)6);
		((Thing)(object)this).ApplyBlueprintMaterials();
	}
}
public class MyGraphicsRaycaster : MyGraphicsRaycaster
{
}
[HarmonyPatch(typeof(CableNetwork))]
public static class CableNetworkPatch
{
	[HarmonyPatch("HandleDataNetTransmissionDevice", new Type[] { typeof(Device) })]
	[HarmonyPostfix]
	private static void HandleDataNetTransmissionDevice(CableNetwork __instance, Device device, List<Device> ____dataDeviceList)
	{
		if (device is IPlacementBoardStructure)
		{
			____dataDeviceList.Add(device);
		}
	}
}
[HarmonyPatch]
internal static class CircuitboardDeviceListChangeTaskPatch
{
	[HarmonyTargetMethod]
	private static MethodInfo TargetMethod()
	{
		Type[] nestedTypes = typeof(Circuitboard).GetNestedTypes(BindingFlags.NonPublic);
		foreach (Type type in nestedTypes)
		{
			if (typeof(IAsyncStateMachine).IsAssignableFrom(type) && type.Name.Contains("DeviceListChangeTask"))
			{
				return type.GetMethod("MoveNext", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			}
		}
		throw new Exception("Could not find state machine for Circuitboard.DeviceListChangeTask");
	}

	[HarmonyTranspiler]
	private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Expected O, but got Unknown
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Expected O, but got Unknown
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Expected O, but got Unknown
		CodeMatcher val = new CodeMatcher(instructions, (ILGenerator)null);
		val.MatchStartForward((CodeMatch[])(object)new CodeMatch[1]
		{
			new CodeMatch((OpCode?)OpCodes.Ldfld, (object)PatchUtils.Field(() => ((Motherboard)null).ParentComputer), (string)null)
		});
		val.ThrowIfInvalid("could not find ParentComputer accessor in Circuitboard.DeviceListChangeTask");
		val.Advance(-1);
		CodeInstruction instruction = val.Instruction;
		val.MatchStartForward((CodeMatch[])(object)new CodeMatch[1]
		{
			new CodeMatch((OpCode?)OpCodes.Callvirt, (object)PatchUtils.PropertyGetter(() => ((IComputer)null).DataCable), (string)null)
		});
		val.MatchStartForward((CodeMatch[])(object)new CodeMatch[1]
		{
			new CodeMatch((OpCode?)OpCodes.Newobj, (object)PatchUtils.Constructor(() => new List<Device>()), (string)null)
		});
		val.ThrowIfInvalid("could not find insertion point for Circuitboard.DeviceListChangeTask");
		val.RemoveInstruction();
		val.InsertAndAdvance((CodeInstruction[])(object)new CodeInstruction[2]
		{
			new CodeInstruction(instruction.opcode, instruction.operand),
			CodeInstruction.Call((Expression<Action>)(() => GetCircuitboardDevices(null)))
		});
		return val.Instructions();
	}

	private static List<Device> GetCircuitboardDevices(Circuitboard circuitboard)
	{
		if (!(((Motherboard)circuitboard).ParentComputer is StructureConsoleConsole structureConsoleConsole))
		{
			return new List<Device>();
		}
		return structureConsoleConsole.GetDataDeviceList();
	}
}
[HarmonyPatch(typeof(Circuitboard))]
internal static class CircuitboardPatch
{
	[HarmonyPatch("MotherboardCommand")]
	[HarmonyTranspiler]
	private static IEnumerable<CodeInstruction> MotherboardCommand(IEnumerable<CodeInstruction> instructions)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Expected O, but got Unknown
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Expected O, but got Unknown
		CodeMatcher val = new CodeMatcher(instructions, (ILGenerator)null);
		while (true)
		{
			val.MatchStartForward((CodeMatch[])(object)new CodeMatch[2]
			{
				CodeMatch.op_Implicit(new CodeInstruction(OpCodes.Callvirt, (object)PatchUtils.PropertyGetter(() => ((IComputer)null).DataCable))),
				CodeMatch.op_Implicit(new CodeInstruction(OpCodes.Call, (object)PatchUtils.UnaryOp(() => (byte)null != 0)))
			});
			if (!val.IsValid)
			{
				break;
			}
			val.RemoveInstructions(2);
			val.InsertAndAdvance((CodeInstruction[])(object)new CodeInstruction[1] { CodeInstruction.Call((Expression<Action>)(() => IsConnected(null))) });
		}
		return val.Instructions();
	}

	public static bool IsConnected(IComputer parentComputer)
	{
		if (parentComputer is StructureConsoleConsole)
		{
			return true;
		}
		return Object.op_Implicit((Object)(object)parentComputer.DataCable);
	}
}
[HarmonyPatch(typeof(ComputerButton))]
internal static class ComputerButtonPatch
{
	[HarmonyPatch("SetColor")]
	[HarmonyPrefix]
	private static bool SetColor(ComputerButton __instance)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		if (!(__instance.Parent?.ParentComputer is StructureConsoleConsole structureConsoleConsole))
		{
			return true;
		}
		ColorBlock colors = ((Selectable)__instance.Button).colors;
		if (!Object.op_Implicit((Object)(object)__instance.AssignedDevice))
		{
			((ColorBlock)(ref colors)).normalColor = Color.red;
		}
		else if (__instance.Parent.LinkedDevices.Contains(__instance.AssignedDevice))
		{
			((ColorBlock)(ref colors)).normalColor = (structureConsoleConsole.IsNetworkDevice(__instance.AssignedDevice) ? Color.green : Color.red);
		}
		else
		{
			((ColorBlock)(ref colors)).normalColor = Color.white;
			((Selectable)__instance.Button).interactable = __instance.Parent.CanDeviceLink(__instance.AssignedDevice);
		}
		((Selectable)__instance.Button).colors = colors;
		return false;
	}
}
[HarmonyPatch(typeof(Motherboard))]
internal static class MotherboardPatch
{
	[HarmonyPatch("IsDeviceConnected")]
	[HarmonyPrefix]
	private static bool IsDeviceConnected(Motherboard __instance, Device device, ref bool __result)
	{
		if (!(__instance.ParentComputer is StructureConsoleConsole structureConsoleConsole))
		{
			return true;
		}
		__result = structureConsoleConsole.IsNetworkDevice(device);
		return false;
	}
}
[HarmonyPatch(typeof(GasDisplay))]
internal static class GasDisplayPatch
{
	[HarmonyPatch("OnThreadUpdate")]
	[HarmonyTranspiler]
	private static IEnumerable<CodeInstruction> OnThreadUpdate(IEnumerable<CodeInstruction> instructions)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Expected O, but got Unknown
		CodeMatcher val = new CodeMatcher(instructions, (ILGenerator)null);
		while (true)
		{
			val.MatchStartForward((CodeMatch[])(object)new CodeMatch[1] { CodeMatch.op_Implicit(new CodeInstruction(OpCodes.Callvirt, (object)PatchUtils.PropertyGetter(() => ((IComputer)null).DataCableNetwork))) });
			if (!val.IsValid)
			{
				break;
			}
			val.RemoveInstruction();
			val.InsertAndAdvance((CodeInstruction[])(object)new CodeInstruction[1] { CodeInstruction.Call((Expression<Action>)(() => CircuitboardPatch.IsConnected(null))) });
		}
		return val.Instructions();
	}
}
[HarmonyPatch(typeof(Computer))]
internal static class ComputerPatch
{
	[HarmonyPatch("EnableAppropriateScreen")]
	[HarmonyPrefix]
	private static bool EnableAppropriateScreen()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		return (int)GameManager.GameState > 0;
	}
}
internal static class PatchUtils
{
	public static MethodInfo Method(Expression<Action> expr)
	{
		return (expr.Body as MethodCallExpression).Method;
	}

	public static FieldInfo Field<T>(Expression<Func<T>> expression)
	{
		return (expression.Body as MemberExpression).Member as FieldInfo;
	}

	public static ConstructorInfo Constructor<T>(Expression<Func<T>> expression)
	{
		return (expression.Body as NewExpression).Constructor;
	}

	public static MethodInfo PropertyGetter<T>(Expression<Func<T>> expr)
	{
		return ((expr.Body as MemberExpression).Member as PropertyInfo).GetGetMethod();
	}

	public static MethodInfo UnaryOp<T>(Expression<Func<T>> expr)
	{
		return (expr.Body as UnaryExpression).Method;
	}
}
[HarmonyPatch(typeof(CursorManager))]
internal static class CursorManagerPatch
{
	[HarmonyPatch("SetCursorTarget")]
	[HarmonyPrefix]
	private static void SetCursorTarget()
	{
		DragCursor.UpdateDragCursor();
	}
}
[HarmonyPatch(typeof(CameraCollisionHandler))]
internal static class CameraCollisionHandlerPatch
{
	[HarmonyPatch("IsCollideableObject")]
	[HarmonyPrefix]
	private static bool IsCollideableObject(Collider collider, bool __result)
	{
		if ((Object)(object)collider == (Object)(object)DragCursor.Collider)
		{
			__result = false;
			return false;
		}
		return true;
	}
}
[HarmonyPatch]
public class PrefabPatch
{
	public const ColorType DefaultPaintableColor = (ColorType)6;

	public static StationeersTool ConsoleExitTool = StationeersTool.ANGLE_GRINDER;

	public static StationeersTool DeviceExitTool = StationeersTool.DRILL;

	public static ReadOnlyCollection<GameObject> prefabs { get; set; }

	[HarmonyPatch(typeof(Prefab), "LoadAll")]
	public static void Prefix()
	{
		try
		{
			Debug.Log((object)"Prefab Patch started");
			Shader.Find("Standard");
			foreach (GameObject prefab in prefabs)
			{
				if ((Object)(object)prefab == (Object)null)
				{
					Debug.Log((object)"GameObject Null");
					continue;
				}
				Thing component = prefab.GetComponent<Thing>();
				if (!((Object)(object)component == (Object)null))
				{
					if (component is IPatchOnLoad patchOnLoad)
					{
						Debug.Log((object)("PatchOnLoad " + ((Object)component).name));
						patchOnLoad.PatchOnLoad();
					}
					else
					{
						Debug.Log((object)("no PatchOnLoad for " + ((Object)component).name));
					}
					Debug.Log((object)(((Object)prefab).name + " added to WorldManager"));
					WorldManager.Instance.SourcePrefabs.Add(component);
				}
			}
		}
		catch (Exception ex)
		{
			Debug.Log((object)ex.Message);
			Debug.LogException(ex);
		}
	}
}
public static class PrefabExtensions
{
	public static void ApplyPaintableParent(this Thing thing, ColorType color)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		MeshRenderer renderer = default(MeshRenderer);
		if (((Component)thing).gameObject.TryGetComponent<MeshRenderer>(ref renderer))
		{
			ApplyPaintable(thing, renderer, color);
		}
	}

	public static void ApplyPaintableChild(this Thing thing, string childName, ColorType color, bool fallback)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		Transform val = ((Component)thing).transform.Find(childName);
		MeshRenderer renderer = default(MeshRenderer);
		if ((Object)(object)val == (Object)null || !((Component)val).TryGetComponent<MeshRenderer>(ref renderer))
		{
			if (fallback)
			{
				Debug.LogWarning((object)(childName + " child not found in " + thing.PrefabName + ". Falling back."));
				thing.ApplyPaintableParent(color);
			}
			else
			{
				Debug.LogWarning((object)(childName + " child not found in " + thing.PrefabName + ". Skipping fallback"));
			}
		}
		else
		{
			ApplyPaintable(thing, renderer, color);
		}
	}

	private static void ApplyPaintable(Thing thing, MeshRenderer renderer, ColorType color)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected I4, but got Unknown
		Material normal = Singleton<GameManager>.Instance.CustomColors[(int)color].Normal;
		Material[] sharedMaterials = ((Renderer)renderer).sharedMaterials;
		if (sharedMaterials.Length != 0)
		{
			sharedMaterials[0] = normal;
			((Renderer)renderer).sharedMaterials = sharedMaterials;
			thing.PaintableMaterial = normal;
		}
	}

	public static void ApplyBlueprintMaterials(this Thing thing)
	{
		if (!((Object)(object)thing.Blueprint == (Object)null))
		{
			MeshRenderer[] componentsInChildren = thing.Blueprint.GetComponentsInChildren<MeshRenderer>(true);
			foreach (MeshRenderer obj in componentsInChildren)
			{
				((Renderer)obj).sharedMaterials = StationeersModsUtility.GetBlueprintMaterials(((Renderer)obj).sharedMaterials.Length);
			}
		}
	}

	public static void SetEntryTool(this Structure structure, string prefabName, int stage = 0)
	{
		ref Item toolEntry = ref ((ToolBasic)structure.BuildStates[stage].Tool).ToolEntry;
		Thing obj = StationeersModsUtility.FindPrefab(prefabName);
		toolEntry = (Item)(object)((obj is Item) ? obj : null);
	}

	public static void SetExitTool(this Structure structure, StationeersTool tool, int stage = 0)
	{
		structure.BuildStates[stage].Tool.ToolExit = StationeersModsUtility.FindTool(tool);
	}
}
public interface IPatchOnLoad
{
	void PatchOnLoad();
}
[HarmonyPatch]
public class SaveDataPatch
{
	[HarmonyPatch(typeof(XmlSaveLoad), "AddExtraTypes")]
	public static void Prefix(ref List<Type> extraTypes)
	{
		extraTypes.Add(typeof(ConsoleBoardSaveData));
		extraTypes.Add(typeof(ConsoleDeviceSaveData));
		extraTypes.Add(typeof(ConsoleLogicSaveData));
	}
}
[HarmonyPatch(typeof(Stationpedia))]
internal static class StationpediaPatches
{
	[HarmonyPatch("AddLogicModeInfo")]
	[HarmonyPrefix]
	private static bool AddLogicModeInfo(Thing prefab, ref StationpediaPage page)
	{
		if (prefab is StructureConsoleGaugeManual)
		{
			return false;
		}
		return true;
	}
}
[HarmonyPatch(typeof(Structure))]
internal static class StructurePatch
{
	[HarmonyPatch(/*Could not decode attribute arguments.*/)]
	[HarmonyPrefix]
	private static bool IsStructureCompleted(Structure __instance, ref bool __result)
	{
		if (__instance is StructureConsoleBoard)
		{
			__result = true;
			return false;
		}
		return true;
	}
}
public static class TextQueue
{
	private struct Entry
	{
		public Thing Thing;

		public RenderTexture TargetTexture;

		public string Text;
	}

	private static readonly Queue<Entry> queue = new Queue<Entry>();

	private static readonly WaitForEndOfFrame WaitFrame = new WaitForEndOfFrame();

	public static void RenderText(Thing thing, RenderTexture targetTexture, string text)
	{
		if (GameManager.IsBatchMode || (Object)(object)targetTexture == (Object)null)
		{
			return;
		}
		lock (queue)
		{
			queue.Enqueue(new Entry
			{
				Thing = thing,
				TargetTexture = targetTexture,
				Text = text
			});
		}
	}

	public static IEnumerator RenderCoroutine()
	{
		if (GameManager.IsBatchMode)
		{
			yield break;
		}
		while (!Object.op_Implicit((Object)(object)CursorManager.Instance))
		{
			yield return WaitFrame;
		}
		while (true)
		{
			Camera camera = CursorManager.Instance.TextCamera;
			Entry entry = default(Entry);
			lock (queue)
			{
				if (queue.Count > 0)
				{
					entry = queue.Dequeue();
				}
			}
			if (!Object.op_Implicit((Object)(object)entry.Thing))
			{
				((Component)camera).gameObject.SetActive(false);
				yield return WaitFrame;
				continue;
			}
			((Component)camera).gameObject.SetActive(true);
			camera.targetTexture = entry.TargetTexture;
			((TMP_Text)CursorManager.Instance.LabelTextField).text = entry.Text;
			yield return WaitFrame;
			camera.Render();
		}
	}
}
[HarmonyPatch(typeof(Sign))]
internal static class SignPatch
{
	[HarmonyPatch("ChangeText")]
	[HarmonyPrefix]
	private static bool ChangeText(Sign __instance, string labelText, RenderTexture ____textTexture)
	{
		TextQueue.RenderText((Thing)(object)__instance, ____textTexture, labelText);
		return false;
	}
}
[HarmonyPatch(typeof(Label))]
internal static class LabelPatch
{
	[HarmonyPatch("ChangeText")]
	[HarmonyPrefix]
	private static bool ChangeText(Label __instance, string labelText, RenderTexture ____textTexture)
	{
		TextQueue.RenderText((Thing)(object)__instance, ____textTexture, labelText);
		return false;
	}
}
public class StructureConsoleBoard : Device, IPlacementBoardHost, IReferencable, IEvaluable, ISmartRotatable, IPseudoNetworkMember<StructureConsoleBoard>, IPatchOnLoad
{
	public static PseudoNetworkType<StructureConsoleBoard> ConsoleNetworkType = new PseudoNetworkType<StructureConsoleBoard>();

	[Header("ISmartRotation")]
	public ConnectionType ConnectionType = (ConnectionType)8;

	public int[] OpenEndsPermutation = new int[6] { 0, 1, 2, 3, 4, 5 };

	public Transform BoardOrigin1;

	public Transform BoardOrigin2;

	public Transform BoardOrigin3;

	public Transform BoardOrigin4;

	public List<BoxCollider> BoardColliders1 = new List<BoxCollider>();

	public List<BoxCollider> BoardColliders2 = new List<BoxCollider>();

	public List<BoxCollider> BoardColliders3 = new List<BoxCollider>();

	public List<BoxCollider> BoardColliders4 = new List<BoxCollider>();

	private BoardRef<ConsoleBoard> BoardRef1;

	private BoardRef<ConsoleBoard> BoardRef2;

	private BoardRef<ConsoleBoard> BoardRef3;

	private BoardRef<ConsoleBoard> BoardRef4;

	public int LightMaterialIndex = 1;

	public ConsoleBoard Board1 => BoardRef1?.Board;

	public ConsoleBoard Board2 => BoardRef2?.Board;

	public ConsoleBoard Board3 => BoardRef3?.Board;

	public ConsoleBoard Board4 => BoardRef4?.Board;

	public Material LightMaterial => ((Renderer)((Component)this).GetComponent<MeshRenderer>()).materials[LightMaterialIndex];

	public PseudoNetwork<StructureConsoleBoard> Network { get; } = ConsoleNetworkType.Join();

	public IEnumerable<Connection> Connections => ((SmallGrid)this).OpenEnds.Where((Connection end) => (int)end.ConnectionType == 64 || end.ConnectionType == ConsoleNetworkType.ConnectionType);

	public void PatchOnLoad()
	{
		((Thing)(object)this).ApplyPaintableParent((ColorType)6);
		((Thing)(object)this).ApplyPaintableChild("Panel", (ColorType)6, fallback: true);
		((Thing)(object)this).ApplyBlueprintMaterials();
		((Structure)(object)this).SetExitTool(PrefabPatch.ConsoleExitTool);
		((Structure)(object)this).SetEntryTool("ItemPlasticSheets", 1);
		((Structure)(object)this).SetExitTool(StationeersTool.CROWBAR, 1);
		ConsoleNetworkType.PatchConnections(this);
	}

	public override ThingSaveData SerializeSave()
	{
		ThingSaveData val;
		ThingSaveData result = (val = (ThingSaveData)(object)new ConsoleBoardSaveData());
		((Thing)this).InitialiseSaveData(ref val);
		return result;
	}

	protected override void InitialiseSaveData(ref ThingSaveData baseData)
	{
		((Structure)this).InitialiseSaveData(ref baseData);
		if (baseData is ConsoleBoardSaveData consoleBoardSaveData)
		{
			if (Board1 != null)
			{
				consoleBoardSaveData.Board = BoardHostHooks.SerializeBoard<ConsoleBoard>((IPlacementBoardHost)(object)this, Board1);
			}
			if (Board2 != null)
			{
				consoleBoardSaveData.Board2 = BoardHostHooks.SerializeBoard<ConsoleBoard>((IPlacementBoardHost)(object)this, Board2);
			}
			if (Board3 != null)
			{
				consoleBoardSaveData.Board3 = BoardHostHooks.SerializeBoard<ConsoleBoard>((IPlacementBoardHost)(object)this, Board3);
			}
			if (Board4 != null)
			{
				consoleBoardSaveData.Board4 = BoardHostHooks.SerializeBoard<ConsoleBoard>((IPlacementBoardHost)(object)this, Board4);
			}
		}
	}

	public override void DeserializeSave(ThingSaveData baseData)
	{
		((Structure)this).DeserializeSave(baseData);
		if (baseData is ConsoleBoardSaveData consoleBoardSaveData)
		{
			if ((Object)(object)BoardOrigin1 != (Object)null && consoleBoardSaveData.Board != null)
			{
				BoardHostHooks.DeserializeBoard<ConsoleBoard>((IPlacementBoardHost)(object)this, consoleBoardSaveData.Board, ref BoardRef1, BoardOrigin1);
			}
			if ((Object)(object)BoardOrigin2 != (Object)null && consoleBoardSaveData.Board2 != null)
			{
				BoardHostHooks.DeserializeBoard<ConsoleBoard>((IPlacementBoardHost)(object)this, consoleBoardSaveData.Board2, ref BoardRef2, BoardOrigin2);
			}
			if ((Object)(object)BoardOrigin3 != (Object)null && consoleBoardSaveData.Board3 != null)
			{
				BoardHostHooks.DeserializeBoard<ConsoleBoard>((IPlacementBoardHost)(object)this, consoleBoardSaveData.Board3, ref BoardRef3, BoardOrigin3);
			}
			if ((Object)(object)BoardOrigin4 != (Object)null && consoleBoardSaveData.Board4 != null)
			{
				BoardHostHooks.DeserializeBoard<ConsoleBoard>((IPlacementBoardHost)(object)this, consoleBoardSaveData.Board4, ref BoardRef4, BoardOrigin4);
			}
		}
	}

	public override void OnFinishedLoad()
	{
		((Structure)this).OnFinishedLoad();
		if ((Object)(object)BoardOrigin1 != (Object)null)
		{
			BoardHostHooks.OnFinishedLoadBoard<ConsoleBoard>((IPlacementBoardHost)(object)this, ref BoardRef1, BoardOrigin1);
		}
		if ((Object)(object)BoardOrigin2 != (Object)null)
		{
			BoardHostHooks.OnFinishedLoadBoard<ConsoleBoard>((IPlacementBoardHost)(object)this, ref BoardRef2, BoardOrigin2);
		}
		if ((Object)(object)BoardOrigin3 != (Object)null)
		{
			BoardHostHooks.OnFinishedLoadBoard<ConsoleBoard>((IPlacementBoardHost)(object)this, ref BoardRef3, BoardOrigin3);
		}
		if ((Object)(object)BoardOrigin4 != (Object)null)
		{
			BoardHostHooks.OnFinishedLoadBoard<ConsoleBoard>((IPlacementBoardHost)(object)this, ref BoardRef4, BoardOrigin4);
		}
	}

	public override void OnRegistered(Cell cell)
	{
		((Device)this).OnRegistered(cell);
		if ((Object)(object)BoardOrigin1 != (Object)null)
		{
			BoardHostHooks.OnRegisteredBoard<ConsoleBoard>((IPlacementBoardHost)(object)this, ref BoardRef1, BoardOrigin1);
		}
		if ((Object)(object)BoardOrigin2 != (Object)null)
		{
			BoardHostHooks.OnRegisteredBoard<ConsoleBoard>((IPlacementBoardHost)(object)this, ref BoardRef2, BoardOrigin2);
		}
		if ((Object)(object)BoardOrigin3 != (Object)null)
		{
			BoardHostHooks.OnRegisteredBoard<ConsoleBoard>((IPlacementBoardHost)(object)this, ref BoardRef3, BoardOrigin3);
		}
		if ((Object)(object)BoardOrigin4 != (Object)null)
		{
			BoardHostHooks.OnRegisteredBoard<ConsoleBoard>((IPlacementBoardHost)(object)this, ref BoardRef4, BoardOrigin4);
		}
		if (GameManager.RunSimulation)
		{
			ConsoleNetworkType.RebuildNetworkCreate(this);
		}
	}

	public override void OnDestroy()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		if ((int)GameManager.GameState != 0)
		{
			if (BoardRef1 != null)
			{
				BoardHostHooks.OnDestroyedBoard<ConsoleBoard>((IPlacementBoardHost)(object)this, BoardRef1);
			}
			if (BoardRef2 != null)
			{
				BoardHostHooks.OnDestroyedBoard<ConsoleBoard>((IPlacementBoardHost)(object)this, BoardRef2);
			}
			if (BoardRef3 != null)
			{
				BoardHostHooks.OnDestroyedBoard<ConsoleBoard>((IPlacementBoardHost)(object)this, BoardRef3);
			}
			if (BoardRef4 != null)
			{
				BoardHostHooks.OnDestroyedBoard<ConsoleBoard>((IPlacementBoardHost)(object)this, BoardRef4);
			}
			ConsoleNetworkType.RebuildNetworkDestroy(this);
			((Device)this).OnDestroy();
		}
	}

	public override void SerializeOnJoin(RocketBinaryWriter writer)
	{
		((Structure)this).SerializeOnJoin(writer);
		if (Board1 != null)
		{
			BoardHostHooks.SerializeBoardOnJoin<ConsoleBoard>(writer, (IPlacementBoardHost)(object)this, Board1);
		}
		if (Board2 != null)
		{
			BoardHostHooks.SerializeBoardOnJoin<ConsoleBoard>(writer, (IPlacementBoardHost)(object)this, Board2);
		}
		if (Board3 != null)
		{
			BoardHostHooks.SerializeBoardOnJoin<ConsoleBoard>(writer, (IPlacementBoardHost)(object)this, Board3);
		}
		if (Board4 != null)
		{
			BoardHostHooks.SerializeBoardOnJoin<ConsoleBoard>(writer, (IPlacementBoardHost)(object)this, Board4);
		}
	}

	public override void DeserializeOnJoin(RocketBinaryReader reader)
	{
		((Structure)this).DeserializeOnJoin(reader);
		if ((Object)(object)BoardOrigin1 != (Object)null)
		{
			BoardHostHooks.DeserializeBoardOnJoin<ConsoleBoard>(reader, (IPlacementBoardHost)(object)this, ref BoardRef1, BoardOrigin1);
		}
		if ((Object)(object)BoardOrigin2 != (Object)null)
		{
			BoardHostHooks.DeserializeBoardOnJoin<ConsoleBoard>(reader, (IPlacementBoardHost)(object)this, ref BoardRef2, BoardOrigin2);
		}
		if ((Object)(object)BoardOrigin3 != (Object)null)
		{
			BoardHostHooks.DeserializeBoardOnJoin<ConsoleBoard>(reader, (IPlacementBoardHost)(object)this, ref BoardRef3, BoardOrigin3);
		}
		if ((Object)(object)BoardOrigin4 != (Object)null)
		{
			BoardHostHooks.DeserializeBoardOnJoin<ConsoleBoard>(reader, (IPlacementBoardHost)(object)this, ref BoardRef4, BoardOrigin4);
		}
		ConsoleNetworkType.RebuildNetworkCreate(this);
	}

	public IEnumerable<BoxCollider> CollidersForBoard(PlacementBoard board)
	{
		if ((object)board == Board1 && BoardColliders1 != null)
		{
			return BoardColliders1;
		}
		if ((object)board == Board2 && BoardColliders2 != null)
		{
			return BoardColliders2;
		}
		if ((object)board == Board3 && BoardColliders3 != null)
		{
			return BoardColliders3;
		}
		if ((object)board == Board4 && BoardColliders4 != null)
		{
			return BoardColliders4;
		}
		return Enumerable.Empty<BoxCollider>();
	}

	public IEnumerable<PlacementBoard> GetPlacementBoards()
	{
		if (Board1 != null)
		{
			yield return (PlacementBoard)(object)Board1;
		}
		if (Board2 != null)
		{
			yield return (PlacementBoard)(object)Board2;
		}
		if (Board3 != null)
		{
			yield return (PlacementBoard)(object)Board3;
		}
		if (Board4 != null)
		{
			yield return (PlacementBoard)(object)Board4;
		}
	}

	public void OnBoardStructureRegistered(PlacementBoard board, IPlacementBoardStructure structure)
	{
		Device val = (Device)(object)((structure is Device) ? structure : null);
		if (val == null)
		{
			return;
		}
		foreach (StructureConsoleBoard member in Network.Members)
		{
			if ((Object)(object)((Device)member).DataCable != (Object)null)
			{
				((Device)member).DataCable.CableNetwork.AddDevice(((Device)member).DataCable, val);
			}
		}
	}

	public void OnBoardStructureDeregistered(PlacementBoard board, IPlacementBoardStructure structure)
	{
	}

	public override PassiveTooltip GetPassiveTooltip(Collider hitCollider)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		PassiveTooltip passiveTooltip = ((Device)this).GetPassiveTooltip(hitCollider);
		if (!((Thing)this).OnOff)
		{
			if (string.IsNullOrEmpty(passiveTooltip.Title))
			{
				passiveTooltip.Title = ((Thing)this).DisplayName;
			}
			passiveTooltip.Extended = (((Thing)this).DisplayName + " is off. Set On with logic to re-enable\n" + passiveTooltip.Extended).TrimEnd();
		}
		return passiveTooltip;
	}

	public ConnectionType GetConnectionType()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return ConnectionType;
	}

	public void SetOpenEndsPermutation(int[] permutation)
	{
		OpenEndsPermutation = (int[])permutation.Clone();
	}

	public void SetConnectionType(ConnectionType connectionType)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		ConnectionType = connectionType;
	}

	public int[] GetOpenEndsPermutation()
	{
		return (int[])OpenEndsPermutation.Clone();
	}

	public void OnMembersChanged()
	{
		RefreshNetworkPowered();
	}

	public IEnumerable<IPlacementBoardStructure> BoardStructures()
	{
		if (Board1 != null)
		{
			foreach (IPlacementBoardStructure structure in ((PlacementBoard)Board1).Structures)
			{
				yield return structure;
			}
		}
		if (Board2 != null)
		{
			foreach (IPlacementBoardStructure structure2 in ((PlacementBoard)Board2).Structures)
			{
				yield return structure2;
			}
		}
		if (Board3 != null)
		{
			foreach (IPlacementBoardStructure structure3 in ((PlacementBoard)Board3).Structures)
			{
				yield return structure3;
			}
		}
		if (Board4 == null)
		{
			yield break;
		}
		foreach (IPlacementBoardStructure structure4 in ((PlacementBoard)Board4).Structures)
		{
			yield return structure4;
		}
	}

	public override void OnInteractableUpdated(Interactable interactable)
	{
		((Device)this).OnInteractableUpdated(interactable);
		if (interactable == ((Thing)this).InteractPowered)
		{
			RefreshNetworkPowered();
		}
	}

	public void RefreshNetworkPowered()
	{
		bool flag = false;
		foreach (StructureConsoleBoard member in Network.Members)
		{
			flag |= ((Thing)member).Powered;
		}
		foreach (StructureConsoleBoard member2 in Network.Members)
		{
			member2.SetBoardsPowered(flag);
		}
	}

	public void SetBoardsPowered(bool powered)
	{
		if (Board1 != null)
		{
			Board1.Powered = powered;
		}
		if (Board2 != null)
		{
			Board2.Powered = powered;
		}
		if (Board3 != null)
		{
			Board3.Powered = powered;
		}
		if (Board4 != null)
		{
			Board4.Powered = powered;
		}
		if (LightMaterialIndex == -1)
		{
			return;
		}
		MeshRenderer component = ((Component)this).GetComponent<MeshRenderer>();
		if ((Object)(object)component != (Object)null && LightMaterialIndex < ((Renderer)component).materials.Length)
		{
			Material val = ((Renderer)component).materials[LightMaterialIndex];
			if (powered)
			{
				val.EnableKeyword("_EMISSION");
				val.globalIlluminationFlags = (MaterialGlobalIlluminationFlags)1;
			}
			else
			{
				val.DisableKeyword("_EMISSION");
				val.globalIlluminationFlags = (MaterialGlobalIlluminationFlags)4;
			}
		}
	}

	public override bool CanLogicRead(LogicType logicType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Invalid comparison between Unknown and I4
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Invalid comparison between Unknown and I4
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Invalid comparison between Unknown and I4
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Invalid comparison between Unknown and I4
		if ((int)logicType == 33 || (int)logicType == 84 || (int)logicType == 217 || (int)logicType == 268)
		{
			return true;
		}
		if ((int)GameManager.GameState == 3 && ((int)logicType == 1 || (int)logicType == 28))
		{
			return true;
		}
		return ((Device)this).CanLogicRead(logicType);
	}

	public override bool CanLogicWrite(LogicType logicType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Invalid comparison between Unknown and I4
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		if ((int)GameManager.GameState != 3)
		{
			return false;
		}
		if ((int)logicType != 9)
		{
			if ((int)logicType == 28)
			{
				return true;
			}
			return ((Device)this).CanLogicWrite(logicType);
		}
		return true;
	}

	public override double GetLogicValue(LogicType logicType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Invalid comparison between Unknown and I4
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Invalid comparison between Unknown and I4
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Invalid comparison between Unknown and I4
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Invalid comparison between Unknown and I4
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		int num;
		if ((int)logicType <= 33)
		{
			if ((int)logicType == 1)
			{
				if (((Thing)this).OnOff)
				{
					Cable powerCable = ((Device)this).PowerCable;
					if (((powerCable != null) ? powerCable.CableNetwork : null) != null)
					{
						num = 1;
						goto IL_004c;
					}
				}
				num = 0;
				goto IL_004c;
			}
			if ((int)logicType == 28)
			{
				return ((Thing)this).OnOff ? 1 : 0;
			}
			if ((int)logicType == 33)
			{
				return base.UsedPower;
			}
		}
		else
		{
			if ((int)logicType == 84)
			{
				return ((Thing)this).PrefabHash;
			}
			if ((int)logicType == 217)
			{
				return ((Thing)this).ReferenceId;
			}
			if ((int)logicType == 268)
			{
				return ((Thing)this).GetNameHash();
			}
		}
		return ((Device)this).GetLogicValue(logicType);
		IL_004c:
		return num;
	}

	public override void SetLogicValue(LogicType logicType, double value)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if ((int)logicType == 9 || (int)logicType == 28)
		{
			OnServer.Interact(((Thing)this).InteractOnOff, (int)value, false);
		}
		else
		{
			((Device)this).SetLogicValue(logicType, value);
		}
	}

	public override void InitializeDevice()
	{
		((Device)this).InitializeDevice();
		((Device)this).FindDataCable();
		if ((Object)(object)((Device)this).DataCable != (Object)null)
		{
			AddDataCableAll(((Device)this).DataCable);
		}
		RefreshNetworkPowered();
	}

	public override void OnAddCableNetwork(CableNetwork newNetwork)
	{
		((Device)this).OnAddCableNetwork(newNetwork);
		((Device)this).FindDataCable();
		if ((Object)(object)((Device)this).DataCable != (Object)null)
		{
			AddDataCableAll(((Device)this).DataCable);
		}
	}

	public override void OnRemoveCableNetwork(CableNetwork oldNetwork)
	{
		((Device)this).OnRemoveCableNetwork(oldNetwork);
		Cable dataCable = ((Device)this).DataCable;
		if (oldNetwork == ((dataCable != null) ? dataCable.CableNetwork : null))
		{
			RemoveDataCableAll(((Device)this).DataCable);
		}
	}

	public void OnMemberAdded(StructureConsoleBoard member)
	{
		if ((Object)(object)((Device)member).DataCable != (Object)null)
		{
			AddDataCable(((Device)member).DataCable);
		}
	}

	public void OnMemberRemoved(StructureConsoleBoard member)
	{
		if ((Object)(object)((Device)member).DataCable != (Object)null)
		{
			RemoveDataCable(((Device)member).DataCable);
		}
	}

	public void AddDataCableAll(Cable cable)
	{
		foreach (StructureConsoleBoard member in Network.Members)
		{
			member.AddDataCable(cable);
		}
	}

	public void AddDataCable(Cable cable)
	{
		foreach (IPlacementBoardStructure item in BoardStructures())
		{
			Device val = (Device)(object)((item is Device) ? item : null);
			if (val != null)
			{
				cable.CableNetwork.AddDevice(cable, val);
			}
		}
	}

	public void RemoveDataCableAll(Cable cable)
	{
		foreach (StructureConsoleBoard member in Network.Members)
		{
			member.RemoveDataCable(cable);
		}
	}

	public void RemoveDataCable(Cable cable)
	{
		foreach (IPlacementBoardStructure item in BoardStructures())
		{
			Device val = (Device)(object)((item is Device) ? item : null);
			if (val != null)
			{
				cable.CableNetwork.RemoveDevice(cable, val);
			}
		}
	}
}
public class StructureConsoleButton : LogicUnitBase, IPlacementBoardRelocatable, IPlacementBoardStructure, IReferencable, IEvaluable, IPatchOnLoad, IBoardPoweredListener
{
	public MeshRenderer emissiveRenderer;

	private Coroutine _resetRoutine;

	[CompilerGenerated]
	private Event m_ButtonPress;

	private bool _activated;

	public PlacementBoard Board { get; set; }

	public BoardCell[] BoardCells { get; set; }

	public override bool Powered => (Board as IPoweredBoard)?.Powered ?? false;

	public override bool OnOff => ((Thing)this).Powered;

	public event Event ButtonPress
	{
		[CompilerGenerated]
		add
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Expected O, but got Unknown
			Event val = this.m_ButtonPress;
			Event val2;
			do
			{
				val2 = val;
				Event value2 = (Event)Delegate.Combine((Delegate)(object)val2, (Delegate)(object)value);
				val = Interlocked.CompareExchange(ref this.m_ButtonPress, value2, val2);
			}
			while (val != val2);
		}
		[CompilerGenerated]
		remove
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Expected O, but got Unknown
			Event val = this.m_ButtonPress;
			Event val2;
			do
			{
				val2 = val;
				Event value2 = (Event)Delegate.Remove((Delegate)(object)val2, (Delegate)(object)value);
				val = Interlocked.CompareExchange(ref this.m_ButtonPress, value2, val2);
			}
			while (val != val2);
		}
	}

	public void PatchOnLoad()
	{
		((Thing)(object)this).ApplyPaintableChild("Button", (ColorType)2, fallback: false);
		((Thing)(object)this).ApplyBlueprintMaterials();
		((Structure)(object)this).SetExitTool(PrefabPatch.DeviceExitTool);
	}

	public override void Awake()
	{
		((Device)this).Awake();
		((Structure)(object)this).InitializeEmissive(emissiveRenderer);
		BoardStructureHooks.Awake<StructureConsoleButton>(this);
	}

	private IEnumerator WaitThenStop()
	{
		yield return (object)new WaitForSeconds(0.55f);
		((LogicUnitBase)this).Setting = 0.0;
		_activated = false;
		OnServer.Interact(((Thing)this).InteractActivate, 0, false);
	}

	public override void OnInteractableUpdated(Interactable interactable)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Invalid comparison between Unknown and I4
		((LogicUnitBase)this).OnInteractableUpdated(interactable);
		if (!_activated && GameManager.RunSimulation && (int)interactable.Action == 41 && interactable.State == 1)
		{
			((LogicUnitBase)this).Setting = interactable.State;
			if (_resetRoutine != null)
			{
				((MonoBehaviour)this).StopCoroutine(_resetRoutine);
			}
			_resetRoutine = ((MonoBehaviour)this).StartCoroutine(WaitThenStop());
		}
		((Structure)(object)this).SyncMaterials(emissiveRenderer);
	}

	public override void SetCustomColor(int index, bool emissive = false)
	{
		((Structure)this).SetCustomColor(index, emissive);
		((Structure)(object)this).SetEmissive(emissiveRenderer, emissive);
	}

	public void OnBoardPowerChanged()
	{
		((Structure)(object)this).SyncMaterials(emissiveRenderer);
	}

	public override CanConstructInfo CanConstruct()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return BoardStructureHooks.CanConstruct<StructureConsoleButton>(this);
	}

	public override void OnDeregistered()
	{
		((Device)this).OnDeregistered();
		BoardStructureHooks.OnDeregistered<StructureConsoleButton>(this);
	}

	public override ThingSaveData SerializeSave()
	{
		ThingSaveData val;
		ThingSaveData result = (val = (ThingSaveData)(object)new ConsoleLogicSaveData());
		((Thing)this).InitialiseSaveData(ref val);
		return result;
	}

	protected override void InitialiseSaveData(ref ThingSaveData baseData)
	{
		((LogicUnitBase)this).InitialiseSaveData(ref baseData);
		if (baseData is ConsoleLogicSaveData consoleLogicSaveData)
		{
			consoleLogicSaveData.Board = BoardStructureHooks.SerializeSave((IPlacementBoardStructure)(object)this);
		}
	}

	public override void DeserializeSave(ThingSaveData baseData)
	{
		((LogicUnitBase)this).DeserializeSave(baseData);
		if (baseData is ConsoleLogicSaveData consoleLogicSaveData)
		{
			BoardStructureHooks.DeserializeSave((IPlacementBoardStructure)(object)this, consoleLogicSaveData.Board);
		}
	}

	public override void SerializeOnJoin(RocketBinaryWriter writer)
	{
		((LogicUnitBase)this).SerializeOnJoin(writer);
		BoardStructureHooks.SerializeOnJoin(writer, (IPlacementBoardStructure)(object)this);
	}

	public override void DeserializeOnJoin(RocketBinaryReader reader)
	{
		((LogicUnitBase)this).DeserializeOnJoin(reader);
		BoardStructureHooks.DeserializeOnJoin(reader, (IPlacementBoardStructure)(object)this);
	}

	public override bool CanLogicRead(LogicType logicType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if ((int)logicType != 12)
		{
			return ((Device)this).CanLogicRead(logicType);
		}
		return true;
	}

	public override double GetLogicValue(LogicType logicType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if ((int)logicType != 12)
		{
			return ((Device)this).GetLogicValue(logicType);
		}
		return ((LogicUnitBase)this).Setting;
	}

	public override DelayedActionInstance InteractWith(Interactable interactable, Interaction interaction, bool doAction = true)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Invalid comparison between Unknown and I4
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		DelayedActionInstance val = new DelayedActionInstance
		{
			Duration = 0f,
			ActionMessage = interactable.ContextualName
		};
		if (((Thing)this).IsLocked)
		{
			return val.Fail(GameStrings.DeviceLocked.AsColor("red"));
		}
		if ((int)interactable.Action == 41)
		{
			if (((Thing)this).Activate == 1)
			{
				return val.Fail(GameStrings.GlobalAlreadyInUse);
			}
			if (!((Thing)this).IsAuthorized(((Interaction)(ref interaction)).SourceThing))
			{
				return val.Fail(GameStrings.AccessCardUnableToInteract);
			}
			if (!doAction)
			{
				return val.Succeed();
			}
			OnServer.Interact(interactable, 1, false);
			Event obj = this.ButtonPress;
			if (obj != null)
			{
				obj.Invoke();
			}
			return val.Succeed();
		}
		return ((LogicUnitBase)this).InteractWith(interactable, interaction, doAction);
	}

	public override DelayedActionInstance AttackWith(Attack attack, bool doAction = true)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		DynamicThing sourceItem = ((Attack)(ref attack)).SourceItem;
		Screwdriver val = (Screwdriver)(object)((sourceItem is Screwdriver) ? sourceItem : null);
		if (val != null)
		{
			return BoardRelocateHooks.StructureAttackWith((IPlacementBoardRelocatable)(object)this, attack, doAction, BoardRelocateHooks.NormalToolRelocateContinue((DynamicThing)(object)val));
		}
		return ((Device)this).AttackWith(attack, doAction);
	}

	public void OnStructureRelocated()
	{
	}

	Structure IPlacementBoardRelocatable.get_AsStructure()
	{
		return ((Thing)this).AsStructure;
	}

	string IPlacementBoardStructure.get_name()
	{
		return ((Object)this).name;
	}

	void IPlacementBoardStructure.SetStructureData(Quaternion localRotation, ulong ownerClientId, Grid3 localGrid, int customColourIndex)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		((Structure)this).SetStructureData(localRotation, ownerClientId, localGrid, customColourIndex);
	}
}
public class StructureConsoleCardReader : LogicUnitBase, IPlacementBoardRelocatable, IPlacementBoardStructure, IReferencable, IEvaluable, IPatchOnLoad, IBoardPoweredListener
{
	public static readonly string[] CardReaderModeStrings = Enum.GetNames(typeof(ColorType));

	[Header("Colorable/emissive part")]
	public MeshRenderer emissiveRenderer;

	private int modeValue;

	public PlacementBoard Board { get; set; }

	public BoardCell[] BoardCells { get; set; }

	public override bool Powered => (Board as IPoweredBoard)?.Powered ?? false;

	public override string[] ModeStrings => CardReaderModeStrings;

	public void PatchOnLoad()
	{
		((Thing)(object)this).ApplyPaintableChild("Bulb", (ColorType)2, fallback: false);
		((Thing)(object)this).ApplyBlueprintMaterials();
		((Structure)(object)this).SetExitTool(PrefabPatch.DeviceExitTool);
	}

	public override void Awake()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		((Device)this).Awake();
		((Structure)(object)this).InitializeEmissive(emissiveRenderer);
		BoardStructureHooks.Awake<StructureConsoleCardReader>(this);
		if (((Thing)this).Slots != null && ((Thing)this).Slots.Count > 0 && ((Thing)this).Slots[0] != null)
		{
			((Thing)this).Slots[0].OnOccupantChange += new Event(OnCardSlotOccupantChanged);
		}
		((Thing)this).SetCustomColor(((Thing)this).ColorState, ((Thing)this).Powered);
	}

	public override void OnInteractableUpdated(Interactable interactable)
	{
		((LogicUnitBase)this).OnInteractableUpdated(interactable);
		((Structure)(object)this).SyncMaterials(emissiveRenderer);
	}

	public override void OnDestroy()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		((Device)this).OnDestroy();
		if (((Thing)this).Slots != null && ((Thing)this).Slots.Count > 0 && ((Thing)this).Slots[0] != null)
		{
			((Thing)this).Slots[0].OnOccupantChange -= new Event(OnCardSlotOccupantChanged);
		}
	}

	public override void SetCustomColor(int index, bool emissive = false)
	{
		((Structure)this).SetCustomColor(index, emissive);
		((Structure)(object)this).SetEmissive(emissiveRenderer, emissive);
	}

	public void OnBoardPowerChanged()
	{
		((Structure)(object)this).SyncMaterials(emissiveRenderer);
	}

	public override CanConstructInfo CanConstruct()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return BoardStructureHooks.CanConstruct<StructureConsoleCardReader>(this);
	}

	public override void OnDeregistered()
	{
		((Device)this).OnDeregistered();
		if (Board != null)
		{
			BoardStructureHooks.OnDeregistered<StructureConsoleCardReader>(this);
		}
	}

	public override ThingSaveData SerializeSave()
	{
		ThingSaveData val;
		ThingSaveData result = (val = (ThingSaveData)(object)new ConsoleLogicSaveData());
		((Thing)this).InitialiseSaveData(ref val);
		return result;
	}

	protected override void InitialiseSaveData(ref ThingSaveData baseData)
	{
		((LogicUnitBase)this).InitialiseSaveData(ref baseData);
		if (baseData is ConsoleLogicSaveData consoleLogicSaveData)
		{
			consoleLogicSaveData.Board = BoardStructureHooks.SerializeSave((IPlacementBoardStructure)(object)this);
		}
	}

	public override void DeserializeSave(ThingSaveData baseData)
	{
		((LogicUnitBase)this).DeserializeSave(baseData);
		if (baseData is ConsoleLogicSaveData consoleLogicSaveData)
		{
			BoardStructureHooks.DeserializeSave((IPlacementBoardStructure)(object)this, consoleLogicSaveData.Board);
		}
	}

	public override void SerializeOnJoin(RocketBinaryWriter writer)
	{
		((LogicUnitBase)this).SerializeOnJoin(writer);
		BoardStructureHooks.SerializeOnJoin(writer, (IPlacementBoardStructure)(object)this);
	}

	public override void DeserializeOnJoin(RocketBinaryReader reader)
	{
		((LogicUnitBase)this).DeserializeOnJoin(reader);
		BoardStructureHooks.DeserializeOnJoin(reader, (IPlacementBoardStructure)(object)this);
	}

	public override void SetLogicValue(LogicType logicType, double value)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if ((int)logicType == 3)
		{
			modeValue = Mathf.Clamp((int)value, 0, 12);
			((LogicUnitBase)this).Setting = ((Device)this).GetLogicValue((LogicType)12);
		}
		else
		{
			((Device)this).SetLogicValue(logicType, value);
		}
	}

	public override bool CanLogicRead(LogicType logicType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if ((int)logicType != 12 && (int)logicType != 3)
		{
			return ((Device)this).CanLogicRead(logicType);
		}
		return true;
	}

	public override double GetLogicValue(LogicType logicType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Invalid comparison between Unknown and I4
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		if ((int)logicType == 12)
		{
			Slot obj = ((Thing)this).Slots[0];
			AccessCard val = ((obj != null) ? obj.Get<AccessCard>() : null);
			int num = 0;
			if ((Object)(object)val != (Object)null)
			{
				num = ((GetCardValue(((Thing)val).PrefabName) == (double)modeValue) ? 1 : 0);
			}
			((LogicUnitBase)this).Setting = num;
			return num;
		}
		if ((int)logicType == 3)
		{
			return modeValue;
		}
		return ((Device)this).GetLogicValue(logicType);
	}

	private double GetCardValue(string cardName)
	{
		return cardName switch
		{
			"AccessCardBlue" => 0, 
			"AccessCardGray" => 1, 
			"AccessCardGreen" => 2, 
			"AccessCardOrange" => 3, 
			"AccessCardRed" => 4, 
			"AccessCardYellow" => 5, 
			"AccessCardWhite" => 6, 
			"AccessCardBlack" => 7, 
			"AccessCardBrown" => 8, 
			"AccessCardKhaki" => 9, 
			"AccessCardPink" => 10, 
			"AccessCardPurple" => 11, 
			_ => 12, 
		};
	}

	private void OnCardSlotOccupantChanged()
	{
		((LogicUnitBase)this).Setting = ((Device)this).GetLogicValue((LogicType)12);
	}

	public override DelayedActionInstance AttackWith(Attack attack, bool doAction = true)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		DynamicThing sourceItem = ((Attack)(ref attack)).SourceItem;
		Screwdriver val = (Screwdriver)(object)((sourceItem is Screwdriver) ? sourceItem : null);
		if (val != null)
		{
			if (Board != null)
			{
				return BoardRelocateHooks.StructureAttackWith((IPlacementBoardRelocatable)(object)this, attack, doAction, BoardRelocateHooks.NormalToolRelocateContinue((DynamicThing)(object)val));
			}
			return ((Device)this).AttackWith(attack, doAction);
		}
		return ((Device)this).AttackWith(attack, doAction);
	}

	public void OnStructureRelocated()
	{
	}

	Structure IPlacementBoardRelocatable.get_AsStructure()
	{
		return ((Thing)this).AsStructure;
	}

	string IPlacementBoardStructure.get_name()
	{
		return ((Object)this).name;
	}

	void IPlacementBoardStructure.SetStructureData(Quaternion localRotation, ulong ownerClientId, Grid3 localGrid, int customColourIndex)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		((Structure)this).SetStructureData(localRotation, ownerClientId, localGrid, customColourIndex);
	}
}
public class StructureConsoleComputer : Computer, IPlacementBoardRelocatable, IPlacementBoardStructure, IReferencable, IEvaluable, IPatchOnLoad, IBoardPoweredListener, IComputer
{
	public PlacementBoard Board { get; set; }

	public BoardCell[] BoardCells { get; set; }

	public override bool Powered => (Board as IPoweredBoard)?.Powered ?? false;

	public void PatchOnLoad()
	{
		((Thing)(object)this).ApplyBlueprintMaterials();
		((Structure)(object)this).SetExitTool(PrefabPatch.DeviceExitTool);
	}

	public override void Awake()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		((Computer)this).Awake();
		BoardStructureHooks.Awake<StructureConsoleComputer>(this);
		((Computer)this).CheckStatus();
		SafeEnableScreen();
		FieldInfo field = typeof(Computer).GetField("_viewableBounds", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (field != null && field.FieldType == typeof(Bounds))
		{
			field.SetValue(this, (object)new Bounds(((Thing)this).Transform.position, new Vector3(20f, 20f, 20f)));
		}
	}

	private void SafeEnableScreen()
	{
		bool showComputerScreen = ((Computer)this).ShowComputerScreen;
		if ((Object)(object)((Computer)this).Screen != (Object)null)
		{
			((Computer)this).Screen.SetActive(showComputerScreen);
		}
		if ((Object)(object)base.ComputerScreen != (Object)null)
		{
			base.ComputerScreen.SetActive(showComputerScreen);
		}
	}

	List<ILogicable> IComputer.DeviceList()
	{
		HashSet<long> hashSet = new HashSet<long>();
		List<ILogicable> list = new List<ILogicable>();
		foreach (CableNetwork connectedCableNetwork in ((Device)this).ConnectedCableNetworks)
		{
			foreach (Device dataDevice in connectedCableNetwork.DataDeviceList)
			{
				if (!hashSet.Contains(((Thing)dataDevice).ReferenceId))
				{
					hashSet.Add(((Thing)dataDevice).ReferenceId);
					list.Add((ILogicable)(object)dataDevice);
				}
			}
		}
		return list;
	}

	public void OnBoardPowerChanged()
	{
		((Computer)this).CheckStatus();
		SafeEnableScreen();
	}

	public override ThingSaveData SerializeSave()
	{
		ThingSaveData val;
		ThingSaveData result = (val = (ThingSaveData)(object)new ConsoleDeviceSaveData());
		((Thing)this).InitialiseSaveData(ref val);
		return result;
	}

	public override void DeserializeSave(ThingSaveData baseData)
	{
		((Structure)this).DeserializeSave(baseData);
		if (baseData is ConsoleDeviceSaveData consoleDeviceSaveData)
		{
			BoardStructureHooks.DeserializeSave((IPlacementBoardStructure)(object)this, consoleDeviceSaveData.Board);
		}
	}

	public override void DeserializeOnJoin(RocketBinaryReader reader)
	{
		((Structure)this).DeserializeOnJoin(reader);
		BoardStructureHooks.DeserializeOnJoin(reader, (IPlacementBoardStructure)(object)this);
	}

	protected override void InitialiseSaveData(ref ThingSaveData baseData)
	{
		((Structure)this).InitialiseSaveData(ref baseData);
		if (baseData is ConsoleDeviceSaveData consoleDeviceSaveData)
		{
			consoleDeviceSaveData.Board = BoardStructureHooks.SerializeSave((IPlacementBoardStructure)(object)this);
		}
	}

	public override void SerializeOnJoin(RocketBinaryWriter writer)
	{
		((Structure)this).SerializeOnJoin(writer);
		BoardStructureHooks.SerializeOnJoin(writer, (IPlacementBoardStructure)(object)this);
	}

	public override CanConstructInfo CanConstruct()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return BoardStructureHooks.CanConstruct<StructureConsoleComputer>(this);
	}

	public override void OnDeregistered()
	{
		((Device)this).OnDeregistered();
		BoardStructureHooks.OnDeregistered<StructureConsoleComputer>(this);
	}

	public override DelayedActionInstance AttackWith(Attack attack, bool doAction = true)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		DynamicThing sourceItem = ((Attack)(ref attack)).SourceItem;
		Screwdriver val = (Screwdriver)(object)((sourceItem is Screwdriver) ? sourceItem : null);
		if (val != null)
		{
			return BoardRelocateHooks.StructureAttackWith((IPlacementBoardRelocatable)(object)this, attack, doAction, BoardRelocateHooks.NormalToolRelocateContinue((DynamicThing)(object)val));
		}
		return ((Device)this).AttackWith(attack, doAction);
	}

	public void OnStructureRelocated()
	{
	}

	public override void OnInteractableUpdated(Interactable interactable)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		((Computer)this).OnInteractableUpdated(interactable);
		if ((int)interactable.Action == 45)
		{
			((Thing)this).SetCustomColor(((Thing)this).ColorState, false);
		}
	}

	Structure IPlacementBoardRelocatable.get_AsStructure()
	{
		return ((Thing)this).AsStructure;
	}

	string IPlacementBoardStructure.get_name()
	{
		return ((Object)this).name;
	}

	void IPlacementBoardStructure.SetStructureData(Quaternion localRotation, ulong ownerClientId, Grid3 localGrid, int customColourIndex)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		((Structure)this).SetStructureData(localRotation, ownerClientId, localGrid, customColourIndex);
	}
}
public class StructureConsoleConsole : Console, IPlacementBoardRelocatable, IPlacementBoardStructure, IReferencable, IEvaluable, IPatchOnLoad, IBoardPoweredListener
{
	public PlacementBoard Board { get; set; }

	public BoardCell[] BoardCells { get; set; }

	public override bool Powered => (Board as IPoweredBoard)?.Powered ?? false;

	public void PatchOnLoad()
	{
		((Thing)(object)this).ApplyBlueprintMaterials();
		((Structure)(object)this).SetExitTool(PrefabPatch.DeviceExitTool);
		((Structure)(object)this).SetEntryTool("ItemGlassSheets", 1);
		((Structure)(object)this).SetExitTool(StationeersTool.CROWBAR, 1);
	}

	public override void Awake()
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		((Computer)this).Awake();
		BoardStructureHooks.Awake<StructureConsoleConsole>(this);
		if (((Thing)this).HasColorState)
		{
			((Thing)this).ColorState = ((Thing)this).InteractColor.State;
		}
		FieldInfo field = typeof(Computer).GetField("_viewableBounds", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (field != null && field.FieldType == typeof(Bounds))
		{
			field.SetValue(this, (object)new Bounds(((Thing)this).Transform.position, new Vector3(20f, 20f, 20f)));
		}
	}

	public void OnBoardPowerChanged()
	{
		((Computer)this).CheckStatus();
		((Structure)this).UpdateStateVisualizer(false);
	}

	public override ThingSaveData SerializeSave()
	{
		ThingSaveData val;
		ThingSaveData result = (val = (ThingSaveData)(object)new ConsoleDeviceSaveData());
		((Thing)this).InitialiseSaveData(ref val);
		return result;
	}

	protected override void InitialiseSaveData(ref ThingSaveData baseData)
	{
		((Structure)this).InitialiseSaveData(ref baseData);
		if (baseData is ConsoleDeviceSaveData consoleDeviceSaveData)
		{
			consoleDeviceSaveData.Board = BoardStructureHooks.SerializeSave((IPlacementBoardStructure)(object)this);
		}
	}

	public override void DeserializeSave(ThingSaveData baseData)
	{
		((Structure)this).DeserializeSave(baseData);
		if (baseData is ConsoleDeviceSaveData consoleDeviceSaveData)
		{
			BoardStructureHooks.DeserializeSave((IPlacementBoardStructure)(object)this, consoleDeviceSaveData.Board);
		}
	}

	public override void SerializeOnJoin(RocketBinaryWriter writer)
	{
		((Structure)this).SerializeOnJoin(writer);
		BoardStructureHooks.SerializeOnJoin(writer, (IPlacementBoardStructure)(object)this);
	}

	public override void DeserializeOnJoin(RocketBinaryReader reader)
	{
		((Structure)this).DeserializeOnJoin(reader);
		BoardStructureHooks.DeserializeOnJoin(reader, (IPlacementBoardStructure)(object)this);
	}

	public override CanConstructInfo CanConstruct()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return BoardStructureHooks.CanConstruct<StructureConsoleConsole>(this);
	}

	public override void OnDeregistered()
	{
		((Device)this).OnDeregistered();
		BoardStructureHooks.OnDeregistered<StructureConsoleConsole>(this);
	}

	public override DelayedActionInstance AttackWith(Attack attack, bool doAction = true)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		DynamicThing sourceItem = ((Attack)(ref attack)).SourceItem;
		Screwdriver val = (Screwdriver)(object)((sourceItem is Screwdriver) ? sourceItem : null);
		if (val != null)
		{
			return BoardRelocateHooks.StructureAttackWith((IPlacementBoardRelocatable)(object)this, attack, doAction, BoardRelocateHooks.NormalToolRelocateContinue((DynamicThing)(object)val));
		}
		return ((Device)this).AttackWith(attack, doAction);
	}

	public void OnStructureRelocated()
	{
	}

	public override void OnInteractableUpdated(Interactable interactable)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Invalid comparison between Unknown and I4
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Invalid comparison between Unknown and I4
		((Computer)this).OnInteractableUpdated(interactable);
		if ((int)interactable.Action == 41 && (Object)(object)((Computer)this).CurrentMotherboard != (Object)null)
		{
			((Computer)this).CurrentMotherboard.SetMode(interactable.State == 0);
		}
		if ((int)interactable.Action == 45)
		{
			((Thing)this).SetCustomColor(((Thing)this).ColorState, false);
		}
	}

	public List<Device> GetDataDeviceList()
	{
		HashSet<long> hashSet = new HashSet<long>();
		List<Device> list = new List<Device>();
		foreach (CableNetwork connectedCableNetwork in ((Device)this).ConnectedCableNetworks)
		{
			foreach (Device dataDevice in connectedCableNetwork.DataDeviceList)
			{
				if (hashSet.Add(((Thing)dataDevice).ReferenceId))
				{
					list.Add(dataDevice);
				}
			}
		}
		return list;
	}

	public bool IsNetworkDevice(Device device)
	{
		foreach (CableNetwork connectedCableNetwork in ((Device)this).ConnectedCableNetworks)
		{
			if (connectedCableNetwork.IsNetworkDevice(device))
			{
				return true;
			}
		}
		return false;
	}

	public override void UpdateStateVisualizer(bool visualOnly = false)
	{
		((Console)this).UpdateStateVisualizer(visualOnly);
		if (((Thing)this).Interactables != null && ((Thing)this).Interactables.Count > 0 && Object.op_Implicit((Object)(object)((Thing)this).Interactables[0].Collider))
		{
			((Thing)this).Interactables[0].Collider.enabled = ((Structure)this).CurrentBuildStateIndex == 0;
		}
		((Computer)this).Screen.SetActive(((Computer)this).ShowComputerScreen);
	}

	Structure IPlacementBoardRelocatable.get_AsStructure()
	{
		return ((Thing)this).AsStructure;
	}

	string IPlacementBoardStructure.get_name()
	{
		return ((Object)this).name;
	}

	void IPlacementBoardStructure.SetStructureData(Quaternion localRotation, ulong ownerClientId, Grid3 localGrid, int customColourIndex)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		((Structure)this).SetStructureData(localRotation, ownerClientId, localGrid, customColourIndex);
	}
}
public class StructureConsoleDiagram : Structure, IPlacementBoardRelocatable, IPlacementBoardStructure, IReferencable, IEvaluable, IPatchOnLoad
{
	public PlacementBoard Board { get; set; }

	public BoardCell[] BoardCells { get; set; }

	public void PatchOnLoad()
	{
		((Thing)(object)this).ApplyPaintableParent((ColorType)6);
		((Thing)(object)this).ApplyBlueprintMaterials();
		((Structure)(object)this).SetExitTool(PrefabPatch.DeviceExitTool);
	}

	public override void Awake()
	{
		((Structure)this).Awake();
		BoardStructureHooks.Awake<StructureConsoleDiagram>(this);
	}

	public override void DeserializeSave(ThingSaveData baseData)
	{
		((Structure)this).DeserializeSave(baseData);
		if (baseData is ConsoleLogicSaveData consoleLogicSaveData)
		{
			BoardStructureHooks.DeserializeSave((IPlacementBoardStructure)(object)this, consoleLogicSaveData.Board);
		}
	}

	public override void DeserializeOnJoin(RocketBinaryReader reader)
	{
		((Structure)this).DeserializeOnJoin(reader);
		BoardStructureHooks.DeserializeOnJoin(reader, (IPlacementBoardStructure)(object)this);
	}

	public override ThingSaveData SerializeSave()
	{
		ThingSaveData val;
		ThingSaveData obj = (val = (ThingSaveData)(object)new ConsoleLogicSaveData());
		((Structure)this).InitialiseSaveData(ref val);
		((ConsoleLogicSaveData)(object)obj).Board = BoardStructureHooks.SerializeSave((IPlacementBoardStructure)(object)this);
		return obj;
	}

	protected override void InitialiseSaveData(ref ThingSaveData baseData)
	{
		((Structure)this).InitialiseSaveData(ref baseData);
		if (baseData is ConsoleLogicSaveData consoleLogicSaveData)
		{
			consoleLogicSaveData.Board = BoardStructureHooks.SerializeSave((IPlacementBoardStructure)(object)this);
		}
	}

	public override void SerializeOnJoin(RocketBinaryWriter writer)
	{
		((Structure)this).SerializeOnJoin(writer);
		BoardStructureHooks.SerializeOnJoin(writer, (IPlacementBoardStructure)(object)this);
	}

	public override CanConstructInfo CanConstruct()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return BoardStructureHooks.CanConstruct<StructureConsoleDiagram>(this);
	}

	public override void OnDeregistered()
	{
		((Structure)this).OnDeregistered();
		BoardStructureHooks.OnDeregistered<StructureConsoleDiagram>(this);
	}

	public override DelayedActionInstance AttackWith(Attack attack, bool doAction = true)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		DynamicThing sourceItem = ((Attack)(ref attack)).SourceItem;
		Screwdriver val = (Screwdriver)(object)((sourceItem is Screwdriver) ? sourceItem : null);
		if (val != null)
		{
			return BoardRelocateHooks.StructureAttackWith((IPlacementBoardRelocatable)(object)this, attack, doAction, BoardRelocateHooks.NormalToolRelocateContinue((DynamicThing)(object)val));
		}
		return ((Structure)this).AttackWith(attack, doAction);
	}

	public void OnStructureRelocated()
	{
	}

	Structure IPlacementBoardRelocatable.get_AsStructure()
	{
		return ((Thing)this).AsStructure;
	}

	string IPlacementBoardStructure.get_name()
	{
		return ((Object)this).name;
	}

	void IPlacementBoardStructure.SetStructureData(Quaternion localRotation, ulong ownerClientId, Grid3 localGrid, int customColourIndex)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		((Structure)this).SetStructureData(localRotation, ownerClientId, localGrid, customColourIndex);
	}
}
public class StructureConsoleDial : LogicDial, IPlacementBoardRelocatable, IPlacementBoardStructure, IReferencable, IEvaluable, IPatchOnLoad
{
	public static int MaxMode = 100000;

	public PlacementBoard Board { get; set; }

	public BoardCell[] BoardCells { get; set; }

	public override bool Powered => (Board as IPoweredBoard)?.Powered ?? false;

	public void PatchOnLoad()
	{
		((Thing)(object)this).ApplyPaintableChild("DialKnob", (ColorType)1, fallback: false);
		((Thing)(object)this).ApplyBlueprintMaterials();
		((Structure)(object)this).SetExitTool(PrefabPatch.DeviceExitTool);
	}

	public override void Awake()
	{
		((LogicDial)this).Awake();
		BoardStructureHooks.Awake<StructureConsoleDial>(this);
	}

	public override CanConstructInfo CanConstruct()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return BoardStructureHooks.CanConstruct<StructureConsoleDial>(this);
	}

	public override void OnDeregistered()
	{
		((Device)this).OnDeregistered();
		BoardStructureHooks.OnDeregistered<StructureConsoleDial>(this);
	}

	public override ThingSaveData SerializeSave()
	{
		ThingSaveData val;
		ThingSaveData result = (val = (ThingSaveData)(object)new ConsoleLogicSaveData());
		((Thing)this).InitialiseSaveData(ref val);
		return result;
	}

	protected override void InitialiseSaveData(ref ThingSaveData baseData)
	{
		((LogicUnitBase)this).InitialiseSaveData(ref baseData);
		if (baseData is ConsoleLogicSaveData consoleLogicSaveData)
		{
			consoleLogicSaveData.Board = BoardStructureHooks.SerializeSave((IPlacementBoardStructure)(object)this);
		}
	}

	public override void DeserializeSave(ThingSaveData baseData)
	{
		((LogicUnitBase)this).DeserializeSave(baseData);
		if (baseData is ConsoleLogicSaveData consoleLogicSaveData)
		{
			BoardStructureHooks.DeserializeSave((IPlacementBoardStructure)(object)this, consoleLogicSaveData.Board);
		}
	}

	public override void SerializeOnJoin(RocketBinaryWriter writer)
	{
		((LogicUnitBase)this).SerializeOnJoin(writer);
		BoardStructureHooks.SerializeOnJoin(writer, (IPlacementBoardStructure)(object)this);
	}

	public override void DeserializeOnJoin(RocketBinaryReader reader)
	{
		((LogicUnitBase)this).DeserializeOnJoin(reader);
		BoardStructureHooks.DeserializeOnJoin(reader, (IPlacementBoardStructure)(object)this);
	}

	public override DelayedActionInstance InteractWith(Interactable interactable, Interaction interaction, bool doAction = true)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Invalid comparison between Unknown and I4
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Invalid comparison between Unknown and I4
		if ((int)interactable.Action == 31 || (int)interactable.Action == 32)
		{
			DelayedActionInstance val = new DelayedActionInstance
			{
				Duration = 0f,
				ActionMessage = interactable.ContextualName
			};
			DynamicThing val2 = ((Interaction)(ref interaction)).SourceSlot.Get();
			Labeller val3 = (Labeller)(object)((val2 is Labeller) ? val2 : null);
			if ((Object)(object)val3 != (Object)null)
			{
				val.ActionMessage = ActionStrings.Set;
				val.AppendStateMessage(GameStrings.DeviceManualInputWindow);
				if (!((Thing)val3).OnOff)
				{
					return val.Fail(GameStrings.DeviceNotOn);
				}
				if (!((Tool)val3).IsOperable)
				{
					return val.Fail(GameStrings.DeviceNoPower);
				}
				if (!doAction)
				{
					return val.Succeed();
				}
				val3.Set((ISetable)(object)this, (LogicType)3);
				return val.Succeed();
			}
			if ((Object)(object)val2 == (Object)null)
			{
				val.AppendStateMessage(GameStrings.GlobalMaxMode, StringManager.Get(((Thing)this).Mode));
				val.AppendStateMessage(GameStrings.HoldForSmallIncrements, Localization.QuantityModifierKey);
				if (!doAction)
				{
					return val.Succeed();
				}
				if (GameManager.RunSimulation)
				{
					int num = ((Thing)this).Mode;
					InteractableType action = interactable.Action;
					if ((int)action != 31)
					{
						if ((int)action == 32)
						{
							num = Mathf.Clamp(((Thing)this).Mode - (((Interaction)(ref interaction)).AltKey ? 1 : 10), 0, MaxMode);
						}
					}
					else
					{
						num = Mathf.Clamp(((Thing)this).Mode + (((Interaction)(ref interaction)).AltKey ? 1 : 10), 0, MaxMode);
					}
					if (num != ((Thing)this).Mode)
					{
						((Thing)this).Mode = num;
						OnServer.Interact(((Thing)this).InteractMode, num, false);
					}
				}
				return val.Succeed();
			}
		}
		return ((LogicDial)this).InteractWith(interactable, interaction, doAction);
	}

	public override DelayedActionInstance AttackWith(Attack attack, bool doAction = true)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		DynamicThing sourceItem = ((Attack)(ref attack)).SourceItem;
		Screwdriver val = (Screwdriver)(object)((sourceItem is Screwdriver) ? sourceItem : null);
		if (val != null)
		{
			return BoardRelocateHooks.StructureAttackWith((IPlacementBoardRelocatable)(object)this, attack, doAction, BoardRelocateHooks.NormalToolRelocateContinue((DynamicThing)(object)val));
		}
		return ((Device)this).AttackWith(attack, doAction);
	}

	public void OnStructureRelocated()
	{
	}

	Structure IPlacementBoardRelocatable.get_AsStructure()
	{
		return ((Thing)this).AsStructure;
	}

	string IPlacementBoardStructure.get_name()
	{
		return ((Object)this).name;
	}

	void IPlacementBoardStructure.SetStructureData(Quaternion localRotation, ulong ownerClientId, Grid3 localGrid, int customColourIndex)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		((Structure)this).SetStructureData(localRotation, ownerClientId, localGrid, customColourIndex);
	}
}
public class StructureConsoleDiode : Diode, IPlacementBoardRelocatable, IPlacementBoardStructure, IReferencable, IEvaluable, IPatchOnLoad, IBoardPoweredListener
{
	[Header("Colorable/emissive part")]
	public MeshRenderer emissiveRenderer;

	public PlacementBoard Board { get; set; }

	public BoardCell[] BoardCells { get; set; }

	public override bool Powered => (Board as IPoweredBoard)?.Powered ?? false;

	public void PatchOnLoad()
	{
		((Thing)(object)this).ApplyPaintableChild("Bulb", (ColorType)6, fallback: false);
		((Thing)(object)this).ApplyBlueprintMaterials();
		((Structure)(object)this).SetExitTool(PrefabPatch.DeviceExitTool);
	}

	public override void Awake()
	{
		((WallLight)this).Awake();
		((Structure)(object)this).InitializeEmissive(emissiveRenderer);
		BoardStructureHooks.Awake<StructureConsoleDiode>(this);
	}

	public override void OnInteractableUpdated(Interactable interactable)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Invalid comparison between Unknown and I4
		if ((int)interactable.Action == 42 || (int)interactable.Action == 36)
		{
			ToggleLightsOn(((Thing)this).OnOff && ((Thing)this).Powered);
			return;
		}
		((Diode)this).OnInteractableUpdated(interactable);
		if ((int)interactable.Action == 45)
		{
			((Thing)this).SetCustomColor(interactable.State, ((Thing)this).Powered);
		}
	}

	private void ToggleLightsOn(bool on)
	{
		object obj = typeof(WallLight).GetField("light", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(this);
		if ((Object)((obj is Light) ? obj : null) != (Object)null)
		{
			((WallLight)this).ToggleLightsOn(on);
		}
		((Structure)(object)this).SyncMaterials(emissiveRenderer);
	}

	public override void SetCustomColor(int index, bool emissive = false)
	{
		((Diode)this).SetCustomColor(index, emissive);
		((Structure)(object)this).SetEmissive(emissiveRenderer, emissive);
	}

	public void OnBoardPowerChanged()
	{
		((Structure)(object)this).SyncMaterials(emissiveRenderer);
		if (Object.op_Implicit((Object)(object)((Thing)this).BaseAnimator))
		{
			((Thing)this).PoweredValue = (((Thing)this).Powered ? 1 : 0);
		}
	}

	public override CanConstructInfo CanConstruct()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return BoardStructureHooks.CanConstruct<StructureConsoleDiode>(this);
	}

	public override void OnDeregistered()
	{
		((Device)this).OnDeregistered();
		BoardStructureHooks.OnDeregistered<StructureConsoleDiode>(this);
	}

	public override ThingSaveData SerializeSave()
	{
		ThingSaveData val;
		ThingSaveData result = (val = (ThingSaveData)(object)new ConsoleLogicSaveData());
		((Thing)this).InitialiseSaveData(ref val);
		return result;
	}

	protected override void InitialiseSaveData(ref ThingSaveData baseData)
	{
		((Structure)this).InitialiseSaveData(ref baseData);
		if (baseData is ConsoleLogicSaveData consoleLogicSaveData)
		{
			consoleLogicSaveData.Board = BoardStructureHooks.SerializeSave((IPlacementBoardStructure)(object)this);
		}
	}

	public override void DeserializeSave(ThingSaveData baseData)
	{
		((Structure)this).DeserializeSave(baseData);
		if (baseData is ConsoleLogicSaveData consoleLogicSaveData)
		{
			BoardStructureHooks.DeserializeSave((IPlacementBoardStructure)(object)this, consoleLogicSaveData.Board);
		}
	}

	public override void SerializeOnJoin(RocketBinaryWriter writer)
	{
		((Structure)this).SerializeOnJoin(writer);
		BoardStructureHooks.SerializeOnJoin(writer, (IPlacementBoardStructure)(object)this);
	}

	public override void DeserializeOnJoin(RocketBinaryReader reader)
	{
		((Structure)this).DeserializeOnJoin(reader);
		BoardStructureHooks.DeserializeOnJoin(reader, (IPlacementBoardStructure)(object)this);
	}

	public override DelayedActionInstance AttackWith(Attack attack, bool doAction = true)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		DynamicThing sourceItem = ((Attack)(ref attack)).SourceItem;
		Screwdriver val = (Screwdriver)(object)((sourceItem is Screwdriver) ? sourceItem : null);
		if (val != null)
		{
			return BoardRelocateHooks.StructureAttackWith((IPlacementBoardRelocatable)(object)this, attack, doAction, BoardRelocateHooks.NormalToolRelocateContinue((DynamicThing)(object)val));
		}
		return ((Device)this).AttackWith(attack, doAction);
	}

	public void OnStructureRelocated()
	{
	}

	Structure IPlacementBoardRelocatable.get_AsStructure()
	{
		return ((Thing)this).AsStructure;
	}

	string IPlacementBoardStructure.get_name()
	{
		return ((Object)this).name;
	}

	void IPlacementBoardStructure.SetStructureData(Quaternion localRotation, ulong ownerClientId, Grid3 localGrid, int customColourIndex)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		((Structure)this).SetStructureData(localRotation, ownerClientId, localGrid, customColourIndex);
	}
}
public class StructureConsoleDiodeSlider : Diode, IPlacementBoardRelocatable, IPlacementBoardStructure, IReferencable, IEvaluable, IPatchOnLoad, IBoardPoweredListener
{
	[SerializeField]
	private Transform bulb1Transform;

	private double _setting;

	[Header("Colorable/emissive part")]
	public MeshRenderer emissiveRenderer;

	public double Setting
	{
		get
		{
			return _setting;
		}
		set
		{
			if (!RocketMath.Approximately(_setting, value, 1E-07))
			{
				_setting = value;
				if (NetworkManager.IsServer && NetworkServer.HasClients())
				{
					((Thing)this).NetworkUpdateFlags = (ushort)(((Thing)this).NetworkUpdateFlags | 0x100);
				}
				if (ThreadedManager.IsThread)
				{
					UnityMainThreadDispatcher.Instance().Enqueue((Action)UpdateBulbTransform);
				}
				else
				{
					UpdateBulbTransform();
				}
			}
		}
	}

	public PlacementBoard Board { get; set; }

	public BoardCell[] BoardCells { get; set; }

	public override bool Powered => (Board as IPoweredBoard)?.Powered ?? false;

	private void UpdateBulbTransform()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Clamp01((float)Setting);
		if (!float.IsNaN(num))
		{
			bulb1Transform.localScale = new Vector3(num, 1f, 1f);
		}
	}

	public void PatchOnLoad()
	{
		((Thing)(object)this).ApplyPaintableChild("Bulb", (ColorType)2, fallback: false);
		((Thing)(object)this).ApplyBlueprintMaterials();
		((Structure)(object)this).SetExitTool(PrefabPatch.DeviceExitTool);
	}

	public override void Awake()
	{
		((WallLight)this).Awake();
		((Structure)(object)this).InitializeEmissive(emissiveRenderer);
		UpdateBulbTransform();
		BoardStructureHooks.Awake<StructureConsoleDiodeSlider>(this);
	}

	protected override void ToggleLightsOn(bool on)
	{
		if ((Object)(object)bulb1Transform != (Object)null)
		{
			((Component)bulb1Transform).gameObject.SetActive(on);
		}
	}

	public override void OnInteractableUpdated(Interactable interactable)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Invalid comparison between Unknown and I4
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Invalid comparison between Unknown and I4
		((Structure)(object)this).SyncMaterials(emissiveRenderer);
		if ((int)interactable.Action == 42 || (int)interactable.Action == 36)
		{
			((WallLight)this).ToggleLightsOn(((Thing)this).OnOff && ((Thing)this).Powered);
		}
		else
		{
			((Diode)this).OnInteractableUpdated(interactable);
		}
	}

	public override void SetCustomColor(int index, bool emissive = false)
	{
		((Diode)this).SetCustomColor(index, emissive);
		((Structure)(object)this).SetEmissive(emissiveRenderer, emissive);
	}

	public override void SetCustomColor(bool emissive)
	{
		((Thing)this).SetCustomColor(emissive);
		((Structure)(object)this).SetEmissive(emissiveRenderer, emissive);
	}

	public void OnBoardPowerChanged()
	{
		((WallLight)this).ToggleLightsOn(((Thing)this).OnOff && ((Thing)this).Powered);
		((Structure)(object)this).SyncMaterials(emissiveRenderer);
	}

	public override CanConstructInfo CanConstruct()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return BoardStructureHooks.CanConstruct<StructureConsoleDiodeSlider>(this);
	}

	public override void OnDeregistered()
	{
		((Device)this).OnDeregistered();
		BoardStructureHooks.OnDeregistered<StructureConsoleDiodeSlider>(this);
	}

	public override ThingSaveData SerializeSave()
	{
		ThingSaveData val;
		ThingSaveData result = (val = (ThingSaveData)(object)new ConsoleLogicSaveData());
		((Thing)this).InitialiseSaveData(ref val);
		return result;
	}

	protected override void InitialiseSaveData(ref ThingSaveData baseData)
	{
		((Structure)this).InitialiseSaveData(ref baseData);
		if (baseData is ConsoleLogicSaveData consoleLogicSaveData)
		{
			consoleLogicSaveData.Board = BoardStructureHooks.SerializeSave((IPlacementBoardStructure)(object)this);
			((LogicBaseSaveData)consoleLogicSaveData).Setting = Setting;
		}
	}

	public override void DeserializeSave(ThingSaveData baseData)
	{
		((Structure)this).DeserializeSave(baseData);
		if (baseData is ConsoleLogicSaveData consoleLogicSaveData)
		{
			BoardStructureHooks.DeserializeSave((IPlacementBoardStructure)(object)this, consoleLogicSaveData.Board);
			Setting = ((LogicBaseSaveData)consoleLogicSaveData).Setting;
		}
	}

	public override void SerializeOnJoin(RocketBinaryWriter writer)
	{
		((Structure)this).SerializeOnJoin(writer);
		BoardStructureHooks.SerializeOnJoin(writer, (IPlacementBoardStructure)(object)this);
	}

	public override void DeserializeOnJoin(RocketBinaryReader reader)
	{
		((Structure)this).DeserializeOnJoin(reader);
		BoardStructureHooks.DeserializeOnJoin(reader, (IPlacementBoardStructure)(object)this);
	}

	public override void BuildUpdate(RocketBinaryWriter writer, ushort networkUpdateType)
	{
		((Structure)this).BuildUpdate(writer, networkUpdateType);
		if (Thing.IsNetworkUpdateRequired(256u, networkUpdateType))
		{
			writer.WriteDouble(Setting);
		}
	}

	public override void ProcessUpdate(RocketBinaryReader reader, ushort networkUpdateType)
	{
		((Structure)this).ProcessUpdate(reader, networkUpdateType);
		if (Thing.IsNetworkUpdateRequired(256u, networkUpdateType))
		{
			Setting = reader.ReadDouble();
		}
	}

	public override bool CanLogicRead(LogicType logicType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if ((int)logicType != 12)
		{
			return ((Device)this).CanLogicRead(logicType);
		}
		return true;
	}

	public override bool CanLogicWrite(LogicType logicType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		if ((int)logicType == 12)
		{
			return true;
		}
		return ((Device)this).CanLogicWrite(logicType);
	}

	public override double GetLogicValue(LogicType logicType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		if ((int)logicType == 12)
		{
			return Setting;
		}
		return ((Device)this).GetLogicValue(logicType);
	}

	public override void SetLogicValue(LogicType logicType, double value)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		((Device)this).SetLogicValue(logicType, value);
		if ((int)logicType == 12)
		{
			Setting = value;
		}
	}

	public override DelayedActionInstance AttackWith(Attack attack, bool doAction = true)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		DynamicThing sourceItem = ((Attack)(ref attack)).SourceItem;
		Screwdriver val = (Screwdriver)(object)((sourceItem is Screwdriver) ? sourceItem : null);
		if (val != null)
		{
			return BoardRelocateHooks.StructureAttackWith((IPlacementBoardRelocatable)(object)this, attack, doAction, BoardRelocateHooks.NormalToolRelocateContinue((DynamicThing)(object)val));
		}
		return ((Device)this).AttackWith(attack, doAction);
	}

	public void OnStructureRelocated()
	{
	}

	Structure IPlacementBoardRelocatable.get_AsStructure()
	{
		return ((Thing)this).AsStructure;
	}

	string IPlacementBoardStructure.get_name()
	{
		return ((Object)this).name;
	}

	void IPlacementBoardStructure.SetStructureData(Quaternion localRotation, ulong ownerClientId, Grid3 localGrid, int customColourIndex)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		((Structure)this).SetStructureData(localRotation, ownerClientId, localGrid, customColourIndex);
	}
}
public class StructureConsoleDisplay : LogicDisplay, IPlacementBoardRelocatable, IPlacementBoardStructure, IReferencable, IEvaluable, IPatchOnLoad, IBoardPoweredListener
{
	public BoardCell[] BoardCells { get; set; }

	public PlacementBoard Board { get; set; }

	public override bool Powered => (Board as IPoweredBoard)?.Powered ?? false;

	public void PatchOnLoad()
	{
		((Thing)(object)this).ApplyPaintableParent((ColorType)2);
		((Thing)(object)this).ApplyBlueprintMaterials();
		((Structure)(object)this).SetExitTool(PrefabPatch.DeviceExitTool);
		base.DigitOn = StationeersModsUtility.GetMaterial((StationeersColor)2, (ShaderType)0);
	}

	public override void Awake()
	{
		((LogicDisplay)this).Awake();
		BoardStructureHooks.Awake<StructureConsoleDisplay>(this);
	}

	public override void OnDeregistered()
	{
		((Device)this).OnDeregistered();
		BoardStructureHooks.OnDeregistered<StructureConsoleDisplay>(this);
	}

	public override CanConstructInfo CanConstruct()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return BoardStructureHooks.CanConstruct<StructureConsoleDisplay>(this);
	}

	public void OnBoardPowerChanged()
	{
		((Thing)this).OnOff = ((Thing)this).Powered;
		if (!GameManager.RunSimulation)
		{
			((LogicUnitBase)this).OnSettingChanged();
		}
	}

	public override ThingSaveData SerializeSave()
	{
		ThingSaveData val;
		ThingSaveData obj = (val = (ThingSaveData)(object)new ConsoleLogicSaveData());
		((LogicUnitBase)this).InitialiseSaveData(ref val);
		((ConsoleLogicSaveData)(object)obj).Board = BoardStructureHooks.SerializeSave((IPlacementBoardStructure)(object)this);
		return obj;
	}

	protected override void InitialiseSaveData(ref ThingSaveData baseData)
	{
		((LogicUnitBase)this).InitialiseSaveData(ref baseData);
		if (baseData is ConsoleLogicSaveData consoleLogicSaveData)
		{
			consoleLogicSaveData.Board = BoardStructureHooks.SerializeSave((IPlacementBoardStructure)(object)this);
		}
	}

	public override void DeserializeSave(ThingSaveData baseData)
	{
		((LogicDisplay)this).DeserializeSave(baseData);
		if (baseData is ConsoleLogicSaveData consoleLogicSaveData)
		{
			BoardStructureHooks.DeserializeSave((IPlacementBoardStructure)(object)this, consoleLogicSaveData.Board);
		}
	}

	public override void SerializeOnJoin(RocketBinaryWriter writer)
	{
		((LogicUnitBase)this).SerializeOnJoin(writer);
		BoardStructureHooks.SerializeOnJoin(writer, (IPlacementBoardStructure)(object)this);
	}

	public override void DeserializeOnJoin(RocketBinaryReader reader)
	{
		((LogicUnitBase)this).DeserializeOnJoin(reader);
		BoardStructureHooks.DeserializeOnJoin(reader, (IPlacementBoardStructure)(object)this);
	}

	public override DelayedActionInstance AttackWith(Attack attack, bool doAction = true)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		DynamicThing sourceItem = ((Attack)(ref attack)).SourceItem;
		Screwdriver val = (Screwdriver)(object)((sourceItem is Screwdriver) ? sourceItem : null);
		if (val != null)
		{
			return BoardRelocateHooks.StructureAttackWith((IPlacementBoardRelocatable)(object)this, attack, doAction, BoardRelocateHooks.NormalToolRelocateContinue((DynamicThing)(object)val));
		}
		return ((Device)this).AttackWith(attack, doAction);
	}

	public void OnStructureRelocated()
	{
	}

	Structure IPlacementBoardRelocatable.get_AsStructure()
	{
		return ((Thing)this).AsStructure;
	}

	string IPlacementBoardStructure.get_name()
	{
		return ((Object)this).name;
	}

	void IPlacementBoardStructure.SetStructureData(Quaternion localRotation, ulong ownerClientId, Grid3 localGrid, int customColourIndex)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		((Structure)this).SetStructureData(localRotation, ownerClientId, localGrid, customColourIndex);
	}
}
public abstract class StructureConsoleDraggable : LogicUnitBase, IPlacementBoardRelocatable, IPlacementBoardStructure, IReferencable, IEvaluable
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct <DragAsync>d__35 : IAsyncStateMachine
	{
		public int <>1__state;

		public AsyncUniTaskMethodBuilder <>t__builder;

		public StructureConsoleDraggable <>4__this;

		private double <lastSetting>5__2;

		private Stopwatch <updateStopwatch>5__3;

		private Awaiter <>u__1;

		private void MoveNext()
		{
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Invalid comparison between Unknown and I4
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0134: Unknown result type (might be due to invalid IL or missing references)
			//IL_0139: Unknown result type (might be due to invalid IL or missing references)
			//IL_014c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0151: Unknown result type (might be due to invalid IL or missing references)
			//IL_0154: Unknown result type (might be due to invalid IL or missing references)
			int num = <>1__state;
			StructureConsoleDraggable structureConsoleDraggable = <>4__this;
			try
			{
				if (num != 0)
				{
					<lastSetting>5__2 = ((LogicUnitBase)structureConsoleDraggable).Setting;
					<updateStopwatch>5__3 = new Stopwatch();
					goto IL_01d0;
				}
				Awaiter val = <>u__1;
				<>u__1 = default(Awaiter);
				num = (<>1__state = -1);
				goto IL_0084;
				IL_0152:
				Ray ray;
				<lastSetting>5__2 = Math.Clamp(structureConsoleDraggable.SettingFromRay(ray), 0.0, 1.0);
				structureConsoleDraggable._displaySetting = <lastSetting>5__2;
				InventoryManager.Instance.UIProgressionBar.SetActionName(StateString(<lastSetting>5__2));
				if (!<updateStopwatch>5__3.IsRunning || <updateStopwatch>5__3.ElapsedMilliseconds > 100)
				{
					((LogicUnitBase)structureConsoleDraggable).Setting = <lastSetting>5__2;
					<updateStopwatch>5__3.Restart();
				}
				goto IL_01d0;
				IL_0084:
				((Awaiter)(ref val)).GetResult();
				if ((int)GameManager.GameState != 3)
				{
					goto IL_01d0;
				}
				if (KeyManager.GetMouse("Primary"))
				{
					if (Cursor.visible)
					{
						if (InputMouseWorldInteractable == ((Thing)structureConsoleDraggable).InteractActivate)
						{
							ray = CameraController.CurrentCamera.ScreenPointToRay(Input.mousePosition);
							RaycastHit val2 = default(RaycastHit);
							if (Physics.Raycast(CameraController.CurrentCamera.ScreenPointToRay(Input.mousePosition), ref val2, CursorManager.MaxInteractDistance, LayerMask.op_Implicit(CursorManager.Instance.CursorHitMask)) && structureConsoleDraggable.IsDragCollider(((RaycastHit)(ref val2)).collider))
							{
								goto IL_0152;
							}
						}
					}
					else if (((GameBase)InventoryManager.Instance.UIProgressionBar).IsVisible && !((Object)(object)CursorManager.CursorThing != (Object)(object)structureConsoleDraggable))
					{
						RaycastHit cursorHit = CursorManager.CursorHit;
						if (structureConsoleDraggable.IsDragCollider(((RaycastHit)(ref cursorHit)).collider))
						{
							ray = InputHelpers.GetCameraRay();
							goto IL_0152;
						}
					}
				}
				goto IL_01db;
				IL_01d0:
				if (Object.op_Implicit((Object)(object)structureConsoleDraggable))
				{
					UniTask val3 = UniTask.NextFrame();
					val = ((UniTask)(ref val3)).GetAwaiter();
					if (!((Awaiter)(ref val)).IsCompleted)
					{
						num = (<>1__state = 0);
						<>u__1 = val;
						((AsyncUniTaskMethodBuilder)(ref <>t__builder)).AwaitUnsafeOnCompleted<Awaiter, <DragAsync>d__35>(ref val, ref this);
						return;
					}
					goto IL_0084;
				}
				goto IL_01db;
				IL_01db:
				((LogicUnitBase)structureConsoleDraggable).Setting = <lastSetting>5__2;
				structureConsoleDraggable._isDragging = false;
				DragCursor.ReleaseDragCursor();
			}
			catch (Exception exception)
			{
				<>1__state = -2;
				<updateStopwatch>5__3 = null;
				((AsyncUniTaskMethodBuilder)(ref <>t__builder)).SetException(exception);
				return;
			}
			<>1__state = -2;
			<updateStopwatch>5__3 = null;
			((AsyncUniTaskMethodBuilder)(ref <>t__builder)).SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			((AsyncUniTaskMethodBuilder)(ref <>t__builder)).SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	public Collider DragCollider;

	protected bool _isDragging;

	protected double _displaySetting;

	public PlacementBoard Board { get; set; }

	public BoardCell[] BoardCells { get; set; }

	public override bool Powered => (Board as IPoweredBoard)?.Powered ?? false;

	public override double Setting
	{
		get
		{
			return (double)((Thing)this).InteractActivate.State / 2147483647.0;
		}
		set
		{
			int num = (int)(Math.Clamp(value, 0.0, 1.0) * 2147483647.0);
			if (GameManager.RunSimulation)
			{
				OnServer.Interact(((Thing)this).InteractActivate, num, false);
			}
			else
			{
				NetworkClient.Interact(((Thing)this).InteractActivate, num);
			}
		}
	}

	private static Interactable InputMouseWorldInteractable
	{
		get
		{
			object value = typeof(InputMouse).GetField("WorldInteractable", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
			return (Interactable)((value is Interactable) ? value : null);
		}
	}

	public override void Awake()
	{
		((Device)this).Awake();
		BoardStructureHooks.Awake<StructureConsoleDraggable>(this);
	}

	public override CanConstructInfo CanConstruct()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return BoardStructureHooks.CanConstruct<StructureConsoleDraggable>(this);
	}

	public override void OnDeregistered()
	{
		((Device)this).OnDeregistered();
		BoardStructureHooks.OnDeregistered<StructureConsoleDraggable>(this);
	}

	public override ThingSaveData SerializeSave()
	{
		ThingSaveData val;
		ThingSaveData result = (val = (ThingSaveData)(object)new ConsoleLogicSaveData());
		((Thing)this).InitialiseSaveData(ref val);
		return result;
	}

	protected override void InitialiseSaveData(ref ThingSaveData baseData)
	{
		((LogicUnitBase)this).InitialiseSaveData(ref baseData);
		if (baseData is ConsoleLogicSaveData consoleLogicSaveData)
		{
			consoleLogicSaveData.Board = BoardStructureHooks.SerializeSave((IPlacementBoardStructure)(object)this);
		}
	}

	public override void DeserializeSave(ThingSaveData baseData)
	{
		((LogicUnitBase)this).DeserializeSave(baseData);
		if (baseData is ConsoleLogicSaveData consoleLogicSaveData)
		{
			BoardStructureHooks.DeserializeSave((IPlacementBoardStructure)(object)this, consoleLogicSaveData.Board);
		}
	}

	public override void SerializeOnJoin(RocketBinaryWriter writer)
	{
		((LogicUnitBase)this).SerializeOnJoin(writer);
		BoardStructureHooks.SerializeOnJoin(writer, (IPlacementBoardStructure)(object)this);
	}

	public override void DeserializeOnJoin(RocketBinaryReader reader)
	{
		((LogicUnitBase)this).DeserializeOnJoin(reader);
		BoardStructureHooks.DeserializeOnJoin(reader, (IPlacementBoardStructure)(object)this);
	}

	public override DelayedActionInstance AttackWith(Attack attack, bool doAction = true)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		DynamicThing sourceItem = ((Attack)(ref attack)).SourceItem;
		Screwdriver val = (Screwdriver)(object)((sourceItem is Screwdriver) ? sourceItem : null);
		if (val != null)
		{
			return BoardRelocateHooks.StructureAttackWith((IPlacementBoardRelocatable)(object)this, attack, doAction, BoardRelocateHooks.NormalToolRelocateContinue((DynamicThing)(object)val));
		}
		return ((Device)this).AttackWith(attack, doAction);
	}

	public void OnStructureRelocated()
	{
	}

	public static string StateString(double value)
	{
		Localization.Variable1 = ExtensionMethods.ToStringExact(value);
		return InterfaceStrings.LogicState;
	}

	public override DelayedActionInstance InteractWith(Interactable interactable, Interaction interaction, bool doAction = true)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		if ((int)interactable.Action != 41)
		{
			return ((LogicUnitBase)this).InteractWith(interactable, interaction, doAction);
		}
		DelayedActionInstance val = new DelayedActionInstance
		{
			ActionMessage = ActionStrings.Set,
			Duration = float.MaxValue,
			ExtendedMessage = StateString(_isDragging ? _displaySetting : ((LogicUnitBase)this).Setting)
		};
		if (!_isDragging && KeyManager.GetMouseDown("Primary") && (Object)(object)((Interaction)(ref interaction)).SourceThing == (Object)(object)InventoryManager.Parent)
		{
			StartDrag();
		}
		return val.Succeed();
	}

	protected abstract void UpdateDraggablePosition(double setting);

	public override void UpdateEachFrame()
	{
		if (!_isDragging)
		{
			if (Math.Abs(((LogicUnitBase)this).Setting - _displaySetting) < 0.01)
			{
				_displaySetting = ((LogicUnitBase)this).Setting;
			}
			else
			{
				_displaySetting += (((LogicUnitBase)this).Setting - _displaySetting) * 0.5;
			}
		}
		UpdateDraggablePosition(_displaySetting);
	}

	private void StartDrag()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		DragCursor.TakeDragCursor((Thing)(object)this, ((Thing)this).InteractActivate, ((Thing)this)._interactableColliderLookup);
		_isDragging = true;
		_displaySetting = ((LogicUnitBase)this).Setting;
		UniTaskExtensions.Forget(DragAsync());
	}

	protected abstract double SettingFromRay(Ray ray);

	private bool IsDragCollider(Collider collider)
	{
		if (!((Object)(object)collider == (Object)(object)DragCollider))
		{
			return (Object)(object)collider == (Object)(object)DragCursor.Collider;
		}
		return true;
	}

	[AsyncStateMachine(typeof(<DragAsync>d__35))]
	private UniTask DragAsync()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		<DragAsync>d__35 <DragAsync>d__ = default(<DragAsync>d__35);
		<DragAsync>d__.<>t__builder = AsyncUniTaskMethodBuilder.Create();
		<DragAsync>d__.<>4__this = this;
		<DragAsync>d__.<>1__state = -1;
		((AsyncUniTaskMethodBuilder)(ref <DragAsync>d__.<>t__builder)).Start<<DragAsync>d__35>(ref <DragAsync>d__);
		return ((AsyncUniTaskMethodBuilder)(ref <DragAsync>d__.<>t__builder)).Task;
	}

	protected override int ReadInteractableState(RocketBinaryReader reader, Interactable interactable)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		if ((int)interactable.Action == 41)
		{
			return reader.ReadInt32();
		}
		return ((Thing)this).ReadInteractableState(reader, interactable);
	}

	protected override void WriteInteractableState(RocketBinaryWriter writer, Interactable interactable)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		if ((int)interactable.Action == 41)
		{
			writer.WriteInt32(interactable.State);
		}
		else
		{
			((Thing)this).WriteInteractableState(writer, interactable);
		}
	}

	public override bool CanLogicRead(LogicType logicType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if ((int)logicType != 9)
		{
			if ((int)logicType == 12)
			{
				return true;
			}
			return ((Device)this).CanLogicRead(logicType);
		}
		return false;
	}

	public override double GetLogicValue(LogicType logicType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		if ((int)logicType == 12)
		{
			return ((LogicUnitBase)this).Setting;
		}
		return ((Device)this).GetLogicValue(logicType);
	}

	public override bool CanLogicWrite(LogicType logicType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if ((int)logicType != 9)
		{
			if ((int)logicType == 12)
			{
				return true;
			}
			return ((Device)this).CanLogicWrite(logicType);
		}
		return false;
	}

	public override void SetLogicValue(LogicType logicType, double value)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if ((int)logicType == 12)
		{
			((LogicUnitBase)this).Setting = value;
		}
		else
		{
			((Device)this).SetLogicValue(logicType, value);
		}
	}

	Structure IPlacementBoardRelocatable.get_AsStructure()
	{
		return ((Thing)this).AsStructure;
	}

	string IPlacementBoardStructure.get_name()
	{
		return ((Object)this).name;
	}

	void IPlacementBoardStructure.SetStructureData(Quaternion localRotation, ulong ownerClientId, Grid3 localGrid, int customColourIndex)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		((Structure)this).SetStructureData(localRotation, ownerClientId, localGrid, customColourIndex);
	}
}
public class StructureConsoleGauge : LogicUnitBase, IPlacementBoardRelocatable, IPlacementBoardStructure, IReferencable, IEvaluable, IPatchOnLoad, IBoardPoweredListener
{
	[SerializeField]
	private Dial dial;

	[Header("Main label mesh")]
	public MeshRenderer MeshRenderer;

	public int materialLabelIndex;

	[Header("Colorable/emissive part")]
	public MeshRenderer emissiveRenderer;

	public int textTextureWidth = 512;

	public int textTextureHeight = 128;

	private RenderTexture _textTexture;

	private string RawName => ((Thing)this).CustomName ?? "";

	public PlacementBoard Board { get; set; }

	public BoardCell[] BoardCells { get; set; }

	public override bool Powered => (Board as IPoweredBoard)?.Powered ?? false;

	public void PatchOnLoad()
	{
		((Thing)(object)this).ApplyPaintableChild("Bulb", (ColorType)2, fallback: true);
		((Thing)(object)this).ApplyBlueprintMaterials();
		((Structure)(object)this).SetExitTool(PrefabPatch.DeviceExitTool);
	}

	public override void Awake()
	{
		((Device)this).Awake();
		BoardStructureHooks.Awake<StructureConsoleGauge>(this);
		dial.Init();
		((Structure)(object)this).InitializeEmissive(emissiveRenderer);
		((Structure)(object)this).InitializeText(MeshRenderer, materialLabelIndex, ref _textTexture, textTextureWidth, textTextureHeight);
		UpdateLabelText();
	}

	public override CanConstructInfo CanConstruct()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return BoardStructureHooks.CanConstruct<StructureConsoleGauge>(this);
	}

	public override void OnFinishedThingSync()
	{
		((Thing)this).OnFinishedThingSync();
		UpdateLabelText();
	}

	public override void OnRenamed()
	{
		((Device)this).OnRenamed();
		UpdateLabelText();
	}

	public override void OnInteractableUpdated(Interactable interactable)
	{
		((LogicUnitBase)this).OnInteractableUpdated(interactable);
		((Structure)(object)this).SyncMaterials(emissiveRenderer);
	}

	public override void SetCustomColor(int index, bool emissive = false)
	{
		((Structure)this).SetCustomColor(index, emissive);
		((Structure)(object)this).SetEmissive(emissiveRenderer, emissive);
	}

	public void OnBoardPowerChanged()
	{
		((Structure)(object)this).SyncMaterials(emissiveRenderer);
	}

	public override void OnDeregistered()
	{
		((Device)this).OnDeregistered();
		BoardStructureHooks.OnDeregistered<StructureConsoleGauge>(this);
	}

	public override ThingSaveData SerializeSave()
	{
		ThingSaveData val;
		ThingSaveData result = (val = (ThingSaveData)(object)new ConsoleLogicSaveData());
		((Thing)this).InitialiseSaveData(ref val);
		return result;
	}

	protected override void InitialiseSaveData(ref ThingSaveData baseData)
	{
		((LogicUnitBase)this).InitialiseSaveData(ref baseData);
		if (baseData is ConsoleLogicSaveData consoleLogicSaveData)
		{
			consoleLogicSaveData.Board = BoardStructureHooks.SerializeSave((IPlacementBoardStructure)(object)this);
		}
	}

	public override void DeserializeSave(ThingSaveData baseData)
	{
		((LogicUnitBase)this).DeserializeSave(baseData);
		if (baseData is ConsoleLogicSaveData consoleLogicSaveData)
		{
			BoardStructureHooks.DeserializeSave((IPlacementBoardStructure)(object)this, consoleLogicSaveData.Board);
			UpdateLabelText();
		}
	}

	public override void SerializeOnJoin(RocketBinaryWriter writer)
	{
		((LogicUnitBase)this).SerializeOnJoin(writer);
		BoardStructureHooks.SerializeOnJoin(writer, (IPlacementBoardStructure)(object)this);
	}

	public override void DeserializeOnJoin(RocketBinaryReader reader)
	{
		((LogicUnitBase)this).DeserializeOnJoin(reader);
		BoardStructureHooks.DeserializeOnJoin(reader, (IPlacementBoardStructure)(object)this);
	}

	public override void UpdateEachFrame()
	{
		((Thing)this).UpdateEachFrame();
		if (!((Thing)this).IsOccluded && !double.IsNaN(((LogicUnitBase)this).Setting))
		{
			dial.UpdatePosition((float)((LogicUnitBase)this).Setting, Time.deltaTime);
		}
	}

	public override bool CanLogicRead(LogicType logicType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if ((int)logicType == 12)
		{
			return true;
		}
		return ((Device)this).CanLogicRead(logicType);
	}

	public override bool CanLogicWrite(LogicType logicType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if ((int)logicType == 12)
		{
			return true;
		}
		return ((Device)this).CanLogicWrite(logicType);
	}

	public override double GetLogicValue(LogicType logicType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if ((int)logicType == 12)
		{
			return ((LogicUnitBase)this).Setting;
		}
		return ((Device)this).GetLogicValue(logicType);
	}

	public override void SetLogicValue(LogicType logicType, double value)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if ((int)logicType == 12)
		{
			((LogicUnitBase)this).Setting = value;
		}
		else
		{
			((Device)this).SetLogicValue(logicType, value);
		}
	}

	private void UpdateLabelText()
	{
		string text = RawName;
		if (string.IsNullOrEmpty(text))
		{
			text = ((Thing)this).DisplayName;
		}
		if (text.EndsWith("*"))
		{
			text = "";
		}
		TextQueue.RenderText((Thing)(object)this, _textTexture, text);
	}

	public override DelayedActionInstance AttackWith(Attack attack, bool doAction = true)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		DynamicThing sourceItem = ((Attack)(ref attack)).SourceItem;
		Screwdriver val = (Screwdriver)(object)((sourceItem is Screwdriver) ? sourceItem : null);
		if (val != null)
		{
			return BoardRelocateHooks.StructureAttackWith((IPlacementBoardRelocatable)(object)this, attack, doAction, BoardRelocateHooks.NormalToolRelocateContinue((DynamicThing)(object)val));
		}
		return ((Device)this).AttackWith(attack, doAction);
	}

	public void OnStructureRelocated()
	{
	}

	Structure IPlacementBoardRelocatable.get_AsStructure()
	{
		return ((Thing)this).AsStructure;
	}

	string IPlacementBoardStructure.get_name()
	{
		return ((Object)this).name;
	}

	void IPlacementBoardStructure.SetStructureData(Quaternion localRotation, ulong ownerClientId, Grid3 localGrid, int customColourIndex)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		((Structure)this).SetStructureData(localRotation, ownerClientId, localGrid, customColourIndex);
	}
}
public class StructureConsoleGaugeManual : LogicUnitBase, IPlacementBoardRelocatable, IPlacementBoardStructure, IReferencable, IEvaluable, IPatchOnLoad, IBoardPoweredListener
{
	[Header("Main label mesh")]
	public MeshRenderer MeshRenderer;

	public int materialLabelIndex;

	[Header("Colorable/emissive part")]
	public MeshRenderer emissiveRenderer;

	public int textTextureWidth = 512;

	public int textTextureHeight = 128;

	private RenderTexture _textTexture;

	[Header("Custom Needle Settings")]
	[SerializeField]
	private Transform customNeedle;

	[SerializeField]
	private float angleMin = -90f;

	[SerializeField]
	private float angleMax = 90f;

	[SerializeField]
	private float lerpSpeed = 5f;

	private static string[] _modeStrings;

	public static readonly int DialTurnHash = Animator.StringToHash("DialTurn");

	public static int MaxMode = 100000;

	private float smoothedValue;

	private string RawName => ((Thing)this).CustomName ?? "";

	public PlacementBoard Board { get; set; }

	public BoardCell[] BoardCells { get; set; }

	public override string[] ModeStrings => _modeStrings;

	public override bool Powered => (Board as ConsoleBoard)?.Powered ?? false;

	public void PatchOnLoad()
	{
		((Thing)(object)this).ApplyPaintableChild("Bulb", (ColorType)2, fallback: true);
		((Thing)(object)this).ApplyBlueprintMaterials();
		((Structure)(object)this).SetExitTool(PrefabPatch.DeviceExitTool);
	}

	public override void Awake()
	{
		((Device)this).Awake();
		BoardStructureHooks.Awake<StructureConsoleGaugeManual>(this);
		((Structure)(object)this).InitializeEmissive(emissiveRenderer);
		((Structure)(object)this).InitializeText(MeshRenderer, materialLabelIndex, ref _textTexture, textTextureWidth, textTextureHeight);
		UpdateLabelText();
		smoothedValue = (float)((LogicUnitBase)this).Setting / (float)Mathf.Max(1, ((Thing)this).Mode);
	}

	public override void OnPrefabLoad()
	{
		((Thing)this).OnPrefabLoad();
		if (_modeStrings == null)
		{
			GenerateStrings();
		}
	}

	private void GenerateStrings()
	{
		_modeStrings = new string[MaxMode];
		for (int i = 0; i < MaxMode; i++)
		{
			_modeStrings[i] = i.ToString();
		}
	}

	protected override int ReadInteractableState(RocketBinaryReader reader, Interactable interactable)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		if ((int)interactable.Action == 37)
		{
			return reader.ReadInt16();
		}
		return ((Thing)this).ReadInteractableState(reader, interactable);
	}

	protected override void WriteInteractableState(RocketBinaryWriter writer, Interactable interactable)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		if ((int)interactable.Action == 37)
		{
			writer.WriteInt16((short)interactable.State);
		}
		else
		{
			((Thing)this).WriteInteractableState(writer, interactable);
		}
	}

	public override void UpdateEachFrame()
	{
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)customNeedle == (Object)null) && !double.IsNaN(((LogicUnitBase)this).Setting))
		{
			float num = (float)((LogicUnitBase)this).Setting / Mathf.Max(1f, (float)((Thing)this).Mode);
			smoothedValue = Mathf.Lerp(smoothedValue, num, Time.deltaTime * lerpSpeed);
			if (float.IsNaN(smoothedValue))
			{
				smoothedValue = (float)((LogicUnitBase)this).Setting;
			}
			float num2 = Mathf.Lerp(angleMin, angleMax, smoothedValue);
			customNeedle.localRotation = Quaternion.Euler(0f, 0f, num2);
		}
	}

	public override CanConstructInfo CanConstruct()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return BoardStructureHooks.CanConstruct<StructureConsoleGaugeManual>(this);
	}

	public override void OnFinishedThingSync()
	{
		((Thing)this).OnFinishedThingSync();
		UpdateLabelText();
	}

	public override void OnRenamed()
	{
		((Device)this).OnRenamed();
		UpdateLabelText();
	}

	public override void OnInteractableUpdated(Interactable interactable)
	{
		((LogicUnitBase)this).OnInteractableUpdated(interactable);
		((Structure)(object)this).SyncMaterials(emissiveRenderer);
	}

	public override void SetCustomColor(int index, bool emissive = false)
	{
		((Structure)this).SetCustomColor(index, emissive);
		((Structure)(object)this).SetEmissive(emissiveRenderer, emissive);
	}

	public void OnBoardPowerChanged()
	{
		((Structure)(object)this).SyncMaterials(emissiveRenderer);
	}

	public override void OnDeregistered()
	{
		((Device)this).OnDeregistered();
		BoardStructureHooks.OnDeregistered<StructureConsoleGaugeManual>(this);
	}

	public override ThingSaveData SerializeSave()
	{
		ThingSaveData val;
		ThingSaveData result = (val = (ThingSaveData)(object)new ConsoleLogicSaveData());
		((Thing)this).InitialiseSaveData(ref val);
		return result;
	}

	protected override void InitialiseSaveData(ref ThingSaveData baseData)
	{
		((LogicUnitBase)this).InitialiseSaveData(ref baseData);
		if (baseData is ConsoleLogicSaveData consoleLogicSaveData)
		{
			consoleLogicSaveData.Board = BoardStructureHooks.SerializeSave((IPlacementBoardStructure)(object)this);
		}
	}

	public override void DeserializeSave(ThingSaveData baseData)
	{
		((LogicUnitBase)this).DeserializeSave(baseData);
		if (baseData is ConsoleLogicSaveData consoleLogicSaveData)
		{
			BoardStructureHooks.DeserializeSave((IPlacementBoardStructure)(object)this, consoleLogicSaveData.Board);
			UpdateLabelText();
		}
	}

	public override void SerializeOnJoin(RocketBinaryWriter writer)
	{
		((LogicUnitBase)this).SerializeOnJoin(writer);
		BoardStructureHooks.SerializeOnJoin(writer, (IPlacementBoardStructure)(object)this);
	}

	public override void DeserializeOnJoin(RocketBinaryReader reader)
	{
		((LogicUnitBase)this).DeserializeOnJoin(reader);
		BoardStructureHooks.DeserializeOnJoin(reader, (IPlacementBoardStructure)(object)this);
	}

	public override bool CanLogicRead(LogicType logicType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Invalid comparison between Unknown and I4
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if ((int)logicType != 12 && (int)logicType != 24 && (int)logicType != 23)
		{
			return ((Device)this).CanLogicRead(logicType);
		}
		return true;
	}

	public override double GetLogicValue(LogicType logicType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Invalid comparison between Unknown and I4
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if ((int)logicType != 12)
		{
			if ((int)logicType != 23)
			{
				if ((int)logicType == 24)
				{
					return ((LogicUnitBase)this).Setting / (double)Mathf.Max(1f, (float)((Thing)this).Mode);
				}
				return ((Device)this).GetLogicValue(logicType);
			}
			return ((Thing)this).Mode;
		}
		return ((LogicUnitBase)this).Setting;
	}

	public override bool CanLogicWrite(LogicType logicType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if ((int)logicType != 12)
		{
			return ((Device)this).CanLogicWrite(logicType);
		}
		return true;
	}

	public override void SetLogicValue(LogicType logicType, double value)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		if (((Structure)this).IsStructureCompleted && (int)logicType == 12)
		{
			((LogicUnitBase)this).Setting = Math.Clamp(value, 0.0, (double)((Thing)this).Mode);
			UpdateLabelText();
		}
		else
		{
			((Device)this).SetLogicValue(logicType, value);
		}
	}

	private void UpdateLabelText()
	{
		string text = RawName;
		if (string.IsNullOrEmpty(text))
		{
			text = ((Thing)this).DisplayName;
		}
		if (text.EndsWith("*"))
		{
			text = "";
		}
		TextQueue.RenderText((Thing)(object)this, _textTexture, text);
	}

	public override DelayedActionInstance AttackWith(Attack attack, bool doAction = true)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		DynamicThing sourceItem = ((Attack)(ref attack)).SourceItem;
		Screwdriver val = (Screwdriver)(object)((sourceItem is Screwdriver) ? sourceItem : null);
		if (val != null)
		{
			return BoardRelocateHooks.StructureAttackWith((IPlacementBoardRelocatable)(object)this, attack, doAction, BoardRelocateHooks.NormalToolRelocateContinue((DynamicThing)(object)val));
		}
		return ((Device)this).AttackWith(attack, doAction);
	}

	public override DelayedActionInstance InteractWith(Interactable interactable, Interaction interaction, bool doAction = true)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Invalid comparison between Unknown and I4
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Invalid comparison between Unknown and I4
		if ((int)interactable.Action == 31 || (int)interactable.Action == 32)
		{
			DelayedActionInstance val = new DelayedActionInstance
			{
				Duration = 0f,
				ActionMessage = interactable.ContextualName
			};
			DynamicThing val2 = ((Interaction)(ref interaction)).SourceSlot.Get();
			Labeller val3 = (Labeller)(object)((val2 is Labeller) ? val2 : null);
			if ((Object)(object)val3 != (Object)null)
			{
				val.ActionMessage = ActionStrings.Set;
				val.AppendStateMessage(GameStrings.DeviceManualInputWindow);
				if (!((Thing)val3).OnOff)
				{
					return val.Fail(GameStrings.DeviceNotOn);
				}
				if (!((Tool)val3).IsOperable)
				{
					return val.Fail(GameStrings.DeviceNoPower);
				}
				if (!doAction)
				{
					return val.Succeed();
				}
				val3.Set((ISetable)(object)this, (LogicType)3);
				return val.Succeed();
			}
			if ((Object)(object)val2 == (Object)null)
			{
				val.AppendStateMessage(GameStrings.GlobalMaxMode, StringManager.Get(((Thing)this).Mode));
				val.AppendStateMessage(GameStrings.HoldForSmallIncrements, Localization.QuantityModifierKey);
				if (!doAction)
				{
					return val.Succeed();
				}
				if (GameManager.RunSimulation)
				{
					int num = ((Thing)this).Mode;
					InteractableType action = interactable.Action;
					if ((int)action != 31)
					{
						if ((int)action == 32)
						{
							num = Mathf.Clamp(((Thing)this).Mode - (((Interaction)(ref interaction)).AltKey ? 1 : 10), 0, MaxMode);
						}
					}
					else
					{
						num = Mathf.Clamp(((Thing)this).Mode + (((Interaction)(ref interaction)).AltKey ? 1 : 10), 0, MaxMode);
					}
					if (num != ((Thing)this).Mode)
					{
						((Thing)this).Mode = num;
						OnServer.Interact(((Thing)this).InteractMode, num, false);
					}
				}
				return val.Succeed();
			}
		}
		return ((LogicUnitBase)this).InteractWith(interactable, interaction, doAction);
	}

	public void OnStructureRelocated()
	{
	}

	Structure IPlacementBoardRelocatable.get_AsStructure()
	{
		return ((Thing)this).AsStructure;
	}

	string IPlacementBoardStructure.get_name()
	{
		return ((Object)this).name;
	}

	void IPlacementBoardStructure.SetStructureData(Quaternion localRotation, ulong ownerClientId, Grid3 localGrid, int customColourIndex)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		((Structure)this).SetStructureData(localRotation, ownerClientId, localGrid, customColourIndex);
	}
}
public class StructureConsoleLabel : Structure, IPlacementBoardRelocatable, IPlacementBoardStructure, IReferencable, IEvaluable, IPatchOnLoad
{
	public int textTextureWidth = 512;

	public int textTextureHeight = 128;

	public MeshRenderer MeshRenderer;

	public int materialLabelIndex;

	private RenderTexture _textTexture;

	public PlacementBoard Board { get; set; }

	public BoardCell[] BoardCells { get; set; }

	public void PatchOnLoad()
	{
		((Thing)(object)this).ApplyBlueprintMaterials();
		((Structure)(object)this).SetExitTool(PrefabPatch.DeviceExitTool);
	}

	public override void Awake()
	{
		((Structure)this).Awake();
		BoardStructureHooks.Awake<StructureConsoleLabel>(this);
		((Structure)(object)this).InitializeText(MeshRenderer, materialLabelIndex, ref _textTexture, textTextureWidth, textTextureHeight);
		UpdateLabelText();
	}

	private void UpdateLabelText()
	{
		TextQueue.RenderText((Thing)(object)this, _textTexture, ((Thing)this).CustomName ?? "");
	}

	public override void OnRenamed()
	{
		((Thing)this).OnRenamed();
		UpdateLabelText();
	}

	public override void DeserializeSave(ThingSaveData baseData)
	{
		((Structure)this).DeserializeSave(baseData);
		if (baseData is ConsoleLogicSaveData consoleLogicSaveData)
		{
			BoardStructureHooks.DeserializeSave((IPlacementBoardStructure)(object)this, consoleLogicSaveData.Board);
		}
		UpdateLabelText();
	}

	public override void DeserializeOnJoin(RocketBinaryReader reader)
	{
		((Structure)this).DeserializeOnJoin(reader);
		BoardStructureHooks.DeserializeOnJoin(reader, (IPlacementBoardStructure)(object)this);
		UpdateLabelText();
	}

	public override ThingSaveData SerializeSave()
	{
		ThingSaveData val;
		ThingSaveData obj = (val = (ThingSaveData)(object)new ConsoleLogicSaveData());
		((Structure)this).InitialiseSaveData(ref val);
		((ConsoleLogicSaveData)(object)obj).Board = BoardStructureHooks.SerializeSave((IPlacementBoardStructure)(object)this);
		return obj;
	}

	protected override void InitialiseSaveData(ref ThingSaveData baseData)
	{
		((Structure)this).InitialiseSaveData(ref baseData);
		if (baseData is ConsoleLogicSaveData consoleLogicSaveData)
		{
			consoleLogicSaveData.Board = BoardStructureHooks.SerializeSave((IPlacementBoardStructure)(object)this);
		}
	}

	public override void SerializeOnJoin(RocketBinaryWriter writer)
	{
		((Structure)this).SerializeOnJoin(writer);
		BoardStructureHooks.SerializeOnJoin(writer, (IPlacementBoardStructure)(object)this);
	}

	public override CanConstructInfo CanConstruct()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return BoardStructureHooks.CanConstruct<StructureConsoleLabel>(this);
	}

	public override void OnDeregistered()
	{
		((Structure)this).OnDeregistered();
		BoardStructureHooks.OnDeregistered<StructureConsoleLabel>(this);
	}

	public override DelayedActionInstance AttackWith(Attack attack, bool doAction = true)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		DynamicThing sourceItem = ((Attack)(ref attack)).SourceItem;
		if (!Object.op_Implicit((Object)(object)sourceItem))
		{
			return null;
		}
		Labeller val = (Labeller)(object)((sourceItem is Labeller) ? sourceItem : null);
		if (Object.op_Implicit((Object)(object)val))
		{
			DelayedActionInstance val2 = new DelayedActionInstance
			{
				Duration = 0f,
				ActionMessage = ActionStrings.Rename
			};
			if (!((Thing)val).OnOff)
			{
				return val2.Fail(GameStrings.DeviceNotOn);
			}
			if (!((Tool)val).IsOperable)
			{
				return val2.Fail(GameStrings.DeviceNoPower);
			}
			if (!doAction)
			{
				return val2;
			}
			val.Rename((Thing)(object)this);
			return val2;
		}
		DynamicThing sourceItem2 = ((Attack)(ref attack)).SourceItem;
		Screwdriver val3 = (Screwdriver)(object)((sourceItem2 is Screwdriver) ? sourceItem2 : null);
		if (val3 != null)
		{
			return BoardRelocateHooks.StructureAttackWith((IPlacementBoardRelocatable)(object)this, attack, doAction, BoardRelocateHooks.NormalToolRelocateContinue((DynamicThing)(object)val3));
		}
		return ((Structure)this).AttackWith(attack, doAction);
	}

	public void OnStructureRelocated()
	{
	}

	Structure IPlacementBoardRelocatable.get_AsStructure()
	{
		return ((Thing)this).AsStructure;
	}

	string IPlacementBoardStructure.get_name()
	{
		return ((Object)this).name;
	}

	void IPlacementBoardStructure.SetStructureData(Quaternion localRotation, ulong ownerClientId, Grid3 localGrid, int customColourIndex)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		((Structure)this).SetStructureData(localRotation, ownerClientId, localGrid, customColourIndex);
	}
}
public class StructureConsoleLabelDiode : Diode, IPlacementBoardRelocatable, IPlacementBoardStructure, IReferencable, IEvaluable, IPatchOnLoad, IBoardPoweredListener
{
	[Header("Main label mesh")]
	public MeshRenderer MeshRenderer;

	public int materialLabelIndex;

	[Header("Colorable/emissive part")]
	public MeshRenderer emissiveRenderer;

	public int textTextureWidth = 512;

	public int textTextureHeight = 128;

	private RenderTexture _textTexture;

	private Coroutine blinkCoroutine;

	private static readonly float[] EmissionIntensityByColor = new float[12]
	{
		3.6f, 2.8f, 2.4f, 1.5f, 1.6f, 1.5f, 1.3f, 1.2f, 2f, 2f,
		1.6f, 4f
	};

	private static WaitForSeconds BlinkWait = new WaitForSeconds(0.5f);

	private string DisplayText => ((Thing)this).CustomName ?? "";

	public PlacementBoard Board { get; set; }

	public BoardCell[] BoardCells { get; set; }

	public override bool Powered => (Board as IPoweredBoard)?.Powered ?? false;

	public void PatchOnLoad()
	{
		((Thing)(object)this).ApplyPaintableChild("Bulb", (ColorType)5, fallback: true);
		((Thing)(object)this).ApplyBlueprintMaterials();
		((Structure)(object)this).SetExitTool(PrefabPatch.DeviceExitTool);
	}

	public override void Awake()
	{
		((WallLight)this).Awake();
		BoardStructureHooks.Awake<StructureConsoleLabelDiode>(this);
		((Structure)(object)this).InitializeText(MeshRenderer, materialLabelIndex, ref _textTexture, textTextureWidth, textTextureHeight);
		UpdateLabelText();
	}

	public override void OnFinishedLoad()
	{
		((Diode)this).OnFinishedLoad();
		UpdateLabelText();
		UpdateLight();
	}

	public override void OnRenamed()
	{
		((Device)this).OnRenamed();
		UpdateLabelText();
	}

	private void UpdateLight()
	{
		if (((Thing)this).Mode == 1 && ((Thing)this).Powered && ((Thing)this).OnOff)
		{
			if (blinkCoroutine == null)
			{
				blinkCoroutine = ((MonoBehaviour)this).StartCoroutine(BlinkLoop());
			}
			return;
		}
		if (blinkCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(blinkCoroutine);
			blinkCoroutine = null;
		}
		ApplyEmissive(((Thing)this).OnOff && ((Thing)this).Powered);
	}

	private IEnumerator BlinkLoop()
	{
		bool blinkState = false;
		while (true)
		{
			blinkState = !blinkState;
			ApplyEmissive(blinkState && ((Thing)this).Powered && ((Thing)this).OnOff);
			yield return BlinkWait;
		}
	}

	public override CanConstructInfo CanConstruct()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return BoardStructureHooks.CanConstruct<StructureConsoleLabelDiode>(this);
	}

	private void ApplyEmissive(bool enable)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)emissiveRenderer == (Object)null)
		{
			return;
		}
		((Thing)this).SetCustomColor(((Thing)this).ColorState, enable);
		if ((Object)(object)((Renderer)emissiveRenderer).sharedMaterial == (Object)(object)((Renderer)emissiveRenderer).material)
		{
			((Renderer)emissiveRenderer).material = new Material(((Renderer)emissiveRenderer).material);
		}
		Material material = ((Renderer)emissiveRenderer).material;
		if ((Object)(object)material == (Object)null || !material.HasProperty("_EmissionColor"))
		{
			return;
		}
		if (enable)
		{
			float num = 1f;
			if (((Thing)this).ColorState >= 0 && ((Thing)this).ColorState < EmissionIntensityByColor.Length)
			{
				num = EmissionIntensityByColor[((Thing)this).ColorState];
			}
			Color val = GetColorByIndex(((Thing)this).ColorState) * num;
			material.EnableKeyword("_EMISSION");
			material.SetColor("_EmissionColor", val);
		}
		else
		{
			material.DisableKeyword("_EMISSION");
			material.SetColor("_EmissionColor", Color.black);
		}
		ToggleLightsOn(((Behaviour)this).enabled);
	}

	private Color GetColorByIndex(int index)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		return (Color)(index switch
		{
			0 => Color.blue, 
			1 => Color.gray, 
			2 => Color.green, 
			3 => new Color(1f, 0.5f, 0f), 
			4 => Color.red, 
			5 => Color.yellow, 
			6 => Color.white, 
			7 => Color.black, 
			8 => new Color(0.4f, 0.26f, 0.13f), 
			9 => new Color(0.94f, 0.9f, 0.55f), 
			10 => new Color(1f, 0.4f, 0.6f), 
			11 => new Color(0.5f, 0f, 0.5f), 
			_ => Color.white, 
		});
	}

	private void ToggleLightsOn(bool on)
	{
		object obj = typeof(WallLight).GetField("light", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(this);
		if ((Object)((obj is Light) ? obj : null) != (Object)null)
		{
			((WallLight)this).ToggleLightsOn(on);
		}
	}

	public override void OnInteractableUpdated(Interactable interactable)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Invalid comparison between Unknown and I4
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Invalid comparison between Unknown and I4
		InteractableType action = interactable.Action;
		if (action - 36 <= 1 || (int)action == 42 || (int)action == 45)
		{
			UpdateLight();
		}
		else
		{
			((Diode)this).OnInteractableUpdated(interactable);
		}
	}

	public void OnBoardPowerChanged()
	{
		if (!((Object)(object)emissiveRenderer == (Object)null) && !((Object)(object)((Renderer)emissiveRenderer).material == (Object)null))
		{
			UpdateLight();
		}
	}

	public override void OnDeregistered()
	{
		((Device)this).OnDeregistered();
		if (blinkCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(blinkCoroutine);
			blinkCoroutine = null;
		}
		BoardStructureHooks.OnDeregistered<StructureConsoleLabelDiode>(this);
	}

	public override ThingSaveData SerializeSave()
	{
		ThingSaveData val;
		ThingSaveData result = (val = (ThingSaveData)(object)new ConsoleLogicSaveData());
		((Thing)this).InitialiseSaveData(ref val);
		return result;
	}

	protected override void InitialiseSaveData(ref ThingSaveData baseData)
	{
		((Structure)this).InitialiseSaveData(ref baseData);
		if (baseData is ConsoleLogicSaveData consoleLogicSaveData)
		{
			consoleLogicSaveData.Board = BoardStructureHooks.SerializeSave((IPlacementBoardStructure)(object)this);
		}
	}

	public override void DeserializeSave(ThingSaveData baseData)
	{
		((Structure)this).DeserializeSave(baseData);
		if (baseData is ConsoleLogicSaveData consoleLogicSaveData)
		{
			BoardStructureHooks.DeserializeSave((IPlacementBoardStructure)(object)this, consoleLogicSaveData.Board);
			UpdateLabelText();
			OnBoardPowerChanged();
		}
	}

	public override void SerializeOnJoin(RocketBinaryWriter writer)
	{
		((Structure)this).SerializeOnJoin(writer);
		BoardStructureHooks.SerializeOnJoin(writer, (IPlacementBoardStructure)(object)this);
	}

	public override void DeserializeOnJoin(RocketBinaryReader reader)
	{
		((Structure)this).DeserializeOnJoin(reader);
		BoardStructureHooks.DeserializeOnJoin(reader, (IPlacementBoardStructure)(object)this);
	}

	private void UpdateLabelText()
	{
		TextQueue.RenderText((Thing)(object)this, _textTexture, DisplayText);
	}

	public override DelayedActionInstance AttackWith(Attack attack, bool doAction = true)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		DynamicThing sourceItem = ((Attack)(ref attack)).SourceItem;
		Screwdriver val = (Screwdriver)(object)((sourceItem is Screwdriver) ? sourceItem : null);
		if (val != null)
		{
			return BoardRelocateHooks.StructureAttackWith((IPlacementBoardRelocatable)(object)this, attack, doAction, BoardRelocateHooks.NormalToolRelocateContinue((DynamicThing)(object)val));
		}
		return ((Device)this).AttackWith(attack, doAction);
	}

	public void OnStructureRelocated()
	{
	}

	Structure IPlacementBoardRelocatable.get_AsStructure()
	{
		return ((Thing)this).AsStructure;
	}

	string IPlacementBoardStructure.get_name()
	{
		return ((Object)this).name;
	}

	void IPlacementBoardStructure.SetStructureData(Quaternion localRotation, ulong ownerClientId, Grid3 localGrid, int customColourIndex)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		((Structure)this).SetStructureData(localRotation, ownerClientId, localGrid, customColourIndex);
	}
}
public class StructureConsoleLever : LogicUnitBase, IPlacementBoardRelocatable, IPlacementBoardStructure, IReferencable, IEvaluable, IPatchOnLoad
{
	[Header("Main label mesh")]
	public MeshRenderer MeshRenderer;

	public int materialLabelIndex;

	public int textTextureWidth = 512;

	public int textTextureHeight = 128;

	private RenderTexture _textTexture;

	[CompilerGenerated]
	private Event m_ButtonPress;

	private string RawName => ((Thing)this).CustomName ?? "";

	public PlacementBoard Board { get; set; }

	public BoardCell[] BoardCells { get; set; }

	public override bool Powered => (Board as IPoweredBoard)?.Powered ?? false;

	public override double Setting
	{
		get
		{
			return ((Thing)this).IsOpen ? 1 : 0;
		}
		set
		{
			if (((Thing)this).IsOpen != (value != 0.0))
			{
				OnServer.Interact(((Thing)this).InteractOpen, (value != 0.0) ? 1 : 0, false);
			}
		}
	}

	public event Event ButtonPress
	{
		[CompilerGenerated]
		add
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Expected O, but got Unknown
			Event val = this.m_ButtonPress;
			Event val2;
			do
			{
				val2 = val;
				Event value2 = (Event)Delegate.Combine((Delegate)(object)val2, (Delegate)(object)value);
				val = Interlocked.CompareExchange(ref this.m_ButtonPress, value2, val2);
			}
			while (val != val2);
		}
		[CompilerGenerated]
		remove
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Expected O, but got Unknown
			Event val = this.m_ButtonPress;
			Event val2;
			do
			{
				val2 = val;
				Event value2 = (Event)Delegate.Remove((Delegate)(object)val2, (Delegate)(object)value);
				val = Interlocked.CompareExchange(ref this.m_ButtonPress, value2, val2);
			}
			while (val != val2);
		}
	}

	public void PatchOnLoad()
	{
		((Thing)(object)this).ApplyBlueprintMaterials();
		((Structure)(object)this).SetExitTool(PrefabPatch.DeviceExitTool);
	}

	public override void Awake()
	{
		((Device)this).Awake();
		BoardStructureHooks.Awake<StructureConsoleLever>(this);
		((Structure)(object)this).InitializeText(MeshRenderer, materialLabelIndex, ref _textTexture, textTextureWidth, textTextureHeight);
		UpdateLabelText();
	}

	public override void OnFinishedThingSync()
	{
		((Thing)this).OnFinishedThingSync();
		UpdateLabelText();
	}

	public override void OnRenamed()
	{
		((Device)this).OnRenamed();
		UpdateLabelText();
	}

	public override CanConstructInfo CanConstruct()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return BoardStructureHooks.CanConstruct<StructureConsoleLever>(this);
	}

	public override void OnDeregistered()
	{
		((Device)this).OnDeregistered();
		BoardStructureHooks.OnDeregistered<StructureConsoleLever>(this);
	}

	public override ThingSaveData SerializeSave()
	{
		ThingSaveData val;
		ThingSaveData result = (val = (ThingSaveData)(object)new ConsoleLogicSaveData());
		((Thing)this).InitialiseSaveData(ref val);
		return result;
	}

	protected override void InitialiseSaveData(ref ThingSaveData baseData)
	{
		((LogicUnitBase)this).InitialiseSaveData(ref baseData);
		if (baseData is ConsoleLogicSaveData consoleLogicSaveData)
		{
			consoleLogicSaveData.Board = BoardStructureHooks.SerializeSave((IPlacementBoardStructure)(object)this);
		}
	}

	public override void DeserializeSave(ThingSaveData baseData)
	{
		((LogicUnitBase)this).DeserializeSave(baseData);
		if (baseData is ConsoleLogicSaveData consoleLogicSaveData)
		{
			BoardStructureHooks.DeserializeSave((IPlacementBoardStructure)(object)this, consoleLogicSaveData.Board);
			UpdateLabelText();
		}
	}

	public override void SerializeOnJoin(RocketBinaryWriter writer)
	{
		((LogicUnitBase)this).SerializeOnJoin(writer);
		BoardStructureHooks.SerializeOnJoin(writer, (IPlacementBoardStructure)(object)this);
	}

	public override void DeserializeOnJoin(RocketBinaryReader reader)
	{
		((LogicUnitBase)this).DeserializeOnJoin(reader);
		BoardStructureHooks.DeserializeOnJoin(reader, (IPlacementBoardStructure)(object)this);
	}

	private void UpdateLabelText()
	{
		string text = RawName;
		if (string.IsNullOrEmpty(text))
		{
			text = ((Thing)this).DisplayName;
		}
		if (text.EndsWith("*"))
		{
			text = "";
		}
		TextQueue.RenderText((Thing)(object)this, _textTexture, text);
	}

	public override bool CanLogicRead(LogicType logicType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if ((int)logicType != 12)
		{
			return ((Device)this).CanLogicRead(logicType);
		}
		return true;
	}

	public override double GetLogicValue(LogicType logicType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if ((int)logicType != 12)
		{
			return ((Device)this).GetLogicValue(logicType);
		}
		return ((LogicUnitBase)this).Setting;
	}

	public override DelayedActionInstance AttackWith(Attack attack, bool doAction = true)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		DynamicThing sourceItem = ((Attack)(ref attack)).SourceItem;
		Screwdriver val = (Screwdriver)(object)((sourceItem is Screwdriver) ? sourceItem : null);
		if (val != null)
		{
			return BoardRelocateHooks.StructureAttackWith((IPlacementBoardRelocatable)(object)this, attack, doAction, BoardRelocateHooks.NormalToolRelocateContinue((DynamicThing)(object)val));
		}
		return ((Device)this).AttackWith(attack, doAction);
	}

	public void OnStructureRelocated()
	{
	}

	Structure IPlacementBoardRelocatable.get_AsStructure()
	{
		return ((Thing)this).AsStructure;
	}

	string IPlacementBoardStructure.get_name()
	{
		return ((Object)this).name;
	}

	void IPlacementBoardStructure.SetStructureData(Quaternion localRotation, ulong ownerClientId, Grid3 localGrid, int customColourIndex)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		((Structure)this).SetStructureData(localRotation, ownerClientId, localGrid, customColourIndex);
	}
}
public class StructureConsoleNumpad : LogicUnitBase, IPlacementBoardRelocatable, IPlacementBoardStructure, IReferencable, IEvaluable, IPatchOnLoad, IBoardPoweredListener
{
	private static readonly int LabelSound = Animator.StringToHash("Label");

	private static readonly int ConfirmSound = Animator.StringToHash("LabelConfirm");

	private static readonly int CancelSound = Animator.StringToHash("LabelCancel");

	[Header("Colorable/emissive part")]
	[SerializeField]
	private MeshRenderer emissiveRenderer;

	private bool _initialized;

	private bool _interactionLocked;

	private Coroutine _pulseRoutine;

	private object _onSubmitDelegate;

	private MethodInfo _addOnSubmit;

	private MethodInfo _addOnCancel;

	private MethodInfo _removeOnCancel;

	private object _onCancelDelegate;

	public PlacementBoard Board { get; set; }

	public BoardCell[] BoardCells { get; set; }

	public override bool Powered => (Board as IPoweredBoard)?.Powered ?? false;

	public void PatchOnLoad()
	{
		((Thing)(object)this).ApplyPaintableChild("Button", (ColorType)2, fallback: true);
		((Thing)(object)this).ApplyBlueprintMaterials();
		((Structure)(object)this).SetExitTool(PrefabPatch.DeviceExitTool);
	}

	public override void Awake()
	{
		((Device)this).Awake();
		((Structure)(object)this).InitializeEmissive(emissiveRenderer);
		BoardStructureHooks.Awake<StructureConsoleNumpad>(this);
		_initialized = true;
	}

	public override CanConstructInfo CanConstruct()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return BoardStructureHooks.CanConstruct<StructureConsoleNumpad>(this);
	}

	public override void OnDeregistered()
	{
		((Device)this).OnDeregistered();
		BoardStructureHooks.OnDeregistered<StructureConsoleNumpad>(this);
	}

	public override ThingSaveData SerializeSave()
	{
		ThingSaveData val;
		ThingSaveData result = (val = (ThingSaveData)(object)new ConsoleLogicSaveData());
		((Thing)this).InitialiseSaveData(ref val);
		return result;
	}

	protected override void InitialiseSaveData(ref ThingSaveData baseData)
	{
		((LogicUnitBase)this).InitialiseSaveData(ref baseData);
		if (baseData is ConsoleLogicSaveData consoleLogicSaveData)
		{
			consoleLogicSaveData.Board = BoardStructureHooks.SerializeSave((IPlacementBoardStructure)(object)this);
		}
	}

	public override void DeserializeSave(ThingSaveData baseData)
	{
		((LogicUnitBase)this).DeserializeSave(baseData);
		if (baseData is ConsoleLogicSaveData consoleLogicSaveData)
		{
			BoardStructureHooks.DeserializeSave((IPlacementBoardStructure)(object)this, consoleLogicSaveData.Board);
		}
	}

	public override void SerializeOnJoin(RocketBinaryWriter writer)
	{
		((LogicUnitBase)this).SerializeOnJoin(writer);
		BoardStructureHooks.SerializeOnJoin(writer, (IPlacementBoardStructure)(object)this);
	}

	public override void DeserializeOnJoin(RocketBinaryReader reader)
	{
		((LogicUnitBase)this).DeserializeOnJoin(reader);
		BoardStructureHooks.DeserializeOnJoin(reader, (IPlacementBoardStructure)(object)this);
	}

	public override void OnInteractableUpdated(Interactable target)
	{
		((LogicUnitBase)this).OnInteractableUpdated(target);
		((Structure)(object)this).SyncMaterials(emissiveRenderer);
	}

	public void OnBoardPowerChanged()
	{
		((Structure)(object)this).SyncMaterials(emissiveRenderer);
	}

	public override void SetCustomColor(int index, bool emissive = false)
	{
		((Structure)this).SetCustomColor(index, emissive);
		((Structure)(object)this).SetEmissive(emissiveRenderer, emissive);
	}

	public override DelayedActionInstance InteractWith(Interactable target, Interaction interaction, bool performAction = true)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Invalid comparison between Unknown and I4
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Invalid comparison between Unknown and I4
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Invalid comparison between Unknown and I4
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		DelayedActionInstance val = new DelayedActionInstance
		{
			Duration = 0f,
			ActionMessage = ActionStrings.Set
		};
		val.AppendStateMessage(GameStrings.GlobalValue, ExtensionMethods.ToStringExact(((LogicUnitBase)this).Setting));
		if (_interactionLocked)
		{
			return val.Fail(GameStrings.GlobalAlreadyInUse);
		}
		if (!performAction)
		{
			return val;
		}
		if (!((Thing)this).Powered)
		{
			return val.Fail(GameStrings.DeviceNoPower);
		}
		InteractableType action = target.Action;
		if ((int)action != 31)
		{
			if (action - 32 <= 3 || action - 126 <= 5)
			{
				HandleNumberButton(target.Action);
				return val.Succeed();
			}
			return ((LogicUnitBase)this).InteractWith(target, interaction, performAction);
		}
		if (!GameManager.IsBatchMode && (Object)(object)((Interaction)(ref interaction)).SourceThing == (Object)(object)InventoryManager.Parent)
		{
			ApplySetting();
		}
		return val.Succeed();
	}

	private void HandleNumberButton(InteractableType action)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected I4, but got Unknown
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected I4, but got Unknown
		int num = (action - 32) switch
		{
			0 => 1, 
			1 => 2, 
			2 => 3, 
			3 => 4, 
			_ => (action - 126) switch
			{
				0 => 5, 
				1 => 6, 
				2 => 7, 
				3 => 8, 
				4 => 9, 
				5 => 0, 
				_ => -1, 
			}, 
		};
		if (num >= 0)
		{
			((LogicUnitBase)this).Setting = num;
			if (_pulseRoutine != null)
			{
				((MonoBehaviour)this).StopCoroutine(_pulseRoutine);
			}
			_pulseRoutine = ((MonoBehaviour)this).StartCoroutine(PulseModeCoroutine());
		}
	}

	private IEnumerator PulseModeCoroutine()
	{
		if (!_interactionLocked && ((Thing)this).Powered && _initialized)
		{
			_interactionLocked = true;
			((Device)this).SetLogicValue((LogicType)3, 1.0);
			yield return (object)new WaitForSeconds(0.55f);
			((Device)this).SetLogicValue((LogicType)3, 0.0);
			yield return (object)new WaitForSeconds(0.2f);
			_interactionLocked = false;
		}
	}

	private void InitDelegates()
	{
		if (_onCancelDelegate == null)
		{
			_addOnSubmit = typeof(InputWindow).GetMethod("add_OnSubmit", BindingFlags.Static | BindingFlags.Public);
			_addOnCancel = typeof(InputWindow).GetMethod("add_OnCancel", BindingFlags.Static | BindingFlags.Public);
			_removeOnCancel = typeof(InputWindow).GetMethod("remove_OnCancel", BindingFlags.Static | BindingFlags.Public);
			Type parameterType = _addOnSubmit.GetParameters()[0].ParameterType;
			_onSubmitDelegate = Delegate.CreateDelegate(parameterType, this, PatchUtils.Method(() => ProcessInputValue(null, null)));
			Type parameterType2 = _addOnCancel.GetParameters()[0].ParameterType;
			_onCancelDelegate = Delegate.CreateDelegate(parameterType2, this, PatchUtils.Method(() => PlayCancelFeedback()));
		}
	}

	private void AddOnSubmit()
	{
		InitDelegates();
		_addOnSubmit.Invoke(null, new object[1] { _onSubmitDelegate });
	}

	private void AddOnCancel()
	{
		InitDelegates();
		_addOnCancel.Invoke(null, new object[1] { _onCancelDelegate });
	}

	private void RemoveOnCancel()
	{
		InitDelegates();
		_removeOnCancel.Invoke(null, new object[1] { _onCancelDelegate });
	}

	private void ApplySetting()
	{
		if (InputWindow.ShowInputPanel("Set " + ((Thing)this).DisplayName, ExtensionMethods.ToStringExact(((LogicUnitBase)this).Setting), (Thing)(object)this, 32, (ContentType)3, 600))
		{
			AddOnSubmit();
			AddOnCancel();
			((Thing)this).PlaySound(LabelSound, 1f, 1f);
		}
	}

	private void PlayCancelFeedback()
	{
		((Thing)this).PlaySound(CancelSound, 1f, 1f);
		RemoveOnCancel();
	}

	private void ProcessInputValue(string input, string _)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected O, but got Unknown
		if (Object.op_Implicit((Object)(object)this) && double.TryParse(input, NumberStyles.Float, CultureInfo.CurrentCulture, out var result))
		{
			if (double.IsPositiveInfinity(result))
			{
				result = double.MaxValue;
			}
			if (NetworkManager.IsClient)
			{
				((MessageBase<SetLogicFromClient>)new SetLogicFromClient
				{
					LogicId = ((Thing)this).NetworkId,
					LogicType = (LogicType)12,
					Value = result
				}).SendToServer();
			}
			else
			{
				((LogicUnitBase)this).Setting = result;
			}
			((Thing)this).PlaySound(ConfirmSound, 1f, 1f);
		}
	}

	public override bool CanLogicRead(LogicType type)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if ((int)type != 12 && (int)type != 3)
		{
			return ((Device)this).CanLogicRead(type);
		}
		return true;
	}

	public override bool CanLogicWrite(LogicType type)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if ((int)type != 12 && (int)type != 3)
		{
			return ((Device)this).CanLogicWrite(type);
		}
		return true;
	}

	public override double GetLogicValue(LogicType type)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if ((int)type != 12)
		{
			return ((Device)this).GetLogicValue(type);
		}
		return ((LogicUnitBase)this).Setting;
	}

	public override void SetLogicValue(LogicType type, double value)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		((Device)this).SetLogicValue(type, value);
		if ((int)type == 12)
		{
			((LogicUnitBase)this).Setting = value;
		}
	}

	public override DelayedActionInstance AttackWith(Attack attack, bool doAction = true)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		DynamicThing sourceItem = ((Attack)(ref attack)).SourceItem;
		Screwdriver val = (Screwdriver)(object)((sourceItem is Screwdriver) ? sourceItem : null);
		if (val != null)
		{
			return BoardRelocateHooks.StructureAttackWith((IPlacementBoardRelocatable)(object)this, attack, doAction, BoardRelocateHooks.NormalToolRelocateContinue((DynamicThing)(object)val));
		}
		return ((Device)this).AttackWith(attack, doAction);
	}

	public void OnStructureRelocated()
	{
	}

	Structure IPlacementBoardRelocatable.get_AsStructure()
	{
		return ((Thing)this).AsStructure;
	}

	string IPlacementBoardStructure.get_name()
	{
		return ((Object)this).name;
	}

	void IPlacementBoardStructure.SetStructureData(Quaternion localRotation, ulong ownerClientId, Grid3 localGrid, int customColourIndex)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		((Structure)this).SetStructureData(localRotation, ownerClientId, localGrid, customColourIndex);
	}
}
public class StructureConsoleSlider : StructureConsoleDraggable, IPatchOnLoad
{
	public Transform SliderTransform;

	public Vector3 SliderAxis;

	public float SliderMinPosition;

	public float SliderMaxPosition;

	public void PatchOnLoad()
	{
		((Thing)(object)this).ApplyPaintableChild("SliderKnob", (ColorType)7, fallback: false);
		((Thing)(object)this).ApplyBlueprintMaterials();
		((Structure)(object)this).SetExitTool(PrefabPatch.DeviceExitTool);
	}

	protected override double SettingFromRay(Ray ray)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		return Math.Clamp((MathHelpers.RayPlaneOffset(ray, ((Component)this).transform.position, SliderTransform.forward, SliderTransform.rotation * SliderAxis) - SliderMinPosition) / (SliderMaxPosition - SliderMinPosition), 0f, 1f);
	}

	protected override void UpdateDraggablePosition(double setting)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		SliderTransform.localPosition = SliderAxis * Mathf.Lerp(SliderMinPosition, SliderMaxPosition, (float)setting);
	}
}
public class StructureConsoleSwitch : LogicUnitBase, IPlacementBoardRelocatable, IPlacementBoardStructure, IReferencable, IEvaluable, IPatchOnLoad, IBoardPoweredListener
{
	[Header("Colorable/emissive part")]
	public MeshRenderer emissiveRenderer;

	public PlacementBoard Board { get; set; }

	public BoardCell[] BoardCells { get; set; }

	public override bool Powered => (Board as IPoweredBoard)?.Powered ?? false;

	public override double Setting
	{
		get
		{
			return ((Thing)this).OnOff ? 1 : 0;
		}
		set
		{
			bool flag = value != 0.0;
			if (flag != ((Thing)this).OnOff)
			{
				OnServer.Interact(((Thing)this).InteractOnOff, flag ? 1 : 0, false);
			}
		}
	}

	public void PatchOnLoad()
	{
		((Thing)(object)this).ApplyPaintableChild("Button", (ColorType)2, fallback: false);
		((Thing)(object)this).ApplyBlueprintMaterials();
		((Structure)(object)this).SetExitTool(PrefabPatch.DeviceExitTool);
	}

	public override void Awake()
	{
		((Device)this).Awake();
		((Structure)(object)this).InitializeEmissive(emissiveRenderer);
		BoardStructureHooks.Awake<StructureConsoleSwitch>(this);
	}

	public override void OnInteractableUpdated(Interactable interactable)
	{
		((LogicUnitBase)this).OnInteractableUpdated(interactable);
		((Structure)(object)this).SyncMaterials(emissiveRenderer, poweredOnly: true);
	}

	public override void SetCustomColor(int index, bool emissive = false)
	{
		((Structure)this).SetCustomColor(index, emissive);
		((Structure)(object)this).SetEmissive(emissiveRenderer, emissive);
	}

	public void OnBoardPowerChanged()
	{
		((Structure)(object)this).SyncMaterials(emissiveRenderer, poweredOnly: true);
	}

	public override CanConstructInfo CanConstruct()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return BoardStructureHooks.CanConstruct<StructureConsoleSwitch>(this);
	}

	public override void OnDeregistered()
	{
		((Device)this).OnDeregistered();
		BoardStructureHooks.OnDeregistered<StructureConsoleSwitch>(this);
	}

	public override ThingSaveData SerializeSave()
	{
		ThingSaveData val;
		ThingSaveData result = (val = (ThingSaveData)(object)new ConsoleLogicSaveData());
		((Thing)this).InitialiseSaveData(ref val);
		return result;
	}

	protected override void InitialiseSaveData(ref ThingSaveData baseData)
	{
		((LogicUnitBase)this).InitialiseSaveData(ref baseData);
		if (baseData is ConsoleLogicSaveData consoleLogicSaveData)
		{
			consoleLogicSaveData.Board = BoardStructureHooks.SerializeSave((IPlacementBoardStructure)(object)this);
		}
	}

	public override void DeserializeSave(ThingSaveData baseData)
	{
		((LogicUnitBase)this).DeserializeSave(baseData);
		if (baseData is ConsoleLogicSaveData consoleLogicSaveData)
		{
			BoardStructureHooks.DeserializeSave((IPlacementBoardStructure)(object)this, consoleLogicSaveData.Board);
		}
	}

	public override void SerializeOnJoin(RocketBinaryWriter writer)
	{
		((LogicUnitBase)this).SerializeOnJoin(writer);
		BoardStructureHooks.SerializeOnJoin(writer, (IPlacementBoardStructure)(object)this);
	}

	public override void DeserializeOnJoin(RocketBinaryReader reader)
	{
		((LogicUnitBase)this).DeserializeOnJoin(reader);
		BoardStructureHooks.DeserializeOnJoin(reader, (IPlacementBoardStructure)(object)this);
	}

	public override bool CanLogicRead(LogicType logicType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if ((int)logicType != 12)
		{
			return ((Device)this).CanLogicRead(logicType);
		}
		return true;
	}

	public override double GetLogicValue(LogicType logicType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if ((int)logicType != 12)
		{
			return ((Device)this).GetLogicValue(logicType);
		}
		return ((LogicUnitBase)this).Setting;
	}

	public override DelayedActionInstance AttackWith(Attack attack, bool doAction = true)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		DynamicThing sourceItem = ((Attack)(ref attack)).SourceItem;
		Screwdriver val = (Screwdriver)(object)((sourceItem is Screwdriver) ? sourceItem : null);
		if (val != null)
		{
			return BoardRelocateHooks.StructureAttackWith((IPlacementBoardRelocatable)(object)this, attack, doAction, BoardRelocateHooks.NormalToolRelocateContinue((DynamicThing)(object)val));
		}
		return ((Device)this).AttackWith(attack, doAction);
	}

	public void OnStructureRelocated()
	{
	}

	Structure IPlacementBoardRelocatable.get_AsStructure()
	{
		return ((Thing)this).AsStructure;
	}

	string IPlacementBoardStructure.get_name()
	{
		return ((Object)this).name;
	}

	void IPlacementBoardStructure.SetStructureData(Quaternion localRotation, ulong ownerClientId, Grid3 localGrid, int customColourIndex)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		((Structure)this).SetStructureData(localRotation, ownerClientId, localGrid, customColourIndex);
	}
}
public class StructureConsoleThrottle : StructureConsoleDraggable, IPatchOnLoad
{
	public Transform ThrottleTransform;

	public Vector3 ThrottleRotAxis = Vector3.right;

	public float ThrottleMinAngle = 60f;

	public float ThrottleMaxAngle = -60f;

	private float ThrottleSqRadius
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			Bounds bounds = DragCollider.bounds;
			Vector3 val = ((Bounds)(ref bounds)).center - ThrottleTransform.position;
			return ((Vector3)(ref val)).sqrMagnitude;
		}
	}

	public void PatchOnLoad()
	{
		((Thing)(object)this).ApplyPaintableChild("ThrottleStick", (ColorType)1, fallback: false);
		((Thing)(object)this).ApplyBlueprintMaterials();
		((Structure)(object)this).SetExitTool(PrefabPatch.DeviceExitTool);
	}

	private float SettingToAngle(double setting)
	{
		return Mathf.Clamp01((float)setting) * (ThrottleMaxAngle - ThrottleMinAngle) + ThrottleMinAngle;
	}

	private double AngleToSetting(float angle)
	{
		return Math.Clamp((double)(angle - ThrottleMinAngle) / (double)(ThrottleMaxAngle - ThrottleMinAngle), 0.0, 1.0);
	}

	protected override void UpdateDraggablePosition(double setting)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		ThrottleTransform.localRotation = Quaternion.AngleAxis(SettingToAngle(setting), ThrottleRotAxis);
	}

	protected override double SettingFromRay(Ray ray)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		float num = MathHelpers.RayCylinderAngle(ray, ThrottleTransform.position, ((Component)this).transform.rotation * ThrottleRotAxis, ((Component)this).transform.forward, ThrottleSqRadius);
		float throttleMinAngle = ThrottleMinAngle;
		float throttleMaxAngle = ThrottleMaxAngle;
		float num2 = throttleMinAngle;
		float num3 = throttleMaxAngle;
		if (num2 > num3)
		{
			float num4 = num3;
			throttleMinAngle = num2;
			num2 = num4;
			num3 = throttleMinAngle;
		}
		num = Mathf.Clamp(num, num2, num3);
		return AngleToSetting(num);
	}
}
public class StructureConsoleUtilityButton : LogicUnitBase, IPlacementBoardRelocatable, IPlacementBoardStructure, IReferencable, IEvaluable, IPatchOnLoad
{
	[Header("Main label mesh")]
	public MeshRenderer MeshRenderer;

	public int materialLabelIndex;

	public int textTextureWidth = 512;

	public int textTextureHeight = 512;

	private RenderTexture _textTexture;

	private Coroutine _resetRoutine;

	[CompilerGenerated]
	private Event m_ButtonPress;

	private bool _activated;

	private string RawName => ((Thing)this).CustomName ?? "";

	public PlacementBoard Board { get; set; }

	public BoardCell[] BoardCells { get; set; }

	public override bool Powered => (Board as IPoweredBoard)?.Powered ?? false;

	public override bool OnOff => ((Thing)this).Powered;

	public event Event ButtonPress
	{
		[CompilerGenerated]
		add
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Expected O, but got Unknown
			Event val = this.m_ButtonPress;
			Event val2;
			do
			{
				val2 = val;
				Event value2 = (Event)Delegate.Combine((Delegate)(object)val2, (Delegate)(object)value);
				val = Interlocked.CompareExchange(ref this.m_ButtonPress, value2, val2);
			}
			while (val != val2);
		}
		[CompilerGenerated]
		remove
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Expected O, but got Unknown
			Event val = this.m_ButtonPress;
			Event val2;
			do
			{
				val2 = val;
				Event value2 = (Event)Delegate.Remove((Delegate)(object)val2, (Delegate)(object)value);
				val = Interlocked.CompareExchange(ref this.m_ButtonPress, value2, val2);
			}
			while (val != val2);
		}
	}

	public void PatchOnLoad()
	{
		((Thing)(object)this).ApplyPaintableChild("UtilityButton", (ColorType)2, fallback: false);
		((Thing)(object)this).ApplyBlueprintMaterials();
		((Structure)(object)this).SetExitTool(PrefabPatch.DeviceExitTool);
	}

	public override void Awake()
	{
		((Device)this).Awake();
		BoardStructureHooks.Awake<StructureConsoleUtilityButton>(this);
		((Structure)(object)this).InitializeText(MeshRenderer, materialLabelIndex, ref _textTexture, textTextureWidth, textTextureHeight);
		UpdateLabelText();
	}

	public override void OnFinishedThingSync()
	{
		((Thing)this).OnFinishedThingSync();
		UpdateLabelText();
	}

	public override void OnRenamed()
	{
		((Device)this).OnRenamed();
		UpdateLabelText();
	}

	private IEnumerator WaitThenStop()
	{
		yield return (object)new WaitForSeconds(0.55f);
		((LogicUnitBase)this).Setting = 0.0;
		_activated = false;
		OnServer.Interact(((Thing)this).InteractActivate, 0, false);
	}

	public override void OnInteractableUpdated(Interactable interactable)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Invalid comparison between Unknown and I4
		((LogicUnitBase)this).OnInteractableUpdated(interactable);
		if (!_activated && GameManager.RunSimulation && (int)interactable.Action == 41 && interactable.State == 1)
		{
			((LogicUnitBase)this).Setting = interactable.State;
			if (_resetRoutine != null)
			{
				((MonoBehaviour)this).StopCoroutine(_resetRoutine);
			}
			_resetRoutine = ((MonoBehaviour)this).StartCoroutine(WaitThenStop());
		}
	}

	public override CanConstructInfo CanConstruct()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return BoardStructureHooks.CanConstruct<StructureConsoleUtilityButton>(this);
	}

	public override void OnDeregistered()
	{
		((Device)this).OnDeregistered();
		BoardStructureHooks.OnDeregistered<StructureConsoleUtilityButton>(this);
	}

	public override ThingSaveData SerializeSave()
	{
		ThingSaveData val;
		ThingSaveData result = (val = (ThingSaveData)(object)new ConsoleLogicSaveData());
		((Thing)this).InitialiseSaveData(ref val);
		return result;
	}

	protected override void InitialiseSaveData(ref ThingSaveData baseData)
	{
		((LogicUnitBase)this).InitialiseSaveData(ref baseData);
		if (baseData is ConsoleLogicSaveData consoleLogicSaveData)
		{
			((ThingSaveData)consoleLogicSaveData).CustomName = ((Thing)this).CustomName;
			consoleLogicSaveData.Board = BoardStructureHooks.SerializeSave((IPlacementBoardStructure)(object)this);
		}
	}

	public override void DeserializeSave(ThingSaveData baseData)
	{
		((LogicUnitBase)this).DeserializeSave(baseData);
		if (baseData is ConsoleLogicSaveData consoleLogicSaveData)
		{
			((Thing)this).CustomName = ((ThingSaveData)consoleLogicSaveData).CustomName;
			BoardStructureHooks.DeserializeSave((IPlacementBoardStructure)(object)this, consoleLogicSaveData.Board);
			UpdateLabelText();
		}
	}

	private void UpdateLabelText()
	{
		string text = RawName;
		if (string.IsNullOrEmpty(text))
		{
			text = ((Thing)this).DisplayName;
		}
		if (text.EndsWith("*"))
		{
			text = "";
		}
		TextQueue.RenderText((Thing)(object)this, _textTexture, text);
	}

	public override void SerializeOnJoin(RocketBinaryWriter writer)
	{
		((LogicUnitBase)this).SerializeOnJoin(writer);
		BoardStructureHooks.SerializeOnJoin(writer, (IPlacementBoardStructure)(object)this);
	}

	public override void DeserializeOnJoin(RocketBinaryReader reader)
	{
		((LogicUnitBase)this).DeserializeOnJoin(reader);
		BoardStructureHooks.DeserializeOnJoin(reader, (IPlacementBoardStructure)(object)this);
	}

	public override bool CanLogicRead(LogicType logicType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if ((int)logicType != 12)
		{
			return ((Device)this).CanLogicRead(logicType);
		}
		return true;
	}

	public override double GetLogicValue(LogicType logicType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if ((int)logicType != 12)
		{
			return ((Device)this).GetLogicValue(logicType);
		}
		return ((LogicUnitBase)this).Setting;
	}

	public override DelayedActionInstance InteractWith(Interactable interactable, Interaction interaction, bool doAction = true)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Invalid comparison between Unknown and I4
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		DelayedActionInstance val = new DelayedActionInstance
		{
			Duration = 0f,
			ActionMessage = interactable.ContextualName
		};
		if (((Thing)this).IsLocked)
		{
			return val.Fail(GameStrings.DeviceLocked.AsColor("red"));
		}
		if ((int)interactable.Action == 41)
		{
			if (((Thing)this).Activate == 1)
			{
				return val.Fail(GameStrings.GlobalAlreadyInUse);
			}
			if (!((Thing)this).IsAuthorized(((Interaction)(ref interaction)).SourceThing))
			{
				return val.Fail(GameStrings.AccessCardUnableToInteract);
			}
			if (!doAction)
			{
				return val.Succeed();
			}
			OnServer.Interact(interactable, 1, false);
			Event obj = this.ButtonPress;
			if (obj != null)
			{
				obj.Invoke();
			}
			return val.Succeed();
		}
		return ((LogicUnitBase)this).InteractWith(interactable, interaction, doAction);
	}

	public override DelayedActionInstance AttackWith(Attack attack, bool doAction = true)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		DynamicThing sourceItem = ((Attack)(ref attack)).SourceItem;
		Screwdriver val = (Screwdriver)(object)((sourceItem is Screwdriver) ? sourceItem : null);
		if (val != null)
		{
			return BoardRelocateHooks.StructureAttackWith((IPlacementBoardRelocatable)(object)this, attack, doAction, BoardRelocateHooks.NormalToolRelocateContinue((DynamicThing)(object)val));
		}
		return ((Device)this).AttackWith(attack, doAction);
	}

	public void OnStructureRelocated()
	{
	}

	Structure IPlacementBoardRelocatable.get_AsStructure()
	{
		return ((Thing)this).AsStructure;
	}

	string IPlacementBoardStructure.get_name()
	{
		return ((Object)this).name;
	}

	void IPlacementBoardStructure.SetStructureData(Quaternion localRotation, ulong ownerClientId, Grid3 localGrid, int customColourIndex)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		((Structure)this).SetStructureData(localRotation, ownerClientId, localGrid, customColourIndex);
	}
}
public static class StructureExtensions
{
	public static void InitializeEmissive(this Structure structure, MeshRenderer emissiveRenderer)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Expected O, but got Unknown
		if ((Object)(object)emissiveRenderer != (Object)null)
		{
			((Renderer)emissiveRenderer).material = new Material(((Renderer)emissiveRenderer).material);
		}
		if (((Thing)structure).HasColorState)
		{
			((Thing)structure).ColorState = ((Thing)structure).InteractColor.State;
		}
	}

	public static void SetEmissive(this Structure structure, MeshRenderer emissiveRenderer, bool emissive)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)emissiveRenderer == (Object)null) && !((Object)(object)((Renderer)emissiveRenderer).material == (Object)null))
		{
			Material material = ((Renderer)emissiveRenderer).material;
			Color val = (material.color = ((Thing)structure).CustomColor.Light);
			if (emissive)
			{
				material.SetColor("_EmissionColor", val);
				material.EnableKeyword("_EMISSION");
			}
			else
			{
				material.SetColor("_EmissionColor", Color.black);
				material.DisableKeyword("_EMISSION");
			}
		}
	}

	public static void SyncMaterials(this Structure structure, MeshRenderer emissiveRenderer, bool poweredOnly = false)
	{
		((Thing)structure).SetCustomColor(((Thing)structure).ColorState, ((Thing)structure).Powered && (!((Thing)structure).HasOnOffState || ((Thing)structure).OnOff || poweredOnly) && (Object)(object)emissiveRenderer != (Object)null);
	}

	public static void InitializeText(this Structure structure, MeshRenderer meshRenderer, int materialLabelIndex, ref RenderTexture textTexture, int texWidth, int texHeight)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		if (!((Object)(object)meshRenderer == (Object)null) && materialLabelIndex < ((Renderer)meshRenderer).materials.Length)
		{
			textTexture = new RenderTexture(texWidth, texHeight, 0);
			((Renderer)meshRenderer).materials[materialLabelIndex].mainTexture = (Texture)(object)textTexture;
		}
	}
}
public enum SwitchColorState
{
	Undefined,
	Off,
	On,
	OnPowered,
	Error,
	OffPowered
}
public class SwitchOnOff : SwitchOnOff
{
}
public class Wireframe : Wireframe
{
}
