using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Building;

public class BuildingSkillSoundDefinition : TheLastStand.Framework.Serialization.Definition
{
	public List<string> SoundsIds = new List<string>();

	public Node Delay { get; private set; }

	public string BuildingTemplateDefinitionId { get; private set; }

	public int MaximumSimultaneousSounds { get; private set; }

	public BuildingSkillSoundDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XElement val = obj.Element(XName.op_Implicit("BuildingTemplateDefinitionId"));
		XElement val2 = obj.Element(XName.op_Implicit("MaximumSimultaneousSounds"));
		XElement val3 = obj.Element(XName.op_Implicit("Sounds"));
		if (val != null && val.Attribute(XName.op_Implicit("Value")) != null)
		{
			BuildingTemplateDefinitionId = val.Attribute(XName.op_Implicit("Value")).Value;
		}
		if (val2 != null)
		{
			if (!int.TryParse(val2.Value, out var result))
			{
				CLoggerManager.Log((object)"MaximumSimultaneousSounds should be of type int !", (LogType)3, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			MaximumSimultaneousSounds = result;
		}
		if (val3 == null)
		{
			return;
		}
		foreach (XElement item in ((XContainer)val3).Elements(XName.op_Implicit("SoundId")))
		{
			if (item.Attribute(XName.op_Implicit("Value")) != null)
			{
				SoundsIds.Add(item.Attribute(XName.op_Implicit("Value")).Value);
			}
		}
		XAttribute val4 = val3.Attribute(XName.op_Implicit("Delay"));
		Delay = ((val4 != null) ? Parser.Parse(val4.Value) : new NodeNumber(0.0));
	}
}
