using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Building;

public class ShopDefinition : TheLastStand.Framework.Serialization.Definition
{
	public float SellingMultiplier { get; private set; }

	public List<int> RerollPrices { get; private set; }

	public Dictionary<string, ShopEvolutionDefinition> ShopEvolutionDefinitions { get; private set; }

	public ShopDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XElement val2 = ((XContainer)val).Element(XName.op_Implicit("SellingMultiplier"));
		if (val2 == null)
		{
			Debug.LogError((object)"ShopDefinion must have a SellingMultiplier");
			return;
		}
		if (!float.TryParse(val2.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			Debug.LogError((object)"ShopDefinition SellingMultiplier must have a valid float value");
			return;
		}
		SellingMultiplier = result;
		RerollPrices = new List<int>();
		XElement obj = ((XContainer)val).Element(XName.op_Implicit("Rerolls"));
		int i = 0;
		int item = 0;
		foreach (XElement item2 in ((XContainer)obj).Elements(XName.op_Implicit("Price")))
		{
			XAttribute val3 = item2.Attribute(XName.op_Implicit("Id"));
			if (!int.TryParse(val3.Value, out var result2))
			{
				CLoggerManager.Log((object)("Can't parse id attribute in price element into an int : " + val3.Value), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
				continue;
			}
			if (!int.TryParse(item2.Value, out var result3))
			{
				CLoggerManager.Log((object)("Can't parse price element into an int : " + item2.Value), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
				continue;
			}
			if (result2 <= i)
			{
				CLoggerManager.Log((object)$"index ({result2}) is inferior or equal to the current index ({i}), skipping this one.", (LogType)0, (CLogLevel)2, true, "StaticLog", false);
				continue;
			}
			for (; i < result2 - 1; i++)
			{
				RerollPrices.Add(item);
			}
			RerollPrices.Add(result3);
			i++;
			item = result3;
		}
		if (i == 0)
		{
			CLoggerManager.Log((object)"There is no valid shop reroll prices with strictly positive index, that is unexpected !", (LogType)0, (CLogLevel)2, true, "StaticLog", false);
			RerollPrices.Add(0);
		}
		XElement obj2 = ((XContainer)val).Element(XName.op_Implicit("ShopEvolutions"));
		ShopEvolutionDefinitions = new Dictionary<string, ShopEvolutionDefinition>();
		foreach (XElement item3 in ((XContainer)obj2).Elements(XName.op_Implicit("ShopEvolution")))
		{
			XAttribute val4 = item3.Attribute(XName.op_Implicit("Id"));
			ShopEvolutionDefinitions.Add(val4.Value, new ShopEvolutionDefinition((XContainer)(object)item3));
		}
	}
}
