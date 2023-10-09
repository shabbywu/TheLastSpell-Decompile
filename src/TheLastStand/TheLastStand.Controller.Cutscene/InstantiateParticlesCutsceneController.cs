using System.Collections;
using TPLib;
using TPLib.Log;
using TheLastStand.Definition.Cutscene;
using TheLastStand.Framework;
using TheLastStand.Manager;
using TheLastStand.View.Cutscene;
using TheLastStand.View.TileMap;
using UnityEngine;

namespace TheLastStand.Controller.Cutscene;

public class InstantiateParticlesCutsceneController : CutsceneController
{
	public InstantiateParticlesCutsceneDefinition InstantiateParticlesCutsceneDefinition => base.CutsceneDefinition as InstantiateParticlesCutsceneDefinition;

	public InstantiateParticlesCutsceneController(ICutsceneDefinition cutsceneDefinition)
		: base(cutsceneDefinition)
	{
	}

	public override IEnumerator Play(CutsceneData cutsceneData)
	{
		if (cutsceneData.Tile == null)
		{
			((CLogger<CutsceneManager>)TPSingleton<CutsceneManager>.Instance).LogError((object)"Tried to play a InstantiateParticlesCutsceneController with a null tile.", (CLogLevel)1, true, true);
			yield break;
		}
		GameObject pooledGameObject = ObjectPooler.GetPooledGameObject(InstantiateParticlesCutsceneDefinition.ParticlesId, ResourcePooler.LoadOnce<GameObject>(InstantiateParticlesCutsceneDefinition.ParticlesPath, failSilently: false));
		if ((Object)(object)pooledGameObject != (Object)null)
		{
			pooledGameObject.transform.position = Vector2.op_Implicit(TileMapView.GetTileCenter(cutsceneData.Tile));
		}
	}
}
