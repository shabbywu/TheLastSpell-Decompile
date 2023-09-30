using System.Xml.Linq;
using TPLib;
using UnityEngine;

namespace TheLastStand.Definition.Trophy.TrophyCondition;

public class NightCompletedTrophyDefinition : ValueIntTrophyConditionDefinition
{
	public const string Name = "NightCompleted";

	public string CityId { get; private set; }

	public NightCompletedTrophyDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("CityId"));
		CityId = val.Value;
		if (!int.TryParse(((XElement)obj).Value, out var result))
		{
			TPDebug.LogError((object)"The Value of an Element : NightCompleted in TrophiesDefinitions isn't a valid int", (Object)null);
		}
		else
		{
			base.Value = result;
		}
	}

	public override string ToString()
	{
		return "NightCompleted";
	}
}
