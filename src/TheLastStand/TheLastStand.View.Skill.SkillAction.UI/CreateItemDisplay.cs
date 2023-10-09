using TheLastStand.Definition.Item;
using TheLastStand.Framework;
using TheLastStand.Model.ProductionReport;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Skill.SkillAction.UI;

public class CreateItemDisplay : AppearingEffectDisplay
{
	[SerializeField]
	private Image itemDisplay;

	public void Init(ItemDefinition itemDefinition)
	{
		base.Init();
		itemDisplay.sprite = ItemView.GetUiSprite(itemDefinition.ArtId);
	}

	public void Init(ProductionItems productionItem)
	{
		base.Init();
		Sprite val = ResourcePooler.LoadOnce<Sprite>("View/Sprites/UI/ProductionReportPanel/Production_" + productionItem.ProductionBuildingDefinition.Id, failSilently: false);
		if ((Object)(object)val != (Object)null)
		{
			itemDisplay.sprite = val;
		}
	}
}
