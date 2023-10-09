using System;
using System.Collections.Generic;

namespace TheLastStand.Framework.EventSystem;

public class EventManager : SingletonBehaviour<EventManager>
{
	public delegate void EventHandler(Event raisedEvent);

	private Dictionary<Type, List<EventHandler>> handlers;

	private Dictionary<Type, List<EventHandler>> onlyListenOnceHandlers;

	public static void AddListener(Type eventType, EventHandler handler, bool onlyListenOnce = false)
	{
		if (SingletonBehaviour<EventManager>.Instance.handlers.ContainsKey(eventType))
		{
			if (SingletonBehaviour<EventManager>.Instance.handlers[eventType] == null)
			{
				SingletonBehaviour<EventManager>.Instance.handlers[eventType] = new List<EventHandler>();
			}
			if (!SingletonBehaviour<EventManager>.Instance.handlers[eventType].Contains(handler))
			{
				SingletonBehaviour<EventManager>.Instance.handlers[eventType].Add(handler);
			}
		}
		else
		{
			List<EventHandler> value = new List<EventHandler> { handler };
			SingletonBehaviour<EventManager>.Instance.handlers.Add(eventType, value);
		}
		if (!onlyListenOnce)
		{
			return;
		}
		if (SingletonBehaviour<EventManager>.Instance.onlyListenOnceHandlers.ContainsKey(eventType))
		{
			if (SingletonBehaviour<EventManager>.Instance.onlyListenOnceHandlers[eventType] == null)
			{
				SingletonBehaviour<EventManager>.Instance.onlyListenOnceHandlers[eventType] = new List<EventHandler>();
			}
			if (!SingletonBehaviour<EventManager>.Instance.onlyListenOnceHandlers[eventType].Contains(handler))
			{
				SingletonBehaviour<EventManager>.Instance.onlyListenOnceHandlers[eventType].Add(handler);
			}
		}
		else
		{
			List<EventHandler> value2 = new List<EventHandler> { handler };
			SingletonBehaviour<EventManager>.Instance.onlyListenOnceHandlers.Add(eventType, value2);
		}
	}

	public static void RemoveListener(Type eventType, EventHandler handler)
	{
		if (SingletonBehaviour<EventManager>.Instance.handlers.ContainsKey(eventType) && SingletonBehaviour<EventManager>.Instance.handlers[eventType].Contains(handler))
		{
			SingletonBehaviour<EventManager>.Instance.handlers[eventType].Remove(handler);
		}
		if (SingletonBehaviour<EventManager>.Instance.onlyListenOnceHandlers.ContainsKey(eventType) && SingletonBehaviour<EventManager>.Instance.onlyListenOnceHandlers[eventType].Contains(handler))
		{
			SingletonBehaviour<EventManager>.Instance.onlyListenOnceHandlers[eventType].Remove(handler);
		}
	}

	public static void TriggerEvent(Event eventRaised)
	{
		Type type = eventRaised.GetType();
		if (!SingletonBehaviour<EventManager>.Instance.handlers.ContainsKey(type) || SingletonBehaviour<EventManager>.Instance.handlers[type] == null)
		{
			return;
		}
		Dictionary<Type, EventHandler> dictionary = new Dictionary<Type, EventHandler>();
		foreach (EventHandler item in SingletonBehaviour<EventManager>.Instance.handlers[type])
		{
			item(eventRaised);
			if (SingletonBehaviour<EventManager>.Instance.onlyListenOnceHandlers.ContainsKey(type) && SingletonBehaviour<EventManager>.Instance.onlyListenOnceHandlers[type] != null && SingletonBehaviour<EventManager>.Instance.onlyListenOnceHandlers[type].Contains(item))
			{
				dictionary.Add(type, item);
			}
		}
		foreach (KeyValuePair<Type, EventHandler> item2 in dictionary)
		{
			SingletonBehaviour<EventManager>.Instance.handlers[item2.Key].Remove(item2.Value);
			SingletonBehaviour<EventManager>.Instance.onlyListenOnceHandlers[item2.Key].Remove(item2.Value);
		}
	}

	private void Awake()
	{
		handlers = new Dictionary<Type, List<EventHandler>>();
		onlyListenOnceHandlers = new Dictionary<Type, List<EventHandler>>();
	}
}
