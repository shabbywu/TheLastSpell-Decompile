using System.Collections;
using System.Collections.Generic;
using Com.LuisPedroFonseca.ProCamera2D;
using TPLib.Yield;
using UnityEngine;

namespace TheLastStand.View.Camera;

public class CameraPOI : MonoBehaviour
{
	[SerializeField]
	[Range(0.01f, 1f)]
	private float transitionSpeed = 0.2f;

	[SerializeField]
	private CamTarget cameraTargetPrefab;

	private Coroutine moveCameraCoroutine;

	private ProCamera2D proCamera2D;

	private CamTarget camTarget;

	private List<Transform> oldTargetTransforms = new List<Transform>();

	private List<CameraTarget> cameraTargetsToRemove = new List<CameraTarget>();

	private ProCamera2D ProCamera2D
	{
		get
		{
			if ((Object)(object)proCamera2D == (Object)null)
			{
				proCamera2D = ProCamera2D.Instance;
			}
			return proCamera2D;
		}
	}

	public void AddTarget(Transform targetTransform)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)camTarget != (Object)null)
		{
			oldTargetTransforms.Add(((Component)camTarget).transform);
		}
		camTarget = Object.Instantiate<CamTarget>(cameraTargetPrefab);
		((Component)camTarget).transform.position = targetTransform.position;
		camTarget.TargetTransform = targetTransform;
		ProCamera2D.AddCameraTarget(((Component)camTarget).transform, 0f, 0f, 0f, default(Vector2));
		MoveCamera();
	}

	public Coroutine MoveCamera()
	{
		if (moveCameraCoroutine != null)
		{
			return moveCameraCoroutine;
		}
		moveCameraCoroutine = ((MonoBehaviour)this).StartCoroutine(MoveCameraCoroutine());
		return moveCameraCoroutine;
	}

	public void RemoveTargets()
	{
		if ((Object)(object)camTarget != (Object)null)
		{
			oldTargetTransforms.Add(((Component)camTarget).transform);
		}
		camTarget = null;
		foreach (CameraTarget cameraTarget in ProCamera2D.CameraTargets)
		{
			if (oldTargetTransforms.Contains(cameraTarget.TargetTransform))
			{
				cameraTargetsToRemove.Add(cameraTarget);
			}
			else
			{
				cameraTarget.TargetInfluence = 1f;
			}
		}
		foreach (CameraTarget item in cameraTargetsToRemove)
		{
			oldTargetTransforms.Remove(item.TargetTransform);
			proCamera2D.CameraTargets.Remove(item);
			Object.Destroy((Object)(object)((Component)item.TargetTransform).gameObject);
		}
		cameraTargetsToRemove.Clear();
		oldTargetTransforms.Clear();
		MoveCamera();
	}

	private IEnumerator MoveCameraCoroutine()
	{
		while (ProCamera2D.CameraTargets.Count > 0 && ((Object)(object)camTarget != (Object)null || oldTargetTransforms.Count > 0))
		{
			foreach (CameraTarget cameraTarget in ProCamera2D.CameraTargets)
			{
				cameraTarget.TargetInfluence = Mathf.Clamp01(((Object)(object)cameraTarget.TargetTransform == (Object)(object)((Component)camTarget).transform) ? (cameraTarget.TargetInfluenceH + transitionSpeed) : (cameraTarget.TargetInfluenceH - transitionSpeed));
				if (cameraTarget.TargetInfluenceH <= 0f && oldTargetTransforms.Contains(cameraTarget.TargetTransform))
				{
					cameraTargetsToRemove.Add(cameraTarget);
				}
			}
			foreach (CameraTarget item in cameraTargetsToRemove)
			{
				oldTargetTransforms.Remove(item.TargetTransform);
				proCamera2D.CameraTargets.Remove(item);
				Object.Destroy((Object)(object)((Component)item.TargetTransform).gameObject);
			}
			cameraTargetsToRemove.Clear();
			yield return SharedYields.WaitForEndOfFrame;
		}
		moveCameraCoroutine = null;
	}
}
