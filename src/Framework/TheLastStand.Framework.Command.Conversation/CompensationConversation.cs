namespace TheLastStand.Framework.Command.Conversation;

public class CompensationConversation : Conversation<ICompensableCommand, ICompensableCommand>
{
	public CompensationConversation(bool isRedoable = true)
		: base(isRedoable)
	{
	}

	public override void Execute(ICompensableCommand command)
	{
		if (command.Execute())
		{
			base.UndoStack.Push(command);
			if (base.IsRedoable)
			{
				base.RedoStack.Clear();
			}
		}
	}

	public override ICompensableCommand Redo()
	{
		if (!base.IsRedoable)
		{
			return null;
		}
		if (base.RedoStack.Count == 0)
		{
			return null;
		}
		ICompensableCommand compensableCommand = base.RedoStack.Pop();
		if (compensableCommand == null)
		{
			return compensableCommand;
		}
		compensableCommand.Execute();
		base.UndoStack.Push(compensableCommand);
		return compensableCommand;
	}

	public override ICompensableCommand Undo()
	{
		if (base.UndoStack.Count == 0)
		{
			return null;
		}
		ICompensableCommand compensableCommand = base.UndoStack.Pop();
		if (compensableCommand == null)
		{
			return compensableCommand;
		}
		compensableCommand.Compensate();
		if (base.IsRedoable)
		{
			base.RedoStack.Push(compensableCommand);
		}
		return compensableCommand;
	}
}
