using System;
using System.Collections.Generic;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Model.Item;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Skill.SkillAction;
using TheLastStand.Model.Unit;
using TheLastStand.View.Generic;
using TheLastStand.View.Skill.UI;
using TheLastStand.View.Unit.Stat;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Item;

public class ItemTooltip : TooltipBase
{
	public delegate void OnItemTooltipDisplayedChange(bool isDisplayed);

	public class Constants
	{
		public const float PanelMinWidth = 275f;

		public const float PanelWidthOffest = -20f;
	}

	[SerializeField]
	private HorizontalLayoutGroup horizontalLayoutGroup;

	[SerializeField]
	private SkillTooltip skillTooltip;

	[SerializeField]
	private RectTransform spaceRectTransform;

	[SerializeField]
	private float minSpaceSize = 20f;

	[SerializeField]
	private float maxSpaceSize = 30f;

	[SerializeField]
	private RectTransform glowRectTransform;

	[SerializeField]
	private Image glowImage;

	[SerializeField]
	private DataSpriteTable glowSprites;

	[SerializeField]
	private float glowDeltaHeight = 25f;

	[SerializeField]
	private Image titleBGImage;

	[SerializeField]
	private DataSpriteTable titleBGSprites;

	[SerializeField]
	private Image itemIconImage;

	[SerializeField]
	private Image itemIconBGImage;

	[SerializeField]
	private TextMeshProUGUI itemNameText;

	[SerializeField]
	private TextMeshProUGUI subtitleText;

	[SerializeField]
	private Image categoryImage;

	[SerializeField]
	private Image handsImage;

	[SerializeField]
	private TextMeshProUGUI mainStatNameText;

	[SerializeField]
	private TextMeshProUGUI mainStatValueText;

	[SerializeField]
	private Image rarityIconImage;

	[SerializeField]
	private TextMeshProUGUI equippedText;

	[SerializeField]
	private TextMeshProUGUI sellPriceText;

	[SerializeField]
	private DataSpriteTable rarityIcons;

	[SerializeField]
	private DataColorTable rarityColors;

	[SerializeField]
	private Image itemLevelImage;

	[SerializeField]
	private DataSpriteTable itemLevelSprites;

	[SerializeField]
	private RectTransform allStatsPanel;

	[SerializeField]
	private VerticalLayoutGroup allStatsLayout;

	[SerializeField]
	private Image allStatsBG;

	[SerializeField]
	private DataSpriteTable itemTooltipBGSprites;

	[SerializeField]
	private GameObject baseAffixesPanelGameObject;

	[SerializeField]
	private RectTransform baseAffixesBG;

	[SerializeField]
	private RectTransform baseAffixesBotBG;

	[SerializeField]
	private GameObject additionalAffixesPanelGameObject;

	[SerializeField]
	private Image additionalAffixesBGImage;

	[SerializeField]
	private DataSpriteTable additionalAffixesRartityBGSprites;

	[SerializeField]
	private AffixStatView affixTextPrefab;

	[SerializeField]
	private GameObject spaceGameObject;

	[SerializeField]
	private SkillListDisplay skillsListDisplay;

	[SerializeField]
	private SkillDisplay selectedSkillDisplayPrefab;

	[SerializeField]
	private GameObject skillsPanel;

	[SerializeField]
	private SkillDisplay skillDisplay;

	[SerializeField]
	private GameObject skillDisplayPanel;

	[SerializeField]
	private RectTransform skillDisplayPanelRect;

	[SerializeField]
	private RectTransform skillDetailsRect;

	[SerializeField]
	private RectTransform skillEffectsRect;

	[SerializeField]
	private GameObject epicParticles;

	[SerializeField]
	private GameObject rareParticles;

	private VerticalLayoutGroup additionalAffixeVerticalLayoutGroup;

	private float baseAffixesBGBotOffsetInit;

	private VerticalLayoutGroup baseAffixeVerticalLayoutGroup;

	private PlayableUnit itemOwner;

	private int skillIndex;

	private SkillDisplay selectedSkillDisplay;

	private bool useDefaultValues;

	private RectTransform AdditionalAffixesPanelRectTransform
	{
		get
		{
			Transform transform = additionalAffixesPanelGameObject.transform;
			return (RectTransform)(object)((transform is RectTransform) ? transform : null);
		}
	}

	private VerticalLayoutGroup AdditionalAffixeVerticalLayoutGroup
	{
		get
		{
			if ((Object)(object)additionalAffixeVerticalLayoutGroup == (Object)null)
			{
				additionalAffixeVerticalLayoutGroup = additionalAffixesPanelGameObject.GetComponent<VerticalLayoutGroup>();
			}
			return additionalAffixeVerticalLayoutGroup;
		}
	}

