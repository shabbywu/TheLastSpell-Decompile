using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TheLastStand.View.Generic;

public class SliderWithDisplayedValue : MonoBehaviour
{
	[SerializeField]
	private string key = "<sprite name=DamnedSouls>XXX";

	[SerializeField]
	[FormerlySerializedAs("ValueDisplayer")]
	private TextMeshProUGUI valueDisplayer;

	[SerializeField]
	private Slider slider;

	[SerializeField]
	private RectTransform boxRect;

	private void Awake()
	{
		((UnityEvent<float>)(object)slider.onValueChanged)?.AddListener((UnityAction<float>)delegate(float x)
		{
			OnValueChanged(x);
		});
	}

	public void OnValueChanged(float value)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		((TMP_Text)valueDisplayer).text = key.Replace("XXX", (slider.maxValue - value).ToString());
		boxRect.sizeDelta = new Vector2(((TMP_Text)valueDisplayer).preferredWidth, boxRect.sizeDelta.y);
	}

	[ContextMenu("ChangeValue")]
	private void Test()
	{
		Slider obj = slider;
		float value = obj.value;
		obj.value = value + 1f;
	}
}
