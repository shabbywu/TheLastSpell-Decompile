using System.Xml.Linq;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Apocalypse.ApocalypseEffects;

public class IncreaseEnemiesNumberApocalypseEffectDefinition : ApocalypseEffectDefinition
{
	public int Value { get; private set; }

	public IncreaseEnemiesNumberApocalypseEffectDefinition(XContainer container)
		: base(container)
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
				Debug.LogError((object)("Apocalypse's IncreaseEnemiesNumber Effect " + ((Definition)this).HasAnInvalidInt(val.Value)));
			}
			Value = result;
		}
	}
}
