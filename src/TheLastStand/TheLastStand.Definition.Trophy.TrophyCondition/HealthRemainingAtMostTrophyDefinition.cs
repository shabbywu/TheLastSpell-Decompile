using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Manager;

namespace TheLastStand.Definition.Trophy.TrophyCondition;

public class HealthRemainingAtMostTrophyDefinition : ValueIntHeroesTrophyConditionDefinition
{
	public const string Name = "HealthRemainingAtMost";

	public HealthRemainingAtMostTrophyDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		if (!int.TryParse(((XElement)((container is XElement) ? container : null)).Value, out var result))
		{
			((CLogger<TrophyManager>)TPSingleton<TrophyManager>.Instance).LogError((object)"The Value of an Element : HealthRemainingAtMost in TrophiesDefinitions isn't a valid float", (CLogLevel)1, true, true);
		}
		else
		{
			base.Value = result;
		}
	}

	public override string ToString()
	{
		return "HealthRemainingAtMost";
	}
}
