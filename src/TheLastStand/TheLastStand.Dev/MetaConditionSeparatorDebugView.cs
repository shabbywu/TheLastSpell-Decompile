using TMPro;
using UnityEngine;

namespace TheLastStand.Dev;

public class MetaConditionSeparatorDebugView : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI name;

	public void SetText(string text)
	{
		((TMP_Text)name).text = text;
	}
}
