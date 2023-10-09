using UnityEngine;

namespace TheLastStand.Framework.Extensions;

public static class TransformExtensions
{
	public static void DestroyChildren(this Transform transform)
	{
		for (int num = transform.childCount - 1; num >= 0; num--)
		{
			Object.Destroy((Object)(object)((Component)transform.GetChild(num)).gameObject);
		}
		transform.DetachChildren();
	}

	public static void DestroyChildrenImmediate(this Transform transform)
	{
		for (int num = transform.childCount - 1; num >= 0; num--)
		{
			Object.DestroyImmediate((Object)(object)((Component)transform.GetChild(num)).gameObject);
		}
		transform.DetachChildren();
	}
}
