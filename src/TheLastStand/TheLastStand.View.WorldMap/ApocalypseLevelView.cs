using TMPro;
using TPLib.Localization;
using TheLastStand.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.WorldMap;

public class ApocalypseLevelView : MonoBehaviour
{
	[SerializeField]
	private string activateState = string.Empty;

	[SerializeField]
	private TextMeshProUGUI apocalypseName;

	[SerializeField]
	private Animator animator;

	[SerializeField]
	private string disabledState = string.Empty;

	[SerializeField]
	private Image levelImage;

	private float animSpeed;

	public void Init(int level)
	{
		if (level > 0)
		{
			levelImage.sprite = ResourcePooler<Sprite>.LoadOnce("View/Sprites/UI/WorldMap/ApocalypseLevels/ApocalypseLevel_" + level.ToString("00"), false);
		}
		else
		{
			((Behaviour)levelImage).enabled = false;
		}
		if (level == -1)
		{
			((TMP_Text)apocalypseName).text = string.Empty;
		}
		else if ((Object)(object)apocalypseName != (Object)null)
		{
			((TMP_Text)apocalypseName).text = Localizer.Get((level == 0) ? "WorldMap_ApocalypseDifficulty_Normal" : "WorldMap_ApocalypseDifficulty_Apocalypse");
		}
		animator.Play((level != -1) ? activateState : disabledState, 0, Random.value);
		animSpeed = animator.speed;
	}

	public void StopAnimation(bool stopAnimations)
	{
		animator.speed = (stopAnimations ? 0f : animSpeed);
	}
}
