using Rewired;
using TPLib;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.View.Tooltip;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.HUD;

public class JoystickSelectable : Selectable, ISelectHandler, IEventSystemHandler, IDeselectHandler
{
	[SerializeField]
	private UnityEvent onSelect;

	[SerializeField]
	private UnityEvent onDeselect;

	[SerializeField]
	private BoolEvent onTooltipsToggled;

	[SerializeField]
	private TooltipDisplayer tooltipDisplayer;

	public TooltipDisplayer TooltipDisplayer => tooltipDisplayer;

	public override void OnSelect(BaseEventData eventData)
	{
		((Selectable)this).OnSelect(eventData);
		UnityEvent obj = onSelect;
		if (obj != null)
		{
			obj.Invoke();
		}
		if ((Object)(object)tooltipDisplayer != (Object)null && TPSingleton<HUDJoystickNavigationManager>.Instance.ShowTooltips)
		{
			tooltipDisplayer.DisplayTooltip();
		}
	}

	public override void OnDeselect(BaseEventData eventData)
	{
		if ((Object)(object)tooltipDisplayer != (Object)null && tooltipDisplayer.Displayed)
		{
			tooltipDisplayer.HideTooltip();
		}
		((Selectable)this).OnDeselect(eventData);
		UnityEvent obj = onDeselect;
		if (obj != null)
		{
			obj.Invoke();
		}
	}

	public void AddListenerOnSelect(UnityAction unityAction)
	{
		onSelect.AddListener(unityAction);
	}

	public void AddListenerOnDeselect(UnityAction unityAction)
	{
		onDeselect.AddListener(unityAction);
	}

	public void RemoveListenerOnSelect(UnityAction unityAction)
	{
		onSelect.RemoveListener(unityAction);
	}

	public void RemoveListenerOnDeselect(UnityAction unityAction)
	{
		onDeselect.RemoveListener(unityAction);
	}

	public void ClearEvents()
	{
		if (TPSingleton<HUDJoystickNavigationManager>.Exist())
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.TooltipsToggled -= OnTooltipsToggled;
		}
		if (TPSingleton<InputManager>.Exist())
		{
			TPSingleton<InputManager>.Instance.LastActiveControllerChanged -= OnLastActiveControllerChanged;
		}
	}

	protected override void Awake()
	{
		((Selectable)this).Awake();
		if (TPSingleton<HUDJoystickNavigationManager>.Exist())
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.TooltipsToggled += OnTooltipsToggled;
		}
		TPSingleton<InputManager>.Instance.LastActiveControllerChanged += OnLastActiveControllerChanged;
	}

	private void OnTooltipsToggled(bool showTooltips)
	{
		if ((Object)(object)this == (Object)null || (Object)(object)EventSystem.current.currentSelectedGameObject != (Object)(object)((Component)this).gameObject)
		{
			return;
		}
		if ((Object)(object)tooltipDisplayer != (Object)null)
		{
			if (showTooltips)
			{
				tooltipDisplayer.DisplayTooltip();
			}
			else
			{
				tooltipDisplayer.HideTooltip();
			}
		}
		((UnityEvent<bool>)(object)onTooltipsToggled)?.Invoke(showTooltips);
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
				((Selectable)(object)this).SetMode((Mode)4);
				return;
			}
			if ((int)controllerType == 20)
			{
			}
		}
		((Selectable)(object)this).SetMode((Mode)0);
	}
}
