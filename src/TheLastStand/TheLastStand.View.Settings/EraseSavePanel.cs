using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Log;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Manager.Meta;
using TheLastStand.View.Generic;
using TheLastStand.View.Menus;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.Settings;

public class EraseSavePanel : SettingsFieldPanel
{
	[SerializeField]
	private BetterButton eraseSaveButton;

	[SerializeField]
	private GenericTooltipDisplayer genericTooltipDisplayer;

	public Selectable Selectable => (Selectable)(object)eraseSaveButton;

	public override void Refresh()
	{
		base.Refresh();
		eraseSaveButton.Interactable = !ScenesManager.IsActiveSceneLevel();
		((Behaviour)genericTooltipDisplayer).enabled = ScenesManager.IsActiveSceneLevel();
	}

	protected override void RefreshLocalizedTexts()
	{
		((TMP_Text)labelText).text = Localizer.Get("Settings_EraseSave");
	}

	protected override void Awake()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		base.Awake();
		Refresh();
		((UnityEvent)((Button)eraseSaveButton).onClick).AddListener(new UnityAction(OpenConsentPopup));
	}

	protected override void OnDestroy()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		base.OnDestroy();
		((UnityEvent)((Button)eraseSaveButton).onClick).RemoveListener(new UnityAction(OpenConsentPopup));
	}

	private void EraseSaves()
	{
		SaveManager.EraseSave();
		GameManager.EraseSave();
		SaveManager.Load();
		ApocalypseManager.SetApocalypse(0);
		GlyphManager.ResetSelectedGlyphs();
		SaveManager.SafeLoadGameSave();
		TPSingleton<MainMenuView>.Instance.Refresh();
		if (SettingsManager.CanCloseSettings())
		{
			SettingsManager.CloseSettings();
		}
		((CLogger<SettingsManager>)TPSingleton<SettingsManager>.Instance).Log((object)"Saves has been erased!", (CLogLevel)1, false, false);
	}

	private void OnCancel()
	{
		OnConsentPopupClosed();
	}

	private void OnConfirm()
	{
		EraseSaves();
		OnConsentPopupClosed();
	}

	private void OnConsentPopupClosed()
	{
		if (InputManager.IsLastControllerJoystick)
		{
			TPSingleton<SettingsManager>.Instance.SettingsPanel.OnEraseSavePopupClosed();
		}
	}

	private void OpenConsentPopup()
	{
		GenericConsent.Open("Settings_ConfirmEraseSaveText", OnConfirm, OnCancel);
	}
}
