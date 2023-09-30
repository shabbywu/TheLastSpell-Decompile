using System.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.TileMap;
using TheLastStand.Definition;
using TheLastStand.Definition.CastFx;
using TheLastStand.Framework;
using TheLastStand.Framework.Animation;
using TheLastStand.Framework.ExpressionInterpreter;
using TheLastStand.Manager;
using TheLastStand.Manager.Skill;
using TheLastStand.Model;
using TheLastStand.Model.Building;
using TheLastStand.Model.Building.Module;
using TheLastStand.Model.CastFx;
using TheLastStand.View.Camera;
using TheLastStand.View.Sound;
using TheLastStand.View.TileMap;
using UnityEngine;

namespace TheLastStand.View.CastFX;

public class CastFxView : MonoBehaviour
{
	public static class Constants
	{
		public static class Fx
		{
			public const string AnimPlayerPrefabPath = "Prefab/Skills FXs/Skill FX";

			public const string AnimRootFolderPath = "Animation/Skills FXs/";

			public const int SortingOrderBefore = 100;

			public const int SortingOrderBehind = 11;

			public const int SortingOrderAboveAll = 150;
		}

		public static class Sound
		{
			public const string SkillSFXSpatializedObjectPoolName = "Skill SFX Spatialized";

			public const string BuildingsSkillSFXObjectPoolName = "BuildingSkillSFX";

			public const string SFXAudioClipPathPrefix = "Sounds/SFX/Skills SFXs/";

			public const string SFXPrefabPath = "Prefab/Skills SFXs/Skill SFX";

			public const string SFXSpatializedPrefabPath = "Prefab/Skills SFXs/Skill SFX Spatialized";
		}
	}

	private static GameObject animPlayerPrefab;

	private static GameObject AnimPlayerPrefab
	{
		get
		{
			if ((Object)(object)animPlayerPrefab == (Object)null)
			{
				animPlayerPrefab = ResourcePooler.LoadOnce<GameObject>("Prefab/Skills FXs/Skill FX", false);
			}
			return animPlayerPrefab;
		}
	}

