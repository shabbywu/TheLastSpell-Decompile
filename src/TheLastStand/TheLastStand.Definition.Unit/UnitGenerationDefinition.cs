using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Database.Unit;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit;

public class UnitGenerationDefinition : Definition
{
	public List<string> PlayableUnitGenerationDefinitionArchetypeIds { get; set; }

	public UnitGenerationLevelDefinition UnitGenerationLevelDefinition { get; set; }

	public UnitGenerationDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = container.Element(XName.op_Implicit("PlayableUnitGenerationDefinitions"));
		if (val == null)
		{
			Debug.LogError((object)"UnitGenerationStartDefinition does not contain any PlayableUnitGenerationDefinitions");
			return;
		}
		PlayableUnitGenerationDefinitionArchetypeIds = new List<string>();
		foreach (XElement item in ((XContainer)val).Elements(XName.op_Implicit("PlayableUnitGenerationDefinition")))
		{
			XAttribute val2 = item.Attribute(XName.op_Implicit("ArchetypeId"));
			if (val2 == null)
			{
				Debug.LogError((object)"PlayableUnitGenerationDefinition must have an attribute ArchetypeId");
			}
			else
			{
				PlayableUnitGenerationDefinitionArchetypeIds.Add(val2.Value);
			}
		}
		XElement val3 = container.Element(XName.op_Implicit("UnitGenerationLevelDefinition"));
		if (val3 == null)
		{
			Debug.LogError((object)"UnitGenerationStartDefinition does not contain any UnitGenerationLevelDefinition");
			return;
		}
		XAttribute val4 = val3.Attribute(XName.op_Implicit("Id"));
		if (XDocumentExtensions.IsNullOrEmpty(val4))
		{
			Debug.LogError((object)"UnitGenerationLevelDefinition must have a valid Id");
		}
		else if (!PlayableUnitDatabase.UnitGenerationLevelDefinitions.ContainsKey(val4.Value))
		{
			Debug.LogError((object)("PlayableUnitManager.UnitGenerationLevelDefinitions does not contain this id: " + val4.Value));
		}
		else
		{
			UnitGenerationLevelDefinition = PlayableUnitDatabase.UnitGenerationLevelDefinitions[val4.Value];
		}
	}
}
