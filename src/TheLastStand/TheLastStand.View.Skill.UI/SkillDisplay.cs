using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Yield;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Skill.SkillAction;
using TheLastStand.Definition.Skill.SkillEffect;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Manager.Item;
using TheLastStand.Manager.Skill;
using TheLastStand.Model;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.Item;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Skill.SkillAction;
using TheLastStand.Model.Unit;
using TheLastStand.View.Skill.UI.Effect;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Skill.UI;

public class SkillDisplay : SerializedMonoBehaviour
{
	public static class Constants
	{
		public static class DamageType
		{
			public const string AdaptativeDamageIconPath = "View/Sprites/UI/Skills/DamageType/Icon_AdaptativeDamage";

			public const string AdaptativeDamageModifierIconPath = "View/Sprites/UI/Skills/DamageType/Icon_AdaptativeDamage_Modifier";

			public const string MagicalDamageIconPath = "View/Sprites/UI/Skills/DamageType/Icon_MagicalDamage";

			public const string PhysicalDamageIconPath = "View/Sprites/UI/Skills/DamageType/Icon_PhysicalDamage";

			public const string RangedDamageIconPath = "View/Sprites/UI/Skills/DamageType/Icon_RangedDamage";
		}

		public const float GridMaxXPosition = 210f;

		public const float GridYPositionOffset = 49f;

		public const float GridXPositionOffset = -5f;

		public static readonly Color RemainingUsesColor = new Color(0f, 0f, 0f);
	}

	[SerializeField]
	private GameObject browseTextsContainer;

	[SerializeField]
	private SkillTooltip skillTooltip;

	[SerializeField]
	private SkillTooltipDisplayer skillTooltipDisplayer;

	[SerializeField]
	private Image skillIcon;

	[SerializeField]
	private TextMeshProUGUI skillNameDisplay;

	[SerializeField]
	[Tooltip("If false, the name will only be displayed when the icon is not found (Image unlinked or actual icon not found in resources).")]
	private bool alwaysDisplayName;

	[SerializeField]
	private SkillStatDisplay actionPointsCostDisplay;

	[SerializeField]
	private bool hideActionPointsCostIfZero;

	[SerializeField]
	private SkillStatDisplay healthCostDisplay;

	[SerializeField]
	private bool hideHealthCostIfZero;

	[SerializeField]
	private SkillStatDisplay movePointsCostDisplay;

	[SerializeField]
	private bool hideMovePointsCostIfZero;

	[SerializeField]
	private SkillStatDisplay manaCostDisplay;

	[SerializeField]
	private bool hideManaCostIfZero;

	[SerializeField]
	private SkillParameterDisplay remainingUseDisplay;

	[SerializeField]
	private TextMeshProUGUI remainingUseDisplayText;

	[SerializeField]
	private bool displayRemainingUseLabel;

	[SerializeField]
	private GameObject remainingUseDisplayHolder;

	[SerializeField]
	private SkillParameterDisplay remainingChargesDisplay;

	[SerializeField]
	private GameObject remainingChargesDisplaySeparator;

	[SerializeField]
	private BetterButton usesPerTurnSmallButton;

	[SerializeField]
	private TextMeshProUGUI usesPerTurnSmallText;

	[SerializeField]
	private BetterButton usesPerTurnBigButton;

	[SerializeField]
	private TextMeshProUGUI usesPerTurnBigText;

	[SerializeField]
	private TextMeshProUGUI skillPointsCostDisplay;

	[SerializeField]
	private bool displaySkillPointsCostLabel;

	[SerializeField]
	private RectTransform skillParametersParent;

	[SerializeField]
	private RectTransform skillParametersContainer;

	[SerializeField]
	private PixelPerfectVerticalLayoutGroup skillParametersLayout;

	[SerializeField]
	private TextMeshProUGUI skillDescriptionText;

	[SerializeField]
	private RectTransform skillDescriptionRect;

	[SerializeField]
	private SkillParameterDisplay damageDisplay;

	[SerializeField]
	[Tooltip("If true, the value displayed is the skill's base DMG (raw damage). Otherwise, it will be the caster DMG (all available modifiers related to the caster taken into account).")]
	private bool damageValueIsBaseDamage;

	[SerializeField]
	private DataColorDictionary damageTypeColor;

