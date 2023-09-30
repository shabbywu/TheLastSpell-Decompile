using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Database;
using TheLastStand.Database.Building;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Building;
using TheLastStand.Definition.CastFx;
using TheLastStand.Definition.Skill.SkillAction;
using TheLastStand.Definition.Skill.SkillEffect;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using TheLastStand.Model.Extensions;
using UnityEngine;

namespace TheLastStand.Definition.Skill;

public class SkillDefinition : Definition
{
	public enum E_Phase
	{
		None = 0,
		Production = 1,
		Deployment = 2,
		Night = 4,
		Day = 3,
		All = 7
	}

	public enum E_InvalidCastDisplayBehaviour
	{
		None,
		Hidden,
		DisplayedUnavailable
	}

	public static class Constants
	{
		public static class Ids
		{
			public const string SkipTurn = "SkipTurn";

			public const string GargoyleSkipTurn = "GargoyleSkipTurn2";
		}

		public const char AreaOfEffectSymbol = 'X';

		public const char ManeuverEffectSymbol = 'M';

		public const char SurroundingEffectSymbol = 'e';

		public const char EmptyEffectSymbol = '_';
	}

	public AffectingUnitSkillEffectDefinition.E_SkillUnitAffect AffectedUnits = AffectingUnitSkillEffectDefinition.E_SkillUnitAffect.All;

	public int ActionPointsCost { get; private set; }

	public int AffectedTilesCount { get; private set; }

	public E_Phase AllowDuringPhase { get; private set; }

	public bool AllowFriendlyFire { get; private set; }

	public AreaOfEffectDefinition AreaOfEffectDefinition { get; private set; }

	public string ArtId { get; private set; }

	public bool CanRotate { get; private set; }

	public bool LockAutoOrientation { get; private set; }

	public bool CardinalDirectionOnly { get; private set; }

	public List<SkillConditionDefinition> ContextualConditions { get; private set; } = new List<SkillConditionDefinition>();


	public E_Phase DisplayDuringPhase { get; private set; }

	public int HealthCost { get; private set; }

	public bool InfiniteRange { get; private set; }

	public E_InvalidCastDisplayBehaviour InvalidCastDisplayBehaviour { get; private set; }

	public string Id { get; private set; }

	public string GroupId { get; private set; }

	public bool IsContextual { get; private set; }

	public bool IsLockedByPerk { get; private set; }

	public bool IsBrazierSpecific { get; private set; }

	public int Level { get; private set; }

	public string LocalizationId { get; private set; }

	public int ManaCost { get; private set; }

	public int MovePointsCost { get; private set; }

	public SkillCastFxDefinition PreSkillCastFxDefinition { get; private set; }

	public bool RangeModifiable { get; private set; }

	public Vector2Int Range { get; private set; }

	public SkillActionDefinition SkillActionDefinition { get; private set; }

	public SkillCastFxDefinition SkillCastFxDefinition { get; private set; }

	public string SoundId { get; private set; }

	public int SurroundingEffectTilesCount { get; private set; }

	public int TotalAreaOfEffectTilesCount => AffectedTilesCount + SurroundingEffectTilesCount;

	public ValidTargets ValidTargets { get; private set; }

	public int UsesPerTurnCount { get; private set; } = -1;


	public SkillDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public bool CanAffectUnitOfType(AffectingUnitSkillEffectDefinition.E_SkillUnitAffect type, bool isSurroundingEffect)
	{
		if (isSurroundingEffect)
		{
			if (SkillActionDefinition.SkillEffectDefinitions != null && SkillActionDefinition.SkillEffectDefinitions.TryGetValue("SurroundingEffect", out var value))
			{
				foreach (SkillEffectDefinition item in value)
				{
					if (item is AffectingUnitSkillEffectDefinition affectingUnitSkillEffectDefinition && affectingUnitSkillEffectDefinition.AffectedUnits.AffectsUnitType(type))
					{
						return true;
					}
				}
			}
			return false;
		}
		if (SkillActionDefinition.SkillEffectDefinitions != null)
		{
			foreach (KeyValuePair<string, List<SkillEffectDefinition>> skillEffectDefinition in SkillActionDefinition.SkillEffectDefinitions)
			{
				if (skillEffectDefinition.Key == "SurroundingEffect")
				{
					continue;
				}
				foreach (SkillEffectDefinition item2 in skillEffectDefinition.Value)
				{
					if (item2 is AffectingUnitSkillEffectDefinition affectingUnitSkillEffectDefinition2 && affectingUnitSkillEffectDefinition2.AffectedUnits.AffectsUnitType(type))
					{
						return true;
					}
				}
			}
		}
		return AffectedUnits.HasFlag(type);
	}

