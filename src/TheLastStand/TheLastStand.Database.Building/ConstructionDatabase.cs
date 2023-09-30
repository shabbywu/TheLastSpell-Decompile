using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.Building;
using TheLastStand.Framework.Database;
using UnityEngine;

namespace TheLastStand.Database.Building;

public class ConstructionDatabase : Database<ConstructionDatabase>
{
	[SerializeField]
	private TextAsset constructionDefinition;

	public static ConstructionDefinition ConstructionDefinition;

	public override void Deserialize(XContainer container = null)
	{
		if (ConstructionDefinition == null)
		{
			XElement val = ((XContainer)XDocument.Parse(constructionDefinition.text, (LoadOptions)2)).Element(XName.op_Implicit("ConstructionDefinition"));
			if (val == null)
			{
				CLoggerManager.Log((object)"ConstructionDefinitionDocument must have a ConstructionDefinition", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			}
			else
			{
				ConstructionDefinition = new ConstructionDefinition((XContainer)(object)val);
			}
		}
	}
}
