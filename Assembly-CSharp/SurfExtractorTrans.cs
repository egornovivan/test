using System;
using System.Collections.Generic;
using Transvoxel.SurfaceExtractor;

public class SurfExtractorTrans : IVxSurfExtractor
{
	private const int NumReqInFin = 12;

	private Dictionary<int, SurfExtractReqTrans> _reqList;

	private List<IVxSurfExtractReq> _reqFinishedList;

	public bool IsIdle => _reqList.Count == 0;

	public bool IsAllClear => _reqList.Count == 0 && _reqFinishedList.Count == 0;

	public bool Pause { get; set; }

	public void Init()
	{
		_reqList = new Dictionary<int, SurfExtractReqTrans>();
		_reqFinishedList = new List<IVxSurfExtractReq>();
		Pause = false;
	}

	public void Exec()
	{
		if (_reqList.Count == 0 || Pause)
		{
			return;
		}
		foreach (SurfExtractReqTrans value in _reqList.Values)
		{
			if (value._faceMask != 0)
			{
				TransVertices verts = new TransVertices();
				List<int> indices = new List<int>();
				float cellSize = 0.01f;
				TransvoxelExtractor2.BuildTransitionCells(value._faceMask, value._chunkData, cellSize, verts, indices);
				int num = TransvoxelGoCreator.UnindexedVertex(verts, indices, out value.vert, out value.norm01, out value.norm2t);
				value.indice = new int[num];
				Array.Copy(SurfExtractorsMan.s_indiceMax, value.indice, num);
			}
			lock (_reqFinishedList)
			{
				_reqFinishedList.Add(value);
			}
		}
		_reqList.Clear();
	}

	public int OnFin()
	{
		if (_reqFinishedList.Count == 0)
		{
			return 0;
		}
		int count = _reqFinishedList.Count;
		int num = ((count > 12) ? (count - 12) : 0);
		for (int num2 = count - 1; num2 >= num; num2--)
		{
			_reqFinishedList[num2].OnReqFinished();
			_reqFinishedList.RemoveAt(num2);
		}
		return _reqFinishedList.Count;
	}

	public void Reset()
	{
		Init();
	}

	public void CleanUp()
	{
	}

	public bool AddSurfExtractReq(IVxSurfExtractReq req)
	{
		_reqList[req.Signature] = (SurfExtractReqTrans)req;
		return true;
	}
}
