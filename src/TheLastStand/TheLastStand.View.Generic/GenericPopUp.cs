using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Localization.Fonts;
using TPLib.Log;
using TPLib.UI;
using TPLib.Yield;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.View.Camera;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.Generic;

public class GenericPopUp : MonoBehaviour, IOverlayUser
{
	private static GenericPopUp instance;

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private SimpleFontLocalizedParent simpleFontLocalizedParent;

	[SerializeField]
	private BetterButton confirmButton;

	[SerializeField]
	private TextMeshProUGUI coreText;

	[SerializeField]
	private HyperlinkListener hyperlinkListener;

	[SerializeField]
	private Image overlay;

	[SerializeField]
	private Image titleIcon;

	[SerializeField]
	private TextMeshProUGUI titleText;

	public static bool IsOpen => ((Behaviour)instance.Canvas).enabled;

	public ParameterizedLocalizationLine ButtonContent { get; private set; }

	public Canvas Canvas => canvas;

	public int OverlaySortingOrder => canvas.sortingOrder - 1;

	public Queue<ParameterizedLocalizationLine> Texts { get; private set; } = new Queue<ParameterizedLocalizationLine>();


	public Queue<ParameterizedLocalizationLine> Titles { get; private set; } = new Queue<ParameterizedLocalizationLine>();


	public static GenericPopUp Open(string titleLocKey, string textLocKey, Sprite titleIcon = null, string okButtonLocKey = null)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		return Open(new List<ParameterizedLocalizationLine>
		{
			new ParameterizedLocalizationLine(titleLocKey, Array.Empty<string>())
		}, new List<ParameterizedLocalizationLine>
		{
			new ParameterizedLocalizationLine(textLocKey, Array.Empty<string>())
		}, titleIcon, (okButtonLocKey == null) ? null : new ParameterizedLocalizationLine?(new ParameterizedLocalizationLine(okButtonLocKey, Array.Empty<string>())));
	}

	public static GenericPopUp Open(ParameterizedLocalizationLine titleLocKey, ParameterizedLocalizationLine textLocKey, Sprite titleIcon = null, string okButtonLocKey = null)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		return Open(new List<ParameterizedLocalizationLine> { titleLocKey }, new List<ParameterizedLocalizationLine> { textLocKey }, titleIcon, (okButtonLocKey == null) ? null : new ParameterizedLocalizationLine?(new ParameterizedLocalizationLine(okButtonLocKey, Array.Empty<string>())));
	}

	public static GenericPopUp Open(List<string> titleLocKeys, List<string> textLocKeys, List<string[]> formatsParameters, Sprite titleIcon = null, string okButtonLocKey = null)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		List<ParameterizedLocalizationLine> titleLocKeys2 = titleLocKeys.ConvertAll((Converter<string, ParameterizedLocalizationLine>)((string o) => new ParameterizedLocalizationLine(o, Array.Empty<string>())));
		List<ParameterizedLocalizationLine> list = new List<ParameterizedLocalizationLine>();
		for (int i = 0; i < textLocKeys.Count; i++)
		{
			list.Add(new ParameterizedLocalizationLine
			{
				key = textLocKeys[i],
				parameters = formatsParameters[i]
			});
		}
		return Open(titleLocKeys2, list, titleIcon, (okButtonLocKey == null) ? null : new ParameterizedLocalizationLine?(new ParameterizedLocalizationLine(okButtonLocKey, Array.Empty<string>())));
	}

	public static GenericPopUp Open(List<ParameterizedLocalizationLine> titleLocKeys, List<ParameterizedLocalizationLine> textLocKeys, Sprite titleIcon = null, string okButtonLocKey = null)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return Open(titleLocKeys, textLocKeys, titleIcon, (okButtonLocKey == null) ? null : new ParameterizedLocalizationLine?(new ParameterizedLocalizationLine(okButtonLocKey, Array.Empty<string>())));
	}

	private static GenericPopUp Open(List<ParameterizedLocalizationLine> titleLocKeys, List<ParameterizedLocalizationLine> textLocKeys, Sprite titleIcon = null, ParameterizedLocalizationLine? okButtonLocKey = null)
	{
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)instance == (Object)null)
		{
			((CLogger<UIManager>)TPSingleton<UIManager>.Instance).LogError((object)"Someone tried to open a generic popup, but there are NONE here! Please add it to the scene.", (CLogLevel)1, true, true);
			return null;
		}
		((Behaviour)instance.Canvas).enabled = true;
		instance.Titles.Clear();
		instance.Texts.Clear();
		titleLocKeys.ForEach(delegate(ParameterizedLocalizationLine o)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			instance.Titles.Enqueue(o);
		});
		textLocKeys.ForEach(delegate(ParameterizedLocalizationLine o)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			instance.Texts.Enqueue(o);
		});
		instance.titleIcon.sprite = titleIcon;
		instance.ButtonContent = (ParameterizedLocalizationLine)(((_003F?)okButtonLocKey) ?? new ParameterizedLocalizationLine("GenericPopup_Confirm", Array.Empty<string>()));
		instance.DisplayNextContent();
		CameraView.AttenuateWorldForPopupFocus((IOverlayUser)(object)instance);
		return instance;
	}

	private void Awake()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		instance = this;
		((UnityEvent)((Button)confirmButton).onClick).AddListener(new UnityAction(CloseOrNext));
	}

	private void Close()
	{
		CameraView.AttenuateWorldForPopupFocus(null);
		((Behaviour)instance.Canvas).enabled = false;
	}

	private IEnumerator CloseAtEndOfFrame()
	{
		yield return SharedYields.WaitForEndOfFrame;
		Close();
	}

	private void CloseOrNext()
	{
		if (Titles.Count == 0)
		{
			((MonoBehaviour)this).StartCoroutine(CloseAtEndOfFrame());
		}
		else
		{
			DisplayNextContent();
		}
	}

	private void DisplayNextContent()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		((Behaviour)overlay).enabled = !TPSingleton<ACameraView>.Exist();
		if (Texts.Count != 0)
		{
			((TMP_Text)coreText).text = Localizer.Get(Texts.Dequeue());
			hyperlinkListener?.ForceRefresh();
			((TMP_Text)titleText).text = Localizer.Get(Titles.Dequeue());
			confirmButton.ChangeText(Localizer.Get(ButtonContent));
			((Component)titleIcon).gameObject.SetActive((Object)(object)titleIcon.sprite != (Object)null);
			SimpleFontLocalizedParent obj = simpleFontLocalizedParent;
			if (obj != null)
			{
				((FontLocalizedParent)obj).RefreshChilds();
			}
		}
	}

	private void OnDestroy()
	{
		instance = null;
	}

	private void Update()
	{
		if (((Behaviour)instance.Canvas).enabled && (InputManager.GetButtonDown(29) || InputManager.GetButtonDown(7) || InputManager.GetButtonDown(66) || InputManager.GetButtonDown(80)))
		{
			CloseOrNext();
		}
	}
}
