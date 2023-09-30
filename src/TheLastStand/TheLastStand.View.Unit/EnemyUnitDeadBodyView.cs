using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TPLib;
using TPLib.Yield;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Model;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit.Enemy;
using UnityEngine;

namespace TheLastStand.View.Unit;

[SelectionBase]
public class EnemyUnitDeadBodyView : MonoBehaviour
{
	[SerializeField]
	private SpriteRenderer deadBodySpriteRend;

	[SerializeField]
	private Vector2 waitRandomRange = new Vector2(1f, 300f);

	[SerializeField]
	[Range(0f, 3f)]
	private float fadeTime = 0.5f;

	[SerializeField]
	private Ease fadeEasing = (Ease)2;

	[SerializeField]
	[Range(-3f, 0f)]
	private float translateMoveY = -0.15f;

	[SerializeField]
	[Range(0f, 3f)]
	private float translateTime = 0.5f;

	[SerializeField]
	private Ease translateEasing = (Ease)8;

	private Coroutine waitCoroutine;

	public Tile Tile { get; set; }

	public void StartWaitDisappear()
	{
		if (waitCoroutine == null)
		{
			waitCoroutine = ((MonoBehaviour)this).StartCoroutine(WaitDisappear());
		}
	}

	public void Init(EnemyUnit enemyUnit)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		Transform transform = ((Component)this).transform;
		transform.position += Vector2.op_Implicit(enemyUnit.EnemyUnitTemplateDefinition.VisualOffset);
		((Component)this).transform.localScale = new Vector3(enemyUnit.EnemyUnitView.OrientationRootTransform.localScale.x, ((Component)this).transform.localScale.y, ((Component)this).transform.localScale.z);
		string text = ((enemyUnit is EliteEnemyUnit) ? ("View/Sprites/Units/" + enemyUnit.EnemyUnitView.GetDefaultSpritesFolder() + "/DeadBodies/" + enemyUnit.SpecificId + "/" + enemyUnit.VariantId + "/" + enemyUnit.SpecificId + "_" + enemyUnit.VariantId + "_DeadBody_Front") : ("View/Sprites/Units/" + enemyUnit.EnemyUnitView.GetDefaultSpritesFolder() + "/DeadBodies/" + enemyUnit.SpecificAssetsId + "/" + enemyUnit.VariantId + "/" + enemyUnit.SpecificAssetsId + "_Lvl1_" + enemyUnit.VariantId + "_DeadBody_Front"));
		deadBodySpriteRend.sprite = ResourcePooler.LoadOnce<Sprite>(text, false);
		((Renderer)deadBodySpriteRend).sortingOrder = Tile.EnemyUnitDeadBodyViews.Count;
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Day && waitCoroutine == null)
		{
			waitCoroutine = ((MonoBehaviour)this).StartCoroutine(WaitDisappear());
		}
	}

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

	private void Disappear()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Expected O, but got Unknown
		Tile.EnemyUnitDeadBodyViews.Remove(this);
		TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOLocalMoveY(((Component)deadBodySpriteRend).transform, ((Component)deadBodySpriteRend).transform.localPosition.y + translateMoveY, translateTime, false), translateEasing);
		TweenSettingsExtensions.OnComplete<TweenerCore<Color, Color, ColorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Color, Color, ColorOptions>>(DOTweenModuleSprite.DOFade(deadBodySpriteRend, 0f, fadeTime), fadeEasing), (TweenCallback)delegate
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
