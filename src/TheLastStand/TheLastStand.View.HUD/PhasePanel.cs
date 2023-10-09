using DG.Tweening;
using TMPro;
using TPLib;
using TheLastStand.Framework;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Manager.WorldMap;
using TheLastStand.Model;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.View.Skill.SkillAction.UI;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.HUD;

public class PhasePanel : MonoBehaviour
{
	public static class Constants
	{
		public const string GoldEffectDisplayName = "TurnPanelGainGoldDisplay";

		public const string MaterialsEffectDisplayName = "TurnPanelGainMaterialsDisplay";
	}

	[SerializeField]
	private Image goldIconImage;

	[SerializeField]
	private Sprite goldIconSpriteOn;

	[SerializeField]
	private Sprite goldIconSpriteOff;

	[SerializeField]
	private ResourceTextDisplay goldDisplay;

	[SerializeField]
	private Color goldTextDisabledColor = new Color(0.254902f, 0.254902f, 0.254902f);

	[SerializeField]
	private GainGoldDisplay goldDisplayPrefab;

	[SerializeField]
	private Image workersIconImage;

	[SerializeField]
	private Sprite workersIconSpriteOn;

	[SerializeField]
	private Sprite workersIconSpriteOff;

	[SerializeField]
	private TextMeshProUGUI workersText;

	[SerializeField]
	private Color workersTextDisabledColor = new Color(0.254902f, 0.254902f, 0.254902f);

	[SerializeField]
	private Image deploymentIconImage;

	[SerializeField]
	private Sprite deploymentIconSpriteOn;

	[SerializeField]
	private Sprite deploymentIconSpriteOff;

	[SerializeField]
	private Image materialsIconImage;

	[SerializeField]
	private Sprite materialsIconSpriteOn;

	[SerializeField]
	private Sprite materialsIconSpriteOff;

	[SerializeField]
	private ResourceTextDisplay materialsDisplay;

	[SerializeField]
	private Color materialsTextDisabledColor = new Color(0.254902f, 0.254902f, 0.254902f);

	[SerializeField]
	private GainMaterialDisplay materialsDisplayPrefab;

	[SerializeField]
	private BetterSlider nightSlider;

	[SerializeField]
	[Range(0.1f, 1f)]
	private float nightSliderTweenDuration = 0.25f;

	[SerializeField]
	private Image nightSliderBackgroundImage;

	[SerializeField]
	private BetterImage nightSliderFillImage;

	[SerializeField]
	private Sprite nightSliderBackgroundSpriteOn;

	[SerializeField]
	private Sprite nightSliderBackgroundSpriteOff;

	[SerializeField]
	private Sprite bossPhaseNightSliderBackgroundSpriteOn;

	[SerializeField]
	private Sprite nightSliderFillSprite;

	[SerializeField]
	private Sprite bossPhaseNightSliderFillSprite;

	[SerializeField]
	private Image nightSliderHandlerImage;

	[SerializeField]
	private Sprite nightSliderHandlerSpriteOn;

	[SerializeField]
	private Sprite dawnSliderHandlerSpriteOn;

	[SerializeField]
	private Sprite nightSliderHandlerSpriteOff;

	[SerializeField]
	private Sprite bossPhaseNightSliderHandlerSpriteOn;

	[SerializeField]
	private Sprite bossPhaseNightSliderHandlerSpriteOff;

	[SerializeField]
	private Image remainingEnemiesIconImage;

	[SerializeField]
	private TextMeshProUGUI remainingEnemiesText;

	[SerializeField]
	private Graphic remainingEnemiesTooltipDisplayer;

	[SerializeField]
	private Image soulsIconImage;

	[SerializeField]
	private GameObject soulsTooltipDisplayer;

	[SerializeField]
	private TextMeshProUGUI soulsText;

	private Sequence nightSliderSequence;

	private float nextRefreshNightSliderValue = -1f;

	public float CurrentNightProgressionValue => nightSlider.value;

	public bool RemainingEnemiesTextEnabled => ((Behaviour)remainingEnemiesText).enabled;

	public void Refresh()
	{
		if (TPSingleton<WorldMapCityManager>.Instance.SelectedCity.CityDefinition.IsTutorialMap)
		{
			((Component)soulsText).gameObject.SetActive(false);
			((Component)soulsIconImage).gameObject.SetActive(false);
			soulsTooltipDisplayer.gameObject.SetActive(false);
		}
		RefreshResourcesPhasePanel();
		RefreshDeploymentPhasePanel();
		RefreshNightPanel();
	}

