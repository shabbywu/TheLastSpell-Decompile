using TMPro;
using UnityEngine;

namespace TheLastStand.View.Skill.SkillAction.UI;

public class GainDamageDisplay : AppearingEffectDisplay
{
	public new static class Constants
	{
		public const string DisplayPrefabResourcePath = "Prefab/Displayable Effect/UI Effect Displays/GainDamageDisplay";
	}

	[SerializeField]
	private TextMeshProUGUI damageGainDisplay;

	public override void Init(int damageGain)
	{
		base.Init();
		((TMP_Text)damageGainDisplay).text = ((damageGain > 0) ? "+" : string.Empty);
		TextMeshProUGUI obj = damageGainDisplay;
		((TMP_Text)obj).text = ((TMP_Text)obj).text + $"{damageGain}%";
	}
}
