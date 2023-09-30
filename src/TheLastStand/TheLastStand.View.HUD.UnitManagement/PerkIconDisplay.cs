using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TPLib;
using TPLib.Localization.Fonts;
using TPLib.Yield;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.View.Generic;
using TheLastStand.View.Unit.Perk;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.HUD.UnitManagement;

public class PerkIconDisplay : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[SerializeField]
	private Image perkIcon;

	[SerializeField]
	private TextMeshProUGUI dynamicValueText;

	[SerializeField]
	private GameObject counterContainer;

	[SerializeField]
	private TextMeshProUGUI counterText;

	[SerializeField]
	private GameObject highlightSign;

	[SerializeField]
	private Canvas highlightSignCanvas;

	[SerializeField]
	private List<LocalizedFont> localizedFonts;

	[SerializeField]
	private JoystickSelectable joystickSelectable;

	[SerializeField]
	private FollowElement.FollowDatas perkTooltipFollowDatas;

	[SerializeField]
	private Color greyOutColor = Color.gray;

	private RectTransform rectTransform;

	public List<LocalizedFont> LocalizedFonts => localizedFonts;

	public JoystickSelectable JoystickSelectable => joystickSelectable;

	public RectTransform RectTransform
	{
		get
		{
			if ((Object)(object)rectTransform == (Object)null)
			{
				ref RectTransform reference = ref rectTransform;
				Transform transform = ((Component)this).transform;
				reference = (RectTransform)(object)((transform is RectTransform) ? transform : null);
			}
			return rectTransform;
		}
	}

	public Perk Perk { get; private set; }

	public static event Action<PerkIconDisplay> HighlightSignDisplayed;

	public event Action<PerkIconDisplay> Hovered;

	public event Action<PerkIconDisplay> Unhovered;

	public void Display(Perk unitPerk, bool greyOut, bool displayDynamicValue = true, bool displayCounter = true)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		Perk = unitPerk;
		perkIcon.sprite = Perk.PerkDefinition.PerkSprite;
		((Graphic)perkIcon).color = (greyOut ? greyOutColor : Color.white);
		if (displayDynamicValue && !greyOut && Perk.DisplayDynamicValue(out var value))
		{
			((TMP_Text)dynamicValueText).text = value.ToString();
			((Component)dynamicValueText).gameObject.SetActive(true);
		}
		else
		{
			((Component)dynamicValueText).gameObject.SetActive(false);
		}
		if (displayCounter && Perk.DisplayCounter(out var counter))
		{
			((TMP_Text)counterText).text = counter.ToString();
			counterContainer.SetActive(true);
		}
		else
		{
			counterContainer.SetActive(false);
		}
		((Component)this).gameObject.SetActive(true);
	}

	public void DisplayHighlightSign(bool show, bool triggerEvent)
	{
		highlightSign.SetActive(show);
		if (show && triggerEvent)
		{
			PerkIconDisplay.HighlightSignDisplayed?.Invoke(this);
		}
	}

	public void OverrideHighlightSignCanvasSorting(bool state)
	{
		highlightSignCanvas.overrideSorting = state;
	}

	public void Hide()
	{
		OnPointerExit(null);
		((Component)this).gameObject.SetActive(false);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		this.Hovered?.Invoke(this);
		((MonoBehaviour)this).StartCoroutine(DisplayTooltipDelayed());
		Perk.PerkController.DisplayRange(show: true);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		this.Unhovered?.Invoke(this);
		PlayableUnitManager.PerkTooltip.Hide();
		Perk.PerkController.DisplayRange(show: false);
	}

	public void OnJoystickSelect()
	{
		this.Hovered?.Invoke(this);
		if (TPSingleton<HUDJoystickNavigationManager>.Instance.ShowTooltips)
		{
			((MonoBehaviour)this).StartCoroutine(DisplayTooltipDelayed());
		}
		Perk.PerkController.DisplayRange(show: true);
	}

	public void OnJoystickDeselect()
	{
		OnPointerExit(null);
	}

	public void OnTooltipsToggled(bool showTooltips)
	{
		if (showTooltips)
		{
			((MonoBehaviour)this).StartCoroutine(DisplayTooltipDelayed());
		}
		else
		{
			PlayableUnitManager.PerkTooltip.Hide();
		}
	}

	private IEnumerator DisplayTooltipDelayed()
	{
		yield return SharedYields.WaitForEndOfFrame;
		PerkTooltip perkTooltip = PlayableUnitManager.PerkTooltip;
		perkTooltip.CompendiumPanel.Hide();
		perkTooltip.TooltipRectTransform.anchorMin = Vector2.zero;
		perkTooltip.TooltipRectTransform.anchorMax = Vector2.zero;
		perkTooltip.TooltipRectTransform.pivot = new Vector2(0.5f, 0f);
		perkTooltip.SetContent(Perk);
		perkTooltip.FollowElement.ChangeFollowDatas(perkTooltipFollowDatas);
		perkTooltip.FollowElement.ChangeTarget((Transform)(object)RectTransform);
		perkTooltip.Display();
	}
}
