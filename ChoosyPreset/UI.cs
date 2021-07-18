using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ChoosyPreset
{
	class UI
	{
		private static readonly int WindowID = 777777;
		private static Vector2 scrollPosition = Vector2.zero;

		private static Rect windowRect = new Rect(Screen.width / 1.3f, Screen.height / 4, Screen.width / 7f, Screen.height / 2f);

		internal static Dictionary<string, bool> ButtonsMPN = new Dictionary<string, bool>();
		internal static Dictionary<MaidParts.PARTS_COLOR, bool> ColorParts = new Dictionary<MaidParts.PARTS_COLOR, bool>();

		internal static Dictionary<string, bool> SimpleModeToggles = new Dictionary<string, bool>();

		internal static readonly Dictionary<string, string[]> Categories = new Dictionary<string, string[]> 
		{ 
			{"Clothes", new string[]{"wear","skirt","mizugi","bra","panz","stkg","shoes","headset","onepiece", "acchat"} },

			{"Accesories", new string[]{"accha","acchana","acckamisub","acckami","accmimi","accnip","acckubi","acckubiwa","accheso", "accude", "accude", "accashi", "accsenaka", "accshippo", "accanl", "accvag", "megane", "accxxx", "acchead", "glove" } },

			{"Body", new string[]{"munel","munes","munetare","regfat","arml","hara","regmeet","kubiscl","udescl", "body", "muneupdown", "muneyori", "muneyawaraka", "douper", "sintyou", "koshi", "kata", "west", "skin", "acctatoo", "accnail", "underhair", "chikubi", "chikubicolor", "folder_underhair", "folder_skin"} },

			{"Face", new string[]{ "eyescl", "eyesclx", "eyescly", "eyeposx", "eyeposy", "eyeclose", "eyeballposx", "eyeballposy", "eyeballsclx", "eyeballscly", "earnone", "earelf", "earrot", "earscl", "nosepos", "nosescl", "faceshape", "faceshapeslim", "mayushapein", "mayushapeout", "mayux", "mayuy", "mayurot", "headx", "heady", "mayuthick", "mayulong", "yorime", "mabutaupin", "mabutaupin2", "mabutaupmiddle", "mabutaupout", "mabutaupout2", "mabutalowin", "mabutalowupmiddle", "mabutalowupout", "head", "hokuro", "mayu", "lip", "eye", "eye_hi", "eye_hi_r", "eyewhite", "nose", "facegloss", "matsuge_up", "matsuge_low", "futae", "folder_eye", "folder_eyewhite", "folder_mayu", "folder_matsuge_up", "folder_matsuge_low", "folder_futae", "kousoku_upper", "kousoku_lower" } },

			{"Hair", new string[]{"hairf","hairr","hairt","hairr","hairs","hairaho", "haircolor" } },

			{"Custom Hair Colors", new string[]{"hair", "eye_brow", "under_hair", "hair_outline", "matsuge_up", "matsuge_low", "futae"} },

			{"Custom Eye Colors", new string[]{"eye_l", "eye_r", "eye_white"} },

			{"Custom Body Colors", new string[]{"skin", "nipple", "skin_outline"} },
		};

		internal static bool SkipMaidVoiceXML = true;

		public static void ShowUI() 
		{
			windowRect = GUILayout.Window(WindowID, windowRect, GuiWindowControls, "ChoosyPreset");
		}

		static void GuiWindowControls(int windowID)
		{
			GUI.DragWindow(new Rect(0, 0, 10000, 20));

			scrollPosition = GUILayout.BeginScrollView(scrollPosition);


			if (Main.AdvancedMode.Value)
			{
				ShowAdvancedMode();
			}
			else 
			{
				ShowSimpleMode();
			}

			SkipMaidVoiceXML = GUILayout.Toggle(SkipMaidVoiceXML, "AddModsSlider Settings");

			GUILayout.EndScrollView();

			if (GUILayout.Button("Disable All"))
			{
				var keys = ButtonsMPN.Keys.ToArray();

				foreach (string key in keys)
				{
					ButtonsMPN[key] = false;
				}

				var keys2 = ColorParts.Keys.ToArray();

				foreach (MaidParts.PARTS_COLOR key in keys2)
				{
					ColorParts[key] = false;
				}

				var keys3 = SimpleModeToggles.Keys.ToArray();

				foreach (string key in keys3)
				{
					SimpleModeToggles[key] = false;
				}

				SkipMaidVoiceXML = false;
			}
			if (GUILayout.Button("Enable All"))
			{
				var keys = ButtonsMPN.Keys.ToArray();

				foreach (string key in keys)
				{
					ButtonsMPN[key] = true;
				}

				var keys2 = ColorParts.Keys.ToArray();

				foreach (MaidParts.PARTS_COLOR key in keys2)
				{
					ColorParts[key] = true;
				}

				var keys3 = SimpleModeToggles.Keys.ToArray();

				foreach (string key in keys3)
				{
					SimpleModeToggles[key] = true;
				}

				SkipMaidVoiceXML = true;
			}

			Main.AdvancedMode.Value = GUILayout.Toggle(Main.AdvancedMode.Value, "Advanced Mode");

			ChkMouseClick(windowRect);
		}
		internal static void ShowSimpleMode() 
		{
			foreach (string s in UI.Categories.Keys) 
			{
				SimpleModeToggles[s] = GUILayout.Toggle(SimpleModeToggles[s], s);
			}
		}
		internal static void ShowAdvancedMode() 
		{
			GUILayout.BeginVertical();

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("Slots");
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			foreach (string s in Enum.GetNames(typeof(MPN)))
			{
				ButtonsMPN[s] = GUILayout.Toggle(ButtonsMPN[s], s);
			}

			GUILayout.EndVertical();
			GUILayout.BeginVertical();

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("Colors");
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			foreach (string s in Enum.GetNames(typeof(MaidParts.PARTS_COLOR)))
			{
				ColorParts[(MaidParts.PARTS_COLOR)Enum.Parse(typeof(MaidParts.PARTS_COLOR), s)] = GUILayout.Toggle(ColorParts[(MaidParts.PARTS_COLOR)Enum.Parse(typeof(MaidParts.PARTS_COLOR), s)], s);
			}

			GUILayout.EndVertical();
		}


		public static void ChkMouseClick(Rect windowRect)
		{
			if ((Input.mouseScrollDelta.y != 0 || Input.GetMouseButtonUp(0)) && IsMouseOnGUI(windowRect))
			{
				Input.ResetInputAxes();
			}
		}
		public static bool IsMouseOnGUI(Rect windowRect)
		{
			Vector2 point = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
			return windowRect.Contains(point);
		}
	}
}
