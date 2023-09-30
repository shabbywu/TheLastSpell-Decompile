using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Log;
using TPLib.Yield;
using TheLastStand.Database;
using TheLastStand.Definition.Skill.SkillAction;
using TheLastStand.Definition.Skill.SkillEffect;
using TheLastStand.Definition.Tooltip.Compendium;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Model.Skill.SkillAction;
using TheLastStand.View.Generic;
using TheLastStand.View.Tooltip.Compendium;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Tooltip.Tooltip.Compendium;

public class CompendiumPanel : TooltipBase
{
	public enum AnchorType
	{
		LeftTop,
		LeftBot,
		RightTop,
		RightBot
	}

	public static class Constants
	{
		public const string SkillEffectEntryPrefab = "Prefab/Tooltip/Skill Effect Tooltip";

		public const string GameConceptEntryPrefab = "Prefab/Tooltip/Game Concept Tooltip";

		public const string AttackTypeEntryPrefab = "Prefab/Tooltip/Attack Type Tooltip";

		public const string CompendiumEntriesPath = "Prefab/Tooltip/";
	}

	[SerializeField]
	private RectTransform firstColumnParent;

	[SerializeField]
	private RectTransform secondColumnParent;

	[SerializeField]
	private HorizontalLayoutGroup columnsLayout;

	[SerializeField]
	private VerticalLayoutGroup firstColumnLayout;

	[SerializeField]
	private VerticalLayoutGroup secondColumnLayout;

	[SerializeField]
	private int compendiumEntriesLimitByColumn = 5;

	private RectTransform parentRectTransform;

	private List<CompendiumEntryTooltip> compendiumEntryTooltips = new List<CompendiumEntryTooltip>();

	private bool usePooler;

	public HashSet<ACompendiumEntryDefinition> CompendiumEntries { get; private set; } = new HashSet<ACompendiumEntryDefinition>();


	public bool IsTwoColumnsWide { get; private set; }

	public void AddCompendiumEntry(string compendiumEntryId, bool includeLinkedEntries)
	{
		if (!TooltipDatabase.CompendiumDefinition.CompendiumEntryDefinitions.TryGetValue(compendiumEntryId, out var value))
		{
			CLoggerManager.Log((object)("Compendium entry id " + compendiumEntryId + " doesn't exist in the database. Abort."), (LogType)0, (CLogLevel)2, true, "CompendiumPanel", false);
		}
		else
		{
			if (CompendiumEntries.Contains(value))
			{
				return;
			}
			CompendiumEntries.Add(value);
			if (!includeLinkedEntries || value.LinkedEntries == null || value.LinkedEntries.Count <= 0)
			{
				return;
			}
			foreach (string linkedEntry in value.LinkedEntries)
			{
				AddCompendiumEntry(linkedEntry, includeLinkedEntries: true);
			}
		}
	}

	public void AddDamageType(AttackSkillAction attackSkillAction)
	{
		if (attackSkillAction.AttackSkillActionDefinition.AttackType == AttackSkillActionDefinition.E_AttackType.Adaptative)
		{
			AddCompendiumEntry(TooltipDatabase.CompendiumDefinition.AttackTypeAliases[AttackSkillActionDefinition.E_AttackType.Adaptative.ToString()], includeLinkedEntries: true);
		}
		AddCompendiumEntry(TooltipDatabase.CompendiumDefinition.AttackTypeAliases[attackSkillAction.AttackType.ToString()], includeLinkedEntries: true);
	}

