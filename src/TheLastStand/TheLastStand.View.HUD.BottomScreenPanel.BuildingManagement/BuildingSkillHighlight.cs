using UnityEngine;
using UnityEngine.EventSystems;

namespace TheLastStand.View.HUD.BottomScreenPanel.BuildingManagement;

public class BuildingSkillHighlight : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[SerializeField]
	private GameObject highlight;

	private void OnDisable()
	{
		highlight.SetActive(false);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		highlight.SetActive(true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		highlight.SetActive(false);
	}
}
