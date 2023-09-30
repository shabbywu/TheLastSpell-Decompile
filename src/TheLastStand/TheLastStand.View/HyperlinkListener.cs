using System;
using TMPro;
using TPLib.Localization;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TheLastStand.View;

[RequireComponent(typeof(TMP_Text))]
public class HyperlinkListener : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler
{
	[SerializeField]
	private string openTag;

	[SerializeField]
	private string closeTag;

	[SerializeField]
	private string openTagOnHover;

	[SerializeField]
	private string closeTagOnHover;

	private TMP_Text textMesh;

	private bool pointerIsHover;

	private TMP_LinkInfo? hoverLink;

	private int previousLinkIndex;

	private TMP_Text TextMesh
	{
		get
		{
			if ((Object)(object)textMesh == (Object)null)
			{
				textMesh = ((Component)this).GetComponent<TMP_Text>();
			}
			return textMesh;
		}
	}

	private void Start()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		ForceRefresh();
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Combine((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
	}

	private void OnLocalize()
	{
		previousLinkIndex = -1;
		ForceRefresh();
	}

	private void OnDestroy()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Remove((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
	}

	private void OnDisable()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Remove((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
	}

	public void ForceRefresh()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		TextMesh.ForceMeshUpdate(true, true);
		TMP_LinkInfo[] linkInfo = TextMesh.textInfo.linkInfo;
		for (int i = 0; i < linkInfo.Length; i++)
		{
			TMP_LinkInfo linkInfo2 = TextMesh.textInfo.linkInfo[i];
			SetStyle(linkInfo2, openTag, closeTag);
			TextMesh.ForceMeshUpdate(false, false);
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		int num = TMP_TextUtilities.FindIntersectingLink(TextMesh, Vector2.op_Implicit(eventData.position), (Camera)null);
		if (num != -1)
		{
			TMP_LinkInfo val = TextMesh.textInfo.linkInfo[num];
			Application.OpenURL(((TMP_LinkInfo)(ref val)).GetLinkID());
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		pointerIsHover = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		pointerIsHover = false;
	}

	private void Update()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		if (!pointerIsHover)
		{
			return;
		}
		Vector3 mousePosition = Input.mousePosition;
		int num = TMP_TextUtilities.FindIntersectingLink(TextMesh, mousePosition, (Camera)null);
		if (num != -1 && num != previousLinkIndex)
		{
			if (previousLinkIndex != -1 && num != previousLinkIndex && hoverLink.HasValue)
			{
				TMP_LinkInfo linkInfo = TextMesh.textInfo.linkInfo[previousLinkIndex];
				RemoveStyle(linkInfo, openTagOnHover, closeTagOnHover);
				SetStyle(linkInfo, openTag, closeTag);
			}
			TMP_LinkInfo val = TextMesh.textInfo.linkInfo[num];
			RemoveStyle(val, openTag, closeTag);
			SetStyle(val, openTagOnHover, closeTagOnHover);
			hoverLink = val;
			TextMesh.ForceMeshUpdate(false, false);
		}
		else if (num == -1 && previousLinkIndex != -1 && hoverLink.HasValue)
		{
			TMP_LinkInfo val2 = TextMesh.textInfo.linkInfo[previousLinkIndex];
			if (hoverLink.Value.hashCode == val2.hashCode)
			{
				RemoveStyle(val2, openTagOnHover, closeTagOnHover);
				SetStyle(val2, openTag, closeTag);
				hoverLink = null;
				TextMesh.ForceMeshUpdate(false, false);
			}
		}
		previousLinkIndex = num;
	}

	private void SetStyle(TMP_LinkInfo linkInfo, string openTg, string closeTg)
	{
		string linkText = ((TMP_LinkInfo)(ref linkInfo)).GetLinkText();
		string newValue = openTg + linkText + closeTg;
		TextMesh.text = TextMesh.text.Replace(linkText, newValue);
	}

	private void RemoveStyle(TMP_LinkInfo linkInfo, string openTg, string closeTg)
	{
		string linkText = ((TMP_LinkInfo)(ref linkInfo)).GetLinkText();
		string oldValue = openTg + linkText + closeTg;
		TextMesh.text = TextMesh.text.Replace(oldValue, linkText);
	}
}
