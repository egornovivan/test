using System;
using System.Collections.Generic;
using UnityEngine;

public class SurfExtractReqMC : IVxSurfExtractReq
{
	private List<MCOutputData> _outDatas = new List<MCOutputData>(4);

	public int _chunkSig;

	public int _chunkStamp;

	public byte[] _chunkData;

	public VFVoxelChunkData _chunk;

	private int _nVertsPerMesh;

	private int _validChunkDataLen;

	private Action<SurfExtractReqMC> _customFinHandler;

	private static GenericPool<SurfExtractReqMC> s_reqPool = new GenericPool<SurfExtractReqMC>(768);

	public byte[] VolumeData => _chunkData;

	public bool IsInvalid => (_customFinHandler != null) ? (!_chunk.IsMatch(_chunkSig, _chunkStamp)) : (!_chunk.IsMatch(_chunkSig, _chunkStamp) || _chunkData == null || _chunkData.Length != 85750);

	public int Signature => _chunkSig;

	public int Priority => _chunk.LOD;

	public int MeshSplitThreshold => _nVertsPerMesh;

	public int FillMesh(Mesh mesh)
	{
		int count = _outDatas.Count;
		if (mesh == null)
		{
			return count;
		}
		count--;
		mesh.triangles = null;
		_outDatas[count].SetToMesh(mesh);
		MCOutputData.Free(_outDatas[count]);
		_outDatas.RemoveAt(count);
		return count;
	}

	public void AddOutData(MCOutputData data)
	{
		_outDatas.Add(data);
	}

	public void FillOutData(Vector3[] srcPosArray, Vector2[] srcNorm01Array, Vector2[] srcNorm2tArray, int srcIdx, int len)
	{
		while (len > 0)
		{
			int num = ((len <= _nVertsPerMesh) ? len : _nVertsPerMesh);
			MCOutputData item = MCOutputData.Get(srcPosArray, srcNorm01Array, srcNorm2tArray, srcIdx, num);
			_outDatas.Add(item);
			srcIdx += num;
			len -= num;
		}
	}

	public void FillOutData(List<Vector3> srcPosLst, List<Vector2> srcNorm01Lst, List<Vector2> srcNorm2tLst, int srcIdx, int len)
	{
		while (len > 0)
		{
			int num = ((len <= _nVertsPerMesh) ? len : _nVertsPerMesh);
			MCOutputData item = MCOutputData.Get(srcPosLst, srcNorm01Lst, srcNorm2tLst, srcIdx, num);
			_outDatas.Add(item);
			srcIdx += num;
			len -= num;
		}
	}

	public bool OnReqFinished()
	{
		bool result = false;
		if (!IsInvalid)
		{
			if (_customFinHandler != null)
			{
				_customFinHandler(this);
			}
			else if (_chunk.HelperProc != null)
			{
				_chunk.HelperProc.ChunkProcPostGenMesh(this);
			}
			result = true;
		}
		Free(this);
		return result;
	}

	public static SurfExtractReqMC Get(VFVoxelChunkData chunk, Action<SurfExtractReqMC> finHandler)
	{
		SurfExtractReqMC surfExtractReqMC = Get(chunk);
		surfExtractReqMC._customFinHandler = finHandler;
		return surfExtractReqMC;
	}

	public static SurfExtractReqMC Get(VFVoxelChunkData chunk, int nVertsPerMesh = 3999)
	{
		SurfExtractReqMC surfExtractReqMC = s_reqPool.Get();
		surfExtractReqMC._chunk = chunk;
		surfExtractReqMC._chunkData = chunk.DataVT;
		surfExtractReqMC._chunkSig = chunk.SigOfChnk;
		surfExtractReqMC._chunkStamp = chunk.StampOfUpdating;
		surfExtractReqMC._customFinHandler = null;
		surfExtractReqMC._nVertsPerMesh = nVertsPerMesh;
		return surfExtractReqMC;
	}

	public static void Free(SurfExtractReqMC req)
	{
		int count = req._outDatas.Count;
		if (count > 0)
		{
			for (int i = 0; i < count; i++)
			{
				MCOutputData.Free(req._outDatas[i]);
			}
			req._outDatas.Clear();
		}
		s_reqPool.Free(req);
	}
}
