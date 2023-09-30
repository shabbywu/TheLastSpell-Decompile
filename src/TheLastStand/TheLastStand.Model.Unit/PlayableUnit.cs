using System;
using System.Collections.Generic;
using System.Linq;
using PortraitAPI;
using PortraitAPI.Misc;
using TPLib;
using TPLib.Debugging.Console;
using TPLib.Log;
using TheLastStand.Controller.Item;
using TheLastStand.Controller.Skill;
using TheLastStand.Controller.TileMap;
using TheLastStand.Controller.Unit;
using TheLastStand.Controller.Unit.Perk;
using TheLastStand.Controller.Unit.Stat;
using TheLastStand.Database;
using TheLastStand.Database.Unit;
using TheLastStand.Definition;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Unit;
using TheLastStand.Definition.Unit.Trait;
using TheLastStand.Framework.Database;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Item;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Item;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Status;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.Model.Unit.Perk.PerkEffect;
using TheLastStand.Serialization;
using TheLastStand.Serialization.Item;
using TheLastStand.Serialization.Perk;
using TheLastStand.Serialization.Unit;
using TheLastStand.View;
using TheLastStand.View.CharacterSheet;
using TheLastStand.View.Unit;
using UnityEngine;

namespace TheLastStand.Model.Unit;

public class PlayableUnit : Unit, ISkillContainer
{
	public static class Constants
	{
		public static class Gender
		{
			public const string Male = "Male";

			public const string Female = "Female";
		}

		public static class Datas
		{
			public const int MinNameSize = 1;

			public const int MaxNameSize = 20;
		}

		public static class Perks
		{
			public const string BackProtectionBuildings = "BackProtectionBuildings";
		}
	}

	public class StringToTraitIdConverter : StringToStringCollectionEntryConverter
	{
		protected override List<string> Entries => new List<string>(PlayableUnitDatabase.UnitTraitDefinitions.Keys);
	}

	public class StringToCurrentTraitsConverter : StringToStringCollectionEntryConverter
	{
		protected override List<string> Entries
		{
			get
			{
				List<string> list = new List<string>();
				for (int num = TileObjectSelectionManager.SelectedPlayableUnit.UnitTraitDefinitions.Count - 1; num >= 0; num--)
				{
					list.Add(TileObjectSelectionManager.SelectedPlayableUnit.UnitTraitDefinitions[num].Id);
				}
				return list;
			}
		}
	}

	public class StringToHairPaletteIdConverter : StringToStringCollectionEntryConverter
	{
		protected override List<string> Entries => new List<string>(PlayableUnitDatabase.PlayableUnitHairColorDefinitions.Keys);
	}

	public class StringToPortraitIdConverter : StringToStringCollectionEntryConverter
	{
		public static class Constants
		{
			public const string RandomValue = "Random";
		}

		protected override List<string> Entries => PlayableUnitView.FaceIdAvailablePortraitIds[TileObjectSelectionManager.SelectedPlayableUnit.FaceId];

		public override bool TryConvert(string value, out object result)
		{
			if (!((StringToStringCollectionEntryConverter)this).TryConvert(value, ref result))
			{
				if (string.Equals(value, "Random", StringComparison.OrdinalIgnoreCase))
				{
					result = 0;
					return true;
				}
				return false;
			}
			return true;
		}

		public override List<string> GetAutoCompleteTexts(string argument)
		{
			List<string> autoCompleteTexts = ((StringToStringCollectionEntryConverter)this).GetAutoCompleteTexts(argument);
			autoCompleteTexts.Insert(0, "Random");
			return autoCompleteTexts;
		}
	}

	public class StringToSkinPaletteIdConverter : StringToStringCollectionEntryConverter
	{
		protected override List<string> Entries => new List<string>(PlayableUnitDatabase.PlayableUnitSkinColorDefinitions.Keys);
	}

	public class StringToStatIdConverter : StringToStringCollectionEntryConverter
	{
		protected override List<string> Entries
		{
			get
			{
				List<string> list = new List<string>();
				foreach (KeyValuePair<UnitStatDefinition.E_Stat, UnitStatDefinition> unitStatDefinition in UnitDatabase.UnitStatDefinitions)
				{
					list.Add(unitStatDefinition.Key.ToString());
				}
				return list;
			}
		}
	}