	[SerializeField]
	private Image damageTypeIcon;

	[SerializeField]
	private Image additionalDamageIcon;

	[SerializeField]
	private GameObject noDamageTypeSpacing;

	[SerializeField]
	private RectTransform additionalDamageIconContainer;

	[SerializeField]
	private RangeSkillParameterDisplay rangeDisplay;

	[SerializeField]
	private RectTransform iconsParent;

	[SerializeField]
	private SkillParameterDisplay targetsDisplay;

	[SerializeField]
	private SkillParameterDisplay maxUsesPerTurnDisplay;

	[SerializeField]
	private RectTransform verticalSeparatorRect;

	[SerializeField]
	private SkillAreaOfEffectGrid skillAreaOfEffectGrid;

	[SerializeField]
	private TextMeshProUGUI targetingDisplay;

	[SerializeField]
	private bool displayTargetingLabel;

	[SerializeField]
	private TextMeshProUGUI critDisplay;

	[SerializeField]
	private bool displayCritLabel;

	[SerializeField]
	private TextMeshProUGUI effectDisplay;

	[SerializeField]
	private GameObject effectsContainerGameObject;

	[SerializeField]
	private RectTransform effectsContainerRect;

	[SerializeField]
	private RectTransform effectsParent;

	[SerializeField]
	private SkillEffectDisplay skillEffectPrefab;

	[SerializeField]
	private bool displayPerkSkillEffects;

	protected bool fullRefreshNeeded = true;

	private TheLastStand.Model.Skill.Skill skill;

	private ISkillCaster skillOwner;

	private List<SkillEffectDisplay> effectDisplays = new List<SkillEffectDisplay>();

	public SkillAreaOfEffectGrid SkillAreaOfEffectGrid => skillAreaOfEffectGrid;

	public TheLastStand.Model.Skill.Skill Skill
	{
		get
		{
			return skill;
		}
		set
		{
			if (value != skill)
			{
				skill = value;
				this.SkillChangedEvent?.Invoke(value);
				fullRefreshNeeded = true;
			}
		}
	}

	public ISkillCaster SkillOwner
	{
		get
		{
			return skillOwner;
		}
		set
		{
			if (value != skillOwner)
			{
				fullRefreshNeeded = true;
				skillOwner = value;
			}
		}
	}

	public TheLastStand.Model.Item.Item ItemSource => skill?.SkillContainer as TheLastStand.Model.Item.Item;

	public RectTransform SkillParametersContainer => skillParametersContainer;

	private bool EditorShowAlwaysDisplayNameField
	{
		get
		{
			if ((Object)(object)skillNameDisplay != (Object)null)
			{
				return (Object)(object)skillIcon != (Object)null;
			}
			return false;
		}
	}

	public event Action<TheLastStand.Model.Skill.Skill> SkillChangedEvent;

	public event Action SkillAreaOfEffectGridPlacedEvent;

	public void Init(SkillTooltip newSkillTooltip)
	{
		skillTooltip = newSkillTooltip;
		if ((Object)(object)skillTooltipDisplayer != (Object)null)
		{
			skillTooltipDisplayer.Init(newSkillTooltip);
		}
	}

	public void Refresh(bool forceFullRefresh = false)
	{
		if (Skill != null)
		{
			fullRefreshNeeded |= forceFullRefresh;
			RefreshInternal();
			fullRefreshNeeded = false;
		}
	}

