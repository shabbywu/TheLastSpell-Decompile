using System.Xml.Linq;
using TPLib.Log;
using UnityEngine;

namespace TheLastStand.Definition.Brazier;

public class BraziersAmountPerNight : NightIndexedItem
{
	private static class Constants
	{
		public const string BraziersAmountElement = "BraziersAmount";

		public const string AmountAttribute = "Amount";
	}

	public int Amount;

	public override void Init(int nightIndex, XElement xElement)
	{
		base.Init(nightIndex, xElement);
		XAttribute val = xElement.Attribute(XName.op_Implicit("Amount"));
		if (!int.TryParse(val.Value, out var result))
		{
			CLoggerManager.Log((object)("Amount attribute could not be parsed into an int (" + val.Value + "). Skipped."), (LogType)0, (CLogLevel)2, true, "BrazierDefinition", false);
		}
		else
		{
			Amount = result;
		}
	}
}
