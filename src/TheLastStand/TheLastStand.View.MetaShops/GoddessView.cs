using System;
using System.Collections;
using Coffee.UIExtensions;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TPLib;
using TheLastStand.DRM.Achievements;
using TheLastStand.Manager.Achievements;
using UnityEngine;

namespace TheLastStand.View.MetaShops;

public class GoddessView : MonoBehaviour
{
	private static class Constants
	{
		public const string IdleAnimatorParameterName = "Idle";
	}

	[Tooltip("Transform containing all the goddess visuals (sprites and particles).")]
	[SerializeField]
	private RectTransform goddessScaler;

	[SerializeField]
	private GameObject fadeInContainer;

	[SerializeField]
	private GameObject idleContainer;

	[SerializeField]
	private RectTransform goddessContainer;

	[Tooltip("Idle animator to switch between visual evolutions.")]
	[SerializeField]
	private Animator goddessAnimator;

	[Tooltip("Script attached to animator responsible of goddess appearance.")]
	[SerializeField]
	private GoddessAnimatorEventHandler goddessAnimatorEventHandler;

	[Tooltip("Script attached to animator responsible of goddess appearance.")]
	[SerializeField]
	private UIParticle[] particles;

	[Tooltip("Offset applied to goddess when being offset to show replicas (in screen space). Should be negative for one of the goddesses.")]
	[SerializeField]
	private float goddessOffset = 300f;

	[Tooltip("Duration the goddess takes to go from its initial position to the offset position.")]
	[SerializeField]
	private float goddessOffsetDuration = 0.3f;

	[Tooltip("Curve the goddess uses to go from its initial position to the offset position.")]
	[SerializeField]
	private Ease goddessOffsetEase = (Ease)21;

	public int CurrentEvolutionIndex { get; private set; }

	public GameObject IdleContainer => idleContainer;

	public GameObject FadeInContainer => fadeInContainer;

	public float GoddessOffset => goddessOffset;

	public void ChangeEvolution(int index)
	{
		CurrentEvolutionIndex = index;
		goddessAnimator.SetTrigger(string.Format("{0}{1}", "Idle", CurrentEvolutionIndex));
		if (index > 0)
		{
			TPSingleton<AchievementManager>.Instance.UnlockAchievement(TPSingleton<OraculumView>.Instance.IsInLightShop ? AchievementContainer.ACH_FREUDE_REVEAL : AchievementContainer.ACH_SCHADEN_REVEAL);
		}
	}

	public void OffsetAfterGreeting(bool instantly = false)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		if (instantly)
		{
			SetPositionX(GoddessOffset);
			return;
		}
		TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => 0f), (DOSetter<float>)SetPositionX, GoddessOffset, goddessOffsetDuration), goddessOffsetEase);
	}

	public IEnumerator PlayVisualAnimationAtIndexCoroutine(int index)
	{
		if (((Behaviour)goddessAnimator).isActiveAndEnabled)
		{
			goddessAnimatorEventHandler.AppearFrame = false;
			yield return (object)new WaitUntil((Func<bool>)(() => goddessAnimatorEventHandler.AppearFrame));
			goddessAnimatorEventHandler.AppearFrame = false;
		}
		yield return (object)new WaitUntil((Func<bool>)(() => ((Behaviour)goddessAnimator).isActiveAndEnabled));
		ChangeEvolution(index);
	}

	public void Refresh()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		((Transform)goddessScaler).localScale = Vector3.one * ((Screen.height > 768) ? 2f : 1f);
		for (int num = particles.Length - 1; num >= 0; num--)
		{
			particles[num].scale = (float)Screen.height / 1080f;
		}
	}

	public void SetPositionX(float x)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		goddessScaler.anchoredPosition = new Vector2(x, goddessScaler.anchoredPosition.y);
	}

	private void Awake()
	{
		goddessAnimator.keepAnimatorControllerStateOnDisable = true;
	}

	[ContextMenu("Locate Particles In Children")]
	private void LocateParticlesInChildren()
	{
		particles = ((Component)goddessContainer).GetComponentsInChildren<UIParticle>();
	}
}
