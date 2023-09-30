using UnityEngine;

namespace Dev;

public class TestShaderPositionArrayTarget : MonoBehaviour
{
	[SerializeField]
	private Vector2 offset = Vector2.zero;

	public Vector4 ShaderPosition => new Vector4(((Component)this).transform.position.x + offset.x, ((Component)this).transform.position.y + offset.y, ((Component)this).transform.position.z, 0f);
}