	public class StringToFaceIdConverter : StringToStringCollectionEntryConverter
	{
		protected override List<string> Entries => new List<string>(PlayableUnitDatabase.GetFaceIdsForGender(TileObjectSelectionManager.SelectedPlayableUnit.Gender));
	}

	public int MomentumTilesActive;

	public int TotalMomentumTilesCrossedThisTurn;

	public int TilesCrossedThisTurn;

	public int ActionPointsSpentThisTurn;

	public float AdditionalNightExperience { get; set; }

	public string ArchetypeId { get; set; }

	public Dictionary<string, BodyPart> BodyParts { get; } = new Dictionary<string, BodyPart>();


	public List<TheLastStand.Model.Skill.Skill> ContextualSkills { get; set; }

	public List<TheLastStand.Model.Skill.Skill> NativeSkills { get; } = new List<TheLastStand.Model.Skill.Skill>();


	public List<TheLastStand.Model.Skill.Skill> MomentumSkills => PlayableUnitController.GetAllSkillsNoCheck().FindAll((TheLastStand.Model.Skill.Skill s) => s.HasMomentum);

	public Dictionary<string, int> SkillLocksBuffers { get; } = new Dictionary<string, int>();


	public Dictionary<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlot>> EquipmentSlots { get; } = new Dictionary<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlot>>();


	public int EquippedWeaponSetIndex { get; set; }

	public float Experience { get; set; }

	public float ExperienceInCurrentLevel { get; set; }

	public float ExperienceNeededToNextLevel { get; set; }

	public string FaceId { get; set; }

	public string Gender { get; set; }

	public ColorSwapPaletteDefinition HairColorPalette
	{
		get
		{
			if (PortraitCodeData == null)
			{
				return null;
			}
			return PlayableUnitDatabase.PlayableUnitHairColorDefinitions.ElementAt(PortraitCodeData.CodeColorDatas[(E_ColorTypes)1].Index).Value;
		}
	}

	public bool HelmetDisplayed { get; set; } = true;


	public ISkillCaster Holder => this;

	public ColorSwapPaletteDefinition EyesColorPalette
	{
		get
		{
			if (PortraitCodeData == null)
			{
				return null;
			}
			return PlayableUnitDatabase.PlayableUnitEyesColorDefinitions.ElementAt(PortraitCodeData.CodeColorDatas[(E_ColorTypes)2].Index).Value;
		}
	}

	public override string Id => PlayableUnitName;

	public override string Name => PlayableUnitName;

	public float LastTurnHealth { get; set; }

	public double Level { get; set; } = 1.0;


	public int LevelPoints => UnitLevelUpPoints.Count;

	public UnitLevelUp LevelUp { get; set; }

	public LifetimeStats LifetimeStats { get; set; }

	public int MainStatsPoints
	{
		get
		{
			int num = 0;
			for (int i = 0; i < UnitLevelUpPoints.Count; i++)
			{
				if (UnitLevelUpPoints[i].HasMainStatPoint)
				{
					num++;
				}
			}
			return num;
		}
	}

	public bool MovedThisDay { get; set; }

	public List<AddSkillEffect> PerkAddedSkillEffects { get; } = new List<AddSkillEffect>();


	public Dictionary<TheLastStand.Model.Skill.Skill.E_ComputationStat, int> PerkComputationStatsLocksBuffer { get; } = new Dictionary<TheLastStand.Model.Skill.Skill.E_ComputationStat, int>();


	public Dictionary<TheLastStand.Model.Skill.Skill.E_ComputationStat, List<SkillModifierEffect>> PerkSkillModifierEffects { get; } = new Dictionary<TheLastStand.Model.Skill.Skill.E_ComputationStat, List<SkillModifierEffect>>();


	public List<AllowDiagonalPropagationEffect> AllowDiagonalPropagationEffects { get; } = new List<AllowDiagonalPropagationEffect>();


	public int PerksPoints { get; set; }

	public UnitPerkTree PerkTree { get; set; }

