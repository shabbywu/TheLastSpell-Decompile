using TheLastStand.Model.TileMap;
using UnityEngine;

namespace TheLastStand.Model;

public interface IBarker
{
	bool HasBark { get; set; }

	Tile OriginTile { get; }

	Transform BarkViewFollowTarget { get; }
}
