using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Tutorial;

public abstract class TutorialConditionDefinition : Definition
{
	public bool Invert { get; private set; }

	public TutorialConditionDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute val = ((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("Invert"));
		if (val != null)
		{
			if (!bool.TryParse(val.Value, out var result))
			{
				CLoggerManager.Log((object)("Could not parse TutorialConditionDefinition Invert attribute value " + val.Value + " to a valid bool!"), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
			}
			else
			{
				Invert = result;
			}
		}
		else
		{
			Invert = false;
		}
	}
}
