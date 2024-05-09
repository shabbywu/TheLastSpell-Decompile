using System.Collections.Generic;
using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Log;
using TheLastStand.Framework;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Tutorial;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.View.CharacterSheet;
using TheLastStand.View.HUD;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.Unit.Perk;

public class UnitPerkTreeView : TabbedPageView
{
	public static class Constants
	{
		private const string CollectionAssetsPathFormat = "View/Sprites/UI/CharacterSheet/PerkTree/{0}/";

		private const string BotAssetPathFormat = "View/Sprites/UI/CharacterSheet/PerkTree/{0}/Collection_{0}_Bot";

		private const string CenterAssetPathFormat = "View/Sprites/UI/CharacterSheet/PerkTree/{0}/Collection_{0}_Center";

		private const string CrestAssetPathFormat = "View/Sprites/UI/CharacterSheet/PerkTree/{0}/Crest_{0}";

		private const string OffSuffix = "_Off";

		private const string OnSuffix = "_On";

		public const string TopAssetPathFormat = "View/Sprites/UI/CharacterSheet/PerkTree/{0}/Collection_{0}_Top";

		public const string BotOffAssetPathFormat = "View/Sprites/UI/CharacterSheet/PerkTree/{0}/Collection_{0}_Bot_Off";

		public const string BotOnAssetPathFormat = "View/Sprites/UI/CharacterSheet/PerkTree/{0}/Collection_{0}_Bot_On";

		public const string CenterOffAssetPathFormat = "View/Sprites/UI/CharacterSheet/PerkTree/{0}/Collection_{0}_Center_Off";

		public const string CenterOnAssetPathFormat = "View/Sprites/UI/CharacterSheet/PerkTree/{0}/Collection_{0}_Center_On";

		public const string CrestOffAssetPathFormat = "View/Sprites/UI/CharacterSheet/PerkTree/{0}/Crest_{0}_Off";

		public const string CrestOnAssetPathFormat = "View/Sprites/UI/CharacterSheet/PerkTree/{0}/Crest_{0}_On";

		public const string PerkUnlockAnimationPathFormat = "Animation/PerkUnlock/PerkUnlock_{0}";

		public const string DefaultCollectionName = "Misc";
	}

	[SerializeField]
	private int bannersSortingOrder;

	[SerializeField]
	private List<UnitPerkTierView> unitPerkTierView;

	[SerializeField]
	private BetterButton trainButton;

	[SerializeField]
	private Canvas bannersCanvas;

	[SerializeField]
	private RectTransform perkSelectorRect;

	[SerializeField]
	private Animator perkSelectorAnimator;

	[SerializeField]
	private TextMeshProUGUI unavailableText;

	[SerializeField]
	private TextMeshProUGUI perkPointsCount;

	[SerializeField]
	private RectTransform chainsRect;

	[SerializeField]
	private RectTransform perkLinesRect;

	[SerializeField]
	private List<PerkCollectionBannerView> bannerViews = new List<PerkCollectionBannerView>();

	[SerializeField]
	private AudioSource audioSource;

	[SerializeField]
	private AudioClip[] perkUnlockedClips;

	[SerializeField]
	private HUDJoystickSimpleTarget joystickTarget;

	public static PerkTooltipDisplayer HoveredPerkTooltipDisplayer;

	private int lastUnlockPerkClipIndex = -1;

	public bool Inited { get; private set; }

	public HUDJoystickSimpleTarget JoystickTarget => joystickTarget;

	public UnitPerkDisplay SelectedPerk { get; private set; }

	public UnitPerkTree UnitPerkTree { get; private set; }

	public List<UnitPerkTierView> UnitPerkTierViews => unitPerkTierView;

