using System.Collections;
using TPLib.Yield;
using UnityEngine;

namespace TheLastStand.View.Building;

public class DestructionAnimationView : MonoBehaviour
{
	[SerializeField]
	private SpriteRenderer spriteRenderer;

	[SerializeField]
	private int animationFrameRate = 12;

	private Sprite[] destructionAnimationSprites;

	private float delay;

	public void Init(Vector3 worldPosition, Sprite[] sprites, float delay)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).gameObject.SetActive(true);
		((Component)this).transform.position = worldPosition;
		destructionAnimationSprites = sprites;
		this.delay = delay;
	}

	public void PlayDestructionAnimation()
	{
		((MonoBehaviour)this).StartCoroutine(PlayDestructionAnimationCoroutine());
	}

	private IEnumerator PlayDestructionAnimationCoroutine()
	{
		spriteRenderer.sprite = destructionAnimationSprites[0];
		float num = delay;
		yield return SharedYields.WaitForSeconds(num);
		float framesStep = 1f / (float)animationFrameRate;
		int i = 0;
		while (i < destructionAnimationSprites.Length)
		{
			spriteRenderer.sprite = destructionAnimationSprites[i];
			yield return SharedYields.WaitForSeconds(framesStep);
			int num2 = i + 1;
			i = num2;
		}
		((Component)this).gameObject.SetActive(false);
	}
}
