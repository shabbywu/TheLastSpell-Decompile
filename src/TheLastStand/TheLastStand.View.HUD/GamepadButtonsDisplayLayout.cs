using System;
using System.Collections.Generic;
using System.Linq;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.HUD;

public class GamepadButtonsDisplayLayout : MonoBehaviour
{
	[SerializeField]
	private Image gamepadInputDisplayPrefab;

	[SerializeField]
	private LayoutGroup gamepadInputDisplaysLayoutGroup;

	[SerializeField]
	private float heightMultiplier = 10f;

	[HideInInspector]
	public Vector3 TargetPosition;

	private Camera mainCamera;

	private RectTransform rectTransform;

	private E_GamepadButtonType[] gamepadButtonTypesValues;

	private readonly List<Image> gamepadInputDisplays = new List<Image>();

	public void Show()
	{
		((Component)this).gameObject.SetActive(true);
	}

	public void Hide()
	{
		((Component)this).gameObject.SetActive(false);
	}

	public void Init()
	{
		mainCamera = Camera.main;
		ref RectTransform reference = ref rectTransform;
		Transform transform = ((Component)this).transform;
		reference = (RectTransform)(object)((transform is RectTransform) ? transform : null);
		gamepadButtonTypesValues = Enum.GetValues(typeof(E_GamepadButtonType)).Cast<E_GamepadButtonType>().ToArray();
	}

	public void ToggleLayoutGroup(bool state)
	{
		((Behaviour)gamepadInputDisplaysLayoutGroup).enabled = state;
	}

	public void RefreshGamepadInputDisplays(E_GamepadButtonType gamepadButtonTypes)
	{
		for (int num = gamepadInputDisplays.Count - 1; num >= 0; num--)
		{
			((Component)gamepadInputDisplays[num]).gameObject.SetActive(false);
		}
		int num2 = 0;
		E_GamepadButtonType[] array = gamepadButtonTypesValues;
		foreach (E_GamepadButtonType e_GamepadButtonType in array)
		{
			if (e_GamepadButtonType != 0 && (gamepadButtonTypes & e_GamepadButtonType) != 0)
			{
				if (num2 + 1 > gamepadInputDisplays.Count)
				{
					AddInputDisplay();
				}
				Image obj = gamepadInputDisplays[num2];
				GamepadButtonsSet setForButtonType = InputManager.JoystickConfig.GamepadButtonsSetsTable.GetSetForButtonType(e_GamepadButtonType);
				obj.sprite = setForButtonType.GetIcon();
				((Component)obj).gameObject.SetActive(true);
				num2++;
			}
		}
	}

	private void AddInputDisplay()
	{
		Image item = Object.Instantiate<Image>(gamepadInputDisplayPrefab, ((Component)gamepadInputDisplaysLayoutGroup).transform);
		gamepadInputDisplays.Add(item);
	}

	private void Update()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = mainCamera.WorldToViewportPoint(TargetPosition);
		if (val.y < 0f)
		{
			float num = heightMultiplier * ((float)Screen.height / 1080f);
			rectTransform.anchoredPosition = new Vector2(0f, (0f - val.y) * num);
		}
		else
		{
			rectTransform.anchoredPosition = Vector2.zero;
		}
	}
}
