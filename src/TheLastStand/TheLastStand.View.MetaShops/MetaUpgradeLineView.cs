using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Localization.Fonts;
using TPLib.Yield;
using TheLastStand.Database;
using TheLastStand.Database.Building;
using TheLastStand.Database.Meta;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Manager.Meta;
using TheLastStand.Model.Meta;
using TheLastStand.View.Building.UI;
using TheLastStand.View.Camera;
using TheLastStand.View.HUD;
using TheLastStand.View.Item;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.MetaShops;

public abstract class MetaUpgradeLineView : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
{
	public enum E_State
	{
		None,
		Locked,
		Unlocked,
		Activated
	}

	private static class Constants
	{
		public const string MetaUnlocksLockedTitle = "<color=#fff>???</color>";

		public const string LockedUpgradeTitle = "???";

		public const float LockedLineAlpha = 0.35f;

		public const float UnlockedLineAlpha = 1f;

		public const float FxWidth = 1200f;

		public const float FxHeight = 400f;

		public const float LineMinHeight = 104f;

		public const float DelayBeforeFxAnimation = 0.25f;

		public const float DelayAfterFxAnimation = 0.35f;

		public const float SliderHeight = 45f;

		public const float SpacingHeight = 15f;
	}

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	protected RectTransform parentRectTransform;

	[SerializeField]
	protected LayoutElement layoutElement;

	[SerializeField]
	protected Image fillerDynamic;

	[SerializeField]
	private GameObject newLabel;

	[SerializeField]
	private Image noSliderBackground;

	[SerializeField]
	protected LayoutElement noSliderLayoutElement;

	[SerializeField]
	private Image stateImage;

	[SerializeField]
	private SimpleFontLocalizedParent fontLocalizedParent;

	[SerializeField]
	private RectTransform rectTransform;

	[SerializeField]
	private GameObject selector;

	[SerializeField]
	protected TextMeshProUGUI selectorLabel;

	[SerializeField]
	private Image bottomSeparator;

	[SerializeField]
	private Image topSeparator;

	[SerializeField]
	private TextMeshProUGUI upgradeNameText;

	[SerializeField]
	private Image upgradeIconBorder;

	[SerializeField]
	private Image upgradeIcon;

	[SerializeField]
	protected Slider slider;

	[SerializeField]
	private Animator sliderBackgroundAnimator;

	[SerializeField]
	protected Image sliderFill;

	[SerializeField]
	private Animator sliderFillAreaAnimator;

	[SerializeField]
	private Animator sliderHandleAnimator;

	[SerializeField]
	protected RectTransform descriptionContainer;

	[SerializeField]
	protected TextMeshProUGUI unlocksDescription;

	[SerializeField]
	protected RectTransform unlocksDescriptionRectTransform;

	[SerializeField]
	protected TextMeshProUGUI unlocksTitle;

	[SerializeField]
	protected RectTransform unlocksTitleRectTransform;

	[SerializeField]
	protected RectTransform descriptionIconsContainer;

	[SerializeField]
	private ItemIcon ItemIconPrefab;

	[SerializeField]
	private GlyphIcon GlyphIconPrefab;

	[SerializeField]
	private BuildingIcon BuildingIconPrefab;

	[SerializeField]
	private BuildingActionIcon BuildingActionIconPrefab;

	[SerializeField]
	private BuildingUpgradeIcon BuildingUpgradeIconPrefab;

	[SerializeField]
	private Ease fadeLineTweenEasing = (Ease)6;

	[SerializeField]
	private float fadeLineTweenDuration = 0.2f;

	[SerializeField]
	protected float fillingTweenDuration = 2f;

	[SerializeField]
	protected Ease fillingTweenEasing = (Ease)5;

	[SerializeField]
	protected float lineResizeTweenDuration = 0.35f;

	[SerializeField]
	protected Ease lineResizeTweenEasing = (Ease)3;

	[SerializeField]
	private Sprite activatedBoxMetaIconSprite;

	[SerializeField]
	private Sprite activateNoSliderSprite;

