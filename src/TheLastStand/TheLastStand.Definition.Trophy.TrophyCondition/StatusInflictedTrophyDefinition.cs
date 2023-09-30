using System;
using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Manager;
using TheLastStand.Model.Status;
using UnityEngine;

namespace TheLastStand.Definition.Trophy.TrophyCondition;

public class StatusInflictedTrophyDefinition : ValueIntHeroesTrophyConditionDefinition
{
	public const string Name = "StatusInflicted";

	public override object[] DescriptionLocalizationParameters => new object[2]
	{
		StylizeStatus(),
		base.Value
	};

	public Status.E_StatusType StatusType { get; private set; }

	public StatusInflictedTrophyDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		string[] array = val.Attribute(XName.op_Implicit("Status")).Value.Split(new char[1] { '|' });
		if (array[0] == "Any")
		{
			StatusType = Status.E_StatusType.Buff | Status.E_StatusType.Debuff | Status.E_StatusType.Poison | Status.E_StatusType.Stun;
		}
		else
		{
			foreach (string text in array)
			{
				if (Enum.TryParse<Status.E_StatusType>(text, out var result))
				{
					StatusType |= result;
					continue;
				}
				((CLogger<TrophyManager>)TPSingleton<TrophyManager>.Instance).LogError((object)("A Status in an Element : StatusInflicted in TrophiesDefinitions doesn't exist (status : " + text + ")"), (CLogLevel)1, true, true);
				return;
			}
		}
		if (!int.TryParse(val.Value, out var result2))
		{
			TPDebug.LogError((object)"The Value of an Element : StatusInflicted in TrophiesDefinitions isn't a valid int", (Object)null);
		}
		else
		{
			base.Value = result2;
		}
	}

	public override string ToString()
	{
		return "StatusInflicted";
	}

	private string StylizeStatus()
	{
		string text = string.Empty;
		if ((StatusType & Status.E_StatusType.Poison) == Status.E_StatusType.Poison)
		{
			text += "<style=Poison>Poison</style>";
		}
		if ((StatusType & Status.E_StatusType.Stun) == Status.E_StatusType.Stun)
		{
			if (text != string.Empty)
			{
				text += " | ";
			}
			text += "<style=Stun>Stun</style>";
		}
		if ((StatusType & Status.E_StatusType.Debuff) == Status.E_StatusType.Debuff)
		{
			if (text != string.Empty)
			{
				text += " | ";
			}
			text += "<style=Debuff>Debuff</style>";
		}
		return text;
	}
}
