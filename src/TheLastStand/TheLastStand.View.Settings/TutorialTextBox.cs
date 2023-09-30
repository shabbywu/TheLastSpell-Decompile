using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using TPLib;
using TPLib.Localization.Fonts;
using TPLib.Log;
using TheLastStand.Database;
using TheLastStand.Manager;
using TheLastStand.Model.Tutorial;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Settings;

public class TutorialTextBox : MonoBehaviour
{
	[Serializable]
	private struct ImagesRow
	{
		public List<Image> images;
	}

	[SerializeField]
	private RectTransform contentTransform;

	[SerializeField]
	private List<ImagesRow> imagesRows;

	private List<TextMeshProUGUI> textList = new List<TextMeshProUGUI>();

	private List<LocalizedFont> fontList = new List<LocalizedFont>();

	public void Refresh(TheLastStand.Model.Tutorial.Tutorial tutorial)
	{
		HideContents();
		List<string> list = tutorial.LocalizeTexts();
		if (list.Count == 0)
		{
			((CLogger<TutorialManager>)TPSingleton<TutorialManager>.Instance).LogError((object)("Localization for TutorialPopup with id : " + tutorial.TutorialDefinition.Id + " is not found"), (CLogLevel)2, true, true);
		}
		else
		{
			foreach (string item in list)
			{
				DisplayText(item);
			}
		}
		DisplayImages(tutorial);
	}

	public void Show()
	{
		Display(show: true);
	}

	public void Hide()
	{
		Display(show: false);
	}

	private void Awake()
	{
		fontList.AddRange(((Component)((Component)this).transform).GetComponentsInChildren<LocalizedFont>(true));
		textList.AddRange(((Component)contentTransform).GetComponentsInChildren<TextMeshProUGUI>(true));
	}

	private void Display(bool show)
	{
		((Component)this).gameObject.SetActive(show);
	}

	private void DisplayImages(TheLastStand.Model.Tutorial.Tutorial tutorial)
	{
		if (!InputManager.IsLastControllerJoystick)
		{
			return;
		}
		for (int i = 0; i < imagesRows.Count; i++)
		{
			if (!TPSingleton<TutorialDatabase>.Instance.TutorialSpritesSetsTable.Table.ContainsKey(tutorial.TutorialDefinition.Id))
			{
				break;
			}
			List<Sprite> sprites = TPSingleton<TutorialDatabase>.Instance.TutorialSpritesSetsTable.Table[tutorial.TutorialDefinition.Id].GetSprites(i);
			if (sprites == null)
			{
				break;
			}
			foreach (Sprite item in sprites)
			{
				Image obj = imagesRows[i].images.First((Image image) => !((Component)image).gameObject.activeSelf);
				((Component)obj).gameObject.SetActive(true);
				obj.sprite = item;
				((Graphic)obj).SetNativeSize();
			}
		}
	}

	private void DisplayText(string textString)
	{
		TextMeshProUGUI obj = textList.First((TextMeshProUGUI text) => !((Component)text).gameObject.activeSelf);
		((Component)obj).gameObject.SetActive(true);
		((TMP_Text)obj).text = textString;
	}

	private void HideContents()
	{
		textList.ForEach(delegate(TextMeshProUGUI text)
		{
			((Component)text).gameObject.SetActive(false);
		});
		imagesRows.ForEach(delegate(ImagesRow row)
		{
			row.images.ForEach(delegate(Image image)
			{
				((Component)image).gameObject.SetActive(false);
			});
		});
	}
}
