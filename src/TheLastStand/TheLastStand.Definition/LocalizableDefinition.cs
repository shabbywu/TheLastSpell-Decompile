using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using TheLastStand.Model;
using TheLastStand.Model.Status;

namespace TheLastStand.Definition;

public abstract class LocalizableDefinition : TheLastStand.Framework.Serialization.Definition
{
	public List<LocArgument> LocArguments { get; private set; }

	protected LocalizableDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public object[] GetArguments(InterpreterContext interpreterContext)
	{
		object[] array = new object[LocArguments.Count];
		for (int i = 0; i < LocArguments.Count; i++)
		{
			array[i] = LocArguments[i].GetFinalValue(interpreterContext);
		}
		return array;
	}

	private void DeserializeLocArguments(XElement xLocArguments)
	{
		LocArguments = new List<LocArgument>();
		foreach (XElement item2 in ((XContainer)xLocArguments).Elements())
		{
			XAttribute obj = item2.Attribute(XName.op_Implicit("Value"));
			XAttribute val = item2.Attribute(XName.op_Implicit("Interpreted"));
			XAttribute val2 = item2.Attribute(XName.op_Implicit("Prefix"));
			XAttribute val3 = item2.Attribute(XName.op_Implicit("Suffix"));
			XAttribute val4 = item2.Attribute(XName.op_Implicit("Style"));
			if (item2.Name.LocalName != "StatArgument" && item2.Name.LocalName != "StatusArgument")
			{
				_ = item2.Name.LocalName != "AttackTypeArgument";
			}
			string text = ((obj != null) ? obj.Value.Replace(base.TokenVariables) : null);
			Node valueExpression = null;
			if (!string.IsNullOrEmpty((val != null) ? val.Value : null) && bool.Parse(val.Value) && !string.IsNullOrEmpty(text))
			{
				valueExpression = Parser.Parse(text);
			}
			LocArgument item = null;
			switch (item2.Name.LocalName)
			{
			case "LocArgument":
				item = new LocArgument(text, valueExpression, (val4 != null) ? val4.Value : null, (val2 != null) ? val2.Value : null, (val3 != null) ? val3.Value : null);
				break;
			case "AttackTypeArgument":
			{
				XAttribute val12 = item2.Attribute(XName.op_Implicit("AttackType"));
				XAttribute val13 = item2.Attribute(XName.op_Implicit("InterpretedAttackType"));
				Node attackTypeExpression = ((!string.IsNullOrEmpty((val13 != null) ? val13.Value : null)) ? Parser.Parse(val13.Value) : null);
				item = new AttackTypeArgument(text, valueExpression, (val4 != null) ? val4.Value : null, (val2 != null) ? val2.Value : null, (val3 != null) ? val3.Value : null, attackTypeExpression, (val12 != null) ? val12.Value : null);
				break;
			}
			case "LocalizedArgument":
				item = new LocalizedArgument(text, valueExpression, (val4 != null) ? val4.Value : null, (val2 != null) ? val2.Value : null, (val3 != null) ? val3.Value : null);
				break;
			case "StatArgument":
			{
				XAttribute val9 = item2.Attribute(XName.op_Implicit("Stat"));
				XAttribute val10 = item2.Attribute(XName.op_Implicit("DisplaySign"));
				XAttribute val11 = item2.Attribute(XName.op_Implicit("InterpretedStat"));
				Node statExpression = ((!string.IsNullOrEmpty((val11 != null) ? val11.Value : null)) ? Parser.Parse(val11.Value) : null);
				bool displaySign = string.IsNullOrEmpty((val10 != null) ? val10.Value : null) || bool.Parse(val10.Value);
				UnitStatDefinition.E_Stat result3 = UnitStatDefinition.E_Stat.Undefined;
				if (!string.IsNullOrEmpty((val9 != null) ? val9.Value : null))
				{
					Enum.TryParse<UnitStatDefinition.E_Stat>(val9.Value, out result3);
				}
				item = new StatArgument(text, valueExpression, (val4 != null) ? val4.Value : null, (val2 != null) ? val2.Value : null, (val3 != null) ? val3.Value : null, result3, statExpression, displaySign);
				break;
			}
			case "RestoreStatArgument":
			{
				XAttribute obj3 = item2.Attribute(XName.op_Implicit("Stat"));
				XAttribute val14 = item2.Attribute(XName.op_Implicit("DisplaySign"));
				bool displaySign2 = string.IsNullOrEmpty((val14 != null) ? val14.Value : null) || bool.Parse(val14.Value);
				Enum.TryParse<UnitStatDefinition.E_Stat>(obj3.Value, out var result4);
				XAttribute val15 = item2.Attribute(XName.op_Implicit("DisplayRestoreText"));
				bool displayRestoreText = !string.IsNullOrEmpty((val15 != null) ? val15.Value : null) && bool.Parse(val15.Value);
				XAttribute val16 = item2.Attribute(XName.op_Implicit("ModifiedValue"));
				bool modifiedValue2 = !string.IsNullOrEmpty((val16 != null) ? val16.Value : null) && bool.Parse(val16.Value);
				item = new RestoreStatArgument(text, valueExpression, (val4 != null) ? val4.Value : null, (val2 != null) ? val2.Value : null, (val3 != null) ? val3.Value : null, result4, displaySign2, modifiedValue2, displayRestoreText);
				break;
			}
			case "StatusArgument":
			{
				XAttribute obj2 = item2.Attribute(XName.op_Implicit("Status"));
				XAttribute val5 = item2.Attribute(XName.op_Implicit("TurnsCount"));
				XAttribute val6 = item2.Attribute(XName.op_Implicit("Chance"));
				XAttribute val7 = item2.Attribute(XName.op_Implicit("ModifiedValue"));
				XAttribute val8 = item2.Attribute(XName.op_Implicit("Stat"));
				Enum.TryParse<Status.E_StatusType>(obj2.Value.Replace(base.TokenVariables), out var result);
				UnitStatDefinition.E_Stat result2 = UnitStatDefinition.E_Stat.Undefined;
				if (!string.IsNullOrEmpty((val8 != null) ? val8.Value : null))
				{
					Enum.TryParse<UnitStatDefinition.E_Stat>(val8.Value.Replace(base.TokenVariables), out result2);
				}
				Node turnsCountExpression = null;
				string text2 = ((val5 != null) ? val5.Value.Replace(base.TokenVariables) : null);
				if (!string.IsNullOrEmpty(text2))
				{
					turnsCountExpression = Parser.Parse(text2);
				}
				Node chanceExpression = null;
				string text3 = ((val6 != null) ? val6.Value.Replace(base.TokenVariables) : null);
				if (!string.IsNullOrEmpty(text3))
				{
					chanceExpression = Parser.Parse(text3);
				}
				bool modifiedValue = !string.IsNullOrEmpty((val7 != null) ? val7.Value : null) && bool.Parse(val7.Value);
				item = new StatusArgument(text, valueExpression, (val4 != null) ? val4.Value : null, (val2 != null) ? val2.Value : null, (val3 != null) ? val3.Value : null, result, turnsCountExpression, chanceExpression, modifiedValue, result2);
				break;
			}
			}
			LocArguments.Add(item);
		}
	}

	public override void Deserialize(XContainer container)
	{
		LocArguments = new List<LocArgument>();
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		if (val != null)
		{
			DeserializeLocArguments(val);
		}
	}
}
