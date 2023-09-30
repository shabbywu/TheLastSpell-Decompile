using System;
using System.Collections;
using System.Collections.Generic;
using TPLib;
using TPLib.Debugging.Console;
using TPLib.Localization;
using TPLib.Yield;
using TheLastStand.Framework;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Skill;
using TheLastStand.Manager.Sound;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.ScriptableObjects;
using TheLastStand.View;
using TheLastStand.View.CharacterSheet;
using TheLastStand.View.Cursor;
using TheLastStand.View.Generic;
using TheLastStand.View.Seer;
using TheLastStand.View.ToDoList;
using TheLastStand.View.Tooltip;
using TheLastStand.View.Unit.Injury;
using TheLastStand.View.Unit.UI;
using UnityEngine;

namespace TheLastStand.Manager;

public class UIManager : Manager<UIManager>
{
	[SerializeField]
	private GenericTooltip genericTooltip;

	[SerializeField]
	private GenericTitledTooltip genericTitledTooltip;

	[SerializeField]
	private InjuryTooltip injuryTooltip;

	[SerializeField]
	private GenericTitledTooltip unitExperienceTooltip;

	[SerializeField]
	private EliteAffixTooltip eliteAffixTooltip;

	[SerializeField]
	private UnitHUDStatusesAndInjuriesTooltip unitHUDStatusesTooltip;

	[SerializeField]
	private PooledAudioSourceData pooledAudioSourceData;

	[SerializeField]
	private AudioClip buttonClickAudioClip;

	[SerializeField]
	private AudioClip buttonHoverAudioClip;

	[SerializeField]
	private AudioClip beginDragAudioClip;

	[SerializeField]
	private AudioClip changeEquipmentAudioClip;

	[SerializeField]
	private AudioClip dropSuccessAudioClip;

	[SerializeField]
	private AudioClip dropFailAudioClip;

	[SerializeField]
	private AudioClip barkTextDisplayAudioClip;

	[SerializeField]
	private float gaugeMarkerSectionSizeRatio = 2f;

	[SerializeField]
	private GameObject gaugeMarker;

	[SerializeField]
	private List<GaugeMarkerData> gaugeMarkersData;

	[SerializeField]
	[Tooltip("The How To Play panel is only displayed at the beginning of a run in builds. This field is used to force to do it in unity editor.")]
	private bool forceShowHowToPlayAtStart;

	[SerializeField]
	private CanvasGroup whiteScreen;

	public static bool? DebugToggleUI;

	public static PooledAudioSourceData PooledAudioSourceData => TPSingleton<UIManager>.Instance.pooledAudioSourceData;

	public static AudioClip BarkTextDisplayAudioClip => TPSingleton<UIManager>.Instance.barkTextDisplayAudioClip;

	public static AudioClip BeginDragAudioClip => TPSingleton<UIManager>.Instance.beginDragAudioClip;

	public static AudioClip ButtonClickAudioClip => TPSingleton<UIManager>.Instance.buttonClickAudioClip;

	public static AudioClip ButtonHoverAudioClip => TPSingleton<UIManager>.Instance.buttonHoverAudioClip;

	public static AudioClip ChangeEquipmentAudioClip => TPSingleton<UIManager>.Instance.changeEquipmentAudioClip;

	public static AudioClip DropFailAudioClip => TPSingleton<UIManager>.Instance.dropFailAudioClip;

	public static AudioClip DropSuccessAudioClip => TPSingleton<UIManager>.Instance.dropSuccessAudioClip;

	public static bool ForceShowHowToPlayAtStart => TPSingleton<UIManager>.Instance.forceShowHowToPlayAtStart;

	public static GameObject GaugeMarker => TPSingleton<UIManager>.Instance.gaugeMarker;

	public static List<GaugeMarkerData> GaugeMarkersData => TPSingleton<UIManager>.Instance.gaugeMarkersData;

	public static float GaugeMarkerSectionSizeRatio => TPSingleton<UIManager>.Instance.gaugeMarkerSectionSizeRatio;

	public static GenericTitledTooltip GenericTitledTooltip => TPSingleton<UIManager>.Instance.genericTitledTooltip;

	public static GenericTooltip GenericTooltip => TPSingleton<UIManager>.Instance.genericTooltip;

	public static InjuryTooltip InjuryTooltip => TPSingleton<UIManager>.Instance.injuryTooltip;

	public static EliteAffixTooltip EliteAffixTooltip => TPSingleton<UIManager>.Instance.eliteAffixTooltip;

	public static GenericTitledTooltip UnitExperienceTooltip => TPSingleton<UIManager>.Instance.unitExperienceTooltip;

