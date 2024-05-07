using TPLib;
using TheLastStand.Manager.Item;
using UnityEngine;

namespace TheLastStand.View.Tooltip;

public class WeaponsRestrictionsTooltipDisplayer : SpriteChangeTooltipDisplayer
{
	[SerializeField]
	private Sprite customModeSpriteOff;

	[SerializeField]
	private Sprite customModeSpriteOn;

	private void Start()
	{
		if (TPSingleton<ItemRestrictionManager>.Instance.WeaponsRestrictionsCategories.IsBoundlessModeActive)
		{
			offSprite = customModeSpriteOff;
			onSprite = customModeSpriteOn;
			image.sprite = customModeSpriteOff;
		}
	}
}
