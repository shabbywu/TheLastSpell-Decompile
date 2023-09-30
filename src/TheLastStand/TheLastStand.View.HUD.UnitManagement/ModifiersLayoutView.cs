using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Localization.Fonts;
using TheLastStand.Definition.Unit.Enemy.Affix;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Skill;
using TheLastStand.Model.Status;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.View.HUD.BottomScreenPanel.UnitManagement;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.HUD.UnitManagement;

public class ModifiersLayoutView : MonoBehaviour
{
	[SerializeField]
	private RectTransform container;

	[SerializeField]
	private LayoutGroup containerLayout;

	[SerializeField]
	private SimpleFontLocalizedParent simpleFontLocalizedParent;

	[SerializeField]
	private GuardianIconDisplay guardianIconDisplay;

	[SerializeField]
	private RectTransform enemiesAffixesParent;

	[SerializeField]
	private LayoutGroup enemiesAffixesLayoutGroup;

	[SerializeField]
	private EliteAffixIconDisplay enemyAffixIconDisplayPrefab;

	[SerializeField]
	private EnemyAffixSeparator enemyAffixSeparatorPrefab;

	[SerializeField]
	private InjuryIconDisplay injuryIconDisplayPrefab;

	[SerializeField]
	private RectTransform statusesParent;

	[SerializeField]
	private RectTransform statusesBackground;

	[SerializeField]
	private LayoutGroup statusesLayoutGroup;

	[SerializeField]
	private Status.E_StatusType[] statusesDisplayOrder = new Status.E_StatusType[8]
	{
		Status.E_StatusType.Debuff,
		Status.E_StatusType.Stun,
		Status.E_StatusType.Poison,
		Status.E_StatusType.Contagion,
		Status.E_StatusType.Buff,
		Status.E_StatusType.Charged,
		Status.E_StatusType.AllNegativeImmunity,
		Status.E_StatusType.All
	};

	[SerializeField]
	private StatusIconDisplay statusIconDisplayPrefab;

	[SerializeField]
	private RectTransform statusesHover;

	[SerializeField]
	private int statusesBackgroundWidthBase = 46;

	[SerializeField]
	private int statusesBackgroundWidthIncrement = 36;

	[SerializeField]
	private MomentumIconDisplay momentumIconDisplay;

	[SerializeField]
	private RectTransform perksParent;

	[SerializeField]
	private GameObject perksSpacing;

	[SerializeField]
	private LayoutGroup perksLayoutGroup;

	[SerializeField]
	private PerkIconDisplay perkIconDisplayPrefab;

	[SerializeField]
	private RectTransform perksBackground;

	[SerializeField]
	private RectTransform perksHover;

	[SerializeField]
	private int perkBackgroundWidthBase = 50;

	[SerializeField]
	private int perkBackgroundWidthIncrement = 43;

	[SerializeField]
	private HUDJoystickSimpleTarget joystickTarget;

	[SerializeField]
	private RectTransform joystickTargetFeedback;

	[SerializeField]
	private float joystickTargetFeedbackMinWidth = 116f;

	private List<EliteAffixIconDisplay> enemyAffixIconDisplays = new List<EliteAffixIconDisplay>();

	private List<EnemyAffixSeparator> enemyAffixSeparators = new List<EnemyAffixSeparator>();

	private readonly List<PerkIconDisplay> perkIconDisplays = new List<PerkIconDisplay>();

	private int displayedStatusesCount;

	private TheLastStand.Model.Unit.Unit unit;

	private InjuryIconDisplay injuryIconDisplay;

	private readonly ConcurrentQueue<StatusIconDisplay> statusIconDisplayPool = new ConcurrentQueue<StatusIconDisplay>();

	public HUDJoystickSimpleTarget JoystickTarget => joystickTarget;

	public bool IsDisplayed()
	{
		if ((!((Object)(object)guardianIconDisplay != (Object)null) || !((Component)guardianIconDisplay).gameObject.activeSelf) && (enemyAffixIconDisplays == null || !enemyAffixIconDisplays.Any((EliteAffixIconDisplay icon) => ((Component)icon).gameObject.activeSelf)) && (!((Object)(object)injuryIconDisplay != (Object)null) || !((Component)injuryIconDisplay).gameObject.activeSelf) && !statusIconDisplayPool.Any((StatusIconDisplay o) => ((Component)o).gameObject.activeSelf))
		{
			return perkIconDisplays.Any((PerkIconDisplay o) => ((Component)o).gameObject.activeSelf);
		}
		return true;
	}

