using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using ChoosyPreset.Hooks;

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

            var harmony = new Harmony(Guid);
            if (CoreHooks.InstallHooks(harmony) == false)
            {
                Harmony.UnpatchAll();
                return;
            }
            UIHooks.InstallHooks(harmony);
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
            if (UIHooks.PresetPanelOpen && !UIHooks.ViewMode)
            {
                UIElements.IMGUIUI.ShowUi();
            }
        }
    }
}