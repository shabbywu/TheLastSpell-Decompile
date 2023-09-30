using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using PortraitAPI;
using PortraitAPI.Misc;
using TPLib;
using TPLib.Log;
using TPLib.Yield;
using TheLastStand.Controller.Unit;
using TheLastStand.Database.Unit;
using TheLastStand.Definition;
using TheLastStand.Definition.Item;
using TheLastStand.Definition.Unit;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.Manager.SDK;
using TheLastStand.Manager.Unit;
using TheLastStand.Model.Unit;
using TheLastStand.Serialization.Unit;
using TheLastStand.View.Camera;
using TheLastStand.View.HUD.UnitPortraitPanel;
using TheLastStand.View.Sound;
using TheLastStand.View.Unit.UI;
using UnityEngine;
using UnityEngine.Rendering;

namespace TheLastStand.View.Unit;

public class PlayableUnitView : UnitView, ISnapshotable
{
	[SerializeField]
	private SortingGroup frontSortingGroup;

	[SerializeField]
	private Material colorSwapMaterial;

	[SerializeField]
	private Material colorSwapPortraitMaterial;

	[SerializeField]
	private GameObject skillTargetingFeedbackParent;

	[SerializeField]
	private SpriteRenderer skillTargetingGroundRenderer;

	[SerializeField]
	private SpriteRenderer skillTargetingHaloRenderer;

	[SerializeField]
	private Color skillTargetingHoverColor = Color.green;

	[SerializeField]
	private Transform snapshotTarget;

	[SerializeField]
	private int deathSortingOrder = 10;

	[SerializeField]
	private ParticleSystem prepareDeathParticles;

	[SerializeField]
	private ParticleSystem deathParticles;

	[SerializeField]
	private GameObject frontShadow;

	[SerializeField]
	private GameObject executeCross;

	[SerializeField]
	private GameObject bloodPool;

	[SerializeField]
	private float delayBeforeBark = 1f;

	[SerializeField]
	private float delayAfterBark = 1f;

	[SerializeField]
	private float delayBeforeDeathSequence = 0.3f;

	[SerializeField]
	private float delayAfterDeathSequence = 0.5f;

	[SerializeField]
	private float cameraFocusHeightOffset = 1f;

	[SerializeField]
	private AudioSource deathStartAudioSource;

	[SerializeField]
	private AudioSource deathImpactAudioSource;

	[SerializeField]
	private float deathShakeDuration = 0.4f;

	[SerializeField]
	private Vector2 deathShakeStrength = new Vector2(0.3f, 0.3f);

	protected BodyPartView[] bodyPartViews;

	private Texture2D colorSwapTex;

	private bool snapshotActiveBackup;

	private Dictionary<GameObject, int> snapshotLayersBackup;

	private GameDefinition.E_Direction snapshotLookDirectionBackup = GameDefinition.E_Direction.None;

	private WaitUntil waitUntilAnimatorStateIsPrepareDie;

	public static Dictionary<Sprite, int> UsedUnitPortrait = new Dictionary<Sprite, int>();

	public static Dictionary<string, List<string>> FaceIdAvailablePortraitIds = new Dictionary<string, List<string>>();

	public static Dictionary<DataColor, int> UsedUnitPortraitColors { get; private set; } = new Dictionary<DataColor, int>();


	public Material ColorSwapPortraitMaterial => colorSwapPortraitMaterial;

	public bool DeathSequenceOver { get; private set; }

	public override float MoveSpeed => GameManager.MoveSpeedMultiplier * PlayableUnitDatabase.UnitMoveSpeed;

	public PlayableUnit PlayableUnit { get; private set; }

	public PlayableUnitHUD PlayableUnitHUD => base.UnitHUD as PlayableUnitHUD;

	public UnitPortraitPanel PortraitPanel { get; private set; }

	public override TheLastStand.Model.Unit.Unit Unit
	{
		get
		{
			return base.Unit;
		}
		set
		{
			base.Unit = value;
			if (PlayableUnit != null)
			{
				InitBodyPartViews();
				PlayableUnit.RegisterBodyPartViews(bodyPartViews, register: false);
			}
			PlayableUnit = value as PlayableUnit;
			if (PlayableUnit != null)
			{
				InitBodyPartViews();
				PlayableUnit.RegisterBodyPartViews(bodyPartViews, register: true);
			}
		}
	}

	public Transform SnapshotPosition => snapshotTarget;

