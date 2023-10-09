using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.Framework.UI;

[AddComponentMenu("UI/InteractiveImage")]
public class InteractiveImage : Image, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
	[SerializeField]
	private Sprite highlightedSprite;

	[SerializeField]
	private Sprite pressedSprite;

	[SerializeField]
	private Sprite disabledSprite;

	private Sprite baseSprite;

	private bool isInside;

	public void OnPointerDown(PointerEventData eventData)
	{
		((Image)this).sprite = pressedSprite;
		if (Object.op_Implicit((Object)(object)pressedSprite))
		{
			((Image)this).sprite = pressedSprite;
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		((Image)this).sprite = baseSprite;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		isInside = true;
		if (Object.op_Implicit((Object)(object)highlightedSprite))
		{
			((Image)this).sprite = highlightedSprite;
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		isInside = false;
		((Image)this).sprite = baseSprite;
	}

	private void Awake()
	{
		((UIBehaviour)this).Awake();
		baseSprite = ((Image)this).sprite;
	}
}
