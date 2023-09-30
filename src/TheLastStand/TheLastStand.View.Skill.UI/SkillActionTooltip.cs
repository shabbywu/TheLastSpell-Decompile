using System;
using System.Collections.Generic;
using TPLib;
using TheLastStand.Framework.Maths;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Skill;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Perk;
using TheLastStand.View.Generic;
using TheLastStand.View.HUD.UnitManagement;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Skill.UI;

public abstract class SkillActionTooltip : TooltipBase
{
	[SerializeField]
	protected GameObject chainsGameObject;

	[SerializeField]
	protected SkillDisplay skillDisplay;

	[SerializeField]
	private VerticalLayoutGroup perksLayout;

	[SerializeField]
	private PerkIconDisplay perkIconDisplayPrefab;

	[SerializeField]
	private Vector2 layoutSpacingMinMax = new Vector2(10f, 20f);

	[SerializeField]
	private Vector2Int spacingPerksCountMinMax = new Vector2Int(3, 6);

	private List<PerkIconDisplay> perkIconDisplays = new List<PerkIconDisplay>();

	public Tile TargetTile { get; set; }

	public TheLastStand.Model.Unit.Unit TargetUnit { get; set; }

	public event Action<PerkIconDisplay> PerkIconHidden;

	public void RefreshChains()
	{
		chainsGameObject.SetActive(PlayableUnitManager.PlayableUnitTooltip.Displayed || EnemyUnitManager.IsAnyEnemyTooltipDisplayed());
	}

	public void RefreshPerks()
	{
		for (int num = perkIconDisplays.Count - 1; num >= 0; num--)
		{
			perkIconDisplays[num].Hide();
			this.PerkIconHidden?.Invoke(perkIconDisplays[num]);
		}
		if (!(skillDisplay.SkillOwner is PlayableUnit playableUnit))
		{
			((Component)perksLayout).gameObject.SetActive(false);
			return;
		}
		skillDisplay.Skill.SkillAction.SkillActionController.ResetPerkData(TargetTile, TargetTile?.Damageable);
		int num2 = 0;
		foreach (KeyValuePair<string, Perk> perk in playableUnit.Perks)
		{
			if (perk.Value.DisplayInAttackTooltip(skillDisplay.Skill.SkillAction.PerkDataContainer))
			{
				AdjustPerksPoolLength(num2);
				perkIconDisplays[num2].Display(perk.Value, greyOut: false, displayDynamicValue: false, displayCounter: false);
				perkIconDisplays[num2].DisplayHighlightSign(show: true, triggerEvent: true);
				perkIconDisplays[num2].OverrideHighlightSignCanvasSorting(state: false);
				num2++;
			}
		}
		if (num2 > 0)
		{
			((Component)perksLayout).gameObject.SetActive(true);
			((HorizontalOrVerticalLayoutGroup)perksLayout).spacing = 0f - Maths.NormalizeClamped((float)num2, (float)((Vector2Int)(ref spacingPerksCountMinMax)).x, (float)((Vector2Int)(ref spacingPerksCountMinMax)).y, layoutSpacingMinMax.x, layoutSpacingMinMax.y);
		}
		else
		{
			((Component)perksLayout).gameObject.SetActive(false);
		}
	}

	public void SetSkill(TheLastStand.Model.Skill.Skill skill, ISkillCaster skillOwner = null)
	{
		skillDisplay.Skill = skill;
		skillDisplay.SkillOwner = skillOwner;
	}

	protected override bool CanBeDisplayed()
	{
		if (skillDisplay.Skill != null)
		{
			return UIManager.DebugToggleUI != false;
		}
		return false;
	}

	protected override void RefreshContent()
	{
		RefreshChains();
		skillDisplay.Refresh();
		if (TPSingleton<GameManager>.Instance.Game.State != Game.E_State.UnitExecutingSkill)
		{
			RefreshPerks();
		}
	}

	private void AdjustPerksPoolLength(int perkIndex)
	{
		if (perkIndex > perkIconDisplays.Count - 1)
		{
			PerkIconDisplay item = Object.Instantiate<PerkIconDisplay>(perkIconDisplayPrefab, ((Component)perksLayout).transform);
			perkIconDisplays.Add(item);
		}
	}
}
