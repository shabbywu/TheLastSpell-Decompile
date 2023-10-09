using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Skill;
using TheLastStand.Definition.Unit.Enemy.Affix;
using TheLastStand.Framework.Extensions;
using TheLastStand.Model;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.View.Unit;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Enemy;

public class EnemyUnitTemplateDefinition : UnitTemplateDefinition
{
	public static class Consts
	{
		public static class Ids
		{
			public const string Clawer = "Clawer";

			public const string ClawerElite = "ClawerElite";

			public const string SpeedyClawer = "SpeedyClawer";

			public const string Boomer = "Boomer";

			public const string SpawnerCocoon = "SpawnerCocoon";

			public const string Ghost = "Ghost";

			public const string Bloody = "Bloody";
		}

		public const string BaseVariantId = "01";
	}

	public class Stat
	{
		public UnitStatDefinition.E_Stat Id { get; private set; }

		public float Max { get; set; }

		public float Min { get; set; }

		public int Odd { get; set; }

		public Stat(UnitStatDefinition.E_Stat id)
		{
			Id = id;
		}
	}

	public class StatProgression
	{
		public UnitStatDefinition.E_Stat Id { get; }

		public byte Delay { get; set; }

		public byte IncreaseEveryXDay { get; set; } = 1;


		public float Value { get; set; }

		public int MaxIncreases { get; set; } = int.MaxValue;


		public StatProgression(UnitStatDefinition.E_Stat id)
		{
			Id = id;
		}
	}

	public List<EnemyAffixDefinition> AffixDefinitions { get; protected set; }

	public float AppearanceDelay { get; private set; }

	public string AssetsId
	{
		get
		{
			if (!UseTemplateAssets)
			{
				return Id;
			}
			return TemplateId;
		}
	}

	public virtual string SpecificAssetsId
	{
		get
		{
			if (!UseTemplateAssets)
			{
				return Id;
			}
			return TemplateId;
		}
	}

	public float CastSpawnSkillDelay { get; private set; }

	public Stat Reliability { get; protected set; } = new Stat(UnitStatDefinition.E_Stat.Reliability);


	public Stat Accuracy { get; protected set; } = new Stat(UnitStatDefinition.E_Stat.Accuracy);


	public Stat ArmorTotal { get; protected set; } = new Stat(UnitStatDefinition.E_Stat.ActionPointsTotal);


	public BehaviorDefinition Behavior { get; protected set; }

	public Stat Block { get; protected set; } = new Stat(UnitStatDefinition.E_Stat.Block);


	public Stat Critical { get; protected set; } = new Stat(UnitStatDefinition.E_Stat.Critical);


	public Stat CriticalPower { get; protected set; } = new Stat(UnitStatDefinition.E_Stat.CriticalPower);


	public Stat Dodge { get; protected set; } = new Stat(UnitStatDefinition.E_Stat.Dodge);


	public Stat ExperienceGain { get; protected set; } = new Stat(UnitStatDefinition.E_Stat.ExperienceGain);


	public string DamageSkillId { get; protected set; }

	public Stat DamnedSoulsEarned { get; protected set; } = new Stat(UnitStatDefinition.E_Stat.DamnedSoulsEarned);


	public string DeathSoundFolderName { get; protected set; } = string.Empty;


	public UnitView.E_GaugeSize HealthGaugeSize { get; protected set; }

	public Stat HealthRegen { get; protected set; } = new Stat(UnitStatDefinition.E_Stat.HealthRegen);


	public Stat HealthTotal { get; protected set; } = new Stat(UnitStatDefinition.E_Stat.HealthTotal);


	public bool HideInNightReport { get; protected set; }

	public string Id { get; protected set; }

	public bool IsInvulnerable { get; protected set; }

	public bool IsTargetableByAI { get; protected set; }

	public GameDefinition.E_Direction LockedOrientation { get; protected set; }

	public Stat MagicalDamage { get; protected set; } = new Stat(UnitStatDefinition.E_Stat.MagicalDamage);


	public Stat MovePointsTotal { get; protected set; } = new Stat(UnitStatDefinition.E_Stat.MovePointsTotal);


	public float MoveSpeed { get; protected set; }

	public string MoveSoundFolderName { get; protected set; } = string.Empty;


	public BehaviorDefinition OnDeathBehavior { get; protected set; }

