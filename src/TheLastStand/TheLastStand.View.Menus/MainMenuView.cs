using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Rewired;
using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Log;
using TPLib.Yield;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager;
using TheLastStand.Manager.Sound;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model;
using TheLastStand.Serialization;
using TheLastStand.View.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.Menus;

public class MainMenuView : TPSingleton<MainMenuView>
{
	public static class Constants
	{
		public const string ContinueGameButtonKey = "MainMenu_ContinueGameButton";

		public const string ContinueGameInfoValidKey = "MainMenu_ContinueGameInfo_Valid";

		public const string ContinueGameInfoValidNoApocalypseKey = "MainMenu_ContinueGameInfo_ValidNoApocalypse";

		public const string ContinueGameInfoOutdatedKey = "MainMenu_ContinueGameInfo_Outdated";

		public const string ContinueGameInfoMissingModKey = "MainMenu_ContinueGameInfo_MissingMod";

		public const string ContinueGameInfoCorruptedKey = "MainMenu_ContinueGameInfo_Corrupted";

		public const string ContinueCampaignButtonKey = "MainMenu_ContinueCampaignButton";

		public const string NewCampaignButtonKey = "MainMenu_NewCampaignButton";

		public const string PopupOutdatedGameSaveTitleKey = "MainMenu_Popup_OutdatedGameSave_Title";

		public const string PopupOutdatedGameSaveTextKey = "MainMenu_Popup_OutdatedGameSave_Text";

		public const string PopupMissingModGameSaveTitleKey = "MainMenu_Popup_MissingModGameSave_Title";

		public const string PopupMissingModGameSaveTextKey = "MainMenu_Popup_MissingModGameSave_Text";

		public const string PopupWillCorruptGameSaveTitleKey = "MainMenu_Popup_WillCorruptGameSave_Title";

		public const string PopupWillCorruptGameSaveTextKey = "MainMenu_Popup_WillCorruptGameSave_Text";

		public const string KeyRemappingChangesDisclaimerTitle = "KeyRemappingChanges_Disclaimer_Title";

		public const string SettingsChangesDisclaimerTitle = "SettingsChanges_Disclaimer_Title";
	}

	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static Func<SettingsManager.SettingsChangesNoteLocalizationKeys, string> _003C_003E9__9_0;

		public static Func<SettingsManager.SettingsChangesNoteLocalizationKeys, string> _003C_003E9__9_1;

		public static Func<SettingsManager.SettingsChangesNoteLocalizationKeys, string[]> _003C_003E9__9_2;

		public static UnityAction _003C_003E9__10_0;

		public static TweenCallback _003C_003E9__14_0;

		internal string _003COpenSettingsChangesWarning_003Eb__9_0(SettingsManager.SettingsChangesNoteLocalizationKeys o)
		{
			return o.TitleKey;
		}

		internal string _003COpenSettingsChangesWarning_003Eb__9_1(SettingsManager.SettingsChangesNoteLocalizationKeys o)
		{
			return o.TextKey;
		}

		internal string[] _003COpenSettingsChangesWarning_003Eb__9_2(SettingsManager.SettingsChangesNoteLocalizationKeys o)
		{
			return o.FormatArgs;
		}

		internal void _003COnContinueClick_003Eb__10_0()
		{
			SaveManager.CorruptGameSave();
			ApplicationManager.Application.ApplicationController.SetState("LoadWorldMap");
		}

