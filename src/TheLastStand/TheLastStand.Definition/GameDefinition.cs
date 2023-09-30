using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Framework.Serialization;

namespace TheLastStand.Definition;

public class GameDefinition : Definition
{
	public enum E_Direction
	{
		None = -1,
		North,
		South,
		East,
		West
	}

	public GameDefinition(XContainer container)
		: base(container, (Dictionary<string, string>)null)
	{
	}

	public override void Deserialize(XContainer container)
	{
	}
}