	public Stat Panic { get; protected set; } = new Stat(UnitStatDefinition.E_Stat.Panic);


	public Stat PhysicalDamage { get; protected set; } = new Stat(UnitStatDefinition.E_Stat.PhysicalDamage);


	public List<StatProgression> StatProgressions { get; protected set; } = new List<StatProgression>();


	public List<SkillProgression> SkillProgressions { get; protected set; } = new List<SkillProgression>();


	public Stat RangedDamage { get; protected set; } = new Stat(UnitStatDefinition.E_Stat.RangedDamage);


	public Stat Resistance { get; protected set; } = new Stat(UnitStatDefinition.E_Stat.Resistance);


	public SkillDefinition ZoneControlSkill { get; private set; }

	public List<string> SkillsToDisplayIds { get; protected set; } = new List<string>();


	public int? SortingOrderOverride { get; protected set; }

	public Stat StunResistance { get; protected set; } = new Stat(UnitStatDefinition.E_Stat.StunResistance);


	public string TemplateId { get; protected set; }

	public int Tier { get; protected set; } = 1;


	public override Tile.E_UnitAccess UnitAccessNeeded => Tile.E_UnitAccess.Enemy;

	public bool UseTemplateAssets { get; protected set; }

	public string SpawnCutsceneId { get; private set; }

	public List<string> VisualEvolutions { get; protected set; }

	public Vector2 VisualOffset { get; protected set; }

	public List<string> VisualVariants { get; protected set; }

	public int Weight { get; protected set; } = 1;


	public EnemyUnitTemplateDefinition(XContainer container)
		: base(container)
	{
		base.UnitType = DamageableType.Enemy;
	}

	protected override bool CanStopOnSingleTile(Tile tile, TheLastStand.Model.Unit.Unit unit = null)
	{
		if (!base.CanStopOnSingleTile(tile, unit))
		{
			return false;
		}
		TheLastStand.Model.Unit.Unit unit2 = tile.Unit;
		if (unit2 != null && !unit2.IsDead && tile.Unit != unit)
		{
			if (tile.Unit is PlayableUnit)
			{
				return false;
			}
			if (tile.Unit is EnemyUnit enemyUnit && (enemyUnit.TargetTile == tile || enemyUnit.TargetTile == null))
			{
				return false;
			}
		}
		return true;
	}

	public override bool CanTravelThrough(Tile tile, E_MoveMethod moveMethod, bool ignoreUnits = false, bool ignoreBuildings = false)
	{
		if (!base.CanTravelThrough(tile, moveMethod, ignoreUnits, ignoreBuildings))
		{
			return false;
		}
		switch (moveMethod)
		{
		case E_MoveMethod.Walking:
			if (!ignoreUnits && tile.Unit is PlayableUnit && !tile.Unit.IsDead)
			{
				return false;
			}
			break;
		}
		return true;
	}

