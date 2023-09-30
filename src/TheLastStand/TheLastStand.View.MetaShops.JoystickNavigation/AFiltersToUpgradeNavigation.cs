using System.Collections;
using System.Collections.Generic;
using TPLib;
using TheLastStand.Manager;
using TheLastStand.View.HUD;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.MetaShops.JoystickNavigation;

public abstract class AFiltersToUpgradeNavigation : Selectable
{
	[SerializeField]
	private HUDJoystickTarget upgradesPanel;

	public override void OnSelect(BaseEventData eventData)
	{
		((Selectable)this).OnSelect(eventData);
		foreach (MetaUpgradeLineView sortedLine in GetSortedLines())
		{
			if (sortedLine.IsActive == true && sortedLine.IsDisplayed)
			{
				((MonoBehaviour)this).StartCoroutine(RedirectSelection(((Component)sortedLine).gameObject));
				break;
			}
		}
	}

	protected abstract List<MetaUpgradeLineView> GetSortedLines();

	private IEnumerator RedirectSelection(GameObject selected)
	{
		yield return null;
		EventSystem.current.SetSelectedGameObject(selected);
		TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(upgradesPanel.GetSelectionInfo(), updateSelection: false);
	}
}
