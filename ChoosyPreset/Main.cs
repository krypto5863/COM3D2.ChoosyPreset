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

//These two lines tell your plugin to not give a flying fuck about accessing private variables/classes whatever. It requires a publicized stubb of the library with those private objects though.
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace ChoosyPreset
{
	//This is the metadata set for your plugin.
	[BepInPlugin("ChoosyPreset", "ChoosyPreset", "3.0")]
	[BepInDependency("net.perdition.com3d2.editbodyloadfix", BepInDependency.DependencyFlags.SoftDependency)]
	public class Main : BaseUnityPlugin
	{
		//static saving of the main instance. This makes it easier to run stuff like coroutines from static methods or accessing non-static vars.
		public static Main @this;

		//Static var for the logger so you can log from other classes.
		public static ManualLogSource logger;

		//Config entry variable. You set your configs to this.
		internal static ConfigEntry<string> LanguageFile;

		//internal static ConfigEntry<bool> UsingUniLib;

		public static bool PresetPanelOpen { get; private set; }
		public static bool ViewMode { get; private set; }

		internal static Dictionary<string, string> Translations;

		internal static bool UniLibInit = false;

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

			/*
			UsingUniLib = Config.Bind("General", "Use UniLib (Not Recommended)", false, new ConfigDescription("Will use UniLib and a UniLib UI. UniLib has been known to cause issues, so it's suggested you keep this off."));

			UsingUniLib.SettingChanged += (d, r) =>
			{
				try
				{
					if (UsingUniLib.Value == false)
					{
						ToggleUIState(false);
						return;
					}

					if (Main.UniLibInit == false)
					{
						Type.GetType("ChoosyPreset.Main_UniLib").GetMethod("Start").Invoke(null, null);
					}

					if (PresetPanelOpen && ViewMode == false)
					{
						ToggleUIState(true);
					}
				}
				catch (TypeLoadException t)
				{
					UsingUniLib.Value = false;
				}
			};
			*/

			var translationFiles = Directory.GetFiles(BepInEx.Paths.ConfigPath + $"\\ChoosyPreset\\", "*.*")
				.Where(s => s.ToLower().EndsWith(".json"))
				.Select(file => Path.GetFileName(file))
				.ToArray();

			if (translationFiles.Count() <= 0)
			{
				logger.LogFatal("It seems we're lacking any translation files for ChoosyPreset! This is bad and we can't start without them! Please download the translation files, they come with the plugin, and place them in the proper directory!");

				return;
			}

			AcceptableValueList<string> settingTransFiles = new AcceptableValueList<string>(translationFiles);

			LanguageFile = Config.Bind("General", "Language File", "english.json", new ConfigDescription("This denotes the translation file to use in ChoosyPreset.", settingTransFiles));

			LanguageFile.SettingChanged += (e, s) =>
			{
				Translations = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(BepInEx.Paths.ConfigPath + $"\\ChoosyPreset\\" + LanguageFile.Value));
				/*
				if (UsingUniLib.Value)
				{
					Type.GetType("ChoosyPreset.Main_UniLib").GetMethod("UpdateTranslations").Invoke(null, null);
				}*/
			};

			Translations = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(BepInEx.Paths.ConfigPath + $"\\ChoosyPreset\\" + LanguageFile.Value));
			/*
			if (UsingUniLib.Value)
			{
				try
				{
					Type.GetType("ChoosyPreset.Main_UniLib").GetMethod("Start").Invoke(null, null);
				}
				catch (TypeLoadException t)
				{
					logger.LogError("UniverseLib was removed! Reverting to default.");
					UsingUniLib.Value = false;
				}
			}*/

			//Installs the patches in the Main class.
			var harmony = Harmony.CreateAndPatchAll(typeof(Main));

			try
			{
				var methodBase = typeof(CharacterMgr)
					.GetMethods()
					.Where(r => r.Name.Equals("PresetSet"))
					.Aggregate((i1, i2) => i1.GetParameters().Count() < i2.GetParameters().Count() ? i1 : i2);

				Main.logger.LogDebug($"Choosy has found method: {methodBase.FullDescription()}");

				var prefix = new HarmonyMethod(typeof(Main), "PresetSet");
				var postfix = new HarmonyMethod(typeof(Main), "PresetColorFix");

				harmony.Patch(methodBase, prefix, postfix);
			}
			catch
			{
				Main.logger.LogFatal("Failed to patch the PresetSet function!");
				harmony.UnpatchSelf();
			}
		}

		private void OnGUI()
		{
			if (PresetPanelOpen && !ViewMode)
			{
				UIElements.IMGUIUI.ShowUI();
			}
		}

		[HarmonyPatch(typeof(PresetMgr), "OpenPresetPanel")]
		[HarmonyPatch(typeof(PresetMgr), "ClosePresetPanel")]
		[HarmonyPostfix]
		private static void PresetPanelStatusChanged(ref PresetMgr __instance)
		{
			PresetPanelOpen = __instance.m_goPresetPanel.activeSelf;

			if (PresetPanelOpen && ViewMode == false)
			{
				ToggleUIState(true);
				//myGUI.Enabled = true;
			}
			else
			{
				ToggleUIState(false);
				//myGUI.Enabled = false;
			}
		}

		[HarmonyPatch(typeof(SceneEdit), "FromView")]
		[HarmonyPostfix]
		private static void FromView()
		{
			ViewMode = false;

			if (PresetPanelOpen)
			{
				ToggleUIState(true);
				//myGUI.Enabled = true;
			}
			else
			{
				ToggleUIState(false);
				//myGUI.Enabled = false;
			}
		}

		[HarmonyPatch(typeof(SceneEdit), "ToView")]
		[HarmonyPostfix]
		private static void ToView()
		{
			ViewMode = true;
			ToggleUIState(false);
			//myGUI.Enabled = false;
		}

		[HarmonyPatch(typeof(SceneEdit), "OnDestroy")]
		[HarmonyPrefix]
		private static void ExitingEditMode()
		{
			PresetPanelOpen = false;
			ViewMode = false;
			ToggleUIState(false);
			//myGUI.Enabled = false;
		}

		private static Dictionary<MaidParts.PARTS_COLOR, MaidParts.PartsColor> MaidColorsToKeepDic = new Dictionary<MaidParts.PARTS_COLOR, MaidParts.PartsColor>();
		private static List<MaidProp> listofProps;

		//[HarmonyPatch(typeof(CharacterMgr), "PresetSet", new Type[] { typeof(Maid), typeof(CharacterMgr.Preset) })]
		//[HarmonyPrefix]
		private static bool PresetSet(Maid __0, ref CharacterMgr.Preset __1)
		{
			if (PresetPanelOpen == false)
			{
				return true;
			}

			MaidColorsToKeepDic.Clear();
			listofProps = new List<MaidProp>(__1.listMprop);

			var props = __1.listMprop.ToArray();

			if (__1.aryPartsColor != null)
			{
				for (int k = 0; k < __1.aryPartsColor.Length; k++)
				{
					var colorName = Enum.GetName(typeof(MaidParts.PARTS_COLOR), k);

					if (!ItemStates.CurrentItemState.MPNStates[colorName])
					{
						MaidColorsToKeepDic[(MaidParts.PARTS_COLOR)k] = __0.Parts.GetPartsColor((MaidParts.PARTS_COLOR)k);
					}
				}
			}

			foreach (MaidProp part in props)
			{
				var MPN = (MPN)part.idx;

				if (!ItemStates.CurrentItemState.MPNStates[MPN.ToString()])
				{
					__1.listMprop.Remove(part);
				}
			}

			if (!ItemStates.CurrentItemState.MPNStates["AddModsSlider Settings"])
			{
				__1.strFileName = "";
			}

			return true;
		}

		//[HarmonyPatch(typeof(CharacterMgr), "PresetSet", new Type[] { typeof(Maid), typeof(CharacterMgr.Preset) }, new ArgumentType[] {  })]
		//[HarmonyPostfix]
		private static void PresetColorFix(Maid __0, ref CharacterMgr.Preset __1)
		{
			if (PresetPanelOpen == false)
			{
				return;
			}

			foreach (KeyValuePair<MaidParts.PARTS_COLOR, MaidParts.PartsColor> keyValue in MaidColorsToKeepDic)
			{
				__0.Parts.SetPartsColor(keyValue.Key, keyValue.Value);
			}

			__1.listMprop = new List<MaidProp>(listofProps);

			listofProps = null;
		}

		private static void ToggleUIState(bool enabled)
		{
			try
			{
				//Main_UniLib.UIState = enabled;
			}
			catch (TypeLoadException t)
			{
			}
			catch
			{
			}
		}
	}
}