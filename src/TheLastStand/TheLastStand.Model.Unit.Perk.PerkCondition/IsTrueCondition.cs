using TheLastStand.Definition.Unit.Perk.PerkCondition;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Model.Unit.Perk.PerkModule;

namespace TheLastStand.Model.Unit.Perk.PerkCondition;

public class IsTrueCondition : APerkCondition
{
	public IsTrueConditionDefinition IsTrueConditionDefinition => base.PerkConditionDefinition as IsTrueConditionDefinition;

	public IsTrueCondition(IsTrueConditionDefinition aPerkConditionDefinition, APerkModule perkModule)
		: base(aPerkConditionDefinition, perkModule)
	{
	}

	public override bool IsValid()
	{
		return IsTrueConditionDefinition.ValueExpression.EvalToBool((InterpreterContext)(object)base.PerkModule.Perk);
	}
}
