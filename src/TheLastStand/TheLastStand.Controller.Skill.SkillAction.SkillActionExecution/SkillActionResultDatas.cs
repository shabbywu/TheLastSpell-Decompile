using System.Collections.Generic;
using System.Linq;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;

namespace TheLastStand.Controller.Skill.SkillAction.SkillActionExecution;

public class SkillActionResultDatas
{
	public HashSet<TheLastStand.Model.Building.Building> AffectedBuildings { get; } = new HashSet<TheLastStand.Model.Building.Building>();


	public HashSet<TheLastStand.Model.Unit.Unit> AffectedUnits { get; } = new HashSet<TheLastStand.Model.Unit.Unit>();


	public HashSet<IDamageable> PotentiallyAffectedDamageables { get; } = new HashSet<IDamageable>();


	public List<IDamageable> MissedDamageables => PotentiallyAffectedDamageables.Except(AffectedUnits).Except(AffectedBuildings.Select((TheLastStand.Model.Building.Building b) => b.DamageableModule)).ToList();

	public Dictionary<Tile, (string UnitId, UnitCreationSettings unitCreationSettings)> UnitsToSpawnTarget { get; } = new Dictionary<Tile, (string, UnitCreationSettings)>();


	public float TotalDamagesToHealth { get; set; }

	public void AddAffectedBuilding(TheLastStand.Model.Building.Building building)
	{
		AffectedBuildings.Add(building);
	}

	public void AddAffectedUnit(TheLastStand.Model.Unit.Unit unit)
	{
		AffectedUnits.Add(unit);
	}
}
