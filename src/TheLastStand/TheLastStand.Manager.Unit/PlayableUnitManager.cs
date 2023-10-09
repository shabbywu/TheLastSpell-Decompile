using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TPLib;
using TPLib.Debugging;
using TPLib.Debugging.Console;
using TPLib.Log;
using TPLib.Yield;
using TheLastStand.Controller;
using TheLastStand.Controller.ApplicationState;
using TheLastStand.Controller.Item;
using TheLastStand.Controller.TileMap;
using TheLastStand.Controller.Trophy.TrophyConditions;
using TheLastStand.Controller.Unit;
using TheLastStand.Controller.Unit.Pathfinding;
using TheLastStand.Controller.Unit.Stat;
using TheLastStand.DRM.Achievements;
using TheLastStand.Database;
using TheLastStand.Database.Unit;
using TheLastStand.Definition;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Meta.Glyphs.GlyphEffects;
using TheLastStand.Definition.Skill.SkillAction;
using TheLastStand.Definition.Skill.SkillEffect;
using TheLastStand.Definition.Unit;
using TheLastStand.Definition.Unit.Trait;
using TheLastStand.Framework.Command.Conversation;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Sequencing;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager.Achievements;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Item;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.Skill;
using TheLastStand.Manager.Sound;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Item;
using TheLastStand.Model.Meta;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Skill.SkillAction;
using TheLastStand.Model.Skill.SkillAction.SkillActionExecution;
using TheLastStand.Model.Status;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Model.Unit.Movement;
using TheLastStand.Model.Unit.Pathfinding;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.Model.Unit.Stat;
using TheLastStand.Serialization;
using TheLastStand.Serialization.Unit;
using TheLastStand.View;
using TheLastStand.View.Camera;
using TheLastStand.View.CharacterSheet;
using TheLastStand.View.Cursor;
using TheLastStand.View.HUD.UnitManagement;
using TheLastStand.View.ProductionReport;
using TheLastStand.View.Seer;
using TheLastStand.View.Sound;
using TheLastStand.View.TileMap;
using TheLastStand.View.ToDoList;
using TheLastStand.View.Unit;
using TheLastStand.View.Unit.Pathfinding;
using TheLastStand.View.Unit.Perk;
using TheLastStand.View.Unit.Stat;
using TheLastStand.View.Unit.Trait;
using TheLastStand.View.Unit.UI;
using TheLastStand.View.UnitManagement.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace TheLastStand.Manager.Unit;

[StringConverter(typeof(StringToTPSingletonConverter<PlayableUnitManager>))]
public sealed class PlayableUnitManager : Manager<PlayableUnitManager>, ISerializable, IDeserializable
{
	public static class Consts
	{
		public const string PlayableUnits = "PlayableUnits";

		public const string HitsAudioSourcePoolId = "HitsSFX";

		public const string HitsSpatializedAudioSourcePoolId = "HitsSFX Spatialized";

		public const string PlayableUnitHitsSoundAssetPrefix = "Sounds/SFX/PlayableUnitHits/";
	}

	public class StringToPerkIdConverter : StringToStringCollectionEntryConverter
	{
		protected override List<string> Entries
		{
			get
			{
				List<string> list = new List<string>();
				if (TileObjectSelectionManager.SelectedPlayableUnit != null)
				{
					foreach (UnitPerkTier unitPerkTier in TileObjectSelectionManager.SelectedPlayableUnit.PerkTree.UnitPerkTiers)
					{
						foreach (Perk perk in unitPerkTier.Perks)
						{
							if (perk != null && !perk.Unlocked)
							{
								list.Add(perk.PerkDefinition.Id);
							}
						}
					}
				}
				return list;
			}
		}
	}

	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static Func<PlayableUnit, bool> _003C_003E9__40_0;

		public static Func<PlayableUnit, bool> _003C_003E9__91_0;

		public static Func<PlayableUnit, bool> _003C_003E9__91_1;

		public static Func<PlayableUnit, bool> _003C_003E9__93_0;

		public static Func<PlayableUnit, bool> _003C_003E9__93_1;

		public static Func<PlayableUnit, bool> _003C_003E9__97_0;

		public static Func<PlayableUnit, bool> _003C_003E9__97_2;

		public static Func<PlayableUnit, int> _003C_003E9__104_0;

		public static Func<PlayableUnit, int> _003C_003E9__104_1;

		public static Func<Tile, bool> _003C_003E9__108_0;

		public static UnityAction _003C_003E9__131_0;

		public static Func<UnitPerkTier, IEnumerable<Perk>> _003C_003E9__192_0;

		public static Func<PlayableUnit, SerializedPlayableUnit> _003C_003E9__200_0;

		internal bool _003Cget_HasUnitInFog_003Eb__40_0(PlayableUnit x)
		{
			return x.OriginTile.HasFog;
		}

		internal bool _003Cget_ShouldWaitUntilDeathSequences_003Eb__91_0(PlayableUnit o)
		{
			return o.IsDead;
		}

		internal bool _003Cget_ShouldWaitUntilDeathSequences_003Eb__91_1(PlayableUnit o)
		{
			return !o.PlayableUnitView.DeathSequenceOver;
		}

		internal bool _003Cget_ShouldTriggerPlayableUnitsDeathSequence_003Eb__93_0(PlayableUnit u)
		{
			return u.IsDead;
		}

		internal bool _003Cget_ShouldTriggerPlayableUnitsDeathSequence_003Eb__93_1(PlayableUnit u)
		{
			return !u.IsDying;
		}

		internal bool _003Cget_WaitUntilTakeDamageSequences_003Eb__97_0(PlayableUnit o)
		{
			return o.UnitView.IsTakingDamage;
		}

		internal bool _003Cget_WaitUntilTakeDamageSequences_003Eb__97_2(PlayableUnit o)
		{
			return o.UnitView.IsTakingDamage;
		}

		internal int _003CEndTurn_003Eb__104_0(PlayableUnit o)
		{
			return o.ActionPointsSpentThisTurn;
		}

		internal int _003CEndTurn_003Eb__104_1(PlayableUnit o)
		{
			return o.TilesCrossedThisTurn;
		}

		internal bool _003CInstantiateUnit_003Eb__108_0(Tile tile)
		{
			return tile.HasLightFogOn;
		}

		internal void _003CMoveUnit_003Eb__131_0()
		{
			GameController.SetState(Game.E_State.Management);
		}

		internal IEnumerable<Perk> _003CDebug_UnlockAllPerks_003Eb__192_0(UnitPerkTier perkTier)
		{
			return perkTier.Perks;
		}

