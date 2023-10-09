using System.Collections.Generic;
using TPLib.Lib.Scripts.UI;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.Framework.Extensions;

public static class GridLayoutExtensions
{
	public static Vector2Int GetColumnAndRow(this GridLayoutGroup gridLayoutGroup)
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Invalid comparison between Unknown and I4
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		List<Transform> list = new List<Transform>();
		LayoutElement val = default(LayoutElement);
		for (int i = 0; i < ((Component)gridLayoutGroup).transform.childCount; i++)
		{
			Transform child = ((Component)gridLayoutGroup).transform.GetChild(i);
			if (((Component)child).gameObject.activeInHierarchy && (!((Component)child).TryGetComponent<LayoutElement>(ref val) || !val.ignoreLayout))
			{
				list.Add(child);
			}
		}
		if (list.Count == 0)
		{
			return Vector2Int.zero;
		}
		int num = 1;
		int num2 = 1;
		bool flag = false;
		bool flag2 = (int)gridLayoutGroup.startAxis == 0;
		Transform transform = ((Component)list[0]).transform;
		Vector2 anchoredPosition = ((RectTransform)((transform is RectTransform) ? transform : null)).anchoredPosition;
		for (int j = 1; j < list.Count; j++)
		{
			Transform obj = list[j];
			Vector2 anchoredPosition2 = ((RectTransform)((obj is RectTransform) ? obj : null)).anchoredPosition;
			if (flag2 ? (Mathf.Abs(anchoredPosition.x - anchoredPosition2.x) < 0.01f) : (Mathf.Abs(anchoredPosition.y - anchoredPosition2.y) < 0.01f))
			{
				num2++;
				flag = true;
			}
			else if (!flag)
			{
				num++;
			}
		}
		if (!flag2)
		{
			return new Vector2Int(num2, num);
		}
		return new Vector2Int(num, num2);
	}

	public static Vector2Int GetColumnAndRow(this AlternatingGridLayoutGroup alternatingGridLayoutGroup)
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		LayoutElement val = default(LayoutElement);
		for (int i = 0; i < ((Component)alternatingGridLayoutGroup).transform.childCount; i++)
		{
			Transform child = ((Component)alternatingGridLayoutGroup).transform.GetChild(i);
			if (((Component)child).gameObject.activeInHierarchy && (!((Component)child).TryGetComponent<LayoutElement>(ref val) || !val.ignoreLayout))
			{
				num++;
			}
		}
		int num2 = 0;
		int num3 = 0;
		while (num2 < num)
		{
			num2 += ((num3 % 2 == 0) ? alternatingGridLayoutGroup.ColumnsCount : (alternatingGridLayoutGroup.ColumnsCount - 1));
			num3++;
		}
		return new Vector2Int(alternatingGridLayoutGroup.ColumnsCount, num3);
	}
}
