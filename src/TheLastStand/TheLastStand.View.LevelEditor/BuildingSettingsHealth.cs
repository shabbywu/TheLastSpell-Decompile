using TMPro;
using TheLastStand.Definition.Building;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.LevelEditor;

public class BuildingSettingsHealth : MonoBehaviour
{
	[SerializeField]
	private Slider slider;

	[SerializeField]
	private TextMeshProUGUI healthText;

	private BuildingDefinition buildingDefinition;

	public int CurrentHealth => Mathf.RoundToInt(buildingDefinition.DamageableModuleDefinition.NativeHealthTotal * slider.value);

	public bool IsFullHealth => slider.value == 1f;

	public void Init(BuildingDefinition buildingDefinition)
	{
		this.buildingDefinition = buildingDefinition;
		OnSliderValueChanged(1f);
	}

	public void SetHealth(int value)
	{
		float nativeHealthTotal = buildingDefinition.DamageableModuleDefinition.NativeHealthTotal;
		slider.value = (float)value / nativeHealthTotal;
		((TMP_Text)healthText).text = $"{value}/{nativeHealthTotal}";
	}

	private void Awake()
	{
		((UnityEvent<float>)(object)slider.onValueChanged).AddListener((UnityAction<float>)OnSliderValueChanged);
	}

	private void OnSliderValueChanged(float value)
	{
		float nativeHealthTotal = buildingDefinition.DamageableModuleDefinition.NativeHealthTotal;
		((TMP_Text)healthText).text = $"{Mathf.RoundToInt(nativeHealthTotal * value)}/{nativeHealthTotal}";
	}
}
