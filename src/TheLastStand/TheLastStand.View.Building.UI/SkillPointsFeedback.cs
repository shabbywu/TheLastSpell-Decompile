using TMPro;
using UnityEngine;

namespace TheLastStand.View.Building.UI;

public class SkillPointsFeedback : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI text;

	public void Refresh(int skillPoints, int skillPointsTotal)
	{
		((TMP_Text)text).text = $"{skillPoints}/{skillPointsTotal}";
	}
}
