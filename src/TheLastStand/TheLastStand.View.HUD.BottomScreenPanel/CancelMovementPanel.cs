using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TheLastStand.Framework.UI;
using TheLastStand.Manager.Unit;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.HUD.BottomScreenPanel;

public class CancelMovementPanel : MonoBehaviour
{
	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private RectTransform rectTransform;

	[SerializeField]
	private float rectTransformClosedX = -73f;

	[SerializeField]
	private float rectTransformOpenedX = -14f;

	[SerializeField]
	[Range(0f, 1f)]
	private float tweenDuration = 0.3f;

	[SerializeField]
	private BetterButton cancelMovementButton;

	private Tween openTween;

	private Tween closeTween;

	public void Refresh()
	{
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		if (PlayableUnitManager.CanUndoLastCommand())
		{
			if (((Selectable)cancelMovementButton).interactable)
			{
				return;
			}
			closeTween = null;
			((Selectable)cancelMovementButton).interactable = true;
			((Behaviour)canvas).enabled = true;
			openTween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<Vector2, Vector2, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOAnchorPosX(rectTransform, rectTransformOpenedX, tweenDuration, true), (Ease)27), (TweenCallback)delegate
			{
				if (((Selectable)cancelMovementButton).interactable)
				{
					((Behaviour)canvas).enabled = true;
				}
				openTween = null;
			});
			TweenExtensions.Play<Tween>(openTween);
		}
		else
		{
			if (!((Selectable)cancelMovementButton).interactable)
			{
				return;
			}
			openTween = null;
			((Selectable)cancelMovementButton).interactable = false;
			closeTween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<Vector2, Vector2, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOAnchorPosX(rectTransform, rectTransformClosedX, tweenDuration, true), (Ease)26), (TweenCallback)delegate
			{
				if (!((Selectable)cancelMovementButton).interactable)
				{
					((Behaviour)canvas).enabled = false;
				}
				closeTween = null;
			});
			TweenExtensions.Play<Tween>(closeTween);
		}
	}
}
