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
	[BepInPlugin(Guid, Name, Version)]
	[BepInDependency("net.perdition.com3d2.editbodyloadfix", BepInDependency.DependencyFlags.SoftDependency)]
	public class ChoosyPreset : BaseUnityPlugin
	{
		public const string Guid = "org.krypto5863.com3d2.choosypreset";
		public const string Name = "ChoosyPreset";
		public const string Version = "3.1.1";

		//static saving of the main instance. This makes it easier to run stuff like coroutines from static methods or accessing non-static vars.
		public static ChoosyPreset Instance;

		//Static var for the Logger so you can log from other classes.
		public new static ManualLogSource Logger;

		//Config entry variable. You set your configs to Instance.
		internal static ConfigEntry<string> LanguageFile;

		//internal static ConfigEntry<bool> UsingUniLib;

		public static bool PresetPanelOpen { get; private set; }
		public static bool ViewMode { get; private set; }

		internal static Dictionary<string, string> Translations;

		//internal static bool UniLibInit = false;

		private void Awake()
		{
			//Useful for engaging coroutines or accessing variables non-static variables. Completely optional though.
			Instance = this;

			//pushes the Logger to a public static var so you can use the bepinex Logger from other classes.
			Logger = base.Logger;

			if (!Directory.Exists(Paths.ConfigPath + @"\ChoosyPreset"))
			{
				Directory.CreateDirectory(Paths.ConfigPath + @"\ChoosyPreset");
			}

			/*
			UsingUniLib = Config.Bind("General", "Use UniLib (Not Recommended)", false, new ConfigDescription("Will use UniLib and a UniLib UI. UniLib has been known to cause issues, so it's suggested you keep Instance off."));

			UsingUniLib.SettingChanged += (d, r) =>
			{
				try
				{
					if (UsingUniLib.Value == false)
					{
						ToggleUIState(false);
						return;
					}

					if (ChoosyPreset.UniLibInit == false)
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

			var translationFiles = Directory.GetFiles(Paths.ConfigPath + @"\ChoosyPreset\", "*.*")
				.Where(s => s.ToLower().EndsWith(".json"))
				.Select(Path.GetFileName)
				.ToArray();

			if (!translationFiles.Any())
			{
				Logger.LogFatal("It seems we're lacking any translation files for ChoosyPreset! This is bad and we can't start without them! Please download the translation files, they come with the plugin, and place them in the proper directory!");

				return;
			}

			var settingTransFiles = new AcceptableValueList<string>(translationFiles);

			LanguageFile = Config.Bind("General", "Language File", "english.json", new ConfigDescription("This denotes the translation file to use in ChoosyPreset.", settingTransFiles));

			LanguageFile.SettingChanged += (e, s) =>
			{
				Translations = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(
					$@"{Paths.ConfigPath}\ChoosyPreset\{LanguageFile.Value}"));
				/*
				if (UsingUniLib.Value)
				{
					Type.GetType("ChoosyPreset.Main_UniLib").GetMethod("UpdateTranslations").Invoke(null, null);
				}*/
			};

			Translations = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(
				$@"{Paths.ConfigPath}\ChoosyPreset\{LanguageFile.Value}"));
			/*
			if (UsingUniLib.Value)
			{
				try
				{
					Type.GetType("ChoosyPreset.Main_UniLib").GetMethod("Start").Invoke(null, null);
				}
				catch (TypeLoadException t)
				{
					Logger.LogError("UniverseLib was removed! Reverting to default.");
					UsingUniLib.Value = false;
				}
			}*/

			//Installs the patches in the ChoosyPreset class.
			var harmony = Harmony.CreateAndPatchAll(typeof(ChoosyPreset));
			try
			{
				var methodBase = typeof(CharacterMgr)
					.GetMethods()
					.Where(r => r.Name.Equals(nameof(CharacterMgr.PresetSet)))
					.Aggregate((i1, i2) => i1.GetParameters().Length < i2.GetParameters().Length ? i1 : i2);

				Logger.LogDebug($"Choosy has found method: {methodBase.FullDescription()}");

				var prefix = new HarmonyMethod(typeof(ChoosyPreset), nameof(PresetSet));
				var postfix = new HarmonyMethod(typeof(ChoosyPreset), nameof(PresetColorFix));

				harmony.Patch(methodBase, prefix, postfix);
			}
			catch
			{
				Logger.LogFatal("Failed to patch the PresetSet function!");
				harmony.UnpatchSelf();
			}
		}

		private void OnGUI()
		{
			if (PresetPanelOpen && !ViewMode)
			{
				UIElements.IMGUIUI.ShowUi();
			}
		}

		[HarmonyPatch(typeof(PresetMgr), nameof(PresetMgr.OpenPresetPanel))]
		[HarmonyPatch(typeof(PresetMgr), nameof(PresetMgr.ClosePresetPanel))]
		[HarmonyPostfix]
		private static void PresetPanelStatusChanged(ref PresetMgr __instance)
		{
			PresetPanelOpen = __instance.m_goPresetPanel.activeSelf;

			/*
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
			*/
		}

		[HarmonyPatch(typeof(SceneEdit), nameof(SceneEdit.FromView))]
		[HarmonyPostfix]
		private static void FromView()
		{
			ViewMode = false;

			/*
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
			*/
		}

		[HarmonyPatch(typeof(SceneEdit), nameof(SceneEdit.ToView))]
		[HarmonyPostfix]
		private static void ToView()
		{
			ViewMode = true;
			//ToggleUIState(false);
			//myGUI.Enabled = false;
		}

		[HarmonyPatch(typeof(SceneEdit), nameof(SceneEdit.OnDestroy))]
		[HarmonyPrefix]
		private static void ExitingEditMode()
		{
			PresetPanelOpen = false;
			ViewMode = false;
			//ToggleUIState(false);
			//myGUI.Enabled = false;
		}

		
		private static readonly Dictionary<MaidParts.PARTS_COLOR, MaidParts.PartsColor> MaidColorsToKeepDic = new Dictionary<MaidParts.PARTS_COLOR, MaidParts.PartsColor>();
		private static List<MaidProp> _listOfProps;
		private static string _presetFileName;

		//[HarmonyPatch(typeof(CharacterMgr), nameof(CharacterMgr.PresetSet))]
		//[HarmonyPrefix]
		private static bool PresetSet(Maid __0, ref CharacterMgr.Preset __1)
		{
			if (PresetPanelOpen == false)
			{
				return true;
			}

			MaidColorsToKeepDic.Clear();
			_listOfProps = new List<MaidProp>(__1.listMprop);
			_presetFileName = __1.strFileName;

			var props = __1.listMprop.ToArray();

			if (__1.aryPartsColor != null)
			{
				for (var k = 0; k < __1.aryPartsColor.Length; k++)
				{
					var colorName = Enum.GetName(typeof(MaidParts.PARTS_COLOR), k);

					if (!ItemStates.CurrentItemState.MpnStates[colorName])
					{
						MaidColorsToKeepDic[(MaidParts.PARTS_COLOR)k] = __0.Parts.GetPartsColor((MaidParts.PARTS_COLOR)k);
					}
				}
			}

			foreach (var part in props)
			{
				var mpn = (MPN)part.idx;

				if (!ItemStates.CurrentItemState.MpnStates[mpn.ToString()])
				{
					__1.listMprop.Remove(part);
				}
			}

			if (!ItemStates.CurrentItemState.MpnStates["AddModsSlider Settings"])
			{
				__1.strFileName = "";
			}

			return true;
		}

		//[HarmonyPatch(typeof(CharacterMgr), nameof(CharacterMgr.PresetSet))]
		//[HarmonyPostfix]
		private static void PresetColorFix(Maid __0, ref CharacterMgr.Preset __1)
		{
			if (PresetPanelOpen == false)
			{
				return;
			}

			foreach (var keyValue in MaidColorsToKeepDic)
			{
				__0.Parts.SetPartsColor(keyValue.Key, keyValue.Value);
			}

			__1.listMprop = new List<MaidProp>(_listOfProps);
			// avoid restoring to an empty string, in case an earlier patch has done the same
			if (_presetFileName != string.Empty)
			{
				__1.strFileName = _presetFileName;
			}

			_listOfProps = null;
		}
		
		/*
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
		*/
	}
}
