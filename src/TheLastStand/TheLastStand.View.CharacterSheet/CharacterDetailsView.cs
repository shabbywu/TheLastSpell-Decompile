using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Controller.Unit;
using TheLastStand.Database;
using TheLastStand.Framework;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Tutorial;
using TheLastStand.Model.Unit;
using TheLastStand.View.HUD;
using TheLastStand.View.Unit.Stat;
using TheLastStand.View.Unit.Trait;
using TheLastStand.View.Unit.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.CharacterSheet;

public class CharacterDetailsView : TabbedPageView
{
	public static class Constants
	{
		public const string LifetimeStatsNamePrefix = "LifetimeStats_";

		public const string LifetimeStatsNameLocalizationKey = "LifetimeStats_Name";

		public const string SecondaryAttributesNameLocalizationKey = "SecondaryAttributes_Name";

		public const string BestBlowNameLocalizationKey = "LifetimeStats_BestBlow";

		public const string BestFiendNameLocalizationKey = "LifetimeStats_BestFiend";

		public const string CriticalHitsNameLocalizationKey = "LifetimeStats_CriticalHits";

		public const string DamagesBlockedNameLocalizationKey = "LifetimeStats_DamagesBlocked";

		public const string DamagesInflictedNameLocalizationKey = "LifetimeStats_DamagesInflicted";

		public const string DamagesTakenOnArmorNameLocalizationKey = "LifetimeStats_DamagesTakenOnArmor";

		public const string DodgesNameLocalizationKey = "LifetimeStats_Dodges";

		public const string HealthLostNameLocalizationKey = "LifetimeStats_HealthLost";

		public const string JumpsOverWallNameLocalizationKey = "LifetimeStats_JumpsOverWall";

		public const string KillsLocalizationKey = "LifetimeStats_Kills";

		public const string ManaSpentNameLocalizationKey = "LifetimeStats_ManaSpent";

		public const string MostUnitsInOneBlowNameLocalizationKey = "LifetimeStats_MostUnitsKilledInOneBlow";

		public const string NemesisNameLocalizationKey = "LifetimeStats_Nemesis";

		public const string PreferredWeaponNameLocalizationKey = "LifetimeStats_PreferredWeapon";

		public const string PunchesUsedNameLocalizationKey = "LifetimeStats_PunchesUsed";

		public const string StunnedEnemiesNameLocalizationKey = "LifetimeStats_StunnedEnemies";

		public const string TilesCrossedNameLocalizationKey = "LifetimeStats_TilesCrossed";

		public const string NoBestFiendLocalizationKey = "LifetimeStats_NoBestFiend";

		public const string NoNemesisLocalizationKey = "LifetimeStats_NoNemesis";

		public const string NoPreferredWeaponLocalizationKey = "LifetimeStats_NoPreferredWeapon";
	}

	[SerializeField]
	private Scrollbar characterDetailsPanelScrollbar;

	[SerializeField]
	[Range(0f, 1f)]
	private float scrollButtonsSensitivity = 0.1f;

	[SerializeField]
	private GameObject levelUpButtonPanel;

	[SerializeField]
	private Button levelUpButton;

	[SerializeField]
	private RectTransform detailsViewportJoystickScroll;

	[SerializeField]
	private Scrollbar detailsScrollbar;

	[SerializeField]
	private ScrollRect detailsScrollRect;

	[SerializeField]
	private DismissHeroButton dismissHeroButton;

	[SerializeField]
	private TextMeshProUGUI lifetimeStatsTitleText;

	[SerializeField]
	private LifetimeStatDisplay bestBlowDisplay;

	[SerializeField]
	private LifetimeStatDisplay bestFiendDisplay;

	[SerializeField]
	private LifetimeStatDisplay criticalHitsDisplay;

	[SerializeField]
	private LifetimeStatDisplay damagesBlockedDisplay;

	[SerializeField]
	private LifetimeStatDisplay damagesInflictedDisplay;

	[SerializeField]
	private LifetimeStatDisplay damagedTakenOnArmorDisplay;

	[SerializeField]
	private LifetimeStatDisplay dodgesDisplay;

