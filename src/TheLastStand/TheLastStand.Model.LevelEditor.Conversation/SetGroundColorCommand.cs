using TheLastStand.Definition;
using TheLastStand.Framework.Command;
using TheLastStand.Framework.Command.Conversation;
using TheLastStand.Framework.Extensions;
using TheLastStand.View.TileMap;
using UnityEngine;

namespace TheLastStand.Model.LevelEditor.Conversation;

public class SetGroundColorCommand : ICompensableCommand, ICommand
{
	private GroundDefinition groundDefinition;

	private Color color = Color.white;

	private Color previousColor = Color.white;

	public SetGroundColorCommand(GroundDefinition groundDefinition, Color color)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		this.groundDefinition = groundDefinition;
		this.color = color;
		switch (this.groundDefinition.GroundCategory)
		{
		case GroundDefinition.E_GroundCategory.City:
			previousColor = TileMapView.GroundCityTilemap.color;
			break;
		case GroundDefinition.E_GroundCategory.NoBuilding:
			previousColor = TileMapView.GroundCraterTilemap.color;
			break;
		}
	}

	public void Compensate()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		SetGroundColor(previousColor);
	}

	public bool Execute()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		SetGroundColor(color);
		return true;
	}

	private void SetGroundColor(Color color)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		switch (groundDefinition.GroundCategory)
		{
		case GroundDefinition.E_GroundCategory.City:
			TileMapView.GroundCityTilemap.color = ColorExtensions.WithA(color, TileMapView.GroundCityTilemap.color.a);
			break;
		case GroundDefinition.E_GroundCategory.NoBuilding:
			TileMapView.GroundCraterTilemap.color = ColorExtensions.WithA(color, TileMapView.GroundCraterTilemap.color.a);
			break;
		}
	}

	public override string ToString()
	{
		return "Set ground " + groundDefinition.Id + " color";
	}
}
