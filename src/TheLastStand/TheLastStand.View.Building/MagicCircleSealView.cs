using System;
using System.Collections;
using Com.LuisPedroFonseca.ProCamera2D;
using TPLib;
using TPLib.Yield;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Sound;
using TheLastStand.Manager.WorldMap;
using TheLastStand.View.Camera;
using UnityEngine;
using UnityEngine.Rendering;

namespace TheLastStand.View.Building;

public class MagicCircleSealView : MonoBehaviour
{
	private static class AnimatorParameters
	{
		public const string VictoryAnticipation = "victory_anticipation";

		public const string VictoryTransition = "victory_transition";

		public const string VictoryAnticipationMultiplier = "anticipation_multiplier";
	}

	public static class Constants
	{
		public const string AnimationBaseMagicCirclePath = "Animation/MagicCircle/";

		public const string AnimationNamePrefix = "MagicCircle_";

		public const string AnimationNameSealPrefix = "MagicCircle_Seal_";

		public const string IdleSuffix = "Idle";

		public const string AnticipationSuffix = "Anticipation";

		public const string PillarIdleSuffix = "Pillar_Idle";
	}

	[SerializeField]
	private Animator animator;

	[SerializeField]
	private Animator pillarAnimator;

	[SerializeField]
	private SortingGroup sortingGroup;

	[SerializeField]
	private SortingGroup[] mageSortingGroups;

	[SerializeField]
	private SpriteRenderer pillar;

	[SerializeField]
	private SpriteRenderer pillarFlash;

	[SerializeField]
	private int cutsceneSealOrder = 100;

	[SerializeField]
	private AudioSource magesCastStartAudioSource;

	[SerializeField]
	private AudioSource pillarAppearAudioSource;

	[SerializeField]
	private AudioSource pillarLoopAudioSource;

	[SerializeField]
	private AudioClip[] magesCastStartSfx;

	[SerializeField]
	private AudioClip[] pillarAppearSfx;

	[SerializeField]
	private AudioClip pillarLoopSfx;

	[SerializeField]
	private float delayBeforeAcceleration = 1f;

	[SerializeField]
	private float anticipationMultiplierMax = 2f;

	[SerializeField]
	private float anticipationMultiplierIncreasePerSec = 0.08f;

	private float anticipationMultiplier;

	private IEnumerator sealAccelerationCoroutine;

