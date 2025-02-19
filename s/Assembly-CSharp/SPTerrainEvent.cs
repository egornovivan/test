using System.Collections.Generic;
using System.IO;
using System.Linq;
using CustomData;
using ItemAsset;
using Mono.Data.SqliteClient;
using Pathea;
using PETools;
using SkillSystem;
using uLink;
using UnityEngine;

public class SPTerrainEvent : UnityEngine.MonoBehaviour
{
	public const int HumanMonsterMask = 10000;

	public static float ModelScaleMin = 0.5f;

	public static float ModelScaleMax = 1.5f;

	public static int MaxNPCNum = 3;

	public static int MaxAiNum = 5;

	public static int MaxStRdNpcNum = 1;

	public static int MaxStNpcNum = 1;

	public static int MaxMonsterGroup = 3;

	protected static Dictionary<int, List<EntityType>> _spawnPos = new Dictionary<int, List<EntityType>>();

	protected static Dictionary<int, List<EntityType>> _storyNpc = new Dictionary<int, List<EntityType>>();

	protected static Dictionary<int, int> _groupMonsters = new Dictionary<int, int>();

	protected static Dictionary<int, CustomObjData> CustomIds = new Dictionary<int, CustomObjData>();

	protected static Dictionary<int, List<CustomObjData>> CustomNpcs = new Dictionary<int, List<CustomObjData>>();

	protected static List<int> _adMainNpcs = new List<int>();

	public static int CreateNpc(int controllerId, int worldId, Vector3 pos, int templateId, int type, float scale, int nObjID, bool isStand = false, float rotY = 0f, bool forcedServant = false, bool dontCheck = false)
	{
		int index = AreaHelper.Vector2Int(pos, 256);
		if (!dontCheck)
		{
			switch (type)
			{
			case 1:
				if (!CheckGeneration(index, EntityType.EntityType_Npc))
				{
					return -1;
				}
				break;
			case 4:
				if (!CheckGeneration(nObjID, EntityType.EntityType_StRdNpc))
				{
					return -1;
				}
				break;
			case 5:
				if (!CheckGeneration(templateId, EntityType.EntityType_StNpc))
				{
					return -1;
				}
				break;
			case 7:
				if (!CheckGeneration(templateId, EntityType.EntityType_AdMainNpc))
				{
					return -1;
				}
				break;
			}
		}
		nObjID = CreateNpcWithoutLimit(controllerId, worldId, pos, templateId, type, scale, nObjID, isStand, rotY, forcedServant);
		return nObjID;
	}

	public static int CreateNpcWithoutLimit(int controllerId, int worldId, Vector3 pos, int templateId, int type, float scale, int nObjID, bool isStand = false, float rotY = 0f, bool forcedServant = false, string npcName = "npc")
	{
		if (nObjID == -1 && templateId < 100 && !ServerConfig.IsCustom)
		{
			nObjID = templateId;
		}
		else if (nObjID == -1)
		{
			nObjID = IdGenerator.NewNpcId;
		}
		if (templateId < 100)
		{
			_adMainNpcs.Add(templateId);
		}
		NetInterface.Instantiate(PrefabManager.Self.AiAdNpcNetworkSeed, pos, Quaternion.Euler(0f, rotY, 0f), worldId, nObjID, scale, templateId, type, controllerId, isStand, rotY, forcedServant, npcName);
		return nObjID;
	}

