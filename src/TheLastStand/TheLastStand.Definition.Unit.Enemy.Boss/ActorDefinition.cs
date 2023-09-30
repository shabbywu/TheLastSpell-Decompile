using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition.TileMap;
using TheLastStand.Framework.Serialization;
using TheLastStand.Model;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy.Boss;

public class ActorDefinition : Definition
{
	public string ActorId { get; private set; }

	public DamageableType ActorType { get; private set; } = DamageableType.Other;


	public TileFlagDefinition.E_TileFlagTag TileFlagTag { get; private set; }

	public string UnitId { get; private set; }

	public ActorDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XContainer obj = ((container is XElement) ? container : null);
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("Id"));
		ActorId = val.Value;
		XAttribute val2 = ((XElement)obj).Attribute(XName.op_Implicit("UnitId"));
		UnitId = val2.Value;
		XAttribute val3 = ((XElement)obj).Attribute(XName.op_Implicit("ActorType"));
		if (Enum.TryParse<DamageableType>(val3.Value, out var result))
		{
			ActorType = result;
		}
		else
		{
			CLoggerManager.Log((object)("Unable to parse " + val3.Value + " into DamageableType"), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
		}
		XAttribute val4 = ((XElement)obj).Attribute(XName.op_Implicit("TileFlag"));
		if (val4 != null)
		{
			if (Enum.TryParse<TileFlagDefinition.E_TileFlagTag>(val4.Value, out var result2))
			{
				TileFlagTag = result2;
			}
			else
			{
				CLoggerManager.Log((object)("Unable to parse " + val4.Value + " into E_TileFlagTag"), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
			}
		}
		else
		{
			TileFlagTag = TileFlagDefinition.E_TileFlagTag.None;
		}
	}
}
