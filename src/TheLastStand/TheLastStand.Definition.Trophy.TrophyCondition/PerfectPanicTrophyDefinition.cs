using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Manager;

namespace TheLastStand.Definition.Trophy.TrophyCondition;

public class PerfectPanicTrophyDefinition : ValueIntTrophyConditionDefinition
{
	public const string Name = "PerfectPanic";

	public PerfectPanicTrophyDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		if (!int.TryParse(((XElement)((container is XElement) ? container : null)).Value, out var result))
		{
			((CLogger<TrophyManager>)TPSingleton<TrophyManager>.Instance).LogError((object)"The Value of an Element: PerfectPanic in TrophiesDefinitions isn't a valid int", (CLogLevel)1, true, true);
		}
		else
		{
			base.Value = result;
		}
	}

	public override string ToString()
	{
		return "PerfectPanic";
	}
}
