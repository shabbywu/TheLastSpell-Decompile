using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Perk.PerkAction;

public class RefreshPerkActivationFeedbackDefinition : APerkActionDefinition
{
	public static class Constants
	{
		public const string Id = "RefreshPerkActivationFeedback";
	}

	public bool RefreshView { get; private set; }

	public RefreshPerkActivationFeedbackDefinition(XContainer container, Dictionary<string, string> tokenVariables)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XAttribute val = ((XElement)((container is XElement) ? container : null)).Attribute(XName.op_Implicit("RefreshView"));
		if (val != null)
		{
			if (bool.TryParse(val.Value, out var result))
			{
				RefreshView = result;
			}
			else
			{
				CLoggerManager.Log((object)"Could not parse RefreshView attribute into a bool in RefreshPerkActivationFeedbackDefinition", (LogType)0, (CLogLevel)2, true, "RefreshPerkActivationFeedbackDefinition", false);
			}
		}
		else
		{
			RefreshView = true;
		}
	}
}
