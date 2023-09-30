using System.Collections;
using Sirenix.OdinInspector;
using TPLib;
using TPLib.Yield;
using TheLastStand.Manager;
using TheLastStand.Manager.Unit;
using TheLastStand.Model;
using TheLastStand.Model.TileMap;
using TheLastStand.View.TileMap;
using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Unit;

public class SpawnWaveViewPreviewFeedback : SerializedMonoBehaviour
{
	[SerializeField]
	private Vector2Int offsetFromFogLine = Vector2Int.zero;

	[SerializeField]
	private GameObject arrowDisplay;

	[SerializeField]
	private Transform dangerIndicatorTransform;

	[SerializeField]
	private Transform camTarget;

	private Animator arrowDisplayAnimator;

	private Image arrowDisplayImage;

	private Animator dangerIndicatorAnimator;

	private Image dangerIndicatorImage;

	public Transform CamTarget => camTarget;

	public Animator ArrowDisplayAnimator
	{
		get
		{
			if (arrowDisplayAnimator == null)
			{
				arrowDisplayAnimator = arrowDisplay.GetComponent<Animator>();
			}
			return arrowDisplayAnimator;
		}
	}

	public Image ArrowDisplayImage
	{
		get
		{
			if (arrowDisplayImage == null)
			{
				arrowDisplayImage = arrowDisplay.GetComponent<Image>();
			}
			return arrowDisplayImage;
		}
	}

	public Animator DangerIndicatorAnimator
	{
		get
		{
			if (dangerIndicatorAnimator == null)
			{
				dangerIndicatorAnimator = ((Component)dangerIndicatorTransform).GetComponent<Animator>();
			}
			return dangerIndicatorAnimator;
		}
	}

	public Image DangerIndicatorImage
	{
		get
		{
			if (dangerIndicatorImage == null)
			{
				dangerIndicatorImage = ((Component)dangerIndicatorTransform).GetComponent<Image>();
			}
			return dangerIndicatorImage;
		}
	}

	public void Clear()
	{
		((Component)this).gameObject.SetActive(false);
		if ((Object)(object)dangerIndicatorTransform != (Object)null)
		{
			((Behaviour)DangerIndicatorImage).enabled = false;
		}
		((Behaviour)ArrowDisplayImage).enabled = false;
	}

	public void RefreshPosition(Tile tile)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).gameObject.SetActive(true);
		((Component)this).transform.position = TileMapView.GetWorldPosition(tile.Position + offsetFromFogLine);
	}

	public void Refresh(SpawnWaveView.SpawnWaveViewRefreshInfo spawnWaveViewRefreshInfo, bool forceDisplay = false)
	{
		Clear();
		if (forceDisplay || (TPSingleton<GameManager>.Instance.Game.Cycle != Game.E_Cycle.Night && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.NightReport && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.GameOver && TPSingleton<GameManager>.Instance.Game.State != Game.E_State.CutscenePlaying))
		{
			((Component)this).gameObject.SetActive(true);
			if (spawnWaveViewRefreshInfo.Tile != null)
			{
				RefreshPosition(spawnWaveViewRefreshInfo.Tile);
			}
			if (spawnWaveViewRefreshInfo.LocalProportionPercentage > 0 && (Object)(object)arrowDisplay != (Object)null)
			{
				((MonoBehaviour)this).StartCoroutine(UpdateArrowAnimationCoroutine(spawnWaveViewRefreshInfo));
			}
			if ((Object)(object)dangerIndicatorTransform != (Object)null && spawnWaveViewRefreshInfo.DirectionDangerLevel > 0)
			{
				DangerIndicatorAnimator.SetInteger("DangerLevel", spawnWaveViewRefreshInfo.DirectionDangerLevel);
				((MonoBehaviour)this).StartCoroutine(DisplayDangerLevelCoroutine());
			}
		}
	}

	private IEnumerator DisplayDangerLevelCoroutine()
	{
		yield return SharedYields.WaitForEndOfFrame;
		((Behaviour)DangerIndicatorImage).enabled = true;
	}

	private IEnumerator UpdateArrowAnimationCoroutine(SpawnWaveView.SpawnWaveViewRefreshInfo spawnWaveViewRefreshInfo)
	{
		if (SpawnWaveManager.SpawnWaveView.GetArrowAssetId(spawnWaveViewRefreshInfo.Zone, spawnWaveViewRefreshInfo.LocalProportionPercentage, out var animatorController))
		{
			ArrowDisplayAnimator.runtimeAnimatorController = animatorController;
			yield return SharedYields.WaitForEndOfFrame;
			((Behaviour)ArrowDisplayImage).enabled = true;
			((Graphic)arrowDisplayImage).SetNativeSize();
		}
	}
}
