using System.Collections.Generic;
using UnityEngine;

public class Block45CurMan : MonoBehaviour
{
	public static Block45CurMan self;

	public Material[] _b45Materials;

	private Block45OctDataSource _dataSource;

	private List<Block45OctNode> _octNodesToBuild = new List<Block45OctNode>();

	public Block45OctDataSource DataSource => _dataSource;

	public bool IsInRebuilding => SurfExtractorsMan.B45BuildSurfExtractor != null && !SurfExtractorsMan.B45BuildSurfExtractor.IsAllClear;

	private void Awake()
	{
		if (self == null)
		{
			self = this;
		}
		_dataSource = new Block45OctDataSource(AddNodeToBuildList);
	}

	private void AddNodeToBuildList(Block45OctNode node)
	{
		if (node.LOD == 0)
		{
			_octNodesToBuild.Add(node);
		}
	}

	public void RebuildMesh()
	{
		if (SurfExtractorsMan.B45BuildSurfExtractor == null)
		{
			return;
		}
		foreach (Block45OctNode item in _octNodesToBuild)
		{
			SurfExtractorsMan.B45BuildSurfExtractor.AddSurfExtractReq(SurfExtractReqB45.Get(item.GetStamp(), item, ChunkProcPostGenMesh));
		}
	}

	public void AlterBlockInBuild(int vx, int vy, int vz, B45Block blk)
	{
		DataSource.SafeWrite(blk, vx, vy, vz);
	}

	private void ChunkProcPostGenMesh(IVxSurfExtractReq ireq)
	{
		SurfExtractReqB45 surfExtractReqB = ireq as SurfExtractReqB45;
		Block45ChunkGo b45Go = Block45ChunkGo.CreateChunkGo(surfExtractReqB, base.transform);
		surfExtractReqB._chunkData.AttachChunkGo(b45Go);
	}
}
