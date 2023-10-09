using UnityEngine.Events;

namespace TheLastStand.Framework.Sequencing;

public abstract class Task
{
	public bool Completed { get; set; }

	public UnityAction OnCompleteAction { get; set; }

	public UnityAction OnStartAction { get; set; }

	public TaskGroup TaskGroup { get; set; }

	public virtual void StartTask()
	{
		UnityAction onStartAction = OnStartAction;
		if (onStartAction != null)
		{
			onStartAction.Invoke();
		}
	}

	public override string ToString()
	{
		return GetType().ToString();
	}

	protected virtual void Complete()
	{
		UnityAction onCompleteAction = OnCompleteAction;
		if (onCompleteAction != null)
		{
			onCompleteAction.Invoke();
		}
		if (TaskGroup != null)
		{
			TaskGroup.RemoveTask(this);
		}
		Completed = true;
	}
}
