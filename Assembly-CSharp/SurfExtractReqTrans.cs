using UnityEngine;

public class SurfExtractReqTrans : IVxSurfExtractReq
{
	public Vector3[] vert;

	public Vector2[] norm01;

	public Vector2[] norm2t;

	public int[] indice;

	public int _chunkStamp;

	public int _faceMask;

	public VFVoxelChunkData _chunkData;

	public byte[] VolumeData => _chunkData.DataVT;

	public bool IsInvalid => _chunkData.ChunkPosLod.GetHashCode() != _chunkStamp;

	public int Signature => _chunkStamp;

	public int Priority => 0;

	public int MeshSplitThreshold => 64998;

	public SurfExtractReqTrans(int faceMask, VFVoxelChunkData chunkData)
	{
		_faceMask = faceMask;
		_chunkStamp = chunkData.ChunkPosLod.GetHashCode();
		_chunkData = chunkData;
	}

	public int FillMesh(Mesh mesh)
	{
		mesh.name = "trans";
		mesh.vertices = vert;
		mesh.uv = norm01;
		mesh.uv2 = norm2t;
		mesh.SetTriangles(indice, 0);
		return 0;
	}

	public bool OnReqFinished()
	{
		bool result = false;
		if (!IsInvalid)
		{
			VFVoxelChunkGo chunkGo = _chunkData.ChunkGo;
			if (chunkGo != null && !chunkGo.Equals(null))
			{
				chunkGo.SetTransGo(this, _faceMask);
				result = true;
			}
		}
		return result;
	}
}
