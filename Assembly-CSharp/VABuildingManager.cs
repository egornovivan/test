using System.Collections;
using System.Collections.Generic;
using System.IO;
using Pathea;
using UnityEngine;

public class VABuildingManager : MonoBehaviour
{
	private static VABuildingManager mInstance;

	public Dictionary<int, BuildingID> missionBuilding;

	public Dictionary<BuildingID, VABuildingInfo> mBuildingInfoMap;

	public Dictionary<BuildingID, int> mCreatedNpcBuildingID;

	public List<BuildingNpcIdStand> createdNpcIdList;

	private static int itemSum;

	public static VABuildingManager Instance => mInstance;

	private void Awake()
	{
		mInstance = this;
		missionBuilding = new Dictionary<int, BuildingID>();
		mBuildingInfoMap = new Dictionary<BuildingID, VABuildingInfo>();
		mCreatedNpcBuildingID = new Dictionary<BuildingID, int>();
		createdNpcIdList = new List<BuildingNpcIdStand>();
	}

	public void Clear()
	{
		missionBuilding.Clear();
		mBuildingInfoMap.Clear();
		mCreatedNpcBuildingID.Clear();
		createdNpcIdList.Clear();
		itemSum = 0;
	}

	public VABuildingInfo GetBuildingInfoByBuildingID(BuildingID buildingid)
	{
		if (!mBuildingInfoMap.ContainsKey(buildingid))
		{
			return null;
		}
		return mBuildingInfoMap[buildingid];
	}

	public void AddBuilding(VABuildingInfo bdinfo)
	{
		if (!mBuildingInfoMap.ContainsKey(bdinfo.buildingId))
		{
			if (!GameConfig.IsMultiMode)
			{
				mBuildingInfoMap.Add(bdinfo.buildingId, bdinfo);
			}
			else
			{
				mBuildingInfoMap.Add(bdinfo.buildingId, bdinfo);
			}
		}
	}

	public void RenderBuilding(BuildingID buildingId)
	{
		VABuildingInfo buildingInfoByBuildingID = GetBuildingInfoByBuildingID(buildingId);
		if (buildingInfoByBuildingID != null)
		{
			RenderBuilding(buildingInfoByBuildingID);
		}
	}

	public void RenderBuilding(VABuildingInfo b)
	{
		BuildingID buildingId = b.buildingId;
		if (b.pos.y == -1f)
		{
			LogManager.Error("building [", buildingId, "] height not initialized!");
		}
		else
		{
			RenderPrefebBuilding(b);
		}
	}

	private IEnumerator CreateBuildingNpcList(List<BuildingNpc> buildingNpcList)
	{
		if (PeGameMgr.IsMulti)
		{
			while (PeSingleton<PeCreature>.Instance.mainPlayer == null)
			{
				yield return null;
			}
		}
		for (int i = 0; i < buildingNpcList.Count; i++)
		{
			CreateOneBuildingNpc(buildingNpcList[i]);
			yield return null;
		}
	}

	private void OnSpawned(GameObject go)
	{
		if (!(go == null))
		{
		}
	}

	private void CreateOneBuildingNpc(BuildingNpc buildingNpc)
	{
		if (!NpcMissionDataRepository.m_AdRandMisNpcData.ContainsKey(buildingNpc.templateId))
		{
			LogManager.Error("not exist! id = [", buildingNpc.templateId, "] Pos = ", buildingNpc.pos);
			return;
		}
		buildingNpc.pos.y += 0.2f;
		if (PeGameMgr.IsSingleAdventure)
		{
			PeEntity peEntity = NpcEntityCreator.CreateNpc(buildingNpc.templateId, buildingNpc.pos, Vector3.one, Quaternion.Euler(0f, buildingNpc.rotY, 0f));
			if (peEntity == null)
			{
				Debug.LogError("npc id error: templateId = " + buildingNpc.templateId);
			}
			else if (buildingNpc.isStand)
			{
				VArtifactUtil.SetNpcStandRot(peEntity, buildingNpc.rotY, isStand: true);
				createdNpcIdList.Add(new BuildingNpcIdStand(peEntity.Id, isStand: true, buildingNpc.rotY));
			}
			else
			{
				createdNpcIdList.Add(new BuildingNpcIdStand(peEntity.Id, isStand: false, buildingNpc.rotY));
			}
		}
		else if (GameConfig.IsMultiMode)
		{
			PlayerNetwork.mainPlayer.RequestTownNpc(buildingNpc.pos, buildingNpc.templateId, 1, buildingNpc.isStand, buildingNpc.rotY);
		}
	}

	private IEnumerator CreateMapItemInBuilding(BuildingID buildingId, List<CreatItemInfo> itemInfoList, Vector3 root, int id, int rotation)
	{
		if (PeGameMgr.IsSingleAdventure)
		{
			for (int i = 0; i < itemInfoList.Count; i++)
			{
				DragArticleAgent.PutItemByProroId(itemInfoList[i].mItemId, itemInfoList[i].mPos, itemInfoList[i].mRotation);
				yield return null;
			}
			mCreatedNpcBuildingID.Add(buildingId, 0);
		}
		else if (GameConfig.IsMultiMode)
		{
			while (PeSingleton<PeCreature>.Instance.mainPlayer == null)
			{
				yield return null;
			}
			yield return null;
		}
		yield return null;
	}

