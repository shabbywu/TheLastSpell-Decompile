using System.Collections.Generic;
using TheLastStand.Framework.UI;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Building.UI;

public class BuildingManagementPanelGauge : GraduatedGauge
{
	[SerializeField]
	private List<Transform> unitsTransforms;

	[SerializeField]
	private Transform unitsParent;

	[SerializeField]
	private HorizontalLayoutGroup unitsLayoutGroup;

	public override int MaxUnits => unitsTransforms.Count;

	public override void AddUnits(int amount, bool tween = true)
	{
		if (amount <= 0)
		{
			return;
		}
		int num = base.Units;
		for (int i = 0; i < amount; i++)
		{
			if (num > MaxUnits - 1)
			{
				if (!clearOnCapacityExceeded)
				{
					break;
				}
				Clear();
				num = 0;
			}
			((Component)unitsTransforms[num]).gameObject.SetActive(true);
			num++;
		}
		((Behaviour)unitsLayoutGroup).enabled = true;
		Transform obj = unitsParent;
		LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)(object)((obj is RectTransform) ? obj : null));
		((Behaviour)unitsLayoutGroup).enabled = false;
		base.Units = num;
	}

	public override void Clear()
	{
		for (int num = MaxUnits - 1; num >= 0; num--)
		{
			((Component)unitsTransforms[num]).gameObject.SetActive(false);
		}
		base.Units = 0;
	}

	public void SetUnitsCount(int count)
	{
		if (MaxUnits == count)
		{
			return;
		}
		if (MaxUnits < count)
		{
			while (MaxUnits < count)
			{
				Transform item = Object.Instantiate<Transform>(unitsTransforms[0], unitsParent);
				unitsTransforms.Add(item);
			}
			return;
		}
		while (MaxUnits > count)
		{
			Transform val = unitsTransforms[unitsTransforms.Count - 1];
			unitsTransforms.Remove(val);
			Object.Destroy((Object)(object)((Component)val).gameObject);
		}
	}
}
