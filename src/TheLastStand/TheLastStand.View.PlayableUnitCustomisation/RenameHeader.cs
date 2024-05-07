using System.Collections.Generic;
using PortraitAPI;
using TMPro;
using TPLib;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Model.Unit;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.PlayableUnitCustomisation;

public class RenameHeader : RandomizableCustomizationElement
{
	[SerializeField]
	private TextMeshProUGUI heroNameText;

	[SerializeField]
	private BetterButton renameButton;

	[SerializeField]
	private RenamePopup renamePopup;

	[HideInInspector]
	public bool IsEditingName;

	private PlayableUnit playableUnit;

	public string CurrentName { get; private set; } = string.Empty;


	public RenamePopup RenamePopup => renamePopup;

	public override void RandomizeValue(bool useWeight)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected I4, but got Unknown
		List<string> list = new List<string>();
		E_Gender currentGender = TPSingleton<PlayableUnitCustomisationPanel>.Instance.CurrentGender;
		switch ((int)currentGender)
		{
		case 0:
			list.AddRange(playableUnit.RaceDefinition.GetNamesForGender("Male"));
			break;
		case 1:
			list.AddRange(playableUnit.RaceDefinition.GetNamesForGender("Female"));
			break;
		case 2:
			list.AddRange(playableUnit.RaceDefinition.GetNamesForGender("Male"));
			list.AddRange(playableUnit.RaceDefinition.GetNamesForGender("Female"));
			break;
		}
		CurrentName = RandomManager.GetRandomElement(TPSingleton<PlayableUnitCustomisationPanel>.Instance, list);
		((TMP_Text)heroNameText).text = CurrentName;
	}

	public void Refresh(PlayableUnit playableUnit)
	{
		this.playableUnit = playableUnit;
		CurrentName = playableUnit.PlayableUnitName;
		((TMP_Text)heroNameText).text = CurrentName;
	}

	public void Refresh(string name)
	{
		CurrentName = name;
		((TMP_Text)heroNameText).text = CurrentName;
	}

	private void OnRenameButtonClicked()
	{
		renamePopup.PlayableUnitName = ((CurrentName != string.Empty) ? CurrentName : playableUnit.PlayableUnitName);
		IsEditingName = true;
		renamePopup.Open();
	}

	private void Start()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		((UnityEvent)((Button)renameButton).onClick).AddListener(new UnityAction(OnRenameButtonClicked));
	}

	private void OnDestroy()
	{
		((UnityEventBase)((Button)renameButton).onClick).RemoveAllListeners();
	}
}
