using System.Linq;
using TPLib;
using TPLib.Log;
using TheLastStand.Controller.TileMap;
using TheLastStand.Definition.CastFx;
using TheLastStand.Definition.SpawnFx;
using TheLastStand.Framework;
using TheLastStand.Framework.Animation;
using TheLastStand.Manager;
using TheLastStand.Manager.Skill;
using TheLastStand.Manager.Sound;
using TheLastStand.Model.SpawnFx;
using TheLastStand.View.Camera;
using TheLastStand.View.Sound;
using TheLastStand.View.TileMap;
using UnityEngine;

namespace TheLastStand.View.SpawnFx;

public class SpawnFxView : MonoBehaviour
{
	public static class Constants
	{
		public static class Fx
		{
			public const string AnimPlayerPrefabPath = "Prefab/Spawns FXs/Spawn FX";

			public const string AnimRootFolderPath = "Animation/Spawns FXs/";

			public const int SortingOrderBefore = 100;

			public const int SortingOrderBehind = 11;

			public const int SortingOrderAboveAll = 150;
		}

		public static class Sound
		{
			public const string SkillSFXSpatializedObjectPoolName = "Spawn SFX Spatialized";

			public const string SFXAudioClipPathPrefix = "Sounds/SFX/Spawns SFXs/";

			public const string SFXPrefabPath = "Prefab/Spawns SFXs/Spawn SFX";
		}
	}

	private static GameObject animPlayerPrefab;

	private static GameObject AnimPlayerPrefab
	{
		get
		{
			if ((Object)(object)animPlayerPrefab == (Object)null)
			{
				animPlayerPrefab = ResourcePooler.LoadOnce<GameObject>("Prefab/Spawns FXs/Spawn FX", false);
			}
			return animPlayerPrefab;
		}
	}

	public static void PlaySpawnFxs(TheLastStand.Model.SpawnFx.SpawnFx spawnFx)
	{
		PlaySpawnVisualEffects(spawnFx);
		PlaySpawnCamShakes(spawnFx);
		PlaySpawnSoundEffects(spawnFx);
	}

	private static int ComputeSortingOrder(TheLastStand.Model.SpawnFx.SpawnFx spawnFx, SpawnVisualEffectDefinition spawnVisualEffectDefinition, Vector2Int position)
	{
		switch (spawnVisualEffectDefinition.SortingDepth)
		{
		default:
			return 100;
		case VisualEffectDefinition.E_Depth.Behind:
			return 11;
		case VisualEffectDefinition.E_Depth.Dynamic:
			if (((Vector2Int)(ref position)).y < spawnFx.SourceTile.Y || ((Vector2Int)(ref position)).x < spawnFx.SourceTile.X)
			{
				return 100;
			}
			return 11;
		case VisualEffectDefinition.E_Depth.AboveAll:
			return 150;
		}
	}

	private static AudioClip GetSoundEffectAudioClip(SoundEffectDefinition soundEffectDefinition)
	{
		if (!string.IsNullOrEmpty(soundEffectDefinition.FolderPath))
		{
			AudioClip[] list = ResourcePooler.LoadAllOnce<AudioClip>("Sounds/SFX/Spawns SFXs/" + soundEffectDefinition.FolderPath, false);
			return RandomManager.GetRandomElement(TPSingleton<SkillManager>.Instance, list);
		}
		if (!string.IsNullOrEmpty(soundEffectDefinition.Path))
		{
			return ResourcePooler.LoadOnce<AudioClip>("Sounds/SFX/Spawns SFXs/" + soundEffectDefinition.Path, false);
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
					return ResourcePooler.LoadOnce<AudioClip>("Sounds/SFX/Spawns SFXs/" + soundEffectDefinition.RandomPaths.ElementAt(j).Key, false);
				}
			}
			return ResourcePooler.LoadOnce<AudioClip>("Sounds/SFX/Spawns SFXs/" + soundEffectDefinition.RandomPaths.ElementAt(count - 1).Key, false);
		}
		CLoggerManager.Log((object)"A sound effect should have a Path or some random paths !!", (LogType)3, (CLogLevel)1, true, "StaticLog", false);
		return null;
	}

	private static void PlaySpawnVisualEffects(TheLastStand.Model.SpawnFx.SpawnFx spawnFx)
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		int i = 0;
		for (int count = spawnFx.SpawnFxDefinition.SpawnVisualEffectDefinition.Count; i < count; i++)
		{
			AnimationClip clip = ResourcePooler.LoadOnce<AnimationClip>("Animation/Spawns FXs/" + spawnFx.SpawnFxDefinition.SpawnVisualEffectDefinition[i].Path, false);
			SpawnVisualEffectDefinition spawnVisualEffectDefinition = spawnFx.SpawnFxDefinition.SpawnVisualEffectDefinition[i];
			if (spawnVisualEffectDefinition != null)
			{
				SingleAnimPlayer component = Object.Instantiate<GameObject>(AnimPlayerPrefab, GameManager.ViewTransform).GetComponent<SingleAnimPlayer>();
				component.DestroyGoOnFinish = true;
				component.Clip = clip;
				Vector2Int val = spawnFx.SourceTile.Position;
				val = TileMapController.GetRotatedTilemapPosition(Vector2Int.zero, val, 0f);
				((Component)component).transform.position = TileMapView.GetWorldPosition(val);
				((Renderer)((Component)component).GetComponentInChildren<SpriteRenderer>()).sortingOrder = ComputeSortingOrder(spawnFx, spawnFx.SpawnFxDefinition.SpawnVisualEffectDefinition[i], val);
				((Object)((Component)component).gameObject).name = $"SpawnFX_{((Vector2Int)(ref val)).x}-{((Vector2Int)(ref val)).y}";
				float num = spawnVisualEffectDefinition.Delay.EvalToFloat();
				component.Play(num);
			}
		}
	}

	private static void PlaySpawnCamShakes(TheLastStand.Model.SpawnFx.SpawnFx spawnFx)
	{
		foreach (CastFxDefinition.CamShakeDefinition camShakeDefinition in spawnFx.SpawnFxDefinition.CamShakeDefinitions)
		{
			ACameraView.Shake(camShakeDefinition.Id, camShakeDefinition.Delay.EvalToFloat());
		}
	}

	private static void PlaySpawnSoundEffects(TheLastStand.Model.SpawnFx.SpawnFx spawnFx)
	{
		foreach (SoundEffectDefinition soundEffectDefinition in spawnFx.SpawnFxDefinition.SoundEffectDefinitions)
		{
			OneShotSound component = ObjectPooler.GetPooledGameObject("Spawn SFX Spatialized", ResourcePooler.LoadOnce<GameObject>("Prefab/Spawns SFXs/Spawn SFX", false), (Transform)null, false).GetComponent<OneShotSound>();
			if ((Object)(object)GetSoundEffectAudioClip(soundEffectDefinition) == (Object)null)
			{
				((CLogger<SoundManager>)TPSingleton<SoundManager>.Instance).LogError((object)("Failed at loading AudioClip on path Sounds/SFX/Spawns SFXs/" + soundEffectDefinition.Path), (CLogLevel)1, true, true);
			}
			else
			{
				component.PlaySpatialized(GetSoundEffectAudioClip(soundEffectDefinition), spawnFx.SourceTile, soundEffectDefinition.Delay.EvalToFloat());
			}
		}
	}
}
