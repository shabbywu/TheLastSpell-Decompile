using TPLib;
using TheLastStand.Model.Unit;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.Unit.Stat;

public class StatLevelUpDisplay : UnitStatDisplay
{
	[SerializeField]
	private GameObject normalDisplayGameObject;

	[SerializeField]
	private GameObject maxReachedDisplayGameObject;

	[SerializeField]
	private Button upgradeButton;

	[Tooltip("Color the 'Texts To Color' this way if the max BaseValue has been reached")]
	[SerializeField]
	private DataColor maxReachedColor;

	private bool maxReached;

	private bool MaxReached
	{
		get
		{
			return maxReached;
		}
		set
		{
			if (maxReached != value)
			{
				maxReached = value;
				RefreshColor();
				RefreshMaxReachedDependantStuff();
			}
		}
	}

	protected override void CacheStatValues()
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		base.CacheStatValues();
		float num = ((base.SecondaryStatDefinition == null) ? Mathf.Floor(base.TargetUnit.UnitStatsController.GetStat(base.StatDefinition.Id).Boundaries.y) : Mathf.Floor(base.TargetUnit.UnitStatsController.GetStat(base.SecondaryStatDefinition.Id).Boundaries.y));
		MaxReached = base.TargetUnit.UnitStatsController.GetStat(base.SecondaryStatDefinition?.Id ?? base.StatDefinition.Id).Base >= num;
	}

	protected override void RefreshInternal()
	{
		base.RefreshInternal();
		RefreshUpgradeButton();
		if (fullRefreshNeeded)
		{
			RefreshMaxReachedDependantStuff();
		}
	}

	private void RefreshUpgradeButton()
	{
		if ((Object)(object)upgradeButton == (Object)null)
		{
			return;
		}
		if (!valuesCached)
		{
			CacheStatValues();
		}
		((Component)upgradeButton).gameObject.SetActive((base.TargetUnit as PlayableUnit).StatsPoints > 0 && !MaxReached);
		if (fullRefreshNeeded)
		{
			((UnityEventBase)upgradeButton.onClick).RemoveAllListeners();
			if (base.SecondaryStatDefinition == null)
			{
				_ = base.StatDefinition.Id;
			}
			else
			{
				_ = base.SecondaryStatDefinition.Id;
			}
		}
	}

	protected override void RefreshColor()
	{
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		if (labelsToTint == null || labelsToTint.Length == 0)
		{
			return;
		}
		Color? val = null;
		if (!base.ColorOverride.HasValue)
		{
			if ((Object)(object)maxReachedColor != (Object)null && MaxReached)
			{
				val = maxReachedColor._Color;
			}
			else if (useStatColor && useStatColor)
			{
				val = statsColors.GetColorById(base.StatDefinition.Id.ToString());
			}
		}
		else
		{
			val = base.ColorOverride.Value;
		}
		int i = 0;
		for (int num = labelsToTint.Length; i < num; i++)
		{
			if ((Object)(object)labelsToTint[i] != (Object)null)
			{
				((Graphic)labelsToTint[i]).color = (Color)(((_003F?)val) ?? textsOriginalColor[i]);
			}
		}
	}

	private void RefreshMaxReachedDependantStuff()
	{
		if ((Object)(object)normalDisplayGameObject != (Object)null)
		{
			normalDisplayGameObject.SetActive(!MaxReached);
		}
		if ((Object)(object)maxReachedDisplayGameObject != (Object)null)
		{
			maxReachedDisplayGameObject.SetActive(MaxReached);
		}
	}
}
