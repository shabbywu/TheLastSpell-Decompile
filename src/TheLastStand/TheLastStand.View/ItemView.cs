using TheLastStand.Framework;
using UnityEngine;

namespace TheLastStand.View;

public class ItemView
{
	public static Sprite GetIngameSprite(string itemDefinitionId)
	{
		if (string.IsNullOrEmpty(itemDefinitionId))
		{
			return null;
		}
		Sprite val = ResourcePooler.LoadOnce<Sprite>("View/Sprites/Items/Weapon_" + itemDefinitionId, failSilently: false);
		if ((Object)(object)val != (Object)null)
		{
			return val;
		}
		return null;
	}

	public static Sprite GetUiSprite(string itemDefinitionId, bool isBG = false)
	{
		if (string.IsNullOrEmpty(itemDefinitionId))
		{
			return null;
		}
		return ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Items/Icons/" + (isBG ? "Background" : "Foreground") + "/UI_Icon_Items_" + itemDefinitionId + "_" + (isBG ? "BG" : "FG"), failSilently: false);
	}
}
