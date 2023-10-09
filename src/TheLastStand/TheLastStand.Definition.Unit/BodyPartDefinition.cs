using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Unit;

public class BodyPartDefinition : TheLastStand.Framework.Serialization.Definition
{
	[Flags]
	public enum E_Orientation
	{
		Front = 1,
		Back = 2
	}

	public static class Constants
	{
		public static class SpriteId
		{
			public static class Constraint
			{
				public const string FaceId = "FaceId";

				public const string Gender = "Gender";

				public const string Generic = "Generic";
			}

			public static class Orientation
			{
				public const string Back = "Back";

				public const string Front = "Front";
			}
		}

		public static class BodyPartId
		{
			public const string ArmL = "Arm_L";

			public const string Head = "Head";
		}

		public static class AdditionalConstraintsId
		{
			public const string Hide = "Hide";
		}
	}

	private Dictionary<string, string> spritePaths = new Dictionary<string, string>();

	public string Id { get; private set; }

	public BodyPartDefinition(XContainer container)
		: base(container)
	{
	}

	private static string AddOrientationId(string baseId, E_Orientation orientation)
	{
		return baseId + "_" + ((orientation == E_Orientation.Back) ? "Back" : "Front");
	}

	private static string GetSpriteIdForFaceId(string baseId)
	{
		return "FaceId_" + baseId;
	}

	private static string GetSpriteIdForGender(string baseId)
	{
		return "Gender_" + baseId;
	}

	public string GetSpritePath(string faceId, string gender, E_Orientation orientation)
	{
		spritePaths.TryGetValue(AddOrientationId(GetSpriteIdForFaceId(faceId), orientation), out var value);
		if (!string.IsNullOrEmpty(value))
		{
			return value;
		}
		spritePaths.TryGetValue(AddOrientationId(GetSpriteIdForGender(gender), orientation), out value);
		if (!string.IsNullOrEmpty(value))
		{
			return value;
		}
		spritePaths.TryGetValue(AddOrientationId("Generic", orientation), out value);
		return value;
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		XAttribute val2 = val.Attribute(XName.op_Implicit("Id"));
		if (val2.IsNullOrEmpty())
		{
			Debug.LogError((object)"The BodyPartDefinition has no Id!");
			return;
		}
		Id = val2.Value;
		spritePaths.Clear();
		foreach (XElement item in ((XContainer)val).Elements(XName.op_Implicit("Sprite")))
		{
			if (item.IsNullOrEmpty())
			{
				Debug.LogError((object)("The Sprite for BodyPartDefinition '" + Id + "' is invalid: entry is undefined"));
				continue;
			}
			XAttribute val3 = item.Attribute(XName.op_Implicit("Constraint"));
			string text = null;
			if (val3.IsNullOrEmpty())
			{
				Debug.LogError((object)("The Sprite for BodyPartDefinition '" + Id + "' is invalid: Constraint is undefined"));
				continue;
			}
			text = val3.Value;
			XAttribute val4 = item.Attribute(XName.op_Implicit("Orientation"));
			E_Orientation result = E_Orientation.Front | E_Orientation.Back;
			if (!val4.IsNullOrEmpty() && !Enum.TryParse<E_Orientation>(val4.Value, out result))
			{
				Debug.LogError((object)("The Sprite for BodyPartDefinition '" + Id + "' is invalid:  Orientation '" + val4.Value + "' is invalid"));
				continue;
			}
			string text2 = null;
			switch (text)
			{
			case "Generic":
				text2 = "Generic";
				break;
			case "Gender":
			{
				XAttribute obj2 = item.Attribute(XName.op_Implicit("Id"));
				text2 = ((obj2 != null) ? obj2.Value : null);
				if (string.IsNullOrEmpty(text2) || (!string.Equals(text2, "Male", StringComparison.Ordinal) && !string.Equals(text2, "Female", StringComparison.Ordinal)))
				{
					Debug.LogError((object)("The Sprite for BodyPartDefinition '" + Id + "' is invalid: Id '" + text2 + "' is invalid for Constraint '" + text + "'"));
					continue;
				}
				text2 = GetSpriteIdForGender(text2);
				break;
			}
			case "FaceId":
			{
				XAttribute obj = item.Attribute(XName.op_Implicit("Id"));
				text2 = ((obj != null) ? obj.Value : null);
				if (string.IsNullOrEmpty(text2))
				{
					Debug.LogError((object)("The Sprite for BodyPartDefinition '" + Id + "' is invalid: Id '" + text2 + "' is invalid for Constraint '" + text + "'"));
					continue;
				}
				text2 = GetSpriteIdForFaceId(text2);
				break;
			}
			default:
				Debug.Log((object)("The Sprite for BodyPartDefinition '" + Id + "' is invalid: Constraint '" + text + "' is invalid"));
				continue;
			}
			if ((result & E_Orientation.Front) == E_Orientation.Front)
			{
				spritePaths.Add(AddOrientationId(text2, E_Orientation.Front), AddOrientationId(item.Value, E_Orientation.Front));
			}
			if ((result & E_Orientation.Back) == E_Orientation.Back)
			{
				spritePaths.Add(AddOrientationId(text2, E_Orientation.Back), AddOrientationId(item.Value, E_Orientation.Back));
			}
		}
	}
}
