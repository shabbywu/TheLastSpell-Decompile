using System;
using System.Linq;
using TMPro;
using TPLib;
using TPLib.Localization;
using TheLastStand.Definition.Unit.Enemy;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Extensions;
using TheLastStand.Model.Unit.Enemy;
using TheLastStand.View.Unit;
using UnityEngine;

namespace TheLastStand.View.ToDoList;

public class ToDoListWavesNotificationView : ToDoListNotificationView
{
	public override void Refresh()
	{
		base.Refresh();
		if (base.OnlyShowDuringDayTurn != 0 && TPSingleton<GameManager>.Instance.Game.DayTurn != base.OnlyShowDuringDayTurn)
		{
			return;
		}
		for (int num = toDoTexts.Count - 1; num >= 0; num--)
		{
			Object.Destroy((Object)(object)((Component)toDoTexts[num]).gameObject);
		}
		toDoTexts.Clear();
		SpawnWave currentSpawnWave = SpawnWaveManager.CurrentSpawnWave;
		foreach (SpawnDirectionsDefinition.E_Direction eDirection in SpawnDirectionsDefinition.OrderedDirections)
		{
			SpawnWaveView.SpawnWaveArrowPair spawnWaveArrowPair = SpawnWaveManager.SpawnWaveView.SpawnWavePreviewFeedbacks.FirstOrDefault((SpawnWaveView.SpawnWaveArrowPair x) => x.SpawnWaveDetailedZone.IsCentralZone() && x.CentralSpawnDirection == eDirection);
			if (currentSpawnWave.RotatedProportionPerDirection.ContainsKey(spawnWaveArrowPair.CentralSpawnDirection))
			{
				TextMeshProUGUI val = Object.Instantiate<TextMeshProUGUI>(toDoTextPrefab, (Transform)(object)notificationGroupRectTransform);
				((TMP_Text)val).text = "- " + SpawnWave.GetLocalizedDirectionName(spawnWaveArrowPair.CentralSpawnDirection);
				toDoTexts.Add(val);
			}
		}
		CheckDisplay();
	}

	private void Awake()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Combine((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
	}

	private void OnDestroy()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		Localizer.onLocalize = (OnLocalizeNotification)Delegate.Remove((Delegate?)(object)Localizer.onLocalize, (Delegate?)new OnLocalizeNotification(OnLocalize));
	}

	private void OnLocalize()
	{
		Refresh();
	}
}
