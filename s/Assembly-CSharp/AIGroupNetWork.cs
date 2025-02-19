using System.Collections;
using System.Collections.Generic;
using uLink;
using UnityEngine;

public class AIGroupNetWork : ObjNetInterface
{
	protected int _tdId;

	protected int _dungeonId;

	protected int _colorType;

	protected int _playerId;

	protected int _buffId;

	protected int _externId;

	protected int _ownerId;

	private List<AiObject> _aiList = new List<AiObject>();

	protected Vector3 _spawnPos;

	public int ExternId => _externId;

	public Vector3 SpawnPos => _spawnPos;

	protected override void OnPEInstantiate(uLink.NetworkMessageInfo info)
	{
		SetParent("GroupNetMgr");
		_id = info.networkView.initialData.Read<int>(new object[0]);
		_externId = info.networkView.initialData.Read<int>(new object[0]);
		_ownerId = info.networkView.initialData.Read<int>(new object[0]);
		_tdId = info.networkView.initialData.Read<int>(new object[0]);
		_dungeonId = info.networkView.initialData.Read<int>(new object[0]);
		_colorType = info.networkView.initialData.Read<int>(new object[0]);
		_playerId = info.networkView.initialData.Read<int>(new object[0]);
		_buffId = info.networkView.initialData.Read<int>(new object[0]);
		_spawnPos = base.transform.position;
		Add(this);
		_aiList.Clear();
		base.gameObject.name = $"Group protoId:{ExternId}, TdId:{_tdId}";
	}

	protected override void OnPEDestroy()
	{
		base.OnPEDestroy();
		foreach (AiObject ai in _aiList)
		{
			if (null != ai)
			{
				ai.PEDestroyEvent -= DelAiObj;
				ai.PEInstantiateEvent -= AddAiObj;
				NetInterface.NetDestroy(ai);
			}
		}
		SPTerrainEvent.OnGroupMonsterDestroy(this);
		_aiList.Clear();
		AiGroupManager.AiGroupList.Remove(this);
	}

	public static void OnGroupMonsterInstantiate(int groupId, AiObject ai)
	{
		if (!(null == ai))
		{
			AIGroupNetWork aIGroupNetWork = ObjNetInterface.Get<AIGroupNetWork>(groupId);
			if (null != aIGroupNetWork)
			{
				ai.PEInstantiateEvent += aIGroupNetWork.AddAiObj;
				ai.PEDestroyEvent += aIGroupNetWork.DelAiObj;
			}
		}
	}

	private void AddAiObj(NetInterface AiObj)
	{
		AiObject item = AiObj as AiObject;
		if (null != AiObj && !_aiList.Contains(item))
		{
			_aiList.Add(item);
		}
	}

	private void DelAiObj(NetInterface AiObj)
	{
		AiObject aiObject = AiObj as AiObject;
		if (null != aiObject)
		{
			aiObject.PEDestroyEvent -= DelAiObj;
			aiObject.PEInstantiateEvent -= AddAiObj;
			_aiList.Remove(aiObject);
			if (_aiList.Count == 0)
			{
				StartCoroutine(DelayDestory());
			}
		}
	}

	private IEnumerator DelayDestory()
	{
		yield return new WaitForSeconds(10f);
		if (_aiList.Count == 0)
		{
			NetInterface.NetDestroy(this);
		}
	}
}
