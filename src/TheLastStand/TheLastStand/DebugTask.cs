using TheLastStand.Framework.Sequencing;
using UnityEngine;

namespace TheLastStand;

public class DebugTask : Task
{
	private string debugString;

	public DebugTask(string debugString)
	{
		this.debugString = debugString;
	}

	public override void StartTask()
	{
		((Task)this).StartTask();
		Debug.Log((object)debugString);
		((Task)this).Complete();
	}

	public override string ToString()
	{
		return ((Task)this).ToString() + ": " + debugString;
	}
}