		internal SerializedPlayableUnit _003CSerialize_003Eb__200_0(PlayableUnit o)
		{
			return o.Serialize() as SerializedPlayableUnit;
		}
	}

	[SerializeField]
	private Transform unitsTransform;

	[SerializeField]
	private Transform unitHudsTransform;

	[SerializeField]
	private PlayableUnitView playableUnitViewPrefab;

	[SerializeField]
	private PlayableUnitGhostView playableUnitGhostViewPrefab;

	[SerializeField]
	private Transform playableUnitGhostParent;

	[FormerlySerializedAs("statTooltipPanel")]
	[SerializeField]
	private StatTooltip statTooltip;

	[FormerlySerializedAs("traitTooltipPanel")]
	[SerializeField]
	private TraitTooltip traitTooltip;

	[FormerlySerializedAs("perkTooltipPanel")]
	[SerializeField]
	private PerkTooltip perkTooltip;

	[SerializeField]
	private PlayableUnitTooltip playableUnitTooltip;

	[SerializeField]
	private MovePathView movePathView;

	[SerializeField]
	private OneShotSound hitSFXPrefab;

	[SerializeField]
	private OneShotSound hitSFXSpatializedPrefab;

	[SerializeField]
	private Vector2 perkReplacementRandomDelay = new Vector2(0f, 0.01f);

	private TheLastStand.Model.Skill.Skill selectedSkill;

	private SkillActionExecution hoverSkillExecution;

	private readonly Dictionary<int, TheLastStand.Model.Skill.Skill> skillHotkeys = new Dictionary<int, TheLastStand.Model.Skill.Skill>();

	private MovePath movePath;

	private readonly CompensationConversation unitsConversation = new CompensationConversation(isRedoable: false);

	private readonly List<PlayableUnitGhostView> playableUnitGhostView = new List<PlayableUnitGhostView>();

	private OneShotSound hitSFX;

	[SerializeField]
	private bool debugForceSkipNightReport;

	private bool debugDisableHealthDisplay;

	private bool debugToggleDismissHeroValidityChecks = true;

	public static MovePath MovePath => TPSingleton<PlayableUnitManager>.Instance.movePath;

	public static CompensationConversation UnitsConversation => TPSingleton<PlayableUnitManager>.Instance.unitsConversation;

	public static Vector2 PerkReplacementRandomDelay => TPSingleton<PlayableUnitManager>.Instance.perkReplacementRandomDelay;

	public static PerkTooltip PerkTooltip => TPSingleton<PlayableUnitManager>.Instance.perkTooltip;

	public static PlayableUnitTooltip PlayableUnitTooltip => TPSingleton<PlayableUnitManager>.Instance.playableUnitTooltip;

	public static TheLastStand.Model.Skill.Skill SelectedSkill
	{
		get
		{
			return TPSingleton<PlayableUnitManager>.Instance.selectedSkill;
		}
		set
		{
			//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
			//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0324: Unknown result type (might be due to invalid IL or missing references)
			//IL_0329: Unknown result type (might be due to invalid IL or missing references)
			//IL_035b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0360: Unknown result type (might be due to invalid IL or missing references)
			//IL_0381: Unknown result type (might be due to invalid IL or missing references)
			//IL_0386: Unknown result type (might be due to invalid IL or missing references)
			TheLastStand.Model.Skill.Skill skill = TPSingleton<PlayableUnitManager>.Instance.selectedSkill;
			TPSingleton<PlayableUnitManager>.Instance.selectedSkill = value;
			if (TPSingleton<PlayableUnitManager>.Instance.selectedSkill == skill)
			{
				return;
			}
			TPSingleton<TileMapView>.Instance.ClearRangedSkillsModifiers();
			TPSingleton<PlayableUnitManager>.Instance.PreviewSkillExecution?.SkillExecutionController.Reset();
			if (skill != null)
			{
				skill.SkillAction.SkillActionExecution.SkillExecutionController.Reset();
				if ((skill.SkillDefinition.CanRotate || SkillManager.DebugSkillsForceCanRotate) && (TPSingleton<PlayableUnitManager>.Instance.selectedSkill == null || (!TPSingleton<PlayableUnitManager>.Instance.selectedSkill.SkillDefinition.CanRotate && !SkillManager.DebugSkillsForceCanRotate)) && TileObjectSelectionManager.CursorOrientationFromSelection.HasFlag(TileObjectSelectionManager.E_Orientation.LIMIT))
				{
					TileMapView.SetTile(TileMapView.SkillRotationFeedbackTileMap, TPSingleton<GameManager>.Instance.Game.Cursor.Tile);
				}
			}
			if (TPSingleton<PlayableUnitManager>.Instance.selectedSkill != null)
			{
				TPSingleton<GameManager>.Instance.Game.Cursor.Tile?.Building?.BuildingView.HideSkillRangeIfNeeded();
				GenericSkillActionDefinition genericSkillActionDefinition = TPSingleton<PlayableUnitManager>.Instance.selectedSkill.SkillDefinition.SkillActionDefinition as GenericSkillActionDefinition;
				if (TPSingleton<SettingsManager>.Instance.Settings.SmartCast && !InputManager.IsLastControllerJoystick && genericSkillActionDefinition != null && genericSkillActionDefinition.CasterEffectOnly)
				{
					TPSingleton<PlayableUnitManager>.Instance.selectedSkill.SkillAction.SkillActionExecution.SkillExecutionController.PrepareSkill(TPSingleton<PlayableUnitManager>.Instance.selectedSkill.Owner, TPSingleton<PlayableUnitManager>.Instance.selectedSkill.Owner.OriginTile);
					SelectedSkill.SkillAction.SkillActionExecution.SkillExecutionController.AddTarget(TPSingleton<PlayableUnitManager>.Instance.selectedSkill.Owner.OriginTile, SelectedSkill.CursorDependantOrientation);
					TPSingleton<PlayableUnitManager>.Instance.PreviewSkillExecution = null;
					TPSingleton<PlayableUnitManager>.Instance.CastSelectedSkill();
					return;
				}
				GameController.SetState(Game.E_State.UnitPreparingSkill);
				TPSingleton<PlayableUnitManager>.Instance.HasToRecomputeReachableTiles = true;
				TPSingleton<PlayableUnitManager>.Instance.selectedSkill.SkillAction.SkillActionExecution.SkillExecutionController.PrepareSkill(TileObjectSelectionManager.SelectedUnit, TileObjectSelectionManager.SelectedUnit.OriginTile);
				if (TPSingleton<PlayableUnitManager>.Instance.selectedSkill.SkillAction is AttackSkillAction)
				{
					SkillManager.AttackInfoPanel.SetSkill(TPSingleton<PlayableUnitManager>.Instance.selectedSkill, TileObjectSelectionManager.SelectedUnit);
					SkillManager.GenericActionInfoPanel.Hide();
					TheLastStand.Model.Unit.Unit unit = TPSingleton<GameManager>.Instance.Game.Cursor.Tile?.Unit;
					if (unit == TileObjectSelectionManager.SelectedUnit)
					{
						int count = SelectedSkill.SkillDefinition.AreaOfEffectDefinition.Pattern.Count;
						Vector2Int origin = TPSingleton<PlayableUnitManager>.Instance.selectedSkill.SkillDefinition.AreaOfEffectDefinition.Origin;
						if (count > ((Vector2Int)(ref origin)).x)
						{
							List<List<char>> pattern = SelectedSkill.SkillDefinition.AreaOfEffectDefinition.Pattern;
							origin = TPSingleton<PlayableUnitManager>.Instance.selectedSkill.SkillDefinition.AreaOfEffectDefinition.Origin;
							int count2 = pattern[((Vector2Int)(ref origin)).x].Count;
							origin = TPSingleton<PlayableUnitManager>.Instance.selectedSkill.SkillDefinition.AreaOfEffectDefinition.Origin;
							if (count2 > ((Vector2Int)(ref origin)).y)
							{
								List<List<char>> pattern2 = SelectedSkill.SkillDefinition.AreaOfEffectDefinition.Pattern;
								origin = TPSingleton<PlayableUnitManager>.Instance.selectedSkill.SkillDefinition.AreaOfEffectDefinition.Origin;
								List<char> list = pattern2[((Vector2Int)(ref origin)).x];
								origin = TPSingleton<PlayableUnitManager>.Instance.selectedSkill.SkillDefinition.AreaOfEffectDefinition.Origin;
								if (list[((Vector2Int)(ref origin)).y] != 'X')
								{
									goto IL_0472;
								}
							}
						}
					}
					SkillManager.AttackInfoPanel.TargetTile = TPSingleton<GameManager>.Instance.Game.Cursor.Tile;
					SkillManager.AttackInfoPanel.TargetUnit = unit;
					SkillManager.AttackInfoPanel.Display();
					if (EnemyUnitManager.IsAnyEnemyTooltipDisplayed())
					{
						EnemyUnitManager.GetDisplayedEnemyTootlip().Refresh();
					}
					else if (PlayableUnitTooltip.Displayed)
					{
						PlayableUnitTooltip.Refresh();
					}
				}
				else if (genericSkillActionDefinition != null)
				{
					SkillManager.GenericActionInfoPanel.SetSkill(TPSingleton<PlayableUnitManager>.Instance.selectedSkill, TileObjectSelectionManager.SelectedUnit);
					SkillManager.AttackInfoPanel.Hide();
					SkillManager.GenericActionInfoPanel.Display();
					if (EnemyUnitManager.IsAnyEnemyTooltipDisplayed())
					{
						EnemyUnitManager.GetDisplayedEnemyTootlip().Refresh();
					}
					else if (PlayableUnitTooltip.Displayed)
					{
						PlayableUnitTooltip.Refresh();
					}
				}
				else
				{
					SkillManager.AttackInfoPanel.Hide();
					SkillManager.GenericActionInfoPanel.Hide();
				}
			}
			goto IL_0472;
			IL_0472:
			if (TPSingleton<PlayableUnitManager>.Instance.selectedSkill == null)
			{
				SkillManager.AttackInfoPanel.Hide();
				SkillManager.GenericActionInfoPanel.Hide();
				if (EnemyUnitManager.IsAnyEnemyTooltipDisplayed())
				{
					EnemyUnitManager.GetDisplayedEnemyTootlip().Refresh();
				}
				else if (PlayableUnitTooltip.Displayed)
				{
					PlayableUnitTooltip.Refresh();
				}
				if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.UnitPreparingSkill)
				{
					GameController.SetState(Game.E_State.Management);
				}
			}
			TPSingleton<PlayableUnitManagementView>.Instance.PlayableSkillBar.ChangeSelectedSkill();
		}
	}

	public static StatTooltip StatTooltip => TPSingleton<PlayableUnitManager>.Instance.statTooltip;

	public static TraitTooltip TraitTooltip => TPSingleton<PlayableUnitManager>.Instance.traitTooltip;

	public static Transform UnitHudsTransform => TPSingleton<PlayableUnitManager>.Instance.unitHudsTransform;

	public static bool HasUnitInFog => TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Any((PlayableUnit x) => x.OriginTile.HasFog);

	public bool HasToRecomputeReachableTiles { get; set; }

	public OneShotSound HitSFXPrefab => hitSFXPrefab;

	public OneShotSound HitSFXSpatializedPrefab => hitSFXSpatializedPrefab;

	public TaskGroup MoveUnitsTaskGroup { get; set; }

	public NightReport NightReport { get; } = new NightReportController().NightReport;


	public List<PlayableUnit> PlayableUnits { get; private set; }

	public Dictionary<int, List<PlayableUnit>> DeadPlayableUnits { get; private set; } = new Dictionary<int, List<PlayableUnit>>();


	public List<PlayableUnit> PlayableUnitsToRespawn { get; private set; }

	public SkillActionExecution PreviewSkillExecution { get; set; }

	public Recruitment Recruitment { get; private set; } = new Recruitment();


	public PlayableUnitGhostView SelectedPlayableUnitGhost { get; set; }

	public bool ShouldClearUndoStack { get; set; }

	public bool ShouldWaitUntilDeathSequences => PlayableUnits.Where((PlayableUnit o) => o.IsDead).Any((PlayableUnit o) => !o.PlayableUnitView.DeathSequenceOver);

	public bool ShouldTriggerPlayableUnitsDeathSequence => PlayableUnits.Where((PlayableUnit u) => u.IsDead).Any((PlayableUnit u) => !u.IsDying);

	public WaitUntil WaitUntilDeathSequences => new WaitUntil((Func<bool>)(() => !ShouldWaitUntilDeathSequences));

	public WaitUntil WaitUntilTakeDamageSequences
	{
		get
		{
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Expected O, but got Unknown
			IEnumerable<PlayableUnit> damagedPlayableUnits = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Where((PlayableUnit o) => o.UnitView.IsTakingDamage);
			return new WaitUntil((Func<bool>)(() => damagedPlayableUnits.Where((PlayableUnit o) => o.UnitView.IsTakingDamage).Count() == 0));
		}
	}

	public static bool DebugDisableHealthDisplay => TPSingleton<PlayableUnitManager>.Instance.debugDisableHealthDisplay;

	public static bool DebugForceSkipNightReport => TPSingleton<PlayableUnitManager>.Instance.debugForceSkipNightReport;

	public static bool DebugToggleDismissHeroValidityChecks => TPSingleton<PlayableUnitManager>.Instance.debugToggleDismissHeroValidityChecks;

	public static event Action<PlayableUnit, Tile> OnPlayableUnitMoved;

	public static event Action<PlayableUnit> OnPlayableUnitDied;

	public static bool CanUndoLastCommand()
	{
		if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Management && TPSingleton<PlayableUnitManager>.Instance.unitsConversation.UndoStack.Count > 0)
		{
			if (TPSingleton<PlayableUnitManager>.Instance.unitsConversation.UndoStack.Peek() is MoveUnitCommand moveUnitCommand)
			{
				return moveUnitCommand.PlayableUnit.CanStopOn(moveUnitCommand.StartTile);
			}
			return true;
		}
		return false;
	}

	public static void UndoLastCommand()
	{
		if (CanUndoLastCommand())
		{
			UnitCommand unitCommand = TPSingleton<PlayableUnitManager>.Instance.unitsConversation.Undo() as UnitCommand;
			TileObjectSelectionManager.SetSelectedPlayableUnit(unitCommand.PlayableUnit, CameraView.CameraUIMasksHandler.IsPointOffscreenOrHiddenByUI(unitCommand.PlayableUnit.OriginTile));
			GameView.BottomScreenPanel.BottomLeftPanel.CancelMovementPanel.Refresh();
		}
	}

	public static void CreateStartUnits()
	{
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		Vector2Int origin = default(Vector2Int);
		((Vector2Int)(ref origin))._002Ector(TPSingleton<TileMapManager>.Instance.TileMap.Width / 2, TPSingleton<TileMapManager>.Instance.TileMap.Height / 2);
		Tile tile = null;
		int num = PlayableUnitDatabase.UnitTraitGenerationDefinition.StartTraitTotalPointsWithModifiers;
		((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).Log((object)("Generating start units using Id " + TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.UnitGenerationDefinitionId), (CLogLevel)0, false, false);
		List<UnitGenerationDefinition> list = PlayableUnitDatabase.UnitsGenerationStartDefinitions[TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.UnitGenerationDefinitionId];
		Vector2Int unitTraitPointBoundariesWithModifiers;
		for (int i = 0; i < list.Count; i++)
		{
			tile = TileMapController.GetRandomUnoccupiedTile(origin, PlayableUnitDatabase.StartingUnitsSpawnAreaSize / 2);
			if (tile != null)
			{
				int randomRange = RandomManager.GetRandomRange(TPSingleton<PlayableUnitManager>.Instance, 0, list[i].PlayableUnitGenerationDefinitionArchetypeIds.Count);
				string archetypeId = list[i].PlayableUnitGenerationDefinitionArchetypeIds[randomRange];
				int num4;
				if (i < list.Count - 1)
				{
					PlayableUnitManager instance = TPSingleton<PlayableUnitManager>.Instance;
					unitTraitPointBoundariesWithModifiers = PlayableUnitDatabase.UnitTraitGenerationDefinition.UnitTraitPointBoundariesWithModifiers;
					int x = ((Vector2Int)(ref unitTraitPointBoundariesWithModifiers)).x;
					unitTraitPointBoundariesWithModifiers = PlayableUnitDatabase.UnitTraitGenerationDefinition.UnitTraitPointBoundariesWithModifiers;
					int num2 = ((Vector2Int)(ref unitTraitPointBoundariesWithModifiers)).y + 1;
					int num3 = num;
					unitTraitPointBoundariesWithModifiers = PlayableUnitDatabase.UnitTraitGenerationDefinition.UnitTraitPointBoundariesWithModifiers;
					num4 = RandomManager.GetRandomRange(instance, x, Mathf.Min(num2, num3 - ((Vector2Int)(ref unitTraitPointBoundariesWithModifiers)).x));
				}
				else
				{
					num4 = num;
				}
				InstantiateUnit(GenerateUnit(1, archetypeId, num4), tile, -1, onLoad: true);
				num -= num4;
			}
		}
		List<UnitGenerationDefinition> list2 = new List<UnitGenerationDefinition>();
		if (!GlyphManager.TryGetGlyphEffects(out List<GlyphBonusUnitsEffectDefinition> glyphEffects))
		{
			return;
		}
		for (int num5 = glyphEffects.Count - 1; num5 >= 0; num5--)
		{
			list2.AddRange(glyphEffects[num5].UnitGenerationDefinitions);
		}
		for (int num6 = list2.Count - 1; num6 >= 0; num6--)
		{
			tile = TileMapController.GetRandomUnoccupiedTile(origin, PlayableUnitDatabase.StartingUnitsSpawnAreaSize / 2);
			if (tile != null)
			{
				string archetypeId = list2[num6].PlayableUnitGenerationDefinitionArchetypeIds[RandomManager.GetRandomRange(TPSingleton<PlayableUnitManager>.Instance, 0, list2[num6].PlayableUnitGenerationDefinitionArchetypeIds.Count)];
				PlayableUnitManager instance2 = TPSingleton<PlayableUnitManager>.Instance;
				unitTraitPointBoundariesWithModifiers = PlayableUnitDatabase.UnitTraitGenerationDefinition.UnitTraitPointBoundariesWithModifiers;
				int x2 = ((Vector2Int)(ref unitTraitPointBoundariesWithModifiers)).x;
				unitTraitPointBoundariesWithModifiers = PlayableUnitDatabase.UnitTraitGenerationDefinition.UnitTraitPointBoundariesWithModifiers;
				int num4 = RandomManager.GetRandomRange(instance2, x2, ((Vector2Int)(ref unitTraitPointBoundariesWithModifiers)).y + 1);
				InstantiateUnit(GenerateUnit(1, archetypeId, num4), tile, -1, onLoad: true);
			}
		}
		((CLogger<GlyphManager>)TPSingleton<GlyphManager>.Instance).Log((object)$"Added {list2.Count} units.", (CLogLevel)1, false, false);
	}

	public static void DestroyUnit(PlayableUnit playableUnit)
	{
		if (playableUnit.OriginTile.Unit == playableUnit)
		{
			playableUnit.OriginTile.TileController.SetUnit(null);
		}
		GameView.TopScreenPanel.UnitPortraitsPanel.RemovePortrait(TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.IndexOf(playableUnit));
		TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Remove(playableUnit);
		if (TileObjectSelectionManager.SelectedUnit == playableUnit)
		{
			TileObjectSelectionManager.DeselectUnit();
		}
		else if (TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			TPSingleton<PlayableUnitManager>.Instance.RefreshUnitMovePath(TileObjectSelectionManager.SelectedPlayableUnit);
		}
		if (TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count == 0)
		{
			NightTurnsManager.ForceStopTurnExecution();
			TileObjectSelectionManager.DeselectAll();
			CursorView.ClearTiles();
			TPSingleton<SeerPreviewDisplay>.Instance.Displayed = false;
			TPSingleton<ToDoListView>.Instance.Hide();
			GameView.TopScreenPanel.Display(show: false);
			PanicManager.Panic.PanicView.DisplayOrHide();
			GameController.TriggerGameOver(Game.E_GameOverCause.HeroesDeath);
		}
	}

	public static void DestroyDeadUnit(PlayableUnit playableUnit)
	{
		playableUnit.OriginTile.TileController.SetUnit(null);
		((Component)playableUnit.PlayableUnitView).gameObject.SetActive(false);
		UnitHUD unitHUD = playableUnit.PlayableUnitView.UnitHUD;
		if (unitHUD != null)
		{
			((Component)unitHUD).gameObject.SetActive(false);
		}
	}

	public static void DismissPlayableUnit(PlayableUnit playableUnit = null)
	{
		if (playableUnit == null)
		{
			playableUnit = TileObjectSelectionManager.SelectedPlayableUnit;
		}
		if (playableUnit != null)
		{
			playableUnit.PlayableUnitController.PrepareForExile();
			playableUnit.PlayableUnitController.ExecuteExile();
			Object.Destroy((Object)(object)((Component)playableUnit.PlayableUnitView.UnitHUD).gameObject);
			Object.Destroy((Object)(object)((Component)playableUnit.PlayableUnitView).gameObject);
			CharacterSheetManager.CloseCharacterSheetPanel();
			TPSingleton<ToDoListView>.Instance.RefreshUnitLevelUpNotification();
		}
	}

	public static void EndTurn()
	{
		SelectedSkill?.SkillAction.SkillActionExecution?.SkillExecutionController.Reset();
		TPSingleton<PlayableUnitManager>.Instance.PreviewSkillExecution?.SkillExecutionController.Reset();
		int num = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Max((PlayableUnit o) => o.ActionPointsSpentThisTurn);
		if (num > 0)
		{
			TPSingleton<MetaConditionManager>.Instance.RefreshMaxDoubleValue(MetaConditionSpecificContext.E_ValueCategory.MaxActionPointsOnHeroSingleTurn, num);
		}
		int num2 = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Max((PlayableUnit o) => o.TilesCrossedThisTurn);
		if (num2 > 0)
		{
			TPSingleton<MetaConditionManager>.Instance.RefreshMaxDoubleValue(MetaConditionSpecificContext.E_ValueCategory.MaxTilesCrossedSingleTurn, num2);
		}
		for (int i = 0; i < TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count; i++)
		{
			TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i].UnitController.EndTurn();
		}
		switch (TPSingleton<GameManager>.Instance.Game.Cycle)
		{
		case Game.E_Cycle.Day:
			if (TPSingleton<GameManager>.Instance.Game.DayTurn == Game.E_DayTurn.Deployment)
			{
				TPSingleton<MetaConditionManager>.Instance.RefreshEquippedUsables();
			}
			break;
		case Game.E_Cycle.Night:
			if (TPSingleton<GameManager>.Instance.Game.NightTurn == Game.E_NightTurn.PlayableUnits)
			{
				TPSingleton<PlayableUnitManager>.Instance.ClearIconFeedback();
			}
			break;
		}
	}

	[DevConsoleCommand("GatherUnitsForVictorySequence")]
	public static void GatherUnitsForVictorySequence()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		Vector2Int[] array = (Vector2Int[])(object)new Vector2Int[2]
		{
			new Vector2Int(TPSingleton<TileMapManager>.Instance.TileMap.Width / 2 - 3, TPSingleton<TileMapManager>.Instance.TileMap.Height / 2 + 3),
			new Vector2Int(TPSingleton<TileMapManager>.Instance.TileMap.Width / 2 + 3, TPSingleton<TileMapManager>.Instance.TileMap.Height / 2 - 3)
		};
		int num = Random.Range(0, array.Length);
		for (int num2 = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count - 1; num2 >= 0; num2--)
		{
			Tile tile = TileMapController.GetRandomUnoccupiedOrBarricadeTile(array[num], PlayableUnitDatabase.VictoryUnitsGatherAreaSize / 2) ?? TileMapController.GetRandomUnoccupiedOrBarricadeTile(array[++num % array.Length], PlayableUnitDatabase.VictoryUnitsGatherAreaSize / 2);
			if (tile == null)
			{
				((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).Log((object)$"Could not find tiles to gather all playable units around the map center in a radius of {PlayableUnitDatabase.VictoryUnitsGatherAreaSize / 2}.", (CLogLevel)1, false, false);
				break;
			}
			if (tile.Building != null)
			{
				BuildingManager.DestroyBuilding(tile, updateView: true, addDeadBuilding: false, triggerEvent: false);
			}
			TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[num2].UnitController.SetTile(tile);
			TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[num2].UnitView.UpdatePosition();
			num = ++num % array.Length;
		}
	}

	public static PlayableUnit GenerateUnit(int unitLevel, string archetypeId, int traitPoints, int ghostUnitIndex = 0)
	{
		PlayableUnitController playableUnitController = new PlayableUnitController(archetypeId, traitPoints, null, null, unitLevel);
		playableUnitController.Unit.UnitStatsController.SnapBaseStatTo(UnitStatDefinition.E_Stat.ActionPoints, UnitStatDefinition.E_Stat.ActionPointsTotal);
		playableUnitController.Unit.UnitStatsController.SnapBaseStatTo(UnitStatDefinition.E_Stat.MovePoints, UnitStatDefinition.E_Stat.MovePointsTotal);
		SetPlayableUnitGhost(playableUnitController.PlayableUnit, ghostUnitIndex, snapshotOnly: true);
		return playableUnitController.PlayableUnit;
	}

	public static PlayableUnit GetFirstLivingUnit()
	{
		foreach (PlayableUnit playableUnit in TPSingleton<PlayableUnitManager>.Instance.PlayableUnits)
		{
			if (playableUnit.Health > 0f)
			{
				return playableUnit;
			}
		}
		return null;
	}

	public static void InstantiateUnit(PlayableUnit generatedUnit, Tile tile, int saveVersion = -1, bool onLoad = false)
	{
		PlayableUnitView playableUnitView = Object.Instantiate<PlayableUnitView>(TPSingleton<PlayableUnitManager>.Instance.playableUnitViewPrefab, TPSingleton<PlayableUnitManager>.Instance.unitsTransform);
		generatedUnit.UnitController.SetTile(tile);
		generatedUnit.UnitView = playableUnitView;
		generatedUnit.UnitController.LookAtDirection(GameDefinition.E_Direction.South);
		tile.TileController.SetUnit(generatedUnit);
		playableUnitView.Init(generatedUnit);
		TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Add(generatedUnit);
		if (tile.Building != null)
		{
			TPSingleton<TileMapView>.Instance.DisplayBuildingInstantly(tile.Building, tile);
		}
		FogController.SetLightFogTilesFromDictionnary(FogController.ToggleLightFogTiles((from tile in FogManager.GetLightFogRepelTiles(tile)
			where tile.HasLightFogOn
			select tile).ToList()), FogManager.LightFogFadeInEaseAndDuration, FogManager.LightFogFadeOutEaseAndDuration, FogManager.LightFogDisappearEaseAndDuration, instant: false, independently: true);
		if (!onLoad)
		{
			TPSingleton<MetaConditionManager>.Instance.RefreshMaxPlayableUnitStatReached(generatedUnit);
			TileObjectSelectionManager.SetSelectedPlayableUnit(generatedUnit);
		}
		if (TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count >= 6)
		{
			TPSingleton<AchievementManager>.Instance.UnlockAchievement(AchievementContainer.ACH_HAVE_6_HEROES);
		}
	}

	public static void InstantiateDeadUnit(PlayableUnit generatedUnit, Tile tile)
	{
		PlayableUnitView playableUnitView = Object.Instantiate<PlayableUnitView>(TPSingleton<PlayableUnitManager>.Instance.playableUnitViewPrefab, TPSingleton<PlayableUnitManager>.Instance.unitsTransform);
		generatedUnit.UnitController.SetTile(tile);
		generatedUnit.UnitView = playableUnitView;
		tile.TileController.SetUnit(generatedUnit);
		playableUnitView.InitDeadUnit(generatedUnit);
		DestroyDeadUnit(generatedUnit);
	}

	public static void OnCursorTileBecomeNull()
	{
		switch (TPSingleton<GameManager>.Instance.Game.State)
		{
		case Game.E_State.Management:
			TPSingleton<PlayableUnitManager>.Instance.movePath.MovePathController.Clear();
			break;
		case Game.E_State.PlaceUnit:
			TPSingleton<PlayableUnitManager>.Instance.SelectedPlayableUnitGhost.Display(displayed: false);
			break;
		}
	}

	public static void OnGameStateChange(Game.E_State state, Game.E_State previousState)
	{
		TPSingleton<PlayableUnitManager>.Instance.skillHotkeys.Clear();
		TPSingleton<PlayableUnitManager>.Instance.movePath.MovePathController.Clear();
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night && TPSingleton<GameManager>.Instance.Game.NightTurn != Game.E_NightTurn.PlayableUnits)
		{
			return;
		}
		if (previousState == Game.E_State.UnitPreparingSkill && state != Game.E_State.UnitExecutingSkill && state != Game.E_State.UnitPreparingSkill)
		{
			SelectedSkill = null;
		}
		switch (state)
		{
		case Game.E_State.Construction:
		case Game.E_State.PlaceUnit:
			TileObjectSelectionManager.SelectedUnitFeedback.Display(display: false);
			if (TPSingleton<GameManager>.Instance.Game.Cycle != Game.E_Cycle.Day)
			{
				PathfindingManager.Pathfinding.PathfindingController.ClearReachableTiles();
			}
			break;
		case Game.E_State.Management:
			if (TPSingleton<GameManager>.Instance.Game.Cycle != Game.E_Cycle.Day)
			{
				PathfindingManager.Pathfinding.PathfindingController.ClearReachableTiles();
			}
			if (!TileObjectSelectionManager.HasUnitSelected)
			{
				break;
			}
			TPSingleton<PlayableUnitManager>.Instance.SetSkillsHotkeys();
			if (previousState == Game.E_State.Construction && !TileObjectSelectionManager.ClickedOnBuilding)
			{
				if (TileObjectSelectionManager.SelectedBuilding != null)
				{
					ACameraView.MoveTo(((Component)TileObjectSelectionManager.SelectedBuilding.BuildingView).transform);
				}
				else
				{
					ACameraView.MoveTo(((Component)TileObjectSelectionManager.SelectedUnit.UnitView).transform);
				}
			}
			else
			{
				UnitManagementView<PlayableUnitManagementView>.Refresh();
			}
			if (TPSingleton<PlayableUnitManager>.Instance.HasToRecomputeReachableTiles && TileObjectSelectionManager.HasPlayableUnitSelected)
			{
				TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitController.ComputeReachableTiles();
			}
			if (TileObjectSelectionManager.HasUnitSelected)
			{
				TileObjectSelectionManager.SelectedUnitFeedback.Display(display: true);
			}
			break;
		case Game.E_State.CharacterSheet:
		case Game.E_State.Shopping:
		case Game.E_State.ProductionReport:
			UnitManagementView<PlayableUnitManagementView>.Refresh();
			break;
		case Game.E_State.UnitPreparingSkill:
			TPSingleton<PlayableUnitManager>.Instance.SetSkillsHotkeys();
			if (TPSingleton<GameManager>.Instance.Game.Cycle != Game.E_Cycle.Day)
			{
				PathfindingManager.Pathfinding.PathfindingController.ClearReachableTiles();
			}
			break;
		case Game.E_State.UnitExecutingSkill:
		case Game.E_State.Wait:
			TileObjectSelectionManager.SelectedUnitFeedback.Display(display: false);
			break;
		default:
			PlayableUnitTooltip.Hide();
			break;
		}
		PlayableUnitManagementView.OnGameStateChange(state);
		GameView.TopScreenPanel.TurnPanel.Refresh();
	}

	public static void OnTurnStart()
	{
		TPSingleton<PlayableUnitManager>.Instance.movePath.MovePathController.Clear();
	}

	public static void RegisterUnitToRespawn(PlayableUnit unit)
	{
		if (TPSingleton<PlayableUnitManager>.Instance.PlayableUnitsToRespawn == null)
		{
			TPSingleton<PlayableUnitManager>.Instance.PlayableUnitsToRespawn = new List<PlayableUnit>();
		}
		TPSingleton<PlayableUnitManager>.Instance.PlayableUnitsToRespawn.Add(unit);
	}

	public static void RespawnUnits()
	{
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		if (TPSingleton<PlayableUnitManager>.Instance.PlayableUnitsToRespawn == null || TPSingleton<PlayableUnitManager>.Instance.PlayableUnitsToRespawn.Count == 0)
		{
			return;
		}
		((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).Log((object)$"Respawning {TPSingleton<PlayableUnitManager>.Instance.PlayableUnitsToRespawn.Count} playable units.", (CLogLevel)1, false, false);
		Vector2Int origin = default(Vector2Int);
		((Vector2Int)(ref origin))._002Ector(TPSingleton<TileMapManager>.Instance.TileMap.Width / 2, TPSingleton<TileMapManager>.Instance.TileMap.Height / 2);
		for (int num = TPSingleton<PlayableUnitManager>.Instance.PlayableUnitsToRespawn.Count - 1; num >= 0; num--)
		{
			if (TPSingleton<PlayableUnitManager>.Instance.DeadPlayableUnits.TryGetValue(TPSingleton<GameManager>.Instance.DayNumber, out var value) && value.Contains(TPSingleton<PlayableUnitManager>.Instance.PlayableUnitsToRespawn[num]))
			{
				value.Remove(TPSingleton<PlayableUnitManager>.Instance.PlayableUnitsToRespawn[num]);
			}
			TPSingleton<PlayableUnitManager>.Instance.PlayableUnitsToRespawn[num].State = TheLastStand.Model.Unit.Unit.E_State.Ready;
			InstantiateUnit(TPSingleton<PlayableUnitManager>.Instance.PlayableUnitsToRespawn[num], TileMapController.GetRandomUnoccupiedTile(origin, PlayableUnitDatabase.StartingUnitsSpawnAreaSize / 2));
		}
		TPSingleton<PlayableUnitManager>.Instance.PlayableUnitsToRespawn.Clear();
	}

	public static void SelectNextUnit()
	{
		SelectNewUnit(next: true);
	}

	public static void SelectNewUnit(bool next)
	{
		int num = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.IndexOf(TileObjectSelectionManager.SelectedPlayableUnit);
		int index = ((num != -1) ? (num + (next ? 1 : (-1))).Mod(TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count) : (next ? (-1) : 0).Mod(TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count));
		SelectUnitAtIndex(index);
	}

	public static void SelectPreviousUnit()
	{
		SelectNewUnit(next: false);
	}

	public static void SelectUnitAtIndex(int index)
	{
		TPSingleton<PlayableUnitManager>.Instance.movePath?.MovePathController.Clear();
		SelectedSkill?.SkillAction.SkillActionExecution.SkillExecutionController.Reset();
		if (EnemyUnitManager.PreviewedSkill != null)
		{
			EnemyUnitManager.PreviewedSkill = null;
		}
		if (BuildingManager.PreviewedSkill != null)
		{
			BuildingManager.PreviewedSkill = null;
		}
		TPSingleton<PlayableUnitManager>.Instance.PreviewSkillExecution?.SkillExecutionController.Reset();
		if (TPSingleton<GameManager>.Instance.Game.Cycle != Game.E_Cycle.Day)
		{
			PathfindingManager.Pathfinding.PathfindingController.ClearReachableTiles();
		}
		PlayableUnit playableUnit = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[index];
		TileObjectSelectionManager.SetSelectedPlayableUnit(playableUnit, CameraView.CameraUIMasksHandler.IsPointOffscreenOrHiddenByUI(playableUnit.OriginTile));
		if (TPSingleton<PlayableUnitManager>.Instance.HasToRecomputeReachableTiles && TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitController.ComputeReachableTiles();
		}
		if (StatTooltip.Displayed)
		{
			StatTooltip.Refresh();
		}
	}

	public static void SetPlayableUnitGhost(PlayableUnit playableUnit, int unitIndex, bool snapshotOnly = false)
	{
		if (unitIndex >= TPSingleton<PlayableUnitManager>.Instance.playableUnitGhostView.Count)
		{
			PlayableUnitGhostView playableUnitGhostView = Object.Instantiate<PlayableUnitGhostView>(TPSingleton<PlayableUnitManager>.Instance.playableUnitGhostViewPrefab, TPSingleton<PlayableUnitManager>.Instance.playableUnitGhostParent);
			TPSingleton<PlayableUnitManager>.Instance.playableUnitGhostView.Add(playableUnitGhostView);
			playableUnitGhostView.Display(displayed: false);
		}
		playableUnit.UnitView = TPSingleton<PlayableUnitManager>.Instance.playableUnitGhostView[unitIndex];
		TPSingleton<PlayableUnitManager>.Instance.playableUnitGhostView[unitIndex].Unit = playableUnit;
		playableUnit.PlayableUnitView.InitVisuals(playSpawnAnim: false);
		if (!snapshotOnly)
		{
			TPSingleton<PlayableUnitManager>.Instance.playableUnitGhostView[unitIndex].Display(displayed: true);
		}
	}

	public void SetSkillsHotkeys()
	{
		if (TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			skillHotkeys.Clear();
			SetWeaponSkillsHotkeys();
			SetEquipmentSkillsHotkeys();
			SetContextualSkillsHotkeys();
		}
	}

	public static void StartTurn()
	{
		TPSingleton<PlayableUnitManager>.Instance.unitsConversation.Clear();
		if (TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count == 0)
		{
			return;
		}
		for (int i = 0; i < TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count; i++)
		{
			TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i].PlayableUnitController.StartTurn();
		}
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Day || TPSingleton<GameManager>.Instance.Game.NightTurn == Game.E_NightTurn.PlayableUnits)
		{
			TileObjectSelectionManager.EnsureUnitSelection();
			if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night)
			{
				ACameraView.MoveTo(((Component)TileObjectSelectionManager.SelectedUnit.UnitView).transform);
			}
			TPSingleton<PlayableUnitManager>.Instance.HasToRecomputeReachableTiles = TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night;
			if (EnemyUnitManager.DisableHuman)
			{
				GameController.EndTurn();
			}
		}
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night && TPSingleton<GameManager>.Instance.Game.NightTurn == Game.E_NightTurn.PlayableUnits)
		{
			TPSingleton<PlayableUnitManager>.Instance.DisplayIconAndTileFeedback();
		}
	}

	public void CastSelectedSkill()
	{
		GameController.SetState(Game.E_State.UnitExecutingSkill);
		ShouldClearUndoStack = false;
		TrophyManager.SetValueToTrophiesConditions<EnemiesKilledSingleAttackTrophyConditionController>(new object[2]
		{
			TileObjectSelectionManager.SelectedUnit.RandomId,
			0
		});
		SkillCommand command = new SkillCommand(TileObjectSelectionManager.SelectedPlayableUnit, SelectedSkill);
		UnitsConversation.Execute(command);
		SelectedSkill.SkillAction.SkillActionExecution.SkillExecutionController.ExecuteSkill();
		((MonoBehaviour)this).StartCoroutine(WaitForSkillExecution(TileObjectSelectionManager.SelectedUnit));
	}

	public void ChangeEquipment()
	{
		if (!TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			return;
		}
		TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitController.SwitchWeaponSet();
		TPSingleton<UIManager>.Instance.PlayAudioClip(UIManager.ChangeEquipmentAudioClip);
		TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitController.RefreshStats();
		TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitView?.RefreshBodyParts();
		TPSingleton<PlayableUnitManagementView>.Instance.PlayableSkillBar.SetEquippedSkills(TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitController.GetWeaponSkills());
		TPSingleton<PlayableUnitManagementView>.Instance.RefreshEquipmentBoxSelectedSet();
		if (PlayableUnitTooltip.Displayed && PlayableUnitTooltip.PlayableUnit == TileObjectSelectionManager.SelectedPlayableUnit)
		{
			PlayableUnitTooltip.RefreshEquipmentSlots();
		}
		if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.CharacterSheet || TPSingleton<GameManager>.Instance.Game.State == Game.E_State.GameOver)
		{
			TPSingleton<CharacterSheetPanel>.Instance.RefreshAvatar();
			TPSingleton<CharacterSheetPanel>.Instance.RefreshSkills(TileObjectSelectionManager.SelectedPlayableUnit);
		}
		if ((Object)(object)InventoryManager.InventoryView.FocusedInventorySlotView != (Object)null)
		{
			TheLastStand.Model.Item.Item item = InventoryManager.InventoryView.FocusedInventorySlotView.InventorySlot.Item;
			if (item != null && item.ItemDefinition.IsHandItem)
			{
				InventoryManager.InventoryView.FocusedInventorySlotView.Refresh();
			}
		}
		SetSkillsHotkeys();
	}

	public void DistributeDailyExperience()
	{
		float num = 0f;
		foreach (KillReportData item in NightReport.KillsThisNight)
		{
			num += item.TotalExperienceToShare;
		}
		for (int i = 0; i < TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count; i++)
		{
			TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i].PlayableUnitController.ReceiveDailyExperience(num / (float)TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count);
		}
		EffectManager.DisplayEffects();
		NightReport.KillsThisNight.Clear();
	}

	public void FocusCamOnSelectedPlayableUnit()
	{
		if (TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			FocusCamOnUnit(TileObjectSelectionManager.SelectedPlayableUnit);
		}
	}

	public void FocusCamOnUnit(TheLastStand.Model.Unit.Unit unit)
	{
		ACameraView.MoveTo(((Component)unit.UnitView).transform);
	}

	public int GetUnitIndexHotkeyPressed()
	{
		if (DebugManager.DebugMode)
		{
			return -1;
		}
		for (int i = 0; i < PlayableUnits.Count; i++)
		{
			int unitHotkey = GetUnitHotkey(i);
			if (unitHotkey != -1 && InputManager.GetButtonDown(unitHotkey))
			{
				return i;
			}
		}
		return -1;
	}

	public void InvokeDiedPlayableUnit(PlayableUnit playableUnit)
	{
		PlayableUnitManager.OnPlayableUnitDied?.Invoke(playableUnit);
	}

	public void InvokeMovedPlayableUnit(PlayableUnit playableUnit, Tile tile)
	{
		PlayableUnitManager.OnPlayableUnitMoved?.Invoke(playableUnit, tile);
	}

	public bool IsSelectedPlayableUnitInRange(Tile tile)
	{
		if (TileObjectSelectionManager.SelectedPlayableUnit != null && PathfindingManager.Pathfinding.ReachableTiles != null)
		{
			return PathfindingManager.Pathfinding.ReachableTiles.ContainsKey(tile);
		}
		return false;
	}

	public void MoveUnit(PlayableUnit playableUnit)
	{
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Expected O, but got Unknown
		playableUnit.Path = MovePath.Path;
		Task task = playableUnit.PlayableUnitController.PrepareForMovement((TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night) ? (playableUnit.Path.Count - 1) : 0);
		if (TPSingleton<GameManager>.Instance.Game.Cycle != Game.E_Cycle.Day)
		{
			PathfindingManager.Pathfinding.PathfindingController.ClearReachableTiles();
		}
		else if (playableUnit != null)
		{
			playableUnit.MovedThisDay = true;
			TPSingleton<ToDoListView>.Instance.RefreshPositionNotification();
		}
		MovePath.MovePathController.Clear();
		GameController.SetState(Game.E_State.Wait);
		playableUnit.PlayableUnitView.PlayWalkAnim(doWalk: true);
		PlayableUnitManager instance = TPSingleton<PlayableUnitManager>.Instance;
		object obj = _003C_003Ec._003C_003E9__131_0;
		if (obj == null)
		{
			UnityAction val = delegate
			{
				GameController.SetState(Game.E_State.Management);
			};
			_003C_003Ec._003C_003E9__131_0 = val;
			obj = (object)val;
		}
		instance.MoveUnitsTaskGroup = new TaskGroup((UnityAction)obj);
		TPSingleton<PlayableUnitManager>.Instance.MoveUnitsTaskGroup.AddTask(task);
		TPSingleton<PlayableUnitManager>.Instance.MoveUnitsTaskGroup.Run();
	}

	public void RefreshUnitMovePath(PlayableUnit unit)
	{
		Tile tile = TPSingleton<GameManager>.Instance.Game.Cursor.Tile;
		if (tile == null)
		{
			return;
		}
		if (PathfindingManager.Pathfinding.ReachableTiles.ContainsKey(tile) && unit.CanStopOn(tile))
		{
			int moveRange = (int)unit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.MovePoints).FinalClamped;
			if (unit.OriginTile.Building != null && unit.OriginTile.Building.IsWatchtower)
			{
				moveRange = 0;
			}
			else if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Day)
			{
				if (tile.Unit == null)
				{
					movePath.MovePathController.SetPath(new Tile[2] { unit.OriginTile, tile });
					movePath.MovePathController.UpdateState(tile);
					movePath.MovePathView.DisplayMovePath();
				}
				else
				{
					movePath.MovePathView.Clear();
				}
				return;
			}
			PathfindingData pathfindingData = default(PathfindingData);
			pathfindingData.Unit = unit;
			pathfindingData.TargetTiles = new Tile[1] { tile };
			pathfindingData.MoveRange = moveRange;
			pathfindingData.DistanceFromTargetMin = 0;
			pathfindingData.DistanceFromTargetMax = 0;
			pathfindingData.IgnoreCanStopOnConstraints = false;
			pathfindingData.PathfindingStyle = PathfindingDefinition.E_PathfindingStyle.Bresenham;
			PathfindingData pathfindingData2 = pathfindingData;
			if (Input.GetKey((KeyCode)109))
			{
				pathfindingData2.PathfindingStyle = PathfindingDefinition.E_PathfindingStyle.Manhattan;
			}
			else if (Input.GetKey((KeyCode)104))
			{
				pathfindingData2.PathfindingStyle = PathfindingDefinition.E_PathfindingStyle.Hypotenuse;
			}
			movePath.MovePathController.ComputePath(pathfindingData2);
			movePath.MovePathController.UpdateState(tile);
			movePath.MovePathView.DisplayMovePath();
		}
		else
		{
			movePath.MovePathController.Clear();
		}
	}

	public void UpdateSkillHotkeysInput()
	{
		if (TileObjectSelectionManager.SelectedBuilding != null || !(TileObjectSelectionManager.SelectedUnit is PlayableUnit selectedUnit))
		{
			return;
		}
		foreach (KeyValuePair<int, TheLastStand.Model.Skill.Skill> skillHotkey in skillHotkeys)
		{
			if (InputManager.GetButtonDown(skillHotkey.Key) && SelectSkill(skillHotkey.Value, selectedUnit))
			{
				break;
			}
		}
	}

	public int GetSkillHotkey(TheLastStand.Model.Skill.Skill skill)
	{
		foreach (KeyValuePair<int, TheLastStand.Model.Skill.Skill> skillHotkey in skillHotkeys)
		{
			if (skillHotkey.Value == skill)
			{
				return skillHotkey.Key;
			}
		}
		return -1;
	}

	public bool SelectSkill(TheLastStand.Model.Skill.Skill skill, PlayableUnit selectedUnit)
	{
		if (!selectedUnit.PreventedSkillsIds.Contains(skill.SkillDefinition.Id) && CanExecuteSkill(skill, selectedUnit) && (!skill.SkillDefinition.IsContextual || skill.SkillController.ComputeTargetsAndValidity(selectedUnit)))
		{
			SelectedSkill = skill;
			if (SelectedSkill != null)
			{
				Tile tile = TPSingleton<GameManager>.Instance.Game.Cursor.Tile;
				SkillManager.RefreshSelectedSkillValidityOnTile(tile);
				if ((Object)(object)GameView.TopScreenPanel.UnitPortraitsPanel.GetPortraitIsHovered() != (Object)null)
				{
					tile = GameView.TopScreenPanel.UnitPortraitsPanel.GetPortraitIsHovered().PlayableUnit.OriginTile;
				}
				SelectedSkill.SkillAction.SkillActionExecution.SkillExecutionView.DisplayAreaOfEffect(tile);
			}
			return true;
		}
		return false;
	}

	protected override void OnDestroy()
	{
		((CLogger<PlayableUnitManager>)this).OnDestroy();
		TileObjectSelectionManager.OnUnitSelectionChange -= OnNewUnitSelected;
	}

	private bool CanExecuteSkill(TheLastStand.Model.Skill.Skill skill, PlayableUnit selectedUnit)
	{
		PlayableUnitStatsController playableUnitStatsController = selectedUnit.PlayableUnitStatsController;
		return skill.SkillController.CanExecuteSkill(playableUnitStatsController.GetStat(UnitStatDefinition.E_Stat.ActionPoints).FinalClamped, playableUnitStatsController.GetStat(UnitStatDefinition.E_Stat.MovePoints).FinalClamped, playableUnitStatsController.GetStat(UnitStatDefinition.E_Stat.Mana).FinalClamped, playableUnitStatsController.GetStat(UnitStatDefinition.E_Stat.Health).FinalClamped, selectedUnit.IsStunned);
	}

	private void ClearIconFeedback()
	{
		for (int i = 0; i < PlayableUnits.Count; i++)
		{
			PlayableUnits[i].UnitView.UnitHUD.DisplayIconFeedback(show: false);
		}
	}

	private void DisplayIconAndTileFeedback()
	{
		List<Tile> list = new List<Tile>();
		for (int i = 0; i < PlayableUnits.Count; i++)
		{
			if (PlayableUnits[i].WillDieByPoison && !PlayableUnits[i].IsDead)
			{
				PlayableUnits[i].UnitView.UnitHUD.DisplayIconFeedback();
				list.AddRange(PlayableUnits[i].OccupiedTiles);
			}
		}
		TileMapView.SetTiles(TileMapView.UnitFeedbackTilemap, list, "View/Tiles/Feedbacks/PoisonDeath");
	}

	private int GetUnitHotkey(int index)
	{
		return index switch
		{
			0 => 68, 
			1 => 69, 
			2 => 70, 
			3 => 71, 
			4 => 72, 
			5 => 73, 
			6 => 74, 
			_ => -1, 
		};
	}

	private void SelectNextSkill(bool nextSkill)
	{
		if (TileObjectSelectionManager.SelectedUnit is PlayableUnit)
		{
			TPSingleton<PlayableUnitManagementView>.Instance.SkillBar.SelectNextSkill(nextSkill);
		}
	}

	private void OnNewUnitSelected()
	{
		if (!TPSingleton<HUDJoystickNavigationManager>.Instance.HUDNavigationOn && InputManager.IsLastControllerJoystick && TileObjectSelectionManager.SelectedUnit is PlayableUnit)
		{
			TPSingleton<PlayableUnitManagementView>.Instance.SkillBar.JoystickSkillBar.ResetSkillIndex(1);
		}
	}

	private void SetWeaponSkillsHotkeys()
	{
		List<TheLastStand.Model.Skill.Skill> weaponSkills = TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitController.GetWeaponSkills();
		int num = 0;
		foreach (TheLastStand.Model.Skill.Skill item in weaponSkills)
		{
			if (item.IsPunch)
			{
				skillHotkeys.Add(120, item);
				continue;
			}
			int num2 = num switch
			{
				0 => 116, 
				1 => 117, 
				2 => 118, 
				3 => 119, 
				_ => -1, 
			};
			if (num2 == -1)
			{
				break;
			}
			skillHotkeys.Add(num2, item);
			num++;
		}
	}

	private void SetEquipmentSkillsHotkeys()
	{
		List<TheLastStand.Model.Skill.Skill> equipmentSkills = TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitController.GetEquipmentSkills();
		int num = 0;
		bool flag = false;
		foreach (TheLastStand.Model.Skill.Skill item2 in equipmentSkills)
		{
			if (item2.SkillContainer is TheLastStand.Model.Item.Item item && ItemDefinition.E_Category.BodyArmor.HasFlag(item.ItemDefinition.Category))
			{
				if (flag)
				{
					((CLogger<PlayableUnitManager>)this).LogWarning((object)$"Trying to set the hotkey of a {ItemDefinition.E_Category.BodyArmor} skill but one has already been set.", (CLogLevel)1, true, false);
					continue;
				}
				skillHotkeys.Add(121, item2);
				flag = true;
				continue;
			}
			int num2 = num switch
			{
				0 => 122, 
				1 => 123, 
				2 => 124, 
				3 => 125, 
				4 => 126, 
				5 => 127, 
				_ => -1, 
			};
			if (num2 == -1)
			{
				break;
			}
			skillHotkeys.Add(num2, item2);
			num++;
		}
	}

	private void SetContextualSkillsHotkeys()
	{
		List<TheLastStand.Model.Skill.Skill> contextualSkills = TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitController.GetContextualSkills();
		for (int i = 0; i < contextualSkills.Count; i++)
		{
			TheLastStand.Model.Skill.Skill value = contextualSkills[i];
			int num = i switch
			{
				0 => 128, 
				1 => 129, 
				2 => 130, 
				3 => 131, 
				4 => 132, 
				5 => 133, 
				6 => 134, 
				7 => 135, 
				_ => -1, 
			};
			if (num != -1)
			{
				skillHotkeys.Add(num, value);
				continue;
			}
			break;
		}
	}

	private void Start()
	{
		Recruitment?.ListenToBuildingDestroyEvent();
		TileObjectSelectionManager.OnUnitSelectionChange += OnNewUnitSelected;
	}

	private void Update()
	{
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0909: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0764: Unknown result type (might be due to invalid IL or missing references)
		if (!(ApplicationManager.Application.State is GameState) || (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night && TPSingleton<GameManager>.Instance.Game.NightTurn != Game.E_NightTurn.PlayableUnits))
		{
			return;
		}
		foreach (PlayableUnit playableUnit2 in PlayableUnits)
		{
			if (playableUnit2.IsDead && !playableUnit2.PlayableUnitView.DeathSequenceOver)
			{
				return;
			}
		}
		Tile tile = TPSingleton<GameManager>.Instance.Game.Cursor.Tile;
		Tile previousTile = TPSingleton<GameManager>.Instance.Game.Cursor.PreviousTile;
		TheLastStand.Model.Unit.Unit unit;
		switch (TPSingleton<GameManager>.Instance.Game.State)
		{
		case Game.E_State.UnitPreparingSkill:
			if (TPSingleton<GameManager>.Instance.Game.Cursor.TileHasChanged)
			{
				SkillManager.SkillInfoPanel.RefreshOnTileChanged();
				unit = tile?.Unit;
				if (SelectedSkill.SkillDefinition.SkillActionDefinition is AttackSkillActionDefinition)
				{
					if (unit == TileObjectSelectionManager.SelectedUnit)
					{
						int count = SelectedSkill.SkillDefinition.AreaOfEffectDefinition.Pattern.Count;
						Vector2Int origin = TPSingleton<PlayableUnitManager>.Instance.selectedSkill.SkillDefinition.AreaOfEffectDefinition.Origin;
						if (count > ((Vector2Int)(ref origin)).x)
						{
							List<List<char>> pattern = SelectedSkill.SkillDefinition.AreaOfEffectDefinition.Pattern;
							origin = TPSingleton<PlayableUnitManager>.Instance.selectedSkill.SkillDefinition.AreaOfEffectDefinition.Origin;
							int count2 = pattern[((Vector2Int)(ref origin)).x].Count;
							origin = TPSingleton<PlayableUnitManager>.Instance.selectedSkill.SkillDefinition.AreaOfEffectDefinition.Origin;
							if (count2 > ((Vector2Int)(ref origin)).y)
							{
								List<List<char>> pattern2 = SelectedSkill.SkillDefinition.AreaOfEffectDefinition.Pattern;
								origin = TPSingleton<PlayableUnitManager>.Instance.selectedSkill.SkillDefinition.AreaOfEffectDefinition.Origin;
								List<char> list = pattern2[((Vector2Int)(ref origin)).x];
								origin = TPSingleton<PlayableUnitManager>.Instance.selectedSkill.SkillDefinition.AreaOfEffectDefinition.Origin;
								if (list[((Vector2Int)(ref origin)).y] != 'X')
								{
									SkillManager.AttackInfoPanel.TargetTile = tile;
									SkillManager.AttackInfoPanel.TargetUnit = null;
									if (SkillManager.AttackInfoPanel.Displayed)
									{
										SkillManager.AttackInfoPanel.Hide();
									}
									goto IL_0301;
								}
							}
						}
					}
					if (!SkillManager.AttackInfoPanel.Displayed)
					{
						SkillManager.AttackInfoPanel.Display();
					}
					SkillManager.AttackInfoPanel.TargetTile = tile;
					SkillManager.AttackInfoPanel.TargetUnit = unit;
					SkillManager.AttackInfoPanel.Refresh();
				}
				else if (SelectedSkill.SkillDefinition.SkillActionDefinition is GenericSkillActionDefinition)
				{
					SkillManager.GenericActionInfoPanel.TargetTile = tile;
					SkillManager.GenericActionInfoPanel.TargetUnit = null;
					SkillManager.GenericActionInfoPanel.Refresh();
					if (!SkillManager.GenericActionInfoPanel.Displayed)
					{
						SkillManager.GenericActionInfoPanel.Display();
					}
				}
				goto IL_0301;
			}
			goto IL_040c;
		case Game.E_State.Management:
			if (InputManager.GetButtonDown(83))
			{
				SelectNextSkill(nextSkill: false);
			}
			else if (InputManager.GetButtonDown(82))
			{
				SelectNextSkill(nextSkill: true);
			}
			else if (InputManager.GetButtonDown(23) || InputManager.GetButtonDown(137))
			{
				TPSingleton<PlayableUnitManagementView>.Instance.SkillBar.JoystickSkillBar.DeselectCurrentSkill();
			}
			else if (InputManager.GetButtonDown(136))
			{
				FocusCamOnSelectedPlayableUnit();
			}
			break;
		case Game.E_State.CharacterSheet:
			if (InputManager.GetButtonDown(60) && !InventoryManager.InventoryView.DraggableItem.Displayed)
			{
				ChangeEquipment();
			}
			break;
		case Game.E_State.NightReport:
		case Game.E_State.ProductionReport:
			{
				if (InputManager.GetButtonDown(0))
				{
					SelectNextUnit();
					int newUnitIndex = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.IndexOf(TileObjectSelectionManager.SelectedPlayableUnit);
					TPSingleton<ChooseRewardPanel>.Instance.ChangeUnitToCompareAndResetDropdown(newUnitIndex);
					break;
				}
				if (InputManager.GetButtonDown(11))
				{
					SelectPreviousUnit();
					int newUnitIndex2 = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.IndexOf(TileObjectSelectionManager.SelectedPlayableUnit);
					TPSingleton<ChooseRewardPanel>.Instance.ChangeUnitToCompareAndResetDropdown(newUnitIndex2);
					break;
				}
				if (InputManager.GetButtonDown(60))
				{
					ChangeEquipment();
					TPSingleton<PlayableUnitManagementView>.Instance.PlayableSkillBar.CheckSkillButtonsFocus();
					int newUnitIndex3 = TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.IndexOf(TileObjectSelectionManager.SelectedPlayableUnit);
					TPSingleton<ChooseRewardPanel>.Instance.ChangeUnitToCompareAndResetDropdown(newUnitIndex3);
					break;
				}
				int unitIndexHotkeyPressed = TPSingleton<PlayableUnitManager>.Instance.GetUnitIndexHotkeyPressed();
				if (unitIndexHotkeyPressed != -1)
				{
					TileObjectSelectionManager.SetSelectedPlayableUnit(TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[unitIndexHotkeyPressed], CameraView.CameraUIMasksHandler.IsPointOffscreenOrHiddenByUI(((Component)TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[unitIndexHotkeyPressed].UnitView).transform.position));
					TPSingleton<ChooseRewardPanel>.Instance.ChangeUnitToCompareAndResetDropdown(unitIndexHotkeyPressed);
				}
				break;
			}
			IL_0301:
			if (SelectedSkill?.SkillDefinition.ValidTargets != null)
			{
				if (SelectedSkill.SkillDefinition.ValidTargets.AnyUnits)
				{
					if (tile != null && unit != null)
					{
						TheLastStand.Model.Unit.Unit unit2 = unit;
						unit2.UnitView.OnSkillTargetHover(hover: true);
					}
					if (previousTile != null)
					{
						TheLastStand.Model.Unit.Unit unit3 = previousTile.Unit;
						if (unit3 != null && unit3 != unit)
						{
							unit3.UnitView.OnSkillTargetHover(hover: false);
						}
					}
				}
				if (tile != null && tile.Building != null && SelectedSkill.SkillDefinition.ValidTargets.Buildings.ContainsKey(tile.Building.Id))
				{
					tile.Building.BuildingView.OnSkillTargetHover(hover: true);
				}
				if (previousTile != null && previousTile.Building != null)
				{
					previousTile.Building.BuildingView.OnSkillTargetHover(hover: false);
				}
				if (tile != null && SelectedSkill.Targets.Contains(tile))
				{
					tile.TileView.OnSkillTargetHover(hover: true);
				}
				if (previousTile != null && SelectedSkill.Targets.Contains(previousTile))
				{
					previousTile.TileView.OnSkillTargetHover(hover: false);
				}
			}
			goto IL_040c;
			IL_040c:
			if (GameView.TopScreenPanel.UnitPortraitsPanel.TargettedPortraitHasChanged)
			{
				UnitPortraitView portraitIsHovered = GameView.TopScreenPanel.UnitPortraitsPanel.GetPortraitIsHovered();
				if (GameView.TopScreenPanel.UnitPortraitsPanel.CursorIsHoverPortrait)
				{
					portraitIsHovered.PlayableUnit.PlayableUnitView.OnSkillTargetHover(hover: true);
				}
				if ((Object)(object)GameView.TopScreenPanel.UnitPortraitsPanel.GetPreviousPortraitWasHovered() != (Object)null)
				{
					GameView.TopScreenPanel.UnitPortraitsPanel.GetPreviousPortraitWasHovered().PlayableUnit.PlayableUnitView.OnSkillTargetHover(hover: false);
				}
			}
			if (InputManager.GetButtonDown(22) && GameView.TopScreenPanel.UnitPortraitsPanel.CursorIsHoverPortrait)
			{
				PlayableUnit playableUnit = GameView.TopScreenPanel.UnitPortraitsPanel.GetPortraitIsHovered().PlayableUnit;
				SkillManager.RefreshSelectedSkillValidityOnTile(playableUnit.OriginTile);
				if (SkillManager.IsSelectedSkillValid)
				{
					SelectedSkill.SkillAction.SkillActionExecution.SkillExecutionController.AddTarget(playableUnit.OriginTile, SelectedSkill.CursorDependantOrientation);
					CastSelectedSkill();
					SoundManager.PlayAudioClip(SkillManager.AudioSource, SkillManager.SkillValidTileAudioClip);
				}
				else
				{
					SoundManager.PlayAudioClip(SkillManager.AudioSource, SkillManager.SkillInvalidTileAudioClip);
				}
			}
			else if (InputManager.GetButtonDown(0))
			{
				SelectedSkill = null;
				SelectNextUnit();
			}
			else if (InputManager.GetButtonDown(11))
			{
				SelectedSkill = null;
				SelectPreviousUnit();
			}
			else if (InputManager.GetButtonDown(23) || InputManager.GetButtonDown(137))
			{
				TPSingleton<TileObjectSelectionManager>.Instance.HasToWaitForNextFrame = true;
				if (SelectedSkill.SkillAction.HasEffect("MultiHit") && SelectedSkill.SkillAction.SkillActionExecution.TargetTiles.Count > 0)
				{
					SelectedSkill.SkillAction.SkillActionExecution.SkillExecutionController.RemoveLastTarget();
				}
				else
				{
					SelectedSkill.SkillAction.SkillActionExecution.SkillExecutionController.Reset();
					if (SelectedSkill.SkillAction is ResupplySkillAction resupplySkillAction && (resupplySkillAction.TryGetFirstEffect<ResupplyChargesSkillEffectDefinition>("ResupplyCharges", out var _) || resupplySkillAction.TryGetFirstEffect<ResupplyOverallUsesSkillEffectDefinition>("ResupplyOverallUses", out var _)))
					{
						resupplySkillAction.ResupplySkillActionExecution.ResupplySkillActionExecutionView.HideDisplayedHUD();
					}
					SelectedSkill = null;
					if (tile != null && !tile.HasFog && tile.Unit is EnemyUnit enemyUnit && enemyUnit.State != TheLastStand.Model.Unit.Unit.E_State.Dead)
					{
						TPSingleton<EnemyUnitManager>.Instance.DisplayOneEnemyReachableTiles(enemyUnit);
					}
					else if (TileObjectSelectionManager.HasPlayableUnitSelected && TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitController.Unit.UnitController.MoveTask == null)
					{
						TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitController.ComputeReachableTiles();
						TPSingleton<TileObjectSelectionManager>.Instance.HasToWaitForNextFrame = true;
					}
				}
				TPSingleton<PlayableUnitManagementView>.Instance.SkillBar.JoystickSkillBar.DeselectCurrentSkill();
				SoundManager.PlayAudioClip(SkillManager.AudioSource, SkillManager.SkillCancelAudioClip);
			}
			else if (InputManager.GetButtonDown(83))
			{
				SelectNextSkill(nextSkill: false);
			}
			else if (InputManager.GetButtonDown(82))
			{
				SelectNextSkill(nextSkill: true);
			}
			else if (InputManager.GetButtonDown(136))
			{
				FocusCamOnSelectedPlayableUnit();
			}
			else
			{
				int unitIndexHotkeyPressed2 = TPSingleton<PlayableUnitManager>.Instance.GetUnitIndexHotkeyPressed();
				if (unitIndexHotkeyPressed2 != -1)
				{
					SelectedSkill = null;
					TileObjectSelectionManager.SetSelectedPlayableUnit(TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[unitIndexHotkeyPressed2], CameraView.CameraUIMasksHandler.IsPointOffscreenOrHiddenByUI(((Component)TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[unitIndexHotkeyPressed2].UnitView).transform.position));
				}
			}
			UpdateSkillHotkeysInput();
			break;
		}
		if (!DebugManager.DebugMode)
		{
			return;
		}
		if (Input.GetKeyDown((KeyCode)286))
		{
			DebugReplenishEverything();
		}
		else if (Input.GetKeyDown((KeyCode)287))
		{
			ResourceManager.Debug_GainResources();
		}
		else if (Input.GetKeyDown((KeyCode)288) && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.NightReport && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.CutscenePlaying && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.ProductionReport && !TPSingleton<NightTurnsManager>.Instance.IsEndingNight)
		{
			if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night)
			{
				GameManager.ExileAllEnemies(countAsKills: true, resetSpawnWave: true);
				((MonoBehaviour)TPSingleton<NightTurnsManager>.Instance).StartCoroutine(TPSingleton<NightTurnsManager>.Instance.EndNightCoroutine());
			}
			else
			{
				GameController.EndTurn();
			}
		}
	}

	private IEnumerator WaitForSkillExecution(TheLastStand.Model.Unit.Unit unit)
	{
		while (unit.IsExecutingSkill)
		{
			yield return SharedYields.WaitForEndOfFrame;
		}
		SelectedSkill?.SkillAction.SkillActionExecution.SkillExecutionController.Reset();
		SelectedSkill = null;
		if (ShouldClearUndoStack)
		{
			unitsConversation.Clear();
		}
		GameView.BottomScreenPanel.BottomLeftPanel.CancelMovementPanel.Refresh();
		if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.UnitExecutingSkill && !TPSingleton<NightTurnsManager>.Instance.IsEndingNight)
		{
			GameController.SetState(Game.E_State.Management);
			if (InputManager.IsLastControllerJoystick)
			{
				movePath.MovePathController.Clear();
				TPSingleton<PlayableUnitManagementView>.Instance.SkillBar.JoystickSkillBar.SelectCurrentOrPreviousSkill();
				SkillManager.RefreshSelectedSkillValidityOnTile(TPSingleton<GameManager>.Instance.Game.Cursor.Tile);
			}
		}
	}

	[DevConsoleCommand("AddTrait")]
	public static void DebugAddTrait([StringConverter(typeof(PlayableUnit.StringToTraitIdConverter))] string traitId, bool forceAdd = false)
	{
		if (!TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).LogError((object)"Please select a unit before firing this command", (Object)(object)TPSingleton<PlayableUnitManager>.Instance, (CLogLevel)1, true, true);
			return;
		}
		TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitController.AddTrait(traitId, forceAdd);
		TPSingleton<CharacterSheetPanel>.Instance.Refresh();
		UnitManagementView<PlayableUnitManagementView>.Refresh();
	}

	[DevConsoleCommand("AddStatus")]
	public static void DebugAddStatus([StringConverter(typeof(Status.StringToStatusForDefaultCommand))] string statusId, int turnsCount, int value = 1)
	{
		if (!TileObjectSelectionManager.HasUnitSelected)
		{
			((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).LogError((object)"Please select an unit before firing this command", (Object)(object)TPSingleton<PlayableUnitManager>.Instance, (CLogLevel)1, true, true);
			return;
		}
		if (!Enum.TryParse<Status.E_StatusType>(statusId, out var result))
		{
			((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).LogError((object)"Incorrect Status (unable to parse)", (Object)(object)TPSingleton<PlayableUnitManager>.Instance, (CLogLevel)1, true, true);
			return;
		}
		StatusCreationInfo statusCreationInfo = default(StatusCreationInfo);
		statusCreationInfo.Source = null;
		statusCreationInfo.TurnsCount = turnsCount;
		statusCreationInfo.Value = value;
		StatusCreationInfo statusCreationInfo2 = statusCreationInfo;
		SkillManager.AddStatus(TileObjectSelectionManager.SelectedUnit, result, statusCreationInfo2);
	}

	[DevConsoleCommand("AddStatusWithStat")]
	public static void DebugAddStatusWithStat([StringConverter(typeof(Status.StringToStatusWithStatCommand))] string statusId, [StringConverter(typeof(PlayableUnit.StringToStatIdConverter))] string statId, int turnsCount, int value = 1)
	{
		if (!TileObjectSelectionManager.HasUnitSelected)
		{
			((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).LogError((object)"Please select an unit before firing this command", (Object)(object)TPSingleton<PlayableUnitManager>.Instance, (CLogLevel)1, true, true);
			return;
		}
		if (!Enum.TryParse<Status.E_StatusType>(statusId, out var result))
		{
			((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).LogError((object)"Incorrect Status (unable to parse)", (Object)(object)TPSingleton<PlayableUnitManager>.Instance, (CLogLevel)1, true, true);
			return;
		}
		if (!Enum.TryParse<UnitStatDefinition.E_Stat>(statId, out var result2))
		{
			((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).LogError((object)"Incorrect E_Stat (unable to parse)", (Object)(object)TPSingleton<PlayableUnitManager>.Instance, (CLogLevel)1, true, true);
			return;
		}
		StatusCreationInfo statusCreationInfo = default(StatusCreationInfo);
		statusCreationInfo.Source = null;
		statusCreationInfo.Stat = result2;
		statusCreationInfo.TurnsCount = turnsCount;
		statusCreationInfo.Value = value;
		StatusCreationInfo statusCreationInfo2 = statusCreationInfo;
		SkillManager.AddStatus(TileObjectSelectionManager.SelectedUnit, result, statusCreationInfo2);
	}

	[DevConsoleCommand("AddAllImmunity")]
	public static void DebugAddAllImmunity()
	{
		DebugAddStatus("PoisonImmunity", 2, 25);
		DebugAddStatus("StunImmunity", 5, 50);
		DebugAddStatus("DebuffImmunity", 5, 50);
		DebugAddStatus("AllNegativeImmunity", 5, 50);
	}

	[DevConsoleCommand("DisableHealthDisplay")]
	public static void Debug_DisableHealthDisplay()
	{
		TPSingleton<PlayableUnitManager>.Instance.debugDisableHealthDisplay = true;
		RefreshHealthDisplays();
	}

	[DevConsoleCommand("EnableHealthDisplay")]
	public static void Debug_EnableHealthDisplay()
	{
		TPSingleton<PlayableUnitManager>.Instance.debugDisableHealthDisplay = false;
		RefreshHealthDisplays();
	}

	[DevConsoleCommand("GainActionPoints")]
	public static void DebugGainActionPoints(int amount = 100)
	{
		if (!TileObjectSelectionManager.HasUnitSelected)
		{
			((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).LogError((object)"Please select a unit before firing this command", (Object)(object)TPSingleton<PlayableUnitManager>.Instance, (CLogLevel)1, true, true);
			return;
		}
		TileObjectSelectionManager.SelectedUnit.UnitStatsController.IncreaseBaseStat(UnitStatDefinition.E_Stat.ActionPoints, amount, includeChildStat: false);
		TPSingleton<CharacterSheetPanel>.Instance.Refresh();
		UnitManagementView<PlayableUnitManagementView>.Refresh();
	}

	[DevConsoleCommand("GainArmor")]
	public static void DebugGainArmor(int amount = 1000)
	{
		if (!TileObjectSelectionManager.HasUnitSelected)
		{
			((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).LogError((object)"Please select a unit before firing this command", (Object)(object)TPSingleton<PlayableUnitManager>.Instance, (CLogLevel)1, true, true);
			return;
		}
		TileObjectSelectionManager.SelectedUnit.UnitStatsController.IncreaseBaseStat(UnitStatDefinition.E_Stat.Armor, amount, includeChildStat: true);
		if (TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			TPSingleton<CharacterSheetPanel>.Instance.Refresh();
		}
		UnitManagementView<PlayableUnitManagementView>.Refresh();
		UnitManagementView<EnemyUnitManagementView>.Refresh();
	}

	[DevConsoleCommand("GainExperience")]
	public static void DebugGainExperience(int amount = 1000000)
	{
		if (!TileObjectSelectionManager.HasUnitSelected)
		{
			((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).LogError((object)"Please select a unit before firing this command", (Object)(object)TPSingleton<PlayableUnitManager>.Instance, (CLogLevel)1, true, true);
			return;
		}
		TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitController.GainExperience(amount);
		TPSingleton<CharacterSheetPanel>.Instance.Refresh();
		UnitManagementView<PlayableUnitManagementView>.Refresh();
		TPSingleton<ToDoListView>.Instance.RefreshUnitLevelUpNotification();
	}

	[DevConsoleCommand("GainLevel")]
	public static void DebugGainLevel()
	{
		if (!TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).LogError((object)"Please select a unit before firing this command", (Object)(object)TPSingleton<PlayableUnitManager>.Instance, (CLogLevel)1, true, true);
			return;
		}
		TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitController.LevelUp();
		TileObjectSelectionManager.SelectedPlayableUnit.ExperienceInCurrentLevel = 0f;
		TPSingleton<CharacterSheetPanel>.Instance.Refresh();
		UnitManagementView<PlayableUnitManagementView>.Refresh();
		TPSingleton<ToDoListView>.Instance.RefreshUnitLevelUpNotification();
	}

	[DevConsoleCommand("GainHealth")]
	public static void DebugGainHealth(int amount = 1000)
	{
		if (!TileObjectSelectionManager.HasUnitSelected)
		{
			((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).LogError((object)"Please select a unit before firing this command", (Object)(object)TPSingleton<PlayableUnitManager>.Instance, (CLogLevel)1, true, true);
			return;
		}
		TileObjectSelectionManager.SelectedUnit.UnitController.GainHealth(amount);
		if (TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			TPSingleton<CharacterSheetPanel>.Instance.Refresh();
		}
		UnitManagementView<PlayableUnitManagementView>.Refresh();
		UnitManagementView<EnemyUnitManagementView>.Refresh();
	}

	[DevConsoleCommand("GainMana")]
	public static void DebugGainMana(int amount = 100)
	{
		if (!TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).LogError((object)"Please select a unit before firing this command", (Object)(object)TPSingleton<PlayableUnitManager>.Instance, (CLogLevel)1, true, true);
			return;
		}
		TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitController.GainMana(amount);
		TPSingleton<CharacterSheetPanel>.Instance.Refresh();
		UnitManagementView<PlayableUnitManagementView>.Refresh();
	}

	[DevConsoleCommand("GainMovePoints")]
	public static void DebugGainMovePoints(int amount = 100)
	{
		if (!TileObjectSelectionManager.HasUnitSelected)
		{
			((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).LogError((object)"Please select a unit before firing this command", (Object)(object)TPSingleton<PlayableUnitManager>.Instance, (CLogLevel)1, true, true);
			return;
		}
		TileObjectSelectionManager.SelectedUnit.UnitStatsController.IncreaseBaseStat(UnitStatDefinition.E_Stat.MovePoints, amount, includeChildStat: false);
		if (TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			TPSingleton<CharacterSheetPanel>.Instance.Refresh();
			if (TPSingleton<PlayableUnitManager>.Instance.HasToRecomputeReachableTiles)
			{
				TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitController.ComputeReachableTiles();
			}
		}
		UnitManagementView<PlayableUnitManagementView>.Refresh();
		UnitManagementView<EnemyUnitManagementView>.Refresh();
	}

	[DevConsoleCommand("GetStat")]
	public static void DebugGetStat([StringConverter(typeof(PlayableUnit.StringToStatIdConverter))] string statId, bool isFinalValue = false)
	{
		if (!TileObjectSelectionManager.HasUnitSelected)
		{
			((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).LogError((object)"Please select a unit before firing this command", (Object)(object)TPSingleton<PlayableUnitManager>.Instance, (CLogLevel)1, true, true);
			return;
		}
		UnitStatDefinition.E_Stat stat = (UnitStatDefinition.E_Stat)Enum.Parse(typeof(UnitStatDefinition.E_Stat), statId);
		float num = (isFinalValue ? TileObjectSelectionManager.SelectedUnit.UnitStatsController.GetStat(stat).FinalClamped : TileObjectSelectionManager.SelectedUnit.UnitStatsController.GetStat(stat).Base);
		TPSingleton<DebugManager>.Instance.LogDevConsole((object)num);
	}

	[DevConsoleCommand("GainPerkPoints")]
	public static void GainPerkPoints(int perksPoints = 1)
	{
		if (!TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).LogError((object)"Please select a unit before firing this command", (Object)(object)TPSingleton<PlayableUnitManager>.Instance, (CLogLevel)1, true, true);
		}
		else
		{
			TileObjectSelectionManager.SelectedPlayableUnit.PerksPoints += perksPoints;
		}
	}

	[DevConsoleCommand("ReplenishEquippedSkills")]
	public static void DebugReplenishEquippedSkills()
	{
		for (int i = 0; i < TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count; i++)
		{
			TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i].PlayableUnitController.StartEquipmentTurn();
			TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i].PlayableUnitController.ResetContextualSkillsTurnUses();
			foreach (KeyValuePair<ItemSlotDefinition.E_ItemSlotId, List<EquipmentSlot>> equipmentSlot in TPSingleton<PlayableUnitManager>.Instance.PlayableUnits[i].EquipmentSlots)
			{
				for (int num = equipmentSlot.Value.Count - 1; num >= 0; num--)
				{
					equipmentSlot.Value[num].Item?.ItemController.RefillOverallUses();
				}
			}
		}
		UnitManagementView<PlayableUnitManagementView>.Refresh();
	}

	[DevConsoleCommand("ReplenishEverything")]
	public static void DebugReplenishEverything()
	{
		if (TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Management && TileObjectSelectionManager.SelectedBuilding == null && TileObjectSelectionManager.SelectedUnit != null)
		{
			if (TileObjectSelectionManager.SelectedPlayableUnit != null)
			{
				DebugGainActionPoints();
				DebugGainMana();
				DebugReplenishEquippedSkills();
			}
			DebugGainArmor();
			DebugGainHealth();
			DebugGainMovePoints();
			TileObjectSelectionManager.SelectedUnit.UnitView.RefreshInjuryStage();
		}
	}

	[DevConsoleCommand("SetFaceId")]
	public static void DebugSetFaceId([StringConverter(typeof(PlayableUnit.StringToFaceIdConverter))] string faceId)
	{
		if (!TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).LogError((object)"Please select a unit before firing this command", (Object)(object)TPSingleton<PlayableUnitManager>.Instance, (CLogLevel)1, true, true);
			return;
		}
		if (PlayableUnitDatabase.GetFaceIdsForGender(TileObjectSelectionManager.SelectedPlayableUnit.Gender).Contains(faceId))
		{
			GenerateNewPortrait(TileObjectSelectionManager.SelectedPlayableUnit, faceId, changeBackgroundColor: false, changeHairSkinEyesColor: false);
			return;
		}
		((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).LogError((object)("faceId '" + faceId + "' doesn't exist in the " + ((TileObjectSelectionManager.SelectedPlayableUnit.Gender == "Male") ? "PlayableMaleUnitFaceIds" : "PlayableFemaleUnitFaceIds") + " list."), (Object)(object)TPSingleton<PlayableUnitManager>.Instance, (CLogLevel)1, true, true);
	}

	private static void GenerateNewPortrait(PlayableUnit playableUnit, string faceId, bool changeBackgroundColor, bool changeHairSkinEyesColor)
	{
		if (changeBackgroundColor)
		{
			PlayableUnitView.GetRandomPortraitBGColor(playableUnit);
		}
		playableUnit.PlayableUnitController.DebugSetFaceId(faceId);
		playableUnit.PlayableUnitView.DebugRefreshColorSwapping();
		TileObjectSelectionManager.SetSelectedPlayableUnit(playableUnit);
	}

	[DevConsoleCommand("GenerateRandomPortrait")]
	public static void DebugGenerateRandomPortrait(bool changeFaceId = true, bool changeBackgroundColor = true, bool changeHairSkinEyesColor = true)
	{
		if (!TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).LogError((object)"Please select a unit before firing this command", (Object)(object)TPSingleton<PlayableUnitManager>.Instance, (CLogLevel)1, true, true);
			return;
		}
		string faceId = TileObjectSelectionManager.SelectedPlayableUnit.FaceId;
		if (changeFaceId)
		{
			faceId = TPHelpers.RandomElement<string>(PlayableUnitDatabase.GetFaceIdsForGender(TileObjectSelectionManager.SelectedPlayableUnit.Gender));
		}
		GenerateNewPortrait(TileObjectSelectionManager.SelectedPlayableUnit, faceId, changeBackgroundColor, changeHairSkinEyesColor);
	}

	[DevConsoleCommand("RemoveTrait")]
	public static void DebugRemoveTrait([StringConverter(typeof(PlayableUnit.StringToCurrentTraitsConverter))] string traitId)
	{
		if (!TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).LogError((object)"Please select a unit before firing this command", (Object)(object)TPSingleton<PlayableUnitManager>.Instance, (CLogLevel)1, true, true);
			return;
		}
		UnitTraitDefinition unitTraitDefinition = TileObjectSelectionManager.SelectedPlayableUnit.UnitTraitDefinitions.Find((UnitTraitDefinition trait) => trait.Id == traitId);
		if (unitTraitDefinition != null)
		{
			TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitController.RemoveTrait(unitTraitDefinition);
			TPSingleton<CharacterSheetPanel>.Instance.Refresh();
			UnitManagementView<PlayableUnitManagementView>.Refresh();
			return;
		}
		((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).LogError((object)("traitId '" + traitId + "' doesn't exist in " + TileObjectSelectionManager.SelectedUnit.Id + " traits."), (Object)(object)TPSingleton<PlayableUnitManager>.Instance, (CLogLevel)1, true, true);
	}

	[DevConsoleCommand("RemoveAllTraits")]
	public static void DebugRemoveAllTraits()
	{
		if (!TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).LogError((object)"Please select a unit before firing this command", (Object)(object)TPSingleton<PlayableUnitManager>.Instance, (CLogLevel)1, true, true);
			return;
		}
		for (int num = TileObjectSelectionManager.SelectedPlayableUnit.UnitTraitDefinitions.Count - 1; num >= 0; num--)
		{
			TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitController.RemoveTrait(TileObjectSelectionManager.SelectedPlayableUnit.UnitTraitDefinitions[num]);
		}
		TPSingleton<CharacterSheetPanel>.Instance.Refresh();
		UnitManagementView<PlayableUnitManagementView>.Refresh();
	}

	[DevConsoleCommand("SetHairPalette")]
	public static void DebugSetHairPalette([StringConverter(typeof(PlayableUnit.StringToHairPaletteIdConverter))] string hairPaletteId)
	{
		ColorSwapPaletteDefinition value;
		if (!TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).LogError((object)"Please select a unit before firing this command", (Object)(object)TPSingleton<PlayableUnitManager>.Instance, (CLogLevel)1, true, true);
		}
		else if (PlayableUnitDatabase.PlayableUnitHairColorDefinitions.TryGetValue(hairPaletteId, out value))
		{
			TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitController.DebugSetColorPalette(value, isHairPalette: true);
		}
	}

	[DevConsoleCommand("SetRandomPortraitBGColor")]
	public static void DebugSetRandomPortraitBGColor()
	{
		if (!TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).LogError((object)"Please select a unit before firing this command", (Object)(object)TPSingleton<PlayableUnitManager>.Instance, (CLogLevel)1, true, true);
			return;
		}
		PlayableUnitView.RemoveUsedPortraitColor(TileObjectSelectionManager.SelectedPlayableUnit.PortraitColor);
		PlayableUnitView.GetRandomPortraitBGColor(TileObjectSelectionManager.SelectedPlayableUnit);
		PlayableUnitManagementView.UnitPortraitView.RefreshPortrait();
		GameView.TopScreenPanel.UnitPortraitsPanel.RefreshPortraits();
	}

	[DevConsoleCommand("SetSkinPalette")]
	public static void DebugSetSkinPalette([StringConverter(typeof(PlayableUnit.StringToSkinPaletteIdConverter))] string skinPaletteId)
	{
		ColorSwapPaletteDefinition value;
		if (!TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).LogError((object)"Please select an unit before firing this command", (Object)(object)TPSingleton<PlayableUnitManager>.Instance, (CLogLevel)1, true, true);
		}
		else if (PlayableUnitDatabase.PlayableUnitSkinColorDefinitions.TryGetValue(skinPaletteId, out value))
		{
			TileObjectSelectionManager.SelectedPlayableUnit.PlayableUnitController.DebugSetColorPalette(value, isHairPalette: false);
		}
	}

	[DevConsoleCommand("SetStat")]
	public static void DebugSetStat([StringConverter(typeof(PlayableUnit.StringToStatIdConverter))] string statId, float newStatValue)
	{
		if (!TileObjectSelectionManager.HasUnitSelected)
		{
			((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).LogError((object)"Please select an unit before firing this command", (Object)(object)TPSingleton<PlayableUnitManager>.Instance, (CLogLevel)1, true, true);
			return;
		}
		TheLastStand.Model.Unit.Unit selectedUnit = TileObjectSelectionManager.SelectedUnit;
		UnitStatDefinition.E_Stat stat = (UnitStatDefinition.E_Stat)Enum.Parse(typeof(UnitStatDefinition.E_Stat), statId);
		if (selectedUnit is PlayableUnit playableUnit)
		{
			PlayableUnitStat stat2 = playableUnit.PlayableUnitStatsController.GetStat(stat);
			float num = newStatValue - stat2.FinalClamped;
			if (num < 0f)
			{
				playableUnit.PlayableUnitStatsController.DecreaseBaseStat(stat, Mathf.Abs(num), includeChildStat: true);
			}
			else if (num > 0f)
			{
				playableUnit.PlayableUnitStatsController.IncreaseBaseStat(stat, num, includeChildStat: true);
			}
		}
		else
		{
			selectedUnit.UnitStatsController.SetBaseStat(stat, newStatValue);
		}
		selectedUnit.UnitController.UpdateInjuryStage();
		if (TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			UnitManagementView<PlayableUnitManagementView>.Refresh();
			TPSingleton<CharacterSheetPanel>.Instance.Refresh();
		}
		else if (TileObjectSelectionManager.HasEnemyUnitSelected)
		{
			UnitManagementView<EnemyUnitManagementView>.Refresh();
			EnemyUnitManager.EnemyUnitInfoPanel.Refresh();
		}
		selectedUnit.UnitView.RefreshInjuryStage();
		selectedUnit.UnitController.DisplayEffects();
	}

	[DevConsoleCommand(Name = "ForceSkipNightReport")]
	private static void DebugSkipNightReport(bool forceSkipNightReport = true)
	{
		TPSingleton<PlayableUnitManager>.Instance.debugForceSkipNightReport = forceSkipNightReport;
	}

	[DevConsoleCommand("ToggleHealthDisplay")]
	public static void Debug_ToggleHealthDisplay()
	{
		TPSingleton<PlayableUnitManager>.Instance.debugDisableHealthDisplay = !TPSingleton<PlayableUnitManager>.Instance.debugDisableHealthDisplay;
		RefreshHealthDisplays();
	}

	[DevConsoleCommand("DismissHeroToggleValidityChecks")]
	private static void DebugToggleDismissHeroChecks(bool check = false)
	{
		TPSingleton<PlayableUnitManager>.Instance.debugToggleDismissHeroValidityChecks = check;
		TPSingleton<CharacterSheetPanel>.Instance.Refresh();
	}

	[DevConsoleCommand("EquipmentSlotAdd")]
	private static void Debug_EquipmentSlotAdd(ItemSlotDefinition.E_ItemSlotId slotType = ItemSlotDefinition.E_ItemSlotId.Usables, int amount = 1)
	{
		PlayableUnit reliablePlayableUnit = TileObjectSelectionManager.ReliablePlayableUnit;
		UnitTraitDefinition.SlotModifier slotModifier = new UnitTraitDefinition.SlotModifier(slotType, amount, addSlot: true, string.Empty);
		for (int i = 0; i < slotModifier.Amount; i++)
		{
			List<EquipmentSlotView> list = CharacterSheetPanel.EquipmentSlots[slotModifier.Name];
			int num = 0;
			if (reliablePlayableUnit.EquipmentSlots.TryGetValue(slotModifier.Name, out var value))
			{
				num = value.Count;
			}
			if (reliablePlayableUnit.EquipmentSlots.ContainsKey(slotModifier.Name))
			{
				if (list.Count - 1 >= num)
				{
					EquipmentSlot equipmentSlot = new EquipmentSlotController(ItemDatabase.ItemSlotDefinitions[slotModifier.Name], list[num], reliablePlayableUnit).EquipmentSlot;
					reliablePlayableUnit.EquipmentSlots[slotModifier.Name].Add(equipmentSlot);
					list[num].ItemSlot = equipmentSlot;
					list[num].Refresh();
				}
			}
			else
			{
				EquipmentSlot equipmentSlot2 = new EquipmentSlotController(ItemDatabase.ItemSlotDefinitions[slotModifier.Name], list[num], reliablePlayableUnit).EquipmentSlot;
				reliablePlayableUnit.EquipmentSlots.Add(slotModifier.Name, new List<EquipmentSlot>());
				reliablePlayableUnit.EquipmentSlots[slotModifier.Name].Add(equipmentSlot2);
				list[num].ItemSlot = equipmentSlot2;
				list[num].Refresh();
			}
		}
	}

	[DevConsoleCommand("EquipmentSlotRemove")]
	private static void Debug_EquipmentSlotRemove(ItemSlotDefinition.E_ItemSlotId slotId = ItemSlotDefinition.E_ItemSlotId.Usables, int amount = 1)
	{
		PlayableUnit reliablePlayableUnit = TileObjectSelectionManager.ReliablePlayableUnit;
		UnitTraitDefinition.SlotModifier slotModifier = new UnitTraitDefinition.SlotModifier(slotId, amount, addSlot: false, string.Empty);
		for (int i = 0; i < slotModifier.Amount; i++)
		{
			List<EquipmentSlotView> list = CharacterSheetPanel.EquipmentSlots[slotModifier.Name];
			if (!reliablePlayableUnit.EquipmentSlots.ContainsKey(slotId))
			{
				continue;
			}
			int index = reliablePlayableUnit.EquipmentSlots[slotModifier.Name].Count - 1;
			reliablePlayableUnit.EquipmentSlots[slotId].RemoveAt(reliablePlayableUnit.EquipmentSlots[slotId].Count - 1);
			list[index].ItemSlot = null;
			list[index].Refresh();
			if (reliablePlayableUnit.EquipmentSlots[slotId].Count == 0)
			{
				reliablePlayableUnit.EquipmentSlots.Remove(slotId);
				if (slotId == ItemSlotDefinition.E_ItemSlotId.LeftHand)
				{
					reliablePlayableUnit.BodyParts["Arm_L"].ChangeAdditionalConstraint("Hide", add: true);
				}
			}
		}
	}

	[DevConsoleCommand("PerksUnlockAllTiers")]
	private static void Debug_UnlockAllPerksTiers()
	{
		TileObjectSelectionManager.SelectedPlayableUnit.PerkTree.UnitPerkTreeController.DebugUpdateAllTiersAvailability();
	}

	[DevConsoleCommand("PerkUnlock")]
	private static void Debug_UnlockPerk([StringConverter(typeof(StringToPerkIdConverter))] string perkId)
	{
		TileObjectSelectionManager.SelectedPlayableUnit.PerkTree.UnitPerkTreeController.UnlockPerk(perkId);
		UnitManagementView<PlayableUnitManagementView>.Refresh();
	}

	[DevConsoleCommand("PerkUnlockAll")]
	private static void Debug_UnlockAllPerks()
	{
		PlayableUnit selectedPlayableUnit = TileObjectSelectionManager.SelectedPlayableUnit;
		foreach (Perk item in selectedPlayableUnit.PerkTree.UnitPerkTiers.SelectMany((UnitPerkTier perkTier) => perkTier.Perks))
		{
			if (item != null && !selectedPlayableUnit.Perks.ContainsKey(item.PerkDefinition.Id))
			{
				selectedPlayableUnit.PerkTree.UnitPerkTreeController.UnlockPerk(item.PerkDefinition.Id);
			}
		}
		UnitManagementView<PlayableUnitManagementView>.Refresh();
	}

	[DevConsoleCommand("PerkReroll")]
	private static void Debug_RerollPerks()
	{
		PlayableUnit selectedPlayableUnit = TileObjectSelectionManager.SelectedPlayableUnit;
		if (selectedPlayableUnit == null)
		{
			return;
		}
		List<string> list = new List<string>();
		foreach (string key in selectedPlayableUnit.Perks.Keys)
		{
			list.Add(key);
		}
		foreach (string item in list)
		{
			selectedPlayableUnit.Perks[item].PerkController.Lock();
		}
		selectedPlayableUnit.PerkTree.UnitPerkTiers.Clear();
		selectedPlayableUnit.Perks.Clear();
		selectedPlayableUnit.PerkTree.UnitPerkTreeController.GeneratePerkTree(selectedPlayableUnit);
		TPSingleton<CharacterSheetPanel>.Instance.Refresh();
		UnitManagementView<PlayableUnitManagementView>.Refresh();
	}

	[DevConsoleCommand("UltimateCheat")]
	private static void Debug_UltimateCheat()
	{
		if (TileObjectSelectionManager.HasPlayableUnitSelected)
		{
			DebugSetStat("PhysicalDamage", 500f);
			DebugSetStat("MagicalDamage", 500f);
			DebugSetStat("RangedDamage", 500f);
			DebugSetStat("OverallDamage", 500f);
			DebugSetStat("Accuracy", 500f);
			DebugSetStat("Reliability", 500f);
			DebugSetStat("MovePointsTotal", 500f);
			DebugSetStat("MovePoints", 500f);
			DebugSetStat("ActionPointsTotal", 500f);
			DebugSetStat("ActionPoints", 500f);
			DebugSetStat("HealthTotal", 1000f);
			DebugSetStat("Health", 1000f);
			DebugSetStat("ArmorTotal", 1000f);
			DebugSetStat("Armor", 1000f);
			DebugSetStat("ManaTotal", 500f);
			DebugSetStat("Mana", 500f);
			DebugSetStat("Resistance", 500f);
			DebugSetStat("Dodge", 500f);
			DebugSetStat("Block", 500f);
			DebugSetStat("PropagationBouncesModifier", 500f);
			DebugSetStat("PropagationDamage", 500f);
			DebugSetStat("PoisonDamageModifier", 500f);
			DebugSetStat("Critical", 0f);
			DebugSetStat("CriticalPower", 100f);
		}
		ItemManager.DebugGenerateItem("HandCrossbow0");
		ItemManager.DebugGenerateItem("DruidicStaff0");
		ItemManager.DebugGenerateItem("TomeOfMagic0");
		InventoryManager.Debug_ForceInventoryAccess();
		ShopManager.DebugForceShopAccess();
		BuildingManager.DebugToggleMagicCircleIndestructibility();
		TurnEndValidationManager.DebugByPassEndTurnChecks();
		SkillManager.DebugToggleSkillCastValidityCheck();
		SkillManager.DebugSetSkillsAllowAllPhases();
		ConstructionManager.DBG_ForceConstructionAllowed();
		ResourceManager.Debug_GainResources();
		MetaShopsManager.DebugForceOraculumAccess();
	}

	[DevConsoleCommand("SetStatSwole")]
	private static void Debug_SetStatSwole()
	{
		if (TileObjectSelectionManager.SelectedPlayableUnit != null)
		{
			DebugRemoveAllTraits();
			DebugSetStat("PhysicalDamage", 500f);
			DebugSetStat("MagicalDamage", 500f);
			DebugSetStat("RangedDamage", 500f);
			DebugSetStat("OverallDamage", 500f);
			DebugSetStat("Accuracy", 500f);
			DebugSetStat("Critical", 500f);
			DebugSetStat("CriticalPower", 500f);
			DebugSetStat("Reliability", 500f);
			DebugSetStat("MovePointsTotal", 500f);
			DebugSetStat("MovePoints", 500f);
			DebugSetStat("ActionPointsTotal", 500f);
			DebugSetStat("ActionPoints", 500f);
			DebugSetStat("HealthTotal", 1000f);
			DebugSetStat("Health", 1000f);
			DebugSetStat("ArmorTotal", 1000f);
			DebugSetStat("Armor", 1000f);
			DebugSetStat("ManaTotal", 500f);
			DebugSetStat("Mana", 500f);
			DebugSetStat("Resistance", 500f);
			DebugSetStat("Dodge", 500f);
			DebugSetStat("Block", 500f);
			DebugSetStat("PropagationBouncesModifier", 500f);
			DebugSetStat("PropagationDamage", 500f);
			DebugSetStat("PoisonDamageModifier", 500f);
			DebugSetStat("MultiHitsCountModifier", 500f);
			DebugSetStat("SkillRangeModifier", 500f);
		}
	}

	[DevConsoleCommand("BestFiendShowList")]
	private static void Debug_BestFiendShowList()
	{
		PlayableUnit selectedPlayableUnit = TileObjectSelectionManager.SelectedPlayableUnit;
		if (selectedPlayableUnit == null)
		{
			return;
		}
		string text = string.Empty;
		float num = 0f;
		foreach (KeyValuePair<string, float> damagesInflictedToEnemy in selectedPlayableUnit.LifetimeStats.DamagesInflictedToEnemies)
		{
			num += damagesInflictedToEnemy.Value;
			text += $"\n{damagesInflictedToEnemy.Key} : {damagesInflictedToEnemy.Value}";
		}
		text += $"\nTotal : {num}";
		selectedPlayableUnit.Log(text, (CLogLevel)2, forcePrintInUnity: true);
		TPSingleton<DebugManager>.Instance.LogDevConsole((object)text);
	}

	private static void RefreshHealthDisplays()
	{
		foreach (PlayableUnit playableUnit in TPSingleton<PlayableUnitManager>.Instance.PlayableUnits)
		{
			if ((Object)(object)playableUnit.PlayableUnitView != (Object)null && (Object)(object)playableUnit.PlayableUnitView.UnitHUD != (Object)null && (Object)(object)playableUnit.PlayableUnitView.UnitHUD.BgHighlightCanvas != (Object)null)
			{
				((Behaviour)playableUnit.PlayableUnitView.UnitHUD.BgHighlightCanvas).enabled = !TPSingleton<PlayableUnitManager>.Instance.debugDisableHealthDisplay;
				playableUnit.PlayableUnitView.UnitHUD.DisplayHealthIfNeeded();
			}
		}
		foreach (EnemyUnit enemyUnit in TPSingleton<EnemyUnitManager>.Instance.EnemyUnits)
		{
			if ((Object)(object)enemyUnit.EnemyUnitView != (Object)null && (Object)(object)enemyUnit.EnemyUnitView.UnitHUD != (Object)null)
			{
				enemyUnit.EnemyUnitView?.UnitHUD?.DisplayHealthIfNeeded();
			}
		}
		foreach (TheLastStand.Model.Building.Building building in TPSingleton<BuildingManager>.Instance.Buildings)
		{
			if ((Object)(object)building.BuildingView != (Object)null && (Object)(object)building.BuildingView.BuildingHUD != (Object)null)
			{
				building.BuildingView.BuildingHUD.DisplayHealthIfNeeded();
			}
		}
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		PlayableUnits = new List<PlayableUnit>();
		DeadPlayableUnits = new Dictionary<int, List<PlayableUnit>>();
		PlayableUnitView.UsedUnitPortrait.Clear();
		if (container is SerializedPlayableUnits serializedPlayableUnits)
		{
			foreach (SerializedPlayableUnit ownedUnit in serializedPlayableUnits.OwnedUnits)
			{
				PlayableUnit playableUnit = new PlayableUnitController(ownedUnit, saveVersion).PlayableUnit;
				InstantiateUnit(playableUnit, playableUnit.OriginTile, saveVersion, onLoad: true);
			}
			TPSingleton<MetaConditionManager>.Instance.RefreshMaxPlayableUnitStatReached(PlayableUnits);
			Recruitment = new Recruitment(serializedPlayableUnits.Recruitment, saveVersion);
			foreach (SerializedDeadUnit deadUnit in serializedPlayableUnits.DeadUnits)
			{
				if (!DeadPlayableUnits.ContainsKey(deadUnit.DeathTurn))
				{
					DeadPlayableUnits.Add(deadUnit.DeathTurn, new List<PlayableUnit>());
				}
				PlayableUnit playableUnit2 = new PlayableUnitController(deadUnit.Unit, -1, isDead: true).PlayableUnit;
				InstantiateDeadUnit(playableUnit2, TileMapManager.GetTile(0, 0));
				DeadPlayableUnits[deadUnit.DeathTurn].Add(playableUnit2);
			}
			TileObjectSelectionManager.EnsureUnitSelection();
		}
		selectedSkill = null;
		PlayableUnitGhostView playableUnitGhostView = ((Component)playableUnitGhostParent).GetComponentInChildren<PlayableUnitGhostView>();
		if ((Object)(object)playableUnitGhostView == (Object)null)
		{
			playableUnitGhostView = Object.Instantiate<PlayableUnitGhostView>(playableUnitGhostViewPrefab, playableUnitGhostParent);
		}
		this.playableUnitGhostView.Add(playableUnitGhostView);
		playableUnitGhostView.Display(displayed: false);
		if (container == null)
		{
			RecruitmentController.InitMageGenerationProbability();
		}
		movePath = new MovePathController(movePathView).MovePath;
		TPSingleton<MovePathCounterHUD>.Instance.MovePath = movePath;
	}

	public ISerializedData Serialize()
	{
		SerializedPlayableUnits serializedPlayableUnits = new SerializedPlayableUnits
		{
			OwnedUnits = PlayableUnits.Select((PlayableUnit o) => o.Serialize() as SerializedPlayableUnit).ToList(),
			DeadUnits = new List<SerializedDeadUnit>(),
			Recruitment = (Recruitment.Serialize() as SerializedRecruitment)
		};
		foreach (KeyValuePair<int, List<PlayableUnit>> entry in DeadPlayableUnits)
		{
			serializedPlayableUnits.DeadUnits.AddRange(entry.Value.Select((PlayableUnit o) => new SerializedDeadUnit
			{
				Unit = (o.Serialize() as SerializedPlayableUnit),
				DeathTurn = entry.Key
			}).ToList());
		}
		return serializedPlayableUnits;
	}
}
