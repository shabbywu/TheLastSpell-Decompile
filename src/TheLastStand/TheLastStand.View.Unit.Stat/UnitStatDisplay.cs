using System;
using System.Collections;
using TMPro;
using TPLib;
using TPLib.Localization;
using TPLib.Log;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Manager.Skill;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.Skill;
using TheLastStand.Model.Skill.SkillAction;
using TheLastStand.Model.Unit;
using TheLastStand.Model.Unit.Enemy;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TheLastStand.View.Unit.Stat;

public class UnitStatDisplay : MonoBehaviour
{
	public enum E_IconSize
	{
		VerySmall,
		Small
	}

	[SerializeField]
	private UnitStatDefinition.E_Stat defaultStat = UnitStatDefinition.E_Stat.Undefined;

	[SerializeField]
	private TextMeshProUGUI statTitleText;

	[SerializeField]
	private bool showShortName;

	[SerializeField]
	private bool showTitleInUpperCase;

	[SerializeField]
	[Tooltip("Used to show stat final value (ex: Character sheet)")]
	private TextMeshProUGUI statValueText;

	[SerializeField]
	[Tooltip("Use this if you want to show the base value (aka: value without bonuses).")]
	private bool useFinalValue = true;

	[SerializeField]
	[Tooltip("Adds a '+' if the value is positive")]
	[FormerlySerializedAs("showHasABonus")]
	private bool showAsModifier;

	[SerializeField]
	private bool showStatMax = true;

	[SerializeField]
	protected Image gaugeImage;

	[SerializeField]
	private bool useDefaultGaugeSprite;

	[SerializeField]
	private UnitStatBreakdownPanel breakDownPanel;

	[Tooltip("Texts that wil be affected by the various color options")]
	[SerializeField]
	[FormerlySerializedAs("textsToColor")]
	protected TextMeshProUGUI[] labelsToTint;

	[Tooltip("Color the 'Texts To Color' according to Stat's color (if it has one defined)")]
	[SerializeField]
	protected bool useStatColor;

	[SerializeField]
	[FormerlySerializedAs("statColors")]
	protected DataColorDictionary statsColors;

	[SerializeField]
	private TextMeshProUGUI statDescriptionText;

	[SerializeField]
	private Image iconImage;

	[SerializeField]
	private E_IconSize iconSize = E_IconSize.Small;

	[SerializeField]
	private bool preventIconRefresh;

	[SerializeField]
	private Image statBG;

	[SerializeField]
	private DataSpriteDictionary statBGSprites;

	[SerializeField]
	private Image disableBarImage;

	[SerializeField]
	[FormerlySerializedAs("colorTextValueIfDisable")]
	private bool tintValueLabelIfDisabled = true;

	[SerializeField]
	[FormerlySerializedAs("disableColor")]
	private DataColor disabledColor;

	[SerializeField]
	private bool useMarkers;

	[SerializeField]
	private RectTransform markersContainer;

	[SerializeField]
	[Min(1f)]
	private int markerWidth = 2;

	private bool initialized;

	protected Color valueTextOriginalColor;

	protected Color? colorOverride;

	protected bool fullRefreshNeeded = true;

	private GaugeMarkersDisplayer gaugeMarkersDisplayer;

	private UnitStatDefinition statDefinition;

	private float secondaryFinalValue;

	private UnitStatDefinition secondaryStatDefinition;

	private TheLastStand.Model.Unit.Unit targetUnit;

	protected Color[] textsOriginalColor;

	protected bool valuesCached;

	public Color? ColorOverride
	{
		get
		{
			return colorOverride;
		}
		set
		{
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			Color? val = colorOverride;
			Color? val2 = value;
			if (val.HasValue != val2.HasValue || (val.HasValue && !(val.GetValueOrDefault() == val2.GetValueOrDefault())))
			{
				colorOverride = value;
				RefreshColor();
			}
		}
	}

	public bool IsDisabled { get; set; }

	public bool UseSelectedSkill { get; set; } = true;


	public TheLastStand.Model.Unit.Unit TargetUnit
	{
		get
		{
			return targetUnit ?? TileObjectSelectionManager.SelectedUnit;
		}
		set
		{
			if (targetUnit != value)
			{
				targetUnit = value;
			}
		}
	}

	public UnitStatDefinition SecondaryStatDefinition
	{
		get
		{
			return secondaryStatDefinition;
		}
		set
		{
			if (secondaryStatDefinition != value)
			{
				secondaryStatDefinition = value;
				fullRefreshNeeded = true;
			}
		}
	}

