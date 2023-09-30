using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using TheLastStand.Controller.TileMap;
using TheLastStand.Definition.Unit.Perk;
using TheLastStand.Framework.ExpressionInterpreter;
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
		Node amountExpression = PerkTargetingDefinition.AmountExpression;
		int num = ((amountExpression != null) ? amountExpression.EvalToInt((InterpreterContext)(object)perk) : int.MaxValue);
		Node rangeExpression = PerkTargetingDefinition.RangeExpression;
		int maxRange = ((rangeExpression == null) ? 1 : rangeExpression.EvalToInt((InterpreterContext)(object)perk));
		foreach (ITileObject TargetingReferenceTileObject in TargetingReferenceTileObjects)
		{
			switch (PerkTargetingDefinition.TargetingMethod)
			{
			case PerkTargetingDefinition.E_TargetingMethod.AdjacentDamageables:
				LinqExtensions.AddRange<Tile>(hashSet, TargetingReferenceTileObject.TileObjectController.GetAdjacentTiles().Where(delegate(Tile t)
				{
					if (!t.HasFog)
					{
						IDamageable damageable4 = t.Damageable;
						if (damageable4 != null && damageable4.CanBeDamaged())
						{
							ITileObject tileObject3 = t.TileObject;
							if ((tileObject3 == null || tileObject3.IsTargetableByAI()) && t.Damageable != perk.Owner)
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
						ITileObject tileObject = item.TileObject;
						if (tileObject == null || tileObject.IsTargetableByAI())
						{
							hashSet.Add(item);
						}
					}
				}
				break;
			case PerkTargetingDefinition.E_TargetingMethod.DamageablesInRange:
				foreach (Tile item2 in TargetingReferenceTileObject.TileObjectController.GetTilesInRange(maxRange).Where(delegate(Tile t)
				{
					if (!t.HasFog)
					{
						IDamageable damageable3 = t.Damageable;
						if (damageable3 != null && damageable3.CanBeDamaged() && t.Damageable != perk.Owner)
						{
							return PerkTargetingDefinition.ValidDamageableTypes?.Contains(t.Damageable.DamageableType) ?? true;
						}
					}
					return false;
				}).ToList())
				{
					if (hashSet.Count >= num)
					{
						break;
					}
					ITileObject tileObject2 = item2.TileObject;
					if (tileObject2 == null || tileObject2.IsTargetableByAI())
					{
						hashSet.Add(item2);
					}
				}
				break;
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
