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

	public Vector2Int FinalDamageRange
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			Vector2Int result = BaseDamageRange + IsolatedDamageRange + OpportunismDamageRange + MomentumDamageRange + PerksDamageRange + ResistanceReductionRange + BlockReductionRange;
			((Vector2Int)(ref result)).x = Mathf.Max(((Vector2Int)(ref result)).x, 0);
			((Vector2Int)(ref result)).y = Mathf.Max(((Vector2Int)(ref result)).y, ((Vector2Int)(ref result)).x);
			return result;
		}
	}

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
		return $"BaseDamageRange: {BaseDamageRange}{separators}IsolatedDamageRange: {IsolatedDamageRange}{separators}OpportunismDamageRange: {OpportunismDamageRange}{separators}MomentumDamageRange: {MomentumDamageRange}{separators}PerksDamageRange: {PerksDamageRange}{separators}ResistanceReductionRange: {ResistanceReductionRange}{separators}BlockReductionRange: {BlockReductionRange}{separators}FinalDamageRange: {FinalDamageRange}";
	}
}
