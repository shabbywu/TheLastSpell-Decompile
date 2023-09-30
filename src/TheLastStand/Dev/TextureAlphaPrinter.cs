using UnityEngine;

namespace Dev;

public class TextureAlphaPrinter : MonoBehaviour
{
	[SerializeField]
	private Texture2D targetTexture;

	private void Awake()
	{
		if ((Object)(object)targetTexture == (Object)null)
		{
			SpriteRenderer component = ((Component)this).GetComponent<SpriteRenderer>();
			object obj;
			if (component == null)
			{
				obj = null;
			}
			else
			{
				Sprite sprite = component.sprite;
				obj = ((sprite != null) ? sprite.texture : null);
			}
			targetTexture = (Texture2D)obj;
		}
	}
}
