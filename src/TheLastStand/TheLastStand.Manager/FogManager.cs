using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TPLib;
using TPLib.Debugging.Console;
using TPLib.Yield;
using TheLastStand.Controller;
using TheLastStand.Controller.TileMap;
using TheLastStand.Database.Fog;
using TheLastStand.Definition.Apocalypse.LightFogSpawner;
using TheLastStand.Definition.Meta.Glyphs.GlyphEffects;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Meta;
using TheLastStand.Manager.Unit;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model;
using TheLastStand.Model.TileMap;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.Serialization;
using TheLastStand.View;
using TheLastStand.View.Camera;
using TheLastStand.View.TileMap;
using UnityEngine;

namespace TheLastStand.Manager;

public sealed class FogManager : Manager<FogManager>, ISerializable, IDeserializable
{
	public static class Constants
	{
		public const string AllowingLightFogSpawnersIdsList = "AllowingLightFogSpawners";
	}

	[Serializable]
	public class LightFogFadeEaseAndDuration
	{
		public float duration;

		public Ease ease;
	}

	[SerializeField]
	private FogView fogView;

	[SerializeField]
	private LightFogFadeEaseAndDuration lightFogFadeInEaseAndDuration;

	[SerializeField]
	private LightFogFadeEaseAndDuration lightFogFadeOutEaseAndDuration;

	[SerializeField]
	private LightFogFadeEaseAndDuration lightFogDisappearEaseAndDuration;

	[SerializeField]
	private LightFogFadeEaseAndDuration movingLightFogFadeInEaseAndDuration;

	[SerializeField]
	private LightFogFadeEaseAndDuration movingLightFogFadeOutEaseAndDuration;

	[SerializeField]
	private LightFogFadeEaseAndDuration movingLightFogDisappearEaseAndDuration;

	private bool initialized;

	public static Dictionary<Tile, Tween> LightFogTweens = new Dictionary<Tile, Tween>();

	public bool AddLightFogOnClick;

	public bool WillEditLightFogNextFrame;

	public int FogMaxIndex { get; private set; }

	public static LightFogFadeEaseAndDuration LightFogFadeInEaseAndDuration => TPSingleton<FogManager>.Instance.lightFogFadeInEaseAndDuration;

	public static LightFogFadeEaseAndDuration LightFogFadeOutEaseAndDuration => TPSingleton<FogManager>.Instance.lightFogFadeOutEaseAndDuration;

	public static LightFogFadeEaseAndDuration MovingLightFogFadeInEaseAndDuration => TPSingleton<FogManager>.Instance.movingLightFogFadeInEaseAndDuration;

	public static LightFogFadeEaseAndDuration MovingLightFogFadeOutEaseAndDuration => TPSingleton<FogManager>.Instance.movingLightFogFadeOutEaseAndDuration;

	public static LightFogFadeEaseAndDuration LightFogDisappearEaseAndDuration => TPSingleton<FogManager>.Instance.lightFogDisappearEaseAndDuration;

	public static LightFogFadeEaseAndDuration MovingLightFogDisappearEaseAndDuration => TPSingleton<FogManager>.Instance.movingLightFogDisappearEaseAndDuration;

	public static bool MovingCameraToFog { get; private set; }

	public Fog Fog { get; private set; }

	public FogView FogView => fogView;

	public FogManager(SerializedFog container)
	{
		Deserialize(container);
	}

