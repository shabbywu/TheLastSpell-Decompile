using TheLastStand.Definition.Unit.Perk;
using TheLastStand.Definition.Unit.Perk.PerkAction;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.Model.Unit.Perk.PerkAction;
using TheLastStand.Model.Unit.Perk.PerkEvent;

namespace TheLastStand.Controller.Unit.Perk.PerkAction;

public class DecreaseBufferController : APerkActionController
{
	public DecreaseBuffer DecreaseBuffer => PerkAction as DecreaseBuffer;

	public DecreaseBufferController(DecreaseBufferDefinition definition, PerkEvent pEvent)
		: base(definition, pEvent)
	{
	}

	protected override APerkAction CreateModel(APerkActionDefinition definition, PerkEvent pEvent)
	{
		return new DecreaseBuffer(definition as DecreaseBufferDefinition, this, pEvent);
	}

	public override void Trigger(PerkDataContainer data)
	{
		int num = DecreaseBuffer.DecreaseBufferDefinition.ValueExpression.EvalToInt((InterpreterContext)(object)PerkAction.PerkEvent.PerkModule.Perk);
		switch (DecreaseBuffer.DecreaseBufferDefinition.BufferIndex)
		{
		case BufferModuleDefinition.BufferIndex.Buffer:
			DecreaseBuffer.BufferModule.Buffer -= num;
			break;
		case BufferModuleDefinition.BufferIndex.Buffer2:
			DecreaseBuffer.BufferModule.Buffer2 -= num;
			break;
		case BufferModuleDefinition.BufferIndex.Buffer3:
			DecreaseBuffer.BufferModule.Buffer3 -= num;
			break;
		}
	}
}
