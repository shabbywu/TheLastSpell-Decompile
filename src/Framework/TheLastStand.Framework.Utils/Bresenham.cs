using System.Collections.Generic;
using UnityEngine;

namespace TheLastStand.Framework.Utils;

public static class Bresenham
{
	public static void ComputeBresenhamLine(ref List<Vector2Int> line, int x1, int y1, int x2, int y2, bool addLastPoint = true)
	{
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		int num;
		int num2;
		if ((num = x2 - x1) != 0)
		{
			if (num > 0)
			{
				if ((num2 = y2 - y1) != 0)
				{
					if (num2 > 0)
					{
						if (num >= num2)
						{
							int num3 = num;
							num = num3 * 2;
							num2 *= 2;
							while (true)
							{
								line.Add(new Vector2Int(x1, y1));
								if (++x1 == x2)
								{
									break;
								}
								if ((num3 -= num2) < 0)
								{
									y1++;
									num3 += num;
								}
							}
						}
						else
						{
							int num4 = num2;
							num2 = num4 * 2;
							num *= 2;
							while (true)
							{
								line.Add(new Vector2Int(x1, y1));
								if (++y1 == y2)
								{
									break;
								}
								if ((num4 -= num) < 0)
								{
									x1++;
									num4 += num2;
								}
							}
						}
					}
					else if (num >= -num2)
					{
						int num5 = num;
						num = num5 * 2;
						num2 *= 2;
						while (true)
						{
							line.Add(new Vector2Int(x1, y1));
							if (++x1 == x2)
							{
								break;
							}
							if ((num5 += num2) < 0)
							{
								y1--;
								num5 += num;
							}
						}
					}
					else
					{
						int num6 = num2;
						num2 = num6 * 2;
						num *= 2;
						while (true)
						{
							line.Add(new Vector2Int(x1, y1));
							if (--y1 == y2)
							{
								break;
							}
							if ((num6 += num) > 0)
							{
								x1++;
								num6 += num2;
							}
						}
					}
				}
				else
				{
					do
					{
						line.Add(new Vector2Int(x1, y1));
					}
					while (++x1 != x2);
				}
			}
			else if ((num2 = y2 - y1) != 0)
			{
				if (num2 > 0)
				{
					if (-num >= num2)
					{
						int num7 = num;
						num = num7 * 2;
						num2 *= 2;
						while (true)
						{
							line.Add(new Vector2Int(x1, y1));
							if (--x1 == x2)
							{
								break;
							}
							if ((num7 += num2) >= 0)
							{
								y1++;
								num7 += num;
							}
						}
					}
					else
					{
						int num8 = num2;
						num2 = num8 * 2;
						num *= 2;
						while (true)
						{
							line.Add(new Vector2Int(x1, y1));
							if (++y1 == y2)
							{
								break;
							}
							if ((num8 += num) <= 0)
							{
								x1--;
								num8 += num2;
							}
						}
					}
				}
				else if (num <= num2)
				{
					int num9 = num;
					num = num9 * 2;
					num2 *= 2;
					while (true)
					{
						line.Add(new Vector2Int(x1, y1));
						if (--x1 == x2)
						{
							break;
						}
						if ((num9 -= num2) >= 0)
						{
							y1--;
							num9 += num;
						}
					}
				}
				else
				{
					int num10 = num2;
					num2 = num10 * 2;
					num *= 2;
					while (true)
					{
						line.Add(new Vector2Int(x1, y1));
						if (--y1 == y2)
						{
							break;
						}
						if ((num10 -= num) >= 0)
						{
							x1--;
							num10 += num2;
						}
					}
				}
			}
			else
			{
				do
				{
					line.Add(new Vector2Int(x1, y1));
				}
				while (--x1 != x2);
			}
		}
		else if ((num2 = y2 - y1) != 0)
		{
			if (num2 > 0)
			{
				do
				{
					line.Add(new Vector2Int(x1, y1));
				}
				while (++y1 != y2);
			}
			else
			{
				do
				{
					line.Add(new Vector2Int(x1, y1));
				}
				while (--y1 != y2);
			}
		}
		if (addLastPoint)
		{
			line.Add(new Vector2Int(x2, y2));
		}
	}

	public static List<Vector2Int> ComputeBresenhamLine(int x1, int y1, int x2, int y2, bool addLastPoint = true)
	{
		List<Vector2Int> line = new List<Vector2Int>();
		ComputeBresenhamLine(ref line, x1, y1, x2, y2, addLastPoint);
		return line;
	}

	public static void ComputeBresenhamLineWithoutDiagonals(ref List<Vector2Int> line, int x1, int y1, int x2, int y2, bool addLastPoint = true)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		int num = Mathf.Abs(x2 - x1);
		int num2 = -Mathf.Abs(y2 - y1);
		int num3 = ((x1 < x2) ? 1 : (-1));
		int num4 = ((y1 < y2) ? 1 : (-1));
		int num5 = num + num2;
		line.Add(new Vector2Int(x1, y1));
		while (x1 != x2 || y1 != y2)
		{
			if (2 * num5 - num2 >= num - 2 * num5)
			{
				num5 += num2;
				x1 += num3;
			}
			else
			{
				num5 += num;
				y1 += num4;
			}
			line.Add(new Vector2Int(x1, y1));
		}
	}

	public static List<Vector2Int> ComputeBresenhamLineWithoutDiagonals(int x1, int y1, int x2, int y2, bool addLastPoint = true)
	{
		List<Vector2Int> line = new List<Vector2Int>();
		ComputeBresenhamLineWithoutDiagonals(ref line, x1, y1, x2, y2, addLastPoint);
		return line;
	}
}
