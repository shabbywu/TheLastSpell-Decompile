using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.Framework.Animation;

[DisallowMultipleComponent]
[RequireComponent(typeof(Image))]
public class ImageAnimator : MonoBehaviour
{
	[SerializeField]
	[Range(1f, 60f)]
	protected float frameRate = 12f;

	[SerializeField]
	protected bool refreshOnStart = true;

	[SerializeField]
	protected bool destroyGameObjectAfterPlayedOnce;

	[SerializeField]
	protected Image image;

	protected Coroutine animateCoroutine;

	protected int currentSpriteIndex;

	protected float delayBetweenFrames;

	protected List<Sprite> sprites = new List<Sprite>();

	protected float timer;

	public Image Image => image;

	public bool IsPaused { get; set; }

	public string SpritesPath { get; set; }

	public virtual void Refresh()
	{
		Stop();
		if (SpritesPath == string.Empty)
		{
			Debug.LogWarning((object)"No path!");
			return;
		}
		sprites.Clear();
		Sprite[] array = ResourcePooler<Sprite>.LoadAllOnce(SpritesPath);
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			if ((Object)(object)array[i] != (Object)null)
			{
				sprites.Add(array[i]);
			}
		}
		if (sprites.Count > 0)
		{
			((Behaviour)image).enabled = true;
			image.sprite = sprites[0];
			currentSpriteIndex = 0;
			timer = 0f;
		}
		else
		{
			((Behaviour)image).enabled = false;
		}
	}

	public virtual void Stop()
	{
		if (animateCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(animateCoroutine);
			animateCoroutine = null;
		}
	}

	protected void Start()
	{
		if ((Object)(object)image == (Object)null)
		{
			image = ((Component)this).GetComponent<Image>();
		}
		delayBetweenFrames = 1f / frameRate;
		if (refreshOnStart)
		{
			Refresh();
		}
	}

	protected virtual void Update()
	{
		if (IsPaused)
		{
			return;
		}
		timer += Time.deltaTime;
		if (timer < delayBetweenFrames)
		{
			return;
		}
		timer = 0f;
		if (sprites.Count <= 0)
		{
			return;
		}
		if (++currentSpriteIndex == sprites.Count)
		{
			if (destroyGameObjectAfterPlayedOnce)
			{
				Object.Destroy((Object)(object)((Component)this).gameObject);
				return;
			}
			currentSpriteIndex = 0;
		}
		image.sprite = sprites[currentSpriteIndex];
	}
}
