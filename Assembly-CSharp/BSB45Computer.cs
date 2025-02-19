using System.Collections.Generic;
using UnityEngine;

public class BSB45Computer : MonoBehaviour
{
	private Block45OctDataSource _dataSource;

	private List<Block45OctNode> _octNodesToBuild = new List<Block45OctNode>();

	public Block45OctDataSource DataSource => _dataSource;

	public bool IsInRebuilding => SurfExtractorsMan.B45BuildSurfExtractor != null && !SurfExtractorsMan.B45BuildSurfExtractor.IsAllClear;

	public void AlterBlockInBuild(int vx, int vy, int vz, B45Block blk)
	{
		DataSource.SafeWrite(blk, vx, vy, vz);
	}

	public void ClearDataDS()
	{
		if (_dataSource != null)
		{
			_dataSource.Clear();
		}
	}

	public void RebuildMesh()
	{
		if (SurfExtractorsMan.B45BuildSurfExtractor != null)
		{
			for (int i = 0; i < _octNodesToBuild.Count; i++)
			{
				Block45OctNode block45OctNode = _octNodesToBuild[i];
				SurfExtractorsMan.B45BuildSurfExtractor.AddSurfExtractReq(SurfExtractReqB45.Get(block45OctNode.GetStamp(), block45OctNode, ChunkProcPostGenMesh));
			}
		}
	}

	private void AddNodeToBuildList(Block45OctNode node)
	{
		if (node.LOD == 0)
		{
			_octNodesToBuild.Add(node);
		}
	}

	private void ChunkProcPostGenMesh(IVxSurfExtractReq ireq)
	{
		if (!(this == null))
		{
			SurfExtractReqB45 surfExtractReqB = ireq as SurfExtractReqB45;
			Block45ChunkGo b45Go = Block45ChunkGo.CreateChunkGo(surfExtractReqB, base.transform);
			surfExtractReqB._chunkData.AttachChunkGo(b45Go);
		}
	}

	private void Awake()
	{
		_dataSource = new Block45OctDataSource(AddNodeToBuildList);
	}
}
