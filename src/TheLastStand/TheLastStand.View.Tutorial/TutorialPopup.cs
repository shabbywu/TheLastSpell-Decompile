using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Localization.Fonts;
using TPLib.Log;
using TheLastStand.Database;
using TheLastStand.Manager;
using TheLastStand.Manager.Sound;
using TheLastStand.Model.Tutorial;
using TheLastStand.View.HUD;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Tutorial;

public class TutorialPopup : MonoBehaviour
{
	public enum ConfirmButtonType
	{
		Okay,
		Next
	}

	[Serializable]
	private struct ImagesRow
	{
		public GameObject container;

		public List<Image> images;
	}

	public static class Constants
	{
		public const string PopupConfirmationOkay = "TutorialPopupConfirmation_OK";

		public const string PopupConfirmationNext = "TutorialPopupConfirmation_Next";
	}

	[SerializeField]
	private string id = string.Empty;

	[SerializeField]
	private float baseVerticalSize = 72f;

	[SerializeField]
	private RectTransform rectTransform;

	[SerializeField]
	private RectTransform backgroundTransform;

	[SerializeField]
	private VerticalLayoutGroup verticalLayoutGroup;

	[SerializeField]
	private RectTransform contentTransform;

	[SerializeField]
	private RectTransform middleBackgroundTransform;

	[SerializeField]
	private List<GameObject> middleBackgroundList;

	[SerializeField]
	private List<ImagesRow> imagesRows;

	[SerializeField]
	private TextMeshProUGUI confirmButtonText;

	[SerializeField]
	private ConfirmButtonType confirmButtonType;

	[SerializeField]
	private HUDJoystickTarget joystickTargetAfterClose;

	[SerializeField]
	private Selectable selectableAfterClose;

	[SerializeField]
	private AudioClip openAudioClip;

	private List<LocalizedFont> fontList = new List<LocalizedFont>();

	private List<TextMeshProUGUI> textList = new List<TextMeshProUGUI>();

	private TheLastStand.Model.Tutorial.Tutorial tutorial;

	public string Id => id;

	public bool IsOpened => ((Component)this).gameObject.activeSelf;

	public HUDJoystickTarget JoystickTargetAfterClose => joystickTargetAfterClose;

	public Selectable SelectableAfterClose => selectableAfterClose;

	public void Close()
	{
		DisableContents();
		((Component)this).gameObject.SetActive(false);
	}

	public void Open(TheLastStand.Model.Tutorial.Tutorial tutorial)
	{
		SoundManager.PlayAudioClip(openAudioClip);
		this.tutorial = tutorial;
		((Component)this).gameObject.SetActive(true);
		Refresh();
	}

	public void SetSelectableAfterClose(Selectable selectable)
	{
		selectableAfterClose = selectable;
	}

	private void AutoResize()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		LayoutRebuilder.ForceRebuildLayoutImmediate(contentTransform);
		((LayoutGroup)verticalLayoutGroup).childAlignment = (TextAnchor)((rectTransform.pivot.y == 0f) ? 7 : ((rectTransform.pivot.y == 1f) ? 1 : 4));
		int i;
		for (i = middleBackgroundList.Count((GameObject background) => background.activeSelf); (contentTransform.sizeDelta.y - baseVerticalSize) / middleBackgroundTransform.sizeDelta.y > (float)i && i < middleBackgroundList.Count - 1; i++)
		{
			middleBackgroundList.First((GameObject background) => !background.activeSelf).SetActive(true);
		}
		while ((contentTransform.sizeDelta.y - baseVerticalSize) / middleBackgroundTransform.sizeDelta.y <= (float)(i - 1) && i > 0)
		{
			middleBackgroundList.First((GameObject background) => background.activeSelf).SetActive(false);
			i--;
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(backgroundTransform);
		RectTransform obj = rectTransform;
		Rect rect = backgroundTransform.rect;
		obj.SetSizeWithCurrentAnchors((Axis)1, ((Rect)(ref rect)).height);
		rectTransform.ForceUpdateRectTransforms();
	}

	private void Awake()
	{
		fontList.AddRange(((Component)((Component)this).transform).GetComponentsInChildren<LocalizedFont>(true));
		textList.AddRange(((Component)contentTransform).GetComponentsInChildren<TextMeshProUGUI>(true));
		if (string.IsNullOrEmpty(Id))
		{
			((CLogger<TutorialManager>)TPSingleton<TutorialManager>.Instance).LogError((object)"Missing Id for a TutorialPopup instance! (Click on log to select popup)", (Object)(object)((Component)this).gameObject, (CLogLevel)1, true, true);
		}
	}

	private void DisableContents()
	{
		foreach (TextMeshProUGUI text in textList)
		{
			((Component)text).gameObject.SetActive(false);
		}
		foreach (ImagesRow imagesRow in imagesRows)
		{
			imagesRow.images.ForEach(delegate(Image image)
			{
				((Component)image).gameObject.SetActive(false);
			});
			imagesRow.container.SetActive(false);
		}
	}

	private void DisplayImages()
	{
		if (!InputManager.IsLastControllerJoystick)
		{
			return;
		}
		for (int i = 0; i < imagesRows.Count; i++)
		{
			if (!TPSingleton<TutorialDatabase>.Instance.TutorialSpritesSetsTable.Table.ContainsKey(Id))
			{
				break;
			}
			List<Sprite> sprites = TPSingleton<TutorialDatabase>.Instance.TutorialSpritesSetsTable.Table[Id].GetSprites(i);
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
			imagesRows[i].container.SetActive(true);
		}
	}

	private void DisplayText(string textString)
	{
		TextMeshProUGUI obj = textList.First((TextMeshProUGUI text) => !((Component)text).gameObject.activeSelf);
		((Component)obj).gameObject.SetActive(true);
		((TMP_Text)obj).text = textString;
	}

	private void Refresh()
	{
		fontList.ForEach(delegate(LocalizedFont font)
		{
			font.RefreshFont();
		});
		RefreshLocalizedTexts();
		AutoResize();
	}

	private void RefreshLocalizedTexts()
	{
		List<string> list = tutorial.LocalizeTexts();
		if (list.Count <= 0)
		{
			((CLogger<TutorialManager>)TPSingleton<TutorialManager>.Instance).LogError((object)("Localization for TutorialPopup with id : " + Id + " is not found"), (CLogLevel)2, true, true);
		}
		else
		{
			foreach (string item in list)
			{
				DisplayText(item);
			}
			DisplayImages();
		}
		TextMeshProUGUI val = confirmButtonText;
		((TMP_Text)val).text = Localizer.Get(confirmButtonType switch
		{
			ConfirmButtonType.Okay => "TutorialPopupConfirmation_OK", 
			ConfirmButtonType.Next => "TutorialPopupConfirmation_Next", 
			_ => "TutorialPopupConfirmation_OK", 
		});
	}

	private void Update()
	{
		if (InputManager.GetButtonDown(111))
		{
			Close();
		}
	}
}
