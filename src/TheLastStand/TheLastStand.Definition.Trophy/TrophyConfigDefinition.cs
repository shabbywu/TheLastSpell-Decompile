using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Trophy;

public class TrophyConfigDefinition : Definition
{
	public enum E_GemRarity
	{
		Common,
		Uncommon,
		Rare,
		Epic
	}

	public class GemStageData
	{
		public E_GemRarity GemRarity { get; private set; }

		public int Min { get; private set; }

		public int Max { get; private set; }

		public GemStageData(E_GemRarity gemRarity, int min, int max)
		{
			GemRarity = gemRarity;
			Min = min;
			Max = max;
		}
	}

	public List<GemStageData> GemStageDatas { get; private set; } = new List<GemStageData>();


	public TrophyConfigDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = ((container is XElement) ? container : null).Element(XName.op_Implicit("GemsStages"));
		if (val == null)
		{
			return;
		}
		foreach (XElement item in ((XContainer)val).Elements(XName.op_Implicit("Gem")))
		{
			XAttribute val2 = item.Attribute(XName.op_Implicit("Id"));
			XAttribute val3 = item.Attribute(XName.op_Implicit("Min"));
			XAttribute val4 = item.Attribute(XName.op_Implicit("Max"));
			if (val2 == null)
			{
				continue;
			}
			if (!Enum.TryParse<E_GemRarity>(val2.Value, out var result))
			{
				TPDebug.LogError((object)("A Gem stage has an invalid Id value : " + val2.Value), (Object)null);
			}
			if (val3 == null)
			{
				continue;
			}
			if (!int.TryParse(val3.Value, out var result2))
			{
				TPDebug.LogError((object)("A Gem stage has an invalid min value (should be an integer) : " + val3.Value), (Object)null);
			}
			if (val4 != null)
			{
				if (!int.TryParse(val4.Value, out var result3))
				{
					result3 = -1;
					TPDebug.LogError((object)("A Gem stage has an invalid max value (should be an integer) : " + val4.Value), (Object)null);
				}
				GemStageDatas.Add(new GemStageData(result, result2, (result3 != -1) ? result3 : int.MaxValue));
			}
		}
	}
}
