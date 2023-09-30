using System.Collections;
using TPLib;
using TheLastStand.Framework;
using TheLastStand.Framework.Maths;
using TheLastStand.Framework.Serialization;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model;
using TheLastStand.Serialization;
using UnityEngine;

namespace TheLastStand.View.Camera;

public class CameraLUTView : MonoBehaviour, ISerializable, IDeserializable
{
	private static class Constants
	{
		public const string CityLUTDefaultId = "Default";

		public const string CityLUTResourcesPathFormatDawn = "View/Sprites/VFX/LUT/{0}/LUT_{0}_Dawn";

		public const string CityLUTResourcesPathFormatDeployment = "View/Sprites/VFX/LUT/{0}/LUT_{0}_Deployment";

		public const string CityLUTResourcesPathFormatNight = "View/Sprites/VFX/LUT/{0}/LUT_{0}_Night";

		public const string CityLUTResourcesPathFormatPillar = "View/Sprites/VFX/LUT/{0}/LUT_{0}_Pillar";

		public const string CityLUTResourcesPathFormatProduction = "View/Sprites/VFX/LUT/{0}/LUT_{0}_Production";
	}

	[SerializeField]
	private AmplifyColorEffect amplifyColor;

	[SerializeField]
	[Range(0f, 10f)]
	private float lutTransitionDuration = 2.5f;

	[SerializeField]
	[Range(0f, 1f)]
	private float lutDawnStartPercentage = 0.25f;

	[SerializeField]
	[Range(0f, 1f)]
	private float lutDawnTransitionMaxPercentage = 1f;

	[SerializeField]
	[Tooltip("If no Dawn LUT exists, can the night to day transition use the production LUT ?")]
	private bool canTransitionToProductionLut = true;

	private Texture lutDawn;

	private Texture lutProduction;

	private Texture lutDeployment;

	private Texture lutNight;

	private Texture lutPillar;

	private float targetBlendAmount = 1f;

	private int dawnStartRemainingEnemies = -1;

	public void UpdateLutTextures()
	{
		string cityId = TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.Id;
		int dayNumber = TPSingleton<GameManager>.Instance.Game.DayNumber;
		lutProduction = GetLutTexture("View/Sprites/VFX/LUT/{0}/LUT_{0}_Production");
		lutDawn = GetLutTexture("View/Sprites/VFX/LUT/{0}/LUT_{0}_Dawn") ?? lutProduction;
		lutDeployment = GetLutTexture("View/Sprites/VFX/LUT/{0}/LUT_{0}_Deployment");
		lutNight = GetLutTexture("View/Sprites/VFX/LUT/{0}/LUT_{0}_Night");
		lutPillar = GetLutTexture("View/Sprites/VFX/LUT/{0}/LUT_{0}_Pillar");
		Texture GetLutTexture(string path)
		{
			return ResourcePooler.LoadOnce<Texture>($"{string.Format(path, cityId)}_{dayNumber}", true) ?? ResourcePooler.LoadOnce<Texture>(string.Format(path, cityId) ?? "", true) ?? ResourcePooler.LoadOnce<Texture>(string.Format(path, "Default") ?? "", true);
		}
	}

	public void TogglePillarLut(bool state)
	{
		if (!((Object)(object)lutPillar == (Object)null))
		{
			if (state)
			{
				((AmplifyColorBase)amplifyColor).LutTexture = lutPillar;
				((AmplifyColorBase)amplifyColor).BlendAmount = 0f;
			}
			else
			{
				RefreshLut(instant: true);
			}
		}
	}

