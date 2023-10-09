using System;
using System.Globalization;
using System.Xml.Linq;
using TheLastStand.Controller.Meta;
using TheLastStand.Definition.Meta;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager.Meta;
using UnityEngine;

namespace TheLastStand.Definition.Unit.PlayableUnitGeneration;

public class StatGenerationDefinition : TheLastStand.Framework.Serialization.Definition
{
	private string Archetype { get; set; }

	private Vector2 Boundaries { get; set; }

	public UnitStatDefinition.E_Stat Stat { get; private set; }

	public StatGenerationDefinition(XContainer container, string archetype)
		: base(container)
	{
		Archetype = archetype;
	}

	public override void Deserialize(XContainer container)
	{
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		XContainer obj = ((container is XElement) ? container : null);
		Stat = (UnitStatDefinition.E_Stat)Enum.Parse(value: ((XElement)obj).Attribute(XName.op_Implicit("Stat")).Value, enumType: typeof(UnitStatDefinition.E_Stat));
		float num = float.Parse(obj.Element(XName.op_Implicit("Min")).Value, NumberStyles.Float, CultureInfo.InvariantCulture);
		float num2 = float.Parse(obj.Element(XName.op_Implicit("Max")).Value, NumberStyles.Float, CultureInfo.InvariantCulture);
		Boundaries = new Vector2(num, num2);
	}

	public Vector2 GetBoundaries()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = Boundaries;
		if (MetaUpgradeEffectsController.TryGetEffectsOfType<UnitAttributeModifierMetaEffectDefinition>(out var effects, MetaUpgradesManager.E_MetaState.Activated))
		{
			for (int num = effects.Length - 1; num >= 0; num--)
			{
				UnitAttributeModifierMetaEffectDefinition unitAttributeModifierMetaEffectDefinition = effects[num];
				if ((unitAttributeModifierMetaEffectDefinition.Archetype == Archetype || unitAttributeModifierMetaEffectDefinition.AllArchetypes) && unitAttributeModifierMetaEffectDefinition.StatAndValue.TryGetValue(Stat, out var value))
				{
					val += value;
				}
			}
		}
		return val;
	}
}
