using UnityEngine;

namespace TheLastStand.View.Camera;

public class CamTarget : MonoBehaviour
{
	public Transform TargetTransform { get; set; }

	private void Update()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)TargetTransform == (Object)null))
		{
			((Component)this).transform.position = TargetTransform.position;
		}
	}
}
