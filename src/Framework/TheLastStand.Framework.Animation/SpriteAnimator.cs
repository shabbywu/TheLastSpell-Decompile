using System.Collections;
using System.Collections.Generic;
using TPLib.Yield;
using UnityEngine;

namespace TheLastStand.Framework.Animation;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteAnimator : MonoBehaviour
{
	[SerializeField]
	[Range(1f, 60f)]
	private int delayBetweenFrames = 10;

	[SerializeField]
	private bool refreshOnStart = true;

	[SerializeField]
	private bool destroyGameObjectAfterPlayedOnce;

	[SerializeField]
	private SpriteRenderer spriteRenderer;

	[SerializeField]
	private string spritesPath = string.Empty;

	private List<Sprite> sprites = new List<Sprite>();

	private int currentSpriteIndex;

	private Coroutine animateCoroutine;

	public bool IsPaused { get; set; }

	public string SpritesPath
	{
		get
		{
			return spritesPath;
		}
		set
		{
			spritesPath = value;
		}
	}

	public void Refresh()
	{
		if (animateCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(animateCoroutine);
			animateCoroutine = null;
		}
		if (spritesPath == string.Empty)
		{
			Debug.LogWarning((object)"No path!");
			return;
		}
		sprites.Clear();
		Sprite[] array = ResourcePooler.LoadAllOnce<Sprite>(spritesPath, failSilently: false);
		for (int i = 0; i < array.Length; i++)
		{
			if ((Object)(object)array[i] != (Object)null)
			{
				sprites.Add(array[i]);
			}
		}
		if (sprites.Count > 0)
		{
			spriteRenderer.sprite = sprites[0];
			animateCoroutine = ((MonoBehaviour)this).StartCoroutine(AnimateCoroutine());
		}
	}

	private IEnumerator AnimateCoroutine()
	{
		currentSpriteIndex = 0;
		int framesCount = 0;
		while (true)
		{
			yield return SharedYields.WaitForEndOfFrame;
			if (IsPaused)
			{
				continue;
			}
			int num = framesCount + 1;
			framesCount = num;
			if (num < delayBetweenFrames || sprites.Count <= 0)
			{
				continue;
			}
			currentSpriteIndex++;
			if (currentSpriteIndex == sprites.Count)
			{
				if (destroyGameObjectAfterPlayedOnce)
				{
					break;
				}
				currentSpriteIndex = 0;
			}
			spriteRenderer.sprite = sprites[currentSpriteIndex];
			framesCount = 0;
		}
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}

	private void Start()
	{
		if (refreshOnStart)
		{
			Refresh();
		}
	}
}
