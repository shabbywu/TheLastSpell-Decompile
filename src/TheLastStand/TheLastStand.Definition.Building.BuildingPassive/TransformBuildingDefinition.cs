using System.Xml.Linq;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.Building.BuildingPassive;

public class TransformBuildingDefinition : BuildingPassiveEffectDefinition
{
	public string BuildingId { get; private set; }

	public bool Instantaneous { get; private set; }

	public bool PlayDestructionSmoke { get; private set; }

	public TransformBuildingDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		BuildingId = ((XContainer)val).Element(XName.op_Implicit("BuildingId")).Value;
		XElement val2 = ((XContainer)val).Element(XName.op_Implicit("Instantaneous"));
		if (val2 != null)
		{
			if (!bool.TryParse(val2.Value, out var result))
			{
				CLoggerManager.Log((object)"Could not parse TransformBuildingDefinition Instantaneous value to a valid bool.", (LogType)3, (CLogLevel)1, true, "StaticLog", false);
				Instantaneous = false;
			}
			else
			{
				Instantaneous = result;
			}
		}
		XElement val3 = ((XContainer)val).Element(XName.op_Implicit("PlayDestructionSmoke"));
		if (val3 != null)
		{
			if (!bool.TryParse(val3.Value, out var result2))
			{
				CLoggerManager.Log((object)"Could not parse TransformBuildingDefinition PlayDestructionSmoke value to a valid bool.", (LogType)3, (CLogLevel)1, true, "StaticLog", false);
				PlayDestructionSmoke = false;
			}
			else
			{
				PlayDestructionSmoke = result2;
			}
		}
	}
}
