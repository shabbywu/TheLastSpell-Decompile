namespace TheLastStand.Framework.Serialization;

public interface IDeserializable
{
	void Deserialize(ISerializedData container = null, int saveVersion = -1);
}
