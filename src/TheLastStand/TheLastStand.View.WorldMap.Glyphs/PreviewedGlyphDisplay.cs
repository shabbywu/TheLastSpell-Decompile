using TPLib;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TheLastStand.View.WorldMap.Glyphs;

public class PreviewedGlyphDisplay : AGlyphDisplay
{
	public override void OnSelect(BaseEventData eventData)
	{
		base.OnSelect(eventData);
		GameConfigurationsView instance = TPSingleton<GameConfigurationsView>.Instance;
		Transform transform = ((Component)this).transform;
		instance.AdjustScrollView((RectTransform)(object)((transform is RectTransform) ? transform : null));
	}
}
