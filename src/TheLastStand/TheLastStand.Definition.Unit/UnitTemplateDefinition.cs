using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;
using TheLastStand.Model;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.Extensions;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using UnityEngine;

namespace TheLastStand.Definition.Unit;

public abstract class UnitTemplateDefinition : Definition, ITileObjectDefinition
{
	public enum E_MoveMethod
	{
		Undefined,
		Walking,
		Flying,
		AboveAll
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct UnitTypeComparer : IEqualityComparer<DamageableType>
	{
		public bool Equals(DamageableType x, DamageableType y)
		{
			return x == y;
		}

		public int GetHashCode(DamageableType obj)
		{
			return (int)obj;
		}
	}

	public static readonly UnitTypeComparer SharedUnitTypeComparer;

	public string DamagedParticlesId { get; protected set; } = string.Empty;


	public List<InjuryDefinition> InjuryDefinitions { get; protected set; }

	public E_MoveMethod MoveMethod { get; protected set; } = E_MoveMethod.Walking;


	public int OriginX { get; protected set; }

	public int OriginY { get; protected set; }

	public Vector3 SelectedFeedbackSize { get; protected set; } = Vector3.one;


	public List<List<Tile.E_UnitAccess>> Tiles { get; protected set; }

	public abstract Tile.E_UnitAccess UnitAccessNeeded { get; }

	public DamageableType UnitType { get; protected set; }

	public UnitTemplateDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}//IL_0013: Unknown result type (might be due to invalid IL or missing references)
	//IL_0018: Unknown result type (might be due to invalid IL or missing references)


	public bool CanSpawnOn(Tile tile, bool isPhaseActor = false, bool ignoreUnits = false, bool ignoreBuildings = false)
	{
		foreach (Tile occupiedTile in tile.GetOccupiedTiles(this))
		{
			if (!CanSpawnOnSingleTile(occupiedTile, isPhaseActor, ignoreUnits, ignoreBuildings))
			{
				return false;
			}
		}
		return true;
	}

	protected virtual bool CanSpawnOnSingleTile(Tile tile, bool isPhaseActor = false, bool ignoreUnits = false, bool ignoreBuildings = false)
	{
		if (!CanTravelThrough(tile, ignoreUnits, ignoreBuildings))
		{
			return false;
		}
		if (MoveMethod != E_MoveMethod.AboveAll && !ignoreBuildings && tile.Building != null)
		{
			DamageableModule damageableModule = tile.Building.DamageableModule;
			if ((damageableModule == null || !damageableModule.IsDead) && !tile.CurrentUnitAccess.HasFlag(UnitAccessNeeded))
			{
				return false;
			}
		}
		if ((ignoreUnits || tile.Unit == null) && tile.Building == null && !tile.CurrentUnitAccess.HasFlag(UnitAccessNeeded))
		{
			return false;
		}
		if (!ignoreUnits && tile.Unit != null && (!isPhaseActor || (tile.Unit is EnemyUnit enemyUnit && enemyUnit.IsBossPhaseActor)))
		{
			return false;
		}
		return true;
	}

	public bool CanStopOn(Tile tile, TheLastStand.Model.Unit.Unit unit)
	{
		foreach (Tile occupiedTile in tile.GetOccupiedTiles(this))
		{
			if (!CanStopOnSingleTile(occupiedTile, unit))
			{
				return false;
			}
		}
		return true;
	}

	protected virtual bool CanStopOnSingleTile(Tile tile, TheLastStand.Model.Unit.Unit unit)
	{
		if (!CanTravelThrough(tile))
		{
			return false;
		}
		if (MoveMethod != E_MoveMethod.AboveAll && tile.Building != null)
		{
			DamageableModule damageableModule = tile.Building.DamageableModule;
			if ((damageableModule == null || !damageableModule.IsDead) && !tile.CurrentUnitAccess.HasFlag(UnitAccessNeeded))
			{
				return false;
			}
		}
		if (tile.Building == null && tile.Unit == null && !tile.CurrentUnitAccess.HasFlag(UnitAccessNeeded))
		{
			return false;
		}
		if (tile.WillBeReached && tile.WillBeReachedBy != unit.RandomId)
		{
			return false;
		}
		return true;
	}

	public bool CanTravelThrough(Tile tile, bool ignoreUnits = false, bool ignoreBuildings = false)
	{
		return CanTravelThrough(tile, MoveMethod, ignoreUnits, ignoreBuildings);
	}

	public virtual bool CanTravelThrough(Tile tile, E_MoveMethod moveMethod, bool ignoreUnits = false, bool ignoreBuildings = false)
	{
		if (tile == null)
		{
			return false;
		}
		switch (moveMethod)
		{
		case E_MoveMethod.Walking:
			if (!tile.GroundDefinition.IsCrossable)
			{
				return false;
			}
			if (!ignoreBuildings && tile.Building != null && (tile.Building.BlueprintModule.IsIndestructible || tile.Building.ShouldWaitDeathLikeEffect || !tile.Building.DamageableModule.IsDead) && !tile.CurrentUnitAccess.HasFlag(UnitAccessNeeded))
			{
				return false;
			}
			if ((ignoreUnits || tile.Unit == null) && tile.Building == null && !tile.CurrentUnitAccess.HasFlag(UnitAccessNeeded))
			{
				return false;
			}
			break;
		case E_MoveMethod.Flying:
			if (!ignoreBuildings && tile.Building != null && tile.Building.BuildingDefinition.BlueprintModuleDefinition.BlockFlying)
			{
				return false;
			}
			break;
		}
		return true;
	}

	public override void Deserialize(XContainer xContainer)
	{
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		XContainer obj = ((xContainer is XElement) ? xContainer : null);
		XElement val = obj.Element(XName.op_Implicit("DamagedParticles"));
		if (val != null)
		{
			XAttribute val2 = val.Attribute(XName.op_Implicit("Id"));
			if (val2 != null)
			{
				DamagedParticlesId = val2.Value;
			}
		}
		XElement val3 = obj.Element(XName.op_Implicit("Tiles"));
		Tiles = new List<List<Tile.E_UnitAccess>>();
		if (val3 != null)
		{
			string[] array = val3.Value.Split(new char[1] { '\n' });
			for (int num = array.Length - 1; num >= 0; num--)
			{
				array[num] = TPHelpers.RemoveWhitespace(array[num]);
				if (array[num] != string.Empty)
				{
					Tiles.Add(new List<Tile.E_UnitAccess>(array[num].Length));
					for (int i = 0; i < array[num].Length; i++)
					{
						char tileChar = array[num][i];
						Tiles[Tiles.Count - 1].Add(Tile.CharToUnitAccess(tileChar));
					}
				}
			}
			OriginX = ((val3.Attribute(XName.op_Implicit("OriginX")) != null) ? int.Parse(val3.Attribute(XName.op_Implicit("OriginX")).Value) : 0);
			OriginY = ((val3.Attribute(XName.op_Implicit("OriginY")) != null) ? int.Parse(val3.Attribute(XName.op_Implicit("OriginY")).Value) : 0);
		}
		else
		{
			Tiles.Add(new List<Tile.E_UnitAccess> { Tile.E_UnitAccess.Blocked });
			OriginX = 0;
			OriginY = 0;
		}
		SelectedFeedbackSize = new Vector3((float)Tiles.First().Count, (float)Tiles.Count, 1f);
	}

	public abstract void DeserializeInjuries(XContainer container);
}
