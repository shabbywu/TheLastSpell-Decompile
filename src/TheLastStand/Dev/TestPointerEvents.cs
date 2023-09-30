using TPLib;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Dev;

public class TestPointerEvents : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public void OnPointerEnter(PointerEventData eventData)
	{
		TPDebug.Log((object)"The cursor ENTERED the selectable UI element.", (Object)(object)this);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		TPDebug.Log((object)"The cursor EXITED the selectable UI element.", (Object)(object)this);
	}
}
