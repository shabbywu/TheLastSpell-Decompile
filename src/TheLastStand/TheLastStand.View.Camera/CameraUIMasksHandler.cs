using System.Collections.Generic;
using Sirenix.OdinInspector;
using TPLib.Log;
using TheLastStand.Framework.Maths;
using TheLastStand.Model.TileMap;
using UnityEngine;

namespace TheLastStand.View.Camera;

public class CameraUIMasksHandler : SerializedMonoBehaviour
{
	public class Polygon
	{
		private readonly Camera camera;

		private readonly Canvas relatedCanvas;

		private readonly Transform[] cornersTransforms;

		private readonly List<Vector2> cornersPositions = new List<Vector2>();

		private Vector2 centre = Vector2.zero;

		private float boundingCircleInSqr;

		private float boundingCircleOutSqr;

		public Vector2[] CornersWorldSpace { get; }

		public Polygon(Transform[] corners, Canvas relatedCanvas, Camera camera)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			this.camera = camera;
			cornersTransforms = corners;
			CornersWorldSpace = (Vector2[])(object)new Vector2[corners.Length];
			this.relatedCanvas = relatedCanvas;
			Refresh();
		}

		public static float GetFurthestCornerToCenterSqr(Vector2[] polygon, Vector3 centre)
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			float num = 0f;
			for (int num2 = polygon.Length - 1; num2 >= 0; num2--)
			{
				Vector2 val = polygon[num2] - Vector2.op_Implicit(centre);
				float sqrMagnitude = ((Vector2)(ref val)).sqrMagnitude;
				if (num < sqrMagnitude)
				{
					num = sqrMagnitude;
				}
			}
			return num;
		}

		public bool IsPointInside(Vector2 point)
		{
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0120: Unknown result type (might be due to invalid IL or missing references)
			//IL_0143: Unknown result type (might be due to invalid IL or missing references)
			//IL_014d: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_015e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0168: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_0177: Unknown result type (might be due to invalid IL or missing references)
			//IL_0185: Unknown result type (might be due to invalid IL or missing references)
			//IL_018a: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)relatedCanvas != (Object)null && !((Behaviour)relatedCanvas).enabled)
			{
				return false;
			}
			if (cornersTransforms != null && (Object)(object)cornersTransforms[0] != (Object)null && !((Component)cornersTransforms[0]).gameObject.activeInHierarchy)
			{
				return false;
			}
			Refresh();
			Vector2 val = point - centre;
			if (((Vector2)(ref val)).sqrMagnitude < boundingCircleInSqr && boundingCircleInSqr < float.PositiveInfinity)
			{
				CLoggerManager.Log((object)("Point is hidden (canvas: " + (((Object)(object)relatedCanvas != (Object)null) ? ((Object)relatedCanvas).name : "None") + ")."), (Object)(object)(((Object)(object)relatedCanvas != (Object)null) ? ((Component)relatedCanvas).gameObject : null), (LogType)3, (CLogLevel)0, true, "StaticLog", false);
				return true;
			}
			val = point - centre;
			if (((Vector2)(ref val)).sqrMagnitude > boundingCircleOutSqr)
			{
				return false;
			}
			cornersPositions.Clear();
			cornersPositions.AddRange(CornersWorldSpace);
			cornersPositions.Add(CornersWorldSpace[^1]);
			int num = 0;
			for (int num2 = CornersWorldSpace.Length - 1; num2 >= 0; num2--)
			{
				if (cornersPositions[num2].x <= point.x)
				{
					if (cornersPositions[num2 + 1].x > point.x && Maths.IsPointLeftToEdge(cornersPositions[num2], cornersPositions[num2 + 1], point) > 0)
					{
						num++;
					}
				}
				else if (cornersPositions[num2 + 1].x <= point.x && Maths.IsPointLeftToEdge(cornersPositions[num2], cornersPositions[num2 + 1], point) < 0)
				{
					num--;
				}
			}
			if (num != 0)
			{
				CLoggerManager.Log((object)("Point is hidden (canvas: " + (((Object)(object)relatedCanvas != (Object)null) ? ((Object)relatedCanvas).name : "None") + ")."), (Object)(object)(((Object)(object)relatedCanvas != (Object)null) ? ((Component)relatedCanvas).gameObject : null), (LogType)3, (CLogLevel)0, true, "StaticLog", false);
			}
			return num != 0;
		}

		private void Refresh()
		{
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			for (int i = 0; i < cornersTransforms.Length; i++)
			{
				if ((Object)(object)cornersTransforms[i] == (Object)null)
				{
					CLoggerManager.Log((object)("Missing RectTransform found while refreshing Polygon corners (canvas: " + (((Object)(object)relatedCanvas != (Object)null) ? ((Object)relatedCanvas).name : "None") + ")."), (Object)(object)(((Object)(object)relatedCanvas != (Object)null) ? ((Component)relatedCanvas).gameObject : null), (LogType)2, (CLogLevel)1, true, "StaticLog", false);
					break;
				}
				CornersWorldSpace[i] = Vector2.op_Implicit(camera.ScreenToWorldPoint(cornersTransforms[i].position));
			}
			centre = Maths.ComputePolygonCentre(CornersWorldSpace);
			Vector2 val = centre - Maths.GetClosestPointOnPolygon(CornersWorldSpace, centre);
			boundingCircleInSqr = ((Vector2)(ref val)).sqrMagnitude;
			boundingCircleOutSqr = GetFurthestCornerToCenterSqr(CornersWorldSpace, Vector2.op_Implicit(centre));
		}
	}

	[SerializeField]
	private Camera mainCamera;

	[SerializeField]
	private GridLayout grid;

	private readonly List<Polygon> polygons = new List<Polygon>();

	public Camera Camera => mainCamera;

	public void RegisterMask(CameraUIMask mask)
	{
		polygons.Add(new Polygon(mask.Vertices, mask.Canvas, Camera));
	}

	public bool IsPointOffscreen(Vector3 point)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return !IsPointInCameraFrustum(point);
	}

	public bool IsPointOffscreenOrHiddenByUI(Vector3 point)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (IsPointOffscreen(point))
		{
			return true;
		}
		if (polygons == null)
		{
			return false;
		}
		for (int num = polygons.Count - 1; num >= 0; num--)
		{
			if (polygons[num].IsPointInside(Vector2.op_Implicit(point)))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsPointOffscreenOrHiddenByUI(Tile tile)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		return IsPointOffscreenOrHiddenByUI(grid.CellToWorld(new Vector3Int(tile.X, tile.Y, 0)));
	}

	public bool IsPointOffscreenOrHiddenByUI(Vector3 point, Vector3 offset)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		return IsPointOffscreenOrHiddenByUI(point + offset);
	}

	public bool IsPointOffscreenOrHiddenByUI(Tile tile, Vector3 offset)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		return IsPointOffscreenOrHiddenByUI(grid.CellToWorld(new Vector3Int(tile.X, tile.Y, 0)) + offset);
	}

	private bool IsPointInCameraFrustum(Vector3 point, float boundsSize = 0f)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		return GeometryUtility.TestPlanesAABB(GeometryUtility.CalculateFrustumPlanes(mainCamera), new Bounds(point, Vector3.one * boundsSize));
	}
}
