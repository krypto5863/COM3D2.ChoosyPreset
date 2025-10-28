using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ChoosyPreset.UIElements
{
    internal class IMGUIUI
    {
        private const int WindowId = 387462387;
        private static Vector2 _scrollPosition = Vector2.zero;

        private static Rect _windowRect = new Rect(Screen.width / 1.25f, Screen.height / 8, Math.Max(Screen.width / 6f, 300f), Math.Max(Screen.height / 1.5f, 600f));

        private static bool _doOnce;

        private static readonly Dictionary<string, bool> ExpandedCategory = new Dictionary<string, bool>();

        private static Texture2D _normalTexture;
        private static Texture2D _hoverTexture;
        private static Texture2D _onNormalTexture;
        private static Texture2D _sectionsTexture;
        private static Texture2D _sections2Texture;

        private static GUIStyle _mainWindow;
        private static GUIStyle _sections;
        private static GUIStyle _sections2;

        private static GUIStyle _mixedText;
        private static GUIStyle _offText;
        private static GUIStyle _onText;
        private static GUIStyle _mpnText;

        private static string _searchInput = string.Empty;

        public static void ShowUi()
        {
            if (_doOnce == false)
            {
                ItemStates.InitStates();

                _normalTexture = Helpers.MakeWindowTex(new Color(0.01f, 0.01f, 0.01f, 0.3f), new Color(0, 1, 0, 0.5f));
                _hoverTexture = Helpers.MakeWindowTex(new Color(0.01f, 0.01f, 0.01f, 0.6f), new Color(0, 1, 0, 0.5f));
                _onNormalTexture = Helpers.MakeWindowTex(new Color(0.01f, 0.01f, 0.01f, 0.6f), new Color(0, 1, 0, 0.5f));

                _mainWindow = new GUIStyle(GUI.skin.window)
                {
                    normal =
                    {
                        background = _normalTexture,
                        textColor = new Color(1, 1, 1, 0.3f)
                    },
                    hover =
                    {
                        background = _hoverTexture,
                        textColor = new Color(1, 1, 1, 0.6f)
                    },
                    onNormal =
                    {
                        background = _onNormalTexture
                    }
                };

                _sectionsTexture = Helpers.MakeTex(2, 2, new Color(0, 0, 0, 0.3f));

                _sections = new GUIStyle(GUI.skin.box)
                {
                    normal =
                    {
                        background = _sectionsTexture
                    }
                };

                _sections2Texture = Helpers.MakeTexWithRoundedCorner(new Color(0, 0, 0, 0.6f));

                _sections2 = new GUIStyle(GUI.skin.box)
                {
                    normal =
                    {
                        background = _sections2Texture
                    }
                };

                _mixedText = new GUIStyle(GUI.skin.label)
                {
                    normal =
                    {
                        textColor = Color.yellow
                    }
                };

                _offText = new GUIStyle(GUI.skin.label)
                {
                    normal =
                    {
                        textColor = Color.red
                    }
                };

                _onText = new GUIStyle(GUI.skin.label)
                {
                    normal =
                    {
                        textColor = Color.green
                    }
                };

                _mpnText = new GUIStyle(GUI.skin.toggle)
                {
                    richText = true
                };

                _doOnce = true;
            }

            _windowRect = GUI.Window(WindowId, _windowRect, GuiWindowControls, "ChoosyPreset", _mainWindow);
        }

        private static void GuiWindowControls(int windowId)
        {
            GUI.DragWindow(new Rect(0, 0, 10000, 20));

            GUILayout.BeginHorizontal();
            GUILayout.Label(ChoosyPreset.Translations["SearchText1"]);
            _searchInput = GUILayout.TextField(_searchInput, GUILayout.Width(160));
            GUILayout.EndHorizontal();

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

            foreach (var kv in ItemStates.Categorized)
            {
                if (!string.IsNullOrEmpty(_searchInput))
                {
                    if (!ChoosyPreset.Translations[kv.Key].ToLower().Contains(_searchInput.ToLower()) && !kv.Key.ToLower().Contains(_searchInput.ToLower()))
                    {
                        var result = false;

                        foreach (var mpn in kv.Value)
                        {
                            if (ChoosyPreset.Translations[mpn].ToLower().Contains(_searchInput.ToLower()) || mpn.ToLower().Contains(_searchInput.ToLower()))
                            {
                                result = true;
                            }
                        }

                        if (result == false)
                        {
                            continue;
                        }
                    }
                }

                var anyOn = ItemStates.CurrentItemState.IsAnyMPNOn(kv.Key);
                var anyOff = ItemStates.CurrentItemState.IsAnyMpnOff(kv.Key);

                GUILayout.BeginVertical(_sections);

                GUILayout.BeginHorizontal(_sections);

                if (GUILayout.Button("☰"))
                {
                    if (ExpandedCategory.TryGetValue(kv.Key, out _))
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
                    if (anyOn)
                    {
                        foreach (var mpn in kv.Value)
                        {
                            ItemStates.CurrentItemState.MpnStates[mpn] = false;
                        }
                    }
                    else
                    {
                        foreach (var mpn in kv.Value)
                        {
                            ItemStates.CurrentItemState.MpnStates[mpn] = true;
                        }
                    }
                }

                GUILayout.Label($"{ChoosyPreset.Translations[kv.Key]}:");

                GUILayout.FlexibleSpace();

                var status = anyOn && anyOff ? ChoosyPreset.Translations["Mixed"]
                    : anyOff ? ChoosyPreset.Translations["Off"] : ChoosyPreset.Translations["On"];

                var status1 = anyOn && anyOff ? _mixedText
                    : anyOff ? _offText : _onText;

                GUILayout.Label(status, status1);

                GUILayout.EndHorizontal();
                if (ExpandedCategory.TryGetValue(kv.Key, out var val1) && val1)
                {
                    GUILayout.BeginVertical(_sections2);

                    foreach (var mpn in kv.Value.OrderBy(tn => ChoosyPreset.Translations[tn]))
                    {
                        if (!string.IsNullOrEmpty(_searchInput) && !ChoosyPreset.Translations[mpn].ToLower().Contains(_searchInput.ToLower()) && !mpn.ToLower().Contains(_searchInput.ToLower()))
                        {
                            continue;
                        }

                        ItemStates.CurrentItemState.MpnStates[mpn] = GUILayout.Toggle(ItemStates.CurrentItemState.MpnStates[mpn], ChoosyPreset.Translations[mpn] + $" <i>(<color=#808080ff>{mpn}</color>)</i>", _mpnText);
                    }

                    GUILayout.EndVertical();
                }

                GUILayout.EndVertical();
            }

            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal(_sections);

            if (GUILayout.Button(ChoosyPreset.Translations["Enable All"]))
            {
                foreach (var kv in ItemStates.Categorized)
                {
                    foreach (var cat in kv.Value)
                    {
                        ItemStates.CurrentItemState.MpnStates[cat] = true;
                    }
                }
            }
            if (GUILayout.Button(ChoosyPreset.Translations["Disable All"]))
            {
                foreach (var kv in ItemStates.Categorized)
                {
                    foreach (var cat in kv.Value)
                    {
                        ItemStates.CurrentItemState.MpnStates[cat] = false;
                    }
                }
            }

            GUILayout.EndHorizontal();

            Helpers.ChkMouseClick(_windowRect);
        }
    }
}