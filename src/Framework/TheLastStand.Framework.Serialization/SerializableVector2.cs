using System;
using UnityEngine;

namespace TheLastStand.Framework.Serialization;

[Serializable]
public struct SerializableVector2
{
	public float X;

	public float Y;

	public SerializableVector2(Vector2 vec2)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		X = vec2.x;
		Y = vec2.y;
	}
}
