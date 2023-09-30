using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Model.Status;
using TheLastStand.Model.Unit;
using TheLastStand.View.Skill.SkillAction.UI;

namespace TheLastStand.Controller.Status;

public class StunStatusController : StatusController
{
	public StunStatus StunStatus => base.Status as StunStatus;

	public StunStatusController(TheLastStand.Model.Unit.Unit unit, StatusCreationInfo statusCreationInfo)
	{
		base.Status = new StunStatus(this, unit, statusCreationInfo);
	}

	public override bool CreateEffectDisplay(IDamageableController damageableController)
	{
		StyledKeyDisplay pooledComponent = ObjectPooler.GetPooledComponent<StyledKeyDisplay>("StyledKeyDisplay", ResourcePooler.LoadOnce<StyledKeyDisplay>("Prefab/Displayable Effect/UI Effect Displays/StyledKeyDisplay", false), EffectManager.EffectDisplaysParent, false);
		pooledComponent.Init(base.Status.StatusType);
		damageableController.AddEffectDisplay(pooledComponent);
		return true;
	}

	public override StatusController Clone()
	{
		StatusCreationInfo statusCreationInfo = default(StatusCreationInfo);
		statusCreationInfo.Source = base.Status.Source;
		statusCreationInfo.TurnsCount = base.Status.RemainingTurnsCount;
		statusCreationInfo.IsFromInjury = base.Status.IsFromInjury;
		statusCreationInfo.IsFromPerk = base.Status.IsFromPerk;
		statusCreationInfo.HideDisplayEffect = base.Status.HideDisplayEffect;
		StatusCreationInfo statusCreationInfo2 = statusCreationInfo;
		return new StunStatusController(base.Status.Unit, statusCreationInfo2);
	}

	protected override void MergeStatus(TheLastStand.Model.Status.Status otherStatus)
	{
	}
}
