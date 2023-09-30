using TMPro;
using TPLib;
using TheLastStand.Framework;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model;
using TheLastStand.View.ToDoList;
using TheLastStand.View.Tooltip;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.HUD;

public class TurnPanel : MonoBehaviour
{
	[SerializeField]
	private Canvas turnPanelCanvas;

	[SerializeField]
	private TurnMainPanel turnMainPanel;

	[SerializeField]
	private PhasePanel phasePanel;

	[SerializeField]
	private GameObject apocalypseObject;

	[SerializeField]
	private Animator apocalypseFlameAnimator;

	[SerializeField]
	private Image apocalypseNumber;

	[SerializeField]
	private ApocalypseEffectsTooltip apocalypseTooltip;

	[SerializeField]
	private GameObject glyphsObject;

	[SerializeField]
	private TextMeshProUGUI glyphsCustomModeText;

	[SerializeField]
	private Selectable nightEndTurnButton;

	[SerializeField]
	private Selectable dayEndTurnButton;

	[SerializeField]
	private Selectable damnedSoulsSelectable;

	[SerializeField]
	private Selectable goldSelectable;

	[SerializeField]
	private Selectable materialsSelectable;

	[SerializeField]
	private Selectable workersSelectable;

	[SerializeField]
	private Selectable enemiesLeftSelectable;

	[SerializeField]
	private Selectable apocalypseSelectable;

	[SerializeField]
	private Selectable glyphsSelectable;

	public Canvas TurnPanelCanvas => turnPanelCanvas;

	public PhasePanel PhasePanel => phasePanel;

	public void Display(bool show)
	{
		((Behaviour)turnPanelCanvas).enabled = show && UIManager.DebugToggleUI != false;
	}

	public void Refresh()
	{
		RefreshTurnMainPanel();
		phasePanel.Refresh();
		RefreshJoystickNavigation();
	}

	public void RefreshTurnMainPanel()
	{
		turnMainPanel.Refresh();
	}

	private void RefreshJoystickNavigation()
	{
		Selectable val = (((Component)dayEndTurnButton).gameObject.activeSelf ? dayEndTurnButton : nightEndTurnButton);
		SelectableExtensions.SetSelectOnUp(goldSelectable, val);
		SelectableExtensions.SetSelectOnUp(materialsSelectable, val);
		SelectableExtensions.SetSelectOnUp(workersSelectable, goldSelectable);
		SelectableExtensions.SetSelectOnDown(goldSelectable, workersSelectable);
		SelectableExtensions.SetSelectOnDown(materialsSelectable, workersSelectable);
		SelectableExtensions.SetSelectOnRight(goldSelectable, materialsSelectable);
		SelectableExtensions.SetSelectOnLeft(materialsSelectable, goldSelectable);
		if (((Component)damnedSoulsSelectable).gameObject.activeSelf)
		{
			SelectableExtensions.SetSelectOnDown(val, damnedSoulsSelectable);
			SelectableExtensions.SetSelectOnUp(damnedSoulsSelectable, val);
			SelectableExtensions.SetSelectOnLeft(damnedSoulsSelectable, materialsSelectable);
			SelectableExtensions.SetSelectOnRight(materialsSelectable, damnedSoulsSelectable);
			SelectableExtensions.SetSelectOnDown(damnedSoulsSelectable, workersSelectable);
		}
		else
		{
			SelectableExtensions.SetSelectOnDown(val, materialsSelectable);
		}
		if (PhasePanel.RemainingEnemiesTextEnabled)
		{
			if (((Component)damnedSoulsSelectable).gameObject.activeSelf)
			{
				SelectableExtensions.SetSelectOnUp(enemiesLeftSelectable, damnedSoulsSelectable);
				SelectableExtensions.SetSelectOnDown(damnedSoulsSelectable, enemiesLeftSelectable);
			}
			else
			{
				SelectableExtensions.SetSelectOnUp(enemiesLeftSelectable, materialsSelectable);
				SelectableExtensions.SetSelectOnLeft(enemiesLeftSelectable, workersSelectable);
				SelectableExtensions.SetSelectOnRight(workersSelectable, enemiesLeftSelectable);
			}
		}
		else
		{
			SelectableExtensions.SetSelectOnRight(workersSelectable, (Selectable)null);
		}
		SelectableExtensions.SetMode(apocalypseSelectable, (Mode)4);
		SelectableExtensions.SetMode(glyphsSelectable, (Mode)4);
		SelectableExtensions.SetSelectOnRight(val, ((Component)glyphsSelectable).gameObject.activeSelf ? glyphsSelectable : apocalypseSelectable);
		SelectableExtensions.SetSelectOnRight(enemiesLeftSelectable, ((Component)glyphsSelectable).gameObject.activeSelf ? glyphsSelectable : apocalypseSelectable);
		SelectableExtensions.SetSelectOnLeft(apocalypseSelectable, ((Component)glyphsSelectable).gameObject.activeSelf ? glyphsSelectable : val);
		SelectableExtensions.SetSelectOnLeft(glyphsSelectable, val);
		SelectableExtensions.SetSelectOnRight(glyphsSelectable, apocalypseSelectable);
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Day)
		{
			Selectable foldButton = TPSingleton<ToDoListView>.Instance.GetFoldButton();
			SelectableExtensions.SetSelectOnDown(workersSelectable, foldButton);
			SelectableExtensions.SetSelectOnUp(foldButton, workersSelectable);
		}
		else
		{
			SelectableExtensions.SetSelectOnDown(workersSelectable, (Selectable)null);
		}
	}

	private void Start()
	{
		if (TPSingleton<WorldMapCityManager>.Instance.SelectedCity.GlyphsConfig.SelectedGlyphs.Count > 0)
		{
			glyphsObject.SetActive(true);
			if (TPSingleton<WorldMapCityManager>.Instance.SelectedCity.GlyphsConfig.CustomModeEnabled)
			{
				((Behaviour)glyphsCustomModeText).enabled = true;
				((TMP_Text)glyphsCustomModeText).text = $"+{TPSingleton<WorldMapCityManager>.Instance.SelectedCity.GetCustomModeBonusPoints()}";
			}
		}
		if (ApocalypseManager.CurrentApocalypseIndex > 0)
		{
			apocalypseObject.SetActive(true);
			apocalypseNumber.sprite = ResourcePooler<Sprite>.LoadOnce("View/Sprites/UI/WorldMap/ApocalypseLevels/ApocalypseLevel_" + ApocalypseManager.CurrentApocalypseIndex.ToString("00"), false);
			apocalypseFlameAnimator.Play("WorldMapFlamesIdle");
			apocalypseTooltip.SetApocalypsesToDisplay(ApocalypseManager.CurrentApocalypseIndex);
		}
	}
}
