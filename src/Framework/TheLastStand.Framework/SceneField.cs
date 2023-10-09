using System;
using UnityEngine;

namespace TheLastStand.Framework;

[Serializable]
public class SceneField
{
	[SerializeField]
	private Object m_SceneAsset;

	[SerializeField]
	private string m_SceneName = "";

	public bool IsNull => m_SceneAsset == (Object)null;

	public string SceneName => m_SceneName;

	public static implicit operator string(SceneField sceneField)
	{
		return sceneField.SceneName;
	}
}
