using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.Skill.SkillAction.SkillActionExecution;
using TheLastStand.Database.Building;
using TheLastStand.Definition.Building;
using TheLastStand.Definition.Skill.SkillAction;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Skill;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Skill.SkillAction;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit.Enemy;

namespace TheLastStand.Controller.Skill.SkillAction;

public class BuildSkillActionController : SkillActionController
{
	public BuildSkillAction BuildSkillAction => base.SkillAction as BuildSkillAction;

	public BuildSkillActionController(SkillActionDefinition skillActionDefinition, TheLastStand.Model.Skill.Skill skill)
	{
		base.SkillAction = new BuildSkillAction(skillActionDefinition, this, skill);
		base.SkillAction.SkillActionExecution = new BuildSkillActionExecutionController(base.SkillAction.Skill).SkillActionExecution;
	}

	public override List<SkillActionResultDatas> ApplyEffect(ISkillCaster caster, List<Tile> affectedTiles, List<Tile> surroundingTiles)
	{
		BuildSkillAction buildSkillAction = base.SkillAction as BuildSkillAction;
		List<SkillActionResultDatas> list = new List<SkillActionResultDatas>();
		if (!BuildSkillAction.BuildSkillActionDefinition.ApplyOnCaster)
		{
			foreach (Tile affectedTile in affectedTiles)
			{
				list.Add(ApplyActionOnTile(affectedTile, caster));
			}
			if (buildSkillAction != null)
			{
				((CLogger<SkillManager>)TPSingleton<SkillManager>.Instance).Log((object)$"Amount of buildings spawned : {affectedTiles.Count}.", (CLogLevel)1, false, false);
			}
			return list;
		}
		((CLogger<SkillManager>)TPSingleton<SkillManager>.Instance).LogError((object)"A Build Skill Action should target his caster !", (CLogLevel)1, true, true);
		return list;
	}

	public override bool IsBuildingAffected(Tile targetTile)
	{
		return false;
	}

	public override bool IsUnitAffected(Tile targetTile)
	{
		return false;
	}

	protected override SkillActionResultDatas ApplyActionOnTile(Tile targetTile, ISkillCaster caster)
	{
		SkillActionResultDatas result = ApplyActionOnSurroundingTile(targetTile, caster);
		if (targetTile.Unit != null)
		{
			if (!(targetTile.Unit is EnemyUnit enemyUnit))
			{
				return result;
			}
			enemyUnit.EnemyUnitController.PrepareForExile();
			enemyUnit.EnemyUnitController.ExecuteExile();
		}
		if (targetTile.Building != null)
		{
			BuildingManager.DestroyBuilding(targetTile, updateView: true, addDeadBuilding: false, triggerEvent: true, triggerOnDeathEvent: false);
		}
		BuildingManager.CreateBuilding(GetBuildingDefinition(), targetTile);
		return result;
	}

	protected override SkillActionResultDatas ApplyActionOnSurroundingTile(Tile targetTile, ISkillCaster caster)
	{
		return new SkillActionResultDatas();
	}

	private BuildingDefinition GetBuildingDefinition()
	{
		int index = 0;
		int num = 0;
		for (int i = 0; i < BuildSkillAction.BuildSkillActionDefinition.Buildings.Count; i++)
		{
			num += BuildSkillAction.BuildSkillActionDefinition.Buildings.ElementAt(i).Value;
		}
		int randomRange = RandomManager.GetRandomRange(TPSingleton<EnemyUnitManager>.Instance, 0, num);
		int num2 = 0;
		for (int j = 0; j < BuildSkillAction.BuildSkillActionDefinition.Buildings.Count; j++)
		{
			if (randomRange >= num2 && randomRange < BuildSkillAction.BuildSkillActionDefinition.Buildings.ElementAt(j).Value + num2)
			{
				index = j;
				break;
			}
			num2 += BuildSkillAction.BuildSkillActionDefinition.Buildings.ElementAt(j).Value;
		}
		return BuildingDatabase.BuildingDefinitions[BuildSkillAction.BuildSkillActionDefinition.Buildings.ElementAt(index).Key];
	}
}
