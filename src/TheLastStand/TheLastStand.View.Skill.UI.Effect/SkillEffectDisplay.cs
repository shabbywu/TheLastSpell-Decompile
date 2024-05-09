using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Yield;
using TheLastStand.Controller.Skill.SkillAction;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Skill.SkillEffect;
using TheLastStand.Definition.Skill.SkillEffect.SkillSurroundingEffect;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager;
using TheLastStand.Manager.Skill;
using TheLastStand.Model;
using TheLastStand.Model.Item;
using TheLastStand.Model.Skill.SkillAction;
using TheLastStand.Model.Status;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit;
using TheLastStand.View.Unit.Stat;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Skill.UI.Effect;

public class SkillEffectDisplay : MonoBehaviour
{
	[SerializeField]
	private Image bgImage;

	[SerializeField]
	[Tooltip("Choice of sprite depending on the amount of elements to be displayed within it.")]
	private DataSpriteTable bgSprites;

	[SerializeField]
	[Tooltip("[Surrounding Effects] Choice of sprite depending on the amount of elements to be displayed within it.")]
	private DataSpriteTable bgSurroundingSprites;

	[SerializeField]
	[Tooltip("[Negative Effects] Choice of sprite depending on the amount of elements to be displayed within it.")]
	private DataSpriteTable bgNegativeSprites;

	[SerializeField]
	[Tooltip("[Caster Effects] Choice of sprite depending on the amount of elements to be displayed within it.")]
	private DataSpriteTable bgCasterEffectSprites;

	[SerializeField]
	private Image icon;

	[SerializeField]
	[Tooltip("Choice of sprite depending on the effect.")]
	private DataSpriteDictionary skillEffectIcons;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private float paddingBetweenTitleAndIcon;

	[SerializeField]
	[Tooltip("Choice of color depending on the effect.")]
	private DataColorDictionary skillEffectColors;

	[SerializeField]
	[Tooltip("Color of the probablility if reduced by something.")]
	private DataColor negativeColor;

	[SerializeField]
	private GameObject buffStatPanel;

	[SerializeField]
	private Image buffStatIcon;

	[SerializeField]
	private GameObject turnPanel;

	[SerializeField]
	private TextMeshProUGUI turnCount;

	[SerializeField]
	private GameObject turnInfiniteIcon;

	[SerializeField]
	private GameObject damagePanel;

	[SerializeField]
	private TextMeshProUGUI damage;

	[SerializeField]
	private GameObject propagationPanel;

	[SerializeField]
	private TextMeshProUGUI propagationBounces;

	[SerializeField]
	private GameObject surroundingDamagePanel;

	[SerializeField]
	private TextMeshProUGUI surroundingDamage;

	[SerializeField]
	private GameObject inaccuracyPercentagePanel;

	[SerializeField]
	private TextMeshProUGUI inaccuracyPercentage;

	[SerializeField]
	private GameObject isolatedPanel;

	[SerializeField]
	private TextMeshProUGUI isolatedPercentage;

	[SerializeField]
	private GameObject momentumPanel;

	[SerializeField]
	private TextMeshProUGUI momentumPercentage;

	[SerializeField]
	private GameObject opportunisticPanel;

	[SerializeField]
	private TextMeshProUGUI opportunisticPercentage;

	[SerializeField]
	private GameObject immunityPanel;

	[SerializeField]
	private Image immunityStatusIcon;

	private Vector3[] titleTextWorldCorners = (Vector3[])(object)new Vector3[4];

	private Vector3[] iconWorldCorners = (Vector3[])(object)new Vector3[4];

	private bool isSurrounding;

	private float stunChanceStatModifier;

	private SkillEffectDefinition SkillEffectDefinition { get; set; }

	private ISkillCaster SkillOwner { get; set; }

	private TheLastStand.Model.Unit.Unit SkillOwnerUnit { get; set; }

