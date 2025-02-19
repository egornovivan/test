using System;
using System.Collections.Generic;
using ItemAsset;
using Pathea;
using UnityEngine;

namespace PeCustom;

public static class PeScenarioUtility
{
	private static List<SpawnPoint> _tempSPList = new List<SpawnPoint>(10);

	public static bool OwnerCheck(int curr_player_id, int sender_id, OBJECT player)
	{
		if (player.type != OBJECT.OBJECTTYPE.Player)
		{
			return false;
		}
		if (player.isAnyPlayer)
		{
			return true;
		}
		if (player.isPlayerId)
		{
			return player.Id == curr_player_id;
		}
		if (player.isCurrentPlayer)
		{
			return curr_player_id == sender_id;
		}
		if (player.isAnyOtherPlayer)
		{
			return curr_player_id != sender_id;
		}
		int force = PeSingleton<CustomGameData.Mgr>.Instance.curGameData.curPlayer.Force;
		if (player.isForceId)
		{
			return force == player.Group;
		}
		if (player.isAnyOtherForce)
		{
			return force != player.Group;
		}
		return false;
	}

	public static bool PlayerCheck(int curr_player_id, int query_player, OBJECT player)
	{
		if (player.type != OBJECT.OBJECTTYPE.Player)
		{
			return false;
		}
		if (player.isAnyPlayer)
		{
			return true;
		}
		if (player.isPlayerId)
		{
			return player.Id == query_player;
		}
		if (player.isCurrentPlayer)
		{
			return curr_player_id == query_player;
		}
		if (player.isAnyOtherPlayer)
		{
			return curr_player_id != query_player;
		}
		int curr_player_force = PeSingleton<CustomGameData.Mgr>.Instance.curGameData.curPlayer.Force;
		int num = 0;
		PlayerDesc playerDesc = PeSingleton<CustomGameData.Mgr>.Instance.curGameData.mPlayerDescs.Find((PlayerDesc iter) => iter.ID == query_player);
		if (playerDesc != null)
		{
			num = playerDesc.Force;
		}
		if (player.isForceId)
		{
			return num == player.Group;
		}
		if (player.isAnyOtherForce)
		{
			return curr_player_force != num;
		}
		ForceDesc forceDesc = PeSingleton<CustomGameData.Mgr>.Instance.curGameData.mForceDescs.Find((ForceDesc iter) => iter.ID == curr_player_force);
		if (forceDesc != null)
		{
			if (player.isAllyForce)
			{
				return forceDesc.Allies.Contains(num);
			}
			if (player.isEnemyForce)
			{
				return !forceDesc.Allies.Contains(num);
			}
		}
		return false;
	}

	public static GameObject GetGameObject(OBJECT obj)
	{
		if (obj.isCurrentPlayer)
		{
			return PeSingleton<PeCreature>.Instance.mainPlayer.gameObject;
		}
		if (!obj.isSpecificEntity)
		{
			return null;
		}
		if (obj.isPlayerId)
		{
			if (PeSingleton<CustomGameData.Mgr>.Instance != null && PeSingleton<CustomGameData.Mgr>.Instance.curGameData != null && PeSingleton<CustomGameData.Mgr>.Instance.curGameData.curPlayer.ID == obj.Id && PeSingleton<PeCreature>.Instance.mainPlayer != null)
			{
				return PeSingleton<PeCreature>.Instance.mainPlayer.gameObject;
			}
		}
		else if (obj.isNpoId && PeSingleton<CustomGameData.Mgr>.Instance != null && PeSingleton<CustomGameData.Mgr>.Instance.curGameData != null && PeSingleton<CustomGameData.Mgr>.Instance.curGameData.WorldIndex == obj.Group)
		{
			if (PeGameMgr.IsSingle)
			{
				SpawnDataSource spawnData = PeCustomScene.Self.spawnData;
				if (spawnData.ContainMonster(obj.Id))
				{
					MonsterSpawnPoint monster = spawnData.GetMonster(obj.Id);
					if (monster.agent != null)
					{
						if (monster.agent.entity != null)
						{
							return monster.agent.entity.gameObject;
						}
						if (monster.agent.ForceCreateEntity())
						{
							return monster.agent.entity.gameObject;
						}
						Debug.Log("Create Entity Faild");
					}
				}
				else if (spawnData.ContainNpc(obj.Id))
				{
					NPCSpawnPoint npc = spawnData.GetNpc(obj.Id);
					if (npc.agent != null)
					{
						if (npc.agent.entity != null)
						{
							return npc.agent.entity.gameObject;
						}
						if (npc.agent.ForceCreateEntity())
						{
							return npc.agent.entity.gameObject;
						}
						Debug.Log("Create Entity Faild");
					}
				}
				else if (spawnData.ContainDoodad(obj.Id))
				{
					DoodadSpawnPoint doodad = spawnData.GetDoodad(obj.Id);
					if (doodad.agent != null)
					{
						if (doodad.agent.entity != null)
						{
							return doodad.agent.entity.gameObject;
						}
						if (doodad.agent.ForceCreateEntity())
						{
							return doodad.agent.entity.gameObject;
						}
						Debug.Log("Create Entity faild");
					}
				}
				else if (spawnData.ContainItem(obj.Id))
				{
					ItemSpwanPoint item = spawnData.GetItem(obj.Id);
					List<ISceneObjAgent> sceneObjs = SceneMan.GetSceneObjs<DragArticleAgent>();
					DragArticleAgent dragArticleAgent = null;
					for (int i = 0; i < sceneObjs.Count; i++)
					{
						if (sceneObjs[i] is DragArticleAgent dragArticleAgent2 && dragArticleAgent2.itemDrag.itemObj.instanceId == item.ItemObjId)
						{
							dragArticleAgent = dragArticleAgent2;
							break;
						}
					}
					if (dragArticleAgent != null && (dragArticleAgent.itemLogic != null || dragArticleAgent.itemScript != null))
					{
						if (dragArticleAgent.itemLogic != null)
						{
							return dragArticleAgent.itemLogic.gameObject;
						}
						return dragArticleAgent.itemScript.gameObject;
					}
				}
			}
			else
			{
				PeEntity byScenarioId = PeSingleton<EntityMgr>.Instance.GetByScenarioId(obj.Id);
				if (null != byScenarioId)
				{
					return byScenarioId.gameObject;
				}
			}
		}
		return null;
	}

