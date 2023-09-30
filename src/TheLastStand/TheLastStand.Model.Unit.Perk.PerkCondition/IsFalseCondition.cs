using TheLastStand.Definition.Unit.Perk.PerkCondition;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Model.Unit.Perk.PerkModule;

namespace TheLastStand.Model.Unit.Perk.PerkCondition;

public class IsFalseCondition : APerkCondition
{
	public IsFalseConditionDefinition IsFalseConditionDefinition => base.PerkConditionDefinition as IsFalseConditionDefinition;

	public IsFalseCondition(IsFalseConditionDefinition aPerkConditionDefinition, APerkModule perkModule)
		: base(aPerkConditionDefinition, perkModule)
	{
	}

	public override bool IsValid()
	{
		return !IsFalseConditionDefinition.ValueExpression.EvalToBool((InterpreterContext)(object)base.PerkModule.Perk);
	}
}
