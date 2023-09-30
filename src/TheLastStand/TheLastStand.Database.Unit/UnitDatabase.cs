using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.Database;
using UnityEngine;

namespace TheLastStand.Database.Unit;

public class UnitDatabase : Database<UnitDatabase>
{
	[SerializeField]
	private TextAsset unitStatDefinitions;

	[SerializeField]
	private TextAsset pathfindingDefinitionTextAsset;

	public static PathfindingDefinition PathfindingDefinition { get; private set; }

	public static Dictionary<UnitStatDefinition.E_Stat, UnitStatDefinition> UnitStatDefinitions { get; private set; }

	public static float MagicDamagePercentageResistanceReduction { get; private set; }

	public override void Deserialize(XContainer container = null)
	{
		XElement val = ((XContainer)XDocument.Parse(unitStatDefinitions.text, (LoadOptions)2)).Element(XName.op_Implicit("UnitStatDefinitions"));
		UnitStatDefinitions = new Dictionary<UnitStatDefinition.E_Stat, UnitStatDefinition>(UnitStatDefinition.SharedStatComparer);
		foreach (XElement item in ((XContainer)val).Elements(XName.op_Implicit("UnitStatDefinition")))
		{
			UnitStatDefinition unitStatDefinition = new UnitStatDefinition((XContainer)(object)item);
			UnitStatDefinitions.Add(unitStatDefinition.Id, unitStatDefinition);
		}
		if (float.TryParse(((XContainer)val).Element(XName.op_Implicit("MagicDamagePercentageResistanceReduction")).Attribute(XName.op_Implicit("Value")).Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			MagicDamagePercentageResistanceReduction = result;
		}
		else
		{
			CLoggerManager.Log((object)"Could not parse Value attribute into a float for MagicDamagePercentageResistanceReduction in UnitDatabase", (LogType)0, (CLogLevel)2, true, "UnitDatabase", false);
		}
		PathfindingDefinition = new PathfindingDefinition((XContainer)(object)((XContainer)XDocument.Parse(pathfindingDefinitionTextAsset.text, (LoadOptions)2)).Element(XName.op_Implicit("PathfindingDefinition")));
	}
}