	public void RefreshLut(bool instant = false)
	{
		Texture val = null;
		targetBlendAmount = 1f;
		switch (TPSingleton<GameManager>.Instance.Game.Cycle)
		{
		case Game.E_Cycle.Night:
			if (SpawnWaveManager.CurrentSpawnWave != null)
			{
				int num = TPSingleton<EnemyUnitManager>.Instance.ComputedEnemyUnitsCount + SpawnWaveManager.CurrentSpawnWave.UnableToSpawnCount;
				if (dawnStartRemainingEnemies == -1 && (canTransitionToProductionLut || (Object)(object)lutDawn != (Object)(object)lutProduction) && !SpawnWaveManager.CurrentSpawnWave.SpawnWaveDefinition.IsBossWave && TPSingleton<GameManager>.Instance.Game.CurrentNightHour >= SpawnWaveManager.CurrentSpawnWave.SpawnWaveDefinition.Duration)
				{
					dawnStartRemainingEnemies = num;
					TPSingleton<BarkManager>.Instance.CheckDawnStart();
				}
				if (dawnStartRemainingEnemies > -1)
				{
					val = lutDawn;
					float num2 = Maths.Normalize01((float)num, (float)dawnStartRemainingEnemies, 0f);
					num2 = Maths.NormalizeClamped(num2, 0f, 1f, lutDawnStartPercentage, 1f);
					targetBlendAmount = Mathf.Min(num2, lutDawnTransitionMaxPercentage);
				}
				else
				{
					val = lutNight;
				}
			}
			else
			{
				val = lutNight;
			}
			break;
		case Game.E_Cycle.Day:
			val = ((TPSingleton<GameManager>.Instance.Game.State != Game.E_State.NightReport || TPSingleton<GameManager>.Instance.NightReportToDayCoroutineRunning) ? ((TPSingleton<GameManager>.Instance.Game.DayTurn == Game.E_DayTurn.Production) ? lutProduction : lutDeployment) : lutDawn);
			dawnStartRemainingEnemies = -1;
			break;
		}
		if (instant || lutTransitionDuration == 0f)
		{
			((AmplifyColorBase)amplifyColor).LutTexture = val;
			((AmplifyColorBase)amplifyColor).LutBlendTexture = val;
			((AmplifyColorBase)amplifyColor).BlendAmount = targetBlendAmount;
		}
		else
		{
			((MonoBehaviour)this).StopAllCoroutines();
			((MonoBehaviour)this).StartCoroutine(BlendLutCoroutine(val, targetBlendAmount));
		}
	}

	private IEnumerator BlendLutCoroutine(Texture targetLut, float targetAmount)
	{
		if (!((Object)(object)targetLut == (Object)(object)((AmplifyColorBase)amplifyColor).LutTexture))
		{
			((AmplifyColorBase)amplifyColor).LutBlendTexture = targetLut;
			float num = ((((AmplifyColorBase)amplifyColor).BlendAmount < 1f) ? ((AmplifyColorBase)amplifyColor).BlendAmount : 0f);
			for (float t = num; t <= targetAmount; t += Time.deltaTime / lutTransitionDuration)
			{
				((AmplifyColorBase)amplifyColor).BlendAmount = t;
				yield return null;
			}
			if (targetAmount == 1f)
			{
				((AmplifyColorBase)amplifyColor).LutTexture = targetLut;
				((AmplifyColorBase)amplifyColor).LutBlendTexture = null;
				((AmplifyColorBase)amplifyColor).BlendAmount = 0f;
			}
		}
	}

	public void Deserialize(ISerializedData container = null, int saveVersion = -1)
	{
		SerializedLUT serializedLUT = container as SerializedLUT;
		UpdateLutTextures();
		Texture val = null;
		Texture lutTexture = null;
		dawnStartRemainingEnemies = serializedLUT?.DawnStartRemainingEnemies ?? (-1);
		targetBlendAmount = serializedLUT?.TargetBlendAmount ?? 1f;
		switch (TPSingleton<GameManager>.Instance.Game.Cycle)
		{
		case Game.E_Cycle.Night:
			lutTexture = lutNight;
			val = ((dawnStartRemainingEnemies > -1) ? lutDawn : lutNight);
			break;
		case Game.E_Cycle.Day:
			val = ((TPSingleton<GameManager>.Instance.Game.DayTurn == Game.E_DayTurn.Production) ? lutProduction : lutDeployment);
			lutTexture = val;
			dawnStartRemainingEnemies = -1;
			break;
		}
		((AmplifyColorBase)amplifyColor).LutTexture = lutTexture;
		((AmplifyColorBase)amplifyColor).LutBlendTexture = val;
		((AmplifyColorBase)amplifyColor).BlendAmount = targetBlendAmount;
	}

	public ISerializedData Serialize()
	{
		return (ISerializedData)(object)new SerializedLUT
		{
			DawnStartRemainingEnemies = ((TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night) ? dawnStartRemainingEnemies : (-1)),
			TargetBlendAmount = ((TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Night) ? targetBlendAmount : 1f)
		};
	}
}
