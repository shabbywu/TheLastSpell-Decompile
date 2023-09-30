using System.Collections.Generic;
using TPLib;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.PlayableUnitCustomisation;

[RequireComponent(typeof(Button))]
public class RandomizerCustomizationElement : MonoBehaviour
{
	[SerializeField]
	private List<RandomizableCustomizationElement> randomizableCustomizationElements = new List<RandomizableCustomizationElement>();

	[SerializeField]
	private bool ignoreLockState;

	[SerializeField]
	private bool useWeights;

	private Button button;

	private void Awake()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		button = ((Component)this).GetComponent<Button>();
		((UnityEvent)button.onClick).AddListener(new UnityAction(RandomizeAll));
	}

	private void OnDestroy()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		((UnityEvent)button.onClick).RemoveListener(new UnityAction(RandomizeAll));
	}

	private void RandomizeAll()
	{
		for (int i = 0; i < randomizableCustomizationElements.Count; i++)
		{
			RandomizableCustomizationElement randomizableCustomizationElement = randomizableCustomizationElements[i];
			if (!randomizableCustomizationElement.IsLocked || ignoreLockState)
			{
				randomizableCustomizationElement.RandomizeValue(useWeights);
			}
		}
		TPSingleton<PlayableUnitCustomisationPanel>.Instance.RefreshPortraitParts = true;
	}
}