	public void Refresh()
	{
		unit = TileObjectSelectionManager.SelectedUnit;
		RefreshGuardian();
		RefreshEliteAffixes();
		RefreshInjuries();
		RefreshStatuses();
		RefreshMomentum();
		RefreshStatusesBackground();
		RefreshPerks();
		ToggleLayouts(state: true);
		LayoutRebuilder.ForceRebuildLayoutImmediate(container);
		ToggleLayouts(state: false);
		RefreshJoystickNavigation();
		SimpleFontLocalizedParent obj = simpleFontLocalizedParent;
		if (obj != null)
		{
			((FontLocalizedParent)obj).RefreshChilds();
		}
	}

	public void RefreshPerks()
	{
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		if (unit is PlayableUnit playableUnit)
		{
			for (int i = 0; i < perkIconDisplays.Count; i++)
			{
				perkIconDisplays[i].Hide();
			}
			int num = 0;
			foreach (KeyValuePair<string, Perk> perk in playableUnit.Perks)
			{
				if (perk.Value.DisplayInHUD(out var greyedOut))
				{
					AdjustPerksPoolLength(num);
					perkIconDisplays[num].Display(perk.Value, greyedOut);
					perkIconDisplays[num].DisplayHighlightSign(show: false, triggerEvent: false);
					num++;
				}
			}
			if (num > 0)
			{
				((Component)perksParent).gameObject.SetActive(true);
				perksBackground.sizeDelta = new Vector2((float)(perkBackgroundWidthBase + perkBackgroundWidthIncrement * (num - 1)), perksBackground.sizeDelta.y);
			}
			else
			{
				((Component)perksParent).gameObject.SetActive(false);
			}
		}
		else
		{
			((Component)perksParent).gameObject.SetActive(false);
			for (int num2 = perkIconDisplays.Count - 1; num2 >= 0; num2--)
			{
				perkIconDisplays[num2].Hide();
			}
		}
		perksSpacing.SetActive((enemyAffixIconDisplays != null && enemyAffixIconDisplays.Any((EliteAffixIconDisplay icon) => ((Component)icon).gameObject.activeSelf)) || displayedStatusesCount > 0);
	}

	private void AdjustPerksPoolLength(int perkIndex)
	{
		if (perkIndex > perkIconDisplays.Count - 1)
		{
			PerkIconDisplay perkIconDisplay = Object.Instantiate<PerkIconDisplay>(perkIconDisplayPrefab, ((Component)perksLayoutGroup).transform);
			simpleFontLocalizedParent.AddChilds(perkIconDisplay.LocalizedFonts);
			perkIconDisplays.Add(perkIconDisplay);
			perkIconDisplay.Hovered += OnPerkIconHovered;
			perkIconDisplay.Unhovered += OnPerkIconUnhovered;
		}
		((Transform)perksHover).SetAsLastSibling();
	}

	private void Awake()
	{
		PerkIconDisplay.HighlightSignDisplayed += OnPerkIconHighlightSignDisplayed;
		SkillManager.AttackInfoPanel.PerkIconHidden += OnSkillActionTooltipPerkIconHidden;
		SkillManager.GenericActionInfoPanel.PerkIconHidden += OnSkillActionTooltipPerkIconHidden;
		momentumIconDisplay.Hovered += OnStatusIconDisplayHovered;
		momentumIconDisplay.Unhovered += OnStatusIconDisplayUnhovered;
	}

	private void OnSkillActionTooltipPerkIconHidden(PerkIconDisplay perkIconDisplay)
	{
		for (int num = perkIconDisplays.Count - 1; num >= 0; num--)
		{
			if (!(perkIconDisplays[num].Perk.PerkDefinition.Id != perkIconDisplay.Perk.PerkDefinition.Id))
			{
				perkIconDisplays[num].DisplayHighlightSign(show: false, triggerEvent: false);
			}
		}
	}

	private void OnDestroy()
	{
		PerkIconDisplay.HighlightSignDisplayed -= OnPerkIconHighlightSignDisplayed;
		if (TPSingleton<SkillManager>.Exist())
		{
			SkillManager.AttackInfoPanel.PerkIconHidden -= OnSkillActionTooltipPerkIconHidden;
			SkillManager.GenericActionInfoPanel.PerkIconHidden -= OnSkillActionTooltipPerkIconHidden;
		}
	}

	private void OnPerkIconHighlightSignDisplayed(PerkIconDisplay perkIconDisplay)
	{
		for (int num = perkIconDisplays.Count - 1; num >= 0; num--)
		{
			if (!(perkIconDisplays[num].Perk.PerkDefinition.Id != perkIconDisplay.Perk.PerkDefinition.Id))
			{
				perkIconDisplays[num].DisplayHighlightSign(show: true, triggerEvent: false);
			}
		}
	}

