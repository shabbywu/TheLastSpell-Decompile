using TPLib;
using TPLib.Log;
using TheLastStand.Definition.Trophy.TrophyCondition;
using TheLastStand.Manager;
using TheLastStand.Model.Trophy;

namespace TheLastStand.Controller.Trophy.TrophyConditions;

public class HeroSurroundedByEnemiesTrophyConditionController : ValueIntHeroesTrophyConditionController
{
	public override string Name => "HeroSurroundedByEnemies";

	public HeroSurroundedByEnemiesTrophyDefinition HeroSurroundedByEnnemiesTrophyDefinition => base.TrophyConditionDefinition as HeroSurroundedByEnemiesTrophyDefinition;

	public HeroSurroundedByEnemiesTrophyConditionController(TrophyConditionDefinition trophyConditionDefinition, TheLastStand.Model.Trophy.Trophy trophy)
		: base(trophyConditionDefinition, trophy)
	{
	}

	public override void AppendValue(params object[] args)
	{
		base.PrevCompletedState = IsCompleted;
		if (args.Length != 3)
		{
			((CLogger<TrophyManager>)TPSingleton<TrophyManager>.Instance).LogError((object)"Trophy Condition of Type HeroSurroundedByEnnemies need only 3 arguments", (CLogLevel)0, true, true);
			return;
		}
		if (!(args[0] is int key))
		{
			((CLogger<TrophyManager>)TPSingleton<TrophyManager>.Instance).LogError((object)"Trophy Condition of Type HeroSurroundedByEnnemies should have as first argument a string !", (CLogLevel)0, true, true);
			return;
		}
		if (!(args[1] is int num))
		{
			((CLogger<TrophyManager>)TPSingleton<TrophyManager>.Instance).LogError((object)"Trophy Condition of Type HeroSurroundedByEnnemies should have as second argument a int !", (CLogLevel)0, true, true);
			return;
		}
		if (!(args[2] is int num2))
		{
			((CLogger<TrophyManager>)TPSingleton<TrophyManager>.Instance).LogError((object)"Trophy Condition of Type HeroSurroundedByEnnemies should have as third argument a int !", (CLogLevel)0, true, true);
			return;
		}
		if (num2 >= HeroSurroundedByEnnemiesTrophyDefinition.NumberOfEnemiesToBeSurroundedBy)
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
			((CLogger<TrophyManager>)TPSingleton<TrophyManager>.Instance).LogError((object)"Trophy Condition of Type HeroSurroundedByEnnemies need only 3 arguments", (CLogLevel)0, true, true);
			return;
		}
		if (!(args[0] is int key))
		{
			((CLogger<TrophyManager>)TPSingleton<TrophyManager>.Instance).LogError((object)"Trophy Condition of Type HeroSurroundedByEnnemies should have as first argument a string !", (CLogLevel)0, true, true);
			return;
		}
		if (!(args[1] is int value))
		{
			((CLogger<TrophyManager>)TPSingleton<TrophyManager>.Instance).LogError((object)"Trophy Condition of Type HeroSurroundedByEnnemies should have as second argument a int !", (CLogLevel)0, true, true);
			return;
		}
		if (!(args[2] is int num))
		{
			((CLogger<TrophyManager>)TPSingleton<TrophyManager>.Instance).LogError((object)"Trophy Condition of Type HeroSurroundedByEnnemies should have as third argument a int !", (CLogLevel)0, true, true);
			return;
		}
		if (num >= HeroSurroundedByEnnemiesTrophyDefinition.NumberOfEnemiesToBeSurroundedBy)
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
