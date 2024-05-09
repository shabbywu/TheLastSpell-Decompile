using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit;

public class UnitStartingRosterRacesDistributionDefinition : TheLastStand.Framework.Serialization.Definition
{
	public static class Constants
	{
		public const string NonHumanRaceType = "NonHuman";
	}

	public int UnlockedNonHumanRacesNb { get; private set; }

	public int HumanWeight { get; private set; }

	public int NonHumanWeight { get; private set; }

	public UnitStartingRosterRacesDistributionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("UnlockedNonHumanRacesNb"));
		if (!int.TryParse(val.Value, out var result))
		{
			CLoggerManager.Log((object)("The UnitStartingRosterRacesDistributionDefinition " + val.Value + " " + HasAnInvalidInt(val.Value)), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
		}
		UnlockedNonHumanRacesNb = result;
		foreach (XElement item in obj.Elements(XName.op_Implicit("RaceDistribution")))
		{
			XAttribute val2 = item.Attribute(XName.op_Implicit("RaceType"));
			if (val2 == null)
			{
				CLoggerManager.Log((object)"A RaceDistribution has no RaceType !", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				continue;
			}
			if (val2.Value != "Human" && val2.Value != "NonHuman")
			{
				CLoggerManager.Log((object)("A RaceDistribution has an invalid RaceType: " + val2.Value), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				continue;
			}
			string value = val2.Value;
			XAttribute val3 = item.Attribute(XName.op_Implicit("Weight"));
			if (val3 == null)
			{
				CLoggerManager.Log((object)"A RaceDistribution has no Weight !", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				continue;
			}
			if (!int.TryParse(val3.Value, out var result2))
			{
				CLoggerManager.Log((object)("A RaceDistribution " + val3.Value + " " + HasAnInvalidInt(val3.Value)), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			}
			switch (value)
			{
			case "Human":
				HumanWeight = result2;
				break;
			case "NonHuman":
				NonHumanWeight = result2;
				break;
			}
		}
	}
}