	public virtual void Init(SkillEffectDefinition skillEffect, ISkillCaster skillOwner, TheLastStand.Model.Skill.SkillAction.SkillAction skillAction, bool isSurrounding, bool casterEffect, Dictionary<UnitStatDefinition.E_Stat, float> statModifiers)
	{
		//IL_0353: Unknown result type (might be due to invalid IL or missing references)
		//IL_0358: Unknown result type (might be due to invalid IL or missing references)
		SkillEffectDefinition = skillEffect;
		SkillOwner = skillOwner;
		SkillOwnerUnit = SkillOwner as TheLastStand.Model.Unit.Unit;
		this.isSurrounding = isSurrounding;
		stunChanceStatModifier = statModifiers?.GetValueOrDefault(UnitStatDefinition.E_Stat.StunChanceModifier) ?? 0f;
		bool flag = false;
		icon.sprite = skillEffectIcons.GetSpriteById(SkillEffectDefinition.Id);
		RefreshTitle();
		int num = 0;
		turnPanel.SetActive(false);
		damagePanel.SetActive(false);
		immunityPanel.SetActive(false);
		surroundingDamagePanel.SetActive(false);
		inaccuracyPercentagePanel.SetActive(false);
		isolatedPanel.SetActive(false);
		momentumPanel.SetActive(false);
		opportunisticPanel.SetActive(false);
		propagationPanel.SetActive(false);
		buffStatPanel.SetActive(false);
		SkillEffectDefinition skillEffectDefinition = SkillEffectDefinition;
		if (!(skillEffectDefinition is StatusEffectDefinition statusEffectDefinition))
		{
			if (!(skillEffectDefinition is DamageSurroundingEffectDefinition))
			{
				if (!(skillEffectDefinition is InaccurateSkillEffectDefinition inaccurateSkillEffectDefinition))
				{
					if (!(skillEffectDefinition is IsolatedSkillEffectDefinition))
					{
						if (!(skillEffectDefinition is KillSkillEffectDefinition))
						{
							if (!(skillEffectDefinition is MomentumEffectDefinition))
							{
								if (!(skillEffectDefinition is MultiHitSkillEffectDefinition multiHitSkillEffectDefinition))
								{
									if (!(skillEffectDefinition is OpportunisticSkillEffectDefinition opportunisticSkillEffectDefinition))
									{
										if (!(skillEffectDefinition is PropagationSkillEffectDefinition propagationSkillEffectDefinition))
										{
											if (!(skillEffectDefinition is ArmorShreddingEffectDefinition armorShreddingEffectDefinition))
											{
												if (!(skillEffectDefinition is RemoveStatusEffectDefinition removeStatusEffectDefinition))
												{
													if (skillEffectDefinition is ExtinguishBrazierSkillEffectDefinition extinguishBrazierSkillEffectDefinition)
													{
														num = 1;
														damagePanel.SetActive(true);
														((TMP_Text)damage).text = extinguishBrazierSkillEffectDefinition.BrazierDamage.ToString();
													}
												}
												else if (removeStatusEffectDefinition.Id != "Discharge")
												{
													buffStatPanel.SetActive(true);
													buffStatIcon.sprite = skillEffectIcons.GetSpriteById(removeStatusEffectDefinition.RemoveStatusDefinition.Status.ToString());
												}
											}
											else
											{
												num = 1;
												float num2 = armorShreddingEffectDefinition.BonusDamage * 100f;
												num2 += SkillOwnerUnit?.GetClampedStatValueWithModifier(UnitStatDefinition.E_Stat.ArmorShreddingAttacks, statModifiers?.GetValueOrDefault(UnitStatDefinition.E_Stat.ArmorShreddingAttacks)) ?? 0f;
												momentumPanel.SetActive(true);
												((TMP_Text)momentumPercentage).text = $"+{num2}%";
											}
										}
										else
										{
											num = 1;
											int num3 = ((SkillOwnerUnit != null) ? SkillOwnerUnit.UnitController.GetModifiedPropagationsCount(propagationSkillEffectDefinition.PropagationsCount, statModifiers?.GetValueOrDefault(UnitStatDefinition.E_Stat.PropagationBouncesModifier)) : propagationSkillEffectDefinition.PropagationsCount);
											if (SkillOwnerUnit != null)
											{
												damagePanel.SetActive(true);
												((TMP_Text)damage).text = $"{SkillOwnerUnit.GetClampedStatValueWithModifier(UnitStatDefinition.E_Stat.PropagationDamage, statModifiers?.GetValueOrDefault(UnitStatDefinition.E_Stat.PropagationDamage)):N0}%";
												num = 2;
											}
											propagationPanel.SetActive(true);
											((TMP_Text)propagationBounces).text = $"x{num3}";
										}
									}
									else
									{
										num = 3;
										float damageMultiplier = opportunisticSkillEffectDefinition.DamageMultiplier;
										int num4 = Mathf.RoundToInt((SkillOwnerUnit?.GetClampedStatValueWithModifier(UnitStatDefinition.E_Stat.OpportunisticAttacks, statModifiers?.GetValueOrDefault(UnitStatDefinition.E_Stat.OpportunisticAttacks)) ?? 0f) * damageMultiplier);
										opportunisticPanel.SetActive(true);
										((TMP_Text)opportunisticPercentage).text = $"x{damageMultiplier} ({num4}%)";
									}
								}
								else
								{
									num = 1;
									int num5 = ((SkillOwnerUnit != null) ? SkillOwnerUnit.UnitController.GetModifiedMultiHitsCount(multiHitSkillEffectDefinition.HitsCount, statModifiers?.GetValueOrDefault(UnitStatDefinition.E_Stat.MultiHitsCountModifier)) : multiHitSkillEffectDefinition.HitsCount);
									damagePanel.SetActive(true);
									((TMP_Text)damage).text = $"x{num5}";
								}
							}
							else
							{
								num = 1;
								float num6 = ((SkillEffectDefinition is MomentumEffectDefinition momentumEffectDefinition) ? (momentumEffectDefinition.DamageBonusPerTile * 100f) : 0f);
								num6 += SkillOwnerUnit?.GetClampedStatValueWithModifier(UnitStatDefinition.E_Stat.MomentumAttacks, statModifiers?.GetValueOrDefault(UnitStatDefinition.E_Stat.MomentumAttacks)) ?? 0f;
								momentumPanel.SetActive(true);
								((TMP_Text)momentumPercentage).text = $"+{num6}%";
							}
						}
						else
						{
							num = 0;
						}
					}
					else
					{
						num = 3;
						float num7 = ((SkillEffectDefinition is IsolatedSkillEffectDefinition isolatedSkillEffectDefinition) ? isolatedSkillEffectDefinition.DamageMultiplier : 0f);
						float num8 = SkillOwnerUnit?.GetClampedStatValueWithModifier(UnitStatDefinition.E_Stat.IsolatedAttacks, statModifiers?.GetValueOrDefault(UnitStatDefinition.E_Stat.IsolatedAttacks)) ?? 0f;
						int num9 = Mathf.RoundToInt(num7 * num8);
						isolatedPanel.SetActive(true);
						((TMP_Text)isolatedPercentage).text = $"x{num7} ({num9}%)";
					}
				}
				else
				{
					num = 1;
					inaccuracyPercentagePanel.SetActive(true);
					((TMP_Text)inaccuracyPercentage).text = $"x{1f + inaccurateSkillEffectDefinition.Malus}";
					flag = true;
				}
			}
			else
			{
				num = 3;
				surroundingDamagePanel.SetActive(true);
				Vector2Int val = (skillAction.SkillActionController as AttackSkillActionController).ComputeCasterDamageRange(SkillOwner, isSurroundingTile: true, statModifiers);
				((TMP_Text)surroundingDamage).text = $"{((Vector2Int)(ref val)).x}-{((Vector2Int)(ref val)).y}";
			}
		}
		else
		{
			num = 1;
			turnPanel.SetActive(true);
			if ((float)statusEffectDefinition.TurnsCount == -1f)
			{
				turnInfiniteIcon.SetActive(true);
				((Component)turnCount).gameObject.SetActive(false);
			}
			else
			{
				turnInfiniteIcon.SetActive(false);
				((Component)turnCount).gameObject.SetActive(true);
				((TMP_Text)turnCount).text = SkillOwner?.ComputeStatusDuration(statusEffectDefinition.StatusType, statusEffectDefinition.TurnsCount, skillAction.PerkDataContainer).ToString() ?? statusEffectDefinition.TurnsCount.ToString();
			}
			if (SkillEffectDefinition is PoisonEffectDefinition { DamagePerTurn: var num10 })
			{
				if (SkillOwnerUnit != null)
				{
					num10 = SkillOwnerUnit.UnitController.GetModifiedPoisonDamage(num10, statModifiers?.GetValueOrDefault(UnitStatDefinition.E_Stat.PoisonDamageModifier));
				}
				if (skillAction.Skill.SkillContainer is TheLastStand.Model.Item.Item item)
				{
					num10 *= SkillDatabase.PoisonDamageScaleDefinition.GetMultiplierAtLevel(item.Level);
				}
				damagePanel.SetActive(true);
				((TMP_Text)damage).text = Mathf.CeilToInt(num10).ToString();
				num++;
			}
			else if (SkillEffectDefinition is ImmuneToNegativeStatusEffectDefinition immuneToNegativeStatusEffectDefinition)
			{
				immunityPanel.SetActive(true);
				immunityStatusIcon.sprite = GetStatusImmunityIcon(immuneToNegativeStatusEffectDefinition.StatusImmunity);
			}
		}
		if (this.isSurrounding)
		{
			bgImage.sprite = bgSurroundingSprites.GetSpriteAt(num);
		}
		else if (casterEffect)
		{
			bgImage.sprite = bgCasterEffectSprites.GetSpriteAt(num);
		}
		else
		{
			bgImage.sprite = (flag ? bgNegativeSprites.GetSpriteAt(num) : bgSprites.GetSpriteAt(num));
		}
		((MonoBehaviour)this).StartCoroutine(DelayedResizeTitleTextAccordingToIconPosition());
	}

