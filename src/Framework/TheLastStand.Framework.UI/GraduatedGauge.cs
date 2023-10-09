using Sirenix.OdinInspector;
using UnityEngine;

namespace TheLastStand.Framework.UI;

public abstract class GraduatedGauge : SerializedMonoBehaviour
{
	public delegate void ValueChangedHandler(int value);

	[SerializeField]
	protected bool clearOnAwake = true;

	[SerializeField]
	[Tooltip("Does the filling act like a loop or is it blocked when full?")]
	protected bool clearOnCapacityExceeded = true;

	private int units;

	public bool IsFull => Units >= MaxUnits;

	public abstract int MaxUnits { get; }

	public int Units
	{
		get
		{
			return units;
		}
		set
		{
			units = value;
			this.ValueChangedEvent?.Invoke(value);
		}
	}

	public event ValueChangedHandler ValueChangedEvent;

	public abstract void AddUnits(int amount, bool tween = true);

	public abstract void Clear();

	public virtual void SetUnits(int amount, bool tween = true)
	{
		if (amount < 0)
		{
			Debug.LogWarning((object)"GraduatedGauge: Trying to set the units number to a value less than 0!");
			return;
		}
		if (amount > MaxUnits)
		{
			Debug.LogWarning((object)$"GraduatedGauge: Trying to set the units number to {amount} while maximum units is {MaxUnits}!");
			return;
		}
		Clear();
		AddUnits(amount, tween);
	}

	protected virtual void Awake()
	{
		if (clearOnAwake)
		{
			Clear();
		}
	}
}
