using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Definition.Meta.Glyphs;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.WorldMap;
using TheLastStand.View.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Tooltip;

public class GlyphEffectsTooltip : TooltipBase
{
	private static class Constants
	{
		public const string MaxGlyphsDisplayed = "GlyphEffects_MaxGlyphsDisplayed";
	}

	[SerializeField]
	private TextMeshProUGUI effectsDescription;

	[SerializeField]
	private Image customModeIcon;

	[SerializeField]
	private TextMeshProUGUI customModeValue;

	protected override void Awake()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		base.Awake();
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Combine((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(RefreshText));
	}

	protected override bool CanBeDisplayed()
	{
		return TPSingleton<WorldMapCityManager>.Instance.SelectedCity.GlyphsConfig.SelectedGlyphs.Count > 0;
	}

	protected override void RefreshContent()
	{
	}

	private void OnDestroy()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Remove((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(RefreshText));
	}

	private void RefreshText()
	{
		StringBuilder stringBuilder = new StringBuilder();
		List<GlyphDefinition> selectedGlyphs = TPSingleton<WorldMapCityManager>.Instance.SelectedCity.GlyphsConfig.SelectedGlyphs;
		for (int i = 0; i < selectedGlyphs.Count && i < TPSingleton<GlyphManager>.Instance.MaxGlyphsDisplayed; i++)
		{
			stringBuilder.Append("<style=DarkShopKW>• " + selectedGlyphs[i].GetName() + "</style> : " + selectedGlyphs[i].GetDescription(null) + "\r\n");
		}
		if (TPSingleton<GlyphManager>.Instance.MaxGlyphsDisplayed < selectedGlyphs.Count)
		{
			stringBuilder.Append(Localizer.Get("GlyphEffects_MaxGlyphsDisplayed"));
		}
		((TMP_Text)effectsDescription).text = stringBuilder.ToString();
	}

	private void Start()
	{
		if (TPSingleton<WorldMapCityManager>.Instance.SelectedCity.GlyphsConfig.CustomModeEnabled)
		{
			((Behaviour)customModeIcon).enabled = true;
			((Behaviour)customModeValue).enabled = true;
			((TMP_Text)customModeValue).text = $"+{TPSingleton<WorldMapCityManager>.Instance.SelectedCity.GetCustomModeBonusPoints()}";
		}
		RefreshText();
	}
}
