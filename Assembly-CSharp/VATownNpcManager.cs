using System.Collections;
using System.Collections.Generic;
using System.IO;
using Pathea;
using TownData;
using UnityEngine;

public class VATownNpcManager : MonoBehaviour
{
	private static VATownNpcManager mInstance;

	public Dictionary<IntVector2, VATownNpcInfo> npcInfoMap;

	public List<int> createdNpcIdList;

	public List<IntVector2> createdPosList;

	public static VATownNpcManager Instance => mInstance;

	private void Awake()
	{
		mInstance = this;
		npcInfoMap = new Dictionary<IntVector2, VATownNpcInfo>();
		createdNpcIdList = new List<int>();
		createdPosList = new List<IntVector2>();
	}

	public void Clear()
	{
		npcInfoMap = new Dictionary<IntVector2, VATownNpcInfo>();
		createdNpcIdList = new List<int>();
		createdPosList = new List<IntVector2>();
	}

	public bool AddNpc(VATownNpcInfo npcInfo)
	{
		IntVector2 key = new IntVector2(Mathf.RoundToInt(npcInfo.getPos().x), Mathf.RoundToInt(npcInfo.getPos().z));
		if (npcInfoMap.ContainsKey(key))
		{
			return false;
		}
		npcInfoMap.Add(key, npcInfo);
		return true;
	}

	public bool IsCreated(IntVector2 pos)
	{
		return createdPosList.Contains(pos);
	}

