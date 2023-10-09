namespace TheLastStand.Framework.Automaton;

public abstract class State
{
	public abstract string GetName();

	public virtual void OnStateEnter()
	{
	}

	public virtual void OnStateExit()
	{
	}

	public virtual void ResetState()
	{
	}

	public override string ToString()
	{
		return GetName();
	}
}