	private RectTransform AffixTextPrefabRectTransform
	{
		get
		{
			Transform transform = ((Component)affixTextPrefab).transform;
			return (RectTransform)(object)((transform is RectTransform) ? transform : null);
		}
	}

	private RectTransform BaseAffixesPanelRectTransform
	{
		get
		{
			Transform transform = baseAffixesPanelGameObject.transform;
			return (RectTransform)(object)((transform is RectTransform) ? transform : null);
		}
	}

	private VerticalLayoutGroup BaseAffixeVerticalLayoutGroup
	{
		get
		{
			if ((Object)(object)baseAffixeVerticalLayoutGroup == (Object)null)
			{
				baseAffixeVerticalLayoutGroup = baseAffixesPanelGameObject.GetComponent<VerticalLayoutGroup>();
			}
			return baseAffixeVerticalLayoutGroup;
		}
	}

	public TheLastStand.Model.Item.Item Item { get; set; }

	public ItemTooltipSkillCycle ItemTooltipSkillCycle { get; set; }

	protected Dictionary<ItemDefinition.E_Rarity, GameObject> RarityParticles { get; private set; }

	public event OnItemTooltipDisplayedChange ItemTooltipDisplayedChangeEvent;

	public void RefreshSkill()
	{
		//IL_03a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_040e: Unknown result type (might be due to invalid IL or missing references)
		//IL_041f: Unknown result type (might be due to invalid IL or missing references)
		//IL_037b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
		if (Item == null)
		{
			skillDisplay.Skill = null;
			return;
		}
		skillsPanel.GetComponent<RectTransform>();
		TheLastStand.Model.Skill.Skill skill = skillDisplay.Skill;
		if (Item.Skills != null && Item.Skills.Count > 0)
		{
			skillsPanel.SetActive(true);
			spaceGameObject.SetActive(true);
			skillDisplayPanel.SetActive(true);
			skillsListDisplay.SetSkills(Item.Skills, TileObjectSelectionManager.SelectedUnit, skillTooltip);
			if ((Object)(object)selectedSkillDisplay == (Object)null)
			{
				selectedSkillDisplay = Object.Instantiate<SkillDisplay>(selectedSkillDisplayPrefab, ((Component)skillsListDisplay).transform);
			}
			if (skillIndex < Item.Skills.Count)
			{
				skillsListDisplay.DisplaySkill(skillIndex, show: true);
			}
			skillIndex = Mathf.Clamp(ItemTooltipSkillCycle.SkillTabIndex, 0, Item.Skills.Count - 1);
			((Component)selectedSkillDisplay).transform.SetSiblingIndex(skillIndex);
			selectedSkillDisplay.Skill = Item.Skills[skillIndex];
			selectedSkillDisplay.Init(skillTooltip);
			selectedSkillDisplay.Refresh();
			selectedSkillDisplay.ToggleBrowseLabel(Item.Skills.Count > 1);
			float num = ((Item.Skills.Count > 1) ? maxSpaceSize : minSpaceSize);
			spaceRectTransform.sizeDelta = new Vector2(spaceRectTransform.sizeDelta.x, num);
			skillsListDisplay.DisplaySkill(skillIndex, show: false);
			skillDisplay.SkillOwner = itemOwner;
			skillDisplay.Skill = Item.Skills[skillIndex];
			skillDisplay.Init(skillTooltip);
			skillDisplay.Refresh();
			float num2 = 0f;
			((Transform)skillEffectsRect).localPosition = Vector2.op_Implicit(new Vector2(((Transform)skillEffectsRect).localPosition.x, ((Transform)skillDetailsRect).localPosition.y - skillDetailsRect.sizeDelta.y));
			num2 += 0f - ((Transform)skillDetailsRect).localPosition.y + skillDetailsRect.sizeDelta.y + skillEffectsRect.sizeDelta.y;
			skillDisplayPanelRect.sizeDelta = new Vector2(skillDisplayPanelRect.sizeDelta.x, num2);
		}
		else
		{
			skillDisplay.Skill = null;
			skillsPanel.SetActive(false);
			spaceGameObject.SetActive(false);
			skillDisplayPanel.SetActive(false);
		}
		if (skillDisplay.Skill != skill)
		{
			ItemTooltipSkillCycle.UpdateCompendium();
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(allStatsPanel);
		LayoutRebuilder.ForceRebuildLayoutImmediate(skillDisplay.SkillAreaOfEffectGrid.RectTransform);
		float num3 = 0f;
		for (int i = 0; i < ((Transform)tooltipPanel).childCount; i++)
		{
			if ((Object)(object)((Transform)tooltipPanel).GetChild(i) != (Object)(object)((Component)glowImage).transform)
			{
				float num4 = num3;
				Transform transform = ((Component)((Transform)tooltipPanel).GetChild(i)).transform;
				num3 = num4 + ((RectTransform)((transform is RectTransform) ? transform : null)).sizeDelta.y;
			}
		}
		tooltipPanel.sizeDelta = new Vector2(tooltipPanel.sizeDelta.x, num3 - (float)((LayoutGroup)allStatsLayout).padding.top);
		glowImage.sprite = glowSprites.GetSpriteAt((int)(Item.Rarity - 1));
		glowRectTransform.sizeDelta = new Vector2(glowRectTransform.sizeDelta.x, tooltipPanel.sizeDelta.y + glowDeltaHeight);
	}

	public void SetContent(TheLastStand.Model.Item.Item item, PlayableUnit itemOwner = null, bool newUseDefaultValues = false)
	{
		Item = item;
		this.itemOwner = itemOwner;
		useDefaultValues = newUseDefaultValues;
		ItemTooltipSkillCycle.Reset();
	}

	protected override void Awake()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		base.Awake();
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Combine((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
		ItemTooltipDisplayedChangeEvent += InputManager.OnItemTooltipDisplayedChange;
		baseAffixesBGBotOffsetInit = baseAffixesBG.offsetMin.y;
		RarityParticles = new Dictionary<ItemDefinition.E_Rarity, GameObject>(default(ItemDefinition.RarityComparer))
		{
			{
				ItemDefinition.E_Rarity.Epic,
				epicParticles
			},
			{
				ItemDefinition.E_Rarity.Rare,
				rareParticles
			}
		};
	}

	protected override bool CanBeDisplayed()
	{
		return Item != null;
	}

	protected override void RefreshContent()
	{
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_041f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0459: Unknown result type (might be due to invalid IL or missing references)
		//IL_0539: Unknown result type (might be due to invalid IL or missing references)
		//IL_053e: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0597: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_061b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0655: Unknown result type (might be due to invalid IL or missing references)
		if (Item == null)
		{
			return;
		}
		allStatsBG.sprite = itemTooltipBGSprites.GetSpriteAt((int)(Item.Rarity - 1));
		titleBGImage.sprite = titleBGSprites.GetSpriteAt((int)(Item.Rarity - 1));
		itemIconImage.sprite = ItemView.GetUiSprite(Item.ItemDefinition.ArtId);
		itemIconBGImage.sprite = ItemView.GetUiSprite(Item.ItemDefinition.ArtId, isBG: true);
		((Graphic)itemIconBGImage).color = rarityColors.GetColorAt((int)(Item.Rarity - 1));
		((Graphic)subtitleText).color = rarityColors.GetColorAt((int)(Item.Rarity - 1));
		itemLevelImage.sprite = itemLevelSprites.GetSpriteAt(Item.Level);
		((Behaviour)itemLevelImage).enabled = (Object)(object)itemLevelImage.sprite != (Object)null;
		RefreshText();
		categoryImage.sprite = ResourcePooler.LoadOnce<Sprite>(string.Format("{0}{1}_On", "View/Sprites/UI/Items/Categories/Icon_ItemCategory_", ItemDefinition.E_Category.Usable.HasFlag(Item.ItemDefinition.Category) ? ItemDefinition.E_Category.Usable : Item.ItemDefinition.Category), failSilently: false);
		handsImage.sprite = ((Item.ItemDefinition.Hands != 0) ? ResourcePooler.LoadOnce<Sprite>(string.Format("{0}{1}_On", "View/Sprites/UI/Items/Hands/Icon_ItemCategory_", Item.ItemDefinition.Hands), failSilently: false) : null);
		((Behaviour)handsImage).enabled = Item.ItemDefinition.Hands != ItemDefinition.E_Hands.None;
		rarityIconImage.sprite = rarityIcons.GetSpriteAt((int)(Item.Rarity - 1));
		((TMP_Text)sellPriceText).text = (useDefaultValues ? Item.DefaultSellingPrice.ToString() : Item.SellingPrice.ToString());
		((Behaviour)equippedText).enabled = Item.ItemSlot is EquipmentSlot;
		for (int num = ((Transform)BaseAffixesPanelRectTransform).childCount - 1; num >= 0; num--)
		{
			if ((Object)(object)((Transform)BaseAffixesPanelRectTransform).GetChild(num) != (Object)(object)baseAffixesBG && (Object)(object)((Transform)BaseAffixesPanelRectTransform).GetChild(num) != (Object)(object)baseAffixesBotBG)
			{
				Object.Destroy((Object)(object)((Component)((Transform)BaseAffixesPanelRectTransform).GetChild(num)).gameObject);
			}
		}
		if (Item.BaseStatBonuses != null)
		{
			foreach (KeyValuePair<UnitStatDefinition.E_Stat, float> baseStatBonuse in Item.BaseStatBonuses)
			{
				Object.Instantiate<AffixStatView>(affixTextPrefab, baseAffixesPanelGameObject.transform).Init(baseStatBonuse.Key, baseStatBonuse.Value);
			}
		}
		Dictionary<UnitStatDefinition.E_Stat, float> dictionary = Item.ItemController.MergeAffixes(Item.AdditionalAffixesMalus);
		foreach (KeyValuePair<UnitStatDefinition.E_Stat, float> item in dictionary)
		{
			Object.Instantiate<AffixStatView>(affixTextPrefab, baseAffixesPanelGameObject.transform).Init(item.Key, 0f - item.Value);
		}
		Vector2 sizeDelta = BaseAffixesPanelRectTransform.sizeDelta;
		if ((Item.BaseStatBonuses == null || Item.BaseStatBonuses.Count == 0) && dictionary.Count == 0)
		{
			sizeDelta.y = 0f;
			baseAffixesPanelGameObject.SetActive(false);
		}
		else
		{
			int num2 = Item.BaseStatBonuses?.Count ?? 0;
			sizeDelta.y = (float)(num2 + dictionary.Count) * AffixTextPrefabRectTransform.sizeDelta.y + (float)((LayoutGroup)BaseAffixeVerticalLayoutGroup).padding.top + (float)((LayoutGroup)BaseAffixeVerticalLayoutGroup).padding.bottom;
			BaseAffixesPanelRectTransform.sizeDelta = sizeDelta;
			baseAffixesPanelGameObject.SetActive(true);
		}
		for (int num3 = ((Component)AdditionalAffixesPanelRectTransform).transform.childCount - 1; num3 >= 0; num3--)
		{
			if ((Object)(object)((Transform)AdditionalAffixesPanelRectTransform).GetChild(num3) != (Object)(object)((Component)additionalAffixesBGImage).transform)
			{
				Object.Destroy((Object)(object)((Component)((Transform)AdditionalAffixesPanelRectTransform).GetChild(num3)).gameObject);
			}
		}
		Dictionary<UnitStatDefinition.E_Stat, float> dictionary2 = Item.ItemController.MergeAffixes(Item.AdditionalAffixes);
		foreach (KeyValuePair<UnitStatDefinition.E_Stat, float> item2 in dictionary2)
		{
			Object.Instantiate<AffixStatView>(affixTextPrefab, additionalAffixesPanelGameObject.transform).Init(item2.Key, item2.Value);
		}
		Vector2 sizeDelta2 = AdditionalAffixesPanelRectTransform.sizeDelta;
		bool flag = (dictionary2.Count == 0 && Item.BaseStatBonuses != null && Item.BaseStatBonuses.Count > 0) || dictionary.Count > 0;
		((Component)baseAffixesBotBG).gameObject.SetActive(flag);
		if (flag)
		{
			baseAffixesBG.offsetMin = new Vector2(baseAffixesBG.offsetMin.x, baseAffixesBGBotOffsetInit - baseAffixesBotBG.anchoredPosition.y);
		}
		else
		{
			baseAffixesBG.offsetMin = new Vector2(baseAffixesBG.offsetMin.x, baseAffixesBGBotOffsetInit);
		}
		if (dictionary2.Count == 0)
		{
			sizeDelta2.y = 0f;
			additionalAffixesPanelGameObject.SetActive(false);
		}
		else
		{
			sizeDelta2.y = (float)dictionary2.Count * AffixTextPrefabRectTransform.sizeDelta.y + (float)((LayoutGroup)AdditionalAffixeVerticalLayoutGroup).padding.top + (float)((LayoutGroup)AdditionalAffixeVerticalLayoutGroup).padding.bottom;
			AdditionalAffixesPanelRectTransform.sizeDelta = sizeDelta2;
			additionalAffixesPanelGameObject.SetActive(true);
		}
		additionalAffixesBGImage.sprite = additionalAffixesRartityBGSprites.GetSpriteAt((int)(Item.Rarity - 1));
		if (RarityParticles != null)
		{
			foreach (KeyValuePair<ItemDefinition.E_Rarity, GameObject> rarityParticle in RarityParticles)
			{
				GameObject value = rarityParticle.Value;
				if (value != null)
				{
					value.SetActive(rarityParticle.Key == Item.Rarity);
				}
			}
		}
		skillDisplay.SkillAreaOfEffectGridPlacedEvent += RefreshTooltipWidth;
		RefreshSkill();
	}

	private void RefreshTooltipWidth()
	{
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		skillDisplay.SkillAreaOfEffectGridPlacedEvent -= RefreshTooltipWidth;
		float num = 275f;
		if (skillDisplay.SkillAreaOfEffectGrid.Displayed)
		{
			num = Mathf.Max(((Transform)skillDisplay.SkillParametersContainer).localPosition.x + ((Transform)skillDisplay.SkillAreaOfEffectGrid.RectTransform).localPosition.x + skillDisplay.SkillAreaOfEffectGrid.RectTransform.sizeDelta.x + -20f, 275f);
		}
		base.RectTransform.sizeDelta = new Vector2(num, base.RectTransform.sizeDelta.y);
		Transform transform = ((Component)horizontalLayoutGroup).transform;
		Transform obj = ((transform is RectTransform) ? transform : null);
		LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)(object)obj);
		TPHelpers.ClampToParent((RectTransform)(object)obj);
	}

