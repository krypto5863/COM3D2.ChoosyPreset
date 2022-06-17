using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;
using UniverseLib.UI.Widgets;

namespace ChoosyPreset.UIElements
{
	public class MyGUI : UniverseLib.UI.Panels.PanelBase
	{
		public MyGUI(UIBase owner) : base(owner)
		{
			new ItemStates();
		}

		public override string Name => "ChoosyPreset";
		public override int MinWidth => 300;
		public override int MinHeight => 550;
		public override Vector2 DefaultAnchorMin => new Vector2(0.80f, 0.20f);
		public override Vector2 DefaultAnchorMax => new Vector2(0.98f, 0.90f);
		public override bool CanDragAndResize => true;

		private AutoSliderScrollbar Scroller;
		private List<ParentChildToggle> ParentTogs = new List<ParentChildToggle>();
		private Dictionary<Toggle, Text> TogglesText = new Dictionary<Toggle, Text>();
		private Dictionary<string, Text> textElements = new Dictionary<string, Text>();

		private int TogglesMinHeight = 20;
		private int fontSize = 16;
		private Color smokedGlass = new Color(0, 0, 0, 0.75f);
		private Color safeClear = new Color(1f, 0, 0, 0f);

		//public ItemStates State { get; set; }

		public void UpdateTranslations()
		{
			foreach (var KeyVal in TogglesText)
			{
				if (Main.Translations.TryGetValue(KeyVal.Key.name, out string val))
				{
					if (KeyVal.Key is ParentChildToggle)
					{
						KeyVal.Value.text = val;
					}
					else
					{
						KeyVal.Value.text = val + $" <i><size={KeyVal.Value.fontSize - 2}>(<color=grey>{KeyVal.Key.name}</color>)</size></i>";
					}
				}
				else
				{
					Main.logger.LogError($"{KeyVal.Key.name} isn't found in the selected translation for a toggle...");
				}
			}

			foreach (var KeyVal in TogglesText.OrderBy(t => t.Value.text))
			{
				if (KeyVal.Key is ParentChildToggle)
				{
					continue;
				}

				KeyVal.Key.gameObject.transform.SetAsLastSibling();
			}

			foreach (var KeyVal in textElements)
			{
				if (Main.Translations.TryGetValue(KeyVal.Key, out string val))
				{
					KeyVal.Value.text = val;
				}
				else
				{
					Main.logger.LogError($"{KeyVal.Key} isn't found in the selected translation...");
				}
			}
		}

		protected override void OnClosePanelClicked()
		{
		}

		protected override void ConstructPanelContent()
		{
			var PanelImage = UIRoot.GetComponent<Image>();
			PanelImage.color = smokedGlass;

			var ContentImage = ContentRoot.GetComponent<Image>();
			ContentImage.color = safeClear;

			ConstructHeader(ContentRoot);

			var myScroller = UIFactory.CreateScrollView(ContentRoot, "ScrollView", out GameObject ScrollviewContent, out Scroller, smokedGlass);
			UIFactory.SetLayoutElement(myScroller, flexibleWidth: 9999, flexibleHeight: 9999);
			UIFactory.SetLayoutGroup<VerticalLayoutGroup>(ScrollviewContent, spacing: 10);

			ConstructContents(ScrollviewContent);

			ConstructFooters(ContentRoot);

			UpdateTranslations();
		}

