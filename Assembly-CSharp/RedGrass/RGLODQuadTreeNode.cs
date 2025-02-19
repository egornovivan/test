using System.Collections.Generic;
using UnityEngine;

namespace RedGrass;

public class RGLODQuadTreeNode
{
	public int xIndex;

	public int zIndex;

	public int LOD;

	public RGLODQuadTreeNode node1;

	public RGLODQuadTreeNode node2;

	public RGLODQuadTreeNode node3;

	public RGLODQuadTreeNode node4;

	public RGLODQuadTreeNode parent;

	public bool isTail;

	public bool visible;

	private int mTimeStamp;

	private int mTimeStampOutofdate;

	public GameObject gameObject;

	public List<GameObject> oldGos = new List<GameObject>();

	public RGLODQuadTreeNode(int _x, int _z, int _lod)
	{
		xIndex = _x;
		zIndex = _z;
		LOD = _lod;
		visible = true;
	}

	public void UpdateTimeStamp()
	{
		mTimeStampOutofdate++;
	}

	public void SyncTimeStamp()
	{
		mTimeStamp = mTimeStampOutofdate;
	}

	public bool IsOutOfDate()
	{
		return mTimeStamp != mTimeStampOutofdate;
	}

	public List<RGChunk> GetChunks(RGDataSource data)
	{
		List<RGChunk> list = new List<RGChunk>();
		int num = 1 << LOD;
		for (int i = xIndex; i < xIndex + num; i++)
		{
			for (int j = zIndex; j < zIndex + num; j++)
			{
				RGChunk rGChunk = data.Node(i, j);
				if (rGChunk != null)
				{
					list.Add(rGChunk);
				}
			}
		}
		return list;
	}

	public bool HasChild()
	{
		return node1 != null;
	}

	public bool IsDirty(RGDataSource data)
	{
		int num = 1 << LOD;
		for (int i = xIndex; i < xIndex + num; i++)
		{
			for (int j = zIndex; j < zIndex + num; j++)
			{
				RGChunk rGChunk = data.Node(i, j);
				if (rGChunk != null && rGChunk.Dirty)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void Dirty(RGDataSource data, bool dirty)
	{
		int num = 1 << LOD;
		for (int i = xIndex; i < xIndex + num; i++)
		{
			for (int j = zIndex; j < zIndex + num; j++)
			{
				RGChunk rGChunk = data.Node(i, j);
				if (rGChunk != null)
				{
					rGChunk.Dirty = dirty;
				}
			}
		}
	}
}
