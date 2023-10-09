using System;
using System.Collections;
using Com.LuisPedroFonseca.ProCamera2D;
using TPLib;
using TPLib.Yield;
using TheLastStand.Controller;
using TheLastStand.Controller.Settings;
using TheLastStand.Database.WorldMap;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.SDK;
using TheLastStand.Manager.Sound;
using TheLastStand.Manager.Unit;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model;
using TheLastStand.View.Building.UI;
using TheLastStand.View.Camera;
using TheLastStand.View.Cursor;
using TheLastStand.View.Seer;
using TheLastStand.View.Sound;
using TheLastStand.View.ToDoList;
using UnityEngine;
using UnityEngine.Serialization;

namespace TheLastStand.View.Building;

public class MagicCircleView : BuildingView
{
	public static class AnimatorParameters
	{
		public const string MageDeathSteps = "death_step";

		public const string MageDeath = "is_dead";

		public const string MageRevive = "revive";

		public const string MagicCircleDestroyed = "is_destroyed";

		public const string MagicCircleIdle = "idle";

		public const string MagicCircleTakeDamage = "take_damage";
	}

	public new static class Constants
	{
		public const string AnimationBaseMagicCirclePath = "Animation/MagicCircle/";

		public const string AnimationNamePrefix = "MagicCircle_";

		public const string IdleSuffix = "Idle";

		public const string HitSuffix = "Hit";

		public const string DestructionSuffix = "Destruction";

		public const string IdleDestroyedSuffix = "IdleDestroyed";

		public const string SpriteBaseMagicCirclePath = "View/Tiles/Buildings/Diffuse/MagicCircle/";

		public const string SpriteBaseNamePrefix = "MagicCircle_Base_";

		public const int MaxMages = 4;
	}

	[SerializeField]
	private float fadeDuration;

	[SerializeField]
	private AnimationCurve fadeCurve;

	[SerializeField]
	private float gameOverShakeDuration;

	[SerializeField]
	private AnimationCurve gameOverShakeCurve;

	[SerializeField]
	private int gameOverShakeIntensityMultiplier = 1;

	[SerializeField]
	private float victoryShakeDuration;

	[SerializeField]
	private AnimationCurve victoryShakeCurve;

	[SerializeField]
	private float victoryShakeIntensityMultiplier = 1f;

	[SerializeField]
	private float victoryExplosionShake = 3.3f;

	[SerializeField]
	private float pillarShakeIntensity = 0.25f;

	[SerializeField]
	private int pillarShakeVibrato = 50;

	[SerializeField]
	private RandomIdleDuration[] magesAnimators;

	[SerializeField]
	private ShakePreset onHitShake;

	[SerializeField]
	private Animator animator;

	[SerializeField]
	private MagicCircleBaseView baseView;

	[SerializeField]
	private MagicCircleSealView sealView;

	[SerializeField]
	private SpriteRenderer[] idleRenderers;

	[SerializeField]
	private SpriteRenderer[] hitRenderers;

	[SerializeField]
	[FormerlySerializedAs("mages")]
	private SpriteRenderer[] mageRenderers;

	[SerializeField]
	private float timeIntervalBetweenIdleAnimations = 5f;

	[SerializeField]
	private OneShotSound sfxPrefab;

	[SerializeField]
	private Animator firstMageAnimator;

	[SerializeField]
	private RuntimeAnimatorController commanderAnimator;

	[SerializeField]
	private RuntimeAnimatorController commanderEyeOfTheLawAnimator;

	[SerializeField]
	private SpriteRenderer firstMageSpriteRenderer;

	[SerializeField]
	private Material commanderLutMaterial;

	private int destructionStage;

	private Action[] onDestructionStages = new Action[3];

	private IEnumerator idleCoroutine;

	private IEnumerator shakeCoroutine;

	public bool Dirty { get; set; }

	public SpriteRenderer[] MageRenderers => mageRenderers;

	public MagicCircleHUD MagicCircleHUD => base.BuildingHUD as MagicCircleHUD;

	public MagicCircleSealView SealView => sealView;

	public OneShotSound SFXPrefab => sfxPrefab;

