using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework.Serialization;
using TheLastStand.View;
using UnityEngine;

namespace TheLastStand.Model.Unit;

public class BodyPart : ILegacySerializable, ILegacyDeserializable
{
	public HashSet<string> AdditionalConstraints { get; private set; } = new HashSet<string>();


	public BodyPartDefinition BodyPartDefinition { get; private set; }

	public BodyPartDefinition BodyPartDefinitionOverride { get; set; }

	private BodyPartView BodyPartViewBack { get; set; }

	private BodyPartView BodyPartViewFront { get; set; }

	public BodyPart(XContainer container)
	{
		Deserialize(container);
	}

	public BodyPart(BodyPartDefinition definition, BodyPartView viewFront = null, BodyPartView viewBack = null)
	{
		BodyPartDefinition = definition;
		BodyPartViewFront = viewFront;
		BodyPartViewBack = viewBack;
	}

	public void ChangeAdditionalConstraint(string constraintId, bool add)
	{
		if ((add && AdditionalConstraints.Add(constraintId)) || AdditionalConstraints.Remove(constraintId))
		{
			if ((Object)(object)BodyPartViewFront != (Object)null)
			{
				BodyPartViewFront.IsDirty = true;
			}
			if ((Object)(object)BodyPartViewBack != (Object)null)
			{
				BodyPartViewBack.IsDirty = true;
			}
		}
	}

	public virtual void Deserialize(XContainer container = null)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XAttribute val2 = val.Attribute(XName.op_Implicit("Id"));
		BodyPartDefinition = PlayableUnitDatabase.PlayableUnitNakedBodyPartsDefinitions[val2.Value];
		if (((XContainer)val).Element(XName.op_Implicit("BodyPartDefinitionOverride")) != null)
		{
			XElement val3 = ((XContainer)val).Element(XName.op_Implicit("BodyPartDefinitionOverride"));
			if (val3.HasAttributes)
			{
				XAttribute val4 = val3.Attribute(XName.op_Implicit("Id"));
				BodyPartDefinitionOverride = PlayableUnitDatabase.PlayableUnitNakedBodyPartsDefinitions[val4.Value];
			}
		}
		foreach (XElement item in ((XContainer)val).Elements(XName.op_Implicit("AdditionalConstraints")))
		{
			if (item.HasAttributes)
			{
				AdditionalConstraints.Add(item.Attribute(XName.op_Implicit("Value")).Value);
			}
		}
	}

	public BodyPartView GetBodyPartView(BodyPartDefinition.E_Orientation orientation)
	{
		if (orientation != BodyPartDefinition.E_Orientation.Back)
		{
			return BodyPartViewFront;
		}
		return BodyPartViewBack;
	}

	public string GetSpritePath(string faceId, string gender, BodyPartDefinition.E_Orientation orientation)
	{
		string text = null;
		text = BodyPartDefinitionOverride?.GetSpritePath(faceId, gender, orientation);
		if (string.IsNullOrEmpty(text))
		{
			text = BodyPartDefinition?.GetSpritePath(faceId, gender, orientation);
		}
		return text;
	}

	public virtual XContainer Serialize()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Expected O, but got Unknown
		XElement val = new XElement(XName.op_Implicit("BodyPart"));
		((XContainer)val).Add((object)new XAttribute(XName.op_Implicit("Id"), (object)BodyPartDefinition.Id));
		if (BodyPartDefinitionOverride != null)
		{
			((XContainer)val).Add((object)new XElement(XName.op_Implicit("BodyPartDefinitionOverride"), (object)new XAttribute(XName.op_Implicit("Id"), (object)BodyPartDefinitionOverride.Id)));
		}
		XElement val2 = new XElement(XName.op_Implicit("AdditionalConstraints"));
		foreach (string additionalConstraint in AdditionalConstraints)
		{
			((XContainer)val2).Add((object)new XElement(XName.op_Implicit(additionalConstraint)));
		}
		((XContainer)val).Add((object)val2);
		return (XContainer)(object)val;
	}

	public void SetBodyPartView(BodyPartDefinition.E_Orientation orientation, BodyPartView bodyPartView)
	{
		if ((orientation & BodyPartDefinition.E_Orientation.Front) == BodyPartDefinition.E_Orientation.Front)
		{
			BodyPartViewFront = bodyPartView;
		}
		else if ((orientation & BodyPartDefinition.E_Orientation.Back) == BodyPartDefinition.E_Orientation.Back)
		{
			BodyPartViewBack = bodyPartView;
		}
	}
}
