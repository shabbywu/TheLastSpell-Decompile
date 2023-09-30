using UnityEngine;
using UnityEngine.UI;

public class TestGoddessScaler : MonoBehaviour
{
	[SerializeField]
	private CanvasScaler refScaler;

	private void Update()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.localScale = new Vector3(1f / refScaler.scaleFactor, 1f / refScaler.scaleFactor);
	}
}