	public override void Deserialize(XContainer container)
	{
		//IL_04c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0584: Unknown result type (might be due to invalid IL or missing references)
		//IL_0625: Unknown result type (might be due to invalid IL or missing references)
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		SkillDefinition skillDefinition = null;
		XAttribute val2 = val.Attribute(XName.op_Implicit("Id"));
		if (val2 == null)
		{
			CLoggerManager.Log((object)"The skill has no ID !", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		Id = val2.Value;
		XAttribute val3 = val.Attribute(XName.op_Implicit("TemplateId"));
		if (val3 != null)
		{
			if (SkillDatabase.SkillDefinitions.ContainsKey(val3.Value))
			{
				skillDefinition = SkillDatabase.SkillDefinitions[val3.Value];
			}
			else
			{
				CLoggerManager.Log((object)("Error while parsing Template parameter of skill " + Id + "! The skill with ID " + val3.Value + " does'nt exist."), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			}
		}
		AllowFriendlyFire = ((XContainer)val).Element(XName.op_Implicit("AllowFriendlyFire")) != null || (skillDefinition?.AllowFriendlyFire ?? false);
		XAttribute val4 = val.Attribute(XName.op_Implicit("GroupId"));
		GroupId = ((val4 != null) ? val4.Value : null) ?? skillDefinition?.GroupId ?? Id;
		XAttribute val5 = val.Attribute(XName.op_Implicit("IsContextual"));
		IsContextual = val5 != null && bool.Parse(val5.Value);
		XAttribute val6 = val.Attribute(XName.op_Implicit("IsLockedByPerk"));
		IsLockedByPerk = val6 != null && bool.Parse(val6.Value);
		XAttribute val7 = val.Attribute(XName.op_Implicit("IsBrazierSpecific"));
		IsBrazierSpecific = val7 != null && bool.Parse(val7.Value);
		XElement val8 = ((XContainer)val).Element(XName.op_Implicit("OverrideLocalizationId"));
		LocalizationId = ((val8 != null) ? val8.Value : null) ?? skillDefinition?.LocalizationId ?? Id;
		XElement val9 = ((XContainer)val).Element(XName.op_Implicit("OverrideArtId"));
		ArtId = ((val9 != null) ? val9.Value : null) ?? skillDefinition?.ArtId ?? Id;
		XElement val10 = ((XContainer)val).Element(XName.op_Implicit("OverrideSoundId"));
		SoundId = ((val10 != null) ? val10.Value : null) ?? skillDefinition?.SoundId ?? GroupId;
		XElement val11 = ((XContainer)val).Element(XName.op_Implicit("ActionPointsCost"));
		int result;
		if (val11 != null)
		{
			if (!int.TryParse(val11.Value, out result))
			{
				CLoggerManager.Log((object)$"Error while parsing ActionPointsCost parameter of level {Level} of skill {Id} !", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			ActionPointsCost = result;
		}
		else if (skillDefinition != null)
		{
			ActionPointsCost = skillDefinition.ActionPointsCost;
		}
		XElement val12 = ((XContainer)val).Element(XName.op_Implicit("MovePointsCost"));
		if (val12 != null)
		{
			if (!int.TryParse(val12.Value, out result))
			{
				CLoggerManager.Log((object)$"Error while parsing MovePointsCost parameter of level {Level} of skill {Id} !", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			MovePointsCost = result;
		}
		else if (skillDefinition != null)
		{
			MovePointsCost = skillDefinition.MovePointsCost;
		}
		XElement val13 = ((XContainer)val).Element(XName.op_Implicit("ManaCost"));
		if (val13 != null)
		{
			if (!int.TryParse(val13.Value, out result))
			{
				CLoggerManager.Log((object)$"Error while parsing ManaCost parameter of level {Level} of skill {Id} !", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			ManaCost = result;
		}
		else if (skillDefinition != null)
		{
			ManaCost = skillDefinition.ManaCost;
		}
		XElement val14 = ((XContainer)val).Element(XName.op_Implicit("HealthCost"));
		if (val14 != null)
		{
			if (!int.TryParse(val14.Value, out result))
			{
				CLoggerManager.Log((object)$"Error while parsing HealthCost parameter of level {Level} of skill {Id} !", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			HealthCost = result;
		}
		else if (skillDefinition != null)
		{
			HealthCost = skillDefinition.HealthCost;
		}
		XElement val15 = ((XContainer)val).Element(XName.op_Implicit("UsesPerTurnCount"));
		if (val15 != null)
		{
			if (!int.TryParse(val15.Value, out result))
			{
				CLoggerManager.Log((object)$"Error while parsing UsesPerTurnCount parameter of level {Level} of skill {Id} !", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			UsesPerTurnCount = result;
		}
		else if (skillDefinition != null)
		{
			UsesPerTurnCount = skillDefinition.UsesPerTurnCount;
		}
		XElement val16 = ((XContainer)val).Element(XName.op_Implicit("Range"));
		if (val16 != null)
		{
			Range = new Vector2Int(int.Parse(val16.Attribute(XName.op_Implicit("Min")).Value), int.Parse(val16.Attribute(XName.op_Implicit("Max")).Value));
			XAttribute val17 = val16.Attribute(XName.op_Implicit("CardinalDirectionOnly"));
			if (val17 != null)
			{
				if (!bool.TryParse(val17.Value, out var result2))
				{
					CLoggerManager.Log((object)$"The level {Level} of skill {Id} has an invalid CardinalDirectionOnly!", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					return;
				}
				CardinalDirectionOnly = result2;
			}
			XAttribute val18 = val16.Attribute(XName.op_Implicit("Modifiable"));
			if (val18 != null)
			{
				if (!bool.TryParse(val18.Value, out var result3))
				{
					CLoggerManager.Log((object)$"The level {Level} of skill {Id} has an invalid Modifiable!", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					return;
				}
				RangeModifiable = result3;
			}
		}
		else if (skillDefinition != null)
		{
			Range = skillDefinition.Range;
			CardinalDirectionOnly = skillDefinition.CardinalDirectionOnly;
			RangeModifiable = skillDefinition.RangeModifiable;
		}
		InfiniteRange = ((XContainer)val).Element(XName.op_Implicit("InfiniteRange")) != null || (skillDefinition?.InfiniteRange ?? false);
		int num = 0;
		XElement val19 = ((XContainer)val).Element(XName.op_Implicit("AreaOfEffect"));
		if (val19 != null)
		{
			AreaOfEffectDefinition = new AreaOfEffectDefinition
			{
				Origin = new Vector2Int(int.Parse(val19.Attribute(XName.op_Implicit("OriginX")).Value), int.Parse(val19.Attribute(XName.op_Implicit("OriginY")).Value)),
				Pattern = new List<List<char>>(),
				IsSingleTarget = false
			};
			string[] array = val19.Value.Split(new char[1] { '\n' });
			for (int num2 = array.Length - 1; num2 >= 0; num2--)
			{
				array[num2] = TPHelpers.RemoveWhitespace(array[num2]);
				if (array[num2] != string.Empty)
				{
					AreaOfEffectDefinition.Pattern.Add(new List<char>(array[num2].Length));
					for (int i = 0; i < array[num2].Length; i++)
					{
						AreaOfEffectDefinition.Pattern[AreaOfEffectDefinition.Pattern.Count - 1].Add(array[num2][i]);
						if (array[num2][i] == 'X')
						{
							AffectedTilesCount++;
						}
						else if (array[num2][i] == 'e')
						{
							SurroundingEffectTilesCount++;
						}
						else if (array[num2][i] == 'M')
						{
							num++;
						}
					}
				}
			}
			AreaOfEffectDefinition.IsSingleTarget |= AffectedTilesCount == 1;
			if (num > 1)
			{
				CLoggerManager.Log((object)("Skill " + Id + " must have 0 or 1 maneuver tile in area of effect!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
		}
		else if (skillDefinition != null)
		{
			AreaOfEffectDefinition = skillDefinition.AreaOfEffectDefinition;
			AffectedTilesCount = skillDefinition.AffectedTilesCount;
			SurroundingEffectTilesCount = skillDefinition.SurroundingEffectTilesCount;
		}
		CanRotate = ((XContainer)val).Element(XName.op_Implicit("CanRotate")) != null || (skillDefinition?.CanRotate ?? false);
		LockAutoOrientation = ((XContainer)val).Element(XName.op_Implicit("LockAutoOrientation")) != null || (skillDefinition?.LockAutoOrientation ?? false);
		if (CanRotate && LockAutoOrientation)
		{
			CLoggerManager.Log((object)("Both CanRotate and LockAutoOrientation are set to true in the skill " + Id + ", something is probably wrong here."), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
		}
		XElement val20 = ((XContainer)val).Element(XName.op_Implicit("AffectedUnits"));
		if (val20 != null)
		{
			AffectedUnits.Deserialize((XContainer)(object)val20);
		}
		XElement val21 = ((XContainer)val).Element(XName.op_Implicit("SkillAction"));
		if (val21 != null)
		{
			foreach (XElement item in ((XContainer)val21).Elements())
			{
				if (SkillActionDefinition != null)
				{
					CLoggerManager.Log((object)("Skill " + Id + " already has a skill action!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					break;
				}
				switch (item.Name.LocalName)
				{
				case "Attack":
					SkillActionDefinition = new AttackSkillActionDefinition((XContainer)(object)val21);
					continue;
				case "Generic":
					SkillActionDefinition = new GenericSkillActionDefinition((XContainer)(object)val21);
					continue;
				case "GoIntoWatchtower":
					SkillActionDefinition = new GoIntoWatchtowerSkillActionDefinition((XContainer)(object)val21);
					continue;
				case "SkipTurn":
					SkillActionDefinition = new SkipTurnSkillActionDefinition((XContainer)(object)val21);
					continue;
				case "QuitWatchtower":
					SkillActionDefinition = new QuitWatchtowerSkillActionDefinition((XContainer)(object)val21);
					continue;
				case "Spawn":
					SkillActionDefinition = new SpawnSkillActionDefinition((XContainer)(object)val21);
					continue;
				case "Build":
					SkillActionDefinition = new BuildSkillActionDefinition((XContainer)(object)val21);
					continue;
				case "Resupply":
					SkillActionDefinition = new ResupplySkillActionDefinition((XContainer)(object)val21);
					continue;
				}
				CLoggerManager.Log((object)("Unknown skill effect type: " + item.Name.LocalName + " on skill " + Id + "."), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			}
		}
		else
		{
			SkillActionDefinition = skillDefinition.SkillActionDefinition;
		}
		if (SkillActionDefinition.HasEffect("Maneuver"))
		{
			if (num == 0 && skillDefinition == null)
			{
				CLoggerManager.Log((object)("Skill " + Id + " has the maneuver skill effect but no maneuver tile in area of effect!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
		}
		else if (num > 0)
		{
			CLoggerManager.Log((object)("Skill " + Id + " has a maneuver tile in area of effect but not the maneuver skill effect!"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			return;
		}
		XElement val22 = ((XContainer)val).Element(XName.op_Implicit("ValidTargets"));
		if (val22 != null)
		{
			ValidTargets = new ValidTargets
			{
				Buildings = new Dictionary<string, ValidTargets.Constraints>()
			};
			foreach (XElement item2 in ((XContainer)val22).Elements(XName.op_Implicit("Building")))
			{
				XAttribute val23 = item2.Attribute(XName.op_Implicit("Id"));
				if (XDocumentExtensions.IsNullOrEmpty(val23))
				{
					CLoggerManager.Log((object)("ValidTargets' building of skill " + Id + " must have a valid Id"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					continue;
				}
				XAttribute val24 = item2.Attribute(XName.op_Implicit("MustBeEmpty"));
				bool result4 = false;
				if (val24 != null && !bool.TryParse(val24.Value, out result4))
				{
					CLoggerManager.Log((object)"Invalid MustBeEmptyAttribute", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					continue;
				}
				XAttribute val25 = item2.Attribute(XName.op_Implicit("NeedRepair"));
				bool result5 = false;
				if (val25 != null && !bool.TryParse(val25.Value, out result5))
				{
					CLoggerManager.Log((object)"Invalid NeedRepairAttribute", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				}
				else
				{
					ValidTargets.Buildings.Add(val23.Value, new ValidTargets.Constraints(result4, result5));
				}
			}
			foreach (XElement item3 in ((XContainer)val22).Elements(XName.op_Implicit("BuildingsList")))
			{
				XAttribute val26 = item3.Attribute(XName.op_Implicit("Id"));
				if (XDocumentExtensions.IsNullOrEmpty(val26))
				{
					CLoggerManager.Log((object)("ValidTargets' buildings list of skill " + Id + " must have a valid Id"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					continue;
				}
				XAttribute val27 = item3.Attribute(XName.op_Implicit("NeedRepair"));
				bool result6 = false;
				if (val27 != null && !bool.TryParse(val27.Value, out result6))
				{
					CLoggerManager.Log((object)"Invalid NeedRepairAttribute", (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					continue;
				}
				foreach (string id in GenericDatabase.IdsListDefinitions[val26.Value].Ids)
				{
					if (!ValidTargets.Buildings.ContainsKey(id))
					{
						ValidTargets.Buildings.Add(id, new ValidTargets.Constraints(mustBeEmpty: false, result6));
					}
				}
			}
			foreach (XElement item4 in ((XContainer)val22).Elements(XName.op_Implicit("BuildingCategory")))
			{
				XAttribute val28 = item4.Attribute(XName.op_Implicit("Category"));
				if (XDocumentExtensions.IsNullOrEmpty(val28) || !Enum.TryParse<BuildingDefinition.E_BuildingCategory>(val28.Value, out var result7))
				{
					CLoggerManager.Log((object)("ValidTargets' building category of skill " + Id + " must have a valid category : \"" + ((val28 != null) ? val28.Value : null) + "\""), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					continue;
				}
				foreach (KeyValuePair<string, BuildingDefinition> buildingDefinition in BuildingDatabase.BuildingDefinitions)
				{
					if (buildingDefinition.Value.BlueprintModuleDefinition.Category.HasFlag(result7))
					{
						ValidTargets.Buildings.Add(buildingDefinition.Key, new ValidTargets.Constraints(mustBeEmpty: false, needRepair: false));
					}
				}
			}
			ValidTargets.PlayableUnits = ((XContainer)val22).Element(XName.op_Implicit("PlayableUnits")) != null;
			ValidTargets.EnemyUnits = ((XContainer)val22).Element(XName.op_Implicit("EnemyUnits")) != null;
			ValidTargets.EmptyTiles = ((XContainer)val22).Element(XName.op_Implicit("EmptyTiles")) != null;
			ValidTargets.WalkableTiles = ((XContainer)val22).Element(XName.op_Implicit("WalkableTiles")) != null;
			ValidTargets.UncrossableGrounds = ((XContainer)val22).Element(XName.op_Implicit("UncrossableGrounds")) != null;
		}
		else if (skillDefinition?.ValidTargets != null)
		{
			ValidTargets = skillDefinition.ValidTargets;
		}
		XElement val29 = ((XContainer)val).Element(XName.op_Implicit("AllowDuringPhases"));
		if (val29 != null)
		{
			foreach (XElement item5 in ((XContainer)val29).Elements())
			{
				if (!Enum.TryParse<E_Phase>(item5.Name.LocalName, out var result8))
				{
					CLoggerManager.Log((object)("Could not parse " + item5.Name.LocalName + " to a valid E_Phase."), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					return;
				}
				AllowDuringPhase |= result8;
			}
		}
		else if (skillDefinition != null)
		{
			AllowDuringPhase = skillDefinition.AllowDuringPhase;
		}
		else
		{
			AllowDuringPhase = E_Phase.Night;
		}
		XElement val30 = ((XContainer)val).Element(XName.op_Implicit("DisplayDuringPhases"));
		if (val30 != null)
		{
			foreach (XElement item6 in ((XContainer)val30).Elements())
			{
				if (!Enum.TryParse<E_Phase>(item6.Name.LocalName, out var result9))
				{
					CLoggerManager.Log((object)("Could not parse " + item6.Name.LocalName + " to a valid E_Phase."), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					return;
				}
				DisplayDuringPhase |= result9;
			}
		}
		else if (skillDefinition != null)
		{
			DisplayDuringPhase = skillDefinition.DisplayDuringPhase;
		}
		else
		{
			DisplayDuringPhase = E_Phase.All;
		}
		XElement val31 = ((XContainer)val).Element(XName.op_Implicit("ContextualConditions"));
		if (val31 != null)
		{
			ContextualConditions = new List<SkillConditionDefinition>();
			foreach (XElement item7 in ((XContainer)val31).Elements())
			{
				switch (((object)item7.Name).ToString())
				{
				case "InWatchtower":
					ContextualConditions.Add(new InWatchtowerConditionDefinition((XContainer)(object)item7));
					break;
				case "OnlyDuringPhase":
					ContextualConditions.Add(new OnlyDuringPhaseConditionDefinition((XContainer)(object)item7));
					break;
				case "MaxTargetHealthLeft":
					ContextualConditions.Add(new MaxTargetHealthLeftConditionDefinition((XContainer)(object)item7));
					break;
				case "NotInBuilding":
					ContextualConditions.Add(new NotInBuildingConditionDefinition((XContainer)(object)item7));
					break;
				case "NextToBuilding":
					ContextualConditions.Add(new NextToBuildingConditionDefinition((XContainer)(object)item7));
					break;
				case "OntoBuilding":
					ContextualConditions.Add(new OntoBuildingConditionDefinition((XContainer)(object)item7));
					break;
				}
			}
		}
		else if (skillDefinition != null)
		{
			ContextualConditions = skillDefinition.ContextualConditions;
		}
		XElement val32 = ((XContainer)val).Element(XName.op_Implicit("InvalidCastDisplayBehaviour"));
		if (val32 != null)
		{
			if (!Enum.TryParse<E_InvalidCastDisplayBehaviour>(val32.Value, out var result10))
			{
				CLoggerManager.Log((object)("Could not parse " + val32.Value + " to a valid E_InvalidCastDisplayBehaviour."), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			InvalidCastDisplayBehaviour = result10;
		}
		else if (skillDefinition != null)
		{
			InvalidCastDisplayBehaviour = skillDefinition.InvalidCastDisplayBehaviour;
		}
		else
		{
			InvalidCastDisplayBehaviour = E_InvalidCastDisplayBehaviour.Hidden;
		}
		XElement val33 = ((XContainer)val).Element(XName.op_Implicit("CastFXs"));
		SkillCastFxDefinition = ((val33 != null) ? new SkillCastFxDefinition((XContainer)(object)val33) : skillDefinition?.SkillCastFxDefinition);
		XElement val34 = ((XContainer)val).Element(XName.op_Implicit("PreCastFXs"));
		PreSkillCastFxDefinition = ((val34 != null) ? new SkillCastFxDefinition((XContainer)(object)val34) : skillDefinition?.PreSkillCastFxDefinition);
		if (AreaOfEffectDefinition == null)
		{
			AreaOfEffectDefinition = new AreaOfEffectDefinition
			{
				Pattern = new List<List<char>>
				{
					new List<char> { 'X' }
				},
				IsSingleTarget = true
			};
		}
	}
}
