using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition;
using TheLastStand.Framework.Database;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Database;

public class BarkDatabase : Database<BarkDatabase>
{
	[SerializeField]
	private TextAsset barkDefinitionsTextAsset;

	public static Dictionary<string, BarkDefinition> BarkDefinitions { get; private set; }

	public override void Deserialize(XContainer container = null)
	{
		if (BarkDefinitions != null)
		{
			return;
		}
		XElement val = ((XContainer)XDocument.Parse(barkDefinitionsTextAsset.text, (LoadOptions)2)).Element(XName.op_Implicit("BarkDefinitions"));
		if (val.IsNullOrEmpty())
		{
			CLoggerManager.Log((object)("The document " + ((Object)barkDefinitionsTextAsset).name + " must have BarkDefinitions!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		BarkDefinitions = new Dictionary<string, BarkDefinition>();
		foreach (XElement item in ((XContainer)val).Elements(XName.op_Implicit("BarkDefinition")))
		{
			BarkDefinition barkDefinition = new BarkDefinition((XContainer)(object)item);
			BarkDefinitions.Add(barkDefinition.Id, barkDefinition);
		}
	}
}
