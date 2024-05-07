using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Log;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Item.ItemRestriction;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Manager.Item;
using TheLastStand.Manager.Sound;
using TheLastStand.Model.Item.ItemRestriction;
using TheLastStand.Model.Tutorial;
using TheLastStand.View.Tutorial;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.WorldMap.ItemRestriction;

public class WeaponRestrictionsPanel : TPSingleton<WeaponRestrictionsPanel>
{
	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private BetterButton closeButton;

	[SerializeField]
	private TextMeshProUGUI weaponCategoriesWarningText;

	[SerializeField]
	private BetterToggle customModeToggle;

	[SerializeField]
	private TutorialPopup tutorialPopup;

	[SerializeField]
	protected Selectable selectableAfterTutorialClosed;

	[SerializeField]
	private WeaponFamiliesCountDisplay weaponFamiliesCountDisplay;

	[SerializeField]
	private List<WeaponRestrictionsCategoryPanel> weaponCategoryPanels;

	[SerializeField]
	private WeaponFamilyTooltip weaponFamilyTooltip;

	[SerializeField]
	private List<AudioClip> activateClips;

	[SerializeField]
	private List<AudioClip> deactivateClips;

	[SerializeField]
	private List<AudioClip> errorClips;

	[SerializeField]
	private AudioSource audioSourceTemplate;

	[SerializeField]
	[Min(1f)]
	private int audioSourcesCount = 3;

	private AudioSource[] audioSources;

	private int nextAudioSourceIndex;

	public bool AreCategoriesCorrectlyConfigured { get; private set; }

	public bool CanClosePanel => AreCategoriesCorrectlyConfigured;

	public ItemRestrictionCategoriesCollection CurrentRestrictionCategoriesCollection { get; private set; }

	public bool Displayed { get; protected set; }

	public bool OpeningOrClosing { get; private set; }

	public WeaponFamilyTooltip WeaponFamilyTooltip => weaponFamilyTooltip;

	public static event Action OnPanelClosed;

	public void Open()
	{
		if (!Displayed)
		{
			CheckIfMustCreateNewWeaponFamilyDisplay();
			Display(mustDisplay: true);
		}
	}

	public void Close()
	{
		if (Displayed && CanClosePanel)
		{
			DeactivateCustomModeIfNeeded();
			Display(mustDisplay: false);
			WeaponRestrictionsPanel.OnPanelClosed();
		}
	}

	public AudioSource GetNextAudioSource()
	{
		return audioSources[nextAudioSourceIndex++ % audioSources.Length];
	}

	public void PlayErrorClip()
	{
		SoundManager.PlayAudioClip(GetNextAudioSource(), TPHelpers.RandomElement<AudioClip>(errorClips));
	}

	public void OnWeaponFamilyDisplaySelectChanged(ItemRestrictionFamily itemRestrictionFamily)
	{
		RefreshWeaponCategoryPanel(itemRestrictionFamily.ItemFamilyDefinition.ItemCategory);
		RefreshContentRelatedToFamiliesSelection();
		PlayWeaponFamilySelectClip(itemRestrictionFamily.IsActive);
		if (weaponFamilyTooltip.Displayed)
		{
			weaponFamilyTooltip.Refresh();
		}
	}

	protected override void Awake()
	{
		base.Awake();
		CurrentRestrictionCategoriesCollection = TPSingleton<ItemRestrictionManager>.Instance.WeaponsRestrictionsCategories;
		((UnityEvent<bool>)(object)((Toggle)customModeToggle).onValueChanged).AddListener((UnityAction<bool>)ToggleCustomMode);
	}