	public static PeEntity GetEntity(OBJECT obj)
	{
		if (obj.isCurrentPlayer)
		{
			return PeSingleton<PeCreature>.Instance.mainPlayer;
		}
		GameObject gameObject = GetGameObject(obj);
		if (gameObject != null)
		{
			return gameObject.GetComponent<PeEntity>();
		}
		return null;
	}

	public static Transform GetObjectTransform(OBJECT obj)
	{
		GameObject gameObject = GetGameObject(obj);
		if (gameObject != null)
		{
			PeEntity component = gameObject.GetComponent<PeEntity>();
			if (component != null)
			{
				return component.GetCmpt<PeTrans>().trans;
			}
			return gameObject.transform;
		}
		return null;
	}

	public static bool IsObjectContainEntity(OBJECT obj, PeEntity target)
	{
		return IsObjectContainEntity(obj, target.scenarioId);
	}

	public static bool IsObjectContainEntity(OBJECT obj, int scenarioId)
	{
		if (obj.type == OBJECT.OBJECTTYPE.AnyObject)
		{
			return true;
		}
		if (scenarioId > 0)
		{
			if (obj.isSpecificEntity)
			{
				if (PeSingleton<CustomGameData.Mgr>.Instance != null)
				{
					return obj.Id == scenarioId && PeSingleton<CustomGameData.Mgr>.Instance.curGameData.WorldIndex == obj.Group;
				}
			}
			else if (obj.type == OBJECT.OBJECTTYPE.MonsterProto)
			{
				SpawnPoint monster = PeCustomScene.Self.spawnData.GetMonster(scenarioId);
				if (monster != null)
				{
					if (obj.isSpecificPrototype)
					{
						if (obj.Id == monster.Prototype)
						{
							return true;
						}
					}
					else if (obj.isAnyPrototypeInCategory)
					{
						MonsterProtoDb.Item item = MonsterProtoDb.Get(monster.Prototype);
						if (item != null && Array.FindIndex(item.monsterAreaId, (int ite) => ite == obj.Group) != -1)
						{
							return true;
						}
					}
				}
				MonsterSpawnArea monsterArea = PeCustomScene.Self.spawnData.GetMonsterArea(scenarioId);
				if (monsterArea != null)
				{
					if (obj.isSpecificPrototype)
					{
						if (obj.Id == monsterArea.Prototype)
						{
							return true;
						}
					}
					else if (obj.isAnyPrototypeInCategory)
					{
						MonsterProtoDb.Item item2 = MonsterProtoDb.Get(monsterArea.Prototype);
						if (item2 != null && Array.FindIndex(item2.monsterAreaId, (int ite) => ite == obj.Group) != -1)
						{
							return true;
						}
					}
				}
			}
			else if (obj.type == OBJECT.OBJECTTYPE.ItemProto)
			{
				SpawnPoint item3 = PeCustomScene.Self.spawnData.GetItem(scenarioId);
				if (item3 != null)
				{
					if (obj.isSpecificPrototype)
					{
						if (obj.Id == item3.Prototype)
						{
							return true;
						}
					}
					else if (obj.isAnyPrototypeInCategory)
					{
						ItemProto itemData = ItemProto.GetItemData(item3.Prototype);
						return itemData.editorTypeId == obj.Group;
					}
				}
			}
			else if (obj.isAnyNpo || obj.isAnyNpoInSpecificWorld)
			{
				SpawnPoint spawnPoint = PeCustomScene.Self.spawnData.GetSpawnPoint(scenarioId);
				if (spawnPoint == null)
				{
					return false;
				}
				return true;
			}
			return false;
		}
		int playerId = PeCustomScene.Self.scenario.playerId;
		return PlayerCheck(playerId, -scenarioId, obj);
	}

