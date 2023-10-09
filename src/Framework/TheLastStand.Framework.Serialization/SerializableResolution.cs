using System;
using System.Xml.Serialization;
using UnityEngine;

namespace TheLastStand.Framework.Serialization;

[Serializable]
public struct SerializableResolution
{
	[XmlAttribute]
	public int Width;

	[XmlAttribute]
	public int Height;

	[XmlAttribute]
	public int RefreshRate;

	public SerializableResolution(int width, int height, int refreshRate)
	{
		Width = width;
		Height = height;
		RefreshRate = refreshRate;
	}

	public SerializableResolution(Resolution resolution)
	{
		Width = ((Resolution)(ref resolution)).width;
		Height = ((Resolution)(ref resolution)).height;
		RefreshRate = ((Resolution)(ref resolution)).refreshRate;
	}

	public Resolution Deserialize()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		Resolution result = default(Resolution);
		((Resolution)(ref result)).width = Width;
		((Resolution)(ref result)).height = Height;
		((Resolution)(ref result)).refreshRate = RefreshRate;
		return result;
	}
}
