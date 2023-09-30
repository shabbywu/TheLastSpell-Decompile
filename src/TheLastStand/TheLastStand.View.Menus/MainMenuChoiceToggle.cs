using TMPro;
using TheLastStand.Framework.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.Menus;

public class MainMenuChoiceToggle : MonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler, ISubmitHandler
{
	[SerializeField]
	private TextMeshProUGUI choiceText;

	[SerializeField]
	private BetterToggle choiceToggle;

	[SerializeField]
	private UnityEvent onSubmit;

	private void Start()
	{
		((UnityEvent<bool>)(object)((Toggle)choiceToggle).onValueChanged).AddListener((UnityAction<bool>)ChangeFontSize);
	}

	private void ChangeFontSize(bool isOn)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		((Transform)((TMP_Text)choiceText).rectTransform).localScale = Vector3.one * (isOn ? 1.2f : 1f);
	}

	public void Submit()
	{
		UnityEvent obj = onSubmit;
		if (obj != null)
		{
			obj.Invoke();
		}
	}

	public void OnSelect(BaseEventData eventData)
	{
		((Toggle)choiceToggle).isOn = true;
	}

	public void OnDeselect(BaseEventData eventData)
	{
		((Toggle)choiceToggle).isOn = false;
	}

	public void OnSubmit(BaseEventData eventData)
	{
		Submit();
	}
}
