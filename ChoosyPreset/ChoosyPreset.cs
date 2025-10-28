using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace ChoosyPreset
{
    [BepInPlugin(Guid, Name, Version)]
    [BepInDependency("net.perdition.com3d2.editbodyloadfix", BepInDependency.DependencyFlags.SoftDependency)]
    public class ChoosyPreset : BaseUnityPlugin
    {
        public const string Guid = "org.krypto5863.com3d2.choosypreset";
        public const string Name = "ChoosyPreset";
        public const string Version = "3.2";

        public static ChoosyPreset Instance;

        public new static ManualLogSource Logger;

        internal static ConfigEntry<string> LanguageFile;

        public static bool PresetPanelOpen { get; private set; }
        public static bool ViewMode { get; private set; }

        internal static Dictionary<string, string> Translations;

        private void Awake()
        {
            Instance = this;
            Logger = base.Logger;

            CreateChoosyPresetDirectory();

            if (SetupTranslations() == false)
            {
                return;
            }

            Hooks.InstallHooks(new Harmony(Guid));
        }

        private static void CreateChoosyPresetDirectory()
        {
            if (!Directory.Exists(Paths.ConfigPath + @"\ChoosyPreset"))
            {
                Directory.CreateDirectory(Paths.ConfigPath + @"\ChoosyPreset");
            }
        }

        private bool SetupTranslations()
        {
            var translationFiles = Directory.GetFiles(Paths.ConfigPath + @"\ChoosyPreset\", "*.*")
                .Where(s => s.ToLower().EndsWith(".json"))
                .Select(Path.GetFileName)
                .ToArray();

            if (!translationFiles.Any())
            {
                Logger.LogFatal("Not translations found! ChoosyPreset will not work! Please download the translation files, they come with the plugin, and place them in the proper directory!");
                return false;
            }

            var settingTransFiles = new AcceptableValueList<string>(translationFiles);

            LanguageFile = Config.Bind("General", "Language File", "english.json", new ConfigDescription("This denotes the translation file to use in ChoosyPreset.", settingTransFiles));
            LanguageFile.SettingChanged += (@object, eventArgs) => SetTranslations();

            SetTranslations();

            return true;
        }

        private static void SetTranslations()
        {
            Translations = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(
                $@"{Paths.ConfigPath}\ChoosyPreset\{LanguageFile.Value}"));
        }

        private void OnGUI()
        {
            if (PresetPanelOpen && !ViewMode)
            {
                UIElements.IMGUIUI.ShowUi();
            }
        }

        internal static class Hooks
        {
            internal static void InstallHooks(Harmony harmony)
            {
                harmony.PatchAll(typeof(Hooks));

                try
                {
                    var presetSetMethod = GetPresetSetOverloadWithLeastParameters();
#if DEBUG
                    Logger.LogDebug($"Choosy has found method: {methodBase.FullDescription()}");
#endif
                    var prefix = new HarmonyMethod(typeof(ChoosyPreset), nameof(PresetSet));
                    var postfix = new HarmonyMethod(typeof(ChoosyPreset), nameof(PresetColorFix));

                    harmony.Patch(presetSetMethod, prefix, postfix);
                }
                catch
                {
                    Logger.LogFatal("Failed to patch the PresetSet function!");
                    harmony.UnpatchSelf();
                }
            }

            private static MethodInfo GetPresetSetOverloadWithLeastParameters()
            {
                return
                    typeof(CharacterMgr)
                        .GetMethods()
                        .Where(methodInfo => methodInfo.Name.Equals(nameof(CharacterMgr.PresetSet)))
                        .Aggregate((methodInfo, otherMethodInfo) => methodInfo.GetParameters().Length < otherMethodInfo.GetParameters().Length ? methodInfo : otherMethodInfo);
            }

            [HarmonyPatch(typeof(PresetMgr), nameof(PresetMgr.OpenPresetPanel))]
            [HarmonyPatch(typeof(PresetMgr), nameof(PresetMgr.ClosePresetPanel))]
            [HarmonyPostfix]
            private static void PresetPanelStatusChanged(ref PresetMgr __instance)
            {
                PresetPanelOpen = __instance.m_goPresetPanel.activeSelf;
            }

            [HarmonyPatch(typeof(SceneEdit), nameof(SceneEdit.FromView))]
            [HarmonyPostfix]
            private static void FromView()
            {
                ViewMode = false;
            }

            [HarmonyPatch(typeof(SceneEdit), nameof(SceneEdit.ToView))]
            [HarmonyPostfix]
            private static void ToView()
            {
                ViewMode = true;
            }

            [HarmonyPatch(typeof(SceneEdit), nameof(SceneEdit.OnDestroy))]
            [HarmonyPrefix]
            private static void ExitingEditMode()
            {
                PresetPanelOpen = false;
                ViewMode = false;
            }

            private static readonly Dictionary<MaidParts.PARTS_COLOR, MaidParts.PartsColor> MaidColorsToKeepDic =
                new Dictionary<MaidParts.PARTS_COLOR, MaidParts.PartsColor>();

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
                            MaidColorsToKeepDic[(MaidParts.PARTS_COLOR)k] =
                                __0.Parts.GetPartsColor((MaidParts.PARTS_COLOR)k);
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
                if (string.IsNullOrEmpty(_presetFileName) == false)
                {
                    __1.strFileName = _presetFileName;
                }

                _listOfProps = null;
            }
        }
    }
}