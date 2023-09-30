using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.TileMap;
using TheLastStand.Controller.Unit.Perk;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit.Perk;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Unit.Perk.PerkDataCondition;
using TheLastStand.Model.Unit.Perk.PerkEffect;
using TheLastStand.Model.Unit.Perk.PerkModule;
using TheLastStand.Serialization.Perk;
using TheLastStand.View.Unit.Perk;

namespace TheLastStand.Model.Unit.Perk;

public class Perk : FormulaInterpreterContext, ISkillContainer, ISerializable, IDeserializable
{
	public bool ContextualSkillActive
	{
		get
		{
			foreach (APerkModule perkModule in PerkModules)
			{
				foreach (APerkEffect perkEffect in perkModule.PerkEffects)
				{
					if (!(perkEffect is UnlockContextualSkillEffect unlockContextualSkillEffect))
					{
						continue;
					}
					string contextualSkillId = unlockContextualSkillEffect.UnlockContextualSkillEffectDefinition.ContextualSkillId;
					foreach (TheLastStand.Model.Skill.Skill contextualSkill in Owner.ContextualSkills)
					{
						if (contextualSkill.SkillDefinition.Id == contextualSkillId && contextualSkill.SkillController.CheckConditions(Owner))
						{
							contextualSkill.SkillAction.SkillActionExecution.Caster = Owner;
							contextualSkill.SkillAction.SkillActionExecution.SkillExecutionController.ComputeSkillRangeTiles(updateView: false);
							return contextualSkill.SkillController.ComputeTargetsAndValidity(Owner);
						}
					}
				}
			}
			return false;
		}
	}

	public APerkModule Module => PerkModules[0];

	public bool OwnerIsCaster
	{
		get
		{
			if (((InterpreterContext)this).TargetObject is PerkDataContainer perkDataContainer)
			{
				return perkDataContainer.Caster == Owner;
			}
			return false;
		}
	}

	public bool OwnerIsTarget
	{
		get
		{
			if (((InterpreterContext)this).TargetObject is PerkDataContainer perkDataContainer)
			{
				return perkDataContainer.TargetDamageable == Owner;
			}
			return false;
		}
	}

	public int DistanceFromOwnerToTarget
	{
		get
		{
			if (!(((InterpreterContext)this).TargetObject is PerkDataContainer perkDataContainer))
			{
				return 0;
			}
			return TileMapController.DistanceBetweenTiles(Owner.OriginTile, perkDataContainer.TargetTile);
		}
	}

	public bool HasPerkData => ((InterpreterContext)this).TargetObject is PerkDataContainer;

	public bool IsNightCycle => TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night;

	public bool IsDayCycle => TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Day;

	public bool IsEnemyTurn
	{
		get
		{
			if (IsNightCycle)
			{
				return TPSingleton<GameManager>.Instance.Game.NightTurn == Game.E_NightTurn.EnemyUnits;
			}
			return false;
		}
	}

	public bool IsPlayableTurn
	{
		get
		{
			if (IsNightCycle)
			{
				return TPSingleton<GameManager>.Instance.Game.NightTurn == Game.E_NightTurn.PlayableUnits;
			}
			return false;
		}
	}

	public int CurrentNightHour => TPSingleton<GameManager>.Instance.Game.CurrentNightHour;

	public int DayNumber => TPSingleton<GameManager>.Instance.Game.DayNumber;

	public float KillerBonusExperienceFactor => PlayableUnitDatabase.KillerBonusExperienceFactor;

	public float CurrentWavePercentageModifier => SpawnWaveManager.CurrentWavePercentageModifier;

	public bool HasDynamicStatsModifierEffect => Module.PerkEffects.Any((APerkEffect e) => e is DynamicStatsModifierEffect);

	public DynamicStatsModifierEffect DynamicStatsModifierEffect => Module.PerkEffects.Where((APerkEffect e) => e is DynamicStatsModifierEffect).FirstOrDefault() as DynamicStatsModifierEffect;

	public bool Bookmarked { get; set; }

	public string CollectionId { get; private set; }

	public PerkDataConditions FeedbackActivationConditions { get; private set; }

	public PerkDataConditions GreyOutConditions { get; private set; }

	public PerkDataConditions HighlightConditions { get; private set; }

	public ISkillCaster Holder => Owner;

	public bool IsNative { get; private set; }

	public PlayableUnit Owner { get; protected set; }

	public PerkController PerkController { get; private set; }

