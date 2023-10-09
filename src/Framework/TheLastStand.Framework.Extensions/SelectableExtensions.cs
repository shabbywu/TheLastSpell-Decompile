using UnityEngine.UI;

namespace TheLastStand.Framework.Extensions;

public static class SelectableExtensions
{
	public static void SetMode(this Selectable selectable, Mode mode)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		Navigation navigation = selectable.navigation;
		if (((Navigation)(ref navigation)).mode != mode)
		{
			navigation = default(Navigation);
			((Navigation)(ref navigation)).mode = mode;
			Navigation navigation2 = selectable.navigation;
			((Navigation)(ref navigation)).selectOnUp = ((Navigation)(ref navigation2)).selectOnUp;
			navigation2 = selectable.navigation;
			((Navigation)(ref navigation)).selectOnDown = ((Navigation)(ref navigation2)).selectOnDown;
			navigation2 = selectable.navigation;
			((Navigation)(ref navigation)).selectOnLeft = ((Navigation)(ref navigation2)).selectOnLeft;
			navigation2 = selectable.navigation;
			((Navigation)(ref navigation)).selectOnRight = ((Navigation)(ref navigation2)).selectOnRight;
			selectable.navigation = navigation;
		}
	}

	public static void SetSelectOnUp(this Selectable selectable, Selectable selectOnUp)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		Navigation navigation = default(Navigation);
		Navigation navigation2 = selectable.navigation;
		((Navigation)(ref navigation)).mode = ((Navigation)(ref navigation2)).mode;
		((Navigation)(ref navigation)).selectOnUp = selectOnUp;
		navigation2 = selectable.navigation;
		((Navigation)(ref navigation)).selectOnDown = ((Navigation)(ref navigation2)).selectOnDown;
		navigation2 = selectable.navigation;
		((Navigation)(ref navigation)).selectOnLeft = ((Navigation)(ref navigation2)).selectOnLeft;
		navigation2 = selectable.navigation;
		((Navigation)(ref navigation)).selectOnRight = ((Navigation)(ref navigation2)).selectOnRight;
		selectable.navigation = navigation;
	}

	public static void SetSelectOnDown(this Selectable selectable, Selectable selectOnDown)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		Navigation navigation = default(Navigation);
		Navigation navigation2 = selectable.navigation;
		((Navigation)(ref navigation)).mode = ((Navigation)(ref navigation2)).mode;
		navigation2 = selectable.navigation;
		((Navigation)(ref navigation)).selectOnUp = ((Navigation)(ref navigation2)).selectOnUp;
		((Navigation)(ref navigation)).selectOnDown = selectOnDown;
		navigation2 = selectable.navigation;
		((Navigation)(ref navigation)).selectOnLeft = ((Navigation)(ref navigation2)).selectOnLeft;
		navigation2 = selectable.navigation;
		((Navigation)(ref navigation)).selectOnRight = ((Navigation)(ref navigation2)).selectOnRight;
		selectable.navigation = navigation;
	}

	public static void SetSelectOnLeft(this Selectable selectable, Selectable selectOnLeft)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		Navigation navigation = default(Navigation);
		Navigation navigation2 = selectable.navigation;
		((Navigation)(ref navigation)).mode = ((Navigation)(ref navigation2)).mode;
		navigation2 = selectable.navigation;
		((Navigation)(ref navigation)).selectOnUp = ((Navigation)(ref navigation2)).selectOnUp;
		navigation2 = selectable.navigation;
		((Navigation)(ref navigation)).selectOnDown = ((Navigation)(ref navigation2)).selectOnDown;
		((Navigation)(ref navigation)).selectOnLeft = selectOnLeft;
		navigation2 = selectable.navigation;
		((Navigation)(ref navigation)).selectOnRight = ((Navigation)(ref navigation2)).selectOnRight;
		selectable.navigation = navigation;
	}

	public static void SetSelectOnRight(this Selectable selectable, Selectable selectOnRight)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		Navigation navigation = default(Navigation);
		Navigation navigation2 = selectable.navigation;
		((Navigation)(ref navigation)).mode = ((Navigation)(ref navigation2)).mode;
		navigation2 = selectable.navigation;
		((Navigation)(ref navigation)).selectOnUp = ((Navigation)(ref navigation2)).selectOnUp;
		navigation2 = selectable.navigation;
		((Navigation)(ref navigation)).selectOnDown = ((Navigation)(ref navigation2)).selectOnDown;
		navigation2 = selectable.navigation;
		((Navigation)(ref navigation)).selectOnLeft = ((Navigation)(ref navigation2)).selectOnLeft;
		((Navigation)(ref navigation)).selectOnRight = selectOnRight;
		selectable.navigation = navigation;
	}

	public static void ClearNavigation(this Selectable selectable)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		Navigation navigation = default(Navigation);
		Navigation navigation2 = selectable.navigation;
		((Navigation)(ref navigation)).mode = ((Navigation)(ref navigation2)).mode;
		((Navigation)(ref navigation)).selectOnUp = null;
		((Navigation)(ref navigation)).selectOnDown = null;
		((Navigation)(ref navigation)).selectOnLeft = null;
		((Navigation)(ref navigation)).selectOnRight = null;
		selectable.navigation = navigation;
	}
}
