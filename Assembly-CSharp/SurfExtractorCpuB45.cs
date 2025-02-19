using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class SurfExtractorCpuB45 : IVxSurfExtractor
{
	private Block45Kernel swBlock45;

	private byte[] _chunkDataBuffer;

	private byte[] _chunkDataToProceed;

	private bool _bThreadOn;

	private Thread _threadB45;

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
		swBlock45 = new Block45Kernel();
		_chunkDataBuffer = new byte[2000];
		_chunkDataToProceed = null;
		_reqList = new Dictionary<int, IVxSurfExtractReq>();
		_reqFinishedList = new List<IVxSurfExtractReq>();
		Pause = false;
		_threadB45 = new Thread(ThreadExec);
		_threadB45.Start();
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
					Debug.LogError("[SurfB45_CPU]Error in thread:" + Environment.NewLine + ex.ToString());
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
		if (reqToProceed == null)
		{
			return;
		}
		swBlock45.setInputChunkData(_chunkDataToProceed);
		swBlock45.Rebuild();
		SurfExtractReqB45 surfExtractReqB = reqToProceed as SurfExtractReqB45;
		int count = swBlock45.verts.Count;
		if (count > 0)
		{
			int meshSplitThreshold = reqToProceed.MeshSplitThreshold;
			if (count <= meshSplitThreshold)
			{
				surfExtractReqB.verts = swBlock45.verts.ToArray();
				surfExtractReqB.uvs = swBlock45.uvs.ToArray();
				surfExtractReqB.matCnt = swBlock45.matCnt;
				surfExtractReqB.materialMap = swBlock45.materialMap;
				foreach (List<int> subMeshIndex in swBlock45.subMeshIndices)
				{
					surfExtractReqB.subMeshIndices.Add(subMeshIndex.ToArray());
				}
			}
			else
			{
				Debug.LogError("[SurfB45_CPU]:Out Of chunkVertsCntThreshold.");
			}
		}
		lock (_reqFinishedList)
		{
			_reqFinishedList.Add(reqToProceed);
		}
		_chunkDataToProceed = null;
	}

	public int OnFin()
	{
		if (Monitor.TryEnter(_reqFinishedList))
		{
			for (int i = 0; i < _reqFinishedList.Count; i++)
			{
				IVxSurfExtractReq vxSurfExtractReq = _reqFinishedList[i];
				vxSurfExtractReq.OnReqFinished();
			}
			_reqFinishedList.Clear();
			Monitor.Exit(_reqFinishedList);
		}
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
			_threadB45.Join();
			_threadB45 = new Thread(ThreadExec);
			_threadB45.Start();
			Debug.Log("[SurfB45_CPU]Thread Reset");
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
			_threadB45.Join();
			Debug.Log("[SurfB45_CPU]Thread stopped");
		}
		catch
		{
			MonoBehaviour.print("[SurfB45_CPU]Thread stopped with exception");
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
