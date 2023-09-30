using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using PortraitAPI.Misc;
using TMPro;
using TPLib.Yield;
using TheLastStand.Framework.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.PlayableUnitCustomisation;

public class PortraitCodePanel : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI codeText;

	[SerializeField]
	private BetterButton editCodeButton;

	[SerializeField]
	private BetterButton copyCodeButton;

	[SerializeField]
	private PortraitCodePopup portraitCodePopup;

	[SerializeField]
	private CanvasGroup copyFeedbackCanvasGroup;

	public bool IsEditingCode;

	private Tween copyFeedbackApparitionTween;

	private Tween copyFeedbackDisappearTween;

	public CodeData CurrentCode { get; private set; }

	public PortraitCodePopup PortraitCodePopup => portraitCodePopup;

	public void Refresh(CodeData codeData)
	{
		CurrentCode = codeData;
		((TMP_Text)codeText).text = ((object)CurrentCode).ToString();
	}

	public void Refresh(string code)
	{
		CodeData currentCode = default(CodeData);
		if (CodeGenerator.TryDecode(code, ref currentCode))
		{
			CurrentCode = currentCode;
			((TMP_Text)codeText).text = ((object)CurrentCode).ToString();
		}
	}

	private void OnCopyCodeButtonClicked()
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		GUIUtility.systemCopyBuffer = ((TMP_Text)codeText).text;
		Tween obj = copyFeedbackApparitionTween;
		if (obj != null && TweenExtensions.IsPlaying(obj))
		{
			TweenExtensions.Kill(copyFeedbackApparitionTween, false);
		}
		Tween obj2 = copyFeedbackDisappearTween;
		if (obj2 != null && TweenExtensions.IsPlaying(obj2))
		{
			TweenExtensions.Kill(copyFeedbackDisappearTween, false);
		}
		copyFeedbackApparitionTween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(DOTweenModuleUI.DOFade(copyFeedbackCanvasGroup, 1f, 0.25f), (TweenCallback)delegate
		{
			((MonoBehaviour)this).StartCoroutine(WaitThenPlayDisappear());
		});
	}

	private void OnEditCodeButtonClicked()
	{
		IsEditingCode = true;
		portraitCodePopup.Open();
		portraitCodePopup.Refresh(((TMP_Text)codeText).text);
	}

	private void Start()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		((UnityEvent)((Button)editCodeButton).onClick).AddListener(new UnityAction(OnEditCodeButtonClicked));
		((UnityEvent)((Button)copyCodeButton).onClick).AddListener(new UnityAction(OnCopyCodeButtonClicked));
	}

	private void OnDestroy()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		((UnityEvent)((Button)editCodeButton).onClick).RemoveListener(new UnityAction(OnEditCodeButtonClicked));
		((UnityEvent)((Button)copyCodeButton).onClick).RemoveListener(new UnityAction(OnCopyCodeButtonClicked));
	}

	private IEnumerator WaitThenPlayDisappear()
	{
		yield return SharedYields.WaitForSeconds(1f);
		copyFeedbackDisappearTween = (Tween)(object)DOTweenModuleUI.DOFade(copyFeedbackCanvasGroup, 0f, 0.25f);
	}
}
