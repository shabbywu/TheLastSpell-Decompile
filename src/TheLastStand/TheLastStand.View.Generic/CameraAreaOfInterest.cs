using UnityEngine;

namespace TheLastStand.View.Generic;

public class CameraAreaOfInterest : MonoBehaviour
{
	[SerializeField]
	private float areaWeight;

	[SerializeField]
	private Collider2D areaCollider;

	public float AreaWeight => areaWeight;

	public Collider2D AreaCollider => areaCollider;

	private void OnDrawGizmosSelected()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.color = new Color(1f, 0f, 0f, 1f);
		Gizmos.DrawWireSphere(((Component)this).transform.position + Vector2.op_Implicit(areaCollider.offset), areaWeight);
	}
}
