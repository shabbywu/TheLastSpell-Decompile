using System.Collections;
using TPLib.Yield;
using TheLastStand.Manager.Sound;
using TheLastStand.Model.TileMap;
using TheLastStand.View.TileMap;
using UnityEngine;

namespace TheLastStand.View.Sound;

public class OneShotSound : MonoBehaviour
{
	[SerializeField]
	private AudioSource audioSource;

	public void Play(AudioClip clip, float delay = 0f)
	{
		SoundManager.PlayAudioClip(audioSource, clip, delay, doNotInterrupt: true);
		((MonoBehaviour)this).StartCoroutine(Hide(delay + clip.length));
	}

	public void PlaySpatialized(AudioClip clip, Vector3 position, float delay = 0f)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (audioSource.spatialBlend == 0f)
		{
			Debug.LogWarning((object)"Calling the PlaySpatialized method on an AudioSource with no 3D blending.");
		}
		((Component)this).transform.position = position;
		SoundManager.PlayAudioClip(audioSource, clip, delay, doNotInterrupt: true);
		((MonoBehaviour)this).StartCoroutine(Hide(delay + clip.length));
	}

	public void PlaySpatialized(AudioClip clip, Tile tile, float delay = 0f)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (tile == null)
		{
			Play(clip, delay);
		}
		else
		{
			PlaySpatialized(clip, Vector2.op_Implicit(TileMapView.GetTileCenter(tile)), delay);
		}
	}

	private IEnumerator Hide(float delay)
	{
		yield return SharedYields.WaitForSeconds(delay);
		((Component)this).gameObject.SetActive(false);
	}
}