	public PlayableUnitController PlayableUnitController => base.UnitController as PlayableUnitController;

	public PlayableUnitStatsController PlayableUnitStatsController => base.UnitStatsController as PlayableUnitStatsController;

	public string PlayableUnitName { get; set; }

	public PlayableUnitView PlayableUnitView { get; private set; }

	public DataColor PortraitColor
	{
		get
		{
			if (PortraitCodeData == null)
			{
				return null;
			}
			return PlayableUnitDatabase.PortraitBackgroundColors[PortraitCodeData.CodeColorDatas[(E_ColorTypes)3].Index];
		}
	}

	public Sprite PortraitSprite { get; set; }

	public Sprite PortraitBackgroundSprite { get; set; }

	public CodeData PortraitCodeData { get; set; }

	public int SecondaryStatsPoints
	{
		get
		{
			int num = 0;
			for (int i = 0; i < UnitLevelUpPoints.Count; i++)
			{
				if (UnitLevelUpPoints[i].HasSecondaryStatPoint)
				{
					num++;
				}
			}
			return num;
		}
	}

	public ColorSwapPaletteDefinition SkinColorPalette
	{
		get
		{
			if (PortraitCodeData == null)
			{
				return null;
			}
			return PlayableUnitDatabase.PlayableUnitSkinColorDefinitions.ElementAt(PortraitCodeData.CodeColorDatas[(E_ColorTypes)0].Index).Value;
		}
	}

	public int StatsPoints => MainStatsPoints + SecondaryStatsPoints;

	public override UnitView UnitView
	{
		get
		{
			return base.UnitView;
		}
		set
		{
			base.UnitView = value;
			PlayableUnitView = UnitView as PlayableUnitView;
		}
	}

	public List<UnitLevelUpPoint> UnitLevelUpPoints { get; private set; } = new List<UnitLevelUpPoint>();


	public Dictionary<string, TheLastStand.Model.Unit.Perk.Perk> Perks { get; } = new Dictionary<string, TheLastStand.Model.Unit.Perk.Perk>();


	public int UnlockedPerksCount
	{
		get
		{
			int num = 0;
			foreach (KeyValuePair<string, TheLastStand.Model.Unit.Perk.Perk> perk in Perks)
			{
				if (!perk.Value.IsNative)
				{
					num++;
				}
			}
			return num;
		}
	}

	public List<UnitTraitDefinition> UnitTraitDefinitions { get; } = new List<UnitTraitDefinition>();


	private int HeroesCount => TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count;

	public float UnlockedArmorTotal => PlayableUnitStatsController.GetStat(UnitStatDefinition.E_Stat.ArmorTotal).FinalUnlockedClamped;

	public float UnlockedManaTotal => PlayableUnitStatsController.GetStat(UnitStatDefinition.E_Stat.ManaTotal).FinalUnlockedClamped;

	public float UnlockedManaRegen => PlayableUnitStatsController.GetStat(UnitStatDefinition.E_Stat.ManaRegen).FinalUnlockedClamped;

