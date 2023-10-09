using System.Globalization;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Night;

public class NightReportRankDefinition : TheLastStand.Framework.Serialization.Definition
{
	public string Id { get; set; }

	public float MaxHPsLostRatio { get; set; }

	public NightReportRankDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XAttribute val2 = val.Attribute(XName.op_Implicit("Id"));
		if (val2.IsNullOrEmpty())
		{
			CLoggerManager.Log((object)"NightReportDefinition Id is null or empty!", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		Id = val2.Value;
		XElement val3 = ((XContainer)val).Element(XName.op_Implicit("MaxHPsLostRatio"));
		float result;
		if (val3.IsNullOrEmpty())
		{
			CLoggerManager.Log((object)("NightReportDefinition with Id " + Id + " must have a MaxHPsLostRatio!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
		}
		else if (!float.TryParse(val3.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
		{
			CLoggerManager.Log((object)("NightReportRankDefinition with Id " + Id + " MaxHPsLostRatio " + val3.Value + " must be a valid float value!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
		}
		else
		{
			MaxHPsLostRatio = result;
		}
	}
}
