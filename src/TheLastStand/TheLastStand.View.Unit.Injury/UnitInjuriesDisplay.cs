using System.Collections.Generic;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.Extensions;
using TheLastStand.Model.Unit;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Unit.Injury;

public class UnitInjuriesDisplay : MonoBehaviour
{
	[SerializeField]
	private RectTransform injuriesParent;

	[SerializeField]
	private UnitInjuryDisplay injuryPrefab;

	[SerializeField]
	private bool shouldSetInjuriesBoxUnRaycastable;

	[SerializeField]
	private Selectable[] selectOnUp;

	[SerializeField]
	private Selectable[] selectOnDown;

	private TheLastStand.Model.Unit.Unit currentUnit;

	private List<UnitInjuryDisplay> injuries = new List<UnitInjuryDisplay>();

	public void Refresh(TheLastStand.Model.Unit.Unit newUnit = null)
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		if (currentUnit != newUnit)
		{
			currentUnit = newUnit;
			Init();
		}
		if (currentUnit != null)
		{
			int i = 0;
			for (int count = currentUnit.UnitTemplateDefinition.InjuryDefinitions.Count; i < count; i++)
			{
				InjuryDefinition injuryDefinition = currentUnit.UnitTemplateDefinition.InjuryDefinitions[i];
				float xPos = Mathf.Ceil(currentUnit.UnitController.ComputeInjuryThresholdRatio(injuryDefinition) * injuriesParent.sizeDelta.x);
				injuries[i].Refresh(currentUnit.UnitStatsController.UnitStats.InjuryStage > i, xPos, injuryDefinition, i);
			}
		}
	}

	private void Init()
	{
		TransformExtensions.DestroyChildren((Transform)(object)injuriesParent);
		injuries.Clear();
		if (currentUnit == null)
		{
			return;
		}
		for (int i = 0; i < currentUnit.UnitTemplateDefinition.InjuryDefinitions.Count; i++)
		{
			UnitInjuryDisplay unitInjuryDisplay = Object.Instantiate<UnitInjuryDisplay>(injuryPrefab, (Transform)(object)injuriesParent);
			injuries.Add(unitInjuryDisplay);
			if (shouldSetInjuriesBoxUnRaycastable)
			{
				unitInjuryDisplay.ToggleRaycaster(state: false);
			}
			else
			{
				SelectableExtensions.SetSelectOnUp((Selectable)(object)unitInjuryDisplay.JoystickSelectable, (selectOnUp.Length != 0) ? selectOnUp[0] : null);
				SelectableExtensions.SetSelectOnDown((Selectable)(object)unitInjuryDisplay.JoystickSelectable, (selectOnDown.Length != 0) ? selectOnDown[0] : null);
			}
			if (i != currentUnit.UnitTemplateDefinition.InjuryDefinitions.Count - 1)
			{
				continue;
			}
			for (int j = 0; j < selectOnUp.Length; j++)
			{
				if ((Object)(object)selectOnUp[j] != (Object)null)
				{
					SelectableExtensions.SetSelectOnDown(selectOnUp[j], (Selectable)(object)unitInjuryDisplay.JoystickSelectable);
				}
			}
			for (int k = 0; k < selectOnDown.Length; k++)
			{
				if ((Object)(object)selectOnDown[k] != (Object)null)
				{
					SelectableExtensions.SetSelectOnUp(selectOnDown[k], (Selectable)(object)unitInjuryDisplay.JoystickSelectable);
				}
			}
		}
		for (int l = 0; l < injuries.Count; l++)
		{
			SelectableExtensions.SetMode((Selectable)(object)injuries[l].JoystickSelectable, (Mode)4);
			if (l > 0)
			{
				SelectableExtensions.SetSelectOnRight((Selectable)(object)injuries[l].JoystickSelectable, (Selectable)(object)injuries[l - 1].JoystickSelectable);
			}
			if (l < injuries.Count - 1)
			{
				SelectableExtensions.SetSelectOnLeft((Selectable)(object)injuries[l].JoystickSelectable, (Selectable)(object)injuries[l + 1].JoystickSelectable);
			}
		}
	}
}
