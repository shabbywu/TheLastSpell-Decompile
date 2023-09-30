using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TPLib;
using TPLib.Debugging.Console;
using TPLib.Log;
using TheLastStand.Database;
using TheLastStand.Manager.WorldMap;
using TheLastStand.View.Cutscene;
using UnityEngine;

namespace TheLastStand.Manager;

public class CutsceneManager : Manager<CutsceneManager>
{
	public class StringToCutsceneIdConverter : StringToStringCollectionEntryConverter
	{
		protected override List<string> Entries => GameDatabase.CutsceneDefinitions.Keys.ToList();
	}

	[SerializeField]
	private VictorySequenceView victorySequenceView;

	[SerializeField]
	private PillarsCutsceneView pillarsCutsceneView;

	[SerializeField]
	private TutorialSequenceView tutorialSequenceView;

	[SerializeField]
	private GenericCutsceneView genericCutsceneViewPrefab;

	private readonly List<GenericCutsceneView> genericCutsceneViews = new List<GenericCutsceneView>();

	public static bool AnyCutscenePlaying
	{
		get
		{
			if (!TPSingleton<CutsceneManager>.Instance.VictorySequenceView.IsPlaying && !TPSingleton<CutsceneManager>.Instance.TutorialSequenceView.IsPlaying && !TPSingleton<CutsceneManager>.Instance.PillarsCutsceneView.IsPlaying)
			{
				return TPSingleton<CutsceneManager>.Instance.genericCutsceneViews.Any((GenericCutsceneView x) => x.IsPlaying);
			}
			return true;
		}
	}

	public VictorySequenceView VictorySequenceView => victorySequenceView;

	public PillarsCutsceneView PillarsCutsceneView => pillarsCutsceneView;

	public TutorialSequenceView TutorialSequenceView => tutorialSequenceView;

	public bool VictorySequenceSkipped { get; private set; }

	public GenericCutsceneView GetGenericCutsceneView()
	{
		GenericCutsceneView genericCutsceneView = Object.Instantiate<GenericCutsceneView>(genericCutsceneViewPrefab, ((Component)this).gameObject.transform);
		genericCutsceneViews.Add(genericCutsceneView);
		return genericCutsceneView;
	}

	public static void PlayCutscene(CutsceneView cutsceneView, Action callback = null)
	{
		((MonoBehaviour)TPSingleton<CutsceneManager>.Instance).StartCoroutine(cutsceneView.PlayCutscene(callback));
	}

	private IEnumerator SkipVictoryCutscene()
	{
		if (PillarsCutsceneView.CanBeSkipped())
		{
			yield return PillarsCutsceneView.Skip();
			yield return VictorySequenceView.Skip();
		}
		else
		{
			yield return VictorySequenceView.Skip();
		}
	}

	private void Start()
	{
		if ((Object)(object)victorySequenceView == (Object)null)
		{
			((CLogger<CutsceneManager>)TPSingleton<CutsceneManager>.Instance).LogWarning((object)"Missing reference for victorySequenceView, trying to get it using FindObjectOfType.", (CLogLevel)1, true, false);
			victorySequenceView = Object.FindObjectOfType<VictorySequenceView>();
		}
		if ((Object)(object)pillarsCutsceneView == (Object)null)
		{
			((CLogger<CutsceneManager>)TPSingleton<CutsceneManager>.Instance).LogWarning((object)"Missing reference for pillarsCutsceneView, trying to get it using FindObjectOfType.", (CLogLevel)1, true, false);
			pillarsCutsceneView = Object.FindObjectOfType<PillarsCutsceneView>();
		}
		if ((Object)(object)tutorialSequenceView == (Object)null)
		{
			((CLogger<CutsceneManager>)TPSingleton<CutsceneManager>.Instance).LogWarning((object)"Missing reference for tutorialSequenceView, trying to get it using FindObjectOfType.", (CLogLevel)1, true, false);
			tutorialSequenceView = Object.FindObjectOfType<TutorialSequenceView>();
		}
	}

	private void Update()
	{
		if (InputManager.GetButtonDown(23) && VictorySequenceView.CanBeSkipped())
		{
			VictorySequenceSkipped = true;
			((MonoBehaviour)TPSingleton<CutsceneManager>.Instance).StopAllCoroutines();
			((MonoBehaviour)this).StartCoroutine(SkipVictoryCutscene());
		}
	}

	public void UnregisterGenericCutscene(GenericCutsceneView cutsceneView)
	{
		genericCutsceneViews.Remove(cutsceneView);
		Object.Destroy((Object)(object)((Component)cutsceneView).gameObject);
	}

	[DevConsoleCommand(/*Could not decode attribute arguments.*/)]
	private static void DebugPlayCutscene([StringConverter(typeof(StringToCutsceneIdConverter))] string cutsceneId = "")
	{
		GenericCutsceneView genericCutsceneView = TPSingleton<CutsceneManager>.Instance.GetGenericCutsceneView();
		genericCutsceneView.Init(cutsceneId, new CutsceneData(TPSingleton<WorldMapCityManager>.Instance.SelectedCity));
		PlayCutscene(genericCutsceneView);
	}
}
