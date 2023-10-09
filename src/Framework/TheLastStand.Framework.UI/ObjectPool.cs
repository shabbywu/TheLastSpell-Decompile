using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TheLastStand.Framework.UI;

internal class ObjectPool<T> where T : new()
{
	private readonly Stack<T> stack = new Stack<T>();

	private readonly UnityAction<T> actionOnGet;

	private readonly UnityAction<T> actionOnRelease;

	public int countAll { get; private set; }

	public int countActive => countAll - countInactive;

	public int countInactive => stack.Count;

	public ObjectPool(UnityAction<T> actionOnGet, UnityAction<T> actionOnRelease)
	{
		this.actionOnGet = actionOnGet;
		this.actionOnRelease = actionOnRelease;
	}

	public T Get()
	{
		T val;
		if (stack.Count == 0)
		{
			val = new T();
			countAll++;
		}
		else
		{
			val = stack.Pop();
		}
		if (actionOnGet != null)
		{
			actionOnGet.Invoke(val);
		}
		return val;
	}

	public void Release(T element)
	{
		if (stack.Count > 0 && (object)stack.Peek() == (object)element)
		{
			Debug.LogError((object)"Internal error. Trying to destroy object that is already released to pool.");
		}
		if (actionOnRelease != null)
		{
			actionOnRelease.Invoke(element);
		}
		stack.Push(element);
	}
}
