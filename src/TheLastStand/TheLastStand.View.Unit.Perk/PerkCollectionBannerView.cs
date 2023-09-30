using UnityEngine;
using UnityEngine.UI;

namespace TheLastStand.View.Unit.Perk;

public class PerkCollectionBannerView : MonoBehaviour
{
	[SerializeField]
	private Image crest;

	[SerializeField]
	private Image topBackground;

	public void Refresh(string collectionId)
	{
		Sprite collectionAssetOrDefault = UnitPerkTreeView.GetCollectionAssetOrDefault<Sprite>("View/Sprites/UI/CharacterSheet/PerkTree/{0}/Collection_{0}_Top", collectionId);
		topBackground.sprite = collectionAssetOrDefault;
		collectionAssetOrDefault = UnitPerkTreeView.GetCollectionAssetOrDefault<Sprite>("View/Sprites/UI/CharacterSheet/PerkTree/{0}/Crest_{0}_Off", collectionId);
		crest.sprite = collectionAssetOrDefault;
	}
}
