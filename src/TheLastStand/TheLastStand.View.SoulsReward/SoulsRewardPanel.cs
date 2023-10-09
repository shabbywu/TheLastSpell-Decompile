using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Yield;
using TheLastStand.Database;
using TheLastStand.Framework;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Trophy;
using TheLastStand.View.HUD;
using TheLastStand.View.Trophy;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.SoulsReward;

public class SoulsRewardPanel : MonoBehaviour
{
	public static class Constants
	{
		public const string SoulGainAnimatorFadeIn = "FadeIn";

		public const string SoulGainAnimatorFadeOut = "FadeOut";
	}

	[SerializeField]
	private CanvasGroup soulsRewardCanvasGroup;

	[SerializeField]
	private RectTransform trophyParent;

	[SerializeField]
	private TrophyDisplay trophyPrefab;

	[SerializeField]
	private RectTransform trophyBoardMask;

	[SerializeField]
	private ScrollRect trophiesScrollRect;

	[SerializeField]
	[Range(0f, 5f)]
	private float waitBetweenEachTrophyAppear = 0.6f;

	[SerializeField]
	[Range(0f, 5f)]
	private float waitAfterShowingAllTrophy = 0.5f;

	[SerializeField]
	private GameObject trophyLeftButton;

	[SerializeField]
	private GameObject trophyRightButton;

	[SerializeField]
	private RectTransform scrollViewport;

	[SerializeField]
	private HUDJoystickSimpleTarget joystickTarget;

	[SerializeField]
	private LayoutNavigationInitializer layoutNavigationInitializer;

	[SerializeField]
	private HUDJoystickTarget joystickParent;

	[SerializeField]
	private RectTransform damnedSoulsNightTotalMask;

	[SerializeField]
	private RectTransform damnedSoulsNightTotalRectTransform;

	[SerializeField]
	private TextMeshProUGUI damnedSoulsNightTotalText;

	[SerializeField]
	private TextMeshProUGUI damnedSoulsNightTotalTransparentText;

	[SerializeField]
	[Range(0f, 2f)]
	private float damnedSoulsNightPunchDuration = 0.2f;

	[SerializeField]
	[Range(1f, 3f)]
	private float damnedSoulsNightPunchStrength = 1.2f;

	[SerializeField]
	private TextMeshProUGUI damnedSoulsTotalText;

	[SerializeField]
	[Range(0f, 5f)]
	private float damnedSoulsTotalBlinkDuration = 0.4f;

	[SerializeField]
	[Range(0f, 1f)]
	private float damnedSoulsTotalBlinkFadeTo = 0.6f;

	[SerializeField]
	[Range(1f, 5000f)]
	private float damnedSoulsTransferSoulsPerSecond = 500f;

	[SerializeField]
	private Ease damnedSoulsTransferEasing = (Ease)1;

	[SerializeField]
	[Range(0f, 2f)]
	private float damnedSoulsTotalPunchDuration = 0.2f;

	[SerializeField]
	[Range(1f, 3f)]
	private float damnedSoulsTotalPunchStrength = 1.2f;

	[SerializeField]
	private Animator soulGainAnimator;

	[SerializeField]
	private Image soulGainImage;

	[SerializeField]
	private float soulGainFadeInDuration = 0.5f;

	[SerializeField]
	private float soulGainFadeOutDuration = 0.5f;

	[SerializeField]
	private float soulGainIdleLoopDurationMultiplier = 1f;

	[SerializeField]
	private float soulGainIdleLoopDurationMax = 7f;

	[SerializeField]
	private AudioSource trophyAudioSource;

	[SerializeField]
	private List<AudioClip> trophyAudioClips;

	[SerializeField]
	private AudioSource soulsBumpAudioSource;

	[SerializeField]
	private AudioSource soulsRiseAudioSource;

	[SerializeField]
	private AudioSource soulsHitAudioSource;

	[SerializeField]
	[Min(0f)]
	private float soulsRiseFadeOutDuration = 0.5f;

	[SerializeField]
	private Ease soulsRiseFadeOutEase = (Ease)4;

	private Tween damnedSoulsBlinkTween;

	private Tween damnedSoulsNightTotalBlinkTween;

	private int maxAmountOfSoulGainIdleLoops;

	private float soulGainByIdleLoop;

	private float soulGainByFade;

	private List<TrophyDisplay> trophyDisplays = new List<TrophyDisplay>();

	private bool canContinue;

	private int nextTrophySourceIndex;

	private float soulsRiseAudioSourceTargetVolume;

