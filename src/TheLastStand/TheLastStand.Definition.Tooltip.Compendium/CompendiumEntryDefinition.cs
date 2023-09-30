using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Tooltip.Compendium;

public class CompendiumEntryDefinition : Definition
{
	public string Id { get; private set; }

	public bool DisplayLinkedEntries { get; private set; } = true;


	public CompendiumEntryDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("Id"));
		Id = val.Value;
		XElement val2 = obj.Element(XName.op_Implicit("DisplayLinkedEntries"));
		if (val2 != null)
		{
			if (bool.TryParse(val2.Value, out var result))
			{
				DisplayLinkedEntries = result;
			}
			else
			{
				CLoggerManager.Log((object)"Could not parse DisplayLinkedEntries in CompendiumEntryDefinition element into a bool", (LogType)0, (CLogLevel)2, true, "CompendiumEntryDefinition", false);
			}
		}
	}
}
