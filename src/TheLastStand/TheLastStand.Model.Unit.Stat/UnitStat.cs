using TheLastStand.Controller.Unit.Stat;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.Serialization;
using TheLastStand.Serialization.Unit;
using UnityEngine;

namespace TheLastStand.Model.Unit.Stat;

public class UnitStat : ISerializable, IDeserializable
{
	public static readonly UnitStatDefinition.E_Stat[] NullifiedWhenStunned = new UnitStatDefinition.E_Stat[3]
	{
		UnitStatDefinition.E_Stat.ActionPoints,
		UnitStatDefinition.E_Stat.MovePoints,
		UnitStatDefinition.E_Stat.Dodge
	};

	public bool IsChildStat => ParentStat != null;

	public bool IsParentStat => ChildStat != null;

	public Vector2 BaseBoundaries => ComputeBaseBoundaries();

	public Vector2 Boundaries { get; private set; }

	public UnitStat ChildStat { get; set; }

	public UnitStat ParentStat { get; set; }

	public UnitStatDefinition.E_Stat StatId { get; private set; }

	public UnitStatsController UnitStatsController { get; private set; }

	public virtual float Base { get; set; }

	public virtual float BaseWithInjuryMultiplier
	{
		get
		{
			if (InjuriesValueMultiplier != InjuryDefinition.E_ValueMultiplier.Half)
			{
				return Mathf.Round(Base * InjuryDefinition.Multipliers[InjuriesValueMultiplier]);
			}
			return Mathf.Floor(Base * InjuryDefinition.Multipliers[InjuriesValueMultiplier]);
		}
	}

	public float Injuries { get; set; }

	public float InjuriesStatuses { get; set; }

	public InjuryDefinition.E_ValueMultiplier InjuriesValueMultiplier { get; set; }

	public float Status { get; set; }

	public float InjuryMultiplierLoss => BaseWithInjuryMultiplier - Base;

	public virtual float Final
	{
		get
		{
			if (ShouldNullifyStat())
			{
				return 0f;
			}
			return BaseWithInjuryMultiplier + Status + Injuries + InjuriesStatuses;
		}
	}

	public virtual float FinalClamped => ClampStatValue(Final);

	public UnitStat(UnitStatsController statsController, UnitStatDefinition.E_Stat id, Vector2 boundaries)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		UnitStatsController = statsController;
		StatId = id;
		Boundaries = boundaries;
	}

	public float ClampStatValue(float baseValue)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		float num = TPHelpers.Clamp(baseValue, Boundaries);
		if (ParentStat != null)
		{
			num = Mathf.Min(num, ParentStat.FinalClamped);
		}
		return num;
	}

	public float FinalClampedStatValueWithModifier(float modifier)
	{
		return ClampStatValue(Final + modifier);
	}

	protected virtual Vector2 ComputeBaseBoundaries()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (ParentStat == null)
		{
			return Boundaries;
		}
		Vector2 boundaries = Boundaries;
		boundaries.y = Mathf.Min(ParentStat.FinalClamped, boundaries.y);
		return boundaries;
	}

	protected bool ShouldNullifyStat()
	{
		if (UnitStatsController.UnitStats.Unit.IsStunned)
		{
			for (int num = NullifiedWhenStunned.Length - 1; num >= 0; num--)
			{
				if (NullifiedWhenStunned[num] == StatId)
				{
					return true;
				}
			}
		}
		return false;
	}

	public virtual void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		SerializedUnitStat serializedUnitStat = container as SerializedUnitStat;
		StatId = serializedUnitStat.StatId;
		Base = serializedUnitStat.Base;
	}

	public virtual ISerializedData Serialize()
	{
		return new SerializedUnitStat
		{
			StatId = StatId,
			Base = Base
		};
	}
}
