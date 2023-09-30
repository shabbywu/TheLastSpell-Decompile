using System.Collections;
using DG.Tweening;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Model.Skill.SkillAction.SkillActionExecution.SkillActionExecutionTileData;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Skill.SkillAction.UI;

public class DamageDisplay : EffectDisplay
{
	public new static class Constants
	{
		public const string DodgedLabel = "DodgeFeedback";

		public const string CriticalFormatting = "DamageDisplay_CriticalHitFormatting";
	}

	[SerializeField]
	private float critFontSizeMult = 2f;

	[SerializeField]
	private TextMeshProUGUI damageText;

	[SerializeField]
	private DataColor blinkColor;

	[SerializeField]
	private DataColor damageColor;

	[SerializeField]
	private DataColor poisonDamageColor;

	[SerializeField]
	private DataColor dodgeColor;

	[SerializeField]
	private DataColor critColor;

	[SerializeField]
	private float blinkDuration = 0.1f;

	[SerializeField]
	private Ease blinkEasing = (Ease)20;

	[SerializeField]
	private Ease fadeEasing = (Ease)12;

	[SerializeField]
	private Transform translateTarget;

	[SerializeField]
	private Vector2 translateOffsetYRange = new Vector2(30f, 60f);

	[SerializeField]
	private Vector2 translateOffsetYRangeSpecial = new Vector2(30f, 60f);

	[SerializeField]
	private Vector2 translateOffsetXRange = new Vector2(-2f, 40f);

	[SerializeField]
	private AnimationCurve translateYEasingCurve;

	[SerializeField]
	private Ease translateXEasing = (Ease)12;

	private float translateYOffset;

	private float translateXOffset;

	private float defaultFontSize = 18f;

	private void Awake()
	{
		defaultFontSize = ((TMP_Text)damageText).fontSize;
	}

	public void Init(int damage)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		((TMP_Text)damageText).text = damage.ToString();
		((TMP_Text)damageText).fontSize = defaultFontSize;
		((Graphic)damageText).color = damageColor._Color;
		translateYOffset = TPHelpers.RandomIntInRange(translateOffsetYRange, false);
	}

	public void Init(AttackSkillActionExecutionTileData attackData)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		translateXOffset = TPHelpers.RandomIntInRange(translateOffsetXRange, false) * ((!TPHelpers.RandomBool()) ? 1 : (-1));
		if (attackData.Dodged)
		{
			((TMP_Text)damageText).fontSize = defaultFontSize;
			((TMP_Text)damageText).text = Localizer.Get("DodgeFeedback");
			((Graphic)damageText).color = dodgeColor._Color;
			translateYOffset = TPHelpers.RandomIntInRange(translateOffsetYRangeSpecial, false);
			return;
		}
		float num = attackData.HealthDamage + attackData.ArmorDamage + attackData.OverkillDamage;
		((TMP_Text)damageText).text = ((int)num).ToString();
		if (attackData.IsCrit)
		{
			((TMP_Text)damageText).text = Localizer.Format("DamageDisplay_CriticalHitFormatting", new object[1] { ((TMP_Text)damageText).text });
			((TMP_Text)damageText).fontSize = defaultFontSize * critFontSizeMult;
			((Graphic)damageText).color = critColor._Color;
			translateYOffset = TPHelpers.RandomIntInRange(translateOffsetYRangeSpecial, false);
		}
		else
		{
			((TMP_Text)damageText).fontSize = defaultFontSize;
			((Graphic)damageText).color = (attackData.IsPoison ? poisonDamageColor._Color : damageColor._Color);
			translateYOffset = TPHelpers.RandomIntInRange(translateOffsetYRange, false);
		}
	}

	protected override IEnumerator DisplayCoroutine()
	{
		Color color = ((Graphic)damageText).color;
		color.a = 0f;
		TweenSettingsExtensions.SetEase<Tweener>(TweenSettingsExtensions.From<Tweener>(DOTweenModuleUI.DOBlendableColor((Graphic)(object)damageText, blinkColor._Color, blinkDuration)), blinkEasing);
		TweenSettingsExtensions.SetEase<Tweener>(DOTweenModuleUI.DOBlendableColor((Graphic)(object)damageText, color, DisplayDuration), fadeEasing);
		Transform obj = translateTarget;
		if (obj != null)
		{
			TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOBlendableLocalMoveBy(obj, new Vector3(0f, translateYOffset), DisplayDuration, false), translateYEasingCurve);
		}
		Transform obj2 = translateTarget;
		if (obj2 != null)
		{
			TweenSettingsExtensions.SetEase<Tweener>(ShortcutExtensions.DOBlendableLocalMoveBy(obj2, new Vector3(translateXOffset, 0f), DisplayDuration, false), translateXEasing);
		}
		yield return base.DisplayCoroutine();
	}
}
