using System;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.View.Generic;

public class GenericTooltip : TooltipBase
{
	[SerializeField]
	protected TextMeshProUGUI tooltipText;

	[SerializeField]
	private DataColor hotkeysColor;

	protected object[] argsToFormat;

	protected string textToDisplay = string.Empty;

	protected string hotkeyText = string.Empty;

	protected bool mustFormatHotkey;

	public void SetLocalizedContent(string text, string rewiredActionKey, bool shouldFormatHotKey = false)
	{
		SetContentWithHotkeys(text, rewiredActionKey, shouldFormatHotKey);
	}

	public void SetContent(string localizationKey, params object[] argsToFormat)
	{
		string empty = string.Empty;
		empty = ((argsToFormat == null) ? Localizer.Get(localizationKey) : string.Format(Localizer.Format(localizationKey, argsToFormat)));
		SetContentWithHotkeys(empty, null);
	}

	public void SetContentWithHotkeys(string text, string rewiredActionKey, bool mustFormatHotkey = false)
	{
		textToDisplay = text;
		hotkeyText = string.Empty;
		string[] localizedHotkeysForAction;
		if (!string.IsNullOrEmpty(rewiredActionKey) && (localizedHotkeysForAction = InputManager.GetLocalizedHotkeysForAction(rewiredActionKey)) != null)
		{
			string text2 = string.Empty;
			int i = 0;
			for (int num = localizedHotkeysForAction.Length; i < num; i++)
			{
				text2 = text2 + ((i > 0) ? ", " : string.Empty) + localizedHotkeysForAction[i];
			}
			text2 = "<b>[" + text2 + "]</b>";
			if ((Object)(object)hotkeysColor != (Object)null)
			{
				text2 = "<color=#" + hotkeysColor._HexCode + ">" + text2 + "</color>";
			}
			hotkeyText = text2;
		}
		this.mustFormatHotkey = mustFormatHotkey;
	}

	protected override bool CanBeDisplayed()
	{
		return ((TMP_Text)tooltipText).text != string.Empty;
	}

	protected override void RefreshContent()
	{
		((TMP_Text)tooltipText).text = (mustFormatHotkey ? string.Format(textToDisplay, hotkeyText) : (textToDisplay + " " + hotkeyText));
	}

	protected override void Awake()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		base.Awake();
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
			RefreshContent();
		}
	}
}
