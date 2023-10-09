using System.Xml.Linq;
using UnityEngine;

namespace TheLastStand.Framework.Extensions;

public static class XDocumentExtensions
{
	public static bool IsNullOrEmpty(this XAttribute xAttribute)
	{
		if (xAttribute != null)
		{
			return xAttribute.Value == string.Empty;
		}
		return true;
	}

	public static bool IsNullOrEmpty(this XElement xElement)
	{
		if (xElement != null)
		{
			return xElement.Value == string.Empty;
		}
		return true;
	}

	public static Vector2Int ParseMinMax(this XElement xElement)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		if (!string.IsNullOrEmpty(xElement.Value))
		{
			if (int.TryParse(xElement.Value, out var result))
			{
				return new Vector2Int(result, result);
			}
			Debug.LogError((object)$"Value of {xElement.Name} is not an integer: {xElement.Value}");
			return new Vector2Int(0, 0);
		}
		Vector2Int result2 = default(Vector2Int);
		bool flag = false;
		if (xElement.Attribute(XName.op_Implicit("Min")) != null)
		{
			if (int.TryParse(xElement.Attribute(XName.op_Implicit("Min")).Value, out var result3))
			{
				((Vector2Int)(ref result2))._002Ector(result3, result3);
				flag = true;
			}
			else
			{
				Debug.LogError((object)string.Format("Min attribute of {0} in AttackDefinition is not an integer: {1}", xElement.Name, xElement.Attribute(XName.op_Implicit("Min")).Value));
			}
		}
		if (xElement.Attribute(XName.op_Implicit("Max")) != null)
		{
			if (int.TryParse(xElement.Attribute(XName.op_Implicit("Max")).Value, out var result4))
			{
				if (flag && result4 < ((Vector2Int)(ref result2)).x)
				{
					Debug.LogError((object)$"Max attribute of {xElement.Name} in AttackDefinition is lower than min value: {result4} < {((Vector2Int)(ref result2)).x}");
				}
				else
				{
					((Vector2Int)(ref result2))._002Ector(flag ? ((Vector2Int)(ref result2)).x : result4, result4);
				}
			}
			else
			{
				Debug.LogError((object)string.Format("Max attribute of {0} in AttackDefinition is not an integer: {1}", xElement.Name, xElement.Attribute(XName.op_Implicit("Max")).Value));
			}
		}
		return result2;
	}
}
