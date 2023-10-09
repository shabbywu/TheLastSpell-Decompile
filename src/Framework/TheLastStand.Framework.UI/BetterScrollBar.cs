using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.Framework.UI;

public class BetterScrollBar : Scrollbar
{
	[SerializeField]
	private List<GameObject> objectsToHideWithIt = new List<GameObject>();

	protected override void OnDisable()
	{
		((Scrollbar)this).OnDisable();
		if (objectsToHideWithIt == null || objectsToHideWithIt.Count <= 0)
		{
			return;
		}
		foreach (GameObject item in objectsToHideWithIt)
		{
			if ((Object)(object)item != (Object)null)
			{
				item.SetActive(false);
			}
		}
	}

	protected override void OnEnable()
	{
		((Scrollbar)this).OnEnable();
		if (objectsToHideWithIt == null || objectsToHideWithIt.Count <= 0)
		{
			return;
		}
		foreach (GameObject item in objectsToHideWithIt)
		{
			if ((Object)(object)item != (Object)null)
			{
				item.SetActive(true);
			}
		}
	}
}
