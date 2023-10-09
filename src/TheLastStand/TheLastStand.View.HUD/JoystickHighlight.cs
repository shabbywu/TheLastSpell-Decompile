using System;
using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.HUD;

public class JoystickHighlight : MonoBehaviour
{
	[SerializeField]
	private Animator animator;

	[SerializeField]
	private Image image;

	[SerializeField]
	private GamepadButtonsDisplayLayout gamepadButtonsDisplayLayout;

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private Vector2 sizeDeltaIncrement = Vector2.zero;

	[SerializeField]
	private Vector2 minSize = new Vector2(1000f, 1000f);

	private bool alwaysFollow;

	private Tween highlightPositionTween;

	private Tween highlightSizeTween;

	private JoystickHighlighter currentHighlighter;

	public bool IsMoving => TweenExtensions.IsPlaying(highlightPositionTween);

	public RectTransform RectTransform { get; private set; }

	public Vector2 SizeDeltaIncrement => sizeDeltaIncrement;

	public bool AlwaysFollow
	{
		get
		{
			return alwaysFollow;
		}
		private set
		{
			if (alwaysFollow != value)
			{
				alwaysFollow = value;
				if (alwaysFollow)
				{
					((MonoBehaviour)this).StartCoroutine(FollowHighlighterCoroutine());
				}
			}
		}
	}

	public bool Displayed { get; private set; }

	public void ToggleAlwaysFollow(bool state, bool force = false)
	{
		if (force || InputManager.IsLastControllerJoystick)
		{
			AlwaysFollow = state;
		}
	}

	public void Display(bool state, bool force = false)
	{
		if (Displayed != state || force)
		{
			((Behaviour)animator).enabled = state;
			((Behaviour)image).enabled = state;
			if (state)
			{
				gamepadButtonsDisplayLayout.Show();
			}
			else
			{
				gamepadButtonsDisplayLayout.Hide();
			}
			Displayed = state;
		}
	}

	public void UpdateGamepadInputDisplays()
	{
		gamepadButtonsDisplayLayout.ToggleLayoutGroup(state: true);
		gamepadButtonsDisplayLayout.RefreshGamepadInputDisplays(currentHighlighter.GamepadButtonTypes);
	}

	public void FollowHighlighter(JoystickHighlighter highlighter)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Expected O, but got Unknown
		currentHighlighter = highlighter;
		UpdateGamepadInputDisplays();
		Tween obj = highlightPositionTween;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
		Tween obj2 = highlightSizeTween;
		if (obj2 != null)
		{
			TweenExtensions.Kill(obj2, false);
		}
		Rect val = currentHighlighter.RectTransform.GetWorldRect();
		Vector3 targetPosition = Vector2.op_Implicit(((Rect)(ref val)).center);
		GamepadButtonsDisplayLayout obj3 = gamepadButtonsDisplayLayout;
		val = currentHighlighter.RectTransform.GetWorldRect();
		obj3.TargetPosition = Vector2.op_Implicit(((Rect)(ref val)).min);
		val = currentHighlighter.RectTransform.rect;
		Vector3 val2 = Vector2.op_Implicit(((Rect)(ref val)).size + SizeDeltaIncrement);
		val2.x = Mathf.Max(val2.x, minSize.x);
		val2.y = Mathf.Max(val2.y, minSize.y);
		Func<Vector3> func = delegate
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			if (!AlwaysFollow)
			{
				return targetPosition;
			}
			Rect worldRect = currentHighlighter.RectTransform.GetWorldRect();
			return Vector2.op_Implicit(((Rect)(ref worldRect)).center);
		};
		highlightPositionTween = (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOMove(((Component)this).transform, func(), InputManager.JoystickConfig.HUDNavigation.HighlightTweenDuration, false), InputManager.JoystickConfig.HUDNavigation.HighlightTweenEase);
		highlightSizeTween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<Vector2, Vector2, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOSizeDelta(RectTransform, Vector2.op_Implicit(val2), InputManager.JoystickConfig.HUDNavigation.HighlightTweenDuration, false), InputManager.JoystickConfig.HUDNavigation.HighlightTweenEase), (TweenCallback)delegate
		{
			gamepadButtonsDisplayLayout.ToggleLayoutGroup(state: false);
		});
		Display(state: true);
	}

	public void ForcePositionUpdate()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)currentHighlighter != (Object)null)
		{
			Transform transform = ((Component)this).transform;
			Rect worldRect = currentHighlighter.RectTransform.GetWorldRect();
			transform.position = Vector2.op_Implicit(((Rect)(ref worldRect)).center);
		}
	}

	private void Awake()
	{
		gamepadButtonsDisplayLayout.Init();
		Transform transform = ((Component)this).transform;
		RectTransform = (RectTransform)(object)((transform is RectTransform) ? transform : null);
		Display(state: false, force: true);
	}

	private IEnumerator Start()
	{
		Canvas obj = canvas;
		int sortingOrder = obj.sortingOrder;
		obj.sortingOrder = sortingOrder + 1;
		yield return null;
		Canvas obj2 = canvas;
		sortingOrder = obj2.sortingOrder;
		obj2.sortingOrder = sortingOrder - 1;
	}

	private IEnumerator FollowHighlighterCoroutine()
	{
		while (AlwaysFollow)
		{
			yield return null;
			ForcePositionUpdate();
		}
	}

	private void DebugToggleAlwaysFollow()
	{
		ToggleAlwaysFollow(state: true, force: true);
	}

	private void DebugRemoveAlwaysFollow()
	{
		ToggleAlwaysFollow(state: false, force: true);
	}
}
