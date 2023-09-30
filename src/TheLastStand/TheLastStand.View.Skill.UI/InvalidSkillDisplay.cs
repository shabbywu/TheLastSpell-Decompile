using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using TPLib.Localization;
using TPLib.Yield;
using TheLastStand.Manager.Skill;
using TheLastStand.View.Skill.SkillAction.UI;
using UnityEngine;

namespace TheLastStand.View.Skill.UI;

public class InvalidSkillDisplay : AppearingEffectDisplay
{
	public new static class Constants
	{
		public const string DisplayPrefabResourcePath = "Prefab/Displayable Effect/UI Effect Displays/InvalidSkillDisplay";
	}

	[SerializeField]
	private TextMeshProUGUI invalidityCauseText;

	private float initY;

	public void Init(SkillManager.E_InvalidSkillCause cause)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		((MonoBehaviour)this).StopAllCoroutines();
		translateTarget.localPosition = new Vector3(translateTarget.localPosition.x, initY);
		((TMP_Text)invalidityCauseText).text = Localizer.Get("InvalidSkillCause_" + cause);
	}

	protected override IEnumerator DisplayCoroutine()
	{
		if (translateDuration < 0f)
		{
			translateDuration = displayDuration;
		}
		if (fadeInDuration < 0f)
		{
			fadeInDuration = displayDuration;
		}
		if (fadeInDuration > 0f && (Object)(object)fadeTarget != (Object)null)
		{
			fadeTarget.alpha = 0f;
			DOTweenModuleUI.DOFade(fadeTarget, 1f, fadeInDuration);
		}
		if (translateDuration > 0f && (Object)(object)translateTarget != (Object)null)
		{
			initY = translateTarget.position.y;
			TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(TweenSettingsExtensions.From<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOLocalMoveY(translateTarget, 0f - translateOffsetY, translateDuration, false), true), translateEasing);
		}
		_ = DisplayDuration;
		if (displayDuration > 0f)
		{
			yield return SharedYields.WaitForSeconds(displayDuration);
		}
		if (fadeOutDuration > 0f && (Object)(object)fadeTarget != (Object)null)
		{
			yield return TweenExtensions.WaitForCompletion((Tween)(object)DOTweenModuleUI.DOFade(fadeTarget, 0f, fadeOutDuration));
		}
		translateTarget.localPosition = new Vector3(translateTarget.localPosition.x, initY);
	}

	protected override IEnumerator DisplayAndDestroyCoroutine()
	{
		yield return ((MonoBehaviour)this).StartCoroutine(DisplayCoroutine());
	}
}
