using System.Collections;
using TPLib.Yield;
using TheLastStand.Framework.Sequencing;
using UnityEngine;

namespace TheLastStand;

public class MoveObjectTask : Task
{
	private MonoBehaviour coroutineRunner;

	private Transform target;

	private Vector3 targetPosition;

	private float moveSpeed;

	public MoveObjectTask(MonoBehaviour coroutineRunner, Transform target, Vector3 targetPosition, float moveSpeed)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		this.coroutineRunner = coroutineRunner;
		this.target = target;
		this.targetPosition = targetPosition;
		this.moveSpeed = moveSpeed;
	}

	public override void StartTask()
	{
		base.StartTask();
		coroutineRunner.StartCoroutine(MoveCoroutine());
	}

	public override string ToString()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		return $"{base.ToString()}: {((Object)target).name} to {targetPosition} at speed {moveSpeed}";
	}

	private IEnumerator MoveCoroutine()
	{
		while (Vector3.Distance(((Component)target).transform.position, targetPosition) > 0.1f)
		{
			((Component)target).transform.position = Vector3.Lerp(((Component)target).transform.position, targetPosition, moveSpeed);
			yield return SharedYields.WaitForEndOfFrame;
		}
		((Component)target).transform.position = targetPosition;
		Complete();
	}
}
