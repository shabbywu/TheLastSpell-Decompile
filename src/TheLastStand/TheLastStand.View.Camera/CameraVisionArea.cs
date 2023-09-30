using UnityEngine;

namespace TheLastStand.View.Camera;

public class CameraVisionArea : MonoBehaviour
{
	[SerializeField]
	private BoxCollider2D cameraBoxCollider;

	[SerializeField]
	[Range(0.1f, 1f)]
	private float ratio = 0.5f;

	private float camRefAspect = -1f;

	private float camRefOrthoSize = -1f;

	private float lastRatioUsed;

	public Vector2 ColliderSize => cameraBoxCollider.size;

	private void LateUpdate()
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		if (camRefOrthoSize != ACameraView.MainCam.orthographicSize || lastRatioUsed != ratio)
		{
			camRefOrthoSize = ACameraView.MainCam.orthographicSize;
			camRefAspect = ACameraView.MainCam.aspect;
			lastRatioUsed = ratio;
			Vector2 val = default(Vector2);
			((Vector2)(ref val))._002Ector(camRefOrthoSize * 2f * camRefAspect, camRefOrthoSize * 2f);
			val *= ratio;
			cameraBoxCollider.size = val;
		}
	}
}