	public void RefreshEffects(Dictionary<UnitStatDefinition.E_Stat, float> statModifiers = null)
	{
		//IL_0323: Unknown result type (might be due to invalid IL or missing references)
		//IL_0332: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0300: Unknown result type (might be due to invalid IL or missing references)
		if (Skill == null)
		{
			return;
		}
		if (Skill.SkillDefinition.SkillActionDefinition is AttackSkillActionDefinition attackSkillActionDefinition)
		{
			PlayableUnit playableUnit = SkillOwner as PlayableUnit;
			if ((Object)(object)critDisplay != (Object)null)
			{
				int num = Mathf.FloorToInt(attackSkillActionDefinition.CriticProbability + (playableUnit?.GetClampedStatValue(UnitStatDefinition.E_Stat.Critical) ?? 0f));
				((TMP_Text)critDisplay).text = string.Format("{0}{1}%", displayCritLabel ? "Critical hit: " : string.Empty, num);
			}
			if ((Object)(object)effectDisplay != (Object)null)
			{
				((TMP_Text)effectDisplay).text = ((attackSkillActionDefinition.SkillEffectDefinitions != null) ? ("Effects:\n[" + string.Join(";", attackSkillActionDefinition.SkillEffectDefinitions.Keys) + "]") : string.Empty);
			}
		}
		else
		{
			if ((Object)(object)critDisplay != (Object)null)
			{
				((TMP_Text)critDisplay).text = string.Empty;
			}
			if ((Object)(object)effectDisplay != (Object)null)
			{
				((TMP_Text)effectDisplay).text = string.Empty;
			}
		}
		if (!((Object)(object)effectsParent != (Object)null))
		{
			return;
		}
		while (effectDisplays.Count > 0)
		{
			((Component)effectDisplays[0]).gameObject.SetActive(false);
			effectDisplays.RemoveAt(0);
		}
		Dictionary<string, List<SkillEffectDefinition>> dictionary = (displayPerkSkillEffects ? Skill.SkillAction.GetAllEffects() : Skill.SkillAction.SkillActionDefinition.SkillEffectDefinitions);
		if (dictionary != null && dictionary.Count > 0)
		{
			effectsContainerGameObject.SetActive(true);
			foreach (KeyValuePair<string, List<SkillEffectDefinition>> item in dictionary)
			{
				foreach (SkillEffectDefinition item2 in item.Value)
				{
					if (item2 is SurroundingEffectDefinition surroundingEffectDefinition)
					{
						foreach (SkillEffectDefinition skillEffectDefinition in surroundingEffectDefinition.SkillEffectDefinitions)
						{
							InstantiateSkillEffectDisplay(skillEffectDefinition, isSurrounding: true, casterEffect: false, statModifiers);
						}
					}
					else if (item2 is CasterEffectDefinition casterEffectDefinition)
					{
						foreach (SkillEffectDefinition skillEffectDefinition2 in casterEffectDefinition.SkillEffectDefinitions)
						{
							InstantiateSkillEffectDisplay(skillEffectDefinition2, isSurrounding: false, casterEffect: true, statModifiers);
						}
					}
					else if (!(item2 is ExileCasterEffectDefinition))
					{
						InstantiateSkillEffectDisplay(item2, isSurrounding: false, casterEffect: false, statModifiers);
					}
				}
			}
			LayoutRebuilder.ForceRebuildLayoutImmediate(effectsParent);
			effectsContainerRect.sizeDelta = new Vector2(effectsContainerRect.sizeDelta.x, effectsContainerGameObject.activeInHierarchy ? effectsParent.sizeDelta.y : 10f);
		}
		else
		{
			effectsContainerGameObject.SetActive(false);
			effectsContainerRect.sizeDelta = new Vector2(effectsContainerRect.sizeDelta.x, 10f);
		}
	}

	public void ToggleBrowseLabel(bool state)
	{
		if ((Object)(object)browseTextsContainer != (Object)null)
		{
			browseTextsContainer.SetActive(state);
		}
	}

	private void Awake()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Combine((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
	}

	private Vector3 ComputeSkillAreaOfEffectGridPosition()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		float num = ((Transform)iconsParent).localPosition.x + iconsParent.sizeDelta.x + -5f;
		if ((Object)(object)maxUsesPerTurnDisplay != (Object)null && ((Component)maxUsesPerTurnDisplay).gameObject.activeSelf)
		{
			float num2 = maxUsesPerTurnDisplay.GetPositionAtTheEndOfLine() + -5f;
			num = Mathf.Max(num, num2);
		}
		if ((Object)(object)remainingUseDisplay != (Object)null && ((Component)remainingUseDisplay).gameObject.activeSelf)
		{
			float num3 = remainingUseDisplay.GetPositionAtTheEndOfLine() + -5f;
			num = Mathf.Max(num, num3);
		}
		num = Mathf.Max(targetsDisplay.GetPositionAtTheEndOfLine() + -5f, num);
		Vector3 localPosition = ((Transform)skillAreaOfEffectGrid.RectTransform).localPosition;
		localPosition.x = Mathf.Min(num, 210f);
		localPosition.y = targetsDisplay.GetVerticalPosition() + 49f;
		return localPosition;
	}

