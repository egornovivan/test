using System;
using System.Collections.Generic;
using UnityEngine;

public class SurfExtractReqB45 : IVxSurfExtractReq
{
	public Vector3[] verts;

	public Vector2[] uvs;

	public int matCnt;

	public int[] materialMap;

	public List<int[]> subMeshIndices = new List<int[]>();

	private int _nVertsPerMesh;

	public int _chunkStamp;

	public Block45OctNode _chunkData;

	public Action<SurfExtractReqB45> _finHandler;

	private static GenericPool<SurfExtractReqB45> s_reqPool = new GenericPool<SurfExtractReqB45>();

	public byte[] VolumeData => _chunkData.DataVT;

	public bool IsInvalid
	{
		get
		{
			byte[] volumeData = VolumeData;
			return !_chunkData.IsStampIdentical(_chunkStamp) || volumeData == null || volumeData.Length != 2000;
		}
	}

	public int Signature => _chunkData.Signature;

	public int Priority => _chunkData.LOD;

	public int MeshSplitThreshold => _nVertsPerMesh;

	public int FillMesh(Mesh mesh)
	{
		if (mesh == null)
		{
			return (verts != null && verts.Length > 0) ? 1 : 0;
		}
		mesh.name = "b45_mesh";
		mesh.vertices = verts;
		mesh.uv = uvs;
		mesh.subMeshCount = matCnt;
		for (int i = 0; i < matCnt; i++)
		{
			mesh.SetTriangles(subMeshIndices[i], i);
		}
		mesh.RecalculateNormals();
		Block45Kernel.TangentSolver(mesh);
		return 0;
	}

	public bool OnReqFinished()
	{
		bool result = false;
		if (!IsInvalid && _finHandler != null)
		{
			_finHandler(this);
			result = true;
		}
		Free(this);
		return result;
	}

	public static SurfExtractReqB45 Get(int chunkStamp, Block45OctNode chunkData, Action<SurfExtractReqB45> finHandler, int nVertsPerMesh = 64998)
	{
		SurfExtractReqB45 surfExtractReqB = s_reqPool.Get();
		surfExtractReqB._chunkStamp = chunkStamp;
		surfExtractReqB._chunkData = chunkData;
		surfExtractReqB._nVertsPerMesh = nVertsPerMesh;
		surfExtractReqB._finHandler = finHandler;
		return surfExtractReqB;
	}

	public static void Free(SurfExtractReqB45 req)
	{
		req.verts = null;
		req.uvs = null;
		req.materialMap = null;
		req.subMeshIndices.Clear();
		s_reqPool.Free(req);
	}
}
