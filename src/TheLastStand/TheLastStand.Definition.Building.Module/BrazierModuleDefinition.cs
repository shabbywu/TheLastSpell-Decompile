using System.Xml.Linq;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.Building.Module;

public class BrazierModuleDefinition : BuildingModuleDefinition
{
	private static class Constants
	{
		public const string PointsTotalElement = "PointsTotal";
	}

	public int BrazierPointsTotal { get; private set; }

	public BrazierModuleDefinition(BuildingDefinition buildingDefinition, XContainer constructionDefinition)
		: base(buildingDefinition, constructionDefinition)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		if (val != null)
		{
			XElement val2 = ((XContainer)val).Element(XName.op_Implicit("PointsTotal"));
			if (!int.TryParse(val2.Value, out var result))
			{
				CLoggerManager.Log((object)("PointsTotal couldn't be parsed into an int : " + val2.Value + ".,"), (LogType)0, (CLogLevel)2, true, "BrazierModuleDefinition", false);
			}
			BrazierPointsTotal = result;
		}
	}
}
