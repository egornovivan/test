using System;
using System.Collections;
using System.Collections.Generic;
using uLink;
using UnityEngine;

public class AiTowerDefense : AiCommonTD
{
	public class TDInfo
	{
		public int missionId;

		public int teamId;

		public TDInfo(int mId, int t)
		{
			missionId = mId;
			teamId = t;
		}
	}

	public static List<TDInfo> _tdList = new List<TDInfo>();

	protected int _missionId;

	protected int _targetId;

	protected bool isStart;

	public static void AddTowerInfo(int mId, int t)
	{
		bool flag = false;
		foreach (TDInfo td in _tdList)
		{
			if (td.teamId == t)
			{
				flag = true;
			}
		}
		if (!flag)
		{
			_tdList.Add(new TDInfo(mId, t));
		}
	}

	protected override void OnPEInstantiate(uLink.NetworkMessageInfo info)
	{
		_id = info.networkView.initialData.Read<int>(new object[0]);
		_missionId = info.networkView.initialData.Read<int>(new object[0]);
		_targetId = info.networkView.initialData.Read<int>(new object[0]);
		base.authId = info.networkView.initialData.Read<int>(new object[0]);
		_teamId = info.networkView.initialData.Read<int>(new object[0]);
		_worldId = info.networkView.group.id;
		isStart = false;
		AddTowerInfo(_missionId, _teamId);
		Player.PlayerDisconnected += OnPlayerDisconnect;
		if (LogFilter.logDebug)
		{
			Debug.LogFormat("MissionTD:{0} Target ID:{1}", _missionId, _targetId);
		}
	}

	protected override void OnPEStart()
	{
		base.OnPEStart();
		BindAction(EPacketType.PT_InGame_TDInitData, RPC_C2S_RequestInitData);
		BindAction(EPacketType.PT_InGame_TDStartInfo, RPC_C2S_TDStartInfo);
		BindAction(EPacketType.PT_InGame_TDInfo, RPC_C2S_TDInfo);
	}

	protected override void OnPEDestroy()
	{
		Player.PlayerDisconnected -= OnPlayerDisconnect;
		StopAllCoroutines();
		base.OnPEDestroy();
		foreach (TDInfo td in _tdList)
		{
			if (td.teamId == base.TeamId)
			{
				_tdList.Remove(td);
				break;
			}
		}
	}

	protected override void OnPlayerDisconnect(Player player)
	{
		if (!(null == player) && player.Id == base.authId)
		{
			NetInterface.NetDestroy(this);
			PlayerMission curPlayerMissionByMissionId = MissionManager.Manager.GetCurPlayerMissionByMissionId(base.authId, _missionId);
			if (curPlayerMissionByMissionId != null)
			{
				curPlayerMissionByMissionId.FailureMission(player, _missionId);
				TowerDefenseFinish(_missionId);
			}
		}
	}

	public override void OnMonsterDeath(SkNetworkInterface ai)
	{
		base.OnMonsterDeath(ai);
		if (_missionId == -1)
		{
			return;
		}
		MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(_missionId);
		if (missionCommonData == null)
		{
			return;
		}
		int num = -1;
		for (int i = 0; i < missionCommonData.m_TargetIDList.Count; i++)
		{
			if (missionCommonData.m_TargetIDList[i] == _targetId)
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			return;
		}
		TypeTowerDefendsData typeTowerDefendsData = MissionManager.GetTypeTowerDefendsData(_targetId);
		if (typeTowerDefendsData == null)
		{
			return;
		}
		Player player = Player.GetPlayer(base.authId);
		if (!(null != player))
		{
			return;
		}
		PlayerMission curPlayerMissionByMissionId = MissionManager.Manager.GetCurPlayerMissionByMissionId(base.authId, _missionId);
		if (curPlayerMissionByMissionId != null)
		{
			string questVariable = curPlayerMissionByMissionId.GetQuestVariable(_missionId, "TDMONS" + num);
			string[] array = questVariable.Split('_');
			int num2 = ((array.Length >= 2) ? Convert.ToInt32(array[1]) : 0);
			num2++;
			if (num2 < typeTowerDefendsData.m_Count)
			{
				questVariable = "0_" + num2 + "_0_1_0";
				curPlayerMissionByMissionId.ModifyQuestVariable(_missionId, "TDMONS" + num, questVariable, player);
			}
			else if (num2 == typeTowerDefendsData.m_Count)
			{
				questVariable = "0_" + num2 + "_0_1_1";
				curPlayerMissionByMissionId.ModifyQuestVariable(_missionId, "TDMONS" + num, questVariable, player);
				curPlayerMissionByMissionId.CompleteTarget(_targetId, _missionId, player);
			}
		}
	}

	public static bool IsOnlyOneLimit(int mId, int teamId)
	{
		if (mId == -1)
		{
			foreach (TDInfo td in _tdList)
			{
				if (td.teamId == teamId)
				{
					return true;
				}
			}
		}
		else
		{
			foreach (TDInfo td2 in _tdList)
			{
				if (td2.teamId == teamId && td2.missionId == mId)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static void OnTDMonsterInstantiate(int tdId, AiObject ai)
	{
		if (!(null == ai) && tdId != -1)
		{
			AiTowerDefense aiTowerDefense = ObjNetInterface.Get<AiTowerDefense>(tdId);
			if (null != aiTowerDefense)
			{
				ai.PEInstantiateEvent += aiTowerDefense.OnMonsterInstantiate;
				ai.DeathEventHandler += aiTowerDefense.OnMonsterDeath;
			}
		}
	}

	public static void TowerDefenseFinish(int missionId)
	{
		List<AiTowerDefense> list = ObjNetInterface.Get<AiTowerDefense>();
		foreach (AiTowerDefense item in list)
		{
			if (item._missionId != -1 && item._missionId == missionId)
			{
				NetInterface.NetDestroy(item);
				ChannelNetwork.SyncChannel(item.WorldId, EPacketType.PT_InGame_TowerDefense);
			}
		}
	}

	protected void RPC_C2S_RequestInitData(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (isStart)
		{
			float num = (float)((double)Time.time - startTime);
			bool flag = coolTime <= num;
			coolTime -= num;
			coolTime = Mathf.Clamp(coolTime, 0f, 1000f);
			if (flag)
			{
				preTime -= num - coolTime;
				preTime = Mathf.Clamp(preTime, 0f, 1000f);
			}
			RPCPeer(info.sender, EPacketType.PT_InGame_TDInitData, totalWave, deathCount, waveIndex, preTime, coolTime);
		}
	}

	protected void RPC_C2S_TDStartInfo(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		totalWave = stream.Read<int>(new object[0]);
		preTime = stream.Read<float>(new object[0]);
		startTime = Time.time;
		isStart = true;
		RPCOthers(EPacketType.PT_InGame_TDStartInfo, totalWave, 0, 0, preTime);
	}

	protected void RPC_C2S_TDInfo(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		totalCount = stream.Read<int>(new object[0]);
		waveIndex = stream.Read<int>(new object[0]);
		preTime = stream.Read<float>(new object[0]);
		coolTime = stream.Read<float>(new object[0]);
		startTime = Time.time;
		RPCOthers(EPacketType.PT_InGame_TDInfo, totalCount, deathCount, waveIndex, preTime, coolTime);
		if (_missionId == -1 && waveIndex == totalWave)
		{
			StartCoroutine(DelayDestory());
		}
	}

	private IEnumerator DelayDestory()
	{
		yield return new WaitForSeconds(30f);
		NetInterface.NetDestroy(this);
	}
}
