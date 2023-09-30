using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Night;

public class NightReportRankDefinition : Definition
{
	public string Id { get; set; }

	public float MaxHPsLostRatio { get; set; }

	public NightReportRankDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XAttribute val2 = val.Attribute(XName.op_Implicit("Id"));
		if (XDocumentExtensions.IsNullOrEmpty(val2))
		{
			CLoggerManager.Log((object)"NightReportDefinition Id is null or empty!", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		Id = val2.Value;
		XElement val3 = ((XContainer)val).Element(XName.op_Implicit("MaxHPsLostRatio"));
		float result;
		if (XDocumentExtensions.IsNullOrEmpty(val3))
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