	public static List<Tile> GetLightFogRepelTiles(Tile sourceTile)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		if (FogDatabase.LightFogDefinition.Repel.CheckDiagonals)
		{
			Vector2Int position = sourceTile.Position;
			int num = ((Vector2Int)(ref position)).x - FogDatabase.LightFogDefinition.Repel.Range;
			position = sourceTile.Position;
			return TileMapController.GetTilesInRect(new RectInt(num, ((Vector2Int)(ref position)).y - FogDatabase.LightFogDefinition.Repel.Range, FogDatabase.LightFogDefinition.Repel.Range * 2, FogDatabase.LightFogDefinition.Repel.Range * 2)).ToList();
		}
		return TileMapController.GetTilesInRange(sourceTile, FogDatabase.LightFogDefinition.Repel.Range);
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		Init();
		FogMaxIndex = int.MaxValue;
		if (GlyphManager.TryGetGlyphEffects(out List<GlyphSetFogCapEffectDefinition> glyphEffects))
		{
			foreach (GlyphSetFogCapEffectDefinition item in glyphEffects)
			{
				for (int i = 0; i < Fog.FogDefinition.FogDensities.Count; i++)
				{
					if (FogMaxIndex > i && Fog.FogDefinition.FogDensities[i].Name == item.IndexName)
					{
						FogMaxIndex = i;
						break;
					}
				}
			}
		}
		if (container is SerializedFog serializedFog)
		{
			Fog.DensityIndex = serializedFog.FogDensityIndex;
			Fog.DailyUpdateFrequency = serializedFog.FogDailyUpdateFrequency;
			FogController.SetFogTilesAndRecomputeSpawnPoints(instant: true);
		}
	}

	public void GenerateLightFogSpawners()
	{
		LightFogSpawnersGenerationDefinition lightFogSpawnersGenerationDefinition = TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.LightFogSpawnersGenerationDefinition;
		int count = lightFogSpawnersGenerationDefinition.InitialCount + lightFogSpawnersGenerationDefinition.ScalingCount * Mathf.FloorToInt((float)TPSingleton<GameManager>.Instance.Game.DayNumber / (float)lightFogSpawnersGenerationDefinition.Period);
		TPSingleton<BuildingManager>.Instance.GenerateLightFogSpawners(ApocalypseManager.CurrentApocalypse.GetModifiedLightFogSpawnersCount(count));
	}

	public void Init()
	{
		if (!initialized)
		{
			Fog = new Fog(FogDatabase.FogsDefinitions[TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.FogDefinitionId]);
			FogController.SetFogTiles(instant: true);
			TileMapView.SetFogOutlinesTileBases();
			initialized = true;
		}
	}

	public IEnumerator MoveCameraToNearestFogWithWave(Action callback = null, float callbackDelay = 0f)
	{
		MovingCameraToFog = true;
		ACameraView.Zoom(zoomIn: true);
		ACameraView.MoveTo(FindNearestFogWithWavePosition().position, FogView.FogFocusTime, (Ease)0);
		yield return SharedYields.WaitForSeconds(callbackDelay);
		callback?.Invoke();
		MovingCameraToFog = false;
	}

	public ISerializedData Serialize()
	{
		return new SerializedFog
		{
			FogDensityIndex = Fog.DensityIndex,
			FogDailyUpdateFrequency = Fog.DailyUpdateFrequency
		};
	}

	public void StartTurn()
	{
		FogController.RefreshLightFog();
	}

	protected override void Awake()
	{
		base.Awake();
		Init();
	}

	private void Update()
	{
		DebugUpdate();
	}

	private Transform FindNearestFogWithWavePosition()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		SpawnWave currentSpawnWave = SpawnWaveManager.CurrentSpawnWave;
		Transform val = null;
		Vector3 position = ((Component)TPSingleton<ACameraView>.Instance).transform.position;
		for (int i = 0; i < SpawnWaveManager.SpawnWaveView.SpawnWavePreviewFeedbacks.Count; i++)
		{
			if (!currentSpawnWave.RotatedProportionPerDirection.ContainsKey(SpawnWaveManager.SpawnWaveView.SpawnWavePreviewFeedbacks[i].CentralSpawnDirection))
			{
				continue;
			}
			if (!((Object)(object)val == (Object)null))
			{
				Vector3 val2 = ((Component)SpawnWaveManager.SpawnWaveView.SpawnWavePreviewFeedbacks[i].SpawnWaveViewPreviewFeedback).transform.position - position;
				float sqrMagnitude = ((Vector3)(ref val2)).sqrMagnitude;
				val2 = val.position - position;
				if (!(sqrMagnitude < ((Vector3)(ref val2)).sqrMagnitude))
				{
					continue;
				}
			}
			val = ((Component)SpawnWaveManager.SpawnWaveView.SpawnWavePreviewFeedbacks[i].SpawnWaveViewPreviewFeedback).transform;
		}
		return val;
	}

	[DevConsoleCommand("IncreaseFogDensity")]
	public static void DebugIncreaseFogDensity(int value = 1)
	{
		if (value >= 1)
		{
			FogController.IncreaseDensity(refreshFog: true, value);
			SpawnWaveManager.SpawnWaveView.Refresh();
		}
	}

	[DevConsoleCommand("DecreaseFogDensity")]
	public static void DebugDecreaseFogDensity(int value = 1)
	{
		if (value >= 1)
		{
			FogController.DecreaseDensity(refreshFog: true, value);
			SpawnWaveManager.SpawnWaveView.Refresh();
		}
	}

	[DevConsoleCommand("LightFogOnClick")]
	[ContextMenu("LightFogOnClick")]
	public static void DebugSetLightFogOnClick(bool state = true)
	{
		TPSingleton<FogManager>.Instance.AddLightFogOnClick = state;
		TPSingleton<FogManager>.Instance.WillEditLightFogNextFrame = false;
	}

	[DevConsoleCommand("LightFogSpawnersRegenerate")]
	public static void DebugRegenerateLightFogSpawners()
	{
		((MonoBehaviour)TPSingleton<FogManager>.Instance).StopCoroutine(DebugRegenerateLightFogSpawnersCoroutine());
		((MonoBehaviour)TPSingleton<FogManager>.Instance).StartCoroutine(DebugRegenerateLightFogSpawnersCoroutine());
	}

	private static IEnumerator DebugRegenerateLightFogSpawnersCoroutine()
	{
		TPSingleton<BuildingManager>.Instance.DestroyLightFogSpawners();
		yield return (object)new WaitForSeconds(1f);
		TPSingleton<FogManager>.Instance.GenerateLightFogSpawners();
	}

	private void DebugUpdate()
	{
		if (TPSingleton<FogManager>.Instance.AddLightFogOnClick && TPSingleton<GameManager>.Instance.Game.State == Game.E_State.Management)
		{
			if (InputManager.GetButton(24))
			{
				if (!WillEditLightFogNextFrame)
				{
					WillEditLightFogNextFrame = true;
					return;
				}
				Tile tile = TPSingleton<GameManager>.Instance.Game.Cursor.Tile;
				if (tile != null && !TPSingleton<FogManager>.Instance.Fog.LightFogTiles.ContainsKey(tile))
				{
					FogController.SetLightFogTilesFromDictionnary(FogController.IncrementLightFogTilesBuffer(new List<Tile> { tile }), LightFogFadeInEaseAndDuration, LightFogFadeOutEaseAndDuration, LightFogDisappearEaseAndDuration);
				}
				WillEditLightFogNextFrame = false;
			}
			else
			{
				if (!InputManager.GetButton(137))
				{
					return;
				}
				if (!WillEditLightFogNextFrame)
				{
					WillEditLightFogNextFrame = true;
					return;
				}
				Tile tile2 = TPSingleton<GameManager>.Instance.Game.Cursor.Tile;
				if (tile2 != null && TPSingleton<FogManager>.Instance.Fog.LightFogTiles.ContainsKey(tile2))
				{
					FogController.SetLightFogTilesFromDictionnary(FogController.DecrementLightFogTilesBuffer(new List<Tile> { tile2 }), LightFogFadeInEaseAndDuration, LightFogFadeOutEaseAndDuration, LightFogDisappearEaseAndDuration);
				}
				WillEditLightFogNextFrame = false;
			}
		}
		else
		{
			WillEditLightFogNextFrame = false;
		}
	}
}
