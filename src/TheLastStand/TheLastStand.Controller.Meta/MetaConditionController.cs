using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TPLib;
using TPLib.Localization;
using TPLib.Log;
using TheLastStand.Database;
using TheLastStand.Framework.Collections;
using TheLastStand.Manager.Meta;
using TheLastStand.Model.Meta;

namespace TheLastStand.Controller.Meta;

public class MetaConditionController
{
	private readonly IList<Type> supportedContextTypes = new List<Type>
	{
		typeof(double),
		typeof(List<string>),
		typeof(Dictionary<string, int>),
		typeof(StringIntDictionary)
	};

	private readonly string[] keywordPotentialLocalizationPrefixes = new string[12]
	{
		"BuildingName_", "BuildingActionName_", "BuildingUpgradeTooltipName_", "DLC_Name_", "EnemyName_", "ItemName_", "PerkName_", "SkillName_", "SkillEffectName_", "UnitStat_Name_",
		"WorldMap_CityName_", "LifetimeStats_"
	};

	private Dictionary<string, MetaConditionContext> contexts = new Dictionary<string, MetaConditionContext>();

	public MetaCondition MetaCondition { get; protected set; }

	public MetaConditionController(MetaCondition condition, MetaConditionSpecificContext runContext, MetaConditionSpecificContext campaignContext, MetaConditionGlobalContext globalContext)
	{
		contexts.Add("ENVIRONMENT", globalContext);
		contexts.Add("CAMPAIGN", campaignContext);
		contexts.Add("RUN", runContext);
		contexts.Add("LOCAL", condition.LocalContext);
		condition.MetaConditionController = this;
		MetaCondition = condition;
	}

	public string GetLocalizedDescription()
	{
		_ = "Localizing " + MetaCondition.Id + ":\n";
		MetaConditionsDatabase.ProgressionDatas progressionValues = GetProgressionValues(TPSingleton<MetaConditionManager>.Instance.ConditionsLibrary);
		string text = ((!string.IsNullOrEmpty(MetaCondition.MetaConditionDefinition.LocalizationKey)) ? MetaCondition.MetaConditionDefinition.LocalizationKey : ParseLocalizationKey());
		if (string.IsNullOrEmpty(text))
		{
			((CLogger<MetaConditionManager>)TPSingleton<MetaConditionManager>.Instance).LogWarning((object)("Computed localization key for MetaCondition " + MetaCondition.Id + " is null or empty."), (CLogLevel)2, true, false);
			return MetaCondition.Id;
		}
		List<string> list = ParseLocalizationArguments();
		list.Add(IsComplete() ? progressionValues.GoalValueToString() : progressionValues.ProgressionValueToString());
		object[] array = list.ToArray();
		string text2 = Localizer.Format(text, array);
		if (MetaCondition.MetaConditionDefinition.Occurences > 1)
		{
			text2 = text2 + " " + Localizer.Format("MetaCondition_Occurences", new object[2]
			{
				MetaCondition.MetaConditionDefinition.Occurences,
				MetaCondition.OccurenceProgression
			});
		}
		string text3 = MetaCondition.MetaConditionDefinition.Arguments.Find((string o) => contexts.ContainsKey(o.Split(new char[1] { ':' })[0])).Split(new char[1] { ':' })[0];
		string text4 = default(string);
		if (Localizer.TryGet("MetaCondition_ContextInfo_" + text3, ref text4))
		{
			text2 = text2 + " " + text4;
		}
		return text2;
	}

