using System.Collections;
using TPLib;
using TPLib.Log;
using TPLib.Yield;
using TheLastStand.Manager.Building;
using TheLastStand.Manager.Unit;
using TheLastStand.View.CharacterSheet;
using TheLastStand.View.HUD;
using TheLastStand.View.HUD.BottomScreenPanel;
using UnityEngine;

namespace TheLastStand.View;

public class GameView : TPSingleton<GameView>
{
	[SerializeField]
	private GameObject mainCanvasGameObject;

	[SerializeField]
	private TopScreenPanel topScreenPanel;

	[SerializeField]
	private BottomScreenPanel bottomScreenPanel;

	[SerializeField]
	private GameAccelerationPanel gameAccelerationPanel;

	[SerializeField]
	private CharacterDetailsView characterDetailsView;

	[SerializeField]
	private Canvas mainHUDCanvas;

	[SerializeField]
	private DataColor positiveColor;

	[SerializeField]
	private DataColor negativeColor;

	public static CharacterDetailsView CharacterDetailsView
	{
		get
		{
			if ((Object)(object)TPSingleton<GameView>.Instance.characterDetailsView == (Object)null)
			{
				((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).LogWarning((object)("characterDetailsView is missing in " + ((object)TPSingleton<GameView>.Instance).GetType().Name + ", getting it with FindObjectOfType!"), (CLogLevel)1, true, false);
				TPSingleton<GameView>.Instance.characterDetailsView = Object.FindObjectOfType<CharacterDetailsView>();
			}
			return TPSingleton<GameView>.Instance.characterDetailsView;
		}
	}

	public static BottomScreenPanel BottomScreenPanel
	{
		get
		{
			if ((Object)(object)TPSingleton<GameView>.Instance.bottomScreenPanel == (Object)null)
			{
				((CLogger<BuildingManager>)TPSingleton<BuildingManager>.Instance).LogWarning((object)("bottomScreenPanel is missing in " + ((object)TPSingleton<GameView>.Instance).GetType().Name + ", getting it with FindObjectOfType!"), (CLogLevel)1, true, false);
				TPSingleton<GameView>.Instance.bottomScreenPanel = Object.FindObjectOfType<BottomScreenPanel>();
			}
			return TPSingleton<GameView>.Instance.bottomScreenPanel;
		}
	}

	public static GameAccelerationPanel GameAccelerationPanel => TPSingleton<GameView>.Instance.gameAccelerationPanel;

	public static Color NegativeColor => TPSingleton<GameView>.Instance.negativeColor._Color;

	public static Color PositiveColor => TPSingleton<GameView>.Instance.positiveColor._Color;

	public static TopScreenPanel TopScreenPanel => TPSingleton<GameView>.Instance.topScreenPanel;

	public GameObject MainCanvasGameObject => mainCanvasGameObject;

	public void DisplayHud()
	{
		((MonoBehaviour)this).StartCoroutine(DisplayHudCoroutine());
	}

	public void HideHud()
	{
		((Behaviour)mainHUDCanvas).enabled = false;
		((Behaviour)topScreenPanel.TurnPanel.TurnPanelCanvas).enabled = false;
		((Behaviour)topScreenPanel.UnitPortraitsPanel.PortraitsCanvas).enabled = false;
	}

	protected override void Awake()
	{
		base.Awake();
		TPEnableGOsOnGameStart component = mainCanvasGameObject.GetComponent<TPEnableGOsOnGameStart>();
		if (component != null)
		{
			component.EnableTargets();
		}
	}

	private IEnumerator DisplayHudCoroutine()
	{
		((Behaviour)mainHUDCanvas).enabled = true;
		Canvas obj = mainHUDCanvas;
		int sortingOrder = obj.sortingOrder + 1;
		obj.sortingOrder = sortingOrder;
		yield return SharedYields.WaitForEndOfFrame;
		Canvas obj2 = mainHUDCanvas;
		sortingOrder = obj2.sortingOrder - 1;
		obj2.sortingOrder = sortingOrder;
		((Behaviour)topScreenPanel.TurnPanel.TurnPanelCanvas).enabled = true;
		((Behaviour)topScreenPanel.UnitPortraitsPanel.PortraitsCanvas).enabled = true;
	}
}
