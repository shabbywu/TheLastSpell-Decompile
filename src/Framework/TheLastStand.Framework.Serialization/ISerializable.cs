namespace TheLastStand.Framework.Serialization;

public interface ISerializable : IDeserializable
{
	ISerializedData Serialize();
}
