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
using TPLib.Log;
using TPLib.UI;
using TPLib.Yield;
using TheLastStand.Controller.ProductionReport;
using TheLastStand.Database;
using TheLastStand.Definition.Item;
using TheLastStand.Framework;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.UI;
using TheLastStand.Framework.UI.TMPro;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Item;
using TheLastStand.Manager.Sound;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Item;
using TheLastStand.Model.ProductionReport;
using TheLastStand.View.Camera;
using TheLastStand.View.CharacterSheet;
using TheLastStand.View.Generic;
using TheLastStand.View.HUD;
using TheLastStand.View.NightReport;
using TheLastStand.View.Unit.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.ProductionReport;

public class ChooseRewardPanel : TPSingleton<ChooseRewardPanel>, IOverlayUser
{
	private static class Constants
	{
		public const string AppearAudioClipsFolderPath = "Sounds/SFX/UI_Reroll/UI_Reroll_Reward/Appear";

		public const string DisappearAudioClipsFolderPath = "Sounds/SFX/UI_Reroll/UI_Reroll_Reward/Disappear";

		public const string SelectAudioClipsFolderPath = "Sounds/SFX/UI_Reroll/UI_Reroll_Reward/Select";
	}

	[SerializeField]
	[Range(0f, 3f)]
	private float delayBetweenItemApparition = 0.5f;

	[SerializeField]
	[Range(0f, 3f)]
	private float fadeInTweenDuration = 0.4f;

	[SerializeField]
	private Ease fadeInTweenEasing = (Ease)9;

	[SerializeField]
	[Range(0f, 3f)]
	private float fadeOutTweenDuration = 0.4f;

	[SerializeField]
	private Ease fadeOutTweenEasing = (Ease)9;

	[SerializeField]
	private BetterButton confirmButton;

	[SerializeField]
	private ChooseRewardShelf chooseRewardShelf;

	[SerializeField]
	private RectMask2D shelvesMask;

	[SerializeField]
	private RectTransform shelvesContainer;

	[SerializeField]
	private TextMeshProUGUI productionBuildingText;

	[SerializeField]
	private GenericTooltipDisplayer tooltipDisplayer;

	[SerializeField]
	private UnitDropdownPanel unitDropdownPanel;

	[SerializeField]
	private TMP_BetterDropdown unitDropdown;

	[SerializeField]
	private Button previousUnitButton;

	[SerializeField]
	private Button nextUnitButton;

	[SerializeField]
	private BetterButton rerollButton;

	[SerializeField]
	private TextMeshProUGUI remainingRerollText;

	[SerializeField]
	private AudioSource chooseRewardAudioSource;

	[SerializeField]
	private AudioClip openAudioClip;

	[SerializeField]
	private DataColor validRemainingRerollColor;

	[SerializeField]
	private DataColor invalidRemainingRerollColor;

	[SerializeField]
	private SimpleFontLocalizedParent simpleFontLocalizedParent;

	[SerializeField]
	private HUDJoystickTarget joystickTarget;

	private Canvas canvas;

	private CanvasGroup canvasGroup;

	private List<ChooseRewardShelf> chooseRewardShelves = new List<ChooseRewardShelf>();

	private Tween fadeTween;

	private bool isItemChanging;

	private Coroutine rerollCoroutine;

	private Coroutine selectPanelCoroutine;

	private bool firstFrameOpened = true;

	private AudioClip[] appearAudioClips;

	private AudioClip[] disappearAudioClips;

	private AudioClip[] selectAudioClips;

	public static AudioSource ChooseRewardAudioSource => TPSingleton<ChooseRewardPanel>.Instance.chooseRewardAudioSource;

	public bool IsOpened { get; set; }

	public int OverlaySortingOrder => canvas.sortingOrder - 1;

	public ProductionItems ProductionItem { get; set; }

	public int UnitToCompareIndex { get; private set; } = -1;


	public HUDJoystickTarget JoystickTarget => joystickTarget;

	public event Action<bool> OnRewardPanelToggle;

	public void ChangeUnitToCompare(int newUnitIndex)
	{
		UnitToCompareIndex = newUnitIndex;
		if (newUnitIndex == -1)
		{
			TileObjectSelectionManager.DeselectUnit();
		}
		else
		{
			PlayableUnitManager.SelectUnitAtIndex(newUnitIndex);
		}
	}

