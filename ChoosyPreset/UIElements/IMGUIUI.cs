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

        private static bool _guiInitialized;

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
            if (_guiInitialized == false)
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

                _guiInitialized = true;
            }

            _windowRect = GUI.Window(WindowId, _windowRect, DrawWindow, "ChoosyPreset", _mainWindow);
        }

        private static void DrawWindow(int windowId)
        {
            GUI.DragWindow(new Rect(0, 0, 10000, 20));

            var lowerCaseSearchInput = DrawSearchBar();

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

            foreach (var categoriesMpns in ItemStates.CategoriesMpn)
            {
                var category = categoriesMpns.Key;
                var mpns = categoriesMpns.Value;

                if (FilterCategoriesBySearchInput(lowerCaseSearchInput, categoriesMpns) == false)
                {
                    continue;
                }

                DrawCategory(category, mpns, lowerCaseSearchInput);
            }

            GUILayout.EndScrollView();

            DrawFooterButtons();

            Helpers.ChkMouseClick(_windowRect);
        }

        private static void DrawCategory(string category, string[] mpns, string lowerCaseSearchInput)
        {
            var anyOn = ItemStates.CurrentItemState.IsAnyMPNOn(category);
            var anyOff = ItemStates.CurrentItemState.IsAnyMpnOff(category);

            GUILayout.BeginVertical(_sections);

            GUILayout.BeginHorizontal(_sections);

            if (GUILayout.Button("☰"))
            {
                if (ExpandedCategory.TryGetValue(category, out _))
                {
                    ExpandedCategory[category] = !ExpandedCategory[category];
                }
                else
                {
                    ExpandedCategory[category] = true;
                }
            }

            if (GUILayout.Button("I/O"))
            {
                if (anyOn)
                {
                    foreach (var mpn in mpns)
                    {
                        ItemStates.CurrentItemState.MpnStates[mpn] = false;
                    }
                }
                else
                {
                    foreach (var mpn in mpns)
                    {
                        ItemStates.CurrentItemState.MpnStates[mpn] = true;
                    }
                }
            }

            GUILayout.Label($"{ChoosyPreset.Translations[category]}:");

            GUILayout.FlexibleSpace();

            var status = anyOn && anyOff ? ChoosyPreset.Translations["Mixed"]
                : anyOff ? ChoosyPreset.Translations["Off"] : ChoosyPreset.Translations["On"];

            var statusStyle = anyOn && anyOff ? _mixedText
                : anyOff ? _offText : _onText;

            GUILayout.Label(status, statusStyle);

            GUILayout.EndHorizontal();
            DrawCategoriesItems(category, mpns, lowerCaseSearchInput);

            GUILayout.EndVertical();
        }

        private static void DrawCategoriesItems(string category, string[] mpns, string lowerCaseSearchInput)
        {
            if (!ExpandedCategory.TryGetValue(category, out var categoryExpanded))
            {
                return;
            }

            if (!categoryExpanded)
            {
                return;
            }

            GUILayout.BeginVertical(_sections2);

            foreach (var mpn in mpns.OrderBy(tn => ChoosyPreset.Translations[tn]))
            {
                var searchInputNotBlankAndDoesNotMatchSearch = !string.IsNullOrEmpty(lowerCaseSearchInput) && !ChoosyPreset.Translations[mpn].ToLower().Contains(lowerCaseSearchInput) && !mpn.ToLower().Contains(lowerCaseSearchInput);
                if (searchInputNotBlankAndDoesNotMatchSearch)
                {
                    continue;
                }

                ItemStates.CurrentItemState.MpnStates[mpn] = GUILayout.Toggle(ItemStates.CurrentItemState.MpnStates[mpn], ChoosyPreset.Translations[mpn] + $" <i>(<color=#808080ff>{mpn}</color>)</i>", _mpnText);
            }

            GUILayout.EndVertical();
        }

        private static void DrawFooterButtons()
        {
            GUILayout.BeginHorizontal(_sections);

            if (GUILayout.Button(ChoosyPreset.Translations["Enable All"]))
            {
                SetStateOfAll(true);
            }
            if (GUILayout.Button(ChoosyPreset.Translations["Disable All"]))
            {
                SetStateOfAll(false);
            }

            GUILayout.EndHorizontal();
        }

        private static void SetStateOfAll(bool state)
        {
            foreach (var kv in ItemStates.CategoriesMpn)
            {
                foreach (var cat in kv.Value)
                {
                    ItemStates.CurrentItemState.MpnStates[cat] = state;
                }
            }
        }

        private static bool FilterCategoriesBySearchInput(string lowerCaseSearchInput, KeyValuePair<string, string[]> categoriesMpn)
        {
            if (string.IsNullOrEmpty(lowerCaseSearchInput))
            {
                return true;
            }

            if (ChoosyPreset.Translations[categoriesMpn.Key].ToLower().Contains(lowerCaseSearchInput) ||
                categoriesMpn.Key.ToLower().Contains(lowerCaseSearchInput))
            {
                return true;
            }

            foreach (var mpn in categoriesMpn.Value)
            {
                if (ChoosyPreset.Translations[mpn].ToLower().Contains(lowerCaseSearchInput) || mpn.ToLower().Contains(lowerCaseSearchInput))
                {
                    return true;
                }
            }

            return false;
        }

        private static string DrawSearchBar()
        {
            GUILayout.BeginHorizontal();

            GUILayout.Label(ChoosyPreset.Translations["SearchText1"]);
            _searchInput = GUILayout.TextField(_searchInput, GUILayout.Width(160));
            GUILayout.EndHorizontal();

            var lowerCaseSearchInput = _searchInput.ToLower();
            return lowerCaseSearchInput;
        }
    }
}