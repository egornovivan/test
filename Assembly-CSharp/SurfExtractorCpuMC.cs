using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class SurfExtractorCpuMC : IVxSurfExtractor
{
	private MarchingCubesSW swMarchingCubes;

	private byte[] _chunkDataBuffer;

	private byte[] _chunkDataToProceed;

	private bool _bThreadOn;

	private Thread _threadMC;

	private Dictionary<int, IVxSurfExtractReq> _reqList;

	private List<IVxSurfExtractReq> _reqFinishedList;

	private IVxSurfExtractReq reqToProceed;

	private bool bNeedUpdateTrans;

	public bool IsIdle => _reqList.Count == 0 && _chunkDataToProceed == null;

	public bool IsAllClear => _reqList.Count == 0 && _chunkDataToProceed == null && _reqFinishedList.Count == 0;

	public bool Pause { get; set; }

	private IVxSurfExtractReq PickReqToProceed()
	{
		if (_reqList.Count == 0)
		{
			return null;
		}
		foreach (KeyValuePair<int, IVxSurfExtractReq> item in _reqList.Where((KeyValuePair<int, IVxSurfExtractReq> kvp) => kvp.Value.IsInvalid).ToList())
		{
			_reqList.Remove(item.Key);
		}
		for (int i = 0; i <= 4; i++)
		{
			foreach (KeyValuePair<int, IVxSurfExtractReq> req in _reqList)
			{
				if (req.Value.Priority == i)
				{
					IVxSurfExtractReq value = req.Value;
					Array.Copy(value.VolumeData, _chunkDataBuffer, _chunkDataBuffer.Length);
					_chunkDataToProceed = _chunkDataBuffer;
					_reqList.Remove(req.Key);
					return value;
				}
			}
		}
		return null;
	}

	public void Init()
	{
		swMarchingCubes = new MarchingCubesSW();
		_chunkDataBuffer = new byte[85750];
		_chunkDataToProceed = null;
		_reqList = new Dictionary<int, IVxSurfExtractReq>();
		_reqFinishedList = new List<IVxSurfExtractReq>();
		Pause = false;
		_threadMC = new Thread(ThreadExec);
		_threadMC.Start();
	}

	private void ThreadExec()
	{
		_bThreadOn = true;
		while (_bThreadOn)
		{
			if (_reqList.Count > 0 && !Pause)
			{
				try
				{
					Exec();
				}
				catch (Exception ex)
				{
					Debug.LogError("[SurfMC_CPU]Error in thread:" + Environment.NewLine + ex.ToString());
					Recover();
				}
			}
			Thread.Sleep(1);
		}
	}

	private void Recover()
	{
		reqToProceed = null;
	}

	public void Exec()
	{
		reqToProceed = null;
		lock (_reqList)
		{
			reqToProceed = PickReqToProceed();
		}
		if (reqToProceed != null)
		{
			swMarchingCubes.setInputChunkData(_chunkDataToProceed);
			swMarchingCubes.Rebuild();
			SurfExtractReqMC surfExtractReqMC = reqToProceed as SurfExtractReqMC;
			int count = swMarchingCubes.VertexList.Count;
			if (count > 0)
			{
				surfExtractReqMC.FillOutData(swMarchingCubes.VertexList, swMarchingCubes.Norm01List, swMarchingCubes.Norm2tList, 0, count);
			}
			lock (_reqFinishedList)
			{
				_reqFinishedList.Add(reqToProceed);
			}
			_chunkDataToProceed = null;
		}
	}

	public int OnFin()
	{
		if (_reqFinishedList.Count == 0)
		{
			if (bNeedUpdateTrans && VFVoxelTerrain.self != null && VFVoxelTerrain.self.TransGoCreator.IsTransvoxelEnabled && VFVoxelTerrain.self.TransGoCreator.PrepChunkList())
			{
				bNeedUpdateTrans = false;
			}
			return 0;
		}
		if (Monitor.TryEnter(_reqFinishedList))
		{
			int i = 0;
			for (int count = _reqFinishedList.Count; i < count; i++)
			{
				_reqFinishedList[i].OnReqFinished();
			}
			_reqFinishedList.Clear();
			Monitor.Exit(_reqFinishedList);
		}
		bNeedUpdateTrans = true;
		return _reqFinishedList.Count;
	}

	public void Reset()
	{
		lock (_reqList)
		{
			_reqList.Clear();
		}
		Pause = false;
		if (!IsIdle)
		{
			_bThreadOn = false;
			_threadMC.Join();
			_threadMC = new Thread(ThreadExec);
			_threadMC.Start();
			Debug.Log("[SurfMC_CPU]Thread Reset");
		}
		_reqFinishedList.Clear();
	}

	public void CleanUp()
	{
		lock (_reqList)
		{
			_reqList.Clear();
		}
		_bThreadOn = false;
		try
		{
			_threadMC.Join();
			Debug.Log("[SurfMC_CPU]Thread stopped");
		}
		catch
		{
			MonoBehaviour.print("[SurfMC_CPU]Thread stopped with exception");
		}
		_reqFinishedList.Clear();
	}

	public bool AddSurfExtractReq(IVxSurfExtractReq req)
	{
		lock (_reqList)
		{
			_reqList[req.Signature] = req;
		}
		return true;
	}
}
