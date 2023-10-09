using System;
using DG.Tweening;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.View.Camera;
using UnityEngine;

namespace TheLastStand.View.Cursor;

public class JoystickCursorView : MonoBehaviour
{
	[SerializeField]
	private SpriteRenderer spriteRenderer;

	private Tween hideTween;

	private Tween scaleTween;

	public bool Enabled => ((Component)this).gameObject.activeSelf;

	public void Enable(bool isOn)
	{
		if (isOn != Enabled)
		{
			((Component)this).gameObject.SetActive(isOn);
		}
	}

	public void Show(bool show)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		if ((show && spriteRenderer.color.a >= 1f) || (!show && spriteRenderer.color.a == 0f))
		{
			return;
		}
		if (show)
		{
			Tween obj = hideTween;
			if (obj != null)
			{
				TweenExtensions.Kill(obj, false);
			}
			hideTween = null;
			spriteRenderer.color = spriteRenderer.color.WithA(1f);
		}
		else if (hideTween == null && spriteRenderer.color.a > 0f)
		{
			hideTween = (Tween)(object)DOTweenModuleSprite.DOFade(spriteRenderer, 0f, InputManager.JoystickConfig.Cursor.HideDuration);
		}
	}

	public void SetPosition(Vector3 position)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.position = position;
	}

	private void Awake()
	{
		ACameraView.OnZoomHasChanged = (ACameraView.DelZoom)Delegate.Combine(ACameraView.OnZoomHasChanged, new ACameraView.DelZoom(OnZoomHasChanged));
	}

	private void OnDestroy()
	{
		ACameraView.OnZoomHasChanged = (ACameraView.DelZoom)Delegate.Remove(ACameraView.OnZoomHasChanged, new ACameraView.DelZoom(OnZoomHasChanged));
	}

	private void OnZoomHasChanged(bool zoomed)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		Tween obj = scaleTween;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
		scaleTween = (Tween)(object)ShortcutExtensions.DOScale(((Component)spriteRenderer).transform, Vector3.one * (zoomed ? 0.5f : 1f), InputManager.JoystickConfig.Cursor.ScaleDuration);
	}
}
