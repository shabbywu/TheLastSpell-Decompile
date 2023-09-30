using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.Trophy;
using TheLastStand.Framework.Database;
using UnityEngine;

namespace TheLastStand.Database;

public class TrophyDatabase : Database<TrophyDatabase>
{
	[SerializeField]
	private TextAsset trophiesDefinitions;

	[SerializeField]
	private TextAsset trophyConfig;

	public static DefaultTrophyDefinition DefaultTrophyDefinition { get; private set; }

	public static TrophyConfigDefinition TrophyConfigDefinition { get; private set; }

	public static List<TrophyDefinition> TrophyDefinitions { get; private set; }

	public static TrophyConfigDefinition.GemStageData GetGemStageData(uint damnedSoulsValue)
	{
		foreach (TrophyConfigDefinition.GemStageData gemStageData in TrophyConfigDefinition.GemStageDatas)
		{
			if (damnedSoulsValue >= gemStageData.Min && damnedSoulsValue <= gemStageData.Max)
			{
				return gemStageData;
			}
		}
		return TrophyConfigDefinition.GemStageDatas.Last();
	}

	public override void Deserialize(XContainer container = null)
	{
		DeserializeTrophyDefinitions();
		DeserializeTrophyConfig();
	}

	private void DeserializeTrophyDefinitions()
	{
		if (TrophyDefinitions != null)
		{
			return;
		}
		TrophyDefinitions = new List<TrophyDefinition>();
		XElement val = ((XContainer)XDocument.Parse(trophiesDefinitions.text, (LoadOptions)2)).Element(XName.op_Implicit("TrophiesDefinitions"));
		if (val == null)
		{
			CLoggerManager.Log((object)"TrophiesDefinitions.xml doesn't contains an element named : TrophiesDefinitions", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		foreach (XElement item2 in ((XContainer)val).Elements(XName.op_Implicit("TrophyDefinition")))
		{
			TrophyDefinition item = new TrophyDefinition((XContainer)(object)item2);
			TrophyDefinitions.Add(item);
		}
		XElement val2 = ((XContainer)val).Element(XName.op_Implicit("DefaultTrophyDefinition"));
		if (val2 != null)
		{
			DefaultTrophyDefinition = new DefaultTrophyDefinition((XContainer)(object)val2);
		}
	}

	private void DeserializeTrophyConfig()
	{
		if (TrophyConfigDefinition == null)
		{
			XElement val = ((XContainer)XDocument.Parse(trophyConfig.text, (LoadOptions)2)).Element(XName.op_Implicit("TrophyConfig"));
			if (val == null)
			{
				CLoggerManager.Log((object)("There is no TrophyConfig Element in " + ((Object)trophyConfig).name + " text file."), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			}
			else
			{
				TrophyConfigDefinition = new TrophyConfigDefinition((XContainer)(object)val);
			}
		}
	}
}
