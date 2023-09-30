using TheLastStand.Definition.Item;
using TheLastStand.View.MetaShops;
using TheLastStand.View.Tooltip;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Item;

public class ItemIcon : OraculumUnlockIcon
{
	[SerializeField]
	private Image background;

	[SerializeField]
	private Image foreground;

	[SerializeField]
	private ItemTooltipDisplayer itemTooltipDisplayer;

	[SerializeField]
	private Color darkShopBackgroundColor = Color.white;

	[SerializeField]
	private Color lightShopBackgroundColor = Color.white;

	public void Init(ItemDefinition itemDefinition, MetaUpgradeLineView containerUpgrade, ItemTooltip itemTooltip, bool isLightShop)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		background.sprite = ItemView.GetUiSprite(itemDefinition.Id, isBG: true);
		foreground.sprite = ItemView.GetUiSprite(itemDefinition.Id);
		((Graphic)background).color = (isLightShop ? lightShopBackgroundColor : darkShopBackgroundColor);
		itemTooltipDisplayer.Init(itemDefinition, itemTooltip, isLightShop);
		SetMetaUpgrade(containerUpgrade);
	}

	private void OnDisable()
	{
		if (itemTooltipDisplayer.Displayed)
		{
			itemTooltipDisplayer.HideTooltip();
		}
	}
}