	[SerializeField]
	private LifetimeStatDisplay healthLostDisplay;

	[SerializeField]
	private LifetimeStatDisplay jumpsOverWallUsedDisplay;

	[SerializeField]
	private LifetimeStatDisplay killsDisplay;

	[SerializeField]
	private LifetimeStatDisplay manaSpentDisplay;

	[SerializeField]
	private LifetimeStatDisplay mostUnitsInOneBlowDisplay;

	[SerializeField]
	private LifetimeStatDisplay nemesisDisplay;

	[SerializeField]
	private LifetimeStatDisplay preferredWeaponDisplay;

	[SerializeField]
	private LifetimeStatDisplay punchesUsedDisplay;

	[SerializeField]
	private LifetimeStatDisplay stunnedEnemiesDisplay;

	[SerializeField]
	private LifetimeStatDisplay tilesCrossedDisplay;

	[SerializeField]
	private TextMeshProUGUI secondaryStatsTitleText;

	[SerializeField]
	private UnitStatDisplay[] secondaryAttributesDisplays;

	[SerializeField]
	private Color defaultColor = Color.white;

	[SerializeField]
	private DataColor bonusColor;

	[SerializeField]
	private DataColor malusColor;

	[SerializeField]
	private List<UnitTraitDisplay> unitTraits;

	[SerializeField]
	private TextMeshProUGUI unitNameDetails;

	[SerializeField]
	private RectTransform secondaryStatsLeftPanel;

	[SerializeField]
	private RectTransform secondaryStatsRightPanel;

	[SerializeField]
	private Selectable levelUpButtonDisabledTarget;

	[SerializeField]
	private HUDJoystickTarget secondaryAttributesHUDJoystickTarget;

	[SerializeField]
	private bool initSecondaryAttributesNavigationOnInit;

	private PlayableUnit playableUnit;

	public Button LevelUpButton => levelUpButton;

	public Selectable LevelUpButtonDisabledTarget => levelUpButtonDisabledTarget;

	public HUDJoystickTarget SecondaryAttributesHUDJoystickTarget => secondaryAttributesHUDJoystickTarget;

	public override void Close()
	{
		if (base.IsOpened)
		{
			PlayableUnitManager.TraitTooltip.Hide();
			PlayableUnitManager.StatTooltip.Hide();
			base.Close();
			((Behaviour)detailsScrollRect).enabled = false;
		}
	}

	public void OnBotButtonClick()
	{
		characterDetailsPanelScrollbar.value = Mathf.Clamp01(characterDetailsPanelScrollbar.value - scrollButtonsSensitivity);
	}

	public void OnTopButtonClick()
	{
		characterDetailsPanelScrollbar.value = Mathf.Clamp01(characterDetailsPanelScrollbar.value + scrollButtonsSensitivity);
	}

	public void OnLevelUpButtonJoystickSelect()
	{
		detailsScrollbar.value = 1f;
	}

	public override void Open()
	{
		if (!base.IsOpened)
		{
			((Behaviour)detailsScrollRect).enabled = true;
			base.Open();
			((MonoBehaviour)(object)this).DoAfter(0.05f, delegate
			{
				characterDetailsPanelScrollbar.value = 1f;
			});
			((MonoBehaviour)this).StartCoroutine(TriggerTutorialAfterCharacterSheetTween());
		}
	}

	public override void Refresh()
	{
		if (TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			base.Refresh();
			playableUnit = TileObjectSelectionManager.SelectedPlayableUnit;
			((TMP_Text)unitNameDetails).text = playableUnit.Name;
			RefreshLocalizedTexts();
			RefreshLifetimeStats();
			RefreshSecondaryStats();
			RefreshTraits();
			if ((Object)(object)dismissHeroButton != (Object)null)
			{
				dismissHeroButton.Refresh();
			}
			if ((Object)(object)fontLocalizedParent != (Object)null)
			{
				fontLocalizedParent.RefreshChilds();
			}
			bool flag = playableUnit.StatsPoints > 0 && UnitLevelUpController.CanOpenUnitLevelUpView;
			levelUpButtonPanel.SetActive(flag);
			((Selectable)levelUpButton).interactable = flag;
		}
	}

