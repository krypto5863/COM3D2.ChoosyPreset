using System;
using System.Collections.Generic;

namespace ChoosyPreset
{
	public class ItemStates
	{
		public static ItemStates CurrentItemState;

		public ItemStates()
		{
			foreach (string s in Enum.GetNames(typeof(MPN)))
			{
				MPNStates[s] = true;
			}
			foreach (string s in Enum.GetNames(typeof(MaidParts.PARTS_COLOR)))
			{
				MPNStates[s] = true;
			}
			MPNStates["AddModsSlider Settings"] = true;

			CurrentItemState = this;
		}

		public Dictionary<string, bool> MPNStates = new Dictionary<string, bool>();

		public static readonly Dictionary<string, string[]> Categorized = new Dictionary<string, string[]>
		{
			{"Clothes", new string[]{"wear","skirt","mizugi","bra","panz","stkg","shoes","headset","onepiece", "acchat"} },

			{"Accesories", new string[]{"accha","acchana","acckamisub","acckami","accmimi","accnip","acckubi","acckubiwa","accheso", "accude", "accashi", "accsenaka", "accshippo", "accanl", "accvag", "megane", "accxxx", "acchead", "glove" } },

			{"Body", new string[]{ "MuneL", "MuneS", "MuneTare", "RegFat", "ArmL", "Hara", "RegMeet", "KubiScl", "UdeScl", "MuneUpDown", "MuneYori", "MuneYawaraka", "DouPer", "body", "sintyou", "koshi", "kata", "west", "skin", "acctatoo", "accnail", "underhair", "chikubi", "chikubicolor", "folder_underhair", "folder_skin"} },

			{"Face", new string[]{ "EyeScl", "EyeSclX", "EyeSclY", "EyePosX", "EyePosY", "EyeClose", "EyeBallPosX", "EyeBallPosY", "EyeBallSclX", "EyeBallSclY", "EarNone", "EarElf", "EarRot", "EarScl", "NosePos", "NoseScl", "FaceShape", "FaceShapeSlim", "MayuShapeIn", "MayuShapeOut", "MayuX", "MayuY", "MayuRot", "HeadX", "HeadY", "MayuThick", "MayuLong", "Yorime", "MabutaUpIn", "MabutaUpIn2", "MabutaUpMiddle", "MabutaUpOut", "MabutaUpOut2", "MabutaLowIn", "MabutaLowUpMiddle", "MabutaLowUpOut", "head", "hokuro", "mayu", "lip", "eye", "eye_hi", "eye_hi_r", "eyewhite", "nose", "facegloss", "matsuge_up", "matsuge_low", "futae", "folder_eye", "folder_eyewhite", "folder_mayu", "folder_matsuge_up", "folder_matsuge_low", "folder_futae", "kousoku_upper", "kousoku_lower" } },

			{"Hair", new string[]{"hairf","hairt","hairr","hairs","hairaho", "haircolor" } },

			{"Custom Hair Colors", new string[]{ "HAIR", "EYE_BROW", "UNDER_HAIR", "HAIR_OUTLINE", "MATSUGE_UP", "MATSUGE_LOW", "FUTAE" } },

			{"Custom Eye Colors", new string[]{ "EYE_L", "EYE_R", "EYE_WHITE" } },

			{"Custom Body Colors", new string[]{ "SKIN", "NIPPLE", "SKIN_OUTLINE" } },

			{"AddModsSlider Settings", new string[]{ "AddModsSlider Settings" } }
		};

		public bool IsAnyMPNOn(string category)
		{
			var result = false;

			foreach (var MPN in Categorized[category])
			{
				if (MPNStates[MPN])
				{
					result = true;
					continue;
				}
			}

			return result;
		}

		public bool IsAnyMPNOff(string category)
		{
			var result = false;

			foreach (var MPN in Categorized[category])
			{
				if (MPNStates[MPN] == false)
				{
					result = true;
					continue;
				}
			}

			return result;
		}
	}
}