using HarmonyLib;

namespace ChoosyPreset.Hooks
{
    internal static class UIHooks
    {
        internal static bool PresetPanelOpen { get; private set; }
        internal static bool ViewMode { get; private set; }

        internal static void InstallHooks(Harmony harmony)
        {
            harmony.PatchAll(typeof(UIHooks));
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
    }
}