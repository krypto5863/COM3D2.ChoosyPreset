using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using System.Text;
using UnityEngine;

//These two lines tell your plugin to not give a flying fuck about accessing private variables/classes whatever. It requires a publicized stubb of the library with those private objects though.
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace ChoosyPreset
{
	//This is the metadata set for your plugin.
	[BepInPlugin("ChoosyPreset", "ChoosyPreset", "1.1")]
	public class Main : BaseUnityPlugin
	{
		//static saving of the main instance. This makes it easier to run stuff like coroutines from static methods or accessing non-static vars.
		public static Main @this;

		//Static var for the logger so you can log from other classes.
		public static ManualLogSource logger;

		//Config entry variable. You set your configs to this.
		internal static ConfigEntry<bool> AdvancedMode;
		public static bool PresetPanelOpen { get; private set; }
		public static bool ViewMode { get; private set; }

		private void Awake()
		{
			//Useful for engaging coroutines or accessing variables non-static variables. Completely optional though.
			@this = this;

			//pushes the logger to a public static var so you can use the bepinex logger from other classes.
			logger = Logger;

			//Binds the configuration. In other words it sets your ConfigEntry var to your config setup.
			AdvancedMode = Config.Bind("General", "Advanced Mode", false, "This mode lets you switch individual slots for your items. It's way more confusing than simple mode.");

			foreach (string s in Enum.GetNames(typeof(MPN)))
			{
				UI.ButtonsMPN[s] = true;

#if DEBUG
				var KeyVal = UI.Categories.FirstOrDefault(kv => kv.Value.Contains(s.ToLower())).Key;
				if (KeyVal == null)
				{

					Main.logger.LogWarning($"{s} falls out of simple scope...");
				}
#endif
			}
			foreach (string s in Enum.GetNames(typeof(MaidParts.PARTS_COLOR)))
			{
				UI.ColorParts[(MaidParts.PARTS_COLOR)Enum.Parse(typeof(MaidParts.PARTS_COLOR), s)] = true;
			}

			foreach (string key in UI.Categories.Keys)
			{
				UI.SimpleModeToggles[key] = true;
			}

			//Installs the patches in the Main class.
			Harmony.CreateAndPatchAll(typeof(Main));
		}

		private void OnGUI()
		{
			if (PresetPanelOpen && !ViewMode)
			{
				UI.ShowUI();
			}
		}

		[HarmonyPatch(typeof(PresetMgr), "OpenPresetPanel")]
		[HarmonyPatch(typeof(PresetMgr), "ClosePresetPanel")]
		[HarmonyPostfix]
		static void PresetPanelStatusChanged(ref PresetMgr __instance)
		{
			PresetPanelOpen = __instance.m_goPresetPanel.activeSelf;
		}

		[HarmonyPatch(typeof(SceneEdit), "FromView")]
		[HarmonyPostfix]
		static void FromView()
		{
			ViewMode = false;
		}
		[HarmonyPatch(typeof(SceneEdit), "ToView")]
		[HarmonyPostfix]
		static void ToView()
		{
			ViewMode = true;
		}

		[HarmonyPatch(typeof(SceneEdit), "OnDestroy")]
		[HarmonyPrefix]
		static void ExitingEditMode()
		{
			PresetPanelOpen = false;
			ViewMode = false;
		}

		private static Dictionary<MaidParts.PARTS_COLOR, MaidParts.PartsColor> MaidColorsToKeepDic;
		private static List<MaidProp> listofProps;

		[HarmonyPatch(typeof(CharacterMgr), "PresetSet", new Type[] { typeof(Maid), typeof(CharacterMgr.Preset) })]
		[HarmonyPrefix]
		private static bool PresetSet(Maid __0, ref CharacterMgr.Preset __1)
		{
			MaidColorsToKeepDic = new Dictionary<MaidParts.PARTS_COLOR, MaidParts.PartsColor>();
			listofProps = new List<MaidProp>(__1.listMprop);

			if (AdvancedMode.Value)
			{
				var props = __1.listMprop.ToArray();

				if (__1.aryPartsColor != null)
				{
					for (int k = 0; k < __1.aryPartsColor.Length; k++)
					{
						if (!UI.ColorParts[(MaidParts.PARTS_COLOR)k])
						{
							MaidColorsToKeepDic[(MaidParts.PARTS_COLOR)k] = __0.Parts.GetPartsColor((MaidParts.PARTS_COLOR)k);
						}
					}
				}

				foreach (MaidProp part in props)
				{
					var MPN = (MPN)part.idx;

					if (!UI.ButtonsMPN[MPN.ToString()])
					{
						__1.listMprop.Remove(part);
					}
				}
			}
			else
			{
				var props = __1.listMprop.ToArray();

				if (__1.aryPartsColor != null)
				{
					for (int k = 0; k < __1.aryPartsColor.Length; k++)
					{
						string PartsColor = ((MaidParts.PARTS_COLOR)k).ToString();

						var KeyVal = UI.Categories.FirstOrDefault(kv => kv.Value.Contains(PartsColor.ToLower())).Key;

						if (KeyVal != null && !UI.SimpleModeToggles[KeyVal])
						{
							MaidColorsToKeepDic[(MaidParts.PARTS_COLOR)k] = __0.Parts.GetPartsColor((MaidParts.PARTS_COLOR)k);
						}
					}
				}

				foreach (MaidProp part in props)
				{
					var MPN = ((MPN)part.idx).ToString();

					var KeyVal = UI.Categories.FirstOrDefault(kv => kv.Value.Contains(MPN.ToLower())).Key;

#if DEBUG
					Main.logger.LogDebug($"Logger returned: {KeyVal} for MPN {MPN}");
#endif

					if (KeyVal != null && !UI.SimpleModeToggles[KeyVal])
					{
#if DEBUG
						Main.logger.LogDebug($"Removing item in slot: {MPN} because category {KeyVal} is disabled and contains this slot...");
#endif

						__1.listMprop.Remove(part);
					}
				}
			}

			if (!UI.SkipMaidVoiceXML)
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