	public static void AddUsedPortrait(Sprite portraitUsed)
	{
		if (!UsedUnitPortrait.ContainsKey(portraitUsed))
		{
			UsedUnitPortrait.Add(portraitUsed, 1);
		}
		else
		{
			UsedUnitPortrait[portraitUsed]++;
		}
	}

	public static void GeneratePortrait(PlayableUnit playableUnit, SerializedPlayableUnit playableUnitElement)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
		PortraitGenerationResult val;
		CodeData val2 = default(CodeData);
		if (string.IsNullOrEmpty(playableUnitElement.Portrait.Code))
		{
			val = PortraitAPIManager.GeneratePortrait(playableUnit.Gender, playableUnit.FaceId, RandomManager.GetRandomForCaller(((object)TPSingleton<PlayableUnitManager>.Instance).GetType().Name));
			playableUnit.PlayableUnitController.RandomizeColors(ref val.Code);
		}
		else if (!CodeGenerator.TryDecode(playableUnitElement.Portrait.Code, ref val2))
		{
			CLoggerManager.Log((object)("We are trying to decode an invalid portrait code ! (Code : " + playableUnitElement.Portrait.Code + ")"), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
			val = PortraitAPIManager.GeneratePortrait(playableUnit.Gender, playableUnit.FaceId, RandomManager.GetRandomForCaller(((object)TPSingleton<PlayableUnitManager>.Instance).GetType().Name));
			playableUnit.PlayableUnitController.RandomizeColors(ref val.Code);
		}
		else
		{
			val = PortraitAPIManager.GeneratePortrait(val2);
			if (playableUnitElement.Portrait.Code.Length == 12)
			{
				ColorSwapPaletteDefinition value;
				if (playableUnitElement.Portrait.SkinColorPaletteId != string.Empty)
				{
					PlayableUnitDatabase.PlayableUnitSkinColorDefinitions.TryGetValue(playableUnitElement.Portrait.SkinColorPaletteId, out value);
				}
				else
				{
					value = PlayableUnitController.RandomizeColorSwapPaletteDefinition(PlayableUnitDatabase.PlayableUnitSkinColorDefinitions, out var _);
				}
				ColorSwapPaletteDefinition value2;
				if (playableUnitElement.Portrait.HairColorPaletteId != string.Empty)
				{
					PlayableUnitDatabase.PlayableUnitHairColorDefinitions.TryGetValue(playableUnitElement.Portrait.HairColorPaletteId, out value2);
				}
				else
				{
					value2 = PlayableUnitController.GetRandomHairColorSwapPalette(value.Id, PlayableUnitDatabase.PlayableUnitHairColorDefinitions);
				}
				ColorSwapPaletteDefinition value3;
				if (playableUnitElement.Portrait.EyesColorPaletteId != string.Empty)
				{
					PlayableUnitDatabase.PlayableUnitEyesColorDefinitions.TryGetValue(playableUnitElement.Portrait.EyesColorPaletteId, out value3);
				}
				else
				{
					value3 = PlayableUnitController.RandomizeColorSwapPaletteDefinition(PlayableUnitDatabase.PlayableUnitEyesColorDefinitions, out var _);
				}
				DataColor val3 = PlayableUnitDatabase.PortraitBackgroundColors.Find((DataColor x) => x._HexCode == playableUnitElement.Portrait.BackgroundColor);
				if ((Object)(object)val3 != (Object)null)
				{
					if (!UsedUnitPortraitColors.ContainsKey(val3))
					{
						UsedUnitPortraitColors.Add(val3, 0);
					}
					UsedUnitPortraitColors[val3]++;
				}
				else
				{
					val3 = GetRandomPortraitBGColor(playableUnit);
				}
				CodeGenerator.EncodeColorDatas(new KeyValuePair<E_ColorTypes, int>[4]
				{
					new KeyValuePair<E_ColorTypes, int>((E_ColorTypes)0, IEnumerableExtension.IndexOf<string, ColorSwapPaletteDefinition>((IDictionary<string, ColorSwapPaletteDefinition>)PlayableUnitDatabase.PlayableUnitSkinColorDefinitions, value.Id)),
					new KeyValuePair<E_ColorTypes, int>((E_ColorTypes)1, IEnumerableExtension.IndexOf<string, ColorSwapPaletteDefinition>((IDictionary<string, ColorSwapPaletteDefinition>)PlayableUnitDatabase.PlayableUnitHairColorDefinitions, value2.Id)),
					new KeyValuePair<E_ColorTypes, int>((E_ColorTypes)2, IEnumerableExtension.IndexOf<string, ColorSwapPaletteDefinition>((IDictionary<string, ColorSwapPaletteDefinition>)PlayableUnitDatabase.PlayableUnitEyesColorDefinitions, value3.Id)),
					new KeyValuePair<E_ColorTypes, int>((E_ColorTypes)3, PlayableUnitDatabase.PortraitBackgroundColors.IndexOf(val3))
				}, ref val2);
				((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).Log((object)("The code used for the portrait of " + playableUnit.PlayableUnitName + " come from an older version, this code will be converted." + $"Previous code : {playableUnitElement.Portrait.Code} => New code : {val.Code}"), (CLogLevel)0, false, false);
			}
			else
			{
				string hexCode = PlayableUnitDatabase.PortraitBackgroundColors[val2.CodeColorDatas[(E_ColorTypes)3].Index]._HexCode;
				for (int num = PlayableUnitDatabase.PortraitBackgroundColors.Count - 1; num >= 0; num--)
				{
					if (PlayableUnitDatabase.PortraitBackgroundColors[num]._HexCode == hexCode)
					{
						DataColor key = PlayableUnitDatabase.PortraitBackgroundColors[num];
						if (!UsedUnitPortraitColors.ContainsKey(key))
						{
							UsedUnitPortraitColors.Add(key, 0);
						}
						UsedUnitPortraitColors[key]++;
						break;
					}
				}
			}
		}
		AddUsedPortrait(val.Front);
		playableUnit.PortraitCodeData = val.Code;
		playableUnit.PortraitSprite = val.Front;
		playableUnit.PortraitBackgroundSprite = val.Back;
	}

	public static void GenerateRandomPortrait(PlayableUnit playableUnit)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrEmpty(playableUnit.Gender) || string.IsNullOrEmpty(playableUnit.FaceId))
		{
			TPDebug.LogError((object)"Trying to GetPortraitForUnit but Gender or FaceId is not setup --> aborting", (Object)null);
			return;
		}
		PortraitGenerationResult val = PortraitAPIManager.GeneratePortrait(playableUnit.Gender, playableUnit.FaceId, RandomManager.GetRandomForCaller(((object)TPSingleton<PlayableUnitManager>.Instance).GetType().Name));
		playableUnit.PlayableUnitController.RandomizeColors(ref val.Code);
		AddUsedPortrait(val.Front);
		playableUnit.PortraitSprite = val.Front;
		playableUnit.PortraitBackgroundSprite = val.Back;
		playableUnit.PortraitCodeData = val.Code;
		((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).Log((object)$"Portrait generated with code : {val.Code} !", (CLogLevel)1, false, false);
	}

