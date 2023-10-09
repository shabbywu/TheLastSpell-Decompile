using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.Item;
using TheLastStand.Framework.ExpressionInterpreter;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Perk.PerkEffect;

public class EquipmentSlotModifierEffectDefinition : APerkEffectDefinition
{
	public static class Constants
	{
		public const string Id = "EquipmentSlotModifier";
	}

	public ItemSlotDefinition.E_ItemSlotId SlotId { get; set; }

	public Node ValueExpression { get; set; }

	public EquipmentSlotModifierEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("SlotId"));
		if (!Enum.TryParse<ItemSlotDefinition.E_ItemSlotId>(val.Value, out var result))
		{
			CLoggerManager.Log((object)("An EquipmentSlotModifierEffectDefinition has an invalid Id " + val.Value + "!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
		}
		SlotId = result;
		XAttribute val2 = ((XElement)obj).Attribute(XName.op_Implicit("Value"));
		if (val2 != null)
		{
			ValueExpression = Parser.Parse(val2.Value, base.TokenVariables);
		}
	}
}
