using System.Collections;
using TPLib;
using TPLib.Log;
using TPLib.Yield;
using TheLastStand.Manager.Building;
using TheLastStand.View.TileMap;
using UnityEngine;

namespace TheLastStand.View.Building.Construction;

public class ConstructionAnimationView : MonoBehaviour
{
	[SerializeField]
	private SpriteRenderer spriteRenderer;

	[SerializeField]
	private SpriteRenderer spriteRendererLUT;

	[SerializeField]
	private Sprite[] shockwaveSprites;

	private int animationFrameRate;

	private int shockwaveFrame;

	private Transform animationTransform;

	private Sprite[] spritesDiffuse;

	private Sprite[] spritesLUT;

	public int GetAnimationFrameRate()
	{
		return animationFrameRate;
	}

	public void Init(int sortingOrder, Sprite[] sprites, int animationFrameRate, int shockwaveFrame, Sprite[] spritesLUT = null)
	{
		this.animationFrameRate = animationFrameRate;
		this.shockwaveFrame = shockwaveFrame;
		((Renderer)spriteRenderer).sortingOrder = sortingOrder;
		((Renderer)spriteRendererLUT).sortingOrder = sortingOrder;
		spritesDiffuse = sprites;
		this.spritesLUT = spritesLUT;
	}

	public void PlayConstructionAnimation()
	{
		((MonoBehaviour)this).StartCoroutine(PlayConstructionAnimationCoroutine());
	}

	private Vector3 GetAnimationPosition()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)animationTransform == (Object)null)
		{
			animationTransform = ((Component)this).transform;
		}
		return animationTransform.position;
	}

	private IEnumerator PlayConstructionAnimationCoroutine()
	{
		int spritesLength = spritesDiffuse.Length;
		if (shockwaveFrame > spritesLength - 1)
		{
			((CLogger<BuildingManager>)TPSingleton<BuildingManager>.Instance).LogWarning((object)"Shockwave frame number is higher than the actual frames count and won't appear.", (CLogLevel)1, true, false);
		}
		if (spritesLUT != null && spritesLUT.Length != spritesDiffuse.Length)
		{
			((CLogger<BuildingManager>)TPSingleton<BuildingManager>.Instance).LogWarning((object)("lut frame count is different than the actual frames count. Since this can lead to errors, we hide the lut anim :" + $" {spritesLUT.Length} != {spritesDiffuse.Length}"), (CLogLevel)1, true, false);
			spritesLUT = null;
		}
		bool isLUTActive = spritesLUT != null;
		((Renderer)spriteRendererLUT).enabled = isLUTActive;
		float framesStep = 1f / (float)animationFrameRate;
		int spriteIndex = 0;
		while (spriteIndex < spritesLength)
		{
			spriteRenderer.sprite = spritesDiffuse[spriteIndex];
			if (isLUTActive)
			{
				spriteRendererLUT.sprite = spritesLUT[spriteIndex];
			}
			if (spriteIndex == shockwaveFrame)
			{
				TileMapView.SpawnConstructionAnimation(GetAnimationPosition(), shockwaveSprites, ((Renderer)spriteRenderer).sortingOrder - 1, animationFrameRate, -1);
			}
			yield return SharedYields.WaitForSeconds(framesStep);
			int num = spriteIndex + 1;
			spriteIndex = num;
		}
		((Component)this).gameObject.SetActive(false);
	}
}
