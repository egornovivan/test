using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class ObjNetInterface : NetInterface
{
	public delegate bool CheckValidDistEventHandler(ObjNetInterface obj);

	protected static Dictionary<int, ObjNetInterface> _netObjs = new Dictionary<int, ObjNetInterface>();

	protected Dictionary<ECmptType, DataCmpt> mCmpts = new Dictionary<ECmptType, DataCmpt>();

	protected int _id;

	protected AbnormalCmpt m_AbnormalModule;

	public int Id => _id;

	public AbnormalCmpt AbnormalModule => m_AbnormalModule;

	public static ObjNetInterface Get(int id)
	{
		if (id == -1 || !_netObjs.ContainsKey(id))
		{
			return null;
		}
		return _netObjs[id];
	}

	public static T Get<T>(int id) where T : ObjNetInterface
	{
		if (id == -1 || !_netObjs.ContainsKey(id))
		{
			return (T)null;
		}
		return _netObjs[id] as T;
	}

	public static List<T> Get<T>() where T : ObjNetInterface
	{
		IEnumerable<T> source = from obj in _netObjs
			where obj.Value is T
			select obj.Value as T;
		return source.ToList();
	}

	public static bool IsExisted(int id)
	{
		return _netObjs.ContainsKey(id);
	}

	public void Add(ObjNetInterface netObj)
	{
		if (_netObjs.ContainsKey(netObj.Id))
		{
			_netObjs.Remove(netObj.Id);
		}
		_netObjs.Add(netObj.Id, netObj);
		netObj.PEDestroyEvent += OnDestroyed;
	}

	public static void Del(int id)
	{
		if (IsExisted(id))
		{
			_netObjs.Remove(id);
		}
	}

	private void OnDestroyed(NetInterface netObj)
	{
		Del(Id);
	}

	public DataCmpt GetCmpt(ECmptType type)
	{
		if (!mCmpts.ContainsKey(type))
		{
			return null;
		}
		return mCmpts[type];
	}

	public DataCmpt AddCmpt(ECmptType type)
	{
		if (mCmpts.ContainsKey(type))
		{
			return mCmpts[type];
		}
		DataCmpt dataCmpt = null;
		switch (type)
		{
		case ECmptType.Abnormal:
			dataCmpt = new AbnormalCmpt();
			break;
		case ECmptType.Equipment:
			dataCmpt = new EquipmentCmpt();
			break;
		case ECmptType.Item:
		case ECmptType.ServantItem:
			dataCmpt = new ItemCmpt();
			break;
		case ECmptType.NpcSkillAbility:
			dataCmpt = new NpcAbilityCmpt();
			break;
		case ECmptType.Package:
			dataCmpt = new PackageCmpt();
			break;
		case ECmptType.PlayerPackage:
			dataCmpt = new PlayerPackageCmpt();
			break;
		}
		dataCmpt.LinkNet(this);
		mCmpts[type] = dataCmpt;
		return dataCmpt;
	}

	public bool HasCmpt(ECmptType type)
	{
		return mCmpts.ContainsKey(type);
	}

	protected virtual void InitCmpt()
	{
		m_AbnormalModule = (AbnormalCmpt)AddCmpt(ECmptType.Abnormal);
	}

	public void ExportCmpt(BinaryWriter w)
	{
		BufferHelper.Serialize(w, mCmpts.Count);
		foreach (KeyValuePair<ECmptType, DataCmpt> mCmpt in mCmpts)
		{
			mCmpt.Value.Export(w);
		}
	}

	public void ImportCmpt(BinaryReader r)
	{
		int num = BufferHelper.ReadInt32(r);
		for (int i = 0; i < num; i++)
		{
			ECmptType type = (ECmptType)BufferHelper.ReadInt32(r);
			GetCmpt(type)?.Import(r);
		}
	}

	protected void CheckValidDist(CheckValidDistEventHandler handler, Action invalidEvent)
	{
		CheckValidDist(handler, invalidEvent, 3f);
	}

	protected void CheckValidDist(CheckValidDistEventHandler handler, Action invalidEvent, float intervalTime)
	{
		StartCoroutine(CheckDist(handler, invalidEvent, intervalTime));
	}

	private IEnumerator CheckDist(CheckValidDistEventHandler handler, Action invalidEvent, float intervalTime)
	{
		while (true)
		{
			if (handler != null && !handler(this))
			{
				invalidEvent?.Invoke();
			}
			yield return new WaitForSeconds(intervalTime);
		}
	}
}