	private void InstantiateSkillEffectDisplay(SkillEffectDefinition skillEffectDefinition, bool isSurrounding = false, bool casterEffect = false, Dictionary<UnitStatDefinition.E_Stat, float> statModifiers = null)
	{
		if (skillEffectDefinition.ShouldBeDisplayed)
		{
			SkillEffectDisplay skillEffectDisplay = (((Object)(object)SingletonBehaviour<ObjectPooler>.Instance == (Object)null) ? Object.Instantiate<SkillEffectDisplay>(skillEffectPrefab, (Transform)(object)effectsParent) : ObjectPooler.GetPooledComponent<SkillEffectDisplay>("SkillEffectDisplay", skillEffectPrefab, (Transform)(object)effectsParent, dontSetParent: false));
			skillEffectDisplay.Init(skillEffectDefinition, SkillOwner, Skill.SkillAction, isSurrounding, casterEffect, statModifiers);
			effectDisplays.Add(skillEffectDisplay);
		}
	}

	private void OnDestroy()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Remove((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
	}

	private void OnLocalize()
	{
		if (((Component)this).gameObject.activeInHierarchy)
		{
			Refresh(forceFullRefresh: true);
		}
	}

	private IEnumerator PlaceSkillAreaOfEffectGrid()
	{
		yield return SharedYields.WaitForEndOfFrame;
		((Component)skillAreaOfEffectGrid).transform.localPosition = ComputeSkillAreaOfEffectGridPosition();
		this.SkillAreaOfEffectGridPlacedEvent?.Invoke();
		Canvas canvas = SkillAreaOfEffectGrid.Canvas;
		int sortingOrder = canvas.sortingOrder;
		canvas.sortingOrder = sortingOrder + 1;
		yield return SharedYields.WaitForEndOfFrame;
		Canvas canvas2 = SkillAreaOfEffectGrid.Canvas;
		sortingOrder = canvas2.sortingOrder;
		canvas2.sortingOrder = sortingOrder - 1;
	}

	private void RefreshDamageTypeIcon()
	{
		if ((Object)(object)damageTypeIcon == (Object)null)
		{
			return;
		}
		AttackSkillAction attackSkillAction = Skill.SkillAction as AttackSkillAction;
		((Behaviour)damageTypeIcon).enabled = attackSkillAction != null;
		if (attackSkillAction == null)
		{
			return;
		}
		if (attackSkillAction.AttackType == AttackSkillActionDefinition.E_AttackType.Adaptative)
		{
			((Behaviour)damageTypeIcon).enabled = false;
			if ((Object)(object)noDamageTypeSpacing != (Object)null)
			{
				noDamageTypeSpacing.SetActive(true);
			}
			return;
		}
		((Behaviour)damageTypeIcon).enabled = true;
		if ((Object)(object)noDamageTypeSpacing != (Object)null)
		{
			noDamageTypeSpacing.SetActive(false);
		}
		if ((Object)(object)noDamageTypeSpacing != (Object)null)
		{
			noDamageTypeSpacing.SetActive(false);
		}
		switch (attackSkillAction.AttackType)
		{
		case AttackSkillActionDefinition.E_AttackType.Physical:
			damageTypeIcon.sprite = ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Skills/DamageType/Icon_PhysicalDamage", failSilently: false);
			break;
		case AttackSkillActionDefinition.E_AttackType.Magical:
			damageTypeIcon.sprite = ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Skills/DamageType/Icon_MagicalDamage", failSilently: false);
			break;
		case AttackSkillActionDefinition.E_AttackType.Ranged:
			damageTypeIcon.sprite = ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Skills/DamageType/Icon_RangedDamage", failSilently: false);
			break;
		}
	}

	private void RefreshIcon()
	{
		if (!((Object)(object)skillIcon == (Object)null))
		{
			skillIcon.sprite = SkillView.GetIconSprite(skill.SkillDefinition.ArtId);
			((Behaviour)skillIcon).enabled = (Object)(object)skillIcon.sprite != (Object)null;
		}
	}

	private void RefreshInternal()
	{
		Dictionary<UnitStatDefinition.E_Stat, float> statModifiers = null;
		if (TPSingleton<ItemManager>.Exist() && ItemSource != null && !(ItemSource.ItemSlot is EquipmentSlot))
		{
			statModifiers = (ItemSource.IsTwoHandedWeapon ? TPSingleton<ItemManager>.Instance.GetStatsDiffBetweenItems(ItemSource, TPSingleton<ItemManager>.Instance.EquippedItemBeingCompared, TPSingleton<ItemManager>.Instance.EquippedItemBeingComparedOffHand) : TPSingleton<ItemManager>.Instance.GetStatsDiffBetweenItems(ItemSource, TPSingleton<ItemManager>.Instance.EquippedItemBeingCompared));
		}
		RefreshRequirements();
		RefreshEffects(statModifiers);
		RefreshDamageTypeIcon();
		if (fullRefreshNeeded)
		{
			RefreshIcon();
			RefreshName();
			RefreshTargeting(statModifiers);
			if ((Object)(object)skillTooltip != (Object)null)
			{
				skillTooltip.SetContent(Skill, SkillOwner);
			}
			else
			{
				SkillManager.SkillInfoPanel.SetContent(Skill, SkillOwner);
			}
		}
	}

	private void RefreshName()
	{
		if ((Object)(object)skillNameDisplay == (Object)null)
		{
			return;
		}
		if (!alwaysDisplayName)
		{
			Image obj = skillIcon;
			if (!((Object)(object)((obj != null) ? obj.sprite : null) == (Object)null))
			{
				((Component)skillNameDisplay).gameObject.SetActive(false);
				return;
			}
		}
		((TMP_Text)skillNameDisplay).text = skill.Name;
		((Component)skillNameDisplay).gameObject.SetActive(true);
	}

	private void RefreshRequirements()
	{
		PlayableUnit playableUnit = SkillOwner as PlayableUnit;
		Skill.SkillAction.SkillActionController.EnsurePerkData();
		RefreshRequirement(actionPointsCostDisplay, Skill.BaseActionPointsCost, Skill.SkillAction.SkillActionController.ComputeActionPointsCost(playableUnit, refreshPerkData: false), UnitStatDefinition.E_Stat.ActionPoints, hideActionPointsCostIfZero, playableUnit?.IsComputationStatLocked(TheLastStand.Model.Skill.Skill.E_ComputationStat.ActionPointsCost) ?? false);
		RefreshRequirement(movePointsCostDisplay, Skill.BaseMovePointsCost, Skill.SkillAction.SkillActionController.ComputeMovePointsCost(playableUnit, refreshPerkData: false), UnitStatDefinition.E_Stat.MovePoints, hideMovePointsCostIfZero, playableUnit?.IsComputationStatLocked(TheLastStand.Model.Skill.Skill.E_ComputationStat.MovePointsCost) ?? false);
		RefreshRequirement(manaCostDisplay, Skill.BaseManaCost, Skill.SkillAction.SkillActionController.ComputeManaCost(playableUnit, refreshPerkData: false), UnitStatDefinition.E_Stat.Mana, hideManaCostIfZero, playableUnit?.IsComputationStatLocked(TheLastStand.Model.Skill.Skill.E_ComputationStat.ManaCost) ?? false);
		RefreshRequirement(healthCostDisplay, Skill.BaseHealthCost, Skill.SkillAction.SkillActionController.ComputeHealthCost(playableUnit, refreshPerkData: false), UnitStatDefinition.E_Stat.Health, hideHealthCostIfZero, playableUnit?.IsComputationStatLocked(TheLastStand.Model.Skill.Skill.E_ComputationStat.HealthCost) ?? false);
		RefreshRequirementOverallRemainingCharges();
		RefreshRequirementOverallRemainingUse();
		RefreshRequirementRemainingUsePerTurn();
		RefreshRequirementSkillPointsCost();
	}

	private void RefreshRequirement(SkillStatDisplay statDisplay, int baseCost, int finalCost, UnitStatDefinition.E_Stat stat, bool hideIfZero, bool isLocked)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)statDisplay == (Object)null))
		{
			PlayableUnit playableUnit = SkillOwner as PlayableUnit;
			if (playableUnit != null && playableUnit.GetClampedStatValue(stat) < (float)finalCost)
			{
				statDisplay.ColorOverride = GameView.NegativeColor;
			}
			else if (playableUnit != null && baseCost != finalCost && baseCost != 0)
			{
				statDisplay.ColorOverride = ((finalCost > baseCost) ? GameView.NegativeColor : GameView.PositiveColor);
			}
			else
			{
				statDisplay.ColorOverride = null;
			}
			if (fullRefreshNeeded)
			{
				statDisplay.Display(!isLocked && (!hideIfZero || baseCost != 0 || finalCost != 0));
				statDisplay.Skill = skill;
				statDisplay.Refresh();
			}
		}
	}

	private void RefreshRequirementOverallRemainingCharges()
	{
		BattleModule battleModule = SkillOwner as BattleModule;
		bool flag = battleModule?.BuildingParent.IsTrap ?? false;
		if ((Object)(object)remainingChargesDisplay != (Object)null)
		{
			remainingChargesDisplay.Display(flag);
		}
		if ((Object)(object)remainingChargesDisplaySeparator != (Object)null)
		{
			remainingChargesDisplaySeparator.SetActive(flag);
		}
		if ((fullRefreshNeeded || flag) && (Object)(object)remainingChargesDisplay != (Object)null && flag)
		{
			string text = string.Empty;
			if (flag)
			{
				text = $"{battleModule.RemainingTrapCharges}/{battleModule.BuildingParent.BattleModule.BattleModuleDefinition.MaximumTrapCharges}";
			}
			string format = "<style=RemainingCharges>{0}</color>";
			remainingChargesDisplay.Refresh("SkillTooltip_OverallUsesCount_Trap", string.Format(format, text ?? ""));
		}
	}

	private void RefreshRequirementOverallRemainingUse()
	{
		bool flag = skill.OverallUses >= 0;
		if ((Object)(object)remainingUseDisplayHolder != (Object)null)
		{
			remainingUseDisplayHolder.SetActive(flag);
		}
		else if ((Object)(object)remainingUseDisplay != (Object)null)
		{
			remainingUseDisplay.Display(flag);
		}
		else
		{
			if (!((Object)(object)remainingUseDisplayText != (Object)null))
			{
				return;
			}
			((Component)remainingUseDisplayText).gameObject.SetActive(flag);
		}
		if ((fullRefreshNeeded || flag) && flag)
		{
			int num = ((TPSingleton<GameManager>.Exist() && TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Day) ? skill.ComputeTotalUses(SkillOwner) : skill.OverallUsesRemaining);
			string format = ((num == 0) ? "<style=Bad>{0}</style>" : "<style=Skill>{0}</style>");
			if ((Object)(object)remainingUseDisplay != (Object)null)
			{
				remainingUseDisplay.Refresh("SkillTooltip_OverallUsesCount", string.Format(format, string.Format("{0}{1}", displayRemainingUseLabel ? "Remaining uses: " : string.Empty, num)), 0);
			}
			else
			{
				((TMP_Text)remainingUseDisplayText).text = string.Format(format, string.Format("{0}{1}", displayRemainingUseLabel ? "Remaining uses: " : string.Empty, num));
			}
		}
	}

	private void RefreshRequirementRemainingUsePerTurn()
	{
		if ((Object)(object)usesPerTurnSmallButton == (Object)null || (Object)(object)usesPerTurnBigButton == (Object)null || (!fullRefreshNeeded && skill.UsesPerTurnRemaining < 0))
		{
			return;
		}
		((Component)usesPerTurnSmallButton).gameObject.SetActive(skill.UsesPerTurnRemaining >= 0 && skill.OverallUsesRemaining != -1);
		((Component)usesPerTurnBigButton).gameObject.SetActive(skill.UsesPerTurnRemaining >= 0 && skill.OverallUsesRemaining == -1);
		if (skill.UsesPerTurnRemaining >= 0)
		{
			if (skill.OverallUsesRemaining != -1)
			{
				((TMP_Text)usesPerTurnSmallText).text = $"{skill.UsesPerTurnRemaining}/{skill.UsesPerTurn}";
				UpdateUsesPerTurnTextColor(usesPerTurnSmallButton, usesPerTurnSmallText);
			}
			else
			{
				((TMP_Text)usesPerTurnBigText).text = $"{skill.UsesPerTurnRemaining}/{skill.UsesPerTurn}";
				UpdateUsesPerTurnTextColor(usesPerTurnBigButton, usesPerTurnBigText);
			}
		}
	}

	private void RefreshRequirementSkillPointsCost()
	{
		if (!((Object)(object)skillPointsCostDisplay == (Object)null) && fullRefreshNeeded)
		{
			int num = 0;
			((TMP_Text)skillPointsCostDisplay).text = string.Format("{0}{1}", displaySkillPointsCostLabel ? "Skill points: " : "x", num);
		}
	}

	private void RefreshTargeting(Dictionary<UnitStatDefinition.E_Stat, float> statModifiers = null)
	{
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		//IL_04aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_052b: Unknown result type (might be due to invalid IL or missing references)
		//IL_053b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0557: Unknown result type (might be due to invalid IL or missing references)
		AttackSkillAction attackSkillAction = Skill.SkillAction as AttackSkillAction;
		if ((Object)(object)skillDescriptionText != (Object)null)
		{
			((TMP_Text)skillDescriptionText).text = Skill.Description;
			LayoutRebuilder.ForceRebuildLayoutImmediate(((TMP_Text)skillDescriptionText).rectTransform);
		}
		if ((Object)(object)damageDisplay != (Object)null)
		{
			((Component)damageDisplay).gameObject.SetActive(attackSkillAction != null);
			if (attackSkillAction != null)
			{
				Vector2Int v = ((damageValueIsBaseDamage || SkillOwner == null) ? attackSkillAction.AttackSkillActionController.ComputeBaseDamageRange() : attackSkillAction.AttackSkillActionController.ComputeCasterDamageRange(SkillOwner, isSurroundingTile: false, statModifiers));
				DataColorDictionary obj = damageTypeColor;
				Color? valueColor = ((obj != null) ? new Color?(obj.GetColorById(attackSkillAction.AttackType.ToString()).Value) : null);
				damageDisplay.Refresh("SkillTooltip_Damage", v.GetSimplifiedRange(), valueColor);
			}
		}
		if ((Object)(object)additionalDamageIcon != (Object)null)
		{
			if (attackSkillAction != null && attackSkillAction.AttackSkillActionDefinition.AttackType == AttackSkillActionDefinition.E_AttackType.Adaptative)
			{
				((Component)additionalDamageIconContainer).gameObject.SetActive(true);
				additionalDamageIcon.sprite = ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Skills/DamageType/Icon_AdaptativeDamage_Modifier", failSilently: false) ?? ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Skills/DamageType/Icon_AdaptativeDamage", failSilently: false);
			}
			else
			{
				((Component)additionalDamageIconContainer).gameObject.SetActive(false);
			}
		}
		string empty;
		if ((Object)(object)rangeDisplay != (Object)null)
		{
			empty = string.Empty;
			Vector2Int range;
			int num;
			if (!(SkillOwner is TheLastStand.Model.Unit.Unit unit))
			{
				range = Skill.SkillDefinition.Range;
				num = ((Vector2Int)(ref range)).y;
			}
			else
			{
				num = unit.UnitController.GetModifiedMaxRange(Skill, statModifiers);
			}
			int num2 = num;
			if (skill.SkillDefinition.InfiniteRange)
			{
				empty = Localizer.Get("SkillTooltip_RangeAnywhere");
			}
			else
			{
				range = Skill.SkillDefinition.Range;
				if (((Vector2Int)(ref range)).x <= 1)
				{
					range = Skill.SkillDefinition.Range;
					if (((Vector2Int)(ref range)).x == num2)
					{
						range = Skill.SkillDefinition.Range;
						empty = ((((Vector2Int)(ref range)).x != 1) ? Localizer.Get("SkillTooltip_RangeSelf") : Localizer.Get("SkillTooltip_RangeMelee"));
						goto IL_02b7;
					}
				}
				range = Skill.SkillDefinition.Range;
				object arg = ((Vector2Int)(ref range)).x;
				range = Skill.SkillDefinition.Range;
				empty = $"{arg}{((((Vector2Int)(ref range)).x != num2) ? $"-{num2}" : string.Empty)}";
			}
			goto IL_02b7;
		}
		goto IL_030a;
		IL_030a:
		if ((Object)(object)targetingDisplay != (Object)null)
		{
			((TMP_Text)targetingDisplay).text = (displayTargetingLabel ? "Targeting: " : string.Empty) + (Skill.SkillDefinition.CardinalDirectionOnly ? Localizer.Get("SkillTooltip_Cardinal") : "Free");
		}
		if ((Object)(object)targetsDisplay != (Object)null)
		{
			targetsDisplay.Refresh("SkillTooltip_Targets", $"{Mathf.Max(1, Skill.SkillDefinition.AffectedTilesCount)} {((Skill.SkillDefinition.SurroundingEffectTilesCount > 0) ? $" (+{Skill.SkillDefinition.SurroundingEffectTilesCount})" : string.Empty)}");
		}
		int usesPerTurnCount = skill.SkillDefinition.UsesPerTurnCount;
		if ((Object)(object)maxUsesPerTurnDisplay != (Object)null)
		{
			maxUsesPerTurnDisplay.Refresh("SkillTooltip_UsePerTurn", ((usesPerTurnCount != -1) ? usesPerTurnCount.ToString() : Localizer.Get("SkillTooltip_UsePerTurnUnlimited")) ?? "");
		}
		if ((Object)(object)remainingUseDisplay != (Object)null)
		{
			maxUsesPerTurnDisplay.Refresh("SkillTooltip_UsePerTurn", ((usesPerTurnCount != -1) ? usesPerTurnCount.ToString() : Localizer.Get("SkillTooltip_UsePerTurnUnlimited")) ?? "");
		}
		if ((Object)(object)skillParametersContainer != (Object)null)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(skillParametersContainer);
		}
		if ((Object)(object)verticalSeparatorRect != (Object)null)
		{
			verticalSeparatorRect.sizeDelta = new Vector2(verticalSeparatorRect.sizeDelta.x, skillParametersContainer.sizeDelta.y - (float)skillParametersLayout.padding.top - (float)skillParametersLayout.padding.bottom - skillDescriptionRect.sizeDelta.y);
		}
		if ((Object)(object)skillParametersParent != (Object)null && (Object)(object)skillParametersContainer != (Object)null)
		{
			skillParametersParent.sizeDelta = new Vector2(skillParametersParent.sizeDelta.x, skillParametersContainer.sizeDelta.y + (float)skillParametersLayout.padding.bottom);
		}
		if ((Object)(object)skillAreaOfEffectGrid != (Object)null)
		{
			skillAreaOfEffectGrid.Refresh(Skill);
			if ((Object)(object)rangeDisplay != (Object)null && skillAreaOfEffectGrid.Displayed)
			{
				((MonoBehaviour)this).StartCoroutine(PlaceSkillAreaOfEffectGrid());
			}
			else
			{
				this.SkillAreaOfEffectGridPlacedEvent?.Invoke();
			}
		}
		return;
		IL_02b7:
		bool flag = skill.SkillController.ComputeMaxRange() > 1;
		rangeDisplay.Refresh("SkillTooltip_Range", empty, flag && Skill.SkillDefinition.CardinalDirectionOnly, Skill.SkillDefinition.RangeModifiable);
		goto IL_030a;
	}

	private void Start()
	{
		if ((Object)(object)actionPointsCostDisplay != (Object)null)
		{
			actionPointsCostDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.ActionPoints];
		}
		if ((Object)(object)healthCostDisplay != (Object)null)
		{
			healthCostDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.Health];
		}
		if ((Object)(object)movePointsCostDisplay != (Object)null)
		{
			movePointsCostDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.MovePoints];
		}
		if ((Object)(object)manaCostDisplay != (Object)null)
		{
			manaCostDisplay.StatDefinition = UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.Mana];
		}
		Refresh(forceFullRefresh: true);
	}

	public void UpdateUsesPerTurnTextColor(BetterButton button, TextMeshProUGUI text)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (((Selectable)button).interactable)
		{
			((Graphic)text).color = SkillManager.AvailableUsesPerTurnColor;
		}
		else if (skill.UsesPerTurnRemaining == 0)
		{
			((Graphic)text).color = SkillManager.NoUsesPerTurnColor;
		}
		else
		{
			((Graphic)text).color = SkillManager.UnavailableUsesPerTurnColor;
		}
	}

	[ContextMenu("Force Full Refresh")]
	private void DebugForceFullRefresh()
	{
		Refresh(forceFullRefresh: true);
	}
}