	public string LogLocalizedDescriptionBreakdown()
	{
		string text = "Localizing " + MetaCondition.Id + ":\n";
		text += "<b>Definition arguments:</b>\n";
		for (int i = 0; i < MetaCondition.MetaConditionDefinition.Arguments.Count; i++)
		{
			text = text + "- " + MetaCondition.MetaConditionDefinition.Arguments[i] + "\n";
		}
		MetaConditionsDatabase.ProgressionDatas progressionValues = GetProgressionValues(TPSingleton<MetaConditionManager>.Instance.ConditionsLibrary);
		string text2 = ((!string.IsNullOrEmpty(MetaCondition.MetaConditionDefinition.LocalizationKey)) ? MetaCondition.MetaConditionDefinition.LocalizationKey : ParseLocalizationKey());
		if (string.IsNullOrEmpty(text2))
		{
			((CLogger<MetaConditionManager>)TPSingleton<MetaConditionManager>.Instance).LogWarning((object)("Computed localization key for MetaCondition " + MetaCondition.Id + " is null or empty."), (CLogLevel)2, true, false);
			return MetaCondition.Id;
		}
		List<string> list = ParseLocalizationArguments();
		list.Add(IsComplete() ? progressionValues.GoalValueToString() : progressionValues.ProgressionValueToString());
		text = text + "<b>Key:</b> " + text2 + "\n";
		text += "<b>Localization format arguments:</b>\n";
		for (int j = 0; j < list.Count; j++)
		{
			text = text + "- " + list[j] + "\n";
		}
		object[] array = list.ToArray();
		string text3 = Localizer.Format(text2, array);
		if (MetaCondition.MetaConditionDefinition.Occurences > 1)
		{
			text3 = text3 + " " + Localizer.Format("MetaCondition_Occurences", new object[2]
			{
				MetaCondition.MetaConditionDefinition.Occurences,
				MetaCondition.OccurenceProgression
			});
		}
		text = text + "<b>RESULT =></b> " + text3;
		((CLogger<MetaConditionManager>)TPSingleton<MetaConditionManager>.Instance).Log((object)text, (CLogLevel)1, false, false);
		return text3;
	}

	public MetaConditionsDatabase.ProgressionDatas GetProgressionValues(MetaConditionsDatabase conditionsLibrary)
	{
		if (conditionsLibrary.ContainsKey(MetaCondition.MetaConditionDefinition.Name))
		{
			List<object> list = new List<object>();
			foreach (string argument2 in MetaCondition.MetaConditionDefinition.Arguments)
			{
				if (ParseArgument(argument2, out var argument))
				{
					list.Add(argument);
					continue;
				}
				((CLogger<MetaConditionManager>)TPSingleton<MetaConditionManager>.Instance).LogError((object)$"Aborted script execution of {MetaCondition.MetaConditionDefinition} due to an argument error.", (CLogLevel)0, true, true);
				return null;
			}
			return conditionsLibrary[MetaCondition.MetaConditionDefinition.Name](list.ToArray());
		}
		return null;
	}

	public bool IsComplete()
	{
		return MetaCondition.OccurenceProgression >= MetaCondition.MetaConditionDefinition.Occurences;
	}

	public void RefreshProgression(MetaConditionsDatabase conditionsLibrary)
	{
		if (IsComplete())
		{
			return;
		}
		if (conditionsLibrary.ContainsKey(MetaCondition.MetaConditionDefinition.Name))
		{
			List<object> list = new List<object>();
			foreach (string argument2 in MetaCondition.MetaConditionDefinition.Arguments)
			{
				if (ParseArgument(argument2, out var argument))
				{
					list.Add(argument);
					continue;
				}
				((CLogger<MetaConditionManager>)TPSingleton<MetaConditionManager>.Instance).LogError((object)$"Aborted script execution of {MetaCondition.MetaConditionDefinition} due to an argument error.", (CLogLevel)0, true, true);
				return;
			}
			try
			{
				if (conditionsLibrary[MetaCondition.MetaConditionDefinition.Name](list.ToArray()).IsComplete)
				{
					MetaCondition.OccurenceProgression++;
					((CLogger<MetaConditionManager>)TPSingleton<MetaConditionManager>.Instance).Log((object)$"Conditions for meta condition {MetaCondition.MetaUpgradeController} are all fulfilled, bumping occurence progression to {MetaCondition.OccurenceProgression}/{MetaCondition.MetaConditionDefinition.Occurences}", (CLogLevel)1, false, false);
					if (MetaCondition.OccurenceProgression < MetaCondition.MetaConditionDefinition.Occurences)
					{
						((CLogger<MetaConditionManager>)TPSingleton<MetaConditionManager>.Instance).Log((object)$"Renewing own self-context for {MetaCondition.MetaUpgradeController} ({MetaCondition.OccurenceProgression}/{MetaCondition.MetaConditionDefinition.Occurences}) (occurence was bumped)", (CLogLevel)0, false, false);
						RenewLocalContext();
					}
					else
					{
						((CLogger<MetaConditionManager>)TPSingleton<MetaConditionManager>.Instance).Log((object)$"Max number of occurences reached for meta condition {MetaCondition.MetaUpgradeController} ({MetaCondition.OccurenceProgression}/{MetaCondition.MetaConditionDefinition.Occurences})!\\nThis condition is now entirely fulfilled", (CLogLevel)1, false, false);
					}
				}
				return;
			}
			catch (FormatException arg)
			{
				((CLogger<MetaConditionManager>)TPSingleton<MetaConditionManager>.Instance).LogError((object)$"Script error in {MetaCondition.ToString()}: Wrong data types supplied (Expected Number, received otherwise)\n{arg}", (CLogLevel)0, true, true);
				return;
			}
			catch (InvalidCastException arg2)
			{
				((CLogger<MetaConditionManager>)TPSingleton<MetaConditionManager>.Instance).LogError((object)$"Script error in {MetaCondition.ToString()}: Wrong data types supplied\n{arg2}", (CLogLevel)0, true, true);
				return;
			}
			catch (NullReferenceException arg3)
			{
				((CLogger<MetaConditionManager>)TPSingleton<MetaConditionManager>.Instance).LogError((object)$"Script error in {MetaCondition.ToString()}: Wrong data types supplied (Expected List, received otherwise)\n{arg3}", (CLogLevel)0, true, true);
				return;
			}
			catch (IndexOutOfRangeException arg4)
			{
				((CLogger<MetaConditionManager>)TPSingleton<MetaConditionManager>.Instance).LogError((object)$"Script error in {MetaCondition.ToString()}: Too few arguments supplied\n{arg4}", (CLogLevel)0, true, true);
				return;
			}
		}
		((CLogger<MetaConditionManager>)TPSingleton<MetaConditionManager>.Instance).LogError((object)("Script error: There is no such condition such as " + MetaCondition.MetaConditionDefinition.Name + " in the condition library! Valid conditions are: " + string.Join(", ", conditionsLibrary.Keys)), (CLogLevel)0, true, true);
	}

