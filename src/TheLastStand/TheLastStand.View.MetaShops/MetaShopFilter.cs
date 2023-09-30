using System;
using TMPro;
using TheLastStand.Definition.Meta;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.MetaShops;

public class MetaShopFilter : MonoBehaviour
{
	[SerializeField]
	private MetaUpgradeDefinition.E_MetaUpgradeFilter filter;

	[SerializeField]
	private Selectable selectable;

	[SerializeField]
	private Image iconImage;

	[SerializeField]
	private Sprite iconSpriteOn;

	[SerializeField]
	private Sprite iconSpriteOff;

	[SerializeField]
	private TextMeshProUGUI title;

	[SerializeField]
	private float titleDisabledOpacity = 0.5f;

	[SerializeField]
	private MetaShopFilter exclusiveLinkedFilter;

	public Selectable Selectable => selectable;

	public bool Toggled { get; private set; }

	public MetaUpgradeDefinition.E_MetaUpgradeFilter Filter => filter;

	public event Action<MetaUpgradeDefinition.E_MetaUpgradeFilter, bool> OnToggled;

	public void OnClick()
	{
		Toggle(!Toggled);
		if (Toggled && (Object)(object)exclusiveLinkedFilter != (Object)null && exclusiveLinkedFilter.Toggled)
		{
			exclusiveLinkedFilter.OnClick();
		}
		this.OnToggled?.Invoke(filter, Toggled);
	}

	public void Toggle(bool toggle)
	{
		Toggled = toggle;
		ToggleView(Toggled);
	}

	private void ToggleView(bool toggle)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		iconImage.sprite = (toggle ? iconSpriteOn : iconSpriteOff);
		Color color = ((Graphic)title).color;
		color.a = (toggle ? 1f : titleDisabledOpacity);
		((Graphic)title).color = color;
	}
}
