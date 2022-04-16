using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using UniverseLib.UI;

//These two lines tell your plugin to not give a flying fuck about accessing private variables/classes whatever. It requires a publicized stubb of the library with those private objects though.
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace ChoosyPreset
{
	//This is the metadata set for your plugin.
	[BepInPlugin("ChoosyPreset", "ChoosyPreset", "1.1")]
	[BepInDependency("net.perdition.com3d2.editbodyloadfix", BepInDependency.DependencyFlags.SoftDependency)]
	public class Main : BaseUnityPlugin
	{
		//static saving of the main instance. This makes it easier to run stuff like coroutines from static methods or accessing non-static vars.
		public static Main @this;

		//Static var for the logger so you can log from other classes.
		public static ManualLogSource logger;

		//Config entry variable. You set your configs to this.
		internal static ConfigEntry<string> LanguageFile;

		public static bool PresetPanelOpen { get; private set; }
		public static bool ViewMode { get; private set; }

		internal static Dictionary<string, string> Translations;

		internal static UIBase uIBase;
		internal static MyGUI myGUI;

		private void Awake()
		{
			//Useful for engaging coroutines or accessing variables non-static variables. Completely optional though.
			@this = this;

			//pushes the logger to a public static var so you can use the bepinex logger from other classes.
			logger = Logger;

			if (!Directory.Exists(BepInEx.Paths.ConfigPath + $"\\ChoosyPreset"))
			{
				Directory.CreateDirectory(BepInEx.Paths.ConfigPath + $"\\ChoosyPreset");
			}

			AcceptableValueList<string> translationFiles = new AcceptableValueList<string>(Directory.GetFiles(BepInEx.Paths.ConfigPath + $"\\ChoosyPreset\\", "*.*").Where(s => s.ToLower().EndsWith(".json")).Select(file => Path.GetFileName(file)).ToArray());

			if (translationFiles.AcceptableValues.Count() <= 0)
			{
				logger.LogFatal("It seems we're lacking any translation files for ChoosyPreset! This is bad and we can't start without them! Please download the translation files, they come with the plugin, and place them in the proper directory!");

				return;
			}

			LanguageFile = Config.Bind("General", "Language File", "english.json", new ConfigDescription("This denotes the translation file to use in ChoosyPreset.", translationFiles));

			LanguageFile.SettingChanged += (e, s) =>
			{
				Translations = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(BepInEx.Paths.ConfigPath + $"\\ChoosyPreset\\" + LanguageFile.Value));

				myGUI.UpdateTranslations();
			};

			Translations = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(BepInEx.Paths.ConfigPath + $"\\ChoosyPreset\\" + LanguageFile.Value));

			var UniLibConfig = new UniverseLib.Config.UniverseLibConfig() { Force_Unlock_Mouse = true };

			UniverseLib.Universe.Init(0f, UniverseLib_Init, null, UniLibConfig);

			//Installs the patches in the Main class.
			Harmony.CreateAndPatchAll(typeof(Main));
		}

		private void UniverseLib_Init()
		{
			uIBase = UniversalUI.RegisterUI("ChoosyPresetUI", null);
			myGUI = new MyGUI(uIBase);
			myGUI.Enabled = false;
		}

		[HarmonyPatch(typeof(PresetMgr), "OpenPresetPanel")]
		[HarmonyPatch(typeof(PresetMgr), "ClosePresetPanel")]
		[HarmonyPostfix]
		private static void PresetPanelStatusChanged(ref PresetMgr __instance)
		{
			PresetPanelOpen = __instance.m_goPresetPanel.activeSelf;

			if (PresetPanelOpen && ViewMode == false)
			{
				myGUI.Enabled = true;
			}
			else
			{
				myGUI.Enabled = false;
			}
		}

		[HarmonyPatch(typeof(SceneEdit), "FromView")]
		[HarmonyPostfix]
		private static void FromView()
		{
			ViewMode = false;

			if (PresetPanelOpen)
			{
				myGUI.Enabled = true;
			}
			else
			{
				myGUI.Enabled = false;
			}
		}

		[HarmonyPatch(typeof(SceneEdit), "ToView")]
		[HarmonyPostfix]
		private static void ToView()
		{
			ViewMode = true;
			myGUI.Enabled = false;
		}

		[HarmonyPatch(typeof(SceneEdit), "OnDestroy")]
		[HarmonyPrefix]
		private static void ExitingEditMode()
		{
			PresetPanelOpen = false;
			ViewMode = false;
			myGUI.Enabled = false;
		}

		private static Dictionary<MaidParts.PARTS_COLOR, MaidParts.PartsColor> MaidColorsToKeepDic;
		private static List<MaidProp> listofProps;

		[HarmonyPatch(typeof(CharacterMgr), "PresetSet", new Type[] { typeof(Maid), typeof(CharacterMgr.Preset) })]
		[HarmonyPrefix]
		private static bool PresetSet(Maid __0, ref CharacterMgr.Preset __1)
		{
			MaidColorsToKeepDic = new Dictionary<MaidParts.PARTS_COLOR, MaidParts.PartsColor>();
			listofProps = new List<MaidProp>(__1.listMprop);

			var props = __1.listMprop.ToArray();

			if (__1.aryPartsColor != null)
			{
				for (int k = 0; k < __1.aryPartsColor.Length; k++)
				{
					var colorName = Enum.GetName(typeof(MaidParts.PARTS_COLOR), k);

					if (!myGUI.State.MPNStates[colorName])
					{
						MaidColorsToKeepDic[(MaidParts.PARTS_COLOR)k] = __0.Parts.GetPartsColor((MaidParts.PARTS_COLOR)k);
					}
				}
			}

			foreach (MaidProp part in props)
			{
				var MPN = (MPN)part.idx;

				if (!myGUI.State.MPNStates[MPN.ToString()])
				{
					__1.listMprop.Remove(part);
				}
			}

			if (!myGUI.State.MPNStates["AddModsSlider Settings"])
			{
				__1.strFileName = "";
			}

			return true;
		}

		[HarmonyPatch(typeof(CharacterMgr), "PresetSet", new Type[] { typeof(Maid), typeof(CharacterMgr.Preset) })]
		[HarmonyPostfix]
		private static void PresetColorFix(Maid __0, ref CharacterMgr.Preset __1)
		{
			foreach (KeyValuePair<MaidParts.PARTS_COLOR, MaidParts.PartsColor> keyValue in MaidColorsToKeepDic)
			{
				__0.Parts.SetPartsColor(keyValue.Key, keyValue.Value);
			}

			__1.listMprop = new List<MaidProp>(listofProps);

			listofProps = null;
			MaidColorsToKeepDic = null;
		}
	}
}