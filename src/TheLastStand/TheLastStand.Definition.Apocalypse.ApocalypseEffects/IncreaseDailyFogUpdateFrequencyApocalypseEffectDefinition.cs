using System.Xml.Linq;
using UnityEngine;

namespace TheLastStand.Definition.Apocalypse.ApocalypseEffects;

public class IncreaseDailyFogUpdateFrequencyApocalypseEffectDefinition : ApocalypseEffectDefinition
{
	public int Value { get; private set; }

	public IncreaseDailyFogUpdateFrequencyApocalypseEffectDefinition(XContainer xContainer)
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
				Debug.LogError((object)("Apocalypse's IncreaseDailyFogUpdateFrequency Effect " + HasAnInvalidInt(val.Value)));
			}
			Value = result;
		}
	}
}
