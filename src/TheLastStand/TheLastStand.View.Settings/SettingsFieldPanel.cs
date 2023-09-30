using System;
using TMPro;
using TPLib.Localization;
using UnityEngine;

namespace TheLastStand.View.Settings;

public abstract class SettingsFieldPanel : MonoBehaviour
{
	protected static class Constants
	{
		public static class LocalizationKeys
		{
			public const string Language = "Settings_Language";

			public const string SmartCast = "Settings_SmartCast_Label";

			public const string SpeedMode = "Settings_SpeedMode_Label";

			public const string SpeedScale = "Settings_SpeedScale_Label";

			public const string RunInBackground = "Settings_RunInBackground";

			public const string RestrictedCursor = "Settings_RestrictedCursor";

			public const string ShowSkillsHotkeys = "Settings_ShowSkillsHotkeys";

			public const string EraseSave = "Settings_EraseSave";

			public const string InputDeviceType = "Settings_InputDeviceType";

			public const string ConfirmEraseSaveLocalizationKey = "Settings_ConfirmEraseSaveText";
		}
	}

	[SerializeField]
	protected TextMeshProUGUI labelText;

	public virtual void Refresh()
	{
		RefreshLocalizedTexts();
	}

	protected virtual void Awake()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Combine((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
	}

	protected virtual void OnDestroy()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Remove((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
	}

	protected virtual void OnLocalize()
	{
		RefreshLocalizedTexts();
	}

	protected abstract void RefreshLocalizedTexts();
}
