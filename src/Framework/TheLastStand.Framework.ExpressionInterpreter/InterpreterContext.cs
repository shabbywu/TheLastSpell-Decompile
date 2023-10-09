using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace TheLastStand.Framework.ExpressionInterpreter;

public class InterpreterContext
{
	private const BindingFlags BindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

	public object TargetObject;

	private DefaultInterpreterContext commonInterpreterContext;

	private DefaultInterpreterContext CommonInterpreterContext
	{
		get
		{
			if (commonInterpreterContext == null)
			{
				commonInterpreterContext = new DefaultInterpreterContext(TargetObject);
			}
			return commonInterpreterContext;
		}
	}

	public InterpreterContext(object targetObject)
	{
		TargetObject = targetObject;
	}

	public object CallMethod(string name, object[] arguments)
	{
		ResolveRecursiveVariable(name, TargetObject, out var lastParentFound);
		ResolveRecursiveVariable(name, this, out var lastParentFound2);
		name = name.Split(new char[1] { '.' }).Last();
		MethodInfo methodInfo = lastParentFound?.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod);
		if (methodInfo != null)
		{
			return methodInfo.Invoke(lastParentFound, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod, Type.DefaultBinder, arguments, CultureInfo.InvariantCulture);
		}
		if (lastParentFound2.GetType() != typeof(DefaultInterpreterContext))
		{
			methodInfo = lastParentFound2.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod);
			if (methodInfo != null)
			{
				return methodInfo.Invoke(lastParentFound2, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod, Type.DefaultBinder, arguments, CultureInfo.InvariantCulture);
			}
		}
		methodInfo = CommonInterpreterContext.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod);
		if (methodInfo == null)
		{
			Debug.LogError((object)("[ExpressionInterpreter] Method " + name + " not found! returned 0."));
			return -1;
		}
		return methodInfo.Invoke(CommonInterpreterContext, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod, Type.DefaultBinder, arguments, CultureInfo.InvariantCulture);
	}

	public object ResolveVariable(string name)
	{
		object lastParentFound;
		object obj = ResolveRecursiveVariable(name, TargetObject, out lastParentFound) ?? ResolveRecursiveVariable(name, this, out lastParentFound);
		if (obj == null)
		{
			Debug.LogError((object)("Unknown variable: '" + name + "'"));
			return 0;
		}
		return obj;
	}

	private object ResolveRecursiveVariable(string name, object parent, out object lastParentFound)
	{
		object obj = null;
		lastParentFound = parent;
		string[] array = name.Split(new char[1] { '.' });
		foreach (string name2 in array)
		{
			obj = null;
			PropertyInfo propertyInfo = lastParentFound?.GetType().GetProperty(name2, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (propertyInfo != null)
			{
				obj = propertyInfo.GetValue(lastParentFound);
			}
			FieldInfo fieldInfo = lastParentFound?.GetType().GetField(name2, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (fieldInfo != null)
			{
				obj = fieldInfo.GetValue(lastParentFound);
			}
			if (obj == null)
			{
				break;
			}
			lastParentFound = obj;
		}
		return obj;
	}
}
