using System.Xml.Linq;
using TPLib;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager.DLC;

namespace TheLastStand.Definition.Unit;

public class UnitFaceIdDefinition : TheLastStand.Framework.Serialization.Definition
{
	public string DLCId { get; private set; }

	public string FaceId { get; private set; }

	public bool IsLinkedDLCOwned
	{
		get
		{
			if (IsLinkedToDLC)
			{
				return TPSingleton<DLCManager>.Instance.IsDLCOwned(DLCId);
			}
			return false;
		}
	}

	public bool IsLinkedToDLC => !string.IsNullOrEmpty(DLCId);

	public string RestrictedToRaceId { get; private set; }

	public int Weight { get; private set; }

	public UnitFaceIdDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XAttribute val2 = val.Attribute(XName.op_Implicit("DLCId"));
		if (val2 != null && !string.IsNullOrEmpty(val2.Value))
		{
			DLCId = val2.Value;
		}
		XAttribute val3 = val.Attribute(XName.op_Implicit("RestrictedToRaceId"));
		if (val3 != null && !string.IsNullOrEmpty(val3.Value))
		{
			RestrictedToRaceId = val3.Value;
		}
		else
		{
			RestrictedToRaceId = "Human";
		}
		XAttribute val4 = val.Attribute(XName.op_Implicit("Weight"));
		if (val4 != null && int.TryParse(val4.Value, out var result))
		{
			Weight = result;
		}
		FaceId = val.Value;
	}
}
