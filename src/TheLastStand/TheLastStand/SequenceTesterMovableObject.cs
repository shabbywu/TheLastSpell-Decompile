using System.Collections;
using TPLib;
using TPLib.Yield;
using TheLastStand.Framework.Sequencing;
using UnityEngine;

namespace TheLastStand;

public class SequenceTesterMovableObject : TPSingleton<SequenceTesterMovableObject>
{
	[SerializeField]
	private Transform movableTransform;

	public TaskGroup TaskGroup { get; private set; }

	private void Start()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected O, but got Unknown
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Expected O, but got Unknown
		TaskGroup = new TaskGroup((Task[])(object)new Task[3]
		{
			new DebugTask("Start sequence..."),
			new MoveObjectTask((MonoBehaviour)(object)this, movableTransform, new Vector3(0f, 10f), 0.1f),
			(Task)new CoroutineTask((MonoBehaviour)(object)this, WaitCoroutine(2f), "Waiting a bit")
		});
		TaskGroup.Append(new TaskGroup((Task[])(object)new Task[2]
		{
			new MoveObjectTask((MonoBehaviour)(object)this, movableTransform, new Vector3(-5f, 10f), 0.05f),
			new MoveCameraTask(new Vector3(-5f, 10f, -10f), 1f)
		}));
		TaskGroup.Run();
	}

	private IEnumerator WaitCoroutine(float waitingTime)
	{
		yield return SharedYields.WaitForSeconds(waitingTime);
	}
}