	public void AddSkillEffectIds(Dictionary<string, List<SkillEffectDefinition>> skillEffectDefinitions)
	{
		foreach (KeyValuePair<string, List<SkillEffectDefinition>> skillEffectDefinition2 in skillEffectDefinitions)
		{
			SkillEffectDefinition? obj = skillEffectDefinition2.Value?.FirstOrDefault();
			if (obj != null && obj.DisplayCompendiumEntry)
			{
				AddCompendiumEntry(TooltipDatabase.CompendiumDefinition.SkillEffectAliases[skillEffectDefinition2.Key], includeLinkedEntries: true);
			}
			if (!(skillEffectDefinition2.Key == "SurroundingEffect") && !(skillEffectDefinition2.Key == "CasterEffect"))
			{
				continue;
			}
			int i = 0;
			for (int count = skillEffectDefinition2.Value.Count; i < count; i++)
			{
				SkillEffectDefinition skillEffectDefinition = skillEffectDefinition2.Value[i];
				List<SkillEffectDefinition> skillEffectDefinitions2;
				if (!(skillEffectDefinition is CasterEffectDefinition casterEffectDefinition))
				{
					if (!(skillEffectDefinition is SurroundingEffectDefinition surroundingEffectDefinition))
					{
						continue;
					}
					skillEffectDefinitions2 = surroundingEffectDefinition.SkillEffectDefinitions;
				}
				else
				{
					skillEffectDefinitions2 = casterEffectDefinition.SkillEffectDefinitions;
				}
				foreach (SkillEffectDefinition item in skillEffectDefinitions2)
				{
					if (item.DisplayCompendiumEntry)
					{
						AddCompendiumEntry(TooltipDatabase.CompendiumDefinition.SkillEffectAliases[item.Id], includeLinkedEntries: true);
					}
				}
			}
		}
	}

	public void Clear()
	{
		CompendiumEntries.Clear();
		ClearCompendiumEntryTooltips();
	}

