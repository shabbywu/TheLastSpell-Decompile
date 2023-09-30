using TheLastStand.Definition.Unit.Perk.PerkDataCondition;
using TheLastStand.Framework.ExpressionInterpreter;

namespace TheLastStand.Model.Unit.Perk.PerkDataCondition;

public class IsTrueDataCondition : APerkDataCondition
{
	public IsTrueDataConditionDefinition IsTrueDataConditionDefinition => base.PerkDataConditionDefinition as IsTrueDataConditionDefinition;

	public IsTrueDataCondition(IsTrueDataConditionDefinition aPerkDataConditionDefinition, Perk perk)
		: base(aPerkDataConditionDefinition, perk)
	{
	}

	public override bool IsValid()
	{
		return IsTrueDataConditionDefinition.ValueExpression.EvalToBool((InterpreterContext)(object)base.Perk);
	}
}
