using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TPLib;
using TheLastStand.Framework.Automaton;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Model;
using TheLastStand.Model.TileMap;
using TheLastStand.View.TileMap;
using UnityEngine;

namespace TheLastStand.View;

public class FogView : MonoBehaviour
{
	public static class Constants
	{
		public static class LocalizationPrefixes
		{
			public const string FogNamePrefix = "FogName_";

			public const string FogDescriptionPrefix = "FogDescription_";
		}

		public static class Sprites
		{
			public const string FogIconPrefix = "View/Sprites/UI/Fog/Icons/FogIcon_";
		}

		public static class Names
		{
			public const string FogName = "Fog";

			public const string LightFogName = "LightFog";
		}
	}

	[SerializeField]
	private float dayIntensity;

	[SerializeField]
	private float nightIntensity = 1f;

	[SerializeField]
	private float intensityTransitionDuration = 2f;

	[SerializeField]
	private Ease nightIntensityTransitionEasing = (Ease)4;

	[SerializeField]
	private Ease dayIntensityTransitionEasing = (Ease)4;

	[SerializeField]
	private float fogAreaTransitionDuration = 2f;

	[SerializeField]
	private Ease appearFogAreaTransitionEasing = (Ease)9;

	[SerializeField]
	private Ease disappearFogAreaTransitionEasing = (Ease)9;

	[SerializeField]
	private float fogLimitDayIntensity;

	[SerializeField]
	private float fogLimitNightIntensity = 1f;

	[SerializeField]
	private Ease densityFadeInEasing = (Ease)9;

	[SerializeField]
	private float densityFadeInDuration = 0.4f;

	[SerializeField]
	private Ease densityFadeOutEasing = (Ease)9;

	[SerializeField]
	private float densityFadeOutDuration = 0.8f;

	[SerializeField]
	[Tooltip("Time to wait after night report before going to the nearest fog to show the increase.")]
	private float waitBeforeFogIncreaseSequence = 0.5f;

	[SerializeField]
	[Tooltip("Time to wait starting right after the camera movement has started to go show the nearest fog.")]
	private float waitBeforeFogIncreaseShow = 1f;

	[SerializeField]
	[Tooltip("Time to wait after the fog increaseing has been shown and the arrows has been displayed.")]
	private float waitAfterFogIncreaseShow = 1f;

	[SerializeField]
	private float fogFocusTime = 0.5f;

	[SerializeField]
	[Tooltip("The material applied to all the enemies in fog or light fog.")]
	private Material enemiesInFogMaterial;

	private Tween fogAlphaTween;

	private Color fogAreaColorInit;

	private Tween fogAreaIntensityTween;

	private float fogIntensity;

	private float fogLimitIntensity;

	private Tweener fogIntensityTweener;

	private Tweener fogLimitIntensityTweener;

	public float FogFocusTime => fogFocusTime;

