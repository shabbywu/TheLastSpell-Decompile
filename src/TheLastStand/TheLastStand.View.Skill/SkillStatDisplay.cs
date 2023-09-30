using TMPro;
using TPLib;
using TheLastStand.Definition.Unit;
using TheLastStand.Model.Skill;
using TheLastStand.View.Unit.Stat;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Skill;

public class SkillStatDisplay : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI statValueText;

	[SerializeField]
	private bool showName;

	[SerializeField]
	private TextMeshProUGUI[] textsToColor;

	[SerializeField]
	private bool useStatColor;

	[SerializeField]
	private DataColorDictionary statColors;

	[SerializeField]
	private Image iconImage;

	[SerializeField]
	private UnitStatDisplay.E_IconSize iconSize = UnitStatDisplay.E_IconSize.Small;

	protected Color[] textsOriginalColor;

	private Color? colorOverride;

	private bool fullRefreshNeeded;

	private UnitStatDefinition statDefinition;

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

	public TheLastStand.Model.Skill.Skill Skill { get; set; }

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

	public void Display(bool display)
	{
		((Component)this).gameObject.SetActive(display);
	}

	public void Refresh(bool forceFullRefresh = false)
	{
		if (StatDefinition != null && Skill != null)
		{
			fullRefreshNeeded |= forceFullRefresh;
			RefreshValue();
			if (fullRefreshNeeded)
			{
				RefreshIcon();
				RefreshColor();
				fullRefreshNeeded = false;
			}
		}
	}

	private void Awake()
	{
		BackupOriginalColors();
	}

	private void BackupOriginalColors()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		if (textsToColor == null || textsToColor.Length == 0)
		{
			return;
		}
		textsOriginalColor = (Color[])(object)new Color[textsToColor.Length];
		int i = 0;
		for (int num = textsToColor.Length; i < num; i++)
		{
			if ((Object)(object)textsToColor[i] != (Object)null)
			{
				textsOriginalColor[i] = ((Graphic)textsToColor[i]).color;
			}
		}
	}

	private void RefreshColor()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		if (textsToColor == null || textsToColor.Length == 0)
		{
			return;
		}
		Color? val = null;
		if (ColorOverride.HasValue)
		{
			val = ColorOverride.Value;
		}
		else if (useStatColor)
		{
			val = statColors.GetColorById(StatDefinition.Id.ToString());
		}
		int i = 0;
		for (int num = textsToColor.Length; i < num; i++)
		{
			if ((Object)(object)textsToColor[i] != (Object)null)
			{
				((Graphic)textsToColor[i]).color = (Color)(((_003F?)val) ?? textsOriginalColor[i]);
			}
		}
	}

	private void RefreshIcon()
	{
		if (!((Object)(object)iconImage == (Object)null))
		{
			iconImage.sprite = UnitStatDisplay.GetStatIconSprite(StatDefinition.Id, iconSize);
			((Behaviour)iconImage).enabled = (Object)(object)iconImage.sprite != (Object)null;
		}
	}

	private void RefreshValue()
	{
		if (!((Object)(object)statValueText == (Object)null))
		{
			string text = string.Empty;
			string text2 = string.Empty;
			if (showName)
			{
				text2 = StatDefinition.Name + ": ";
			}
			switch (StatDefinition.Id)
			{
			case UnitStatDefinition.E_Stat.ActionPoints:
				text = Skill.SkillAction.SkillActionController.ComputeActionPointsCost(Skill.OwnerOrSelected).ToString();
				break;
			case UnitStatDefinition.E_Stat.MovePoints:
				text = Skill.SkillAction.SkillActionController.ComputeMovePointsCost(Skill.OwnerOrSelected).ToString();
				break;
			case UnitStatDefinition.E_Stat.Mana:
				text = Skill.SkillAction.SkillActionController.ComputeManaCost(Skill.OwnerOrSelected).ToString();
				break;
			case UnitStatDefinition.E_Stat.Health:
				text = Skill.SkillAction.SkillActionController.ComputeHealthCost(Skill.OwnerOrSelected).ToString();
				break;
			}
			((TMP_Text)statValueText).text = text2 + text;
		}
	}

	private void Start()
	{
		Refresh(forceFullRefresh: true);
	}

	[ContextMenu("Force Full Refresh")]
	private void DBG_ForceFullRefresh()
	{
		BackupOriginalColors();
		Refresh(forceFullRefresh: true);
	}
}
