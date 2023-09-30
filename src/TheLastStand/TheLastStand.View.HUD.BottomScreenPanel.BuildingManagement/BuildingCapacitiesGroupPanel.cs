using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TheLastStand.View.HUD.BottomScreenPanel.BuildingManagement;

public class BuildingCapacitiesGroupPanel : MonoBehaviour
{
	[SerializeField]
	private Canvas panelCanvas;

	[SerializeField]
	private LayoutElement layoutElement;

	[SerializeField]
	[FormerlySerializedAs("buildingSkillsGroupParent")]
	private RectTransform buildingCapacitiesGroupParent;

	[SerializeField]
	[FormerlySerializedAs("buildingSkills")]
	private List<BuildingCapacityPanel> buildingCapacities = new List<BuildingCapacityPanel>();

	[SerializeField]
	private HorizontalLayoutGroup buildingSkillsGroupLayout;

	public List<BuildingCapacityPanel> BuildingCapacities => buildingCapacities;

	public bool IsDisplayed()
	{
		return ((Behaviour)panelCanvas).enabled;
	}

	public void Display(bool show = true)
	{
		((Behaviour)panelCanvas).enabled = show;
		layoutElement.ignoreLayout = !show;
		if (show)
		{
			Refresh();
		}
	}

	private void Refresh()
	{
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		if (BuildingCapacities.Count == 0)
		{
			return;
		}
		float num = 0f;
		int i = 0;
		for (int count = BuildingCapacities.Count; i < count; i++)
		{
			if (((Component)BuildingCapacities[i]).gameObject.activeInHierarchy && (Object)(object)buildingSkillsGroupLayout != (Object)null)
			{
				num += BuildingCapacities[i].BuildingCapacityRect.sizeDelta.x + ((HorizontalOrVerticalLayoutGroup)buildingSkillsGroupLayout).spacing;
			}
			BuildingCapacities[i].Refresh();
		}
		if ((Object)(object)buildingSkillsGroupLayout != (Object)null)
		{
			num += (float)(((LayoutGroup)buildingSkillsGroupLayout).padding.left + ((LayoutGroup)buildingSkillsGroupLayout).padding.right) - ((HorizontalOrVerticalLayoutGroup)buildingSkillsGroupLayout).spacing;
			buildingCapacitiesGroupParent.sizeDelta = new Vector2(num, buildingCapacitiesGroupParent.sizeDelta.y);
			LayoutRebuilder.ForceRebuildLayoutImmediate(buildingCapacitiesGroupParent);
		}
	}
}
