using TheLastStand.View.Camera;
using UnityEngine;

namespace TheLastStand.View.WorldMap;

public class WorldMapWorldInputsView : WorldInputsView
{
	public override void LateUpdate()
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		if (camRefOrthoSize != ACameraView.MainCam.orthographicSize)
		{
			camRefOrthoSize = ACameraView.MainCam.orthographicSize;
			camRefAspect = ACameraView.MainCam.aspect;
			Vector2 val = default(Vector2);
			((Vector2)(ref val))._002Ector(camRefOrthoSize * 2f * camRefAspect, camRefOrthoSize * 2f);
			val *= 1.1f;
			worldCollider.size = val;
		}
	}
}
