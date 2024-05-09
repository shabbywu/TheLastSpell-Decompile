using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Localization.Fonts;
using TheLastStand.Manager.Skill;
using TheLastStand.Model.Unit.Perk;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.HUD.UnitManagement;

public class ModifiersLayoutViewPerksView : MonoBehaviour
{
	[SerializeField]
	private SimpleFontLocalizedParent simpleFontLocalizedParent;

	[SerializeField]
	private RectTransform perksParent;

	[SerializeField]
	private LayoutGroup perksLayoutGroup;

	[SerializeField]
	private PerkIconDisplay perkIconDisplayPrefab;

	[SerializeField]
	private RectTransform perksBackground;

	[SerializeField]
	private RectTransform perksHover;

	[SerializeField]
	private int perkBackgroundWidthBase = 50;

	[SerializeField]
	private int perkBackgroundWidthIncrement = 43;

	[SerializeField]
	private LayoutNavigationInitializer perksLayoutNavigationInitializer;

	public readonly List<PerkIconDisplay> PerkIconDisplays = new List<PerkIconDisplay>();

	public bool IsDisplayed()
	{
		return PerkIconDisplays.Any((PerkIconDisplay o) => ((Component)o).gameObject.activeSelf);
	}

	public void HideAllPerkDisplays()
	{
		for (int i = 0; i < PerkIconDisplays.Count; i++)
		{
			PerkIconDisplays[i].Hide();
		}
	}

	public List<Selectable> GetSelectables()
	{
		List<Selectable> list = new List<Selectable>();
		for (int i = 0; i < PerkIconDisplays.Count; i++)
		{
			if (((Component)PerkIconDisplays[i]).gameObject.activeSelf)
			{
				list.Add((Selectable)(object)PerkIconDisplays[i].JoystickSelectable);
			}
		}
		return list;
	}

	public PerkIconDisplay GetFarRightPerkIconDisplay()
	{
		for (int num = PerkIconDisplays.Count - 1; num >= 0; num--)
		{
			if (((Component)PerkIconDisplays[num]).gameObject.activeSelf)
			{
				return PerkIconDisplays[num];
			}
		}
		return null;
	}

	public void Refresh(List<Perk> perks)
	{
		RefreshPerks(perks);
		if ((Object)(object)simpleFontLocalizedParent != (Object)null)
		{
			((FontLocalizedParent)simpleFontLocalizedParent).RefreshChilds();
		}
	}

	public void RefreshJoystickNavigation()
	{
		perksLayoutNavigationInitializer.InitNavigation(reset: true);
	}

	private void RefreshPerks(List<Perk> perks)
	{
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		if (perks != null && perks.Count > 0)
		{
			for (int i = 0; i < PerkIconDisplays.Count; i++)
			{
				PerkIconDisplays[i].Hide();
			}
			int num = 0;
			foreach (Perk perk in perks)
			{
				if (perk.Unlocked && perk.DisplayInHUD(out var greyedOut))
				{
					AdjustPerksPoolLength(num);
					PerkIconDisplays[num].Display(perk, greyedOut);
					PerkIconDisplays[num].DisplayHighlightSign(show: false, triggerEvent: false);
					num++;
				}
			}
			if (num > 0)
			{
				((Component)perksParent).gameObject.SetActive(true);
				perksBackground.sizeDelta = new Vector2((float)(perkBackgroundWidthBase + perkBackgroundWidthIncrement * (num - 1)), perksBackground.sizeDelta.y);
			}
			else
			{
				((Component)perksParent).gameObject.SetActive(false);
			}
		}
		else
		{
			((Component)perksParent).gameObject.SetActive(false);
			HideAllPerkDisplays();
		}
	}

	private void AdjustPerksPoolLength(int perkIndex)
	{
		if (perkIndex > PerkIconDisplays.Count - 1)
		{
			PerkIconDisplay perkIconDisplay = Object.Instantiate<PerkIconDisplay>(perkIconDisplayPrefab, ((Component)perksLayoutGroup).transform);
			simpleFontLocalizedParent.AddChilds(perkIconDisplay.LocalizedFonts);
			PerkIconDisplays.Add(perkIconDisplay);
			perkIconDisplay.Hovered += OnPerkIconHovered;
			perkIconDisplay.Unhovered += OnPerkIconUnhovered;
		}
		((Transform)perksHover).SetAsLastSibling();
	}

	private void Awake()
	{
		PerkIconDisplay.HighlightSignDisplayed += OnPerkIconHighlightSignDisplayed;
		SkillManager.AttackInfoPanel.PerkIconHidden += OnSkillActionTooltipPerkIconHidden;
		SkillManager.GenericActionInfoPanel.PerkIconHidden += OnSkillActionTooltipPerkIconHidden;
	}

	private void OnSkillActionTooltipPerkIconHidden(PerkIconDisplay perkIconDisplay)
	{
		for (int num = PerkIconDisplays.Count - 1; num >= 0; num--)
		{
			if (!(PerkIconDisplays[num].Perk.PerkDefinition.Id != perkIconDisplay.Perk.PerkDefinition.Id))
			{
				PerkIconDisplays[num].DisplayHighlightSign(show: false, triggerEvent: false);
			}
		}
	}

	private void OnDestroy()
	{
		PerkIconDisplay.HighlightSignDisplayed -= OnPerkIconHighlightSignDisplayed;
		foreach (PerkIconDisplay perkIconDisplay in PerkIconDisplays)
		{
			if ((Object)(object)perkIconDisplay != (Object)null)
			{
				perkIconDisplay.Hovered -= OnPerkIconHovered;
				perkIconDisplay.Unhovered -= OnPerkIconUnhovered;
			}
		}
		if (TPSingleton<SkillManager>.Exist())
		{
			SkillManager.AttackInfoPanel.PerkIconHidden -= OnSkillActionTooltipPerkIconHidden;
			SkillManager.GenericActionInfoPanel.PerkIconHidden -= OnSkillActionTooltipPerkIconHidden;
		}
	}

	private void OnPerkIconHighlightSignDisplayed(PerkIconDisplay perkIconDisplay)
	{
		for (int num = PerkIconDisplays.Count - 1; num >= 0; num--)
		{
			if (!(PerkIconDisplays[num].Perk.PerkDefinition.Id != perkIconDisplay.Perk.PerkDefinition.Id))
			{
				PerkIconDisplays[num].DisplayHighlightSign(show: true, triggerEvent: false);
			}
		}
	}

	private void OnPerkIconHovered(PerkIconDisplay perkIconDisplay)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		((Component)perksHover).gameObject.SetActive(true);
		_003F val = perksHover;
		Transform transform = ((Component)perkIconDisplay).transform;
		((Transform)val).position = ((transform is RectTransform) ? transform : null).position;
	}

	private void OnPerkIconUnhovered(PerkIconDisplay perkIconDisplay)
	{
		((Component)perksHover).gameObject.SetActive(false);
	}
}
