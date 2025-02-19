using System;
using System.Collections.Generic;
using Pathea;
using UnityEngine;

public class KillNPC
{
	public static int ashBox_inScene;

	private static GameObject go;

	public static int burriedNum;

	public static List<PeEntity> NPCBeKilled(int npcNum)
	{
		List<PeEntity> list = new List<PeEntity>();
		if (npcNum == 25001 && ServantLeaderCmpt.Instance.GetServant(0) != null)
		{
			list.Add(ServantLeaderCmpt.Instance.GetServant(0).GetComponent<PeEntity>());
		}
		else if (npcNum == 25002 && ServantLeaderCmpt.Instance.GetServant(1) != null)
		{
			list.Add(ServantLeaderCmpt.Instance.GetServant(1).GetComponent<PeEntity>());
		}
		else if (npcNum == 25010)
		{
			if (ServantLeaderCmpt.Instance.GetServant(0) != null)
			{
				list.Add(ServantLeaderCmpt.Instance.GetServant(0).GetComponent<PeEntity>());
			}
			if (ServantLeaderCmpt.Instance.GetServant(1) != null)
			{
				list.Add(ServantLeaderCmpt.Instance.GetServant(1).GetComponent<PeEntity>());
			}
		}
		else
		{
			list.Add(PeSingleton<EntityMgr>.Instance.Get(npcNum));
		}
		foreach (PeEntity item in list)
		{
			if (item == null)
			{
				continue;
			}
			CSMain.RemoveNpc(item);
			item.SetAttribute(AttribType.Hp, 0f, offEvent: false);
			List<int> list2 = new List<int>();
			foreach (int key in MissionManager.Instance.m_PlayerMission.m_MissionInfo.Keys)
			{
				MissionCommonData missionCommonData = MissionManager.GetMissionCommonData(key);
				if (missionCommonData.m_iReplyNpc == item.Id)
				{
					list2.Add(key);
				}
			}
			foreach (int item2 in list2)
			{
				MissionManager.Instance.FailureMission(item2);
			}
			if (!MissionManager.Instance.m_PlayerMission.recordNpcName.ContainsKey(item.Id))
			{
				MissionManager.Instance.m_PlayerMission.recordNpcName.Add(item.Id, item.name.Substring(0, item.name.Length - 1 - Convert.ToString(item.Id).Length));
			}
		}
		return list;
	}

	public static void NPCaddItem(List<PeEntity> npcs, int itemProtoID, int itemNum)
	{
		foreach (PeEntity npc in npcs)
		{
			if (npc == null)
			{
				continue;
			}
			if (PeGameMgr.IsMultiStory)
			{
				PlayerNetwork.mainPlayer.CreateSceneItem("ash_ball", npc.position, "1339,1", npc.Id);
				if (npc.aliveEntity != null && npc.aliveEntity._net != null && (npc.aliveEntity.IsController() || !npc.aliveEntity._net.hasAuth))
				{
					npc.aliveEntity._net.RPCServer(EPacketType.PT_NPC_Destroy);
				}
			}
			else
			{
				NPCaddItem(npc.Id, itemProtoID, itemNum);
			}
		}
	}

	public static void NPCaddItem(int npcid, int itemProtoID, int itemNum)
	{
		PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(npcid);
		if (!(peEntity == null))
		{
			ClickPEentityLootItem clickPEentityLootItem = peEntity.gameObject.AddComponent<ClickPEentityLootItem>();
			clickPEentityLootItem.AddItem(itemProtoID, itemNum);
			clickPEentityLootItem.onClick = (Action<int>)Delegate.Combine(clickPEentityLootItem.onClick, new Action<int>(RemoveRecordItem));
			if (MissionManager.Instance.m_PlayerMission.recordKillNpcItem.Find((int[] ite) => ite[0] == npcid) == null)
			{
				MissionManager.Instance.m_PlayerMission.recordKillNpcItem.Add(new int[3] { peEntity.Id, itemProtoID, itemNum });
			}
		}
	}

