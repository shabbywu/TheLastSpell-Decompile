using TPLib;
using TheLastStand.Manager.WorldMap;
using UnityEngine;

namespace TheLastStand.View.Tooltip;

public class GlyphsTooltipDisplayer : SpriteChangeTooltipDisplayer
{
	[SerializeField]
	private Sprite customModeSpriteOff;

	[SerializeField]
	private Sprite customModeSpriteOn;

	private void Start()
	{
		if (TPSingleton<WorldMapCityManager>.Instance.SelectedCity.GlyphsConfig.CustomModeEnabled)
		{
			offSprite = customModeSpriteOff;
			onSprite = customModeSpriteOn;
			image.sprite = offSprite;
		}
	}
}
