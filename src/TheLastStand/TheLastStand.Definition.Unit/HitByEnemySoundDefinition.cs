using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager;
using UnityEngine;

namespace TheLastStand.Definition.Unit;

public class HitByEnemySoundDefinition : Definition
{
	public static class Consts
	{
		public static class Targets
		{
			public const string PlayingUnits = "PlayingUnits";

			public const string Buildings = "Buildings";
		}
	}

	public string Target { get; private set; }

	public Dictionary<Vector2Int, List<string>> SoundsPerNumberOfEnemies { get; private set; }

	public HitByEnemySoundDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		XContainer obj = ((container is XElement) ? container : null);
		XAttribute val = ((XElement)obj).Attribute(XName.op_Implicit("Target"));
		if (val == null)
		{
			Debug.LogError((object)"HitByEnemySoundDefinition must have Target");
		}
		Target = val.Value;
		SoundsPerNumberOfEnemies = new Dictionary<Vector2Int, List<string>>();
		foreach (XElement item in obj.Elements(XName.op_Implicit("SoundsPerNumberOfEnemies")))
		{
			XAttribute obj2 = item.Attribute(XName.op_Implicit("Min"));
			if (obj2 == null)
			{
				Debug.LogError((object)"SoundsPerNumberOfEnemies must have Min");
			}
			if (!int.TryParse(obj2.Value, out var result))
			{
				Debug.LogError((object)"SoundsPerNumberOfEnemies must have valid Min");
			}
			int result2 = -1;
			XAttribute val2 = item.Attribute(XName.op_Implicit("Max"));
			if (val2 != null && !int.TryParse(val2.Value, out result2))
			{
				Debug.LogError((object)"SoundsPerNumberOfEnemies Max is invalid");
			}
			List<string> list = new List<string>();
			foreach (XElement item2 in ((XContainer)item).Elements(XName.op_Implicit("SoundId")))
			{
				XAttribute val3 = item2.Attribute(XName.op_Implicit("Value"));
				if (val3 == null)
				{
					Debug.LogError((object)"SoundId must have value");
				}
				list.Add(val3.Value);
			}
			SoundsPerNumberOfEnemies.Add(new Vector2Int(result, result2), list);
		}
	}

	public string GetSoundId(int targetsCount)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		foreach (KeyValuePair<Vector2Int, List<string>> soundsPerNumberOfEnemy in SoundsPerNumberOfEnemies)
		{
			Vector2Int key = soundsPerNumberOfEnemy.Key;
			if (targetsCount < ((Vector2Int)(ref key)).x)
			{
				continue;
			}
			key = soundsPerNumberOfEnemy.Key;
			if (targetsCount > ((Vector2Int)(ref key)).y)
			{
				key = soundsPerNumberOfEnemy.Key;
				if (((Vector2Int)(ref key)).y != -1)
				{
					continue;
				}
			}
			return soundsPerNumberOfEnemy.Value[RandomManager.GetRandomRange(this, 0, soundsPerNumberOfEnemy.Value.Count)];
		}
		Debug.LogError((object)$"No sound found, {targetsCount} targets)");
		return string.Empty;
	}
}
