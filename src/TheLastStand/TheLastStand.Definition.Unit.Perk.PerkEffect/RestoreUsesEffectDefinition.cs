using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.Item;
using TheLastStand.Framework.ExpressionInterpreter;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Perk.PerkEffect;

public class RestoreUsesEffectDefinition : APerkEffectDefinition
{
	public static class Constants
	{
		public const string Id = "RestoreUses";
	}

	public ItemSlotDefinition.E_ItemSlotId SlotType { get; private set; }

	public Node ValueExpression { get; private set; }

	public RestoreUsesEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("Value"));
		ValueExpression = Parser.Parse(val.Value, base.TokenVariables);
		XAttribute val2 = ((XElement)obj).Attribute(XName.op_Implicit("SlotId"));
		if (Enum.TryParse<ItemSlotDefinition.E_ItemSlotId>(val2.Value, out var result))
		{
			SlotType = result;
			return;
		}
		SlotType = ItemSlotDefinition.E_ItemSlotId.None;
		CLoggerManager.Log((object)("Could not parse SlotId attribute into a E_ItemSlotId. Value is \"" + val2.Value + "\""), (LogType)0, (CLogLevel)2, true, "RestoreUsesEffectDefinition", false);
	}
}
