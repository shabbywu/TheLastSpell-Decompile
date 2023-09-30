using TMPro;
using TPLib;
using TheLastStand.Controller.Unit;
using TheLastStand.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Recruitment;

public class RecruitmentMageDisplay : RecruitDisplay
{
	[SerializeField]
	private TextMeshProUGUI mageTitle;

	[SerializeField]
	private TextMeshProUGUI noMageTitle;

	[SerializeField]
	private TextMeshProUGUI noMageSubtitle;

	[SerializeField]
	private Image costIcon;

	[SerializeField]
	private TextMeshProUGUI costText;

	[SerializeField]
	private Toggle mageToggle;

	[SerializeField]
	private Image mageImage;

	[SerializeField]
	private Sprite mageNormalAvailableSprite;

	[SerializeField]
	private Sprite mageNormalUnavailableSprite;

	[SerializeField]
	private Sprite mageHoverAvailableSprite;

	[SerializeField]
	private Sprite mageHoverUnavailableSprite;

	public override Toggle Toggle => mageToggle;

	public void Refresh(bool hasMage)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		SpriteState spriteState = ((Selectable)mageToggle).spriteState;
		((SpriteState)(ref spriteState)).highlightedSprite = (hasMage ? mageHoverAvailableSprite : mageHoverUnavailableSprite);
		((Selectable)mageToggle).spriteState = spriteState;
		mageImage.sprite = (hasMage ? mageNormalAvailableSprite : mageNormalUnavailableSprite);
		((Behaviour)mageTitle).enabled = hasMage;
		((Behaviour)noMageTitle).enabled = !hasMage;
		((Behaviour)noMageSubtitle).enabled = !hasMage;
		((TMP_Text)costText).text = (hasMage ? $"{RecruitmentController.ComputeMageCost()}" : string.Empty);
		((Behaviour)costIcon).enabled = hasMage;
		RefreshButtonDisplay();
	}

	public void RefreshButtonDisplay()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)costText).color = ((RecruitmentController.ComputeMageCost() <= TPSingleton<ResourceManager>.Instance.Gold) ? Color.white : Color.red);
	}

	protected override void OnUnitToggleValueChanged(bool value)
	{
		if (InputManager.IsLastControllerJoystick)
		{
			base.OnUnitToggleValueChanged(value);
			if (value)
			{
				TPSingleton<RecruitmentView>.Instance.SelectRecruitButton();
			}
			else
			{
				RecruitmentView.WarningTooltip.Hide();
			}
		}
	}
}
