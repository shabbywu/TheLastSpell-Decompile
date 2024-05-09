using UnityEngine;

namespace TheLastStand.Model.Skill.SkillAction;

public class DamageRangeData
{
	public Vector2Int BaseDamageRange;

	public Vector2Int IsolatedDamageRange = Vector2Int.zero;

	public Vector2Int OpportunismDamageRange = Vector2Int.zero;

	public Vector2Int MomentumDamageRange = Vector2Int.zero;

	public Vector2Int PerksDamageRange = Vector2Int.zero;

	public Vector2Int ResistanceReductionRange = Vector2Int.zero;

	public Vector2Int BlockReductionRange = Vector2Int.zero;

	public Vector2Int BlockableDamageRange
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			Vector2Int damageToClamp = Vector2Int.Min(AllDamageSumRange + ResistanceReductionRange, new Vector2Int(Mathf.Abs(((Vector2Int)(ref BlockReductionRange)).x), Mathf.Abs(((Vector2Int)(ref BlockReductionRange)).y)));
			ClampDamage(ref damageToClamp);
			return damageToClamp;
		}
	}

	public Vector2Int FinalDamageRange
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			Vector2Int damageToClamp = AllDamageSumRange + ResistanceReductionRange + BlockReductionRange;
			ClampDamage(ref damageToClamp);
			return damageToClamp;
		}
	}

	private Vector2Int AllDamageSumRange => BaseDamageRange + IsolatedDamageRange + OpportunismDamageRange + MomentumDamageRange + PerksDamageRange;

	public override string ToString()
	{
		return ToString(" | ");
	}

	public string ToString(string separators)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		return $"BaseDamageRange: {BaseDamageRange}{separators}IsolatedDamageRange: {IsolatedDamageRange}{separators}OpportunismDamageRange: {OpportunismDamageRange}{separators}MomentumDamageRange: {MomentumDamageRange}{separators}PerksDamageRange: {PerksDamageRange}{separators}ResistanceReductionRange: {ResistanceReductionRange}{separators}BlockReductionRange: {BlockReductionRange}{separators}BlockableDamageRange: {BlockableDamageRange}{separators}FinalDamageRange: {FinalDamageRange}";
	}

	private void ClampDamage(ref Vector2Int damageToClamp)
	{
		((Vector2Int)(ref damageToClamp)).x = Mathf.Max(((Vector2Int)(ref damageToClamp)).x, 0);
		((Vector2Int)(ref damageToClamp)).y = Mathf.Max(((Vector2Int)(ref damageToClamp)).y, ((Vector2Int)(ref damageToClamp)).x);
	}
}
