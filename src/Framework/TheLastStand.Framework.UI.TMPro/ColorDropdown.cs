using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.Framework.UI.TMPro;

public class ColorDropdown : TMP_BetterDropdown
{
	[Serializable]
	public class ColorOptionData : OptionData
	{
		[SerializeField]
		protected Material m_material;

		[SerializeField]
		protected Color m_color;

		public Material Material
		{
			get
			{
				return m_material;
			}
			set
			{
				m_material = value;
			}
		}

		public Color Color
		{
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				return m_color;
			}
			set
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				m_color = value;
			}
		}

		public ColorOptionData(string text, Sprite image, Material material)
			: base(text, image)
		{
			Material = material;
		}

		public ColorOptionData(string text, Sprite image, Color color)
			: base(text, image)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			Color = color;
		}
	}

	protected override DropdownItem AddItem(OptionData data, bool selected, DropdownItem itemTemplate, List<DropdownItem> items)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		DropdownItem dropdownItem = base.AddItem(data, selected, itemTemplate, items);
		if (data is ColorOptionData colorOptionData)
		{
			if ((Object)(object)colorOptionData.Material != (Object)null)
			{
				((Graphic)dropdownItem.image).material = colorOptionData.Material;
			}
			else
			{
				((Graphic)dropdownItem.image).color = colorOptionData.Color;
			}
		}
		return dropdownItem;
	}

	public void AddOptions(List<ColorOptionData> options)
	{
		base.options.AddRange(options);
		RefreshShownValue();
	}

	public override void RefreshShownValue()
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		base.RefreshShownValue();
		if (((base.options.Count > 0) ? base.options[Mathf.Clamp(base.value, 0, base.options.Count - 1)] : null) is ColorOptionData colorOptionData)
		{
			if ((Object)(object)colorOptionData.Material != (Object)null)
			{
				((Graphic)base.captionImage).material = colorOptionData.Material;
			}
			else
			{
				((Graphic)base.captionImage).color = colorOptionData.Color;
			}
		}
	}
}
