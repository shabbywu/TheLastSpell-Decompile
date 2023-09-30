using TPLib;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Item;

public class ItemLevelBadgeView : MonoBehaviour
{
	[SerializeField]
	private DataSpriteTable levelSprites;

	[SerializeField]
	private Image levelBadge;

	[SerializeField]
	private bool isSmallSlot;

	public bool IsSmallSlot
	{
		get
		{
			return isSmallSlot;
		}
		set
		{
			isSmallSlot = value;
		}
	}

	public void Refresh(int level)
	{
		if (!((Object)(object)levelSprites == (Object)null) && !((Object)(object)levelBadge == (Object)null))
		{
			if (level <= 0)
			{
				levelBadge.sprite = null;
				((Behaviour)levelBadge).enabled = false;
			}
			else
			{
				levelBadge.sprite = levelSprites.GetSpriteAt(level);
				((Graphic)levelBadge).SetNativeSize();
				((Behaviour)levelBadge).enabled = true;
			}
		}
	}
}
