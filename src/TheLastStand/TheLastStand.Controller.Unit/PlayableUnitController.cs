using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using PortraitAPI;
using PortraitAPI.Misc;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.Item;
using TheLastStand.Controller.Meta;
using TheLastStand.Controller.Skill;
using TheLastStand.Controller.Trophy.TrophyConditions;
using TheLastStand.Controller.Unit.Perk;
using TheLastStand.Controller.Unit.Stat;
using TheLastStand.DRM.Achievements;
using TheLastStand.Database;
using TheLastStand.Database.Unit;
using TheLastStand.Definition;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Meta;
using TheLastStand.Definition.Skill;
using TheLastStand.Definition.Unit;
using TheLastStand.Definition.Unit.Perk;
using TheLastStand.Definition.Unit.PlayableUnitGeneration;
using TheLastStand.Definition.Unit.Trait;
using TheLastStand.Framework;
using TheLastStand.Framework.Sequencing;
using TheLastStand.Manager;
using TheLastStand.Manager.Achievements;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Item;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.Unit;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model;
using TheLastStand.Model.Item;
using TheLastStand.Model.Meta;
using TheLastStand.Model.Skill;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Tutorial;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Model.Unit.Movement;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.Serialization.Perk;
using TheLastStand.Serialization.Unit;
using TheLastStand.View;
using TheLastStand.View.CharacterSheet;
using TheLastStand.View.HUD.UnitManagement;
using TheLastStand.View.Skill.SkillAction;
using TheLastStand.View.Skill.SkillAction.UI;
using TheLastStand.View.TileMap;
using TheLastStand.View.ToDoList;
using TheLastStand.View.Unit;
using TheLastStand.View.Unit.UI;
using UnityEngine;
using UnityEngine.Events;

namespace TheLastStand.Controller.Unit;

