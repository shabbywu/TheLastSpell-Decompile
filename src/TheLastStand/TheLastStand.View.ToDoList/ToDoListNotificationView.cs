using System.Collections.Generic;
using TMPro;
using TPLib;
using TPLib.Localization.Fonts;
using TheLastStand.Manager;
using TheLastStand.Model;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.ToDoList;

public class ToDoListNotificationView : MonoBehaviour
{
	private const string TriggerName = "appear";

	[SerializeField]
	private Game.E_DayTurn onlyShowDuringDayTurn;

	[SerializeField]
	private bool hideIfEmpty = true;

	[SerializeField]
	private SimpleFontLocalizedParent simpleFontLocalizedParent;

	[SerializeField]
	protected List<TextMeshProUGUI> toDoTexts = new List<TextMeshProUGUI>();

	[SerializeField]
	protected RectTransform notificationRectTransform;

	[SerializeField]
	protected RectTransform notificationGroupRectTransform;

	[SerializeField]
	private VerticalLayoutGroup notificationLayoutGroup;

	[SerializeField]
	private Animator notificationAnimator;

	[SerializeField]
	protected TextMeshProUGUI toDoTextPrefab;

	protected Game.E_DayTurn OnlyShowDuringDayTurn => onlyShowDuringDayTurn;

	public void Display(bool show)
	{
		if (show)
		{
			((FontLocalizedParent)simpleFontLocalizedParent).RegisterChilds();
		}
		else
		{
			((FontLocalizedParent)simpleFontLocalizedParent).UnregisterChilds();
		}
		if (!(((Component)this).gameObject.activeInHierarchy && show))
		{
			((Component)this).gameObject.SetActive(show);
			notificationAnimator.SetTrigger("appear");
		}
	}

	public virtual void Refresh()
	{
		if (onlyShowDuringDayTurn != 0)
		{
			Display(onlyShowDuringDayTurn == TPSingleton<GameManager>.Instance.Game.DayTurn);
		}
	}

	protected virtual void CheckDisplay()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		if (hideIfEmpty)
		{
			Display(toDoTexts.Count > 0);
		}
		if (toDoTexts.Count > 0)
		{
			notificationGroupRectTransform.sizeDelta = new Vector2(notificationRectTransform.sizeDelta.x, (float)(((LayoutGroup)notificationLayoutGroup).padding.top + ((LayoutGroup)notificationLayoutGroup).padding.bottom) + (float)toDoTexts.Count * ((TMP_Text)toDoTextPrefab).rectTransform.sizeDelta.y + (float)Mathf.Max(0, toDoTexts.Count - 1) * ((HorizontalOrVerticalLayoutGroup)notificationLayoutGroup).spacing);
			notificationRectTransform.sizeDelta = notificationGroupRectTransform.sizeDelta;
		}
	}

	private void Start()
	{
		if ((Object)(object)simpleFontLocalizedParent != (Object)null)
		{
			((FontLocalizedParent)simpleFontLocalizedParent).RegisterChilds();
		}
	}
}
