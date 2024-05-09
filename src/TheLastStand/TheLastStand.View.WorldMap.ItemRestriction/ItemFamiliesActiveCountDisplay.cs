using TMPro;
using TPLib;
using TheLastStand.Definition.Item;
using TheLastStand.Model.Item.ItemRestriction;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.WorldMap.ItemRestriction;

public class ItemFamiliesActiveCountDisplay : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI activeFamiliesCountText;

	[SerializeField]
	private Image missingActiveFamiliesNotification;

	[SerializeField]
	private DataColor missingActiveFamiliesColor;

	[SerializeField]
	private DataColor requiredFamiliesMetColor;

	[SerializeField]
	private Material missingFamiliesNotificationMaterial;

	private ItemRestrictionCategoriesCollection currentRestrictionCategoriesCollection;

	private ItemDefinition.E_Category currentCategory;

	public void Init(ItemRestrictionCategoriesCollection restrictionCategoriesCollection, ItemDefinition.E_Category category)
	{
		currentRestrictionCategoriesCollection = restrictionCategoriesCollection;
		currentCategory = category;
	}

	public void Refresh()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		int activeFamiliesNb = currentRestrictionCategoriesCollection.GetActiveFamiliesNb(currentCategory);
		int requiredSelectedFamiliesNb = currentRestrictionCategoriesCollection.GetRequiredSelectedFamiliesNb(currentCategory);
		bool flag = currentRestrictionCategoriesCollection.IsCategoryCorrectlyConfigured(currentCategory);
		((Graphic)activeFamiliesCountText).color = (flag ? missingActiveFamiliesColor._Color : requiredFamiliesMetColor._Color);
		((TMP_Text)activeFamiliesCountText).text = $"{activeFamiliesNb}/{requiredSelectedFamiliesNb}";
		if ((Object)(object)missingActiveFamiliesNotification != (Object)null)
		{
			((Behaviour)missingActiveFamiliesNotification).enabled = !flag;
			((Graphic)missingActiveFamiliesNotification).material = ((!flag) ? missingFamiliesNotificationMaterial : null);
		}
	}
}
