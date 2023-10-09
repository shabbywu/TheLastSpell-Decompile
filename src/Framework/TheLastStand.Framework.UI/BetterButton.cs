using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.Framework.UI;

public class BetterButton : Button
{
	[Serializable]
	public class BoolEvent : UnityEvent<bool>
	{
	}

	[Serializable]
	public class BetterButtonEvent : UnityEvent<BetterButton>
	{
	}

	[SerializeField]
	private UnityEvent onPointerEnter = new UnityEvent();

	[SerializeField]
	private UnityEvent onPointerExit = new UnityEvent();

	[SerializeField]
	private BoolEvent onInteractableChanged = new BoolEvent();

	[SerializeField]
	private BetterButtonEvent onSelect = new BetterButtonEvent();

	[SerializeField]
	private TextMeshProUGUI buttonText;

	[SerializeField]
	private Color nonInteractableTextColor = Color.grey;

	private Color textColorInit;

	public bool Interactable
	{
		get
		{
			return ((Selectable)this).interactable;
		}
		set
		{
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			if (((Selectable)this).interactable != value)
			{
				((UnityEvent<bool>)onInteractableChanged)?.Invoke(value);
			}
			((Selectable)this).interactable = value;
			if ((Object)(object)buttonText != (Object)null)
			{
				((Graphic)buttonText).color = (((Selectable)this).interactable ? textColorInit : nonInteractableTextColor);
			}
		}
	}

	public Color NonInteractableTextColor => nonInteractableTextColor;

	public UnityEvent OnPointerEnterEvent => onPointerEnter;

	public UnityEvent OnPointerExitEvent => onPointerExit;

	public static void InitOnCreation(BetterButton betterButton)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		((Selectable)betterButton).targetGraphic = (Graphic)(object)((Component)betterButton).GetComponent<Image>();
		betterButton.buttonText = ((Component)betterButton).GetComponentInChildren<TextMeshProUGUI>();
		Navigation val = default(Navigation);
		((Navigation)(ref val)).mode = (Mode)0;
		Navigation navigation = val;
		((Selectable)betterButton).navigation = navigation;
	}

	public void ChangeText(string newText)
	{
		if ((Object)(object)buttonText == (Object)null)
		{
			Debug.LogWarning((object)("Trying to change text of BetterButton attached to " + ((Object)((Component)this).transform).name + " though it has not been referenced. Aborting."));
		}
		else
		{
			((TMP_Text)buttonText).text = newText;
		}
	}

	public void ChangeTextColor(Color color)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)buttonText == (Object)null)
		{
			Debug.LogWarning((object)("Trying to change color of BetterButton text attached to " + ((Object)((Component)this).transform).name + " though it has not been referenced. Aborting."));
		}
		else
		{
			((Graphic)buttonText).color = color;
		}
	}

	public string GetText()
	{
		return ((TMP_Text)buttonText).text;
	}

	public void UnSelect()
	{
		((Selectable)this).DoStateTransition((SelectionState)0, true);
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		((Selectable)this).OnPointerEnter(eventData);
		if (Interactable)
		{
			onPointerEnter.Invoke();
		}
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		((Selectable)this).OnPointerExit(eventData);
		if (Interactable)
		{
			onPointerExit.Invoke();
		}
	}

	public override void OnSelect(BaseEventData eventData)
	{
		((Selectable)this).OnSelect(eventData);
		if (Interactable)
		{
			((UnityEvent<BetterButton>)onSelect)?.Invoke(this);
		}
	}

	protected override void Awake()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		((Selectable)this).Awake();
		if ((Object)(object)buttonText != (Object)null)
		{
			textColorInit = ((Graphic)buttonText).color;
			if (!((Selectable)this).interactable)
			{
				((Graphic)buttonText).color = nonInteractableTextColor;
			}
		}
	}
}
