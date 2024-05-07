using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Yield;
using TheLastStand.Controller.Skill;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Tooltip.Compendium;
using TheLastStand.Definition.Unit;
using TheLastStand.Definition.Unit.Perk;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Model.Item;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Skill.SkillAction;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.View.Generic;
using TheLastStand.View.Skill.UI;
using TheLastStand.View.Unit.Perk;
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

		public const float PerkPanelTileAndBackgroundOffset = 50f;
	}

	[SerializeField]
	private HorizontalLayoutGroup horizontalLayoutGroup;

	[SerializeField]
	private SkillTooltip skillTooltip;

	[SerializeField]
	private RectTransform itemTooltipPanel;

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
	private ItemCarouselEntryListDisplay carouselEntriesListDisplay;

	[SerializeField]
	private ItemCarouselEntryIconDisplay selectedCarouselEntryDisplayPrefab;

	[SerializeField]
	private GameObject carouselPanel;

	[SerializeField]
	private Canvas carouselPanelCanvas;

	[SerializeField]
	private GameObject spaceGameObject;

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
	private UnitPerkDisplay unitPerkDisplay;

	[SerializeField]
	private GameObject perkDisplayPanel;

	[SerializeField]
	private RectTransform perkDisplayPanelRect;

	[SerializeField]
	private RectTransform perkDisplayPanelContentRectTransform;

	[SerializeField]
	private RectTransform[] perkDisplayPanelRectTransforms;

	[SerializeField]
	private GameObject perkSkillTooltipContainer;

	[SerializeField]
	private SkillWithoutCompendiumTooltip perkSkillTooltip;

	[SerializeField]
	private bool displayPerkSkillTooltipToTheRight = true;

	[SerializeField]
	private GameObject perkSkillCycleHelper;

	[SerializeField]
	private GameObject epicParticles;

	[SerializeField]
	private GameObject rareParticles;

	private int skillsNb;

	private int perksNb;

	private VerticalLayoutGroup additionalAffixeVerticalLayoutGroup;

	private float baseAffixesBGBotOffsetInit;

	private VerticalLayoutGroup baseAffixeVerticalLayoutGroup;

	private PlayableUnit itemOwner;

	private int carouselEntryIndex;

	private ItemCarouselEntryIconDisplay selectedCarouselEntryDisplay;

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

	public Perk CurrentPerk { get; private set; }

	public TheLastStand.Model.Skill.Skill CurrentPerkSkill { get; private set; }

	public TheLastStand.Model.Item.Item Item { get; set; }

	public ItemTooltipSkillCycle ItemTooltipSkillCycle { get; set; }

	protected Dictionary<ItemDefinition.E_Rarity, GameObject> RarityParticles { get; private set; }

	public event OnItemTooltipDisplayedChange ItemTooltipDisplayedChangeEvent;

	public void RefreshCarousel()
	{
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		if (Item == null)
		{
			return;
		}
		skillsNb = 0;
		perksNb = 0;
		if ((Item.Skills != null && Item.Skills.Count > 0) || (Item.Perks != null && Item.Perks.Count > 0))
		{
			((MonoBehaviour)this).StartCoroutine(RefreshCarouselPanelCanvasSortingOrder());
			carouselPanel.SetActive(true);
			spaceGameObject.SetActive(true);
			carouselEntriesListDisplay.SetContent(Item.Skills, Item.Perks.Values.ToList());
			if (Item.Skills != null)
			{
				skillsNb = Item.Skills.Count;
			}
			if (Item.Perks != null)
			{
				perksNb = Item.Perks.Count;
			}
			int num = skillsNb + perksNb;
			if ((Object)(object)selectedCarouselEntryDisplay == (Object)null)
			{
				selectedCarouselEntryDisplay = Object.Instantiate<ItemCarouselEntryIconDisplay>(selectedCarouselEntryDisplayPrefab, ((Component)carouselEntriesListDisplay).transform);
			}
			if (carouselEntryIndex < num)
			{
				carouselEntriesListDisplay.DisplayElement(carouselEntryIndex, show: true);
			}
			carouselEntryIndex = Mathf.Clamp(ItemTooltipSkillCycle.SkillTabIndex, 0, num - 1);
			((Component)selectedCarouselEntryDisplay).transform.SetSiblingIndex(carouselEntryIndex);
			if (carouselEntryIndex < skillsNb)
			{
				selectedCarouselEntryDisplay.SetContent(Item.Skills[carouselEntryIndex].SkillDefinition);
			}
			else
			{
				selectedCarouselEntryDisplay.SetContent(Item.Perks.Values.ToList()[carouselEntryIndex - skillsNb].PerkDefinition);
			}
			bool isNextEntryASkill = skillsNb > 0 && (carouselEntryIndex + 1 < skillsNb || carouselEntryIndex + 1 >= num);
			selectedCarouselEntryDisplay.Refresh();
			selectedCarouselEntryDisplay.ToggleNextElementLabel(num > 1, isNextEntryASkill);
			float num2 = ((num > 1) ? maxSpaceSize : minSpaceSize);
			spaceRectTransform.sizeDelta = new Vector2(spaceRectTransform.sizeDelta.x, num2);
			carouselEntriesListDisplay.DisplayElement(carouselEntryIndex, show: false);
		}
		else
		{
			carouselPanel.SetActive(false);
			spaceGameObject.SetActive(false);
		}
	}

	public void RefreshSkill()
	{
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		if (Item == null)
		{
			skillDisplay.Skill = null;
			return;
		}
		TheLastStand.Model.Skill.Skill skill = skillDisplay.Skill;
		if (Item.Skills != null && Item.Skills.Count > 0 && carouselEntryIndex < Item.Skills.Count)
		{
			skillDisplayPanel.SetActive(true);
			skillDisplay.SkillOwner = itemOwner;
			skillDisplay.Skill = Item.Skills[carouselEntryIndex];
			skillDisplay.Init(skillTooltip);
			skillDisplay.Refresh();
			float num = 0f;
			((Transform)skillEffectsRect).localPosition = Vector2.op_Implicit(new Vector2(((Transform)skillEffectsRect).localPosition.x, ((Transform)skillDetailsRect).localPosition.y - skillDetailsRect.sizeDelta.y));
			num += 0f - ((Transform)skillDetailsRect).localPosition.y + skillDetailsRect.sizeDelta.y + skillEffectsRect.sizeDelta.y;
			skillDisplayPanelRect.sizeDelta = new Vector2(skillDisplayPanelRect.sizeDelta.x, num);
		}
		else
		{
			skillDisplay.Skill = null;
			skillDisplayPanel.SetActive(false);
		}
		if (skillDisplay.Skill != skill)
		{
			ItemTooltipSkillCycle.UpdateCompendium();
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(skillDisplay.SkillAreaOfEffectGrid.RectTransform);
	}

	public void RefreshPerk()
	{
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		if (Item == null)
		{
			RemovePerkLink();
		}
		else if (perksNb > 0 && carouselEntryIndex >= skillsNb)
		{
			RefreshTooltipWidth();
			RemovePerkLink();
			int index = carouselEntryIndex - skillsNb;
			perkDisplayPanel.SetActive(true);
			CurrentPerkSkill = null;
			CurrentPerk = Item.Perks.Values.ToList()[index];
			if (CurrentPerk != null)
			{
				PerkDefinition perkDefinition = CurrentPerk.PerkDefinition;
				if (itemOwner != null)
				{
					if (itemOwner.Perks.ContainsKey(perkDefinition.Id))
					{
						unitPerkDisplay.SetContent(itemOwner.Perks[perkDefinition.Id]);
					}
					else
					{
						CurrentPerk.PerkController.ChangeOwner(itemOwner);
						unitPerkDisplay.SetContent(CurrentPerk);
					}
				}
				else
				{
					unitPerkDisplay.SetContent(null, perkDefinition);
				}
				perkSkillCycleHelper.SetActive(unitPerkDisplay.PerkDefinition != null && unitPerkDisplay.PerkDefinition.SkillsToShow.Count > 1);
				int num = Mathf.Min(ItemTooltipSkillCycle.PerkSkillTabIndex, (unitPerkDisplay.PerkDefinition != null) ? (unitPerkDisplay.PerkDefinition.SkillsToShow.Count - 1) : 0);
				if (CurrentPerk.PerkDefinition.SkillsToShow.Count > 0 && num < CurrentPerk.PerkDefinition.SkillsToShow.Count)
				{
					string item = unitPerkDisplay.PerkDefinition.SkillsToShow[num].Item1;
					if (SkillDatabase.SkillDefinitions.TryGetValue(item, out var value))
					{
						CurrentPerkSkill = new SkillController(value, unitPerkDisplay.Perk, unitPerkDisplay.PerkDefinition.SkillsToShow[num].Item2).Skill;
						perkSkillTooltip.SetContent(CurrentPerkSkill, TileObjectSelectionManager.SelectedPlayableUnit);
						perkSkillTooltip.DisplayInvalidityPanel = false;
						SetPerkSkillTooltipVisible(isVisible: true);
					}
				}
				else
				{
					SetPerkSkillTooltipVisible(isVisible: false);
				}
			}
			unitPerkDisplay.Init();
			ItemTooltipSkillCycle.UpdateCompendium();
			LayoutRebuilder.ForceRebuildLayoutImmediate(perkDisplayPanelContentRectTransform);
			float num2 = 0f;
			RectTransform[] array = perkDisplayPanelRectTransforms;
			foreach (RectTransform val in array)
			{
				if (((Component)val).gameObject.activeSelf)
				{
					num2 += val.sizeDelta.y;
				}
			}
			num2 += 50f;
			perkDisplayPanelRect.sizeDelta = new Vector2(skillDisplayPanelRect.sizeDelta.x, num2);
		}
		else
		{
			CurrentPerkSkill = null;
			RemovePerkLink();
			perkSkillCycleHelper.SetActive(false);
			SetPerkSkillTooltipVisible(isVisible: false);
			unitPerkDisplay.SetContent(null);
			perkDisplayPanel.SetActive(false);
		}
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
		if (displayPerkSkillTooltipToTheRight)
		{
			perkSkillTooltipContainer.transform.SetAsLastSibling();
		}
		else
		{
			perkSkillTooltipContainer.transform.SetAsFirstSibling();
		}
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
		RefreshCarousel();
		RefreshSkill();
		RefreshPerk();
		if (Item != null)
		{
			ResizeGlow();
		}
	}

	private void RefreshTooltipWidth()
	{
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		skillDisplay.SkillAreaOfEffectGridPlacedEvent -= RefreshTooltipWidth;
		float num = 275f;
		if (skillDisplayPanel.activeSelf && skillDisplay.SkillAreaOfEffectGrid.Displayed)
		{
			num = Mathf.Max(((Transform)skillDisplay.SkillParametersContainer).localPosition.x + ((Transform)skillDisplay.SkillAreaOfEffectGrid.RectTransform).localPosition.x + skillDisplay.SkillAreaOfEffectGrid.RectTransform.sizeDelta.x + -20f, 275f);
		}
		itemTooltipPanel.sizeDelta = new Vector2(num, itemTooltipPanel.sizeDelta.y);
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

	public void AddPerkEffectEntries()
	{
		if (!((Object)(object)unitPerkDisplay != (Object)null) || unitPerkDisplay.PerkDefinition == null)
		{
			return;
		}
		foreach (CompendiumEntryDefinition compendiumEntry in unitPerkDisplay.PerkDefinition.CompendiumEntries)
		{
			ItemTooltipSkillCycle.CompendiumPanel.AddCompendiumEntry(compendiumEntry.Id, compendiumEntry.DisplayLinkedEntries);
		}
		if (unitPerkDisplay.PerkDefinition.SkillsToShow.Count > 0)
		{
			string item = unitPerkDisplay.PerkDefinition.SkillsToShow[Mathf.Min(ItemTooltipSkillCycle.PerkSkillTabIndex, unitPerkDisplay.PerkDefinition.SkillsToShow.Count - 1)].Item1;
			if (SkillDatabase.SkillDefinitions.TryGetValue(item, out var value) && value.SkillActionDefinition.SkillEffectDefinitions != null)
			{
				ItemTooltipSkillCycle.CompendiumPanel.AddSkillEffectIds(value.SkillActionDefinition.SkillEffectDefinitions);
			}
			if (CurrentPerkSkill?.SkillAction is AttackSkillAction attackSkillAction)
			{
				ItemTooltipSkillCycle.CompendiumPanel.AddDamageType(attackSkillAction);
			}
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
		RefreshPerk();
		SetPerkSkillTooltipVisible(isVisible: false);
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

	private void ResizeGlow()
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		LayoutRebuilder.ForceRebuildLayoutImmediate(allStatsPanel);
		float num = 0f;
		for (int i = 0; i < ((Transform)tooltipPanel).childCount; i++)
		{
			if ((Object)(object)((Transform)tooltipPanel).GetChild(i) != (Object)(object)((Component)glowImage).transform)
			{
				float num2 = num;
				Transform transform = ((Component)((Transform)tooltipPanel).GetChild(i)).transform;
				num = num2 + ((RectTransform)((transform is RectTransform) ? transform : null)).sizeDelta.y;
			}
		}
		tooltipPanel.sizeDelta = new Vector2(tooltipPanel.sizeDelta.x, num - (float)((LayoutGroup)allStatsLayout).padding.top);
		glowImage.sprite = glowSprites.GetSpriteAt((int)(Item.Rarity - 1));
		glowRectTransform.sizeDelta = new Vector2(glowRectTransform.sizeDelta.x, tooltipPanel.sizeDelta.y + glowDeltaHeight);
	}

	private void RemovePerkLink()
	{
		if (CurrentPerk != null)
		{
			CurrentPerk.PerkController.ChangeOwner(null);
			CurrentPerk = null;
		}
	}

	private void SetPerkSkillTooltipVisible(bool isVisible)
	{
		perkSkillTooltipContainer.SetActive(isVisible);
		if (isVisible)
		{
			perkSkillTooltip.Display();
		}
		else
		{
			perkSkillTooltip.Hide();
		}
	}

	private IEnumerator RefreshCarouselPanelCanvasSortingOrder()
	{
		Canvas obj = carouselPanelCanvas;
		int sortingOrder = obj.sortingOrder;
		obj.sortingOrder = sortingOrder + 1;
		yield return SharedYields.WaitForEndOfFrame;
		Canvas obj2 = carouselPanelCanvas;
		sortingOrder = obj2.sortingOrder;
		obj2.sortingOrder = sortingOrder - 1;
	}
}
