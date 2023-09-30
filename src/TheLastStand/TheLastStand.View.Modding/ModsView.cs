using System.Collections.Generic;
using Rewired;
using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Localization.Fonts;
using TPLib.UI;
using TheLastStand.Framework.Automaton;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Manager.Modding;
using TheLastStand.Model.Modding;
using TheLastStand.View.Camera;
using TheLastStand.View.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.Modding;

public class ModsView : TPSingleton<ModsView>, IOverlayUser
{
	public static class Constants
	{
		public const string PopupLocalizedFontChildren = "Popup";

		public const string ModsListLocalizedFontChildren = "ModsList";
	}

	[SerializeField]
	private ComplexFontLocalizedParent complexFontLocalizedParent;

	[SerializeField]
	private Canvas windowCanvas;

	[SerializeField]
	private BetterButton openWindowButton;

	[SerializeField]
	private BetterButton closeWindowButton;

	[SerializeField]
	private ModItemView modItemViewPrefab;

	[SerializeField]
	private Transform modItemParent;

	[SerializeField]
	private RawTextTooltip rawTextTooltip;

	[SerializeField]
	private GenericTooltip warningTooltip;

	[SerializeField]
	private GameObject blocker;

	[SerializeField]
	private TextMeshProUGUI moddingVersionsLabel;

	[SerializeField]
	private Canvas changeNotePopupCanvas;

	[SerializeField]
	private TMP_InputField changeNoteInputField;

	[SerializeField]
	private BetterButton cancelChangeNoteButton;

	[SerializeField]
	private BetterButton submitChangeNoteButton;

	public bool IsUploadingMod;

	private readonly List<ModItemView> modItemViews = new List<ModItemView>();

	private bool populated;

	private bool previousDeveloperMode;

	public int OverlaySortingOrder => windowCanvas.sortingOrder - 1;

	public RawTextTooltip RawTextTooltip => rawTextTooltip;

	public GenericTooltip WarningTooltip => warningTooltip;

	public BetterButton SubmitChangeNoteButton => submitChangeNoteButton;

	public void LockRaycasts()
	{
		blocker.SetActive(true);
	}

	public void Open()
	{
		modItemViews.ForEach(delegate(ModItemView x)
		{
			x.Refresh();
		});
		Refresh();
		((Behaviour)windowCanvas).enabled = true;
		CameraView.AttenuateWorldForPopupFocus((IOverlayUser)(object)this);
	}

	public void OpenChangeNotePopup()
	{
		((Behaviour)changeNotePopupCanvas).enabled = true;
		changeNoteInputField.text = string.Empty;
		complexFontLocalizedParent.TargetKey = "Popup";
		((FontLocalizedParent)complexFontLocalizedParent).RefreshChilds();
	}

	public void UnlockRaycasts()
	{
		blocker.SetActive(false);
	}

	public string GetChangeNoteValue()
	{
		if (!(changeNoteInputField.text != string.Empty))
		{
			return null;
		}
		return changeNoteInputField.text;
	}

	private void Close()
	{
		ApplicationManager.Application.ApplicationController.BackToPreviousState();
		((Behaviour)windowCanvas).enabled = false;
		CameraView.AttenuateWorldForPopupFocus(null);
	}

	private void CloseChangeNotePopup()
	{
		((Behaviour)changeNotePopupCanvas).enabled = false;
	}

	private void OnDestroy()
	{
		if (TPSingleton<InputManager>.Exist())
		{
			TPSingleton<InputManager>.Instance.LastActiveControllerChanged -= OnLastActiveControllerChanged;
		}
	}

	private void OnLastActiveControllerChanged(ControllerType controllerType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		if ((int)controllerType > 1)
		{
			if ((int)controllerType == 2)
			{
				((Component)openWindowButton).gameObject.SetActive(false);
				return;
			}
			if ((int)controllerType == 20)
			{
			}
		}
		((Component)openWindowButton).gameObject.SetActive(ShouldEnableOpenButton());
	}

	private void PopulateWindow(List<Mod> subscribedMods, List<Mod> outdatedMods)
	{
		if (!populated)
		{
			for (int i = 0; i < subscribedMods.Count; i++)
			{
				ModItemView modItemView = Object.Instantiate<ModItemView>(modItemViewPrefab, modItemParent);
				((Object)((Component)modItemView).gameObject).name = "Mod Item " + subscribedMods[i].Title;
				modItemView.Refresh(subscribedMods[i]);
				modItemViews.Add(modItemView);
			}
			for (int j = 0; j < outdatedMods.Count; j++)
			{
				ModItemView modItemView2 = Object.Instantiate<ModItemView>(modItemViewPrefab, modItemParent);
				((Object)((Component)modItemView2).gameObject).name = "Mod Item " + outdatedMods[j].Title;
				modItemView2.Refresh(outdatedMods[j]);
				modItemViews.Add(modItemView2);
			}
			populated = true;
		}
	}

	private void Refresh()
	{
		((TMP_Text)moddingVersionsLabel).text = Localizer.Format("Modding_Versions_Label", new object[2]
		{
			ModManager.ModVersion,
			ModManager.ModMinVersion
		});
		complexFontLocalizedParent.TargetKey = "ModsList";
		((FontLocalizedParent)complexFontLocalizedParent).RefreshChilds();
	}

	private bool ShouldEnableOpenButton()
	{
		if (TPSingleton<ModManager>.Exist())
		{
			if (ModManager.SubscribedMods.Count <= 0)
			{
				return ModManager.OutdatedMods.Count > 0;
			}
			return true;
		}
		return false;
	}

	private void Start()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Expected O, but got Unknown
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Expected O, but got Unknown
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		TPSingleton<InputManager>.Instance.LastActiveControllerChanged += OnLastActiveControllerChanged;
		if (ShouldEnableOpenButton())
		{
			PopulateWindow(ModManager.SubscribedMods, ModManager.OutdatedMods);
			((UnityEvent)((Button)openWindowButton).onClick).AddListener(new UnityAction(OnOpenWindowButtonClicked));
			((UnityEvent)((Button)closeWindowButton).onClick).AddListener(new UnityAction(OnCloseWindowButtonClicked));
			((UnityEvent)((Button)cancelChangeNoteButton).onClick).AddListener(new UnityAction(CloseChangeNotePopup));
			((UnityEvent)((Button)submitChangeNoteButton).onClick).AddListener(new UnityAction(CloseChangeNotePopup));
		}
		else
		{
			((Component)openWindowButton).gameObject.SetActive(false);
		}
	}

	private void OnCloseWindowButtonClicked()
	{
		Close();
	}

	private void OnOpenWindowButtonClicked()
	{
		ApplicationManager.Application.ApplicationController.SetState("ModList");
	}

	private void Update()
	{
		if (((StateMachine)ApplicationManager.Application).State.GetName() != "ModList")
		{
			return;
		}
		if (InputManager.GetButtonDown(23) && !IsUploadingMod && !GenericPopUp.IsOpen)
		{
			Close();
		}
		if (previousDeveloperMode != ModManager.IsModder)
		{
			previousDeveloperMode = ModManager.IsModder;
			modItemViews.ForEach(delegate(ModItemView x)
			{
				x.Refresh();
			});
		}
	}
}