	private void OnPerkIconHovered(PerkIconDisplay perkIconDisplay)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		((Component)perksHover).gameObject.SetActive(true);
		_003F val = perksHover;
		Transform transform = ((Component)perkIconDisplay).transform;
		((Transform)val).position = ((transform is RectTransform) ? transform : null).position;
	}

	private void OnPerkIconUnhovered(PerkIconDisplay perkIconDisplay)
	{
		((Component)perksHover).gameObject.SetActive(false);
	}

	private void OnStatusIconDisplayHovered(UnitModifiersIconDisplay statusIconDisplay)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		((Component)statusesHover).gameObject.SetActive(true);
		_003F val = statusesHover;
		Transform transform = ((Component)statusIconDisplay).transform;
		((Transform)val).position = ((transform is RectTransform) ? transform : null).position;
	}

	private void OnStatusIconDisplayUnhovered(UnitModifiersIconDisplay statusIconDisplay)
	{
		((Component)statusesHover).gameObject.SetActive(false);
	}

	private void RefreshEliteAffixes()
	{
		if (!(unit is EnemyUnit enemyUnit))
		{
			foreach (EliteAffixIconDisplay enemyAffixIconDisplay in enemyAffixIconDisplays)
			{
				enemyAffixIconDisplay.Hide();
			}
			return;
		}
		while (enemyAffixIconDisplays.Count < enemyUnit.Affixes.Count)
		{
			if (enemyAffixIconDisplays.Count > 0)
			{
				enemyAffixSeparators.Add(Object.Instantiate<EnemyAffixSeparator>(enemyAffixSeparatorPrefab, (Transform)(object)enemiesAffixesParent));
			}
			enemyAffixIconDisplays.Add(Object.Instantiate<EliteAffixIconDisplay>(enemyAffixIconDisplayPrefab, (Transform)(object)enemiesAffixesParent));
		}
		int i;
		for (i = 0; i < enemyUnit.Affixes.Count; i++)
		{
			if (i > 0)
			{
				enemyAffixSeparators[i - 1].Toggle(toggle: true);
			}
			enemyAffixIconDisplays[i].Display(enemyUnit.Affixes[i], (i == 0 && enemyUnit is EliteEnemyUnit) ? EnemyAffixEffectDefinition.E_EnemyAffixBoxType.Elite : enemyUnit.Affixes[i].EnemyAffixDefinition.BoxType);
		}
		for (; i < enemyAffixIconDisplays.Count; i++)
		{
			if (i > 0)
			{
				enemyAffixSeparators[i - 1].Toggle(toggle: false);
			}
			enemyAffixIconDisplays[i].Hide();
		}
	}

	private void RefreshGuardian()
	{
		if (unit is EnemyUnit enemyUnit && enemyUnit.IsGuardian)
		{
			guardianIconDisplay.Display(enemyUnit.LinkedBuilding == null);
		}
		else
		{
			guardianIconDisplay.Hide();
		}
	}

	private void RefreshMomentum()
	{
		if (!(unit is PlayableUnit playableUnit) || playableUnit.MomentumTilesActive == 0 || playableUnit.MomentumSkills.Count == 0)
		{
			momentumIconDisplay.Hide();
			return;
		}
		momentumIconDisplay.Display();
		momentumIconDisplay.Refresh(playableUnit);
	}

	private void RefreshStatuses()
	{
		if (unit == null)
		{
			return;
		}
		List<StatusIconDisplay> list = new List<StatusIconDisplay>();
		Status.E_StatusType e_StatusType = unit.StatusOwned;
		displayedStatusesCount = 0;
		int statusInOrderIndex = 0;
		while (statusInOrderIndex < statusesDisplayOrder.Length)
		{
			e_StatusType &= ~statusesDisplayOrder[statusInOrderIndex];
			Status[] array = unit.StatusList.FindAll((Status o) => statusesDisplayOrder[statusInOrderIndex].HasFlag(o.StatusType) && !o.IsFromInjury).ToArray();
			if (array.Length != 0)
			{
				if (!statusIconDisplayPool.TryDequeue(out var result))
				{
					result = Object.Instantiate<StatusIconDisplay>(statusIconDisplayPrefab, (Transform)(object)statusesParent);
					simpleFontLocalizedParent.AddChilds(result.LocalizedFonts);
					result.Hovered += OnStatusIconDisplayHovered;
					result.Unhovered += OnStatusIconDisplayUnhovered;
				}
				result.Display();
				result.Refresh(statusesDisplayOrder[statusInOrderIndex], array);
				list.Add(result);
				displayedStatusesCount++;
			}
			if (e_StatusType == Status.E_StatusType.None)
			{
				break;
			}
			int num = statusInOrderIndex + 1;
			statusInOrderIndex = num;
		}
		StatusIconDisplay result2;
		while (statusIconDisplayPool.TryDequeue(out result2))
		{
			list.Add(result2);
			result2.Hide();
		}
		foreach (StatusIconDisplay item in list)
		{
			statusIconDisplayPool.Enqueue(item);
		}
		((Component)momentumIconDisplay).transform.SetAsLastSibling();
	}

	private void RefreshStatusesBackground()
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		int num = displayedStatusesCount;
		if ((Object)(object)injuryIconDisplay != (Object)null && ((Component)injuryIconDisplay).gameObject.activeSelf)
		{
			num++;
		}
		if ((Object)(object)momentumIconDisplay != (Object)null && ((Component)momentumIconDisplay).gameObject.activeSelf)
		{
			num++;
		}
		if (num > 0)
		{
			((Component)statusesParent).gameObject.SetActive(true);
			statusesBackground.sizeDelta = new Vector2((float)(statusesBackgroundWidthBase + statusesBackgroundWidthIncrement * (num - 1)), statusesBackground.sizeDelta.y);
		}
		else
		{
			((Component)statusesParent).gameObject.SetActive(false);
		}
		((Transform)statusesHover).SetAsLastSibling();
	}

	private void RefreshInjuries()
	{
		if (unit == null)
		{
			injuryIconDisplay?.Hide();
		}
		else if (unit.UnitStatsController.UnitStats.InjuryStage != 0)
		{
			if ((Object)(object)injuryIconDisplay == (Object)null)
			{
				injuryIconDisplay = Object.Instantiate<InjuryIconDisplay>(injuryIconDisplayPrefab, (Transform)(object)statusesParent);
				injuryIconDisplay.Hovered += OnStatusIconDisplayHovered;
				injuryIconDisplay.Unhovered += OnStatusIconDisplayUnhovered;
			}
			((Component)injuryIconDisplay).transform.SetAsFirstSibling();
			((Transform)statusesBackground).SetAsFirstSibling();
			injuryIconDisplay.Refresh(unit);
			injuryIconDisplay.Display();
		}
		else
		{
			injuryIconDisplay?.Hide();
		}
	}

	private void RefreshJoystickNavigation()
	{
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		List<Selectable> list = new List<Selectable>();
		if ((Object)(object)guardianIconDisplay != (Object)null && ((Component)guardianIconDisplay).gameObject.activeSelf)
		{
			list.Add((Selectable)(object)guardianIconDisplay.JoystickSelectable);
		}
		for (int i = 0; i < enemyAffixIconDisplays.Count && ((Component)enemyAffixIconDisplays[i]).gameObject.activeSelf; i++)
		{
			list.Add((Selectable)(object)enemyAffixIconDisplays[i].JoystickSelectable);
		}
		if ((Object)(object)injuryIconDisplay != (Object)null && ((Component)injuryIconDisplay).gameObject.activeSelf)
		{
			list.Add((Selectable)(object)injuryIconDisplay.JoystickSelectable);
		}
		foreach (StatusIconDisplay item in statusIconDisplayPool)
		{
			if (((Component)item).gameObject.activeSelf)
			{
				list.Add((Selectable)(object)item.JoystickSelectable);
			}
		}
		if (((Component)momentumIconDisplay).gameObject.activeSelf)
		{
			list.Add((Selectable)(object)momentumIconDisplay.JoystickSelectable);
		}
		for (int j = 0; j < perkIconDisplays.Count; j++)
		{
			if (((Component)perkIconDisplays[j]).gameObject.activeSelf)
			{
				list.Add((Selectable)(object)perkIconDisplays[j].JoystickSelectable);
			}
		}
		for (int k = 0; k < list.Count; k++)
		{
			Selectable val = list[k];
			SelectableExtensions.SetMode(val, (Mode)4);
			SelectableExtensions.ClearNavigation(val);
			if (k > 0)
			{
				SelectableExtensions.SetSelectOnLeft(val, list[k - 1]);
			}
			if (k < list.Count - 1)
			{
				SelectableExtensions.SetSelectOnRight(val, list[k + 1]);
			}
		}
		JoystickTarget.NavigationEnabled = IsDisplayed();
		JoystickTarget.ClearSelectables();
		JoystickTarget.AddSelectables(list);
		if (list.Count > 0)
		{
			joystickTargetFeedback.sizeDelta = new Vector2(joystickTargetFeedbackMinWidth + ((Component)list[^1]).transform.position.x - ((Component)list[0]).transform.position.x, joystickTargetFeedback.sizeDelta.y);
		}
	}

	private void ToggleLayouts(bool state)
	{
		((Behaviour)containerLayout).enabled = state;
		((Behaviour)enemiesAffixesLayoutGroup).enabled = state;
		((Behaviour)statusesLayoutGroup).enabled = state;
		((Behaviour)perksLayoutGroup).enabled = state;
	}
}