	public override void InitVisuals()
	{
		base.InitVisuals();
		for (int i = 0; i < mageRenderers.Length; i++)
		{
			((Component)mageRenderers[i]).gameObject.SetActive(false);
		}
		if (TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.UseCommanderAsMage)
		{
			firstMageAnimator.runtimeAnimatorController = commanderAnimator;
			((Renderer)firstMageSpriteRenderer).material = commanderLutMaterial;
		}
		InitAnimations();
		sealView.InitAnimations();
		baseView.InitAnimations();
		RefreshSlotsAndMagesQuantity();
		idleCoroutine = DisplayIdleAnimation();
		((MonoBehaviour)TPSingleton<GameManager>.Instance).StartCoroutine(idleCoroutine);
		MagicCircleHUD.ProductionPanelMagicCircle?.UnitsGauge.SetUnitsCount(CityDatabase.CityDefinitions[TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id].VictoryDaysCount);
		Dirty = true;
	}

	public void InitAnimations()
	{
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Expected O, but got Unknown
		AnimationClip val = ResourcePooler.LoadOnce<AnimationClip>("Animation/MagicCircle/MagicCircle_Idle/MagicCircle_" + TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id + "_Idle", failSilently: false);
		AnimationClip val2 = ResourcePooler.LoadOnce<AnimationClip>("Animation/MagicCircle/MagicCircle_Hit/MagicCircle_" + TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id + "_Hit", failSilently: false);
		AnimationClip val3 = ResourcePooler.LoadOnce<AnimationClip>("Animation/MagicCircle/MagicCircle_Destruction/MagicCircle_" + TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id + "_Destruction", failSilently: true);
		AnimationClip val4 = ResourcePooler.LoadOnce<AnimationClip>("Animation/MagicCircle/MagicCircle_IdleDestroyed/MagicCircle_" + TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id + "_IdleDestroyed", failSilently: true);
		AnimatorOverrideController val5 = new AnimatorOverrideController(animator.runtimeAnimatorController);
		if ((Object)(object)val != (Object)null)
		{
			val5["MagicCircle_Idle"] = val;
		}
		if ((Object)(object)val2 != (Object)null)
		{
			val5["MagicCircle_Hit"] = val2;
		}
		if ((Object)(object)val3 != (Object)null)
		{
			val5["MagicCircle_Destruction"] = val3;
		}
		if ((Object)(object)val4 != (Object)null)
		{
			val5["MagicCircle_IdleDestroyed"] = val4;
		}
		animator.runtimeAnimatorController = (RuntimeAnimatorController)(object)val5;
	}

	public void NextDestructionStage()
	{
		if (destructionStage < onDestructionStages.Length)
		{
			onDestructionStages[destructionStage]();
		}
		destructionStage++;
	}

