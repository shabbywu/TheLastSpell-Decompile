using UnityEngine;

namespace TheLastStand.View.Generic;

public class WorldUiScaler : MonoBehaviour
{
	[SerializeField]
	private int worldPpu = 28;

	[ContextMenu("Scale")]
	public void DoScale()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		float num = 1f / (float)worldPpu;
		((Component)this).transform.localScale = new Vector3(num, num, num);
	}

	private void Start()
	{
		DoScale();
	}
}
