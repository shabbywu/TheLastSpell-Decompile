using TPLib;
using TheLastStand.Framework.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.PlayableUnitCustomisation;

public abstract class Handler : RandomizableCustomizationElement
{
	[SerializeField]
	protected Image dropdownBackground;

	[SerializeField]
	protected BetterButton prevTextureButton;

	[SerializeField]
	protected BetterButton nextTextureButton;

	[SerializeField]
	protected BetterToggle lockToggle;

	[SerializeField]
	protected BetterButton randomizeButton;

	[SerializeField]
	protected Color interactableColor;

	[SerializeField]
	protected Color uninteractableColor;

	[SerializeField]
	private GameObject nextOptionInputDisplay;

	[SerializeField]
	private GameObject previousOptionInputDisplay;

	protected UnityAction<int> onValueChanged;

	public BetterToggle LockToggle => lockToggle;

	public abstract bool IsDropdownOpen { get; }

	public abstract void ChangeCurrentValue();

	public abstract void DecreaseCurrentValue();

	public abstract void IncreaseCurrentValue();

	public void MarkAsJoystickSelectedHandler()
	{
		TPSingleton<PlayableUnitCustomisationPanel>.Instance.SetHandlerAsJoystickTarget(this);
		if ((Object)(object)nextOptionInputDisplay != (Object)null)
		{
			nextOptionInputDisplay.gameObject.SetActive(true);
		}
		if ((Object)(object)previousOptionInputDisplay != (Object)null)
		{
			previousOptionInputDisplay.gameObject.SetActive(true);
		}
	}

	public void UnmarkAsJoystickSelectedHandler()
	{
		TPSingleton<PlayableUnitCustomisationPanel>.Instance.SetHandlerAsJoystickTarget(null);
		if ((Object)(object)nextOptionInputDisplay != (Object)null)
		{
			nextOptionInputDisplay.gameObject.SetActive(false);
		}
		if ((Object)(object)previousOptionInputDisplay != (Object)null)
		{
			previousOptionInputDisplay.gameObject.SetActive(false);
		}
	}

	public override void RandomizeValue(bool useWeights)
	{
	}

	public virtual void SwitchHandlerLockState(bool state)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		nextTextureButton.Interactable = state;
		((Graphic)((Selectable)nextTextureButton).image).CrossFadeColor(state ? interactableColor : uninteractableColor, 0.2f, false, true);
		prevTextureButton.Interactable = state;
		((Graphic)((Selectable)prevTextureButton).image).CrossFadeColor(state ? interactableColor : uninteractableColor, 0.2f, false, true);
		randomizeButton.Interactable = state;
		((Graphic)((Selectable)randomizeButton).image).CrossFadeColor(state ? interactableColor : uninteractableColor, 0.2f, false, true);
		((Selectable)lockToggle).interactable = state;
		((Graphic)((Selectable)lockToggle).image).CrossFadeColor(state ? interactableColor : uninteractableColor, 0.2f, false, true);
		((Graphic)dropdownBackground).CrossFadeColor(state ? interactableColor : uninteractableColor, 0.2f, false, true);
	}

	private void Awake()
	{
		UnmarkAsJoystickSelectedHandler();
	}
}
