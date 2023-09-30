using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Shop;

public class ShopCategoryButtonView : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI categoryNameText;

	public string Category { get; set; }

	public void Init(string category)
	{
		Category = category;
		((TMP_Text)categoryNameText).text = category;
	}

	public void ChangeButtonDisplay(string selectedCategory)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)categoryNameText).color = ((selectedCategory == Category) ? Color.white : Color.grey);
	}
}
