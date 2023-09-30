using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Definition.WorldMap;
using TheLastStand.Framework.Database;
using UnityEngine;

namespace TheLastStand.Database.WorldMap;

public class CityDatabase : Database<CityDatabase>
{
	[SerializeField]
	private TextAsset[] cityDefinitionsTextAssets;

	public static Dictionary<string, CityDefinition> CityDefinitions { get; set; }

	public override void Deserialize(XContainer container = null)
	{
		if (CityDefinitions != null)
		{
			return;
		}
		Queue<XElement> queue = base.GatherElements((IEnumerable<TextAsset>)cityDefinitionsTextAssets, (IEnumerable<TextAsset>)null, "CityDefinition", (string)null);
		IEnumerable<XElement> enumerable = base.SortElementsByDependencies((IEnumerable<XElement>)queue);
		CityDefinitions = new Dictionary<string, CityDefinition>();
		foreach (XElement item in enumerable)
		{
			CityDefinition cityDefinition = new CityDefinition((XContainer)(object)item);
			CityDefinitions.Add(cityDefinition.Id, cityDefinition);
		}
	}
}
