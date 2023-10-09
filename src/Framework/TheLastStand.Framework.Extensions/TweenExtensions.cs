using DG.Tweening;
using UnityEngine;

namespace TheLastStand.Framework.Extensions;

public static class TweenExtensions
{
	public static T SetFullId<T>(this T t, string tweenName, Component component) where T : Tween
	{
		return TweenSettingsExtensions.SetId<T>(t, tweenName + " [" + ((Object)component).name + "] [" + ((object)component).GetType().Name + "]");
	}
}
