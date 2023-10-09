using System;
using UnityEngine.Events;

namespace TheLastStand.Framework.UI;

[Serializable]
public class StateEvent : UnityEvent<BetterToggleGauge.E_BetterToggleGaugeState>
{
}
