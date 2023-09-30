using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib;
using TheLastStand.Definition.Skill.SkillAction.BuildLocation;
using UnityEngine;

namespace TheLastStand.Definition.Skill.SkillAction;

public class BuildSkillActionDefinition : SkillActionDefinition
{
	public const string Name = "Build";

	public Dictionary<string, int> Buildings { get; set; } = new Dictionary<string, int>();


	public float Delay { get; private set; }

	public BuildLocationDefinition BuildLocationDefinition { get; private set; }

	public BuildSkillActionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement obj = container.Element(XName.op_Implicit("Build"));
		XAttribute val = obj.Attribute(XName.op_Implicit("Delay"));
		XElement val2 = ((XContainer)obj).Element(XName.op_Implicit("Location"));
		XElement val3 = ((XContainer)obj).Element(XName.op_Implicit("Buildings"));
		if (!float.TryParse(val.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			TPDebug.LogError((object)"Delay of skill build action should be of type float !", (Object)null);
		}
		Delay = result;
		foreach (XElement item in ((XContainer)val2).Elements())
		{
			string localName = item.Name.LocalName;
			if (localName != null && localName == "AroundTheCaster")
			{
				BuildLocationDefinition = new AroundTheCasterBuildLocationDefinition((XContainer)(object)item);
			}
		}
		foreach (XElement item2 in ((XContainer)val3).Elements(XName.op_Implicit("Building")))
		{
			XAttribute val4 = item2.Attribute(XName.op_Implicit("Id"));
			if (!int.TryParse(item2.Attribute(XName.op_Implicit("Weight")).Value, out var result2))
			{
				Debug.LogError((object)("Building " + val4.Value + " in a building skill action have an invalid Weight !"));
			}
			if (Buildings.ContainsKey(val4.Value))
			{
				Buildings[val4.Value] += result2;
			}
			else
			{
				Buildings.Add(val4.Value, result2);
			}
		}
	}
}
