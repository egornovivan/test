using System.Collections.Generic;
using System.IO;
using Pathea;
using Pathea.PeEntityExt;
using SkillSystem;
using UnityEngine;

public class SceneEntityCreatorArchiver : ArchivableSingleton<SceneEntityCreatorArchiver>
{
	public class AgentInfo : MonsterEntityCreator.AgentInfo
	{
		private int _fixedId = -1;

		public AgentInfo(int fixedId)
			: base(-1f)
		{
			_fixedId = fixedId;
		}

		public override void OnSuceededToCreate(SceneEntityPosAgent agent)
		{
			base.OnSuceededToCreate(agent);
			if (_fixedId != -1)
			{
				PeEntity entityByFixedSpId = PeSingleton<SceneEntityCreatorArchiver>.Instance.GetEntityByFixedSpId(_fixedId);
				if (entityByFixedSpId != null && entityByFixedSpId.aliveEntity != null)
				{
					entityByFixedSpId.aliveEntity.deathEvent += AliveEntityDeathEvent;
				}
			}
		}

		private void AliveEntityDeathEvent(SkEntity arg1, SkEntity arg2)
		{
			FixedSpawnPointInfo info = null;
			if (!PeSingleton<SceneEntityCreatorArchiver>.Instance._fixedSpawnPointInfos.TryGetValue(_fixedId, out info))
			{
				return;
			}
			SceneMan.RemoveSceneObj(info._agent);
			if (info._needCD > 0f)
			{
				MissionManager.Instance.PeTimeToDo(delegate
				{
					if (info._bActive)
					{
						SceneMan.RemoveSceneObj(info._agent);
						SceneMan.AddSceneObj(info._agent);
					}
				}, info._needCD, _fixedId);
			}
			else
			{
				PeSingleton<SceneEntityCreatorArchiver>.Instance.SetFixedSpawnPointActive(_fixedId, active: false);
			}
			PeSingleton<SceneEntityCreatorArchiver>.Instance.GetEntityByFixedSpId(_fixedId).aliveEntity.deathEvent -= AliveEntityDeathEvent;
		}
	}

	public class FixedSpawnPointInfo
	{
		public bool _bActive;

		public SceneEntityPosAgent _agent;

		public float _needCD;
	}

	private Dictionary<int, FixedSpawnPointInfo> _fixedSpawnPointInfos = new Dictionary<int, FixedSpawnPointInfo>();

	private Dictionary<IntVector2, int> _posRectNpcCnt = new Dictionary<IntVector2, int>();

	private Dictionary<IntVector2, SceneEntityPosRect> _entityRects = new Dictionary<IntVector2, SceneEntityPosRect>();

	public void AddFixedSpawnPointToScene(List<int> pointIds)
	{
		List<SceneEntityPosAgent> list = new List<SceneEntityPosAgent>();
		FixedSpawnPointInfo value = null;
		foreach (int pointId in pointIds)
		{
			if (_fixedSpawnPointInfos.TryGetValue(pointId, out value) && value._bActive && (!PeGameMgr.IsMultiStory || !AISpawnPoint.s_spawnPointData[pointId].active))
			{
				list.Add(value._agent);
			}
		}
		SceneMan.AddSceneObjs(list);
	}

	public void RemoveFixedSpawnPointFromScene(List<int> pointIds)
	{
		List<SceneEntityPosAgent> list = new List<SceneEntityPosAgent>();
		FixedSpawnPointInfo value = null;
		foreach (int pointId in pointIds)
		{
			if (_fixedSpawnPointInfos.TryGetValue(pointId, out value) && value._bActive)
			{
				list.Add(value._agent);
			}
		}
		SceneMan.RemoveSceneObjs(list);
	}