	protected void Start()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		((UnityEvent)((Button)closeButton).onClick).AddListener(new UnityAction(Close));
		Init();
		InitAudio();
	}

	protected void OnDestroy()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		((UnityEvent)((Button)closeButton).onClick).RemoveListener(new UnityAction(Close));
		((UnityEvent<bool>)(object)((Toggle)customModeToggle).onValueChanged).RemoveListener((UnityAction<bool>)ToggleCustomMode);
	}

	private void CheckIfMustCreateNewWeaponFamilyDisplay()
	{
		for (int i = 0; i < weaponCategoryPanels.Count; i++)
		{
			weaponCategoryPanels[i].TryAddNewWeaponFamilyDisplays();
		}
	}

	private void DeactivateCustomModeIfNeeded()
	{
		if (!TPSingleton<ItemRestrictionManager>.Instance.WeaponsRestrictionsCategories.IsBoundlessModeRequired() && TPSingleton<ItemRestrictionManager>.Instance.WeaponsRestrictionsCategories.IsBoundlessModeActive)
		{
			TPSingleton<ItemRestrictionManager>.Instance.WeaponsRestrictionsCategories.IsBoundlessModeActive = false;
		}
	}

	private void Display(bool mustDisplay)
	{
		((Behaviour)canvas).enabled = mustDisplay;
		Displayed = mustDisplay;
		if (Displayed)
		{
			RefreshContent();
			RefreshJoystickNavigation();
			if (InputManager.IsLastControllerJoystick)
			{
				JoystickSelectPanel();
			}
			if ((Object)(object)tutorialPopup != (Object)null)
			{
				tutorialPopup.SetSelectableAfterClose(selectableAfterTutorialClosed);
			}
			TPSingleton<TutorialManager>.Instance.OnTrigger(E_TutorialTrigger.OnWeaponRestrictionsPanelOpen);
		}
		else
		{
			weaponFamilyTooltip.Hide();
			if (InputManager.IsLastControllerJoystick)
			{
				EventSystem.current.SetSelectedGameObject((GameObject)null);
			}
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: false);
		}
		WorldMapCameraView.CheckIfCanMoveCameraInExplorationState();
	}

	private void InitAudio()
	{
		activateClips.Shuffle();
		deactivateClips.Shuffle();
		audioSources = (AudioSource[])(object)new AudioSource[audioSourcesCount];
		audioSources[0] = audioSourceTemplate;
		for (int i = 1; i < audioSources.Length; i++)
		{
			AudioSource val = Object.Instantiate<AudioSource>(audioSourceTemplate, ((Component)audioSourceTemplate).transform.parent);
			audioSources[i] = val;
		}
	}

	private void Init()
	{
		weaponFamiliesCountDisplay.Init();
		List<ItemDefinition.E_Category> list = new List<ItemDefinition.E_Category>
		{
			ItemDefinition.E_Category.MeleeWeapon,
			ItemDefinition.E_Category.RangeWeapon,
			ItemDefinition.E_Category.MagicWeapon
		};
		for (int i = 0; i < weaponCategoryPanels.Count; i++)
		{
			if (i < list.Count)
			{
				weaponCategoryPanels[i].Init(list[i]);
			}
		}
	}

	private void PlayWeaponFamilySelectClip(bool selected)
	{
		AudioClip val = GetNextClip(selected ? activateClips : deactivateClips);
		if ((Object)(object)val == (Object)null)
		{
			CLoggerManager.Log((object)"Could not find a valid weapon family audio clip!", (LogType)3, (CLogLevel)1, true, "StaticLog", false);
		}
		else
		{
			SoundManager.PlayAudioClip(GetNextAudioSource(), val);
		}
		static AudioClip GetNextClip(List<AudioClip> list)
		{
			AudioClip val2 = list[0];
			int index = Random.Range(2, list.Count);
			list.RemoveAt(0);
			list.Insert(index, val2);
			return val2;
		}
	}

	private void RefreshContent()
	{
		((Toggle)customModeToggle).isOn = TPSingleton<ItemRestrictionManager>.Instance.WeaponsRestrictionsCategories.IsBoundlessModeActive;
		RefreshContentRelatedToFamiliesSelection();
		foreach (WeaponRestrictionsCategoryPanel weaponCategoryPanel in weaponCategoryPanels)
		{
			weaponCategoryPanel.Refresh();
		}
	}

	private void RefreshContentRelatedToFamiliesSelection()
	{
		AreCategoriesCorrectlyConfigured = CurrentRestrictionCategoriesCollection.AreAllCategoriesCorrectlyConfigured();
		RefreshWeaponFamiliesCountDisplay();
		((Selectable)closeButton).interactable = AreCategoriesCorrectlyConfigured;
		((Behaviour)weaponCategoriesWarningText).enabled = !AreCategoriesCorrectlyConfigured;
		if (!AreCategoriesCorrectlyConfigured)
		{
			RefreshWeaponCategoriesWarningText();
		}
	}

	private void RefreshWeaponFamiliesCountDisplay()
	{
		weaponFamiliesCountDisplay.Refresh();
	}

	private void RefreshWeaponCategoryPanel(ItemDefinition.E_Category category)
	{
		foreach (WeaponRestrictionsCategoryPanel weaponCategoryPanel in weaponCategoryPanels)
		{
			if (category == ItemDefinition.E_Category.All || category == weaponCategoryPanel.CurrentItemCategory)
			{
				weaponCategoryPanel.Refresh();
			}
		}
	}

	private void RefreshWeaponCategoriesWarningText()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (ItemRestrictionCategoryDefinition itemCategoryDefinition in CurrentRestrictionCategoriesCollection.ItemCategoriesCollectionDefinition.itemCategoryDefinitions)
		{
			if (!CurrentRestrictionCategoriesCollection.IsCategoryCorrectlyConfigured(itemCategoryDefinition.ItemCategory))
			{
				int requiredSelectedFamiliesNb = CurrentRestrictionCategoriesCollection.GetRequiredSelectedFamiliesNb(itemCategoryDefinition.ItemCategory, itemCategoryDefinition);
				stringBuilder.Append(Localizer.Format("WeaponRestrictionsPanel_CategoryWarning_" + itemCategoryDefinition.ItemCategory, new object[1] { requiredSelectedFamiliesNb })).AppendLine();
			}
		}
		((TMP_Text)weaponCategoriesWarningText).text = stringBuilder.ToString();
	}

	private void ToggleCustomMode(bool toggle)
	{
		TPSingleton<ItemRestrictionManager>.Instance.WeaponsRestrictionsCategories.IsBoundlessModeActive = toggle;
		RefreshContent();
	}

	private void RefreshJoystickNavigation()
	{
		((Selectable)(object)customModeToggle).SetMode((Mode)4);
		((Selectable)(object)customModeToggle).ClearNavigation();
		((Selectable)(object)customModeToggle).SetSelectOnDown((Selectable)(object)GetFirstWeaponFamilyDisplay().JoystickSelectable);
		RefreshWeaponFamilyDisplaysJoystickNavigation();
	}

	private void RefreshWeaponFamilyDisplaysJoystickNavigation()
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		int count = weaponCategoryPanels.Count;
		int num = 0;
		for (num = 0; num < count; num++)
		{
			List<WeaponFamilyDisplay> weaponFamilyDisplays = weaponCategoryPanels[num].WeaponFamilyDisplays;
			int count2 = weaponFamilyDisplays.Count;
			for (int i = 0; i < count2; i++)
			{
				int rowIndex = i / weaponCategoryPanels[num].GridLayoutGroup.constraintCount;
				Navigation navigation = ((Selectable)weaponFamilyDisplays[i].JoystickSelectable).navigation;
				if ((Object)(object)((Navigation)(ref navigation)).selectOnUp == (Object)null)
				{
					((Selectable)(object)weaponFamilyDisplays[i].JoystickSelectable).SetSelectOnUp((Selectable)(object)customModeToggle);
				}
				navigation = ((Selectable)weaponFamilyDisplays[i].JoystickSelectable).navigation;
				if ((Object)(object)((Navigation)(ref navigation)).selectOnLeft == (Object)null && num - 1 >= 0)
				{
					((Selectable)(object)weaponFamilyDisplays[i].JoystickSelectable).SetSelectOnLeft((Selectable)(object)weaponCategoryPanels[num - 1].GetClosestWeaponFamilyDisplayFromRowIndex(rowIndex, getClosestFromLeft: false)?.JoystickSelectable);
				}
				navigation = ((Selectable)weaponFamilyDisplays[i].JoystickSelectable).navigation;
				if ((Object)(object)((Navigation)(ref navigation)).selectOnRight == (Object)null && num + 1 < count)
				{
					((Selectable)(object)weaponFamilyDisplays[i].JoystickSelectable).SetSelectOnRight((Selectable)(object)weaponCategoryPanels[num + 1].GetClosestWeaponFamilyDisplayFromRowIndex(rowIndex, getClosestFromLeft: true)?.JoystickSelectable);
				}
			}
		}
	}

	private void JoystickSelectPanel()
	{
		WeaponFamilyDisplay firstWeaponFamilyDisplay = GetFirstWeaponFamilyDisplay();
		GameObject val = null;
		if ((Object)(object)firstWeaponFamilyDisplay != (Object)null)
		{
			val = ((Component)firstWeaponFamilyDisplay).gameObject;
			selectableAfterTutorialClosed = (Selectable)(object)firstWeaponFamilyDisplay.JoystickSelectable;
		}
		else
		{
			val = ((Component)customModeToggle).gameObject;
			selectableAfterTutorialClosed = (Selectable)(object)customModeToggle;
		}
		EventSystem.current.SetSelectedGameObject(val);
	}

	private WeaponFamilyDisplay GetFirstWeaponFamilyDisplay()
	{
		foreach (WeaponRestrictionsCategoryPanel weaponCategoryPanel in weaponCategoryPanels)
		{
			WeaponFamilyDisplay weaponFamilyDisplay = weaponCategoryPanel.WeaponFamilyDisplays.FirstOrDefault((WeaponFamilyDisplay o) => ((Component)o).gameObject.activeSelf);
			if ((Object)(object)weaponFamilyDisplay != (Object)null)
			{
				return weaponFamilyDisplay;
			}
		}
		return null;
	}

	static WeaponRestrictionsPanel()
	{
		WeaponRestrictionsPanel.OnPanelClosed = delegate
		{
		};
	}
}
