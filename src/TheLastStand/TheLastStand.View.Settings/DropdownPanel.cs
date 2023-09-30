using System;
using TMPro;
using TPLib.Localization;
using UnityEngine;

namespace TheLastStand.View.Settings;

public class DropdownPanel : MonoBehaviour
{
	[SerializeField]
	protected bool useLocalization = true;

	[SerializeField]
	protected TMP_Dropdown dropdown;

	protected int optionsCount;

	protected string[] OptionKeys { get; private set; }

	public virtual void OnDropdownValueChange()
	{
	}

	public virtual void Refresh()
	{
		RefreshOptionTexts();
	}

	protected virtual void Awake()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		if (useLocalization)
		{
			Localizer.onLocalize = (OnLocalizeNotification)Delegate.Combine((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
		}
	}

	protected virtual void InitializeOptionKeys()
	{
		optionsCount = dropdown.options.Count;
		OptionKeys = new string[optionsCount];
		for (int i = 0; i < optionsCount; i++)
		{
			OptionKeys[i] = dropdown.options[i].text;
		}
	}

	protected virtual void OnDestroy()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		if (useLocalization)
		{
			Localizer.onLocalize = (OnLocalizeNotification)Delegate.Remove((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
		}
	}

	protected virtual void RefreshOptionTexts()
	{
		if (optionsCount != 0)
		{
			for (int i = 0; i < optionsCount; i++)
			{
				dropdown.options[i].text = (useLocalization ? Localizer.Get(OptionKeys[i]) : OptionKeys[i]);
			}
			dropdown.RefreshShownValue();
		}
	}

	protected virtual void Start()
	{
		InitializeOptionKeys();
		Refresh();
	}

	private void OnLocalize()
	{
		RefreshOptionTexts();
	}
}
