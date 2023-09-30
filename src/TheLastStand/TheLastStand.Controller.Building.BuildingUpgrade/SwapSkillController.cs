using System.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.Skill;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Building.BuildingUpgrade;
using TheLastStand.Manager.Skill;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.BuildingUpgrade;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Unit.Enemy;

namespace TheLastStand.Controller.Building.BuildingUpgrade;

public class SwapSkillController : BuildingUpgradeEffectController
{
	public SwapSkill SwapSkill => base.BuildingUpgradeEffect as SwapSkill;

	public SwapSkillController(SwapSkillDefinition definition, TheLastStand.Model.Building.BuildingUpgrade.BuildingUpgrade buildingUpgrade)
	{
		base.BuildingUpgradeEffect = new SwapSkill(definition, this, buildingUpgrade);
	}

	public override void TriggerEffect(bool onLoad = false)
	{
		if (!SkillDatabase.SkillDefinitions.TryGetValue(SwapSkill.SwapSkillDefinition.NewSkillId, out var value))
		{
			((CLogger<SkillManager>)TPSingleton<SkillManager>.Instance).LogError((object)("Skill " + SwapSkill.SwapSkillDefinition.NewSkillId + " not found!"), (CLogLevel)2, true, true);
		}
		TheLastStand.Model.Building.Building building = base.BuildingUpgradeEffect.BuildingUpgrade.Building;
		TheLastStand.Model.Skill.Skill skill = new SkillController(value, building.BattleModule, -1, value.UsesPerTurnCount).Skill;
		if (building.BattleModule.Skills != null)
		{
			building.BattleModule.Skills = building.BattleModule.Skills.Select((TheLastStand.Model.Skill.Skill x) => (!(x.SkillDefinition.Id == SwapSkill.SwapSkillDefinition.OldSkillId)) ? x : skill).ToList();
		}
		if (building.BattleModule?.Goals == null)
		{
			return;
		}
		foreach (Goal item in building.BattleModule.Goals.Where((Goal x) => x.Skill.SkillDefinition.Id == SwapSkill.SwapSkillDefinition.OldSkillId))
		{
			item.Skill = skill;
		}
	}
}
