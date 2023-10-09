using TheLastStand.Definition.Meta.Glyphs;
using TheLastStand.Definition.WorldMap;
using TheLastStand.Model.WorldMap;
using TheLastStand.Serialization;
using TheLastStand.View.WorldMap;

namespace TheLastStand.Controller.WorldMap;

public class WorldMapCityController
{
	public WorldMapCity WorldMapCity { get; private set; }

	public WorldMapCityController(SerializedCity cityElement, CityDefinition definition, WorldMapCityView view, int saveVersion)
	{
		WorldMapCity = new WorldMapCity(definition, this, view);
		WorldMapCity.Deserialize(cityElement, saveVersion);
	}

	public bool CanAddGlyph(GlyphDefinition glyphDefinition)
	{
		if (!WorldMapCity.GlyphsConfig.CustomModeEnabled)
		{
			return WorldMapCity.CurrentGlyphPoints + glyphDefinition.Cost <= WorldMapCity.CityDefinition.MaxGlyphPoints;
		}
		return true;
	}

	public void AddGlyph(GlyphDefinition glyphDefinition)
	{
		WorldMapCity.CurrentGlyphPoints += glyphDefinition.Cost;
		WorldMapCity.GlyphsConfig.SelectedGlyphs.Add(glyphDefinition);
	}

	public void RemoveGlyph(GlyphDefinition glyphDefinition)
	{
		if (WorldMapCity.GlyphsConfig.SelectedGlyphs.Remove(glyphDefinition))
		{
			WorldMapCity.CurrentGlyphPoints -= glyphDefinition.Cost;
		}
	}

	public void RemoveGlyphAt(int index)
	{
		WorldMapCity.CurrentGlyphPoints -= WorldMapCity.GlyphsConfig.SelectedGlyphs[index].Cost;
		WorldMapCity.GlyphsConfig.SelectedGlyphs.RemoveAt(index);
	}
}