	public static T GetCollectionAssetOrDefault<T>(string assetPathFormat, string collectionId) where T : Object
	{
		T val = ResourcePooler.LoadOnce<T>(string.Format(assetPathFormat, collectionId), failSilently: true);
		if ((Object)(object)val == (Object)null)
		{
			((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).Log((object)("Could not find collection asset at path \"" + string.Format(assetPathFormat, collectionId) + "\". Using default (Misc) instead."), (CLogLevel)1, false, false);
			val = ResourcePooler.LoadOnce<T>(string.Format(assetPathFormat, "Misc"));
		}
		return val;
	}

	public override void Close()
	{
		if (base.IsOpened)
		{
			PlayableUnitManager.PerkTooltip.Hide();
			base.Close();
		}
	}

	public void Init()
	{
		for (int i = 0; i < unitPerkTierView.Count; i++)
		{
			unitPerkTierView[i].Tier = i + 1;
			unitPerkTierView[i].RefreshText();
		}
		Inited = true;
	}

	public void OnTrainButtonClick()
	{
		UnitPerkTree.UnitPerkTreeController.BuyPerk();
	}

	public override void Open()
	{
		if (!base.IsOpened)
		{
			base.Open();
			TPSingleton<TutorialManager>.Instance.OnTrigger(E_TutorialTrigger.OnPerksOpen);
		}
	}

	public void PlayPerkSelectionSound()
	{
		int num = Random.Range(0, perkUnlockedClips.Length);
		if (perkUnlockedClips.Length > 1)
		{
			while (num == lastUnlockPerkClipIndex)
			{
				num = Random.Range(0, perkUnlockedClips.Length);
			}
		}
		lastUnlockPerkClipIndex = num;
		audioSource.PlayOneShot(perkUnlockedClips[num]);
	}

	public void RefreshPerkPoints()
	{
		((TMP_Text)perkPointsCount).text = (UnitPerkTree.HasReachedMaxPerks() ? Localizer.Get("CharacterSheet_MaxPerksPoints") : UnitPerkTree.PlayableUnit.PerksPoints.ToString());
	}

	public void RefreshSelectedPerk(UnitPerkDisplay selectedPerk)
	{
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		UnitPerkDisplay selectedPerk2 = SelectedPerk;
		SelectedPerk = selectedPerk;
		if ((Object)(object)selectedPerk2 != (Object)null)
		{
			selectedPerk2.Refresh();
		}
		if ((Object)(object)SelectedPerk != (Object)null)
		{
			SelectedPerk.Refresh();
		}
		bool flag = (Object)(object)SelectedPerk != (Object)null && !SelectedPerk.Perk.UnlockedInPerkTree && SelectedPerk.Perk.PerkTier.Available && UnitPerkTree.CanBuyPerk();
		bool flag2 = TPSingleton<GameManager>.Instance.Game.State == Game.E_State.GameOver;
		trainButton.Interactable = (Object)(object)SelectedPerk != (Object)null && flag;
		((Component)perkSelectorRect).gameObject.SetActive((Object)(object)SelectedPerk != (Object)null && (!flag2 || InputManager.IsLastControllerJoystick));
		if (!((Object)(object)SelectedPerk == (Object)null))
		{
			perkSelectorAnimator.SetBool("isAvailable", flag);
			string text = string.Empty;
			if (!flag && !flag2)
			{
				text = ((!SelectedPerk.Perk.UnlockedInPerkTree) ? (UnitPerkTree.HasReachedMaxPerks() ? "CharacterSheet_MaxPerksReached" : (SelectedPerk.Perk.PerkTier.Available ? "CharacterSheet_NotEnoughPoints" : "CharacterSheet_TierLocked")) : "CharacterSheet_PerkAlreadyUnlocked");
			}
			((TMP_Text)unavailableText).text = Localizer.Get(text);
			((Transform)perkSelectorRect).SetParent(((Component)SelectedPerk).transform);
			((Transform)perkSelectorRect).localPosition = ((Transform)SelectedPerk.PerkSelectorPos).localPosition;
		}
	}

	public override void Refresh()
	{
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		base.Refresh();
		UnitPerkTree = TileObjectSelectionManager.SelectedPlayableUnit.PerkTree;
		if (!InputManager.IsLastControllerJoystick)
		{
			SelectedPerk = null;
		}
		else if (InputManager.JoystickConfig.HUDNavigation.SelectFirstPerkOnUnitChange)
		{
			SelectedPerk = UnitPerkTierViews[0].PerkDisplays[0];
			if (!EventSystem.current.alreadySelecting)
			{
				EventSystem.current.SetSelectedGameObject(((Component)SelectedPerk).gameObject);
			}
		}
		HashSet<int> lockedPerkCollectionSlots = TPSingleton<MetaUpgradesManager>.Instance.GetLockedPerkCollectionSlots();
		for (int i = 0; i < UnitPerkTree.UnitPerkCollectionIds.Count; i++)
		{
			if (lockedPerkCollectionSlots.Contains(i + 1))
			{
				((Component)bannerViews[i]).gameObject.SetActive(false);
				continue;
			}
			((Component)bannerViews[i]).gameObject.SetActive(true);
			bannerViews[i].Refresh(UnitPerkTree.UnitPerkCollectionIds[i]);
		}
		bool flag = true;
		for (int j = 0; j < UnitPerkTierViews.Count; j++)
		{
			UnitPerkTierViews[j].UnitPerkTier = UnitPerkTree.UnitPerkTiers[j];
			for (int k = 0; k < UnitPerkTierViews[j].PerkDisplays.Count; k++)
			{
				if (lockedPerkCollectionSlots.Contains(k + 1))
				{
					((Component)UnitPerkTierViews[j].PerkDisplays[k]).gameObject.SetActive(false);
					continue;
				}
				((Component)UnitPerkTierViews[j].PerkDisplays[k]).gameObject.SetActive(true);
				UnitPerkTierViews[j].PerkDisplays[k].SetContent(UnitPerkTree.UnitPerkTiers[j].Perks[k]);
				UnitPerkTierViews[j].PerkDisplays[k].Init();
			}
			bool flag2 = flag && !UnitPerkTierViews[j].UnitPerkTier.Available;
			if (flag2)
			{
				((Component)chainsRect).gameObject.SetActive(true);
				chainsRect.offsetMax = new Vector2(chainsRect.offsetMax.x, ((Transform)UnitPerkTierViews[j].Separator).position.y - ((Transform)perkLinesRect).position.y);
			}
			UnitPerkTierViews[j].RefreshAvailability(flag2);
			flag = UnitPerkTierViews[j].UnitPerkTier.Available;
		}
		if (flag)
		{
			((Component)chainsRect).gameObject.SetActive(false);
		}
		RefreshHoveredPerkTooltipDisplayer();
		RefreshSelectedPerk(SelectedPerk);
		RefreshPerkPoints();
		bannersCanvas.sortingOrder = bannersSortingOrder;
		InitializeJoystickNavigation();
	}

	private void RefreshHoveredPerkTooltipDisplayer()
	{
		if ((Object)(object)HoveredPerkTooltipDisplayer != (Object)null && TileObjectSelectionManager.SelectedPlayableUnit != null)
		{
			HoveredPerkTooltipDisplayer.DisplayTooltip(display: false);
			HoveredPerkTooltipDisplayer.DisplayTooltip(display: true);
		}
	}

	private void RevertJoystickNavigation()
	{
		foreach (UnitPerkTierView unitPerkTierView in UnitPerkTierViews)
		{
			foreach (UnitPerkDisplay perkDisplay in unitPerkTierView.PerkDisplays)
			{
				((Selectable)(object)perkDisplay.JoystickSelectable).ClearNavigation();
			}
		}
	}

	private void InitializeJoystickNavigation()
	{
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		RevertJoystickNavigation();
		List<List<UnitPerkDisplay>> list = new List<List<UnitPerkDisplay>>();
		foreach (UnitPerkTierView unitPerkTierView in UnitPerkTierViews)
		{
			List<UnitPerkDisplay> list2 = new List<UnitPerkDisplay>();
			foreach (UnitPerkDisplay perkDisplay in unitPerkTierView.PerkDisplays)
			{
				if (((Component)perkDisplay).gameObject.activeInHierarchy)
				{
					list2.Add(perkDisplay);
				}
			}
			list.Add(list2);
		}
		for (int i = 0; i < list.Count; i++)
		{
			for (int j = 0; j < list[i].Count; j++)
			{
				bool num = j == 0;
				bool flag = j == list[i].Count - 1;
				bool flag2 = i == 0;
				bool flag3 = i == list.Count - 1;
				UnitPerkDisplay unitPerkDisplay = list[i][j];
				((Selectable)(object)unitPerkDisplay.JoystickSelectable).SetMode((Mode)4);
				if (!num)
				{
					((Selectable)(object)unitPerkDisplay.JoystickSelectable).SetSelectOnLeft((Selectable)(object)list[i][j - 1].JoystickSelectable);
				}
				if (!flag)
				{
					((Selectable)(object)unitPerkDisplay.JoystickSelectable).SetSelectOnRight((Selectable)(object)list[i][j + 1].JoystickSelectable);
				}
				if (!flag2)
				{
					((Selectable)(object)unitPerkDisplay.JoystickSelectable).SetSelectOnUp((Selectable)(object)list[i - 1][j].JoystickSelectable);
				}
				else
				{
					((Selectable)(object)unitPerkDisplay.JoystickSelectable).SetSelectOnUp((Selectable)(object)TPSingleton<CharacterSheetPanel>.Instance.UnitRaceDisplay.JoystickSelectable);
				}
				if (!flag3)
				{
					((Selectable)(object)unitPerkDisplay.JoystickSelectable).SetSelectOnDown((Selectable)(object)list[i + 1][j].JoystickSelectable);
				}
				Navigation navigation = ((Selectable)unitPerkDisplay.JoystickSelectable).navigation;
				if ((Object)(object)((Navigation)(ref navigation)).selectOnLeft == (Object)null)
				{
					((Selectable)(object)unitPerkDisplay.JoystickSelectable).SetSelectOnLeft(unitPerkDisplay.DefaultSelectOnLeft);
				}
			}
		}
	}
}