	public static void SetNpoReqDialogue(PeEntity npc, string RqAction = "", object npoidOrVecter3 = null)
	{
		NpcCmpt npcCmpt = npc.NpcCmpt;
		if (!(npcCmpt != null))
		{
			return;
		}
		if (npoidOrVecter3 != null)
		{
			if (npoidOrVecter3 is int)
			{
				if ((int)npoidOrVecter3 == 0 || PeSingleton<EntityMgr>.Instance.Get((int)npoidOrVecter3) == null)
				{
					npcCmpt.Req_Dialogue(RqAction, null);
					return;
				}
				PeTrans peTrans = PeSingleton<EntityMgr>.Instance.Get((int)npoidOrVecter3).peTrans;
				if (peTrans != null)
				{
					npcCmpt.Req_Dialogue(RqAction, peTrans);
				}
			}
			else if (npoidOrVecter3 is Vector3)
			{
				npcCmpt.Req_Dialogue(RqAction, (Vector3)npoidOrVecter3);
			}
		}
		else
		{
			npcCmpt.Req_Dialogue(RqAction);
		}
	}

	public static void RemoveNpoReq(PeEntity npc, EReqType type)
	{
		NpcCmpt npcCmpt = npc.NpcCmpt;
		if (npcCmpt != null)
		{
			npcCmpt.Req_Remove(type);
		}
	}

	public static bool CreateObject(OBJECT proto, Vector3 pos)
	{
		if (!proto.isSpecificPrototype)
		{
			return false;
		}
		if (proto.type == OBJECT.OBJECTTYPE.MonsterProto)
		{
			int iD = PeCustomScene.Self.spawnData.GenerateId();
			MonsterSpawnPoint monsterSpawnPoint = new MonsterSpawnPoint();
			monsterSpawnPoint.ID = iD;
			monsterSpawnPoint.spawnPos = pos;
			monsterSpawnPoint.Rotation = Quaternion.identity;
			monsterSpawnPoint.Scale = Vector3.one;
			monsterSpawnPoint.entityPos = pos;
			monsterSpawnPoint.Prototype = proto.Id;
			monsterSpawnPoint.IsTarget = true;
			monsterSpawnPoint.Visible = true;
			monsterSpawnPoint.isDead = false;
			monsterSpawnPoint.MaxRespawnCount = 0;
			monsterSpawnPoint.RespawnTime = 0f;
			monsterSpawnPoint.bound = default(Bounds);
			PeCustomScene.Self.CreateAgent(monsterSpawnPoint);
		}
		else if (proto.type == OBJECT.OBJECTTYPE.ItemProto)
		{
			int iD2 = PeCustomScene.Self.spawnData.GenerateId();
			ItemSpwanPoint itemSpwanPoint = new ItemSpwanPoint();
			itemSpwanPoint.ID = iD2;
			itemSpwanPoint.spawnPos = pos;
			itemSpwanPoint.Rotation = Quaternion.identity;
			itemSpwanPoint.Scale = Vector3.one;
			itemSpwanPoint.entityPos = pos;
			itemSpwanPoint.Prototype = proto.Id;
			itemSpwanPoint.IsTarget = false;
			itemSpwanPoint.Visible = true;
			itemSpwanPoint.isDead = false;
			itemSpwanPoint.CanPickup = true;
			PeCustomScene.Self.CreateAgent(itemSpwanPoint);
		}
		return true;
	}