	public override void Deserialize(XContainer container)
	{
		//IL_10fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_10e9: Unknown result type (might be due to invalid IL or missing references)
		base.Deserialize(container);
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		EnemyUnitTemplateDefinition value = null;
		Id = val.Attribute(XName.op_Implicit("Id")).Value;
		XAttribute val2 = val.Attribute(XName.op_Implicit("TemplateId"));
		if (val2 != null)
		{
			TemplateId = val2.Value;
			if (!EnemyUnitDatabase.EnemyUnitTemplateDefinitions.TryGetValue(TemplateId, out value))
			{
				BossUnitTemplateDefinition value2 = null;
				Dictionary<string, BossUnitTemplateDefinition> bossUnitTemplateDefinitions = BossUnitDatabase.BossUnitTemplateDefinitions;
				if (bossUnitTemplateDefinitions == null || !bossUnitTemplateDefinitions.TryGetValue(TemplateId, out value2))
				{
					CLoggerManager.Log((object)("EnemyUnit " + Id + " could not find template with Id " + TemplateId + "!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					return;
				}
				value = value2;
			}
			XAttribute val3 = val.Attribute(XName.op_Implicit("UseTemplateAssets"));
			if (val3 != null)
			{
				if (!bool.TryParse(val3.Value, out var result))
				{
					CLoggerManager.Log((object)("Could not parse UseTemplateAssets attribute value " + val3.Value + " to a valid bool!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					return;
				}
				UseTemplateAssets = result;
			}
		}
		DeserializeAffixes(((XContainer)val).Element(XName.op_Implicit("Affixes")));
		XElement val4 = ((XContainer)val).Element(XName.op_Implicit("Tier"));
		if (val4 != null)
		{
			if (!int.TryParse(val4.Value, out var result2))
			{
				CLoggerManager.Log((object)"EnemyUnitTemplateDefinition must have a valid Tier!", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			Tier = result2;
		}
		else
		{
			if (value == null)
			{
				CLoggerManager.Log((object)("EnemyUnitTemplateDefinition (" + Id + ") must have a Tier element!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			Tier = value.Tier;
		}
		XElement val5 = ((XContainer)val).Element(XName.op_Implicit("Weight"));
		if (val5 != null)
		{
			if (!int.TryParse(val5.Value, out var result3))
			{
				CLoggerManager.Log((object)"EnemyUnitTemplateDefinition must have a valid Weight!", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			Weight = result3;
		}
		else
		{
			if (value == null)
			{
				CLoggerManager.Log((object)"EnemyUnitTemplateDefinition must have a Weight element!", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			Weight = value.Weight;
		}
		if (((XContainer)val).Element(XName.op_Implicit("VisualVariants")) != null)
		{
			VisualVariants = new List<string>();
			foreach (XElement item in ((XContainer)((XContainer)val).Element(XName.op_Implicit("VisualVariants"))).Elements(XName.op_Implicit("VisualVariant")))
			{
				XAttribute val6 = item.Attribute(XName.op_Implicit("Id"));
				if (val6.IsNullOrEmpty())
				{
					CLoggerManager.Log((object)("EnemyUnitTemplateDefinition " + Id + " VisualVariant must have a valid Id"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				}
				else
				{
					VisualVariants.Add(val6.Value);
				}
			}
		}
		else if (value != null)
		{
			VisualVariants = value.VisualVariants;
		}
		XElement val7 = ((XContainer)val).Element(XName.op_Implicit("DeathSoundFolderName"));
		if (val7 != null)
		{
			XAttribute val8 = val7.Attribute(XName.op_Implicit("Value"));
			if (val8.IsNullOrEmpty())
			{
				CLoggerManager.Log((object)("EnemyUnitTemplateDefinition " + Id + " has an Element DeathSoundFolderName but without a valid Value attribute"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			DeathSoundFolderName = val8.Value;
		}
		else if (value != null)
		{
			DeathSoundFolderName = value.DeathSoundFolderName;
		}
		XElement val9 = ((XContainer)val).Element(XName.op_Implicit("MoveSoundFolderName"));
		if (val9 != null)
		{
			XAttribute val10 = val9.Attribute(XName.op_Implicit("Value"));
			if (val10.IsNullOrEmpty())
			{
				CLoggerManager.Log((object)("EnemyUnitTemplateDefinition " + Id + " has an Element MoveSoundFolderName but without a valid Value attribute"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			MoveSoundFolderName = val10.Value;
		}
		else if (value != null)
		{
			MoveSoundFolderName = value.MoveSoundFolderName;
		}
		XElement val11 = ((XContainer)val).Element(XName.op_Implicit("AppearanceDelay"));
		if (val11 != null)
		{
			if (!float.TryParse(val11.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result4))
			{
				CLoggerManager.Log((object)("AppearanceDelay of Boss Unit : " + Id + " has an invalid value !"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			}
			AppearanceDelay = result4;
		}
		else if (value != null)
		{
			AppearanceDelay = value.AppearanceDelay;
		}
		else
		{
			AppearanceDelay = 0f;
		}
		XElement val12 = ((XContainer)val).Element(XName.op_Implicit("CastSpawnSkillDelay"));
		if (val12 != null)
		{
			if (!float.TryParse(val12.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result5))
			{
				CLoggerManager.Log((object)("CastSpawnSkillDelay of Boss Unit : " + Id + " has an invalid value !"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			}
			CastSpawnSkillDelay = result5;
		}
		else if (value != null)
		{
			CastSpawnSkillDelay = value.CastSpawnSkillDelay;
		}
		else
		{
			CastSpawnSkillDelay = -1f;
		}
		XElement val13 = ((XContainer)val).Element(XName.op_Implicit("MoveMethod"));
		if (val13 != null)
		{
			XAttribute val14 = val13.Attribute(XName.op_Implicit("Method"));
			base.MoveMethod = (E_MoveMethod)Enum.Parse(typeof(E_MoveMethod), val14.Value);
		}
		else if (value != null)
		{
			base.MoveMethod = value.MoveMethod;
		}
		else
		{
			base.MoveMethod = E_MoveMethod.Walking;
		}
		if (((XContainer)val).Element(XName.op_Implicit("IsInvulnerable")) != null)
		{
			IsInvulnerable = true;
		}
		else if (value != null)
		{
			IsInvulnerable = value.IsInvulnerable;
		}
		else
		{
			IsInvulnerable = false;
		}
		XElement val15 = ((XContainer)val).Element(XName.op_Implicit("AvoidAutoTargeting"));
		if (val15 != null)
		{
			XAttribute val16 = val15.Attribute(XName.op_Implicit("Value"));
			IsTargetableByAI = !bool.Parse(val16.Value);
		}
		else if (value != null)
		{
			IsTargetableByAI = value.IsTargetableByAI;
		}
		else
		{
			IsTargetableByAI = true;
		}
		FillUnitTemplateStat(((XContainer)val).Element(XName.op_Implicit(UnitStatDefinition.E_Stat.HealthTotal.ToString())), HealthTotal, value?.HealthTotal);
		FillUnitTemplateStat(((XContainer)val).Element(XName.op_Implicit(UnitStatDefinition.E_Stat.ArmorTotal.ToString())), ArmorTotal, value?.ArmorTotal);
		FillUnitTemplateStat(((XContainer)val).Element(XName.op_Implicit(UnitStatDefinition.E_Stat.Accuracy.ToString())), Accuracy, value?.Accuracy);
		FillUnitTemplateStat(((XContainer)val).Element(XName.op_Implicit(UnitStatDefinition.E_Stat.Block.ToString())), Block, value?.Block);
		FillUnitTemplateStat(((XContainer)val).Element(XName.op_Implicit(UnitStatDefinition.E_Stat.Dodge.ToString())), Dodge, value?.Dodge);
		FillUnitTemplateStat(((XContainer)val).Element(XName.op_Implicit(UnitStatDefinition.E_Stat.MovePointsTotal.ToString())), MovePointsTotal, value?.MovePointsTotal);
		FillUnitTemplateStat(((XContainer)val).Element(XName.op_Implicit(UnitStatDefinition.E_Stat.Resistance.ToString())), Resistance, value?.Resistance);
		FillUnitTemplateStat(((XContainer)val).Element(XName.op_Implicit(UnitStatDefinition.E_Stat.PhysicalDamage.ToString())), PhysicalDamage, value?.PhysicalDamage);
		FillUnitTemplateStat(((XContainer)val).Element(XName.op_Implicit(UnitStatDefinition.E_Stat.RangedDamage.ToString())), RangedDamage, value?.RangedDamage);
		FillUnitTemplateStat(((XContainer)val).Element(XName.op_Implicit(UnitStatDefinition.E_Stat.MagicalDamage.ToString())), MagicalDamage, value?.MagicalDamage);
		FillUnitTemplateStat(((XContainer)val).Element(XName.op_Implicit(UnitStatDefinition.E_Stat.Reliability.ToString())), Reliability, value?.Reliability);
		FillUnitTemplateStat(((XContainer)val).Element(XName.op_Implicit(UnitStatDefinition.E_Stat.HealthRegen.ToString())), HealthRegen, value?.HealthRegen);
		FillUnitTemplateStat(((XContainer)val).Element(XName.op_Implicit(UnitStatDefinition.E_Stat.Critical.ToString())), Critical, value?.Critical);
		FillUnitTemplateStat(((XContainer)val).Element(XName.op_Implicit(UnitStatDefinition.E_Stat.CriticalPower.ToString())), CriticalPower, value?.CriticalPower);
		FillUnitTemplateStat(((XContainer)val).Element(XName.op_Implicit(UnitStatDefinition.E_Stat.StunResistance.ToString())), StunResistance, value?.StunResistance);
		FillUnitTemplateStat(((XContainer)val).Element(XName.op_Implicit(UnitStatDefinition.E_Stat.DamnedSoulsEarned.ToString())), DamnedSoulsEarned, value?.DamnedSoulsEarned);
		FillUnitTemplateStat(((XContainer)val).Element(XName.op_Implicit(UnitStatDefinition.E_Stat.ExperienceGain.ToString())), ExperienceGain, value?.ExperienceGain);
		XElement val17 = ((XContainer)val).Element(XName.op_Implicit("DamageSkillId"));
		if (val17 != null)
		{
			DamageSkillId = val17.Value;
		}
		else if (value != null)
		{
			DamageSkillId = value.DamageSkillId;
		}
		XElement val18 = ((XContainer)val).Element(XName.op_Implicit("MoveSpeed"));
		if (val18 != null)
		{
			if (!float.TryParse(val18.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result6))
			{
				CLoggerManager.Log((object)"Invalid MoveSpeed!", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				result6 = EnemyUnitDatabase.DefaultUnitMoveSpeed;
			}
			MoveSpeed = result6;
		}
		else if (value != null)
		{
			MoveSpeed = value.MoveSpeed;
		}
		else
		{
			MoveSpeed = EnemyUnitDatabase.DefaultUnitMoveSpeed;
		}
		XElement val19 = ((XContainer)val).Element(XName.op_Implicit("ZoneControlSkill"));
		if (val19 != null)
		{
			if (SkillDatabase.SkillDefinitions.TryGetValue(val19.Value, out var value3))
			{
				ZoneControlSkill = value3;
			}
		}
		else if (value != null)
		{
			ZoneControlSkill = value.ZoneControlSkill;
		}
		XElement val20 = ((XContainer)val).Element(XName.op_Implicit("SkillsToDisplay"));
		if (val20 != null)
		{
			foreach (XElement item2 in ((XContainer)val20).Elements(XName.op_Implicit("SkillToDisplay")))
			{
				SkillsToDisplayIds.Add(item2.Value);
			}
		}
		else if (value != null)
		{
			SkillsToDisplayIds = value.SkillsToDisplayIds;
		}
		XElement val21 = ((XContainer)val).Element(XName.op_Implicit("HealthGaugeSize"));
		if (val21 != null)
		{
			HealthGaugeSize = (UnitView.E_GaugeSize)Enum.Parse(typeof(UnitView.E_GaugeSize), val21.Value);
		}
		else if (value != null)
		{
			HealthGaugeSize = value.HealthGaugeSize;
		}
		XElement val22 = ((XContainer)val).Element(XName.op_Implicit("Panic"));
		if (val22 != null)
		{
			XAttribute val23 = val22.Attribute(XName.op_Implicit("Value"));
			if (!float.TryParse(val23.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result7))
			{
				CLoggerManager.Log((object)("Invalid Panic " + val23.Value), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			}
			XAttribute val24 = val22.Attribute(XName.op_Implicit("Min"));
			Panic.Min = ((val24 != null) ? int.Parse(val24.Value) : ((int)result7));
			XAttribute val25 = val22.Attribute(XName.op_Implicit("Max"));
			Panic.Max = ((val25 != null) ? ((float)int.Parse(val25.Value)) : Panic.Min);
			XAttribute val26 = val22.Attribute(XName.op_Implicit("Odd"));
			Panic.Odd = ((val26 == null) ? 1 : int.Parse(val26.Value));
		}
		else if (value != null)
		{
			Panic = value.Panic;
		}
		XElement val27 = ((XContainer)val).Element(XName.op_Implicit("Behavior"));
		if (val27.IsNullOrEmpty() && value == null)
		{
			CLoggerManager.Log((object)("Enemy " + Id + " must have a valid Behavior element or a template to take it from!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		Behavior = ((val27 != null) ? new BehaviorDefinition((XContainer)(object)val27) : value.Behavior);
		XElement val28 = ((XContainer)val).Element(XName.op_Implicit("OnDeathBehavior"));
		if (val28 != null)
		{
			OnDeathBehavior = new BehaviorDefinition((XContainer)(object)val28);
		}
		else if (value != null)
		{
			OnDeathBehavior = value.OnDeathBehavior;
		}
		XElement val29 = ((XContainer)val).Element(XName.op_Implicit("StatsProgressions"));
		if (val29 != null)
		{
			IEnumerable<XElement> enumerable = ((val29 != null) ? ((XContainer)val29).Elements(XName.op_Implicit("StatProgression")) : null);
			if (enumerable != null)
			{
				foreach (XElement item3 in enumerable)
				{
					if (Enum.TryParse<UnitStatDefinition.E_Stat>(item3.Attribute(XName.op_Implicit("Id")).Value, out var result8))
					{
						try
						{
							XAttribute obj = item3.Attribute(XName.op_Implicit("MaxIncreases"));
							string text = ((obj != null) ? obj.Value : null);
							List<StatProgression> statProgressions = StatProgressions;
							StatProgression statProgression = new StatProgression(result8);
							XAttribute obj2 = item3.Attribute(XName.op_Implicit("Delay"));
							statProgression.Delay = byte.Parse(((obj2 != null) ? obj2.Value : null) ?? "0", NumberStyles.Any, CultureInfo.InvariantCulture);
							XAttribute obj3 = item3.Attribute(XName.op_Implicit("IncreaseEveryXDay"));
							statProgression.IncreaseEveryXDay = byte.Parse(((obj3 != null) ? obj3.Value : null) ?? "1", NumberStyles.Any, CultureInfo.InvariantCulture);
							statProgression.Value = float.Parse(item3.Value, NumberStyles.Any, CultureInfo.InvariantCulture);
							statProgression.MaxIncreases = ((text != null) ? int.Parse(text) : int.MaxValue);
							statProgressions.Add(statProgression);
						}
						catch (FormatException ex)
						{
							CLoggerManager.Log((object)("Invalid Stat progression supplied for enemy " + Id + " and stat " + result8.ToString() + ":\n" + ex), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
						}
					}
				}
			}
		}
		else if (value != null)
		{
			StatProgressions = value.StatProgressions;
		}
		XElement obj4 = ((XContainer)val).Element(XName.op_Implicit("SkillProgressions"));
		IEnumerable<XElement> enumerable2 = ((obj4 != null) ? ((XContainer)obj4).Elements(XName.op_Implicit("SkillProgression")) : null);
		if (enumerable2 != null)
		{
			foreach (XElement item4 in enumerable2)
			{
				SkillProgressions.Add(SkillProgression.Deserialize(item4));
			}
		}
		else if (value != null)
		{
			SkillProgressions = value.SkillProgressions;
		}
		if (((XContainer)val).Element(XName.op_Implicit("HideInNightReport")) != null)
		{
			HideInNightReport = true;
		}
		else if (value != null)
		{
			HideInNightReport = value.HideInNightReport;
		}
		else
		{
			HideInNightReport = false;
		}
		if (((XContainer)val).Element(XName.op_Implicit("VisualEvolutions")) != null)
		{
			VisualEvolutions = new List<string>();
			foreach (XElement item5 in ((XContainer)((XContainer)val).Element(XName.op_Implicit("VisualEvolutions"))).Elements(XName.op_Implicit("VisualId")))
			{
				XAttribute val30 = item5.Attribute(XName.op_Implicit("Id"));
				if (val30.IsNullOrEmpty())
				{
					CLoggerManager.Log((object)("EnemyUnitTemplateDefinition " + Id + " VisualId must have a valid Id"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				}
				else
				{
					VisualEvolutions.Add(val30.Value);
				}
			}
		}
		else if (value != null)
		{
			VisualEvolutions = value.VisualEvolutions;
		}
		if (((XContainer)val).Element(XName.op_Implicit("Injuries")) != null)
		{
			DeserializeInjuries((XContainer)(object)((XContainer)val).Element(XName.op_Implicit("Injuries")));
		}
		else if (value != null)
		{
			base.InjuryDefinitions = value.InjuryDefinitions;
		}
		XElement val31 = ((XContainer)val).Element(XName.op_Implicit("VisualOffset"));
		if (val31 != null)
		{
			XAttribute val32 = val31.Attribute(XName.op_Implicit("X"));
			XAttribute val33 = val31.Attribute(XName.op_Implicit("Y"));
			if (!float.TryParse(val32.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result9))
			{
				CLoggerManager.Log((object)("EnemyUnitTemplateDefinition " + Id + " VisualOffset X attribute value " + val32.Value + " could not be parsed to a valid float!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				result9 = 0f;
			}
			if (!float.TryParse(val33.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result10))
			{
				CLoggerManager.Log((object)("EnemyUnitTemplateDefinition " + Id + " VisualOffset Y attribute value " + val33.Value + " could not be parsed to a valid float!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				result10 = 0f;
			}
			VisualOffset = new Vector2(result9, result10);
		}
		else if (value != null)
		{
			VisualOffset = value.VisualOffset;
		}
		XElement val34 = ((XContainer)val).Element(XName.op_Implicit("SortingOrderOverride"));
		if (val34 != null)
		{
			if (!int.TryParse(val34.Value, out var result11))
			{
				CLoggerManager.Log((object)("EnemyUnitTemplateDefinition " + Id + " SortingOrderOverride value " + val34.Value + " could not be parsed to a valid int!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				SortingOrderOverride = null;
			}
			else
			{
				SortingOrderOverride = result11;
			}
		}
		else if (value != null)
		{
			SortingOrderOverride = value.SortingOrderOverride;
		}
		XElement val35 = ((XContainer)val).Element(XName.op_Implicit("LockedOrientation"));
		if (val35 != null)
		{
			if (!Enum.TryParse<GameDefinition.E_Direction>(val35.Value, out var result12))
			{
				CLoggerManager.Log((object)("EnemyUnitTemplateDefinition " + Id + " LockedOrientation value " + val35.Value + " could not be parsed to a valid direction!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				LockedOrientation = GameDefinition.E_Direction.None;
			}
			else
			{
				LockedOrientation = result12;
			}
		}
		else if (value != null)
		{
			LockedOrientation = value.LockedOrientation;
		}
		else
		{
			LockedOrientation = GameDefinition.E_Direction.None;
		}
		XElement val36 = ((XContainer)val).Element(XName.op_Implicit("DamagedParticles"));
		if (val36 != null)
		{
			XAttribute val37 = val36.Attribute(XName.op_Implicit("Id"));
			base.DamagedParticlesId = val37.Value;
		}
		else if (value != null)
		{
			base.DamagedParticlesId = value.DamagedParticlesId;
		}
		XElement val38 = ((XContainer)val).Element(XName.op_Implicit("SpawnCutscene"));
		if (val38 != null)
		{
			XAttribute val39 = val38.Attribute(XName.op_Implicit("Id"));
			SpawnCutsceneId = val39.Value;
		}
		else if (value != null)
		{
			SpawnCutsceneId = value.SpawnCutsceneId;
		}
	}

	public override void DeserializeInjuries(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		if (val == null)
		{
			return;
		}
		base.InjuryDefinitions = new List<InjuryDefinition>();
		foreach (XElement item2 in ((XContainer)val).Elements(XName.op_Implicit("Injury")))
		{
			InjuryDefinition item = new InjuryDefinition((XContainer)(object)item2, HealthTotal.Min);
			base.InjuryDefinitions.Add(item);
		}
	}

	private void DeserializeAffixes(XElement xAffixes)
	{
		AffixDefinitions = new List<EnemyAffixDefinition>();
		if (xAffixes == null)
		{
			return;
		}
		foreach (XElement item in ((XContainer)xAffixes).Elements(XName.op_Implicit("Affix")))
		{
			XAttribute val = item.Attribute(XName.op_Implicit("Id"));
			AffixDefinitions.Add(EnemyUnitDatabase.EnemyAffixDefinitions[val.Value]);
		}
	}

	private void FillUnitTemplateStat(XElement xStat, Stat stat, Stat templateStat = null)
	{
		if (xStat != null)
		{
			XAttribute val = xStat.Attribute(XName.op_Implicit("Min"));
			stat.Min = ((val != null) ? int.Parse(val.Value) : int.Parse(xStat.Value));
			XAttribute val2 = xStat.Attribute(XName.op_Implicit("Max"));
			stat.Max = ((val2 != null) ? ((float)int.Parse(val2.Value)) : stat.Min);
			XAttribute val3 = xStat.Attribute(XName.op_Implicit("Odd"));
			stat.Odd = ((val3 == null) ? 1 : int.Parse(val3.Value));
		}
		else if (templateStat != null)
		{
			stat.Min = templateStat.Min;
			stat.Max = templateStat.Max;
			stat.Odd = templateStat.Odd;
		}
	}
}