	public void RenewLocalContext()
	{
		MetaCondition.LocalContext = new MetaConditionSpecificContext();
		contexts["LOCAL"] = MetaCondition.LocalContext;
	}

	public void SetRunContext(MetaConditionContext context)
	{
		contexts["RUN"] = context;
	}

	public void SetCampaignContext(MetaConditionContext context)
	{
		contexts["CAMPAIGN"] = context;
	}

	private string GetLocalizedKeyword(string keyword)
	{
		string result = default(string);
		for (int i = 0; i < keywordPotentialLocalizationPrefixes.Length; i++)
		{
			if (Localizer.TryGet(keywordPotentialLocalizationPrefixes[i] + keyword, ref result))
			{
				return result;
			}
		}
		return keyword;
	}

	private bool ParseArgument(string strArgument, out object argument)
	{
		argument = null;
		if (double.TryParse(strArgument, out var result))
		{
			argument = result;
			return true;
		}
		string[] array = strArgument.Split(new char[1] { ':' });
		if (array.Length > 1)
		{
			string text = array[0];
			string keywordName = array[1];
			if (!contexts.ContainsKey(text))
			{
				((CLogger<MetaConditionManager>)TPSingleton<MetaConditionManager>.Instance).LogError((object)("Invalid context name specified: " + text + " in condition " + MetaCondition.ToString() + ". Valid context names are: " + string.Join(", ", contexts.Keys)), (CLogLevel)0, true, true);
				return false;
			}
			MetaConditionContext metaConditionContext = contexts[text];
			if (metaConditionContext == null)
			{
				((CLogger<MetaConditionManager>)TPSingleton<MetaConditionManager>.Instance).LogError((object)($"Could not fetch the appropriate existing context {text} for condition {MetaCondition}! " + "If you did not forget to deserialize Game or App, THIS COULD BE BIG PROBLEM!\nAdditional info: [Upgrade Id:" + MetaCondition.MetaUpgradeController.MetaUpgrade.MetaUpgradeDefinition.Id + "] [" + string.Join(", ", contexts.Select((KeyValuePair<string, MetaConditionContext> o) => o.Key + "=>" + o.Value)) + "]"), (CLogLevel)0, true, true);
				return false;
			}
			keywordName = keywordName.Trim();
			if (Enumerable.Contains(keywordName, ' '))
			{
				string[] array2 = keywordName.Split(new char[1] { ' ' });
				string functionName = array2[0];
				string[] array3 = array2.Skip(1).ToArray();
				MethodInfo[] methods = metaConditionContext.GetType().GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
				IEnumerable<MethodInfo> source;
				if ((source = methods.Where((MethodInfo o) => o.Name.ToLower() == functionName.ToLower())).Count() > 0)
				{
					MethodInfo? methodInfo = source.FirstOrDefault();
					object[] parameters = array3;
					argument = methodInfo.Invoke(metaConditionContext, parameters);
					return true;
				}
				((CLogger<MetaConditionManager>)TPSingleton<MetaConditionManager>.Instance).LogError((object)("Invalid context element: function " + functionName + " does NOT exist in context " + text + ". Available elements: " + string.Join(", ", from o in methods
					where !o.Name.StartsWith("get_") && !o.Name.StartsWith("set_")
					select o.Name)), (CLogLevel)0, true, true);
				return false;
			}
			FieldInfo[] fields = metaConditionContext.GetType().GetFields();
			PropertyInfo[] properties = metaConditionContext.GetType().GetProperties();
			IEnumerable<FieldInfo> source2;
			if ((source2 = fields.Where((FieldInfo o) => o.Name.ToLower() == keywordName.ToLower())).Count() > 0)
			{
				FieldInfo fieldInfo = source2.LastOrDefault();
				if (!supportedContextTypes.Contains(fieldInfo.FieldType))
				{
					((CLogger<MetaConditionManager>)TPSingleton<MetaConditionManager>.Instance).LogError((object)("You cannot access member " + keywordName + " from context, because its type is not supported yet."), (CLogLevel)0, true, true);
					return false;
				}
				argument = source2.LastOrDefault().GetValue(metaConditionContext);
				return true;
			}
			IEnumerable<PropertyInfo> source3;
			if ((source3 = properties.Where((PropertyInfo o) => o.Name.ToLower() == keywordName.ToLower())).Count() > 0)
			{
				PropertyInfo propertyInfo = source3.FirstOrDefault();
				if (!supportedContextTypes.Contains(propertyInfo.PropertyType))
				{
					((CLogger<MetaConditionManager>)TPSingleton<MetaConditionManager>.Instance).LogError((object)("You cannot access member " + keywordName + " from context, because its type is not supported yet."), (CLogLevel)0, true, true);
					return false;
				}
				argument = propertyInfo.GetValue(metaConditionContext);
				return true;
			}
			IEnumerable<string> values = (from o in fields
				where supportedContextTypes.Contains(o.FieldType)
				select o.Name).Concat(from o in properties
				where supportedContextTypes.Contains(o.PropertyType)
				select o.Name);
			((CLogger<MetaConditionManager>)TPSingleton<MetaConditionManager>.Instance).LogError((object)("Invalid context element: " + keywordName + " does NOT exist in context " + text + ". Available elements: " + string.Join(",", values)), (CLogLevel)0, true, true);
			return false;
		}
		argument = strArgument;
		return true;
	}