	public static bool CreateObjects(int amout, OBJECT proto, RANGE range)
	{
		if (range.type == RANGE.RANGETYPE.Anywhere || range.type == RANGE.RANGETYPE.Circle)
		{
			return false;
		}
		if (!proto.isSpecificPrototype)
		{
			return false;
		}
		int min_sqr_dis = 5;
		List<Vector3> list = new List<Vector3>(10);
		if (range.type == RANGE.RANGETYPE.Sphere)
		{
			float radius = range.radius;
			for (int i = 0; i < amout; i++)
			{
				int num = 0;
				int num2 = -1;
				Vector3 pos = Vector3.zero;
				do
				{
					pos = range.center + (UnityEngine.Random.insideUnitSphere * radius * 2f - new Vector3(radius, radius, radius));
					num2 = list.FindIndex((Vector3 item0) => (pos - item0).sqrMagnitude < (float)min_sqr_dis);
					num++;
				}
				while (num2 != -1 && num < 30);
				list.Add(pos);
				CreateObject(proto, pos);
				if (num >= 30)
				{
					break;
				}
			}
		}
		else if (range.type == RANGE.RANGETYPE.Box)
		{
			Vector3 vector = new Vector3(range.extend.x, range.extend.y, range.extend.z);
			for (int j = 0; j < amout; j++)
			{
				int num3 = 0;
				int num4 = -1;
				Vector3 pos2 = Vector3.zero;
				do
				{
					pos2 = range.center + (new Vector3(UnityEngine.Random.value * vector.x, UnityEngine.Random.value * vector.y, UnityEngine.Random.value * vector.z) * 2f - vector);
					num4 = list.FindIndex((Vector3 item0) => (pos2 - item0).sqrMagnitude < (float)min_sqr_dis);
					num3++;
				}
				while (num4 != -1 && num3 < 30);
				list.Add(pos2);
				CreateObject(proto, pos2);
				if (num3 >= 30)
				{
					break;
				}
			}
		}
		return true;
	}

	public static bool RemoveObject(OBJECT obj)
	{
		if (!obj.isSpecificEntity)
		{
			return false;
		}
		if (obj.isNpoId)
		{
			SpawnDataSource spawnData = PeCustomScene.Self.spawnData;
			if (spawnData.ContainMonster(obj.Id))
			{
				SpawnPoint monster = spawnData.GetMonster(obj.Id);
				PeCustomScene.Self.RemoveSpawnPoint(monster);
			}
			else if (spawnData.ContainNpc(obj.Id))
			{
				SpawnPoint npc = spawnData.GetNpc(obj.Id);
				PeCustomScene.Self.RemoveSpawnPoint(npc);
			}
			else if (spawnData.ContainDoodad(obj.Id))
			{
				SpawnPoint doodad = spawnData.GetDoodad(obj.Id);
				PeCustomScene.Self.RemoveSpawnPoint(doodad);
			}
			else if (spawnData.ContainItem(obj.Id))
			{
				ItemSpwanPoint item = spawnData.GetItem(obj.Id);
				List<ISceneObjAgent> sceneObjs = SceneMan.GetSceneObjs<DragItemAgent>();
				for (int i = 0; i < sceneObjs.Count; i++)
				{
					DragItemAgent dragItemAgent = sceneObjs[i] as DragItemAgent;
					if (dragItemAgent.itemDrag.itemObj.protoId == item.Prototype && dragItemAgent.itemDrag.itemObj.instanceId == item.ItemObjId)
					{
						DragItemAgent.Destory(dragItemAgent);
					}
				}
			}
		}
		return false;
	}