	public static UnitHUDStatusesAndInjuriesTooltip UnitHUDStatusesTooltip => TPSingleton<UIManager>.Instance.unitHUDStatusesTooltip;

	public static CanvasGroup WhiteScreen => TPSingleton<UIManager>.Instance.whiteScreen;

	public static void HideInfoPanels()
	{
		PlayableUnitManager.PlayableUnitTooltip.Hide();
		EnemyUnitManager.HideInfoPanels();
		BuildingManager.BuildingInfoPanel.Hide();
		SkillManager.AttackInfoPanel.Hide();
		SkillManager.GenericActionInfoPanel.Hide();
	}

	public static bool DisplayGameSaveErrorPopUp(string fileCopyPath, SaveManager.E_BrokenSaveReason brokenSaveReason, bool backupHasBeenLoaded)
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		string text;
		string text2;
		switch (brokenSaveReason)
		{
		default:
			text = "Popup_SaveLoading_ErrorTitle_UnknownReason";
			text2 = "Popup_SaveLoading_ErrorText_UnknownReason" + (backupHasBeenLoaded ? "_BackupLoadSuccessful" : "_BackupLoadFail");
			break;
		case SaveManager.E_BrokenSaveReason.LOADING_ERROR:
			text = "Popup_SaveLoading_ErrorTitle_LoadingError";
			text2 = "Popup_SaveLoading_ErrorText_LoadingError" + (backupHasBeenLoaded ? "_BackupLoadSuccessful" : "_BackupLoadFail");
			break;
		case SaveManager.E_BrokenSaveReason.WRONG_VERSION:
			text = "Popup_SaveLoading_ErrorTitle_WrongVersion";
			text2 = "Popup_SaveLoading_ErrorText_WrongVersion" + (backupHasBeenLoaded ? "_BackupLoadSuccessful" : "_BackupLoadFail");
			break;
		}
		return (Object)(object)GenericPopUp.Open(new ParameterizedLocalizationLine(text, Array.Empty<string>()), new ParameterizedLocalizationLine(text2, new string[1] { fileCopyPath }), ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Stats/Icons/VerySmall_Critical", false)) != (Object)null;
	}

	public static void CloseAllOpenedPopups()
	{
		if (TPSingleton<UIManager>.Exist())
		{
			if (TPSingleton<CharacterSheetPanel>.Exist())
			{
				TPSingleton<CharacterSheetPanel>.Instance.Close();
			}
			if (TPSingleton<SettingsManager>.Exist())
			{
				SettingsManager.CloseSettings();
			}
		}
	}

	public void PlayAudioClip(AudioClip audioClip)
	{
		SoundManager.PlayAudioClip(audioClip, PooledAudioSourceData);
	}

	public void PlayAudioClipWithoutInterrupting(AudioClip audioClip)
	{
		SoundManager.PlayAudioClip(audioClip, PooledAudioSourceData, 0f, doNotInterrupt: true);
	}

	public void ToggleUI()
	{
		((MonoBehaviour)this).StartCoroutine(ToggleUICoroutine());
	}

	public IEnumerator ToggleUICoroutine(bool display = false)
	{
		TileObjectSelectionManager.DeselectAll();
		CursorView.ClearTiles();
		yield return (object)new WaitForEndOfFrame();
		if (!display)
		{
			PanicManager.Panic.PanicView.Hide();
			yield return SharedYields.WaitForSeconds(PanicManager.Panic.PanicView.PanicPanel.ClosePanelTweenDuration + 0.1f);
			TPSingleton<ToDoListView>.Instance.Hide();
		}
		else
		{
			PanicManager.Panic.PanicView.DisplayOrHide();
			TPSingleton<ToDoListView>.Instance.Show();
		}
		bool displayed = display && SpawnWaveManager.AliveSeer && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.NightReport && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.GameOver && TPSingleton<GameManager>.Instance.Game.Cycle != Game.E_Cycle.Night;
		GameView.TopScreenPanel.Display(display);
		TPSingleton<SeerPreviewDisplay>.Instance.Displayed = displayed;
		((Behaviour)GameView.BottomScreenPanel.BottomLeftPanel.Canvas).enabled = display;
	}

	[DevConsoleCommand(Name = "ToggleHUD")]
	public static void Debug_ToggleUI(bool display)
	{
		((MonoBehaviour)TPSingleton<UIManager>.Instance).StartCoroutine(TPSingleton<UIManager>.Instance.ToggleUICoroutine(display));
		DebugToggleUI = display;
		GameView.GameAccelerationPanel.Hide();
		PanicManager.Panic.PanicView.PanicPanel.Close();
	}
}
