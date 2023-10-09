using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.Framework.Animation;

[DisallowMultipleComponent]
[RequireComponent(typeof(Image))]
public class MultipleImageAnimator : ImageAnimator
{
	private Dictionary<string, List<Sprite>> allSprites = new Dictionary<string, List<Sprite>>();

	private string currentPath = string.Empty;

	private int currentAllSpriteIndex;

	public List<string> AllSpritesPath { get; set; }

	public override void Refresh()
	{
		Stop();
		if (AllSpritesPath == null || AllSpritesPath.Count == 0)
		{
			Debug.LogWarning((object)"No paths!");
			return;
		}
		allSprites.Clear();
		for (int i = 0; i < AllSpritesPath.Count; i++)
		{
			Sprite[] array = ResourcePooler<Sprite>.LoadAllOnce(AllSpritesPath[i]);
			allSprites.Add(AllSpritesPath[i], new List<Sprite>());
			int j = 0;
			for (int num = array.Length; j < num; j++)
			{
				if ((Object)(object)array[j] != (Object)null)
				{
					allSprites[AllSpritesPath[i]].Add(array[j]);
				}
			}
		}
		if (allSprites.Count > 0)
		{
			currentAllSpriteIndex = 0;
			currentPath = allSprites.ElementAt(currentAllSpriteIndex).Key;
			image.sprite = allSprites[currentPath][0];
			currentSpriteIndex = 0;
			timer = 0f;
		}
	}

	protected override void Update()
	{
		if (base.IsPaused)
		{
			return;
		}
		timer += Time.deltaTime;
		if (timer < delayBetweenFrames)
		{
			return;
		}
		timer = 0f;
		if (allSprites[currentPath].Count <= 0)
		{
			return;
		}
		if (++currentSpriteIndex == allSprites[currentPath].Count)
		{
			if (destroyGameObjectAfterPlayedOnce)
			{
				Object.Destroy((Object)(object)((Component)this).gameObject);
				return;
			}
			currentSpriteIndex = 0;
			currentAllSpriteIndex++;
			if (currentAllSpriteIndex == allSprites.Count)
			{
				currentAllSpriteIndex = 0;
			}
			currentPath = allSprites.ElementAt(currentAllSpriteIndex).Key;
		}
		image.sprite = allSprites[currentPath][currentSpriteIndex];
	}
}
