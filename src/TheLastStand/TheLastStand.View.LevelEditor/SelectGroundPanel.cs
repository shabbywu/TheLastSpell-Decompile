using System.Collections.Generic;
using System.Linq;
using TheLastStand.Database;
using TheLastStand.Framework.Extensions;
using TheLastStand.Manager.LevelEditor;
using UnityEngine;
using UnityEngine.Events;

namespace TheLastStand.View.LevelEditor;

public class SelectGroundPanel : MonoBehaviour
{
	private void OnBackButtonClick()
	{
		LevelEditorManager.SetState(LevelEditorManager.E_State.Default);
	}

	private void OnGroundButtonClick(string groundDefinitionId)
	{
		LevelEditorManager.SelectGround(groundDefinitionId);
	}

	private void Awake()
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Expected O, but got Unknown
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Expected O, but got Unknown
		TransformExtensions.DestroyChildren(((Component)this).transform);
		List<string> list = TileDatabase.GroundDefinitions.Keys.ToList();
		list.Sort();
		foreach (string groundId in list)
		{
			Object.Instantiate<LevelEditorButton>(LevelEditorManager.LevelEditorButtonPrefab, ((Component)this).gameObject.transform).Init(groundId, (UnityAction)delegate
			{
				OnGroundButtonClick(groundId);
			});
		}
		Object.Instantiate<LevelEditorButton>(LevelEditorManager.LevelEditorButtonPrefab, ((Component)this).gameObject.transform).Init("BACK (Esc)", new UnityAction(OnBackButtonClick));
	}
}
