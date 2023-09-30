using System;
using System.Collections;
using TMPro;
using TPLib.Yield;
using UnityEngine;

namespace TheLastStand.View.Generic;

public class TextReveal : MonoBehaviour
{
	private enum E_RevealType
	{
		Letter,
		Word,
		Line
	}

	[SerializeField]
	private E_RevealType revealType;

	[SerializeField]
	private float delayBetweenCharacters = 0.05f;

	[SerializeField]
	private bool prepareOnAwake;

	[SerializeField]
	private bool playOnStart;

	[SerializeField]
	private TextMeshProUGUI text;

	private Coroutine revealCoroutine;

	private int totalVisibleItems;

	private int visibleCount = -1;

	public bool IsPrepared => visibleCount == 0;

	[ContextMenu("Finish")]
	public void Finish()
	{
		if (revealCoroutine != null)
		{
			((MonoBehaviour)this).StopCoroutine(revealCoroutine);
			revealCoroutine = null;
		}
		((TMP_Text)text).maxVisibleCharacters = 99999;
		((TMP_Text)text).maxVisibleWords = 99999;
		((TMP_Text)text).maxVisibleLines = 99999;
	}

	[ContextMenu("Play")]
	public void Play()
	{
		if (revealCoroutine == null)
		{
			revealCoroutine = ((MonoBehaviour)this).StartCoroutine(Reveal());
		}
	}

	[ContextMenu("Prepare")]
	public void Prepare()
	{
		Prepare(forceMeshUpdate: true);
	}

	public void Prepare(bool forceMeshUpdate)
	{
		if (forceMeshUpdate)
		{
			((TMP_Text)text).ForceMeshUpdate(false, false);
		}
		((TMP_Text)text).maxVisibleCharacters = 99999;
		((TMP_Text)text).maxVisibleWords = 99999;
		((TMP_Text)text).maxVisibleLines = 99999;
		visibleCount = 0;
		switch (revealType)
		{
		case E_RevealType.Letter:
			totalVisibleItems = ((TMP_Text)text).textInfo.characterCount;
			break;
		case E_RevealType.Word:
			totalVisibleItems = ((TMP_Text)text).textInfo.wordCount;
			break;
		case E_RevealType.Line:
			totalVisibleItems = ((TMP_Text)text).textInfo.lineCount;
			break;
		}
		RefreshVisibleItems();
	}

	private void Awake()
	{
		if ((Object)(object)text == (Object)null)
		{
			text = ((Component)this).GetComponent<TextMeshProUGUI>();
		}
		if (prepareOnAwake)
		{
			Prepare();
		}
	}

	private void OnEnable()
	{
		TMPro_EventManager.TEXT_CHANGED_EVENT.Add((Action<Object>)OnTextChanged);
	}

	private void OnDisable()
	{
		TMPro_EventManager.TEXT_CHANGED_EVENT.Remove((Action<Object>)OnTextChanged);
	}

	private void OnTextChanged(Object obj)
	{
		if (obj == (Object)(object)text && IsPrepared)
		{
			Prepare(forceMeshUpdate: false);
		}
	}

	private void RefreshVisibleItems()
	{
		switch (revealType)
		{
		case E_RevealType.Letter:
			((TMP_Text)text).maxVisibleCharacters = visibleCount;
			break;
		case E_RevealType.Word:
			((TMP_Text)text).maxVisibleWords = visibleCount;
			break;
		case E_RevealType.Line:
			((TMP_Text)text).maxVisibleLines = visibleCount;
			break;
		}
	}

	private IEnumerator Reveal()
	{
		if (!IsPrepared)
		{
			Prepare();
		}
		int counter = 0;
		while (visibleCount < totalVisibleItems)
		{
			visibleCount = counter++ % (totalVisibleItems + 1);
			RefreshVisibleItems();
			yield return SharedYields.WaitForSeconds(delayBetweenCharacters);
		}
		Finish();
	}

	private void Start()
	{
		if (playOnStart)
		{
			Play();
		}
	}
}
