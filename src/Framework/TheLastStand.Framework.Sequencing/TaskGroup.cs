using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Events;

namespace TheLastStand.Framework.Sequencing;

public class TaskGroup
{
	private List<Task> tasks = new List<Task>();

	public TaskGroup NextTaskGroup { get; private set; }

	public UnityAction OnCompleteAction { get; set; }

	public UnityAction OnStartAction { get; set; }

	public TaskGroup PreviousTaskGroup { get; private set; }

	public TaskGroup(UnityAction onCompleteAction = null)
	{
		OnCompleteAction = onCompleteAction;
	}

	public TaskGroup(params Task[] tasks)
	{
		foreach (Task task in tasks)
		{
			this.tasks.Add(task);
			task.TaskGroup = this;
		}
	}

	public TaskGroup(List<Task> tasks, UnityAction onCompleteAction = null)
	{
		foreach (Task task in tasks)
		{
			this.tasks.Add(task);
			task.TaskGroup = this;
		}
		OnCompleteAction = onCompleteAction;
	}

	public TaskGroup Append(TaskGroup nextTaskGroup)
	{
		NextTaskGroup = nextTaskGroup;
		NextTaskGroup.PreviousTaskGroup = this;
		return nextTaskGroup;
	}

	public void AddTask(Task task)
	{
		tasks.Add(task);
		task.TaskGroup = this;
	}

	public void RemoveTask(Task task)
	{
		tasks.Remove(task);
		if (tasks.Count == 0)
		{
			Complete();
		}
	}

	public void Run()
	{
		UnityAction onStartAction = OnStartAction;
		if (onStartAction != null)
		{
			onStartAction.Invoke();
		}
		if (tasks.Count == 0)
		{
			Complete();
			return;
		}
		for (int num = tasks.Count - 1; num >= 0; num--)
		{
			tasks[num].StartTask();
		}
	}

	public async void RunAsSequence()
	{
		UnityAction onStartAction = OnStartAction;
		if (onStartAction != null)
		{
			onStartAction.Invoke();
		}
		if (tasks.Count == 0)
		{
			Complete();
			return;
		}
		int taskIndex = tasks.Count - 1;
		while (taskIndex >= 0)
		{
			tasks[taskIndex].StartTask();
			while (tasks[taskIndex].Completed)
			{
				await System.Threading.Tasks.Task.Delay(1);
			}
			int num = taskIndex - 1;
			taskIndex = num;
		}
	}

	private void Complete()
	{
		UnityAction onCompleteAction = OnCompleteAction;
		if (onCompleteAction != null)
		{
			onCompleteAction.Invoke();
		}
		if (NextTaskGroup != null)
		{
			NextTaskGroup.PreviousTaskGroup = null;
			NextTaskGroup.Run();
		}
	}
}
