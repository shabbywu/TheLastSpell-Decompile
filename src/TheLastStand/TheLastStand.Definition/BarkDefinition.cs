using System.Globalization;
using System.Xml.Linq;
using TPLib;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition;

public class BarkDefinition : TheLastStand.Framework.Serialization.Definition
{
	public static class Constants
	{
		public class Ids
		{
			public const string ManyEnemyUnitSpawn = "ManyEnemyUnitSpawn";

			public const string BuildingHasCriticalHealth = "BuildingHasCriticalHealth";

			public const string BuildingHit = "BuildingHit";

			public const string BuildingGaugeCompletion = "BuildingGaugeCompletion";

			public const string DayCycleBeginning = "DayCycleBeginning";

			public const string NightCycleBeginning = "NightCycleBeginning";

			public const string PlayableUnitAttackPlayableUnit = "PlayableUnitAttackPlayableUnit";

			public const string PlayableUnitsAlmostOutOfMana = "PlayableUnitsAlmostOutOfMana";

			public const string PlayableUnitCriticAttack = "PlayableUnitCriticAttack";

			public const string PlayableUnitDeath = "PlayableUnitDeath";

			public const string PlayableUnitSelfDeath = "PlayableUnitSelfDeath";

			public const string PlayableUnitHit = "PlayableUnitHit";

			public const string PlayableUnitHitByPlayableUnit = "PlayableUnitHitByPlayableUnit";

			public const string PlayableUnitKillAnEnemy = "PlayableUnitKillAnEnemy";

			public const string PlayableUnitLoseABigAmountofHealth = "PlayableUnitLoseABigAmountofHealth";

			public const string PlayableUnitsHaveKilledALot = "PlayableUnitsHaveKilledALot";

			public const string DawnStart = "DawnStart";

			public const string BarkIDMask = "Bark_{0}_{1}";
		}
	}

	public string Id { get; private set; }

	public float Proba { get; private set; }

	public int SentencesCount { get; private set; }

	public BarkDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XAttribute val2 = val.Attribute(XName.op_Implicit("Id"));
		if (val2.IsNullOrEmpty())
		{
			TPDebug.LogError((object)"BarkDefinition must have an Id", (Object)null);
			return;
		}
		Id = val2.Value;
		XElement val3 = ((XContainer)val).Element(XName.op_Implicit("Proba"));
		if (val3.IsNullOrEmpty())
		{
			TPDebug.LogError((object)("Bark " + Id + " hasn't a Proba !"), (Object)null);
			return;
		}
		if (!float.TryParse(val3.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			TPDebug.LogError((object)("Bark " + Id + "'s Proba " + HasAnInvalidFloat(val3.Value)), (Object)null);
			return;
		}
		Proba = result * 0.01f;
		XElement val4 = ((XContainer)val).Element(XName.op_Implicit("Sentences"));
		if (val4 == null)
		{
			TPDebug.LogError((object)("Bark " + Id + " hasn't a Sentences !"), (Object)null);
			return;
		}
		XAttribute val5 = val4.Attribute(XName.op_Implicit("Count"));
		if (!val5.IsNullOrEmpty())
		{
			if (!int.TryParse(val5.Value, out var result2))
			{
				TPDebug.LogError((object)("Bark " + Id + "'s Sentences Count " + HasAnInvalidInt(val5.Value)), (Object)null);
			}
			else
			{
				SentencesCount = result2;
			}
		}
		else
		{
			TPDebug.LogError((object)("Bark " + Id + "'s Sentences hasn't a Count !"), (Object)null);
		}
	}
}