	public void RenderPrefebBuilding(VABuildingInfo buildinginfo)
	{
		int id = buildinginfo.id;
		Quaternion quaternion = Quaternion.Euler(0f, buildinginfo.rotation, 0f);
		if (PeGameMgr.IsSingleAdventure)
		{
			if (buildinginfo.buildingId.buildingNo != -1)
			{
				if (!BlockBuilding.s_tblBlockBuildingMap.ContainsKey(id))
				{
					LogManager.Error("bid = [", id, "] not exist in database!");
					return;
				}
				int campId = 28;
				int damageId = 28;
				if (buildinginfo.vau.vat.type == VArtifactType.NpcTown)
				{
					if (!buildinginfo.vau.vat.IsPlayerTown)
					{
						if (id == 63 || id == 64)
						{
							return;
						}
						campId = 8;
						damageId = 8;
					}
				}
				else if (buildinginfo.vau.vat.nativeType == NativeType.Puja)
				{
					campId = 15;
					damageId = 15;
				}
				else
				{
					campId = 18;
					damageId = 18;
				}
				int playerId = VATownGenerator.Instance.GetPlayerId(buildinginfo.vau.vat.AllyId);
				if (!buildinginfo.vau.isDoodadNpcRendered)
				{
					VArtifactTownManager.Instance.AddAliveBuilding(buildinginfo.vau.vat.townId, DoodadEntityCreator.CreateRandTerDoodad(BlockBuilding.s_tblBlockBuildingMap[id].mDoodadProtoId, buildinginfo.root, Vector3.one, quaternion, buildinginfo.buildingId.townId, campId, damageId, playerId).Id);
				}
				if (!mCreatedNpcBuildingID.ContainsKey(buildinginfo.buildingId))
				{
					BlockBuilding blockBuilding = BlockBuilding.s_tblBlockBuildingMap[id];
					blockBuilding.GetNpcInfo(out var buildingNpcs);
					for (int i = 0; i < buildingNpcs.Count; i++)
					{
						BuildingNpc buildingNpc = buildingNpcs[i];
						VArtifactUtil.GetPosRotFromPointRot(ref buildingNpc.pos, ref buildingNpc.rotY, buildinginfo.root, buildinginfo.rotation);
					}
					if (buildingNpcs != null && buildingNpcs.Count > 0)
					{
						if (buildinginfo.vau.vat.IsPlayerTown)
						{
							if (!buildinginfo.vau.isDoodadNpcRendered)
							{
								StartCoroutine(CreateBuildingNpcList(buildingNpcs));
								mCreatedNpcBuildingID.Add(buildinginfo.buildingId, 0);
							}
						}
						else
						{
							GenEnemyNpc(buildingNpcs, buildinginfo.vau.vat.townId, buildinginfo.vau.vat.AllyId);
						}
					}
				}
			}
			else if (buildinginfo.buildingId.buildingNo == -1 && !buildinginfo.vau.isDoodadNpcRendered)
			{
				int playerId2 = VATownGenerator.Instance.GetPlayerId(buildinginfo.vau.vat.AllyId);
				VArtifactTownManager.Instance.AddAliveBuilding(buildinginfo.vau.vat.townId, DoodadEntityCreator.CreateRandTerDoodad(buildinginfo.pathID, buildinginfo.root, Vector3.one, quaternion, buildinginfo.vau.vat.townId, buildinginfo.campID, buildinginfo.damageID, playerId2).Id);
			}
			if (missionBuilding.ContainsKey(0))
			{
				if (buildinginfo.buildingId != missionBuilding[0])
				{
					RemoveBuilding(buildinginfo.buildingId);
				}
			}
			else
			{
				RemoveBuilding(buildinginfo.buildingId);
			}
		}
		else
		{
			if (!PeGameMgr.IsMulti)
			{
				return;
			}
			if (buildinginfo.buildingId.buildingNo != -1)
			{
				if (!BlockBuilding.s_tblBlockBuildingMap.ContainsKey(id))
				{
					LogManager.Error("bid = [", id, "] not exist in database!");
					return;
				}
				Debug.Log("RenderPrefebBuilding():" + id);
				int num = 28;
				int num2 = 28;
				if (buildinginfo.vau.vat.type == VArtifactType.NpcTown)
				{
					if (!buildinginfo.vau.vat.IsPlayerTown)
					{
						if (id == 63 || id == 64)
						{
							return;
						}
						num = 8;
						num2 = 8;
					}
				}
				else if (buildinginfo.vau.vat.nativeType == NativeType.Puja)
				{
					num = 15;
					num2 = 15;
				}
				else
				{
					num = 18;
					num2 = 18;
				}
				int playerId3 = VATownGenerator.Instance.GetPlayerId(buildinginfo.vau.vat.AllyId);
				if (!buildinginfo.vau.isDoodadNpcRendered)
				{
					PlayerNetwork.RequestServer(EPacketType.PT_Common_TownDoodad, buildinginfo.buildingId, BlockBuilding.s_tblBlockBuildingMap[id].mDoodadProtoId, buildinginfo.root, Vector3.one, quaternion, buildinginfo.vau.vat.townId, num, num2, playerId3);
				}
				if (!mCreatedNpcBuildingID.ContainsKey(buildinginfo.buildingId))
				{
					BlockBuilding blockBuilding2 = BlockBuilding.s_tblBlockBuildingMap[id];
					blockBuilding2.GetNpcInfo(out var buildingNpcs2);
					for (int j = 0; j < buildingNpcs2.Count; j++)
					{
						BuildingNpc buildingNpc2 = buildingNpcs2[j];
						VArtifactUtil.GetPosRotFromPointRot(ref buildingNpc2.pos, ref buildingNpc2.rotY, buildinginfo.root, buildinginfo.rotation);
					}
					if (buildingNpcs2 != null && buildingNpcs2.Count > 0 && buildingNpcs2 != null && buildingNpcs2.Count > 0)
					{
						if (buildinginfo.vau.vat.IsPlayerTown)
						{
							if (!buildinginfo.vau.isDoodadNpcRendered)
							{
								StartCoroutine(CreateBuildingNpcList(buildingNpcs2));
								mCreatedNpcBuildingID.Add(buildinginfo.buildingId, 0);
							}
						}
						else
						{
							GenEnemyNpc(buildingNpcs2, buildinginfo.vau.vat.townId, buildinginfo.vau.vat.AllyId);
						}
					}
				}
			}
			else if (buildinginfo.buildingId.buildingNo == -1 && !buildinginfo.vau.isDoodadNpcRendered)
			{
				int playerId4 = VATownGenerator.Instance.GetPlayerId(buildinginfo.vau.vat.AllyId);
				PlayerNetwork.RequestServer(EPacketType.PT_Common_NativeTowerCreate, buildinginfo.buildingId, buildinginfo.pathID, buildinginfo.root, Vector3.one, quaternion, buildinginfo.vau.vat.townId, buildinginfo.campID, buildinginfo.damageID, playerId4);
			}
			RemoveBuilding(buildinginfo.buildingId);
		}
	}

