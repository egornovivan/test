using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class SurfExtractorOclMC : IVxSurfExtractor
{
	private const int MaxReqsToProceedOnce = 12;

	private Dictionary<int, IVxSurfExtractReq> _reqList;

	private List<IVxSurfExtractReq> _reqInProcessList;

	private List<IVxSurfExtractReq> _reqFinishedList;

	private List<KeyValuePair<int, IVxSurfExtractReq>> _tmpReqList;

	private Thread _threadMC;

	private bool _bThreadOn;

	private int _errCnt;

	private bool bNeedUpdateTrans;

	public bool IsIdle => _reqList.Count == 0 && _reqInProcessList.Count == 0;

	public bool IsAllClear => _reqList.Count == 0 && _reqInProcessList.Count == 0 && _reqFinishedList.Count == 0;

	public bool Pause { get; set; }

	private void PickReqToProceed()
	{
		if (_reqList.Count == 0)
		{
			return;
		}
		foreach (KeyValuePair<int, IVxSurfExtractReq> item in _reqList.Where((KeyValuePair<int, IVxSurfExtractReq> kvp) => kvp.Value.IsInvalid).ToList())
		{
			_reqList.Remove(item.Key);
		}
		_tmpReqList.Clear();
		int curlod;
		for (curlod = 0; curlod <= 4; curlod++)
		{
			if (_tmpReqList.Count >= 31)
			{
				break;
			}
			_tmpReqList.AddRange(_reqList.Where((KeyValuePair<int, IVxSurfExtractReq> kvp) => kvp.Value.Priority == curlod));
		}
		int num = 0;
		int count = _tmpReqList.Count;
		while (oclMarchingCube.numChunks < 31 && num < count)
		{
			IVxSurfExtractReq value = _tmpReqList[num].Value;
			_reqInProcessList.Add(value);
			oclMarchingCube.AddChunkVolumeData(value.VolumeData);
			_reqList.Remove(_tmpReqList[num].Key);
			num++;
		}
	}

	public void Init()
	{
		_reqList = new Dictionary<int, IVxSurfExtractReq>();
		_reqInProcessList = new List<IVxSurfExtractReq>(32);
		_reqFinishedList = new List<IVxSurfExtractReq>();
		_tmpReqList = new List<KeyValuePair<int, IVxSurfExtractReq>>();
		Pause = false;
		_errCnt = 0;
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
					Debug.LogError("[SurfMC_OCL]Error in thread:" + Environment.NewLine + ex.ToString());
					_errCnt++;
					break;
				}
			}
			Thread.Sleep(1);
		}
	}

	public void Exec()
	{
		lock (_reqList)
		{
			PickReqToProceed();
		}
		int numChunks = oclMarchingCube.numChunks;
		if (numChunks == 0)
		{
			return;
		}
		IEnumerator enumerator = oclMarchingCube.computeIsosurfaceAsyn();
		while (enumerator.MoveNext())
		{
		}
		Vector3[] srcPosArray = oclMarchingCube.hPosArray.Target as Vector3[];
		Vector2[] srcNorm01Array = oclMarchingCube.hNorm01Array.Target as Vector2[];
		Vector2[] srcNorm2tArray = oclMarchingCube.hNorm2tArray.Target as Vector2[];
		for (int i = 0; i < numChunks; i++)
		{
			SurfExtractReqMC surfExtractReqMC = _reqInProcessList[i] as SurfExtractReqMC;
			int num = oclScanLaucher.ofsData[i];
			int num2 = ((i != numChunks - 1) ? (oclScanLaucher.ofsData[i + 1] - num) : (oclScanLaucher.sumData[0] - num));
			if (num2 > 0)
			{
				surfExtractReqMC.FillOutData(srcPosArray, srcNorm01Array, srcNorm2tArray, num, num2);
			}
		}
		lock (_reqFinishedList)
		{
			_reqFinishedList.AddRange(_reqInProcessList);
		}
		_reqInProcessList.Clear();
	}

	public int OnFin()
	{
		if (_errCnt > 0 && _reqFinishedList.Count == 0)
		{
			SurfExtractorsMan.SwitchToVxCpu(_reqInProcessList, _reqList);
			CleanUp();
			return 0;
		}
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
			int num = 12;
			int num2 = 0;
			int count = _reqFinishedList.Count;
			for (int i = 0; i < count; i++)
			{
				num2++;
				if (_reqFinishedList[i].OnReqFinished() && --num == 0)
				{
					break;
				}
			}
			_reqFinishedList.RemoveRange(0, num2);
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
			Debug.Log("[SurfMC_OCL]Thread Reset");
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
			Debug.Log("[SurfMC_OCL]Thread stopped");
		}
		catch
		{
			MonoBehaviour.print("[SurfMC_OCL]Thread stopped with exception");
		}
		_reqFinishedList.Clear();
		oclMarchingCube.Cleanup();
	}

	public bool AddSurfExtractReq(IVxSurfExtractReq req)
	{
		lock (_reqList)
		{
			_reqList[req.Signature] = req;
		}
		return true;
	}

	public IVxSurfExtractReq Contains(VFVoxelChunkData chunk)
	{
		lock (_reqList)
		{
			List<IVxSurfExtractReq> list = _reqList.Values.ToList();
			IVxSurfExtractReq value = list.Find((IVxSurfExtractReq r) => ((SurfExtractReqMC)r)._chunk == chunk);
			if (value == null)
			{
				value = list.Find((IVxSurfExtractReq r) => ((SurfExtractReqMC)r)._chunk.ChunkPosLod.Equals(chunk.ChunkPosLod));
				if (value == null)
				{
					_reqList.TryGetValue(chunk.SigOfChnk, out value);
				}
			}
			return value;
		}
	}
}