	[SerializeField]
	private Sprite checkSprite;

	[SerializeField]
	private Sprite lockBoxMetaIconSprite;

	[SerializeField]
	private Sprite lockNoSliderSprite;

	[SerializeField]
	private Sprite lockSeparatorSprite;

	[SerializeField]
	private Sprite lockSprite;

	[SerializeField]
	private Sprite unlockBoxMetaIconSprite;

	[SerializeField]
	private Sprite unlockSeparatorSprite;

	[SerializeField]
	private JoystickSelectable joystickSelectable;

	[SerializeField]
	private LayoutNavigationInitializer iconsLayoutNavigationInitializer;

	protected float previousSoulsSliderValue;

	private E_State currentState;

	private bool isANewMetaUpgrade;

	private Tween unlockTween;

	private Tween fadeLineTween;

	private MetaUpgradeLineTooltipPackage metaUpgradeLineTooltipPackage;

	private readonly List<ItemIcon> itemIcons = new List<ItemIcon>();

	private readonly List<GlyphIcon> glyphIcons = new List<GlyphIcon>();

	private readonly List<BuildingIcon> buildingIcons = new List<BuildingIcon>();

	private readonly List<BuildingActionIcon> buildingActionIcons = new List<BuildingActionIcon>();

	private readonly List<BuildingUpgradeIcon> buildingUpgradeIcons = new List<BuildingUpgradeIcon>();

	private bool? isActive;

	private bool dirty;

	private bool textsDirty;

	private bool redirectingSelectionToIcon;

	public bool IsDisplayed { get; private set; }

	protected abstract string SliderBackgroundIdleKey { get; }

	protected abstract string SliderFillAreaIdleKey { get; }

	protected abstract string SliderHandleIdleKey { get; }

	protected abstract string FxExplodeAnimationLabel { get; }

	public bool? IsActive => isActive;

	public JoystickSelectable JoystickSelectable => joystickSelectable;

	public RectTransform LineRectTransform
	{
		get
		{
			Transform transform = ((Component)this).transform;
			return (RectTransform)(object)((transform is RectTransform) ? transform : null);
		}
	}

	public MetaUpgrade MetaUpgrade { get; private set; }

	public Slider Slider => slider;

	protected bool IsANewMetaUpgrade
	{
		get
		{
			return isANewMetaUpgrade;
		}
		set
		{
			newLabel.SetActive(value);
			isANewMetaUpgrade = value;
		}
	}

	public E_State State
	{
		get
		{
			return currentState;
		}
		private set
		{
			StateHasChanged = currentState != value;
			currentState = value;
			RefreshData();
		}
	}

	public bool StateHasChanged { get; private set; }

	public void Init(MetaUpgrade metaUpgrade, E_State initialState, MetaUpgradeLineTooltipPackage newMetaUpgradeLineTooltipPackage)
	{
		MetaUpgrade = metaUpgrade;
		((Object)((Component)this).transform).name = ((Object)((Component)this).transform).name.Replace("(Clone)", " " + metaUpgrade.MetaUpgradeDefinition.Id);
		metaUpgradeLineTooltipPackage = newMetaUpgradeLineTooltipPackage;
		ChangeState(initialState);
	}

	public void ChangeState(E_State state)
	{
		State = state;
	}

