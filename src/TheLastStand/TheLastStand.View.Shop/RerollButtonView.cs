using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.Shop;

public class RerollButtonView : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[SerializeField]
	private Image priceBackground;

	public void OnPointerEnter(PointerEventData eventData)
	{
		((Behaviour)priceBackground).enabled = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		((Behaviour)priceBackground).enabled = false;
	}
}