	public static bool CheckGeneration(int index, EntityType type)
	{
		bool flag = false;
		switch (type)
		{
		case EntityType.EntityType_GroupMonster:
		{
			if (!_groupMonsters.ContainsKey(index))
			{
				_groupMonsters.Add(index, 0);
			}
			AIGroupNetWork aIGroupNetWork = ObjNetInterface.Get<AIGroupNetWork>(index);
			if (null != aIGroupNetWork)
			{
				MonsterGroupProtoDb.Item item = MonsterGroupProtoDb.Get(aIGroupNetWork.ExternId & -1073741825);
				if (item != null)
				{
					flag = ((_groupMonsters[index] < item.limitNum) ? true : false);
				}
			}
			break;
		}
		case EntityType.EntityType_Monster:
		{
			if (!_spawnPos.ContainsKey(index))
			{
				_spawnPos[index] = new List<EntityType>();
			}
			int num5 = _spawnPos[index].Count((EntityType iter) => iter == type);
			flag = ((num5 < MaxAiNum) ? true : false);
			break;
		}
		case EntityType.EntityType_MonsterGroup:
		{
			if (!_spawnPos.ContainsKey(index))
			{
				_spawnPos[index] = new List<EntityType>();
			}
			int num = _spawnPos[index].Count((EntityType iter) => iter == type);
			flag = ((num < MaxMonsterGroup) ? true : false);
			break;
		}
		case EntityType.EntityType_Npc:
		{
			if (!_spawnPos.ContainsKey(index))
			{
				_spawnPos[index] = new List<EntityType>();
			}
			int num3 = _spawnPos[index].Count((EntityType iter) => iter == type);
			flag = ((num3 < MaxNPCNum) ? true : false);
			break;
		}
		case EntityType.EntityType_StRdNpc:
		{
			if (!_storyNpc.ContainsKey(index))
			{
				_storyNpc[index] = new List<EntityType>();
			}
			int num4 = _storyNpc[index].Count((EntityType iter) => iter == type);
			flag = ((num4 < MaxStRdNpcNum) ? true : false);
			break;
		}
		case EntityType.EntityType_StNpc:
		{
			if (!_storyNpc.ContainsKey(index))
			{
				_storyNpc[index] = new List<EntityType>();
			}
			int num2 = _storyNpc[index].Count((EntityType iter) => iter == type);
			flag = ((num2 < MaxStNpcNum) ? true : false);
			break;
		}
		case EntityType.EntityType_AdMainNpc:
			if (_adMainNpcs.Contains(index))
			{
				return false;
			}
			return true;
		}
		if (flag)
		{
			switch (type)
			{
			case EntityType.EntityType_GroupMonster:
			{
				Dictionary<int, int> groupMonsters;
				Dictionary<int, int> dictionary = (groupMonsters = _groupMonsters);
				int key;
				int key2 = (key = index);
				key = groupMonsters[key];
				dictionary[key2] = key + 1;
				break;
			}
			case EntityType.EntityType_StRdNpc:
			case EntityType.EntityType_StNpc:
				_storyNpc[index].Add(type);
				break;
			case EntityType.EntityType_Npc:
			case EntityType.EntityType_Monster:
			case EntityType.EntityType_MonsterGroup:
				_spawnPos[index].Add(type);
				break;
			}
		}
		return flag;
	}

	public static void OnNpcDestroyed(AiAdNpcNetwork npc)
	{
		int key = AreaHelper.Vector2Int(npc.SpawnPos, 256);
		if (_spawnPos.ContainsKey(key))
		{
			_spawnPos[key].Remove(EntityType.EntityType_Npc);
		}
	}

	public static void OnMonsterDeath(AiMonsterNetwork monster)
	{
		if (monster.GroupId == -1 || monster.TdId == -1 || monster.FixId == -1)
		{
			int key = AreaHelper.Vector2Int(monster.SpawnPos, 256);
			if (_spawnPos.ContainsKey(key))
			{
				_spawnPos[key].Remove(EntityType.EntityType_Monster);
			}
		}
		if (monster.GroupId != -1 && _groupMonsters.ContainsKey(monster.GroupId) && _groupMonsters[monster.GroupId] >= 1)
		{
			Dictionary<int, int> groupMonsters;
			Dictionary<int, int> dictionary = (groupMonsters = _groupMonsters);
			int groupId;
			int key2 = (groupId = monster.GroupId);
			groupId = groupMonsters[groupId];
			dictionary[key2] = groupId - 1;
		}
	}

	public static void OnGroupMonsterDestroy(AIGroupNetWork aiGroup)
	{
		int key = AreaHelper.Vector2Int(aiGroup.SpawnPos, 256);
		if (_spawnPos.ContainsKey(key))
		{
			_spawnPos[key].Remove(EntityType.EntityType_MonsterGroup);
		}
	}

