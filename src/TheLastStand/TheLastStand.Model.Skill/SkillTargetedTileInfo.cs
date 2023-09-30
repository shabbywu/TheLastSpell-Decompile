using TheLastStand.Manager;
using TheLastStand.Model.TileMap;

namespace TheLastStand.Model.Skill;

public class SkillTargetedTileInfo
{
	public readonly TileObjectSelectionManager.E_Orientation Orientation;

	public readonly Tile Tile;

	public SkillTargetedTileInfo(Tile tile, TileObjectSelectionManager.E_Orientation orientation)
	{
		Tile = tile;
		Orientation = orientation;
	}
}
