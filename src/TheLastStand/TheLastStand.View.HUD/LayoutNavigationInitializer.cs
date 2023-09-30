using System.Collections;
using System.Collections.Generic;
using TPLib;
using TPLib.Lib.Scripts.UI;
using TPLib.Log;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.HUD;

public class LayoutNavigationInitializer : MonoBehaviour
{
	private struct Edge
	{
		public bool Left;

		public bool Right;

		public bool Top;

		public bool Bottom;
	}

	private struct GridInfo
	{
		public List<Selectable> Selectables;

		public Vector2Int GridSize;

		public bool IsHorizontal;

		public int ChildCount;
	}

	[SerializeField]
	private bool initOnStart = true;

	private LayoutGroup layoutGroup;

	[ContextMenu("Init Navigation")]
	public void InitNavigation(bool reset = false)
	{
		if ((Object)(object)layoutGroup == (Object)null && !((Component)this).TryGetComponent<LayoutGroup>(ref layoutGroup))
		{
			((CLogger<UIManager>)TPSingleton<UIManager>.Instance).LogWarning((object)("No layout found on " + ((Object)((Component)this).transform).name + "!"), (CLogLevel)1, true, false);
			return;
		}
		LayoutGroup val = layoutGroup;
		GridLayoutGroup val2 = (GridLayoutGroup)(object)((val is GridLayoutGroup) ? val : null);
		if (val2 == null)
		{
			HorizontalOrVerticalLayoutGroup val3 = (HorizontalOrVerticalLayoutGroup)(object)((val is HorizontalOrVerticalLayoutGroup) ? val : null);
			if (val3 == null)
			{
				AlternatingGridLayoutGroup val4 = (AlternatingGridLayoutGroup)(object)((val is AlternatingGridLayoutGroup) ? val : null);
				if (val4 != null)
				{
					InitAlternatingGridNavigation(val4, reset);
				}
				else
				{
					((CLogger<UIManager>)TPSingleton<UIManager>.Instance).LogWarning((object)("Unhandled layout type " + ((object)layoutGroup).GetType().Name + "!"), (CLogLevel)1, true, false);
				}
			}
			else
			{
				InitHorizontalOrVerticalNavigation(val3, reset);
			}
		}
		else
		{
			InitGridNavigation(val2, reset);
		}
	}