public class PlayableUnitController : UnitController
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static Func<EquipmentSlot, bool> _003C_003E9__25_0;

		public static UnityAction _003C_003E9__36_1;

		public static Func<Tile, bool> _003C_003E9__47_0;

		public static Func<ItemDefinition, bool> _003C_003E9__57_0;

		internal bool _003CGetBestItemSlotToCompare_003Eb__25_0(EquipmentSlot slot)
		{
			return slot.Item != null;
		}

		internal void _003CPrepareForMovement_003Eb__36_1()
		{
			TPSingleton<TutorialManager>.Instance.OnTrigger(E_TutorialTrigger.OnPlayableUnitMovement);
		}

		internal bool _003CStartTurn_003Eb__47_0(Tile tile)
		{
			return tile.Unit is EnemyUnit;
		}

		internal bool _003CGenerateEquipment_003Eb__57_0(ItemDefinition itemDefinition)
		{
			return itemDefinition.Hands == ItemDefinition.E_Hands.OneHand;
		}
	}

	private readonly List<TheLastStand.Model.Skill.Skill> contextualSkills = new List<TheLastStand.Model.Skill.Skill>();

	private readonly List<TheLastStand.Model.Skill.Skill> equipmentSkills = new List<TheLastStand.Model.Skill.Skill>();

	private readonly List<TheLastStand.Model.Skill.Skill> weaponSkills = new List<TheLastStand.Model.Skill.Skill>();

	public PlayableUnit PlayableUnit => base.Unit as PlayableUnit;

	public List<int> DebuffedByGhosts { get; } = new List<int>();


	public PlayableUnitController(SerializedPlayableUnit serializedPlayableUnit, int saveVersion = -1, bool isDead = false)
	{
		base.Unit = new PlayableUnit(PlayableUnitDatabase.PlayableUnitTemplateDefinition, serializedPlayableUnit, this, saveVersion, isDead);
		base.Unit.DeserializeAfterInit(serializedPlayableUnit, saveVersion);
		ComputeExperienceNeededToNextLevel();
		foreach (KeyValuePair<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlot>> equipmentSlot in PlayableUnit.EquipmentSlots)
		{
			for (int i = 0; i < equipmentSlot.Value.Count; i++)
			{
				if (equipmentSlot.Value[i].Item != null)
				{
					EquipItem(equipmentSlot.Value[i].Item, equipmentSlot.Value[i], shouldRefreshMetaCondition: true, onLoad: true);
				}
			}
		}
		PlayableUnitView.GeneratePortrait(PlayableUnit, serializedPlayableUnit);
		UpdateInjuryStage();
		GenerateNativeSkills(onLoad: true);
		GenerateNativePerks(serializedPlayableUnit.NativePerks, isDead);
	}

	public PlayableUnitController(string archetypeId, int traitPoints, UnitView view = null, Tile tile = null, int level = 1)
	{
		base.Unit = new PlayableUnit(PlayableUnitDatabase.PlayableUnitTemplateDefinition, this, view, archetypeId)
		{
			State = TheLastStand.Model.Unit.Unit.E_State.Ready
		};
		SetTile(tile);
		PlayableUnit.Gender = (RandomManager.GetRandomBool(this) ? "Male" : "Female");
		PlayableUnit.PlayableUnitName = RandomManager.GetRandomElement(this, PlayableUnitDatabase.GetNamesForGender(PlayableUnit.Gender));
		if ((Object)(object)base.Unit.UnitView != (Object)null)
		{
			((Object)base.Unit.UnitView).name = base.Unit.Id;
		}
		PlayableUnit.FaceId = GetRandomFaceId(PlayableUnit.Gender);
		PlayableUnitView.GenerateRandomPortrait(PlayableUnit);
		PlayableUnit.LevelUp = new UnitLevelUpController(PlayableUnitDatabase.UnitLevelUpDefinition, TPSingleton<UnitLevelUpView>.Instance).UnitLevelUp;
		PlayableUnit.LevelUp.PlayableUnit = PlayableUnit;
		PlayableUnit.PerkTree = new UnitPerkTreeController(TPSingleton<CharacterSheetPanel>.Instance.UnitPerkTreeView, PlayableUnit).UnitPerkTree;
		PlayableUnit.PerkTree.UnitPerkTreeController.GeneratePerkTree(PlayableUnit);
		Generate(traitPoints, level);
		ComputeExperienceNeededToNextLevel();
		GenerateContextualSkills();
		GenerateNativeSkills(onLoad: false);
		GenerateNativePerks();
	}

	public static ColorSwapPaletteDefinition GetRandomHairColorSwapPalette(string skinName, Dictionary<string, ColorSwapPaletteDefinition> colorSwapPalettes)
	{
		string randomHairColorId = PlayableUnitDatabase.UnitLinkHairSkin.GetRandomHairColorId(skinName);
		return colorSwapPalettes[randomHairColorId];
	}

	public static ColorSwapPaletteDefinition RandomizeColorSwapPaletteDefinition(Dictionary<string, ColorSwapPaletteDefinition> colorSwapPalettes, out string colorSwapName)
	{
		int num = 0;
		for (int i = 0; i < colorSwapPalettes.Values.Count; i++)
		{
			num += colorSwapPalettes.Values.ElementAt(i).Weight;
		}
		int randomRange = RandomManager.GetRandomRange(TPSingleton<PlayableUnitManager>.Instance, 0, num);
		int num2 = 0;
		for (int j = 0; j < colorSwapPalettes.Values.Count; j++)
		{
			if (j == 0)
			{
				if (randomRange >= 0 && randomRange < colorSwapPalettes.Values.ElementAt(j).Weight)
				{
					colorSwapName = colorSwapPalettes.Values.ElementAt(j).Id;
					return colorSwapPalettes.Values.ElementAt(j);
				}
				num2 += colorSwapPalettes.Values.ElementAt(j).Weight;
			}
			else
			{
				if (randomRange >= num2 && randomRange < colorSwapPalettes.Values.ElementAt(j).Weight + num2)
				{
					colorSwapName = colorSwapPalettes.Values.ElementAt(j).Id;
					return colorSwapPalettes.Values.ElementAt(j);
				}
				num2 += colorSwapPalettes.Values.ElementAt(j).Weight;
			}
		}
		colorSwapName = colorSwapPalettes.Values.ElementAt(0).Id;
		return colorSwapPalettes.Values.ElementAt(0);
	}

	public void AddCrossedTiles(int amount, bool ignoreMomentum = false)
	{
		PlayableUnit.TilesCrossedThisTurn += amount;
		if (!ignoreMomentum)
		{
			PlayableUnit.TotalMomentumTilesCrossedThisTurn += amount;
			PlayableUnit.MomentumTilesActive += amount;
		}
	}

	public bool AddTrait(string traitId, bool forceAdd = false)
	{
		UnitTraitDefinition unitTraitDefinition = PlayableUnitDatabase.UnitTraitDefinitions[traitId];
		bool flag = false;
		if (!forceAdd)
		{
			bool flag2 = false;
			for (int i = 0; i < PlayableUnit.UnitTraitDefinitions.Count; i++)
			{
				if (PlayableUnit.UnitTraitDefinitions[i].Incompatibilities.Contains(traitId))
				{
					flag2 = true;
					break;
				}
			}
			if (!PlayableUnit.UnitTraitDefinitions.Contains(unitTraitDefinition) && !flag2)
			{
				PlayableUnit.UnitTraitDefinitions.Add(unitTraitDefinition);
				flag = true;
			}
		}
		else
		{
			PlayableUnit.UnitTraitDefinitions.Add(unitTraitDefinition);
			flag = true;
		}
		if (flag)
		{
			AddSlots(unitTraitDefinition.AddSlots);
			RemoveSlots(unitTraitDefinition.RemoveSlots);
			PlayableUnit.PlayableUnitStatsController.OnTraitGenerated(unitTraitDefinition);
		}
		return flag;
	}

	public void AssignItemSlotViewsToUnit()
	{
		foreach (KeyValuePair<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlotView>> equipmentSlot in CharacterSheetPanel.EquipmentSlots)
		{
			if (ItemSlotDefinition.E_ItemSlotId.WeaponSlot.HasFlag(equipmentSlot.Key))
			{
				equipmentSlot.Value[0].ItemSlot = ((PlayableUnit.EquipmentSlots.ContainsKey(equipmentSlot.Key) && PlayableUnit.EquipmentSlots[equipmentSlot.Key].Count > PlayableUnit.EquippedWeaponSetIndex) ? PlayableUnit.EquipmentSlots[equipmentSlot.Key][PlayableUnit.EquippedWeaponSetIndex] : null);
				equipmentSlot.Value[1].ItemSlot = ((PlayableUnit.EquipmentSlots.ContainsKey(equipmentSlot.Key) && PlayableUnit.EquipmentSlots[equipmentSlot.Key].Count > ((PlayableUnit.EquippedWeaponSetIndex == 0) ? 1 : 0)) ? PlayableUnit.EquipmentSlots[equipmentSlot.Key][(PlayableUnit.EquippedWeaponSetIndex == 0) ? 1 : 0] : null);
				if (TPSingleton<CharacterSheetPanel>.Instance.IsOpened)
				{
					equipmentSlot.Value[0].Refresh();
					equipmentSlot.Value[1].Refresh();
				}
			}
			else
			{
				for (int i = 0; i < equipmentSlot.Value.Count; i++)
				{
					equipmentSlot.Value[i].ItemSlot = ((PlayableUnit.EquipmentSlots.ContainsKey(equipmentSlot.Key) && PlayableUnit.EquipmentSlots[equipmentSlot.Key].Count > i) ? PlayableUnit.EquipmentSlots[equipmentSlot.Key][i] : null);
					equipmentSlot.Value[i].Refresh();
				}
			}
		}
	}

	public void ComputeReachableTiles()
	{
		if (TPSingleton<PlayableUnitManager>.Instance.PreviewSkillExecution == null && TileObjectSelectionManager.HasPlayableUnitSelected && base.MoveTask == null)
		{
			int movePoints = (int)PlayableUnit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.MovePoints).FinalClamped;
			if (PlayableUnit.OriginTile.Building != null && PlayableUnit.OriginTile.Building.IsWatchtower)
			{
				movePoints = 0;
			}
			else if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Day)
			{
				PathfindingManager.Pathfinding.PathfindingController.AddAllReachableTilesToPath(PlayableUnit);
				return;
			}
			PathfindingManager.Pathfinding.PathfindingController.ComputeReachableTiles(base.Unit, movePoints);
		}
	}

	public override void EndTurn()
	{
		base.EndTurn();
		PlayableUnit.LastTurnHealth = PlayableUnit.Health;
		PlayableUnit.ActionPointsSpentThisTurn = 0;
		EffectManager.DisplayEffects();
	}

	public void EquipItem(TheLastStand.Model.Item.Item item, EquipmentSlot targetEquipmentSlot = null, bool shouldRefreshMetaCondition = true, bool onLoad = false)
	{
		if (targetEquipmentSlot == null)
		{
			targetEquipmentSlot = GetBestItemSlot(item);
			if (targetEquipmentSlot == null)
			{
				return;
			}
		}
		if ((item.IsTwoHandedWeapon && targetEquipmentSlot.ItemSlotDefinition.Id == ItemSlotDefinition.E_ItemSlotId.RightHand && !targetEquipmentSlot.EquipmentSlotController.CanEquipTwoHandedWeapon(item)) || targetEquipmentSlot.BlockedByOtherSlot != null)
		{
			return;
		}
		if (item.ItemSlot != null)
		{
			if (!(item.ItemSlot is EquipmentSlot equipmentSlot) || targetEquipmentSlot.Item == null || equipmentSlot.ItemSlotController.IsItemCompatible(targetEquipmentSlot.Item))
			{
				targetEquipmentSlot.ItemSlotController.SwapItems(item.ItemSlot, onLoad);
			}
		}
		else
		{
			targetEquipmentSlot.ItemSlotController.SetItem(item, onLoad);
		}
		if (item.IsTwoHandedWeapon && targetEquipmentSlot.ItemSlotDefinition.Id == ItemSlotDefinition.E_ItemSlotId.RightHand)
		{
			int num = PlayableUnit.EquipmentSlots[targetEquipmentSlot.ItemSlotDefinition.Id].IndexOf(targetEquipmentSlot);
			bool flag = false;
			foreach (KeyValuePair<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlot>> equipmentSlot3 in PlayableUnit.EquipmentSlots)
			{
				for (int i = 0; i < equipmentSlot3.Value.Count; i++)
				{
					EquipmentSlot equipmentSlot2 = equipmentSlot3.Value[i];
					if (equipmentSlot2.ItemSlotDefinition.Id == ItemSlotDefinition.E_ItemSlotId.LeftHand && i == num)
					{
						equipmentSlot2.ItemSlotController.SwapItems(null, onLoad);
						targetEquipmentSlot.BlockOtherSlot = equipmentSlot2;
						equipmentSlot2.BlockedByOtherSlot = targetEquipmentSlot;
						flag = true;
						break;
					}
				}
				if (flag)
				{
					break;
				}
			}
		}
		if (!onLoad)
		{
			RefreshStats();
			PlayableUnit.PlayableUnitView?.RefreshBodyParts();
			PlayableUnit.PlayableUnitView?.RefreshHealth();
			PlayableUnit.PlayableUnitView?.RefreshArmor();
		}
		if (!onLoad && shouldRefreshMetaCondition)
		{
			foreach (KeyValuePair<UnitStatDefinition.E_Stat, float> item2 in item.GetAllStatBonusesMerged())
			{
				TPSingleton<MetaConditionManager>.Instance.RefreshMaxHeroStatReached(item2.Key, PlayableUnit.UnitStatsController.GetStat(item2.Key).FinalClamped);
			}
		}
		if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.CharacterSheet && TPSingleton<CharacterSheetPanel>.Instance.IsInventoryOpened)
		{
			TPSingleton<InventoryManager>.Instance.Inventory.InventoryView.IsDirty = true;
		}
	}

	public override void FilterTilesInRange(TilesInRangeInfos tilesInRangeInfos, List<Tile> skillSourceTiles)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		base.FilterTilesInRange(tilesInRangeInfos, skillSourceTiles);
		foreach (KeyValuePair<Tile, TilesInRangeInfos.TileDisplayInfos> item in tilesInRangeInfos.Range)
		{
			if (item.Key.HasAnyFog)
			{
				item.Value.HasLineOfSight = false;
				item.Value.TileColor = TileMapView.SkillHiddenRangeTilesColorInvalidOrientation._Color;
			}
		}
	}

	public void GainExperience(float amount)
	{
		if (amount < 0f)
		{
			((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).LogError((object)"Negative experience !", (CLogLevel)2, true, true);
			return;
		}
		amount = amount * PlayableUnit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.ExperienceGainMultiplier).FinalClamped / 100f;
		PlayableUnit.Experience += amount;
		PlayableUnit.ExperienceInCurrentLevel += amount;
		while (PlayableUnit.ExperienceInCurrentLevel >= PlayableUnit.ExperienceNeededToNextLevel)
		{
			LevelUp();
		}
	}

	public void LevelUp()
	{
		PlayableUnit.ExperienceInCurrentLevel -= PlayableUnit.ExperienceNeededToNextLevel;
		PlayableUnit.Level++;
		PlayableUnit.UnitLevelUpPoints.Add(new UnitLevelUpPoint());
		PlayableUnit.LevelUp.CommonNbReroll += PlayableUnit.LevelUp.UnitLevelUpDefinition.MaxAmountOfReroll + TPSingleton<GlyphManager>.Instance.BonusLevelupRerolls;
		TPSingleton<MetaConditionManager>.Instance.RefreshMaxDoubleValue(MetaConditionSpecificContext.E_ValueCategory.MaxHeroLevelReached, PlayableUnit.Level);
		if (PlayableUnitDatabase.PerksPointsPerLevel.TryGetValue((int)PlayableUnit.Level, out var value))
		{
			PlayableUnit.PerksPoints += value;
		}
		ComputeExperienceNeededToNextLevel();
	}

	public override float GainHealth(float amount, bool refreshHud = true)
	{
		float num = base.GainHealth(amount, refreshHud);
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night)
		{
			TPSingleton<PlayableUnitManager>.Instance.NightReport.TonightHpLost -= num;
		}
		return num;
	}

	public float GainMana(float amount)
	{
		float @base = PlayableUnit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.Mana).Base;
		PlayableUnit.UnitStatsController.IncreaseBaseStat(UnitStatDefinition.E_Stat.Mana, amount, includeChildStat: false);
		(PlayableUnit.PlayableUnitView.UnitHUD as PlayableUnitHUD).PlayManaGainAnim(amount, base.Unit.GetClampedStatValue(UnitStatDefinition.E_Stat.Mana));
		return PlayableUnit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.Mana).Base - @base;
	}

	public List<TheLastStand.Model.Skill.Skill> GetAllSkillsNoCheck(bool sort = false)
	{
		List<TheLastStand.Model.Skill.Skill> list = new List<TheLastStand.Model.Skill.Skill>();
		foreach (KeyValuePair<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlot>> equipmentSlot in PlayableUnit.EquipmentSlots)
		{
			foreach (EquipmentSlot item in equipmentSlot.Value)
			{
				if (item.Item != null)
				{
					list.AddRange(item.Item.Skills);
				}
			}
		}
		list.AddRange(PlayableUnit.ContextualSkills);
		list.AddRange(PlayableUnit.NativeSkills);
		if (sort)
		{
			list.Sort(TheLastStand.Model.Skill.Skill.SharedSkillBarIndexComparer);
		}
		return list;
	}

	public List<EquipmentSlot> GetCompatibleSlots(TheLastStand.Model.Item.Item item)
	{
		List<EquipmentSlot> list = new List<EquipmentSlot>();
		foreach (KeyValuePair<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlot>> equipmentSlot in PlayableUnit.EquipmentSlots)
		{
			for (int i = 0; i < equipmentSlot.Value.Count; i++)
			{
				if (equipmentSlot.Value[i].EquipmentSlotController.IsItemCompatible(item))
				{
					list.Add(equipmentSlot.Value[i]);
				}
			}
		}
		return list;
	}

	public EquipmentSlot GetBestItemSlotToCompare(TheLastStand.Model.Item.Item item)
	{
		if (item.ItemDefinition.IsHandItem)
		{
			List<EquipmentSlot> value2;
			if (item.ItemDefinition.Hands == ItemDefinition.E_Hands.OffHand)
			{
				if (PlayableUnit.EquipmentSlots.TryGetValue(ItemSlotDefinition.E_ItemSlotId.LeftHand, out var value))
				{
					EquipmentSlot equipmentSlot = value[PlayableUnit.EquippedWeaponSetIndex];
					return equipmentSlot.BlockedByOtherSlot ?? equipmentSlot;
				}
			}
			else if (PlayableUnit.EquipmentSlots.TryGetValue(ItemSlotDefinition.E_ItemSlotId.RightHand, out value2))
			{
				return value2[PlayableUnit.EquippedWeaponSetIndex];
			}
			return null;
		}
		List<EquipmentSlot> list = new List<EquipmentSlot>();
		foreach (KeyValuePair<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlot>> equipmentSlot3 in PlayableUnit.EquipmentSlots)
		{
			EquipmentSlot? equipmentSlot2 = equipmentSlot3.Value.FirstOrDefault();
			if (equipmentSlot2 != null && equipmentSlot2.ItemSlotController.IsItemCompatible(item))
			{
				list.AddRange(equipmentSlot3.Value);
			}
		}
		return list.FirstOrDefault((EquipmentSlot slot) => slot.Item != null);
	}

	public List<TheLastStand.Model.Skill.Skill> GetEquipmentSkills(bool dontCheckPhase = false)
	{
		equipmentSkills.Clear();
		foreach (KeyValuePair<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlot>> equipmentSlot in PlayableUnit.EquipmentSlots)
		{
			if (ItemSlotDefinition.E_ItemSlotId.WeaponSlot.HasFlag(equipmentSlot.Key))
			{
				continue;
			}
			foreach (EquipmentSlot item in equipmentSlot.Value)
			{
				if (item.Item == null)
				{
					continue;
				}
				foreach (TheLastStand.Model.Skill.Skill skill in item.Item.Skills)
				{
					if (skill.SkillController.CheckConditions(PlayableUnit, dontCheckPhase))
					{
						equipmentSkills.Add(skill);
					}
				}
			}
		}
		return equipmentSkills;
	}

	public List<TheLastStand.Model.Skill.Skill> GetContextualSkills()
	{
		contextualSkills.Clear();
		foreach (TheLastStand.Model.Skill.Skill contextualSkill in PlayableUnit.ContextualSkills)
		{
			contextualSkill.SkillAction.SkillActionExecution.Caster = PlayableUnit;
			contextualSkill.SkillAction.SkillActionExecution.SkillSourceTileObject = PlayableUnit;
			if (contextualSkill.SkillController.CheckConditions(PlayableUnit))
			{
				contextualSkill.SkillAction.SkillActionExecution.SkillExecutionController.ComputeSkillRangeTiles(updateView: false);
				if (contextualSkill.SkillController.ComputeTargetsAndValidity(PlayableUnit) || contextualSkill.SkillDefinition.InvalidCastDisplayBehaviour == SkillDefinition.E_InvalidCastDisplayBehaviour.DisplayedUnavailable)
				{
					contextualSkills.Add(contextualSkill);
				}
			}
		}
		contextualSkills.Sort(TheLastStand.Model.Skill.Skill.SharedSkillBarIndexComparer);
		return contextualSkills;
	}

	public List<TheLastStand.Model.Skill.Skill> GetWeaponSkills(bool dontCheckPhase = false)
	{
		weaponSkills.Clear();
		foreach (KeyValuePair<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlot>> equipmentSlot in PlayableUnit.EquipmentSlots)
		{
			if (!ItemSlotDefinition.E_ItemSlotId.WeaponSlot.HasFlag(equipmentSlot.Key))
			{
				continue;
			}
			for (int i = 0; i < equipmentSlot.Value.Count; i++)
			{
				if (PlayableUnit.EquippedWeaponSetIndex != i || equipmentSlot.Value[i].Item == null)
				{
					continue;
				}
				foreach (TheLastStand.Model.Skill.Skill skill in equipmentSlot.Value[i].Item.Skills)
				{
					if (skill.SkillController.CheckConditions(PlayableUnit, dontCheckPhase))
					{
						weaponSkills.Add(skill);
					}
				}
			}
		}
		foreach (TheLastStand.Model.Skill.Skill nativeSkill in PlayableUnit.NativeSkills)
		{
			if (nativeSkill.SkillController.CheckConditions(PlayableUnit, dontCheckPhase))
			{
				weaponSkills.Add(nativeSkill);
			}
		}
		return weaponSkills;
	}

	public List<TheLastStand.Model.Skill.Skill> GetSkills(bool avoidContextualSkills = false, bool dontCheckPhase = false)
	{
		List<TheLastStand.Model.Skill.Skill> list = new List<TheLastStand.Model.Skill.Skill>();
		list.AddRange(GetWeaponSkills(dontCheckPhase));
		list.AddRange(GetEquipmentSkills(dontCheckPhase));
		if (!avoidContextualSkills)
		{
			list.AddRange(GetContextualSkills());
		}
		return list;
	}

	public List<TheLastStand.Model.Skill.Skill> GetSkillsFromSlotType(ItemSlotDefinition.E_ItemSlotId slotTypes)
	{
		List<TheLastStand.Model.Skill.Skill> list = new List<TheLastStand.Model.Skill.Skill>();
		foreach (KeyValuePair<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlot>> equipmentSlot in PlayableUnit.EquipmentSlots)
		{
			for (int i = 0; i < equipmentSlot.Value.Count; i++)
			{
				if (!slotTypes.HasFlag(equipmentSlot.Key) || (ItemSlotDefinition.E_ItemSlotId.WeaponSlot.HasFlag(equipmentSlot.Key) && PlayableUnit.EquippedWeaponSetIndex != i) || equipmentSlot.Value[i].ItemSlotDefinition.Id == ItemSlotDefinition.E_ItemSlotId.Usables || equipmentSlot.Value[i].Item == null)
				{
					continue;
				}
				foreach (TheLastStand.Model.Skill.Skill skill in equipmentSlot.Value[i].Item.Skills)
				{
					if (skill.SkillController.CheckConditions(PlayableUnit))
					{
						list.Add(skill);
					}
				}
			}
		}
		return list;
	}

	public override void LoseArmor(float amount, ISkillCaster attacker = null, bool refreshHud = true)
	{
		if (base.Unit.State != TheLastStand.Model.Unit.Unit.E_State.Dead)
		{
			base.LoseArmor(amount, attacker, refreshHud);
			PlayableUnit.LifetimeStats.LifetimeStatsController.IncreaseDamagesTakenOnArmor(amount);
		}
	}

	public override void LoseHealth(float amount, ISkillCaster attacker = null, bool refreshHud = true)
	{
		if (base.Unit.State != TheLastStand.Model.Unit.Unit.E_State.Dead)
		{
			base.LoseHealth(amount, attacker, refreshHud);
			float num = base.Unit.UnitStatsController.DecreaseBaseStat(UnitStatDefinition.E_Stat.Health, amount, includeChildStat: false, refreshHud);
			PlayableUnit.LifetimeStats.LifetimeStatsController.IncreaseHealthLost(num);
			TrophyManager.AppendValueToTrophiesConditions<NoHealthLostTrophyConditionController>(new object[2] { PlayableUnit.RandomId, num });
			TrophyManager.AppendValueToTrophiesConditions<HealthLostTrophyConditionController>(new object[2] { PlayableUnit.RandomId, num });
			if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night)
			{
				TPSingleton<PlayableUnitManager>.Instance.NightReport.TonightHpLost += num;
			}
			if (base.Unit.Health <= 0f)
			{
				PrepareForDeath(attacker);
			}
			else
			{
				UpdatePoisonFeedbackCondition();
			}
		}
	}

	public void OverrideBodyParts(Dictionary<string, BodyPartDefinition> overridingBodyParts, bool clear = false)
	{
		if (overridingBodyParts == null)
		{
			return;
		}
		BodyPart value = null;
		BodyPartDefinition bodyPartDefinition = null;
		BodyPartView bodyPartView = null;
		foreach (KeyValuePair<string, BodyPartDefinition> overridingBodyPart in overridingBodyParts)
		{
			bodyPartDefinition = (clear ? null : overridingBodyPart.Value);
			if (PlayableUnit.BodyParts.TryGetValue(overridingBodyPart.Key, out value) && bodyPartDefinition != value.BodyPartDefinitionOverride)
			{
				value.BodyPartDefinitionOverride = bodyPartDefinition;
				if ((Object)(object)(bodyPartView = value.GetBodyPartView(BodyPartDefinition.E_Orientation.Front)) != (Object)null)
				{
					bodyPartView.IsDirty = true;
				}
				if ((Object)(object)(bodyPartView = value.GetBodyPartView(BodyPartDefinition.E_Orientation.Back)) != (Object)null)
				{
					bodyPartView.IsDirty = true;
				}
			}
		}
	}

	public override void PaySkillCost(TheLastStand.Model.Skill.Skill skill)
	{
		base.Unit.UnitStatsController.DecreaseBaseStat(UnitStatDefinition.E_Stat.Mana, skill.ManaCost, includeChildStat: false, refreshHud: false);
		base.Unit.UnitStatsController.DecreaseBaseStat(UnitStatDefinition.E_Stat.Health, skill.HealthCost, includeChildStat: false, refreshHud: false);
		base.Unit.UnitStatsController.DecreaseBaseStat(UnitStatDefinition.E_Stat.ActionPoints, skill.ActionPointsCost, includeChildStat: false, refreshHud: false);
		TrophyManager.AppendValueToTrophiesConditions<ManaSpentTrophyConditionController>(new object[2] { PlayableUnit.RandomId, skill.ManaCost });
		PlayableUnit.LifetimeStats.LifetimeStatsController.IncreaseManaSpent(skill.ManaCost);
		if (skill.ActionPointsCost > 0)
		{
			TPSingleton<ToDoListView>.Instance.RefreshActionPointsNotification();
			PlayableUnit.ActionPointsSpentThisTurn += skill.ActionPointsCost;
		}
		PlayableUnit.PlayableUnitView.PlayableUnitHUD.PlayManaLossAnim(skill.ManaCost, base.Unit.GetClampedStatValue(UnitStatDefinition.E_Stat.Mana));
		PlayableUnit.PlayableUnitView.UnitHUD.PlayHealthLossAnim(skill.HealthCost, base.Unit.GetClampedStatValue(UnitStatDefinition.E_Stat.Health));
		SpendMovePoints(skill.MovePointsCost);
		if ((float)skill.HealthCost > 0f)
		{
			PlayableUnit.PlayableUnitController.UpdateInjuryStage();
		}
		if ((float)skill.ManaCost > 0f && PlayableUnit.GetClampedStatValue(UnitStatDefinition.E_Stat.Mana) == 0f)
		{
			TPSingleton<AchievementManager>.Instance.UnlockAchievement(AchievementContainer.ACH_OUT_OF_MANA);
		}
	}

	public override void PrepareForDeath(ISkillCaster killer = null)
	{
		base.PrepareForDeath(killer);
		TPSingleton<PlayableUnitManager>.Instance.InvokeDiedPlayableUnit(PlayableUnit);
	}

	public Task PrepareForMovement(int movePointsSpent, bool forceInstant = false)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Expected O, but got Unknown
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Expected O, but got Unknown
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Expected O, but got Unknown
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Expected O, but got Unknown
		MoveUnitCommand moveUnitCommand = new MoveUnitCommand(PlayableUnit, movePointsSpent, forceInstant);
		if (base.Unit.WillDieByPoison)
		{
			base.Unit.UnitView.UnitHUD.DisplayIconAndTileFeedback(show: false);
		}
		PlayableUnitManager.UnitsConversation.Execute(moveUnitCommand);
		if (base.Unit.WillDieByPoison)
		{
			Task moveUnitTask = moveUnitCommand.MoveUnitTask;
			moveUnitTask.OnCompleteAction = (UnityAction)Delegate.Combine((Delegate?)(object)moveUnitTask.OnCompleteAction, (Delegate?)(UnityAction)delegate
			{
				base.Unit.UnitView.UnitHUD.DisplayIconAndTileFeedback(show: true);
			});
		}
		Task moveUnitTask2 = moveUnitCommand.MoveUnitTask;
		UnityAction onCompleteAction = moveUnitTask2.OnCompleteAction;
		object obj = _003C_003Ec._003C_003E9__36_1;
		if (obj == null)
		{
			UnityAction val = delegate
			{
				TPSingleton<TutorialManager>.Instance.OnTrigger(E_TutorialTrigger.OnPlayableUnitMovement);
			};
			_003C_003Ec._003C_003E9__36_1 = val;
			obj = (object)val;
		}
		moveUnitTask2.OnCompleteAction = (UnityAction)Delegate.Combine((Delegate?)(object)onCompleteAction, (Delegate?)obj);
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night)
		{
			PlayableUnit.LifetimeStats.LifetimeStatsController.IncreaseTilesCrossed(movePointsSpent);
		}
		if (PlayableUnit.Path.Count > 0)
		{
			TPSingleton<PlayableUnitManager>.Instance.InvokeMovedPlayableUnit(PlayableUnit, PlayableUnit.Path[^1]);
		}
		return moveUnitCommand.MoveUnitTask;
	}

	public override Task PrepareForMovement(bool playWalkAnim = true, bool followPathOrientation = true, float moveSpeed = -1f, float delay = 0f, bool isMovementInstant = false)
	{
		Task result = base.PrepareForMovement(playWalkAnim, followPathOrientation, moveSpeed, delay, isMovementInstant);
		if (PlayableUnit.Path.Count > 0)
		{
			TPSingleton<PlayableUnitManager>.Instance.InvokeMovedPlayableUnit(PlayableUnit, PlayableUnit.Path[^1]);
		}
		return result;
	}

	public void RandomizeColors(ref CodeData codeData)
	{
		RandomizeColorSwapPaletteDefinition(PlayableUnitDatabase.PlayableUnitSkinColorDefinitions, out var colorSwapName);
		string id = GetRandomHairColorSwapPalette(colorSwapName, PlayableUnitDatabase.PlayableUnitHairColorDefinitions).Id;
		RandomizeColorSwapPaletteDefinition(PlayableUnitDatabase.PlayableUnitEyesColorDefinitions, out var colorSwapName2);
		DataColor randomPortraitBGColor = PlayableUnitView.GetRandomPortraitBGColor(PlayableUnit);
		CodeGenerator.EncodeColorDatas(new KeyValuePair<E_ColorTypes, int>[4]
		{
			new KeyValuePair<E_ColorTypes, int>((E_ColorTypes)0, IEnumerableExtension.IndexOf<string, ColorSwapPaletteDefinition>((IDictionary<string, ColorSwapPaletteDefinition>)PlayableUnitDatabase.PlayableUnitSkinColorDefinitions, colorSwapName)),
			new KeyValuePair<E_ColorTypes, int>((E_ColorTypes)1, IEnumerableExtension.IndexOf<string, ColorSwapPaletteDefinition>((IDictionary<string, ColorSwapPaletteDefinition>)PlayableUnitDatabase.PlayableUnitHairColorDefinitions, id)),
			new KeyValuePair<E_ColorTypes, int>((E_ColorTypes)2, IEnumerableExtension.IndexOf<string, ColorSwapPaletteDefinition>((IDictionary<string, ColorSwapPaletteDefinition>)PlayableUnitDatabase.PlayableUnitEyesColorDefinitions, colorSwapName2)),
			new KeyValuePair<E_ColorTypes, int>((E_ColorTypes)3, PlayableUnitDatabase.PortraitBackgroundColors.IndexOf(randomPortraitBGColor))
		}, ref codeData);
	}

	public void ReceiveDailyExperience(float experienceShare)
	{
		float num = 0f;
		int i = 0;
		for (int count = TPSingleton<PlayableUnitManager>.Instance.NightReport.KillsThisNight.Count; i < count; i++)
		{
			num += TPSingleton<PlayableUnitManager>.Instance.NightReport.KillsThisNight[i].GetTotalExperienceForEntity(PlayableUnit);
		}
		PlayableUnit.AdditionalNightExperience = Mathf.Round(PlayableUnit.AdditionalNightExperience);
		float num2 = experienceShare + num + PlayableUnit.AdditionalNightExperience;
		GainExperience(num2);
		((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).Log((object)$"Daily XP for {PlayableUnit.Name}: {experienceShare} shared + {num} from kills + {PlayableUnit.AdditionalNightExperience} additional.", (CLogLevel)0, false, false);
		if ((Object)(object)PlayableUnit.PlayableUnitView != (Object)null && num2 > 0f)
		{
			GainExperienceDisplay pooledComponent = ObjectPooler.GetPooledComponent<GainExperienceDisplay>("GainExperienceDisplay", ResourcePooler.LoadOnce<GainExperienceDisplay>("Prefab/Displayable Effect/UI Effect Displays/GainExperienceDisplay", failSilently: false), EffectManager.EffectDisplaysParent, dontSetParent: false);
			pooledComponent.Init((int)num2);
			PlayableUnit.UnitController.AddEffectDisplay(pooledComponent);
		}
		PlayableUnit.AdditionalNightExperience = 0f;
	}

	public override void RefreshStats()
	{
		base.RefreshStats();
		base.Unit.UnitStatsController.SetBaseStat(UnitStatDefinition.E_Stat.Mana, base.Unit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.Mana).Base);
		base.Unit.UnitStatsController.SetBaseStat(UnitStatDefinition.E_Stat.ActionPoints, base.Unit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.ActionPoints).Base);
	}

	public void RemoveTrait(UnitTraitDefinition trait)
	{
		AddSlots(trait.RemoveSlots);
		RemoveSlots(trait.AddSlots);
		PlayableUnit.PlayableUnitStatsController.OnTraitRemoved(trait);
		PlayableUnit.UnitTraitDefinitions.Remove(trait);
	}

	public void ResetContextualSkillsTurnUses()
	{
		if (PlayableUnit.ContextualSkills == null)
		{
			return;
		}
		for (int i = 0; i < PlayableUnit.ContextualSkills.Count; i++)
		{
			if (PlayableUnit.ContextualSkills[i].SkillDefinition.UsesPerTurnCount != -1)
			{
				PlayableUnit.ContextualSkills[i].UsesPerTurnRemaining = PlayableUnit.ContextualSkills[i].SkillDefinition.UsesPerTurnCount;
			}
		}
	}

	public void DecreaseActiveMomentumTiles(int amount)
	{
		PlayableUnit.MomentumTilesActive -= amount;
	}

	public void ResetPerksData(PerkDataContainer perkDataContainer = null)
	{
		foreach (KeyValuePair<string, TheLastStand.Model.Unit.Perk.Perk> perk in PlayableUnit.Perks)
		{
			perk.Value.TargetObject = perkDataContainer;
		}
	}

	public void SendEquipmentsToCityStash()
	{
		foreach (KeyValuePair<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlot>> equipmentSlot in PlayableUnit.EquipmentSlots)
		{
			foreach (EquipmentSlot item in equipmentSlot.Value)
			{
				if (item.Item != null)
				{
					TPSingleton<InventoryManager>.Instance.Inventory.InventoryController.AddItem(new ItemController(item.Item).Item);
				}
			}
		}
	}

	public void StartEquipmentTurn()
	{
		foreach (KeyValuePair<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlot>> equipmentSlot in PlayableUnit.EquipmentSlots)
		{
			for (int num = equipmentSlot.Value.Count - 1; num >= 0; num--)
			{
				equipmentSlot.Value[num].Item?.ItemController.StartTurn();
			}
		}
	}

	public override void StartTurn()
	{
		if (base.Unit.IsDead)
		{
			return;
		}
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Day || TPSingleton<GameManager>.Instance.Game.NightTurn == Game.E_NightTurn.PlayableUnits)
		{
			PlayableUnit.UnitStatsController.SnapBaseStatTo(UnitStatDefinition.E_Stat.MovePoints, UnitStatDefinition.E_Stat.MovePointsTotal);
			PlayableUnit.UnitStatsController.SnapBaseStatTo(UnitStatDefinition.E_Stat.ActionPoints, UnitStatDefinition.E_Stat.ActionPointsTotal);
			StartEquipmentTurn();
			switch (TPSingleton<GameManager>.Instance.Game.Cycle)
			{
			case Game.E_Cycle.Day:
				if (TPSingleton<GameManager>.Instance.Game.DayTurn != Game.E_DayTurn.Production)
				{
					break;
				}
				if (base.Unit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.HealthRegen).FinalClamped > 0f)
				{
					float num2 = GainHealth(PlayableUnit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.HealthRegen).FinalClamped, refreshHud: false);
					if (num2 > 0f)
					{
						HealFeedback healFeedback = base.Unit.DamageableView.HealFeedback;
						healFeedback.AddHealInstance(num2, base.Unit.Health);
						AddEffectDisplay(healFeedback);
					}
				}
				if (PlayableUnit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.ManaRegen).FinalClamped > 0f)
				{
					float num3 = GainMana(PlayableUnit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.ManaRegen).FinalClamped);
					if (num3 > 0f)
					{
						RestoreStatDisplay pooledComponent = ObjectPooler.GetPooledComponent<RestoreStatDisplay>("RestoreStatDisplay", ResourcePooler.LoadOnce<RestoreStatDisplay>("Prefab/Displayable Effect/UI Effect Displays/RestoreStatDisplay", failSilently: false), EffectManager.EffectDisplaysParent, dontSetParent: false);
						pooledComponent.Init(UnitStatDefinition.E_Stat.Mana, (int)num3);
						AddEffectDisplay(pooledComponent);
					}
				}
				RefillContextualSkillsOverallUses();
				RefillNativeSkillsOverallUses();
				RemoveAllStatuses();
				base.Unit.UnitView.RefreshInjuryStage();
				PlayableUnit.MovedThisDay = false;
				break;
			case Game.E_Cycle.Night:
			{
				int num = GetAdjacentTilesWithDiagonals().Count((Tile tile) => tile.Unit is EnemyUnit);
				TrophyManager.AppendValueToTrophiesConditions<HeroSurroundedByEnemiesTrophyConditionController>(new object[3] { PlayableUnit.RandomId, 1, num });
				TrophyManager.SetValueToTrophiesConditions<CriticalsInflictedSingleTurnTrophyConditionController>(new object[2] { PlayableUnit.RandomId, 0 });
				break;
			}
			}
			ResetContextualSkillsTurnUses();
			ResetNativeSkillsTurnUses();
			ResetCrossedTiles();
			ResetActiveMomentumTiles();
		}
		base.StartTurn();
		EffectManager.DisplayEffects();
	}

	public void SwitchWeaponSet()
	{
		if (!PlayableUnit.EquipmentSlots.TryGetValue(ItemSlotDefinition.E_ItemSlotId.RightHand, out var value) || value.Count > 1)
		{
			int equippedWeaponSetIndex = PlayableUnit.EquippedWeaponSetIndex;
			PlayableUnit.EquippedWeaponSetIndex = ((PlayableUnit.EquippedWeaponSetIndex == 0) ? 1 : 0);
			PlayableUnitManager.SelectedSkill = null;
			if (value != null)
			{
				EquipmentSlotView equipmentSlotView = CharacterSheetPanel.EquipmentSlots[ItemSlotDefinition.E_ItemSlotId.RightHand][0];
				value[PlayableUnit.EquippedWeaponSetIndex].EquipmentSlotView = equipmentSlotView;
				equipmentSlotView.EquipmentSlot = value[PlayableUnit.EquippedWeaponSetIndex];
				equipmentSlotView.Refresh();
				EquipmentSlotView equipmentSlotView2 = CharacterSheetPanel.EquipmentSlots[ItemSlotDefinition.E_ItemSlotId.RightHand][1];
				value[equippedWeaponSetIndex].EquipmentSlotView = equipmentSlotView2;
				equipmentSlotView2.EquipmentSlot = value[equippedWeaponSetIndex];
				equipmentSlotView2.Refresh();
				OverrideBodyParts(value[equippedWeaponSetIndex].Item?.ItemDefinition.BodyPartsDefinitions, clear: true);
				OverrideBodyParts(value[PlayableUnit.EquippedWeaponSetIndex].Item?.ItemDefinition.BodyPartsDefinitions);
			}
			if (PlayableUnit.EquipmentSlots.TryGetValue(ItemSlotDefinition.E_ItemSlotId.LeftHand, out var value2))
			{
				EquipmentSlotView equipmentSlotView3 = CharacterSheetPanel.EquipmentSlots[ItemSlotDefinition.E_ItemSlotId.LeftHand][0];
				value2[PlayableUnit.EquippedWeaponSetIndex].EquipmentSlotView = equipmentSlotView3;
				equipmentSlotView3.EquipmentSlot = value2[PlayableUnit.EquippedWeaponSetIndex];
				equipmentSlotView3.Refresh();
				EquipmentSlotView equipmentSlotView4 = CharacterSheetPanel.EquipmentSlots[ItemSlotDefinition.E_ItemSlotId.LeftHand][1];
				value2[equippedWeaponSetIndex].EquipmentSlotView = equipmentSlotView4;
				equipmentSlotView4.EquipmentSlot = value2[equippedWeaponSetIndex];
				equipmentSlotView4.Refresh();
				OverrideBodyParts(value2[equippedWeaponSetIndex].Item?.ItemDefinition.BodyPartsDefinitions, clear: true);
				OverrideBodyParts(value2[PlayableUnit.EquippedWeaponSetIndex].Item?.ItemDefinition.BodyPartsDefinitions);
			}
			for (int i = 0; i < TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count; i++)
			{
				TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i].UnitView.ToggleSkillTargeting(show: false);
			}
			for (int j = 0; j < TPSingleton<BuildingManager>.Instance.Buildings.Count; j++)
			{
				TPSingleton<BuildingManager>.Instance.Buildings[j].BuildingView.ToggleSkillTargeting(display: false);
			}
			if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Management || TPSingleton<GameManager>.Instance.Game.State == Game.E_State.UnitExecutingSkill)
			{
				UnitManagementView<PlayableUnitManagementView>.Refresh();
			}
		}
	}

	protected override Vector2Int ReduceIncomingDamageWithBlock(Vector2Int incomingDamage, bool updateLifetimeStats, out int blockValue)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		incomingDamage = base.ReduceIncomingDamageWithBlock(incomingDamage, updateLifetimeStats, out blockValue);
		if (updateLifetimeStats)
		{
			PlayableUnit.LifetimeStats.LifetimeStatsController.IncreaseDamagesBlocked(blockValue);
		}
		return incomingDamage;
	}

	protected override void OnDeath()
	{
		base.OnDeath();
		List<string> list = new List<string>();
		foreach (string key in PlayableUnit.Perks.Keys)
		{
			list.Add(key);
		}
		foreach (string item in list)
		{
			PlayableUnit.Perks[item].PerkController.UnHook(onLoad: false);
		}
		TrophyManager.AppendValueToTrophiesConditions<HeroDeadTrophyConditionController>(new object[1] { 1 });
		if (!TPSingleton<PlayableUnitManager>.Instance.DeadPlayableUnits.ContainsKey(TPSingleton<GameManager>.Instance.DayNumber))
		{
			TPSingleton<PlayableUnitManager>.Instance.DeadPlayableUnits.Add(TPSingleton<GameManager>.Instance.DayNumber, new List<PlayableUnit>());
		}
		TPSingleton<PlayableUnitManager>.Instance.DeadPlayableUnits[TPSingleton<GameManager>.Instance.DayNumber].Add(PlayableUnit);
		SendEquipmentsToCityStash();
		PlayableUnitManager.DestroyUnit(PlayableUnit);
	}

	private void AddGeneratedTrait(string traitId, ref int currentTraitPoints, bool forceAdd = false)
	{
		if (AddTrait(traitId, forceAdd))
		{
			currentTraitPoints -= PlayableUnitDatabase.UnitTraitDefinitions[traitId].Cost;
		}
	}

	private void AddSlots(List<UnitTraitDefinition.SlotModifier> addedSlots)
	{
		foreach (UnitTraitDefinition.SlotModifier addedSlot in addedSlots)
		{
			ItemSlotDefinition.E_ItemSlotId name = addedSlot.Name;
			for (int num = addedSlot.Amount - 1; num >= 0; num--)
			{
				List<EquipmentSlotView> list = CharacterSheetPanel.EquipmentSlots[name];
				if (!PlayableUnit.EquipmentSlots.TryGetValue(name, out var _))
				{
					((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).LogError((object)$"Slot {name} not found", (CLogLevel)1, true, true);
				}
				else
				{
					int count = PlayableUnit.EquipmentSlots[name].Count;
					if (PlayableUnit.EquipmentSlots.ContainsKey(name))
					{
						if (list.Count - 1 >= count)
						{
							PlayableUnit.EquipmentSlots[name].Add(new EquipmentSlotController(ItemDatabase.ItemSlotDefinitions[name], list[count], PlayableUnit).EquipmentSlot);
						}
					}
					else if (list.Count - 1 >= count)
					{
						PlayableUnit.EquipmentSlots.Add(name, new List<EquipmentSlot>());
						PlayableUnit.EquipmentSlots[name].Add(new EquipmentSlotController(ItemDatabase.ItemSlotDefinitions[name], list[count], PlayableUnit).EquipmentSlot);
					}
					if (count == 1 && name == ItemSlotDefinition.E_ItemSlotId.LeftHand)
					{
						PlayableUnit.BodyParts["Arm_L"].ChangeAdditionalConstraint("Hide", add: false);
					}
				}
			}
		}
	}

	private bool CanOnlyHaveTwoHandsWeapons(PlayableUnitGenerationDefinition unitGenerationDefinition)
	{
		List<string> unlockedItemIds = new List<string>();
		if (MetaUpgradeEffectsController.TryGetEffectsOfType<UnlockEquipmentGenerationMetaEffectDefinition>(out var effects, MetaUpgradesManager.E_MetaState.Activated))
		{
			for (int i = 0; i < effects.Length; i++)
			{
				unlockedItemIds.Add(effects[i].Id);
			}
		}
		TPSingleton<MetaUpgradesManager>.Instance.GetLockedItemsIds();
		List<Tuple<int, EquipmentGenerationDefinition.ItemGenerationData>> itemsPerWeight = unitGenerationDefinition.EquipmentGenerationDefinitions[ItemSlotDefinition.E_ItemSlotId.RightHand].ItemsPerWeight;
		for (int num = itemsPerWeight.Count - 1; num >= 0; num--)
		{
			string itemId = itemsPerWeight[num].Item2.ItemId;
			string itemsList = itemsPerWeight[num].Item2.ItemsList;
			if (string.IsNullOrEmpty(itemId) || unlockedItemIds.Contains(itemId))
			{
				if (!ItemDatabase.ItemsListDefinitions.ContainsKey(itemsList))
				{
					((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).LogError((object)("During hero generation, we failed to access the itemsList with Id " + itemsList + ". This id comes from the equipment generation definition, archetype : " + unitGenerationDefinition.ArchetypeId), (CLogLevel)2, true, true);
				}
				else if (ItemManager.AnyItemMatchingCondition(ItemDatabase.ItemsListDefinitions[itemsList], (ItemDefinition item) => item.Hands == ItemDefinition.E_Hands.OneHand && unlockedItemIds.Contains(itemId)))
				{
					return false;
				}
			}
		}
		return true;
	}

	private void ComputeExperienceNeededToNextLevel()
	{
		PlayableUnit.ExperienceNeededToNextLevel = PlayableUnitDatabase.ExperienceNeededToNextLevel.EvalToFloat(PlayableUnit);
	}

	private void Generate(int traitPoints, int level = 1)
	{
		string text = "--- PlayableUnit generation: " + PlayableUnit.Name + " ---";
		PlayableUnitGenerationDefinition playableUnitGenerationDefinition = PlayableUnitDatabase.PlayableUnitGenerationDefinitions[PlayableUnit.ArchetypeId];
		text += "\n---- Unit stats generation ----";
		base.Unit.UnitStatsController = new PlayableUnitStatsController(PlayableUnit);
		text += "\n---- End of unit stats generation ----";
		foreach (KeyValuePair<ItemSlotDefinition.E_ItemSlotId, UnitEquipmentSlotDefinition> unitEquipmentSlotDefinition in PlayableUnitDatabase.UnitEquipmentSlotDefinitions)
		{
			int i = 0;
			for (int @base = unitEquipmentSlotDefinition.Value.Base; i < @base; i++)
			{
				if (!PlayableUnit.EquipmentSlots.ContainsKey(unitEquipmentSlotDefinition.Key))
				{
					PlayableUnit.EquipmentSlots.Add(unitEquipmentSlotDefinition.Key, new List<EquipmentSlot>());
				}
				PlayableUnit.EquipmentSlots[unitEquipmentSlotDefinition.Key].Add(new EquipmentSlotController(ItemDatabase.ItemSlotDefinitions[unitEquipmentSlotDefinition.Key], CharacterSheetPanel.EquipmentSlots[unitEquipmentSlotDefinition.Key][i], PlayableUnit).EquipmentSlot);
			}
		}
		text += "\n---- Unit traits generation ----";
		text += $"\nStarts with {traitPoints} trait points";
		bool flag = false;
		List<string> list = new List<string>(playableUnitGenerationDefinition.BackgroundTraitAvailableIds);
		bool flag2 = CanOnlyHaveTwoHandsWeapons(playableUnitGenerationDefinition);
		if (flag2)
		{
			list.Remove("One-Armed");
		}
		string[] lockedTraitsIds = TPSingleton<MetaUpgradesManager>.Instance.GetLockedTraitsIds();
		foreach (string item in lockedTraitsIds)
		{
			if (list.Contains(item))
			{
				list.Remove(item);
			}
		}
		for (int num = list.Count - 1; num >= 0; num--)
		{
			string backgroundTraitId = RandomManager.GetRandomElement(this, list);
			text += $"\nTrying to add background trait: {backgroundTraitId} which costs {PlayableUnitDatabase.UnitTraitDefinitions[backgroundTraitId].Cost} points";
			List<string> list2 = new List<string>(PlayableUnitDatabase.SecondaryTraitIds);
			lockedTraitsIds = TPSingleton<MetaUpgradesManager>.Instance.GetLockedTraitsIds();
			foreach (string item2 in lockedTraitsIds)
			{
				if (list2.Contains(item2))
				{
					list2.Remove(item2);
				}
			}
			if (flag2)
			{
				list2.Remove("One-Armed");
			}
			for (int num2 = list2.Count - 1; num2 >= 0; num2--)
			{
				string secondTraitId = RandomManager.GetRandomElement(this, list2);
				text += $"\nTrying to add second trait: {secondTraitId} which costs {PlayableUnitDatabase.UnitTraitDefinitions[secondTraitId].Cost} points";
				if (!PlayableUnitDatabase.UnitTraitDefinitions[backgroundTraitId].Incompatibilities.Contains(secondTraitId))
				{
					int remainingCost = traitPoints - PlayableUnitDatabase.UnitTraitDefinitions[backgroundTraitId].Cost - PlayableUnitDatabase.UnitTraitDefinitions[secondTraitId].Cost;
					if (!PlayableUnitDatabase.SecondaryTraitCost.Contains(remainingCost))
					{
						text = text + "\nFAILED --> Can't find any matching third trait for " + backgroundTraitId + " & " + secondTraitId + " because there are all in first traits Incompatibilities or the trait points will not be all spent";
					}
					else
					{
						List<string> list3 = list2.FindAll((string id) => id != secondTraitId && !PlayableUnitDatabase.UnitTraitDefinitions[id].Incompatibilities.Contains(backgroundTraitId) && !PlayableUnitDatabase.UnitTraitDefinitions[id].Incompatibilities.Contains(secondTraitId) && PlayableUnitDatabase.UnitTraitDefinitions[id].Cost == remainingCost);
						if (list3.Count != 0)
						{
							string randomElement = RandomManager.GetRandomElement(this, list3);
							AddGeneratedTrait(backgroundTraitId, ref traitPoints, forceAdd: true);
							AddGeneratedTrait(secondTraitId, ref traitPoints, forceAdd: true);
							AddGeneratedTrait(randomElement, ref traitPoints, forceAdd: true);
							text += $"\nSUCCESS --> Third trait picked: {randomElement} which costs {PlayableUnitDatabase.UnitTraitDefinitions[randomElement].Cost} points";
							flag = true;
							break;
						}
						text = text + "\nFAILED --> Can't find any matching third trait for " + backgroundTraitId + " & " + secondTraitId + " because there are all in first traits Incompatibilities or the trait points will not be all spent";
					}
				}
				else
				{
					text = text + "\nFAILED --> " + secondTraitId + " is in " + backgroundTraitId + " Incompatibilities";
				}
				list2.Remove(backgroundTraitId);
			}
			if (flag)
			{
				break;
			}
			list.Remove(backgroundTraitId);
		}
		text += "\n---- End of unit traits generation ----";
		GenerateEquipment(playableUnitGenerationDefinition);
		PlayableUnit.UnitStatsController.SnapBaseStatTo(UnitStatDefinition.E_Stat.Health, UnitStatDefinition.E_Stat.HealthTotal);
		PlayableUnit.UnitStatsController.SnapBaseStatTo(UnitStatDefinition.E_Stat.Armor, UnitStatDefinition.E_Stat.ArmorTotal);
		PlayableUnit.UnitStatsController.SnapBaseStatTo(UnitStatDefinition.E_Stat.Mana, UnitStatDefinition.E_Stat.ManaTotal);
		PlayableUnit.UnitStatsController.SnapBaseStatTo(UnitStatDefinition.E_Stat.ActionPoints, UnitStatDefinition.E_Stat.ActionPointsTotal);
		PlayableUnit.UnitStatsController.SnapBaseStatTo(UnitStatDefinition.E_Stat.MovePoints, UnitStatDefinition.E_Stat.MovePointsTotal);
		UpdateInjuryStage();
		if (PlayableUnit.EquippedWeaponSetIndex != 0)
		{
			SwitchWeaponSet();
		}
		while ((double)level > PlayableUnit.Level)
		{
			LevelUp();
			PlayableUnit.ExperienceInCurrentLevel = 0f;
		}
		text += "\n--- End of playableUnit generation ---\n";
		((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).Log((object)text, (CLogLevel)1, false, false);
	}

	private void GenerateContextualSkills()
	{
		PlayableUnit.ContextualSkills = new List<TheLastStand.Model.Skill.Skill>();
		foreach (string contextualSkill in SkillDatabase.ContextualSkills)
		{
			if (!SkillDatabase.SkillDefinitions.TryGetValue(contextualSkill, out var value))
			{
				((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).LogError((object)("Skill " + contextualSkill + " not found!"), (CLogLevel)1, true, true);
			}
			else if (!value.IsLockedByPerk && (!value.IsBrazierSpecific || TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.BrazierDefinition != null))
			{
				TheLastStand.Model.Skill.Skill skill = new SkillController(value, PlayableUnit, -1, value.UsesPerTurnCount).Skill;
				PlayableUnit.ContextualSkills.Add(skill);
			}
		}
	}

	private void GenerateEquipment(PlayableUnitGenerationDefinition unitGenerationDefinition)
	{
		List<string> list = new List<string>();
		string[] lockedItemsIds = TPSingleton<MetaUpgradesManager>.Instance.GetLockedItemsIds();
		if (MetaUpgradeEffectsController.TryGetEffectsOfType<UnlockEquipmentGenerationMetaEffectDefinition>(out var effects, MetaUpgradesManager.E_MetaState.Activated))
		{
			for (int i = 0; i < effects.Length; i++)
			{
				list.Add(effects[i].Id);
			}
		}
		foreach (KeyValuePair<ItemSlotDefinition.E_ItemSlotId, EquipmentGenerationDefinition> equipmentGenerationDefinition in unitGenerationDefinition.EquipmentGenerationDefinitions)
		{
			ItemSlotDefinition.E_ItemSlotId key = equipmentGenerationDefinition.Key;
			if (!PlayableUnit.EquipmentSlots.ContainsKey(key))
			{
				continue;
			}
			EquipmentGenerationDefinition value = equipmentGenerationDefinition.Value;
			int num = value.TotalWeight;
			List<Tuple<int, EquipmentGenerationDefinition.ItemGenerationData>> list2 = new List<Tuple<int, EquipmentGenerationDefinition.ItemGenerationData>>(value.ItemsPerWeight);
			for (int j = 0; j < list2.Count; j++)
			{
				if (list2[j].Item2.ItemId != string.Empty && (!list.Contains(list2[j].Item2.ItemId) || IsItemGenerationDataLocked(list2[j].Item2, lockedItemsIds)))
				{
					num -= list2[j].Item1;
					list2.RemoveAt(j--);
				}
			}
			int num2 = RandomManager.GetRandomRange(this, 0, num);
			foreach (Tuple<int, EquipmentGenerationDefinition.ItemGenerationData> item2 in list2)
			{
				num2 -= item2.Item1;
				if (num2 >= 0)
				{
					continue;
				}
				string itemsList = item2.Item2.ItemsList;
				int level = new LevelProbabilitiesTreeController(unitGenerationDefinition.BaseGenerationLevel, ItemDatabase.ItemGenerationModifierListDefinitions[item2.Item2.ItemLevelModifiersList]).GenerateLevel();
				ItemsListDefinition itemsListDefinition = ItemDatabase.ItemsListDefinitions[itemsList];
				int num3 = 0;
				bool flag = false;
				foreach (KeyValuePair<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlot>> equipmentSlot in PlayableUnit.EquipmentSlots)
				{
					if (equipmentSlot.Key == ItemSlotDefinition.E_ItemSlotId.LeftHand)
					{
						flag = true;
						break;
					}
				}
				Func<ItemDefinition, bool> func = (ItemDefinition itemDefinition) => itemDefinition.Hands == ItemDefinition.E_Hands.OneHand;
				bool flag2;
				do
				{
					flag2 = true;
					ItemDefinition itemDefinition2 = ItemManager.TakeRandomItemInList(itemsListDefinition, (key == ItemSlotDefinition.E_ItemSlotId.RightHand && !flag) ? func : null);
					if (itemDefinition2 != null)
					{
						if (itemDefinition2.Hands == ItemDefinition.E_Hands.TwoHands && !flag)
						{
							flag2 = false;
						}
						if (flag2)
						{
							ItemManager.ItemGenerationInfo generationInfo = default(ItemManager.ItemGenerationInfo);
							generationInfo.Destination = key;
							generationInfo.ItemDefinition = itemDefinition2;
							generationInfo.Level = level;
							generationInfo.Rarity = RarityProbabilitiesTreeController.GenerateRarity(ItemDatabase.ItemRaritiesListDefinitions[item2.Item2.ItemRaritiesList]);
							generationInfo.SkipMalusAffixes = true;
							TheLastStand.Model.Item.Item item = ItemManager.GenerateItem(generationInfo);
							EquipItem(item, null, shouldRefreshMetaCondition: false, onLoad: true);
						}
					}
				}
				while (!flag2 && ++num3 < 1000);
				if (num3 == 1000)
				{
					((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).LogWarning((object)("The generation of " + PlayableUnit.PlayableUnitName + "'s equipment took way longer than expected and couldn't find a suitable item."), (CLogLevel)1, true, false);
				}
				break;
			}
		}
	}

	private bool IsItemGenerationDataLocked(EquipmentGenerationDefinition.ItemGenerationData itemGenerationData, string[] lockedItemIds)
	{
		if (ItemDatabase.ItemsListDefinitions.TryGetValue(itemGenerationData.ItemsList, out var value))
		{
			return ItemManager.IsItemsListContentLocked(value, lockedItemIds);
		}
		return true;
	}

	private void GenerateNativePerks(List<SerializedPerk> serializedNativePerks = null, bool isDead = false)
	{
		if (serializedNativePerks != null)
		{
			foreach (SerializedPerk serializedNativePerk in serializedNativePerks)
			{
				if (PlayableUnitDatabase.PerkDefinitions.TryGetValue(serializedNativePerk.Id, out var value))
				{
					new PerkController(serializedNativePerk, value, null, PlayableUnit, null, string.Empty, isDead, isNative: true).Perk.PerkController.Unlock();
				}
			}
			return;
		}
		foreach (PerkDefinition value2 in TPSingleton<GlyphManager>.Instance.NativePerksToUnlock.Values)
		{
			new PerkController(value2, null, PlayableUnit, null, string.Empty, isNative: true).Perk.PerkController.Unlock();
		}
		PlayableUnit.PerksPoints += TPSingleton<GlyphManager>.Instance.NativePerkPointsBonus;
	}

	private void GenerateNativeSkills(bool onLoad)
	{
		if (SkillDatabase.SkillDefinitions.TryGetValue("Punch", out var value))
		{
			PlayableUnit.NativeSkills.Add(new SkillController(value, PlayableUnit).Skill);
		}
	}

	private EquipmentSlot GetBestItemSlot(TheLastStand.Model.Item.Item item)
	{
		foreach (KeyValuePair<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlot>> equipmentSlot2 in PlayableUnit.EquipmentSlots)
		{
			EquipmentSlot equipmentSlot = null;
			for (int i = 0; i < equipmentSlot2.Value.Count; i++)
			{
				if (!equipmentSlot2.Value[i].EquipmentSlotController.IsItemCompatible(item))
				{
					continue;
				}
				if ((ItemSlotDefinition.E_ItemSlotId.Trinket | ItemSlotDefinition.E_ItemSlotId.Usables).HasFlag(equipmentSlot2.Value[i].ItemSlotDefinition.Id) && equipmentSlot2.Value[i].Item != null)
				{
					if (equipmentSlot == null)
					{
						equipmentSlot = equipmentSlot2.Value[i];
					}
				}
				else if (!ItemSlotDefinition.E_ItemSlotId.WeaponSlot.HasFlag(equipmentSlot2.Value[i].ItemSlotDefinition.Id) || (PlayableUnit.EquippedWeaponSetIndex == i && (equipmentSlot2.Value[i].ItemSlotDefinition.Id != ItemSlotDefinition.E_ItemSlotId.LeftHand || equipmentSlot2.Value[i].BlockedByOtherSlot == null) && (equipmentSlot2.Value[i].ItemSlotDefinition.Id != ItemSlotDefinition.E_ItemSlotId.RightHand || !item.IsTwoHandedWeapon || equipmentSlot2.Value[i].EquipmentSlotController.CanEquipTwoHandedWeapon(item))))
				{
					return equipmentSlot2.Value[i];
				}
			}
			if (equipmentSlot != null)
			{
				return equipmentSlot;
			}
		}
		return null;
	}

	private string GetRandomFaceId(string gender)
	{
		UnitFaceIdDefinitions unitFaceIdDefinitions = ((gender == "Female") ? PlayableUnitDatabase.PlayableFemaleUnitFaceIds : PlayableUnitDatabase.PlayableMaleUnitFaceIds);
		string result = string.Empty;
		int num = 0;
		for (int i = 0; i < unitFaceIdDefinitions.Count; i++)
		{
			num += unitFaceIdDefinitions[i].Weight;
		}
		int randomRange = RandomManager.GetRandomRange(TPSingleton<PlayableUnitManager>.Instance, 0, num);
		int num2 = 0;
		for (int j = 0; j < unitFaceIdDefinitions.Count; j++)
		{
			if (randomRange >= num2 && randomRange < unitFaceIdDefinitions[j].Weight + num2)
			{
				result = unitFaceIdDefinitions[j].FaceId;
				break;
			}
			num2 += unitFaceIdDefinitions[j].Weight;
		}
		return result;
	}

	private void RefillNativeSkillsOverallUses()
	{
		foreach (TheLastStand.Model.Skill.Skill nativeSkill in PlayableUnit.NativeSkills)
		{
			if (nativeSkill.OverallUsesRemaining != -1)
			{
				nativeSkill.OverallUsesRemaining = nativeSkill.ComputeTotalUses();
			}
		}
	}

	private void ResetNativeSkillsTurnUses()
	{
		foreach (TheLastStand.Model.Skill.Skill nativeSkill in PlayableUnit.NativeSkills)
		{
			if (nativeSkill.SkillDefinition.UsesPerTurnCount != -1)
			{
				nativeSkill.UsesPerTurnRemaining = nativeSkill.SkillDefinition.UsesPerTurnCount;
			}
		}
	}

	private void RefillContextualSkillsOverallUses()
	{
		foreach (TheLastStand.Model.Skill.Skill contextualSkill in PlayableUnit.ContextualSkills)
		{
			if (contextualSkill.OverallUsesRemaining != -1)
			{
				contextualSkill.OverallUsesRemaining = contextualSkill.ComputeTotalUses();
			}
		}
	}

	private void ResetCrossedTiles()
	{
		PlayableUnit.TilesCrossedThisTurn = 0;
		PlayableUnit.TotalMomentumTilesCrossedThisTurn = 0;
	}

	private void ResetActiveMomentumTiles()
	{
		PlayableUnit.MomentumTilesActive = 0;
	}

	private void RemoveSlots(List<UnitTraitDefinition.SlotModifier> removedSlots)
	{
		foreach (UnitTraitDefinition.SlotModifier removedSlot in removedSlots)
		{
			ItemSlotDefinition.E_ItemSlotId name = removedSlot.Name;
			for (int num = removedSlot.Amount - 1; num >= 0; num--)
			{
				if (PlayableUnit.EquipmentSlots.ContainsKey(name))
				{
					PlayableUnit.EquipmentSlots[name].RemoveAt(PlayableUnit.EquipmentSlots[name].Count - 1);
					if (PlayableUnit.EquipmentSlots[name].Count == 0)
					{
						PlayableUnit.EquipmentSlots.Remove(name);
						if (name == ItemSlotDefinition.E_ItemSlotId.LeftHand)
						{
							PlayableUnit.BodyParts["Arm_L"].ChangeAdditionalConstraint("Hide", add: true);
						}
					}
				}
			}
		}
	}

	public void DebugSetColorPalette(ColorSwapPaletteDefinition paletteDefinition, bool isHairPalette)
	{
	}

	public void DebugSetFaceId(string faceId)
	{
		if (!string.IsNullOrEmpty(faceId))
		{
			PlayableUnit.FaceId = faceId;
			PlayableUnit.PlayableUnitView.RefreshBodyParts(forceFullRefresh: true);
			DebugSetPortraitSprite();
		}
	}

	public void DebugSetPortraitSprite(string portraitId = null)
	{
		PlayableUnitView.RemoveUsedPortrait(PlayableUnit.PortraitSprite);
		if (portraitId == null)
		{
			PlayableUnitView.GenerateRandomPortrait(PlayableUnit);
		}
		else
		{
			Sprite val = ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Units/Portaits/Playable Unit/Foreground/" + PlayableUnit.Gender + "/" + PlayableUnit.FaceId + "/" + portraitId, failSilently: false);
			Sprite portraitBackgroundSprite = ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/Units/Portaits/Playable Unit/Background/" + PlayableUnit.Gender + "/" + PlayableUnit.FaceId + "/" + portraitId, failSilently: false);
			PlayableUnit.PortraitSprite = val;
			PlayableUnit.PortraitBackgroundSprite = portraitBackgroundSprite;
			PlayableUnitView.AddUsedPortrait(val);
		}
		PlayableUnitManagementView.UnitPortraitView.RefreshPortrait();
		GameView.TopScreenPanel.UnitPortraitsPanel.RefreshPortraits();
	}
}
