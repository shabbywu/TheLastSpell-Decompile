using System.Collections.Generic;

namespace TheLastStand.Framework.Automaton;

public class PushdownAutomata : StateMachine
{
	protected const int StatesStackCapacity = 10;

	public Stack<State> PreviousStatesStack { get; private set; } = new Stack<State>(10);


	public override void SetState(State newState)
	{
		PreviousStatesStack.Push(base.State);
		base.SetState(newState);
	}
}
