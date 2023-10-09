using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.View.Generic;
using UnityEngine;

namespace TheLastStand.View.Tooltip;

public class ApocalypseEffectsTooltip : TooltipBase
{
	private static class Constants
	{
		public const string Title = "WorldMap_ApocalypseEffects";

		public const string DamnedSoulsModifier = "WorldMap_ApocalypseDamnedSoulsModifier";
	}

	[SerializeField]
	private TextMeshProUGUI effectsDescription;

	[SerializeField]
	private TextMeshProUGUI title;

	[SerializeField]
	private TextMeshProUGUI damnedSoulsModifier;

	private uint? damnedSoulsPercentageModifier;

	private Vector2Int apocalypseRange = Vector2Int.zero;

	public void SetDamnedSoulsPercentageModifier(uint? value)
	{
		damnedSoulsPercentageModifier = value;
	}

	public void SetApocalypsesToDisplay(int max, int min = 1)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		apocalypseRange = new Vector2Int(Mathf.Min(min, max), max);
	}

	protected override bool CanBeDisplayed()
	{
		if (((Vector2Int)(ref apocalypseRange)).y <= TPSingleton<ApocalypseManager>.Instance.MaxApocalypseIndexAvailable)
		{
			return ((Vector2Int)(ref apocalypseRange)).y > 0;
		}
		return false;
	}

	protected override void RefreshContent()
	{
		((TMP_Text)title).text = Localizer.Get("WorldMap_ApocalypseEffects");
		((TMP_Text)damnedSoulsModifier).text = string.Format(Localizer.Get("WorldMap_ApocalypseDamnedSoulsModifier"), damnedSoulsPercentageModifier ?? TPSingleton<ApocalypseManager>.Instance.DamnedSoulsPercentageModifier);
		DisplayApocalypses();
	}

	private void DisplayApocalypses()
	{
		((TMP_Text)effectsDescription).text = string.Empty;
		for (int i = Mathf.Max(0, ((Vector2Int)(ref apocalypseRange)).x); i <= ((Vector2Int)(ref apocalypseRange)).y; i++)
		{
			TextMeshProUGUI val = effectsDescription;
			((TMP_Text)val).text = ((TMP_Text)val).text + "  <style=KeyWord>" + i.ToRoman() + ".</style>  " + Localizer.Get(string.Format("{0}{1:00}", "WorldMap_ApocalypseDescription_", i)) + "\r\n";
		}
	}
}
