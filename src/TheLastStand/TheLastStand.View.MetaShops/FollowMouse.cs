using UnityEngine;

namespace TheLastStand.View.MetaShops;

public class FollowMouse : MonoBehaviour
{
	private void Update()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		((Transform)(RectTransform)((Component)this).transform).position = Input.mousePosition;
	}
}
