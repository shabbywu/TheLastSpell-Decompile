using UnityEngine;

namespace Dev;

public class TestShaderPosition : MonoBehaviour
{
	[SerializeField]
	private Vector3 offset = Vector2.op_Implicit(Vector2.zero);

	private void Update()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		Shader.SetGlobalVector("_Position", Vector4.op_Implicit(((Component)this).transform.position + offset));
	}
}
