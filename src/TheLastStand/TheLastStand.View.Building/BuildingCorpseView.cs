using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TPLib;
using TPLib.Log;
using TPLib.Yield;
using TheLastStand.Definition.Building;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Model;
using TheLastStand.Model.TileMap;
using UnityEngine;

namespace TheLastStand.View.Building;

public class BuildingCorpseView : MonoBehaviour
{
	[SerializeField]
	private SpriteRenderer deadBuildingSpriteRenderer;

	[SerializeField]
	private Vector2 waitRandomRange = new Vector2(1f, 300f);

	[SerializeField]
	[Range(0f, 3f)]
	private float fadeTime = 0.5f;

	[SerializeField]
	private Ease fadeEasing = (Ease)2;

	private Coroutine waitCoroutine;

	public Tile Tile { get; set; }

	[ContextMenu("ForceDisappear")]
	public void ForceDisappear()
	{
		if (waitCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(waitCoroutine);
			waitCoroutine = null;
		}
		Disappear();
	}

	public void Init(BuildingDefinition building)
	{
		Sprite val = ResourcePooler<Sprite>.LoadOnce(string.Format("View\\Sprites\\Buildings\\Destroyed\\{0}\\TLS_Buildings_{0}Remains", building.Id) ?? "");
		if ((Object)(object)val == (Object)null)
		{
			List<Sprite> list = new List<Sprite>();
			for (int i = 1; i < 99; i++)
			{
				string text = i.ToString("00");
				val = ResourcePooler<Sprite>.LoadOnce(string.Format("View\\Sprites\\Buildings\\Destroyed\\{0}\\TLS_Buildings_{0}Remains", building.Id) + text);
				if ((Object)(object)val == (Object)null)
				{
					break;
				}
				list.Add(val);
			}
			if (list.Count <= 0)
			{
				((CLogger<BuildingManager>)TPSingleton<BuildingManager>.Instance)?.LogWarning((object)("No dead building sprite specified for " + building.Id + " - the building may simply 'pop off' once dead, which may look odd."), (CLogLevel)0, true, false);
			}
			else
			{
				val = RandomManager.GetRandomElement(this, list);
			}
		}
		deadBuildingSpriteRenderer.sprite = val;
		_ = (Object)(object)deadBuildingSpriteRenderer.sprite == (Object)null;
		((Renderer)deadBuildingSpriteRenderer).sortingOrder = Tile.DeadBuildingViews.Count;
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Day && waitCoroutine == null)
		{
			waitCoroutine = ((MonoBehaviour)this).StartCoroutine(WaitDisappear());
		}
	}

	private void Disappear()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		Tile.DeadBuildingViews.Remove(this);
		TweenSettingsExtensions.OnComplete<TweenerCore<Color, Color, ColorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Color, Color, ColorOptions>>(DOTweenModuleSprite.DOFade(deadBuildingSpriteRenderer, 0f, fadeTime), fadeEasing), (TweenCallback)delegate
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		});
	}

	private IEnumerator WaitDisappear()
	{
		yield return SharedYields.WaitForSeconds(RandomManager.GetRandomRange(this, waitRandomRange.x, waitRandomRange.y));
		Disappear();
		waitCoroutine = null;
	}
}