	private void Awake()
	{
		soulGainByIdleLoop = soulGainIdleLoopDurationMultiplier * damnedSoulsTransferSoulsPerSecond;
		soulGainByFade = (soulGainFadeInDuration + soulGainFadeOutDuration) * damnedSoulsTransferSoulsPerSecond;
		maxAmountOfSoulGainIdleLoops = Mathf.FloorToInt((soulsRiseAudioSource.clip.length - 0.5f - soulGainFadeInDuration - soulGainFadeOutDuration) / soulGainIdleLoopDurationMultiplier);
		soulsRiseAudioSourceTargetVolume = soulsRiseAudioSource.volume;
	}

	public void RefreshPosition()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		((Transform)damnedSoulsNightTotalMask).localPosition = new Vector3(0f, ((Transform)damnedSoulsNightTotalMask).localPosition.y, 0f);
		((Transform)damnedSoulsNightTotalRectTransform).localPosition = new Vector3(0f, ((Transform)damnedSoulsNightTotalRectTransform).localPosition.y, 0f);
	}

	public void Display(bool firstTimeOpenedThisNight)
	{
		((Behaviour)trophiesScrollRect).enabled = true;
		if (firstTimeOpenedThisNight)
		{
			soulsRewardCanvasGroup.alpha = 0f;
			soulsRewardCanvasGroup.blocksRaycasts = false;
			trophyLeftButton.SetActive(false);
			trophyRightButton.SetActive(false);
			return;
		}
		if (ApplicationManager.Application.RunsCompleted != 0)
		{
			soulsRewardCanvasGroup.alpha = 1f;
			soulsRewardCanvasGroup.blocksRaycasts = true;
		}
		if (InputManager.IsLastControllerJoystick && trophyDisplays.Count > 0)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.OpenHUDNavigationMode(selectDefaultPanel: false);
			TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(joystickTarget.GetSelectionInfo());
		}
	}

	public void ClearTrophies()
	{
		for (int i = 0; i < trophyDisplays.Count; i++)
		{
			((Component)trophyDisplays[i]).gameObject.SetActive(false);
		}
		trophyDisplays.Clear();
		joystickTarget.ClearSelectables();
	}

	public Selectable GetFirstSelectableTrophy()
	{
		return trophyDisplays.FirstOrDefault()?.Selectable;
	}

	public void Hide()
	{
		soulsRewardCanvasGroup.alpha = 0f;
		soulsRewardCanvasGroup.blocksRaycasts = false;
		((Behaviour)trophiesScrollRect).enabled = false;
	}

	public void MoveLeft()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		((Transform)trophyParent).localPosition = new Vector3(((Transform)trophyParent).localPosition.x + 125f, ((Transform)trophyParent).localPosition.y, ((Transform)trophyParent).localPosition.z);
	}

	public void MoveRight()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		((Transform)trophyParent).localPosition = new Vector3(((Transform)trophyParent).localPosition.x - 125f, ((Transform)trophyParent).localPosition.y, ((Transform)trophyParent).localPosition.z);
	}

	public IEnumerator ShowTrophiesPanel(bool isDefeat)
	{
		((TMP_Text)damnedSoulsTotalText).text = $"{Mathf.Max(0f, (float)(ApplicationManager.Application.DamnedSouls - TPSingleton<TrophyManager>.Instance.ComputedDamnedSoulsEarnedThisNight))}";
		DOTweenModuleUI.DOFade(soulsRewardCanvasGroup, 1f, PlayableUnitManager.DebugForceSkipNightReport ? 0f : 1f).SetFullId<TweenerCore<float, float, FloatOptions>>("SoulsRewardFadeIn", (Component)(object)this);
		soulsRewardCanvasGroup.blocksRaycasts = true;
		int nightTotal = 0;
		TrophyDisplay pooledComponent = ObjectPooler.GetPooledComponent<TrophyDisplay>("TrophyDisplays", trophyPrefab, (Transform)(object)trophyParent, dontSetParent: false);
		pooledComponent.Init(trophyParent, scrollViewport);
		string name = Localizer.Get("TrophyName_" + TrophyDatabase.DefaultTrophyDefinition.Id);
		string description = "TrophyDescription_" + TrophyDatabase.DefaultTrophyDefinition.Id;
		pooledComponent.Refresh(name, TPSingleton<TrophyManager>.Instance.DamnedSoulsEarnedThisNightWithMultiplier, description, TrophyDatabase.DefaultTrophyDefinition.IgnoreGem, TrophyDatabase.DefaultTrophyDefinition.BackgroundPath);
		pooledComponent.Show();
		trophyDisplays.Add(pooledComponent);
		soulsBumpAudioSource.Play();
		nightTotal += (int)TPSingleton<TrophyManager>.Instance.DamnedSoulsEarnedThisNightWithMultiplier;
		((TMP_Text)damnedSoulsNightTotalText).text = $"{nightTotal}";
		((TMP_Text)damnedSoulsNightTotalTransparentText).text = $"{nightTotal}";
		TweenSettingsExtensions.OnComplete<Tweener>(ShortcutExtensions.DOPunchScale((Transform)(object)((TMP_Text)damnedSoulsNightTotalText).rectTransform, Vector3.one * damnedSoulsNightPunchStrength, damnedSoulsNightPunchDuration, 1, 0.1f).SetFullId<Tweener>("NightTotalPunchScale", (Component)(object)this), (TweenCallback)delegate
		{
			canContinue = true;
		});
		TweenSettingsExtensions.OnComplete<Tweener>(ShortcutExtensions.DOPunchScale((Transform)(object)((TMP_Text)damnedSoulsNightTotalTransparentText).rectTransform, Vector3.one * damnedSoulsNightPunchStrength, damnedSoulsNightPunchDuration, 1, 0.1f).SetFullId<Tweener>("NightTotalPunchScale", (Component)(object)this), (TweenCallback)delegate
		{
			canContinue = true;
		});
		while (!canContinue)
		{
			yield return null;
		}
		canContinue = false;
		yield return SharedYields.WaitForSeconds(PlayableUnitManager.DebugForceSkipNightReport ? 0f : waitBetweenEachTrophyAppear);
		LayoutRebuilder.ForceRebuildLayoutImmediate(trophyParent);
		trophyAudioClips.Shuffle();
		List<TheLastStand.Model.Trophy.Trophy> trophies = TPSingleton<TrophyManager>.Instance.GetSuccessfulTrophies(isDefeat);
		int trophyIndex = 0;
		int trophyCount = trophies.Count;
		while (trophyIndex < trophyCount)
		{
			TheLastStand.Model.Trophy.Trophy trophy = trophies[trophyIndex];
			TrophyDisplay pooledComponent2 = ObjectPooler.GetPooledComponent<TrophyDisplay>("TrophyDisplays", trophyPrefab, (Transform)(object)trophyParent, dontSetParent: false);
			pooledComponent2.Init(trophyParent, scrollViewport);
			pooledComponent2.Refresh(trophy);
			pooledComponent2.Show();
			trophyDisplays.Add(pooledComponent2);
			trophyAudioSource.PlayOneShot(trophyAudioClips[nextTrophySourceIndex++ % trophyAudioClips.Count]);
			nightTotal += (int)trophy.TrophyDefinition.DamnedSoulsEarned;
			((TMP_Text)damnedSoulsNightTotalText).text = $"{nightTotal}";
			((TMP_Text)damnedSoulsNightTotalTransparentText).text = $"{nightTotal}";
			TweenSettingsExtensions.OnComplete<Tweener>(ShortcutExtensions.DOPunchScale((Transform)(object)((TMP_Text)damnedSoulsNightTotalText).rectTransform, Vector3.one * damnedSoulsNightPunchStrength, damnedSoulsNightPunchDuration, 1, 0.1f).SetFullId<Tweener>("NightTotalPunchScale", (Component)(object)this), (TweenCallback)delegate
			{
				canContinue = true;
			});
			TweenSettingsExtensions.OnComplete<Tweener>(ShortcutExtensions.DOPunchScale((Transform)(object)((TMP_Text)damnedSoulsNightTotalTransparentText).rectTransform, Vector3.one * damnedSoulsNightPunchStrength, damnedSoulsNightPunchDuration, 1, 0.1f).SetFullId<Tweener>("NightTotalTransparentPunchScale", (Component)(object)this), (TweenCallback)delegate
			{
				canContinue = true;
			});
			while (!canContinue)
			{
				yield return null;
			}
			canContinue = false;
			if (trophyIndex != trophyCount - 1)
			{
				yield return SharedYields.WaitForSeconds(PlayableUnitManager.DebugForceSkipNightReport ? 0f : waitBetweenEachTrophyAppear);
			}
			LayoutRebuilder.ForceRebuildLayoutImmediate(trophyParent);
			if (trophyParent.sizeDelta.x > trophyBoardMask.sizeDelta.x - 100f)
			{
				Vector3 localPosition = ((Transform)trophyParent).localPosition;
				localPosition.x -= 1000f;
				((Transform)trophyParent).localPosition = localPosition;
			}
			int num = trophyIndex + 1;
			trophyIndex = num;
		}
		layoutNavigationInitializer.InitNavigation();
		joystickTarget.AddSelectables(trophyDisplays.Select((TrophyDisplay x) => x.Selectable));
		if (InputManager.IsLastControllerJoystick && trophyDisplays.Count > 0)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.OpenHUDNavigationMode(selectDefaultPanel: false);
			TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(joystickParent.GetSelectionInfo());
		}
		trophyLeftButton.SetActive(trophyParent.sizeDelta.x > trophyBoardMask.sizeDelta.x);
		trophyRightButton.SetActive(trophyParent.sizeDelta.x > trophyBoardMask.sizeDelta.x);
		yield return PlaySoulsTransferAnim(nightTotal);
	}

	public IEnumerator PlaySoulsTransferAnim(int nightTotal)
	{
		yield return SharedYields.WaitForSeconds(PlayableUnitManager.DebugForceSkipNightReport ? 0f : waitAfterShowingAllTrophy);
		soulsRiseAudioSource.Play();
		soulsRiseAudioSource.volume = soulsRiseAudioSourceTargetVolume;
		float num = soulGainFadeInDuration + soulGainFadeOutDuration;
		int num2 = Mathf.RoundToInt(Mathf.Max((float)nightTotal - soulGainByFade, 0f) / soulGainByIdleLoop);
		num2 = Mathf.Min(num2, maxAmountOfSoulGainIdleLoops);
		num += (float)num2 * soulGainIdleLoopDurationMultiplier;
		((Behaviour)soulGainImage).enabled = true;
		soulGainAnimator.SetTrigger("FadeIn");
		TweenSettingsExtensions.SetEase<TweenerCore<int, int, NoOptions>>(DOTween.To((DOGetter<int>)(() => nightTotal), (DOSetter<int>)delegate(int x)
		{
			((TMP_Text)damnedSoulsTotalText).text = $"{ApplicationManager.Application.DamnedSouls - x}";
		}, 0, PlayableUnitManager.DebugForceSkipNightReport ? 0f : num), damnedSoulsTransferEasing).SetFullId<TweenerCore<int, int, NoOptions>>("TransferDamnedSouls", (Component)(object)this);
		DOGetter<float> obj = () => ((Transform)damnedSoulsNightTotalMask).localPosition.x;
		DOSetter<float> obj2 = delegate(float x)
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			((Transform)damnedSoulsNightTotalMask).localPosition = new Vector3(x, ((Transform)damnedSoulsNightTotalMask).localPosition.y, 0f);
			((Transform)damnedSoulsNightTotalRectTransform).localPosition = new Vector3(0f - x, ((Transform)damnedSoulsNightTotalRectTransform).localPosition.y, 0f);
		};
		Rect rect = damnedSoulsNightTotalMask.rect;
		TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTween.To(obj, obj2, ((Rect)(ref rect)).width, PlayableUnitManager.DebugForceSkipNightReport ? 0f : num), (Ease)1);
		damnedSoulsBlinkTween = (Tween)(object)TweenSettingsExtensions.SetLoops<TweenerCore<Color, Color, ColorOptions>>(DOTweenModuleUI.DOFade((Graphic)(object)damnedSoulsTotalText, damnedSoulsTotalBlinkFadeTo, damnedSoulsTotalBlinkDuration), -1, (LoopType)1).SetFullId<TweenerCore<Color, Color, ColorOptions>>("DamnedSoulsBlink", (Component)(object)this);
		damnedSoulsNightTotalBlinkTween = (Tween)(object)TweenSettingsExtensions.SetLoops<TweenerCore<Color, Color, ColorOptions>>(DOTweenModuleUI.DOFade((Graphic)(object)damnedSoulsNightTotalText, damnedSoulsTotalBlinkFadeTo, damnedSoulsTotalBlinkDuration), -1, (LoopType)1).SetFullId<TweenerCore<Color, Color, ColorOptions>>("DamnedSoulsNightTotalBlink", (Component)(object)this);
		yield return SharedYields.WaitForSeconds(PlayableUnitManager.DebugForceSkipNightReport ? 0f : (soulGainFadeInDuration + (float)num2 * soulGainIdleLoopDurationMultiplier));
		soulGainAnimator.SetTrigger("FadeOut");
		soulsHitAudioSource.Play();
		TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTweenModuleAudio.DOFade(soulsRiseAudioSource, 0f, soulsRiseFadeOutDuration), soulsRiseFadeOutEase), new TweenCallback(soulsRiseAudioSource.Stop));
		yield return SharedYields.WaitForSeconds(PlayableUnitManager.DebugForceSkipNightReport ? 0f : soulGainFadeOutDuration);
		((Behaviour)soulGainImage).enabled = false;
		TweenExtensions.Kill(damnedSoulsBlinkTween, false);
		TweenExtensions.Kill(damnedSoulsNightTotalBlinkTween, false);
		((TMP_Text)damnedSoulsNightTotalText).alpha = 1f;
		((TMP_Text)damnedSoulsTotalText).alpha = 1f;
		ShortcutExtensions.DOPunchScale((Transform)(object)((TMP_Text)damnedSoulsTotalText).rectTransform, Vector3.one * damnedSoulsTotalPunchStrength, damnedSoulsTotalPunchDuration, 1, 0.1f).SetFullId<Tweener>("DamnedSoulsTotalPunchScale", (Component)(object)this);
	}
}
