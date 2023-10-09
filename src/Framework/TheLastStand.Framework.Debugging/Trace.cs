using System.Diagnostics;
using UnityEngine;

namespace TheLastStand.Framework.Debugging;

public static class Trace
{
	public static void Print()
	{
		Debug.Log((object)new StackTrace(new StackFrame(1, needFileInfo: true)));
	}
}
