using System;
using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Localization.Fonts;
using TPLib.Log;
using TPLib.UI;
using TheLastStand.Controller;
using TheLastStand.Manager;
using TheLastStand.Manager.Meta;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View;

public class RetryTutorialPanel : TPSingleton<RetryTutorialPanel>, IOverlayUser
{
	private static class Constants
	{
		public const string TitleTextKey = "TutorialGameOver_Title";

		public const string ContentTextKey = "TutorialGameOver_Content";

		public const string RetryButtonTextKey = "TutorialGameOver_Retry";

		public const string SkipButtonTextKey = "TutorialGameOver_Skip";
	}

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private TextMeshProUGUI contentText;

	[SerializeField]
	private TextMeshProUGUI retryButtonText;

	[SerializeField]
	private TextMeshProUGUI skipButtonText;

	[SerializeField]
	private Button retryButton;

	[SerializeField]
	private Button skipButton;

	[SerializeField]
	private ComplexFontLocalizedParent complexFontLocalizedParent;

	public Canvas Canvas => canvas;

	public int OverlaySortingOrder => TPSingleton<RetryTutorialPanel>.Instance.canvas.sortingOrder - 2;

	public void Open()
	{
		CLoggerManager.Log((object)"Opening RetryTutorialPanel.", (Object)(object)this, (LogType)3, (CLogLevel)0, true, "StaticLog", false);
		ComplexFontLocalizedParent obj = complexFontLocalizedParent;
		if (obj != null)
		{
			((FontLocalizedParent)obj).RefreshChilds();
		}
		RefreshLocalizedText();
		((Behaviour)Canvas).enabled = true;
		canvasGroup.blocksRaycasts = true;
	}

	protected override void Awake()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected O, but got Unknown
		base.Awake();
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Combine((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
		((UnityEvent)retryButton.onClick).AddListener(new UnityAction(OnRetryButtonClicked));
		((UnityEvent)skipButton.onClick).AddListener(new UnityAction(OnSkipButtonClicked));
		((Behaviour)Canvas).enabled = false;
	}

	private void OnRetryButtonClicked()
	{
		((Behaviour)canvas).enabled = false;
		GameController.RestartLevel();
	}

	private void OnSkipButtonClicked()
	{
		((Behaviour)canvas).enabled = false;
		ApplicationManager.Application.TutorialDone = true;
		TPSingleton<MetaConditionManager>.Instance.RefreshProgression();
		GameController.GoToMetaShops();
	}

	private void OnDestroy()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected O, but got Unknown
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Remove((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
		((UnityEvent)retryButton.onClick).RemoveListener(new UnityAction(OnRetryButtonClicked));
		((UnityEvent)skipButton.onClick).RemoveListener(new UnityAction(OnSkipButtonClicked));
	}

	private void OnLocalize()
	{
		if (((Component)this).gameObject.activeInHierarchy)
		{
			RefreshLocalizedText();
		}
	}

	private void RefreshLocalizedText()
	{
		((TMP_Text)titleText).text = Localizer.Get("TutorialGameOver_Title");
		((TMP_Text)contentText).text = Localizer.Get("TutorialGameOver_Content");
		((TMP_Text)retryButtonText).text = Localizer.Get("TutorialGameOver_Retry");
		((TMP_Text)skipButtonText).text = Localizer.Get("TutorialGameOver_Skip");
	}

	private void Update()
	{
		if (((Behaviour)Canvas).enabled)
		{
			if (InputManager.GetButtonDown(80))
			{
				OnSkipButtonClicked();
			}
			else if (InputManager.GetButtonDown(7) || InputManager.GetButtonDown(66))
			{
				OnRetryButtonClicked();
			}
		}
	}
}
