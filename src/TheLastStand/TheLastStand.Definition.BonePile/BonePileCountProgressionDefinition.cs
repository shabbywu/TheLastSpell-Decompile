using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.BonePile;

public class BonePileCountProgressionDefinition : Definition
{
	public struct ProgressionData
	{
		public int BaseValue;

		public int Limit;

		public int Delay;

		public int IncreaseEveryXDays;

		public int IncreaseValue;
	}

	public string CityId { get; private set; }

	public string TemplateCityId { get; private set; }

	public bool HasTemplate => !string.IsNullOrEmpty(TemplateCityId);

	public Dictionary<string, ProgressionData> BonePileProgressions { get; private set; } = new Dictionary<string, ProgressionData>();


	public BonePileCountProgressionDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("CityId"));
		CityId = val.Value;
		XAttribute val2 = ((XElement)obj).Attribute(XName.op_Implicit("TemplateCityId"));
		if (val2 != null)
		{
			TemplateCityId = val2.Value;
		}
		foreach (XElement item in obj.Elements(XName.op_Implicit("Progression")))
		{
			XAttribute val3 = item.Attribute(XName.op_Implicit("BonePileId"));
			if (!int.TryParse(item.Attribute(XName.op_Implicit("BaseValue")).Value, out var result))
			{
				CLoggerManager.Log((object)("Could not parse BaseValue value to a valid int! (Bone Pile " + val3.Value + ", CityId " + CityId + ")"), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
				break;
			}
			XAttribute val4 = item.Attribute(XName.op_Implicit("Limit"));
			int result2 = -1;
			if (val4 != null && !int.TryParse(val4.Value, out result2))
			{
				CLoggerManager.Log((object)("Could not parse Limit value to a valid int! (Bone Pile " + val3.Value + ", CityId " + CityId + ")"), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
				break;
			}
			if (!int.TryParse(item.Attribute(XName.op_Implicit("Delay")).Value, out var result3))
			{
				CLoggerManager.Log((object)("Could not parse Delay value to a valid int! (Bone Pile " + val3.Value + ", CityId " + CityId + ")"), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
				break;
			}
			if (!int.TryParse(item.Attribute(XName.op_Implicit("IncreaseEveryXDays")).Value, out var result4))
			{
				CLoggerManager.Log((object)("Could not parse IncreaseEveryXDays value to a valid int! (Bone Pile " + val3.Value + ", CityId " + CityId + ")"), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
				break;
			}
			if (!int.TryParse(item.Attribute(XName.op_Implicit("IncreaseValue")).Value, out var result5))
			{
				CLoggerManager.Log((object)("Could not parse IncreaseValue value to a valid int! (Bone Pile " + val3.Value + ", CityId " + CityId + ")"), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
				break;
			}
			ProgressionData progressionData = default(ProgressionData);
			progressionData.BaseValue = result;
			progressionData.Limit = result2;
			progressionData.Delay = result3;
			progressionData.IncreaseEveryXDays = result4;
			progressionData.IncreaseValue = result5;
			ProgressionData value = progressionData;
			BonePileProgressions.Add(val3.Value, value);
		}
	}

	public void DeserializeUsingTemplate(BonePileCountProgressionDefinition template)
	{
		foreach (KeyValuePair<string, ProgressionData> bonePileProgression in template.BonePileProgressions)
		{
			if (!BonePileProgressions.ContainsKey(bonePileProgression.Key))
			{
				BonePileProgressions.Add(bonePileProgression.Key, bonePileProgression.Value);
			}
		}
	}
}
