using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Localization.Fonts;
using TPLib.UI;
using TPLib.Yield;
using TheLastStand.Controller;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Unit;
using TheLastStand.View.Camera;
using TheLastStand.View.ProductionReport;
using TheLastStand.View.ToDoList;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.Generic;

public class GenericBlockingPopup : MonoBehaviour, IOverlayUser
{
	public static class Constants
	{
		public const string SmallContentLocalizedFontChilds = "SmallContent";

		public const string ContentLocalizedFontChilds = "Content";
	}

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private Image overlay;

	[SerializeField]
	private ComplexFontLocalizedParent complexFontLocalizedParent;

	[SerializeField]
	private GameObject popupContainer;

	[SerializeField]
	private BetterButton closeButton;

	[SerializeField]
	private GameObject complexBox;

	[SerializeField]
	private BetterButton confirmButton;

	[SerializeField]
	private TextMeshProUGUI coreComplexText;

	[SerializeField]
	private HyperlinkListener coreComplexHyperlink;

	[SerializeField]
	private TextMeshProUGUI coreSimpleText;

	[SerializeField]
	private HyperlinkListener coreSimpleHyperlink;

	[SerializeField]
	private BlockingPopupLine levelUpBlockingPopupLine;

	[SerializeField]
	private BlockingPopupLine rewardBlockingPopupLine;

	[SerializeField]
	private GameObject simpleBox;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private GameObject smallPopupContainer;

	[SerializeField]
	private BetterButton smallCloseButton;

	[SerializeField]
	private BetterButton smallConfirmButton;

	[SerializeField]
	private TextMeshProUGUI smallCoreSimpleText;

	[SerializeField]
	private HyperlinkListener smallCoreSimpleHyperlink;

	[SerializeField]
	private TextMeshProUGUI smallTitleText;

	private bool isSmallVersion;

	private bool openThisFrame;

	public static GenericBlockingPopup Instance { get; private set; }

	public Canvas Canvas => canvas;

	public ParameterizedLocalizationLine Text { get; private set; }

	public ParameterizedLocalizationLine Title { get; private set; }

	public int OverlaySortingOrder => Canvas.sortingOrder - 1;

	public static bool IsWaitingForInput()
	{
		if ((Object)(object)Instance != (Object)null && ((Behaviour)Instance.Canvas).enabled)
		{
			return !Instance.openThisFrame;
		}
		return false;
	}

