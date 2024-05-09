using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Localization.Fonts;
using TheLastStand.Definition.Unit.Enemy.Affix;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Model.Status;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.View.HUD.BottomScreenPanel.UnitManagement;
using UnityEngine;
using UnityEngine.EventSystems;
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
	private LayoutGroup[] perksLayoutGroups;

	[SerializeField]
	private ModifiersLayoutViewAllPerksView allPerksView;

	[SerializeField]
	private HUDJoystickSimpleTarget joystickTarget;

	[SerializeField]
	private RectTransform joystickTargetFeedback;

	[SerializeField]
	private float joystickTargetFeedbackMinWidth = 116f;

	[SerializeField]
	private float joystickTargetFeedbackMinHeight = 110f;

	[SerializeField]
	private float joystickTargetFeedbackTwoLinesLayoutHeight = 190f;

	private List<EliteAffixIconDisplay> enemyAffixIconDisplays = new List<EliteAffixIconDisplay>();

	private List<EnemyAffixSeparator> enemyAffixSeparators = new List<EnemyAffixSeparator>();

	private int displayedStatusesCount;

	private TheLastStand.Model.Unit.Unit unit;

	private InjuryIconDisplay injuryIconDisplay;

	private readonly ConcurrentQueue<StatusIconDisplay> statusIconDisplayPool = new ConcurrentQueue<StatusIconDisplay>();

	public HUDJoystickSimpleTarget JoystickTarget => joystickTarget;

	public bool IsDisplayed()
	{
		if ((!((Object)(object)guardianIconDisplay != (Object)null) || !((Component)guardianIconDisplay).gameObject.activeSelf) && (enemyAffixIconDisplays == null || !enemyAffixIconDisplays.Any((EliteAffixIconDisplay icon) => ((Component)icon).gameObject.activeSelf)) && (!((Object)(object)injuryIconDisplay != (Object)null) || !((Component)injuryIconDisplay).gameObject.activeSelf) && !statusIconDisplayPool.Any((StatusIconDisplay o) => ((Component)o).gameObject.activeSelf))
		{
			return allPerksView.IsDisplayed();
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
		if (unit is PlayableUnit playableUnit)
		{
			List<Perk> list = new List<Perk>();
			List<Perk> list2 = new List<Perk>();
			List<Perk> list3 = new List<Perk>();
			foreach (Perk value in playableUnit.Perks.Values)
			{
				if (value.Unlocked && value.DisplayInHUD(out var _))
				{
					if (value.IsFromRace)
					{
						list.Add(value);
					}
					else if (value.IsNative)
					{
						list3.Add(value);
					}
					else if (value.OnlyUnlockedByItem)
					{
						list2.Add(value);
					}
					else
					{
						list.Add(value);
					}
				}
			}
			allPerksView.PerksTreeView.Refresh(list);
			allPerksView.PerksItemView.Refresh(list2);
			allPerksView.PerksOmenView.Refresh(list3);
			((Component)perksParent).gameObject.SetActive(allPerksView.IsDisplayed());
		}
		else
		{
			((Component)perksParent).gameObject.SetActive(false);
			allPerksView.HideAllPerkDisplays();
		}
		perksSpacing.SetActive((enemyAffixIconDisplays != null && enemyAffixIconDisplays.Any((EliteAffixIconDisplay icon) => ((Component)icon).gameObject.activeSelf)) || displayedStatusesCount > 0);
	}

	private void Awake()
	{
		momentumIconDisplay.Hovered += OnStatusIconDisplayHovered;
		momentumIconDisplay.Unhovered += OnStatusIconDisplayUnhovered;
		TileObjectSelectionManager.OnUnitSelectionChange += OnNewUnitSelected;
	}

	private void OnDestroy()
	{
		if ((Object)(object)momentumIconDisplay != (Object)null)
		{
			momentumIconDisplay.Hovered -= OnStatusIconDisplayHovered;
			momentumIconDisplay.Unhovered -= OnStatusIconDisplayUnhovered;
		}
		TileObjectSelectionManager.OnUnitSelectionChange -= OnNewUnitSelected;
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
		if (unit is EnemyUnit { IsGuardian: not false } enemyUnit)
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
		if (!(unit is PlayableUnit { MomentumTilesActive: not 0 } playableUnit) || playableUnit.MomentumSkills.Count == 0)
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
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Unknown result type (might be due to invalid IL or missing references)
		//IL_029c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0319: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
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
		bool flag = list.Count > 0;
		for (int j = 0; j < list.Count; j++)
		{
			Selectable selectable = list[j];
			selectable.SetMode((Mode)4);
			selectable.ClearNavigation();
			if (j > 0)
			{
				selectable.SetSelectOnLeft(list[j - 1]);
			}
			if (j < list.Count - 1)
			{
				selectable.SetSelectOnRight(list[j + 1]);
			}
		}
		bool flag2 = allPerksView.IsDisplayed();
		allPerksView.RefreshJoystickNavigation();
		if (flag && flag2)
		{
			Selectable val = (Selectable)(object)allPerksView.GetBottomLeftPerkIconDisplay()?.JoystickSelectable;
			if ((Object)(object)val != (Object)null)
			{
				list[^1].SetSelectOnRight(val);
				val.SetSelectOnLeft(list[^1]);
			}
		}
		JoystickTarget.NavigationEnabled = IsDisplayed();
		JoystickTarget.ClearSelectables();
		List<Selectable> list2 = list;
		list2.AddRange(allPerksView.GetAllSelectables());
		JoystickTarget.AddSelectables(list2);
		if (flag || list2.Count > 0)
		{
			float num = joystickTargetFeedbackMinHeight;
			if (allPerksView.HasTwoPerksLines())
			{
				num = joystickTargetFeedbackTwoLinesLayoutHeight;
			}
			float num2 = 0f;
			float x;
			if (flag)
			{
				x = ((Component)list[0]).transform.position.x;
				num2 = ((Component)list[^1]).transform.position.x;
			}
			else
			{
				x = ((Component)allPerksView.GetBottomLeftPerkIconDisplay()).transform.position.x;
			}
			PerkIconDisplay farRightPerkIconDisplay = allPerksView.GetFarRightPerkIconDisplay();
			if ((Object)(object)farRightPerkIconDisplay != (Object)null && ((Component)farRightPerkIconDisplay).transform.position.x > num2)
			{
				num2 = ((Component)farRightPerkIconDisplay).transform.position.x;
			}
			joystickTargetFeedback.sizeDelta = new Vector2(joystickTargetFeedbackMinWidth + num2 - x, num);
		}
	}

	private void ToggleLayouts(bool state)
	{
		((Behaviour)containerLayout).enabled = state;
		((Behaviour)enemiesAffixesLayoutGroup).enabled = state;
		((Behaviour)statusesLayoutGroup).enabled = state;
		LayoutGroup[] array = perksLayoutGroups;
		for (int i = 0; i < array.Length; i++)
		{
			((Behaviour)array[i]).enabled = state;
		}
	}

	private void OnNewUnitSelected()
	{
		if (!InputManager.IsLastControllerJoystick || !((Component)joystickTargetFeedback).gameObject.activeSelf)
		{
			return;
		}
		if (joystickTarget.IsSelectable())
		{
			Selectable selectable = joystickTarget.GetSelectionInfo().Selectable;
			if ((Object)(object)selectable != (Object)null)
			{
				EventSystem.current.SetSelectedGameObject(((Component)selectable).gameObject);
			}
			return;
		}
		if (TileObjectSelectionManager.HasPlayableUnitSelected && TPSingleton<PlayableUnitManagementView>.Instance.Displayed && TPSingleton<PlayableUnitManagementView>.Instance.JoystickTarget.IsSelectable())
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(TPSingleton<PlayableUnitManagementView>.Instance.JoystickTarget.GetSelectionInfo());
		}
		if (TileObjectSelectionManager.HasEnemyUnitSelected && TPSingleton<EnemyUnitManagementView>.Instance.Displayed && TPSingleton<EnemyUnitManagementView>.Instance.JoystickTarget.IsSelectable())
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(TPSingleton<EnemyUnitManagementView>.Instance.JoystickTarget.GetSelectionInfo());
		}
	}
}
