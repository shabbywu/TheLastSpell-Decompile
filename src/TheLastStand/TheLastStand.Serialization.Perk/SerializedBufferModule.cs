using System;
using System.Xml.Serialization;

namespace TheLastStand.Serialization.Perk;

[Serializable]
[XmlInclude(typeof(SerializedGaugeModule))]
public class SerializedBufferModule : SerializedModule, ISerializedData
{
	public int Buffer;

	public int Buffer2;

	public int Buffer3;
}
