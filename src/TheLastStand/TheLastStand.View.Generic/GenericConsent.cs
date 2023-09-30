using System;
using System.Collections;
using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Localization.Fonts;
using TPLib.UI;
using TPLib.Yield;
using TheLastStand.Controller;
using TheLastStand.Framework.Automaton;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Model;
using TheLastStand.View.Camera;
using TheLastStand.View.CharacterSheet;
using TheLastStand.View.NightReport;
using TheLastStand.View.PlayableUnitCustomisation;
using TheLastStand.View.ProductionReport;
using TheLastStand.View.Recruitment;
using TheLastStand.View.Shop;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.Generic;

public class GenericConsent : MonoBehaviour, IOverlayUser
{
	public class Constants
	{
		public const string SmallContentLocalizedFontChilds = "SmallContent";

		public const string ContentLocalizedFontChilds = "Content";
	}

	private static GenericConsent instance;

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private Image overlay;

	[SerializeField]
	private ComplexFontLocalizedParent complexFontLocalizedParent;

	[SerializeField]
	private GameObject popupContainer;

	[SerializeField]
	private Button closeButton;

	[SerializeField]
	private BetterButton cancelButton;

	[SerializeField]
	private BetterButton confirmButton;

	[SerializeField]
	private TextMeshProUGUI coreText;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private GameObject smallPopupContainer;

	[SerializeField]
	private Button smallCloseButton;

	[SerializeField]
	private BetterButton smallCancelButton;

	[SerializeField]
	private BetterButton smallConfirmButton;

	[SerializeField]
	private TextMeshProUGUI smallCoreText;

	[SerializeField]
	private TextMeshProUGUI smallTitleText;

	private Action onCancel;

	private Action onConfirm;

	private bool isSmallVersion;

	private bool openThisFrame;

	private bool isOpen;

	public Canvas Canvas => canvas;

	public ParameterizedLocalizationLine CancelButtonContent { get; private set; }

	public ParameterizedLocalizationLine ConfirmButtonContent { get; private set; }

	public string LocalizedText { get; private set; }

	public int OverlaySortingOrder => Canvas.sortingOrder - 1;

	public ParameterizedLocalizationLine Text { get; private set; }

	public ParameterizedLocalizationLine Title { get; private set; }

	public static bool IsOpen => instance.isOpen;

	public static bool IsWaitingForInput()
	{
		if ((Object)(object)instance != (Object)null && ((Behaviour)instance.Canvas).enabled)
		{
			return !instance.openThisFrame;
		}
		return false;
	}

