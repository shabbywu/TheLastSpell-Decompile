using TPLib.Localization;
using TheLastStand.Definition;
using TheLastStand.Manager;
using TheLastStand.Model;
using TheLastStand.View;

namespace TheLastStand.Controller;

public class BarkController
{
	public Bark Bark { get; private set; }

	public BarkController(BarkDefinition definition, BarkView view)
	{
		Bark = new Bark(definition, this, view);
		view.Bark = Bark;
		int randomRange = RandomManager.GetRandomRange(this, 0, definition.SentencesCount);
		Bark.Sentence = Localizer.Get($"Bark_{definition.Id}_{randomRange}");
	}
}