	public float ActionPoints => base.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.ActionPoints).FinalClamped;

	public float HealthRegen => base.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.HealthRegen).FinalClamped;

	public float MagicalDamage => base.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.MagicalDamage).FinalClamped;

	public float PhysicalDamage => base.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.PhysicalDamage).FinalClamped;

	public float RangedDamage => base.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.RangedDamage).FinalClamped;

	public float ResistanceReduction => GetClampedStatValue(UnitStatDefinition.E_Stat.ResistanceReduction);

	public float PercentageResistanceReduction => base.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.PercentageResistanceReduction).FinalClamped;

	public int TrinketsLevels => GetEquippedTrinketsLevels();

	public int ClosestAllyDistance
	{
		get
		{
			int num = int.MaxValue;
			foreach (PlayableUnit playableUnit in TPSingleton<PlayableUnitManager>.Instance.PlayableUnits)
			{
				if (playableUnit != this)
				{
					int num2 = TileMapController.DistanceBetweenTiles(base.OriginTile, playableUnit.OriginTile);
					if (num2 < num)
					{
						num = num2;
					}
				}
			}
			return num;
		}
	}

	public bool IsBackProtectionValid
	{
		get
		{
			IdsListDefinition backProtectionIds = GenericDatabase.IdsListDefinitions["BackProtectionBuildings"];
			return PlayableUnitController.GetAdjacentTiles().Any((Tile tile) => tile.Building != null && backProtectionIds.Ids.Contains(tile.Building.Id));
		}
	}

	public bool IsManaCostLocked => DictionaryExtensions.GetValueOrDefault<TheLastStand.Model.Skill.Skill.E_ComputationStat, int>(PerkComputationStatsLocksBuffer, TheLastStand.Model.Skill.Skill.E_ComputationStat.ManaCost) > 0;

	public bool IsHealthCostLocked => DictionaryExtensions.GetValueOrDefault<TheLastStand.Model.Skill.Skill.E_ComputationStat, int>(PerkComputationStatsLocksBuffer, TheLastStand.Model.Skill.Skill.E_ComputationStat.HealthCost) > 0;

	public PlayableUnit(UnitTemplateDefinition unitTemplateDefinition, SerializedPlayableUnit serializedPlayableUnit, UnitController unitController, int saveVersion, bool isDead)
		: base(unitTemplateDefinition, unitController)
	{
		Deserialize((ISerializedData)(object)serializedPlayableUnit, saveVersion, isDead);
		Init();
		InitLifetimeStats(serializedPlayableUnit.LifetimeStats);
	}

	public PlayableUnit(UnitTemplateDefinition unitTemplateDefinition, UnitController unitController, UnitView unitView, string archetypeId)
		: base(unitTemplateDefinition, unitController, unitView)
	{
		ArchetypeId = archetypeId;
		base.RandomId = RandomManager.GetRandomRange(TPSingleton<PlayableUnitManager>.Instance, 0, int.MaxValue);
		Init();
		InitLifetimeStats();
	}

	public bool AllowDiagonalPropagation(PerkDataContainer perkDataContainer)
	{
		return AllowDiagonalPropagationEffects.Any((AllowDiagonalPropagationEffect x) => x.PerkDataConditions.IsValid(perkDataContainer));
	}

	public override int ComputeStatusDuration(TheLastStand.Model.Status.Status.E_StatusType statusType, int baseValue, PerkDataContainer perkDataContainer = null, Dictionary<UnitStatDefinition.E_Stat, float> statModifiers = null)
	{
		if (baseValue == -1)
		{
			return -1;
		}
		int num = 0;
		if ((statusType & TheLastStand.Model.Status.Status.E_StatusType.Poison) != 0)
		{
			num += (int)GetClampedStatValueWithModifier(UnitStatDefinition.E_Stat.PoisonDurationModifier, (statModifiers != null) ? new float?(DictionaryExtensions.GetValueOrDefault<UnitStatDefinition.E_Stat, float>(statModifiers, UnitStatDefinition.E_Stat.PoisonDurationModifier)) : null);
			num += (int)GetPerkModifierForComputationStat(TheLastStand.Model.Skill.Skill.E_ComputationStat.PoisonDurationModifier, perkDataContainer);
		}
		if ((statusType & TheLastStand.Model.Status.Status.E_StatusType.Debuff) != 0)
		{
			num += (int)GetClampedStatValueWithModifier(UnitStatDefinition.E_Stat.DebuffDurationModifier, (statModifiers != null) ? new float?(DictionaryExtensions.GetValueOrDefault<UnitStatDefinition.E_Stat, float>(statModifiers, UnitStatDefinition.E_Stat.DebuffDurationModifier)) : null);
			num += (int)GetPerkModifierForComputationStat(TheLastStand.Model.Skill.Skill.E_ComputationStat.DebuffDurationModifier, perkDataContainer);
		}
		if ((statusType & TheLastStand.Model.Status.Status.E_StatusType.Buff) != 0)
		{
			num += (int)GetClampedStatValueWithModifier(UnitStatDefinition.E_Stat.BuffDurationModifier, (statModifiers != null) ? new float?(DictionaryExtensions.GetValueOrDefault<UnitStatDefinition.E_Stat, float>(statModifiers, UnitStatDefinition.E_Stat.BuffDurationModifier)) : null);
			num += (int)GetPerkModifierForComputationStat(TheLastStand.Model.Skill.Skill.E_ComputationStat.BuffDurationModifier, perkDataContainer);
		}
		if ((statusType & TheLastStand.Model.Status.Status.E_StatusType.Stun) != 0)
		{
			num += (int)GetClampedStatValueWithModifier(UnitStatDefinition.E_Stat.StunDurationModifier, (statModifiers != null) ? new float?(DictionaryExtensions.GetValueOrDefault<UnitStatDefinition.E_Stat, float>(statModifiers, UnitStatDefinition.E_Stat.StunDurationModifier)) : null);
			num += (int)GetPerkModifierForComputationStat(TheLastStand.Model.Skill.Skill.E_ComputationStat.StunDurationModifier, perkDataContainer);
		}
		if ((statusType & TheLastStand.Model.Status.Status.E_StatusType.Contagion) != 0)
		{
			num += (int)GetClampedStatValueWithModifier(UnitStatDefinition.E_Stat.ContagionDurationModifier, (statModifiers != null) ? new float?(DictionaryExtensions.GetValueOrDefault<UnitStatDefinition.E_Stat, float>(statModifiers, UnitStatDefinition.E_Stat.ContagionDurationModifier)) : null);
			num += (int)GetPerkModifierForComputationStat(TheLastStand.Model.Skill.Skill.E_ComputationStat.ContagionDurationModifier, perkDataContainer);
		}
		return Mathf.Max(1, baseValue + num);
	}

	public float GetPerkModifierForComputationStat(TheLastStand.Model.Skill.Skill.E_ComputationStat computationStat, PerkDataContainer perkDataContainer, bool? affectBase = null)
	{
		if (perkDataContainer == null || !PerkSkillModifierEffects.ContainsKey(computationStat) || PerkSkillModifierEffects[computationStat] == null)
		{
			return 0f;
		}
		float num = 0f;
		foreach (SkillModifierEffect item in PerkSkillModifierEffects[computationStat])
		{
			bool flag = !affectBase.HasValue || affectBase == item.SkillModifierEffectDefinition.AffectBase;
			if (!item.HasBeenUsed && flag && item.PerkDataConditions.IsValid(perkDataContainer))
			{
				item.HasBeenUsed = true;
				num += item.Value;
				item.HasBeenUsed = false;
			}
		}
		return num;
	}

	public bool IsComputationStatLocked(TheLastStand.Model.Skill.Skill.E_ComputationStat computationStat)
	{
		if (PerkComputationStatsLocksBuffer.TryGetValue(computationStat, out var value))
		{
			return value > 0;
		}
		return false;
	}

	public void RegisterBodyPartViews(BodyPartView[] bodyPartViews, bool register)
	{
		if ((Object)(object)PlayableUnitView == (Object)null)
		{
			return;
		}
		BodyPart value = null;
		int i = 0;
		for (int num = bodyPartViews.Length; i < num; i++)
		{
			if (!((Object)(object)bodyPartViews[i] != (Object)null))
			{
				continue;
			}
			if (!BodyParts.TryGetValue(((Object)bodyPartViews[i]).name, out value))
			{
				((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).LogError((object)$"Trying to register an invalid BodyPartView to a BodyPart (Bodypart view : {bodyPartViews[i]}", (CLogLevel)0, true, true);
				continue;
			}
			if (register)
			{
				bodyPartViews[i].BodyPart = value;
				value.SetBodyPartView(bodyPartViews[i].Orientation, bodyPartViews[i]);
				bodyPartViews[i].IsDirty = true;
				continue;
			}
			if (bodyPartViews[i].BodyPart == value)
			{
				bodyPartViews[i].BodyPart = null;
				bodyPartViews[i].IsDirty = true;
			}
			_ = (Object)(object)value.GetBodyPartView(bodyPartViews[i].Orientation) == (Object)(object)bodyPartViews[i];
		}
	}

	public void ToggleContextualSkillLock(string skillId, bool locks, TheLastStand.Model.Unit.Perk.Perk perkContainer = null, int overallUses = -1)
	{
		if (locks)
		{
			TheLastStand.Model.Skill.Skill skill = ContextualSkills.Find((TheLastStand.Model.Skill.Skill x) => x.SkillDefinition.Id == skillId);
			if (skill != null)
			{
				ContextualSkills.Remove(skill);
			}
		}
		else if (ContextualSkills.All((TheLastStand.Model.Skill.Skill x) => x.SkillDefinition.Id != skillId))
		{
			if (!SkillDatabase.SkillDefinitions.TryGetValue(skillId, out var value))
			{
				((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).LogError((object)("Skill " + skillId + " not found!"), (CLogLevel)1, true, true);
				return;
			}
			TheLastStand.Model.Skill.Skill skill2 = new SkillController(value, (ISkillContainer)(((object)perkContainer) ?? ((object)this)), overallUses, value.UsesPerTurnCount).Skill;
			ContextualSkills.Add(skill2);
		}
	}

	public override string ToString()
	{
		string name = Name;
		name += "\n";
		foreach (UnitStatDefinition.E_Stat statsKey in base.UnitStatsController.UnitStats.StatsKeys)
		{
			name += $"{statsKey}: {base.UnitStatsController.GetStat(statsKey).FinalClamped}\n";
		}
		name += "\n";
		for (int i = 0; i < UnitTraitDefinitions.Count; i++)
		{
			name = name + "Trait " + UnitTraitDefinitions[i].Id + "\n";
		}
		name += "\n";
		foreach (KeyValuePair<string, TheLastStand.Model.Unit.Perk.Perk> perk in Perks)
		{
			name = name + "Perk " + perk.Value.PerkDefinition.Id + "\n";
		}
		return name;
	}

	protected override bool ComputeIsolation()
	{
		List<Tile> adjacentTiles = base.UnitController.GetAdjacentTiles();
		for (int num = adjacentTiles.Count - 1; num >= 0; num--)
		{
			if (adjacentTiles[num]?.Unit != null && adjacentTiles[num].Unit != this && adjacentTiles[num].Unit is PlayableUnit)
			{
				return false;
			}
		}
		return true;
	}

	protected override void Init()
	{
		base.Init();
		foreach (KeyValuePair<string, BodyPartDefinition> playableUnitNakedBodyPartsDefinition in PlayableUnitDatabase.PlayableUnitNakedBodyPartsDefinitions)
		{
			BodyParts.Add(playableUnitNakedBodyPartsDefinition.Key, new BodyPart(playableUnitNakedBodyPartsDefinition.Value));
		}
	}

	private int GetEquippedTrinketsLevels()
	{
		int num = 0;
		if (EquipmentSlots.TryGetValue(ItemSlotDefinition.E_ItemSlotId.Trinket, out var value))
		{
			for (int i = 0; i < value.Count; i++)
			{
				if (value[i].Item != null && value[i].Item.ItemDefinition.Category == ItemDefinition.E_Category.Trinket)
				{
					num += value[i].Item.Level;
				}
			}
		}
		return num;
	}

	private void InitLifetimeStats(SerializedLifetimeStats container = null)
	{
		if (container != null)
		{
			LifetimeStats = new LifetimeStatsController(container).LifetimeStats;
		}
		else
		{
			LifetimeStats = new LifetimeStatsController().LifetimeStats;
		}
	}

	public override void Log(object message, CLogLevel logLevel = 1, bool forcePrintInUnity = false, bool printStackTrace = false)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).Log((object)$"[{UniqueIdentifier}]: {message}", (Object)(object)UnitView, logLevel, forcePrintInUnity, printStackTrace);
	}

	public override void LogError(object message, CLogLevel logLevel = 1, bool forcePrintInUnity = true, bool printStackTrace = true)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).LogError((object)$"[{UniqueIdentifier}]: {message}", (Object)(object)UnitView, logLevel, forcePrintInUnity, printStackTrace);
	}

	public override void LogWarning(object message, CLogLevel logLevel = 1, bool forcePrintInUnity = true, bool printStackTrace = false)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).LogWarning((object)$"[{UniqueIdentifier}]: {message}", (Object)(object)UnitView, logLevel, forcePrintInUnity, printStackTrace);
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1, bool isDead = false)
	{
		SerializedPlayableUnit serializedPlayableUnit = container as SerializedPlayableUnit;
		base.Deserialize((ISerializedData)(object)serializedPlayableUnit.Unit, saveVersion);
		PlayableUnitName = serializedPlayableUnit.Name;
		ArchetypeId = serializedPlayableUnit.ArchetypeId;
		Gender = serializedPlayableUnit.Portrait.Gender;
		FaceId = serializedPlayableUnit.Portrait.FaceId;
		MovedThisDay = serializedPlayableUnit.MovedThisDay;
		LastTurnHealth = serializedPlayableUnit.LastTurnHealth;
		HelmetDisplayed = serializedPlayableUnit.HelmetDisplayed;
		ActionPointsSpentThisTurn = serializedPlayableUnit.ActionPointsSpentThisTurn;
		MomentumTilesActive = serializedPlayableUnit.MomentumTilesActive;
		TotalMomentumTilesCrossedThisTurn = serializedPlayableUnit.TotalMomentumTilesCrossedThisTurn;
		TilesCrossedThisTurn = serializedPlayableUnit.TilesCrossedThisTurn;
		Level = serializedPlayableUnit.Level;
		LevelUp = new UnitLevelUpController(PlayableUnitDatabase.UnitLevelUpDefinition, serializedPlayableUnit.LevelUp).UnitLevelUp;
		LevelUp.PlayableUnit = this;
		PerksPoints = serializedPlayableUnit.PerksPoints;
		UnitLevelUpPoints = new List<UnitLevelUpPoint>();
		foreach (SerializedLevelUpPoint serializedLevelUpPoint in serializedPlayableUnit.SerializedLevelUpPoints)
		{
			UnitLevelUpPoints.Add(new UnitLevelUpPoint(serializedLevelUpPoint));
		}
		Experience = serializedPlayableUnit.Experience;
		ExperienceInCurrentLevel = serializedPlayableUnit.ExperienceInCurrentLevel;
		EquippedWeaponSetIndex = serializedPlayableUnit.EquippedWeaponSetIndex;
		ContextualSkills = new List<TheLastStand.Model.Skill.Skill>();
		foreach (SerializedSkill contextualSkill in serializedPlayableUnit.ContextualSkills)
		{
			ContextualSkills.Add(new SkillController(contextualSkill, this).Skill);
		}
		foreach (string trait in serializedPlayableUnit.Traits)
		{
			if (!PlayableUnitDatabase.UnitTraitDefinitions.TryGetValue(trait, out var value))
			{
				((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).LogWarning((object)("Trying to load TraitDefinition " + trait + " but it wasn't found in Database. Skipping it."), (CLogLevel)1, true, false);
			}
			else
			{
				UnitTraitDefinitions.Add(value);
			}
		}
		foreach (SerializedEquipmentSlot equipmentSlot2 in serializedPlayableUnit.EquipmentSlots)
		{
			foreach (SerializedItemSlot itemSlot in equipmentSlot2.ItemSlots)
			{
				ItemSlotDefinition.E_ItemSlotId id = equipmentSlot2.Id;
				int num = (EquipmentSlots.ContainsKey(id) ? EquipmentSlots[id].Count : 0);
				int num2 = ((EquippedWeaponSetIndex != num) ? 1 : 0);
				EquipmentSlotView equipmentSlotView = CharacterSheetPanel.EquipmentSlots[id][ItemSlotDefinition.E_ItemSlotId.WeaponSlot.HasFlag(itemSlot.Id) ? num2 : num];
				EquipmentSlot equipmentSlot;
				try
				{
					equipmentSlot = new EquipmentSlotController(itemSlot, equipmentSlotView, this).EquipmentSlot;
				}
				catch (MissingAssetException<ItemDatabase> arg)
				{
					((CLogger<InventoryManager>)TPSingleton<InventoryManager>.Instance).LogError((object)$"Could not find equipment {itemSlot.Item.Id} for slot {itemSlot.Id}, this equipment will be skipped.\n{arg}", (CLogLevel)0, true, true);
					continue;
				}
				if (!EquipmentSlots.ContainsKey(id))
				{
					EquipmentSlots.Add(id, new List<EquipmentSlot>());
				}
				EquipmentSlots[id].Add(equipmentSlot);
			}
		}
		base.UnitStatsController = new PlayableUnitStatsController(container as SerializedUnitStats, this);
		PerkTree = new UnitPerkTreeController(TPSingleton<CharacterSheetPanel>.Instance.UnitPerkTreeView, this).UnitPerkTree;
		PerkTree.UnitPerkTreeController.GeneratePerkTree(serializedPlayableUnit.PerkCollections, this, isDead);
	}

	public override void DeserializeAfterInit(ISerializedData container, int saveVersion)
	{
		base.DeserializeAfterInit(container, saveVersion);
		SerializedPlayableUnit serializedPlayableUnit = container as SerializedPlayableUnit;
		if (serializedPlayableUnit.Stats != null)
		{
			DeserializeStats((ISerializedData)(object)serializedPlayableUnit.Stats, saveVersion);
		}
		PlayableUnitStatsController.RefreshEquipmentValues();
		if ((Object)(object)UnitView != (Object)null)
		{
			UnitView.RefreshHud(UnitStatDefinition.E_Stat.Health);
			UnitView.RefreshHud(UnitStatDefinition.E_Stat.Armor);
			UnitView.RefreshHud(UnitStatDefinition.E_Stat.Mana);
			UnitView.RefreshHud(UnitStatDefinition.E_Stat.ActionPoints);
			UnitView.RefreshHud(UnitStatDefinition.E_Stat.MovePoints);
		}
	}

	public override void DeserializeStats(ISerializedData container, int saveVersion)
	{
		PlayableUnitStatsController.PlayableUnitStats.Deserialize(container, saveVersion);
	}

	public override ISerializedData Serialize()
	{
		return (ISerializedData)(object)new SerializedPlayableUnit((SerializedUnit)(object)base.Serialize())
		{
			Name = PlayableUnitName,
			ArchetypeId = ArchetypeId,
			Portrait = new SerializedPortrait
			{
				Gender = Gender,
				FaceId = FaceId,
				Code = ((object)PortraitCodeData).ToString()
			},
			LastTurnHealth = LastTurnHealth,
			PerksPoints = PerksPoints,
			SerializedLevelUpPoints = UnitLevelUpPoints.Select((UnitLevelUpPoint o) => (SerializedLevelUpPoint)(object)o.Serialize()).ToList(),
			Experience = Experience,
			ExperienceInCurrentLevel = ExperienceInCurrentLevel,
			ContextualSkills = ContextualSkills.Select((TheLastStand.Model.Skill.Skill o) => (SerializedSkill)(object)o.Serialize()).ToList(),
			Level = Level,
			LevelUp = (SerializedLevelUpBonuses)(object)LevelUp.Serialize(),
			EquippedWeaponSetIndex = EquippedWeaponSetIndex,
			EquipmentSlots = EquipmentSlots.Select((KeyValuePair<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlot>> o) => new SerializedEquipmentSlot
			{
				Id = o.Key,
				ItemSlots = o.Value.Select((EquipmentSlot equipSlot) => (SerializedItemSlot)(object)equipSlot.Serialize()).ToList()
			}).ToList(),
			Traits = UnitTraitDefinitions.Select((UnitTraitDefinition o) => o.Id).ToList(),
			PerkCollections = PerkTree.Serialize(),
			NativePerks = (from p in Perks
				where p.Value.PerkTier == null
				select (SerializedPerk)(object)p.Value.Serialize()).ToList(),
			LifetimeStats = (SerializedLifetimeStats)(object)LifetimeStats.Serialize(),
			MovedThisDay = MovedThisDay,
			HelmetDisplayed = HelmetDisplayed,
			ActionPointsSpentThisTurn = ActionPointsSpentThisTurn,
			MomentumTilesActive = MomentumTilesActive,
			TotalMomentumTilesCrossedThisTurn = TotalMomentumTilesCrossedThisTurn,
			TilesCrossedThisTurn = TilesCrossedThisTurn,
			Stats = (SerializedUnitStats)(object)base.UnitStatsController.UnitStats.Serialize()
		};
	}
}
