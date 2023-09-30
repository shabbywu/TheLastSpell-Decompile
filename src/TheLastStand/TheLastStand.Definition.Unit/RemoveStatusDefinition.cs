using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using TheLastStand.Model.Status;
using UnityEngine;

namespace TheLastStand.Definition.Unit;

public class RemoveStatusDefinition : Definition
{
	public static class Constants
	{
		public const string Id = "RemoveStatus";

		public const string DispelId = "Dispel";

		public const string DischargeId = "Discharge";
	}

	public float BaseChance { get; private set; } = 1f;


	public string Id { get; private set; } = "RemoveStatus";


	public Status.E_StatusType Status { get; private set; }

	public RemoveStatusDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public static string GetRemovedStatusIconName(Status.E_StatusType status)
	{
		return status switch
		{
			TheLastStand.Model.Status.Status.E_StatusType.Buff => "Buff", 
			TheLastStand.Model.Status.Status.E_StatusType.Debuff => "Debuff", 
			TheLastStand.Model.Status.Status.E_StatusType.Poison => "Poison", 
			TheLastStand.Model.Status.Status.E_StatusType.Stun => "Stun", 
			TheLastStand.Model.Status.Status.E_StatusType.Charged => "Charged", 
			TheLastStand.Model.Status.Status.E_StatusType.AllNegative => "NegativeAlterations", 
			_ => string.Empty, 
		};
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XElement val2 = ((XContainer)val).Element(XName.op_Implicit("BaseChance"));
		if (val2 != null)
		{
			if (!float.TryParse(val2.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
			{
				CLoggerManager.Log((object)("Could not parse RemoveStatus BaseChance element value " + val2.Value + " to a valid float value."), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			BaseChance = result;
		}
		XElement val3 = ((XContainer)val).Element(XName.op_Implicit("Status"));
		if (Enum.TryParse<Status.E_StatusType>(val3.Value, out var result2))
		{
			Status = result2;
			if (Status == TheLastStand.Model.Status.Status.E_StatusType.Charged)
			{
				Id = "Discharge";
			}
			else if (TheLastStand.Model.Status.Status.E_StatusType.AllNegative.HasFlag(Status))
			{
				Id = "Dispel";
			}
		}
		else
		{
			CLoggerManager.Log((object)("Incorrect Status used for " + Id + " definition! : unable to parse " + val3.Value), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
		}
	}
}
