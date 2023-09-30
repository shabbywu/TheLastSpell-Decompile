using System.Xml.Linq;
using TPLib;
using UnityEngine;

namespace TheLastStand.Definition.Trophy.TrophyCondition;

public class EnemiesDebuffedSeveralTimesSingleTurnTrophyDefinition : HeroesTrophyConditionDefinition
{
	public const string Name = "EnemiesDebuffedSeveralTimesSingleTurn";

	public override object[] DescriptionLocalizationParameters => new object[2] { Value, NumberOfDebuffs };

	public int Value { get; private set; }

	public int NumberOfDebuffs { get; private set; }

	public EnemiesDebuffedSeveralTimesSingleTurnTrophyDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		base.Deserialize(container);
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		if (!int.TryParse(val.Value, out var result))
		{
			TPDebug.LogError((object)"The Value of an Element : EnemiesDebuffedSeveralTimesSingleTurn in TrophiesDefinitions isn't a valid int", (Object)null);
			return;
		}
		if (!int.TryParse(val.Attribute(XName.op_Implicit("NumberOfDebuffs")).Value, out var result2))
		{
			TPDebug.LogError((object)"The Value of the Attribute NumberOfDebuffs for EnemiesDebuffedSeveralTimesSingleTurn in TrophiesDefinitions isn't a valid int", (Object)null);
			return;
		}
		NumberOfDebuffs = result2;
		Value = result;
	}

	public override string ToString()
	{
		return "EnemiesDebuffedSeveralTimesSingleTurn";
	}
}
