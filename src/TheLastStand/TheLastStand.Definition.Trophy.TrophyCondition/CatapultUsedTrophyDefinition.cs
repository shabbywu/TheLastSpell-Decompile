using System.Xml.Linq;
using TPLib;
using UnityEngine;

namespace TheLastStand.Definition.Trophy.TrophyCondition;

public class CatapultUsedTrophyDefinition : ValueIntTrophyConditionDefinition
{
	public const string Name = "CatapultUsed";

	public CatapultUsedTrophyDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		if (!int.TryParse(((XElement)((container is XElement) ? container : null)).Value, out var result))
		{
			TPDebug.LogError((object)"The Value of an Element : CatapultUsed in TrophiesDefinitions isn't a valid int", (Object)null);
		}
		else
		{
			base.Value = result;
		}
	}

	public override string ToString()
	{
		return "CatapultUsed";
	}
}
