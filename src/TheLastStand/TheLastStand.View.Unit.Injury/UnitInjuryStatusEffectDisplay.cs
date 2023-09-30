using System;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit;
using TheLastStand.Model.Status;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Unit.Injury;

public class UnitInjuryStatusEffectDisplay : MonoBehaviour
{
	[SerializeField]
	private Image injuryIcon;

	[SerializeField]
	private DataSpriteTable injuryIcons;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private GameObject turnPanel;

	[SerializeField]
	private TextMeshProUGUI remainingTurnsText;

	[SerializeField]
	protected GameObject turnInfiniteIcon;

	private Status status;

	private UnitStatDefinition.E_Stat stat = UnitStatDefinition.E_Stat.Undefined;

	private float modifier;

	public virtual void Init(Status status, int injuryStage)
	{
		this.status = status;
		turnPanel.SetActive(true);
		if ((float)this.status.RemainingTurnsCount == -1f)
		{
			turnInfiniteIcon.SetActive(true);
			((Component)remainingTurnsText).gameObject.SetActive(false);
		}
		else
		{
			turnInfiniteIcon.SetActive(false);
			((Component)remainingTurnsText).gameObject.SetActive(true);
			((TMP_Text)remainingTurnsText).text = status.RemainingTurnsCount.ToString();
		}
		if (status is StatModifierStatus statModifierStatus)
		{
			stat = statModifierStatus.Stat;
			modifier = statModifierStatus.ModifierValue;
		}
		injuryIcon.sprite = injuryIcons.GetSpriteAt(injuryStage - 1);
		RefreshTitle();
	}

	private void Awake()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Combine((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
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
			RefreshTitle();
		}
	}

	private void RefreshTitle()
	{
		if (stat != UnitStatDefinition.E_Stat.Undefined)
		{
			((TMP_Text)titleText).text = string.Format("{0}{1}{2} <sprite name={3}>{4}", (modifier >= 0f) ? "+" : string.Empty, modifier, stat.ShownAsPercentage() ? "%" : string.Empty, stat, UnitDatabase.UnitStatDefinitions[stat].Name);
		}
		else
		{
			((TMP_Text)titleText).text = $"<sprite name={status.StatusType}>{status.Name} {((status is PoisonStatus poisonStatus) ? $"<color=#FF0000>({poisonStatus.DamagePerTurn})</color>" : string.Empty)}";
		}
	}
}
