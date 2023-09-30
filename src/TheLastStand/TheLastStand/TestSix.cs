using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand;

public class TestSix : MonoBehaviour
{
	public RectTransform globalRect;

	public Ease toShopEase = (Ease)1;

	public Ease toHubEase = (Ease)1;

	private Tween tween;

	private void Transition(Vector2 pivot, Ease ease)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		RectTransformExtensions.SetAnchors(globalRect, pivot);
		RectTransformExtensions.SetPivot(globalRect, pivot);
		tween = (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOAnchorPosX(globalRect, 0f, 1f, false), ease);
	}

	private void Update()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		if (Input.GetKeyDown((KeyCode)275))
		{
			Transition(new Vector2(1f, 0.5f), toShopEase);
		}
		else if (Input.GetKeyDown((KeyCode)276))
		{
			Transition(new Vector2(0f, 0.5f), toShopEase);
		}
		else if (Input.GetKeyDown((KeyCode)273))
		{
			Transition(new Vector2(0.5f, 0.5f), toHubEase);
		}
	}
}
