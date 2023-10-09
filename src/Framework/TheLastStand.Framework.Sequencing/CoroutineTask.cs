using System.Collections;
using UnityEngine;

namespace TheLastStand.Framework.Sequencing;

public class CoroutineTask : Task
{
	private MonoBehaviour coroutineRunner;

	private IEnumerator coroutine;

	private string coroutineName;

	public CoroutineTask(MonoBehaviour coroutineRunner, IEnumerator coroutine)
	{
		this.coroutineRunner = coroutineRunner;
		this.coroutine = coroutine;
	}

	public CoroutineTask(MonoBehaviour coroutineRunner, IEnumerator coroutine, string coroutineName)
	{
		this.coroutineRunner = coroutineRunner;
		this.coroutine = coroutine;
		if (coroutineName != null)
		{
			this.coroutineName = coroutineName;
		}
	}

	public override void StartTask()
	{
		base.StartTask();
		coroutineRunner.StartCoroutine(WaitForCompletion());
	}

	public override string ToString()
	{
		if (coroutineName == null)
		{
			return base.ToString();
		}
		return base.ToString() + ": " + coroutineName;
	}

	private IEnumerator WaitForCompletion()
	{
		yield return coroutine;
		Complete();
	}
}
