using System.Collections;
using TPLib;
using TPLib.Log;
using TPLib.Yield;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.HUD;

public class JoystickSelectableDynamic : Selectable
{
	[SerializeField]
	private Selectable[] selectables;

	[SerializeField]
	private bool checkSelectablesParentCanvas;

	public Selectable[] Selectables => selectables;

	public override void OnSelect(BaseEventData eventData)
	{
		((Selectable)this).OnSelect(eventData);
		Selectable val = null;
		for (int i = 0; i < selectables.Length; i++)
		{
			Selectable val2 = selectables[i];
			if ((Object)(object)val2 == (Object)null)
			{
				continue;
			}
			if (checkSelectablesParentCanvas)
			{
				Canvas componentInParent = ((Component)val2).GetComponentInParent<Canvas>();
				if ((Object)(object)componentInParent != (Object)null && !((Behaviour)componentInParent).enabled)
				{
					continue;
				}
			}
			if (val2.IsInteractable() && ((Component)val2).gameObject.activeInHierarchy)
			{
				val = val2;
				break;
			}
		}
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
