using TPLib;
using TPLib.Localization;
using TPLib.UI;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.View.CharacterSheet;
using TheLastStand.View.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.Unit.UI;

public class DismissHeroButton : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
	public static class Constants
	{
		public const string TooltipNotEnoughHeroes = "DismissHero_Tooltip_Unavailable_NotEnoughHeroes";

		public const string TooltipNotProduction = "DismissHero_Tooltip_Unavailable_NotProduction";

		public const string TooltipAvailable = "DismissHero_Tooltip_Available";

		public const string PopupDescription = "DismissHero_ConsentPopup_Description";
	}

	[SerializeField]
	private BetterButton button;

	[SerializeField]
	private FollowDatas joystickTooltipFollowData;

	private string tooltipContent = string.Empty;

	public void OnPointerEnter(PointerEventData eventData)
	{
		DisplayTooltip();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		UIManager.GenericTooltip.Hide();
	}

	public void OnSelect(BaseEventData eventData)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (InputManager.IsLastControllerJoystick)
		{
			FollowElement followElement = UIManager.GenericTooltip.FollowElement;
			followElement.ChangeOffset(joystickTooltipFollowData.Offset);
			followElement.ChangeTarget(joystickTooltipFollowData.FollowTarget);
			if (TPSingleton<HUDJoystickNavigationManager>.Instance.ShowTooltips)
			{
				DisplayTooltip();
			}
		}
	}

	public void OnDeselect(BaseEventData eventData)
	{
		FollowElement followElement = UIManager.GenericTooltip.FollowElement;
		followElement.RestoreFollowDatasOffset();
		followElement.ChangeTarget(null);
		UIManager.GenericTooltip.Hide();
	}

	public void Refresh()
	{
		if (!CanDismiss(out var reason))
		{
			button.Interactable = false;
			tooltipContent = reason;
		}
		else
		{
			button.Interactable = true;
			tooltipContent = "DismissHero_Tooltip_Available";
		}
	}

	private void Awake()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		((UnityEvent)((Button)button).onClick).AddListener(new UnityAction(OnDismissClicked));
	}

	private bool CanDismiss(out string reason)
	{
		if (!PlayableUnitManager.DebugToggleDismissHeroValidityChecks)
		{
			reason = string.Empty;
			return true;
		}
		if (TPSingleton<GameManager>.Instance.Game.DayTurn != Game.E_DayTurn.Production)
		{
			reason = "DismissHero_Tooltip_Unavailable_NotProduction";
			return false;
		}
		if (TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count < 2)
		{
			reason = "DismissHero_Tooltip_Unavailable_NotEnoughHeroes";
			return false;
		}
		reason = string.Empty;
		return true;
	}

	private void DisplayTooltip()
	{
		UIManager.GenericTooltip.SetContent(tooltipContent);
		UIManager.GenericTooltip.Display();
	}

	private void OnEnable()
	{
		HUDJoystickNavigationManager.TooltipsToggled += OnTooltipsToggled;
	}

	private void OnDisable()
	{
		HUDJoystickNavigationManager.TooltipsToggled -= OnTooltipsToggled;
	}

	private void OnDismissClicked()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (TPSingleton<UnitLevelUpView>.Instance.IsOpened)
		{
			TPSingleton<UnitLevelUpView>.Instance.Close(instant: true);
		}
		GenericConsent.Open(new ParameterizedLocalizationLine("DismissHero_ConsentPopup_Description", new string[1] { TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitName }), delegate
		{
			PlayableUnitManager.DismissPlayableUnit(TileObjectSelectionManager.SelectedPlayableUnit);
			EventSystem.current.SetSelectedGameObject((GameObject)null);
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: false);
		}, delegate
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(TPSingleton<CharacterSheetPanel>.Instance.RightPanelJoystickTarget.GetSelectionInfo());
		});
	}

	private void OnTooltipsToggled(bool showTooltips)
	{
		if (!((Object)(object)EventSystem.current.currentSelectedGameObject != (Object)(object)((Component)this).gameObject))
		{
			if (showTooltips)
			{
				DisplayTooltip();
			}
			else
			{
				UIManager.GenericTooltip.Hide();
			}
		}
	}
}
