using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Model.Status;
using TheLastStand.Model.Unit;
using TheLastStand.View.Skill.SkillAction.UI;

namespace TheLastStand.Controller.Status;

public class DebuffStatusController : StatModifierStatusController
{
	public DebuffStatus DebuffStatus => base.Status as DebuffStatus;

	public DebuffStatusController(TheLastStand.Model.Unit.Unit unit, StatusCreationInfo statusCreationInfo)
	{
		base.Status = new DebuffStatus(this, unit, statusCreationInfo);
		ComputeStatusDestructionTime();
	}

	public override StatusController Clone()
	{
		StatusCreationInfo statusCreationInfo = default(StatusCreationInfo);
		statusCreationInfo.Source = base.Status.Source;
		statusCreationInfo.TurnsCount = base.Status.RemainingTurnsCount;
		statusCreationInfo.Stat = DebuffStatus.Stat;
		statusCreationInfo.Value = DebuffStatus.ModifierValue;
		statusCreationInfo.IsFromInjury = base.Status.IsFromInjury;
		statusCreationInfo.IsFromPerk = base.Status.IsFromPerk;
		statusCreationInfo.HideDisplayEffect = base.Status.HideDisplayEffect;
		StatusCreationInfo statusCreationInfo2 = statusCreationInfo;
		return new DebuffStatusController(base.Status.Unit, statusCreationInfo2);
	}

	public override bool CreateEffectDisplay(IDamageableController damageableController)
	{
		DebuffDisplay pooledComponent = ObjectPooler.GetPooledComponent<DebuffDisplay>("DebuffDisplay", ResourcePooler.LoadOnce<DebuffDisplay>("Prefab/Displayable Effect/UI Effect Displays/DebuffDisplay", failSilently: false), EffectManager.EffectDisplaysParent, dontSetParent: false);
		pooledComponent.Init(DebuffStatus.Stat, (int)DebuffStatus.ModifierValue);
		damageableController.AddEffectDisplay(pooledComponent);
		return true;
	}
}
