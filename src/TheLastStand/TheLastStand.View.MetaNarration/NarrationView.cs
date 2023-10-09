using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using RedBlueGames.Tools.TextTyper;
using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Localization.Fonts;
using TPLib.Yield;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.Sound;
using TheLastStand.Model.Meta;
using TheLastStand.View.MetaShops;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.MetaNarration;

public class NarrationView : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup replicasGroup;

	[SerializeField]
	private NarrationReplicaView[] replicaViews;

	[SerializeField]
	private RectTransform greetingPanelParent;

	[SerializeField]
	private RectTransform greetingPanelScaler;

	[SerializeField]
	private CanvasGroup greetingPanelGroup;

	[SerializeField]
	private TextTyper greetingTextTyper;

	[SerializeField]
	private TextMeshProUGUI nameText;

	[SerializeField]
	private RectTransform contentRoot;

	[SerializeField]
	private float greetingPanelInDuration = 0.3f;

	[SerializeField]
	private float greetingPanelOutDuration = 0.15f;

	[SerializeField]
	private Ease greetingPanelInEase = (Ease)4;

	[SerializeField]
	private Ease greetingPanelOutEase = (Ease)9;

	[SerializeField]
	private float greetingPanelOffset = -50f;

	[SerializeField]
	private float greetingPanelOffsetDuration = 0.3f;

	[SerializeField]
	private Ease greetingPanelOffsetEase = (Ease)21;

	[SerializeField]
	private AudioClip onCharacterPrintedSound;

	[SerializeField]
	private float greetingPanelSmokeOffset = 235f;

	[SerializeField]
	private SimpleFontLocalizedParent fontLocalizedParent;

	private int answersCounter;

	private int answersToDisplayCount;

	private bool dialogueOver;

	private bool isTypingText;

	private bool isWaitingForSkipInput;

	private NarrationReplicaView selectedReplica;

	public TheLastStand.Model.Meta.MetaNarration MetaNarration { get; set; }

	public bool HasNarrationToPlay
	{
		get
		{
			if (!MetaNarration.MetaNarrationController.TryGetValidMandatoryReplica(1, out var replicas))
			{
				if (!MetaNarrationsManager.NarrationDoneThisDay)
				{
					return MetaNarration.MetaNarrationController.TryGetValidReplicas(1, out replicas);
				}
				return false;
			}
			return true;
		}
	}

	public void DisplayShopGreeting(string greetingId)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		if (greetingPanelGroup.alpha == 0f)
		{
			FadeInGreetingPanel();
		}
		greetingTextTyper.TypeText(Localizer.Get(MetaNarration.LocalizationGreetingPrefix + greetingId), -1f);
		greetingPanelScaler.anchoredPosition = new Vector2(0f, greetingPanelScaler.anchoredPosition.y);
	}

	private void FadeInGreetingPanel()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		((Transform)greetingPanelScaler).localScale = Vector3.one * ((Screen.height > 768) ? 1f : (2f / 3f));
		RefreshGreetingPanelSize();
		if (!(greetingPanelGroup.alpha >= 1f))
		{
			RefreshDisplayedName();
			greetingTextTyper.TypeText(" ", -1f);
			greetingPanelGroup.alpha = 0f;
			TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTweenModuleUI.DOFade(greetingPanelGroup, 1f, greetingPanelInDuration), greetingPanelInEase);
		}
	}

	private void RefreshGreetingPanelSize()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		Vector2 sizeDelta = greetingPanelScaler.sizeDelta;
		Rect rect = greetingPanelParent.rect;
		float num = ((Rect)(ref rect)).width / ((Transform)greetingPanelScaler).localScale.x + greetingPanelSmokeOffset * 2f;
		rect = greetingPanelScaler.rect;
		float num2 = ((Rect)(ref rect)).width - sizeDelta.x;
		((Vector2)(ref sizeDelta))._002Ector(num - num2, sizeDelta.y);
		greetingPanelScaler.sizeDelta = sizeDelta;
	}

	public IEnumerator GreetingSequenceCoroutine(string greetingId)
	{
		greetingPanelScaler.anchoredPosition = new Vector2(greetingPanelOffset, greetingPanelScaler.anchoredPosition.y);
		FadeInGreetingPanel();
		yield return SharedYields.WaitForSeconds(greetingPanelInDuration);
		greetingTextTyper.TypeText(Localizer.Get(MetaNarration.LocalizationGreetingPrefix + greetingId), -1f);
		isTypingText = true;
		yield return (object)new WaitUntil((Func<bool>)InputManager.GetSubmitButtonDown);
		TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTween.To((DOGetter<float>)(() => greetingPanelOffset), (DOSetter<float>)delegate(float x)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			greetingPanelScaler.anchoredPosition = new Vector2(x, greetingPanelScaler.anchoredPosition.y);
		}, 0f, greetingPanelOffsetDuration), greetingPanelOffsetEase);
	}

	public void Hide()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTweenModuleUI.DOFade(greetingPanelGroup, 0f, greetingPanelOutDuration), greetingPanelOutEase);
		HideReplicas();
	}

	public IEnumerator NarrationSequenceCoroutine(List<MetaReplica> replicas)
	{
		replicasGroup.alpha = 0f;
		TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetDelay<TweenerCore<float, float, FloatOptions>>(DOTweenModuleUI.DOFade(replicasGroup, 1f, 0.3f), 0.2f), (Ease)4);
		SetReplicas(replicas);
		yield return (object)new WaitUntil((Func<bool>)(() => dialogueOver));
		dialogueOver = false;
		HideReplicas();
	}

	public void OnCharacterPrinted()
	{
		SoundManager.PlayAudioClip(onCharacterPrintedSound);
	}

	public void OnReplicaSelected(NarrationReplicaView replicaView)
	{
		if ((Object)(object)selectedReplica != (Object)null)
		{
			return;
		}
		if (TPSingleton<OraculumView>.Instance.IsInDarkShop)
		{
			TPSingleton<DarkShopManager>.Instance.MetaShopView.ExitText.SetActive(true);
		}
		else
		{
			TPSingleton<LightShopManager>.Instance.MetaShopView.ExitText.SetActive(true);
		}
		selectedReplica = replicaView;
		MetaNarration.MetaNarrationController.MarkReplicaAsUsed(replicaView.Replica);
		FadeInGreetingPanel();
		for (int i = 0; i < replicaViews.Length; i++)
		{
			if ((Object)(object)replicaViews[i] != (Object)(object)replicaView)
			{
				replicaViews[i].HideWithoutDisabling();
			}
		}
		AllowReplicasInteraction(allowed: false);
		answersToDisplayCount = replicaView.Replica.MetaReplicaDefinition.AnswersCount;
		answersCounter = 0;
		DisplayNextAnswer();
		if (InputManager.IsLastControllerJoystick)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: false);
			EventSystem.current.SetSelectedGameObject((GameObject)null);
		}
	}

	public void RefreshDisplayedName()
	{
		((TMP_Text)nameText).text = (MetaNarration.MetaNarrationController.CanDisplayGoddessName() ? MetaNarration.GoddessName : Localizer.Get("MetaShops_UnknownGoddessName"));
	}

	public void RefreshLocalizedFonts()
	{
		SimpleFontLocalizedParent obj = fontLocalizedParent;
		if (obj != null)
		{
			((FontLocalizedParent)obj).RefreshChilds();
		}
	}

	private void AllowReplicasInteraction(bool allowed)
	{
		for (int i = 0; i < replicaViews.Length; i++)
		{
			replicaViews[i].AllowInteraction(allowed);
		}
	}

	private void Awake()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		for (int i = 0; i < replicaViews.Length; i++)
		{
			replicaViews[i].NarrationView = this;
		}
		greetingTextTyper.PrintCompleted.AddListener(new UnityAction(OnGreetingOrAnswerTypeCompleted));
	}

	private void DisplayNextAnswer()
	{
		greetingTextTyper.TypeText(Localizer.Get(string.Format(selectedReplica.Replica.LocalizationAnswerFormat, selectedReplica.Replica.Id, answersCounter)), -1f);
		isTypingText = true;
		answersCounter++;
	}

	private void HideReplicas()
	{
		for (int i = 0; i < replicaViews.Length; i++)
		{
			replicaViews[i].Display(show: false);
		}
	}

	private void OnGreetingOrAnswerTypeCompleted()
	{
		isTypingText = false;
		if ((Object)(object)selectedReplica != (Object)null)
		{
			isWaitingForSkipInput = true;
		}
	}

	private void OnNextButtonClicked()
	{
		if (answersCounter == answersToDisplayCount)
		{
			dialogueOver = true;
			selectedReplica = null;
			HideReplicas();
		}
		else
		{
			DisplayNextAnswer();
		}
	}

	private void SetReplicas(List<MetaReplica> replicas)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		int i;
		for (i = 0; i < replicas.Count; i++)
		{
			NarrationReplicaView narrationReplicaView = replicaViews[i];
			((UnityEventBase)((Button)narrationReplicaView.Button).onClick).RemoveAllListeners();
			((UnityEvent)((Button)narrationReplicaView.Button).onClick).AddListener(new UnityAction(narrationReplicaView.OnClick));
			narrationReplicaView.SetReplica(replicas[i]);
		}
		for (; i < replicaViews.Length; i++)
		{
			replicaViews[i].Display(show: false);
		}
		AllowReplicasInteraction(allowed: true);
		LayoutRebuilder.ForceRebuildLayoutImmediate(contentRoot);
		RefreshJoystickNavigation();
		((MonoBehaviour)this).StartCoroutine(SelectFirstReplicaJoystickEndOfFrame());
	}

	private void RefreshJoystickNavigation()
	{
		for (int i = 0; i < replicaViews.Length; i++)
		{
			((Selectable)(object)replicaViews[i].Button).SetMode((Mode)4);
			((Selectable)(object)replicaViews[i].Button).ClearNavigation();
			if (((Component)replicaViews[i]).gameObject.activeSelf)
			{
				if (i > 0)
				{
					((Selectable)(object)replicaViews[i].Button).SetSelectOnUp((Selectable)(object)replicaViews[i - 1].Button);
				}
				if (i < replicaViews.Length - 1 && ((Component)replicaViews[i + 1]).gameObject.activeSelf)
				{
					((Selectable)(object)replicaViews[i].Button).SetSelectOnDown((Selectable)(object)replicaViews[i + 1].Button);
				}
				continue;
			}
			break;
		}
	}

	private IEnumerator SelectFirstReplicaJoystickEndOfFrame()
	{
		yield return SharedYields.WaitForEndOfFrame;
		if (InputManager.IsLastControllerJoystick)
		{
			EventSystem.current.SetSelectedGameObject(((Component)replicaViews[0]).gameObject);
		}
	}

	public void SkipNextNarration()
	{
		if (isTypingText)
		{
			greetingTextTyper.Skip();
			isTypingText = false;
		}
		if (isWaitingForSkipInput || (Object)(object)selectedReplica != (Object)null)
		{
			isWaitingForSkipInput = false;
			dialogueOver = true;
			selectedReplica = null;
			HideReplicas();
		}
	}

	private void Update()
	{
		if ((!isTypingText && !isWaitingForSkipInput) || !InputManager.GetSubmitButtonDown())
		{
			return;
		}
		if (isTypingText)
		{
			greetingTextTyper.Skip();
			isTypingText = false;
			if ((Object)(object)selectedReplica != (Object)null)
			{
				isWaitingForSkipInput = true;
			}
		}
		else if (isWaitingForSkipInput)
		{
			OnNextButtonClicked();
			isWaitingForSkipInput = false;
		}
		else if ((Object)(object)selectedReplica != (Object)null)
		{
			selectedReplica.OnClick();
		}
	}
}