	public static void CreateKillMonster(int controllerId, int worldId, Vector3 pos, int aiId, int groupId = -1, int tdId = -1, int dungeonId = -1, int colorType = -1, int playerId = -1, int fixId = -1, int buffId = 0, bool canride = true)
	{
		int num = AreaHelper.Vector2Int(pos, 256);
		if ((aiId & 0x40000000) == 1073741824)
		{
			if (!(pos.y >= 0f) || CheckGeneration(num, EntityType.EntityType_MonsterGroup))
			{
				int newMonsterId = IdGenerator.NewMonsterId;
				NetInterface.Instantiate(PrefabManager.Self.AiGroupNetwork, pos, Quaternion.identity, worldId, newMonsterId, aiId, controllerId, tdId, dungeonId, colorType, playerId, buffId, canride);
			}
			return;
		}
		int newMonsterId2 = IdGenerator.NewMonsterId;
		if (fixId == -1 && groupId == -1 && tdId == -1 && pos.y >= 0f && !CheckGeneration(num, EntityType.EntityType_Monster))
		{
			return;
		}
		if (fixId != -1)
		{
			if (!AISpawnPoint.AddSpawnPoint(fixId, newMonsterId2))
			{
				return;
			}
		}
		else if (groupId != -1 && pos.y >= 0f && !CheckGeneration(groupId, EntityType.EntityType_GroupMonster))
		{
			return;
		}
		if (CreateMonsterWithoutLimit(controllerId, worldId, pos, aiId, groupId, tdId, dungeonId, colorType, playerId, fixId, newMonsterId2, buffId, canride) == -1)
		{
			if (fixId == -1 && groupId == -1 && tdId == -1)
			{
				_spawnPos[num].Remove(EntityType.EntityType_Monster);
			}
			if (fixId != -1)
			{
				AISpawnPoint.DeleteSpawnPoint(newMonsterId2);
			}
			else if (groupId != -1)
			{
				Dictionary<int, int> groupMonsters;
				Dictionary<int, int> dictionary = (groupMonsters = _groupMonsters);
				int key;
				int key2 = (key = num);
				key = groupMonsters[key];
				dictionary[key2] = key - 1;
			}
		}
	}

	public static int CreateMonsterWithoutLimit(int controllerId, int worldId, Vector3 pos, int aiId, int groupId = -1, int tdId = -1, int dungeonId = -1, int colorType = -1, int playerId = -1, int fixId = -1, int id = -1, int buffId = 0, bool canride = true)
	{
		MonsterProtoDb.Item item = MonsterProtoDb.Get(aiId);
		if (item == null)
		{
			return -1;
		}
		if (id == -1)
		{
			id = IdGenerator.NewMonsterId;
		}
		float num = 1f;
		if (fixId == -1)
		{
			num = Random.Range(item.minScale, item.maxScale);
		}
		NetInterface.Instantiate(PrefabManager.Self.AiMonster, pos, Quaternion.identity, worldId, id, aiId, num, controllerId, groupId, tdId, dungeonId, colorType, playerId, fixId, buffId, canride);
		return id;
	}

	public static void SaveCustomNpc()
	{
		CustomNpcData customNpcData = new CustomNpcData();
		customNpcData.ExportData(CustomNpcs);
		AsyncSqlite.AddRecord(customNpcData);
	}

	public static void LoadCustomNpc()
	{
		PEDbOp pEDbOp = AsyncSqlite.NewReadOp();
		if (pEDbOp != null)
		{
			pEDbOp.SetCmdText("SELECT * FROM customnpcs;");
			pEDbOp.BindReaderHandler(LoadCustomNpcDone);
			pEDbOp.Exec();
			pEDbOp = null;
		}
	}

	private static void LoadCustomNpcDone(SqliteDataReader reader)
	{
		while (reader.Read())
		{
			int @int = reader.GetInt32(reader.GetOrdinal("ver"));
			byte[] buffer = (byte[])reader.GetValue(reader.GetOrdinal("data"));
			BufferHelper.Import(buffer, delegate(BinaryReader r)
			{
				int num = r.ReadInt32();
				for (int i = 0; i < num; i++)
				{
					int num2 = r.ReadInt32();
					int num3 = r.ReadInt32();
					for (int j = 0; j < num3; j++)
					{
						int instanceId = r.ReadInt32();
						int customId = r.ReadInt32();
						int protoId = r.ReadInt32();
						int defaultPlayerId = r.ReadInt32();
						AddCustomId(instanceId, customId, protoId, defaultPlayerId);
					}
				}
			});
		}
	}

	public static void AddCustomNpc(int worldId, int id, int customId, int protoId, int playerId)
	{
		if (!CustomNpcs.ContainsKey(worldId))
		{
			CustomNpcs.Add(worldId, new List<CustomObjData>());
		}
		if (!CustomNpcs[worldId].Exists((CustomObjData iter) => iter.Id == id))
		{
			CustomObjData item = default(CustomObjData);
			item.Id = id;
			item.CustomId = customId;
			item.ProtoId = protoId;
			item.DefaultPlayerId = playerId;
			CustomNpcs[worldId].Add(item);
		}
	}