	public static GenericConsent Open(string textLocKey, Action onConfirm, Action onCancel, bool smallVersion = false)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		return Open(new ParameterizedLocalizationLine("GenericConsent_Title", Array.Empty<string>()), new ParameterizedLocalizationLine(textLocKey, Array.Empty<string>()), onConfirm, onCancel, null, null, smallVersion);
	}

	public static GenericConsent Open(string titleLocKey, string textLocKey, Action onConfirm, Action onCancel, bool smallVersion = false)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		return Open(new ParameterizedLocalizationLine(titleLocKey, Array.Empty<string>()), new ParameterizedLocalizationLine(textLocKey, Array.Empty<string>()), onConfirm, onCancel, null, null, smallVersion);
	}

	public static GenericConsent Open(ParameterizedLocalizationLine titleLocKey, string localizedText, Action onConfirm, Action onCancel, ParameterizedLocalizationLine? confirmButtonLocKey = null, ParameterizedLocalizationLine? cancelButtonLocKey = null, bool smallVersion = false)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)instance == (Object)null)
		{
			Debug.LogError((object)"Someone tried to open a generic consent, but there are NONE here! Please add it to the scene.");
			return null;
		}
		instance.Title = titleLocKey;
		instance.LocalizedText = localizedText;
		instance.ConfirmButtonContent = (ParameterizedLocalizationLine)(((_003F?)confirmButtonLocKey) ?? new ParameterizedLocalizationLine("GenericPopup_Confirm", Array.Empty<string>()));
		instance.CancelButtonContent = (ParameterizedLocalizationLine)(((_003F?)cancelButtonLocKey) ?? new ParameterizedLocalizationLine("GenericConsent_Cancel", Array.Empty<string>()));
		instance.isSmallVersion = smallVersion;
		return Open(onConfirm, onCancel);
	}

	public static GenericConsent Open(ParameterizedLocalizationLine titleLocKey, ParameterizedLocalizationLine textLocKey, Action onConfirm, Action onCancel, ParameterizedLocalizationLine? confirmButtonLocKey = null, ParameterizedLocalizationLine? cancelButtonLocKey = null, bool smallVersion = false)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)instance == (Object)null)
		{
			Debug.LogError((object)"Someone tried to open a generic consent, but there are NONE here! Please add it to the scene.");
			return null;
		}
		instance.Title = titleLocKey;
		instance.Text = textLocKey;
		instance.LocalizedText = string.Empty;
		instance.ConfirmButtonContent = (ParameterizedLocalizationLine)(((_003F?)confirmButtonLocKey) ?? new ParameterizedLocalizationLine("GenericPopup_Confirm", Array.Empty<string>()));
		instance.CancelButtonContent = (ParameterizedLocalizationLine)(((_003F?)cancelButtonLocKey) ?? new ParameterizedLocalizationLine("GenericConsent_Cancel", Array.Empty<string>()));
		instance.isSmallVersion = smallVersion;
		return Open(onConfirm, onCancel);
	}

	public static GenericConsent Open(ParameterizedLocalizationLine textLocKey, Action onConfirm, ParameterizedLocalizationLine? confirmButtonLocKey = null, ParameterizedLocalizationLine? cancelButtonLocKey = null, bool smallVersion = false)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)instance == (Object)null)
		{
			Debug.LogError((object)"Someone tried to open a generic consent, but there are NONE here! Please add it to the scene.");
			return null;
		}
		instance.Title = new ParameterizedLocalizationLine("GenericConsent_Title", Array.Empty<string>());
		instance.Text = textLocKey;
		instance.LocalizedText = string.Empty;
		instance.ConfirmButtonContent = (ParameterizedLocalizationLine)(((_003F?)confirmButtonLocKey) ?? new ParameterizedLocalizationLine("GenericPopup_Confirm", Array.Empty<string>()));
		instance.CancelButtonContent = (ParameterizedLocalizationLine)(((_003F?)cancelButtonLocKey) ?? new ParameterizedLocalizationLine("GenericConsent_Cancel", Array.Empty<string>()));
		instance.isSmallVersion = smallVersion;
		return Open(onConfirm, null);
	}

	public static GenericConsent Open(ParameterizedLocalizationLine textLocKey, Action onConfirm, Action onCancel, ParameterizedLocalizationLine? confirmButtonLocKey = null, ParameterizedLocalizationLine? cancelButtonLocKey = null, bool smallVersion = false)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)instance == (Object)null)
		{
			Debug.LogError((object)"Someone tried to open a generic consent, but there are NONE here! Please add it to the scene.");
			return null;
		}
		instance.Title = new ParameterizedLocalizationLine("GenericConsent_Title", Array.Empty<string>());
		instance.Text = textLocKey;
		instance.LocalizedText = string.Empty;
		instance.ConfirmButtonContent = (ParameterizedLocalizationLine)(((_003F?)confirmButtonLocKey) ?? new ParameterizedLocalizationLine("GenericPopup_Confirm", Array.Empty<string>()));
		instance.CancelButtonContent = (ParameterizedLocalizationLine)(((_003F?)cancelButtonLocKey) ?? new ParameterizedLocalizationLine("GenericConsent_Cancel", Array.Empty<string>()));
		instance.isSmallVersion = smallVersion;
		return Open(onConfirm, onCancel);
	}

	public static GenericConsent OpenLocalized(string localizedText, Action onConfirm, Action onCancel, bool smallVersion = false)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return Open(new ParameterizedLocalizationLine("GenericConsent_Title", Array.Empty<string>()), localizedText, onConfirm, onCancel, null, null, smallVersion);
	}

	public static GenericConsent OpenLocalized(string titleLocKey, string localizedText, Action onConfirm, Action onCancel, bool smallVersion = false)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return Open(new ParameterizedLocalizationLine(titleLocKey, Array.Empty<string>()), localizedText, onConfirm, onCancel, null, null, smallVersion);
	}

	public void RefreshContent()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		((Behaviour)overlay).enabled = !TPSingleton<ACameraView>.Exist() || !(TPSingleton<ACameraView>.Instance is CameraView);
		if ((Text.key != null || !string.IsNullOrEmpty(LocalizedText)) && Title.key != null)
		{
			smallPopupContainer.SetActive(isSmallVersion && ((Behaviour)Canvas).enabled);
			popupContainer.SetActive(!isSmallVersion && ((Behaviour)Canvas).enabled);
			if (isSmallVersion)
			{
				((TMP_Text)smallCoreText).text = (string.IsNullOrEmpty(LocalizedText) ? Localizer.Get(Text) : LocalizedText);
				((TMP_Text)smallTitleText).text = Localizer.Get(Title);
				smallCancelButton.ChangeText(Localizer.Get(CancelButtonContent));
				smallConfirmButton.ChangeText(Localizer.Get(ConfirmButtonContent));
			}
			else
			{
				((TMP_Text)coreText).text = (string.IsNullOrEmpty(LocalizedText) ? Localizer.Get(Text) : LocalizedText);
				((TMP_Text)titleText).text = Localizer.Get(Title);
				cancelButton.ChangeText(Localizer.Get(CancelButtonContent));
				confirmButton.ChangeText(Localizer.Get(ConfirmButtonContent));
			}
			complexFontLocalizedParent.TargetKey = (isSmallVersion ? "SmallContent" : "Content");
			((FontLocalizedParent)complexFontLocalizedParent).RefreshChilds();
		}
	}

	private static GenericConsent Open(Action onConfirm, Action onCancel)
	{
		((Behaviour)instance.Canvas).enabled = true;
		instance.onConfirm = onConfirm;
		instance.onCancel = onCancel;
		instance.isOpen = true;
		instance.RefreshContent();
		if (TPSingleton<ACameraView>.Exist() && TPSingleton<ACameraView>.Instance is CameraView)
		{
			CameraView.AttenuateWorldForPopupFocus((IOverlayUser)(object)instance);
		}
		if (TPSingleton<GameManager>.Exist())
		{
			GameController.SetState(Game.E_State.ConsentPopup);
		}
		InputManager.OnGenericConsentViewToggled(state: true);
		((MonoBehaviour)instance).StartCoroutine(instance.OpenThisFrameCoroutine());
		if (InputManager.IsLastControllerJoystick)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.ExitHUDNavigationMode();
		}
		return instance;
	}

	private void Awake()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected O, but got Unknown
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Expected O, but got Unknown
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Expected O, but got Unknown
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Expected O, but got Unknown
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Expected O, but got Unknown
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Expected O, but got Unknown
		instance = this;
		((UnityEvent)closeButton.onClick).AddListener((UnityAction)delegate
		{
			StartCloseCoroutine();
		});
		((UnityEvent)((Button)confirmButton).onClick).AddListener(new UnityAction(Confirm));
		((UnityEvent)((Button)cancelButton).onClick).AddListener((UnityAction)delegate
		{
			StartCloseCoroutine();
		});
		((UnityEvent)smallCloseButton.onClick).AddListener((UnityAction)delegate
		{
			StartCloseCoroutine();
		});
		((UnityEvent)((Button)smallConfirmButton).onClick).AddListener(new UnityAction(Confirm));
		((UnityEvent)((Button)smallCancelButton).onClick).AddListener((UnityAction)delegate
		{
			StartCloseCoroutine();
		});
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Combine((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
	}

	private void Close()
	{
		if (TPSingleton<GameManager>.Exist() && TPSingleton<GameManager>.Instance.Game.PreviousState != Game.E_State.ConsentPopup)
		{
			GameController.SetState(TPSingleton<GameManager>.Instance.Game.PreviousState);
		}
		if (TPSingleton<ACameraView>.Exist() && TPSingleton<ACameraView>.Instance is CameraView)
		{
			if (TPSingleton<GameManager>.Exist())
			{
				switch (TPSingleton<GameManager>.Instance.Game.State)
				{
				case Game.E_State.CharacterSheet:
					CameraView.AttenuateWorldForPopupFocus((IOverlayUser)(object)TPSingleton<CharacterSheetPanel>.Instance);
					break;
				case Game.E_State.Recruitment:
					CameraView.AttenuateWorldForPopupFocus((IOverlayUser)(object)TPSingleton<RecruitmentView>.Instance);
					break;
				case Game.E_State.Shopping:
					CameraView.AttenuateWorldForPopupFocus((IOverlayUser)(object)TPSingleton<ShopView>.Instance);
					break;
				case Game.E_State.NightReport:
					CameraView.AttenuateWorldForPopupFocus((IOverlayUser)(object)TPSingleton<NightReportPanel>.Instance);
					break;
				case Game.E_State.ProductionReport:
					CameraView.AttenuateWorldForPopupFocus((IOverlayUser)(object)TPSingleton<ProductionReportPanel>.Instance);
					break;
				case Game.E_State.Settings:
					CameraView.AttenuateWorldForPopupFocus((IOverlayUser)(object)TPSingleton<SettingsManager>.Instance.SettingsPanel);
					break;
				case Game.E_State.GameOver:
					CameraView.AttenuateWorldForPopupFocus((IOverlayUser)(object)TPSingleton<GameOverPanel>.Instance);
					break;
				case Game.E_State.UnitCustomisation:
					CameraView.AttenuateWorldForPopupFocus((IOverlayUser)(object)TPSingleton<PlayableUnitCustomisationPanel>.Instance);
					break;
				default:
					CameraView.AttenuateWorldForPopupFocus(null);
					break;
				}
			}
			else
			{
				CameraView.AttenuateWorldForPopupFocus((IOverlayUser)(object)((((StateMachine)ApplicationManager.Application).State.GetName() == "Settings") ? TPSingleton<SettingsManager>.Instance.SettingsPanel : null));
			}
		}
		InputManager.OnGenericConsentViewToggled(state: false);
		((Behaviour)instance.Canvas).enabled = false;
		ACameraView.AllowUserPan = true;
		instance.isOpen = false;
	}

	private IEnumerator CloseAtEndOfFrame()
	{
		yield return SharedYields.WaitForEndOfFrame;
		Close();
	}

	private void Confirm()
	{
		StartCloseCoroutine(mustCallOnCancel: false);
		onConfirm?.Invoke();
	}

	private void OnDestroy()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Remove((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
		instance = null;
	}

	private void OnEnable()
	{
		RefreshContent();
	}

	private void OnLocalize()
	{
		if (((Component)this).gameObject.activeInHierarchy)
		{
			RefreshContent();
		}
	}

	private IEnumerator OpenThisFrameCoroutine()
	{
		openThisFrame = true;
		yield return SharedYields.WaitForEndOfFrame;
		openThisFrame = false;
	}

	private void StartCloseCoroutine(bool mustCallOnCancel = true)
	{
		if (mustCallOnCancel)
		{
			onCancel?.Invoke();
		}
		((MonoBehaviour)this).StartCoroutine(CloseAtEndOfFrame());
	}

	private void Update()
	{
		if (IsWaitingForInput())
		{
			if (InputManager.GetButtonDown(29) || InputManager.GetButtonDown(80))
			{
				StartCloseCoroutine();
			}
			if (InputManager.GetButtonDown(7) || InputManager.GetButtonDown(66))
			{
				Confirm();
			}
		}
	}
}