	public static GenericBlockingPopup OpenAsComplex(string titleLocKey, string textLocKey, UnityAction confirmAction)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		return Open(new ParameterizedLocalizationLine(titleLocKey, Array.Empty<string>()), new ParameterizedLocalizationLine(textLocKey, Array.Empty<string>()), shouldShowAsComplex: true, confirmAction);
	}

	public static GenericBlockingPopup OpenAsSimple(string titleLocKey, string textLocKey, UnityAction confirmAction, bool smallVersion = false)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		return Open(new ParameterizedLocalizationLine(titleLocKey, Array.Empty<string>()), new ParameterizedLocalizationLine(textLocKey, Array.Empty<string>()), shouldShowAsComplex: false, confirmAction, smallVersion);
	}

	public static GenericBlockingPopup Open(ParameterizedLocalizationLine titleLocKey, ParameterizedLocalizationLine textLocKey, bool shouldShowAsComplex, UnityAction confirmAction, bool smallVersion = false)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Expected O, but got Unknown
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Expected O, but got Unknown
		if ((Object)(object)Instance == (Object)null)
		{
			Debug.LogError((object)"Someone tried to open a generic blocking popup, but there are NONE here! Please add it to the scene.");
			return null;
		}
		Instance.isSmallVersion = smallVersion;
		((Behaviour)Instance.Canvas).enabled = true;
		Instance.Title = titleLocKey;
		Instance.Text = textLocKey;
		Instance.RefreshContent(shouldShowAsComplex);
		((UnityEventBase)((Button)Instance.confirmButton).onClick).RemoveAllListeners();
		((UnityEvent)((Button)Instance.confirmButton).onClick).AddListener(new UnityAction(Instance.StartCloseCoroutine));
		((UnityEvent)((Button)Instance.confirmButton).onClick).AddListener(confirmAction);
		((UnityEventBase)((Button)Instance.smallConfirmButton).onClick).RemoveAllListeners();
		((UnityEvent)((Button)Instance.smallConfirmButton).onClick).AddListener(new UnityAction(Instance.StartCloseCoroutine));
		((UnityEvent)((Button)Instance.smallConfirmButton).onClick).AddListener(confirmAction);
		CameraView.AttenuateWorldForPopupFocus((IOverlayUser)(object)Instance);
		if (TPSingleton<GameManager>.Exist())
		{
			GameController.SetState(Game.E_State.BlockingPopup);
		}
		InputManager.OnGenericConsentViewToggled(state: true);
		Instance.InitJoystickNavigation();
		if (InputManager.IsLastControllerJoystick)
		{
			((MonoBehaviour)Instance).StartCoroutine(Instance.JoystickHighlightFollowTarget());
		}
		((MonoBehaviour)Instance).StartCoroutine(Instance.OpenThisFrameCoroutine());
		return Instance;
	}

	public void RefreshContent(bool shouldShowAsComplex)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		((Behaviour)overlay).enabled = !TPSingleton<ACameraView>.Exist();
		if (Text.key == null || Title.key == null)
		{
			return;
		}
		popupContainer.SetActive(((Behaviour)Canvas).enabled && !isSmallVersion);
		smallPopupContainer.SetActive(((Behaviour)Canvas).enabled && isSmallVersion);
		if (isSmallVersion)
		{
			((TMP_Text)smallTitleText).text = Localizer.Get(Title);
			((TMP_Text)smallCoreSimpleText).text = Localizer.Get(Text);
			smallCoreSimpleHyperlink.ForceRefresh();
		}
		else
		{
			((TMP_Text)titleText).text = Localizer.Get(Title);
			complexBox.SetActive(shouldShowAsComplex);
			simpleBox.SetActive(!shouldShowAsComplex);
			if (shouldShowAsComplex)
			{
				((TMP_Text)coreComplexText).text = Localizer.Get(Text);
				coreComplexHyperlink.ForceRefresh();
				if (TurnEndValidationManager.AnyPlayableUnitWaitingForLevelUp)
				{
					((Component)levelUpBlockingPopupLine).gameObject.SetActive(true);
					levelUpBlockingPopupLine.UpdateDisplayedText("GenericBlocking_FinishLevelUp", GetPlayableUnitsLevelingUpNames());
				}
				else
				{
					((Component)levelUpBlockingPopupLine).gameObject.SetActive(false);
				}
				if (TurnEndValidationManager.AnyProdItemsLeft)
				{
					((Component)rewardBlockingPopupLine).gameObject.SetActive(true);
					rewardBlockingPopupLine.UpdateDisplayedText("GenericBlocking_CollectReward");
				}
				else
				{
					((Component)rewardBlockingPopupLine).gameObject.SetActive(false);
				}
			}
			else
			{
				((TMP_Text)coreSimpleText).text = Localizer.Get(Text);
				coreSimpleHyperlink.ForceRefresh();
			}
		}
		if ((Object)(object)complexFontLocalizedParent != (Object)null)
		{
			complexFontLocalizedParent.TargetKey = (isSmallVersion ? "SmallContent" : "Content");
			((FontLocalizedParent)complexFontLocalizedParent).RefreshChilds();
		}
	}

	private List<string> GetPlayableUnitsLevelingUpNames()
	{
		List<string> list = new List<string>();
		for (int i = 0; i < TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count; i++)
		{
			PlayableUnit playableUnit = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i];
			if (playableUnit.LevelPoints > 0)
			{
				list.Add(playableUnit.PlayableUnitName);
			}
		}
		return list;
	}

	private void Awake()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Expected O, but got Unknown
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Expected O, but got Unknown
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		Instance = this;
		((UnityEvent)((Button)Instance.closeButton).onClick).AddListener(new UnityAction(StartCloseCoroutine));
		((UnityEvent)((Button)Instance.smallCloseButton).onClick).AddListener(new UnityAction(StartCloseCoroutine));
		((UnityEvent)((Button)Instance.confirmButton).onClick).AddListener(new UnityAction(StartCloseCoroutine));
		((UnityEvent)((Button)Instance.smallConfirmButton).onClick).AddListener(new UnityAction(StartCloseCoroutine));
		((UnityEvent)((Button)levelUpBlockingPopupLine.MainButton).onClick).AddListener((UnityAction)delegate
		{
			StartCloseCoroutine(attenuateOverlayBlur: false);
			TPSingleton<ToDoListView>.Instance.OnLevelUpButtonClick();
			EventSystem.current.SetSelectedGameObject((GameObject)null);
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: false);
		});
		((UnityEvent)((Button)rewardBlockingPopupLine.MainButton).onClick).AddListener((UnityAction)delegate
		{
			StartCloseCoroutine(attenuateOverlayBlur: false);
			TPSingleton<ProductionReportPanel>.Instance.Open();
		});
	}

	private void Close(bool attenuateOverlayBlur = true)
	{
		if (TPSingleton<GameManager>.Exist() && TPSingleton<GameManager>.Instance.Game.PreviousState != Game.E_State.BlockingPopup)
		{
			GameController.SetState(TPSingleton<GameManager>.Instance.Game.PreviousState);
		}
		InputManager.OnGenericConsentViewToggled(state: false);
		if (attenuateOverlayBlur)
		{
			CameraView.AttenuateWorldForPopupFocus(null);
		}
		((Behaviour)Instance.Canvas).enabled = false;
	}

	private IEnumerator CloseAtEndOfFrame(bool attenuateOverlayBlur = true)
	{
		yield return SharedYields.WaitForEndOfFrame;
		Close(attenuateOverlayBlur);
	}

	private void InitJoystickNavigation()
	{
		if (InputManager.IsLastControllerJoystick)
		{
			((Selectable)(object)levelUpBlockingPopupLine.MainButton).SetMode((Mode)4);
			((Selectable)(object)rewardBlockingPopupLine.MainButton).SetMode((Mode)4);
			if (((Component)levelUpBlockingPopupLine.MainButton).gameObject.activeInHierarchy && ((Component)rewardBlockingPopupLine.MainButton).gameObject.activeInHierarchy)
			{
				((Selectable)(object)levelUpBlockingPopupLine.MainButton).SetSelectOnDown((Selectable)(object)rewardBlockingPopupLine.MainButton);
				((Selectable)(object)rewardBlockingPopupLine.MainButton).SetSelectOnUp((Selectable)(object)levelUpBlockingPopupLine.MainButton);
			}
			EventSystem.current.SetSelectedGameObject(((Component)levelUpBlockingPopupLine.MainButton).gameObject.activeInHierarchy ? ((Component)levelUpBlockingPopupLine.MainButton).gameObject : ((Component)rewardBlockingPopupLine.MainButton).gameObject);
		}
	}

	private IEnumerator OpenThisFrameCoroutine()
	{
		openThisFrame = true;
		yield return SharedYields.WaitForEndOfFrame;
		openThisFrame = false;
	}

	private void StartCloseCoroutine()
	{
		StartCloseCoroutine(attenuateOverlayBlur: true);
	}

	private void StartCloseCoroutine(bool attenuateOverlayBlur)
	{
		((MonoBehaviour)this).StartCoroutine(CloseAtEndOfFrame(attenuateOverlayBlur));
	}

	private void Update()
	{
		if (IsWaitingForInput() && (InputManager.GetButtonDown(29) || InputManager.GetButtonDown(66) || InputManager.GetButtonDown(7) || InputManager.GetButtonDown(80)))
		{
			StartCloseCoroutine();
			EventSystem.current.SetSelectedGameObject((GameObject)null);
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: false);
		}
	}

	private IEnumerator JoystickHighlightFollowTarget()
	{
		TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: true);
		yield return SharedYields.WaitForSeconds(0.5f);
		TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: false);
	}
}