	private static void RemoveRecordItem(int npcId)
	{
		int[] array = MissionManager.Instance.m_PlayerMission.recordKillNpcItem.Find((int[] ite) => ite[0] == npcId);
		if (array != null)
		{
			MissionManager.Instance.m_PlayerMission.recordKillNpcItem.Remove(array);
		}
	}

	public static bool isHaveAsh_inScene()
	{
		if (ashBox_inScene == 0)
		{
			return false;
		}
		if (go == null)
		{
			go = DragArticleAgent.Root.gameObject;
		}
		return true;
	}

	public static bool IsBurried(Vector3 pos, out Vector3 upPos)
	{
		upPos = Vector3.zero;
		int num = 0;
		RaycastHit hitInfo = default(RaycastHit);
		Vector3 vector = pos + 4f * Vector3.up;
		Physics.Raycast(vector, pos - vector, out hitInfo, 4f, LayerMask.GetMask("Unwalkable", "VFVoxelTerrain"));
		if (hitInfo.collider == null || hitInfo.collider.gameObject.layer != 12)
		{
			return false;
		}
		upPos = hitInfo.point;
		for (int i = 0; i < 4; i++)
		{
			Vector3 vector2 = i switch
			{
				0 => Vector3.forward, 
				1 => Vector3.back, 
				2 => Vector3.left, 
				_ => Vector3.right, 
			};
			vector = pos + 3f * vector2 + Vector3.up;
			Physics.Raycast(vector, pos - vector, out hitInfo, 4f, LayerMask.GetMask("Unwalkable", "VFVoxelTerrain"));
			if (!(hitInfo.collider != null))
			{
				break;
			}
			if (hitInfo.collider.gameObject.layer == 12)
			{
				num++;
				continue;
			}
			vector = pos + 3f * vector2 + 5f * Vector3.up;
			Physics.Raycast(vector, Vector3.down, out hitInfo, 5f, LayerMask.GetMask("Unwalkable", "VFVoxelTerrain"));
			if (hitInfo.collider != null && hitInfo.collider.gameObject.layer == 12)
			{
				num++;
			}
		}
		if (num != 4)
		{
			return false;
		}
		return true;
	}

	public static void UpdateAshBox()
	{
		if (!(DragArticleAgent.Root != null))
		{
			return;
		}
		Transform root = DragArticleAgent.Root;
		foreach (Transform item in root)
		{
			if (!(item.name == "ash_box") || !IsBurried(item.position, out var _))
			{
				continue;
			}
			if (MissionManager.Instance.m_PlayerMission.HasMission(242))
			{
				ashBox_inScene--;
				MissionManager.Instance.ProcessUseItemMissionByID(1339, StroyManager.Instance.GetPlayerPos());
				MissionManager.Instance.m_PlayerMission.ModifyQuestVariable(242, "ITEM0", 1339, 1);
				if (PeGameMgr.IsMultiStory)
				{
					PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_ModifyQuestVariable, 242, "ITEM0", 1339, 1);
				}
				burriedNum++;
				if (burriedNum == 2)
				{
					if (PeGameMgr.IsMulti)
					{
						MissionManager.Instance.RequestCompleteMission(242, -1, bChcek: false);
					}
					else
					{
						MissionManager.Instance.CompleteMission(242, -1, bCheck: false);
					}
				}
			}
			else if (MissionManager.Instance.m_PlayerMission.HasMission(629))
			{
				ashBox_inScene--;
				MissionManager.Instance.ProcessUseItemMissionByID(1339, StroyManager.Instance.GetPlayerPos());
				MissionManager.Instance.m_PlayerMission.ModifyQuestVariable(629, "ITEM0", 1339, 1);
				if (PeGameMgr.IsMultiStory)
				{
					PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_ModifyQuestVariable, 629, "ITEM0", 1339, 1);
				}
				burriedNum++;
				if (burriedNum == 1)
				{
					if (PeGameMgr.IsMulti)
					{
						MissionManager.Instance.RequestCompleteMission(629, -1, bChcek: false);
					}
					else
					{
						MissionManager.Instance.CompleteMission(629, -1, bCheck: false);
					}
				}
			}
			item.name = "ash_box_burried";
		}
	}
}
