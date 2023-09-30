using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace TheLastStand.View.Building.Construction;

public class GamepadInputFeedback : MonoBehaviour
{
	[SerializeField]
	[Range(0f, 10f)]
	private float targetStretchOrShrinkScale = 1.2f;

	[SerializeField]
	[Range(0.01f, 10f)]
	private float tweenDuration = 0.5f;

	protected virtual void Start()
	{
		TweenSettingsExtensions.SetLoops<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale(((Component)this).transform, targetStretchOrShrinkScale, tweenDuration), -1, (LoopType)1);
	}
}
