using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Log;
using TheLastStand.Manager.Meta;
using UnityEngine;

namespace TheLastStand.View.Meta;

public class MetaWorldEvolvingView : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private TextMeshProUGUI worldEvolvingText;

	[SerializeField]
	private float fadeInDuration = 0.8f;

	[SerializeField]
	private float fadeOutDuration = 1.2f;

	[SerializeField]
	private float fullAlphaDuration = 2f;

	[SerializeField]
	private bool forceDisplay;

	private Sequence displaySequence;

	public void DisplayIfNeeded(Action callback)
	{
		if (!forceDisplay && !MetaUpgradesManager.HiddenUpgradeLastlyActivated)
		{
			callback?.Invoke();
			return;
		}
		if (forceDisplay && !MetaUpgradesManager.HiddenUpgradeLastlyActivated)
		{
			((CLogger<MetaUpgradesManager>)TPSingleton<MetaUpgradesManager>.Instance).Log((object)"Forcing display of world evolving screen even though no hidden upgrade has been activated.", (CLogLevel)0, false, false);
		}
		((MonoBehaviour)this).StartCoroutine(Display(callback));
	}

	public IEnumerator Display(Action callback)
	{
		MetaUpgradesManager.HiddenUpgradeLastlyActivated = false;
		displaySequence = DOTween.Sequence();
		TweenSettingsExtensions.Append(displaySequence, (Tween)(object)DOTweenModuleUI.DOFade(canvasGroup, 1f, fadeInDuration));
		TweenSettingsExtensions.AppendInterval(displaySequence, fullAlphaDuration);
		TweenSettingsExtensions.Append(displaySequence, (Tween)(object)DOTweenModuleUI.DOFade(canvasGroup, 0f, fadeOutDuration));
		yield return TweenExtensions.WaitForKill((Tween)(object)displaySequence);
		callback?.Invoke();
	}

	private void Awake()
	{
		canvasGroup.alpha = 0f;
	}

	private void OnLocalize()
	{
		if (((Component)this).gameObject.activeInHierarchy)
		{
			((TMP_Text)worldEvolvingText).text = Localizer.Get("Meta_WorldEvolving");
		}
	}

	private void SetDisplayAlpha(float alpha)
	{
		canvasGroup.alpha = alpha;
	}
}
