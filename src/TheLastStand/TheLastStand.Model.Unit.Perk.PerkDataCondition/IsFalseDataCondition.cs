using TheLastStand.Definition.Unit.Perk.PerkDataCondition;
using TheLastStand.Framework.ExpressionInterpreter;

namespace TheLastStand.Model.Unit.Perk.PerkDataCondition;

public class IsFalseDataCondition : APerkDataCondition
{
	public IsFalseDataConditionDefinition IsFalseDataConditionDefinition => base.PerkDataConditionDefinition as IsFalseDataConditionDefinition;

	public IsFalseDataCondition(IsFalseDataConditionDefinition aPerkDataConditionDefinition, Perk perk)
		: base(aPerkDataConditionDefinition, perk)
	{
	}

	public override bool IsValid()
	{
		return !IsFalseDataConditionDefinition.ValueExpression.EvalToBool((InterpreterContext)(object)base.Perk);
	}
}