	private void Awake()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Combine((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
	}

	[ContextMenu("Resize TitleText")]
	private void ResizeTitleTextAccordingToIconPosition()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		((TMP_Text)titleText).rectTransform.GetWorldCorners(titleTextWorldCorners);
		((Graphic)icon).rectTransform.GetWorldCorners(iconWorldCorners);
		Vector2 sizeDelta = ((TMP_Text)titleText).rectTransform.sizeDelta;
		sizeDelta.x = Mathf.Abs(titleTextWorldCorners[3].x - iconWorldCorners[3].x) - paddingBetweenTitleAndIcon;
		((TMP_Text)titleText).rectTransform.sizeDelta = sizeDelta;
	}

	private IEnumerator DelayedResizeTitleTextAccordingToIconPosition()
	{
		yield return SharedYields.WaitForEndOfFrame;
		ResizeTitleTextAccordingToIconPosition();
	}

	private Sprite GetStatusImmunityIcon(Status.E_StatusType statusImmunity)
	{
		if ((statusImmunity & Status.E_StatusType.AllNegativeImmunity) == Status.E_StatusType.AllNegativeImmunity)
		{
			return skillEffectIcons.GetSpriteById(Status.E_StatusType.AllNegative.ToString());
		}
		if ((statusImmunity & Status.E_StatusType.StunImmunity) == Status.E_StatusType.StunImmunity)
		{
			return skillEffectIcons.GetSpriteById(Status.E_StatusType.Stun.ToString());
		}
		if ((statusImmunity & Status.E_StatusType.PoisonImmunity) == Status.E_StatusType.PoisonImmunity)
		{
			return skillEffectIcons.GetSpriteById(Status.E_StatusType.Poison.ToString());
		}
		if ((statusImmunity & Status.E_StatusType.DebuffImmunity) == Status.E_StatusType.DebuffImmunity)
		{
			return skillEffectIcons.GetSpriteById(Status.E_StatusType.Debuff.ToString());
		}
		if ((statusImmunity & Status.E_StatusType.ContagionImmunity) == Status.E_StatusType.ContagionImmunity)
		{
			return skillEffectIcons.GetSpriteById(Status.E_StatusType.Contagion.ToString());
		}
		return skillEffectIcons.GetSpriteById(Status.E_StatusType.AllNegativeImmunity.ToString());
	}

