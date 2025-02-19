using System;
using System.Collections.Generic;
using Pathfinding.Serialization;
using Pathfinding.Serialization.JsonFx;
using Pathfinding.Util;
using UnityEngine;

namespace Pathfinding;

[JsonOptIn]
public class GridGraph : NavGraph, IUpdatableGraph, IRaycastableGraph
{
	public class TextureData
	{
		public enum ChannelUse
		{
			None,
			Penalty,
			Position,
			WalkablePenalty
		}

		public bool enabled;

		public Texture2D source;

		public float[] factors = new float[3];

		public ChannelUse[] channels = new ChannelUse[3];

		private Color32[] data;

		public void Initialize()
		{
			if (!enabled || !(source != null))
			{
				return;
			}
			for (int i = 0; i < channels.Length; i++)
			{
				if (channels[i] != 0)
				{
					try
					{
						data = source.GetPixels32();
						break;
					}
					catch (UnityException ex)
					{
						Debug.LogWarning(ex.ToString());
						data = null;
						break;
					}
				}
			}
		}

		public void Apply(GridNode node, int x, int z)
		{
			if (enabled && data != null && x < source.width && z < source.height)
			{
				Color32 color = data[z * source.width + x];
				if (channels[0] != 0)
				{
					ApplyChannel(node, x, z, color.r, channels[0], factors[0]);
				}
				if (channels[1] != 0)
				{
					ApplyChannel(node, x, z, color.g, channels[1], factors[1]);
				}
				if (channels[2] != 0)
				{
					ApplyChannel(node, x, z, color.b, channels[2], factors[2]);
				}
			}
		}

		private void ApplyChannel(GridNode node, int x, int z, int value, ChannelUse channelUse, float factor)
		{
			switch (channelUse)
			{
			case ChannelUse.Penalty:
				node.Penalty += (uint)Mathf.RoundToInt((float)value * factor);
				break;
			case ChannelUse.Position:
				node.position = GridNode.GetGridGraph(node.GraphIndex).GetNodePosition(node.NodeInGridIndex, Mathf.RoundToInt((float)value * factor * 1000f));
				break;
			case ChannelUse.WalkablePenalty:
				if (value == 0)
				{
					node.Walkable = false;
				}
				else
				{
					node.Penalty += (uint)Mathf.RoundToInt((float)(value - 1) * factor);
				}
				break;
			}
		}
	}

	public const int getNearestForceOverlap = 2;

	public int width;

	public int depth;

	[JsonMember]
	public float aspectRatio = 1f;

	[JsonMember]
	public float isometricAngle;

	[JsonMember]
	public Vector3 rotation;

	public Bounds bounds;

	[JsonMember]
	public Vector3 center;

	[JsonMember]
	public Vector2 unclampedSize;

	[JsonMember]
	public float nodeSize = 1f;

	[JsonMember]
	public GraphCollision collision;

	[JsonMember]
	public float maxClimb = 0.4f;

	[JsonMember]
	public int maxClimbAxis = 1;

	[JsonMember]
	public float maxSlope = 90f;

	protected float cosMaxSlope;

	[JsonMember]
	public int erodeIterations;

	[JsonMember]
	public bool erosionUseTags;

	[JsonMember]
	public int erosionFirstTag = 1;

	[JsonMember]
	public bool autoLinkGrids;

	[JsonMember]
	public float autoLinkDistLimit = 10f;

	[JsonMember]
	public NumNeighbours neighbours = NumNeighbours.Eight;

	[JsonMember]
	public bool cutCorners = true;

	[JsonMember]
	public float penaltyPositionOffset;

	[JsonMember]
	public bool penaltyPosition;

	[JsonMember]
	public float penaltyPositionFactor = 1f;

	[JsonMember]
	public bool penaltyAngle;

	[JsonMember]
	public float penaltyAngleFactor = 100f;

	[JsonMember]
	public float penaltyAnglePower = 1f;

	[JsonMember]
	public bool useJumpPointSearch;

	[JsonMember]
	public TextureData textureData = new TextureData();

	public Vector2 size;

	[NonSerialized]
	public readonly int[] neighbourOffsets = new int[8];

	[NonSerialized]
	public readonly uint[] neighbourCosts = new uint[8];

	[NonSerialized]
	public readonly int[] neighbourXOffsets = new int[8];

	[NonSerialized]
	public readonly int[] neighbourZOffsets = new int[8];

	public Matrix4x4 boundsMatrix;

	public Matrix4x4 boundsMatrix2;

	public int scans;

	public GridNode[] nodes;

	[NonSerialized]
	protected int[] corners;

	private bool _bClearanceDirty;

	public virtual bool uniformWidthDepthGrid => true;

	public bool useRaycastNormal => Math.Abs(90f - maxSlope) > float.Epsilon;

	public int Width
	{
		get
		{
			return width;
		}
		set
		{
			width = value;
		}
	}

	public int Depth
	{
		get
		{
			return depth;
		}
		set
		{
			depth = value;
		}
	}

	public GridGraph()
	{
		unclampedSize = new Vector2(10f, 10f);
		nodeSize = 1f;
		collision = new GraphCollision();
		cosMaxSlope = Mathf.Cos(maxSlope * ((float)Math.PI / 180f));
	}

	public override bool IntersectWithGraph(ref Bounds bound)
	{
		return new Bounds(new Vector3(center.x, collision.fromHeight * 0.5f, center.z), new Vector3(width, collision.fromHeight, depth)).Intersects(bound);
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		RemoveGridGraphFromStatic();
	}

	public void RemoveGridGraphFromStatic()
	{
		GridNode.SetGridGraph(AstarPath.active.astarData.GetGraphIndex(this), null);
	}

	public override void GetNodes(GraphNodeDelegateCancelable del)
	{
		if (nodes != null)
		{
			for (int i = 0; i < nodes.Length && del(nodes[i]); i++)
			{
			}
		}
	}

	public Int3 GetNodePosition(int index, int yOffset)
	{
		int num = index / Width;
		int num2 = index - num * Width;
		return (Int3)matrix.MultiplyPoint3x4(new Vector3((float)num2 + 0.5f, (float)yOffset * 0.001f, (float)num + 0.5f));
	}

	public uint GetConnectionCost(int dir)
	{
		return neighbourCosts[dir];
	}

	public GridNode GetNodeConnection(GridNode node, int dir)
	{
		if (!node.GetConnectionInternal(dir))
		{
			return null;
		}
		if (!node.EdgeNode)
		{
			if (node.NodeInGridIndex + neighbourOffsets[dir] < 0 || node.NodeInGridIndex + neighbourOffsets[dir] >= nodes.Length)
			{
				return null;
			}
			return nodes[node.NodeInGridIndex + neighbourOffsets[dir]];
		}
		int nodeInGridIndex = node.NodeInGridIndex;
		int num = nodeInGridIndex / Width;
		int x = nodeInGridIndex - num * Width;
		return GetNodeConnection(nodeInGridIndex, x, num, dir);
	}

