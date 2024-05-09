using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using PortraitAPI;
using PortraitAPI.Layers;
using PortraitAPI.Misc;
using TMPro;
using TPLib;
using TPLib.Localization.Fonts;
using TPLib.UI;
using TheLastStand.Controller;
using TheLastStand.DRM.Achievements;
using TheLastStand.Database.Unit;
using TheLastStand.Definition;
using TheLastStand.Framework.UI;
using TheLastStand.Manager;
using TheLastStand.Manager.Achievements;
using TheLastStand.Model;
using TheLastStand.Model.Unit;
using TheLastStand.View.Camera;
using TheLastStand.View.CharacterSheet;
using TheLastStand.View.HUD;
using TheLastStand.View.NightReport;
using TheLastStand.View.ToDoList;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TheLastStand.View.PlayableUnitCustomisation;

public class PlayableUnitCustomisationPanel : TPSingleton<PlayableUnitCustomisationPanel>, IOverlayUser
{
	[SerializeField]
	private Material colorSwapMaterial;

	[SerializeField]
	private Material colorSwapPortraitMaterial;

	[SerializeField]
	private Canvas canvas;

	[SerializeField]
	private CanvasGroup canvasGroup;

	[SerializeField]
	private SimpleFontLocalizedParent fontLocalizedParent;

	[SerializeField]
	private BetterButton confirmButton;

	[SerializeField]
	private BetterButton resetButton;

	[SerializeField]
	private Image imageBG;

	[SerializeField]
	private PortraitPartsDisplay portraitPartsDisplay;

	[SerializeField]
	private AppearanceToggle apperanceToggle;

	[SerializeField]
	private List<LayerTextureHandler> textureHandlers = new List<LayerTextureHandler>();

	[SerializeField]
	private BetterToggle beardLockToHairToggle;

	[SerializeField]
	private TextMeshProUGUI beardLockToHairToggleText;

	[SerializeField]
	private List<ColorHandler> colorHandlers = new List<ColorHandler>();

	[SerializeField]
	private RenameHeader renameHeader;

	[SerializeField]
	private PortraitCodePanel portraitCodePanel;

	[SerializeField]
	private Image unitAvatarImage;

	[SerializeField]
	private HUDJoystickTarget joystickTarget;

	private static readonly int SwapTex = Shader.PropertyToID("_SwapTex");

	private Texture2D currentColorTexture;

	private Tween fadeTween;

	private Handler currentJoystickSelectedHandler;

	private bool initialized;

	private bool openedThisFrame;

	private string initialCode = string.Empty;

	private string initialFaceId = string.Empty;

	private string initialName = string.Empty;

	[HideInInspector]
	public bool RefreshBeardHandler;

	[HideInInspector]
	public bool RefreshColors;

	[HideInInspector]
	public bool RefreshPortraitParts;

	[HideInInspector]
	public E_Gender CurrentGender;

	private PlayableUnit playableUnit;

	private E_Gender initialGender;

	private readonly Dictionary<E_LayerType, SimpleLayer> currentLayers = new Dictionary<E_LayerType, SimpleLayer>();

	private readonly Dictionary<E_LayerKind, Material> layerMaterials = new Dictionary<E_LayerKind, Material>();

	private readonly Dictionary<E_Gender, Dictionary<E_LayerType, CodeLayerData>> previousIndexByLayerType = new Dictionary<E_Gender, Dictionary<E_LayerType, CodeLayerData>>();

	public Dictionary<E_ColorTypes, Dictionary<string, ColorSwapPaletteDefinition>> AllPalettesByColorType { get; } = new Dictionary<E_ColorTypes, Dictionary<string, ColorSwapPaletteDefinition>>();


	public bool BeardIsLockToHairType { get; private set; } = true;


	public Material ColorSwapMaterial => colorSwapMaterial;

	public List<SimpleLayer> CurrentBeards { get; } = new List<SimpleLayer>();


	public Dictionary<E_ColorTypes, CodeColorData> CurrentIndexByColorType { get; } = new Dictionary<E_ColorTypes, CodeColorData>();


	public Dictionary<E_LayerType, CodeLayerData> CurrentIndexByLayerType { get; private set; } = new Dictionary<E_LayerType, CodeLayerData>();


	public int OverlaySortingOrder => canvas.sortingOrder - 2;

	public PortraitCodePanel PortraitCodePanel => portraitCodePanel;

	public RenameHeader RenameHeader => renameHeader;

	public Dictionary<E_ColorTypes, Dictionary<string, Texture2D>> TexturesByColorType { get; } = new Dictionary<E_ColorTypes, Dictionary<string, Texture2D>>();


