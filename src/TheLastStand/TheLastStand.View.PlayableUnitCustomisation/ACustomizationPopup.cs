using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using TPLib;
using TPLib.Localization.Fonts;
using TPLib.UI;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.View.Camera;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.PlayableUnitCustomisation;

public abstract class ACustomizationPopup : MonoBehaviour, IOverlayUser
{
	public enum E_ErrorCause
	{
		None,
		MinSize,
		MaxSize,
		WrongCharacter,
		InvalidLayerValue
	}

	[SerializeField]
	protected Canvas canvas;

	[SerializeField]
	protected CanvasGroup canvasGroup;

	[SerializeField]
	protected TextMeshProUGUI errorText;

	[SerializeField]
	protected TMP_InputField inputField;

	[SerializeField]
	protected SimpleFontLocalizedParent fontLocalizedParent;

	[SerializeField]
	protected BetterButton validateButton;

	[SerializeField]
	protected BetterButton closeButton;

	[SerializeField]
	private Selectable joystickTargetOnClosed;

	private Tween fadeTween;

	protected string previousValue;

	public virtual int OverlaySortingOrder => canvas.sortingOrder - 2;

	public virtual void Close()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		CameraView.AttenuateWorldForPopupFocus((IOverlayUser)(object)TPSingleton<PlayableUnitCustomisationPanel>.Instance);
		Tween obj = fadeTween;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
		fadeTween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(DOTweenModuleUI.DOFade(canvasGroup, 0f, 0.25f), (TweenCallback)delegate
		{
			((Behaviour)canvas).enabled = false;
			if ((Object)(object)joystickTargetOnClosed != (Object)null && InputManager.IsLastControllerJoystick)
			{
				EventSystem.current.SetSelectedGameObject(((Component)joystickTargetOnClosed).gameObject);
			}
		});
	}

	public abstract void OnCloseButtonClicked();

	public abstract void OnValidateButtonClicked();

	public virtual void OnValueChanged(string value)
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (CheckValidity(value) != 0)
		{
			((Selectable)validateButton).interactable = false;
			((Graphic)((Selectable)validateButton).image).color = new Color(1f, 1f, 1f, 0.35f);
		}
		else
		{
			((Selectable)validateButton).interactable = true;
			((Graphic)((Selectable)validateButton).image).color = new Color(1f, 1f, 1f, 1f);
			((TMP_Text)errorText).text = string.Empty;
		}
	}

	public virtual void Open()
	{
		CameraView.AttenuateWorldForPopupFocus((IOverlayUser)(object)this);
		Tween obj = fadeTween;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
		((Behaviour)canvas).enabled = true;
		fadeTween = (Tween)(object)DOTweenModuleUI.DOFade(canvasGroup, 1f, 0.25f);
		if ((Object)(object)fontLocalizedParent != (Object)null)
		{
			((FontLocalizedParent)fontLocalizedParent).RefreshChilds();
		}
		((TMP_Text)errorText).text = string.Empty;
		((Selectable)inputField).Select();
		if (InputManager.IsLastControllerJoystick)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: false);
		}
	}

	protected virtual E_ErrorCause CheckValidity(string value)
	{
		return E_ErrorCause.None;
	}

	private void Start()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		((UnityEvent)((Button)validateButton).onClick).AddListener(new UnityAction(OnValidateButtonClicked));
		((UnityEvent)((Button)closeButton).onClick).AddListener(new UnityAction(OnCloseButtonClicked));
		((UnityEvent<string>)(object)inputField.onValueChanged).AddListener((UnityAction<string>)OnValueChanged);
	}

	private void OnDestroy()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		((UnityEvent)((Button)validateButton).onClick).RemoveListener(new UnityAction(OnValidateButtonClicked));
		((UnityEvent)((Button)closeButton).onClick).RemoveListener(new UnityAction(OnCloseButtonClicked));
		((UnityEvent<string>)(object)inputField.onValueChanged).RemoveListener((UnityAction<string>)OnValueChanged);
	}
}
