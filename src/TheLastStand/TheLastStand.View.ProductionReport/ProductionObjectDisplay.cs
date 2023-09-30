using System;
using System.Collections.Generic;
using Coffee.UIExtensions;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Framework;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Model;
using TheLastStand.Model.ProductionReport;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.ProductionReport;

public class ProductionObjectDisplay : MonoBehaviour, ISubmitHandler, IEventSystemHandler
{
	public static class Constants
	{
		public const string ProductionIconPathPrefix = "View/Sprites/UI/ProductionReportPanel/Production_";

		public const string ProductionNightRewardIconPath = "View/Sprites/UI/ProductionReportPanel/Production_NightItemReward";
	}

	[SerializeField]
	private Image productionIcon;

	[SerializeField]
	private CanvasGroup productionObjectCanvas;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private TextMeshProUGUI productionBuildingText;

	[SerializeField]
	private GameObject productionObjectParent;

	[SerializeField]
	private List<UIParticle> uiParticles;

	[SerializeField]
	private Selectable selectable;

	[SerializeField]
	private BetterButton button;

	[SerializeField]
	private AudioClip goldAudioClip;

	[SerializeField]
	private AudioClip materialAudioClip;

	[SerializeField]
	private AudioClip itemAudioClip;

	private Tween fadeTween;

	public ProductionObject ProductionObject { get; set; }

	public Selectable Selectable => selectable;

	public void Disable()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected O, but got Unknown
		productionObjectCanvas.interactable = false;
		productionObjectCanvas.blocksRaycasts = false;
		Tween obj = fadeTween;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
		fadeTween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTweenModuleUI.DOFade(productionObjectCanvas, 0f, 0.3f), (Ease)9), (TweenCallback)delegate
		{
			productionObjectParent.SetActive(false);
			TPSingleton<ProductionReportPanel>.Instance.CheckOnProductionObjectHide();
		});
		ToggleUIParticles(toggle: false);
	}

	public void Hide()
	{
		ToggleUIParticles(toggle: false);
	}

	public void OnProductionObjectClick()
	{
		if (ProductionObject is ProductionItems productionItems && productionItems.Items.Count > 0)
		{
			TPSingleton<ChooseRewardPanel>.Instance.ProductionItem = productionItems;
			TPSingleton<ChooseRewardPanel>.Instance.Open();
			TPSingleton<UIManager>.Instance.PlayAudioClip(itemAudioClip);
		}
	}

	public void Display()
	{
		Tween obj = fadeTween;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
		productionObjectCanvas.interactable = true;
		productionObjectCanvas.blocksRaycasts = true;
		fadeTween = (Tween)(object)DOTweenModuleUI.DOFade(productionObjectCanvas, 1f, 0f);
		Sprite val = null;
		if (ProductionObject.ProductionBuildingDefinition != null)
		{
			val = ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/ProductionReportPanel/Production_" + ProductionObject.ProductionBuildingDefinition.Id, false);
		}
		else if (ProductionObject is ProductionItems productionItems && productionItems.IsNightProduction)
		{
			val = ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/ProductionReportPanel/Production_NightItemReward", false);
		}
		if ((Object)(object)val != (Object)null)
		{
			productionIcon.sprite = val;
		}
		ToggleUIParticles(toggle: true);
		RefreshText();
	}

	public void RefreshText()
	{
		((TMP_Text)productionBuildingText).text = Localizer.Get((ProductionObject.ProductionBuildingDefinition != null) ? ("BuildingName_" + ProductionObject.ProductionBuildingDefinition.Id) : "NightReportPanel_NightRewardObject");
		((TMP_Text)titleText).text = Localizer.Get("ProductionObject_ItemProduction");
	}

	public void OnHover(bool display)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		if (display)
		{
			if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.ProductionReport)
			{
				TPSingleton<ProductionReportPanel>.Instance.AdjustScrollView((RectTransform)((Component)this).transform);
			}
			((Selectable)button).OnPointerEnter((PointerEventData)null);
		}
		else
		{
			((Selectable)button).OnPointerExit((PointerEventData)null);
		}
	}

	public void OnSubmit(BaseEventData eventData)
	{
		OnProductionObjectClick();
	}

	protected void Awake()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Combine((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
	}

	private void OnDestroy()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Remove((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
	}

	private void OnLocalize()
	{
		if (((Component)this).gameObject.activeInHierarchy)
		{
			RefreshText();
		}
	}

	private void ToggleUIParticles(bool toggle)
	{
		foreach (UIParticle uiParticle in uiParticles)
		{
			((Behaviour)uiParticle).enabled = toggle;
		}
	}
}
