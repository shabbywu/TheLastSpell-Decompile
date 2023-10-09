using System;
using System.Collections;
using TPLib.Yield;
using UnityEngine;

namespace TheLastStand.Framework.Extensions;

public static class MonoBehaviourExtensions
{
	public static void DoAfter(this MonoBehaviour coroutineHost, float delay, Action behavior)
	{
		coroutineHost.StartCoroutine(DoAfter(delay, behavior));
	}

	private static IEnumerator DoAfter(float delay, Action behavior)
	{
		yield return SharedYields.WaitForSeconds(delay);
		behavior();
	}
}