	private void OnDestroy()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Remove((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
	}

	private string ColoredName(string name)
	{
		if (SkillEffectDefinition == null)
		{
			return string.Empty;
		}
		string colorHexCodeById = skillEffectColors.GetColorHexCodeById(SkillEffectDefinition.Id);
		if (colorHexCodeById == null)
		{
			return name;
		}
		return "<color=#" + colorHexCodeById + ">" + name + "</color>";
	}

	private void OnLocalize()
	{
		if (((Component)this).gameObject.activeInHierarchy)
		{
			RefreshTitle();
		}
	}

	private void RefreshTitle()
	{
		float num = 1f;
		bool flag = false;
		bool flag2 = false;
		SkillEffectDefinition skillEffectDefinition = SkillEffectDefinition;
		string text;
		if (!(skillEffectDefinition is StatusEffectDefinition statusEffectDefinition))
		{
			text = ((skillEffectDefinition is IsolatedSkillEffectDefinition) ? (UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.IsolatedAttacks].Name ?? "") : ((skillEffectDefinition is OpportunisticSkillEffectDefinition) ? (UnitDatabase.UnitStatDefinitions[UnitStatDefinition.E_Stat.OpportunisticAttacks].Name ?? "") : ((skillEffectDefinition is RegenStatSkillEffectDefinition regenStatSkillEffectDefinition) ? (regenStatSkillEffectDefinition.Stat.GetValueStylized(regenStatSkillEffectDefinition.Bonus) + " " + UnitStatDisplay.GetStatIconToString(regenStatSkillEffectDefinition.Stat) + "<color=#f8d8b3>" + regenStatSkillEffectDefinition.StatName + "</color>") : ((!(skillEffectDefinition is DecreaseStatSkillEffectDefinition decreaseStatSkillEffectDefinition)) ? ColoredName(SkillManager.GetSkillEffectName(SkillEffectDefinition.Id)) : (decreaseStatSkillEffectDefinition.Stat.GetValueStylized(0f - decreaseStatSkillEffectDefinition.LossValue) + " " + UnitStatDisplay.GetStatIconToString(decreaseStatSkillEffectDefinition.Stat) + "<color=#f8d8b3>" + decreaseStatSkillEffectDefinition.StatName + "</color>")))));
		}
		else
		{
			BuffEffectDefinition buffEffectDefinition = statusEffectDefinition as BuffEffectDefinition;
			DebuffEffectDefinition debuffEffectDefinition = statusEffectDefinition as DebuffEffectDefinition;
			if ((flag2 = statusEffectDefinition is StunEffectDefinition) && SkillOwnerUnit != null)
			{
				num = SkillOwnerUnit.UnitController.GetModifiedStunChance(statusEffectDefinition.BaseChance, stunChanceStatModifier);
				if (!isSurrounding)
				{
					Tile tile = TPSingleton<GameManager>.Instance.Game.Cursor.Tile;
					if (tile != null && tile.Unit != null)
					{
						float num2 = num;
						num = TPSingleton<GameManager>.Instance.Game.Cursor.Tile.Unit.UnitController.GetResistedStunChance(num);
						if (num2 != num)
						{
							flag = true;
						}
					}
				}
			}
			else
			{
				num = statusEffectDefinition.BaseChance;
			}
			if (buffEffectDefinition != null || debuffEffectDefinition != null)
			{
				UnitStatDefinition.E_Stat e_Stat = UnitStatDefinition.E_Stat.Undefined;
				string text2 = string.Empty;
				if (buffEffectDefinition != null)
				{
					e_Stat = buffEffectDefinition.Stat;
					text2 = $"+{buffEffectDefinition.Bonus}";
				}
				else if (debuffEffectDefinition != null)
				{
					e_Stat = debuffEffectDefinition.Stat;
					text2 = $"-{debuffEffectDefinition.Malus}";
				}
				text = ColoredName(string.Format("<size=125%>{0}</size>{1} <sprite name=\"{2}\"> {3}", text2, e_Stat.ShownAsPercentage() ? "%" : string.Empty, e_Stat, UnitDatabase.UnitStatDefinitions[e_Stat].Name));
			}
			else
			{
				text = ColoredName(SkillManager.GetSkillEffectName(SkillEffectDefinition.Id));
			}
		}
		if (flag2)
		{
			((TMP_Text)titleText).text = (flag ? $"{text} (<color=#{negativeColor._HexCode}>{Mathf.RoundToInt(num * 100f)}%</color>)" : $"{text} ({Mathf.RoundToInt(num * 100f)}%)");
		}
		else
		{
			((TMP_Text)titleText).text = text;
		}
	}
}
