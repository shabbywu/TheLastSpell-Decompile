using TheLastStand.Definition.Unit.Perk;
using TheLastStand.Serialization.Perk;

namespace TheLastStand.Model.Unit.Perk.PerkModule;

public class BufferModule : APerkModule
{
	public BufferModuleDefinition BufferModuleDefinition => base.PerkModuleDefinition as BufferModuleDefinition;

	public int Buffer { get; set; }

	public int Buffer2 { get; set; }

	public int Buffer3 { get; set; }

	public BufferModule(BufferModuleDefinition perkModuleDefinition, Perk perk)
		: base(perkModuleDefinition, perk)
	{
		Buffer = BufferModuleDefinition.DefaultBufferValue;
	}

	public override void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		SerializedBufferModule serializedBufferModule = container as SerializedBufferModule;
		Buffer = serializedBufferModule.Buffer;
		Buffer2 = serializedBufferModule.Buffer2;
		Buffer3 = serializedBufferModule.Buffer3;
	}

	public override ISerializedData Serialize()
	{
		return new SerializedBufferModule
		{
			Buffer = Buffer,
			Buffer2 = Buffer2,
			Buffer3 = Buffer3
		};
	}

	public override void ResetDynamicData()
	{
		base.ResetDynamicData();
		Buffer = BufferModuleDefinition.DefaultBufferValue;
		Buffer2 = 0;
		Buffer3 = 0;
	}
}