	public float FogIntensity
	{
		get
		{
			return fogIntensity;
		}
		private set
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			fogIntensity = value;
			Color color = TileMapView.FogTilemap.color;
			color.a = fogIntensity;
			TileMapView.FogTilemap.color = color;
		}
	}

	public float FogLimitIntensity
	{
		get
		{
			return fogLimitIntensity;
		}
		private set
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			fogLimitIntensity = value;
			Color color = TileMapView.FogLimitTilemap.color;
			color.a = fogLimitIntensity;
			TileMapView.FogLimitTilemap.color = color;
		}
	}

	public Material EnemiesInFogMaterial => enemiesInFogMaterial;

	public float WaitAfterFogIncreaseShow => waitAfterFogIncreaseShow;

	public float WaitBeforeFogIncreaseSequence => waitBeforeFogIncreaseSequence;

	public float WaitBeforeFogIncreaseShow => waitBeforeFogIncreaseShow;

	public void DisplayFog(bool instant)
	{
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		bool flag = TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night;
		float intensity = (flag ? nightIntensity : dayIntensity);
		float limitIntensity = (flag ? fogLimitNightIntensity : fogLimitDayIntensity);
		if (flag || dayIntensity > 0f)
		{
			TileMapView.SetTiles(TileMapView.FogTilemap, TPSingleton<FogManager>.Instance.Fog.FogTiles, "View/Tiles/World/Fog");
		}
		TileMapView.FogLimitTilemap.ClearAllTiles();
		TileMapView.SetTiles(TileMapView.FogLimitTilemap, TPSingleton<FogManager>.Instance.Fog.TilesOutOfFog, "View/Tiles/Feedbacks/MistLimits/MistLimits");
		if (instant)
		{
			FogIntensity = intensity;
			FogLimitIntensity = limitIntensity;
		}
		else
		{
			((MonoBehaviour)this).StartCoroutine(SetFogAndLimitIntensityCoroutine(intensity, limitIntensity, intensityTransitionDuration, flag ? nightIntensityTransitionEasing : dayIntensityTransitionEasing));
		}
	}

	public void FadeFogTiles(IEnumerable<Tile> tiles, bool fadeIn, bool instantly = false)
	{
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		if (instantly)
		{
			foreach (Tile tile in tiles)
			{
				if (fadeIn)
				{
					TileMapView.SetTile(TileMapView.FogTilemap, tile, "View/Tiles/World/Fog");
				}
				else
				{
					TileMapView.SetTile(TileMapView.FogTilemap, tile);
				}
			}
			return;
		}
		((MonoBehaviour)this).StartCoroutine(TPSingleton<TileMapView>.Instance.FadeTilesAlphaCoroutine(tiles, fadeIn, TileMapView.FogTilemap, "View/Tiles/World/Fog", fadeIn ? densityFadeInDuration : densityFadeOutDuration, fadeIn ? densityFadeInEasing : densityFadeOutEasing));
	}

	private void Awake()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		fogAreaColorInit = TileMapView.FogAreaTilemap.color;
		DisplayFog(instant: true);
	}

	public void ChangeFogAreaIntensity(bool show)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		float num = (show ? fogAreaColorInit.a : 0f);
		TileMapView.FogAreaTilemap.color = new Color(fogAreaColorInit.r, fogAreaColorInit.g, fogAreaColorInit.b, show ? 0f : fogAreaColorInit.a);
		Tween obj = fogAreaIntensityTween;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
		fogAreaIntensityTween = (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(TweenExtensions.SetFullId<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)GetFogAlpha, (DOSetter<float>)SetFogAlpha, num, fogAreaTransitionDuration), "FogAreaIntensity", (Component)(object)this), show ? appearFogAreaTransitionEasing : disappearFogAreaTransitionEasing);
	}

	private float GetFogAlpha()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (!(((StateMachine)ApplicationManager.Application).State.GetName() == "Game"))
		{
			return 0f;
		}
		return TileMapView.FogAreaTilemap.color.a;
	}

	private void SetFogAlpha(float value)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		if (!(((StateMachine)ApplicationManager.Application).State.GetName() != "Game"))
		{
			TileMapView.FogAreaTilemap.color = new Color(fogAreaColorInit.r, fogAreaColorInit.g, fogAreaColorInit.b, value);
		}
	}

	private IEnumerator SetFogAndLimitIntensityCoroutine(float intensity, float limitIntensity, float duration, Ease easing)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (fogIntensityTweener != null && TweenExtensions.IsPlaying((Tween)(object)fogIntensityTweener))
		{
			fogIntensityTweener.ChangeEndValue((object)intensity, duration, true);
		}
		else
		{
			fogIntensityTweener = (Tweener)(object)TweenSettingsExtensions.OnKill<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetId<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => FogIntensity), (DOSetter<float>)delegate(float v)
			{
				FogIntensity = v;
			}, intensity, duration), "FogIntensity"), easing), (TweenCallback)delegate
			{
				fogIntensityTweener = null;
			});
		}
		if (fogLimitIntensityTweener != null && TweenExtensions.IsPlaying((Tween)(object)fogLimitIntensityTweener))
		{
			fogLimitIntensityTweener.ChangeEndValue((object)limitIntensity, duration, true);
		}
		else
		{
			fogLimitIntensityTweener = (Tweener)(object)TweenSettingsExtensions.OnKill<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetId<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => FogLimitIntensity), (DOSetter<float>)delegate(float v)
			{
				FogLimitIntensity = v;
			}, limitIntensity, duration), "FogLimitIntensity"), easing), (TweenCallback)delegate
			{
				fogLimitIntensityTweener = null;
			});
		}
		yield return TweenExtensions.WaitForCompletion((Tween)(object)fogIntensityTweener);
		if (fogLimitIntensityTweener != null)
		{
			yield return TweenExtensions.WaitForCompletion((Tween)(object)fogLimitIntensityTweener);
		}
	}
}
