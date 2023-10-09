using System;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.Definition.Apocalypse.ApocalypseEffects;

public class IncreasePricesApocalypseEffectDefinition : ApocalypseEffectDefinition
{
	public ResourceManager.E_PriceModifierType Type { get; private set; }

	public int Value { get; private set; }

	public IncreasePricesApocalypseEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		if (container != null)
		{
			base.Deserialize(container);
			XContainer obj = ((container is XElement) ? container : null);
			XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("Type"));
			if (!Enum.TryParse<ResourceManager.E_PriceModifierType>(val.Value, out var result))
			{
				CLoggerManager.Log((object)("An Apocalypse's IncreasePrices Effect " + HasAnInvalid("E_PriceModifierType", val.Value)), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
			}
			Type = result;
			XAttribute val2 = ((XElement)obj).Attribute(XName.op_Implicit("Value"));
			if (!int.TryParse(val2.Value, out var result2))
			{
				CLoggerManager.Log((object)("An Apocalypse's IncreasePrices Effect " + HasAnInvalidInt(val2.Value)), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
			}
			Value = result2;
		}
	}
}
