using System;
using System.Collections;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Controller;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using UnityEngine;

namespace TheLastStand.View.HUD;

public class TurnMainPanel : MonoBehaviour
{
	private static class AnimatorParameters
	{
		public static string ProductionPhase = "ProductionPhase";

		public static string DeploymentPhase = "DeploymentPhase";

		public static string NightTurnEnemies = "NightTurnEnemies";

		public static string NightTurnHeroes = "NightTurnHeroes";
	}

	[SerializeField]
	private Animator backgroundAnimator;

	[SerializeField]
	private GameObject dayTextBoxGameObject;

	[SerializeField]
	private TextMeshProUGUI dayCycleText;

	[SerializeField]
	private TextMeshProUGUI dayCycleNoNumberText;

	[SerializeField]
	private TextMeshProUGUI dayCycleNumberText;

	[SerializeField]
	private TextMeshProUGUI dayPhaseNameText;

	[SerializeField]
	private GameObject nightTextBoxGameObject;

	[SerializeField]
	private TextMeshProUGUI nightCycleText;

	[SerializeField]
	private TextMeshProUGUI nightCycleNumberText;

	[SerializeField]
	private TextMeshProUGUI nightTurnText;

	[SerializeField]
	private TextMeshProUGUI nightTurnNumberText;

	[SerializeField]
	private TextMeshProUGUI nightPhaseNameText;

	[SerializeField]
	private BetterButton nightEndTurnButton;

	[SerializeField]
	private BetterButton dayEndTurnButton;

	public void OnEndTurnButtonClick()
	{
		GameManager.HandleEndTurnInput();
	}

