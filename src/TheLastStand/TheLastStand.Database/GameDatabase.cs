using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.Cutscene;
using TheLastStand.Definition.Night;
using TheLastStand.Framework.Database;
using TheLastStand.Framework.Extensions;
using UnityEngine;
using UnityEngine.Serialization;

namespace TheLastStand.Database;

public class GameDatabase : Database<GameDatabase>
{
	[SerializeField]
	private TextAsset nightReportRankDefinitionTextAsset;

	[FormerlySerializedAs("victoryCutsceneDefinitionsTextAssets")]
	[SerializeField]
	private TextAsset[] cutsceneDefinitionsTextAssets;

	public static List<NightReportRankDefinition> NightReportRankDefinitions { get; private set; }

	public static Dictionary<string, CutsceneDefinition> CutsceneDefinitions { get; private set; } = new Dictionary<string, CutsceneDefinition>();


	public override void Deserialize(XContainer container = null)
	{
		DeserializeNightReportRankDefinition();
		DeserializeVictorySequenceDefinitions();
	}

	private void DeserializeVictorySequenceDefinitions()
	{
		Queue<XElement> queue = base.GatherElements((IEnumerable<TextAsset>)cutsceneDefinitionsTextAssets, (IEnumerable<TextAsset>)null, "CutsceneDefinition", (string)null);
		Queue<XElement> queue2 = base.GatherElements((IEnumerable<TextAsset>)cutsceneDefinitionsTextAssets, (IEnumerable<TextAsset>)null, "UnitCutsceneDefinition", "CutsceneDefinitions");
		while (queue2.Count > 0)
		{
			queue.Enqueue(queue2.Dequeue());
		}
		foreach (XElement item in base.SortElementsByDependencies((IEnumerable<XElement>)queue))
		{
			CutsceneDefinition cutsceneDefinition = new CutsceneDefinition((XContainer)(object)item);
			CutsceneDefinitions.Add(cutsceneDefinition.Id, cutsceneDefinition);
		}
	}

	private void DeserializeNightReportRankDefinition()
	{
		XElement val = ((XContainer)XDocument.Parse(nightReportRankDefinitionTextAsset.text, (LoadOptions)2)).Element(XName.op_Implicit("NightReportRankDefinitions"));
		if (XDocumentExtensions.IsNullOrEmpty(val))
		{
			CLoggerManager.Log((object)("The document " + ((Object)nightReportRankDefinitionTextAsset).name + " must have a NightReportRankDefinitions element!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		NightReportRankDefinitions = new List<NightReportRankDefinition>();
		foreach (XElement item in ((XContainer)val).Elements(XName.op_Implicit("NightReportRankDefinition")))
		{
			NightReportRankDefinitions.Add(new NightReportRankDefinition((XContainer)(object)item));
		}
	}
}
