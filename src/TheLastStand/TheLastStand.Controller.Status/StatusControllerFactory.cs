using System.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.Status.Immunity;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Status;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Serialization.Unit;
using UnityEngine;

namespace TheLastStand.Controller.Status;

public static class StatusControllerFactory
{
	public static ISkillCaster GetSkillCaster(StatusSourceInfo statusSourceInfo)
	{
		if (statusSourceInfo == null)
		{
			return null;
		}
		return statusSourceInfo.SourceType switch
		{
			DamageableType.Playable => TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.FirstOrDefault((PlayableUnit x) => x.RandomId == statusSourceInfo.SourceRandomId), 
			DamageableType.Enemy => TPSingleton<EnemyUnitManager>.Instance.EnemyUnits.FirstOrDefault((EnemyUnit x) => x.RandomId == statusSourceInfo.SourceRandomId), 
			DamageableType.Boss => TPSingleton<BossManager>.Instance.BossUnits.FirstOrDefault((BossUnit x) => x.RandomId == statusSourceInfo.SourceRandomId), 
			DamageableType.Building => TPSingleton<BuildingManager>.Instance.Buildings.FirstOrDefault((TheLastStand.Model.Building.Building x) => x.RandomId == statusSourceInfo.SourceRandomId)?.BattleModule, 
			_ => null, 
		};
	}

	public static TheLastStand.Model.Status.Status DeserializeStatus(SerializedUnitStatus serializedStatus, TheLastStand.Model.Unit.Unit unit)
	{
		StatusCreationInfo statusCreationInfo = default(StatusCreationInfo);
		statusCreationInfo.TurnsCount = serializedStatus.RemainingTurns;
		statusCreationInfo.Stat = serializedStatus.Stat;
		statusCreationInfo.Value = serializedStatus.Value;
		statusCreationInfo.IsFromInjury = serializedStatus.FromInjury;
		statusCreationInfo.IsFromPerk = serializedStatus.FromPerk;
		statusCreationInfo.DelayedSourceInfo = serializedStatus.StatusSourceInfo;
		StatusCreationInfo statusCreationInfo2 = statusCreationInfo;
		object obj = serializedStatus.Type switch
		{
			TheLastStand.Model.Status.Status.E_StatusType.Poison => new PoisonStatusController(unit, statusCreationInfo2).Status, 
			TheLastStand.Model.Status.Status.E_StatusType.Stun => new StunStatusController(unit, statusCreationInfo2).Status, 
			TheLastStand.Model.Status.Status.E_StatusType.Buff => new BuffStatusController(unit, statusCreationInfo2).Status, 
			TheLastStand.Model.Status.Status.E_StatusType.Debuff => new DebuffStatusController(unit, statusCreationInfo2).Status, 
			TheLastStand.Model.Status.Status.E_StatusType.Contagion => new ContagionStatusController(unit, statusCreationInfo2).Status, 
			TheLastStand.Model.Status.Status.E_StatusType.Charged => new ChargedStatusController(unit, statusCreationInfo2).Status, 
			TheLastStand.Model.Status.Status.E_StatusType.PoisonImmunity => new ImmunityStatusController(unit, statusCreationInfo2, serializedStatus.Type).Status, 
			TheLastStand.Model.Status.Status.E_StatusType.StunImmunity => new ImmunityStatusController(unit, statusCreationInfo2, serializedStatus.Type).Status, 
			TheLastStand.Model.Status.Status.E_StatusType.DebuffImmunity => new ImmunityStatusController(unit, statusCreationInfo2, serializedStatus.Type).Status, 
			TheLastStand.Model.Status.Status.E_StatusType.ContagionImmunity => new ImmunityStatusController(unit, statusCreationInfo2, serializedStatus.Type).Status, 
			TheLastStand.Model.Status.Status.E_StatusType.AllNegativeImmunity => new ImmunityStatusController(unit, statusCreationInfo2, serializedStatus.Type).Status, 
			_ => null, 
		};
		if (obj == null)
		{
			CLoggerManager.Log((object)$"No case corresponding to specified Status type! {serializedStatus.Type}", (LogType)0, (CLogLevel)2, true, "StatusControllerFactory", false);
		}
		return (TheLastStand.Model.Status.Status)obj;
	}
}
