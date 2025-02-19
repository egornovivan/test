using System.Collections.Generic;
using UnityEngine;

public class AssetReq
{
	public enum ProcLvl
	{
		DoNothing,
		DoLoading,
		DoInstantiation
	}

	public delegate void ReqFinishDelegate(GameObject go);

	public AssetPRS Prs;

	public string PathName;

	public ProcLvl ProcLevel;

	public bool NeedCaching;

	public bool isActive;

	public event ReqFinishDelegate ReqFinishHandler;

	public AssetReq()
	{
	}

	public AssetReq(string assetPathName, AssetPRS assetPos, ProcLvl procLvl = ProcLvl.DoInstantiation, bool bCaching = true)
	{
		PathName = assetPathName;
		Prs = assetPos;
		ProcLevel = procLvl;
		NeedCaching = bCaching;
	}

	public AssetReq(string assetPathName, ProcLvl procLvl = ProcLvl.DoLoading, bool bCaching = true)
	{
		PathName = assetPathName;
		Prs = AssetPRS.Zero;
		ProcLevel = procLvl;
		NeedCaching = bCaching;
	}

	public void Deactivate()
	{
		ProcLevel = ProcLvl.DoNothing;
	}

	public override bool Equals(object obj)
	{
		if (!(obj is AssetReq assetReq))
		{
			return false;
		}
		return PathName.Equals(assetReq.PathName) && Prs.Equals(assetReq.Prs);
	}

	internal void OnFinish(GameObject go)
	{
		if (this.ReqFinishHandler != null)
		{
			this.ReqFinishHandler(go);
		}
	}

	public static List<AssetReq> ConvertToReq(List<AssetBundleDesc> descList)
	{
		List<AssetReq> list = new List<AssetReq>();
		foreach (AssetBundleDesc desc in descList)
		{
			int num = desc.pos.Length;
			for (int i = 0; i < num; i++)
			{
				list.Add(new AssetReq(desc.pathName, desc.pos[i]));
			}
		}
		return list;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
