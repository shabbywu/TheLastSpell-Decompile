namespace TheLastStand.Framework.Command.Conversation;

public interface IConversation<C> where C : ICommand
{
	void Execute(C command);

	C Redo();

	C Undo();
}