	public void Display(bool display, bool forceRefresh = false)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if (textsDirty)
		{
			RefreshTexts(forceRefresh: true);
			textsDirty = false;
		}
		if (forceRefresh || IsDisplayed != display)
		{
			IsDisplayed = display;
			LayoutElement obj = layoutElement;
			float preferredHeight;
			if (!display)
			{
				Rect rect = parentRectTransform.rect;
				preferredHeight = ((Rect)(ref rect)).height;
			}
			else
			{
				preferredHeight = -1f;
			}
			obj.preferredHeight = preferredHeight;
			((Component)parentRectTransform).gameObject.SetActive(display);
			RefreshSliderIfNeeded();
		}
	}

	public virtual void OnPointerEnter(PointerEventData eventData)
	{
		selector.SetActive(true);
		MarkUpgradeAsSeen();
	}

	public virtual void OnPointerExit(PointerEventData eventData)
	{
		selector.SetActive(false);
	}

	public virtual void OnSliderValueChanged(float value)
	{
		fillerDynamic.fillAmount = sliderFill.fillAmount;
		previousSoulsSliderValue = value;
	}

	public void OnSelect()
	{
		if (InputManager.IsLastControllerJoystick)
		{
			OnPointerEnter(null);
			if (MetaUpgrade.MetaUpgradeDefinition.DamnedSoulsShop)
			{
				TPSingleton<DarkShopManager>.Instance.MetaShopView.OnSlotViewJoystickSelect(rectTransform);
			}
			else
			{
				TPSingleton<LightShopManager>.Instance.MetaShopView.OnSlotViewJoystickSelect(rectTransform);
			}
			iconsLayoutNavigationInitializer.InitNavigation();
			TPSingleton<OraculumView>.Instance.SetSelectedUpgrade(this);
			if (TryGetFirstIconSelectable(out var selectable))
			{
				((MonoBehaviour)this).StartCoroutine(SelectIconEndOfFrame(selectable));
			}
		}
	}

	public void OnDeselect()
	{
		if (InputManager.IsLastControllerJoystick && !redirectingSelectionToIcon)
		{
			TPSingleton<OraculumView>.Instance.SetSelectedUpgrade(null);
			OnPointerExit(null);
		}
	}

	public void OnIconDeselect()
	{
		if (InputManager.IsLastControllerJoystick)
		{
			((MonoBehaviour)this).StartCoroutine(OnIconDeselectEndOfFrame());
		}
	}

	public void RefreshData()
	{
		RefreshNewMetaUpgradeNotification();
		((UnityEventBase)slider.onValueChanged).RemoveAllListeners();
		RefreshSliderIfNeeded();
		RefreshSliderValues();
		RefreshSelector();
		RefreshStateImage();
		RefreshSeparators();
		RefreshMetaIcon();
		SimpleFontLocalizedParent obj = fontLocalizedParent;
		if (obj != null)
		{
			((FontLocalizedParent)obj).RefreshChilds();
		}
		if (currentState != E_State.Locked && canvasGroup.alpha != 1f)
		{
			canvasGroup.alpha = 1f;
		}
		else if (currentState == E_State.Locked && canvasGroup.alpha != 0.35f)
		{
			canvasGroup.alpha = 0.35f;
		}
		RefreshTexts();
		RefreshDescriptionIcons();
		if (State == E_State.Unlocked)
		{
			((UnityEvent<float>)(object)slider.onValueChanged).AddListener((UnityAction<float>)OnSliderValueChanged);
		}
	}

	public void SetActive(bool active)
	{
		if (active != IsActive)
		{
			isActive = active;
			((Component)this).gameObject.SetActive(active);
			RefreshSliderIfNeeded();
		}
	}

	public bool UpdateDisplay(bool forceRefresh = false)
	{
		bool flag = IsTopAboveBottomScreen() && IsBottomBelowTopScreen();
		Display(flag, forceRefresh);
		return flag;
	}

	public bool IsTopAboveBottomScreen()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		return ACameraView.MainCam.ScreenToViewportPoint(((Transform)rectTransform).position).y > 0f;
	}

	public bool IsBottomBelowTopScreen()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		Camera mainCam = ACameraView.MainCam;
		Vector3 position = ((Transform)rectTransform).position;
		Vector3 up = Vector3.up;
		Rect rect = rectTransform.rect;
		return mainCam.ScreenToViewportPoint(position - up * ((Rect)(ref rect)).height).y < 1f;
	}

	protected virtual void ActivateMetaUpgradeLineAndModel()
	{
		MetaUpgradesManager.RefreshFulfilledUpgrades();
		TPSingleton<MetaUpgradesManager>.Instance.ActivateUpgrade(MetaUpgrade);
		TPSingleton<MetaConditionManager>.Instance.RefreshProgression();
		ChangeState(E_State.Activated);
		((MonoBehaviour)this).StartCoroutine(MoveMetaOnTopOfUnlocked());
		LayoutRebuilder.ForceRebuildLayoutImmediate(MetaUpgrade.MetaUpgradeDefinition.DamnedSoulsShop ? TPSingleton<DarkShopManager>.Instance.MetaShopView.LayoutGroupContainer : TPSingleton<LightShopManager>.Instance.MetaShopView.LayoutGroupContainer);
	}

	protected abstract Animator GetUnlockFxAnimator();

	protected abstract RectTransform GetUnlockFxTransform();

	protected abstract bool IsFromLightShop();

	protected virtual IEnumerator MoveMetaOnTopOfUnlocked()
	{
		if (InputManager.IsLastControllerJoystick)
		{
			EventSystem.current.SetSelectedGameObject((GameObject)null);
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: false);
		}
		UnlockUpgrade();
		yield return SharedYields.WaitForSeconds(0.25f);
		PlayUnlockFX();
		PlayFadeTween();
		yield return SharedYields.WaitForSeconds(0.35f);
		yield return null;
		canvasGroup.alpha = 1f;
	}

	protected virtual void RefreshMetaIcon()
	{
		switch (State)
		{
		case E_State.Locked:
			if ((Object)(object)upgradeIconBorder.sprite != (Object)(object)lockBoxMetaIconSprite)
			{
				upgradeIconBorder.sprite = lockBoxMetaIconSprite;
			}
			break;
		case E_State.Unlocked:
			if ((Object)(object)upgradeIconBorder.sprite != (Object)(object)unlockBoxMetaIconSprite)
			{
				upgradeIconBorder.sprite = unlockBoxMetaIconSprite;
			}
			break;
		case E_State.Activated:
			if ((Object)(object)upgradeIconBorder.sprite != (Object)(object)activatedBoxMetaIconSprite)
			{
				upgradeIconBorder.sprite = activatedBoxMetaIconSprite;
			}
			break;
		}
		if (currentState != E_State.Locked && !((Behaviour)upgradeIcon).enabled)
		{
			((Behaviour)upgradeIcon).enabled = true;
		}
		else if (currentState == E_State.Locked && ((Behaviour)upgradeIcon).enabled)
		{
			((Behaviour)upgradeIcon).enabled = false;
		}
		if (((Behaviour)upgradeIcon).enabled && (Object)(object)upgradeIcon.sprite == (Object)null)
		{
			upgradeIcon.sprite = ResourcePooler.LoadOnce<Sprite>(MetaUpgrade.MetaUpgradeDefinition.DamnedSoulsShop ? ("View/Sprites/UI/Meta/DarkShop/Icon_DarkShop_" + MetaUpgrade.MetaUpgradeDefinition.IconName) : ("View/Sprites/UI/Meta/LightShop/Icon_LightShop_" + MetaUpgrade.MetaUpgradeDefinition.IconName), failSilently: false);
		}
		else if (currentState == E_State.Locked)
		{
			upgradeIcon.sprite = null;
		}
	}

	protected virtual void RefreshSelector()
	{
		((Component)selectorLabel).gameObject.SetActive(currentState == E_State.Unlocked);
	}

	protected abstract void RefreshShopNewMetaUpgrades();

	protected abstract void RefreshSliderValues();

	protected virtual void RefreshTexts(bool forceRefresh = false)
	{
		if (forceRefresh || StateHasChanged)
		{
			((TMP_Text)upgradeNameText).text = ((State != E_State.Locked) ? MetaUpgrade.Name : "???");
			switch (State)
			{
			case E_State.Activated:
				((TMP_Text)unlocksTitle).text = Localizer.Get("Meta_Unlocked");
				((TMP_Text)unlocksDescription).text = MetaUpgrade.Description;
				break;
			case E_State.Unlocked:
				((TMP_Text)unlocksTitle).text = Localizer.Get("Meta_Unlock");
				((TMP_Text)unlocksDescription).text = MetaUpgrade.Description;
				break;
			case E_State.Locked:
				((TMP_Text)unlocksTitle).text = "<color=#fff>???</color>";
				((TMP_Text)unlocksDescription).text = string.Empty;
				break;
			}
			StateHasChanged = false;
		}
	}

	private void Awake()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Combine((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
	}

	private void MarkUpgradeAsSeen()
	{
		TPSingleton<MetaShopsManager>.Instance.AddMetaUpgradeToAlreadySeen(MetaUpgrade);
		RefreshNewMetaUpgradeNotification();
		RefreshShopNewMetaUpgrades();
	}

	private void PlayUnlockFX()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		RectTransform unlockFxTransform = GetUnlockFxTransform();
		Transform parent = ((Transform)unlockFxTransform).parent;
		((Transform)unlockFxTransform).SetParent((Transform)(object)LineRectTransform);
		unlockFxTransform.anchoredPosition = Vector2.zero;
		((Transform)unlockFxTransform).SetParent(parent);
		float num = parentRectTransform.sizeDelta.y - 104f;
		unlockFxTransform.sizeDelta = new Vector2(1200f, 400f + num);
		GetUnlockFxAnimator().Play(FxExplodeAnimationLabel);
	}

	private void OnDestroy()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Remove((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
		((UnityEventBase)slider.onValueChanged).RemoveAllListeners();
	}

	private IEnumerator OnIconDeselectEndOfFrame()
	{
		yield return SharedYields.WaitForEndOfFrame;
		for (int i = 0; i < ((Transform)descriptionIconsContainer).childCount; i++)
		{
			if ((Object)(object)EventSystem.current.currentSelectedGameObject == (Object)(object)((Component)((Transform)descriptionIconsContainer).GetChild(i)).gameObject)
			{
				yield break;
			}
		}
		OnPointerExit(null);
		if ((Object)(object)TPSingleton<OraculumView>.Instance.SelectedUpgrade == (Object)(object)this)
		{
			TPSingleton<OraculumView>.Instance.SetSelectedUpgrade(null);
		}
	}

	private void OnLocalize()
	{
		if (((Component)this).gameObject.activeInHierarchy)
		{
			RefreshTexts(forceRefresh: true);
		}
		else
		{
			textsDirty = true;
		}
	}

	private void PlayFadeTween()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		Tween obj = fadeLineTween;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
		fadeLineTween = (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTweenModuleUI.DOFade(canvasGroup, 0f, fadeLineTweenDuration), fadeLineTweenEasing);
	}

	private IEnumerator SelectIconEndOfFrame(Selectable selectable)
	{
		redirectingSelectionToIcon = true;
		yield return SharedYields.WaitForEndOfFrame;
		EventSystem.current.SetSelectedGameObject(((Component)selectable).gameObject);
		redirectingSelectionToIcon = false;
	}

	private bool TryGetFirstIconSelectable(out Selectable selectable)
	{
		selectable = null;
		if (itemIcons.Count > 0)
		{
			selectable = itemIcons[0].Selectable;
		}
		else if (glyphIcons.Count > 0)
		{
			selectable = glyphIcons[0].Selectable;
		}
		else if (buildingIcons.Count > 0)
		{
			selectable = buildingIcons[0].Selectable;
		}
		else if (buildingActionIcons.Count > 0)
		{
			selectable = buildingActionIcons[0].Selectable;
		}
		else if (buildingUpgradeIcons.Count > 0)
		{
			selectable = buildingUpgradeIcons[0].Selectable;
		}
		return (Object)(object)selectable != (Object)null;
	}

	private void RefreshDescriptionIcons()
	{
		if (State != E_State.Locked)
		{
			bool active = MetaUpgrade.MetaUpgradeDefinition.ItemsToShow.Count > 0 || MetaUpgrade.MetaUpgradeDefinition.GlyphsToShow.Count > 0 || MetaUpgrade.MetaUpgradeDefinition.BuildingsToShow.Count > 0 || MetaUpgrade.MetaUpgradeDefinition.BuildingActionsToShow.Count > 0 || MetaUpgrade.MetaUpgradeDefinition.BuildingUpgradesToShow.Count > 0;
			((Component)descriptionIconsContainer).gameObject.SetActive(active);
			RefreshItemIcons();
			RefreshGlyphIcons();
			RefreshBuildingIcons();
			RefreshBuildingActionIcons();
			RefreshBuildingUpgradeIcons();
		}
	}

	private void RefreshItemIcons()
	{
		RefreshIconsOfType(itemIcons, ItemIconPrefab, MetaUpgrade.MetaUpgradeDefinition.ItemsToShow.Count, InitItem);
		void InitItem(int i)
		{
			itemIcons[i].Init(ItemDatabase.ItemDefinitions[MetaUpgrade.MetaUpgradeDefinition.ItemsToShow[i]], this, metaUpgradeLineTooltipPackage.ItemTooltip, IsFromLightShop());
		}
	}

	private void RefreshGlyphIcons()
	{
		RefreshIconsOfType(glyphIcons, GlyphIconPrefab, MetaUpgrade.MetaUpgradeDefinition.GlyphsToShow.Count, InitGlyph);
		void InitGlyph(int i)
		{
			glyphIcons[i].Init(GlyphDatabase.GlyphDefinitions[MetaUpgrade.MetaUpgradeDefinition.GlyphsToShow[i]], this, metaUpgradeLineTooltipPackage.GlyphTooltip, IsFromLightShop());
		}
	}

	private void RefreshBuildingIcons()
	{
		RefreshIconsOfType(buildingIcons, BuildingIconPrefab, MetaUpgrade.MetaUpgradeDefinition.BuildingsToShow.Count, InitBuilding);
		void InitBuilding(int i)
		{
			buildingIcons[i].Init(BuildingDatabase.BuildingDefinitions[MetaUpgrade.MetaUpgradeDefinition.BuildingsToShow[i]], this, metaUpgradeLineTooltipPackage.BuildingTooltip, IsFromLightShop());
		}
	}

	private void RefreshBuildingActionIcons()
	{
		RefreshIconsOfType(buildingActionIcons, BuildingActionIconPrefab, MetaUpgrade.MetaUpgradeDefinition.BuildingActionsToShow.Count, InitBuildingAction);
		void InitBuildingAction(int i)
		{
			buildingActionIcons[i].Init(BuildingDatabase.BuildingActionDefinitions[MetaUpgrade.MetaUpgradeDefinition.BuildingActionsToShow[i]], this, metaUpgradeLineTooltipPackage.BuildingActionTooltip, IsFromLightShop());
		}
	}

	private void RefreshBuildingUpgradeIcons()
	{
		RefreshIconsOfType(buildingUpgradeIcons, BuildingUpgradeIconPrefab, MetaUpgrade.MetaUpgradeDefinition.BuildingUpgradesToShow.Count, InitBuildingUpgrade);
		void InitBuildingUpgrade(int i)
		{
			buildingUpgradeIcons[i].Init(BuildingDatabase.BuildingUpgradeDefinitions[MetaUpgrade.MetaUpgradeDefinition.BuildingUpgradesToShow[i]], this, metaUpgradeLineTooltipPackage.BuildingUpgradeTooltip, IsFromLightShop());
		}
	}

	private void RefreshIconsOfType<T>(List<T> icons, T iconPrefab, int count, Action<int> initAction) where T : OraculumUnlockIcon
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		for (int num = icons.Count - 1; num >= count; num--)
		{
			Object.Destroy((Object)(object)((Component)buildingActionIcons[num]).gameObject);
			icons.RemoveAt(num);
		}
		for (int i = icons.Count; i < count; i++)
		{
			icons.Add(Object.Instantiate<T>(iconPrefab, (Transform)(object)descriptionIconsContainer));
		}
		for (int j = 0; j < count; j++)
		{
			icons[j].PointerEventsListener.OnPointerDownEvent.AddListener((UnityAction)delegate
			{
				OnPointerDown(null);
			});
			icons[j].PointerEventsListener.OnPointerUpEvent.AddListener((UnityAction)delegate
			{
				OnPointerUp(null);
			});
			initAction(j);
		}
	}

	private void RefreshNewMetaUpgradeNotification()
	{
		IsANewMetaUpgrade = TPSingleton<MetaShopsManager>.Instance.IsANewUpgrade(MetaUpgrade) && State != E_State.Locked && State != E_State.Activated;
	}

	private void RefreshSeparators()
	{
		if (State == E_State.Locked && ((Object)(object)topSeparator.sprite != (Object)(object)lockSeparatorSprite || (Object)(object)bottomSeparator.sprite != (Object)(object)lockSeparatorSprite))
		{
			topSeparator.sprite = lockSeparatorSprite;
			bottomSeparator.sprite = lockSeparatorSprite;
		}
		else if (State != E_State.Locked && ((Object)(object)topSeparator.sprite != (Object)(object)unlockSeparatorSprite || (Object)(object)bottomSeparator.sprite != (Object)(object)unlockSeparatorSprite))
		{
			topSeparator.sprite = unlockSeparatorSprite;
			bottomSeparator.sprite = unlockSeparatorSprite;
		}
	}

	private void RefreshSlider()
	{
		((Component)noSliderBackground).gameObject.SetActive(State != E_State.Unlocked);
		((Component)noSliderLayoutElement).gameObject.SetActive(State != E_State.Unlocked);
		((Component)slider).gameObject.SetActive(State == E_State.Unlocked);
		Image val = noSliderBackground;
		val.sprite = (Sprite)(State switch
		{
			E_State.Activated => activateNoSliderSprite, 
			E_State.Locked => lockNoSliderSprite, 
			_ => null, 
		});
		if (State == E_State.Unlocked)
		{
			float value = Random.value;
			sliderBackgroundAnimator.Play(SliderBackgroundIdleKey, 0, value);
			sliderFillAreaAnimator.Play(SliderFillAreaIdleKey, 0, value);
			sliderHandleAnimator.Play(SliderHandleIdleKey, 0, value);
		}
	}

	private void RefreshSliderIfNeeded()
	{
		if (IsDisplayed && isActive == true && dirty)
		{
			RefreshSlider();
			dirty = false;
		}
		else
		{
			dirty = true;
		}
	}

	private void RefreshStateImage()
	{
		if (State == E_State.Unlocked && ((Component)stateImage).gameObject.activeSelf)
		{
			((Component)stateImage).gameObject.SetActive(false);
			return;
		}
		if (currentState == E_State.Unlocked)
		{
			return;
		}
		if (!((Component)stateImage).gameObject.activeSelf)
		{
			((Component)stateImage).gameObject.SetActive(true);
		}
		Image val = stateImage;
		E_State e_State = currentState;
		Sprite sprite;
		if (e_State != E_State.Locked)
		{
			if (e_State != E_State.Activated || !((Object)(object)stateImage.sprite != (Object)(object)checkSprite))
			{
				goto IL_00b4;
			}
			sprite = checkSprite;
		}
		else
		{
			if (!((Object)(object)stateImage.sprite != (Object)(object)lockSprite))
			{
				goto IL_00b4;
			}
			sprite = lockSprite;
		}
		goto IL_00c0;
		IL_00b4:
		sprite = stateImage.sprite;
		goto IL_00c0;
		IL_00c0:
		val.sprite = sprite;
	}

	private void UnlockUpgrade()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		unlockTween = (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<Vector2, Vector2, VectorOptions>>(DOTweenModuleUI.DOPreferredSize(noSliderLayoutElement, new Vector2(noSliderLayoutElement.preferredWidth, 0f), lineResizeTweenDuration, false), lineResizeTweenEasing);
	}

	public abstract void OnPointerUp(PointerEventData eventData);

	public abstract void OnPointerDown(PointerEventData eventData);
}
