using TPLib;
using TheLastStand.Framework;
using TheLastStand.Manager.WorldMap;
using UnityEngine;

namespace TheLastStand.View.Building;

public class MagicCircleBaseView : MonoBehaviour
{
	public static class Constants
	{
		public const string AnimationBaseMagicCirclePath = "Animation/MagicCircle/";

		public const string AnimationNamePrefix = "MagicCircle_";

		public const string IdleBaseSuffix = "IdleBase";
	}

	[SerializeField]
	private Animator animator;

	public void DisableAnimator()
	{
		((Behaviour)animator).enabled = false;
	}

	public void InitAnimations()
	{
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Expected O, but got Unknown
		AnimationClip val = ResourcePooler.LoadOnce<AnimationClip>("Animation/MagicCircle/MagicCircle_IdleBase/" + TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id + "/MagicCircle_" + TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id + "_IdleBase_1", failSilently: true);
		AnimationClip val2 = ResourcePooler.LoadOnce<AnimationClip>("Animation/MagicCircle/MagicCircle_IdleBase/" + TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id + "/MagicCircle_" + TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id + "_IdleBase_2", failSilently: true);
		AnimationClip val3 = ResourcePooler.LoadOnce<AnimationClip>("Animation/MagicCircle/MagicCircle_IdleBase/" + TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id + "/MagicCircle_" + TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id + "_IdleBase_3", failSilently: true);
		AnimationClip val4 = ResourcePooler.LoadOnce<AnimationClip>("Animation/MagicCircle/MagicCircle_IdleBase/" + TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id + "/MagicCircle_" + TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id + "_IdleBase_4", failSilently: true);
		if (!((Object)(object)val == (Object)null) || !((Object)(object)val2 == (Object)null) || !((Object)(object)val3 == (Object)null) || !((Object)(object)val4 == (Object)null))
		{
			AnimatorOverrideController val5 = new AnimatorOverrideController(animator.runtimeAnimatorController);
			val5["MagicCircle_IdleBase_1"] = val;
			val5["MagicCircle_IdleBase_2"] = val2;
			val5["MagicCircle_IdleBase_3"] = val3;
			val5["MagicCircle_IdleBase_4"] = val4;
			animator.runtimeAnimatorController = (RuntimeAnimatorController)(object)val5;
		}
	}

	public void RefreshAnimationBaseWithMagesQuantity(int magesQuantity)
	{
		animator.SetTrigger("Slots" + magesQuantity);
	}
}
