using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Localization.Fonts;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Settings.KeyRemapping;

public class KeyRemappingBindingView : MonoBehaviour
{
	[SerializeField]
	private BetterButton button;

	[SerializeField]
	private TextMeshProUGUI bindingText;

	[SerializeField]
	private TextMeshProUGUI remappingFeedback;

	[SerializeField]
	private GameObject selector;

	[SerializeField]
	private DataColor hoverColor;

	[SerializeField]
	private TextMeshProUGUI debugKeyCodeText;

	[SerializeField]
	private LocalizedFont localizedFont;

	private Color initColor;

	private KeyCode keyCode;

	public BetterButton Button => button;

	public void DisplayRemappingFeedback(bool show)
	{
		((Component)remappingFeedback).gameObject.SetActive(show);
		if (show)
		{
			SetText(string.Empty);
			((TMP_Text)remappingFeedback).text = Localizer.Get("KeyRemapping_PressAnyKeyToBind");
		}
	}

	public void Highlight(bool state)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (!TPSingleton<KeyRemappingManager>.Instance.RemappingInProgress)
		{
			selector.SetActive(state);
			((Graphic)bindingText).color = (state ? hoverColor._Color : initColor);
		}
	}

	public void RefreshLocalizedKeyCode()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		string text = default(string);
		if (Localizer.TryGet($"KeyCode_{keyCode}", ref text) && !string.IsNullOrEmpty(text))
		{
			((TMP_Text)bindingText).text = text;
		}
		LocalizedFont obj = localizedFont;
		if (obj != null)
		{
			obj.RefreshFont();
		}
	}

	public void RefreshText(string defaultName)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		string text = default(string);
		SetText((Localizer.TryGet($"KeyCode_{keyCode}", ref text) && !string.IsNullOrEmpty(text)) ? text : defaultName);
	}

	public void SetKeyCode(KeyCode keyCode)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		this.keyCode = keyCode;
		((TMP_Text)debugKeyCodeText).text = (((int)keyCode == 0) ? string.Empty : ((object)(KeyCode)(ref keyCode)).ToString());
	}

	public void SetText(string text)
	{
		((TMP_Text)bindingText).text = text;
	}

	private void Awake()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		initColor = ((Graphic)bindingText).color;
	}

	public void DebugShowRawKeyCode(bool show)
	{
		((Component)debugKeyCodeText).gameObject.SetActive(show);
	}
}
