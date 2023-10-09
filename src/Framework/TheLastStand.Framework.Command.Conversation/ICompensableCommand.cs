namespace TheLastStand.Framework.Command.Conversation;

public interface ICompensableCommand : ICommand
{
	void Compensate();
}
