using TheLastStand.Framework.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.LevelEditor;

public class LevelEditorButton : MonoBehaviour
{
	[SerializeField]
	private BetterButton button;

	[SerializeField]
	private Toggle toggle;

	[SerializeField]
	private GameObject highlight;

	public BetterButton Button => button;

	public Toggle Toggle => toggle;

	public void Init(string text, UnityAction onClickCallback)
	{
		InitText(text);
		InitOnClick(onClickCallback);
	}

	public void Init(string text, UnityAction onClickCallback, UnityAction<bool> onToggleValueChangedCallback)
	{
		Init(text, onClickCallback);
		InitToggleValueChanged(onToggleValueChangedCallback);
	}

	public void InitText(string text)
	{
		button.ChangeText(text);
	}

	public void AppendText(string text)
	{
		button.ChangeText(button.GetText() + text);
	}

	public void InitOnClick(UnityAction onClickCallback)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		((UnityEvent)((Button)Button).onClick).AddListener(onClickCallback);
		((UnityEvent)((Button)Button).onClick).AddListener(new UnityAction(OnButtonClick));
	}

	public void InitToggleValueChanged(UnityAction<bool> onToggleValueChangedCallback)
	{
		((UnityEvent<bool>)(object)Toggle.onValueChanged).AddListener(onToggleValueChangedCallback);
	}

	public void ToggleOff()
	{
		if ((Object)(object)Toggle != (Object)null && Toggle.isOn)
		{
			Toggle.isOn = false;
			((Selectable)Toggle).interactable = true;
			highlight.SetActive(false);
		}
	}

	private void OnButtonClick()
	{
		if ((Object)(object)Toggle != (Object)null)
		{
			Toggle.isOn = true;
			((Selectable)Toggle).interactable = false;
			highlight.SetActive(true);
		}
	}

	private void OnDisable()
	{
		ToggleOff();
	}
}
