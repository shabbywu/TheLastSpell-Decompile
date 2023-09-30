using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Definition;
using TheLastStand.Definition.Skill;
using TheLastStand.Definition.Skill.SkillEffect;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.Database;
using UnityEngine;

namespace TheLastStand.Database.Unit;

public class SkillDatabase : Database<SkillDatabase>
{
	[SerializeField]
	private TextAsset[] groupSkillDefinitions;

	[SerializeField]
	private TextAsset[] individualSkillDefinitions;

	[SerializeField]
	private TextAsset lineOfSightDefinition;

	[SerializeField]
	private TextAsset damageTypeModifiersDefinition;

	[SerializeField]
	private TextAsset poisonDamageScaleDefinition;

	public static ArmorPiercingEffectDefinition ArmorPiercingEffectSharedDefinition { get; private set; }

	public static DamageTypeModifiersDefinition DamageTypeModifiersDefinition { get; private set; }

	public static List<string> ContextualSkills { get; private set; }

	public static LineOfSightDefinition LineOfSightDefinition { get; private set; }

	public static NoBlockEffectDefinition NoBlockEffectSharedDefinition { get; private set; }

	public static NoDodgeEffectDefinition NoDodgeEffectSharedDefinition { get; private set; }

	public static PoisonDamageScaleDefinition PoisonDamageScaleDefinition { get; private set; }

	public static Dictionary<string, SkillDefinition> SkillDefinitions { get; private set; }

	public override void Deserialize(XContainer container = null)
	{
		DeserializeSkills();
		DeserializeLineOfSight();
		DeserializeDamageTypeModifiers();
		DeserializePoisonDamageScale();
		ArmorPiercingEffectSharedDefinition = new ArmorPiercingEffectDefinition(null);
		NoBlockEffectSharedDefinition = new NoBlockEffectDefinition(null);
		NoDodgeEffectSharedDefinition = new NoDodgeEffectDefinition(null);
	}

	private void DeserializeDamageTypeModifiers()
	{
		if (DamageTypeModifiersDefinition == null)
		{
			DamageTypeModifiersDefinition = new DamageTypeModifiersDefinition((XContainer)(object)XDocument.Parse(damageTypeModifiersDefinition.text, (LoadOptions)2));
		}
	}

	private void DeserializeLineOfSight()
	{
		if (LineOfSightDefinition == null)
		{
			LineOfSightDefinition = new LineOfSightDefinition((XContainer)(object)XDocument.Parse(lineOfSightDefinition.text, (LoadOptions)2));
		}
	}

	private void DeserializePoisonDamageScale()
	{
		if (PoisonDamageScaleDefinition == null)
		{
			PoisonDamageScaleDefinition = new PoisonDamageScaleDefinition((XContainer)(object)XDocument.Parse(poisonDamageScaleDefinition.text, (LoadOptions)2));
		}
	}

	private void DeserializeSkills()
	{
		if (SkillDefinitions != null)
		{
			return;
		}
		SkillDefinitions = new Dictionary<string, SkillDefinition>();
		ContextualSkills = new List<string>();
		Queue<XElement> queue = base.GatherElements((IEnumerable<TextAsset>)groupSkillDefinitions, (IEnumerable<TextAsset>)individualSkillDefinitions, "SkillDefinition", (string)null);
		foreach (XElement item in base.SortElementsByDependencies((IEnumerable<XElement>)queue))
		{
			SkillDefinition skillDefinition = new SkillDefinition((XContainer)(object)item);
			try
			{
				SkillDefinitions.Add(skillDefinition.Id, skillDefinition);
			}
			catch (ArgumentException)
			{
				CLoggerManager.Log((object)("Duplicate SkillDefinition found for ID " + skillDefinition.Id + ": the individual files will have PRIORITY over the all-in-one template file."), (LogType)2, (CLogLevel)1, true, "StaticLog", false);
			}
		}
		ContextualSkills = (from o in SkillDefinitions
			where o.Value.IsContextual
			select o into p
			select p.Key).ToList();
		CLoggerManager.Log((object)string.Format("Deserialized {0} skills among which {1} Contextual Skills have been found ({2}).", SkillDefinitions.Count, ContextualSkills.Count, string.Join(",", ContextualSkills)), (LogType)3, (CLogLevel)0, true, "StaticLog", false);
	}
}
