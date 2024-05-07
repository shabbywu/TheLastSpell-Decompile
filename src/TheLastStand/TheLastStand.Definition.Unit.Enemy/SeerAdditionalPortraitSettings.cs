using System;
using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Database.Unit;
using TheLastStand.Model;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy;

public class SeerAdditionalPortraitSettings
{
	public bool DisplayPortraitAmount { get; }

	public int PortraitAmount { get; }

	public string PortraitTemplateId { get; }

	public DamageableType PortraitType { get; private set; } = DamageableType.Other;


	public SeerAdditionalPortraitSettings(XElement xSeerAdditionalPortrait)
	{
		PortraitType = DamageableType.Enemy;
		XAttribute val = xSeerAdditionalPortrait.Attribute(XName.op_Implicit("Type"));
		if (val != null)
		{
			if (!Enum.TryParse<DamageableType>(val.Value, out var result))
			{
				CLoggerManager.Log((object)("Could not parse attribute Type into a DamageableType : '" + val.Value + "'."), (Object)(object)TPSingleton<SpawnWaveDatabase>.Instance, (LogType)0, (CLogLevel)2, true, "SpawnWaveDatabase", false);
			}
			else
			{
				PortraitType = result;
			}
		}
		XAttribute val2 = xSeerAdditionalPortrait.Attribute(XName.op_Implicit("Id"));
		PortraitTemplateId = val2.Value;
		XAttribute val3 = xSeerAdditionalPortrait.Attribute(XName.op_Implicit("Amount"));
		DisplayPortraitAmount = false;
		if (val3 != null)
		{
			DisplayPortraitAmount = true;
			if (!int.TryParse(val3.Value, out var result2))
			{
				CLoggerManager.Log((object)("Could not parse attribute Amount into an int : '" + val3.Value + "'."), (Object)(object)TPSingleton<SpawnWaveDatabase>.Instance, (LogType)0, (CLogLevel)2, true, "SpawnWaveDatabase", false);
			}
			else
			{
				PortraitAmount = result2;
			}
		}
	}
}
