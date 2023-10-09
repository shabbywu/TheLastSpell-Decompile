using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Building;

public class ConstructionDefinition : TheLastStand.Framework.Serialization.Definition
{
	public float RepairCostRatio { get; private set; }

	public Dictionary<string, List<BuildingDefinition.E_BuildingCategory>> RepairCategoryButtons { get; } = new Dictionary<string, List<BuildingDefinition.E_BuildingCategory>>();


	public ConstructionDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XElement val2 = ((XContainer)val).Element(XName.op_Implicit("RepairCostRatio"));
		if (val2 == null)
		{
			CLoggerManager.Log((object)"ConstructionDefinition must have a RepairCostRatio", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		if (!float.TryParse(val2.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			CLoggerManager.Log((object)"ConstructionDefinition RepairCostRatio must be a valid float", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		RepairCostRatio = result * 0.01f;
		foreach (XElement item in ((XContainer)((XContainer)val).Element(XName.op_Implicit("RepairCategoryButtons"))).Elements(XName.op_Implicit("RepairCategoryButton")))
		{
			XAttribute val3 = item.Attribute(XName.op_Implicit("Id"));
			List<BuildingDefinition.E_BuildingCategory> list = new List<BuildingDefinition.E_BuildingCategory>();
			foreach (XElement item2 in ((XContainer)item).Elements(XName.op_Implicit("FlagId")))
			{
				if (!Enum.TryParse<BuildingDefinition.E_BuildingCategory>(item2.Value, out var result2))
				{
					CLoggerManager.Log((object)("Could not parse " + item2.Value + " as a valid E_BuildingCategory."), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
				}
				else
				{
					list.Add(result2);
				}
			}
			RepairCategoryButtons.Add(val3.Value, list);
		}
	}
}
