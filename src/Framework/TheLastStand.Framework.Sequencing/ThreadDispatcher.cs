using System;
using System.Collections.Generic;
using UnityEngine;

namespace TheLastStand.Framework.Sequencing;

public class ThreadDispatcher : MonoBehaviour
{
	private static ThreadDispatcher instance;

	private Queue<Action> dispatchedActions = new Queue<Action>();

	public static void DispatchToMain(Action action)
	{
		if ((Object)(object)instance == (Object)null)
		{
			Debug.LogError((object)"Someone tried to enqueue a task on the Thread dispatcher, but it has NOT been created!\nPlease ensure you have ONE (1) ThreadDispatcher in your scene, or please get rid of the task dispatching in this script.");
		}
		else
		{
			instance.dispatchedActions.Enqueue(action);
		}
	}

	private void Awake()
	{
		if (Object.op_Implicit((Object)(object)instance))
		{
			Object.DestroyImmediate((Object)(object)this);
			return;
		}
		Object.DontDestroyOnLoad((Object)(object)this);
		instance = this;
	}

	private void Update()
	{
		while (dispatchedActions.Count > 0)
		{
			dispatchedActions.Dequeue()();
		}
	}
}
