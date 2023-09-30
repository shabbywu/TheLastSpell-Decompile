using System.Collections;
using System.Collections.Generic;
using TPLib;
using TPLib.Log;
using TPLib.Yield;
using TheLastStand.Manager;
using TheLastStand.Model.Unit;
using TheLastStand.View.Unit;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.HUD.UnitPortraitPanel;

public class UnitPortraitsPanel : MonoBehaviour
{
	private UnitPortraitView previousPortraitCursorIsHover;

	private UnitPortraitView currentPortraitCursorIsHover;

	[SerializeField]
	private Canvas portraitsCanvas;

	[SerializeField]
	private float leftBorderWidth = 59f;

	[SerializeField]
	private float rightBorderWidth = 43f;

	[SerializeField]
	private Vector2 widthBoundaries = new Vector2(266f, 640f);

	[SerializeField]
	private RectTransform rectTransform;

	[SerializeField]
	private UnitPortraitPanel unitPortraitPanelPrefab;

	[SerializeField]
	private RectTransform unitPortraitsParent;

	[SerializeField]
	private ToggleGroup unitPortraitsToggleGroup;

	[SerializeField]
	private GameObject leftArrowGameObject;

	[SerializeField]
	private GameObject rightArrowGameObject;

	[SerializeField]
	private ScrollRect portraitsScrollRect;

	private List<UnitPortraitView> unitPortraits = new List<UnitPortraitView>();

	public Canvas PortraitsCanvas => portraitsCanvas;

	public bool CursorIsHoverPortrait => (Object)(object)currentPortraitCursorIsHover != (Object)null;

	public bool TargettedPortraitHasChanged => (Object)(object)currentPortraitCursorIsHover != (Object)(object)previousPortraitCursorIsHover;

	public UnitPortraitPanel AddPortrait(PlayableUnit playableUnit)
	{
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Expected O, but got Unknown
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Expected O, but got Unknown
		UnitPortraitPanel newUnitPortrait = Object.Instantiate<UnitPortraitPanel>(unitPortraitPanelPrefab, (Transform)(object)unitPortraitsParent);
		unitPortraits.Add(newUnitPortrait);
		newUnitPortrait.PlayableUnit = playableUnit;
		newUnitPortrait.RefreshPortrait();
		newUnitPortrait.RefreshStats();
		((UnityEvent<bool>)(object)((Toggle)newUnitPortrait.UnitPortraitToggle).onValueChanged).AddListener((UnityAction<bool>)delegate
		{
			newUnitPortrait.OnUnitPortraitClick();
		});
		newUnitPortrait.UnitPortraitToggle.OnPointerEnterEvent.AddListener((UnityAction)delegate
		{
			newUnitPortrait.OnUnitPortraitHoverEnter();
		});
		newUnitPortrait.UnitPortraitToggle.OnPointeExitEvent.AddListener((UnityAction)delegate
		{
			newUnitPortrait.OnUnitPortraitHoverExit();
		});
		unitPortraitsToggleGroup.RegisterToggle((Toggle)(object)newUnitPortrait.UnitPortraitToggle);
		((Toggle)newUnitPortrait.UnitPortraitToggle).group = unitPortraitsToggleGroup;
		((MonoBehaviour)this).StartCoroutine(RefreshSize());
		return newUnitPortrait;
	}

	public void DeselectAll()
	{
		unitPortraitsToggleGroup.SetAllTogglesOff(true);
	}

	public void Display(bool show)
	{
		((Behaviour)portraitsScrollRect).enabled = show && UIManager.DebugToggleUI != false;
		((Behaviour)portraitsCanvas).enabled = show && UIManager.DebugToggleUI != false;
	}

	public UnitPortraitView GetPortraitIsHovered()
	{
		return currentPortraitCursorIsHover;
	}

	public UnitPortraitView GetPreviousPortraitWasHovered()
	{
		return previousPortraitCursorIsHover;
	}

	public void RefreshPortraits()
	{
		for (int i = 0; i < unitPortraits.Count; i++)
		{
			unitPortraits[i].RefreshPortrait();
		}
	}

	public void RefreshPortraitsStats()
	{
		for (int i = 0; i < unitPortraits.Count; i++)
		{
			unitPortraits[i].RefreshStats();
		}
	}

	public void RemovePortrait(int unitIndex)
	{
		unitPortraitsToggleGroup.UnregisterToggle(((Component)unitPortraits[unitIndex]).GetComponent<Toggle>());
		Object.Destroy((Object)(object)((Component)unitPortraits[unitIndex]).gameObject);
		unitPortraits.RemoveAt(unitIndex);
		((MonoBehaviour)this).StartCoroutine(RefreshSize());
	}

	public IEnumerator RefreshSize()
	{
		yield return SharedYields.WaitForFrames(1);
		float num = Mathf.Clamp(leftBorderWidth + unitPortraitsParent.sizeDelta.x + rightBorderWidth, widthBoundaries.x, widthBoundaries.y);
		rectTransform.sizeDelta = new Vector2(num, rectTransform.sizeDelta.y);
		ToggleArrows();
	}

	public void SetPortraitIsHovered(UnitPortraitView unitPortraitView = null)
	{
		currentPortraitCursorIsHover = unitPortraitView;
	}

	public void ToggleSelectedUnit(int unitIndex)
	{
		((Toggle)unitPortraits[unitIndex].UnitPortraitToggle).isOn = true;
	}

	public void ToggleSelectedUnit(PlayableUnit unit)
	{
		UnitPortraitView unitPortraitView = unitPortraits.Find((UnitPortraitView o) => o.PlayableUnit == unit);
		if ((Object)(object)unitPortraitView == (Object)null)
		{
			((CLogger<UIManager>)TPSingleton<UIManager>.Instance).LogWarning((object)("Told to toggle portrait for unit " + unit.Name + ", but they have no portrait! What's up?"), (CLogLevel)1, true, false);
		}
		else
		{
			((Toggle)unitPortraitView.UnitPortraitToggle).isOn = true;
		}
	}

	public void ToggleUnselectedUnit(PlayableUnit unit)
	{
		UnitPortraitView unitPortraitView = unitPortraits.Find((UnitPortraitView o) => o.PlayableUnit == unit);
		if ((Object)(object)unitPortraitView == (Object)null)
		{
			((CLogger<UIManager>)TPSingleton<UIManager>.Instance).LogWarning((object)("Told to toggle portrait for unit " + unit.Name + ", but they have no portrait! What's up?"), (CLogLevel)1, true, false);
		}
		else
		{
			((Toggle)unitPortraitView.UnitPortraitToggle).isOn = false;
		}
	}

	private void ToggleArrows()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (leftBorderWidth + unitPortraitsParent.sizeDelta.x + rightBorderWidth > rectTransform.sizeDelta.x)
		{
			leftArrowGameObject.SetActive(true);
			rightArrowGameObject.SetActive(true);
		}
		else
		{
			leftArrowGameObject.SetActive(false);
			rightArrowGameObject.SetActive(false);
		}
	}

	private void Awake()
	{
		((Component)this).GetComponent<Canvas>().sortingOrder = 0;
	}

	private void Update()
	{
		previousPortraitCursorIsHover = currentPortraitCursorIsHover;
	}
}
