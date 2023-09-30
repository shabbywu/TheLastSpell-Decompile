using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using TPLib;
using TheLastStand.Manager;
using TheLastStand.Model;
using TheLastStand.View.Skill.SkillAction.UI;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.HUD;

public class ResourceTextDisplay : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI text;

	[SerializeField]
	private Ease textEasing = (Ease)1;

	[SerializeField]
	private float textTweenDuration = 0.5f;

	[SerializeField]
	private float pulseScale = 1.2f;

	[SerializeField]
	private float pulseDuration = 0.5f;

	[SerializeField]
	private int periodCount = 2;

	private int previousValue;

	private int textValue;

	private Tween textTween;

	private Tween pulseTween;

	public void RefreshValue<T>(int value, Func<T> effectDisplayGetter) where T : AppearingEffectDisplay
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		if (previousValue != value)
		{
			Tween obj = textTween;
			if (obj != null)
			{
				TweenExtensions.Kill(obj, false);
			}
			textTween = (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<int, int, NoOptions>>(DOTween.To((DOGetter<int>)(() => textValue), (DOSetter<int>)delegate(int x)
			{
				textValue = x;
				((TMP_Text)text).text = x.ToString();
			}, value, textTweenDuration), textEasing);
			Tween obj2 = pulseTween;
			if (obj2 != null)
			{
				TweenExtensions.Kill(obj2, true);
			}
			pulseTween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale(((TMP_Text)text).transform, pulseScale, pulseDuration), (Ease)35, (float)(periodCount * 2)), (TweenCallback)delegate
			{
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				((TMP_Text)text).transform.localScale = Vector3.one;
			});
			if (TPSingleton<GameManager>.Instance.Game.State != Game.E_State.NightReport)
			{
				T val = effectDisplayGetter();
				val.FollowElement.ChangeTarget(((Component)this).transform);
				val.Init(value - previousValue);
				val.Display();
			}
			previousValue = value;
		}
	}

	public void SetColor(Color color)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)text).color = color;
	}

	private void Awake()
	{
		((TMP_Text)text).text = previousValue.ToString();
	}
}
