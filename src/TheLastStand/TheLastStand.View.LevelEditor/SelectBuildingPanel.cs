using System.Collections.Generic;
using System.Linq;
using TheLastStand.Database.Building;
using TheLastStand.Manager.LevelEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.LevelEditor;

public class SelectBuildingPanel : MonoBehaviour
{
	[SerializeField]
	private Transform content;

	[SerializeField]
	private Scrollbar scrollbar;

	private void OnBackButtonClick()
	{
		LevelEditorManager.SetState(LevelEditorManager.E_State.Default);
	}

	private void OnBuildingButtonClick(string buildingDefinitionId)
	{
		LevelEditorManager.SelectBuilding(buildingDefinitionId);
	}

	private void Awake()
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected O, but got Unknown
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Expected O, but got Unknown
		List<string> list = BuildingDatabase.BuildingDefinitions.Keys.ToList();
		list.Sort();
		foreach (string buildingDefinitionId in list)
		{
			Object.Instantiate<LevelEditorButton>(LevelEditorManager.LevelEditorButtonPrefab, content).Init(buildingDefinitionId, (UnityAction)delegate
			{
				OnBuildingButtonClick(buildingDefinitionId);
			});
		}
		Object.Instantiate<LevelEditorButton>(LevelEditorManager.LevelEditorButtonPrefab, content).Init("BACK (Esc)", new UnityAction(OnBackButtonClick));
		Transform transform = ((Component)this).transform;
		LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)(object)((transform is RectTransform) ? transform : null));
	}

	private void OnEnable()
	{
		scrollbar.value = 0f;
	}
}