	public void AddAttackTypeEntries()
	{
		if (skillDisplay.Skill?.SkillAction is AttackSkillAction attackSkillAction)
		{
			ItemTooltipSkillCycle.CompendiumPanel.AddDamageType(attackSkillAction);
		}
	}

	public void AddSkillEffectEntries()
	{
		if (skillDisplay.Skill != null && skillDisplay.Skill.SkillAction.SkillActionDefinition.SkillEffectDefinitions != null)
		{
			ItemTooltipSkillCycle.CompendiumPanel.AddSkillEffectIds(skillDisplay.Skill.SkillAction.SkillActionDefinition.SkillEffectDefinitions);
		}
	}

	protected override void OnDisplay()
	{
		base.OnDisplay();
		this.ItemTooltipDisplayedChangeEvent?.Invoke(isDisplayed: true);
	}

	protected override void OnHide()
	{
		base.OnHide();
		this.ItemTooltipDisplayedChangeEvent?.Invoke(isDisplayed: false);
		Item = null;
		RefreshSkill();
		if ((Object)(object)ItemTooltipSkillCycle != (Object)null)
		{
			ItemTooltipSkillCycle.UpdateCompendium();
		}
	}

	private void OnDestroy()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Remove((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
		ItemTooltipDisplayedChangeEvent -= InputManager.OnItemTooltipDisplayedChange;
	}

	private void OnLocalize()
	{
		if (((Component)this).gameObject.activeInHierarchy)
		{
			RefreshText();
		}
	}

	private void RefreshText()
	{
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		if (Item != null)
		{
			((TMP_Text)itemNameText).text = Item.Name;
			((TMP_Text)subtitleText).text = ((Item.ItemDefinition.Hands != 0) ? (Item.ItemDefinition.HandsName + " - ") : string.Empty) + ((Item.ItemDefinition.Category != 0) ? (Item.ItemDefinition.CategoryName + " - ") : string.Empty) + Item.RarityName;
			if (Item.MainStatBonusByLevel != null)
			{
				float item = Item.MainStatBonusByLevel.Item2;
				((TMP_Text)mainStatNameText).text = "<size=16><sprite name=\"" + Item.MainStatBonusByLevel.Item1.ToString() + "\"></size>" + UnitDatabase.UnitStatDefinitions[Item.MainStatBonusByLevel.Item1].ShortName;
				((TMP_Text)mainStatValueText).text = string.Format("<style=\"{0}\">{1}{2}{3}</style>", (item >= 0f) ? "GoodNbOutlined" : "BadNbOutlined", (item >= 0f) ? "+" : string.Empty, item, Item.MainStatBonusByLevel.Item1.ShownAsPercentage() ? "%" : string.Empty);
			}
			else if (Item.BaseDamages != Vector2.zero)
			{
				((TMP_Text)mainStatNameText).text = Localizer.Get("ItemTooltip_BaseDamageName");
				((TMP_Text)mainStatValueText).text = Localizer.Format("ItemTooltip_BaseDamageValue", new object[2]
				{
					Item.BaseDamages.x,
					Item.BaseDamages.y
				});
			}
			else
			{
				((TMP_Text)mainStatNameText).text = "<color=#999999>-";
				((TMP_Text)mainStatValueText).text = string.Empty;
			}
		}
	}
}
