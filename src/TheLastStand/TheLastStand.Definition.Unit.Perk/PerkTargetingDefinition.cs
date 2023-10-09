using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using TheLastStand.Model;
using UnityEngine;

namespace TheLastStand.Definition.Unit.Perk;

public class PerkTargetingDefinition : TheLastStand.Framework.Serialization.Definition
{
	public enum E_TargetingMethod
	{
		Self,
		ClosestTarget,
		AdjacentDamageables,
		DamageablesInRange
	}

	public enum E_TargetingReference
	{
		Owner,
		Caster,
		Target,
		AllTargets
	}

	public static class Constants
	{
		public const string Id = "PerkTargeting";
	}

	public Node AmountExpression { get; private set; }

	public Node RangeExpression { get; private set; }

	public List<DamageableType> ValidDamageableTypes { get; private set; }

	public E_TargetingMethod TargetingMethod { get; private set; }

	public E_TargetingReference TargetingReference { get; private set; }

	public PerkTargetingDefinition(XContainer container, Dictionary<string, string> tokenVariables = null)
		: base(container, tokenVariables)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		if (val == null)
		{
			return;
		}
		XAttribute val2 = val.Attribute(XName.op_Implicit("Amount"));
		if (!string.IsNullOrEmpty((val2 != null) ? val2.Value : null))
		{
			string text = val2.Value.Replace(base.TokenVariables);
			if (!string.IsNullOrEmpty(text))
			{
				AmountExpression = Parser.Parse(text);
			}
			else
			{
				CLoggerManager.Log((object)("Could not parse Amount attribute : " + val2.Value + "."), (LogType)0, (CLogLevel)2, true, "PerkTargetingDefinition", false);
			}
		}
		XAttribute val3 = val.Attribute(XName.op_Implicit("Range"));
		if (!string.IsNullOrEmpty((val3 != null) ? val3.Value : null))
		{
			string text2 = val3.Value.Replace(base.TokenVariables);
			if (!string.IsNullOrEmpty(text2))
			{
				RangeExpression = Parser.Parse(text2);
			}
			else
			{
				CLoggerManager.Log((object)("Could not parse Range attribute : " + val3.Value + "."), (LogType)0, (CLogLevel)2, true, "PerkTargetingDefinition", false);
			}
		}
		XAttribute val4 = val.Attribute(XName.op_Implicit("TargetingReference"));
		if (val4 != null)
		{
			if (Enum.TryParse<E_TargetingReference>(val4.Value, out var result))
			{
				TargetingReference = result;
			}
			else
			{
				CLoggerManager.Log((object)("Could not parse TargetingReference attribute into an E_TargetingReference : " + val4.Value + "."), (LogType)0, (CLogLevel)2, true, "PerkTargetingDefinition", false);
			}
		}
		XAttribute val5 = val.Attribute(XName.op_Implicit("TargetingMethod"));
		if (val5 != null)
		{
			if (Enum.TryParse<E_TargetingMethod>(val5.Value, out var result2))
			{
				TargetingMethod = result2;
			}
			else
			{
				CLoggerManager.Log((object)("Could not parse TargetingMethod attribute into an E_TargetingMethod : " + val5.Value + "."), (LogType)0, (CLogLevel)2, true, "PerkTargetingDefinition", false);
			}
		}
		foreach (XElement item in ((XContainer)val).Elements(XName.op_Implicit("DamageableTarget")))
		{
			XAttribute val6 = item.Attribute(XName.op_Implicit("Type"));
			if (val6 == null)
			{
				CLoggerManager.Log((object)"Missing Type attribute in a DamageableTarget element.", (LogType)0, (CLogLevel)2, true, "PerkTargetingDefinition", false);
			}
			else
			{
				if (string.IsNullOrEmpty((val6 != null) ? val6.Value : null))
				{
					continue;
				}
				if (Enum.TryParse<DamageableType>(val6.Value, out var result3))
				{
					if (ValidDamageableTypes == null)
					{
						ValidDamageableTypes = new List<DamageableType>();
					}
					ValidDamageableTypes.Add(result3);
				}
				else
				{
					CLoggerManager.Log((object)("Could not parse Type attribute into a DamageableType : " + val6.Value + "."), (LogType)0, (CLogLevel)2, true, "PerkTargetingDefinition", false);
				}
			}
		}
	}
}
