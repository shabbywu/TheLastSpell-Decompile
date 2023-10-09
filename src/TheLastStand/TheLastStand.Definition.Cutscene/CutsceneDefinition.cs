using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TPLib.Log;
using TheLastStand.Database;
using TheLastStand.Framework.Serialization;
using UnityEngine;

namespace TheLastStand.Definition.Cutscene;

public class CutsceneDefinition : TheLastStand.Framework.Serialization.Definition
{
	public string Id { get; private set; }

	public List<ICutsceneDefinition> CutsceneElements { get; private set; }

	public bool ContainsInitVisuals => CutsceneElements.Any((ICutsceneDefinition x) => x is InitUnitVisualsCutsceneDefinition);

	public bool ShouldHideHUD { get; private set; } = true;


	public bool ShouldSetState { get; private set; } = true;


	public CutsceneDefinition(XContainer xContainer)
		: base(xContainer)
	{
	}

	public override void Deserialize(XContainer container)
	{
		XElement val = (XElement)(object)((container is XElement) ? container : null);
		CutsceneDefinition cutsceneDefinition = null;
		XAttribute val2 = val.Attribute(XName.op_Implicit("Id"));
		Id = val2.Value;
		XAttribute val3 = val.Attribute(XName.op_Implicit("TemplateId"));
		if (val3 != null)
		{
			cutsceneDefinition = GameDatabase.CutsceneDefinitions[val3.Value];
		}
		if (cutsceneDefinition != null)
		{
			CutsceneElements = cutsceneDefinition.CutsceneElements;
		}
		else
		{
			CutsceneElements = new List<ICutsceneDefinition>();
			foreach (XElement item2 in ((XContainer)val).Elements())
			{
				switch (item2.Name.LocalName)
				{
				case "Bark":
					CutsceneElements.Add(new BarkCutsceneDefinition((XContainer)(object)item2));
					break;
				case "ChangeMusic":
					CutsceneElements.Add(new ChangeMusicCutsceneDefinition((XContainer)(object)item2));
					break;
				case "EvolutiveLevelArtSetActiveCurrentStage":
					CutsceneElements.Add(new EvolutiveLevelArtSetActiveCurrentStageCutsceneDefinition((XContainer)(object)item2));
					break;
				case "EvolutiveLevelArtSetStage":
					CutsceneElements.Add(new EvolutiveLevelArtSetStageCutsceneDefinition((XContainer)(object)item2));
					break;
				case "ExileAllEnemies":
					CutsceneElements.Add(new ExileAllEnemiesCutsceneDefinition((XContainer)(object)item2));
					break;
				case "FadeIn":
					CutsceneElements.Add(new FadeInCutsceneDefinition((XContainer)(object)item2));
					break;
				case "FadeOut":
					CutsceneElements.Add(new FadeOutCutsceneDefinition((XContainer)(object)item2));
					break;
				case "FocusMagicCircle":
					CutsceneElements.Add(new FocusMagicCircleCutsceneDefinition((XContainer)(object)item2));
					break;
				case "FocusTile":
					CutsceneElements.Add(new FocusTileCutsceneDefinition((XContainer)(object)item2));
					break;
				case "FocusUnit":
					CutsceneElements.Add(new FocusUnitCutsceneDefinition((XContainer)(object)item2));
					break;
				case "InitUnitVisuals":
					CutsceneElements.Add(new InitUnitVisualsCutsceneDefinition((XContainer)(object)item2));
					break;
				case "InstantiateParticles":
					CutsceneElements.Add(new InstantiateParticlesCutsceneDefinition((XContainer)(object)item2));
					break;
				case "InvertImage":
					CutsceneElements.Add(new InvertImageCutsceneDefinition((XContainer)(object)item2));
					break;
				case "LookAtCircle":
					CutsceneElements.Add(new LookAtCircleCutsceneDefinition((XContainer)(object)item2));
					break;
				case "OverrideCurrentSpawnWave":
					CutsceneElements.Add(new OverrideCurrentSpawnWaveCutsceneDefinition((XContainer)(object)item2));
					break;
				case "PlayAnimatedCutscene":
					CutsceneElements.Add(new PlayAnimatedCutsceneDefinition((XContainer)(object)item2));
					break;
				case "PlayCamShakeEffect":
					CutsceneElements.Add(new PlayCamShakeEffectCutsceneDefinition((XContainer)(object)item2));
					break;
				case "PlayDeathAnim":
					CutsceneElements.Add(new PlayDeathAnimCutsceneDefinition((XContainer)(object)item2));
					break;
				case "PlayEnemyTurn":
					CutsceneElements.Add(new PlayEnemyTurnCutsceneDefinition((XContainer)(object)item2));
					break;
				case "PlayMageDeath":
					CutsceneElements.Add(new PlayMageDeathCutsceneDefinition((XContainer)(object)item2));
					break;
				case "PlayMageDeathStep":
					CutsceneElements.Add(new PlayMageDeathStepCutsceneDefinition((XContainer)(object)item2));
					break;
				case "PlayPillarsCutscene":
				{
					PlayPillarsCutsceneDefinition item = new PlayPillarsCutsceneDefinition((XContainer)(object)item2);
					CutsceneElements.Add(item);
					break;
				}
				case "PlayRippleEffect":
					CutsceneElements.Add(new PlayRippleEffectCutsceneDefinition((XContainer)(object)item2));
					break;
				case "PlaySealAnticipation":
					CutsceneElements.Add(new PlaySealAnticipationCutsceneDefinition((XContainer)(object)item2));
					break;
				case "PlaySealDestruction":
					CutsceneElements.Add(new PlaySealDestructionCutsceneDefinition((XContainer)(object)item2));
					break;
				case "PlayFX":
					CutsceneElements.Add(new PlayFXCutsceneDefinition((XContainer)(object)item2));
					break;
				case "StopMagicCircleIdle":
					CutsceneElements.Add(new StopMagicCircleIdleCutsceneDefinition((XContainer)(object)item2));
					break;
				case "StopMusic":
					CutsceneElements.Add(new StopMusicCutsceneDefinition((XContainer)(object)item2));
					break;
				case "PlaySound":
					CutsceneElements.Add(new PlaySoundCutsceneDefinition((XContainer)(object)item2));
					break;
				case "Wait":
					CutsceneElements.Add(new WaitCutsceneDefinition((XContainer)(object)item2));
					break;
				case "IncreaseFog":
					CutsceneElements.Add(new IncreaseFogCutsceneDefinition((XContainer)(object)item2));
					break;
				case "ToggleHUD":
					CutsceneElements.Add(new ToggleHUDCutsceneDefinition((XContainer)(object)item2));
					break;
				case "Zoom":
					CutsceneElements.Add(new ZoomCutsceneDefinition((XContainer)(object)item2));
					break;
				case "CommanderPlayFadeOutAnim":
					CutsceneElements.Add(new PlayCommanderFadeOutAnimCutsceneDefinition((XContainer)(object)item2));
					break;
				default:
					CLoggerManager.Log((object)(item2.Name.LocalName + " is not an implemented cutscene element."), (LogType)0, (CLogLevel)1, true, "StaticLog", false);
					break;
				}
			}
		}
		XAttribute val4 = val.Attribute(XName.op_Implicit("ShouldHideHUD"));
		if (val4 != null && val4.Value != null)
		{
			if (bool.TryParse(val4.Value, out var result))
			{
				ShouldHideHUD = result;
			}
			else
			{
				CLoggerManager.Log((object)("Unable to parse " + val4.Value + " into bool"), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
			}
		}
		else if (cutsceneDefinition != null)
		{
			ShouldHideHUD = cutsceneDefinition.ShouldHideHUD;
		}
		XAttribute val5 = val.Attribute(XName.op_Implicit("ShouldSetState"));
		if (val5 != null && val5.Value != null)
		{
			if (bool.TryParse(val5.Value, out var result2))
			{
				ShouldSetState = result2;
			}
			else
			{
				CLoggerManager.Log((object)("Unable to parse " + val5.Value + " into bool"), (LogType)0, (CLogLevel)2, true, "StaticLog", false);
			}
		}
		else if (cutsceneDefinition != null)
		{
			ShouldSetState = cutsceneDefinition.ShouldSetState;
		}
	}
}
