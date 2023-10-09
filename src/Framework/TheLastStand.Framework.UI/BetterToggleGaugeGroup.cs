using System.Collections.Generic;
using UnityEngine;

namespace TheLastStand.Framework.UI;

[DisallowMultipleComponent]
public class BetterToggleGaugeGroup : MonoBehaviour
{
	private Dictionary<int, BetterToggleGauge> toggles = new Dictionary<int, BetterToggleGauge>();

	public BetterToggleGauge CurrentToggleOn { get; set; }

	public void NotifyToggleOn(int order)
	{
		if (order != -1 && toggles.ContainsKey(order))
		{
			if ((Object)(object)CurrentToggleOn != (Object)null)
			{
				foreach (KeyValuePair<int, BetterToggleGauge> toggle in toggles)
				{
					if (toggle.Value.State == BetterToggleGauge.E_BetterToggleGaugeState.Activated)
					{
						toggle.Value.SetState(BetterToggleGauge.E_BetterToggleGaugeState.Normal);
					}
				}
				CurrentToggleOn.SetState(BetterToggleGauge.E_BetterToggleGaugeState.Normal);
			}
			CurrentToggleOn = toggles[order];
			CurrentToggleOn.SetState(BetterToggleGauge.E_BetterToggleGaugeState.Selected);
			{
				foreach (KeyValuePair<int, BetterToggleGauge> toggle2 in toggles)
				{
					if (toggle2.Key < order)
					{
						toggle2.Value.SetState(BetterToggleGauge.E_BetterToggleGaugeState.Activated);
					}
				}
				return;
			}
		}
		if (order != -1)
		{
			return;
		}
		if ((Object)(object)CurrentToggleOn != (Object)null)
		{
			foreach (KeyValuePair<int, BetterToggleGauge> toggle3 in toggles)
			{
				if (toggle3.Value.State == BetterToggleGauge.E_BetterToggleGaugeState.Activated)
				{
					toggle3.Value.SetState(BetterToggleGauge.E_BetterToggleGaugeState.Normal);
				}
			}
			CurrentToggleOn.SetState(BetterToggleGauge.E_BetterToggleGaugeState.Normal);
		}
		CurrentToggleOn = null;
	}

	public void RegisterToggle(BetterToggleGauge toggle, int order)
	{
		if (!toggles.ContainsValue(toggle) && !toggles.ContainsKey(order))
		{
			toggles.Add(order, toggle);
		}
		else
		{
			Debug.LogError((object)"Be careful ! Some toggles has the same order or you are trying to add a second time an already added toggle !");
		}
	}

	public void UnregisterToggle(int order)
	{
		if (toggles.ContainsKey(order))
		{
			toggles.Remove(order);
		}
	}
}