	public static bool RemoveObjects(OBJECT proto, RANGE range)
	{
		if (!proto.isPrototype)
		{
			return false;
		}
		if (proto.isAnyPrototype)
		{
			SpawnDataSource spawnData = PeCustomScene.Self.spawnData;
			if (proto.type == OBJECT.OBJECTTYPE.MonsterProto)
			{
				_tempSPList.Clear();
				foreach (KeyValuePair<int, MonsterSpawnPoint> monster in spawnData.monsters)
				{
					if (range.Contains(monster.Value.entityPos))
					{
						_tempSPList.Add(monster.Value);
					}
				}
				for (int i = 0; i < _tempSPList.Count; i++)
				{
					PeCustomScene.Self.RemoveSpawnPoint(_tempSPList[i]);
				}
				_tempSPList.Clear();
				foreach (KeyValuePair<int, MonsterSpawnArea> area in spawnData.areas)
				{
					for (int j = 0; j < area.Value.Spawns.Count; j++)
					{
						for (int k = 0; k < area.Value.Spawns[j].spawnPoints.Count; k++)
						{
							MonsterSpawnPoint monsterSpawnPoint = area.Value.Spawns[j].spawnPoints[k];
							if (range.Contains(monsterSpawnPoint.entityPos) && monsterSpawnPoint.EntityID != -1)
							{
								PeSingleton<CreatureMgr>.Instance.Destory(monsterSpawnPoint.EntityID);
							}
						}
					}
				}
			}
			else if (proto.type == OBJECT.OBJECTTYPE.ItemProto)
			{
				List<ISceneObjAgent> sceneObjs = SceneMan.GetSceneObjs<DragItemAgent>();
				for (int l = 0; l < sceneObjs.Count; l++)
				{
					DragItemAgent dragItemAgent = sceneObjs[l] as DragItemAgent;
					if (range.Contains(dragItemAgent.position))
					{
						DragItemAgent.Destory(dragItemAgent);
					}
				}
			}
		}
		else
		{
			SpawnDataSource spawnData2 = PeCustomScene.Self.spawnData;
			if (proto.type == OBJECT.OBJECTTYPE.MonsterProto)
			{
				_tempSPList.Clear();
				foreach (KeyValuePair<int, MonsterSpawnPoint> monster2 in spawnData2.monsters)
				{
					if (monster2.Value.Prototype == proto.Id && range.Contains(monster2.Value.entityPos))
					{
						_tempSPList.Add(monster2.Value);
					}
				}
				for (int m = 0; m < _tempSPList.Count; m++)
				{
					PeCustomScene.Self.RemoveSpawnPoint(_tempSPList[m]);
				}
				_tempSPList.Clear();
				foreach (KeyValuePair<int, MonsterSpawnArea> area2 in spawnData2.areas)
				{
					for (int n = 0; n < area2.Value.Spawns.Count; n++)
					{
						for (int num = 0; num < area2.Value.Spawns[n].spawnPoints.Count; num++)
						{
							MonsterSpawnPoint monsterSpawnPoint2 = area2.Value.Spawns[n].spawnPoints[num];
							if (area2.Value.Prototype == proto.Id && range.Contains(monsterSpawnPoint2.entityPos) && monsterSpawnPoint2.EntityID != -1)
							{
								PeSingleton<CreatureMgr>.Instance.Destory(monsterSpawnPoint2.EntityID);
							}
						}
					}
				}
			}
			else if (proto.type == OBJECT.OBJECTTYPE.ItemProto)
			{
				List<ISceneObjAgent> sceneObjs2 = SceneMan.GetSceneObjs<DragArticleAgent>();
				for (int num2 = 0; num2 < sceneObjs2.Count; num2++)
				{
					DragArticleAgent dragArticleAgent = sceneObjs2[num2] as DragArticleAgent;
					if (dragArticleAgent.itemDrag.itemObj.protoId == proto.Id && range.Contains(dragArticleAgent.position))
					{
						DragItemAgent.Destory(dragArticleAgent);
					}
				}
			}
		}
		return true;
	}

	public static bool EnableSpawnPoint(OBJECT obj, bool enable)
	{
		if (obj.isPlayerId)
		{
			return false;
		}
		if (obj.isAnyNpo || (PeSingleton<CustomGameData.Mgr>.Instance != null && PeSingleton<CustomGameData.Mgr>.Instance.curGameData != null && obj.isAnyNpoInSpecificWorld && obj.Group == PeSingleton<CustomGameData.Mgr>.Instance.curGameData.WorldIndex))
		{
			foreach (KeyValuePair<int, MonsterSpawnPoint> monster in PeCustomScene.Self.spawnData.monsters)
			{
				PeCustomScene.Self.EnableSpawnPoint(monster.Value, enable);
			}
			foreach (KeyValuePair<int, MonsterSpawnArea> area in PeCustomScene.Self.spawnData.areas)
			{
				PeCustomScene.Self.EnableSpawnPoint(area.Value, enable);
			}
			foreach (KeyValuePair<int, NPCSpawnPoint> npc in PeCustomScene.Self.spawnData.npcs)
			{
				PeCustomScene.Self.EnableSpawnPoint(npc.Value, enable);
			}
		}
		else if (obj.isSpecificEntity && obj.Group == PeSingleton<CustomGameData.Mgr>.Instance.curGameData.WorldIndex)
		{
			SpawnDataSource spawnData = PeCustomScene.Self.spawnData;
			SpawnPoint spawnPoint = spawnData.GetSpawnPoint(obj.Id);
			if (spawnPoint != null)
			{
				PeCustomScene.Self.EnableSpawnPoint(spawnPoint, enable);
			}
		}
		return true;
	}
}
