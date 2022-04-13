# COM3D2.ChoosyPreset
A plugin that allows you to selectively load parts of presets.

As it says on the tin. You simply open your preset panel and the menu will open with it. You'll notice there are two modes, simple and advanced. Simple is usually what you want. This plugin will likely be updated with the COM3D2.GUIAPI once I get around to finishing the plugin API so stay tuned for that update.

## Making your own Translations! ##
1. Navigate to the `RootGameFolder/BepinEx/Config/ChoosyPreset` folder. Copy any of the language files and rename it to your language of choice. Naming does not need to be standard except for the `.json` extension.
2. Begin Translating. The translations are ordered as `"KEY": "Translated Text",`. Do not change the key! Furthermore, don't delete keys either. Translations files are encoded in unicode!
3. Restart your game, select the new language file in the [ConfigurationManager](https://github.com/BepInEx/BepInEx.ConfigurationManager) (F1 when installed) or manually set the name of the language file in the ChoosyPreset cfg file.
4. Submit the translation file to me so I can include it with ChoosyPreset releases!

## Installation ##
1. Download the DLL from the release section
2. Place the DLL into BepinEx/plugins
3. Done.

Enjoy and don't abuse your meidos!