		internal void _003COnQuitClick_003Eb__14_0()
		{
			ApplicationManager.Application.ApplicationController.SetState("ExitApp");
		}
	}

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private GraphicRaycaster graphicRaycaster;

	[SerializeField]
	private TextMeshProUGUI buildVersionText;

	[SerializeField]
	private TextMeshProUGUI continueText;

	[SerializeField]
	private TextMeshProUGUI continueInfoText;

	[SerializeField]
	private GameObject openingSequenceButton;

	[SerializeField]
	private Selectable optionToggle;

	[SerializeField]
	private Selectable[] mainMenuToggles;

	[SerializeField]
	private GameObject[] disabledWhenUsingController;

	public static void OpenSettingsChangesWarning()
	{
		if (TPSingleton<SettingsManager>.Instance.SettingsChangesWarningsToDisplay.Count > 0)
		{
			GenericPopUp.Open(TPSingleton<SettingsManager>.Instance.SettingsChangesWarningsToDisplay.Select((SettingsManager.SettingsChangesNoteLocalizationKeys o) => o.TitleKey).ToList(), TPSingleton<SettingsManager>.Instance.SettingsChangesWarningsToDisplay.Select((SettingsManager.SettingsChangesNoteLocalizationKeys o) => o.TextKey).ToList(), TPSingleton<SettingsManager>.Instance.SettingsChangesWarningsToDisplay.Select((SettingsManager.SettingsChangesNoteLocalizationKeys o) => o.FormatArgs).ToList());
			TPSingleton<SettingsManager>.Instance.SettingsChangesWarningsToDisplay.Clear();
		}
	}

	public void OnContinueClick()
	{
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Expected O, but got Unknown
		if ((Object)(object)EventSystem.current.currentSelectedGameObject != (Object)null)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: false);
			EventSystem.current.SetSelectedGameObject((GameObject)null);
		}
		((Behaviour)graphicRaycaster).enabled = false;
		if (ApplicationManager.Application.ApplicationQuitInOraculum)
		{
			ApplicationManager.Application.ApplicationController.SetState("MetaShops");
		}
		else if (SaveManager.GameSaveExists)
		{
			SaverLoader.SerializedContainerLoadingInfo<SerializedGameState> preloadedGameSave = TPSingleton<SaveManager>.Instance.PreloadedGameSave;
			if (preloadedGameSave == null || preloadedGameSave.FailedLoadsInfo[0].Reason != SaveManager.E_BrokenSaveReason.WRONG_VERSION)
			{
				SaverLoader.SerializedContainerLoadingInfo<SerializedGameState> preloadedGameSave2 = TPSingleton<SaveManager>.Instance.PreloadedGameSave;
				if (preloadedGameSave2 == null || !preloadedGameSave2.FailedLoadsInfo[1].Reason.HasValue)
				{
					ApplicationManager.Application.ApplicationController.SetState("LoadGame");
					return;
				}
			}
			object obj = _003C_003Ec._003C_003E9__10_0;
			if (obj == null)
			{
				UnityAction val = delegate
				{
					SaveManager.CorruptGameSave();
					ApplicationManager.Application.ApplicationController.SetState("LoadWorldMap");
				};
				_003C_003Ec._003C_003E9__10_0 = val;
				obj = (object)val;
			}
			GenericBlockingPopup.OpenAsSimple("MainMenu_Popup_WillCorruptGameSave_Title", "MainMenu_Popup_WillCorruptGameSave_Text", (UnityAction)obj);
		}
		else if (ApplicationManager.Application.RunsCompleted == 0)
		{
			ApplicationManager.Application.HasSeenIntroduction = true;
			AnimatedCutsceneManager.PlayPreGameAnimatedCutscene("NewGame");
		}
		else
		{
			ApplicationManager.Application.ApplicationController.SetState("LoadWorldMap");
		}
	}

	public void OnCreditsClick()
	{
		if ((Object)(object)EventSystem.current.currentSelectedGameObject != (Object)null)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: false);
			EventSystem.current.SetSelectedGameObject((GameObject)null);
		}
		((Behaviour)graphicRaycaster).enabled = false;
		ApplicationManager.Application.ApplicationController.SetState("Credits");
	}

	public void OnOpeningSequenceClick()
	{
		if ((Object)(object)EventSystem.current.currentSelectedGameObject != (Object)null)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: false);
			EventSystem.current.SetSelectedGameObject((GameObject)null);
		}
		((Behaviour)graphicRaycaster).enabled = false;
		ApplicationManager.Application.HasSeenIntroduction = true;
		AnimatedCutsceneManager.PlayPreGameAnimatedCutscene("GameLobby");
	}

	public void OnOptionsClick()
	{
		if (SettingsManager.CanOpenSettings())
		{
			ApplicationManager.Application.ApplicationController.SetState("Settings");
			if (InputManager.IsLastControllerJoystick)
			{
				TPSingleton<HUDJoystickNavigationManager>.Instance.OpenHUDNavigationMode(selectDefaultPanel: false);
				TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(TPSingleton<SettingsManager>.Instance.SettingsPanel.JoystickTarget.GetSelectionInfo());
			}
		}
	}

	public void OnQuitClick()
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Expected O, but got Unknown
		if ((Object)(object)EventSystem.current.currentSelectedGameObject != (Object)null)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: false);
			EventSystem.current.SetSelectedGameObject((GameObject)null);
		}
		((Behaviour)graphicRaycaster).enabled = false;
		TweenerCore<float, float, FloatOptions> obj = TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTweenModuleUI.DOFade(canvasGroup, 0f, 0.5f), (Ease)4);
		object obj2 = _003C_003Ec._003C_003E9__14_0;
		if (obj2 == null)
		{
			TweenCallback val = delegate
			{
				ApplicationManager.Application.ApplicationController.SetState("ExitApp");
			};
			_003C_003Ec._003C_003E9__14_0 = val;
			obj2 = (object)val;
		}
		TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(obj, (TweenCallback)obj2);
	}

	public void Refresh()
	{
		RefreshContinueText();
	}

	public void JoystickSelectOptionToggle()
	{
		EventSystem.current.SetSelectedGameObject(((Component)optionToggle).gameObject);
	}

	private void InitJoystickNavigation()
	{
		List<Selectable> list = new List<Selectable>();
		for (int i = 0; i < mainMenuToggles.Length; i++)
		{
			if (((Component)mainMenuToggles[i]).gameObject.activeInHierarchy && mainMenuToggles[i].IsInteractable())
			{
				list.Add(mainMenuToggles[i]);
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			list[j].SetMode((Mode)4);
			if (j > 0)
			{
				list[j].SetSelectOnUp(list[j - 1]);
			}
			if (j < list.Count - 1)
			{
				list[j].SetSelectOnDown(list[j + 1]);
			}
		}
	}

	private void OnDestroy()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Remove((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
		if (TPSingleton<InputManager>.Exist())
		{
			TPSingleton<InputManager>.Instance.LastActiveControllerChanged -= OnLastActiveControllerChanged;
		}
	}

	private void OnLastActiveControllerChanged(ControllerType controllerType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Invalid comparison between Unknown and I4
		if ((int)controllerType == 2 && (Object)(object)EventSystem.current.currentSelectedGameObject == (Object)null)
		{
			EventSystem.current.SetSelectedGameObject(((Component)mainMenuToggles[0]).gameObject);
		}
		for (int i = 0; i < disabledWhenUsingController.Length; i++)
		{
			disabledWhenUsingController[i].SetActive((int)controllerType != 2);
		}
	}

	private void OnLocalize()
	{
		RefreshContinueText();
	}

	private void RefreshContinueText()
	{
		((Component)continueInfoText).gameObject.SetActive(SaveManager.GameSaveExists);
		if (SaveManager.GameSaveExists)
		{
			string name = TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Name;
			string text = (TPSingleton<WorldMapCityManager>.Instance.SelectedCity.NumberOfRuns + 1).ToString();
			string arg = name + ((TPSingleton<WorldMapCityManager>.Instance.SelectedCity.NumberOfRuns > 0) ? (" #" + text) : string.Empty);
			((TMP_Text)continueText).text = string.Format(Localizer.Get("MainMenu_ContinueGameButton"), arg);
			if (TPSingleton<SaveManager>.Instance.PreloadedGameSave?.LoadedContainer != null)
			{
				SerializedGameState loadedContainer = TPSingleton<SaveManager>.Instance.PreloadedGameSave.LoadedContainer;
				TimeSpan timeSpan = TimeSpan.FromSeconds(loadedContainer.TotalTimeSpent);
				string text2 = Localizer.Get((loadedContainer.Game.Cycle == Game.E_Cycle.Night) ? "Cycle_Night" : "Cycle_Day");
				((TMP_Text)continueInfoText).text = (ApocalypseManager.IsApocalypseUnlocked ? string.Format(Localizer.Get("MainMenu_ContinueGameInfo_Valid"), text2, loadedContainer.Apocalypse.ApocalypseIndex, loadedContainer.Game.DayNumber, timeSpan.ToString("hh\\:mm\\:ss")) : string.Format(Localizer.Get("MainMenu_ContinueGameInfo_ValidNoApocalypse"), text2, loadedContainer.Game.DayNumber, timeSpan.ToString("hh\\:mm\\:ss")));
			}
			else
			{
				TextMeshProUGUI val = continueInfoText;
				((TMP_Text)val).text = TPSingleton<SaveManager>.Instance.PreloadedGameSave?.FailedLoadsInfo[0].Reason switch
				{
					SaveManager.E_BrokenSaveReason.WRONG_VERSION => string.Format(Localizer.Get("MainMenu_ContinueGameInfo_Outdated")), 
					SaveManager.E_BrokenSaveReason.MISSING_MOD => string.Format(Localizer.Get("MainMenu_ContinueGameInfo_MissingMod")), 
					_ => string.Format(Localizer.Get("MainMenu_ContinueGameInfo_Corrupted")), 
				};
			}
		}
		else if (ApplicationManager.Application.RunsCompleted == 0)
		{
			((TMP_Text)continueText).text = Localizer.Get("MainMenu_NewCampaignButton");
		}
		else
		{
			((TMP_Text)continueText).text = Localizer.Get("MainMenu_ContinueCampaignButton");
		}
	}

	private IEnumerator SelectFirstOptionEndOfFrame()
	{
		yield return SharedYields.WaitForEndOfFrame;
		EventSystem.current.SetSelectedGameObject(((Component)mainMenuToggles[0]).gameObject);
	}

	private void Start()
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Expected O, but got Unknown
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Expected O, but got Unknown
		TPSingleton<SoundManager>.Instance.FadeMusic(TPSingleton<SoundManager>.Instance.MenuMusic);
		TPSingleton<InputManager>.Instance.LastActiveControllerChanged += OnLastActiveControllerChanged;
		((TMP_Text)buildVersionText).text = ApplicationManager.VersionString;
		OpenSettingsChangesWarning();
		openingSequenceButton.SetActive(ApplicationManager.Application.HasSeenIntroduction);
		OnLocalize();
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Combine((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
		InitJoystickNavigation();
		if (InputManager.IsLastControllerJoystick)
		{
			((MonoBehaviour)this).StartCoroutine(SelectFirstOptionEndOfFrame());
		}
	}

	private void Update()
	{
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		if (SaveManager.BrokenSavePath != null && UIManager.DisplayGameSaveErrorPopUp(SaveManager.BrokenSavePath, SaveManager.BrokenSaveReason, SaveManager.BackupLoaded))
		{
			if (!SaveManager.BackupLoaded)
			{
				((CLogger<SaveManager>)TPSingleton<SaveManager>.Instance).LogError((object)"===============================[ LOGBAR ]===============================", (CLogLevel)1, true, false);
				((CLogger<SaveManager>)TPSingleton<SaveManager>.Instance).LogError((object)"Hello again - from now on, you can stop ignoring NullRefs.", (CLogLevel)1, true, false);
			}
			SaveManager.BrokenSavePath = null;
			SaveManager.BackupLoaded = false;
		}
		if (SaveManager.NewlyPreloadedGameSave)
		{
			switch (TPSingleton<SaveManager>.Instance.PreloadedGameSave?.FailedLoadsInfo[0].Reason)
			{
			case SaveManager.E_BrokenSaveReason.WRONG_VERSION:
				GenericPopUp.Open("MainMenu_Popup_OutdatedGameSave_Title", "MainMenu_Popup_OutdatedGameSave_Text");
				break;
			case SaveManager.E_BrokenSaveReason.MISSING_MOD:
			{
				string[] array = new string[1] { string.Join(", ", (TPSingleton<SaveManager>.Instance.PreloadedGameSave.FailedLoadsInfo[0].Exception as SaverLoader.MissingModException).MissingModIds) };
				ParameterizedLocalizationLine titleLocKey = new ParameterizedLocalizationLine("MainMenu_Popup_MissingModGameSave_Title", Array.Empty<string>());
				ParameterizedLocalizationLine textLocKey = default(ParameterizedLocalizationLine);
				((ParameterizedLocalizationLine)(ref textLocKey))._002Ector("MainMenu_Popup_MissingModGameSave_Text", array);
				GenericPopUp.Open(titleLocKey, textLocKey);
				break;
			}
			}
			SaveManager.NewlyPreloadedGameSave = false;
		}
	}
}
