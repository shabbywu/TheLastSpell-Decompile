using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using TPLib;
using TheLastStand.Controller.TileMap;
using TheLastStand.Definition.Unit.Perk;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Skill.SkillAction.SkillActionExecution.SkillActionExecutionTileData;
using TheLastStand.Model.TileMap;

namespace TheLastStand.Model.Unit.Perk;

public class PerkTargeting
{
	public PerkTargetingDefinition PerkTargetingDefinition { get; }

	public PerkTargeting(PerkTargetingDefinition perkTargetingDefinition)
	{
		PerkTargetingDefinition = perkTargetingDefinition;
	}

	private List<ITileObject> GetTargetingReference(PerkDataContainer data, Perk perk)
	{
		List<ITileObject> list = new List<ITileObject>();
		switch (PerkTargetingDefinition.TargetingReference)
		{
		case PerkTargetingDefinition.E_TargetingReference.Owner:
			list.Add(perk.Owner);
			break;
		case PerkTargetingDefinition.E_TargetingReference.Caster:
			list.Add(data.Caster);
			break;
		case PerkTargetingDefinition.E_TargetingReference.Target:
			list.Add(data.TargetUnit);
			break;
		case PerkTargetingDefinition.E_TargetingReference.AllTargets:
			list.AddRange(data.AllAttackData.Select((AttackSkillActionExecutionTileData x) => x.TargetTile));
			break;
		}
		return list;
	}

	private HashSet<Tile> GetTargetFromBase(PerkDataContainer data, Perk perk, List<ITileObject> TargetingReferenceTileObjects, List<Tile> validTargets)
	{
		HashSet<Tile> hashSet = new HashSet<Tile>();
		int num = PerkTargetingDefinition.AmountExpression?.EvalToInt(perk) ?? int.MaxValue;
		int maxRange = PerkTargetingDefinition.RangeExpression?.EvalToInt(perk) ?? 1;
		foreach (ITileObject TargetingReferenceTileObject in TargetingReferenceTileObjects)
		{
			switch (PerkTargetingDefinition.TargetingMethod)
			{
			case PerkTargetingDefinition.E_TargetingMethod.AdjacentDamageables:
				LinqExtensions.AddRange<Tile>(hashSet, TargetingReferenceTileObject.TileObjectController.GetAdjacentTiles().Where(delegate(Tile t)
				{
					if (!t.HasFog)
					{
						IDamageable damageable6 = t.Damageable;
						if (damageable6 != null && damageable6.CanBeDamaged())
						{
							IDamageable damageable7 = t.Damageable;
							if ((damageable7 == null || damageable7.IsTargetableByAI()) && t.Damageable != perk.Owner)
							{
								return PerkTargetingDefinition.ValidDamageableTypes?.Contains(t.Damageable.DamageableType) ?? true;
							}
						}
					}
					return false;
				}));
				break;
			case PerkTargetingDefinition.E_TargetingMethod.ClosestTarget:
				foreach (Tile item in validTargets.OrderBy((Tile t) => TileMapController.DistanceBetweenTiles(TargetingReferenceTileObject.OriginTile, t)).ToList())
				{
					if (hashSet.Count >= num)
					{
						break;
					}
					IDamageable damageable2 = item.Damageable;
					if (damageable2 != null && damageable2.CanBeDamaged())
					{
						IDamageable damageable3 = item.Damageable;
						if (damageable3 == null || damageable3.IsTargetableByAI())
						{
							hashSet.Add(item);
						}
					}
				}
				break;
			case PerkTargetingDefinition.E_TargetingMethod.DamageablesInRange:
			{
				List<Tile> list = TargetingReferenceTileObject.TileObjectController.GetTilesInRange(maxRange).Where(delegate(Tile t)
				{
					if (!t.HasFog)
					{
						IDamageable damageable5 = t.Damageable;
						if (damageable5 != null && damageable5.CanBeDamaged() && t.Damageable != perk.Owner)
						{
							return PerkTargetingDefinition.ValidDamageableTypes?.Contains(t.Damageable.DamageableType) ?? true;
						}
					}
					return false;
				}).ToList();
				if (num != int.MaxValue)
				{
					list = RandomManager.Shuffle(TPSingleton<PlayableUnitManager>.Instance, list).ToList();
				}
				foreach (Tile item2 in list)
				{
					if (hashSet.Count >= num)
					{
						break;
					}
					IDamageable damageable4 = item2.Damageable;
					if (damageable4 == null || damageable4.IsTargetableByAI())
					{
						hashSet.Add(item2);
					}
				}
				break;
			}
			case PerkTargetingDefinition.E_TargetingMethod.Self:
			{
				IDamageable damageable = TargetingReferenceTileObject.OriginTile.Damageable;
				if (damageable != null && damageable.CanBeDamaged())
				{
					hashSet.Add(TargetingReferenceTileObject.OriginTile);
				}
				break;
			}
			}
		}
		return hashSet;
	}

	public HashSet<Tile> GetTargetTiles(PerkDataContainer data, Perk perk, List<Tile> validTargets = null)
	{
		HashSet<Tile> result = new HashSet<Tile>();
		bool flag = PerkTargetingDefinition.TargetingReference != PerkTargetingDefinition.E_TargetingReference.Owner;
		if (data == null && flag)
		{
			return result;
		}
		List<ITileObject> targetingReference = GetTargetingReference(data, perk);
		return GetTargetFromBase(data, perk, targetingReference, validTargets);
	}
}
