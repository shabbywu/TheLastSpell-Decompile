using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Definition.Panic;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Model;
using TheLastStand.Model.Panic;
using TheLastStand.View.NightReport;
using UnityEngine;

namespace TheLastStand.View.Panic;

public class PanicRewardIndicator : MonoBehaviour
{
	[SerializeField]
	[Range(0f, 10f)]
	private float timeToMoveFromMaxToMin = 3f;

	[SerializeField]
	private Ease moveEasing = (Ease)9;

	[SerializeField]
	[Range(0f, 5f)]
	private float movePauseAtEachPanicLevel = 0.7f;

	[SerializeField]
	[Range(0f, 5f)]
	private float blinkDuration = 0.4f;

	[SerializeField]
	[Range(0f, 1f)]
	private float blinkFadeTo = 0.7f;

	[SerializeField]
	[Range(0f, 2f)]
	private float punchScaleDuration = 0.2f;

	[SerializeField]
	[Range(1f, 3f)]
	private float punchScaleStrength = 1.1f;

	[SerializeField]
	private RectTransform handleRect;

	[SerializeField]
	private TextMeshProUGUI goldRewardText;

	[SerializeField]
	private TextMeshProUGUI materialRewardText;

	[SerializeField]
	private TextMeshProUGUI itemRewardText;

	[SerializeField]
	private CanvasGroup rewardTextsCanvasGroup;

	[SerializeField]
	private AudioClip[] panicStepsMoveAudioClip;

	[SerializeField]
	private AudioClip[] panicStepsCoinAudioClip;

	private Tween goldTween;

	private Tween itemsTween;

	private Tween materialTween;

	private Tween blinkTween;

	private int lastGold;

	private int lastItemsCount;

	private int lastMaterials;

	public bool IsMoving { get; set; }