	public bool HasNodeConnection(GridNode node, int dir)
	{
		if (!node.GetConnectionInternal(dir))
		{
			return false;
		}
		if (!node.EdgeNode)
		{
			return true;
		}
		int nodeInGridIndex = node.NodeInGridIndex;
		int num = nodeInGridIndex / Width;
		int x = nodeInGridIndex - num * Width;
		return HasNodeConnection(nodeInGridIndex, x, num, dir);
	}

	public void SetNodeConnection(GridNode node, int dir, bool value)
	{
		int nodeInGridIndex = node.NodeInGridIndex;
		int num = nodeInGridIndex / Width;
		int x = nodeInGridIndex - num * Width;
		SetNodeConnection(nodeInGridIndex, x, num, dir, value);
	}

	private GridNode GetNodeConnection(int index, int x, int z, int dir)
	{
		if (!nodes[index].GetConnectionInternal(dir))
		{
			return null;
		}
		int num = x + neighbourXOffsets[dir];
		if (num < 0 || num >= Width)
		{
			return null;
		}
		int num2 = z + neighbourZOffsets[dir];
		if (num2 < 0 || num2 >= Depth)
		{
			return null;
		}
		int num3 = index + neighbourOffsets[dir];
		return nodes[num3];
	}

	public void SetNodeConnection(int index, int x, int z, int dir, bool value)
	{
		nodes[index].SetConnectionInternal(dir, value);
	}

	public bool HasNodeConnection(int index, int x, int z, int dir)
	{
		if (!nodes[index].GetConnectionInternal(dir))
		{
			return false;
		}
		int num = x + neighbourXOffsets[dir];
		if (num < 0 || num >= Width)
		{
			return false;
		}
		int num2 = z + neighbourZOffsets[dir];
		if (num2 < 0 || num2 >= Depth)
		{
			return false;
		}
		return true;
	}

	public void UpdateSizeFromWidthDepth()
	{
		unclampedSize = new Vector2(width, depth) * nodeSize;
		GenerateMatrix();
	}

