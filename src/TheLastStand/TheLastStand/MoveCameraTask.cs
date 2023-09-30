using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TheLastStand.Framework.Sequencing;
using UnityEngine;

namespace TheLastStand;

public class MoveCameraTask : Task
{
	private Vector3 targetPosition;

	private float duration;

	public MoveCameraTask(Vector3 targetPosition, float duration)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		this.targetPosition = targetPosition;
		this.duration = duration;
	}

	public override void StartTask()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		((Task)this).StartTask();
		TweenSettingsExtensions.OnComplete<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOMove(((Component)Camera.main).transform, targetPosition, duration, true), (TweenCallback)delegate
		{
			((Task)this).Complete();
		});
	}

	public override string ToString()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return $"{((Task)this).ToString()}: Camera to {targetPosition} during {duration} seconds";
	}
}
