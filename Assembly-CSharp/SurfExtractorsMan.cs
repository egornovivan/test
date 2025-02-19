using System;
using System.Collections.Generic;
using UnityEngine;

public class SurfExtractorsMan
{
	public const int c_vertsCnt4Pool = 3999;

	public const int c_vertsCntMax = 64998;

	public static int[] s_indiceMax;

	public static int[] s_indice4Pool;

	private static IVxSurfExtractor _vxSurfExtractor;

	private static IVxSurfExtractor _b45BuildSurfExtractor;

	private static IVxSurfExtractor _b45CursorSurfExtractor;

	private static string _vxSurfOpt;

	public static IVxSurfExtractor VxSurfExtractor => _vxSurfExtractor;

	public static IVxSurfExtractor B45BuildSurfExtractor => _b45BuildSurfExtractor;

	public static IVxSurfExtractor B45CursorSurfExtractor => _b45CursorSurfExtractor;

	public static void CheckGenSurfExtractor()
	{
		s_indiceMax = new int[64998];
		for (int i = 0; i < 64998; i++)
		{
			s_indiceMax[i] = i;
		}
		s_indice4Pool = new int[3999];
		Array.Copy(s_indiceMax, s_indice4Pool, 3999);
		if (_vxSurfOpt != null && _vxSurfOpt.CompareTo(oclManager.CurOclOpt) == 0)
		{
			_vxSurfExtractor.Reset();
		}
		else
		{
			if (_vxSurfExtractor != null)
			{
				_vxSurfExtractor.CleanUp();
			}
			oclManager.InitOclFromOpt();
			if (oclManager.ActiveOclMan != null)
			{
				_vxSurfExtractor = new SurfExtractorOclMC();
				_vxSurfExtractor.Init();
			}
			else
			{
				_vxSurfExtractor = new SurfExtractorCpuMC();
				_vxSurfExtractor.Init();
			}
			_vxSurfOpt = string.Copy(oclManager.CurOclOpt);
		}
		if (_b45BuildSurfExtractor != null)
		{
			_b45BuildSurfExtractor.CleanUp();
		}
		_b45BuildSurfExtractor = new SurfExtractorCpuB45();
		_b45BuildSurfExtractor.Init();
	}

	public static void SwitchToVxCpu(List<IVxSurfExtractReq> reqsInProcess, Dictionary<int, IVxSurfExtractReq> reqsToProcess)
	{
		Debug.LogError("[SurfExtractMan]: Start to switch to Cpu !");
		SurfExtractorCpuMC surfExtractorCpuMC = new SurfExtractorCpuMC();
		surfExtractorCpuMC.Init();
		_vxSurfExtractor = surfExtractorCpuMC;
		lock (reqsToProcess)
		{
			for (int i = 0; i < reqsInProcess.Count; i++)
			{
				_vxSurfExtractor.AddSurfExtractReq(reqsInProcess[i]);
			}
			foreach (KeyValuePair<int, IVxSurfExtractReq> item in reqsToProcess)
			{
				_vxSurfExtractor.AddSurfExtractReq(item.Value);
			}
			Debug.LogError("[SurfExtractMan]: Finished to switch to Cpu:" + (reqsInProcess.Count + reqsToProcess.Count));
		}
	}

	public static void PostProc()
	{
		if (_vxSurfExtractor != null)
		{
			_vxSurfExtractor.OnFin();
		}
		if (_b45BuildSurfExtractor != null)
		{
			_b45BuildSurfExtractor.OnFin();
		}
		if (_b45CursorSurfExtractor != null)
		{
			_b45CursorSurfExtractor.OnFin();
		}
	}

	public static void CleanUp()
	{
		if (_vxSurfExtractor != null)
		{
			_vxSurfExtractor.CleanUp();
		}
		if (_b45BuildSurfExtractor != null)
		{
			_b45BuildSurfExtractor.CleanUp();
		}
		if (_b45CursorSurfExtractor != null)
		{
			_b45CursorSurfExtractor.CleanUp();
		}
		_vxSurfExtractor = null;
		_b45BuildSurfExtractor = null;
		_b45CursorSurfExtractor = null;
		Debug.Log("Mem size after surfExtractor CleanUp :" + GC.GetTotalMemory(forceFullCollection: true));
	}
}
