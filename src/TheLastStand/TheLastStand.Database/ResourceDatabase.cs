using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition;
using TheLastStand.Framework.Database;
using UnityEngine;

namespace TheLastStand.Database;

public class ResourceDatabase : Database<ResourceDatabase>
{
	[SerializeField]
	private TextAsset[] resourceDefinitionsTextAssets;

	public static Dictionary<string, ResourceDefinition> ResourceDefinitions { get; private set; }

	public int FirstRunDamnedSoulsGain { get; private set; } = -1;


	public override void Deserialize(XContainer container = null)
	{
		if (ResourceDefinitions != null)
		{
			return;
		}
		ResourceDefinitions = new Dictionary<string, ResourceDefinition>();
		TextAsset[] array = resourceDefinitionsTextAssets;
		for (int i = 0; i < array.Length; i++)
		{
			XElement val = ((XContainer)XDocument.Parse(array[i].text, (LoadOptions)2)).Element(XName.op_Implicit("ResourceDefinitions"));
			foreach (XElement item in ((XContainer)val).Elements(XName.op_Implicit("ResourceDefinition")))
			{
				ResourceDefinition resourceDefinition = new ResourceDefinition((XContainer)(object)item);
				ResourceDefinitions.Add(resourceDefinition.Id, resourceDefinition);
			}
			XElement val2 = ((XContainer)val).Element(XName.op_Implicit("FirstRunDamnedSoulsGain"));
			if (val2 != null)
			{
				if (int.TryParse(val2.Value, out var result))
				{
					FirstRunDamnedSoulsGain = result;
					continue;
				}
				CLoggerManager.Log((object)"Element FirstRunDamnedSoulsGain could not be parsed into an int. Set value to 0.", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				FirstRunDamnedSoulsGain = 0;
			}
		}
		if (FirstRunDamnedSoulsGain == -1)
		{
			CLoggerManager.Log((object)"Element FirstRunDamnedSoulsGain wasn't in the given text assets. Set value to 0.", (LogType)0, (CLogLevel)1, true, "ResourceDatabase", false);
			FirstRunDamnedSoulsGain = 0;
		}
	}
}