	private void InitGridNavigation(GridLayoutGroup gridLayoutGroup, bool reset)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Invalid comparison between Unknown and I4
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		GridInfo gridInfo = default(GridInfo);
		Transform transform = ((Component)gridLayoutGroup).transform;
		gridInfo.GridSize = GridLayoutExtensions.GetColumnAndRow(gridLayoutGroup);
		gridInfo.IsHorizontal = (int)gridLayoutGroup.startAxis == 0;
		gridInfo.Selectables = new List<Selectable>();
		Selectable val2 = default(Selectable);
		foreach (Transform item in transform)
		{
			Transform val = item;
			if (((Component)val).TryGetComponent<Selectable>(ref val2))
			{
				if (reset)
				{
					SelectableExtensions.ClearNavigation(val2);
				}
				if (((Component)val).gameObject.activeInHierarchy)
				{
					gridInfo.Selectables.Add(val2);
				}
			}
		}
		gridInfo.ChildCount = gridInfo.Selectables.Count;
		for (int i = 0; i < gridInfo.ChildCount; i++)
		{
			Selectable val3 = gridInfo.Selectables[i];
			SelectableExtensions.SetMode(val3, (Mode)4);
			SetGridEdgeSelection(gridInfo, val3, i);
		}
	}

	private void InitHorizontalOrVerticalNavigation(HorizontalOrVerticalLayoutGroup horizontalOrVerticalLayoutGroup, bool reset)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		Transform transform = ((Component)horizontalOrVerticalLayoutGroup).transform;
		List<Selectable> list = new List<Selectable>();
		Selectable val2 = default(Selectable);
		foreach (Transform item in transform)
		{
			Transform val = item;
			if (((Component)val).TryGetComponent<Selectable>(ref val2))
			{
				if (reset)
				{
					SelectableExtensions.ClearNavigation(val2);
				}
				if (((Component)val).gameObject.activeInHierarchy)
				{
					list.Add(val2);
				}
			}
		}
		bool flag = horizontalOrVerticalLayoutGroup is VerticalLayoutGroup;
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			Selectable val3 = list[i];
			SelectableExtensions.SetMode(val3, (Mode)4);
			if (i > 0)
			{
				if (flag)
				{
					SelectableExtensions.SetSelectOnUp(val3, list[i - 1]);
				}
				else
				{
					SelectableExtensions.SetSelectOnLeft(val3, list[i - 1]);
				}
			}
			if (i < count - 1)
			{
				if (flag)
				{
					SelectableExtensions.SetSelectOnDown(val3, list[i + 1]);
				}
				else
				{
					SelectableExtensions.SetSelectOnRight(val3, list[i + 1]);
				}
			}
		}
	}

	private void InitAlternatingGridNavigation(AlternatingGridLayoutGroup alternatingGridLayoutGroup, bool reset)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		Transform transform = ((Component)alternatingGridLayoutGroup).transform;
		Vector2Int columnAndRow = GridLayoutExtensions.GetColumnAndRow(alternatingGridLayoutGroup);
		List<Selectable> list = new List<Selectable>();
		Selectable val2 = default(Selectable);
		foreach (Transform item in transform)
		{
			Transform val = item;
			if (((Component)val).gameObject.activeInHierarchy && ((Component)val).TryGetComponent<Selectable>(ref val2))
			{
				if (reset)
				{
					SelectableExtensions.ClearNavigation(val2);
				}
				list.Add(val2);
				SelectableExtensions.SetMode(val2, (Mode)4);
			}
		}
		for (int i = 0; i < ((Vector2Int)(ref columnAndRow)).y; i++)
		{
			int num = Mathf.CeilToInt((float)i / 2f) * ((Vector2Int)(ref columnAndRow)).x + Mathf.FloorToInt((float)i / 2f) * (((Vector2Int)(ref columnAndRow)).x - 1);
			int num2 = Mathf.CeilToInt((float)(i + 1) / 2f) * ((Vector2Int)(ref columnAndRow)).x + Mathf.FloorToInt((float)(i + 1) / 2f) * (((Vector2Int)(ref columnAndRow)).x - 1) - 1;
			for (int j = 0; j <= num2 - num; j++)
			{
				int num3 = j + num;
				if (list.Count <= num3)
				{
					continue;
				}
				Selectable val3 = list[num3];
				if (j > 0 && list.Count > num3 - 1)
				{
					SelectableExtensions.SetSelectOnLeft(val3, list[num3 - 1]);
				}
				if (j < num2 - num && list.Count > num3 + 1)
				{
					SelectableExtensions.SetSelectOnRight(val3, list[num3 + 1]);
				}
				if (i < ((Vector2Int)(ref columnAndRow)).y - 1)
				{
					if (i % 2 == 0)
					{
						int num4 = ((j == num2 - num) ? (num3 + ((Vector2Int)(ref columnAndRow)).x - 1) : (num3 + ((Vector2Int)(ref columnAndRow)).x));
						if (list.Count > num4)
						{
							SelectableExtensions.SetSelectOnDown(val3, list[num4]);
						}
					}
					else if (list.Count > num3 + ((Vector2Int)(ref columnAndRow)).x - 1)
					{
						SelectableExtensions.SetSelectOnDown(val3, list[num3 + ((Vector2Int)(ref columnAndRow)).x - 1]);
					}
				}
				if (i <= 0)
				{
					continue;
				}
				if (i % 2 == 0)
				{
					int num5 = num3 - ((Vector2Int)(ref columnAndRow)).x + 1;
					if (list.Count > num5)
					{
						SelectableExtensions.SetSelectOnUp(val3, list[num5]);
					}
				}
				else if (list.Count > num3 - ((Vector2Int)(ref columnAndRow)).x)
				{
					SelectableExtensions.SetSelectOnUp(val3, list[num3 - ((Vector2Int)(ref columnAndRow)).x]);
				}
			}
		}
	}

	private void SetGridEdgeSelection(GridInfo gridInfo, Selectable selectable, int value)
	{
		Edge edge;
		if (gridInfo.IsHorizontal)
		{
			edge = default(Edge);
			edge.Left = value % ((Vector2Int)(ref gridInfo.GridSize)).x == 0;
			edge.Right = (value + 1) % ((Vector2Int)(ref gridInfo.GridSize)).x == 0;
			edge.Top = value < ((Vector2Int)(ref gridInfo.GridSize)).x;
			edge.Bottom = value >= gridInfo.ChildCount - ((Vector2Int)(ref gridInfo.GridSize)).x;
			Edge edge2 = edge;
			if (((Vector2Int)(ref gridInfo.GridSize)).x > 1)
			{
				if (!edge2.Left)
				{
					SelectableExtensions.SetSelectOnLeft(selectable, gridInfo.Selectables[value - 1]);
				}
				if (!edge2.Right && value + 1 < gridInfo.ChildCount)
				{
					SelectableExtensions.SetSelectOnRight(selectable, gridInfo.Selectables[value + 1]);
				}
			}
			if (((Vector2Int)(ref gridInfo.GridSize)).y > 1)
			{
				if (!edge2.Top)
				{
					SelectableExtensions.SetSelectOnUp(selectable, gridInfo.Selectables[value - ((Vector2Int)(ref gridInfo.GridSize)).x]);
				}
				if (!edge2.Bottom && value + ((Vector2Int)(ref gridInfo.GridSize)).x < gridInfo.ChildCount)
				{
					SelectableExtensions.SetSelectOnDown(selectable, gridInfo.Selectables[value + ((Vector2Int)(ref gridInfo.GridSize)).x]);
				}
			}
			return;
		}
		edge = default(Edge);
		edge.Left = value < ((Vector2Int)(ref gridInfo.GridSize)).y;
		edge.Right = value >= gridInfo.ChildCount - ((Vector2Int)(ref gridInfo.GridSize)).y;
		edge.Top = value % ((Vector2Int)(ref gridInfo.GridSize)).y == 0;
		edge.Bottom = (value + 1) % ((Vector2Int)(ref gridInfo.GridSize)).y == 0;
		Edge edge3 = edge;
		if (((Vector2Int)(ref gridInfo.GridSize)).x > 1)
		{
			if (!edge3.Left)
			{
				SelectableExtensions.SetSelectOnLeft(selectable, gridInfo.Selectables[value - ((Vector2Int)(ref gridInfo.GridSize)).y]);
			}
			if (!edge3.Right && value + ((Vector2Int)(ref gridInfo.GridSize)).y < gridInfo.ChildCount)
			{
				SelectableExtensions.SetSelectOnRight(selectable, gridInfo.Selectables[value + ((Vector2Int)(ref gridInfo.GridSize)).y]);
			}
		}
		if (((Vector2Int)(ref gridInfo.GridSize)).y > 1)
		{
			if (!edge3.Top)
			{
				SelectableExtensions.SetSelectOnUp(selectable, gridInfo.Selectables[value - 1]);
			}
			if (!edge3.Bottom && value + 1 < gridInfo.ChildCount)
			{
				SelectableExtensions.SetSelectOnDown(selectable, gridInfo.Selectables[value + 1]);
			}
		}
	}

	private IEnumerator Start()
	{
		if (initOnStart)
		{
			yield return null;
			InitNavigation();
		}
	}
}