	public void SetFixedSpawnPointActive(int pointID, bool active)
	{
		FixedSpawnPointInfo value = null;
		if (!_fixedSpawnPointInfos.TryGetValue(pointID, out value))
		{
			return;
		}
		if (PeGameMgr.IsMultiStory)
		{
			if (PlayerNetwork.mainPlayer != null)
			{
				PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_AI_SetFixActive, pointID, active);
			}
			return;
		}
		value._bActive = active;
		if (null != PeSingleton<SceneEntityCreatorArchiver>.Instance.GetEntityByFixedSpId(pointID))
		{
			if (!active)
			{
				SceneMan.RemoveSceneObj(value._agent);
			}
			else
			{
				ReqReborn(value._agent);
			}
		}
		else if (active)
		{
			value._agent.canRide = false;
			SceneMan.AddSceneObj(value._agent);
		}
	}

	public PeEntity GetEntityByFixedSpId(int pointID)
	{
		FixedSpawnPointInfo value = null;
		if (_fixedSpawnPointInfos.TryGetValue(pointID, out value))
		{
			return value._agent.entity;
		}
		return null;
	}

	public bool GetEntityReviveTimeSpId(int pointID, out float cdTime)
	{
		FixedSpawnPointInfo value = null;
		if (_fixedSpawnPointInfos.TryGetValue(pointID, out value))
		{
			cdTime = value._needCD;
			return true;
		}
		cdTime = 0f;
		return false;
	}

	public SceneEntityPosAgent GetAgentByFixedSpId(int pointID)
	{
		FixedSpawnPointInfo value = null;
		if (_fixedSpawnPointInfos.TryGetValue(pointID, out value))
		{
			return value._agent;
		}
		return null;
	}

	public void SetEntityByFixedSpId(int pointID, PeEntity entity)
	{
		FixedSpawnPointInfo value = null;
		if (_fixedSpawnPointInfos.TryGetValue(pointID, out value))
		{
			value._agent.entity = entity;
		}
	}

	public void ReqReborn(int pointID)
	{
		FixedSpawnPointInfo value = null;
		if (_fixedSpawnPointInfos.TryGetValue(pointID, out value))
		{
			ReqReborn(value._agent);
		}
	}

	private void ReqReborn(SceneEntityPosAgent agent)
	{
		if (!agent.IsIdle)
		{
			return;
		}
		if (agent.entity == null || agent.entity.IsDead())
		{
			agent.entity = null;
			agent.canRide = false;
			SceneMan.RemoveSceneObj(agent);
			SceneMan.AddSceneObj(agent);
			return;
		}
		if (agent.entity != null && agent.entity.monstermountCtrl != null && agent.entity.monstermountCtrl.ctrlType == ECtrlType.Mount)
		{
			agent.entity = null;
			agent.canRide = false;
			SceneMan.AddSceneObj(agent);
			return;
		}
		agent.canRide = false;
		MonsterCmpt component = agent.Go.GetComponent<MonsterCmpt>();
		if (component != null)
		{
			component.Req_MoveToPosition(agent.Pos, 1f, isForce: true, SpeedState.Run);
		}
	}

	private void Init4FixedSpawnPoint()
	{
		foreach (KeyValuePair<int, AISpawnPoint> s_spawnPointDatum in AISpawnPoint.s_spawnPointData)
		{
			AISpawnPoint value = s_spawnPointDatum.Value;
			int num = value.resId;
			if (value.isGroup)
			{
				num |= 0x40000000;
			}
			FixedSpawnPointInfo fixedSpawnPointInfo = new FixedSpawnPointInfo();
			fixedSpawnPointInfo._bActive = value.active;
			fixedSpawnPointInfo._needCD = value.refreshtime;
			fixedSpawnPointInfo._agent = MonsterEntityCreator.CreateAgent(value.Position, num, Vector3.one, Quaternion.Euler(value.euler));
			fixedSpawnPointInfo._agent.spInfo = new AgentInfo(s_spawnPointDatum.Key);
			fixedSpawnPointInfo._agent.FixPos = value.fixPosition;
			_fixedSpawnPointInfos[s_spawnPointDatum.Key] = fixedSpawnPointInfo;
		}
	}

	private void SetData4FixedSpawnPoint(BinaryReader br)
	{
		FixedSpawnPointInfo value = null;
		int num = br.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			int key = br.ReadInt32();
			bool bActive = br.ReadBoolean();
			if (_fixedSpawnPointInfos.TryGetValue(key, out value))
			{
				value._bActive = bActive;
			}
		}
	}

	private void WriteData4FixedSpawnPoint(BinaryWriter bw)
	{
		bw.Write(_fixedSpawnPointInfos.Count);
		foreach (KeyValuePair<int, FixedSpawnPointInfo> fixedSpawnPointInfo in _fixedSpawnPointInfos)
		{
			bw.Write(fixedSpawnPointInfo.Key);
			bw.Write(fixedSpawnPointInfo.Value._bActive);
		}
	}

	private void Init4PosRectNpcCnt()
	{
		_posRectNpcCnt.Clear();
	}

	private void SetData4PosRectNpcCnt(BinaryReader br)
	{
		int num = br.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			int x_ = br.ReadInt32();
			int y_ = br.ReadInt32();
			int value = br.ReadInt32();
			_posRectNpcCnt[new IntVector2(x_, y_)] = value;
		}
	}

	private void WriteData4PosRectNpcCnt(BinaryWriter bw)
	{
		if (SceneEntityPosRect.EntityNpcNum == 0)
		{
			bw.Write(0);
			return;
		}
		bw.Write(_posRectNpcCnt.Count);
		foreach (KeyValuePair<IntVector2, int> item in _posRectNpcCnt)
		{
			IntVector2 key = item.Key;
			bw.Write(key.x);
			bw.Write(key.y);
			if (_entityRects.TryGetValue(item.Key, out var value))
			{
				bw.Write(value.CntNpcNotCreated);
			}
			else
			{
				bw.Write(item.Value);
			}
		}
	}

	public void FillPosRect(int ix, int iz)
	{
		IntVector2 tmp = IntVector2.Tmp;
		tmp.x = ix;
		tmp.y = iz;
		if (!_entityRects.TryGetValue(tmp, out var value))
		{
			int value2 = SceneEntityPosRect.EntityNpcNum;
			int entityMonsterNum = SceneEntityPosRect.EntityMonsterNum;
			IntVector2 key = new IntVector2(ix, iz);
			if (!_posRectNpcCnt.TryGetValue(key, out value2))
			{
				value2 = SceneEntityPosRect.EntityNpcNum;
				_posRectNpcCnt[key] = value2;
			}
			if ((SceneEntityPosRect.EntityNpcNum != 0 && Mathf.Abs(ix) > 16) || Mathf.Abs(iz) > 16)
			{
				value2 = 0;
			}
			value = new SceneEntityPosRect(ix, iz);
			value.Fill(value2, entityMonsterNum);
			_entityRects[key] = value;
		}
	}

	protected override void OnInit()
	{
		base.OnInit();
		Init4FixedSpawnPoint();
	}

	protected override void SetData(byte[] data)
	{
		if (data == null)
		{
			return;
		}
		using MemoryStream memoryStream = new MemoryStream(data);
		using BinaryReader binaryReader = new BinaryReader(memoryStream);
		SetData4FixedSpawnPoint(binaryReader);
		if (memoryStream.Position < memoryStream.Length)
		{
			SetData4PosRectNpcCnt(binaryReader);
		}
	}

	protected override void WriteData(BinaryWriter bw)
	{
		WriteData4FixedSpawnPoint(bw);
		WriteData4PosRectNpcCnt(bw);
	}
}
