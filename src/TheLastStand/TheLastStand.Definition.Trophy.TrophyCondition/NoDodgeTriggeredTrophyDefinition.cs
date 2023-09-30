using System.Xml.Linq;
using TPLib;
using UnityEngine;

namespace TheLastStand.Definition.Trophy.TrophyCondition;

public class NoDodgeTriggeredTrophyDefinition : ValueIntHeroesTrophyConditionDefinition
{
	public const string Name = "NoDodgeTriggered";

	public NoDodgeTriggeredTrophyDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		if (!int.TryParse(((XElement)((container is XElement) ? container : null)).Value, out var result))
		{
			TPDebug.LogError((object)"The Value of an Element : NoDodgeTriggered in TrophiesDefinitions isn't a valid int", (Object)null);
		}
		else
		{
			base.Value = result;
		}
	}

	public override string ToString()
	{
		return "NoDodgeTriggered";
	}
}
