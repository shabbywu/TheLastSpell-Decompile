using System;
using System.Collections.Generic;
using TMPro;
using TPLib.Localization;
using TPLib.Localization.Fonts;
using TPLib.Localization.ScriptableObjects;
using UnityEngine;

namespace TheLastStand.View.Settings;

public class FontAssemblyDropdown : DropdownPanel
{
	[SerializeField]
	private FontAssemblyLabel fontAssemblyLabel;

	private int fontIndex;

	public override void OnDropdownValueChange()
	{
		base.OnDropdownValueChange();
		fontIndex = dropdown.value;
		if (FontSettings.IsActivated)
		{
			fontAssemblyLabel.SetTargettedFontAssemblyIndex(fontIndex, true);
			FontManager.OnAssemblyIndexHasChanged(fontIndex);
		}
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

	private void OnLocalize()
	{
		InitializeOptionKeys();
	}

	protected override void InitializeOptionKeys()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Expected O, but got Unknown
		if (FontSettings.IsActivated)
		{
			fontIndex = FontManager.FontAssemblyIndex;
			List<FontAssembly> fontAssemblies = FontManager.GetFontAssemblies(Localizer.language);
			int count = fontAssemblies.Count;
			dropdown.options = new List<OptionData>(count);
			for (int i = 0; i < count; i++)
			{
				FontAssembly val = fontAssemblies[i];
				dropdown.options.Add(new OptionData(val.Id));
			}
			fontAssemblyLabel.SetTargettedFontAssemblyIndex(fontIndex, true);
		}
		base.InitializeOptionKeys();
		dropdown.SetValueWithoutNotify(fontIndex);
	}

	private void DropdownOpened()
	{
		GameObject gameObject = ((Component)((Component)dropdown).transform.GetChild(((Component)dropdown).transform.childCount - 1)).gameObject;
		if (FontSettings.IsActivated)
		{
			FontAssemblyLabel[] componentsInChildren = gameObject.GetComponentsInChildren<FontAssemblyLabel>(false);
			int count = FontManager.CurrentFontAssemblies.Count;
			for (int i = 0; i < count; i++)
			{
				componentsInChildren[i].SetTargettedFontAssemblyIndex(i, true);
			}
		}
	}
}