	public void Init(TheLastStand.Model.Panic.Panic panic)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		handleRect.anchorMin = new Vector2(1f, handleRect.anchorMin.y);
		handleRect.anchorMax = new Vector2(1f, handleRect.anchorMax.y);
		lastGold = 0;
		lastItemsCount = 0;
		lastMaterials = 0;
		RefreshValues(panic, forceRefresh: true);
	}

	public void Refresh(TheLastStand.Model.Panic.Panic panic, bool instant = false)
	{
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Expected O, but got Unknown
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Expected O, but got Unknown
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Expected O, but got Unknown
		//IL_023a: Expected O, but got Unknown
		IsMoving = true;
		float num = panic.Value / panic.PanicDefinition.ValueMax;
		float num2 = 1f;
		Sequence val = DOTween.Sequence().SetFullId<Sequence>("MoveIndicator", (Component)(object)this);
		TweenCallback val2 = default(TweenCallback);
		for (int panicLevel = panic.PanicDefinition.PanicLevelDefinitions.Length - 1; panicLevel >= 0; panicLevel--)
		{
			float num3 = ((num * 100f >= panic.PanicDefinition.PanicLevelDefinitions[panicLevel].PanicValueNeeded) ? num : (panic.PanicDefinition.PanicLevelDefinitions[panicLevel].PanicValueNeeded / 100f));
			int currentLevel = panicLevel;
			Sequence obj = TweenSettingsExtensions.Append(val, (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.OnPlay<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => handleRect.anchorMin.x), (DOSetter<float>)delegate(float x)
			{
				//IL_0017: Unknown result type (might be due to invalid IL or missing references)
				//IL_0021: Unknown result type (might be due to invalid IL or missing references)
				//IL_0042: Unknown result type (might be due to invalid IL or missing references)
				//IL_004c: Unknown result type (might be due to invalid IL or missing references)
				handleRect.anchorMin = new Vector2(x, handleRect.anchorMin.y);
				handleRect.anchorMax = new Vector2(x, handleRect.anchorMax.y);
			}, num3, instant ? 0f : (timeToMoveFromMaxToMin * (num2 - num3))), moveEasing).SetFullId<TweenerCore<float, float, FloatOptions>>("MoveIndicatorToNextLevel", (Component)(object)this), (TweenCallback)delegate
			{
				PlayMoveSound(currentLevel);
			}), (TweenCallback)delegate
			{
				RefreshValues(panic, panicLevel == panic.PanicDefinition.PanicLevelDefinitions.Length - 1);
			}));
			TweenCallback obj2 = val2;
			if (obj2 == null)
			{
				TweenCallback val3 = delegate
				{
					FinishTween();
				};
				TweenCallback val4 = val3;
				val2 = val3;
				obj2 = val4;
			}
			TweenSettingsExtensions.OnComplete<Sequence>(obj, obj2);
			TweenSettingsExtensions.AppendInterval(val, instant ? 0f : movePauseAtEachPanicLevel);
			num2 = num3;
			if (num * 100f >= panic.PanicDefinition.PanicLevelDefinitions[panicLevel].PanicValueNeeded)
			{
				break;
			}
		}
		Tween obj3 = blinkTween;
		if (obj3 != null)
		{
			TweenExtensions.Kill(obj3, false);
		}
		rewardTextsCanvasGroup.alpha = 1f;
		blinkTween = (Tween)(object)TweenSettingsExtensions.SetLoops<TweenerCore<float, float, FloatOptions>>(DOTweenModuleUI.DOFade(rewardTextsCanvasGroup, blinkFadeTo, blinkDuration), -1, (LoopType)1).SetFullId<TweenerCore<float, float, FloatOptions>>("BlinkRewards", (Component)(object)this);
	}

	private void FinishTween()
	{
		IsMoving = false;
		Tween obj = blinkTween;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
		rewardTextsCanvasGroup.alpha = 1f;
	}

	private void PlayMoveSound(int stepIndex)
	{
		if ((Object)(object)panicStepsMoveAudioClip[stepIndex] != (Object)null)
		{
			TPSingleton<NightReportPanel>.Instance.PlayAudioClip(panicStepsMoveAudioClip[stepIndex]);
		}
	}

	private void RefreshValues(TheLastStand.Model.Panic.Panic panic, bool forceRefresh = false)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f1: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		for (int num2 = panic.PanicDefinition.PanicLevelDefinitions.Length - 1; num2 >= 0; num2--)
		{
			if (handleRect.anchorMin.x >= panic.PanicDefinition.PanicLevelDefinitions[num2].PanicValueNeeded * 0.01f)
			{
				num = num2;
				break;
			}
		}
		string text = null;
		int num3 = -1;
		if (panic.PanicDefinition.PanicLevelDefinitions[num].PanicRewardDefinition.ItemsListsPerDay != null)
		{
			foreach (KeyValuePair<int, PanicRewardDefinition.DayGenerationDatas> item in panic.PanicDefinition.PanicLevelDefinitions[num].PanicRewardDefinition.ItemsListsPerDay)
			{
				if (text == null || (item.Key > num3 && item.Key <= TPSingleton<GameManager>.Instance.Game.DayNumber))
				{
					text = item.Value.ItemsListId;
				}
			}
		}
		bool flag = false;
		int num4 = panic.PanicDefinition.PanicLevelDefinitions[num].PanicRewardDefinition.Gold.EvalToInt(panic.PanicEvalGoldContext);
		int num5 = panic.PanicDefinition.PanicLevelDefinitions[num].PanicRewardDefinition.Materials.EvalToInt(panic.PanicEvalMaterialContext);
		int num6 = ((text != null) ? 1 : 0);
		if (num4 != lastGold || forceRefresh)
		{
			flag = true;
			lastGold = num4;
			((TMP_Text)goldRewardText).text = $"+{num4} <style=Gold></style>";
			Tween obj = goldTween;
			if (obj != null)
			{
				TweenExtensions.Kill(obj, false);
			}
			((Transform)((TMP_Text)goldRewardText).rectTransform).localScale = Vector3.one;
			goldTween = (Tween)(object)ShortcutExtensions.DOPunchScale((Transform)(object)((TMP_Text)goldRewardText).rectTransform, Vector3.one * punchScaleStrength, punchScaleDuration, 1, 0.1f).SetFullId<Tweener>("GoldPunchScale", (Component)(object)this);
		}
		if (num5 != lastMaterials || forceRefresh)
		{
			flag = true;
			lastMaterials = num5;
			((TMP_Text)materialRewardText).text = $"+{num5} <style=Materials></style>";
			Tween obj2 = materialTween;
			if (obj2 != null)
			{
				TweenExtensions.Kill(obj2, false);
			}
			((Transform)((TMP_Text)materialRewardText).rectTransform).localScale = Vector3.one;
			materialTween = (Tween)(object)ShortcutExtensions.DOPunchScale((Transform)(object)((TMP_Text)materialRewardText).rectTransform, Vector3.one * punchScaleStrength, punchScaleDuration, 1, 0.1f).SetFullId<Tweener>("MaterialsPunchScale", (Component)(object)this);
		}
		if (num6 != lastItemsCount || forceRefresh)
		{
			flag = true;
			lastItemsCount = num6;
			((TMP_Text)itemRewardText).text = Localizer.Format("NightReportPanel_NightRewardItem", new object[1] { num6 });
			Tween obj3 = itemsTween;
			if (obj3 != null)
			{
				TweenExtensions.Kill(obj3, false);
			}
			((Transform)((TMP_Text)itemRewardText).rectTransform).localScale = Vector3.one;
			itemsTween = (Tween)(object)ShortcutExtensions.DOPunchScale((Transform)(object)((TMP_Text)itemRewardText).rectTransform, Vector3.one * punchScaleStrength, punchScaleDuration, 1, 0.1f).SetFullId<Tweener>("ItemPunchScale", (Component)(object)this);
		}
		if (flag && (Object)(object)panicStepsCoinAudioClip[num] != (Object)null)
		{
			TPSingleton<NightReportPanel>.Instance.PlayAudioClip(panicStepsCoinAudioClip[num]);
		}
	}

	protected void Awake()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Combine((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
	}

	private void OnDestroy()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Remove((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
	}

	private void OnLocalize()
	{
		if (((Component)this).gameObject.activeInHierarchy && TPSingleton<GameManager>.Instance.Game.Cycle != Game.E_Cycle.Day)
		{
			RefreshValues(PanicManager.Panic);
		}
	}
}
