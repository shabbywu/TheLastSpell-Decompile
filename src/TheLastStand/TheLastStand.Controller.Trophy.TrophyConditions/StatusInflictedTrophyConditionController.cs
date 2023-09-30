using TPLib;
using TPLib.Log;
using TheLastStand.Definition.Trophy.TrophyCondition;
using TheLastStand.Manager;
using TheLastStand.Model.Status;
using TheLastStand.Model.Trophy;

namespace TheLastStand.Controller.Trophy.TrophyConditions;

public class StatusInflictedTrophyConditionController : ValueIntHeroesTrophyConditionController
{
	public override string Name => "StatusInflicted";

	public StatusInflictedTrophyDefinition StatusInflictedTrophyDefinition => base.TrophyConditionDefinition as StatusInflictedTrophyDefinition;

	public StatusInflictedTrophyConditionController(TrophyConditionDefinition trophyConditionDefinition, TheLastStand.Model.Trophy.Trophy trophy)
		: base(trophyConditionDefinition, trophy)
	{
	}

	public override void AppendValue(params object[] args)
	{
		base.PrevCompletedState = IsCompleted;
		if (args.Length != 3)
		{
			((CLogger<TrophyManager>)TPSingleton<TrophyManager>.Instance).LogError((object)"Trophy Condition of Type StatusInflicted need only 3 arguments", (CLogLevel)0, true, true);
			return;
		}
		if (!(args[0] is int key))
		{
			((CLogger<TrophyManager>)TPSingleton<TrophyManager>.Instance).LogError((object)"Trophy Condition of Type StatusInflicted should have as first argument a string !", (CLogLevel)0, true, true);
			return;
		}
		if (!(args[1] is TheLastStand.Model.Status.Status.E_StatusType e_StatusType))
		{
			((CLogger<TrophyManager>)TPSingleton<TrophyManager>.Instance).LogError((object)"Trophy Condition of Type StatusInflicted should have as second argument a Model.Status.Status.E_StatusType !", (CLogLevel)0, true, true);
			return;
		}
		if (!(args[2] is int num))
		{
			((CLogger<TrophyManager>)TPSingleton<TrophyManager>.Instance).LogError((object)"Trophy Condition of Type StatusInflicted should have as third argument an int !", (CLogLevel)0, true, true);
			return;
		}
		if ((StatusInflictedTrophyDefinition.StatusType & e_StatusType) == e_StatusType)
		{
			if (!base.ProgressionPerUnitId.ContainsKey(key))
			{
				base.ProgressionPerUnitId.Add(key, num);
			}
			else
			{
				base.ProgressionPerUnitId[key] += num;
			}
		}
		OnValueChange();
	}

	public override void SetValue(params object[] args)
	{
		base.PrevCompletedState = IsCompleted;
		if (args.Length != 3)
		{
			((CLogger<TrophyManager>)TPSingleton<TrophyManager>.Instance).LogError((object)"Trophy Condition of Type StatusInflicted need only 3 arguments", (CLogLevel)0, true, true);
			return;
		}
		if (!(args[0] is int key))
		{
			((CLogger<TrophyManager>)TPSingleton<TrophyManager>.Instance).LogError((object)"Trophy Condition of Type StatusInflicted should have as first argument a string !", (CLogLevel)0, true, true);
			return;
		}
		if (!(args[1] is TheLastStand.Model.Status.Status.E_StatusType e_StatusType))
		{
			((CLogger<TrophyManager>)TPSingleton<TrophyManager>.Instance).LogError((object)"Trophy Condition of Type StatusInflicted should have as second argument a Model.Status.Status.E_StatusType !", (CLogLevel)0, true, true);
			return;
		}
		if (!(args[2] is int value))
		{
			((CLogger<TrophyManager>)TPSingleton<TrophyManager>.Instance).LogError((object)"Trophy Condition of Type StatusInflicted should have as third argument an int !", (CLogLevel)0, true, true);
			return;
		}
		if ((StatusInflictedTrophyDefinition.StatusType & e_StatusType) == e_StatusType)
		{
			if (!base.ProgressionPerUnitId.ContainsKey(key))
			{
				base.ProgressionPerUnitId.Add(key, value);
			}
			else
			{
				base.ProgressionPerUnitId[key] = value;
			}
		}
		OnValueChange();
	}
}
