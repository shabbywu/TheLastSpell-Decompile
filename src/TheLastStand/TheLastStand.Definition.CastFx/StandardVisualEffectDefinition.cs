using System;
using System.Globalization;
using System.Xml.Linq;
using TheLastStand.Framework.Extensions;
using UnityEngine;

namespace TheLastStand.Definition.CastFx;

public class StandardVisualEffectDefinition : VisualEffectDefinition
{
	public class TargetData
	{
		public enum E_TargetType
		{
			None = -1,
			Caster,
			AoeOrigin,
			HitTiles,
			HitUnits,
			HitPlayableUnits,
			HitEnemyUnits,
			PropagationTiles,
			CastFxSourceTile,
			FollowCaster
		}

		public E_TargetType TargetType { get; set; }

		public Vector2Int TileOffset { get; set; }
	}

	public TargetData Target { get; private set; }

	public string SpawnedParticlesPath { get; set; } = string.Empty;


	public StandardVisualEffectDefinition(XContainer container)
		: base(container)
	{
	}

	public override void Deserialize(XContainer container)
	{
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		base.Deserialize(container);
		XElement val = container.Element(XName.op_Implicit("Target"));
		XElement val2 = container.Element(XName.op_Implicit("SpawnedParticles"));
		if (val2 != null)
		{
			SpawnedParticlesPath = val2.Value;
		}
		if (val != null)
		{
			if (!Enum.TryParse<TargetData.E_TargetType>(val.Value, out var result))
			{
				Debug.LogError((object)("VisualEffectDefinition (Path='" + paths[0] + "') defines an invalid Target (" + val.Value + ")!"));
				return;
			}
			Vector2Int zero = Vector2Int.zero;
			XAttribute val3 = val.Attribute(XName.op_Implicit("XTileOffset"));
			if (!val3.IsNullOrEmpty())
			{
				((Vector2Int)(ref zero)).x = int.Parse(val3.Value, CultureInfo.InvariantCulture);
			}
			XAttribute val4 = val.Attribute(XName.op_Implicit("YTileOffset"));
			if (!val4.IsNullOrEmpty())
			{
				((Vector2Int)(ref zero)).y = int.Parse(val4.Value, CultureInfo.InvariantCulture);
			}
			Target = new TargetData
			{
				TargetType = result,
				TileOffset = zero
			};
		}
		else
		{
			Debug.LogError((object)("VisualEffectDefinition (Path='" + paths[0] + "') must define a Target!"));
		}
	}
}
