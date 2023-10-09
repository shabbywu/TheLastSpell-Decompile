using System;
using System.Collections;
using TPLib;
using TPLib.Yield;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace TheLastStand.Framework.Animation;

[RequireComponent(typeof(Animator))]
public class SingleAnimPlayer : MonoBehaviour
{
	[SerializeField]
	private AnimationClip clip;

	[SerializeField]
	private float delay;

	[SerializeField]
	private bool playOnStart;

	[SerializeField]
	private bool disableGoOnFinish;

	[SerializeField]
	private bool destroyGoOnFinish;

	[SerializeField]
	private bool verbose;

	private AnimationClipPlayable playableClip;

	private PlayableGraph playableGraph;

	private AnimationPlayableOutput playableOutput;

	private bool inited;

	private Coroutine playCoroutine;

	public AnimationClip Clip
	{
		get
		{
			return clip;
		}
		set
		{
			if (!((Object)(object)clip == (Object)(object)value) && !IsPlaying)
			{
				clip = value;
				InitClip();
			}
		}
	}

	public bool DestroyGoOnFinish
	{
		get
		{
			return destroyGoOnFinish;
		}
		set
		{
			destroyGoOnFinish = value;
		}
	}

	public bool DisableGoOnFinish
	{
		get
		{
			return disableGoOnFinish;
		}
		set
		{
			disableGoOnFinish = value;
		}
	}

	public bool IsPlaying => playCoroutine != null;

	[ContextMenu("Cleanup")]
	public void Cleanup()
	{
		if (((PlayableGraph)(ref playableGraph)).IsValid())
		{
			((PlayableGraph)(ref playableGraph)).Destroy();
		}
		if (playCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(playCoroutine);
			playCoroutine = null;
		}
		inited = false;
	}

	[ContextMenu("Play")]
	public Coroutine Play()
	{
		return Play(0f);
	}

	public Coroutine Play(float delay)
	{
		if (playCoroutine == null)
		{
			playCoroutine = ((MonoBehaviour)this).StartCoroutine(PlayCoroutine(delay));
		}
		return playCoroutine;
	}

	public IEnumerator WaitForCompletion()
	{
		yield return (object)new WaitWhile((Func<bool>)(() => IsPlaying));
	}

	private void Init()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		if (!inited)
		{
			playableGraph = PlayableGraph.Create(((Object)this).name + "__" + ((Object)clip).name);
			((PlayableGraph)(ref playableGraph)).SetTimeUpdateMode((DirectorUpdateMode)1);
			playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", ((Component)this).GetComponent<Animator>());
			InitClip();
			GraphVisualizerClient.Show(playableGraph);
			inited = true;
		}
	}

	private void InitClip()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (((PlayableGraph)(ref playableGraph)).IsValid() && !((Object)(object)clip == (Object)null))
		{
			if (PlayableExtensions.IsValid<AnimationClipPlayable>(playableClip))
			{
				PlayableExtensions.Destroy<AnimationClipPlayable>(playableClip);
			}
			playableClip = AnimationClipPlayable.Create(playableGraph, clip);
			PlayableOutputExtensions.SetSourcePlayable<AnimationPlayableOutput, AnimationClipPlayable>(playableOutput, playableClip);
		}
	}

	private bool IsClipDone()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		AnimationClip animationClip = ((AnimationClipPlayable)(ref playableClip)).GetAnimationClip();
		if (!((Motion)animationClip).isLooping)
		{
			return PlayableExtensions.GetTime<AnimationClipPlayable>(playableClip) > (double)animationClip.length;
		}
		return false;
	}

	private void OnDestroy()
	{
		Cleanup();
	}

	private IEnumerator PlayCoroutine(float delay)
	{
		if (!inited)
		{
			Init();
		}
		if (delay <= 0f)
		{
			delay = this.delay;
		}
		if (verbose)
		{
			TPDebug.Log((object)$"[{Time.time}] Starting FX with clip '{((Object)clip).name}'.", (Object)(object)this);
		}
		if (delay > 0f)
		{
			yield return SharedYields.WaitForSeconds(delay);
		}
		if (verbose)
		{
			TPDebug.Log((object)$"[{Time.time}] Playing clip '{((Object)clip).name}'.", (Object)(object)this);
		}
		((PlayableGraph)(ref playableGraph)).Play();
		yield return (object)new WaitUntil((Func<bool>)IsClipDone);
		if (verbose)
		{
			TPDebug.Log((object)$"[{Time.time}] Clip '{((Object)clip).name}' finished.", (Object)(object)this);
		}
		((PlayableGraph)(ref playableGraph)).Stop();
		if (disableGoOnFinish)
		{
			((Component)this).gameObject.SetActive(false);
		}
		if (destroyGoOnFinish)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
		playCoroutine = null;
	}

	private void Start()
	{
		if (playOnStart)
		{
			Play();
		}
	}
}