		protected void ConstructHeader(GameObject parent)
		{
			var SearchBar = UIFactory.CreateInputField(parent, "SearchBar", "SearchText");

			SearchBar.OnValueChanged += (newValue) =>
			{
				if (string.IsNullOrEmpty(newValue))
				{
					foreach (var ParTog in ParentTogs)
					{
						ParTog.gameObject.transform.parent.gameObject.SetActive(true);

						foreach (var childtog in ParTog.m_ChildToggles)
						{
							childtog.gameObject.SetActive(true);
						}
					}
				}
				else
				{
					foreach (var ParTog in ParentTogs)
					{
						foreach (var childtog in ParTog.m_ChildToggles)
						{
							var childText = TogglesText[childtog].text;

							if (!childText.ToLower().Contains(newValue.ToLower()))
							{
								childtog.gameObject.SetActive(false);
							}
							else
							{
								childtog.gameObject.SetActive(true);
							}
						}

						int AmountDisabled = ParTog.m_ChildToggles.Where(tog => tog.gameObject.activeSelf == false).Count();
						var parTogText = TogglesText[ParTog].text;

						if (AmountDisabled == ParTog.m_ChildToggles.Count && !parTogText.ToLower().Contains(newValue.ToLower()))
						{
							ParTog.gameObject.transform.parent.gameObject.SetActive(false);
						}
						else if (parTogText.ToLower().Contains(newValue.ToLower()) || AmountDisabled < ParTog.m_ChildToggles.Count)
						{
							ParTog.gameObject.transform.parent.gameObject.SetActive(true);
						}
					}
				}
			};
			UIFactory.SetLayoutElement(SearchBar.GameObject, 100, 30, 9999, 0);

			textElements["SearchText"] = SearchBar.PlaceholderText;
		}

		protected void ConstructContents(GameObject parent)
		{
			foreach (var KeyVal in ItemStates.Categorized)
			{
				var ParentGroup = UIFactory.CreateHorizontalGroup(parent, "ParentGroup", true, false, true, true, childAlignment: TextAnchor.MiddleLeft);

				var ExpandButton = UIFactory.CreateButton(ParentGroup.gameObject, "ExpandButton", "►", normalColor: safeClear);
				UIFactory.SetLayoutElement(ExpandButton.Component.gameObject, minWidth: 30, flexibleWidth: 0, minHeight: 25, flexibleHeight: 0);

				var ToggObjParent = ParentChildToggle.CreateParentChildTogg(ParentGroup.gameObject, KeyVal.Key, out var ParChildTog, out var parentText);
				UIFactory.SetLayoutElement(ToggObjParent, 20, 45, 9999, 0);
				TogglesText[ParChildTog] = parentText;

				//Make the children and their group.
				var ChildrenGroup = UIFactory.CreateVerticalGroup(parent, "ChildrenGroup", true, false, true, true);
				ChildrenGroup.SetActive(false);

				foreach (string Category in KeyVal.Value)
				{
					var ChildToggleObj = UIFactory.CreateToggle(ChildrenGroup, Category, out var ChildToggle, out var ChildText);
					ChildText.supportRichText = true;
					TogglesText[ChildToggle] = ChildText;

					ChildToggle.onValueChanged.AddListener((value) =>
					{
						ItemStates.CurrentItemState.MPNStates[Category] = value;
					});

					//Link child togg to parent togg
					ParChildTog.AddChildToParent(ChildToggle);
				}

				//Set functionality for expand button
				ExpandButton.OnClick = () =>
				{
					if (ChildrenGroup.activeSelf)
					{
						ExpandButton.ButtonText.text = "►";
					}
					else
					{
						ExpandButton.ButtonText.text = "▼";
					}

					ChildrenGroup.gameObject.SetActive(!ChildrenGroup.activeSelf);
				};

				//Add toggle to parents so we can deal.
				ParentTogs.Add(ParChildTog);
			}
		}

		protected void ConstructFooters(GameObject parent)
		{
			var ToggleAllGroup = UIFactory.CreateHorizontalGroup(parent, "ToggleAllButtons", true, false, true, true, 10, default, smokedGlass);

			var EnableButton = UIFactory.CreateButton(ToggleAllGroup, "EnableAllButton", "Enable All");

			EnableButton.OnClick = () =>
			{
				foreach (var toggle in ParentTogs)
				{
					toggle.isOn = true;
				}
			};

			UIFactory.SetLayoutElement(EnableButton.GameObject, 20, 40, 9999, 0);

			var DisableButton = UIFactory.CreateButton(ToggleAllGroup, "DisableAllButton", "Disable All");

			DisableButton.OnClick = () =>
			{
				foreach (var toggle in ParentTogs)
				{
					toggle.isOn = false;
				}
			};

			UIFactory.SetLayoutElement(DisableButton.GameObject, 20, 40, 9999, 0);

			textElements["Disable All"] = DisableButton.ButtonText;
			textElements["Enable All"] = EnableButton.ButtonText;
		}
	}
}