using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.Building;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Brazier;

public class BrazierDefinition : TheLastStand.Framework.Serialization.Definition
{
	private static class Constants
	{
		public const string BraziersAmountsElement = "BraziersAmounts";

		public const string BraziersAmountElement = "BraziersAmount";

		public const string BraziersToSpawnElement = "BraziersToSpawn";

		public const string BrazierToSpawnElement = "BrazierToSpawn";

		public const string GuardiansElement = "Guardians";

		public const string GuardianElement = "GuardiansGroupsToSpawn";

		public const string IdAttribute = "Id";

		public const string NightAttribute = "Night";
	}

	private List<BraziersAmountPerNight> braziersAmounts;

	private List<BrazierToSpawnPerNight> braziersToSpawn;

	private List<GuardiansGroupsToSpawnPerNight> guardiansGroupsToSpawnPerNights;

	public string Id { get; private set; }

	public BrazierDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public int GetBraziersAmountToSpawn()
	{
		return NightIndexedItem.GetLastNightIndexedItem(braziersAmounts).Amount;
	}

	public BuildingDefinition GetBrazierToSpawn()
	{
		return NightIndexedItem.GetLastNightIndexedItem(braziersToSpawn).BrazierDefinition;
	}

	public GuardiansGroupsToSpawn GetGuardiansGroupsToSpawn()
	{
		return NightIndexedItem.GetLastNightIndexedItem(guardiansGroupsToSpawnPerNights).GuardiansGroupsToSpawn;
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XAttribute val2 = val.Attribute(XName.op_Implicit("Id"));
		Id = val2.Value;
		DeserializeBraziersAmounts(((XContainer)val).Element(XName.op_Implicit("BraziersAmounts")));
		DeserializeBraziersToSpawn(((XContainer)val).Element(XName.op_Implicit("BraziersToSpawn")));
		DeserializeGuardians(((XContainer)val).Element(XName.op_Implicit("Guardians")));
	}

	private void DeserializeBraziersAmounts(XElement xBraziersAmounts)
	{
		braziersAmounts = DeserializeNightIndexedItems<BraziersAmountPerNight>(xBraziersAmounts, "BraziersAmount");
	}

	private void DeserializeBraziersToSpawn(XElement xBraziersToSpawn)
	{
		braziersToSpawn = DeserializeNightIndexedItems<BrazierToSpawnPerNight>(xBraziersToSpawn, "BrazierToSpawn");
	}

	private void DeserializeGuardians(XElement xGuardians)
	{
		guardiansGroupsToSpawnPerNights = DeserializeNightIndexedItems<GuardiansGroupsToSpawnPerNight>(xGuardians, "GuardiansGroupsToSpawn");
	}

	private List<T> DeserializeNightIndexedItems<T>(XElement xElement, string elementName) where T : NightIndexedItem, new()
	{
		List<T> list = new List<T>();
		foreach (XElement item in ((XContainer)xElement).Elements(XName.op_Implicit(elementName)))
		{
			XAttribute val = item.Attribute(XName.op_Implicit("Night"));
			if (!int.TryParse(val.Value, out var result))
			{
				CLoggerManager.Log((object)("Night attribute in BrazierDefinition (" + Id + ") could not be parsed into an int (" + val.Value + "). Skipped."), (LogType)0, (CLogLevel)2, true, "BrazierDefinition", false);
			}
			else if (list.Count > 0 && result <= list[^1].NightIndex)
			{
				CLoggerManager.Log((object)string.Format("{0} attribute in {1} ({2}) is inferior or equal to the previous element ({3} <= {4}). It should always be ascending. Skipped.", "Night", "BrazierDefinition", Id, result, list[^1].NightIndex), (LogType)0, (CLogLevel)2, true, "BrazierDefinition", false);
			}
			else
			{
				T val2 = new T();
				val2.Init(result, item);
				list.Add(val2);
			}
		}
		return list;
	}
}
