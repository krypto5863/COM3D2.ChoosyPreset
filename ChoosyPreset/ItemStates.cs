using System;
using System.Collections.Generic;

namespace ChoosyPreset
{
    public class ItemStates
    {
        public static ItemStates CurrentItemState;

        public static void InitStates()
        {
            CurrentItemState = new ItemStates();
        }

        private ItemStates()
        {
            foreach (var s in Enum.GetNames(typeof(MPN)))
            {
                MpnStates[s] = true;
            }
            foreach (var s in Enum.GetNames(typeof(MaidParts.PARTS_COLOR)))
            {
                MpnStates[s] = true;
            }
            MpnStates["AddModsSlider Settings"] = true;

            CurrentItemState = this;
        }

        public Dictionary<string, bool> MpnStates = new Dictionary<string, bool>();

        public static readonly Dictionary<string, string[]> Categorized = new Dictionary<string, string[]>
        {
            {"Clothes", new[]{"wear","skirt","mizugi","bra","panz","stkg","shoes","headset","onepiece", "acchat"} },

            {"Accesories", new[]{"accha","acchana","acckamisub","acckami","accmimi","accnip","acckubi","acckubiwa","accheso", "accude", "accashi", "accsenaka", "accshippo", "accanl", "accvag", "megane", "accxxx", "acchead", "glove" } },

            {"Body", new[]{ "MuneL", "MuneS", "MuneTare", "RegFat", "ArmL", "Hara", "RegMeet", "KubiScl", "UdeScl", "MuneUpDown", "MuneYori", "MuneYawaraka", "DouPer", "body", "sintyou", "koshi", "kata", "west", "skin", "acctatoo", "accnail", "underhair", "chikubi", "chikubicolor", "folder_underhair", "folder_skin"} },

            {"Face", new[]{ "EyeScl", "EyeSclX", "EyeSclY", "EyePosX", "EyePosY", "EyeClose", "EyeBallPosX", "EyeBallPosY", "EyeBallSclX", "EyeBallSclY", "EarNone", "EarElf", "EarRot", "EarScl", "NosePos", "NoseScl", "FaceShape", "FaceShapeSlim", "MayuShapeIn", "MayuShapeOut", "MayuX", "MayuY", "MayuRot", "HeadX", "HeadY", "MayuThick", "MayuLong", "Yorime", "MabutaUpIn", "MabutaUpIn2", "MabutaUpMiddle", "MabutaUpOut", "MabutaUpOut2", "MabutaLowIn", "MabutaLowUpMiddle", "MabutaLowUpOut", "head", "hokuro", "mayu", "lip", "eye", "eye_hi", "eye_hi_r", "eyewhite", "nose", "facegloss", "matsuge_up", "matsuge_low", "futae", "folder_eye", "folder_eyewhite", "folder_mayu", "folder_matsuge_up", "folder_matsuge_low", "folder_futae", "kousoku_upper", "kousoku_lower" } },

            {"Hair", new[]{"hairf","hairt","hairr","hairs","hairaho", "haircolor" } },

            {"Custom Hair Colors", new[]{ "HAIR", "EYE_BROW", "UNDER_HAIR", "HAIR_OUTLINE", "MATSUGE_UP", "MATSUGE_LOW", "FUTAE" } },

            {"Custom Eye Colors", new[]{ "EYE_L", "EYE_R", "EYE_WHITE" } },

            {"Custom Body Colors", new[]{ "SKIN", "NIPPLE", "SKIN_OUTLINE" } },

            {"AddModsSlider Settings", new[]{ "AddModsSlider Settings" } }
        };

        public bool IsAnyMPNOn(string category)
        {
            var result = false;

            foreach (var mpn in Categorized[category])
            {
                if (MpnStates[mpn])
                {
                    result = true;
                }
            }

            return result;
        }

        public bool IsAnyMpnOff(string category)
        {
            var result = false;

            foreach (var mpn in Categorized[category])
            {
                if (MpnStates[mpn] == false)
                {
                    result = true;
                }
            }

            return result;
        }
    }
}