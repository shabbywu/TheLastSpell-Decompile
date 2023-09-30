using TPLib;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Model.Building;
using TheLastStand.View.HUD;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.Building.Construction;

[RequireComponent(typeof(Button))]
public abstract class ConstructionModeButton : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IJoystickSelect
{
	[SerializeField]
	protected Button button;

	[SerializeField]
	protected GameObject selector;

	public TheLastStand.Model.Building.Construction.E_UnusableActionCause UnusableActionCause { get; private set; }

	public void OnDisplayTooltip(bool display)
	{
		if (!InputManager.JoystickConfig.HUDNavigation.AlwaysShowTooltipOnConstruction)
		{
			if (display)
			{
				OnPointerEnter(null);
			}
			else
			{
				OnPointerExit(null);
			}
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		TPSingleton<UIManager>.Instance.PlayAudioClip(UIManager.ButtonHoverAudioClip);
		TPSingleton<ConstructionView>.Instance.HoveredConstructionModeButton = this;
		BuildingManager.BuildingRepairTooltip.FollowElement.ChangeTarget(((Component)this).transform);
		BuildingManager.BuildingRepairTooltip.Display();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if ((Object)(object)TPSingleton<ConstructionView>.Instance.HoveredConstructionModeButton == (Object)(object)this)
		{
			TPSingleton<ConstructionView>.Instance.HoveredConstructionModeButton = null;
			BuildingManager.BuildingRepairTooltip.Display();
		}
	}

	public void OnSkillHover(bool select)
	{
		if (TPSingleton<HUDJoystickNavigationManager>.Instance.ShowTooltips || InputManager.JoystickConfig.HUDNavigation.AlwaysShowTooltipOnConstruction)
		{
			if (select)
			{
				OnPointerEnter(null);
			}
			else
			{
				OnPointerExit(null);
			}
		}
		EventSystem.current.SetSelectedGameObject(select ? ((Component)button).gameObject : null);
	}

	public void SetInteractable(bool interactable, TheLastStand.Model.Building.Construction.E_UnusableActionCause unusableActionCause)
	{
		UnusableActionCause = unusableActionCause;
		((Selectable)button).interactable = interactable;
	}

	protected virtual void OnDestroy()
	{
		((UnityEventBase)button.onClick).RemoveAllListeners();
	}
}
