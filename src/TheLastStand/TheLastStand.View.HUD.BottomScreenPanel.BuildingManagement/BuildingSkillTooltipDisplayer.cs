using TheLastStand.Manager.Building;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TheLastStand.View.HUD.BottomScreenPanel.BuildingManagement;

public class BuildingSkillTooltipDisplayer : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[SerializeField]
	private string buildingSkillId = string.Empty;

	[SerializeField]
	private float tooltipOffsetY = 64f;

	private BuildingSkillTooltip buildingSkillTooltip;

	public void DisplayTooltip(bool display)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		if (display)
		{
			buildingSkillTooltip.SetContent(buildingSkillId);
			((Component)buildingSkillTooltip).transform.position = new Vector3(((Component)this).transform.position.x, ((Component)this).transform.position.y + tooltipOffsetY, 0f);
			buildingSkillTooltip.Display();
		}
		else
		{
			buildingSkillTooltip.Hide();
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		DisplayTooltip(display: true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		DisplayTooltip(display: false);
	}

	private void Start()
	{
		buildingSkillTooltip = BuildingManager.BuildingSkillTooltip;
	}
}
