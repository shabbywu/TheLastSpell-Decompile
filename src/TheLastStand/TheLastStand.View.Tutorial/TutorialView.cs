using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Log;
using TPLib.Yield;
using TheLastStand.Manager;
using TheLastStand.Model.Tutorial;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TheLastStand.View.Tutorial;

public class TutorialView : TPSingleton<TutorialView>
{
	[SerializeField]
	private List<TutorialPopup> tutorialPopupList = new List<TutorialPopup>();

	[SerializeField]
	private GameObject raycastBlocker;

	[SerializeField]
	private CanvasGroup canvasGroup;

	private readonly Queue<TheLastStand.Model.Tutorial.Tutorial> tutorialsToDisplay = new Queue<TheLastStand.Model.Tutorial.Tutorial>();

	private Coroutine displayCoroutine;

	public bool DisplayCoroutineRunning => displayCoroutine != null;

	public void AddTutorialsToDisplay(List<TheLastStand.Model.Tutorial.Tutorial> tutorials)
	{
		((CLogger<TutorialManager>)TPSingleton<TutorialManager>.Instance).Log((object)("Triggering Tutorial(s) " + string.Join(", ", tutorials.Select((TheLastStand.Model.Tutorial.Tutorial o) => o.TutorialDefinition.Id))), (CLogLevel)0, true, false);
		foreach (TheLastStand.Model.Tutorial.Tutorial tutorial in tutorials)
		{
			tutorialsToDisplay.Enqueue(tutorial);
		}
		if (displayCoroutine == null)
		{
			displayCoroutine = ((MonoBehaviour)this).StartCoroutine(DisplayCoroutine());
		}
	}

	protected override void Awake()
	{
		base.Awake();
		ToggleRaycastBlockers(state: false);
	}

	private IEnumerator DisplayCoroutine()
	{
		OnPopupsDisplayBegan();
		TutorialPopup popup = null;
		while (tutorialsToDisplay.Count > 0)
		{
			yield return SharedYields.WaitForEndOfFrame;
			InputManager.OnTutorialPopupOpen();
			TheLastStand.Model.Tutorial.Tutorial tutorial = tutorialsToDisplay.Dequeue();
			popup = OpenPopup(tutorial);
			if (!((Object)(object)popup == (Object)null))
			{
				TutorialPopup popupToDisplay = popup;
				yield return (object)new WaitUntil((Func<bool>)(() => !popupToDisplay.IsOpened));
				TPSingleton<TutorialManager>.Instance.OnTutorialRead(tutorial);
			}
		}
		yield return SharedYields.WaitForEndOfFrame;
		OnPopupsDisplayOver(popup);
		displayCoroutine = null;
	}

	private void OnPopupsDisplayBegan()
	{
		ToggleRaycastBlockers(state: true);
		if (InputManager.IsLastControllerJoystick)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.ExitHUDNavigationMode();
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: false);
			EventSystem.current.SetSelectedGameObject((GameObject)null);
		}
	}

	private void OnPopupsDisplayOver(TutorialPopup lastPopup)
	{
		ToggleRaycastBlockers(state: false);
		InputManager.OnTutorialPopupClosed();
		if ((Object)(object)lastPopup != (Object)null && InputManager.IsLastControllerJoystick)
		{
			if ((Object)(object)lastPopup.JoystickTargetAfterClose != (Object)null)
			{
				TPSingleton<HUDJoystickNavigationManager>.Instance.OpenHUDNavigationMode();
				TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(lastPopup.JoystickTargetAfterClose.GetSelectionInfo());
			}
			if ((Object)(object)lastPopup.SelectableAfterClose != (Object)null)
			{
				((MonoBehaviour)this).StartCoroutine(RedirectSelectionEndOfFrame(lastPopup.SelectableAfterClose));
			}
		}
	}

	private TutorialPopup OpenPopup(TheLastStand.Model.Tutorial.Tutorial tutorial)
	{
		((CLogger<TutorialManager>)TPSingleton<TutorialManager>.Instance).Log((object)("Opening TutorialPopup " + tutorial.TutorialDefinition.Id + "."), (Object)(object)this, (CLogLevel)1, false, false);
		TutorialPopup tutorialPopup = tutorialPopupList.FirstOrDefault((TutorialPopup popup) => popup.Id == tutorial.TutorialDefinition.Id);
		if ((Object)(object)tutorialPopup == (Object)null)
		{
			((CLogger<TutorialManager>)TPSingleton<TutorialManager>.Instance).LogWarning((object)("Missing TutorialPopup for Id " + tutorial.TutorialDefinition.Id + " -> skipping it."), (CLogLevel)1, true, false);
			return null;
		}
		tutorialPopup.Open(tutorial);
		((Component)tutorialPopup).transform.SetParent(((Component)this).transform);
		return tutorialPopup;
	}

	private IEnumerator RedirectSelectionEndOfFrame(Selectable selectable)
	{
		yield return SharedYields.WaitForEndOfFrame;
		EventSystem.current.SetSelectedGameObject(((Component)selectable).gameObject);
	}

	private void ToggleRaycastBlockers(bool state)
	{
		canvasGroup.blocksRaycasts = state;
		if ((Object)(object)raycastBlocker != (Object)null)
		{
			raycastBlocker.SetActive(state);
		}
	}
}
