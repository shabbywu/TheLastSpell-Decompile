using TMPro;
using TPLib;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.View.Resource;

public class MaterialsPanel : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI materialsText;

	protected virtual void Awake()
	{
		((TMP_Text)materialsText).text = $"{TPSingleton<ResourceManager>.Instance.Materials}";
		TPSingleton<ResourceManager>.Instance.OnMaterialsChange += OnMaterialsChange;
	}

	private void OnDestroy()
	{
		if (TPSingleton<ResourceManager>.Exist())
		{
			TPSingleton<ResourceManager>.Instance.OnMaterialsChange -= OnMaterialsChange;
		}
	}

	private void OnMaterialsChange(int materials)
	{
		((TMP_Text)materialsText).text = materials.ToString();
	}
}
