using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Debugging.Console;
using TPLib.Log;
using TheLastStand.Database;
using TheLastStand.Framework.Serialization;
using TheLastStand.Model.Tutorial;
using UnityEngine;

namespace TheLastStand.Definition.Tutorial;

public class TutorialDefinition : TheLastStand.Framework.Serialization.Definition
{
	public class StringToTutorialIdConverter : StringToStringCollectionEntryConverter
	{
		protected override List<string> Entries => new List<string>(TutorialDatabase.TutorialsDefinitions.Keys);
	}

	public string Id { get; private set; }

	public E_TutorialCategory Category { get; private set; }

	public E_TutorialTrigger Trigger { get; private set; }

	public List<TutorialConditionDefinition> ConditionDefinitions { get; private set; }

	public List<RewiredAction> RewiredActions { get; private set; }

	public List<string> LockTutorials { get; private set; }

	public bool HiddenUntilReadOnce { get; private set; }

	public TutorialDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XAttribute val2 = val.Attribute(XName.op_Implicit("Id"));
		Id = val2.Value;
		XElement val3 = ((XContainer)val).Element(XName.op_Implicit("Category"));
		if (!Enum.TryParse<E_TutorialCategory>(val3.Value, out var result))
		{
			CLoggerManager.Log((object)("Could not parse TutorialDefinition " + Id + " category value " + val3.Value + " to a valid Category!"), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		Category = result;
		XElement val4 = ((XContainer)val).Element(XName.op_Implicit("Conditions"));
		if (val4 != null)
		{
			ConditionDefinitions = new List<TutorialConditionDefinition>();
			foreach (XElement item2 in ((XContainer)val4).Elements())
			{
				TutorialConditionDefinition item = item2.Name.LocalName switch
				{
					"InCity" => new InCityTutorialConditionDefinition((XContainer)(object)item2), 
					"DuringDayTurn" => new DuringDayTurnTutorialConditionDefinition((XContainer)(object)item2), 
					"DuringNight" => new DuringNightTutorialConditionDefinition((XContainer)(object)item2), 
					"CurrentNightHour" => new CurrentNightHourTutorialConditionDefinition((XContainer)(object)item2), 
					"TotalActionPointsSpent" => new TotalActionPointsSpentTutorialConditionDefinition((XContainer)(object)item2), 
					"TutorialMapSkipped" => new TutorialMapSkippedTutorialConditionDefinition((XContainer)(object)item2), 
					"IsWeaponRestrictionAvailable" => new IsWeaponRestrictionAvailableTutorialConditionDefinition((XContainer)(object)item2), 
					_ => null, 
				};
				ConditionDefinitions.Add(item);
			}
		}
		XElement val5 = ((XContainer)val).Element(XName.op_Implicit("Actions"));
		if (val5 != null)
		{
			RewiredActions = new List<RewiredAction>();
			foreach (XElement item3 in ((XContainer)val5).Elements(XName.op_Implicit("Action")))
			{
				XAttribute val6 = item3.Attribute(XName.op_Implicit("Id"));
				XAttribute val7 = item3.Attribute(XName.op_Implicit("Index"));
				RewiredActions.Add(new RewiredAction
				{
					RewiredLabel = val6.Value,
					SpecificIndex = int.Parse(val7.Value)
				});
			}
		}
		XElement val8 = ((XContainer)val).Element(XName.op_Implicit("Trigger"));
		if (!Enum.TryParse<E_TutorialTrigger>(val8.Value, out var result2))
		{
			CLoggerManager.Log((object)("Could not parse TutorialDefinition " + Id + " trigger value " + val8.Value + " to a valid Trigger!"), (LogType)3, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		Trigger = result2;
		XElement val9 = ((XContainer)val).Element(XName.op_Implicit("LockTutorials"));
		if (val9 != null)
		{
			LockTutorials = new List<string>();
			foreach (XElement item4 in ((XContainer)val9).Elements(XName.op_Implicit("LockTutorial")))
			{
				LockTutorials.Add(item4.Value);
			}
		}
		HiddenUntilReadOnce = ((XContainer)val).Element(XName.op_Implicit("HiddenUntilReadOnce")) != null;
	}
}
