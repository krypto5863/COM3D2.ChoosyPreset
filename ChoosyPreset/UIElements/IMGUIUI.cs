using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ChoosyPreset.UIElements
{
	class IMGUIUI
	{
		private static readonly int WindowID = 387462387;
		private static Vector2 scrollPosition = Vector2.zero;

		private static Rect windowRect = new Rect(Screen.width / 1.25f, Screen.height / 8, Math.Max(Screen.width / 6f, 300f), Math.Max(Screen.height / 1.5f, 600f));

		private static bool DoOnce = false;

		private static Dictionary<string, bool> ExpandedCategory = new Dictionary<string, bool>();

		private static GUIStyle MainWindow;
		private static GUIStyle Sections;
		private static GUIStyle Sections2;

		private static GUIStyle MixedText;
		private static GUIStyle OffText;
		private static GUIStyle OnText;
		private static GUIStyle MPNText;

		private static string SearchInput = String.Empty;

		public static void ShowUI()
		{
			if (DoOnce == false) 
			{
				new ItemStates();

				MainWindow = new GUIStyle(GUI.skin.window);
				MainWindow.normal.background = UIElements.Helpers.MakeWindowTex(new Color(0.01f, 0.01f, 0.01f, 0.3f), new Color(0, 1, 0, 0.5f));
				MainWindow.normal.textColor = new Color(1, 1, 1, 0.3f);
				MainWindow.hover.background = UIElements.Helpers.MakeWindowTex(new Color(0.01f, 0.01f, 0.01f, 0.6f), new Color(0, 1, 0, 0.5f));
				MainWindow.hover.textColor = new Color(1, 1, 1, 0.6f);
				MainWindow.onNormal.background = UIElements.Helpers.MakeWindowTex(new Color(0.01f, 0.01f, 0.01f, 0.6f), new Color(0, 1, 0, 0.5f));

				Sections = new GUIStyle(GUI.skin.box);
				Sections.normal.background = UIElements.Helpers.MakeTex(2, 2, new Color(0, 0, 0, 0.3f));
				Sections2 = new GUIStyle(GUI.skin.box);
				Sections2.normal.background = UIElements.Helpers.MakeTexWithRoundedCorner(new Color(0, 0, 0, 0.6f));

				MixedText = new GUIStyle(GUI.skin.label);
				MixedText.normal.textColor = Color.yellow;

				OffText = new GUIStyle(GUI.skin.label);
				OffText.normal.textColor = Color.red;

				OnText = new GUIStyle(GUI.skin.label);
				OnText.normal.textColor = Color.green;

				MPNText = new GUIStyle(GUI.skin.toggle);
				MPNText.richText = true;

				DoOnce = true;
			}

			windowRect = GUI.Window(WindowID, windowRect, GuiWindowControls, "ChoosyPreset", MainWindow);
		}

		static void GuiWindowControls(int windowID)
		{
			GUI.DragWindow(new Rect(0, 0, 10000, 20));

			GUILayout.BeginHorizontal();
			GUILayout.Label(Main.Translations["SearchText1"]);
			SearchInput = GUILayout.TextField(SearchInput, GUILayout.Width(160));
			GUILayout.EndHorizontal();

			scrollPosition = GUILayout.BeginScrollView(scrollPosition);

			foreach (var kv in ItemStates.Categorized) 
			{
				if (!String.IsNullOrEmpty(SearchInput))
				{
					if (!Main.Translations[kv.Key].ToLower().Contains(SearchInput.ToLower()) && !kv.Key.ToLower().Contains(SearchInput.ToLower())) 
					{
						var result = false;

						foreach (string mpn in kv.Value) 
						{
							if (Main.Translations[mpn].ToLower().Contains(SearchInput.ToLower()) || mpn.ToLower().Contains(SearchInput.ToLower()))
							{
								result = true;
								continue;
							}
						}

						if (result == false) 
						{
							continue;
						}
					}
				}

				var AnyOn = ItemStates.CurrentItemState.IsAnyMPNOn(kv.Key);
				var AnyOff = ItemStates.CurrentItemState.IsAnyMPNOff(kv.Key);

				GUILayout.BeginVertical(Sections);

				GUILayout.BeginHorizontal(Sections);

				if (GUILayout.Button("☰")) 
				{
					if (ExpandedCategory.TryGetValue(kv.Key, out var val))
					{
						ExpandedCategory[kv.Key] = !ExpandedCategory[kv.Key];
					}
					else 
					{
						ExpandedCategory[kv.Key] = true;
					}
				}

				if (GUILayout.Button("I/O"))
				{
					if (AnyOn)
					{
						foreach (string mpn in kv.Value) 
						{
							ItemStates.CurrentItemState.MPNStates[mpn] = false;
						}
					}
					else 
					{
						foreach (string mpn in kv.Value)
						{
							ItemStates.CurrentItemState.MPNStates[mpn] = true;
						}
					}
				}

				GUILayout.Label(Main.Translations[kv.Key] + $":");

				GUILayout.FlexibleSpace();

				string status = AnyOn && AnyOff ? Main.Translations["Mixed"]
					: AnyOff ? Main.Translations["Off"] : Main.Translations["On"];

				var status1 = AnyOn && AnyOff ? MixedText
					: AnyOff ? OffText : OnText;

				GUILayout.Label(status, status1);

				GUILayout.EndHorizontal();
				if (ExpandedCategory.TryGetValue(kv.Key, out var val1) && val1 == true) 
				{
					GUILayout.BeginVertical(Sections2);

					foreach (var mpn in kv.Value.OrderBy(tn => Main.Translations[tn]))
					{

						if (!String.IsNullOrEmpty(SearchInput) && !Main.Translations[mpn].ToLower().Contains(SearchInput.ToLower()) && !mpn.ToLower().Contains(SearchInput.ToLower()))
						{
							continue;
						}

						ItemStates.CurrentItemState.MPNStates[mpn] = GUILayout.Toggle(ItemStates.CurrentItemState.MPNStates[mpn], Main.Translations[mpn] + $" <i>(<color=#808080ff>{mpn}</color>)</i>", MPNText);
					}

					GUILayout.EndVertical();
				}

				GUILayout.EndVertical();
			}

			GUILayout.EndScrollView();

			GUILayout.BeginHorizontal(Sections);

			if (GUILayout.Button(Main.Translations["Enable All"]))
			{
				foreach (var kv in ItemStates.Categorized) 
				{
					foreach (var cat in kv.Value) 
					{
						ItemStates.CurrentItemState.MPNStates[cat] = true;
					}
				}
			}
			if (GUILayout.Button(Main.Translations["Disable All"]))
			{
				foreach (var kv in ItemStates.Categorized)
				{
					foreach (var cat in kv.Value)
					{
						ItemStates.CurrentItemState.MPNStates[cat] = false;
					}
				}
			}

			GUILayout.EndHorizontal();

			Helpers.ChkMouseClick(windowRect);
		}
	}
}