	public void RefreshDeploymentPhasePanel()
	{
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Day)
		{
			deploymentIconImage.sprite = deploymentIconSpriteOn;
		}
		else
		{
			deploymentIconImage.sprite = deploymentIconSpriteOff;
		}
	}

	public void RefreshNightPanel()
	{
		RefreshSoulsText();
		RefreshNightSliderValues();
		RefreshNightSliderAssets();
	}

	public void RefreshNightSliderValues()
	{
		SpawnWave currentSpawnWave = SpawnWaveManager.CurrentSpawnWave;
		bool flag = currentSpawnWave != null && TPSingleton<GameManager>.Instance.Game.CurrentNightHour >= currentSpawnWave.SpawnWaveDefinition.Duration;
		switch (TPSingleton<GameManager>.Instance.Game.Cycle)
		{
		case Game.E_Cycle.Night:
			if (currentSpawnWave != null)
			{
				if (TPSingleton<GameManager>.Instance.Game.NightTurn == Game.E_NightTurn.PlayableUnits)
				{
					if (!currentSpawnWave.SpawnWaveDefinition.IsBossWave || currentSpawnWave.SpawnWaveDefinition.BossWaveSettings.AutoUpdateNightSlider)
					{
						SetNightSliderValue((float)TPSingleton<GameManager>.Instance.Game.CurrentNightHour / (float)currentSpawnWave.SpawnWaveDefinition.Duration);
					}
					else if (nextRefreshNightSliderValue > 0f)
					{
						SetNightSliderValue(nextRefreshNightSliderValue);
						nextRefreshNightSliderValue = -1f;
					}
					bool flag2 = flag && (!currentSpawnWave.SpawnWaveDefinition.IsBossWave || TPSingleton<BossManager>.Instance.IsBossVanquished || currentSpawnWave.SpawnWaveDefinition.BossWaveSettings.DisplayEnemiesAmount);
					((TMP_Text)remainingEnemiesText).text = (flag2 ? $"{TPSingleton<EnemyUnitManager>.Instance.ComputedEnemyUnitsCount + SpawnWaveManager.CurrentSpawnWave.UnableToSpawnCount}" : string.Empty);
					((Behaviour)remainingEnemiesText).enabled = flag2;
				}
				break;
			}
			goto case Game.E_Cycle.Day;
		case Game.E_Cycle.Day:
			SetNightSliderValue(0f);
			((TMP_Text)remainingEnemiesText).text = string.Empty;
			((Behaviour)remainingEnemiesText).enabled = false;
			break;
		}
	}

	public void RefreshNightSliderAssets()
	{
		SpawnWave currentSpawnWave = SpawnWaveManager.CurrentSpawnWave;
		bool flag = currentSpawnWave != null && currentSpawnWave.SpawnWaveDefinition.IsBossWave && !currentSpawnWave.SpawnWaveDefinition.BossWaveSettings.UseDefaultProgressBar;
		nightSliderFillImage.sprite = (flag ? bossPhaseNightSliderFillSprite : nightSliderFillSprite);
		switch (TPSingleton<GameManager>.Instance.Game.Cycle)
		{
		case Game.E_Cycle.Day:
			nightSliderBackgroundImage.sprite = nightSliderBackgroundSpriteOff;
			nightSliderHandlerImage.sprite = (flag ? bossPhaseNightSliderHandlerSpriteOff : nightSliderHandlerSpriteOff);
			((Graphic)nightSliderHandlerImage).SetNativeSize();
			((Behaviour)remainingEnemiesIconImage).enabled = false;
			remainingEnemiesTooltipDisplayer.raycastTarget = false;
			break;
		case Game.E_Cycle.Night:
			if (TPSingleton<GameManager>.Instance.Game.NightTurn == Game.E_NightTurn.PlayableUnits)
			{
				bool flag2 = currentSpawnWave != null && TPSingleton<GameManager>.Instance.Game.CurrentNightHour >= currentSpawnWave.SpawnWaveDefinition.Duration;
				bool flag3 = flag2 && (!currentSpawnWave.SpawnWaveDefinition.IsBossWave || TPSingleton<BossManager>.Instance.IsBossVanquished || currentSpawnWave.SpawnWaveDefinition.BossWaveSettings.DisplayEnemiesAmount);
				((Behaviour)remainingEnemiesIconImage).enabled = flag3;
				remainingEnemiesTooltipDisplayer.raycastTarget = flag3;
				if (flag)
				{
					nightSliderBackgroundImage.sprite = bossPhaseNightSliderBackgroundSpriteOn;
					nightSliderHandlerImage.sprite = bossPhaseNightSliderHandlerSpriteOn;
				}
				else
				{
					nightSliderBackgroundImage.sprite = nightSliderBackgroundSpriteOn;
					nightSliderHandlerImage.sprite = (flag2 ? dawnSliderHandlerSpriteOn : nightSliderHandlerSpriteOn);
				}
				((Graphic)nightSliderHandlerImage).SetNativeSize();
			}
			break;
		}
	}

	public void RefreshResourcesPhasePanel()
	{
		RefreshGold(TPSingleton<ResourceManager>.Instance.Gold);
		RefreshMaterials(TPSingleton<ResourceManager>.Instance.Materials);
		RefreshWorkers(TPSingleton<ResourceManager>.Instance.Workers, TPSingleton<ResourceManager>.Instance.MaxWorkers);
	}

	public void RefreshSoulsText()
	{
		((TMP_Text)soulsText).text = $"{ApplicationManager.Application.DamnedSouls}";
	}

	public void SetNightSliderValue(float value)
	{
		nightSlider.SetValueWithTween(Mathf.Clamp(value, 0f, 1f), nightSliderSequence, nightSliderTweenDuration);
	}

	public void SetNextRefreshNightSliderValue(float value)
	{
		nextRefreshNightSliderValue = value;
	}

	private void OnGoldChange(int goldValue)
	{
		RefreshGold(goldValue);
	}

	private void OnMaterialsChange(int materialsValue)
	{
		RefreshMaterials(materialsValue);
	}

	private void OnWorkersChange(int workersValue, int maxWorkersValue)
	{
		RefreshWorkers(workersValue, maxWorkersValue);
	}

	private void RefreshGold(int goldValue)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Day)
		{
			goldIconImage.sprite = goldIconSpriteOn;
			goldDisplay.SetColor(TPSingleton<ResourceManager>.Instance.GetResourceColor("Gold"));
		}
		else
		{
			goldIconImage.sprite = goldIconSpriteOff;
			goldDisplay.SetColor(goldTextDisabledColor);
		}
		goldDisplay.RefreshValue(goldValue, () => ObjectPooler.GetPooledComponent<GainGoldDisplay>("TurnPanelGainGoldDisplay", goldDisplayPrefab, (Transform)null, dontSetParent: false));
	}

	private void RefreshMaterials(int materialsValue)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Day)
		{
			materialsIconImage.sprite = materialsIconSpriteOn;
			materialsDisplay.SetColor(TPSingleton<ResourceManager>.Instance.GetResourceColor("Materials"));
		}
		else
		{
			materialsIconImage.sprite = materialsIconSpriteOff;
			materialsDisplay.SetColor(materialsTextDisabledColor);
		}
		materialsDisplay.RefreshValue(materialsValue, () => ObjectPooler.GetPooledComponent<GainMaterialDisplay>("TurnPanelGainMaterialsDisplay", materialsDisplayPrefab, (Transform)null, dontSetParent: false));
	}

	private void RefreshWorkers(int workersValue, int maxWorkersValue)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (TPSingleton<GameManager>.Instance.Game.Cycle == Game.E_Cycle.Day)
		{
			workersIconImage.sprite = workersIconSpriteOn;
			((Graphic)workersText).color = TPSingleton<ResourceManager>.Instance.GetResourceColor("Workers");
		}
		else
		{
			workersIconImage.sprite = workersIconSpriteOff;
			((Graphic)workersText).color = workersTextDisabledColor;
		}
		((TMP_Text)workersText).text = $"{workersValue}/{maxWorkersValue}";
	}

	private void Start()
	{
		TPSingleton<ResourceManager>.Instance.OnGoldChange += OnGoldChange;
		TPSingleton<ResourceManager>.Instance.OnMaterialsChange += OnMaterialsChange;
		TPSingleton<ResourceManager>.Instance.OnWorkersChange += OnWorkersChange;
	}

	private void OnDestroy()
	{
		if (TPSingleton<ResourceManager>.Exist())
		{
			TPSingleton<ResourceManager>.Instance.OnGoldChange -= OnGoldChange;
			TPSingleton<ResourceManager>.Instance.OnMaterialsChange -= OnMaterialsChange;
			TPSingleton<ResourceManager>.Instance.OnWorkersChange -= OnWorkersChange;
		}
	}
}