	public static Sprite GetBodyPartSprite(PlayableUnit playableUnit, string bodyPartId)
	{
		if (playableUnit == null)
		{
			TPDebug.LogError((object)"PlayableUnitView.GetBodyPartSprite() => playableUnit can't be null! Aborting...", (Object)null);
			return null;
		}
		return BodyPartView.GetSprite(playableUnit.BodyParts[bodyPartId], playableUnit.FaceId, playableUnit.Gender, BodyPartDefinition.E_Orientation.Front);
	}

	public static void RemoveUsedPortrait(Sprite portraitUsed)
	{
		UsedUnitPortrait[portraitUsed]--;
		if (UsedUnitPortrait[portraitUsed] == 0)
		{
			UsedUnitPortrait.Remove(portraitUsed);
		}
	}

	public static void RemoveUsedPortraitColor(DataColor colorUsed)
	{
		UsedUnitPortraitColors[colorUsed]--;
		if (UsedUnitPortraitColors[colorUsed] == 0)
		{
			UsedUnitPortraitColors.Remove(colorUsed);
		}
	}

	public static DataColor GetRandomPortraitBGColor(PlayableUnit playableUnit)
	{
		List<DataColor> list = new List<DataColor>(PlayableUnitDatabase.PortraitBackgroundColors);
		if (UsedUnitPortraitColors.Count < list.Count)
		{
			List<DataColor> usedColors = new List<DataColor>(UsedUnitPortraitColors.Keys);
			list.RemoveAll((DataColor color) => usedColors.Contains(color));
		}
		DataColor randomElement = RandomManager.GetRandomElement(TPSingleton<PlayableUnitManager>.Instance, list);
		if (!UsedUnitPortraitColors.ContainsKey(randomElement))
		{
			UsedUnitPortraitColors.Add(randomElement, 0);
		}
		UsedUnitPortraitColors[randomElement]++;
		return randomElement;
	}

