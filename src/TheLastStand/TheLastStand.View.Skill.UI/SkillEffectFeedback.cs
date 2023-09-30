using System;
using System.Text;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Definition.Skill.SkillAction;
using TheLastStand.Definition.Skill.SkillEffect;
using TheLastStand.Manager;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Skill;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Skill.SkillAction;
using TheLastStand.Model.Unit;
using TheLastStand.View.Cursor;
using TheLastStand.View.Generic;
using UnityEngine;

namespace TheLastStand.View.Skill.UI;

public class SkillEffectFeedback : TooltipBase
{
	private readonly string[] displayedEffects = new string[2] { "MultiHit", "Inaccurate" };

	[SerializeField]
	private TextMeshProUGUI text;

	private TheLastStand.Model.Skill.Skill SelectedSkill => PlayableUnitManager.SelectedSkill ?? BuildingManager.SelectedSkill;

	protected override void Awake()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		base.Awake();
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Combine((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
	}

	protected override bool CanBeDisplayed()
	{
		if (SelectedSkill != null)
		{
			if (!SelectedSkill.SkillDefinition.SkillActionDefinition.HasAnyEffect(displayedEffects) && !SelectedSkill.SkillDefinition.CanRotate && !SkillManager.DebugSkillsForceCanRotate)
			{
				if (SelectedSkill.SkillAction is AttackSkillAction attackSkillAction)
				{
					return attackSkillAction.AttackType == AttackSkillActionDefinition.E_AttackType.Ranged;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	protected override void OnDisplay()
	{
		base.OnDisplay();
		base.FollowElement.ChangeTarget(InputManager.IsLastControllerJoystick ? ((Component)TPSingleton<CursorView>.Instance.JoystickCursorView).transform : null);
		base.FollowElement.ConvertToScreenSpace = InputManager.IsLastControllerJoystick;
	}

	protected override void RefreshContent()
	{
		StringBuilder stringBuilder = new StringBuilder();
		string skillRotationFeedbackText = GetSkillRotationFeedbackText();
		if (!string.IsNullOrEmpty(skillRotationFeedbackText))
		{
			stringBuilder.AppendLine(skillRotationFeedbackText);
		}
		string[] array = displayedEffects;
		foreach (string text in array)
		{
			if (SelectedSkill.SkillAction.TryGetFirstEffect<SkillEffectDefinition>(text, out var effect) && text != null && text == "MultiHit")
			{
				stringBuilder.AppendLine(GetFeedbackText(effect as MultiHitSkillEffectDefinition));
			}
		}
		string dodgeModifierFeedbackText = GetDodgeModifierFeedbackText();
		if (!string.IsNullOrEmpty(dodgeModifierFeedbackText))
		{
			stringBuilder.AppendLine(dodgeModifierFeedbackText);
		}
		((TMP_Text)this.text).text = stringBuilder.ToString();
	}

	private string GetFeedbackText(MultiHitSkillEffectDefinition definition)
	{
		int num = ((SelectedSkill.SkillAction.SkillActionExecution.Caster is PlayableUnit playableUnit) ? playableUnit.PlayableUnitController.GetModifiedMultiHitsCount(definition.HitsCount) : definition.HitsCount);
		return $"{SkillManager.GetSkillEffectName(definition.Id)} {SelectedSkill.SkillAction.SkillActionExecution.TargetTiles.Count + 1}/{num}";
	}

	private string GetDodgeModifierFeedbackText()
	{
		float selectedSkillDodgeMultiplierWithDistance = SkillManager.GetSelectedSkillDodgeMultiplierWithDistance();
		if (selectedSkillDodgeMultiplierWithDistance == 1f)
		{
			return string.Empty;
		}
		return Localizer.Get("DamageTypeModifierName_DodgeMultiplier") + " x" + selectedSkillDodgeMultiplierWithDistance;
	}

	private string GetSkillRotationFeedbackText()
	{
		if (SelectedSkill == null || (!SelectedSkill.SkillDefinition.CanRotate && !SkillManager.DebugSkillsForceCanRotate) || !TileObjectSelectionManager.CursorOrientationFromSelection.HasFlag(TileObjectSelectionManager.E_Orientation.LIMIT))
		{
			return string.Empty;
		}
		string text = string.Empty;
		string[] localizedHotkeysForAction = InputManager.GetLocalizedHotkeysForAction("RotateSkill");
		if (localizedHotkeysForAction != null && localizedHotkeysForAction.Length != 0)
		{
			text = localizedHotkeysForAction[0];
		}
		return Localizer.Format("SkillRotation_FeedbackText", new object[1] { text });
	}

	private void OnDestroy()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Remove((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
	}

	private void OnLocalize()
	{
		if (((Component)this).gameObject.activeInHierarchy)
		{
			RefreshContent();
		}
	}
}
