using System.Xml.Linq;

namespace TheLastStand.Framework.Serialization;

public interface ILegacyDeserializable
{
	void Deserialize(XContainer container = null);
}