	public void EnableBloodPool()
	{
		bloodPool.SetActive(true);
	}

	public Sprite GetBodyPartSprite(string bodyPartId, BodyPartDefinition.E_Orientation orientation)
	{
		return PlayableUnit.BodyParts[bodyPartId].GetBodyPartView(orientation).GetSprite();
	}

	public void Init(PlayableUnit playableUnit)
	{
		((Object)this).name = playableUnit.Name;
		Unit = playableUnit;
		InitVisuals(playSpawnAnim: true);
		UpdatePosition();
		RefreshHudPositionInstantly();
		PortraitPanel = GameView.TopScreenPanel.UnitPortraitsPanel.AddPortrait(PlayableUnit);
	}

	public void InitDeadUnit(PlayableUnit playableUnit)
	{
		((Object)this).name = playableUnit.Name;
		Unit = playableUnit;
		InitVisuals(playSpawnAnim: false);
		UpdatePosition();
		RefreshHudPositionInstantly();
	}

	public override void InitVisuals(bool playSpawnAnim)
	{
		base.InitVisuals(playSpawnAnim);
		InitColorSwapTexture();
		SwapColors();
		RefreshBodyParts();
	}

	public void OnSkillTargetHover(bool hover, bool avoidPortrait = false)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		skillTargetingGroundRenderer.color = (hover ? skillTargetingHoverColor : Color.white);
		skillTargetingHaloRenderer.color = (hover ? skillTargetingHoverColor : Color.white);
		base.OnSkillTargetHover(hover);
		if (avoidPortrait)
		{
			if (hover)
			{
				GameView.TopScreenPanel.UnitPortraitsPanel.ToggleSelectedUnit(PlayableUnit);
			}
			else
			{
				GameView.TopScreenPanel.UnitPortraitsPanel.ToggleUnselectedUnit(PlayableUnit);
			}
		}
	}

	public virtual void OnSnapshotFinished()
	{
		foreach (KeyValuePair<GameObject, int> item in snapshotLayersBackup)
		{
			item.Key.layer = item.Value;
		}
		LookAtDirection(snapshotLookDirectionBackup);
		((Component)this).gameObject.SetActive(snapshotActiveBackup);
	}

	public virtual void PrepareForSnapshot()
	{
		if (snapshotLayersBackup == null)
		{
			snapshotLayersBackup = new Dictionary<GameObject, int>();
		}
		else
		{
			snapshotLayersBackup.Clear();
		}
		Renderer[] componentsInChildren = ((Component)this).GetComponentsInChildren<Renderer>(true);
		foreach (Renderer val in componentsInChildren)
		{
			snapshotLayersBackup.Add(((Component)val).gameObject, ((Component)val).gameObject.layer);
			((Component)val).gameObject.layer = CameraSnapshot.Constants.SnaptshotLayer;
		}
		snapshotLookDirectionBackup = Unit.LookDirection;
		LookAtDirection(GameDefinition.E_Direction.South);
		snapshotActiveBackup = ((Component)this).gameObject.activeSelf;
		((Component)this).gameObject.SetActive(true);
	}

	public void RefreshBodyParts(bool forceFullRefresh = false)
	{
		if (PlayableUnit != null)
		{
			int i = 0;
			for (int num = bodyPartViews.Length; i < num; i++)
			{
				bodyPartViews[i].Refresh(PlayableUnit.FaceId, PlayableUnit.Gender, forceFullRefresh);
			}
			RefreshSnapshot();
		}
	}

	public override void RefreshCursorFeedback()
	{
	}

	public override void RefreshHud(UnitStatDefinition.E_Stat stat)
	{
		base.RefreshHud(stat);
		if (stat == UnitStatDefinition.E_Stat.ManaTotal || stat == UnitStatDefinition.E_Stat.Mana)
		{
			RefreshMana();
		}
	}

	[ContextMenu("Refresh Mana")]
	public void RefreshMana()
	{
		(base.UnitHUD as PlayableUnitHUD).RefreshMana();
	}

	[ContextMenu("Refresh Snapshot")]
	public void RefreshSnapshot()
	{
		Unit.UiSprite = TPSingleton<CameraSnapshot>.Instance.TakeSnapshot(this);
	}

	public override void SetFrontAndBackActive(bool active)
	{
		if (!PlayableUnit.IsDead)
		{
			base.SetFrontAndBackActive(active);
		}
	}

	public void ToggleHelmetDisplay()
	{
		if (PlayableUnit.EquipmentSlots.TryGetValue(ItemSlotDefinition.E_ItemSlotId.Head, out var value) && value != null && value.Count > 0 && value[0].Item != null)
		{
			BodyPart bodyPart = PlayableUnit.BodyParts["Head"];
			if (PlayableUnit.HelmetDisplayed)
			{
				PlayableUnit.PlayableUnitController.OverrideBodyParts(value[0].Item.ItemDefinition.BodyPartsDefinitions);
			}
			else
			{
				bodyPart.BodyPartDefinitionOverride = null;
			}
			BodyPartView bodyPartView;
			if ((Object)(object)(bodyPartView = bodyPart.GetBodyPartView(BodyPartDefinition.E_Orientation.Front)) != (Object)null)
			{
				bodyPartView.IsDirty = true;
				bodyPartView.Refresh(PlayableUnit.FaceId, PlayableUnit.Gender);
			}
			if ((Object)(object)(bodyPartView = bodyPart.GetBodyPartView(BodyPartDefinition.E_Orientation.Back)) != (Object)null)
			{
				bodyPartView.IsDirty = true;
				bodyPartView.Refresh(PlayableUnit.FaceId, PlayableUnit.Gender);
			}
			RefreshSnapshot();
		}
	}

	public override void ToggleSkillTargeting(bool show)
	{
		if (show && TPSingleton<GameManager>.Instance.Game.Cursor.Tile != PlayableUnit.OriginTile)
		{
			OnSkillTargetHover(hover: false);
		}
		skillTargetingFeedbackParent.SetActive(show);
		PortraitPanel.ToggleSkillTargeting(show);
		base.ToggleSkillTargeting(show);
	}

	protected override void OnEnable()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		base.OnEnable();
		colorSwapMaterial = new Material(colorSwapMaterial);
		((Object)colorSwapMaterial).name = ((Object)colorSwapMaterial).name + " (Copy)";
		colorSwapPortraitMaterial = new Material(colorSwapPortraitMaterial);
		((Object)colorSwapPortraitMaterial).name = ((Object)colorSwapPortraitMaterial).name + " (Copy)";
		InitBodyPartViews();
	}

	protected override bool InitAnimations()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		if (!base.InitAnimations())
		{
			return false;
		}
		if (waitUntilAnimatorStateIsPrepareDie == null)
		{
			waitUntilAnimatorStateIsPrepareDie = new WaitUntil((Func<bool>)delegate
			{
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				AnimatorStateInfo currentAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
				return ((AnimatorStateInfo)(ref currentAnimatorStateInfo)).shortNameHash == Constants.Animation.AnimatorPrepareDeathStateHash;
			});
		}
		return true;
	}

	[ContextMenu("Init Swap Texture")]
	protected void InitColorSwapTexture()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)colorSwapTex == (Object)null)
		{
			colorSwapTex = new Texture2D(100, 1, (TextureFormat)4, false, false)
			{
				filterMode = (FilterMode)0
			};
		}
		for (int i = 0; i < ((Texture)colorSwapTex).width; i++)
		{
			colorSwapTex.SetPixel(i, 0, new Color(0f, 0f, 0f, 0f));
		}
		colorSwapTex.Apply();
		colorSwapMaterial.SetTexture("_SwapTex", (Texture)(object)colorSwapTex);
		colorSwapPortraitMaterial.SetTexture("_SwapTex", (Texture)(object)colorSwapTex);
	}

	protected override IEnumerator PlayDieAnimCoroutine()
	{
		deathStartAudioSource.Play();
		yield return (object)new WaitForSeconds(delayBeforeDeathSequence);
		if (EnemyUnitDatabase.HitByEnemySoundDefinitions.TryGetValue("PlayableUnits", out var value))
		{
			string soundId = value.GetSoundId(TPSingleton<EnemyUnitManager>.Instance.TotalCasters);
			ObjectPooler.GetPooledComponent<OneShotSound>("HitsSFX", TPSingleton<PlayableUnitManager>.Instance.HitSFXPrefab, (Transform)null, false).Play(ResourcePooler.LoadOnce<AudioClip>("Sounds/SFX/PlayableUnitHits/" + soundId, false));
		}
		else
		{
			((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).LogWarning((object)"No sound found for PlayableUnits on hit", (CLogLevel)1, true, false);
		}
		bool previousAllowUserPan = ACameraView.AllowUserPan;
		ACameraView.MoveTo(((Component)this).transform.position + new Vector3(0f, cameraFocusHeightOffset), 0f, (Ease)0);
		ACameraView.AllowUserPan = false;
		SetOrientation(front: true);
		animator.SetTrigger("PrepareDie");
		prepareDeathParticles.Play();
		yield return waitUntilAnimatorStateIsPrepareDie;
		yield return (object)new WaitForSeconds(delayBeforeBark);
		((Component)base.UnitHUD).gameObject.SetActive(false);
		TPSingleton<BarkManager>.Instance.AddPotentialBark("PlayableUnitSelfDeath", PlayableUnit, 0f, -1, forceSucceed: false, ignoreDeathCheck: true);
		TPSingleton<BarkManager>.Instance.Display();
		yield return SharedYields.WaitForSeconds(0.1f);
		yield return (object)new WaitUntil((Func<bool>)(() => BarkManager.DisplayedBarksCount == 0));
		yield return (object)new WaitForSeconds(delayAfterBark);
		deathImpactAudioSource.Play();
		executeCross.gameObject.SetActive(true);
		yield return SharedYields.WaitForSeconds(0.05f);
		prepareDeathParticles.Stop();
		ACameraView.Shake(deathShakeDuration, deathShakeStrength, 10, 0.1f, 0f, Vector3.zero, 0f);
		deathParticles.Play();
		frontShadow.SetActive(false);
		frontSortingGroup.sortingOrder = deathSortingOrder;
		animator.SetTrigger("Die");
		TPSingleton<LightningSDKManager>.Instance.TriggerFlashEffect(TPSingleton<PlayableUnitManager>.Instance.PlayableUnits.Count > 1);
		yield return CameraView.CameraVisualEffects.InvertImageCoroutine(0.05f, (Ease)1);
		((MonoBehaviour)CameraView.CameraVisualEffects).StartCoroutine(CameraView.CameraVisualEffects.ResetInvertCoroutine(0.05f, (Ease)1));
		((MonoBehaviour)CameraView.CameraVisualEffects).StartCoroutine(CameraView.CameraVisualEffects.RedFlashCoroutine(-1f, (Ease)0));
		yield return waitUntilAnimatorStateIsDie;
		yield return waitUntilAnimatorStateIsDead;
		ACameraView.AllowUserPan = previousAllowUserPan;
		yield return SharedYields.WaitForSeconds(delayAfterDeathSequence);
		DeathSequenceOver = true;
	}

	private void InitBodyPartViews()
	{
		if (bodyPartViews == null)
		{
			bodyPartViews = ((Component)base.OrientationRootTransform).GetComponentsInChildren<BodyPartView>(true);
			int i = 0;
			for (int num = bodyPartViews.Length; i < num; i++)
			{
				bodyPartViews[i].Init();
				bodyPartViews[i].SetMaterial(colorSwapMaterial);
			}
		}
	}

	private void SwapColor(int index, Color color)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		colorSwapTex.SetPixel(index, 0, color);
	}

	private void SwapColors()
	{
		SwapColorsForPalette(PlayableUnit.HairColorPalette);
		SwapColorsForPalette(PlayableUnit.SkinColorPalette);
		SwapColorsForPalette(PlayableUnit.EyesColorPalette);
		colorSwapTex.Apply();
	}

	private void SwapColorsForPalette(ColorSwapPaletteDefinition paletteDefinition)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (paletteDefinition != null)
		{
			int i = 0;
			for (int count = paletteDefinition.ColorSwapDefinitions.Count; i < count; i++)
			{
				SwapColor(paletteDefinition.ColorSwapDefinitions[i].Index, paletteDefinition.ColorSwapDefinitions[i].OutputColor);
			}
		}
	}

	[ContextMenu("Refresh ColorSwap")]
	public void DebugRefreshColorSwapping()
	{
		InitColorSwapTexture();
		SwapColors();
		RefreshSnapshot();
	}

	[ContextMenu("Refresh BodyParts")]
	private void DebugForceRefreshBodyParts()
	{
		RefreshBodyParts(forceFullRefresh: true);
	}

	[ContextMenu("Debug Lifetime Stats")]
	private void DebugLifetimeStats()
	{
		((CLogger<PlayableUnitManager>)TPSingleton<PlayableUnitManager>.Instance).Log((object)PlayableUnit.LifetimeStats.LifetimeStatsController.DebugGetLifetimeStatsLog(PlayableUnit.PlayableUnitName), (CLogLevel)1, false, false);
	}
}
