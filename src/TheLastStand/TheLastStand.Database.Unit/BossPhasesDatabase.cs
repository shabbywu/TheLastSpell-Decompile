using System.Collections.Generic;
using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Definition.Unit.Enemy.Boss;
using TheLastStand.Framework.Database;
using UnityEngine;

namespace TheLastStand.Database.Unit;

public class BossPhasesDatabase : Database<BossPhasesDatabase>
{
	[SerializeField]
	private TextAsset bossPhasesDefinitionsTextAsset;

	public static Dictionary<string, BossPhasesDefinition> BossPhasesDefinitions { get; private set; }

	public override void Deserialize(XContainer container = null)
	{
		if (BossPhasesDefinitions != null)
		{
			return;
		}
		XElement val = ((XContainer)XDocument.Parse(TPSingleton<BossPhasesDatabase>.Instance.bossPhasesDefinitionsTextAsset.text, (LoadOptions)2)).Element(XName.op_Implicit("BossPhasesDefinitions"));
		if (val == null)
		{
			CLoggerManager.Log((object)"The document has no BossPhasesDefinitions!", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		BossPhasesDefinitions = new Dictionary<string, BossPhasesDefinition>();
		foreach (XElement item in ((XContainer)val).Elements(XName.op_Implicit("BossPhasesDefinition")))
		{
			BossPhasesDefinition bossPhasesDefinition = new BossPhasesDefinition((XContainer)(object)item);
			BossPhasesDefinitions.Add(bossPhasesDefinition.Id, bossPhasesDefinition);
		}
	}
}
