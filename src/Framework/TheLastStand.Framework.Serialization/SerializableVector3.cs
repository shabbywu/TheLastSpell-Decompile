using System;
using UnityEngine;

namespace TheLastStand.Framework.Serialization;

[Serializable]
public struct SerializableVector3
{
	public float X;

	public float Y;

	public float Z;

	public SerializableVector3(Vector3 vec3)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		X = vec3.x;
		Y = vec3.y;
		Z = vec3.z;
	}
}
