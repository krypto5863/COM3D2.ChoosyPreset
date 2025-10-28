using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace ChoosyPreset.Hooks
{
    internal static class CoreHooks
    {
        internal static void InstallHooks(Harmony harmony)
        {
            try
            {
                var presetSetMethod = GetPresetSetOverloadWithLeastParameters();
#if DEBUG
                Logger.LogDebug($"Choosy has found method: {methodBase.FullDescription()}");
#endif
                var prefix = new HarmonyMethod(typeof(ChoosyPreset), nameof(PresetSetPrefix));
                var postfix = new HarmonyMethod(typeof(ChoosyPreset), nameof(PresetSetPostfix));

                harmony.Patch(presetSetMethod, prefix, postfix);
            }
            catch
            {
                ChoosyPreset.Logger.LogFatal("Failed to patch the PresetSet function!");
                harmony.UnpatchSelf();
            }
        }

        private static readonly Dictionary<MaidParts.PARTS_COLOR, MaidParts.PartsColor> MaidColorsToKeepDic =
            new Dictionary<MaidParts.PARTS_COLOR, MaidParts.PartsColor>();

        private static List<MaidProp> _listOfProps;
        private static string _presetFileName;

        private static MethodInfo GetPresetSetOverloadWithLeastParameters()
        {
            return
                typeof(CharacterMgr)
                    .GetMethods()
                    .Where(methodInfo => methodInfo.Name.Equals(nameof(CharacterMgr.PresetSet)))
                    .Aggregate((methodInfo, otherMethodInfo) => methodInfo.GetParameters().Length < otherMethodInfo.GetParameters().Length ? methodInfo : otherMethodInfo);
        }

        private static bool PresetSetPrefix(Maid __0, ref CharacterMgr.Preset __1)
        {
            var maid = __0;
            var preset = __1;

            if (UIHooks.PresetPanelOpen == false)
            {
                return true;
            }

            _listOfProps = new List<MaidProp>(preset.listMprop);
            _presetFileName = preset.strFileName;

            var props = preset.listMprop.ToArray();

            CapturePartsColors(maid, preset);
            FilterPropsToLoad(preset, props);

            return true;
        }

        private static void FilterPropsToLoad(CharacterMgr.Preset preset, MaidProp[] props)
        {
            foreach (var part in props)
            {
                var mpn = (MPN)part.idx;

                if (!ItemStates.CurrentItemState.MpnStates[mpn.ToString()])
                {
                    preset.listMprop.Remove(part);
                }
            }

            if (!ItemStates.CurrentItemState.MpnStates["AddModsSlider Settings"])
            {
                preset.strFileName = "";
            }
        }

        private static void CapturePartsColors(Maid maid, CharacterMgr.Preset preset)
        {
            MaidColorsToKeepDic.Clear();
            if (preset.aryPartsColor != null)
            {
                for (var k = 0; k < preset.aryPartsColor.Length; k++)
                {
                    var colorName = Enum.GetName(typeof(MaidParts.PARTS_COLOR), k);

                    if (!ItemStates.CurrentItemState.MpnStates[colorName])
                    {
                        MaidColorsToKeepDic[(MaidParts.PARTS_COLOR)k] =
                            maid.Parts.GetPartsColor((MaidParts.PARTS_COLOR)k);
                    }
                }
            }
        }

        private static void PresetSetPostfix(Maid __0, ref CharacterMgr.Preset __1)
        {
            var maid = __0;
            var preset = __1;

            if (UIHooks.PresetPanelOpen == false)
            {
                return;
            }

            foreach (var keyValue in MaidColorsToKeepDic)
            {
                maid.Parts.SetPartsColor(keyValue.Key, keyValue.Value);
            }

            preset.listMprop = new List<MaidProp>(_listOfProps);
            // avoid restoring to an empty string, in case an earlier patch has done the same
            if (string.IsNullOrEmpty(_presetFileName) == false)
            {
                preset.strFileName = _presetFileName;
            }

            _listOfProps = null;
        }
    }
}