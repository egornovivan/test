using System.Collections.Generic;
using Pathea;
using uLink;
using UnityEngine;

public class AIGroupNetWork : NetworkInterface
{
	protected int _externId;

	protected int _tdId;

	protected int _dungeonId;

	protected int _colorType;

	protected int _playerId;

	protected int _buffId;

	protected List<AiNetwork> _aiList = new List<AiNetwork>();

	public int ExternId => _externId;

	protected override void OnPEDestroy()
	{
		if (!(null == base.Runner))
		{
			base.Runner.InitNetworkLayer(null);
			if (base.Runner != null)
			{
				Object.Destroy(base.Runner.gameObject);
			}
		}
	}

	protected override void OnPEInstantiate(uLink.NetworkMessageInfo info)
	{
		_id = info.networkView.initialData.Read<int>(new object[0]);
		_externId = info.networkView.initialData.Read<int>(new object[0]);
		base.authId = info.networkView.initialData.Read<int>(new object[0]);
		_tdId = info.networkView.initialData.Read<int>(new object[0]);
		_dungeonId = info.networkView.initialData.Read<int>(new object[0]);
		_colorType = info.networkView.initialData.Read<int>(new object[0]);
		_playerId = info.networkView.initialData.Read<int>(new object[0]);
		_buffId = info.networkView.initialData.Read<int>(new object[0]);
		_aiList.Clear();
		EntityGrp.CreateMonsterGroup(ExternId & -1073741825, base.transform.position, _colorType, _playerId, base.Id, _buffId);
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(base.Id);
		if (!(null == peEntity))
		{
			OnSpawned(peEntity.GetGameObject());
		}
	}

	public static void OnMonsterAdd(int id, AiNetwork ai, PeEntity entity)
	{
		EntityGrp entityGrp = PeSingleton<EntityMgr>.Instance.Get(id) as EntityGrp;
		if (null != entityGrp)
		{
			entityGrp.OnMemberCreated(entity);
		}
	}

	private void AddAiObj(AiNetwork ai)
	{
		if (!(null == ai) && !_aiList.Contains(ai))
		{
			_aiList.Add(ai);
		}
	}

	private void DelAiObj(AiNetwork ai)
	{
		if (!(null == ai) && _aiList.Contains(ai))
		{
			_aiList.Remove(ai);
		}
	}
}