	public static void PlayCastFxs(CastFx castFx, TileObjectSelectionManager.E_Orientation specificOrientation = TileObjectSelectionManager.E_Orientation.NONE, Vector2 offset = default(Vector2), ITileObject source = null)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		PlayCastVisualEffects(castFx, specificOrientation, offset);
		PlayCastCamShakes(castFx);
		PlayCastSoundEffects(castFx, source);
	}

	private static int ComputeSortingOrder(CastFx castFx, VisualEffectDefinition visualEffectDefinition, Vector2Int targetPosition)
	{
		return visualEffectDefinition.SortingDepth switch
		{
			VisualEffectDefinition.E_Depth.Before => 100, 
			VisualEffectDefinition.E_Depth.Behind => 11, 
			VisualEffectDefinition.E_Depth.Dynamic => (((Vector2Int)(ref targetPosition)).y < castFx.SourceTile.Y || ((Vector2Int)(ref targetPosition)).x < castFx.SourceTile.X) ? 100 : 11, 
			VisualEffectDefinition.E_Depth.TargetDynamic => (((Vector2Int)(ref targetPosition)).y < castFx.TargetTile.Y || ((Vector2Int)(ref targetPosition)).x < castFx.TargetTile.X) ? 100 : 11, 
			VisualEffectDefinition.E_Depth.AboveAll => 150, 
			_ => 100, 
		};
	}

	private static void PlayCastCamShakes(CastFx castFx)
	{
		foreach (CastFxDefinition.CamShakeDefinition camShakeDefinition in castFx.CastFxDefinition.CamShakeDefinitions)
		{
			ACameraView.Shake(camShakeDefinition.Id, camShakeDefinition.Delay.EvalToFloat((InterpreterContext)(object)castFx.CastFXInterpreterContext));
		}
	}

	private static void PlayCastVisualEffects(CastFx castFx, TileObjectSelectionManager.E_Orientation specificOrientation = TileObjectSelectionManager.E_Orientation.NONE, Vector2 casterOffset = default(Vector2))
	{
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		int i = 0;
		for (int count = castFx.CastFxDefinition.VisualEffectDefinitions.Count; i < count; i++)
		{
			if (castFx.AffectedTiles[i] == null || castFx.AffectedTiles[i].Count == 0)
			{
				continue;
			}
			GameDefinition.E_Direction e_Direction = ((specificOrientation != 0) ? TileObjectSelectionManager.GetDirectionFromOrientation(specificOrientation) : TileMapController.GetDirectionBetweenTiles(castFx.SourceTile, castFx.TargetTile));
			if (e_Direction == GameDefinition.E_Direction.None)
			{
				e_Direction = castFx.TargetTile.Unit?.LookDirection ?? GameDefinition.E_Direction.North;
			}
			AnimationClip clip = ResourcePooler.LoadOnce<AnimationClip>("Animation/Skills FXs/" + castFx.CastFxDefinition.VisualEffectDefinitions[i].GetPath(e_Direction), false);
			if (!(castFx.CastFxDefinition.VisualEffectDefinitions[i] is StandardVisualEffectDefinition standardVisualEffectDefinition))
			{
				continue;
			}
			Vector3 val = Vector3.zero;
			if (standardVisualEffectDefinition.Target.TargetType == StandardVisualEffectDefinition.TargetData.E_TargetType.Caster)
			{
				val = Vector2.op_Implicit(casterOffset);
			}
			int j = 0;
			for (int count2 = castFx.AffectedTiles[i].Count; j < count2; j++)
			{
				SingleAnimPlayer component = Object.Instantiate<GameObject>(AnimPlayerPrefab, GameManager.ViewTransform).GetComponent<SingleAnimPlayer>();
				component.DestroyGoOnFinish = true;
				component.Clip = clip;
				Vector2Int val2 = castFx.AffectedTiles[i][j].Position;
				val2 = TileMapController.GetRotatedTilemapPosition(angle: TileObjectSelectionManager.GetAngleFromOrientation(specificOrientation), offsetFromPivot: standardVisualEffectDefinition.Target.TileOffset, pivotTilemapPosition: val2);
				((Component)component).transform.position = TileMapView.GetWorldPosition(val2) + val;
				((Renderer)((Component)component).GetComponentInChildren<SpriteRenderer>()).sortingOrder = ComputeSortingOrder(castFx, standardVisualEffectDefinition, val2);
				((Object)((Component)component).gameObject).name = $"CastFX_{((Vector2Int)(ref val2)).x}-{((Vector2Int)(ref val2)).y}";
				if (standardVisualEffectDefinition.SpawnedParticlesPath != string.Empty)
				{
					ObjectPooler.GetPooledGameObject(standardVisualEffectDefinition.SpawnedParticlesPath, ResourcePooler.LoadOnce<GameObject>(standardVisualEffectDefinition.SpawnedParticlesPath, false), (Transform)null, false).transform.position = TileMapView.GetWorldPosition(val2) + val;
				}
				float num = castFx.CastFxDefinition.VisualEffectDefinitions[i].Delay.EvalToFloat((InterpreterContext)(object)castFx.CastFXInterpreterContext);
				if (standardVisualEffectDefinition.Target.TargetType == StandardVisualEffectDefinition.TargetData.E_TargetType.PropagationTiles && castFx.CastFxDefinition is SkillCastFxDefinition skillCastFxDefinition)
				{
					num += skillCastFxDefinition.PropagationDelay * (float)j;
				}
				component.Play(num);
			}
		}
	}

	private static void PlayCastSoundEffects(CastFx castFx, ITileObject source = null)
	{
		bool flag = source is TheLastStand.Model.Building.Building || source is BattleModule;
		foreach (SoundEffectDefinition item in castFx.CastFxDefinition.SoundEffectDefinitionsOnCast)
		{
			OneShotSound component = ObjectPooler.GetPooledGameObject(flag ? "BuildingSkillSFX" : "Skill SFX Spatialized", ResourcePooler.LoadOnce<GameObject>("Prefab/Skills SFXs/Skill SFX", false), (Transform)null, false).GetComponent<OneShotSound>();
			((Object)component).name = (source?.Id ?? "UnknownSource") + "_Launch";
			component.PlaySpatialized(GetSoundEffectAudioClip(item), castFx.SourceTile, item.Delay.EvalToFloat());
		}
		foreach (SoundEffectDefinition item2 in castFx.CastFxDefinition.SoundEffectDefinitionsOnImpact)
		{
			OneShotSound component2 = ObjectPooler.GetPooledGameObject(flag ? "BuildingSkillSFX" : "Skill SFX Spatialized", ResourcePooler.LoadOnce<GameObject>("Prefab/Skills SFXs/Skill SFX", false), (Transform)null, false).GetComponent<OneShotSound>();
			((Object)component2).name = (source?.Id ?? "UnknownSource") + "_Impact";
			component2.PlaySpatialized(GetSoundEffectAudioClip(item2), castFx.TargetTile, item2.Delay.EvalToFloat());
		}
	}

	private static AudioClip GetSoundEffectAudioClip(SoundEffectDefinition soundEffectDefinition)
	{
		if (!string.IsNullOrEmpty(soundEffectDefinition.FolderPath))
		{
			AudioClip[] list = ResourcePooler.LoadAllOnce<AudioClip>("Sounds/SFX/Skills SFXs/" + soundEffectDefinition.FolderPath, false);
			return RandomManager.GetRandomElement(TPSingleton<SkillManager>.Instance, list);
		}
		if (!string.IsNullOrEmpty(soundEffectDefinition.Path))
		{
			return ResourcePooler.LoadOnce<AudioClip>("Sounds/SFX/Skills SFXs/" + soundEffectDefinition.Path, false);
		}
		if (soundEffectDefinition.RandomPaths.Count > 0)
		{
			int count = soundEffectDefinition.RandomPaths.Count;
			int num = 0;
			for (int i = 0; i < count; i++)
			{
				num += soundEffectDefinition.RandomPaths.ElementAt(i).Value;
			}
			int num2 = RandomManager.GetRandomRange(TPSingleton<SkillManager>.Instance, 0, num);
			for (int j = 0; j < count; j++)
			{
				num2 -= soundEffectDefinition.RandomPaths.ElementAt(j).Value;
				if (num2 < 0)
				{
					return ResourcePooler.LoadOnce<AudioClip>("Sounds/SFX/Skills SFXs/" + soundEffectDefinition.RandomPaths.ElementAt(j).Key, false);
				}
			}
			return ResourcePooler.LoadOnce<AudioClip>("Sounds/SFX/Skills SFXs/" + soundEffectDefinition.RandomPaths.ElementAt(count - 1).Key, false);
		}
		CLoggerManager.Log((object)"A sound effect should have a Path or some random paths !!", (LogType)3, (CLogLevel)1, true, "StaticLog", false);
		return null;
	}
}