	protected override void Start()
	{
		base.Start();
		Init();
	}

	private void Init()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		((UnityEvent)levelUpButton.onClick).AddListener((UnityAction)delegate
		{
			TPSingleton<CharacterSheetPanel>.Instance.OnUnitLevelButtonClick(shouldRefreshTooltip: false);
			((Selectable)levelUpButton).interactable = playableUnit.StatsPoints > 0;
		});
		InitJoystickNavigation();
	}

	private void InitJoystickNavigation()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Expected O, but got Unknown
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Expected O, but got Unknown
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Expected O, but got Unknown
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Expected O, but got Unknown
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Expected O, but got Unknown
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Expected O, but got Unknown
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Expected O, but got Unknown
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Expected O, but got Unknown
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Expected O, but got Unknown
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Expected O, but got Unknown
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Expected O, but got Unknown
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Expected O, but got Unknown
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Expected O, but got Unknown
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Expected O, but got Unknown
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Expected O, but got Unknown
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Expected O, but got Unknown
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Expected O, but got Unknown
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Expected O, but got Unknown
		if (initSecondaryAttributesNavigationOnInit)
		{
			InitSecondaryAttributesJoystickNavigation();
		}
		for (int i = 0; i < ((Transform)secondaryStatsLeftPanel).childCount; i++)
		{
			Transform secondaryStat = ((Transform)secondaryStatsLeftPanel).GetChild(i);
			((Component)secondaryStat).GetComponent<JoystickSelectable>().AddListenerOnSelect((UnityAction)delegate
			{
				CharacterDetailsView characterDetailsView2 = this;
				Transform obj2 = secondaryStat;
				characterDetailsView2.OnSecondaryStatJoystickSelect((RectTransform)(object)((obj2 is RectTransform) ? obj2 : null));
			});
		}
		for (int j = 0; j < ((Transform)secondaryStatsRightPanel).childCount; j++)
		{
			Transform secondaryStat2 = ((Transform)secondaryStatsRightPanel).GetChild(j);
			((Component)secondaryStat2).GetComponent<JoystickSelectable>().AddListenerOnSelect((UnityAction)delegate
			{
				CharacterDetailsView characterDetailsView = this;
				Transform obj = secondaryStat2;
				characterDetailsView.OnSecondaryStatJoystickSelect((RectTransform)(object)((obj is RectTransform) ? obj : null));
			});
		}
		bestBlowDisplay.JoystickSelectable.AddListenerOnSelect((UnityAction)delegate
		{
			Transform transform17 = ((Component)bestBlowDisplay).transform;
			OnSecondaryStatJoystickSelect((RectTransform)(object)((transform17 is RectTransform) ? transform17 : null));
		});
		bestFiendDisplay.JoystickSelectable.AddListenerOnSelect((UnityAction)delegate
		{
			Transform transform16 = ((Component)bestFiendDisplay).transform;
			OnSecondaryStatJoystickSelect((RectTransform)(object)((transform16 is RectTransform) ? transform16 : null));
		});
		criticalHitsDisplay.JoystickSelectable.AddListenerOnSelect((UnityAction)delegate
		{
			Transform transform15 = ((Component)criticalHitsDisplay).transform;
			OnSecondaryStatJoystickSelect((RectTransform)(object)((transform15 is RectTransform) ? transform15 : null));
		});
		damagesBlockedDisplay.JoystickSelectable.AddListenerOnSelect((UnityAction)delegate
		{
			Transform transform14 = ((Component)damagesBlockedDisplay).transform;
			OnSecondaryStatJoystickSelect((RectTransform)(object)((transform14 is RectTransform) ? transform14 : null));
		});
		damagesInflictedDisplay.JoystickSelectable.AddListenerOnSelect((UnityAction)delegate
		{
			Transform transform13 = ((Component)damagesInflictedDisplay).transform;
			OnSecondaryStatJoystickSelect((RectTransform)(object)((transform13 is RectTransform) ? transform13 : null));
		});
		damagedTakenOnArmorDisplay.JoystickSelectable.AddListenerOnSelect((UnityAction)delegate
		{
			Transform transform12 = ((Component)damagedTakenOnArmorDisplay).transform;
			OnSecondaryStatJoystickSelect((RectTransform)(object)((transform12 is RectTransform) ? transform12 : null));
		});
		dodgesDisplay.JoystickSelectable.AddListenerOnSelect((UnityAction)delegate
		{
			Transform transform11 = ((Component)dodgesDisplay).transform;
			OnSecondaryStatJoystickSelect((RectTransform)(object)((transform11 is RectTransform) ? transform11 : null));
		});
		healthLostDisplay.JoystickSelectable.AddListenerOnSelect((UnityAction)delegate
		{
			Transform transform10 = ((Component)healthLostDisplay).transform;
			OnSecondaryStatJoystickSelect((RectTransform)(object)((transform10 is RectTransform) ? transform10 : null));
		});
		jumpsOverWallUsedDisplay.JoystickSelectable.AddListenerOnSelect((UnityAction)delegate
		{
			Transform transform9 = ((Component)jumpsOverWallUsedDisplay).transform;
			OnSecondaryStatJoystickSelect((RectTransform)(object)((transform9 is RectTransform) ? transform9 : null));
		});
		killsDisplay.JoystickSelectable.AddListenerOnSelect((UnityAction)delegate
		{
			Transform transform8 = ((Component)killsDisplay).transform;
			OnSecondaryStatJoystickSelect((RectTransform)(object)((transform8 is RectTransform) ? transform8 : null));
		});
		manaSpentDisplay.JoystickSelectable.AddListenerOnSelect((UnityAction)delegate
		{
			Transform transform7 = ((Component)manaSpentDisplay).transform;
			OnSecondaryStatJoystickSelect((RectTransform)(object)((transform7 is RectTransform) ? transform7 : null));
		});
		mostUnitsInOneBlowDisplay.JoystickSelectable.AddListenerOnSelect((UnityAction)delegate
		{
			Transform transform6 = ((Component)mostUnitsInOneBlowDisplay).transform;
			OnSecondaryStatJoystickSelect((RectTransform)(object)((transform6 is RectTransform) ? transform6 : null));
		});
		nemesisDisplay.JoystickSelectable.AddListenerOnSelect((UnityAction)delegate
		{
			Transform transform5 = ((Component)nemesisDisplay).transform;
			OnSecondaryStatJoystickSelect((RectTransform)(object)((transform5 is RectTransform) ? transform5 : null));
		});
		preferredWeaponDisplay.JoystickSelectable.AddListenerOnSelect((UnityAction)delegate
		{
			Transform transform4 = ((Component)preferredWeaponDisplay).transform;
			OnSecondaryStatJoystickSelect((RectTransform)(object)((transform4 is RectTransform) ? transform4 : null));
		});
		punchesUsedDisplay.JoystickSelectable.AddListenerOnSelect((UnityAction)delegate
		{
			Transform transform3 = ((Component)punchesUsedDisplay).transform;
			OnSecondaryStatJoystickSelect((RectTransform)(object)((transform3 is RectTransform) ? transform3 : null));
		});
		stunnedEnemiesDisplay.JoystickSelectable.AddListenerOnSelect((UnityAction)delegate
		{
			Transform transform2 = ((Component)stunnedEnemiesDisplay).transform;
			OnSecondaryStatJoystickSelect((RectTransform)(object)((transform2 is RectTransform) ? transform2 : null));
		});
		tilesCrossedDisplay.JoystickSelectable.AddListenerOnSelect((UnityAction)delegate
		{
			Transform transform = ((Component)tilesCrossedDisplay).transform;
			OnSecondaryStatJoystickSelect((RectTransform)(object)((transform is RectTransform) ? transform : null));
		});
	}

	private void InitSecondaryAttributesJoystickNavigation()
	{
		JoystickSelectable[] array = new JoystickSelectable[((Transform)secondaryStatsLeftPanel).childCount];
		JoystickSelectable[] array2 = new JoystickSelectable[((Transform)secondaryStatsRightPanel).childCount];
		for (int i = 0; i < ((Transform)secondaryStatsLeftPanel).childCount; i++)
		{
			array[i] = ((Component)((Transform)secondaryStatsLeftPanel).GetChild(i)).GetComponent<JoystickSelectable>();
		}
		for (int j = 0; j < ((Transform)secondaryStatsRightPanel).childCount; j++)
		{
			array2[j] = ((Component)((Transform)secondaryStatsRightPanel).GetChild(j)).GetComponent<JoystickSelectable>();
		}
		for (int k = 0; k < array.Length; k++)
		{
			JoystickSelectable selectable = array[k];
			((Selectable)(object)selectable).SetMode((Mode)4);
			if (k > 0)
			{
				((Selectable)(object)selectable).SetSelectOnUp((Selectable)(object)array[k - 1]);
			}
			if (k < array.Length - 1)
			{
				((Selectable)(object)selectable).SetSelectOnDown((Selectable)(object)array[k + 1]);
			}
			if (k < array2.Length)
			{
				((Selectable)(object)selectable).SetSelectOnRight((Selectable)(object)array2[k]);
			}
		}
		for (int l = 0; l < array2.Length; l++)
		{
			JoystickSelectable selectable2 = array2[l];
			((Selectable)(object)selectable2).SetMode((Mode)4);
			if (l > 0)
			{
				((Selectable)(object)selectable2).SetSelectOnUp((Selectable)(object)array2[l - 1]);
			}
			if (l < array2.Length - 1)
			{
				((Selectable)(object)selectable2).SetSelectOnDown((Selectable)(object)array2[l + 1]);
			}
			if (l < array.Length)
			{
				((Selectable)(object)selectable2).SetSelectOnLeft((Selectable)(object)array[l]);
			}
		}
	}

	private void RefreshLocalizedTexts()
	{
		((TMP_Text)lifetimeStatsTitleText).text = Localizer.Get("LifetimeStats_Name");
		((TMP_Text)secondaryStatsTitleText).text = Localizer.Get("SecondaryAttributes_Name");
	}

	private void RefreshLifetimeStats()
	{
		bestBlowDisplay.Refresh(Localizer.Get("LifetimeStats_BestBlow"), playableUnit.LifetimeStats.BestBlow.ToString());
		criticalHitsDisplay.Refresh(Localizer.Get("LifetimeStats_CriticalHits"), playableUnit.LifetimeStats.CriticalHits.ToString());
		damagesBlockedDisplay.Refresh(Localizer.Get("LifetimeStats_DamagesBlocked"), playableUnit.LifetimeStats.DamagesBlocked.ToString());
		damagesInflictedDisplay.Refresh(Localizer.Get("LifetimeStats_DamagesInflicted"), playableUnit.LifetimeStats.DamagesInflicted.ToString());
		damagedTakenOnArmorDisplay.Refresh(Localizer.Get("LifetimeStats_DamagesTakenOnArmor"), playableUnit.LifetimeStats.DamagesTakenOnArmor.ToString());
		dodgesDisplay.Refresh(Localizer.Get("LifetimeStats_Dodges"), playableUnit.LifetimeStats.Dodges.ToString());
		healthLostDisplay.Refresh(Localizer.Get("LifetimeStats_HealthLost"), playableUnit.LifetimeStats.HealthLost.ToString());
		jumpsOverWallUsedDisplay.Refresh(Localizer.Get("LifetimeStats_JumpsOverWall"), playableUnit.LifetimeStats.JumpsOverWallUsed.ToString());
		killsDisplay.Refresh(Localizer.Get("LifetimeStats_Kills"), playableUnit.LifetimeStats.Kills.ToString());
		manaSpentDisplay.Refresh(Localizer.Get("LifetimeStats_ManaSpent"), playableUnit.LifetimeStats.ManaSpent.ToString());
		mostUnitsInOneBlowDisplay.Refresh(Localizer.Get("LifetimeStats_MostUnitsKilledInOneBlow"), playableUnit.LifetimeStats.MostUnitsKilledInOneBlow.ToString());
		punchesUsedDisplay.Refresh(Localizer.Get("LifetimeStats_PunchesUsed"), playableUnit.LifetimeStats.PunchesUsed.ToString());
		stunnedEnemiesDisplay.Refresh(Localizer.Get("LifetimeStats_StunnedEnemies"), playableUnit.LifetimeStats.StunnedEnemies.ToString());
		tilesCrossedDisplay.Refresh(Localizer.Get("LifetimeStats_TilesCrossed"), playableUnit.LifetimeStats.TilesCrossed.ToString());
		if (playableUnit.LifetimeStats.LifetimeStatsController.TryGetBestFiend(out string bestFiendId))
		{
			string empty = string.Empty;
			if (!Localizer.TryGet("EnemyName_" + bestFiendId, ref empty))
			{
				Localizer.TryGet("BossName_" + bestFiendId, ref empty);
			}
			bestFiendDisplay.Refresh(Localizer.Get("LifetimeStats_BestFiend"), empty);
		}
		else
		{
			bestFiendDisplay.Refresh(Localizer.Get("LifetimeStats_BestFiend"), Localizer.Get("LifetimeStats_NoBestFiend"));
		}
		if (playableUnit.LifetimeStats.LifetimeStatsController.TryGetNemesisId(out var nemesisId))
		{
			string text = default(string);
			string statValue = (Localizer.TryGet("BossName_" + nemesisId, ref text) ? text : Localizer.Get("EnemyName_" + nemesisId));
			nemesisDisplay.Refresh(Localizer.Get("LifetimeStats_Nemesis"), statValue);
		}
		else
		{
			nemesisDisplay.Refresh(Localizer.Get("LifetimeStats_Nemesis"), Localizer.Get("LifetimeStats_NoNemesis"));
		}
		preferredWeaponDisplay.Refresh(Localizer.Get("LifetimeStats_PreferredWeapon"), playableUnit.LifetimeStats.LifetimeStatsController.TryGetPreferredWeaponId(out var weaponUses) ? ItemDatabase.ItemDefinitions[weaponUses.Item1].BaseName : Localizer.Get("LifetimeStats_NoPreferredWeapon"));
	}

	private void RefreshSecondaryStats()
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		for (int num = secondaryAttributesDisplays.Length - 1; num >= 0; num--)
		{
			secondaryAttributesDisplays[num].TargetUnit = playableUnit;
			secondaryAttributesDisplays[num].Refresh();
			float num2 = playableUnit.UnitStatsController.ComputeStatBonus(secondaryAttributesDisplays[num].StatDefinition.Id);
			if (num2 == 0f)
			{
				secondaryAttributesDisplays[num].ColorOverride = defaultColor;
			}
			else if (num2 > 0f)
			{
				secondaryAttributesDisplays[num].ColorOverride = bonusColor._Color;
			}
			else
			{
				secondaryAttributesDisplays[num].ColorOverride = malusColor._Color;
			}
			secondaryAttributesDisplays[num].Refresh();
		}
	}

	private void OnSecondaryStatJoystickSelect(RectTransform source)
	{
		GUIHelpers.AdjustScrollViewToFocusedItem(source, detailsViewportJoystickScroll, detailsScrollbar, 0.04f, 0f);
	}

	private void RefreshTraits()
	{
		if (TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			base.Refresh();
			PlayableUnitManager.TraitTooltip.Hide();
			PlayableUnit selectedPlayableUnit = TileObjectSelectionManager.SelectedPlayableUnit;
			for (int i = 0; i < unitTraits.Count; i++)
			{
				unitTraits[i].UnitTraitDefinition = ((selectedPlayableUnit.UnitTraitDefinitions.Count > i) ? selectedPlayableUnit.UnitTraitDefinitions[i] : null);
				unitTraits[i].Refresh();
			}
		}
	}

	private IEnumerator TriggerTutorialAfterCharacterSheetTween()
	{
		yield return (object)new WaitUntil((Func<bool>)(() => !TPSingleton<CharacterSheetPanel>.Instance.IsDisplayTweenPlaying));
		TPSingleton<TutorialManager>.Instance.OnTrigger(E_TutorialTrigger.OnCharacterDetailsOpen);
	}
}
