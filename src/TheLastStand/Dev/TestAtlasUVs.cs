using TPLib;
using UnityEngine;

namespace Dev;

public class TestAtlasUVs : MonoBehaviour
{
	[SerializeField]
	private SpriteRenderer spriteRenderer;

	private MaterialPropertyBlock materialPropBlock;

	[ContextMenu("Set Atlas UV")]
	public void SetAtlasUv()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)spriteRenderer == (Object)null)
		{
			spriteRenderer = ((Component)this).GetComponent<SpriteRenderer>();
		}
		if ((Object)(object)spriteRenderer == (Object)null)
		{
			TPDebug.LogError((object)"Can't work without a renderer. Aborting...", (Object)(object)this);
			return;
		}
		if (materialPropBlock == null)
		{
			materialPropBlock = new MaterialPropertyBlock();
		}
		Sprite sprite = spriteRenderer.sprite;
		if ((Object)(object)sprite == (Object)null)
		{
			TPDebug.LogError((object)"Can't work without a sprite. Aborting...", (Object)(object)this);
			return;
		}
		((Renderer)spriteRenderer).GetPropertyBlock(materialPropBlock);
		Rect textureRect = sprite.textureRect;
		float x = ((Rect)(ref textureRect)).position.x;
		textureRect = sprite.textureRect;
		float y = ((Rect)(ref textureRect)).position.y;
		textureRect = sprite.textureRect;
		float x2 = ((Rect)(ref textureRect)).size.x;
		textureRect = sprite.textureRect;
		Vector4 val = default(Vector4);
		((Vector4)(ref val))._002Ector(x, y, x2, ((Rect)(ref textureRect)).size.y);
		materialPropBlock.SetVector("_AtlasRect", val);
		((Renderer)spriteRenderer).SetPropertyBlock(materialPropBlock);
	}

	private void Awake()
	{
		SetAtlasUv();
	}
}
