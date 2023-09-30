using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace TheLastStand.View.Skill.SkillAction.UI;

public class WavyEffectDisplay : AppearingEffectDisplay
{
	[SerializeField]
	private Vector2 spawnPosOffsetXRange = new Vector2(-10f, 10f);

	[SerializeField]
	protected TextMeshProUGUI valueLbl;

	[SerializeField]
	private float translateXOffsetValue = 2f;

	[SerializeField]
	private AnimationCurve translateXEasingCurve;

	[SerializeField]
	private Vector2 translateYOffsetRange = new Vector2(20f, 35f);

	private float internalTranslateXOffset;

	private float originalAlpha = -1f;

	public override void Init(int value)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		base.Init(value);
		if (originalAlpha != -1f)
		{
			((TMP_Text)valueLbl).alpha = originalAlpha;
		}
		else
		{
			originalAlpha = ((TMP_Text)valueLbl).alpha;
		}
		Transform transform = ((Component)this).transform;
		transform.localPosition += new Vector3((float)TPHelpers.RandomIntInRange(spawnPosOffsetXRange, false), 0f, 0f);
		internalTranslateXOffset = translateXOffsetValue * (float)((!TPHelpers.RandomBool()) ? 1 : (-1));
		internalTranslateOffsetY += TPHelpers.RandomIntInRange(translateYOffsetRange, false);
	}

	protected override IEnumerator DisplayCoroutine()
	{
		if ((Object)(object)translateTarget != (Object)null)
		{
			TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOBlendableLocalMoveBy(translateTarget, new Vector3(internalTranslateXOffset, 0f), translateDuration, false), translateXEasingCurve);
		}
		yield return base.DisplayCoroutine();
	}
}
