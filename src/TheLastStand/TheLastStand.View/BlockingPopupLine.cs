using System.Collections;
using TMPro;
using TPLib.Localization;
using TheLastStand.Framework.UI;
using UnityEngine;

namespace TheLastStand.View;

public class BlockingPopupLine : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI coreText;

	[SerializeField]
	private BetterButton mainButton;

	public BetterButton MainButton => mainButton;

	public void UpdateDisplayedText(string locKey, params object[] obj)
	{
		((TMP_Text)coreText).text = Localizer.Get(locKey);
		for (int i = 0; i < obj.Length; i++)
		{
			if (obj[i] is string text)
			{
				TextMeshProUGUI obj2 = coreText;
				((TMP_Text)obj2).text = ((TMP_Text)obj2).text + ((i == 0) ? " " : ", ") + text;
			}
		}
		((MonoBehaviour)this).StartCoroutine(UpdateSize());
	}

	private IEnumerator UpdateSize()
	{
		yield return null;
		((TMP_Text)coreText).rectTransform.sizeDelta = new Vector2(((TMP_Text)coreText).preferredWidth, ((TMP_Text)coreText).rectTransform.sizeDelta.y);
	}
}
