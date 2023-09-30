using System.Xml.Linq;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Apocalypse.ApocalypseEffects;

public class IncreaseStartingFogDensityApocalypseEffectDefinition : ApocalypseEffectDefinition
{
	public int Value { get; private set; }

	public IncreaseStartingFogDensityApocalypseEffectDefinition(XContainer xContainer)
		: base(xContainer)
	{
	}

	public override void Deserialize(XContainer container)
	{
		if (container != null)
		{
			base.Deserialize(container);
			XAttribute val = ((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("Value"));
			if (!int.TryParse(val.Value, out var result))
			{
				Debug.LogError((object)("An Apocalypse's IncreaseStartingFogDensity Effect " + ((Definition)this).HasAnInvalidInt(val.Value)));
			}
			Value = result;
		}
	}
}
