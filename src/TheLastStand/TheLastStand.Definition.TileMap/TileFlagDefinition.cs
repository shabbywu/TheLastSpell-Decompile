using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace TheLastStand.Definition.TileMap;

[CreateAssetMenu(fileName = "Data", menuName = "TLS/TileFlag", order = 1)]
public class TileFlagDefinition : ScriptableObject
{
	public enum E_TileFlagTag
	{
		None,
		EnemyMagnet,
		SpawnerCocoon,
		KillerCocoon,
		SupportCocoon,
		SpawnerBoss,
		FogSpawner,
		Hole,
		HarpyBoss,
		SpawnerBoss1,
		SpawnerBoss2,
		HarpyBossCommander,
		SideHole,
		HarpyBossCommander1,
		HarpyBossCommander2,
		NessieBoss1,
		NessieBoss2,
		NessieBoss3,
		NessieEgg,
		HarpyBoss2,
		ThroneTowerBoss,
		Brazier,
		MonarchBoss,
		Unnamed,
		Unnamed1,
		Zone_N,
		Zone_N_NE,
		Zone_NE,
		Zone_E_NE,
		Zone_E,
		Zone_E_SE,
		Zone_SE,
		Zone_S_SE,
		Zone_S,
		Zone_S_SW,
		Zone_SW,
		Zone_W_SW,
		Zone_W,
		Zone_W_NW,
		Zone_NW,
		Zone_N_NW,
		Schadenfreude1,
		Schadenfreude2,
		DamnedBomb1,
		DamnedBomb2,
		EliteBirth,
		SchadenSide1,
		SchadenSide2,
		FreudeSide1,
		FreudeSide2,
		RBSpawn_Anywhere,
		RBSpawn_Middle,
		RBSpawn_Sides,
		RBSpawn_NearHaven,
		RBSpawn_AnywhereInFogBoundaries,
		RBSpawn_CenterOnly,
		RBSpawn_ParallelLines,
		RBSpawn_NearHavenCenter,
		RBSpawn_NearHavenSides,
		RBSpawn_ParallelLinesLargeNS,
		RBSpawn_ParallelLinesVeryLarge,
		RBSpawn_MiddleLine,
		RBSpawn_ParallelLinesLargeEW,
		RBSpawn_GridAnywhere,
		RBSpawn_GridAnywhereStaggeredRows,
		DamnedBomb3,
		ScytheSword
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct TileFlagTagComparer : IEqualityComparer<E_TileFlagTag>
	{
		public bool Equals(E_TileFlagTag x, E_TileFlagTag y)
		{
			return x == y;
		}

		public int GetHashCode(E_TileFlagTag obj)
		{
			return (int)obj;
		}
	}

	[SerializeField]
	private E_TileFlagTag tileFlagTag;

	[SerializeField]
	private Color debugColor = Color.white;

	public static readonly TileFlagTagComparer SharedTileFlagTagComparer;

	public Color DebugColor => debugColor;

	public E_TileFlagTag TileFlagTag => tileFlagTag;
}
