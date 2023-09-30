using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Meta;
using UnityEngine;

namespace TheLastStand.Definition.Building.Module;

public class DamageableModuleDefinition : BuildingModuleDefinition
{
	public string DamagedParticlesId { get; private set; } = string.Empty;


	public bool CanPanic => TotalPanicValue > 0f;

	public float TotalPanicValue { get; private set; }

	public bool DisableDestructionSmokeFX { get; private set; }

	public byte FlameCount { get; private set; }

	public List<Vector2> FlamesPositions { get; private set; } = new List<Vector2>();


	public int GlyphHealthTotalPercentageModifier
	{
		get
		{
			int num = 0;
			if (BuildingDefinition.IdListIds != null)
			{
				foreach (string idListId in BuildingDefinition.IdListIds)
				{
					num += DictionaryExtensions.GetValueOrDefault<string, int>(TPSingleton<GlyphManager>.Instance.BuildingHealthModifiers, idListId);
				}
			}
			return num;
		}
	}

	public float HealthTotal => BuildingManager.ComputeBuildingTotalHealth(this);

	public float NativeHealthTotal { get; private set; }

	public DamageableModuleDefinition(BuildingDefinition buildingDefinition, XContainer damageableDefinition)
		: base(buildingDefinition, damageableDefinition)
	{
	}

	public override void Deserialize(XContainer container)
	{
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Expected O, but got Unknown
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		if (val == null)
		{
			return;
		}
		XElement val2 = ((XContainer)val).Element(XName.op_Implicit("HealthTotal"));
		if (val2 != null)
		{
			if (!float.TryParse(val2.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
			{
				CLoggerManager.Log((object)("Building " + BuildingDefinition.Id + "'s Health " + ((Definition)this).HasAnInvalidFloat(val2.Value)), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
				return;
			}
			NativeHealthTotal = result;
		}
		XElement val3 = ((XContainer)val).Element(XName.op_Implicit("FlameCount"));
		if (val3 != null)
		{
			if (byte.TryParse(val3.Value, out var result2))
			{
				FlameCount = result2;
			}
			else
			{
				CLoggerManager.Log((object)("Building " + BuildingDefinition.Id + "'s FlameCount " + ((Definition)this).HasAnInvalid("byte", val3.Value)), (LogType)2, (CLogLevel)1, true, "StaticLog", false);
			}
		}
		XElement val4 = ((XContainer)val).Element(XName.op_Implicit("FlameAnchorPoints"));
		if (val4 != null)
		{
			foreach (XElement item2 in ((XContainer)val4).DescendantNodes())
			{
				XElement val5 = item2;
				try
				{
					Vector2 item = new Vector2(float.Parse(val5.Attribute(XName.op_Implicit("PositionX")).Value, NumberStyles.Float, CultureInfo.InvariantCulture), float.Parse(val5.Attribute(XName.op_Implicit("PositionY")).Value, NumberStyles.Float, CultureInfo.InvariantCulture));
					FlamesPositions.Add(item);
				}
				catch (FormatException)
				{
					CLoggerManager.Log((object)("While deserializing building " + BuildingDefinition.Id + ", I found an invalid flame position!"), (LogType)2, (CLogLevel)1, true, "StaticLog", false);
				}
			}
			if (FlameCount > FlamesPositions.Count)
			{
				CLoggerManager.Log((object)("Error while deserializing Building " + BuildingDefinition.Id + ": You cannot add more flames to the building than there are flamed positions."), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			}
		}
		DisableDestructionSmokeFX = ((XContainer)val).Element(XName.op_Implicit("NoSmokeFXOnDestruction")) != null;
		if (((XContainer)val).Element(XName.op_Implicit("DamagedParticles")) != null)
		{
			XAttribute val6 = ((XContainer)val).Element(XName.op_Implicit("DamagedParticles")).Attribute(XName.op_Implicit("Id"));
			DamagedParticlesId = val6.Value;
		}
		XElement val7 = ((XContainer)val).Element(XName.op_Implicit("TotalPanicValue"));
		if (!string.IsNullOrEmpty((val7 != null) ? val7.Value : null))
		{
			if (float.TryParse(val7.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result3))
			{
				TotalPanicValue = result3;
			}
			else
			{
				CLoggerManager.Log((object)("Error while deserializing Building " + BuildingDefinition.Id + ": Unable to parse totalPanicValueElement.Value into float"), (LogType)2, (CLogLevel)1, true, "StaticLog", false);
			}
		}
	}
}
