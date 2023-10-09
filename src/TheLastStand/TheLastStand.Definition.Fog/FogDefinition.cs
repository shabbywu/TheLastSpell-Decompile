using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Database.Fog;
using TheLastStand.Definition.Hazard;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Definition.Fog;

public class FogDefinition : HazardDefinition
{
	public struct FogDayException
	{
		public int DayNumber;

		public string FogDensityName;

		public FogDayException(int dayNumber, string fogDensityName)
		{
			DayNumber = dayNumber;
			FogDensityName = fogDensityName;
		}
	}

	public struct FogDensity
	{
		public string Name;

		public int Value;

		public FogDensity(string name, int value)
		{
			Name = name;
			Value = value;
		}
	}

	public List<FogDayException> DayExceptions { get; private set; }

	public string Id { get; private set; }

	public int IncreaseEveryXDays { get; private set; }

	public int InitialDensityIndex { get; private set; }

	public List<FogDensity> FogDensities { get; private set; }

	public override E_HazardType HazardType => E_HazardType.Fog;

	public FogDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Expected O, but got Unknown
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		FogDefinition fogDefinition = null;
		XAttribute val2 = val.Attribute(XName.op_Implicit("Id"));
		if (val2.IsNullOrEmpty())
		{
			CLoggerManager.Log((object)"FogDefinition must have an Id!", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		Id = val2.Value;
		XAttribute val3 = val.Attribute(XName.op_Implicit("TemplateId"));
		if (val3 != null)
		{
			fogDefinition = FogDatabase.FogsDefinitions[val3.Value];
		}
		XElement val4 = ((XContainer)val).Element(XName.op_Implicit("IncreaseEveryXDays"));
		if (val4.IsNullOrEmpty())
		{
			if (fogDefinition == null)
			{
				CLoggerManager.Log((object)("FogDefinition " + Id + " has no IncreaseEveryXDays and no Template to copy it from!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			IncreaseEveryXDays = fogDefinition.IncreaseEveryXDays;
		}
		else
		{
			if (!int.TryParse(val4.Value, out var result))
			{
				CLoggerManager.Log((object)("FogDefinition " + Id + " IncreaseEveryXDays " + HasAnInvalidInt(val4.Value) + " !"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			IncreaseEveryXDays = result;
		}
		XElement val5 = ((XContainer)val).Element(XName.op_Implicit("InitialDensityIndex"));
		if (val5.IsNullOrEmpty())
		{
			if (fogDefinition == null)
			{
				CLoggerManager.Log((object)("FogDefinition " + Id + " has no InitialDensityIndex and no Template to copy it from!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			InitialDensityIndex = fogDefinition.InitialDensityIndex;
		}
		else
		{
			if (!int.TryParse(val5.Value, out var result2))
			{
				CLoggerManager.Log((object)("FogDefinition " + Id + " InitialDensityIndex " + HasAnInvalidInt(val5.Value) + " !"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			InitialDensityIndex = result2;
		}
		XElement val6 = ((XContainer)val).Element(XName.op_Implicit("FogDensities"));
		if (val6 == null)
		{
			if (fogDefinition == null)
			{
				CLoggerManager.Log((object)("FogDefinition " + Id + " has no FogDensities and no Template to copy it from!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			FogDensities = new List<FogDensity>(fogDefinition.FogDensities);
		}
		else
		{
			FogDensities = new List<FogDensity>();
			foreach (XElement item in ((XContainer)val6).DescendantNodes())
			{
				XElement val7 = item;
				string value = val7.Attribute(XName.op_Implicit("Name")).Value;
				if (!int.TryParse(val7.Attribute(XName.op_Implicit("Value")).Value, out var result3))
				{
					CLoggerManager.Log((object)("FogDefinition's FogDensity " + HasAnInvalidInt(val7.Attribute(XName.op_Implicit("Value")).Value)), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					return;
				}
				FogDensities.Add(new FogDensity(value, result3));
			}
			FogDensities = FogDensities.OrderByDescending((FogDensity o) => o.Value).ToList();
			if (FogDensities.Count == 0)
			{
				CLoggerManager.Log((object)"Error while deserializing FogDefinition : There should be at least one FogDensity specified.", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			}
		}
		DayExceptions = new List<FogDayException>();
		XElement val8 = ((XContainer)val).Element(XName.op_Implicit("DayExceptions"));
		if (val8 == null)
		{
			if (fogDefinition == null)
			{
				return;
			}
			{
				foreach (FogDayException dayException in fogDefinition.DayExceptions)
				{
					DayExceptions.Add(new FogDayException(dayException.DayNumber, dayException.FogDensityName));
				}
				return;
			}
		}
		List<XElement> list = new List<XElement>(((XContainer)val8).Elements(XName.op_Implicit("DayException")));
		if (list == null || list.Count <= 0)
		{
			return;
		}
		foreach (XElement item2 in list)
		{
			XAttribute val9 = item2.Attribute(XName.op_Implicit("DayId"));
			if (!int.TryParse(val9.Value, out var result4))
			{
				CLoggerManager.Log((object)("Could not parse the DayId attribute of DayException in FogDefinition " + Id + " into an int. value : " + val9.Value), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				break;
			}
			XElement val10 = ((XContainer)item2).Element(XName.op_Implicit("FogDensity"));
			DayExceptions.Add(new FogDayException(result4, val10.Value));
		}
	}
}