	private List<string> ParseLocalizationArguments()
	{
		List<string> list = new List<string>();
		foreach (string argument in MetaCondition.MetaConditionDefinition.Arguments)
		{
			if (double.TryParse(argument, out var result))
			{
				list.Add(result.ToString());
				continue;
			}
			string[] array = argument.Split(new char[1] { ':' });
			if (array.Length > 1)
			{
				_ = array[0];
				string text = array[1];
				if (Enumerable.Contains(text, ' '))
				{
					string[] array2 = text.Split(new char[1] { ' ' });
					_ = array2[0];
					string[] array3 = array2.Skip(1).ToArray();
					for (int i = 0; i < array3.Length; i++)
					{
						list.Add(GetLocalizedKeyword(array3[i]));
					}
				}
			}
			else
			{
				list.Add(GetLocalizedKeyword(argument));
			}
		}
		return list;
	}

	private string ParseLocalizationKey()
	{
		string text4 = default(string);
		foreach (string argument in MetaCondition.MetaConditionDefinition.Arguments)
		{
			string[] array = argument.Split(new char[1] { ':' });
			if (array.Length <= 1)
			{
				continue;
			}
			string text = array[1];
			if (Enumerable.Contains(text, ' '))
			{
				List<string> list = text.Split(new char[1] { ' ' }).ToList();
				string text2 = list[0];
				while (list.Count > 1)
				{
					string text3 = "MetaCondition_";
					for (int i = 0; i < list.Count; i++)
					{
						text3 += list[i];
						if (i < list.Count - 1)
						{
							text3 += "_";
						}
					}
					if (Localizer.TryGet(text3, ref text4))
					{
						return text3;
					}
					list.RemoveAt(list.Count - 1);
				}
				return "MetaCondition_" + text2;
			}
			return "MetaCondition_" + text;
		}
		return string.Empty;
	}
}