	public void Refresh()
	{
		((Component)dayEndTurnButton).gameObject.SetActive(TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Day);
		((Component)nightEndTurnButton).gameObject.SetActive(TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night);
		switch (TPSingleton<GameManager>.Instance.Game.Cycle)
		{
		case Game.E_Cycle.Day:
			nightTextBoxGameObject.SetActive(false);
			dayTextBoxGameObject.SetActive(true);
			if (TPSingleton<GameManager>.Instance.Game.DayNumber > 0)
			{
				((Component)dayCycleText).gameObject.SetActive(true);
				((Component)dayCycleNoNumberText).gameObject.SetActive(false);
				((TMP_Text)dayCycleText).text = Localizer.Get("Cycle_Day") ?? "";
			}
			else
			{
				((Component)dayCycleText).gameObject.SetActive(false);
				((Component)dayCycleNoNumberText).gameObject.SetActive(true);
				((TMP_Text)dayCycleNoNumberText).text = Localizer.Get("Cycle_Day") ?? "";
			}
			((TMP_Text)dayCycleNumberText).text = ((TPSingleton<GameManager>.Instance.Game.DayNumber > 0) ? TPSingleton<GameManager>.Instance.Game.DayNumber.ToString() : string.Empty);
			switch (TPSingleton<GameManager>.Instance.Game.DayTurn)
			{
			case Game.E_DayTurn.Production:
				((TMP_Text)dayPhaseNameText).text = Localizer.Get("TurnInfoPanelTitle_ProductionPhase");
				backgroundAnimator.SetBool(AnimatorParameters.ProductionPhase, true);
				backgroundAnimator.SetBool(AnimatorParameters.DeploymentPhase, false);
				backgroundAnimator.SetBool(AnimatorParameters.NightTurnEnemies, false);
				backgroundAnimator.SetBool(AnimatorParameters.NightTurnHeroes, false);
				break;
			case Game.E_DayTurn.Deployment:
				((TMP_Text)dayPhaseNameText).text = Localizer.Get("TurnInfoPanelTitle_DeploymentPhase");
				backgroundAnimator.SetBool(AnimatorParameters.ProductionPhase, false);
				backgroundAnimator.SetBool(AnimatorParameters.DeploymentPhase, true);
				backgroundAnimator.SetBool(AnimatorParameters.NightTurnEnemies, false);
				backgroundAnimator.SetBool(AnimatorParameters.NightTurnHeroes, false);
				break;
			}
			break;
		case Game.E_Cycle.Night:
			dayTextBoxGameObject.SetActive(false);
			nightTextBoxGameObject.SetActive(true);
			if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Management)
			{
				((MonoBehaviour)this).StopCoroutine(WaitForDyingEnemiesThenUnlockNightEndTurnButton());
				((MonoBehaviour)this).StartCoroutine(WaitForDyingEnemiesThenUnlockNightEndTurnButton());
			}
			else
			{
				nightEndTurnButton.Interactable = GameController.CanEndPlayerTurn();
			}
			((TMP_Text)nightCycleText).text = Localizer.Get("Cycle_Night");
			((TMP_Text)nightCycleNumberText).text = TPSingleton<GameManager>.Instance.Game.DayNumber.ToString();
			((TMP_Text)nightTurnText).text = Localizer.Get("Phase_Turn");
			((TMP_Text)nightTurnNumberText).text = TPSingleton<GameManager>.Instance.Game.CurrentNightHour.ToString();
			switch (TPSingleton<GameManager>.Instance.Game.NightTurn)
			{
			case Game.E_NightTurn.PlayableUnits:
				((TMP_Text)nightPhaseNameText).text = Localizer.Get("TurnInfoPanelTitle_PlayerTurn");
				backgroundAnimator.SetBool(AnimatorParameters.ProductionPhase, false);
				backgroundAnimator.SetBool(AnimatorParameters.DeploymentPhase, false);
				backgroundAnimator.SetBool(AnimatorParameters.NightTurnEnemies, false);
				backgroundAnimator.SetBool(AnimatorParameters.NightTurnHeroes, true);
				break;
			case Game.E_NightTurn.EnemyUnits:
				((TMP_Text)nightPhaseNameText).text = Localizer.Get("TurnInfoPanelTitle_EnemyTurn");
				backgroundAnimator.SetBool(AnimatorParameters.ProductionPhase, false);
				backgroundAnimator.SetBool(AnimatorParameters.DeploymentPhase, false);
				backgroundAnimator.SetBool(AnimatorParameters.NightTurnEnemies, true);
				backgroundAnimator.SetBool(AnimatorParameters.NightTurnHeroes, false);
				break;
			}
			break;
		}
	}

	private void Awake()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Combine((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(RefreshLocalizedTexts));
	}

	private void OnDestroy()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Remove((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(RefreshLocalizedTexts));
	}

	private void RefreshLocalizedTexts()
	{
		((TMP_Text)dayCycleText).text = Localizer.Get("Cycle_Day") ?? "";
		((TMP_Text)dayCycleNoNumberText).text = Localizer.Get("Cycle_Day") ?? "";
		TextMeshProUGUI val = dayPhaseNameText;
		((TMP_Text)val).text = TPSingleton<GameManager>.Instance.Game.DayTurn switch
		{
			Game.E_DayTurn.Production => Localizer.Get("TurnInfoPanelTitle_ProductionPhase"), 
			Game.E_DayTurn.Deployment => Localizer.Get("TurnInfoPanelTitle_DeploymentPhase"), 
			_ => ((TMP_Text)dayPhaseNameText).text, 
		};
	}

	private IEnumerator WaitForDyingEnemiesThenUnlockNightEndTurnButton()
	{
		bool canUnlockButton = false;
		yield return (object)new WaitUntil((Func<bool>)delegate
		{
			if (TPSingleton<GameManager>.Instance.Game.State != Game.E_State.Management || TPSingleton<GameManager>.Instance.Game.Cycle != Game.E_Cycle.Night)
			{
				return true;
			}
			if (!EnemyUnitManager.IsThereAnyEnemyDying())
			{
				canUnlockButton = true;
				return true;
			}
			return false;
		});
		if (canUnlockButton)
		{
			nightEndTurnButton.Interactable = GameController.CanEndPlayerTurn();
		}
	}
}
