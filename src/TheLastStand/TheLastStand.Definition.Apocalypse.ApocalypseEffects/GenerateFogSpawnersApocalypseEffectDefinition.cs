using System.Globalization;
using System.Xml.Linq;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.Apocalypse.ApocalypseEffects;

public class GenerateFogSpawnersApocalypseEffectDefinition : ApocalypseEffectDefinition
{
	public float Multiplier { get; private set; }

	public GenerateFogSpawnersApocalypseEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		if (container == null)
		{
			return;
		}
		base.Deserialize(container);
		XAttribute val = ((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("Multiplier"));
		if (val != null)
		{
			if (!float.TryParse(val.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
			{
				CLoggerManager.Log((object)("Could not parse Multiplier attribute into a float : " + val.Value), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				Multiplier = 1f;
			}
			else
			{
				Multiplier = result;
			}
		}
		else
		{
			Multiplier = 1f;
		}
	}
}
