using System.Collections.Generic;
using TheLastStand.Controller.Unit.Perk;

namespace TheLastStand.Model.Unit.Perk;

public class PlayableUnitPerks
{
	public Dictionary<string, List<string>> ActiveReplaceEffects = new Dictionary<string, List<string>>();

	public Dictionary<string, Perk> Perks => PlayableUnit.Perks;

	public PlayableUnit PlayableUnit { get; }

	public PlayableUnitPerksController PlayableUnitPerksController { get; }

	public PlayableUnitPerks(PlayableUnitPerksController playableUnitPerksController, PlayableUnit playableUnit)
	{
		PlayableUnitPerksController = playableUnitPerksController;
		PlayableUnit = playableUnit;
	}
}
