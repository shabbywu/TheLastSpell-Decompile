using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Log;
using TPLib.Yield;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.Shop;

public class ShopInventoryToSlotsNavigation : Selectable
{
	public readonly List<Selectable> ShelvesSlots = new List<Selectable>();

	public override void OnSelect(BaseEventData eventData)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		((Selectable)this).OnSelect(eventData);
		Vector3 highlightPosition = ((Component)TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight).transform.position;
		Selectable val = ShelvesSlots.OrderBy(delegate(Selectable o)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			Vector3 val2 = ((Component)o).transform.position - highlightPosition;
			return ((Vector3)(ref val2)).sqrMagnitude;
		}).FirstOrDefault();
		if ((Object)(object)val == (Object)null)
		{
			((CLogger<HUDJoystickNavigationManager>)TPSingleton<HUDJoystickNavigationManager>.Instance).LogWarning((object)("No valid selectable target has been found when selecting " + ((Object)((Component)this).transform).name + "."), (CLogLevel)1, true, false);
		}
		else
		{
			((MonoBehaviour)this).StartCoroutine(RedirectSelection(((Component)val).gameObject));
		}
	}

	private IEnumerator RedirectSelection(GameObject selected)
	{
		EventSystem currentEventSystem = EventSystem.current;
		if (currentEventSystem.alreadySelecting)
		{
			yield return SharedYields.WaitForEndOfFrame;
		}
		currentEventSystem.SetSelectedGameObject(selected);
	}
}