	public void InitAnimations()
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Expected O, but got Unknown
		AnimationClip val = ResourcePooler.LoadOnce<AnimationClip>("Animation/MagicCircle/MagicCircle_Seal_Idle/MagicCircle_Seal_" + TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id + "_Idle", false);
		AnimationClip val2 = ResourcePooler.LoadOnce<AnimationClip>("Animation/MagicCircle/MagicCircle_Seal_Anticipation/MagicCircle_Seal_" + TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id + "_Anticipation", false);
		if (!((Object)(object)val == (Object)null) || !((Object)(object)val2 == (Object)null))
		{
			AnimatorOverrideController val3 = new AnimatorOverrideController(animator.runtimeAnimatorController);
			if ((Object)(object)val != (Object)null)
			{
				val3["MagicCircle_Seal_Idle"] = val;
			}
			if ((Object)(object)val2 != (Object)null)
			{
				val3["MagicCircle_Seal_Anticipation"] = val2;
			}
			animator.runtimeAnimatorController = (RuntimeAnimatorController)(object)val3;
			InitPillarAnimations();
		}
	}

	public void InitPillarAnimations()
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Expected O, but got Unknown
		string text = ((!TPSingleton<CutsceneManager>.Exist() || string.IsNullOrEmpty(TPSingleton<CutsceneManager>.Instance.VictorySequenceView.debugCityIdOverride)) ? TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id : TPSingleton<CutsceneManager>.Instance.VictorySequenceView.debugCityIdOverride);
		AnimationClip val = ResourcePooler.LoadOnce<AnimationClip>("Animation/MagicCircle/MagicCircle_Pillar_Idle/MagicCircle_" + text + "_Pillar_Idle", false);
		if (!((Object)(object)val == (Object)null))
		{
			AnimatorOverrideController val2 = new AnimatorOverrideController(pillarAnimator.runtimeAnimatorController);
			val2["MagicCircle_Pillar_Idle"] = val;
			pillarAnimator.runtimeAnimatorController = (RuntimeAnimatorController)(object)val2;
		}
	}

	public void OnMagesCastStart()
	{
		BuildingManager.MagicCircle.MagicCircleView.PlayAllMagesCastAnimation();
		for (int i = 0; i < magesCastStartSfx.Length; i++)
		{
			if ((Object)(object)magesCastStartSfx[i] != (Object)null)
			{
				SoundManager.PlayAudioClip(magesCastStartAudioSource, magesCastStartSfx[i]);
			}
		}
	}

	public void OnPillarAppearFrame()
	{
		ACameraView.Zoom(zoomIn: false);
		((MonoBehaviour)TPSingleton<BuildingManager>.Instance).StartCoroutine(BuildingManager.DestroyAll(4, 8f));
		CameraView.CameraVisualEffects.ToggleChromaticAberration(state: true);
		CameraView.CameraLutView.TogglePillarLut(state: true);
		for (int i = 0; i < pillarAppearSfx.Length; i++)
		{
			if ((Object)(object)pillarAppearSfx[i] != (Object)null)
			{
				SoundManager.PlayAudioClip(pillarAppearAudioSource, pillarAppearSfx[i]);
			}
		}
		if ((Object)(object)pillarLoopSfx != (Object)null)
		{
			SoundManager.PlayAudioClip(pillarLoopAudioSource, pillarLoopSfx, 0f, doNotInterrupt: true);
		}
		AdjustPillarHeight();
		((Renderer)pillar).enabled = true;
	}

	public void OnPulseFrame()
	{
	}

	public void OnInvertImage()
	{
		CameraView.CameraVisualEffects.InvertImage();
	}

	public void OnResetInvertImage()
	{
		CameraView.CameraVisualEffects.ResetInvertEffect();
	}

	public void OnPillarsCutsceneStart()
	{
		pillarLoopAudioSource.Stop();
	}

	public void SetSortingOrder(int order)
	{
		sortingGroup.sortingOrder = order;
		for (int i = 0; i < mageSortingGroups.Length; i++)
		{
			mageSortingGroups[i].sortingOrder = order;
		}
	}

	public void PlaySealAnticipationAnimation()
	{
		anticipationMultiplier = 1f;
		animator.SetFloat("anticipation_multiplier", anticipationMultiplier);
		sealAccelerationCoroutine = AccelerateSealOverTime();
		((MonoBehaviour)this).StartCoroutine(sealAccelerationCoroutine);
		animator.SetTrigger("victory_anticipation");
	}

	public IEnumerator PlaySealTransitionAnimation()
	{
		((MonoBehaviour)this).StopCoroutine(sealAccelerationCoroutine);
		SetSortingOrder(cutsceneSealOrder);
		animator.SetTrigger("victory_transition");
		yield return (object)new WaitUntil((Func<bool>)(() => ((Renderer)pillar).enabled));
	}

	private IEnumerator AccelerateSealOverTime()
	{
		yield return SharedYields.WaitForSeconds(delayBeforeAcceleration);
		while (true)
		{
			anticipationMultiplier = Mathf.Clamp(anticipationMultiplier + anticipationMultiplierIncreasePerSec * Time.deltaTime, 0f, anticipationMultiplierMax);
			animator.SetFloat("anticipation_multiplier", anticipationMultiplier);
			yield return null;
		}
	}

	private void Awake()
	{
		((Renderer)pillar).enabled = false;
	}

	private void AdjustPillarHeight()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		ProCamera2DNumericBoundaries proCamera2DNumericBoundaries = CameraView.ProCamera2DNumericBoundaries;
		Vector2 size = pillar.size;
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector(size.x, proCamera2DNumericBoundaries.TopBoundary);
		pillar.size = val;
		pillarFlash.size = new Vector2(proCamera2DNumericBoundaries.RightBoundary - proCamera2DNumericBoundaries.LeftBoundary, proCamera2DNumericBoundaries.TopBoundary - proCamera2DNumericBoundaries.BottomBoundary);
		float num = val.y - size.y;
		Vector2 pivot = pillar.sprite.pivot;
		Rect rect = pillar.sprite.rect;
		Vector2 val2 = pivot / ((Rect)(ref rect)).size.y;
		Transform transform = ((Component)pillar).transform;
		transform.position += new Vector3(0f, num * val2.y);
	}
}
