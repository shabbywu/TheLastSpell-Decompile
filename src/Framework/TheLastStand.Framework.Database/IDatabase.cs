using TheLastStand.Framework.Serialization;

namespace TheLastStand.Framework.Database;

public interface IDatabase : ILegacyDeserializable
{
	bool GetHasBeenDeserialized();

	void SetHasBeenDeserialized(bool hasBeenDeserialized);
}
