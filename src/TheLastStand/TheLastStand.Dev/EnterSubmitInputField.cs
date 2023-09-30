using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.Dev;

[RequireComponent(typeof(InputField))]
public class EnterSubmitInputField : MonoBehaviour
{
	[Serializable]
	public class InteractionEvent : UnityEvent<string>
	{
	}

	private InputField inputField;

	[SerializeField]
	private InteractionEvent onInteraction = new InteractionEvent();

	private bool wasFocused;

	private void Awake()
	{
		inputField = ((Component)this).GetComponent<InputField>();
	}

	private void Update()
	{
		if (wasFocused && inputField.text != string.Empty && (Input.GetKey((KeyCode)13) || Input.GetKey((KeyCode)271)))
		{
			((UnityEvent<string>)onInteraction).Invoke(inputField.text);
			inputField.text = string.Empty;
			((Selectable)inputField).Select();
			inputField.ActivateInputField();
		}
		wasFocused = inputField.isFocused;
	}
}