	public void GenEnemyNpc(List<BuildingNpc> bNpcs, int townId, int allyId)
	{
		VATownNpcManager.Instance.GenEnemyNpc(bNpcs, townId, allyId);
	}

	public void Export(BinaryWriter bw)
	{
		bw.Write(createdNpcIdList.Count);
		for (int i = 0; i < createdNpcIdList.Count; i++)
		{
			BuildingNpcIdStand buildingNpcIdStand = createdNpcIdList[i];
			int npcId = buildingNpcIdStand.npcId;
			NpcMissionData missionData = NpcMissionDataRepository.GetMissionData(npcId);
			if (missionData == null)
			{
				bw.Write(-1);
				continue;
			}
			bw.Write(npcId);
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
			bw.Write(buildingNpcIdStand.isStand);
			bw.Write(buildingNpcIdStand.rotY);
		}
		bw.Write(mCreatedNpcBuildingID.Count);
		foreach (BuildingID key in mCreatedNpcBuildingID.Keys)
		{
			bw.Write(key.townId);
			bw.Write(key.buildingNo);
		}
	}

	public void Import(byte[] buffer)
	{
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
				bool isStand = binaryReader.ReadBoolean();
				float rotY = binaryReader.ReadSingle();
				createdNpcIdList.Add(new BuildingNpcIdStand(num2, isStand, rotY));
				NpcMissionDataRepository.AddMissionData(num2, npcMissionData);
			}
		}
		num = binaryReader.ReadInt32();
		for (int l = 0; l < num; l++)
		{
			BuildingID buildingID = new BuildingID();
			buildingID.townId = binaryReader.ReadInt32();
			buildingID.buildingNo = binaryReader.ReadInt32();
			mCreatedNpcBuildingID.Add(buildingID, 0);
		}
		binaryReader.Close();
		memoryStream.Close();
	}

	public void InitAdBuildingNpc()
	{
		StartCoroutine(InitAdNpc());
	}

	private IEnumerator InitAdNpc()
	{
		while (PeSingleton<PeCreature>.Instance.mainPlayer == null)
		{
			yield return new WaitForSeconds(0.1f);
		}
	}

	public Vector3 GetMissionBuildingPos(int missionId = 0)
	{
		if (missionBuilding.ContainsKey(missionId))
		{
			VABuildingInfo buildingInfoByBuildingID = GetBuildingInfoByBuildingID(missionBuilding[missionId]);
			if (buildingInfoByBuildingID != null)
			{
				return buildingInfoByBuildingID.frontDoorPos;
			}
		}
		return Vector3.zero;
	}

	public void RemoveBuilding(BuildingID buildingId)
	{
		if (mBuildingInfoMap.ContainsKey(buildingId))
		{
			mBuildingInfoMap.Remove(buildingId);
		}
	}
}