	public static void AddCustomId(int instanceId, int customId, int protoId, int defaultPlayerId)
	{
		if (!CustomIds.ContainsKey(instanceId))
		{
			CustomObjData value = default(CustomObjData);
			value.Id = instanceId;
			value.CustomId = customId;
			value.ProtoId = protoId;
			value.DefaultPlayerId = defaultPlayerId;
			CustomIds.Add(instanceId, value);
		}
	}

	public static int GetInstanceId(OBJECT obj)
	{
		int num = obj.Id;
		if (obj.type == OBJECTTYPE.Player && obj.isPlayerId)
		{
			num = -obj.Id;
		}
		foreach (KeyValuePair<int, CustomObjData> customId in CustomIds)
		{
			if (customId.Value.CustomId == num)
			{
				return customId.Key;
			}
		}
		return -1;
	}

	public static int GetCustomId(int instanceId)
	{
		if (CustomIds.ContainsKey(instanceId))
		{
			return CustomIds[instanceId].CustomId;
		}
		return -1;
	}

	public static int GetCustomProtoId(int instanceId)
	{
		if (CustomIds.ContainsKey(instanceId))
		{
			return CustomIds[instanceId].ProtoId;
		}
		return -1;
	}

	public static int GetCustomDefaultPlayerId(int instanceId)
	{
		if (CustomIds.ContainsKey(instanceId))
		{
			return CustomIds[instanceId].DefaultPlayerId;
		}
		return -1;
	}

	public static ISceneObject GetCustomObj(int worldId, OBJECT obj, SkNetworkInterface opNet)
	{
		if (obj.type == OBJECTTYPE.Player)
		{
			if (obj.isCurrentPlayer)
			{
				return opNet;
			}
			if (obj.isPlayerId)
			{
				int instanceId = GetInstanceId(obj);
				return ObjNetInterface.Get<SkNetworkInterface>(instanceId);
			}
		}
		else if (obj.type == OBJECTTYPE.WorldObject)
		{
			int instanceId2 = GetInstanceId(obj);
			if (ObjNetInterface.IsExisted(instanceId2))
			{
				return ObjNetInterface.Get<SkNetworkInterface>(instanceId2);
			}
			return GameWorld.GetSceneObj(instanceId2, worldId);
		}
		return null;
	}

	public static void InitCustomData(int entityId, SkEntity skEntity)
	{
		if (ServerConfig.IsCustom)
		{
			int customDefaultPlayerId = GetCustomDefaultPlayerId(entityId);
			if (customDefaultPlayerId != -1 && null != skEntity)
			{
				skEntity.SetAllAttribute(AttribType.DefaultPlayerID, customDefaultPlayerId);
			}
		}
	}

