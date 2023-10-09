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
		goldSelectable.SetSelectOnUp(val);
		materialsSelectable.SetSelectOnUp(val);
		workersSelectable.SetSelectOnUp(goldSelectable);
		goldSelectable.SetSelectOnDown(workersSelectable);
		materialsSelectable.SetSelectOnDown(workersSelectable);
		goldSelectable.SetSelectOnRight(materialsSelectable);
		materialsSelectable.SetSelectOnLeft(goldSelectable);
		if (((Component)damnedSoulsSelectable).gameObject.activeSelf)
		{
			val.SetSelectOnDown(damnedSoulsSelectable);
			damnedSoulsSelectable.SetSelectOnUp(val);
			damnedSoulsSelectable.SetSelectOnLeft(materialsSelectable);
			materialsSelectable.SetSelectOnRight(damnedSoulsSelectable);
			damnedSoulsSelectable.SetSelectOnDown(workersSelectable);
		}
		else
		{
			val.SetSelectOnDown(materialsSelectable);
		}
		if (PhasePanel.RemainingEnemiesTextEnabled)
		{
			if (((Component)damnedSoulsSelectable).gameObject.activeSelf)
			{
				enemiesLeftSelectable.SetSelectOnUp(damnedSoulsSelectable);
				damnedSoulsSelectable.SetSelectOnDown(enemiesLeftSelectable);
			}
			else
			{
				enemiesLeftSelectable.SetSelectOnUp(materialsSelectable);
				enemiesLeftSelectable.SetSelectOnLeft(workersSelectable);
				workersSelectable.SetSelectOnRight(enemiesLeftSelectable);
			}
		}
		else
		{
			workersSelectable.SetSelectOnRight(null);
		}
		apocalypseSelectable.SetMode((Mode)4);
		glyphsSelectable.SetMode((Mode)4);
		val.SetSelectOnRight(((Component)glyphsSelectable).gameObject.activeSelf ? glyphsSelectable : apocalypseSelectable);
		enemiesLeftSelectable.SetSelectOnRight(((Component)glyphsSelectable).gameObject.activeSelf ? glyphsSelectable : apocalypseSelectable);
		apocalypseSelectable.SetSelectOnLeft(((Component)glyphsSelectable).gameObject.activeSelf ? glyphsSelectable : val);
		glyphsSelectable.SetSelectOnLeft(val);
		glyphsSelectable.SetSelectOnRight(apocalypseSelectable);
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Day)
		{
			Selectable foldButton = TPSingleton<ToDoListView>.Instance.GetFoldButton();
			workersSelectable.SetSelectOnDown(foldButton);
			foldButton.SetSelectOnUp(workersSelectable);
		}
		else
		{
			workersSelectable.SetSelectOnDown(null);
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
			apocalypseNumber.sprite = ResourcePooler<Sprite>.LoadOnce("View/Sprites/UI/WorldMap/ApocalypseLevels/ApocalypseLevel_" + ApocalypseManager.CurrentApocalypseIndex.ToString("00"));
			apocalypseFlameAnimator.Play("WorldMapFlamesIdle");
			apocalypseTooltip.SetApocalypsesToDisplay(ApocalypseManager.CurrentApocalypseIndex);
		}
	}
}
