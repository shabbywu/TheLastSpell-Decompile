using System.Xml.Linq;
using UnityEngine;

namespace TheLastStand.Definition.Apocalypse;

public class RunCompletedInCityConditionDefinition : ApocalypseUnlockConditionDefinition
{
	public const string RunCompletedInCityName = "RunCompletedInCity";

	public string CityDefinitionId { get; private set; }

	public override string Name => "RunCompletedInCity";

	public int RunCompleted { get; private set; }

	public RunCompletedInCityConditionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("Id"));
		XAttribute val2 = ((XElement)obj).Attribute(XName.op_Implicit("Value"));
		if (!int.TryParse(val2.Value, out var result))
		{
			Debug.LogError((object)("RunCompletedInCity " + val2.Value + " " + HasAnInvalidInt(val2.Value)));
		}
		CityDefinitionId = val.Value;
		RunCompleted = result;
	}
}
