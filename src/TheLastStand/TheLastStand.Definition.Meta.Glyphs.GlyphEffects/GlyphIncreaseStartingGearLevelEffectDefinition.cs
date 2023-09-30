using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Meta.Glyphs.GlyphEffects;

public class GlyphIncreaseStartingGearLevelEffectDefinition : GlyphEffectDefinition
{
	public const string Name = "IncreaseStartingGearLevel";

	public string LevelTreeId { get; private set; }

	public Dictionary<int, int> WeightBonusByLevelProbability { get; set; } = new Dictionary<int, int>();


	public GlyphIncreaseStartingGearLevelEffectDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		if (container == null)
		{
			return;
		}
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XAttribute val2 = val.Attribute(XName.op_Implicit("Id"));
		if (val2 == null || string.IsNullOrEmpty(val2.Value))
		{
			CLoggerManager.Log((object)"IncreaseStartingGearLevel has an invalid Id or Id doesn't exist !", (LogType)0, (CLogLevel)2, true, "StaticLog", false);
			return;
		}
		LevelTreeId = StringExtensions.Replace(val2.Value, ((Definition)this).TokenVariables);
		foreach (XElement item in ((XContainer)val).Elements(XName.op_Implicit("Probability")))
		{
			XAttribute val3 = item.Attribute(XName.op_Implicit("Weight"));
			if (val3 == null || !int.TryParse(StringExtensions.Replace(val3.Value, ((Definition)this).TokenVariables), out var result))
			{
				CLoggerManager.Log((object)"IncreaseStartingGearLevel Probability has an invalid Weight or Weight doesn't exist !", (LogType)0, (CLogLevel)2, true, "StaticLog", false);
				continue;
			}
			if (!int.TryParse(StringExtensions.Replace(item.Value, ((Definition)this).TokenVariables), out var result2))
			{
				CLoggerManager.Log((object)"IncreaseStartingGearLevel Probability has an invalid Value !", (LogType)0, (CLogLevel)2, true, "StaticLog", false);
				continue;
			}
			DictionaryExtensions.AddValueOrCreateKey<int, int>(WeightBonusByLevelProbability, result2, result, (Func<int, int, int>)((int a, int b) => a + b));
		}
	}
}
