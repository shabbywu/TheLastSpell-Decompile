using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Unit.UI;

public class EnemyTypeIndicator : MonoBehaviour
{
	[SerializeField]
	private Image enemyPortraitImage;

	[SerializeField]
	private TextMeshProUGUI quantityText;

	public void Display(Sprite enemyPortrait, int quantity)
	{
		enemyPortraitImage.sprite = enemyPortrait;
		((Behaviour)quantityText).enabled = quantity > 0;
		((TMP_Text)quantityText).text = quantity.ToString();
	}
}