	public static void CreateObjects(OBJECT proto, RANGE range, int amout, int worldId, Player opPlayer)
	{
		if (range.type == RANGE.RANGETYPE.Anywhere || range.type == RANGE.RANGETYPE.Circle || !proto.isSpecificPrototype)
		{
			return;
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
					pos = range.center + (Random.insideUnitSphere * radius * 2f - new Vector3(radius, radius, radius));
					num2 = list.FindIndex((Vector3 item0) => (pos - item0).sqrMagnitude < (float)min_sqr_dis);
					num++;
				}
				while (num2 != -1 && num < 30);
				list.Add(pos);
				CreateObject(proto, pos, worldId, opPlayer);
				if (num >= 30)
				{
					break;
				}
			}
		}
		else
		{
			if (range.type != RANGE.RANGETYPE.Box)
			{
				return;
			}
			Vector3 vector = new Vector3(range.extend.x, range.extend.y, range.extend.z);
			for (int j = 0; j < amout; j++)
			{
				int num3 = 0;
				int num4 = -1;
				Vector3 pos2 = Vector3.zero;
				do
				{
					pos2 = range.center + (new Vector3(Random.value * vector.x, Random.value * vector.y, Random.value * vector.z) * 2f - vector);
					num4 = list.FindIndex((Vector3 item0) => (pos2 - item0).sqrMagnitude < (float)min_sqr_dis);
					num3++;
				}
				while (num4 != -1 && num3 < 30);
				list.Add(pos2);
				CreateObject(proto, pos2, worldId, opPlayer);
				if (num3 >= 30)
				{
					break;
				}
			}
		}
	}

	private static void CreateObject(OBJECT proto, Vector3 pos, int worldId, Player opPlayer)
	{
		if (!proto.isSpecificPrototype)
		{
			return;
		}
		if (proto.type == OBJECTTYPE.MonsterProto)
		{
			CreateMonsterWithoutLimit(-1, worldId, pos, proto.Id);
		}
		else if (proto.type == OBJECTTYPE.ItemProto)
		{
			ItemObject itemObject = ItemManager.CreateItem(proto.Id, 1);
			if (itemObject != null)
			{
				SceneItem sceneItem = SceneObjMgr.Create<SceneItem>();
				sceneItem.Init(itemObject.instanceId, proto.Id, pos, Vector3.one, Quaternion.identity, worldId);
				sceneItem.SetItem(itemObject);
				sceneItem.SetType(ESceneObjType.ITEM);
				GameWorld.AddSceneObj(sceneItem, worldId);
				SceneObjMgr.Save(sceneItem);
				opPlayer.SyncItem(itemObject);
				opPlayer.SyncSceneObject(sceneItem);
			}
		}
	}

	public static bool RemoveObjects(OBJECT proto, RANGE range, int worldId, Player opPlayer)
	{
		if (!proto.isPrototype)
		{
			return false;
		}
		if (proto.isAnyPrototype)
		{
			if (proto.type == OBJECTTYPE.MonsterProto)
			{
				List<AiMonsterNetwork> list = ObjNetInterface.Get<AiMonsterNetwork>();
				int num = 0;
				while (num != list.Count)
				{
					if (list[num].WorldId == worldId && range.Contains(list[num].SpawnPos))
					{
						NetInterface.NetDestroy(list[num]);
						list.RemoveAt(num);
					}
					else
					{
						num++;
					}
				}
			}
			else if (proto.type == OBJECTTYPE.ItemProto)
			{
				GameWorld gameWorld = GameWorld.GetGameWorld(worldId);
				SceneObject[] sceneObjs = gameWorld.GetSceneObjs();
				int i = 0;
				List<int> list2 = new List<int>();
				for (; i != sceneObjs.Length; i++)
				{
					if (range.Contains(sceneObjs[i].Pos))
					{
						gameWorld.DelSceneObj(sceneObjs[i].Id);
						list2.Add(sceneObjs[i].Id);
					}
				}
				opPlayer.SyncDelSceneObjects(list2);
			}
		}
		else if (proto.type == OBJECTTYPE.MonsterProto)
		{
			List<AiMonsterNetwork> list3 = ObjNetInterface.Get<AiMonsterNetwork>();
			int num2 = 0;
			while (num2 != list3.Count)
			{
				if (list3[num2].WorldId == worldId && proto.Id == list3[num2].ExternId && range.Contains(list3[num2].SpawnPos))
				{
					NetInterface.NetDestroy(list3[num2]);
					list3.RemoveAt(num2);
				}
				else
				{
					num2++;
				}
			}
		}
		else if (proto.type == OBJECTTYPE.ItemProto)
		{
			GameWorld gameWorld2 = GameWorld.GetGameWorld(worldId);
			SceneObject[] sceneObjs2 = gameWorld2.GetSceneObjs();
			int j = 0;
			List<int> list4 = new List<int>();
			for (; j != sceneObjs2.Length; j++)
			{
				if (proto.Id == sceneObjs2[j].ProtoId && range.Contains(sceneObjs2[j].Pos))
				{
					gameWorld2.DelSceneObj(sceneObjs2[j].Id);
					list4.Add(sceneObjs2[j].Id);
				}
			}
			opPlayer.SyncDelSceneObjects(list4);
		}
		return true;
	}

	public static bool RemoveSpecObject(OBJECT obj, int worldId, Player opPlayer)
	{
		if (!obj.isSpecificEntity)
		{
			return false;
		}
		if (obj.isNpoId)
		{
			ISceneObject customObj = GetCustomObj(worldId, obj, opPlayer);
			if (!object.Equals(null, customObj))
			{
				if (customObj is SkNetworkInterface)
				{
					NetInterface.NetDestroy((SkNetworkInterface)customObj);
				}
				else if (customObj is SceneItem)
				{
					GameWorld.DelSceneObj(customObj.Id, worldId);
					opPlayer.SyncDelSceneObjects(customObj.Id);
				}
			}
		}
		return true;
	}

	public static void RPC_C2S_SpawnAIAtPoint(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (ServerConfig.MonsterYes)
		{
			int aiId = stream.Read<int>(new object[0]);
			Vector3 pos = stream.Read<Vector3>(new object[0]);
			int groupId = stream.Read<int>(new object[0]);
			int tdId = stream.Read<int>(new object[0]);
			int dungeonId = stream.Read<int>(new object[0]);
			int colorType = stream.Read<int>(new object[0]);
			int playerId = stream.Read<int>(new object[0]);
			int buffId = stream.Read<int>(new object[0]);
			Player player = Player.GetPlayer(info.sender);
			int controllerId = ((!(null == player)) ? player.Id : (-1));
			if (player != null)
			{
				CreateKillMonster(controllerId, player.WorldId, pos, aiId, groupId, tdId, dungeonId, colorType, playerId, -1, buffId);
			}
		}
	}

	public static void RPC_C2S_SetFixActive(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (ServerConfig.MonsterYes)
		{
			int fixId = stream.Read<int>(new object[0]);
			if (stream.Read<bool>(new object[0]))
			{
				AISpawnPoint.CreateKillMonsterFix(fixId);
			}
		}
	}

	public static void RPC_C2S_CreateNativeStatic(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!ServerConfig.MonsterYes)
		{
			return;
		}
		Vector3 pos = stream.Read<Vector3>(new object[0]);
		int num = stream.Read<int>(new object[0]);
		int campId = stream.Read<int>(new object[0]);
		if (RandomTownManager.Instance.IsCaptured(campId))
		{
			return;
		}
		List<NativeStaticNetwork> list = ObjNetInterface.Get<NativeStaticNetwork>();
		if (!list.Exists((NativeStaticNetwork iter) => Vector3.Distance(iter.SpawnPos, pos) < 0.1f))
		{
			if (LogFilter.logDebug)
			{
				Debug.LogWarning("create NativeStatic ok ! monsterID:" + num);
			}
			MonsterProtoDb.Item item = MonsterProtoDb.Get(num);
			if (item != null)
			{
				float num2 = Random.Range(item.minScale, item.maxScale);
				NetInterface.Instantiate(PrefabManager.Self.NativeStaticNetwork, pos, Quaternion.identity, info.networkView.group, num, num2, 0, 0, 0, IntVector4.Zero, uLink.NetworkViewID.unassigned, uLink.NetworkViewID.unassigned);
			}
		}
	}

	public static void RPC_C2S_NativeTowerCreate(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		BuildingID bId = stream.Read<BuildingID>(new object[0]);
		if (BuildingInfoManager.Instance.GeneratetdBuilding(bId))
		{
			return;
		}
		int num = stream.Read<int>(new object[0]);
		Vector3 pos = stream.Read<Vector3>(new object[0]);
		Vector3 scl = stream.Read<Vector3>(new object[0]);
		Quaternion rot = stream.Read<Quaternion>(new object[0]);
		int townId = stream.Read<int>(new object[0]);
		int campId = stream.Read<int>(new object[0]);
		int damageId = stream.Read<int>(new object[0]);
		int dPlayerId = stream.Read<int>(new object[0]);
		if (!ServerConfig.MonsterYes)
		{
			return;
		}
		Player player = Player.GetPlayer(info.sender);
		if (!(null == player))
		{
			int worldId = player.WorldId;
			TownDoodad doodad = new TownDoodad(num, worldId, pos);
			if (BuildingInfoManager.AddTownDoodad(doodad))
			{
				int id = player.Id;
				int newDoodadId = IdGenerator.NewDoodadId;
				DoodadMgr.CreateUnlimitedDoodad(id, -1, worldId, -1, newDoodadId, num, 7, DoodadRandomBuilding.PackParam(townId, campId, damageId, dPlayerId), pos, scl, rot);
				BuildingInfoManager.Instance.AddBuilding(bId);
				BuildingInfoManager.Instance.AddAliveBuilding(townId, newDoodadId);
			}
		}
	}

	public static void RPC_C2S_TownDoodad(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		BuildingID bId = stream.Read<BuildingID>(new object[0]);
		if (BuildingInfoManager.Instance.GeneratetdBuilding(bId))
		{
			return;
		}
		int num = stream.Read<int>(new object[0]);
		Vector3 pos = stream.Read<Vector3>(new object[0]);
		Vector3 scl = stream.Read<Vector3>(new object[0]);
		Quaternion rot = stream.Read<Quaternion>(new object[0]);
		int townId = stream.Read<int>(new object[0]);
		int campId = stream.Read<int>(new object[0]);
		int damageId = stream.Read<int>(new object[0]);
		int dPlayerId = stream.Read<int>(new object[0]);
		Player player = Player.GetPlayer(info.sender);
		if (null == player)
		{
			return;
		}
		int worldId = player.WorldId;
		TownDoodad doodad = new TownDoodad(num, worldId, pos);
		if (!BuildingInfoManager.AddTownDoodad(doodad))
		{
			return;
		}
		int id = player.Id;
		int teamId = player.TeamId;
		int newDoodadId = IdGenerator.NewDoodadId;
		StoryDoodadDesc byProtoId = StoryDoodadMap.GetByProtoId(num);
		if (byProtoId != null)
		{
			if (byProtoId._doodadType == 5)
			{
				DoodadMgr.CreateUnlimitedDoodad(id, -1, worldId, -1, newDoodadId, num, 8, DoodadRandomBuilding.PackParam(townId, campId, damageId, dPlayerId), pos, scl, rot);
			}
			else if (byProtoId._doodadType == 6)
			{
				DoodadMgr.CreateUnlimitedDoodad(id, -1, worldId, -1, newDoodadId, num, 9, DoodadRandomBuilding.PackParam(townId, campId, damageId, dPlayerId), pos, scl, rot);
			}
			else
			{
				DoodadMgr.CreateUnlimitedDoodad(id, -1, worldId, -1, newDoodadId, num, 7, DoodadRandomBuilding.PackParam(townId, campId, damageId, dPlayerId), pos, scl, rot);
			}
		}
		else
		{
			DoodadMgr.CreateUnlimitedDoodad(id, -1, worldId, -1, newDoodadId, num, 7, DoodadRandomBuilding.PackParam(townId, campId, damageId, dPlayerId), pos, scl, rot);
		}
		BuildingInfoManager.Instance.AddBuilding(bId);
		BuildingInfoManager.Instance.AddAliveBuilding(townId, newDoodadId);
	}

	public static void RPC_C2S_SpawnGift(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		Vector3 pos = stream.Read<Vector3>(new object[0]);
		int num = stream.Read<int>(new object[0]);
		MonsterProtoDb.Item item = MonsterProtoDb.Get(num);
		if (item != null)
		{
			float num2 = Random.Range(item.minScale, item.maxScale);
			NetInterface.Instantiate(PrefabManager.Self.NativeTowerGiftNetwork, pos, Quaternion.identity, info.networkView.group, num, num2, 0, 0, 0, IntVector4.Zero, uLink.NetworkViewID.unassigned, uLink.NetworkViewID.unassigned);
			if (LogFilter.logDebug)
			{
				Debug.LogWarning("SpawnGift!!!");
			}
		}
	}

	public static void RPC_C2S_SpawnAIGroupAtPoint(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (ServerConfig.MonsterYes)
		{
			int num = stream.Read<int>(new object[0]);
			Vector3 pos = stream.Read<Vector3>(new object[0]);
			Player player = Player.GetPlayer(info.sender);
			if (player != null)
			{
				int newMonsterId = IdGenerator.NewMonsterId;
				NetInterface.Instantiate(PrefabManager.Self.AiGroupNetwork, pos, Quaternion.identity, info.networkView.group, newMonsterId, num, player.Id, -1, -1, -1, -1);
			}
		}
	}

	public static void RPC_C2S_CreateAdNpc(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!ServerConfig.IsStory)
		{
			int templateId = stream.Read<int>(new object[0]);
			Vector3 pos = stream.Read<Vector3>(new object[0]);
			float scale = Random.Range(ModelScaleMin, ModelScaleMax);
			Player player = Player.GetPlayer(info.sender);
			int controllerId = ((!(null == player)) ? player.Id : (-1));
			int worldId = ((!(null == player)) ? player.WorldId : 200);
			int num = CreateNpc(controllerId, worldId, pos, templateId, 1, scale, -1, isStand: false, 0f);
			if (num != -1)
			{
			}
		}
	}

	public static void PT_NPC_CreateAdMainNpc(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (!ServerConfig.IsStory)
		{
			int templateId = stream.Read<int>(new object[0]);
			Vector3 pos = stream.Read<Vector3>(new object[0]);
			float scale = Random.Range(ModelScaleMin, ModelScaleMax);
			Player player = Player.GetPlayer(info.sender);
			int controllerId = ((!(null == player)) ? player.Id : (-1));
			int worldId = ((!(null == player)) ? player.WorldId : 200);
			int num = CreateNpc(controllerId, worldId, pos, templateId, 7, scale, -1, isStand: false, 0f);
			if (num != -1)
			{
			}
		}
	}

	public static void RPC_C2S_CreateStRdNpc(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int nObjID = stream.Read<int>(new object[0]);
		Vector3 pos = stream.Read<Vector3>(new object[0]);
		int templateId = stream.Read<int>(new object[0]);
		float scale = Random.Range(ModelScaleMin, ModelScaleMax);
		Player player = Player.GetPlayer(info.sender);
		int controllerId = ((!(null == player)) ? player.Id : (-1));
		int worldId = ((!(null == player)) ? player.WorldId : 200);
		int num = CreateNpc(controllerId, worldId, pos, templateId, 4, scale, nObjID, isStand: false, 0f);
		if (num != -1)
		{
		}
	}

	public static void RPC_C2S_CreateStNpc(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		int nObjID = stream.Read<int>(new object[0]);
		Vector3 pos = stream.Read<Vector3>(new object[0]);
		int templateId = stream.Read<int>(new object[0]);
		float scale = Random.Range(ModelScaleMin, ModelScaleMax);
		Player player = Player.GetPlayer(info.sender);
		int controllerId = ((!(null == player)) ? player.Id : (-1));
		int worldId = ((!(null == player)) ? player.WorldId : 200);
		int num = CreateNpc(controllerId, worldId, pos, templateId, 4, scale, nObjID, isStand: false, 0f);
		if (num != -1)
		{
		}
	}

	public static void RPC_C2S_SpawnPos(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		byte[] buffer = stream.Read<byte[]>(new object[0]);
		using MemoryStream input = new MemoryStream(buffer);
		using BinaryReader binaryReader = new BinaryReader(input);
		int num = binaryReader.ReadInt32();
		int num2 = binaryReader.ReadInt32();
		int num3 = binaryReader.ReadInt32();
		for (int i = 0; i < num3; i++)
		{
			int key = binaryReader.ReadInt32();
			if (!_spawnPos.ContainsKey(key))
			{
				_spawnPos[key] = new List<EntityType>();
			}
		}
	}

	public static void RPC_C2S_CreateAdNpcByIndex(uLink.BitStream stream, uLink.NetworkMessageInfo info)
	{
		if (ServerConfig.IsStory)
		{
			return;
		}
		Vector3 pos = stream.Read<Vector3>(new object[0]);
		int num = stream.Read<int>(new object[0]);
		int num2 = stream.Read<int>(new object[0]);
		bool isStand = stream.Read<bool>(new object[0]);
		float rotY = stream.Read<float>(new object[0]);
		Player player = Player.GetPlayer(info.sender);
		int controllerId = ((!(null == player)) ? player.Id : (-1));
		int worldId = ((!(null == player)) ? player.WorldId : 200);
		switch (num2)
		{
		case 0:
		{
			Vector2 pos2 = new Vector2(pos.x, pos.z);
			if (NpcManager.HasTownNpc(pos2))
			{
				if (LogFilter.logDebug)
				{
					Debug.LogWarning("Can not be repeated to create Town Npc !!!");
				}
				break;
			}
			AdNpcData adNpcData2 = NpcMissionDataRepository.GetAdNpcData(num);
			if (adNpcData2 != null)
			{
				float scale2 = Random.Range(ModelScaleMin, ModelScaleMax);
				int num4 = CreateNpc(controllerId, worldId, pos, num, 2, scale2, -1, isStand: false, 0f);
				if (num4 != -1)
				{
					NpcManager.AddTownNpc(pos2, num4);
				}
			}
			break;
		}
		case 1:
		{
			if (NpcManager.HasBuildingNpc(pos))
			{
				if (LogFilter.logDebug)
				{
					Debug.LogWarning("Can not be repeated to create Town Npc !!!");
				}
				break;
			}
			AdNpcData adNpcData = NpcMissionDataRepository.GetAdNpcData(num);
			if (adNpcData != null)
			{
				float scale = Random.Range(ModelScaleMin, ModelScaleMax);
				int num3 = CreateNpc(controllerId, worldId, pos, num, 3, scale, -1, isStand, rotY);
				if (num3 != -1)
				{
					NpcManager.AddBuildingNpc(pos, num3);
				}
			}
			break;
		}
		}
	}
}