	public override void PlayDieAnim()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		SettingsController.ToggleGameSpeed(isOn: false);
		TPSingleton<SoundManager>.Instance.StopMusic();
		TPSingleton<LightningSDKManager>.Instance.TransitionToColor(Color32.op_Implicit(Color.black));
		NightTurnsManager.ForceStopTurnExecution();
		TileObjectSelectionManager.DeselectAll();
		CursorView.ClearTiles();
		TPSingleton<SeerPreviewDisplay>.Instance.Displayed = false;
		TPSingleton<ToDoListView>.Instance.Hide();
		GameView.TopScreenPanel.Display(show: false);
		PanicManager.Panic.PanicView.Hide();
		((MonoBehaviour)this).StartCoroutine(TPSingleton<UIManager>.Instance.ToggleUICoroutine());
		SpawnWaveManager.SpawnWaveView.Refresh();
		for (int num = TPSingleton<BuildingManager>.Instance.Buildings.Count - 1; num >= 0; num--)
		{
			TPSingleton<BuildingManager>.Instance.Buildings[num].BuildingView.BuildingHUD.DisplayHealthIfNeeded();
		}
		ObjectPooler.GetPooledComponent<OneShotSound>("HitsSFX", TPSingleton<PlayableUnitManager>.Instance.HitSFXPrefab, (Transform)null, dontSetParent: false).Play(TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.IsTutorialMap ? GameManager.TutorialDefeatAudioClip : GameManager.DefeatAudioClip);
		((MonoBehaviour)this).StartCoroutine(StartDestructionAnimation());
	}

	public override void PlayTakeDamageAnim()
	{
		base.PlayTakeDamageAnim();
		ToggleHitRenderers(isEnabled: true);
		ACameraView.Shake(((Object)onHitShake).name);
		animator.SetTrigger("take_damage");
		RefreshSlotsAndMagesQuantity();
	}

	public void PlayAllMagesCastAnimation()
	{
		for (int num = magesAnimators.Length - 1; num >= 0; num--)
		{
			magesAnimators[num].PlayAnimation();
		}
	}

	public void PlayCommanderFadeOutAnim()
	{
		firstMageAnimator.Play("Commander_EyeOfLaw_FadeOut");
	}

	public void ReplaceCommanderView()
	{
		firstMageAnimator.runtimeAnimatorController = commanderEyeOfTheLawAnimator;
	}

	public void StopIdleAnimation()
	{
		((MonoBehaviour)TPSingleton<GameManager>.Instance).StopCoroutine(idleCoroutine);
		for (int num = magesAnimators.Length - 1; num >= 0; num--)
		{
			magesAnimators[num].Stop();
		}
	}

	protected override void InitHud()
	{
		base.BuildingHUD = Object.Instantiate<BuildingHUD>(hudPrefab, BuildingManager.BuildingsHudsTransform);
	}

	private IEnumerator DisplayIdleAnimation()
	{
		while (true)
		{
			yield return SharedYields.WaitForSeconds(timeIntervalBetweenIdleAnimations);
			if ((ApplicationManager.Application.State.GetName() == "LevelEditor" && BuildingManager.MagicCircle != base.BuildingController.Building) || base.BuildingController.Building.DamageableModule.IsDead)
			{
				break;
			}
			ToggleIdleRenderers(isEnabled: true);
			animator.SetTrigger("idle");
		}
	}

	private void RefreshSlotsAndMagesQuantity()
	{
		for (int i = 0; i < mageRenderers.Length; i++)
		{
			if (i >= BuildingManager.MagicCircle.MageCount && ((Renderer)mageRenderers[i]).enabled)
			{
				((MonoBehaviour)this).StartCoroutine(TriggerMageDeathAnimation(((Component)mageRenderers[i]).gameObject));
			}
			else if (i < BuildingManager.MagicCircle.MageCount && (!((Component)mageRenderers[i]).gameObject.activeSelf || !((Renderer)mageRenderers[i]).enabled))
			{
				((Component)mageRenderers[i]).gameObject.SetActive(true);
				((Component)mageRenderers[i]).GetComponent<Animator>().SetTrigger("revive");
			}
		}
		baseView.RefreshAnimationBaseWithMagesQuantity(BuildingManager.MagicCircle.MageSlots);
	}

	private void Start()
	{
		ref Action reference = ref onDestructionStages[0];
		reference = (Action)Delegate.Combine(reference, (Action)delegate
		{
			((MonoBehaviour)this).StartCoroutine(GameOverShakeCoroutine());
		});
		ref Action reference2 = ref onDestructionStages[1];
		reference2 = (Action)Delegate.Combine(reference2, (Action)delegate
		{
			((MonoBehaviour)this).StartCoroutine(FadeCoroutine());
		});
		ref Action reference3 = ref onDestructionStages[1];
		reference3 = (Action)Delegate.Combine(reference3, (Action)delegate
		{
			((MonoBehaviour)this).StartCoroutine(BuildingManager.DestroyAll(BuildingManager.MagicCircleExplosionForce));
		});
		ref Action reference4 = ref onDestructionStages[2];
		reference4 = (Action)Delegate.Combine(reference4, (Action)delegate
		{
			((MonoBehaviour)this).StartCoroutine(DisplayGameOverScreen());
		});
	}

	private void ToggleHitRenderers(bool isEnabled = false, bool reciprocate = true)
	{
		for (int num = hitRenderers.Length - 1; num >= 0; num--)
		{
			((Renderer)hitRenderers[num]).enabled = isEnabled;
		}
		if (reciprocate)
		{
			ToggleIdleRenderers(!isEnabled, reciprocate: false);
		}
	}

	private void ToggleIdleRenderers(bool isEnabled = false, bool reciprocate = true)
	{
		for (int num = idleRenderers.Length - 1; num >= 0; num--)
		{
			((Renderer)idleRenderers[num]).enabled = isEnabled;
		}
		if (reciprocate)
		{
			ToggleHitRenderers(!isEnabled, reciprocate: false);
		}
	}

	public IEnumerator TriggerMageDeathAnimation(GameObject mage, bool instant = false)
	{
		if (!instant)
		{
			yield return SharedYields.WaitForSeconds(Random.Range(0.6f, 1.6f));
		}
		mage.GetComponent<Animator>().SetTrigger("is_dead");
	}

	private void Update()
	{
		if (Dirty && BuildingManager.MagicCircle != null)
		{
			RefreshSlotsAndMagesQuantity();
			Dirty = false;
		}
	}

	private IEnumerator StartDestructionAnimation()
	{
		yield return SharedYields.WaitForSeconds(TPSingleton<CutsceneManager>.Instance.TutorialSequenceView.IsPlaying ? 0.5f : 5f);
		animator.SetTrigger("is_destroyed");
		baseView.DisableAnimator();
	}

	private IEnumerator FadeCoroutine()
	{
		float timeSpent = 0f;
		while (timeSpent <= fadeDuration)
		{
			float num = fadeCurve.Evaluate(timeSpent / fadeDuration);
			TPSingleton<LightningSDKManager>.Instance.SetAllLightsToColor(Color32.op_Implicit(Color.white * num));
			UIManager.WhiteScreen.alpha = num;
			timeSpent += Time.deltaTime;
			yield return SharedYields.WaitForEndOfFrame;
		}
	}

	private IEnumerator DisplayGameOverScreen()
	{
		ACameraView.StopShaking();
		yield return SharedYields.WaitForSeconds(2f);
		GameController.TriggerGameOver(Game.E_GameOverCause.MagicCircleDestroyed);
	}

	private IEnumerator GameOverShakeCoroutine()
	{
		float timeSpent = 0f;
		while (timeSpent <= gameOverShakeDuration)
		{
			ACameraView.Shake(0.4f, Vector2.one * (float)gameOverShakeIntensityMultiplier * gameOverShakeCurve.Evaluate(timeSpent / gameOverShakeDuration), 10, 0.5f, 0f, Vector3.zero, 0.05f);
			timeSpent += 0.2f;
			yield return SharedYields.WaitForSeconds(0.2f);
		}
	}

	public void PlaySealAnticipationAnimation()
	{
		sealView.PlaySealAnticipationAnimation();
		shakeCoroutine = SealAnticipationShakeCoroutine();
		((MonoBehaviour)this).StartCoroutine(shakeCoroutine);
	}

	public IEnumerator PlaySealTransitionAnimation()
	{
		yield return sealView.PlaySealTransitionAnimation();
		StopShakeCoroutine();
		CameraView.RippleEffect.RippleAtWorldPosition(((Component)this).transform.position + new Vector3(0f, 2.5f));
		ACameraView.Shake(0.5f, Vector2.one * victoryExplosionShake, 10, 0.5f, 0f, Vector3.zero, 0.05f);
		shakeCoroutine = PillarShakeCoroutine();
		((MonoBehaviour)this).StartCoroutine(shakeCoroutine);
	}

	public void StopShakeCoroutine()
	{
		if (shakeCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(shakeCoroutine);
			ACameraView.StopShaking();
		}
	}

	private IEnumerator SealAnticipationShakeCoroutine()
	{
		float timeSpent = 0f;
		while (timeSpent <= victoryShakeDuration)
		{
			ACameraView.Shake(0.4f, Vector2.one * victoryShakeIntensityMultiplier * victoryShakeCurve.Evaluate(timeSpent / victoryShakeDuration), 10, 0.5f, 0f, Vector3.zero, 0.05f);
			timeSpent += 0.2f;
			yield return SharedYields.WaitForSeconds(0.2f);
		}
	}

	private IEnumerator PillarShakeCoroutine()
	{
		while (true)
		{
			ACameraView.Shake(1f, Vector2.one * pillarShakeIntensity, pillarShakeVibrato, 0.2f, 0f, Vector3.zero, 0.05f);
			yield return null;
		}
	}
}
