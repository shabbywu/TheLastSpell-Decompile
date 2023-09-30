using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Definition;
using TheLastStand.Framework.Database;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Database;

public class TileDatabase : Database<TileDatabase>, ILegacyDeserializable
{
	[SerializeField]
	private TextAsset groundDefinitionsTextAsset;

	public static Dictionary<string, GroundDefinition> GroundDefinitions { get; private set; }

	public override void Deserialize(XContainer container = null)
	{
		if (GroundDefinitions != null)
		{
			return;
		}
		GroundDefinitions = new Dictionary<string, GroundDefinition>();
		foreach (XElement item in ((XContainer)((XContainer)XDocument.Parse(groundDefinitionsTextAsset.text, (LoadOptions)2)).Element(XName.op_Implicit("GroundDefinitions"))).Elements(XName.op_Implicit("GroundDefinition")))
		{
			GroundDefinition groundDefinition = new GroundDefinition((XContainer)(object)item);
			GroundDefinitions.Add(groundDefinition.Id, groundDefinition);
		}
	}
}
