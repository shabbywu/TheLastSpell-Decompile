using TheLastStand.Database.Building;
using TheLastStand.Database.Unit;
using TheLastStand.Definition.Building;
using TheLastStand.Framework.Extensions;
using TheLastStand.Framework.Serialization;
using TheLastStand.Model;

namespace TheLastStand.Definition.Unit.Enemy.Boss;

public static class ActorDefinitionExtension
{
	public static Definition GetCorrespondingDefinition(this ActorDefinition actorDefinition)
	{
		return (Definition)(actorDefinition.ActorType switch
		{
			DamageableType.Enemy => DictionaryExtensions.GetValueOrDefault<string, EnemyUnitTemplateDefinition>(EnemyUnitDatabase.EnemyUnitTemplateDefinitions, actorDefinition.UnitId), 
			DamageableType.Boss => DictionaryExtensions.GetValueOrDefault<string, BossUnitTemplateDefinition>(BossUnitDatabase.BossUnitTemplateDefinitions, actorDefinition.UnitId), 
			DamageableType.Building => DictionaryExtensions.GetValueOrDefault<string, BuildingDefinition>(BuildingDatabase.BuildingDefinitions, actorDefinition.UnitId), 
			_ => null, 
		});
	}
}
