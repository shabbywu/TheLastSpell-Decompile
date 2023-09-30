using TPLib;
using TheLastStand.Definition.Unit;
using TheLastStand.Manager;
using TheLastStand.View.Generic;
using TheLastStand.View.HUD;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.Unit.Injury;

public class UnitInjuryDisplay : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[SerializeField]
	private RectTransform injuryRect;

	[SerializeField]
	private GameObject raycaster;

	[SerializeField]
	private Image injuryImage;

	[SerializeField]
	private Sprite injuryOn;

	[SerializeField]
	private Sprite injuryOff;

	[SerializeField]
	private Sprite injuryOnHover;

	[SerializeField]
	private Sprite injuryOffHover;

	[SerializeField]
	private JoystickSelectable joystickSelectable;

	[SerializeField]
	private FollowElement.FollowDatas tooltipFollowData;

	private InjuryDefinition injuryDefinition;

	private int injuryIndex;

	private bool isOn;

	private bool isJoystickSelected;

	public JoystickSelectable JoystickSelectable => joystickSelectable;

	public void Refresh(bool isOn, float xPos, InjuryDefinition injuryDefinition, int injuryIndex)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		this.isOn = isOn;
		injuryImage.sprite = (isOn ? injuryOn : injuryOff);
		injuryRect.anchoredPosition = new Vector2(xPos, injuryRect.anchoredPosition.y);
		this.injuryDefinition = injuryDefinition;
		this.injuryIndex = injuryIndex;
	}

	public void ToggleRaycaster(bool state)
	{
		raycaster.SetActive(state);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		injuryImage.sprite = (isOn ? injuryOnHover : injuryOffHover);
		DisplayTooltip();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		injuryImage.sprite = (isOn ? injuryOn : injuryOff);
		HideTooltip();
	}

	public void OnJoystickSelect()
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		isJoystickSelected = true;
		injuryImage.sprite = (isOn ? injuryOnHover : injuryOffHover);
		if (TPSingleton<HUDJoystickNavigationManager>.Instance.ShowTooltips)
		{
			FollowElement followElement = UIManager.InjuryTooltip.FollowElement;
			followElement.FollowElementDatas.FollowTarget = tooltipFollowData.FollowTarget;
			followElement.FollowElementDatas.Offset = tooltipFollowData.Offset;
			DisplayTooltip();
		}
	}

	public void OnJoystickDeselect()
	{
		isJoystickSelected = false;
		FollowElement followElement = UIManager.InjuryTooltip.FollowElement;
		followElement.FollowElementDatas.FollowTarget = null;
		followElement.RestoreFollowDatasOffset();
		OnPointerExit(null);
	}

	private void Awake()
	{
		TPSingleton<HUDJoystickNavigationManager>.Instance.TooltipsToggled += OnTooltipsToggled;
	}

	private void OnDestroy()
	{
		JoystickSelectable.ClearEvents();
		if (TPSingleton<HUDJoystickNavigationManager>.Exist())
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.TooltipsToggled -= OnTooltipsToggled;
		}
	}

	private void OnTooltipsToggled(bool state)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if (isJoystickSelected)
		{
			if (state)
			{
				FollowElement followElement = UIManager.InjuryTooltip.FollowElement;
				followElement.FollowElementDatas.FollowTarget = tooltipFollowData.FollowTarget;
				followElement.FollowElementDatas.Offset = tooltipFollowData.Offset;
				DisplayTooltip();
			}
			else
			{
				HideTooltip();
			}
		}
	}

	private void DisplayTooltip()
	{
		UIManager.InjuryTooltip.SetContent(injuryDefinition, injuryIndex);
		UIManager.InjuryTooltip.Display();
	}

	private void HideTooltip()
	{
		UIManager.InjuryTooltip.Hide();
	}
}
