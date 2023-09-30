using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Model.Status;
using TheLastStand.Model.Unit;
using TheLastStand.View.Skill.SkillAction.UI;

namespace TheLastStand.Controller.Status;

public class BuffStatusController : StatModifierStatusController
{
	public BuffStatus BuffStatus => base.Status as BuffStatus;

	public BuffStatusController(TheLastStand.Model.Unit.Unit unit, StatusCreationInfo statusCreationInfo)
	{
		base.Status = new BuffStatus(this, unit, statusCreationInfo);
		ComputeStatusDestructionTime();
	}

	public override StatusController Clone()
	{
		StatusCreationInfo statusCreationInfo = default(StatusCreationInfo);
		statusCreationInfo.Source = base.Status.Source;
		statusCreationInfo.TurnsCount = base.Status.RemainingTurnsCount;
		statusCreationInfo.Stat = BuffStatus.Stat;
		statusCreationInfo.Value = BuffStatus.ModifierValue;
		statusCreationInfo.IsFromInjury = base.Status.IsFromInjury;
		statusCreationInfo.IsFromPerk = base.Status.IsFromPerk;
		statusCreationInfo.HideDisplayEffect = base.Status.HideDisplayEffect;
		StatusCreationInfo statusCreationInfo2 = statusCreationInfo;
		return new BuffStatusController(base.Status.Unit, statusCreationInfo2);
	}

	public override bool CreateEffectDisplay(IDamageableController damageableController)
	{
		BuffDisplay pooledComponent = ObjectPooler.GetPooledComponent<BuffDisplay>("BuffDisplay", ResourcePooler.LoadOnce<BuffDisplay>("Prefab/Displayable Effect/UI Effect Displays/BuffDisplay", false), EffectManager.EffectDisplaysParent, false);
		pooledComponent.Init(BuffStatus.Stat, (int)BuffStatus.ModifierValue);
		damageableController.AddEffectDisplay(pooledComponent);
		return true;
	}
}
