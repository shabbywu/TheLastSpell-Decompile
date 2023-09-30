using System;
using System.Collections;
using System.Runtime.CompilerServices;
using DG.Tweening;
using TPLib;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TheLastStand.View.HUD.BottomScreenPanel.BuildingManagement;

public abstract class BuildingCapacityPanel : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IJoystickSkillConfirmHandler, IJoystickSelect
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static TweenCallback _003C_003E9__27_0;

		internal void _003CSelect_003Eb__27_0()
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: false);
		}
	}

	[SerializeField]
	[FormerlySerializedAs("buildingSkillRect")]
	private RectTransform buildingCapacityRect;

	[SerializeField]
	[FormerlySerializedAs("buildingSkillsPanel")]
	protected BuildingCapacitiesPanel buildingCapacitiesPanel;

	[SerializeField]
	protected BetterButton button;

	[SerializeField]
	protected BetterButton confirmButton;

	[SerializeField]
	protected Canvas confirmButtonCanvas;

	[SerializeField]
	protected Animator selectorAnimator;

	[SerializeField]
	protected Canvas selectorCanvas;

	[SerializeField]
	protected Transform tooltipAnchor;

	[SerializeField]
	protected JoystickHighlighter joystickHighlighter;

	private bool confirmedThisFrame;

	private Tween confirmButtonTween;

	public RectTransform BuildingCapacityRect => buildingCapacityRect;

	public bool IsConfirmSelected { get; set; }

	public virtual void DisplayTooltip(bool show)
	{
		if (show)
		{
			OnPointerEnter(null);
		}
		else
		{
			OnPointerExit(null);
		}
	}

	public abstract void OnSkillPanelHovered(bool hover);

	public abstract void Refresh();

	public void DeselectConfirmButton(bool deselectAll)
	{
		if (IsConfirmSelected)
		{
			if (TPSingleton<HUDJoystickNavigationManager>.Instance.ShowTooltips && deselectAll)
			{
				OnPointerExit(null);
			}
			IsConfirmSelected = false;
			EventSystem.current.SetSelectedGameObject(GetButton());
			buildingCapacitiesPanel.ChangeSelectedCapacityPanel(null);
			if ((Object)(object)joystickHighlighter != (Object)null)
			{
				joystickHighlighter.OnHighlight();
			}
		}
	}

	public void DisplaySelector(bool display)
	{
		((Behaviour)selectorCanvas).enabled = display;
		((Behaviour)selectorAnimator).enabled = display;
	}

	public bool IsDisplayed()
	{
		return ((Component)BuildingCapacityRect).gameObject.activeSelf;
	}

	public void OnDisplayTooltip(bool display)
	{
		if (!InputManager.JoystickConfig.HUDNavigation.AlwaysShowTooltipOnBuildingSkill)
		{
			DisplayTooltip(display);
		}
	}

	public virtual void OnSkillHover(bool select)
	{
		if (TPSingleton<HUDJoystickNavigationManager>.Instance.ShowTooltips || InputManager.JoystickConfig.HUDNavigation.AlwaysShowTooltipOnBuildingSkill)
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
		EventSystem.current.SetSelectedGameObject(select ? GetButton() : null);
		if (select)
		{
			GameView.BottomScreenPanel.BuildingManagementPanel.BuildingCapacitiesPanel.OnCapacityHovered(this);
			if ((Object)(object)joystickHighlighter != (Object)null)
			{
				joystickHighlighter.OnHighlight();
			}
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		OnSkillPanelHovered(hover: true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		OnSkillPanelHovered(hover: false);
	}

	public void Select(bool select)
	{
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Expected O, but got Unknown
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Expected O, but got Unknown
		DisplaySelector(select);
		if (!((Object)(object)confirmButton != (Object)null))
		{
			return;
		}
		Tween obj = confirmButtonTween;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
		confirmButtonTween = (Tween)(object)DOTweenModuleUI.DOAnchorPosY(((Graphic)((Selectable)confirmButton).image).rectTransform, select ? 0f : (-87f), 0.2f, true);
		TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: true);
		if (select)
		{
			Tween obj2 = confirmButtonTween;
			object obj3 = _003C_003Ec._003C_003E9__27_0;
			if (obj3 == null)
			{
				TweenCallback val = delegate
				{
					TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: false);
				};
				_003C_003Ec._003C_003E9__27_0 = val;
				obj3 = (object)val;
			}
			TweenSettingsExtensions.OnComplete<Tween>(obj2, (TweenCallback)obj3);
			((Component)confirmButton).gameObject.SetActive(true);
			((Selectable)confirmButton).interactable = true;
			if (InputManager.IsLastControllerJoystick)
			{
				((MonoBehaviour)TPSingleton<BuildingManager>.Instance).StartCoroutine(ToggleConfirmCanvasCoroutine());
			}
		}
		else
		{
			TweenSettingsExtensions.OnComplete<Tween>(confirmButtonTween, (TweenCallback)delegate
			{
				TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.ToggleAlwaysFollow(state: false);
				((Component)confirmButton).gameObject.SetActive(false);
			});
		}
	}

	public virtual void SelectConfirmButton()
	{
		GameObject val = GetConfirmButton();
		if (!((Object)(object)val == (Object)null) && ((Selectable)button).interactable && !IsConfirmSelected && !confirmedThisFrame)
		{
			IsConfirmSelected = true;
			EventSystem.current.SetSelectedGameObject(val);
		}
	}

	protected void OnConfirmButtonClick()
	{
		((MonoBehaviour)TPSingleton<BuildingManager>.Instance).StartCoroutine(ConfirmedCoroutine());
	}

	private GameObject GetButton()
	{
		return ((Component)button).gameObject;
	}

	private GameObject GetConfirmButton()
	{
		if (!((Object)(object)confirmButton != (Object)null))
		{
			return null;
		}
		return ((Component)confirmButton).gameObject;
	}

	private IEnumerator ToggleConfirmCanvasCoroutine()
	{
		if (!((Object)(object)confirmButtonCanvas == (Object)null))
		{
			Canvas obj = confirmButtonCanvas;
			int sortingOrder = obj.sortingOrder;
			obj.sortingOrder = sortingOrder + 1;
			yield return null;
			Canvas obj2 = confirmButtonCanvas;
			sortingOrder = obj2.sortingOrder;
			obj2.sortingOrder = sortingOrder - 1;
		}
	}

	private IEnumerator ConfirmedCoroutine()
	{
		confirmedThisFrame = true;
		yield return null;
		confirmedThisFrame = false;
	}
}
