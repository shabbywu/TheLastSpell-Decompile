using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Localization.Fonts;
using TPLib.Log;
using TPLib.UI;
using TheLastStand.Controller;
using TheLastStand.Controller.ProductionReport;
using TheLastStand.Database.Building;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Sound;
using TheLastStand.Model;
using TheLastStand.Model.ProductionReport;
using TheLastStand.View.Camera;
using TheLastStand.View.HUD;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.ProductionReport;

public class ProductionReportPanel : TPSingleton<ProductionReportPanel>, IOverlayUser
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static Func<ProductionObjectDisplay, bool> _003C_003E9__35_0;

		public static Func<ProductionObjectDisplay, bool> _003C_003E9__37_0;

		public static TweenCallback _003C_003E9__38_0;

		internal bool _003CClose_003Eb__35_0(ProductionObjectDisplay o)
		{
			return ((Component)o).gameObject.activeSelf;
		}

		internal bool _003CSelectFirstProduct_003Eb__37_0(ProductionObjectDisplay x)
		{
			return ((Component)x).gameObject.activeSelf;
		}

		internal void _003COpen_003Eb__38_0()
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: false);
		}
	}

	[SerializeField]
	private float panelDeltaPosY = -20f;

	[SerializeField]
	private RectTransform productionReportMask;

	[SerializeField]
	private Scrollbar productionReportScrollbar;

	[SerializeField]
	private Button productionReportTopButton;

	[SerializeField]
	private Button productionReportBotButton;

	[SerializeField]
	[Range(0f, 1f)]
	private float scrollButtonsSensitivity = 0.1f;

	[SerializeField]
	private SimpleFontLocalizedParent simpleFontLocalizedParent;

	[SerializeField]
	private RectTransform viewPort;

	[SerializeField]
	private Scrollbar scrollBar;

	[SerializeField]
	private ProductionObjectDisplay productionObjectPrefab;

	[SerializeField]
	private RectTransform productionObjectParent;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private HUDJoystickSimpleTarget joystickTarget;

	[SerializeField]
	private LayoutNavigationInitializer layoutNavigationInitializer;

	[SerializeField]
	private AudioClip openAudioClip;

	[SerializeField]
	private bool playCloseSound = true;

	[SerializeField]
	private AudioSource closeAudioSource;

	[SerializeField]
	private AudioSource newItemsAudioSource;

	private Canvas canvas;

	private CanvasGroup canvasGroup;

	private bool isOpened;

	private Tween moveTween;

	private float posYInit;

	private List<ProductionObjectDisplay> productionObjects = new List<ProductionObjectDisplay>();

	private RectTransform rectTransform;

	private bool firstFrameOpened = true;

	private int lastClosedEnabledObjectsCount;

	public int OverlaySortingOrder => canvas.sortingOrder - 1;

	public HUDJoystickSimpleTarget JoystickTarget => joystickTarget;

	public static void RefreshScrollbar()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		LayoutRebuilder.ForceRebuildLayoutImmediate(TPSingleton<ProductionReportPanel>.Instance.productionObjectParent);
		bool interactable = TPSingleton<ProductionReportPanel>.Instance.productionObjectParent.sizeDelta.y > TPSingleton<ProductionReportPanel>.Instance.productionReportMask.sizeDelta.y;
		((Selectable)TPSingleton<ProductionReportPanel>.Instance.productionReportScrollbar).interactable = interactable;
		((Selectable)TPSingleton<ProductionReportPanel>.Instance.productionReportTopButton).interactable = interactable;
		((Selectable)TPSingleton<ProductionReportPanel>.Instance.productionReportBotButton).interactable = interactable;
	}

	public void AdjustScrollView(RectTransform focusedRect)
	{
		GUIHelpers.AdjustScrollViewToFocusedItem(focusedRect, viewPort, scrollBar, 0.01f, 0.01f, (float?)null);
	}

	public static void RefreshTitle()
	{
		((TMP_Text)TPSingleton<ProductionReportPanel>.Instance.titleText).text = Localizer.Format("ProductionReport_JournalSentence", new object[1] { TPSingleton<GameManager>.Instance.Game.DayNumber });
	}

	public void CheckOnProductionObjectHide()
	{
		RefreshScrollbar();
		if (TPSingleton<BuildingManager>.Instance.ProductionReport.ProducedObjects.Count == 0)
		{
			GameController.SetState(Game.E_State.Management);
			Close();
		}
		else if (InputManager.IsLastControllerJoystick)
		{
			layoutNavigationInitializer.InitNavigation();
			SelectFirstProduct();
		}
	}

	public void Close()
	{
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Expected O, but got Unknown
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		if (isOpened)
		{
			CLoggerManager.Log((object)"ProductionReportPanel closed", (Object)(object)this, (LogType)3, (CLogLevel)0, true, "StaticLog", false);
			CameraView.AttenuateWorldForPopupFocus(null);
			lastClosedEnabledObjectsCount = productionObjects.Count((ProductionObjectDisplay o) => ((Component)o).gameObject.activeSelf);
			if (moveTween != null)
			{
				TweenExtensions.Kill(moveTween, false);
				rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, 0f);
			}
			isOpened = false;
			moveTween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<Vector2, Vector2, VectorOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOAnchorPosY(rectTransform, posYInit, 0.25f, false), (Ease)26), (TweenCallback)delegate
			{
				((Behaviour)canvas).enabled = false;
			});
			canvasGroup.blocksRaycasts = false;
			for (int i = 0; i < TPSingleton<BuildingManager>.Instance.ProductionReport.ProducedObjects.Count; i++)
			{
				productionObjects[i].Hide();
			}
			TPSingleton<HUDJoystickNavigationManager>.Instance.ExitHUDNavigationMode();
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: false);
			if (playCloseSound)
			{
				closeAudioSource.Play();
			}
		}
	}

	public void RefreshGameObjects()
	{
		int count = TPSingleton<BuildingManager>.Instance.ProductionReport.ProducedObjects.Count;
		int i;
		for (i = 0; i < count; i++)
		{
			while (productionObjects.Count <= i)
			{
				ProductionObjectDisplay item = Object.Instantiate<ProductionObjectDisplay>(productionObjectPrefab, (Transform)(object)productionObjectParent);
				productionObjects.Add(item);
			}
			((Component)productionObjects[i]).gameObject.SetActive(true);
			productionObjects[i].ProductionObject = TPSingleton<BuildingManager>.Instance.ProductionReport.ProducedObjects[i];
			productionObjects[i].ProductionObject.ProductionObjectView = productionObjects[i];
			productionObjects[i].Display();
		}
		for (; i < productionObjects.Count; i++)
		{
			((Component)productionObjects[i]).gameObject.SetActive(false);
		}
	}

	public bool SelectFirstProduct()
	{
		ProductionObjectDisplay? productionObjectDisplay = productionObjects.FirstOrDefault((ProductionObjectDisplay x) => ((Component)x).gameObject.activeSelf);
		GameObject val = ((productionObjectDisplay != null) ? ((Component)productionObjectDisplay).gameObject : null);
		EventSystem.current.SetSelectedGameObject(val);
		return (Object)(object)val != (Object)null;
	}

	public void Open()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		if (isOpened)
		{
			return;
		}
		firstFrameOpened = true;
		CLoggerManager.Log((object)"ProductionReportPanel opened", (Object)(object)this, (LogType)3, (CLogLevel)0, true, "StaticLog", false);
		GameController.SetState(Game.E_State.ProductionReport);
		CameraView.AttenuateWorldForPopupFocus((IOverlayUser)(object)this);
		if (moveTween != null)
		{
			TweenExtensions.Kill(moveTween, false);
			rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, posYInit);
		}
		isOpened = true;
		SimpleFontLocalizedParent obj = simpleFontLocalizedParent;
		if (obj != null)
		{
			((FontLocalizedParent)obj).RefreshChilds();
		}
		TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: true);
		TweenerCore<Vector2, Vector2, VectorOptions> obj2 = TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOAnchorPosY(rectTransform, panelDeltaPosY, 0.25f, false), (Ease)27);
		object obj3 = _003C_003Ec._003C_003E9__38_0;
		if (obj3 == null)
		{
			TweenCallback val = delegate
			{
				TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: false);
			};
			_003C_003Ec._003C_003E9__38_0 = val;
			obj3 = (object)val;
		}
		moveTween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<Vector2, Vector2, VectorOptions>>(obj2, (TweenCallback)obj3);
		RefreshGameObjects();
		RefreshTitle();
		RefreshScrollbar();
		((Behaviour)canvas).enabled = true;
		canvasGroup.blocksRaycasts = true;
		if (InputManager.IsLastControllerJoystick)
		{
			joystickTarget.ClearSelectables();
			foreach (ProductionObjectDisplay productionObject in productionObjects)
			{
				joystickTarget.AddSelectable(productionObject.Selectable);
			}
			TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(JoystickTarget.GetSelectionInfo());
			layoutNavigationInitializer.InitNavigation();
			SelectFirstProduct();
		}
		SoundManager.PlayAudioClip(openAudioClip);
		if (TPSingleton<BuildingManager>.Instance.ProductionReport.ProducedObjects.Count > lastClosedEnabledObjectsCount)
		{
			newItemsAudioSource.Play();
		}
	}

	public void OnCloseButtonClick()
	{
		if (!TPSingleton<ChooseRewardPanel>.Instance.IsOpened)
		{
			GameController.SetState(Game.E_State.Management);
			Close();
		}
	}

	public void OnTopButtonClick()
	{
		productionReportScrollbar.value = Mathf.Clamp01(productionReportScrollbar.value + scrollButtonsSensitivity);
	}

	public void OnBotButtonClick()
	{
		productionReportScrollbar.value = Mathf.Clamp01(productionReportScrollbar.value - scrollButtonsSensitivity);
	}

	protected override void Awake()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		base.Awake();
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Combine((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
		canvas = ((Component)TPSingleton<ProductionReportPanel>.Instance).GetComponent<Canvas>();
		((Behaviour)canvas).enabled = false;
		canvasGroup = ((Component)TPSingleton<ProductionReportPanel>.Instance).GetComponent<CanvasGroup>();
		canvasGroup.blocksRaycasts = false;
		isOpened = false;
		rectTransform = ((Component)this).GetComponent<RectTransform>();
		posYInit = rectTransform.anchoredPosition.y;
	}

	private void OnLocalize()
	{
		if (((Component)this).gameObject.activeInHierarchy)
		{
			RefreshTitle();
		}
	}

	private void OnDestroy()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Remove((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
	}

	private void Update()
	{
		if (isOpened && !TPSingleton<ChooseRewardPanel>.Instance.IsOpened)
		{
			if (firstFrameOpened)
			{
				firstFrameOpened = false;
			}
			else if ((InputManager.GetButtonDown(29) || InputManager.GetButtonDown(80)) && TPSingleton<GameManager>.Instance.Game.State == Game.E_State.ProductionReport)
			{
				OnCloseButtonClick();
			}
		}
	}

	[ContextMenu("Open")]
	public void DebugOpen()
	{
		if (!Application.isPlaying)
		{
			Debug.LogError((object)"Unable to use this context menu when the application is not running");
		}
		else
		{
			if (TPSingleton<ProductionReportPanel>.Instance.isOpened)
			{
				return;
			}
			GameController.SetState(Game.E_State.ProductionReport);
			if (TPSingleton<BuildingManager>.Instance.ProductionReport.ProducedObjects.Count == 0)
			{
				for (int i = 0; i < 11; i++)
				{
					ProductionItems productionItem = new ProductionItemController(BuildingDatabase.BuildingDefinitions["Blacksmith"]).ProductionItem;
					productionItem.IsNightProduction = false;
					TPSingleton<BuildingManager>.Instance.ProductionReport.ProductionReportController.AddProductionObject(productionItem);
				}
			}
			Open();
		}
	}
}
