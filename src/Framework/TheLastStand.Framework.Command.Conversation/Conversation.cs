using System.Collections.Generic;

namespace TheLastStand.Framework.Command.Conversation;

public abstract class Conversation<C, S> : IConversation<C> where C : ICommand
{
	public bool IsRedoable { get; private set; }

	public Stack<S> RedoStack { get; private set; }

	public Stack<S> UndoStack { get; private set; }

	public Conversation(bool isRedoable = true)
	{
		IsRedoable = isRedoable;
		UndoStack = new Stack<S>();
		if (IsRedoable)
		{
			RedoStack = new Stack<S>();
		}
	}

	public virtual void Clear()
	{
		UndoStack?.Clear();
		RedoStack?.Clear();
	}

	public abstract void Execute(C command);

	public abstract C Redo();

	public abstract C Undo();
}
