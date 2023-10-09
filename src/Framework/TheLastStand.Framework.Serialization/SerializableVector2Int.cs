using System;
using System.Xml.Serialization;
using UnityEngine;

namespace TheLastStand.Framework.Serialization;

[Serializable]
public struct SerializableVector2Int
{
	[XmlAttribute]
	public int X;

	[XmlAttribute]
	public int Y;

	public SerializableVector2Int(Vector2Int vec2)
	{
		X = ((Vector2Int)(ref vec2)).x;
		Y = ((Vector2Int)(ref vec2)).y;
	}

	public Vector2Int Deserialize()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2Int(X, Y);
	}
}