	public void GenerateMatrix()
	{
		size = unclampedSize;
		size.x *= Mathf.Sign(size.x);
		size.y *= Mathf.Sign(size.y);
		nodeSize = Mathf.Clamp(nodeSize, size.x / 1024f, float.PositiveInfinity);
		nodeSize = Mathf.Clamp(nodeSize, size.y / 1024f, float.PositiveInfinity);
		size.x = ((!(size.x < nodeSize)) ? size.x : nodeSize);
		size.y = ((!(size.y < nodeSize)) ? size.y : nodeSize);
		Matrix4x4 matrix4x = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 45f, 0f), Vector3.one);
		matrix4x = Matrix4x4.Scale(new Vector3(Mathf.Cos((float)Math.PI / 180f * isometricAngle), 1f, 1f)) * matrix4x;
		matrix4x = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, -45f, 0f), Vector3.one) * matrix4x;
		boundsMatrix = Matrix4x4.TRS(center, Quaternion.Euler(rotation), new Vector3(aspectRatio, 1f, 1f)) * matrix4x;
		width = Mathf.FloorToInt(size.x / nodeSize);
		depth = Mathf.FloorToInt(size.y / nodeSize);
		if (Mathf.Approximately(size.x / nodeSize, Mathf.CeilToInt(size.x / nodeSize)))
		{
			width = Mathf.CeilToInt(size.x / nodeSize);
		}
		if (Mathf.Approximately(size.y / nodeSize, Mathf.CeilToInt(size.y / nodeSize)))
		{
			depth = Mathf.CeilToInt(size.y / nodeSize);
		}
		Matrix4x4 matrix4x2 = Matrix4x4.TRS(boundsMatrix.MultiplyPoint3x4(-new Vector3(size.x, 0f, size.y) * 0.5f), Quaternion.Euler(rotation), new Vector3(nodeSize * aspectRatio, 1f, nodeSize)) * matrix4x;
		SetMatrix(matrix4x2);
	}

	public override NNInfo GetNearest(Vector3 position, NNConstraint constraint, GraphNode hint)
	{
		if (nodes == null || depth * width != nodes.Length)
		{
			return default(NNInfo);
		}
		position = inverseMatrix.MultiplyPoint3x4(position);
		float num = position.x - 0.5f;
		float num2 = position.z - 0.5f;
		int num3 = Mathf.Clamp(Mathf.RoundToInt(num), 0, width - 1);
		int num4 = Mathf.Clamp(Mathf.RoundToInt(num2), 0, depth - 1);
		GridNode gridNode = nodes[width * num4 + num3];
		float y = inverseMatrix.MultiplyPoint3x4((Vector3)gridNode.position).y;
		Vector3 clampedPosition = matrix.MultiplyPoint3x4(new Vector3(Mathf.Clamp(num, (float)num3 - 0.5f, (float)num3 + 0.5f) + 0.5f, y, Mathf.Clamp(num2, (float)num4 - 0.5f, (float)num4 + 0.5f) + 0.5f));
		int num5 = (((constraint.pathWidth & 1) == 0) ? (constraint.pathWidth >> 1) : (constraint.pathWidth - 1 >> 1));
		int num6 = Mathf.Clamp(num3 + num5, 0, width - 1);
		int num7 = Mathf.Clamp(num4 + num5, 0, depth - 1);
		gridNode = nodes[width * num7 + num6];
		NNInfo result = new NNInfo(gridNode);
		result.clampedPosition = clampedPosition;
		return result;
	}

	public override NNInfo GetNearestForce(Vector3 position, NNConstraint constraint)
	{
		if (nodes == null || depth * width != nodes.Length)
		{
			return default(NNInfo);
		}
		Vector3 vector = position;
		position = inverseMatrix.MultiplyPoint3x4(position);
		float num = position.x - 0.5f;
		float num2 = position.z - 0.5f;
		int num3 = Mathf.Clamp(Mathf.RoundToInt(num), 0, width - 1);
		int num4 = Mathf.Clamp(Mathf.RoundToInt(num2), 0, depth - 1);
		GridNode gridNode = nodes[width * num4 + num3];
		float y = inverseMatrix.MultiplyPoint3x4((Vector3)gridNode.position).y;
		Vector3 clampedPosition = matrix.MultiplyPoint3x4(new Vector3(Mathf.Clamp(num, (float)num3 - 0.5f, (float)num3 + 0.5f) + 0.5f, y, Mathf.Clamp(num2, (float)num4 - 0.5f, (float)num4 + 0.5f) + 0.5f));
		int num5 = (((constraint.pathWidth & 1) == 0) ? (constraint.pathWidth >> 1) : (constraint.pathWidth - 1 >> 1));
		int num6 = Mathf.Clamp(num3 + num5, 0, width - 1);
		int num7 = Mathf.Clamp(num4 + num5, 0, depth - 1);
		gridNode = nodes[width * num7 + num6];
		GridNode gridNode2 = null;
		float num8 = float.PositiveInfinity;
		int num9 = 2;
		NNInfo result = new NNInfo(null);
		if (constraint.Suitable(gridNode))
		{
			gridNode2 = gridNode;
			num8 = ((Vector3)gridNode2.position - vector).sqrMagnitude;
			clampedPosition = matrix.MultiplyPoint3x4(new Vector3(Mathf.Clamp(num, (float)num3 - 0.5f, (float)num3 + 0.5f) + 0.5f, y, Mathf.Clamp(num2, (float)num4 - 0.5f, (float)num4 + 0.5f) + 0.5f));
		}
		if (gridNode2 != null)
		{
			result.node = gridNode2;
			result.clampedPosition = clampedPosition;
			if (num9 == 0)
			{
				return result;
			}
			num9--;
		}
		float num10 = ((!constraint.constrainDistance) ? float.PositiveInfinity : AstarPath.active.maxNearestNodeDistance);
		float num11 = num10 * num10;
		int num12 = num5;
		int num13 = num5 * width;
		int num14 = 1;
		while (true)
		{
			if (nodeSize * (float)num14 > num10)
			{
				result.node = gridNode2;
				result.clampedPosition = clampedPosition;
				return result;
			}
			bool flag = false;
			int num15 = num3;
			int num16 = num4 + num14;
			int num17 = num16 * width;
			for (num15 = num3 - num14; num15 <= num3 + num14; num15++)
			{
				if (num15 < 0 || num16 < 0 || num15 + num12 >= width || num16 + num12 >= depth)
				{
					continue;
				}
				flag = true;
				gridNode = nodes[num15 + num12 + num17 + num13];
				if (constraint.Suitable(gridNode))
				{
					Vector3 vector2 = (Vector3)nodes[num15 + num17].position;
					float sqrMagnitude = (vector2 - vector).sqrMagnitude;
					if (sqrMagnitude < num8 && sqrMagnitude < num11)
					{
						num8 = sqrMagnitude;
						gridNode2 = gridNode;
						clampedPosition = matrix.MultiplyPoint3x4(new Vector3(Mathf.Clamp(num, (float)num15 - 0.5f, (float)num15 + 0.5f) + 0.5f, inverseMatrix.MultiplyPoint3x4(vector2).y, Mathf.Clamp(num2, (float)num16 - 0.5f, (float)num16 + 0.5f) + 0.5f));
					}
				}
			}
			num16 = num4 - num14;
			num17 = num16 * width;
			for (num15 = num3 - num14; num15 <= num3 + num14; num15++)
			{
				if (num15 < 0 || num16 < 0 || num15 + num12 >= width || num16 + num12 >= depth)
				{
					continue;
				}
				flag = true;
				gridNode = nodes[num15 + num12 + num17 + num13];
				if (constraint.Suitable(gridNode))
				{
					Vector3 vector3 = (Vector3)nodes[num15 + num17].position;
					float sqrMagnitude2 = (vector3 - vector).sqrMagnitude;
					if (sqrMagnitude2 < num8 && sqrMagnitude2 < num11)
					{
						num8 = sqrMagnitude2;
						gridNode2 = gridNode;
						clampedPosition = matrix.MultiplyPoint3x4(new Vector3(Mathf.Clamp(num, (float)num15 - 0.5f, (float)num15 + 0.5f) + 0.5f, inverseMatrix.MultiplyPoint3x4(vector3).y, Mathf.Clamp(num2, (float)num16 - 0.5f, (float)num16 + 0.5f) + 0.5f));
					}
				}
			}
			num15 = num3 - num14;
			num16 = num4 - num14 + 1;
			for (num16 = num4 - num14 + 1; num16 <= num4 + num14 - 1; num16++)
			{
				if (num15 < 0 || num16 < 0 || num15 + num12 >= width || num16 + num12 >= depth)
				{
					continue;
				}
				flag = true;
				gridNode = nodes[num15 + num12 + num16 * width + num13];
				if (constraint.Suitable(gridNode))
				{
					Vector3 vector4 = (Vector3)nodes[num15 + num16 * width].position;
					float sqrMagnitude3 = (vector4 - vector).sqrMagnitude;
					if (sqrMagnitude3 < num8 && sqrMagnitude3 < num11)
					{
						num8 = sqrMagnitude3;
						gridNode2 = gridNode;
						clampedPosition = matrix.MultiplyPoint3x4(new Vector3(Mathf.Clamp(num, (float)num15 - 0.5f, (float)num15 + 0.5f) + 0.5f, inverseMatrix.MultiplyPoint3x4(vector4).y, Mathf.Clamp(num2, (float)num16 - 0.5f, (float)num16 + 0.5f) + 0.5f));
					}
				}
			}
			num15 = num3 + num14;
			for (num16 = num4 - num14 + 1; num16 <= num4 + num14 - 1; num16++)
			{
				if (num15 < 0 || num16 < 0 || num15 + num12 >= width || num16 + num12 >= depth)
				{
					continue;
				}
				flag = true;
				gridNode = nodes[num15 + num12 + num16 * width + num13];
				if (constraint.Suitable(gridNode))
				{
					Vector3 vector5 = (Vector3)nodes[num15 + num16 * width].position;
					float sqrMagnitude4 = (vector5 - vector).sqrMagnitude;
					if (sqrMagnitude4 < num8 && sqrMagnitude4 < num11)
					{
						num8 = sqrMagnitude4;
						gridNode2 = gridNode;
						clampedPosition = matrix.MultiplyPoint3x4(new Vector3(Mathf.Clamp(num, (float)num15 - 0.5f, (float)num15 + 0.5f) + 0.5f, inverseMatrix.MultiplyPoint3x4(vector5).y, Mathf.Clamp(num2, (float)num16 - 0.5f, (float)num16 + 0.5f) + 0.5f));
					}
				}
			}
			if (gridNode2 != null)
			{
				if (num9 == 0)
				{
					result.node = gridNode2;
					result.clampedPosition = clampedPosition;
					return result;
				}
				num9--;
			}
			if (!flag)
			{
				break;
			}
			num14++;
		}
		result.node = gridNode2;
		result.clampedPosition = clampedPosition;
		return result;
	}

	public virtual void SetUpOffsetsAndCosts()
	{
		neighbourOffsets[0] = -width;
		neighbourOffsets[1] = 1;
		neighbourOffsets[2] = width;
		neighbourOffsets[3] = -1;
		neighbourOffsets[4] = -width + 1;
		neighbourOffsets[5] = width + 1;
		neighbourOffsets[6] = width - 1;
		neighbourOffsets[7] = -width - 1;
		uint num = (uint)Mathf.RoundToInt(nodeSize * 1000f);
		uint num2 = (uint)Mathf.RoundToInt(nodeSize * Mathf.Sqrt(2f) * 1000f);
		neighbourCosts[0] = num;
		neighbourCosts[1] = num;
		neighbourCosts[2] = num;
		neighbourCosts[3] = num;
		neighbourCosts[4] = num2;
		neighbourCosts[5] = num2;
		neighbourCosts[6] = num2;
		neighbourCosts[7] = num2;
		neighbourXOffsets[0] = 0;
		neighbourXOffsets[1] = 1;
		neighbourXOffsets[2] = 0;
		neighbourXOffsets[3] = -1;
		neighbourXOffsets[4] = 1;
		neighbourXOffsets[5] = 1;
		neighbourXOffsets[6] = -1;
		neighbourXOffsets[7] = -1;
		neighbourZOffsets[0] = -1;
		neighbourZOffsets[1] = 0;
		neighbourZOffsets[2] = 1;
		neighbourZOffsets[3] = 0;
		neighbourZOffsets[4] = -1;
		neighbourZOffsets[5] = 1;
		neighbourZOffsets[6] = 1;
		neighbourZOffsets[7] = -1;
	}

	public override void ScanInternal(OnScanStatus statusCallback)
	{
		AstarPath.OnPostScan = (OnScanDelegate)Delegate.Combine(AstarPath.OnPostScan, new OnScanDelegate(OnPostScan));
		scans++;
		if (nodeSize <= 0f)
		{
			return;
		}
		GenerateMatrix();
		if (width > 1024 || depth > 1024)
		{
			Debug.LogError("One of the grid's sides is longer than 1024 nodes");
			return;
		}
		if (useJumpPointSearch)
		{
			Debug.LogError("Trying to use Jump Point Search, but support for it is not enabled. Please enable it in the inspector (Grid Graph settings).");
		}
		SetUpOffsetsAndCosts();
		int num = AstarPath.active.astarData.GetGraphIndex(this);
		GridNode.SetGridGraph(num, this);
		nodes = new GridNode[width * depth];
		for (int i = 0; i < nodes.Length; i++)
		{
			nodes[i] = new GridNode(active);
			nodes[i].GraphIndex = (uint)num;
		}
		if (collision == null)
		{
			collision = new GraphCollision();
		}
		collision.Initialize(matrix, nodeSize);
		textureData.Initialize();
		for (int j = 0; j < depth; j++)
		{
			for (int k = 0; k < width; k++)
			{
				GridNode gridNode = nodes[j * width + k];
				gridNode.NodeInGridIndex = j * width + k;
				UpdateNodePositionCollision(gridNode, k, j);
				textureData.Apply(gridNode, k, j);
			}
		}
		for (int l = 0; l < depth; l++)
		{
			for (int m = 0; m < width; m++)
			{
				GridNode node = nodes[l * width + m];
				CalculateConnections(nodes, m, l, node);
			}
		}
		ErodeWalkableArea();
		UpdateClearance();
	}

	public virtual void UpdateNodePositionCollision(GridNode node, int x, int z, bool resetPenalty = true)
	{
		node.position = GetNodePosition(node.NodeInGridIndex, 0);
		bool walkable = true;
		RaycastHit hit;
		Vector3 vector = collision.CheckHeight((Vector3)node.position, out hit, out walkable);
		node.position = (Int3)vector;
		if (resetPenalty)
		{
			node.Penalty = initialPenalty;
		}
		if (penaltyPosition && resetPenalty)
		{
			node.Penalty += (uint)Mathf.RoundToInt(((float)node.position.y - penaltyPositionOffset) * penaltyPositionFactor);
		}
		if (walkable && useRaycastNormal && collision.heightCheck && hit.normal != Vector3.zero)
		{
			float num = Vector3.Dot(hit.normal.normalized, collision.up);
			if (penaltyAngle && resetPenalty)
			{
				node.Penalty += (uint)Mathf.RoundToInt((1f - Mathf.Pow(num, penaltyAnglePower)) * penaltyAngleFactor);
			}
			if (num < cosMaxSlope)
			{
				walkable = false;
			}
		}
		if (walkable)
		{
			node.Walkable = collision.Check(vector);
		}
		else
		{
			node.Walkable = walkable;
		}
		node.WalkableErosion = node.Walkable;
	}

	public void ErodeWalkableAreaIter1(int xmin, int zmin, int xmax, int zmax)
	{
		xmin = ((xmin >= 0) ? ((xmin <= Width) ? xmin : Width) : 0);
		xmax = ((xmax >= 0) ? ((xmax <= Width) ? xmax : Width) : 0);
		zmin = ((zmin >= 0) ? ((zmin <= Depth) ? zmin : Depth) : 0);
		zmax = ((zmax >= 0) ? ((zmax <= Depth) ? zmax : Depth) : 0);
		for (int i = zmin; i < zmax; i++)
		{
			for (int j = xmin; j < xmax; j++)
			{
				GridNode gridNode = nodes[i * Width + j];
				if (!gridNode.Walkable)
				{
					continue;
				}
				bool flag = false;
				for (int k = 0; k < 4; k++)
				{
					if (!HasNodeConnection(gridNode, k))
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					gridNode.Walkable = false;
				}
			}
		}
	}

	public virtual void ErodeWalkableArea()
	{
		ErodeWalkableArea(0, 0, Width, Depth);
	}

	public virtual void ErodeWalkableArea(int xmin, int zmin, int xmax, int zmax)
	{
		xmin = ((xmin >= 0) ? ((xmin <= Width) ? xmin : Width) : 0);
		xmax = ((xmax >= 0) ? ((xmax <= Width) ? xmax : Width) : 0);
		zmin = ((zmin >= 0) ? ((zmin <= Depth) ? zmin : Depth) : 0);
		zmax = ((zmax >= 0) ? ((zmax <= Depth) ? zmax : Depth) : 0);
		if (!erosionUseTags)
		{
			for (int i = 0; i < erodeIterations; i++)
			{
				for (int j = zmin; j < zmax; j++)
				{
					for (int k = xmin; k < xmax; k++)
					{
						GridNode gridNode = nodes[j * Width + k];
						if (!gridNode.Walkable)
						{
							continue;
						}
						bool flag = false;
						for (int l = 0; l < 4; l++)
						{
							if (!HasNodeConnection(gridNode, l))
							{
								flag = true;
								break;
							}
						}
						if (flag)
						{
							gridNode.Walkable = false;
						}
					}
				}
				for (int m = zmin; m < zmax; m++)
				{
					for (int n = xmin; n < xmax; n++)
					{
						GridNode node = nodes[m * Width + n];
						CalculateConnections(nodes, n, m, node);
					}
				}
			}
			return;
		}
		if (erodeIterations + erosionFirstTag > 31)
		{
			Debug.LogError("Too few tags available for " + erodeIterations + " erode iterations and starting with tag " + erosionFirstTag + " (erodeIterations+erosionFirstTag > 31)");
			return;
		}
		if (erosionFirstTag <= 0)
		{
			Debug.LogError("First erosion tag must be greater or equal to 1");
			return;
		}
		for (int num = 0; num < erodeIterations; num++)
		{
			for (int num2 = zmin; num2 < zmax; num2++)
			{
				for (int num3 = xmin; num3 < xmax; num3++)
				{
					GridNode gridNode2 = nodes[num2 * width + num3];
					if (gridNode2.Walkable && gridNode2.Tag >= erosionFirstTag && gridNode2.Tag < erosionFirstTag + num)
					{
						for (int num4 = 0; num4 < 4; num4++)
						{
							GridNode nodeConnection = GetNodeConnection(gridNode2, num4);
							if (nodeConnection != null)
							{
								uint tag = nodeConnection.Tag;
								if (tag > erosionFirstTag + num || tag < erosionFirstTag)
								{
									nodeConnection.Tag = (uint)(erosionFirstTag + num);
								}
							}
						}
					}
					else
					{
						if (!gridNode2.Walkable || num != 0)
						{
							continue;
						}
						bool flag2 = false;
						for (int num5 = 0; num5 < 4; num5++)
						{
							if (!HasNodeConnection(gridNode2, num5))
							{
								flag2 = true;
								break;
							}
						}
						if (flag2)
						{
							gridNode2.Tag = (uint)(erosionFirstTag + num);
						}
					}
				}
			}
		}
	}

	public virtual bool IsValidConnection(GridNode n1, GridNode n2)
	{
		if (!n1.Walkable || !n2.Walkable)
		{
			return false;
		}
		if (maxClimb != 0f && (float)Mathf.Abs(n1.position[maxClimbAxis] - n2.position[maxClimbAxis]) > maxClimb * 1000f)
		{
			return false;
		}
		return true;
	}

	public static void CalculateConnections(GridNode node)
	{
		if (AstarData.GetGraph(node) is GridGraph gridGraph)
		{
			int nodeInGridIndex = node.NodeInGridIndex;
			int x = nodeInGridIndex % gridGraph.width;
			int z = nodeInGridIndex / gridGraph.width;
			gridGraph.CalculateConnections(gridGraph.nodes, x, z, node);
		}
	}

	public virtual void CalculateConnections(GridNode[] nodes, int x, int z, GridNode node)
	{
		node.ResetConnectionsInternal();
		if (!node.Walkable)
		{
			return;
		}
		int nodeInGridIndex = node.NodeInGridIndex;
		if (corners == null)
		{
			corners = new int[4];
		}
		else
		{
			corners[0] = 0;
			corners[1] = 0;
			corners[2] = 0;
			corners[3] = 0;
		}
		int iMaxClimb = (int)(maxClimb * 1000f);
		int i = 0;
		int num = 3;
		for (; i < 4; i++)
		{
			int num2 = x + neighbourXOffsets[i];
			int num3 = z + neighbourZOffsets[i];
			if (num2 >= 0 && num3 >= 0 && num2 < width && num3 < depth)
			{
				GridNode n = nodes[nodeInGridIndex + neighbourOffsets[i]];
				if (node.IsValidConnectionY(n, iMaxClimb))
				{
					node.SetConnectionInternal(i, value: true);
					corners[i]++;
					corners[num]++;
				}
				else
				{
					node.SetConnectionInternal(i, value: false);
				}
			}
			num = i;
		}
		if (neighbours != NumNeighbours.Eight)
		{
			return;
		}
		if (cutCorners)
		{
			for (int j = 0; j < 4; j++)
			{
				if (corners[j] >= 1)
				{
					int num4 = x + neighbourXOffsets[j + 4];
					int num5 = z + neighbourZOffsets[j + 4];
					if (num4 >= 0 && num5 >= 0 && num4 < width && num5 < depth)
					{
						GridNode n = nodes[nodeInGridIndex + neighbourOffsets[j + 4]];
						node.SetConnectionInternal(j + 4, node.IsValidConnectionY(n, iMaxClimb));
					}
				}
			}
			return;
		}
		for (int k = 0; k < 4; k++)
		{
			if (corners[k] == 2)
			{
				GridNode n = nodes[nodeInGridIndex + neighbourOffsets[k + 4]];
				node.SetConnectionInternal(k + 4, node.IsValidConnectionY(n, iMaxClimb));
			}
		}
	}

	public void UpdateClearance()
	{
		int num = neighbourXOffsets[7];
		int num2 = neighbourZOffsets[7];
		int num3 = neighbourOffsets[0];
		int num4 = neighbourOffsets[3];
		int num5 = neighbourOffsets[7];
		int num6 = 0;
		for (int i = 0; i < depth; i++)
		{
			int num7 = 0;
			while (num7 < width)
			{
				GridNode gridNode = nodes[num6];
				if (gridNode.Walkable)
				{
					gridNode.clearance = 1;
					if (num7 + num >= 0 && i + num2 >= 0)
					{
						int nodeInGridIndex = gridNode.NodeInGridIndex;
						int clearance = nodes[nodeInGridIndex + num3].clearance;
						int clearance2 = nodes[nodeInGridIndex + num4].clearance;
						int clearance3 = nodes[nodeInGridIndex + num5].clearance;
						gridNode.clearance = (ushort)(Math.Min(clearance, Math.Min(clearance2, clearance3)) + 1);
					}
				}
				else
				{
					gridNode.clearance = 0;
				}
				num7++;
				num6++;
			}
		}
	}

	private void SetClearanceDirty()
	{
		if (!_bClearanceDirty)
		{
			AstarPath.OnGraphsUpdated = (OnScanDelegate)Delegate.Combine(AstarPath.OnGraphsUpdated, new OnScanDelegate(OnClearanceDirty));
			_bClearanceDirty = true;
		}
	}

	private void ClrClearanceDirty()
	{
		if (_bClearanceDirty)
		{
			AstarPath.OnGraphsUpdated = (OnScanDelegate)Delegate.Remove(AstarPath.OnGraphsUpdated, new OnScanDelegate(OnClearanceDirty));
			_bClearanceDirty = false;
		}
	}

	private void OnClearanceDirty(AstarPath script)
	{
		ClrClearanceDirty();
		UpdateClearance();
	}

	public override Color NodeColor(GraphNode node, PathHandler data)
	{
		if (node is GridNode gridNode)
		{
			Color nodeConnection = AstarColor.NodeConnection;
			return gridNode.clearance switch
			{
				0 => Color.black, 
				1 => Color.blue, 
				2 => Color.green, 
				3 => Color.red, 
				4 => Color.yellow, 
				5 => Color.magenta, 
				_ => Color.white, 
			};
		}
		return base.NodeColor(node, data);
	}

	public void OnPostScan(AstarPath script)
	{
		AstarPath.OnPostScan = (OnScanDelegate)Delegate.Remove(AstarPath.OnPostScan, new OnScanDelegate(OnPostScan));
		if (!autoLinkGrids || autoLinkDistLimit <= 0f)
		{
			return;
		}
		throw new NotSupportedException();
	}

	public override void OnDrawGizmos(bool drawNodes)
	{
		Gizmos.matrix = boundsMatrix;
		Gizmos.color = Color.white;
		Gizmos.DrawWireCube(Vector3.zero, new Vector3(size.x, 0f, size.y));
		Gizmos.matrix = Matrix4x4.identity;
		if (!drawNodes || nodes == null || depth * width != nodes.Length)
		{
			return;
		}
		PathHandler debugPathData = AstarPath.active.debugPathData;
		GridNode gridNode = null;
		for (int i = 0; i < depth; i++)
		{
			for (int j = 0; j < width; j++)
			{
				gridNode = nodes[i * width + j];
				if (!gridNode.Walkable)
				{
					continue;
				}
				Gizmos.color = NodeColor(gridNode, AstarPath.active.debugPathData);
				if (AstarPath.active.showSearchTree && debugPathData != null)
				{
					if (NavGraph.InSearchTree(gridNode, AstarPath.active.debugPath))
					{
						PathNode pathNode = debugPathData.GetPathNode(gridNode);
						if (pathNode != null && pathNode.parent != null)
						{
							Gizmos.DrawLine((Vector3)gridNode.position, (Vector3)pathNode.parent.node.position);
						}
					}
					continue;
				}
				for (int k = 0; k < 8; k++)
				{
					GridNode nodeConnection = GetNodeConnection(gridNode, k);
					if (nodeConnection != null)
					{
						Gizmos.DrawLine((Vector3)gridNode.position, (Vector3)nodeConnection.position);
					}
				}
			}
		}
	}

	public void GetBoundsMinMax(Bounds b, Matrix4x4 matrix, out Vector3 min, out Vector3 max)
	{
		Vector3[] array = new Vector3[8]
		{
			matrix.MultiplyPoint3x4(b.center + new Vector3(b.extents.x, b.extents.y, b.extents.z)),
			matrix.MultiplyPoint3x4(b.center + new Vector3(b.extents.x, b.extents.y, 0f - b.extents.z)),
			matrix.MultiplyPoint3x4(b.center + new Vector3(b.extents.x, 0f - b.extents.y, b.extents.z)),
			matrix.MultiplyPoint3x4(b.center + new Vector3(b.extents.x, 0f - b.extents.y, 0f - b.extents.z)),
			matrix.MultiplyPoint3x4(b.center + new Vector3(0f - b.extents.x, b.extents.y, b.extents.z)),
			matrix.MultiplyPoint3x4(b.center + new Vector3(0f - b.extents.x, b.extents.y, 0f - b.extents.z)),
			matrix.MultiplyPoint3x4(b.center + new Vector3(0f - b.extents.x, 0f - b.extents.y, b.extents.z)),
			matrix.MultiplyPoint3x4(b.center + new Vector3(0f - b.extents.x, 0f - b.extents.y, 0f - b.extents.z))
		};
		min = array[0];
		max = array[0];
		for (int i = 1; i < 8; i++)
		{
			min = Vector3.Min(min, array[i]);
			max = Vector3.Max(max, array[i]);
		}
	}

	public List<GraphNode> GetNodesInArea(Bounds b)
	{
		return GetNodesInArea(b, null);
	}

	public List<GraphNode> GetNodesInArea(GraphUpdateShape shape)
	{
		return GetNodesInArea(shape.GetBounds(), shape);
	}

	private List<GraphNode> GetNodesInArea(Bounds b, GraphUpdateShape shape)
	{
		if (nodes == null || width * depth != nodes.Length)
		{
			return null;
		}
		List<GraphNode> list = ListPool<GraphNode>.Claim();
		GetBoundsMinMax(b, inverseMatrix, out var min, out var max);
		int xmin = Mathf.RoundToInt(min.x - 0.5f);
		int xmax = Mathf.RoundToInt(max.x - 0.5f);
		int ymin = Mathf.RoundToInt(min.z - 0.5f);
		int ymax = Mathf.RoundToInt(max.z - 0.5f);
		IntRect a = new IntRect(xmin, ymin, xmax, ymax);
		IntRect b2 = new IntRect(0, 0, width - 1, depth - 1);
		IntRect intRect = IntRect.Intersection(a, b2);
		for (int i = intRect.xmin; i <= intRect.xmax; i++)
		{
			for (int j = intRect.ymin; j <= intRect.ymax; j++)
			{
				int num = j * width + i;
				GraphNode graphNode = nodes[num];
				if (b.Contains((Vector3)graphNode.position) && (shape == null || shape.Contains((Vector3)graphNode.position)))
				{
					list.Add(graphNode);
				}
			}
		}
		return list;
	}

	public GraphUpdateThreading CanUpdateAsync(GraphUpdateObject o)
	{
		return GraphUpdateThreading.UnityThread;
	}

	public void UpdateAreaInit(GraphUpdateObject o)
	{
	}

	public int UpdateArea(GraphUpdateObject o)
	{
		if (nodes == null || nodes.Length != width * depth)
		{
			Debug.LogWarning("The Grid Graph is not scanned, cannot update area ");
			return 0;
		}
		Bounds b = o.bounds;
		GetBoundsMinMax(b, inverseMatrix, out var min, out var max);
		int xmin = Mathf.RoundToInt(min.x - 0.5f);
		int xmax = Mathf.RoundToInt(max.x - 0.5f);
		int ymin = Mathf.RoundToInt(min.z - 0.5f);
		int ymax = Mathf.RoundToInt(max.z - 0.5f);
		IntRect intRect = new IntRect(xmin, ymin, xmax, ymax);
		IntRect intRect2 = intRect;
		IntRect b2 = new IntRect(0, 0, width - 1, depth - 1);
		IntRect intRect3 = intRect;
		int num = (o.updateErosion ? erodeIterations : 0);
		bool flag = o.updatePhysics || o.modifyWalkability;
		if (o.updatePhysics && !o.modifyWalkability && collision.collisionCheck)
		{
			Vector3 vector = new Vector3(collision.diameter, 0f, collision.diameter) * 0.5f;
			min -= vector * 1.02f;
			max += vector * 1.02f;
			intRect3 = new IntRect(Mathf.RoundToInt(min.x - 0.5f), Mathf.RoundToInt(min.z - 0.5f), Mathf.RoundToInt(max.x - 0.5f), Mathf.RoundToInt(max.z - 0.5f));
			intRect2 = IntRect.Union(intRect3, intRect2);
		}
		if (flag || num > 0)
		{
			intRect2 = intRect2.Expand(num + 1);
		}
		IntRect intRect4 = IntRect.Intersection(intRect2, b2);
		if (intRect4.Height < 0 || intRect4.Width < 0)
		{
			return 0;
		}
		int result = intRect4.Height * intRect4.Width;
		for (int i = intRect4.xmin; i <= intRect4.xmax; i++)
		{
			for (int j = intRect4.ymin; j <= intRect4.ymax; j++)
			{
				o.WillUpdateNode(nodes[j * width + i]);
			}
		}
		if (o.updatePhysics && !o.modifyWalkability)
		{
			collision.Initialize(matrix, nodeSize);
			intRect4 = IntRect.Intersection(intRect3, b2);
			for (int k = intRect4.xmin; k <= intRect4.xmax; k++)
			{
				for (int l = intRect4.ymin; l <= intRect4.ymax; l++)
				{
					int num2 = l * width + k;
					GridNode node = nodes[num2];
					UpdateNodePositionCollision(node, k, l, o.resetPenaltyOnPhysics);
				}
			}
		}
		intRect4 = IntRect.Intersection(intRect, b2);
		for (int m = intRect4.xmin; m <= intRect4.xmax; m++)
		{
			for (int n = intRect4.ymin; n <= intRect4.ymax; n++)
			{
				int num3 = n * width + m;
				GridNode gridNode = nodes[num3];
				if (flag)
				{
					gridNode.Walkable = gridNode.WalkableErosion;
					if (o.bounds.Contains((Vector3)gridNode.position))
					{
						o.Apply(gridNode);
					}
					gridNode.WalkableErosion = gridNode.Walkable;
				}
				else if (o.bounds.Contains((Vector3)gridNode.position))
				{
					o.Apply(gridNode);
				}
			}
		}
		if (flag && num == 0)
		{
			intRect4 = IntRect.Intersection(intRect2, b2);
			for (int num4 = intRect4.xmin; num4 <= intRect4.xmax; num4++)
			{
				for (int num5 = intRect4.ymin; num5 <= intRect4.ymax; num5++)
				{
					int num6 = num5 * width + num4;
					GridNode node2 = nodes[num6];
					CalculateConnections(nodes, num4, num5, node2);
				}
			}
		}
		else if (flag && num > 0)
		{
			IntRect a = IntRect.Union(intRect, intRect3).Expand(num);
			IntRect a2 = a.Expand(num);
			a = IntRect.Intersection(a, b2);
			a2 = IntRect.Intersection(a2, b2);
			for (int num7 = a2.xmin; num7 <= a2.xmax; num7++)
			{
				for (int num8 = a2.ymin; num8 <= a2.ymax; num8++)
				{
					int num9 = num8 * width + num7;
					GridNode gridNode2 = nodes[num9];
					bool walkable = gridNode2.Walkable;
					gridNode2.Walkable = gridNode2.WalkableErosion;
					if (!a.Contains(num7, num8))
					{
						gridNode2.TmpWalkable = walkable;
					}
				}
			}
			for (int num10 = a2.xmin; num10 <= a2.xmax; num10++)
			{
				for (int num11 = a2.ymin; num11 <= a2.ymax; num11++)
				{
					int num12 = num11 * width + num10;
					GridNode node3 = nodes[num12];
					CalculateConnections(nodes, num10, num11, node3);
				}
			}
			ErodeWalkableArea(a2.xmin, a2.ymin, a2.xmax + 1, a2.ymax + 1);
			for (int num13 = a2.xmin; num13 <= a2.xmax; num13++)
			{
				for (int num14 = a2.ymin; num14 <= a2.ymax; num14++)
				{
					if (!a.Contains(num13, num14))
					{
						int num15 = num14 * width + num13;
						GridNode gridNode3 = nodes[num15];
						gridNode3.Walkable = gridNode3.TmpWalkable;
					}
				}
			}
			for (int num16 = a2.xmin; num16 <= a2.xmax; num16++)
			{
				for (int num17 = a2.ymin; num17 <= a2.ymax; num17++)
				{
					CalculateConnections(nodes, num16, num17, nodes[num17 * width + num16]);
				}
			}
		}
		SetClearanceDirty();
		return result;
	}

	public bool Linecast(Vector3 _a, Vector3 _b)
	{
		GraphHitInfo hit;
		return Linecast(_a, _b, null, out hit);
	}

	public bool Linecast(Vector3 _a, Vector3 _b, GraphNode hint)
	{
		GraphHitInfo hit;
		return Linecast(_a, _b, hint, out hit);
	}

	public bool Linecast(Vector3 _a, Vector3 _b, GraphNode hint, out GraphHitInfo hit)
	{
		return Linecast(_a, _b, hint, out hit, null);
	}

	public bool Linecast(Vector3 _a, Vector3 _b, GraphNode hint, out GraphHitInfo hit, List<GraphNode> trace)
	{
		hit = default(GraphHitInfo);
		_a = inverseMatrix.MultiplyPoint3x4(_a);
		_a.x -= 0.5f;
		_a.z -= 0.5f;
		_b = inverseMatrix.MultiplyPoint3x4(_b);
		_b.x -= 0.5f;
		_b.z -= 0.5f;
		if (_a.x < -0.5f || _a.z < -0.5f || _a.x >= (float)width - 0.5f || _a.z >= (float)depth - 0.5f || _b.x < -0.5f || _b.z < -0.5f || _b.x >= (float)width - 0.5f || _b.z >= (float)depth - 0.5f)
		{
			Vector3 vector = new Vector3(-0.5f, 0f, -0.5f);
			Vector3 vector2 = new Vector3(-0.5f, 0f, (float)depth - 0.5f);
			Vector3 vector3 = new Vector3((float)width - 0.5f, 0f, (float)depth - 0.5f);
			Vector3 vector4 = new Vector3((float)width - 0.5f, 0f, -0.5f);
			int num = 0;
			bool intersects = false;
			Vector3 vector5 = Polygon.SegmentIntersectionPoint(vector, vector2, _a, _b, out intersects);
			if (intersects)
			{
				num++;
				if (!Polygon.Left(vector, vector2, _a))
				{
					_a = vector5;
				}
				else
				{
					_b = vector5;
				}
			}
			vector5 = Polygon.SegmentIntersectionPoint(vector2, vector3, _a, _b, out intersects);
			if (intersects)
			{
				num++;
				if (!Polygon.Left(vector2, vector3, _a))
				{
					_a = vector5;
				}
				else
				{
					_b = vector5;
				}
			}
			vector5 = Polygon.SegmentIntersectionPoint(vector3, vector4, _a, _b, out intersects);
			if (intersects)
			{
				num++;
				if (!Polygon.Left(vector3, vector4, _a))
				{
					_a = vector5;
				}
				else
				{
					_b = vector5;
				}
			}
			vector5 = Polygon.SegmentIntersectionPoint(vector4, vector, _a, _b, out intersects);
			if (intersects)
			{
				num++;
				if (!Polygon.Left(vector4, vector, _a))
				{
					_a = vector5;
				}
				else
				{
					_b = vector5;
				}
			}
			if (num == 0)
			{
				return false;
			}
		}
		Vector3 vector6 = _b - _a;
		float magnitude = vector6.magnitude;
		if (magnitude == 0f)
		{
			return false;
		}
		float num2 = 0.2f;
		float num3 = nodeSize * num2;
		num3 -= nodeSize * 0.02f;
		vector6 = vector6 / magnitude * num3;
		int num4 = (int)(magnitude / num3);
		Vector3 vector7 = _a + vector6 * nodeSize * 0.01f;
		GraphNode graphNode = null;
		for (int i = 0; i <= num4; i++)
		{
			Vector3 vector8 = vector7 + vector6 * i;
			int num5 = Mathf.RoundToInt(vector8.x);
			int num6 = Mathf.RoundToInt(vector8.z);
			num5 = ((num5 >= 0) ? ((num5 < width) ? num5 : (width - 1)) : 0);
			num6 = ((num6 >= 0) ? ((num6 < depth) ? num6 : (depth - 1)) : 0);
			GraphNode graphNode2 = nodes[num6 * width + num5];
			if (graphNode2 == graphNode)
			{
				continue;
			}
			if (!graphNode2.Walkable)
			{
				if (i > 0)
				{
					hit.point = matrix.MultiplyPoint3x4(vector7 + vector6 * (i - 1) + new Vector3(0.5f, 0f, 0.5f));
				}
				else
				{
					hit.point = matrix.MultiplyPoint3x4(_a + new Vector3(0.5f, 0f, 0.5f));
				}
				hit.origin = matrix.MultiplyPoint3x4(_a + new Vector3(0.5f, 0f, 0.5f));
				hit.node = graphNode2;
				return true;
			}
			if (i > num4 - 1 && (Mathf.Abs(vector8.x - _b.x) <= 0.50001f || Mathf.Abs(vector8.z - _b.z) <= 0.50001f))
			{
				return false;
			}
			trace?.Add(graphNode2);
			graphNode = graphNode2;
		}
		return false;
	}

	public bool SnappedLinecast(Vector3 _a, Vector3 _b, GraphNode hint, out GraphHitInfo hit)
	{
		hit = default(GraphHitInfo);
		GraphNode node = GetNearest(_a, NNConstraint.None).node;
		GraphNode node2 = GetNearest(_b, NNConstraint.None).node;
		_a = inverseMatrix.MultiplyPoint3x4((Vector3)node.position);
		_a.x -= 0.5f;
		_a.z -= 0.5f;
		_b = inverseMatrix.MultiplyPoint3x4((Vector3)node2.position);
		_b.x -= 0.5f;
		_b.z -= 0.5f;
		Int3 @int = new Int3(Mathf.RoundToInt(_a.x), Mathf.RoundToInt(_a.y), Mathf.RoundToInt(_a.z));
		Int3 int2 = new Int3(Mathf.RoundToInt(_b.x), Mathf.RoundToInt(_b.y), Mathf.RoundToInt(_b.z));
		hit.origin = (Vector3)@int;
		if (!nodes[@int.z * width + @int.x].Walkable)
		{
			hit.node = nodes[@int.z * width + @int.x];
			hit.point = matrix.MultiplyPoint3x4(new Vector3((float)@int.x + 0.5f, 0f, (float)@int.z + 0.5f));
			hit.point.y = ((Vector3)hit.node.position).y;
			return true;
		}
		int num = Mathf.Abs(@int.x - int2.x);
		int num2 = Mathf.Abs(@int.z - int2.z);
		int num3 = 0;
		int num4 = 0;
		num3 = ((@int.x < int2.x) ? 1 : (-1));
		num4 = ((@int.z < int2.z) ? 1 : (-1));
		int num5 = num - num2;
		while (true)
		{
			if (@int.x == int2.x && @int.z == int2.z)
			{
				return false;
			}
			int num6 = num5 * 2;
			int num7 = 0;
			Int3 int3 = @int;
			if (num6 > -num2)
			{
				num5 -= num2;
				num7 = num3;
				int3.x += num3;
			}
			if (num6 < num)
			{
				num5 += num;
				num7 += width * num4;
				int3.z += num4;
			}
			if (num7 == 0)
			{
				break;
			}
			for (int i = 0; i < neighbourOffsets.Length; i++)
			{
				if (neighbourOffsets[i] != num7)
				{
					continue;
				}
				if (CheckConnection(nodes[@int.z * width + @int.x], i))
				{
					if (!nodes[int3.z * width + int3.x].Walkable)
					{
						hit.node = nodes[@int.z * width + @int.x];
						hit.point = matrix.MultiplyPoint3x4(new Vector3((float)@int.x + 0.5f, 0f, (float)@int.z + 0.5f));
						hit.point.y = ((Vector3)hit.node.position).y;
						return true;
					}
					@int = int3;
					break;
				}
				hit.node = nodes[@int.z * width + @int.x];
				hit.point = matrix.MultiplyPoint3x4(new Vector3((float)@int.x + 0.5f, 0f, (float)@int.z + 0.5f));
				hit.point.y = ((Vector3)hit.node.position).y;
				return true;
			}
		}
		Debug.LogError("Offset is zero, this should not happen");
		return false;
	}

	public bool CheckConnection(GridNode node, int dir)
	{
		if (neighbours == NumNeighbours.Eight)
		{
			return HasNodeConnection(node, dir);
		}
		int num = (dir - 4 - 1) & 3;
		int num2 = (dir - 4 + 1) & 3;
		if (!HasNodeConnection(node, num) || !HasNodeConnection(node, num2))
		{
			return false;
		}
		GridNode gridNode = nodes[node.NodeInGridIndex + neighbourOffsets[num]];
		GridNode gridNode2 = nodes[node.NodeInGridIndex + neighbourOffsets[num2]];
		if (!gridNode.Walkable || !gridNode2.Walkable)
		{
			return false;
		}
		if (!HasNodeConnection(gridNode2, num) || !HasNodeConnection(gridNode, num2))
		{
			return false;
		}
		return true;
	}

	public override void SerializeExtraInfo(GraphSerializationContext ctx)
	{
		if (nodes == null)
		{
			ctx.writer.Write(-1);
			return;
		}
		ctx.writer.Write(nodes.Length);
		for (int i = 0; i < nodes.Length; i++)
		{
			nodes[i].SerializeNode(ctx);
		}
	}

	public override void DeserializeExtraInfo(GraphSerializationContext ctx)
	{
		int num = ctx.reader.ReadInt32();
		if (num == -1)
		{
			nodes = null;
			return;
		}
		nodes = new GridNode[num];
		for (int i = 0; i < nodes.Length; i++)
		{
			nodes[i] = new GridNode(active);
			nodes[i].DeserializeNode(ctx);
		}
	}

	public override void PostDeserialization()
	{
		GenerateMatrix();
		SetUpOffsetsAndCosts();
		if (nodes == null || nodes.Length == 0)
		{
			return;
		}
		if (width * depth != nodes.Length)
		{
			Debug.LogWarning("Node data did not match with bounds data. Probably a change to the bounds/width/depth data was made after scanning the graph just prior to saving it. Nodes will be discarded");
			nodes = new GridNode[0];
			return;
		}
		GridNode.SetGridGraph(AstarPath.active.astarData.GetGraphIndex(this), this);
		for (int i = 0; i < depth; i++)
		{
			for (int j = 0; j < width; j++)
			{
				GridNode gridNode = nodes[i * width + j];
				if (gridNode == null)
				{
					Debug.LogError("Deserialization Error : Couldn't cast the node to the appropriate type - GridGenerator. Check the CreateNodes function");
					return;
				}
				gridNode.NodeInGridIndex = i * width + j;
			}
		}
	}
}
