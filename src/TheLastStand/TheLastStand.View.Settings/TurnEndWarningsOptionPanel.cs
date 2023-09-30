using System;
using System.Collections.Generic;
using TMPro;
using TPLib.Localization;
using UnityEngine;

namespace TheLastStand.View.Settings;

public class TurnEndWarningsOptionPanel : MonoBehaviour
{
	public static class Constants
	{
		public const string ResourcesThresholdWarningText = "Settings_Warning_GoldAndMaterialsThreshold";
	}

	[SerializeField]
	private List<EndTurnWarningToggle> endTurnWarningsToggles = new List<EndTurnWarningToggle>();

	[SerializeField]
	private TextMeshProUGUI resourcesThresholdWarningText;

	public void Refresh()
	{
		foreach (EndTurnWarningToggle endTurnWarningsToggle in endTurnWarningsToggles)
		{
			endTurnWarningsToggle.Refresh();
		}
	}

	private void Awake()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		Refresh();
		RefreshLocalizedTexts();
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

	private void RefreshLocalizedTexts()
	{
		((TMP_Text)resourcesThresholdWarningText).text = Localizer.Format("Settings_Warning_GoldAndMaterialsThreshold", new object[1] { 50 });
	}
}