	public void ChangeUnitToCompareAndResetDropdown(int newUnitIndex)
	{
		ChangeUnitToCompare(newUnitIndex);
		unitDropdownPanel.ResetDropdown(newUnitIndex + 1);
		if (ProductionItem == null)
		{
			return;
		}
		for (int i = 0; i < ProductionItem.Items.Count; i++)
		{
			if (chooseRewardShelves[i].RewardItemSlotView.HasFocus)
			{
				chooseRewardShelves[i].RewardItemSlotView.DisplayTooltip(display: true);
			}
		}
	}

	public void Close()
	{
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Expected O, but got Unknown
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Expected O, but got Unknown
		if (!IsOpened)
		{
			CLoggerManager.Log((object)"ChooseRewardPanel is already closed", (LogType)0, (CLogLevel)0, true, "StaticLog", false);
			return;
		}
		IsOpened = false;
		if (selectPanelCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(selectPanelCoroutine);
		}
		this.OnRewardPanelToggle?.Invoke(obj: false);
		if (rerollCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(rerollCoroutine);
			rerollCoroutine = null;
		}
		isItemChanging = false;
		switch (TPSingleton<GameManager>.Instance.Game.State)
		{
		case Game.E_State.ProductionReport:
			CameraView.AttenuateWorldForPopupFocus((IOverlayUser)(object)TPSingleton<ProductionReportPanel>.Instance);
			break;
		case Game.E_State.NightReport:
			CameraView.AttenuateWorldForPopupFocus((IOverlayUser)(object)TPSingleton<NightReportPanel>.Instance);
			break;
		default:
			CameraView.AttenuateWorldForPopupFocus(null);
			break;
		}
		InventoryManager.InventoryView.ItemTooltip.Hide();
		CharacterSheetPanel.ItemTooltip.Hide();
		fadeTween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTweenModuleUI.DOFade(canvasGroup, 0f, fadeOutTweenDuration), fadeOutTweenEasing).SetFullId<TweenerCore<float, float, FloatOptions>>("ShopFadeOut", (Component)(object)this), (TweenCallback)delegate
		{
			((Behaviour)canvas).enabled = false;
		});
		canvasGroup.blocksRaycasts = false;
		TweenSettingsExtensions.OnComplete<Tween>(fadeTween, (TweenCallback)delegate
		{
			ToggleShelvesMask(toggle: false);
		});
		if (InputManager.IsLastControllerJoystick && !TPSingleton<ProductionReportPanel>.Instance.SelectFirstProduct())
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.ExitHUDNavigationMode();
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: false);
		}
	}

	public void OnChosenItemChanged()
	{
		confirmButton.Interactable = ProductionItem.ChosenItem != null;
		if (ProductionItem.ChosenItem != null)
		{
			SoundManager.PlayAudioClip(selectAudioClips.PickRandom());
		}
		int i = 0;
		for (int count = chooseRewardShelves.Count; i < count; i++)
		{
			chooseRewardShelves[i].DisplaySelection();
		}
	}

	public void OnConfirm()
	{
		if (ProductionItem.ChosenItem != null)
		{
			ProductionItem.ProductionObjectController.ObtainContent();
			TPSingleton<BuildingManager>.Instance.ProductionReport.ProductionReportController.RemoveProductionObject(ProductionItem);
			TPSingleton<BuildingManager>.Instance.ProductionReport.ProductionReportPanel.RefreshGameObjects();
			TPSingleton<BuildingManager>.Instance.ProductionReport.ProductionReportPanel.CheckOnProductionObjectHide();
			TPSingleton<ChooseRewardPanel>.Instance.Close();
		}
	}

	public void OnRerollButtonClick()
	{
		if (!isItemChanging)
		{
			rerollCoroutine = ((MonoBehaviour)this).StartCoroutine(ReloadRerollRewardShelfs());
		}
	}

	public void Open()
	{
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		if (IsOpened)
		{
			CLoggerManager.Log((object)"ChooseRewardPanel is already opended", (LogType)0, (CLogLevel)0, true, "StaticLog", false);
			return;
		}
		firstFrameOpened = true;
		CameraView.AttenuateWorldForPopupFocus((IOverlayUser)(object)this);
		Tween obj = TPSingleton<ChooseRewardPanel>.Instance.fadeTween;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
		SoundManager.PlayAudioClip(chooseRewardAudioSource, openAudioClip);
		IsOpened = true;
		this.OnRewardPanelToggle?.Invoke(obj: true);
		SimpleFontLocalizedParent obj2 = simpleFontLocalizedParent;
		if (obj2 != null)
		{
			((FontLocalizedParent)obj2).RefreshChilds();
		}
		int num = PanicManager.Panic.PanicReward.PanicRewardController.ReloadBaseNbRerollReward();
		Refresh();
		ToggleShelvesMask(toggle: true);
		tooltipDisplayer.LocalizationArguments = new object[1] { num };
		((Behaviour)canvas).enabled = true;
		canvasGroup.blocksRaycasts = true;
		TPSingleton<ChooseRewardPanel>.Instance.fadeTween = (Tween)(object)TweenSettingsExtensions.SetEase<TweenerCore<float, float, FloatOptions>>(DOTweenModuleUI.DOFade(TPSingleton<ChooseRewardPanel>.Instance.canvasGroup, 1f, fadeInTweenDuration), fadeInTweenEasing).SetFullId<TweenerCore<float, float, FloatOptions>>("ShopFadeIn", (Component)(object)this);
		if (!TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			unitDropdownPanel.ResetDropdown();
		}
		else
		{
			int num2 = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.IndexOf(TileObjectSelectionManager.SelectedPlayableUnit);
			unitDropdownPanel.ResetDropdown(num2 + 1);
		}
		if (InputManager.IsLastControllerJoystick)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.OpenHUDNavigationMode(selectDefaultPanel: false);
			TPSingleton<HUDJoystickNavigationManager>.Instance.ExitHUDNavigationMode();
			selectPanelCoroutine = ((MonoBehaviour)this).StartCoroutine(SelectPanelCoroutine());
		}
	}

	private bool IsAnyRewardJoystickSelected()
	{
		foreach (ChooseRewardShelf chooseRewardShelf in chooseRewardShelves)
		{
			if ((Object)(object)EventSystem.current.currentSelectedGameObject == (Object)(object)((Component)chooseRewardShelf.RewardItemSlotView.JoystickSelectable).gameObject)
			{
				return true;
			}
		}
		return false;
	}

	private IEnumerator SelectPanelCoroutine()
	{
		float num = ((chooseRewardShelves.Count > 0) ? chooseRewardShelves[0].AppearTweenDuration : 0f);
		yield return (object)new WaitForSeconds(num + fadeInTweenDuration + (float)ProductionItem.Items.Count * delayBetweenItemApparition);
		if (InputManager.IsLastControllerJoystick)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(JoystickTarget.GetSelectionInfo());
			EventSystem.current.SetSelectedGameObject(((Component)chooseRewardShelves[0].RewardItemSlotView).gameObject);
		}
	}

	private Navigation AddRewardToNavigation(Navigation navigation)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		Navigation result = default(Navigation);
		((Navigation)(ref result)).mode = (Mode)4;
		((Navigation)(ref result)).selectOnUp = chooseRewardShelves[0].RewardItemSlotView.Selectable;
		((Navigation)(ref result)).selectOnRight = ((Navigation)(ref navigation)).selectOnRight;
		((Navigation)(ref result)).selectOnLeft = ((Navigation)(ref navigation)).selectOnLeft;
		return result;
	}

	private void GetNewRewardItem()
	{
		if (ProductionItem.IsNightProduction)
		{
			PanicManager.Panic.PanicReward.PanicRewardController.GetReward();
			if (PanicManager.Panic.PanicReward.HasAtLeastOneItem)
			{
				ProductionNightRewardObject productionNightRewardObject = new ProductionNightRewardObjectController(PanicManager.Panic.PanicReward.Items).ProductionNightRewardObject;
				ProductionItem.Items.Clear();
				{
					foreach (TheLastStand.Model.Item.Item item2 in productionNightRewardObject.Items)
					{
						ProductionItem.Items.Add(item2);
					}
					return;
				}
			}
			((CLogger<PanicManager>)TPSingleton<PanicManager>.Instance).LogError((object)"No reward found after the RerollReward action !", (CLogLevel)1, true, true);
		}
		else
		{
			ProductionItem.Items.Clear();
			for (int i = 0; i < chooseRewardShelves.Count; i++)
			{
				TheLastStand.Model.Item.Item item = ItemManager.GenerateItem(ItemSlotDefinition.E_ItemSlotId.None, ProductionItem.CreateItemDefinition, ProductionItem.LevelProbabilitiesTreeController.GenerateLevel());
				ProductionItem.Items.Add(item);
			}
		}
	}

	private void Refresh()
	{
		if (ProductionItem == null)
		{
			Close();
			return;
		}
		RefreshText();
		RefreshRerollButton();
		RefreshShelvesQuantity(ProductionItem.Items.Count);
		ProductionItem.ChosenItem = null;
		OnChosenItemChanged();
		for (int i = 0; i < ProductionItem.Items.Count; i++)
		{
			AudioClip appearClip = ((i == 0) ? appearAudioClips[0] : appearAudioClips[(i + 1) % 2 + 1]);
			chooseRewardShelves[i].ItemIndex = i;
			chooseRewardShelves[i].Refresh();
			chooseRewardShelves[i].Appear(fadeInTweenDuration + (float)i * delayBetweenItemApparition, appearClip);
		}
		SoundManager.PlayAudioClip(appearAudioClips[3], null, fadeInTweenDuration + (float)ProductionItem.Items.Count * delayBetweenItemApparition);
		confirmButton.Interactable = ProductionItem.ChosenItem != null;
	}

	private void RefreshRerollButton()
	{
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		((Component)rerollButton).gameObject.SetActive(PanicManager.Panic.PanicReward.BaseNbRerollReward != 0);
		bool flag = PanicManager.Panic.PanicReward.RemainingNbRerollReward > 0;
		((Selectable)rerollButton).interactable = flag;
		((TMP_Text)remainingRerollText).text = $"x{PanicManager.Panic.PanicReward.RemainingNbRerollReward}";
		((Graphic)remainingRerollText).color = (flag ? validRemainingRerollColor._Color : invalidRemainingRerollColor._Color);
	}

	private IEnumerator ReloadRerollRewardShelfs()
	{
		isItemChanging = true;
		PanicManager.Panic.PanicReward.RemainingNbRerollReward--;
		GetNewRewardItem();
		RefreshRerollButton();
		if (ProductionItem.ChosenItem != null)
		{
			ProductionItem.ChosenItem = null;
			OnChosenItemChanged();
			yield return SharedYields.WaitForSeconds(delayBetweenItemApparition);
		}
		for (int i = 0; i < ProductionItem.Items.Count; i++)
		{
			AudioClip disappearClip = ((i == 0) ? disappearAudioClips[0] : disappearAudioClips[(i + 1) % 2 + 1]);
			chooseRewardShelves[i].Disappear((float)i * delayBetweenItemApparition, disappearClip);
		}
		if (InputManager.IsLastControllerJoystick)
		{
			EventSystem.current.SetSelectedGameObject((GameObject)null);
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: false);
		}
		yield return SharedYields.WaitForSeconds((float)ProductionItem.Items.Count * delayBetweenItemApparition);
		for (int j = 0; j < ProductionItem.Items.Count; j++)
		{
			AudioClip appearClip = ((j == 0) ? appearAudioClips[0] : appearAudioClips[(j + 1) % 2 + 1]);
			chooseRewardShelves[j].ItemIndex = j;
			chooseRewardShelves[j].Reload();
			chooseRewardShelves[j].Refresh();
			chooseRewardShelves[j].Appear((float)j * delayBetweenItemApparition, appearClip);
		}
		yield return SharedYields.WaitForSeconds((float)ProductionItem.Items.Count * delayBetweenItemApparition);
		SoundManager.PlayAudioClip(appearAudioClips[3]);
		if (InputManager.IsLastControllerJoystick)
		{
			EventSystem.current.SetSelectedGameObject(((Component)chooseRewardShelves[0].RewardItemSlotView).gameObject);
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: true);
		}
		isItemChanging = false;
	}

	private void RefreshShelvesQuantity(int shelvesCount)
	{
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		while (chooseRewardShelves.Count > shelvesCount)
		{
			Object.Destroy((Object)(object)((Component)chooseRewardShelves[^1]).gameObject);
			chooseRewardShelves.RemoveAt(chooseRewardShelves.Count - 1);
		}
		while (chooseRewardShelves.Count < shelvesCount)
		{
			chooseRewardShelves.Add(Object.Instantiate<ChooseRewardShelf>(chooseRewardShelf, (Transform)(object)shelvesContainer));
		}
		HUDJoystickSimpleTarget hUDJoystickSimpleTarget = JoystickTarget as HUDJoystickSimpleTarget;
		if ((Object)(object)hUDJoystickSimpleTarget != (Object)null)
		{
			hUDJoystickSimpleTarget.ClearSelectables();
			for (int i = 0; i < chooseRewardShelves.Count; i++)
			{
				hUDJoystickSimpleTarget.AddSelectable(chooseRewardShelves[i].RewardItemSlotView.Selectable);
				Navigation val = default(Navigation);
				((Navigation)(ref val)).mode = (Mode)4;
				Navigation navigation = val;
				if (i < chooseRewardShelves.Count - 1)
				{
					((Navigation)(ref navigation)).selectOnRight = chooseRewardShelves[i + 1].RewardItemSlotView.Selectable;
				}
				if (i > 0)
				{
					((Navigation)(ref navigation)).selectOnLeft = chooseRewardShelves[i - 1].RewardItemSlotView.Selectable;
				}
				((Navigation)(ref navigation)).selectOnDown = (Selectable)(object)unitDropdown;
				chooseRewardShelves[i].RewardItemSlotView.Selectable.navigation = navigation;
			}
		}
		((Selectable)unitDropdown).navigation = AddRewardToNavigation(((Selectable)unitDropdown).navigation);
		((Selectable)previousUnitButton).navigation = AddRewardToNavigation(((Selectable)previousUnitButton).navigation);
		((Selectable)nextUnitButton).navigation = AddRewardToNavigation(((Selectable)nextUnitButton).navigation);
	}

	private void ToggleShelvesMask(bool toggle)
	{
		((Behaviour)shelvesMask).enabled = toggle;
	}

	private void Update()
	{
		if (IsOpened)
		{
			if (firstFrameOpened)
			{
				firstFrameOpened = false;
			}
			else if (InputManager.GetButtonDown(29) || InputManager.GetButtonDown(80))
			{
				Close();
			}
			else if (InputManager.GetButtonDown(79) && IsAnyRewardJoystickSelected())
			{
				OnConfirm();
			}
			else if (InputManager.GetButtonDown(102) && PanicManager.Panic.PanicReward.RemainingNbRerollReward > 0)
			{
				OnRerollButtonClick();
			}
		}
	}

	protected override void Awake()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		base.Awake();
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Combine((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
		canvas = ((Component)TPSingleton<ChooseRewardPanel>.Instance).GetComponent<Canvas>();
		((Behaviour)canvas).enabled = false;
		canvasGroup = ((Component)TPSingleton<ChooseRewardPanel>.Instance).GetComponent<CanvasGroup>();
		canvasGroup.blocksRaycasts = false;
		TPSingleton<ChooseRewardPanel>.Instance.appearAudioClips = ResourcePooler.LoadAllOnce<AudioClip>("Sounds/SFX/UI_Reroll/UI_Reroll_Reward/Appear", failSilently: false);
		TPSingleton<ChooseRewardPanel>.Instance.disappearAudioClips = ResourcePooler.LoadAllOnce<AudioClip>("Sounds/SFX/UI_Reroll/UI_Reroll_Reward/Disappear", failSilently: false);
		TPSingleton<ChooseRewardPanel>.Instance.selectAudioClips = ResourcePooler.LoadAllOnce<AudioClip>("Sounds/SFX/UI_Reroll/UI_Reroll_Reward/Select", failSilently: false);
		unitDropdownPanel.OnUnitToCompareChanged += ChangeUnitToCompare;
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

	private void RefreshText()
	{
		if (ProductionItem != null)
		{
			((TMP_Text)productionBuildingText).text = ((ProductionItem.ProductionBuildingDefinition != null) ? ProductionItem.ProductionBuildingDefinition.Name : Localizer.Get("NightReportPanel_NightRewardObject"));
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
			if (TPSingleton<ChooseRewardPanel>.Instance.IsOpened)
			{
				return;
			}
			if (ProductionItem == null)
			{
				ProductionItem = new ProductionItemController().ProductionItem;
				List<string> list = new List<string>(ItemDatabase.ItemDefinitions.Keys);
				for (int i = 0; i < chooseRewardShelves.Count; i++)
				{
					ItemManager.ItemGenerationInfo generationInfo = default(ItemManager.ItemGenerationInfo);
					generationInfo.ItemDefinition = ItemDatabase.ItemDefinitions[list[RandomManager.GetRandomRange(this, 0, list.Count)]];
					generationInfo.Rarity = ItemDefinition.E_Rarity.Common;
					TheLastStand.Model.Item.Item item = ItemManager.GenerateItem(generationInfo);
					ProductionItem.Items.Add(item);
				}
			}
			TPSingleton<ChooseRewardPanel>.Instance.Open();
		}
	}

	[ContextMenu("Close")]
	public void DebugClose()
	{
		if (!Application.isPlaying)
		{
			Debug.LogError((object)"Unable to use this context menu when the application is not running");
		}
		else if (TPSingleton<ChooseRewardPanel>.Instance.IsOpened)
		{
			Close();
		}
	}
}
