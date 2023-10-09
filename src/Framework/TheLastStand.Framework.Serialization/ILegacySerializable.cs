using System.Xml.Linq;

namespace TheLastStand.Framework.Serialization;

public interface ILegacySerializable : ILegacyDeserializable
{
	XContainer Serialize();
}
