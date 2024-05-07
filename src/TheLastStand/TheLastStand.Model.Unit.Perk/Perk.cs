using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.TileMap;
using TheLastStand.Controller.Unit.Perk;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit.Perk;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Item;
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
			if (TargetObject is PerkDataContainer perkDataContainer)
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
			if (TargetObject is PerkDataContainer perkDataContainer)
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
			if (!(TargetObject is PerkDataContainer perkDataContainer))
			{
				return 0;
			}
			return TileMapController.DistanceBetweenTiles(Owner.OriginTile, perkDataContainer.TargetTile);
		}
	}

	public bool HasPerkData => TargetObject is PerkDataContainer;

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

	public int DayNumber
	{
		get
		{
			if (!TPSingleton<GameManager>.Exist())
			{
				return 0;
			}
			return TPSingleton<GameManager>.Instance.Game.DayNumber;
		}
	}

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

	public bool IsFromRace { get; private set; }

	public bool IsNative { get; private set; }

	public bool IsUnlockedFromItem => Unlockers.Any((IPerkUnlocker perkContainer) => perkContainer is TheLastStand.Model.Item.Item);

	public bool IsUnlockedFromPlayableUnit => Unlockers.Any((IPerkUnlocker perkContainer) => perkContainer is PlayableUnit);

	public PlayableUnit Owner { get; set; }

	public PerkController PerkController { get; private set; }

	public PerkDefinition PerkDefinition { get; private set; }

	public List<APerkModule> PerkModules { get; private set; }

	public UnitPerkTier PerkTier { get; set; }

	public UnitPerkDisplay PerkView { get; set; }

	public bool Unlocked { get; set; }

	public bool UnlockedInPerkTree
	{
		get
		{
			if (Unlocked && PerkTier != null)
			{
				return IsUnlockedFromPlayableUnit;
			}
			return false;
		}
	}

	public bool OnlyUnlockedByItem
	{
		get
		{
			if (Unlockers.Count == 1)
			{
				return !IsUnlockedFromPlayableUnit;
			}
			return false;
		}
	}

	public HashSet<IPerkUnlocker> Unlockers { get; private set; } = new HashSet<IPerkUnlocker>();


	public Perk(PerkDefinition perkDefinition, PerkController perkController, UnitPerkDisplay perkView, PlayableUnit owner, UnitPerkTier perkTier, string collectionId, bool isNative, bool isFromRace)
	{
		IsNative = isNative;
		PerkController = perkController;
		PerkDefinition = perkDefinition;
		Owner = owner;
		PerkView = perkView;
		PerkTier = perkTier;
		CollectionId = collectionId;
		IsFromRace = isFromRace;
		GeneratePerkModules();
		GenerateViewConditions();
	}

	public Perk(SerializedPerk serializedPerk, PerkDefinition perkDefinition, PerkController perkController, UnitPerkDisplay perkView, PlayableUnit owner, UnitPerkTier perkTier, string collectionId, bool isNative, bool isFromRace)
		: this(perkDefinition, perkController, perkView, owner, perkTier, collectionId, isNative, isFromRace)
	{
		Deserialize(serializedPerk);
	}

	public bool DisplayCounter(out int counter)
	{
		if (PerkDefinition.HudBuffer == null)
		{
			counter = -1;
			return false;
		}
		counter = PerkDefinition.HudBuffer.EvalToInt(this);
		return counter != 0;
	}

	public bool DisplayDynamicValue(out int value)
	{
		if (PerkDefinition.HudBonus == null)
		{
			value = -1;
			return false;
		}
		value = PerkDefinition.HudBonus.EvalToInt(this);
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
					Owner.LogError($"Found an unknown type of PerkModuleDefinition: {perkModuleDefinition3.GetType()}.", (CLogLevel)1);
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
		return new SerializedPerk
		{
			Id = PerkDefinition.Id,
			Unlocked = (Unlocked && !OnlyUnlockedByItem),
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
			PerkModules[i].Deserialize(serializedPerk.Modules[i]);
		}
		Bookmarked = serializedPerk.Bookmarked;
	}
}
