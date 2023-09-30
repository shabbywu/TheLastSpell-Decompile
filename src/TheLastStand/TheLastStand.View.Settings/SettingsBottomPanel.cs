using System;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Manager;
using TheLastStand.Manager.WorldMap;
using UnityEngine;

namespace TheLastStand.View.Settings;

public class SettingsBottomPanel : MonoBehaviour
{
	public static class Consts
	{
		public const string QuitGame = "QuitGame";

		public const string ResumeGame = "ResumeGame";

		public const string ResumeMainMenu = "ResumeMainMenu";

		public const string Abandon = "Abandon";

		public const string SkipTutorial = "SkipTutorial";
	}

	[SerializeField]
	private TextMeshProUGUI abandonButtonText;

	[SerializeField]
	private TextMeshProUGUI quitGameButtonText;

	[SerializeField]
	private TextMeshProUGUI resumeButtonText;

	public void Refresh()
	{
		RefreshLocalizedTexts();
	}

	private void RefreshLocalizedTexts()
	{
		((TMP_Text)quitGameButtonText).text = Localizer.Get("QuitGame");
		((TMP_Text)resumeButtonText).text = Localizer.Get(TPSingleton<GameManager>.Exist() ? "ResumeGame" : "ResumeMainMenu");
		((TMP_Text)abandonButtonText).text = Localizer.Get((TPSingleton<WorldMapCityManager>.Instance.SelectedCity != null && TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.IsTutorialMap) ? "SkipTutorial" : "Abandon");
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
		RefreshLocalizedTexts();
	}
}