	public PerkDefinition PerkDefinition { get; private set; }

	public List<APerkModule> PerkModules { get; private set; }

	public UnitPerkTier PerkTier { get; private set; }

	public UnitPerkDisplay PerkView { get; private set; }

	public bool Unlocked { get; set; }

	public Perk(PerkDefinition perkDefinition, PerkController perkController, UnitPerkDisplay perkView, PlayableUnit owner, UnitPerkTier perkTier, string collectionId, bool isNative)
	{
		IsNative = isNative;
		PerkController = perkController;
		PerkDefinition = perkDefinition;
		Owner = owner;
		PerkView = perkView;
		PerkTier = perkTier;
		CollectionId = collectionId;
		GeneratePerkModules();
		GenerateViewConditions();
	}

	public Perk(SerializedPerk serializedPerk, PerkDefinition perkDefinition, PerkController perkController, UnitPerkDisplay perkView, PlayableUnit owner, UnitPerkTier perkTier, string collectionId, bool isNative)
		: this(perkDefinition, perkController, perkView, owner, perkTier, collectionId, isNative)
	{
		Deserialize((ISerializedData)(object)serializedPerk);
	}

	public bool DisplayCounter(out int counter)
	{
		if (PerkDefinition.HudBuffer == null)
		{
			counter = -1;
			return false;
		}
		counter = PerkDefinition.HudBuffer.EvalToInt((InterpreterContext)(object)this);
		return counter != 0;
	}

	public bool DisplayDynamicValue(out int value)
	{
		if (PerkDefinition.HudBonus == null)
		{
			value = -1;
			return false;
		}
		value = PerkDefinition.HudBonus.EvalToInt((InterpreterContext)(object)this);
		return true;
	}

	public bool DisplayInAttackTooltip(PerkDataContainer perkDataContainer)
	{
		if (HighlightConditions.Conditions.Count > 0)
		{
			return HighlightConditions.IsValid(perkDataContainer);
		}
		return false;
	}

	public virtual bool DisplayInHUD(out bool greyedOut)
	{
		greyedOut = GreyOutConditions.Conditions.Count > 0 && GreyOutConditions.IsValid(null);
		return PerkDefinition.DisplayInHUD;
	}

	private void GeneratePerkModules()
	{
		PerkModules = new List<APerkModule>();
		foreach (APerkModuleDefinition perkModuleDefinition3 in PerkDefinition.PerkModuleDefinitions)
		{
			if (!(perkModuleDefinition3 is GaugeModuleDefinition perkModuleDefinition))
			{
				if (perkModuleDefinition3 is BufferModuleDefinition perkModuleDefinition2)
				{
					PerkModules.Add(new BufferModule(perkModuleDefinition2, this));
				}
				else
				{
					Owner.LogError($"Found an unknown type of PerkModuleDefinition: {((object)perkModuleDefinition3).GetType()}.", (CLogLevel)1);
				}
			}
			else
			{
				PerkModules.Add(new GaugeModule(perkModuleDefinition, this));
			}
		}
	}

	private void GenerateViewConditions()
	{
		GreyOutConditions = new PerkDataConditions(PerkDefinition.GreyOutConditionsDefinition, this);
		HighlightConditions = new PerkDataConditions(PerkDefinition.HighlightConditionsDefinition, this);
		FeedbackActivationConditions = new PerkDataConditions(PerkDefinition.FeedbackActivationConditionsDefinition, this);
	}

	public bool IsActive()
	{
		return PerkModules.Any((APerkModule m) => m.IsActive);
	}

	public ISerializedData Serialize()
	{
		return (ISerializedData)(object)new SerializedPerk
		{
			Id = PerkDefinition.Id,
			Unlocked = Unlocked,
			Modules = PerkModules.Select((APerkModule o) => o.Serialize() as SerializedModule).ToList(),
			Bookmarked = Bookmarked
		};
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		SerializedPerk serializedPerk = container as SerializedPerk;
		Unlocked = serializedPerk.Unlocked;
		for (int i = 0; i < PerkModules.Count; i++)
		{
			if (i > serializedPerk.Modules.Count - 1)
			{
				((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).LogWarning((object)"Deserializing perk module but serialized perk modules count is less than modules definition count -> Aborting excess.", (CLogLevel)1, true, false);
				break;
			}
			PerkModules[i].Deserialize((ISerializedData)(object)serializedPerk.Modules[i]);
		}
		Bookmarked = serializedPerk.Bookmarked;
	}
}
