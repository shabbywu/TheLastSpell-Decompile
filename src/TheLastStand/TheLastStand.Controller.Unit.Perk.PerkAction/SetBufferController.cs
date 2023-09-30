using TheLastStand.Definition.Unit.Perk;
using TheLastStand.Definition.Unit.Perk.PerkAction;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.Model.Unit.Perk.PerkAction;
using TheLastStand.Model.Unit.Perk.PerkEvent;

namespace TheLastStand.Controller.Unit.Perk.PerkAction;

public class SetBufferController : APerkActionController
{
	public SetBuffer SetBuffer => PerkAction as SetBuffer;

	public SetBufferController(SetBufferDefinition definition, PerkEvent pEvent)
		: base(definition, pEvent)
	{
	}

	protected override APerkAction CreateModel(APerkActionDefinition definition, PerkEvent pEvent)
	{
		return new SetBuffer(definition as SetBufferDefinition, this, pEvent);
	}

	public override void Trigger(PerkDataContainer data)
	{
		int num = SetBuffer.SetBufferDefinition.ValueExpression.EvalToInt((InterpreterContext)(object)PerkAction.PerkEvent.PerkModule.Perk);
		switch (SetBuffer.SetBufferDefinition.BufferIndex)
		{
		case BufferModuleDefinition.BufferIndex.Buffer:
			SetBuffer.BufferModule.Buffer = num;
			break;
		case BufferModuleDefinition.BufferIndex.Buffer2:
			SetBuffer.BufferModule.Buffer2 = num;
			break;
		case BufferModuleDefinition.BufferIndex.Buffer3:
			SetBuffer.BufferModule.Buffer3 = num;
			break;
		}
	}
}
