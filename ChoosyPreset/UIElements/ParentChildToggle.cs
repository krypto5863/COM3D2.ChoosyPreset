using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UniverseLib.UI
{
	internal class ParentChildToggle : Toggle
	{
		private static readonly string CheckmarkImage = "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";

		private static Sprite CheckmarkSprite;
		private static bool CheckmarkLoaded = false;

		/// <summary>
		/// Create a parent-child toggle that relies on the state of it's children and effects them when enabled and stuff.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="name"></param>
		/// <param name="toggle"></param>
		/// <param name="text"></param>
		internal static GameObject CreateParentChildTogg(GameObject parent, string name, out ParentChildToggle toggle, out Text text)
		{
			var toggObj = UIFactory.CreateToggle(parent, name, out var toggleNorm, out text);

			var graph = toggleNorm.graphic;
			var bg = toggleNorm.image;
			var name1 = toggleNorm.name;
			var targ = toggleNorm.targetGraphic;
			var colors = toggleNorm.colors;

			GameObject.DestroyImmediate(toggleNorm);

			toggle = toggObj.AddComponent<ParentChildToggle>();
			toggle.name = name1;
			toggle.image = bg;
			toggle.targetGraphic = targ;
			toggle.colors = colors;
			toggle.graphic = graph;
			toggle.graphic.color = Color.white;

			if (CheckmarkLoaded == false)
			{
				var texture = new Texture2D(0, 0);
				texture.LoadImage(Convert.FromBase64String(CheckmarkImage));
				texture.Apply();

				CheckmarkSprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0, 0), 100.0f);

				CheckmarkLoaded = true;
			}

			(toggle.graphic as Image).overrideSprite = CheckmarkSprite;
			UIFactory.SetLayoutElement(toggle.graphic.gameObject, 20, 20, 0, 0, 0, 0);

			return toggObj;
		}

		/// <summary>
		/// 2 = full, 1 = mid, 0 = off
		/// </summary>
		public short State { get; private set; } = 2;

		/// <summary>
		/// Only tells boolean state of the toggle.
		/// </summary>
		public new bool isOn
		{
			get
			{
				return base.isOn;
			}

			set
			{
				if (State == 1 && value)
				{
					UpdateChildStates(true);
				}

				base.isOn = value;
			}
		}

		private bool UpdatingNoToggle;

		public ParentChildToggle() : base()
		{
			ChildToggleChangedEvent += ChildToggleChanged;
			base.onValueChanged.AddListener(UpdateChildStates);
			isOn = true;
		}

		public void UpdateChildStates(bool value)
		{
			//Main.logger.LogInfo("State was changed...");

			if (UpdatingNoToggle == false)
			{
				UpdatingNoToggle = true;

				foreach (Toggle tog in m_ChildToggles)
				{
					tog.isOn = value;
				}

				UpdatingNoToggle = false;
			}
		}

		public event EventHandler ChildToggleChangedEvent;

		public void ChildToggleChanged(object sender, EventArgs e)
		{
			if (UpdatingNoToggle)
			{
				(base.graphic as Image).overrideSprite = CheckmarkSprite;
				//base.graphic.color = Full;
				return;
			}

			bool AnyTogglesOff = AnyChildTogglesOff();
			bool AnyTogglesOn = AnyChildTogglesOn();

			if (AnyTogglesOff && AnyTogglesOn)
			{
				(base.graphic as Image).overrideSprite = null;
				//Main.logger.LogInfo("Some child toggles are off!");
				//base.graphic.color = Mid;
				State = 1;
			}
			else if (AnyTogglesOn)
			{
				//Main.logger.LogInfo("All child toggles are on!");
				//base.graphic.color = Full;
				(base.graphic as Image).overrideSprite = CheckmarkSprite;
				State = 2;
			}
			else
			{
				State = 0;
			}

			if (UpdatingNoToggle == false)
			{
				UpdatingNoToggle = true;

				isOn = AnyTogglesOn;

				UpdatingNoToggle = false;
			}
		}

		public List<Toggle> m_ChildToggles { get; private set; } = new List<Toggle>();

		public void AddChildToParent(Toggle toggle)
		{
			toggle.onValueChanged.AddListener((value) => { ChildToggleChangedEvent.Invoke(toggle, null); });
			m_ChildToggles.Add(toggle);
		}

		public bool AnyChildTogglesOn()
		{
			return m_ChildToggles.Find((Toggle x) => x.isOn) != null;
		}

		public bool AnyChildTogglesOff()
		{
			return m_ChildToggles.Find((Toggle x) => x.isOn == false) != null;
		}
	}
}