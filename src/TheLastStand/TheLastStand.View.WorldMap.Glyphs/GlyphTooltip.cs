using System.Collections.Generic;
using TMPro;
using TPLib;
using TheLastStand.Definition.Meta.Glyphs;
using TheLastStand.Manager.Meta;
using TheLastStand.View.Generic;
using TheLastStand.View.Unit.Perk;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.WorldMap.Glyphs;

public class GlyphTooltip : TooltipBase
{
	[SerializeField]
	private PerkTooltip perkTooltip;

	[SerializeField]
	private Image glyphIcon;

	[SerializeField]
	private Image glyphCustomModeIcon;

	[SerializeField]
	private TextMeshProUGUI glyphTitle;

	[SerializeField]
	private TextMeshProUGUI glyphDescription;

	[SerializeField]
	private GameObject glyphCostIcon;

	[SerializeField]
	private RectTransform glyphCostIconsContainer;

	private bool displayTowardsRight;

	private GlyphDefinition glyphDefinition;

	private List<GameObject> glyphIcons = new List<GameObject>();

	protected override void Awake()
	{
		base.Awake();
		TPSingleton<GlyphManager>.Instance.GlyphTooltip = this;
	}

	public void Init(GlyphDefinition newGlyphDefinition, bool newDisplayTowardsRight = true)
	{
		displayTowardsRight = newDisplayTowardsRight;
		glyphDefinition = newGlyphDefinition;
	}

	protected override bool CanBeDisplayed()
	{
		return glyphDefinition != null;
	}

	protected override void OnHide()
	{
		perkTooltip.Hide();
	}

	protected override void RefreshContent()
	{
		((TMP_Text)glyphTitle).text = glyphDefinition.GetName();
		((TMP_Text)glyphDescription).text = glyphDefinition.GetDescription(null);
		glyphIcon.sprite = AGlyphDisplay.GetGlyphIcon(glyphDefinition);
		((Behaviour)glyphCustomModeIcon).enabled = glyphDefinition.IsCustom;
		while (glyphIcons.Count > glyphDefinition.Cost)
		{
			Object.Destroy((Object)(object)glyphIcons[0]);
			glyphIcons.RemoveAt(0);
		}
		while (glyphIcons.Count < glyphDefinition.Cost)
		{
			glyphIcons.Add(Object.Instantiate<GameObject>(glyphCostIcon, (Transform)(object)glyphCostIconsContainer));
		}
		if (glyphDefinition.PerkToShow != null)
		{
			perkTooltip.SetContent(null, glyphDefinition.PerkToShow);
			perkTooltip.Display();
		}
		else
		{
			perkTooltip.Hide();
		}
		RefreshAnchors();
	}

	private void RefreshAnchors()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val;
		if (displayTowardsRight)
		{
			((Component)perkTooltip).transform.SetAsLastSibling();
			val = Vector2.up;
		}
		else
		{
			((Component)perkTooltip).transform.SetAsFirstSibling();
			val = Vector2.one;
		}
		rectTransform.anchorMin = val;
		rectTransform.anchorMax = val;
		rectTransform.pivot = val;
		perkTooltip.UpdateAnchors(displayTowardsRight);
	}
}
