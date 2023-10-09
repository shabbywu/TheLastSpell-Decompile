using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Database.Building;
using TheLastStand.Definition.Building.BuildingAction;
using TheLastStand.Definition.Building.BuildingGaugeEffect;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Definition.Building.Module;

public class ProductionModuleDefinition : BuildingModuleDefinition
{
	public static class Constants
	{
		public const int LevelDefaultValue = 1;
	}

	public List<BuildingActionDefinition> BuildingActionDefinitions { get; private set; }

	public BuildingGaugeEffectDefinition BuildingGaugeEffectDefinition { get; private set; }

	public int Level { get; private set; } = 1;


	public ProductionModuleDefinition(BuildingDefinition buildingDefinition, XContainer productionDefinition)
		: base(buildingDefinition, productionDefinition)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		if (val == null)
		{
			return;
		}
		XElement val2 = ((XContainer)val).Element(XName.op_Implicit("Level"));
		if (val2 != null)
		{
			if (val2.IsNullOrEmpty())
			{
				Debug.LogError((object)("Building " + BuildingDefinition.Id + " has an invalid Level !"));
				return;
			}
			if (!int.TryParse(val2.Value, out var result))
			{
				Debug.LogError((object)("Building " + BuildingDefinition.Id + "'s Level " + HasAnInvalidInt(val2.Value)));
				return;
			}
			Level = result;
		}
		XElement val3 = ((XContainer)val).Element(XName.op_Implicit("BuildingGaugeEffectDefinition"));
		if (val3 != null)
		{
			using IEnumerator<XElement> enumerator = ((XContainer)val3).Elements().GetEnumerator();
			if (enumerator.MoveNext())
			{
				XElement current = enumerator.Current;
				if (BuildingDatabase.BuildingGaugeEffectDefinitions.TryGetValue(current.Name.LocalName, out var value))
				{
					BuildingGaugeEffectDefinition = value.Clone();
					XAttribute val4 = val3.Attribute(XName.op_Implicit("TriggeredOnConstruction"));
					if (val4 != null)
					{
						BuildingGaugeEffectDefinition.TriggeredOnConstruction = bool.Parse(val4.Value);
					}
					switch (BuildingGaugeEffectDefinition.Id)
					{
					case "CreateItem":
						(BuildingGaugeEffectDefinition as CreateItemGaugeEffectDefinition).CreateItemDefinition = new CreateItemDefinition((XContainer)(object)current);
						break;
					case "GlobalUpgradeStat":
						(BuildingGaugeEffectDefinition as UpgradeStatGaugeEffectDefinition).UpgradeStatDefinition = new UpgradeStatDefinition((XContainer)(object)current);
						break;
					}
				}
				else
				{
					Debug.LogError((object)("BuildingGaugeEffectDefinition " + current.Name.LocalName + " not found"));
				}
			}
		}
		XElement val5 = ((XContainer)val).Element(XName.op_Implicit("BuildingActionDefinitions"));
		if (val5 == null)
		{
			return;
		}
		BuildingActionDefinitions = new List<BuildingActionDefinition>();
		foreach (XElement item2 in ((XContainer)val5).Elements(XName.op_Implicit("BuildingActionDefinition")))
		{
			XAttribute val6 = item2.Attribute(XName.op_Implicit("Id"));
			if (val6.IsNullOrEmpty())
			{
				Debug.LogError((object)("BuildingDefinition " + BuildingDefinition.Id + " BuildingActionDefinition must have a valid string"));
			}
			if (BuildingDatabase.BuildingActionDefinitions.TryGetValue(val6.Value, out var value2))
			{
				BuildingActionDefinition item = value2.Clone();
				BuildingActionDefinitions.Add(item);
				continue;
			}
			Debug.LogError((object)("BuildingActionDefinition " + val6.Value + " not found"));
			break;
		}
	}
}