	public UnitStatDefinition StatDefinition
	{
		get
		{
			return statDefinition;
		}
		set
		{
			if (statDefinition != value)
			{
				statDefinition = value;
				fullRefreshNeeded = true;
			}
		}
	}

	public TextMeshProUGUI StatValueText => statValueText;

	public static Sprite GetStatIconSprite(UnitStatDefinition.E_Stat statDefinitionId, E_IconSize size)
	{
		return ResourcePooler.LoadOnce<Sprite>($"View/Sprites/UI/Stats/Icons/{size}_{statDefinitionId}", failSilently: true);
	}

	public static Sprite GetStatIconHoverSprite(UnitStatDefinition.E_Stat statDefinitionId, E_IconSize size)
	{
		return ResourcePooler.LoadOnce<Sprite>($"View/Sprites/UI/Stats/Icons/{size}_{statDefinitionId}_On", failSilently: true);
	}

	public static string GetStatIconToString(UnitStatDefinition.E_Stat statDefinitionId)
	{
		return "<sprite name=\"" + statDefinitionId.ToString() + "\"/>";
	}

	public static Sprite GetStatGaugeSprite(UnitStatDefinition.E_Stat statDefinitionId)
	{
		return ResourcePooler.LoadOnce<Sprite>($"View/Sprites/UI/Stats/Gauges/{statDefinitionId}", failSilently: false);
	}

	public void DisplayHover(bool hover)
	{
		if ((Object)(object)iconImage != (Object)null)
		{
			Sprite statIconHoverSprite = GetStatIconHoverSprite(StatDefinition.Id, iconSize);
			iconImage.sprite = ((hover && (Object)(object)statIconHoverSprite != (Object)null) ? statIconHoverSprite : GetStatIconSprite(StatDefinition.Id, iconSize));
		}
	}

	public void Init()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (!initialized)
		{
			initialized = true;
			BackupOriginalColors();
			if ((Object)(object)statValueText != (Object)null)
			{
				valueTextOriginalColor = ((Graphic)statValueText).color;
			}
			Localizer.onLocalize = (OnLocalizeNotification)Delegate.Combine((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
			if (defaultStat != UnitStatDefinition.E_Stat.Undefined)
			{
				StatDefinition = UnitDatabase.UnitStatDefinitions[defaultStat];
			}
			gaugeMarkersDisplayer = new GaugeMarkersDisplayer(markersContainer);
		}
	}

	public void Refresh(bool forceFullRefresh = false)
	{
		if (StatDefinition != null)
		{
			fullRefreshNeeded |= forceFullRefresh;
			valuesCached = false;
			RefreshInternal();
			fullRefreshNeeded = false;
		}
	}

	protected virtual void CacheStatValues()
	{
		if (SecondaryStatDefinition != null && TargetUnit.UnitStatsController.UnitStats.Stats.ContainsKey(SecondaryStatDefinition.Id))
		{
			secondaryFinalValue = Mathf.Floor(TargetUnit.GetClampedStatValue(SecondaryStatDefinition.Id));
		}
		valuesCached = true;
	}

	protected virtual void RefreshColor()
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		if (labelsToTint == null || labelsToTint.Length == 0)
		{
			return;
		}
		Color? val = null;
		if (!ColorOverride.HasValue)
		{
			if (useStatColor)
			{
				val = statsColors.GetColorById(StatDefinition.Id.ToString());
			}
		}
		else
		{
			val = ColorOverride.Value;
		}
		int i = 0;
		for (int num = labelsToTint.Length; i < num; i++)
		{
			if ((Object)(object)labelsToTint[i] != (Object)null)
			{
				((Graphic)labelsToTint[i]).color = (Color)(((_003F?)val) ?? textsOriginalColor[i]);
			}
		}
	}

	protected virtual void RefreshInternal()
	{
		if (TargetUnit == null)
		{
			return;
		}
		RefreshValues();
		if (fullRefreshNeeded)
		{
			RefreshName();
		}
		if (fullRefreshNeeded)
		{
			if ((Object)(object)statDescriptionText != (Object)null)
			{
				((TMP_Text)statDescriptionText).text = StatDefinition.Description;
			}
			RefreshIcon();
			RefreshGauge();
			RefreshColor();
			RefreshBackground();
		}
	}