	public void ChangeIndexOfColorType(E_ColorTypes colorType, int valueToSet)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		CurrentIndexByColorType[colorType].SetIndex(valueToSet);
		UpdateColorHandler(colorType);
		RefreshColors = true;
	}

	public void ChangeIndexOfLayerType(E_LayerType layerType, int valueToSet)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		E_Gender gender = CurrentGender;
		if ((int)layerType == 6 && BeardIsLockToHairType)
		{
			SimpleLayer item = CurrentBeards[valueToSet];
			valueToSet = LayerManagement.GetTheseLayers<SimpleLayer>(CurrentIndexByLayerType[(E_LayerType)1].Gender, (E_LayerType)6, true).IndexOf(item);
			gender = CurrentIndexByLayerType[(E_LayerType)1].Gender;
		}
		UpdateCurrentIndex(layerType, gender, valueToSet);
	}

	public void Init()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected I4, but got Unknown
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Expected O, but got Unknown
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Expected O, but got Unknown
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Expected O, but got Unknown
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Expected O, but got Unknown
		//IL_0265: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fa: Expected O, but got Unknown
		//IL_0318: Unknown result type (might be due to invalid IL or missing references)
		if (initialized)
		{
			return;
		}
		((UnityEvent)((Button)confirmButton).onClick).AddListener(new UnityAction(OnConfirmButtonClicked));
		((UnityEvent)((Button)resetButton).onClick).AddListener(new UnityAction(OnResetButtonClicked));
		for (int i = 0; i < Enum.GetValues(typeof(E_ColorTypes)).Length; i++)
		{
			E_ColorTypes val = (E_ColorTypes)i;
			Dictionary<string, ColorSwapPaletteDefinition> value = (int)val switch
			{
				0 => PlayableUnitDatabase.PlayableUnitSkinColorDefinitions, 
				1 => PlayableUnitDatabase.PlayableUnitHairColorDefinitions, 
				2 => PlayableUnitDatabase.PlayableUnitEyesColorDefinitions, 
				_ => null, 
			};
			AllPalettesByColorType.Add(val, value);
		}
		if (CurrentIndexByLayerType.Count == 0)
		{
			for (int j = 0; j < Enum.GetValues(typeof(E_LayerType)).Length; j++)
			{
				E_LayerType key = (E_LayerType)j;
				CurrentIndexByLayerType.Add(key, new CodeLayerData(CurrentGender, 1));
			}
		}
		if (CurrentIndexByColorType.Count == 0)
		{
			for (int k = 0; k < Enum.GetValues(typeof(E_ColorTypes)).Length; k++)
			{
				E_ColorTypes key2 = (E_ColorTypes)k;
				CurrentIndexByColorType.Add(key2, new CodeColorData(0));
			}
		}
		TexturesByColorType.Add((E_ColorTypes)1, new Dictionary<string, Texture2D>());
		TexturesByColorType.Add((E_ColorTypes)0, new Dictionary<string, Texture2D>());
		TexturesByColorType.Add((E_ColorTypes)2, new Dictionary<string, Texture2D>());
		foreach (KeyValuePair<string, ColorSwapPaletteDefinition> playableUnitHairColorDefinition in PlayableUnitDatabase.PlayableUnitHairColorDefinitions)
		{
			Texture2D colorSwapTex = new Texture2D(100, 1, (TextureFormat)4, false, false)
			{
				filterMode = (FilterMode)0
			};
			for (int l = 0; l < ((Texture)colorSwapTex).width; l++)
			{
				colorSwapTex.SetPixel(l, 0, new Color(0f, 0f, 0f, 0f));
			}
			SwapColorsForPalette(playableUnitHairColorDefinition.Value, ref colorSwapTex);
			colorSwapTex.Apply();
			TexturesByColorType[(E_ColorTypes)1].Add(playableUnitHairColorDefinition.Key, colorSwapTex);
		}
		foreach (KeyValuePair<string, ColorSwapPaletteDefinition> playableUnitEyesColorDefinition in PlayableUnitDatabase.PlayableUnitEyesColorDefinitions)
		{
			Texture2D colorSwapTex2 = new Texture2D(100, 1, (TextureFormat)4, false, false)
			{
				filterMode = (FilterMode)0
			};
			for (int m = 0; m < ((Texture)colorSwapTex2).width; m++)
			{
				colorSwapTex2.SetPixel(m, 0, new Color(0f, 0f, 0f, 0f));
			}
			SwapColorsForPalette(playableUnitEyesColorDefinition.Value, ref colorSwapTex2);
			colorSwapTex2.Apply();
			TexturesByColorType[(E_ColorTypes)2].Add(playableUnitEyesColorDefinition.Key, colorSwapTex2);
		}
		foreach (KeyValuePair<string, ColorSwapPaletteDefinition> playableUnitSkinColorDefinition in PlayableUnitDatabase.PlayableUnitSkinColorDefinitions)
		{
			Texture2D colorSwapTex3 = new Texture2D(100, 1, (TextureFormat)4, false, false)
			{
				filterMode = (FilterMode)0
			};
			for (int n = 0; n < ((Texture)colorSwapTex3).width; n++)
			{
				colorSwapTex3.SetPixel(n, 0, new Color(0f, 0f, 0f, 0f));
			}
			SwapColorsForPalette(playableUnitSkinColorDefinition.Value, ref colorSwapTex3);
			colorSwapTex3.Apply();
			TexturesByColorType[(E_ColorTypes)0].Add(playableUnitSkinColorDefinition.Key, colorSwapTex3);
		}
		initialized = true;
	}

	public void OnBeardLockToHairAvailabilityChanged(bool availabilityState)
	{
		((Selectable)beardLockToHairToggle).interactable = !availabilityState;
		((Graphic)((Selectable)beardLockToHairToggle).image).CrossFadeAlpha(availabilityState ? 0.35f : 1f, 0.25f, false);
		DOTweenModuleUI.DOFade((Graphic)(object)beardLockToHairToggleText, availabilityState ? 0.35f : 1f, 0.25f);
		if (((Toggle)beardLockToHairToggle).isOn)
		{
			((Toggle)beardLockToHairToggle).isOn = false;
		}
	}

	public void OnCancelButtonClicked()
	{
		if (!InputManager.IsLastControllerJoystick || !openedThisFrame)
		{
			OnResetButtonClicked();
			Close();
		}
	}

	public void Open(PlayableUnit playableUnit)
	{
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Expected O, but got Unknown
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Expected O, but got Unknown
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		Init();
		((Behaviour)canvas).enabled = true;
		Tween obj = fadeTween;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
		fadeTween = (Tween)(object)DOTweenModuleUI.DOFade(canvasGroup, 1f, 0.25f);
		TweenExtensions.Play<Tween>(fadeTween);
		CameraView.AttenuateWorldForPopupFocus((IOverlayUser)(object)this);
		if (playableUnit != null)
		{
			this.playableUnit = playableUnit;
			CodeData portraitCodeData = playableUnit.PortraitCodeData;
			unitAvatarImage.sprite = playableUnit.UiSprite;
			SetInitialsValues(this.playableUnit, portraitCodeData);
			renameHeader.Refresh(playableUnit);
			portraitCodePanel.Refresh(portraitCodeData);
			PopulateTextureHandlers(portraitCodeData);
			PopulateColorHandlers(portraitCodeData);
			currentColorTexture = (Texture2D)playableUnit.PlayableUnitView.ColorSwapPortraitMaterial.GetTexture(SwapTex);
			layerMaterials.Clear();
			for (int i = 0; i < Enum.GetValues(typeof(E_LayerKind)).Length; i++)
			{
				E_LayerKind key = (E_LayerKind)i;
				Material val = new Material(colorSwapPortraitMaterial)
				{
					name = "Copy - " + ((object)(E_LayerKind)(ref key)).ToString()
				};
				val.SetTexture(SwapTex, (Texture)(object)currentColorTexture);
				layerMaterials.Add(key, val);
			}
			imageBG.sprite = playableUnit.PortraitBackgroundSprite;
			((Graphic)imageBG).color = playableUnit.PortraitColor._Color;
			apperanceToggle.SelectState(CurrentGender);
			((UnityEvent<E_Gender>)apperanceToggle.OnAppearanceChanged).AddListener((UnityAction<E_Gender>)OnAppearanceChange);
			((UnityEvent<bool>)(object)((Toggle)beardLockToHairToggle).onValueChanged).AddListener((UnityAction<bool>)delegate
			{
				SwitchBeardIsLockToHairType();
			});
			if ((Object)(object)fontLocalizedParent != (Object)null)
			{
				((FontLocalizedParent)fontLocalizedParent).RefreshChilds();
			}
			RefreshPortraitParts = true;
			openedThisFrame = true;
			if (InputManager.IsLastControllerJoystick)
			{
				TPSingleton<HUDJoystickNavigationManager>.Instance.SelectPanel(joystickTarget.GetSelectionInfo());
			}
			((Component)PortraitCodePanel).gameObject.SetActive(!InputManager.IsLastControllerJoystick);
		}
	}

	public void SetHandlerAsJoystickTarget(Handler handler)
	{
		currentJoystickSelectedHandler = handler;
	}

	public void SwitchBeardIsLockToHairType()
	{
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Invalid comparison between Unknown and I4
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Invalid comparison between Unknown and I4
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		BeardIsLockToHairType = ((Toggle)beardLockToHairToggle).isOn;
		LayerTextureHandler layerTextureHandler = textureHandlers.Find((LayerTextureHandler x) => (int)x.LayerType == 6);
		if (!BeardIsLockToHairType)
		{
			int num = ((CurrentIndexByLayerType[(E_LayerType)6].LayerIndex > -1) ? CurrentIndexByLayerType[(E_LayerType)6].LayerIndex : 0);
			E_Gender gender = CurrentIndexByLayerType[(E_LayerType)6].Gender;
			SimpleLayer val = null;
			List<BeardLayers> theseLayers = LayerManagement.GetTheseLayers<BeardLayers>(gender, (E_LayerType)6, true);
			if (num < theseLayers.Count)
			{
				val = (SimpleLayer)(object)theseLayers[num];
			}
			layerTextureHandler.Refresh();
			int valueWithoutNotify = -1;
			if (val != null)
			{
				valueWithoutNotify = (((int)gender == 1) ? CurrentBeards.LastIndexOf(val) : CurrentBeards.IndexOf(val));
				UpdateCurrentIndex((E_LayerType)6, gender, num);
			}
			else
			{
				UpdateCurrentIndex((E_LayerType)6, CurrentGender, -1);
			}
			layerTextureHandler.DropDown.SetValueWithoutNotify(valueWithoutNotify);
		}
		else if (Object.op_Implicit((Object)(object)beardLockToHairToggle) && CurrentIndexByLayerType[(E_LayerType)6].LayerIndex != -1)
		{
			layerTextureHandler.Refresh();
			int num2 = -1;
			if (CurrentBeards.Count != 0)
			{
				E_Gender gender2 = CurrentIndexByLayerType[(E_LayerType)6].Gender;
				List<BeardLayers> theseLayers2 = LayerManagement.GetTheseLayers<BeardLayers>(gender2, (E_LayerType)6, true);
				SimpleLayer item = (SimpleLayer)(object)theseLayers2[Mathf.Clamp(CurrentIndexByLayerType[(E_LayerType)6].LayerIndex, 0, theseLayers2.Count - 1)];
				num2 = (CurrentBeards.Contains(item) ? (((int)gender2 == 1) ? CurrentBeards.LastIndexOf(item) : CurrentBeards.IndexOf(item)) : 0);
			}
			if (num2 > -1)
			{
				SimpleLayer item2 = CurrentBeards[num2];
				UpdateCurrentIndex((E_LayerType)6, CurrentGender, LayerManagement.GetTheseLayers<SimpleLayer>(CurrentGender, (E_LayerType)6, true).IndexOf(item2));
			}
			else
			{
				UpdateCurrentIndex((E_LayerType)6, CurrentGender, -1);
			}
			layerTextureHandler.DropDown.SetValueWithoutNotify(num2);
		}
		else
		{
			layerTextureHandler.Refresh();
			if (CurrentBeards.Count > 0)
			{
				SimpleLayer item3 = CurrentBeards[0];
				UpdateCurrentIndex((E_LayerType)6, CurrentGender, LayerManagement.GetTheseLayers<SimpleLayer>(CurrentIndexByLayerType[(E_LayerType)6].Gender, (E_LayerType)6, true).IndexOf(item3));
			}
			else
			{
				UpdateCurrentIndex((E_LayerType)6, CurrentGender, -1);
			}
			layerTextureHandler.DropDown.SetValueWithoutNotify((CurrentBeards.Count <= 0) ? (-1) : 0);
		}
		RefreshPortraitParts = true;
	}

	public void UpdateBackgroundTexture()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		Texture2D val = BackgroundCreater.WriteTexture(LayerManagement.CreateOneTextureFromMultipleTextures(currentLayers, Color.white, Color.black));
		Sprite sprite = Sprite.Create(val, new Rect(0f, 0f, (float)((Texture)val).width, (float)((Texture)val).height), PortraitAPIManager.GetTexturesData().PivotPoint, (float)PortraitAPIManager.GetTexturesData().PixelPerUnit);
		imageBG.sprite = sprite;
	}

	public void UpdateTextureHandlersOnChangePortraitCode(CodeData codeData)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Invalid comparison between Unknown and I4
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Invalid comparison between Unknown and I4
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Invalid comparison between Unknown and I4
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		previousIndexByLayerType[CurrentGender] = IEnumerableExtension.Clone(CurrentIndexByLayerType);
		E_Gender gender = codeData.CodeFooterData.Gender;
		if (CurrentGender != gender)
		{
			apperanceToggle.SelectStateWithoutNotify(gender);
		}
		CurrentIndexByLayerType = IEnumerableExtension.Clone(codeData.CodeSectionDatas);
		textureHandlers.ForEach(delegate(LayerTextureHandler x)
		{
			x.Refresh();
		});
		if (codeData.CodeSectionDatas[(E_LayerType)6].LayerIndex != -1)
		{
			HairLayer val = LayerManagement.GetTheseLayers<HairLayer>(CurrentIndexByLayerType[(E_LayerType)1].Gender, (E_LayerType)1, true)[CurrentIndexByLayerType[(E_LayerType)1].LayerIndex];
			BeardLayers val2 = LayerManagement.GetTheseLayers<BeardLayers>(CurrentIndexByLayerType[(E_LayerType)6].Gender, (E_LayerType)6, true)[codeData.CodeSectionDatas[(E_LayerType)6].LayerIndex];
			((Toggle)beardLockToHairToggle).isOn = val2.FaceIds.Contains(val.FaceId);
			BeardIsLockToHairType = val2.FaceIds.Contains(val.FaceId);
		}
		else
		{
			((Toggle)beardLockToHairToggle).isOn = true;
			BeardIsLockToHairType = true;
		}
		for (int i = 0; i < codeData.CodeSectionDatas.Count; i++)
		{
			E_LayerType layerType = (E_LayerType)i;
			LayerTextureHandler layerTextureHandler = textureHandlers.Find((LayerTextureHandler x) => x.LayerType == layerType);
			if (CurrentIndexByLayerType[layerType].LayerIndex <= -1)
			{
				continue;
			}
			if ((int)CurrentGender == 2 && (int)CurrentIndexByLayerType[layerType].Gender == 1)
			{
				layerTextureHandler.DropDown.SetValueWithoutNotify(LayerManagement.GetTheseLayers<SimpleLayer>((E_Gender)0, layerType, true).Count + CurrentIndexByLayerType[layerType].LayerIndex);
				continue;
			}
			int valueWithoutNotify = CurrentIndexByLayerType[layerType].LayerIndex;
			if ((int)layerType == 6)
			{
				SimpleLayer item = LayerManagement.GetTheseLayers<SimpleLayer>(CurrentIndexByLayerType[layerType].Gender, layerType, true)[CurrentIndexByLayerType[layerType].LayerIndex];
				if (CurrentBeards.Count != 0)
				{
					if (CurrentBeards.Contains(item))
					{
						valueWithoutNotify = CurrentBeards.IndexOf(item);
					}
					else
					{
						valueWithoutNotify = 0;
						CurrentIndexByLayerType[layerType].SetIndex(LayerManagement.GetTheseLayers<SimpleLayer>(CurrentIndexByLayerType[layerType].Gender, layerType, true).IndexOf(CurrentBeards[0]));
					}
				}
			}
			layerTextureHandler.DropDown.SetValueWithoutNotify(valueWithoutNotify);
		}
		PortraitCodePanel.Refresh(codeData);
		RefreshPortraitParts = true;
	}

	public void UpdateColorHandlersOnChangePortraitCode(CodeData codeData)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < Enum.GetValues(typeof(E_ColorTypes)).Length; i++)
		{
			E_ColorTypes val = (E_ColorTypes)i;
			ChangeIndexOfColorType(val, codeData.CodeColorDatas[val].Index);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		Init();
	}

	private void Close()
	{
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Expected O, but got Unknown
		previousIndexByLayerType.Clear();
		((UnityEvent<E_Gender>)apperanceToggle.OnAppearanceChanged)?.RemoveListener((UnityAction<E_Gender>)OnAppearanceChange);
		((UnityEvent<bool>)(object)((Toggle)beardLockToHairToggle).onValueChanged)?.RemoveListener((UnityAction<bool>)delegate
		{
			SwitchBeardIsLockToHairType();
		});
		CameraView.AttenuateWorldForPopupFocus((IOverlayUser)(object)TPSingleton<CharacterSheetPanel>.Instance);
		Tween obj = fadeTween;
		if (obj != null)
		{
			TweenExtensions.Kill(obj, false);
		}
		fadeTween = (Tween)(object)TweenSettingsExtensions.OnComplete<TweenerCore<float, float, FloatOptions>>(DOTweenModuleUI.DOFade(canvasGroup, 0f, 0.25f), (TweenCallback)delegate
		{
			((Behaviour)canvas).enabled = false;
			GameController.SetState(Game.E_State.CharacterSheet);
		});
		TweenExtensions.Play<Tween>(fadeTween);
		if (InputManager.IsLastControllerJoystick)
		{
			TPSingleton<HUDJoystickNavigationManager>.Instance.JoystickHighlight.Display(state: false);
			EventSystem.current.SetSelectedGameObject((GameObject)null);
		}
	}

	private void OnAppearanceChange(E_Gender gender)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Expected O, but got Unknown
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Invalid comparison between Unknown and I4
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Invalid comparison between Unknown and I4
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Invalid comparison between Unknown and I4
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0320: Unknown result type (might be due to invalid IL or missing references)
		//IL_0327: Unknown result type (might be due to invalid IL or missing references)
		//IL_0369: Unknown result type (might be due to invalid IL or missing references)
		//IL_036f: Invalid comparison between Unknown and I4
		//IL_0350: Unknown result type (might be due to invalid IL or missing references)
		//IL_0356: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0404: Unknown result type (might be due to invalid IL or missing references)
		//IL_037c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0387: Unknown result type (might be due to invalid IL or missing references)
		//IL_038e: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b7: Unknown result type (might be due to invalid IL or missing references)
		E_Gender currentGender = CurrentGender;
		CurrentGender = gender;
		if (previousIndexByLayerType.ContainsKey(gender) && previousIndexByLayerType[gender].Count != 0)
		{
			Dictionary<E_LayerType, CodeLayerData> dictionary = IEnumerableExtension.Clone(CurrentIndexByLayerType);
			CurrentIndexByLayerType = IEnumerableExtension.Clone(previousIndexByLayerType[gender]);
			if (!previousIndexByLayerType.ContainsKey(currentGender))
			{
				previousIndexByLayerType.Add(currentGender, IEnumerableExtension.Clone(dictionary));
			}
			else
			{
				previousIndexByLayerType[currentGender] = IEnumerableExtension.Clone(dictionary);
			}
			foreach (LayerTextureHandler textureHandler in textureHandlers)
			{
				textureHandler.Refresh();
				if ((int)textureHandler.LayerType == 6 && textureHandler.IsLocked && CurrentIndexByLayerType[textureHandler.LayerType].LayerIndex == -1)
				{
					((Toggle)textureHandler.LockToggle).isOn = false;
					((Toggle)beardLockToHairToggle).isOn = true;
				}
				int valueWithoutNotify = CurrentIndexByLayerType[textureHandler.LayerType].LayerIndex;
				if ((int)CurrentGender == 2 && (int)CurrentIndexByLayerType[textureHandler.LayerType].Gender == 1)
				{
					valueWithoutNotify = LayerManagement.GetTheseLayers<SimpleLayer>(CurrentIndexByLayerType[textureHandler.LayerType].Gender, textureHandler.LayerType, true).Count + CurrentIndexByLayerType[textureHandler.LayerType].LayerIndex;
				}
				textureHandler.DropDown.SetValueWithoutNotify(valueWithoutNotify);
			}
		}
		else
		{
			if (!previousIndexByLayerType.ContainsKey(currentGender))
			{
				previousIndexByLayerType.Add(currentGender, new Dictionary<E_LayerType, CodeLayerData>());
			}
			foreach (KeyValuePair<E_LayerType, CodeLayerData> item2 in CurrentIndexByLayerType)
			{
				if (!previousIndexByLayerType[currentGender].ContainsKey(item2.Key))
				{
					previousIndexByLayerType[currentGender].Add(item2.Key, item2.Value.Clone());
				}
				else
				{
					previousIndexByLayerType[currentGender][item2.Key] = item2.Value.Clone();
				}
			}
			if (!previousIndexByLayerType[currentGender].ContainsKey((E_LayerType)6))
			{
				previousIndexByLayerType[currentGender].Add((E_LayerType)6, new CodeLayerData(CurrentGender, 0));
			}
			foreach (LayerTextureHandler textureHandler2 in textureHandlers)
			{
				textureHandler2.Refresh();
				if (textureHandler2.DropDown.options.Count == 0)
				{
					UpdateCurrentIndex(textureHandler2.LayerType, CurrentGender, -1);
					textureHandler2.DropDown.SetValueWithoutNotify(-1);
					continue;
				}
				List<SimpleLayer> theseLayers = LayerManagement.GetTheseLayers<SimpleLayer>(currentGender, textureHandler2.LayerType, true);
				SimpleLayer item = ((theseLayers.Count > 0) ? theseLayers[Mathf.Clamp(CurrentIndexByLayerType[textureHandler2.LayerType].LayerIndex, 0, theseLayers.Count - 1)] : null);
				List<SimpleLayer> theseLayers2 = LayerManagement.GetTheseLayers<SimpleLayer>(CurrentGender, textureHandler2.LayerType, true);
				int num = 0;
				if (theseLayers2.Contains(item))
				{
					num = theseLayers2.IndexOf(item);
					UpdateCurrentIndex(textureHandler2.LayerType, CurrentGender, num);
				}
				else if ((int)textureHandler2.LayerType == 6)
				{
					num = 0;
					CurrentIndexByLayerType[textureHandler2.LayerType].SetIndex(LayerManagement.GetTheseLayers<SimpleLayer>(CurrentGender, textureHandler2.LayerType, true).IndexOf(CurrentBeards[0]));
					CurrentIndexByLayerType[textureHandler2.LayerType].SetGender((E_Gender)0);
				}
				else
				{
					num = Mathf.Clamp(CurrentIndexByLayerType[textureHandler2.LayerType].LayerIndex, 0, textureHandler2.DropDown.options.Count - 1);
					UpdateCurrentIndex(textureHandler2.LayerType, CurrentGender, num);
				}
				textureHandler2.DropDown.SetValueWithoutNotify(num);
			}
		}
		RefreshPortraitParts = true;
	}

	private void OnConfirmButtonClicked()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		HairLayer val = LayerManagement.GetTheseLayers<HairLayer>(CurrentIndexByLayerType[(E_LayerType)1].Gender, (E_LayerType)1, true)[CurrentIndexByLayerType[(E_LayerType)1].LayerIndex];
		playableUnit.FaceId = val.FaceId;
		playableUnit.PlayableUnitName = RenameHeader.CurrentName;
		((Object)playableUnit.PlayableUnitView).name = playableUnit.PlayableUnitName;
		((Object)playableUnit.PlayableUnitView.UnitHUD).name = playableUnit.PlayableUnitName + " HUD";
		playableUnit.PortraitCodeData = PortraitCodePanel.CurrentCode.Clone();
		SetNewTextures();
		playableUnit.PlayableUnitView.ColorSwapPortraitMaterial.SetTexture(SwapTex, (Texture)(object)currentColorTexture);
		GameView.TopScreenPanel.UnitPortraitsPanel.RefreshPortraits();
		TPSingleton<CharacterSheetPanel>.Instance.RefreshAvatar();
		TPSingleton<CharacterSheetPanel>.Instance.RefreshName(TileObjectSelectionManager.SelectedPlayableUnit);
		TPSingleton<CharacterSheetPanel>.Instance.RefreshPortrait(TileObjectSelectionManager.SelectedPlayableUnit);
		GameView.CharacterDetailsView.IsDirty = true;
		TileObjectSelectionManager.SelectedUnitFeedback.Refresh();
		TPSingleton<ToDoListView>.Instance.RefreshAllNotifications();
		TPSingleton<GameOverPanel>.Instance.RefreshPlayables();
		TPSingleton<NightReportPanel>.Instance.RefreshPlayables();
		TPSingleton<AchievementManager>.Instance.UnlockAchievement(AchievementContainer.ACH_CUSTOMIZE_HERO);
		Close();
	}

	private void OnResetButtonClicked()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Invalid comparison between Unknown and I4
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Invalid comparison between Unknown and I4
		playableUnit.FaceId = initialFaceId;
		playableUnit.Gender = (((int)initialGender == 0) ? "Male" : "Female");
		playableUnit.PlayableUnitName = initialName;
		RenameHeader.Refresh(initialName);
		CodeData val = default(CodeData);
		CodeGenerator.TryDecode(initialCode, ref val);
		playableUnit.PortraitCodeData = val;
		UpdateTextureHandlersOnChangePortraitCode(val);
		CurrentIndexByColorType[(E_ColorTypes)0].SetIndex(val.CodeColorDatas[(E_ColorTypes)0].Index);
		CurrentIndexByColorType[(E_ColorTypes)1].SetIndex(val.CodeColorDatas[(E_ColorTypes)1].Index);
		CurrentIndexByColorType[(E_ColorTypes)2].SetIndex(val.CodeColorDatas[(E_ColorTypes)2].Index);
		CurrentIndexByColorType[(E_ColorTypes)3].SetIndex(val.CodeColorDatas[(E_ColorTypes)3].Index);
		int length = Enum.GetValues(typeof(E_ColorTypes)).Length;
		for (int i = 0; i < length; i++)
		{
			E_ColorTypes colorType = (E_ColorTypes)i;
			ColorHandler colorHandler = colorHandlers.Find((ColorHandler x) => x.ColorType == colorType);
			if ((Object)(object)colorHandler != (Object)null)
			{
				E_ColorTypes val2 = colorType;
				if ((int)val2 > 2)
				{
					if ((int)val2 == 3)
					{
						colorHandler.Refresh(PlayableUnitDatabase.PortraitBackgroundColors);
					}
				}
				else
				{
					colorHandler.Refresh();
				}
			}
			UpdateColorHandler(colorType);
		}
		playableUnit.PlayableUnitView.RefreshBodyParts(forceFullRefresh: true);
	}

	private void PopulateColorHandlers(CodeData codeData)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Invalid comparison between Unknown and I4
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Invalid comparison between Unknown and I4
		int length = Enum.GetValues(typeof(E_ColorTypes)).Length;
		for (int i = 0; i < length; i++)
		{
			E_ColorTypes colorType = (E_ColorTypes)i;
			ColorHandler colorHandler = colorHandlers.Find((ColorHandler x) => x.ColorType == colorType);
			CurrentIndexByColorType[colorType].SetIndex(codeData.CodeColorDatas[colorType].Index);
			if (!((Object)(object)colorHandler != (Object)null))
			{
				continue;
			}
			E_ColorTypes val = colorType;
			if ((int)val > 2)
			{
				if ((int)val == 3)
				{
					colorHandler.Refresh(PlayableUnitDatabase.PortraitBackgroundColors);
				}
			}
			else
			{
				colorHandler.Refresh();
			}
		}
	}

	private void PopulateTextureHandlers(CodeData datas)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Invalid comparison between Unknown and I4
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Invalid comparison between Unknown and I4
		//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Invalid comparison between Unknown and I4
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		CurrentGender = datas.CodeFooterData.Gender;
		BeardLayers val = null;
		if (datas.GetSectionData((E_LayerType)6).LayerIndex != -1)
		{
			HairLayer val2 = LayerManagement.GetTheseLayers<HairLayer>(datas.GetSectionData((E_LayerType)1).Gender, (E_LayerType)1, true)[datas.GetSectionData((E_LayerType)1).LayerIndex];
			val = LayerManagement.GetTheseLayers<BeardLayers>(datas.GetSectionData((E_LayerType)6).Gender, (E_LayerType)6, true)[datas.GetSectionData((E_LayerType)6).LayerIndex];
			((Toggle)beardLockToHairToggle).isOn = val.FaceIds.Contains(val2.FaceId);
			BeardIsLockToHairType = val.FaceIds.Contains(val2.FaceId);
		}
		int length = Enum.GetValues(typeof(E_LayerType)).Length;
		for (int i = 0; i < length; i++)
		{
			E_LayerType layerType = (E_LayerType)i;
			CurrentIndexByLayerType[layerType].SetIndex(datas.GetSectionData(layerType).LayerIndex);
			CurrentIndexByLayerType[layerType].SetGender(datas.GetSectionData(layerType).Gender);
			LayerTextureHandler layerTextureHandler = textureHandlers.Find((LayerTextureHandler x) => x.LayerType == layerType);
			if (!((Object)(object)layerTextureHandler != (Object)null))
			{
				continue;
			}
			layerTextureHandler.Refresh();
			if ((int)layerType == 6 && datas.GetSectionData(layerType).LayerIndex != -1)
			{
				if (CurrentBeards.Count != 0)
				{
					if (CurrentBeards.Contains((SimpleLayer)(object)val))
					{
						CurrentIndexByLayerType[layerType].SetIndex(LayerManagement.GetTheseLayers<SimpleLayer>(datas.GetSectionData(layerType).Gender, layerType, true).IndexOf((SimpleLayer)(object)val));
						layerTextureHandler.DropDown.SetValueWithoutNotify(CurrentBeards.IndexOf((SimpleLayer)(object)val));
					}
					else
					{
						CurrentIndexByLayerType[layerType].SetIndex(LayerManagement.GetTheseLayers<SimpleLayer>(datas.GetSectionData(layerType).Gender, layerType, true).IndexOf(CurrentBeards[0]));
						layerTextureHandler.DropDown.SetValueWithoutNotify(0);
					}
				}
				else
				{
					CurrentIndexByLayerType[layerType].SetIndex(-1);
				}
			}
			else if (CurrentIndexByLayerType[layerType].LayerIndex > -1)
			{
				if ((int)CurrentGender == 2 && (int)CurrentIndexByLayerType[layerType].Gender == 1)
				{
					layerTextureHandler.DropDown.SetValueWithoutNotify(LayerManagement.GetTheseLayers<SimpleLayer>((E_Gender)0, layerType, true).Count + CurrentIndexByLayerType[layerType].LayerIndex);
				}
				else
				{
					layerTextureHandler.DropDown.SetValueWithoutNotify(CurrentIndexByLayerType[layerType].LayerIndex);
				}
			}
		}
	}

	private void Update()
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		if (!((Behaviour)canvas).enabled)
		{
			return;
		}
		if (RefreshBeardHandler)
		{
			RefreshBeardHandler = false;
			LayerTextureHandler layerTextureHandler = textureHandlers.Find((LayerTextureHandler x) => (int)x.LayerType == 6);
			SimpleLayer val = null;
			if (LayerManagement.GetTheseLayers<SimpleLayer>(CurrentIndexByLayerType[(E_LayerType)6].Gender, (E_LayerType)6, true).Count > 0 && CurrentIndexByLayerType[(E_LayerType)6].LayerIndex > -1)
			{
				val = LayerManagement.GetTheseLayers<SimpleLayer>(CurrentIndexByLayerType[(E_LayerType)6].Gender, (E_LayerType)6, true)[CurrentIndexByLayerType[(E_LayerType)6].LayerIndex];
			}
			layerTextureHandler.Refresh();
			if (layerTextureHandler.DropDown.options.Count > 0)
			{
				if (val != null && CurrentBeards.Contains(val))
				{
					layerTextureHandler.DropDown.SetValueWithoutNotify(CurrentBeards.IndexOf(val));
					return;
				}
				int index = LayerManagement.GetTheseLayers<SimpleLayer>(CurrentIndexByLayerType[(E_LayerType)6].Gender, (E_LayerType)6, true).IndexOf(CurrentBeards[0]);
				CurrentIndexByLayerType[(E_LayerType)6].SetIndex(index);
				layerTextureHandler.DropDown.SetValueWithoutNotify(0);
			}
			else
			{
				CurrentIndexByLayerType[(E_LayerType)6].SetIndex(-1);
			}
		}
		if (RefreshColors)
		{
			RefreshColors = false;
			playableUnit.PlayableUnitView.RefreshBodyParts(forceFullRefresh: true);
			unitAvatarImage.sprite = playableUnit.UiSprite;
			RefreshCodePanel(CurrentIndexByLayerType);
		}
		if (RefreshPortraitParts)
		{
			RefreshPortraitParts = false;
			Dictionary<E_LayerType, CodeLayerData> dictionary = IEnumerableExtension.Clone(CurrentIndexByLayerType);
			currentLayers.Clear();
			for (int i = 0; i < dictionary.Count; i++)
			{
				if (dictionary.ElementAt(i).Value.LayerIndex >= 0)
				{
					E_LayerType val2 = (E_LayerType)i;
					SimpleLayer value = LayerManagement.GetTheseLayers<SimpleLayer>(dictionary[val2].Gender, val2, true)[dictionary[val2].LayerIndex];
					currentLayers.Add(val2, value);
				}
			}
			portraitPartsDisplay.UpdateValues(currentLayers, CurrentGender, playableUnit, layerMaterials);
			UpdateBackgroundTexture();
			HairLayer val3 = LayerManagement.GetTheseLayers<HairLayer>(dictionary[(E_LayerType)1].Gender, (E_LayerType)1, true)[dictionary[(E_LayerType)1].LayerIndex];
			RefreshCodePanel(dictionary);
			playableUnit.FaceId = val3.FaceId;
			playableUnit.Gender = (((int)CurrentIndexByLayerType[(E_LayerType)1].Gender == 0) ? "Male" : "Female");
			playableUnit.PlayableUnitView.RefreshBodyParts(forceFullRefresh: true);
			unitAvatarImage.sprite = playableUnit.UiSprite;
		}
		if (InputManager.GetButtonDown(29) || (InputManager.GetButtonDown(80) && !openedThisFrame))
		{
			if ((Object)(object)currentJoystickSelectedHandler == (Object)null || !currentJoystickSelectedHandler.IsDropdownOpen)
			{
				if (PortraitCodePanel.IsEditingCode)
				{
					PortraitCodePanel.PortraitCodePopup.Close();
				}
				else if (RenameHeader.IsEditingName)
				{
					RenameHeader.RenamePopup.Close();
				}
				else
				{
					OnResetButtonClicked();
					Close();
				}
			}
		}
		else if (InputManager.GetButtonDown(66) || InputManager.GetButtonDown(112))
		{
			if (PortraitCodePanel.IsEditingCode)
			{
				PortraitCodePanel.PortraitCodePopup.OnValidateButtonClicked();
			}
			else if (RenameHeader.IsEditingName)
			{
				RenameHeader.RenamePopup.OnValidateButtonClicked();
			}
			else
			{
				OnConfirmButtonClicked();
			}
		}
		else if (InputManager.GetButtonDown(113))
		{
			if ((Object)(object)currentJoystickSelectedHandler != (Object)null)
			{
				currentJoystickSelectedHandler.IncreaseCurrentValue();
			}
		}
		else if (InputManager.GetButtonDown(114) && (Object)(object)currentJoystickSelectedHandler != (Object)null)
		{
			currentJoystickSelectedHandler.DecreaseCurrentValue();
		}
		if (openedThisFrame)
		{
			openedThisFrame = false;
		}
	}

	private void RefreshCodePanel(Dictionary<E_LayerType, CodeLayerData> indexes)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		CodeFooterData footerData = new CodeFooterData(CurrentGender, PortraitAPIManager.PortraitAPIVersion);
		CodeData val = new CodeData();
		val.SetFooterData(footerData);
		val.SetSectionDatas(indexes);
		val.SetColorDatas(CurrentIndexByColorType);
		portraitCodePanel.Refresh(val);
	}

	private void UpdateColorHandler(E_ColorTypes colorType)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Invalid comparison between Unknown and I4
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Invalid comparison between Unknown and I4
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		E_ColorTypes val = colorType;
		if ((int)val > 2)
		{
			if ((int)val == 3)
			{
				colorHandlers.Find((ColorHandler x) => x.ColorType == colorType).Refresh(PlayableUnitDatabase.PortraitBackgroundColors, clearOptions: false);
				((Graphic)imageBG).color = PlayableUnitDatabase.PortraitBackgroundColors[CurrentIndexByColorType[colorType].Index]._Color;
			}
			return;
		}
		colorHandlers.Find((ColorHandler x) => x.ColorType == colorType).Refresh();
		SwapColorsForPalette(AllPalettesByColorType[colorType].ToList()[CurrentIndexByColorType[colorType].Index].Value, ref currentColorTexture);
		currentColorTexture.Apply();
		foreach (KeyValuePair<E_LayerKind, Material> layerMaterial in layerMaterials)
		{
			layerMaterial.Value.SetTexture(SwapTex, (Texture)(object)currentColorTexture);
		}
	}

	private void UpdateCurrentIndex(E_LayerType layerType, E_Gender gender, int valueToSet)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		if ((int)gender > 1)
		{
			if ((int)gender == 2)
			{
				if (valueToSet >= LayerManagement.GetTheseLayers<SimpleLayer>((E_Gender)0, layerType, true).Count)
				{
					CurrentIndexByLayerType[layerType].SetIndex(valueToSet - LayerManagement.GetTheseLayers<SimpleLayer>((E_Gender)0, layerType, true).Count);
					CurrentIndexByLayerType[layerType].SetGender((E_Gender)1);
				}
				else
				{
					CurrentIndexByLayerType[layerType].SetIndex(Mathf.Clamp(valueToSet, -1, LayerManagement.GetTheseLayers<SimpleLayer>(gender, layerType, true).Count - 1));
					CurrentIndexByLayerType[layerType].SetGender((E_Gender)0);
				}
			}
		}
		else
		{
			CurrentIndexByLayerType[layerType].SetIndex(Mathf.Clamp(valueToSet, -1, LayerManagement.GetTheseLayers<SimpleLayer>(gender, layerType, true).Count - 1));
			CurrentIndexByLayerType[layerType].SetGender(gender);
		}
	}

	private void SetInitialsValues(PlayableUnit playableUnit, CodeData codeData)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		initialFaceId = playableUnit.FaceId;
		initialCode = ((object)playableUnit.PortraitCodeData).ToString();
		initialGender = codeData.CodeFooterData.Gender;
		initialName = playableUnit.PlayableUnitName;
	}

	private void SetNewTextures()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		Color[,] array = LayerManagement.CreateOneTextureFromMultipleTextures(currentLayers, default(Color), default(Color));
		Texture2D val = BackgroundCreater.WriteTexture(LayerManagement.CreateOneTextureFromMultipleTextures(currentLayers, Color.white, Color.black));
		Texture2D val2 = PortraitAPIManager.CreateEmptyTexture();
		TexturesData texturesData = PortraitAPIManager.GetTexturesData();
		for (int i = 0; i < texturesData.Width; i++)
		{
			for (int j = 0; j < texturesData.Height; j++)
			{
				val2.SetPixel(i, j, array[i, j]);
			}
		}
		val2.Apply();
		Sprite portraitSprite = Sprite.Create(val2, new Rect(0f, 0f, (float)texturesData.Width, (float)texturesData.Height), texturesData.PivotPoint, (float)texturesData.PixelPerUnit);
		Sprite portraitBackgroundSprite = Sprite.Create(val, new Rect(0f, 0f, (float)((Texture)val).width, (float)((Texture)val).height), texturesData.PivotPoint, (float)texturesData.PixelPerUnit);
		playableUnit.PortraitSprite = portraitSprite;
		playableUnit.PortraitBackgroundSprite = portraitBackgroundSprite;
	}

	private void SwapColor(int index, Color color, ref Texture2D colorSwapTex)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		colorSwapTex.SetPixel(index, 0, color);
	}

	private void SwapColorsForPalette(ColorSwapPaletteDefinition paletteDefinition, ref Texture2D colorSwapTex)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (paletteDefinition != null)
		{
			int i = 0;
			for (int count = paletteDefinition.ColorSwapDefinitions.Count; i < count; i++)
			{
				SwapColor(paletteDefinition.ColorSwapDefinitions[i].Index, paletteDefinition.ColorSwapDefinitions[i].OutputColor, ref colorSwapTex);
			}
		}
	}
}
