using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.Fog;
using TheLastStand.Framework.Database;
using UnityEngine;

namespace TheLastStand.Database.Fog;

public class FogDatabase : Database<FogDatabase>
{
	[SerializeField]
	private TextAsset[] fogDefinitionsTextAssets;

	[SerializeField]
	private TextAsset lightFogDefinition;

	public static Dictionary<string, FogDefinition> FogsDefinitions { get; private set; }

	public static LightFogDefinition LightFogDefinition { get; private set; }

	public override void Deserialize(XContainer container = null)
	{
		if (FogsDefinitions != null)
		{
			return;
		}
		Queue<XElement> elements = GatherElements(fogDefinitionsTextAssets, null, "FogDefinition");
		IEnumerable<XElement> enumerable = SortElementsByDependencies(elements);
		FogsDefinitions = new Dictionary<string, FogDefinition>();
		foreach (XElement item in enumerable)
		{
			FogDefinition fogDefinition = new FogDefinition((XContainer)(object)item);
			FogsDefinitions.Add(fogDefinition.Id, fogDefinition);
		}
		XElement val = ((XContainer)XDocument.Parse(lightFogDefinition.text, (LoadOptions)2)).Element(XName.op_Implicit("LightFogDefinition"));
		if (val.IsEmpty)
		{
			CLoggerManager.Log((object)("The document " + ((Object)lightFogDefinition).name + " must have LightFogDefinition!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
		}
		else
		{
			LightFogDefinition = new LightFogDefinition((XContainer)(object)val);
		}
	}
}