	public bool IsCreated(Vector3 pos)
	{
		IntVector2 item = new IntVector2(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.z));
		return createdPosList.Contains(item);
	}

	public void RenderTownNPC(VATownNpcInfo townNpcInfo)
	{
		IntVector2 index = townNpcInfo.Index;
		if (!npcInfoMap.ContainsKey(index))
		{
			return;
		}
		int id = townNpcInfo.getId();
		if (PeGameMgr.IsSingleAdventure)
		{
			int enemyNpcId = GetEnemyNpcId(id);
			int allyId = VArtifactTownManager.Instance.GetTownByID(townNpcInfo.townId).AllyId;
			int allyColor = VATownGenerator.Instance.GetAllyColor(allyId);
			int playerId = VATownGenerator.Instance.GetPlayerId(allyId);
			if (allyId != 0)
			{
				SceneEntityPosAgent sceneEntityPosAgent = MonsterEntityCreator.CreateAdAgent(townNpcInfo.getPos(), enemyNpcId, allyColor, playerId);
				SceneMan.AddSceneObj(sceneEntityPosAgent);
				VArtifactTownManager.Instance.AddMonsterPointAgent(townNpcInfo.townId, sceneEntityPosAgent);
			}
			else
			{
				PeEntity peEntity = NpcEntityCreator.CreateNpc(id, townNpcInfo.getPos(), Vector3.one, Quaternion.Euler(0f, 180f, 0f));
				if (peEntity == null)
				{
					Debug.LogError("npc id error: templateId = " + id);
					return;
				}
				VArtifactUtil.SetNpcStandRot(peEntity, 180f, isStand: false);
				if (id == VArtifactTownManager.Instance.missionStartNpcID)
				{
					VArtifactTownManager.Instance.missionStartNpcEntityId = peEntity.Id;
				}
				createdPosList.Add(index);
			}
			npcInfoMap.Remove(index);
		}
		else if (GameConfig.IsMultiMode)
		{
			int enemyNpcId2 = GetEnemyNpcId(id);
			int allyId2 = VArtifactTownManager.Instance.GetTownByID(townNpcInfo.townId).AllyId;
			int allyColor2 = VATownGenerator.Instance.GetAllyColor(allyId2);
			int playerId2 = VATownGenerator.Instance.GetPlayerId(allyId2);
			if (allyId2 != 0)
			{
				SceneEntityPosAgent sceneEntityPosAgent2 = MonsterEntityCreator.CreateAdAgent(townNpcInfo.getPos(), enemyNpcId2, allyColor2, playerId2);
				SceneMan.AddSceneObj(sceneEntityPosAgent2);
				VArtifactTownManager.Instance.AddMonsterPointAgent(townNpcInfo.townId, sceneEntityPosAgent2);
			}
			else
			{
				StartCoroutine(RenderOneNpc(townNpcInfo.getPos(), id));
			}
			npcInfoMap.Remove(index);
		}
	}

	private IEnumerator RenderOneNpc(Vector3 pos, int id)
	{
		while (PeSingleton<PeCreature>.Instance.mainPlayer == null)
		{
			yield return null;
		}
		PlayerNetwork.mainPlayer.RequestTownNpc(pos, id, 0, isStand: false, 0f);
	}

	public int GetRNpcIdByPos(IntVector2 posXZ)
	{
		if (!npcInfoMap.ContainsKey(posXZ))
		{
			return -1;
		}
		return npcInfoMap[posXZ].getId();
	}

	public void Export(BinaryWriter bw)
	{
		bw.Write(createdNpcIdList.Count);
		for (int i = 0; i < createdNpcIdList.Count; i++)
		{
			int num = createdNpcIdList[i];
			NpcMissionData missionData = NpcMissionDataRepository.GetMissionData(num);
			if (missionData == null)
			{
				bw.Write(-1);
				continue;
			}
			bw.Write(num);
			bw.Write(missionData.m_Rnpc_ID);
			bw.Write(missionData.m_QCID);
			bw.Write(missionData.m_CurMissionGroup);
			bw.Write(missionData.m_CurGroupTimes);
			bw.Write(missionData.mCurComMisNum);
			bw.Write(missionData.mCompletedMissionCount);
			bw.Write(missionData.m_RandomMission);
			bw.Write(missionData.m_RecruitMissionNum);
			bw.Write(missionData.m_MissionList.Count);
			for (int j = 0; j < missionData.m_MissionList.Count; j++)
			{
				bw.Write(missionData.m_MissionList[j]);
			}
			bw.Write(missionData.m_MissionListReply.Count);
			for (int k = 0; k < missionData.m_MissionListReply.Count; k++)
			{
				bw.Write(missionData.m_MissionListReply[k]);
			}
		}
		bw.Write(createdPosList.Count);
		for (int l = 0; l < createdPosList.Count; l++)
		{
			IntVector2 intVector = createdPosList[l];
			bw.Write(intVector.x);
			bw.Write(intVector.y);
		}
	}

	public void Import(byte[] buffer)
	{
		LogManager.Info("TownNpcManager.Instance.Import()");
		Clear();
		if (buffer.Length == 0)
		{
			return;
		}
		MemoryStream memoryStream = new MemoryStream(buffer);
		BinaryReader binaryReader = new BinaryReader(memoryStream);
		int num = binaryReader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			NpcMissionData npcMissionData = new NpcMissionData();
			int num2 = binaryReader.ReadInt32();
			if (num2 != -1)
			{
				npcMissionData.m_Rnpc_ID = binaryReader.ReadInt32();
				npcMissionData.m_QCID = binaryReader.ReadInt32();
				npcMissionData.m_CurMissionGroup = binaryReader.ReadInt32();
				npcMissionData.m_CurGroupTimes = binaryReader.ReadInt32();
				npcMissionData.mCurComMisNum = binaryReader.ReadByte();
				npcMissionData.mCompletedMissionCount = binaryReader.ReadInt32();
				npcMissionData.m_RandomMission = binaryReader.ReadInt32();
				npcMissionData.m_RecruitMissionNum = binaryReader.ReadInt32();
				int num3 = binaryReader.ReadInt32();
				for (int j = 0; j < num3; j++)
				{
					npcMissionData.m_MissionList.Add(binaryReader.ReadInt32());
				}
				num3 = binaryReader.ReadInt32();
				for (int k = 0; k < num3; k++)
				{
					npcMissionData.m_MissionListReply.Add(binaryReader.ReadInt32());
				}
				createdNpcIdList.Add(num2);
				NpcMissionDataRepository.AddMissionData(num2, npcMissionData);
			}
		}
		num = binaryReader.ReadInt32();
		for (int l = 0; l < num; l++)
		{
			int x_ = binaryReader.ReadInt32();
			int y_ = binaryReader.ReadInt32();
			createdPosList.Add(new IntVector2(x_, y_));
		}
		binaryReader.Close();
		memoryStream.Close();
	}

	public void InitAdTownNpc()
	{
		StartCoroutine(InitAdNpc());
	}

	private IEnumerator InitAdNpc()
	{
		yield return 0;
	}

	public int GetEnemyNpcId(int id)
	{
		return NpcMissionDataRepository.GetRNpcId(id) + 9900;
	}

	public void GenEnemyNpc(List<BuildingNpc> bNpcs, int townId, int allyId)
	{
		foreach (BuildingNpc bNpc in bNpcs)
		{
			int enemyNpcId = GetEnemyNpcId(bNpc.templateId);
			int allyColor = VATownGenerator.Instance.GetAllyColor(allyId);
			int playerId = VATownGenerator.Instance.GetPlayerId(allyId);
			SceneEntityPosAgent sceneEntityPosAgent = MonsterEntityCreator.CreateAdAgent(bNpc.pos, enemyNpcId, allyColor, playerId);
			SceneMan.AddSceneObj(sceneEntityPosAgent);
			VArtifactTownManager.Instance.AddMonsterPointAgent(townId, sceneEntityPosAgent);
		}
	}
}