	private void Awake()
	{
		Init();
	}

	private void BackupOriginalColors()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		if (labelsToTint == null || labelsToTint.Length == 0)
		{
			return;
		}
		textsOriginalColor = (Color[])(object)new Color[labelsToTint.Length];
		int i = 0;
		for (int num = labelsToTint.Length; i < num; i++)
		{
			if ((Object)(object)labelsToTint[i] != (Object)null)
			{
				textsOriginalColor[i] = ((Graphic)labelsToTint[i]).color;
			}
		}
	}

	private void OnDestroy()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Remove((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
	}

	private void OnEnable()
	{
		OnLocalize();
	}

	private void OnLocalize()
	{
		if (!((Object)(object)((Component)this).gameObject == (Object)null) && ((Component)this).gameObject.activeInHierarchy)
		{
			Refresh(forceFullRefresh: true);
		}
	}

	private void RefreshBackground()
	{
		if (!((Object)(object)statBG == (Object)null))
		{
			statBG.sprite = statBGSprites.GetSpriteById(StatDefinition.Id.ToString());
		}
	}

	private void RefreshGauge()
	{
		if (!((Object)(object)gaugeImage == (Object)null))
		{
			if (!useDefaultGaugeSprite)
			{
				gaugeImage.sprite = GetStatGaugeSprite(StatDefinition.Id);
			}
			((Behaviour)gaugeImage).enabled = (Object)(object)gaugeImage.sprite != (Object)null;
		}
	}

	private void RefreshIcon()
	{
		if ((Object)(object)iconImage == (Object)null || preventIconRefresh)
		{
			return;
		}
		iconImage.sprite = GetStatIconSprite(StatDefinition.Id, iconSize);
		if ((Object)(object)iconImage.sprite == (Object)null)
		{
			CLoggerManager.Log((object)$"{iconSize.ToString()} size has not been found for stat {StatDefinition.Id}, trying to get {((iconSize != E_IconSize.Small) ? E_IconSize.Small : E_IconSize.VerySmall).ToString()}.", (LogType)3, (CLogLevel)0, true, "StaticLog", false);
			iconImage.sprite = GetStatIconSprite(StatDefinition.Id, (iconSize != E_IconSize.Small) ? E_IconSize.Small : E_IconSize.VerySmall);
			if ((Object)(object)iconImage.sprite == (Object)null)
			{
				CLoggerManager.Log((object)$"{((iconSize == E_IconSize.Small) ? E_IconSize.VerySmall.ToString() : E_IconSize.Small.ToString())} size has not been found for stat {StatDefinition.Id}.", (LogType)3, (CLogLevel)0, true, "StaticLog", false);
			}
		}
		((Behaviour)iconImage).enabled = (Object)(object)iconImage.sprite != (Object)null;
	}

	private void RefreshName()
	{
		if (!((Object)(object)statTitleText == (Object)null))
		{
			string text = (showShortName ? StatDefinition.ShortName : StatDefinition.Name);
			if (showTitleInUpperCase)
			{
				text = text.ToUpper();
			}
			((TMP_Text)statTitleText).text = text;
		}
	}

	protected virtual void RefreshValues()
	{
		//IL_0302: Unknown result type (might be due to invalid IL or missing references)
		//IL_039c: Unknown result type (might be due to invalid IL or missing references)
		//IL_038f: Unknown result type (might be due to invalid IL or missing references)
		if (((Object)(object)statValueText == (Object)null && (Object)(object)breakDownPanel == (Object)null) || !TargetUnit.UnitStatsController.UnitStats.Stats.ContainsKey(statDefinition.Id))
		{
			return;
		}
		if (!valuesCached)
		{
			CacheStatValues();
		}
		if ((Object)(object)statValueText != (Object)null)
		{
			float num = (useFinalValue ? TargetUnit.UnitStatsController.GetStat(statDefinition.Id).FinalClamped : TargetUnit.UnitStatsController.GetStat(statDefinition.Id).Base);
			if (statDefinition.Id == UnitStatDefinition.E_Stat.Health && TargetUnit is EnemyUnit enemyUnit && enemyUnit.EnemyUnitTemplateDefinition.IsInvulnerable)
			{
				((TMP_Text)statValueText).text = AtlasIcons.InvulnerableIcon + " " + Localizer.Get("Enemy_Invincible");
			}
			else if (SecondaryStatDefinition != null && (showStatMax || TPSingleton<SettingsManager>.Instance.Settings.AlwaysDisplayMaxStatValue))
			{
				((TMP_Text)statValueText).text = $"{num}/{secondaryFinalValue}";
			}
			else
			{
				SkillAction skillAction = PlayableUnitManager.SelectedSkill?.SkillAction;
				if (!IsDisabled && UseSelectedSkill && skillAction != null)
				{
					if (statDefinition.Id == UnitStatDefinition.E_Stat.Dodge && !TargetUnit.IsStunned)
					{
						float num2 = 0f;
						if (skillAction.Skill.Owner is PlayableUnit playableUnit)
						{
							num2 += playableUnit.GetClampedStatValue(UnitStatDefinition.E_Stat.Accuracy);
							num2 += playableUnit.GetPerkModifierForComputationStat(TheLastStand.Model.Skill.Skill.E_ComputationStat.Accuracy, skillAction.PerkDataContainer);
						}
						float selectedSkillDodgeMultiplierWithDistance = SkillManager.GetSelectedSkillDodgeMultiplierWithDistance();
						num = num * selectedSkillDodgeMultiplierWithDistance - num2;
					}
					else if (statDefinition.Id == UnitStatDefinition.E_Stat.Resistance)
					{
						float num3 = 0f;
						float num4 = 0f;
						float num5 = 0f;
						float num6 = 0f;
						if (skillAction.Skill.Owner is TheLastStand.Model.Unit.Unit unit)
						{
							num4 = unit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.PercentageResistanceReduction).ClampStatValue(unit.UnitStatsController.GetStat(UnitStatDefinition.E_Stat.PercentageResistanceReduction).FinalClamped + (skillAction.Skill.IsMagicalDamage ? UnitDatabase.MagicDamagePercentageResistanceReduction : 0f));
							if (skillAction.Skill.Owner is PlayableUnit playableUnit2)
							{
								num3 += playableUnit2.GetClampedStatValue(UnitStatDefinition.E_Stat.ResistanceReduction);
								num5 = playableUnit2.GetPerkModifierForComputationStat(TheLastStand.Model.Skill.Skill.E_ComputationStat.FlatResistanceReduction, skillAction.PerkDataContainer);
								num6 = playableUnit2.GetPerkModifierForComputationStat(TheLastStand.Model.Skill.Skill.E_ComputationStat.PercentageResistanceReduction, skillAction.PerkDataContainer);
							}
						}
						num = TargetUnit.GetReducedResistance(num3 + num5, num4 + num6);
					}
				}
				num = TPHelpers.Clamp(num, TargetUnit.UnitStatsController.GetStat(statDefinition.Id).Boundaries);
				((TMP_Text)statValueText).text = string.Format("{0}{1}{2}", (showAsModifier && num >= 0f) ? "+" : string.Empty, IsDisabled ? 0f : ((float)Math.Round(num)), statDefinition.Id.ShownAsPercentage() ? "<size=80%>%</size>" : string.Empty);
			}
			if (tintValueLabelIfDisabled)
			{
				((Graphic)statValueText).color = (IsDisabled ? disabledColor._Color : valueTextOriginalColor);
			}
		}
		if ((Object)(object)iconImage != (Object)null && (Object)(object)iconImage.sprite != (Object)null && (Object)(object)disableBarImage != (Object)null)
		{
			((Behaviour)disableBarImage).enabled = IsDisabled;
		}
		breakDownPanel?.Refresh(SecondaryStatDefinition ?? StatDefinition, TargetUnit);
		if ((Object)(object)gaugeImage != (Object)null)
		{
			RefreshGaugeValue();
		}
		if (useMarkers)
		{
			gaugeMarkersDisplayer.RefreshMarkers(secondaryFinalValue, TargetUnit.UnitStatsController.GetStat(statDefinition.Id).FinalClamped, markerWidth);
		}
	}

	protected virtual void RefreshGaugeValue()
	{
		gaugeImage.fillAmount = TargetUnit.UnitStatsController.GetStat(statDefinition.Id).FinalClamped / secondaryFinalValue;
	}

	private IEnumerator Start()
	{
		yield return GameManager.WaitForGameInit;
		Refresh(forceFullRefresh: true);
	}

	[ContextMenu("Force Full Refresh")]
	private void DBG_ForceFullRefresh()
	{
		BackupOriginalColors();
		Refresh(forceFullRefresh: true);
	}
}
