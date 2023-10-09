namespace TheLastStand.Framework.Automaton;

public class StateMachine
{
	public State State { get; protected set; }

	public virtual void SetState(State newState)
	{
		State?.OnStateExit();
		State = newState;
		State?.ResetState();
		State?.OnStateEnter();
	}
}
