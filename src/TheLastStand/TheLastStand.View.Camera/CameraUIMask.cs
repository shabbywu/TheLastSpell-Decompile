using System.Collections;
using UnityEngine;

namespace TheLastStand.View.Camera;

public class CameraUIMask : MonoBehaviour
{
	public class ScreenPointsAngleComparer : IComparer
	{
		private readonly Camera camera;

		private readonly Vector2 pointsCenter;

		public ScreenPointsAngleComparer(Camera camera, Vector2 pointsCenterWorld)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			this.camera = camera;
			pointsCenter = pointsCenterWorld;
		}

		public int Compare(object a, object b)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Expected O, but got Unknown
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Expected O, but got Unknown
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			Transform val = (Transform)a;
			Transform val2 = (Transform)b;
			float num = Vector2.SignedAngle(pointsCenter - Vector2.op_Implicit(camera.ScreenToWorldPoint(val.position)), pointsCenter);
			float value = Vector2.SignedAngle(pointsCenter - Vector2.op_Implicit(camera.ScreenToWorldPoint(val2.position)), pointsCenter);
			return num.CompareTo(value);
		}
	}

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private Transform[] vertices;

	public Canvas Canvas => canvas;

	public Transform[] Vertices => vertices;

	private void Start()
	{
		CameraView.CameraUIMasksHandler.RegisterMask(this);
	}

	private void Reset()
	{
		_ = (Object)(object)Canvas == (Object)null;
	}
}
