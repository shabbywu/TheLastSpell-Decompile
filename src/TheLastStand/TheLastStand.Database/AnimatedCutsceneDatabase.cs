using System.Collections.Generic;
using System.Xml.Linq;
using TheLastStand.Definition.AnimatedCutscene;
using TheLastStand.Framework.Database;
using UnityEngine;

namespace TheLastStand.Database;

public class AnimatedCutsceneDatabase : Database<AnimatedCutsceneDatabase>
{
	[SerializeField]
	private List<TextAsset> animatedCutsceneDefinitionTextAsset;

	public static Dictionary<string, AnimatedCutsceneDefinition> AnimatedCutsceneDefinitions { get; private set; } = new Dictionary<string, AnimatedCutsceneDefinition>();


	public override void Deserialize(XContainer container = null)
	{
		Queue<XElement> queue = base.GatherElements((IEnumerable<TextAsset>)null, (IEnumerable<TextAsset>)animatedCutsceneDefinitionTextAsset, "AnimatedCutsceneDefinition", (string)null);
		while (queue.Count > 0)
		{
			AnimatedCutsceneDefinition animatedCutsceneDefinition = new AnimatedCutsceneDefinition((XContainer)(object)queue.Dequeue());
			AnimatedCutsceneDefinitions.Add(animatedCutsceneDefinition.Id, animatedCutsceneDefinition);
		}
	}
}
