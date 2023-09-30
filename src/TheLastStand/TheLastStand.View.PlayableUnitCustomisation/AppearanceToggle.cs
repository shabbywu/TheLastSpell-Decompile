using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using DG.Tweening;
using PortraitAPI;
using Sirenix.OdinInspector;
using TMPro;
using TPLib;
using TheLastStand.Framework.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.PlayableUnitCustomisation;

public class AppearanceToggle : SerializedMonoBehaviour
{
	[Serializable]
	public class ToggleState
	{
		[SerializeField]
		private BetterButton button;

		[SerializeField]
		private Vector2 selectorPosition = Vector2.zero;

		public BetterButton Button => button;

		public Vector2 SelectorPosition => selectorPosition;
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct GenderComparer : IEqualityComparer<E_Gender>
	{
		public bool Equals(E_Gender x, E_Gender y)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return x == y;
		}

		public int GetHashCode(E_Gender obj)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Expected I4, but got Unknown
			return (int)obj;
		}
	}

	[SerializeField]
	private Dictionary<E_Gender, ToggleState> statesByGender = new Dictionary<E_Gender, ToggleState>(default(GenderComparer));

	[SerializeField]
	private RectTransform selectorRectTransform;

	[SerializeField]
	private Color highlightedColor = Color.black;

	[SerializeField]
	private Color normalColor = Color.black;

	public GenderEvent OnAppearanceChanged = new GenderEvent();

	public E_Gender CurrentGender { get; private set; }

	public void SelectState(ToggleState value)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		if (CurrentGender == statesByGender.First((KeyValuePair<E_Gender, ToggleState> x) => x.Value == value).Key)
		{
			return;
		}
		DOTweenModuleUI.DOAnchorPos(selectorRectTransform, value.SelectorPosition, 0.2f, false);
		CurrentGender = statesByGender.First((KeyValuePair<E_Gender, ToggleState> x) => x.Value == value).Key;
		((UnityEvent<E_Gender>)OnAppearanceChanged)?.Invoke(CurrentGender);
		foreach (KeyValuePair<E_Gender, ToggleState> item in statesByGender)
		{
			Unhighlight(item.Value);
		}
	}

	public void SelectStateWithoutNotify(ToggleState value)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		if (CurrentGender == statesByGender.First((KeyValuePair<E_Gender, ToggleState> x) => x.Value == value).Key)
		{
			return;
		}
		DOTweenModuleUI.DOAnchorPos(selectorRectTransform, value.SelectorPosition, 0.2f, false);
		CurrentGender = statesByGender.First((KeyValuePair<E_Gender, ToggleState> x) => x.Value == value).Key;
		TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentGender = CurrentGender;
		foreach (KeyValuePair<E_Gender, ToggleState> item in statesByGender)
		{
			Unhighlight(item.Value);
		}
	}

	public void SelectState(E_Gender e_Gender)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (statesByGender.ContainsKey(e_Gender) && e_Gender != CurrentGender)
		{
			SelectState(statesByGender[e_Gender]);
		}
	}

	public void SelectStateWithoutNotify(E_Gender gender)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (statesByGender.ContainsKey(gender) && gender != CurrentGender)
		{
			SelectStateWithoutNotify(statesByGender[gender]);
		}
	}

	private void Start()
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Expected O, but got Unknown
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Expected O, but got Unknown
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		foreach (KeyValuePair<E_Gender, ToggleState> item in statesByGender)
		{
			item.Value.Button.OnPointerEnterEvent.AddListener((UnityAction)delegate
			{
				Highlight(item.Value);
			});
			item.Value.Button.OnPointerExitEvent.AddListener((UnityAction)delegate
			{
				Unhighlight(item.Value);
			});
			((UnityEvent)((Button)item.Value.Button).onClick).AddListener((UnityAction)delegate
			{
				SelectState(item.Value);
			});
		}
		CurrentGender = (E_Gender)1;
		foreach (KeyValuePair<E_Gender, ToggleState> item2 in statesByGender)
		{
			if (item2.Key != CurrentGender)
			{
				Unhighlight(item2.Value);
			}
		}
	}

	private void Unhighlight(ToggleState value)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		TextMeshProUGUI component = ((Component)((Component)value.Button).transform.GetChild(0)).GetComponent<TextMeshProUGUI>();
		KeyValuePair<E_Gender, ToggleState> keyValuePair = statesByGender.First((KeyValuePair<E_Gender, ToggleState> x) => x.Value == value);
		((Graphic)component).color = (Color)((CurrentGender == keyValuePair.Key) ? normalColor : new Color(normalColor.r, normalColor.g, normalColor.b, 0.35f));
	}

	private void Highlight(ToggleState value)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		TextMeshProUGUI component = ((Component)((Component)value.Button).transform.GetChild(0)).GetComponent<TextMeshProUGUI>();
		KeyValuePair<E_Gender, ToggleState> keyValuePair = statesByGender.First((KeyValuePair<E_Gender, ToggleState> x) => x.Value == value);
		if (CurrentGender != keyValuePair.Key)
		{
			((Graphic)component).color = highlightedColor;
		}
	}
}
