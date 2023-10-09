using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Item;

public class AffixLevelsDefinition : TheLastStand.Framework.Serialization.Definition
{
	public Dictionary<int, Dictionary<int, float>> AffixLevelsProbas { get; private set; } = new Dictionary<int, Dictionary<int, float>>();


	public Dictionary<int, Dictionary<AffixMalusDefinition.E_MalusLevel, float>> AffixMalusLevelsProbas { get; private set; } = new Dictionary<int, Dictionary<AffixMalusDefinition.E_MalusLevel, float>>();


	public AffixLevelsDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		foreach (XElement item in ((container is XElement) ? container : null).Elements(XName.op_Implicit("ItemLevel")))
		{
			XAttribute val = item.Attribute(XName.op_Implicit("Id"));
			if (val.IsNullOrEmpty())
			{
				Debug.LogError((object)"AffixLevelsDefinition ItemLevel must have an Id");
				continue;
			}
			if (!int.TryParse(val.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
			{
				CLoggerManager.Log((object)"AffixLevelsDefinition ItemLevel must have a valid Id (int)", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				continue;
			}
			if (AffixLevelsProbas.ContainsKey(result))
			{
				CLoggerManager.Log((object)$"AffixLevelsDefinition already have this Id : {result}", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				continue;
			}
			AffixLevelsProbas.Add(result, new Dictionary<int, float>());
			foreach (XElement item2 in ((XContainer)item).Elements(XName.op_Implicit("AffixLevelProba")))
			{
				XAttribute val2 = item2.Attribute(XName.op_Implicit("Id"));
				if (val2.IsNullOrEmpty())
				{
					CLoggerManager.Log((object)$"AffixLevelsDefinition {result}'s AffixLevelProba must have an Id", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					continue;
				}
				if (!int.TryParse(val2.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result2) && result2 > 0 && result2 < 4)
				{
					CLoggerManager.Log((object)$"AffixLevelsDefinition {result}'s AffixLevelProba {HasAnInvalidInt(val2.Value)}", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					continue;
				}
				if (AffixLevelsProbas[result].ContainsKey(result2))
				{
					CLoggerManager.Log((object)$"AffixLevelsDefinition ItemLevel with id {result} already have an AffixLevelProba this Id : {result2}", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					continue;
				}
				if (item2.IsEmpty || !float.TryParse(item2.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result3))
				{
					CLoggerManager.Log((object)"AffixLevelsDefinition AffixLevelProba must be a valid float", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					return;
				}
				AffixLevelsProbas[result].Add(result2, result3);
			}
		}
		foreach (KeyValuePair<int, Dictionary<int, float>> affixLevelsProba in AffixLevelsProbas)
		{
			AffixMalusLevelsProbas.Add(affixLevelsProba.Key, new Dictionary<AffixMalusDefinition.E_MalusLevel, float>(AffixMalusDefinition.SharedMalusLevelComparer));
			foreach (KeyValuePair<int, float> item3 in affixLevelsProba.Value)
			{
				AffixMalusLevelsProbas[affixLevelsProba.Key].Add((AffixMalusDefinition.E_MalusLevel)item3.Key, item3.Value);
			}
		}
	}
}
