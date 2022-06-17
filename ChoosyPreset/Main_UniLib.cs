using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UniverseLib.UI;

namespace ChoosyPreset
{
	internal static class Main_UniLib
	{

		internal static UIBase uIBase;
		internal static UIElements.MyGUI myGUI;

		public static bool UIState 
		{
			get => myGUI.Enabled;
			set => myGUI.Enabled = value;
		}

		public static void Start()
		{
			var UniLibConfig = new UniverseLib.Config.UniverseLibConfig() { Force_Unlock_Mouse = true, Allow_UI_Selection_Outside_UIBase = true };

			UniverseLib.Universe.Init(1f, UniverseLib_Init, LogHandler, UniLibConfig);

			Main.UniLibInit = true;

			Main.logger.LogMessage("Calling UNiLib");
		}

		private static void UniverseLib_Init()
		{
			uIBase = UniversalUI.RegisterUI("ChoosyPresetUI", null);
			myGUI = new UIElements.MyGUI(uIBase);
			myGUI.Enabled = false;

			Main.logger.LogMessage("Init uni lib.");
		}
		private static void LogHandler(string log, LogType logType)
		{
			return;
		}

		public static void UpdateTranslations()
		{
			myGUI.UpdateTranslations();
		}
	}
}
