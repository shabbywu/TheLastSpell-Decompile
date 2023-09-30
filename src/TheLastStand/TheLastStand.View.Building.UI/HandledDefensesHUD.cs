using Sirenix.OdinInspector;
using TMPro;
using TPLib.Localization.Fonts;
using TheLastStand.Model.Building;
using TheLastStand.Model.Skill;
using TheLastStand.View.Camera;
using TheLastStand.View.Generic;
using UnityEngine;

namespace TheLastStand.View.Building.UI;

public class HandledDefensesHUD : SerializedMonoBehaviour
{
	public static class Constants
	{
		public const string TrapUseTag = "<sprite name=\"TrapUse\">";

		public const string OverallUseTag = "<sprite name=\"UsePerNight\">";
	}

	[SerializeField]
	private Canvas chargesCanvas;

	[SerializeField]
	private TextMeshProUGUI chargesText;

	[SerializeField]
	private Canvas overallUsesCanvas;

	[SerializeField]
	private TextMeshProUGUI OverallUsesText;

	[SerializeField]
	private FollowElement followElement;

	[SerializeField]
	private SimpleFontLocalizedParent simpleFontLocalizedParent;

	private TheLastStand.Model.Building.Building building;

	public TheLastStand.Model.Building.Building Building
	{
		get
		{
			return building;
		}
		set
		{
			if (value != null)
			{
				building = value;
				((Object)this).name = building.UniqueIdentifier + " HandledDefenses HUD";
				if ((Object)(object)followElement != (Object)null)
				{
					followElement.ChangeTarget(Building.BuildingView.HudFollowTarget);
					followElement.AutoMove();
				}
			}
		}
	}

	private void Awake()
	{
		if ((Object)(object)chargesCanvas != (Object)null)
		{
			chargesCanvas.worldCamera = ACameraView.MainCam;
		}
		if ((Object)(object)overallUsesCanvas != (Object)null)
		{
			overallUsesCanvas.worldCamera = ACameraView.MainCam;
		}
	}

	public void DisplayHandledDefensesUses(bool state)
	{
		string text = null;
		TextMeshProUGUI textToFill = null;
		Canvas canvasToActivate = null;
		if (building.IsTrap)
		{
			RetrieveDataForTrapCharges(state, out text, out textToFill, out canvasToActivate);
		}
		else if (building.IsHandledDefense)
		{
			RetrieveDataForOverallUses(state, out text, out textToFill, out canvasToActivate);
		}
		if (!string.IsNullOrEmpty(text) && (Object)(object)textToFill != (Object)null && (Object)(object)canvasToActivate != (Object)null)
		{
			((Component)canvasToActivate).gameObject.SetActive(true);
			((TMP_Text)textToFill).text = text;
			SimpleFontLocalizedParent obj = simpleFontLocalizedParent;
			if (obj != null)
			{
				((FontLocalizedParent)obj).RefreshChilds();
			}
		}
		else
		{
			((Component)chargesCanvas).gameObject.SetActive(false);
			((Component)overallUsesCanvas).gameObject.SetActive(false);
		}
	}

	public void RefreshPositionInstantly()
	{
		followElement.AutoMove();
	}

	private void RetrieveDataForOverallUses(bool state, out string text, out TextMeshProUGUI textToFill, out Canvas canvasToActivate)
	{
		int num = -1;
		int num2 = -1;
		foreach (TheLastStand.Model.Skill.Skill skill in Building.BattleModule.Skills)
		{
			if (skill != null && skill.OverallUses != 0)
			{
				num = skill.OverallUsesRemaining;
				num2 = skill.OverallUses;
				break;
			}
		}
		if (state && num != -1 && num2 != -1)
		{
			text = string.Format("{0}<style=Skill>{1}/{2}</style>", "<sprite name=\"UsePerNight\">", num, num2);
			textToFill = OverallUsesText;
			canvasToActivate = overallUsesCanvas;
			((Component)chargesCanvas).gameObject.SetActive(false);
		}
		else
		{
			text = string.Empty;
			textToFill = null;
			canvasToActivate = null;
		}
	}

	private void RetrieveDataForTrapCharges(bool state, out string text, out TextMeshProUGUI textToFill, out Canvas canvasToActivate)
	{
		int remainingTrapCharges = building.BattleModule.RemainingTrapCharges;
		int maximumTrapCharges = building.BattleModule.BattleModuleDefinition.MaximumTrapCharges;
		if (state && remainingTrapCharges != -1 && maximumTrapCharges > 0)
		{
			text = string.Format("{0}{1}/{2}", "<sprite name=\"TrapUse\">", remainingTrapCharges, maximumTrapCharges);
			textToFill = chargesText;
			canvasToActivate = chargesCanvas;
			((Component)overallUsesCanvas).gameObject.SetActive(false);
		}
		else
		{
			text = string.Empty;
			textToFill = null;
			canvasToActivate = null;
		}
	}
}