	public void UpdateAnchor(AnchorType anchorType)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val;
		switch (anchorType)
		{
		case AnchorType.LeftTop:
			val = Vector2.up;
			((LayoutGroup)firstColumnLayout).childAlignment = (TextAnchor)0;
			((LayoutGroup)secondColumnLayout).childAlignment = (TextAnchor)0;
			((LayoutGroup)columnsLayout).childAlignment = (TextAnchor)0;
			((HorizontalOrVerticalLayoutGroup)columnsLayout).reverseArrangement = false;
			break;
		case AnchorType.LeftBot:
			val = Vector2.zero;
			((LayoutGroup)firstColumnLayout).childAlignment = (TextAnchor)6;
			((LayoutGroup)secondColumnLayout).childAlignment = (TextAnchor)6;
			((LayoutGroup)columnsLayout).childAlignment = (TextAnchor)6;
			((HorizontalOrVerticalLayoutGroup)columnsLayout).reverseArrangement = false;
			break;
		case AnchorType.RightTop:
			val = Vector2.one;
			((LayoutGroup)firstColumnLayout).childAlignment = (TextAnchor)2;
			((LayoutGroup)secondColumnLayout).childAlignment = (TextAnchor)2;
			((LayoutGroup)columnsLayout).childAlignment = (TextAnchor)2;
			((HorizontalOrVerticalLayoutGroup)columnsLayout).reverseArrangement = true;
			break;
		case AnchorType.RightBot:
			val = Vector2.right;
			((LayoutGroup)firstColumnLayout).childAlignment = (TextAnchor)8;
			((LayoutGroup)secondColumnLayout).childAlignment = (TextAnchor)8;
			((LayoutGroup)columnsLayout).childAlignment = (TextAnchor)8;
			((HorizontalOrVerticalLayoutGroup)columnsLayout).reverseArrangement = true;
			break;
		default:
			val = Vector2.zero;
			break;
		}
		if ((Object)(object)parentRectTransform == (Object)null)
		{
			parentRectTransform = ((Component)this).GetComponent<RectTransform>();
		}
		parentRectTransform.pivot = val;
		parentRectTransform.anchorMin = val;
		parentRectTransform.anchorMax = val;
	}

	protected override void Awake()
	{
		base.Awake();
		usePooler = (Object)(object)SingletonBehaviour<ObjectPooler>.Instance != (Object)null;
		parentRectTransform = ((Component)this).GetComponent<RectTransform>();
	}

	protected override bool CanBeDisplayed()
	{
		if (CompendiumEntries.Count > 0)
		{
			return !TPSingleton<SettingsManager>.Instance.Settings.HideCompendium;
		}
		return false;
	}

	protected override void OnHide()
	{
		Clear();
		base.OnHide();
	}

	protected override void RefreshContent()
	{
		if ((Object)(object)parentRectTransform == (Object)null)
		{
			return;
		}
		ClearCompendiumEntryTooltips();
		RefreshSecondColumn();
		int num = 0;
		foreach (ACompendiumEntryDefinition compendiumEntry in CompendiumEntries)
		{
			InstantiateCompendiumEntryTooltip(compendiumEntry, (num++ < compendiumEntriesLimitByColumn) ? firstColumnParent : secondColumnParent);
		}
	}

	protected override void RefreshLayout(bool showInstantly = false)
	{
		LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
	}

	protected override IEnumerator RefreshLayoutDelayed()
	{
		yield return SharedYields.WaitForEndOfFrame;
		yield return base.RefreshLayoutDelayed();
	}

	private void ClearCompendiumEntryTooltips()
	{
		if (usePooler)
		{
			for (int num = compendiumEntryTooltips.Count - 1; num >= 0; num--)
			{
				((Component)compendiumEntryTooltips[num]).gameObject.SetActive(false);
			}
		}
		else
		{
			for (int num2 = compendiumEntryTooltips.Count - 1; num2 >= 0; num2--)
			{
				Object.Destroy((Object)(object)((Component)compendiumEntryTooltips[num2]).gameObject);
			}
		}
		compendiumEntryTooltips.Clear();
	}

	private void InstantiateCompendiumEntryTooltip(ACompendiumEntryDefinition compendiumEntry, RectTransform parent)
	{
		CompendiumEntryTooltip compendiumEntryTooltip;
		if (!(compendiumEntry is SkillEffectEntryDefinition skillEffectEntryDefinition))
		{
			if (!(compendiumEntry is AttackTypeEntryDefinition attackTypeEntryDefinition))
			{
				if (!(compendiumEntry is GameConceptEntryDefinition gameConceptEntryDefinition))
				{
					return;
				}
				GameConceptTooltip obj = (usePooler ? ObjectPooler.GetPooledComponent<GameConceptTooltip>("CompendiumGameConceptEntry", ResourcePooler.LoadOnce<GameConceptTooltip>("Prefab/Tooltip/Game Concept Tooltip", false), (Transform)(object)parent, false) : Object.Instantiate<GameConceptTooltip>(ResourcePooler.LoadOnce<GameConceptTooltip>("Prefab/Tooltip/Game Concept Tooltip", false), (Transform)(object)parent));
				obj.GameConceptId = gameConceptEntryDefinition.GameConceptId;
				compendiumEntryTooltip = obj;
			}
			else
			{
				AttackTypeTooltip obj2 = (usePooler ? ObjectPooler.GetPooledComponent<AttackTypeTooltip>("CompendiumAttackTypeEntry", ResourcePooler.LoadOnce<AttackTypeTooltip>("Prefab/Tooltip/Attack Type Tooltip", false), (Transform)(object)parent, false) : Object.Instantiate<AttackTypeTooltip>(ResourcePooler.LoadOnce<AttackTypeTooltip>("Prefab/Tooltip/Attack Type Tooltip", false), (Transform)(object)parent));
				obj2.AttackType = attackTypeEntryDefinition.AttackType;
				compendiumEntryTooltip = obj2;
			}
		}
		else
		{
			SkillEffectTooltip obj3 = (usePooler ? ObjectPooler.GetPooledComponent<SkillEffectTooltip>("CompendiumSkillEffectEntry", ResourcePooler.LoadOnce<SkillEffectTooltip>("Prefab/Tooltip/Skill Effect Tooltip", false), (Transform)(object)parent, false) : Object.Instantiate<SkillEffectTooltip>(ResourcePooler.LoadOnce<SkillEffectTooltip>("Prefab/Tooltip/Skill Effect Tooltip", false), (Transform)(object)parent));
			obj3.SkillEffectId = skillEffectEntryDefinition.SkillEffectId;
			compendiumEntryTooltip = obj3;
		}
		compendiumEntryTooltips.Add(compendiumEntryTooltip);
		((Component)compendiumEntryTooltip).transform.SetSiblingIndex(compendiumEntryTooltips.Count);
		compendiumEntryTooltip.Display();
	}

	private void RefreshSecondColumn()
	{
		if (CompendiumEntries.Count > compendiumEntriesLimitByColumn)
		{
			((Component)secondColumnParent).gameObject.SetActive(true);
			IsTwoColumnsWide = true;
		}
		else
		{
			((Component)secondColumnParent).gameObject.SetActive(false);
			IsTwoColumnsWide = false;
		}
	}
}
