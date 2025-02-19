using System.Collections.Generic;
using ItemAsset;
using Pathea;
using Pathea.PeEntityExt;
using Pathea.PeEntityExtTrans;
using PETools;
using UnityEngine;

namespace PeCustom;

public class SceneAgentsContoller : PeCustomScene.SceneElement, ISceneController, IMonoLike
{
	private const string sMonsterGroupPath = "Prefab/Custom/CustomMonsterGroup";

	private HashBinder mBinder = new HashBinder();

	private List<SceneEntityAgent> mMstDeadAgents = new List<SceneEntityAgent>(10);

	public HashBinder Binder => mBinder;

	public void OnNotification(ESceneNoification msg_type, params object[] data)
	{
		SpawnDataSource spawnDataSource = mBinder.Get<SpawnDataSource>();
		switch (msg_type)
		{
		case ESceneNoification.SceneBegin:
			if (PeGameMgr.IsSingle)
			{
				CreateAgents(spawnDataSource);
			}
			break;
		case ESceneNoification.CreateAgent:
		{
			if (data.Length == 0)
			{
				Debug.LogError("Create Agent notification parameters error");
				break;
			}
			SpawnPoint sp = data[0] as SpawnPoint;
			CreateAgent(spawnDataSource, sp);
			break;
		}
		case ESceneNoification.RemoveSpawnPoint:
		{
			if (data.Length == 0)
			{
				Debug.LogError("Remove SpawnPoint notification parameters error");
				break;
			}
			SpawnPoint spawnPoint2 = data[0] as SpawnPoint;
			if (spawnPoint2 is MonsterSpawnPoint)
			{
				MonsterSpawnPoint monsterSpawnPoint = spawnPoint2 as MonsterSpawnPoint;
				if (monsterSpawnPoint.EntityID != -1)
				{
					PeSingleton<CreatureMgr>.Instance.Destory(spawnPoint2.EntityID);
				}
				if (monsterSpawnPoint.agent != null)
				{
					SceneMan.RemoveSceneObj(monsterSpawnPoint.agent);
				}
				spawnDataSource.RemoveMonster(monsterSpawnPoint.ID);
			}
			else if (spawnPoint2 is NPCSpawnPoint)
			{
				NPCSpawnPoint nPCSpawnPoint = spawnPoint2 as NPCSpawnPoint;
				if (nPCSpawnPoint.EntityID != -1)
				{
					PeSingleton<CreatureMgr>.Instance.Destory(spawnPoint2.EntityID);
				}
				if (nPCSpawnPoint.agent != null)
				{
					SceneMan.RemoveSceneObj(nPCSpawnPoint.agent);
				}
				spawnDataSource.RemoveMonster(nPCSpawnPoint.ID);
			}
			else if (spawnPoint2 is DoodadSpawnPoint)
			{
				DoodadSpawnPoint doodadSpawnPoint = spawnPoint2 as DoodadSpawnPoint;
				if (doodadSpawnPoint.EntityID != -1)
				{
					PeSingleton<CreatureMgr>.Instance.Destory(spawnPoint2.EntityID);
				}
				if (doodadSpawnPoint.agent != null)
				{
					SceneMan.RemoveSceneObj(doodadSpawnPoint.agent);
				}
				spawnDataSource.RemoveMonster(doodadSpawnPoint.ID);
			}
			else
			{
				if (!(spawnPoint2 is ItemSpwanPoint))
				{
					break;
				}
				ItemSpwanPoint itemSpwanPoint = spawnPoint2 as ItemSpwanPoint;
				List<ISceneObjAgent> sceneObjs = SceneMan.GetSceneObjs<DragArticleAgent>();
				for (int i = 0; i < sceneObjs.Count; i++)
				{
					if (sceneObjs[i] is DragArticleAgent dragArticleAgent && dragArticleAgent.itemDrag.itemObj.instanceId == itemSpwanPoint.ItemObjId)
					{
						PeSingleton<ItemMgr>.Instance.DestroyItem(itemSpwanPoint.ItemObjId);
						SceneMan.RemoveSceneObj(dragArticleAgent);
						break;
					}
				}
			}
			break;
		}
		case ESceneNoification.EnableSpawnPoint:
		{
			if (data.Length < 1)
			{
				Debug.LogError("Enable SpawnPoint notification parameters error");
				break;
			}
			SpawnPoint spawnPoint3 = data[0] as SpawnPoint;
			bool enable = (bool)data[1];
			if (spawnPoint3 is MonsterSpawnArea)
			{
				MonsterSpawnArea monsterSpawnArea = spawnPoint3 as MonsterSpawnArea;
				for (int j = 0; j < monsterSpawnArea.Spawns.Count; j++)
				{
					for (int k = 0; k < monsterSpawnArea.Spawns[j].spawnPoints.Count; k++)
					{
						monsterSpawnArea.Spawns[j].spawnPoints[k].Enable = enable;
					}
				}
			}
			spawnPoint3.Enable = enable;
			break;
		}
		case ESceneNoification.CreateMonster:
		{
			if (data.Length < 2)
			{
				Debug.LogError("The [CreateMonster] notification parameter is wrong");
				break;
			}
			SceneEntityAgent sceneEntityAgent = data[0] as SceneEntityAgent;
			bool save = (bool)data[1];
			bool flag2 = true;
			if (data.Length > 2)
			{
				flag2 = (bool)data[2];
			}
			Vector3 outPutPos = sceneEntityAgent.spawnPoint.spawnPos;
			if (flag2 && !CheckPos(out outPutPos, outPutPos, sceneEntityAgent.spawnPoint, sceneEntityAgent.spawnArea))
			{
				break;
			}
			sceneEntityAgent.spawnPoint.spawnPos = outPutPos;
			if (sceneEntityAgent.groupPoints != null)
			{
				sceneEntityAgent.entityGp = CreateMonsterGroup(sceneEntityAgent.spawnPoint, sceneEntityAgent.groupPoints, sceneEntityAgent.spawnArea);
				sceneEntityAgent.entity = sceneEntityAgent.entityGp;
				sceneEntityAgent.entity.scenarioId = sceneEntityAgent.ScenarioId;
				break;
			}
			sceneEntityAgent.entity = CreateMonster(sceneEntityAgent.mstPoint, save);
			sceneEntityAgent.entity.scenarioId = sceneEntityAgent.ScenarioId;
			if (sceneEntityAgent.entityGp != null)
			{
				sceneEntityAgent.entity.transform.parent = sceneEntityAgent.entityGp.transform;
			}
			Debug.Log("Create the Monster ");
			break;
		}
		case ESceneNoification.CreateNpc:
		{
			if (data.Length == 0)
			{
				Debug.LogError("The [CreateNpc] notification parameters are wrong");
				break;
			}
			SceneEntityAgent sceneEntityAgent3 = data[0] as SceneEntityAgent;
			bool flag3 = true;
			if (data.Length > 1)
			{
				flag3 = (bool)data[1];
			}
			Vector3 outPutPos2 = sceneEntityAgent3.spawnPoint.spawnPos;
			if (!flag3 || CheckPos(out outPutPos2, outPutPos2, sceneEntityAgent3.spawnPoint, sceneEntityAgent3.spawnArea))
			{
				sceneEntityAgent3.spawnPoint.spawnPos = outPutPos2;
				sceneEntityAgent3.entity = CreateNpc(sceneEntityAgent3.spawnPoint as NPCSpawnPoint);
				if (sceneEntityAgent3.entity == null)
				{
					sceneEntityAgent3.entity.scenarioId = sceneEntityAgent3.ScenarioId;
					Debug.LogError("[SceneEntityCreator]Failed to create npc:" + sceneEntityAgent3.protoId);
				}
				else
				{
					Debug.Log("Create the Npc [" + sceneEntityAgent3.entity.Id + "]");
				}
			}
			break;
		}
		case ESceneNoification.CreateDoodad:
		{
			if (data.Length < 2)
			{
				Debug.LogError("The [CreateNpc] notification parameters are wrong");
				break;
			}
			SceneStaticAgent sceneStaticAgent = data[0] as SceneStaticAgent;
			sceneStaticAgent.entity = CreadteDoodad(sceneStaticAgent.spawnPoint as DoodadSpawnPoint, sceneStaticAgent.IsSave);
			sceneStaticAgent.entity.scenarioId = sceneStaticAgent.ScenarioId;
			break;
		}
		case ESceneNoification.MonsterDead:
		{
			if (data.Length == 0)
			{
				Debug.LogError("The [MonsterDead] notification parameters are wrong ");
				break;
			}
			SceneEntityAgent sceneEntityAgent2 = data[0] as SceneEntityAgent;
			MonsterSpawnPoint mstPoint = sceneEntityAgent2.mstPoint;
			if (mstPoint == null)
			{
				Debug.LogError("he [MonsterDead] notification : the point is not a MonsterSpawnPoint");
				break;
			}
			mstPoint.isDead = true;
			mstPoint.EntityID = -1;
			Debug.Log("The monster [" + sceneEntityAgent2.entity.Id + "] is Dead");
			sceneEntityAgent2.entity = null;
			if (sceneEntityAgent2.spawnArea != null)
			{
				if (sceneEntityAgent2.spawnArea.MaxRespawnCount != 0)
				{
					AddMstDeadAgent(sceneEntityAgent2);
				}
			}
			else if (mstPoint.MaxRespawnCount != 0)
			{
				AddMstDeadAgent(sceneEntityAgent2);
			}
			break;
		}
		case ESceneNoification.DoodadDead:
		{
			if (data.Length == 0)
			{
				Debug.LogError("The [DoodadDead] notification parameters are wrong ");
				break;
			}
			SceneStaticAgent sceneStaticAgent2 = data[0] as SceneStaticAgent;
			if (!(sceneStaticAgent2.spawnPoint is DoodadSpawnPoint doodadSpawnPoint2))
			{
				Debug.LogError("he [DoodadDead] notification : the point is not a DoodadSpawnPoint");
				break;
			}
			doodadSpawnPoint2.isDead = true;
			doodadSpawnPoint2.EntityID = -1;
			break;
		}
		case ESceneNoification.EntityDestroy:
		{
			if (data.Length < 2)
			{
				Debug.LogError("The [EntityDestroy] notification parameters are wrong ");
				break;
			}
			SpawnPoint spawnPoint = data[0] as SpawnPoint;
			PeEntity peEntity = data[1] as PeEntity;
			bool flag = false;
			if (data.Length > 2)
			{
				flag = (bool)data[2];
			}
			if (flag)
			{
				peEntity.Export();
				PeSingleton<CreatureMgr>.Instance.Destory(spawnPoint.EntityID);
				spawnPoint.EntityID = -1;
			}
			else
			{
				PeSingleton<CreatureMgr>.Instance.DestroyAndDontRemove(spawnPoint.EntityID);
			}
			break;
		}
		}
	}

	public void AddMstDeadAgent(SceneEntityAgent agent)
	{
		MonsterSpawnPoint mstPoint = agent.mstPoint;
		if (mstPoint == null)
		{
			return;
		}
		if (agent.spawnArea != null)
		{
			if (agent.spawnArea.MaxRespawnCount != 0)
			{
				mMstDeadAgents.Add(agent);
			}
		}
		else if (agent.mstPoint.MaxRespawnCount != 0)
		{
			mMstDeadAgents.Add(agent);
		}
	}

	private void DestroyEntity()
	{
	}

	public void OnGUI()
	{
	}

	public void Start()
	{
	}

	public void Update()
	{
		for (int num = mMstDeadAgents.Count - 1; num >= 0; num--)
		{
			SceneEntityAgent sceneEntityAgent = mMstDeadAgents[num];
			MonsterSpawnPoint mstPoint = sceneEntityAgent.mstPoint;
			if (!mstPoint.UpdateRespawnTime(Time.deltaTime))
			{
				continue;
			}
			if (sceneEntityAgent.spawnArea != null)
			{
				if (sceneEntityAgent.spawnArea.MaxRespawnCount == 0)
				{
					mMstDeadAgents.RemoveAt(num);
					continue;
				}
				sceneEntityAgent.spawnArea.MaxRespawnCount--;
			}
			else
			{
				if (mstPoint.MaxRespawnCount == 0)
				{
					mMstDeadAgents.RemoveAt(num);
					continue;
				}
				mstPoint.MaxRespawnCount--;
			}
			mstPoint.isDead = false;
			sceneEntityAgent.Respawn();
			Debug.Log("The Agent [" + sceneEntityAgent.Id + "] is respawned");
			mMstDeadAgents.RemoveAt(num);
		}
	}

	public void OnDestroy()
	{
	}

	public static bool CheckPos(out Vector3 outPutPos, Vector3 curPos, SpawnPoint point, MonsterSpawnArea area)
	{
		outPutPos = curPos;
		MonsterProtoDb.Item item = MonsterProtoDb.Get(point.Prototype);
		bool flag = false;
		if (item.movementField == MovementField.Land)
		{
			flag = CheckOnLand(out outPutPos, curPos, point, area);
		}
		else if (item.movementField == MovementField.Sky)
		{
			flag = CheckOnSky(out outPutPos, curPos, point, area);
		}
		else if (item.movementField == MovementField.water)
		{
			flag = CheckInWater(out outPutPos, curPos, point, area);
		}
		else if (item.movementField == MovementField.Amphibian)
		{
			flag = CheckOnLand(out outPutPos, curPos, point, area);
			if (!flag)
			{
				flag = CheckInWater(out outPutPos, curPos, point, area);
			}
		}
		else if (item.movementField == MovementField.All)
		{
			flag = CheckOnLand(out outPutPos, curPos, point, area);
			if (!flag)
			{
				flag = CheckOnSky(out outPutPos, curPos, point, area);
			}
			if (!flag)
			{
				flag = CheckInWater(out outPutPos, curPos, point, area);
			}
		}
		return flag;
	}

	private static bool CheckOnLand(out Vector3 outPutPos, Vector3 curPos, SpawnPoint point, MonsterSpawnArea area)
	{
		outPutPos = curPos;
		if (!(point is MonsterSpawnPoint monsterSpawnPoint) || area == null)
		{
			int num = 1000;
			RaycastHit[] array = Physics.RaycastAll(new Vector3(curPos.x, curPos.y + (float)num, curPos.z), Vector3.down, num * 2, SceneMan.DependenceLayer);
			if (array != null && array.Length != 0)
			{
				float sqrMagnitude = (curPos - array[0].point).sqrMagnitude;
				RaycastHit raycastHit = array[0];
				for (int i = 1; i < array.Length; i++)
				{
					float sqrMagnitude2 = (curPos - array[i].point).sqrMagnitude;
					if (sqrMagnitude > sqrMagnitude2)
					{
						raycastHit = array[i];
					}
				}
				outPutPos = raycastHit.point;
				outPutPos.y += 1f;
				return true;
			}
		}
		else
		{
			Vector3[] array2 = new Vector3[8]
			{
				area.spawnPos + area.Rotation * new Vector3(area.Scale.x, area.Scale.y, area.Scale.z) * 0.5f,
				area.spawnPos + area.Rotation * new Vector3(0f - area.Scale.x, area.Scale.y, area.Scale.z) * 0.5f,
				area.spawnPos + area.Rotation * new Vector3(area.Scale.x, 0f - area.Scale.y, area.Scale.z) * 0.5f,
				area.spawnPos + area.Rotation * new Vector3(0f - area.Scale.x, 0f - area.Scale.y, area.Scale.z) * 0.5f,
				area.spawnPos + area.Rotation * new Vector3(area.Scale.x, area.Scale.y, 0f - area.Scale.z) * 0.5f,
				area.spawnPos + area.Rotation * new Vector3(0f - area.Scale.x, area.Scale.y, 0f - area.Scale.z) * 0.5f,
				area.spawnPos + area.Rotation * new Vector3(area.Scale.x, 0f - area.Scale.y, 0f - area.Scale.z) * 0.5f,
				area.spawnPos + area.Rotation * new Vector3(0f - area.Scale.x, 0f - area.Scale.y, 0f - area.Scale.z) * 0.5f
			};
			float[] array3 = new float[8];
			for (int j = 0; j < 8; j++)
			{
				array3[j] = array2[j].y;
			}
			float num2 = Mathf.Min(array3) - 4f;
			float num3 = Mathf.Max(array3) + 4f;
			RaycastHit[] array4 = Physics.RaycastAll(new Vector3(curPos.x, num3, curPos.z), Vector3.down, num3 - num2, SceneMan.DependenceLayer);
			if (array4 != null && array4.Length != 0)
			{
				float num4 = 100000f;
				for (int k = 0; k < array4.Length; k++)
				{
					if (!(num4 > array4[k].distance))
					{
						continue;
					}
					Vector3 point2 = area.spawnPos + Quaternion.Inverse(area.Rotation) * (array4[k].point - area.spawnPos);
					if (monsterSpawnPoint.bound.Contains(point2))
					{
						outPutPos = array4[k].point;
						num4 = array4[k].distance;
						continue;
					}
					if (outPutPos.y > array4[k].point.y)
					{
						outPutPos.y = array4[k].point.y + 0.5f;
					}
					num4 = array4[k].distance;
				}
				return true;
			}
		}
		return false;
	}

	private static bool CheckInWater(out Vector3 outPutPos, Vector3 curPos, SpawnPoint point, MonsterSpawnArea area)
	{
		outPutPos = curPos;
		if (PE.PointInWater(curPos) > 0.52f && PE.PointInTerrain(curPos) < 0.52f)
		{
			return true;
		}
		if (!(point is MonsterSpawnPoint monsterSpawnPoint) || area == null)
		{
			int num = 5;
			Vector3 vector = new Vector3(curPos.x, curPos.y - (float)num, curPos.z);
			for (int i = 0; i < num * 2; i++)
			{
				vector.y += 1f;
				if (PE.PointInWater(vector) > 0.52f && PE.PointInTerrain(vector) < 0.52f)
				{
					outPutPos = vector;
					return true;
				}
			}
		}
		else
		{
			int min = Mathf.FloorToInt(monsterSpawnPoint.bound.center.x - monsterSpawnPoint.bound.extents.x);
			int max = Mathf.FloorToInt(monsterSpawnPoint.bound.center.x + monsterSpawnPoint.bound.extents.x);
			int min2 = Mathf.FloorToInt(monsterSpawnPoint.bound.center.y - monsterSpawnPoint.bound.extents.y);
			int max2 = Mathf.FloorToInt(monsterSpawnPoint.bound.center.y + monsterSpawnPoint.bound.extents.y);
			int min3 = Mathf.FloorToInt(monsterSpawnPoint.bound.center.z - monsterSpawnPoint.bound.extents.z);
			int max3 = Mathf.FloorToInt(monsterSpawnPoint.bound.center.z + monsterSpawnPoint.bound.extents.z);
			int num2 = 50;
			while (num2 > 0)
			{
				num2--;
				Vector3 vector2 = new Vector3(Random.Range(min, max), Random.Range(min2, max2), Random.Range(min3, max3));
				vector2 = area.spawnPos + area.Rotation * (vector2 - area.spawnPos);
				if (PE.PointInWater(vector2) > 0.52f)
				{
					outPutPos = vector2;
					return true;
				}
			}
		}
		return false;
	}

	private static bool CheckOnSky(out Vector3 outPutPos, Vector3 curPos, SpawnPoint point, MonsterSpawnArea area)
	{
		outPutPos = curPos;
		if (PE.PointInTerrain(outPutPos) < 0.52f && PE.PointInWater(outPutPos) < 0.52f)
		{
			return true;
		}
		if (!(point is MonsterSpawnPoint monsterSpawnPoint) || area == null)
		{
			int num = 5;
			Vector3 vector = new Vector3(curPos.x, curPos.y + (float)num, curPos.z);
			for (int i = 0; i < num * 2; i++)
			{
				vector.y -= 1f;
				if (PE.PointInTerrain(vector) < 0.52f && PE.PointInWater(outPutPos) < 0.52f)
				{
					outPutPos = vector;
					return true;
				}
			}
		}
		else
		{
			Vector3[] array = new Vector3[8];
			Vector3 center = monsterSpawnPoint.bound.center;
			Vector3 size = monsterSpawnPoint.bound.size;
			ref Vector3 reference = ref array[0];
			reference = area.spawnPos + area.Rotation * (area.spawnPos - (center + new Vector3(size.x, size.y, size.z)));
			ref Vector3 reference2 = ref array[1];
			reference2 = area.spawnPos + area.Rotation * (area.spawnPos - (center + new Vector3(0f - size.x, size.y, size.z)));
			ref Vector3 reference3 = ref array[2];
			reference3 = area.spawnPos + area.Rotation * (area.spawnPos - (center + new Vector3(size.x, 0f - size.y, size.z)));
			ref Vector3 reference4 = ref array[3];
			reference4 = area.spawnPos + area.Rotation * (area.spawnPos - (center + new Vector3(0f - size.x, 0f - size.y, size.z)));
			ref Vector3 reference5 = ref array[4];
			reference5 = area.spawnPos + area.Rotation * (area.spawnPos - (center + new Vector3(size.x, size.y, 0f - size.z)));
			ref Vector3 reference6 = ref array[5];
			reference6 = area.spawnPos + area.Rotation * (area.spawnPos - (center + new Vector3(0f - size.x, size.y, 0f - size.z)));
			ref Vector3 reference7 = ref array[6];
			reference7 = area.spawnPos + area.Rotation * (area.spawnPos - (center + new Vector3(size.x, 0f - size.y, 0f - size.z)));
			ref Vector3 reference8 = ref array[7];
			reference8 = area.spawnPos + area.Rotation * (area.spawnPos - (center + new Vector3(0f - size.x, 0f - size.y, 0f - size.z)));
			float[] array2 = new float[8];
			for (int j = 0; j < 8; j++)
			{
				array2[j] = array[j].y;
			}
			float num2 = Mathf.Min(array2);
			float num3 = Mathf.Max(array2);
			int num4 = 2;
			for (float num5 = num3; num5 > num2 - 1f; num5 -= (float)num4)
			{
				Vector3 vector2 = new Vector3(outPutPos.x, num5, outPutPos.z);
				if (PE.PointInTerrain(vector2) < 0.52f && PE.PointInWater(vector2) < 0.52f && area.PointIn(vector2))
				{
					outPutPos = vector2;
					return true;
				}
			}
		}
		return false;
	}

	private void CreateAgents(SpawnDataSource ds)
	{
		foreach (KeyValuePair<int, MonsterSpawnPoint> monster in ds.monsters)
		{
			SceneEntityAgent sceneEntityAgent = _createMstAgent(monster.Value, is_saved: true, null, null, !monster.Value.isDead);
			sceneEntityAgent.ScenarioId = monster.Value.ID;
			monster.Value.agent = sceneEntityAgent;
			AddMstDeadAgent(sceneEntityAgent);
		}
		foreach (KeyValuePair<int, MonsterSpawnArea> area in ds.areas)
		{
			for (int i = 0; i < area.Value.Spawns.Count; i++)
			{
				MonsterSpawnArea.SocialSpawns socialSpawns = area.Value.Spawns[i];
				if (!socialSpawns.isSocial)
				{
					for (int j = 0; j < socialSpawns.spawnPoints.Count; j++)
					{
						MonsterSpawnPoint monsterSpawnPoint = socialSpawns.spawnPoints[j];
						SceneEntityAgent sceneEntityAgent2 = _createMstAgent(monsterSpawnPoint, is_saved: true, area.Value, null, !monsterSpawnPoint.isDead);
						sceneEntityAgent2.ScenarioId = area.Value.ID;
						monsterSpawnPoint.agent = sceneEntityAgent2;
						AddMstDeadAgent(sceneEntityAgent2);
					}
				}
				else
				{
					_createMstAgent(socialSpawns.centerSP, is_saved: false, area.Value, socialSpawns.spawnPoints.ToArray());
				}
			}
		}
		CreateNpcAgents(ds.npcs);
		CreateDoodadAgents(ds.doodads);
		foreach (KeyValuePair<int, EffectSpwanPoint> effect in ds.effects)
		{
			SceneStaticEffectAgent sceneStaticEffectAgent = SceneStaticEffectAgent.Create(effect.Value.Prototype, effect.Value.spawnPos, effect.Value.Rotation, effect.Value.Scale);
			sceneStaticEffectAgent.ScenarioId = effect.Value.ID;
			SceneMan.AddSceneObj(sceneStaticEffectAgent);
		}
		foreach (KeyValuePair<int, ItemSpwanPoint> item in ds.items)
		{
			if (item.Value.isNew)
			{
				DragArticleAgent dragArticleAgent = DragArticleAgent.PutItemByProroId(item.Value.Prototype, item.Value.spawnPos, item.Value.Scale, item.Value.Rotation, item.Value.CanPickup, item.Value.IsTarget);
				if (dragArticleAgent != null)
				{
					dragArticleAgent.ScenarioId = item.Value.ID;
					item.Value.isNew = false;
					item.Value.ItemObjId = dragArticleAgent.itemDrag.itemObj.instanceId;
				}
			}
		}
	}

	private void CreateAgent(SpawnDataSource ds, SpawnPoint sp)
	{
		if (sp is MonsterSpawnPoint)
		{
			MonsterSpawnPoint monsterSpawnPoint = sp as MonsterSpawnPoint;
			if (ds.AddMonster(monsterSpawnPoint))
			{
				SceneEntityAgent sceneEntityAgent = (monsterSpawnPoint.agent = _createMstAgent(monsterSpawnPoint, is_saved: true, null, null, !monsterSpawnPoint.isDead));
				sceneEntityAgent.ScenarioId = sp.ID;
				AddMstDeadAgent(sceneEntityAgent);
			}
			else
			{
				Debug.LogError("Add Monster spawn point error");
			}
		}
		else if (sp is NPCSpawnPoint)
		{
			NPCSpawnPoint nPCSpawnPoint = sp as NPCSpawnPoint;
			if (ds.AddNpc(nPCSpawnPoint))
			{
				SceneEntityAgent sceneEntityAgent2 = (nPCSpawnPoint.agent = new SceneEntityAgent(nPCSpawnPoint));
				sceneEntityAgent2.ScenarioId = sp.ID;
				SceneMan.AddSceneObj(sceneEntityAgent2);
			}
			else
			{
				Debug.LogError("Add npc spawn point error");
			}
		}
		else if (sp is DoodadSpawnPoint)
		{
			DoodadSpawnPoint doodadSpawnPoint = sp as DoodadSpawnPoint;
			if (ds.AddDoodad(doodadSpawnPoint))
			{
				SceneStaticAgent sceneStaticAgent = (doodadSpawnPoint.agent = new SceneStaticAgent(doodadSpawnPoint, is_saved: true));
				sceneStaticAgent.ScenarioId = sp.ID;
				SceneMan.AddSceneObj(sceneStaticAgent);
			}
			else
			{
				Debug.LogError("Add doodad spawn point error");
			}
		}
		else if (sp is ItemSpwanPoint)
		{
			ItemSpwanPoint itemSpwanPoint = sp as ItemSpwanPoint;
			if (ds.AddItem(itemSpwanPoint))
			{
				DragArticleAgent dragArticleAgent = DragArticleAgent.PutItemByProroId(itemSpwanPoint.Prototype, itemSpwanPoint.spawnPos, itemSpwanPoint.Scale, itemSpwanPoint.Rotation, itemSpwanPoint.CanPickup, itemSpwanPoint.IsTarget);
				if (dragArticleAgent != null)
				{
					itemSpwanPoint.isNew = false;
					itemSpwanPoint.ItemObjId = dragArticleAgent.itemDrag.itemObj.instanceId;
					dragArticleAgent.ScenarioId = sp.ID;
				}
			}
			else
			{
				Debug.LogError("Add item spawn point error");
			}
		}
		else if (!(sp is EffectSpwanPoint))
		{
		}
	}

	private SceneEntityAgent _createMstAgent(MonsterSpawnPoint _point, bool is_saved, MonsterSpawnArea _area = null, MonsterSpawnPoint[] _groupPoints = null, bool save_to_scene = true)
	{
		SceneEntityAgent sceneEntityAgent = new SceneEntityAgent(_point, is_saved, _area, _groupPoints);
		if (save_to_scene)
		{
			SceneMan.AddSceneObj(sceneEntityAgent);
		}
		return sceneEntityAgent;
	}

	public void CreateNpcAgents(Dictionary<int, NPCSpawnPoint> points)
	{
		foreach (KeyValuePair<int, NPCSpawnPoint> point in points)
		{
			SceneEntityAgent sceneEntityAgent = new SceneEntityAgent(point.Value);
			sceneEntityAgent.ScenarioId = point.Value.ID;
			point.Value.agent = sceneEntityAgent;
			SceneMan.AddSceneObj(sceneEntityAgent);
		}
	}

	public void CreateDoodadAgents(Dictionary<int, DoodadSpawnPoint> points)
	{
		foreach (KeyValuePair<int, DoodadSpawnPoint> point in points)
		{
			SceneStaticAgent sceneStaticAgent = new SceneStaticAgent(point.Value, is_saved: true);
			sceneStaticAgent.ScenarioId = point.Value.ID;
			point.Value.agent = sceneStaticAgent;
			SceneMan.AddSceneObj(sceneStaticAgent);
		}
	}

	private PeEntity CreateMonster(MonsterSpawnPoint point, bool save)
	{
		PeEntity peEntity = null;
		if (save)
		{
			if (point.EntityID == -1)
			{
				int id = PeSingleton<WorldInfoMgr>.Instance.FetchRecordAutoId();
				peEntity = PeSingleton<CreatureMgr>.Instance.CreateMonster(PeSingleton<CustomGameData.Mgr>.Instance.curGameData.WorldIndex, id, point.Prototype);
				point.EntityID = peEntity.Id;
				peEntity.ExtSetPos(point.spawnPos);
				peEntity.ExtSetRot(point.Rotation);
			}
			else
			{
				peEntity = PeSingleton<CreatureMgr>.Instance.CreateMonster(PeSingleton<CustomGameData.Mgr>.Instance.curGameData.WorldIndex, point.EntityID, point.Prototype);
			}
		}
		else
		{
			int id2 = PeSingleton<WorldInfoMgr>.Instance.FetchNonRecordAutoId();
			peEntity = PeSingleton<PeEntityCreator>.Instance.CreateMonster(id2, point.Prototype, Vector3.zero, Quaternion.identity, Vector3.one);
			point.EntityID = peEntity.Id;
			peEntity.ExtSetPos(point.spawnPos);
			peEntity.ExtSetRot(point.Rotation);
		}
		if (peEntity != null)
		{
			peEntity.scenarioId = point.ID;
			peEntity.SetAttribute(AttribType.DefaultPlayerID, point.PlayerIndex);
			PeScenarioEntity peScenarioEntity = peEntity.gameObject.GetComponent<PeScenarioEntity>();
			if (peScenarioEntity == null)
			{
				peScenarioEntity = peEntity.gameObject.AddComponent<PeScenarioEntity>();
			}
			peScenarioEntity.spawnPoint = point;
		}
		return peEntity;
	}

	private EntityGrp CreateMonsterGroup(SpawnPoint grp_sp, MonsterSpawnPoint[] points, MonsterSpawnArea area)
	{
		int entityId = PeSingleton<WorldInfoMgr>.Instance.FetchNonRecordAutoId();
		EntityGrp entityGrp = PeSingleton<EntityMgr>.Instance.Create(entityId, "Prefab/Custom/CustomMonsterGroup", Vector3.zero, Quaternion.identity, Vector3.one) as EntityGrp;
		if (entityGrp == null)
		{
			Debug.LogError("Load Prefab Error");
			return null;
		}
		entityGrp._protoId = grp_sp.Prototype;
		entityGrp._cntMin = area.AmountPerSocial;
		entityGrp._cntMax = area.AmountPerSocial;
		foreach (MonsterSpawnPoint monsterSpawnPoint in points)
		{
			PeSingleton<WorldInfoMgr>.Instance.FetchNonRecordAutoId();
			SceneEntityAgent sceneEntityAgent = new SceneEntityAgent(monsterSpawnPoint, is_saved: false, area);
			sceneEntityAgent.ScenarioId = area.ID;
			entityGrp.scenarioId = area.ID;
			sceneEntityAgent.entityGp = entityGrp;
			monsterSpawnPoint.agent = sceneEntityAgent;
			AddMstDeadAgent(sceneEntityAgent);
			if (!monsterSpawnPoint.isDead)
			{
				SceneMan.AddSceneObj(sceneEntityAgent);
			}
		}
		return entityGrp;
	}

	private PeEntity CreateNpc(NPCSpawnPoint point)
	{
		PeEntity peEntity = null;
		if (point.EntityID == -1)
		{
			int id = PeSingleton<WorldInfoMgr>.Instance.FetchRecordAutoId();
			peEntity = PeSingleton<CreatureMgr>.Instance.CreateNpc(PeSingleton<CustomGameData.Mgr>.Instance.curGameData.WorldIndex, id, point.Prototype);
			point.EntityID = peEntity.Id;
			peEntity.ExtSetName(new CharacterName(point.Name));
			PeTrans peTrans = peEntity.peTrans;
			if (null == peTrans)
			{
				Debug.LogError("[SceneEntityCreator]No viewCmpt in npc:" + point.Prototype);
				return null;
			}
			peTrans.position = point.spawnPos;
			peTrans.rotation = point.Rotation;
			peTrans.scale = point.Scale;
		}
		else
		{
			peEntity = PeSingleton<CreatureMgr>.Instance.CreateNpc(PeSingleton<CustomGameData.Mgr>.Instance.curGameData.WorldIndex, point.EntityID, point.Prototype);
			if (peEntity == null)
			{
				Debug.LogError("Cant Finde the Entity [ID " + point.EntityID + "]");
			}
		}
		if (peEntity != null)
		{
			peEntity.scenarioId = point.ID;
			peEntity.SetAttribute(AttribType.DefaultPlayerID, point.PlayerIndex);
			peEntity.SetBirthPos(point.spawnPos);
			PeScenarioEntity peScenarioEntity = peEntity.gameObject.GetComponent<PeScenarioEntity>();
			if (peScenarioEntity == null)
			{
				peScenarioEntity = peEntity.gameObject.AddComponent<PeScenarioEntity>();
			}
			peScenarioEntity.spawnPoint = point;
		}
		return peEntity;
	}

	private PeEntity CreadteDoodad(DoodadSpawnPoint point, bool is_save)
	{
		PeEntity peEntity = null;
		if (is_save)
		{
			if (point.EntityID == -1)
			{
				int id = PeSingleton<WorldInfoMgr>.Instance.FetchRecordAutoId();
				peEntity = PeSingleton<CreatureMgr>.Instance.CreateDoodad(PeSingleton<CustomGameData.Mgr>.Instance.curGameData.WorldIndex, id, point.Prototype);
				point.EntityID = peEntity.Id;
				_initDoodad(peEntity, point);
			}
			else
			{
				peEntity = PeSingleton<CreatureMgr>.Instance.CreateDoodad(PeSingleton<CustomGameData.Mgr>.Instance.curGameData.WorldIndex, point.EntityID, point.Prototype);
				if (peEntity == null)
				{
					Debug.LogError("Cant Find the Entity [ID " + point.EntityID + "]");
				}
			}
		}
		else
		{
			PeSingleton<WorldInfoMgr>.Instance.FetchNonRecordAutoId();
			peEntity = PeSingleton<PeEntityCreator>.Instance.CreateDoodad(PeSingleton<WorldInfoMgr>.Instance.FetchNonRecordAutoId(), point.Prototype, Vector3.zero, Quaternion.identity, Vector3.one);
			point.EntityID = peEntity.Id;
			_initDoodad(peEntity, point);
		}
		if (peEntity != null)
		{
			peEntity.scenarioId = point.ID;
			PeScenarioEntity peScenarioEntity = peEntity.gameObject.GetComponent<PeScenarioEntity>();
			if (peScenarioEntity == null)
			{
				peScenarioEntity = peEntity.gameObject.AddComponent<PeScenarioEntity>();
			}
			peScenarioEntity.spawnPoint = point;
		}
		return peEntity;
	}

	private bool _initDoodad(PeEntity entity, DoodadSpawnPoint sp)
	{
		if (entity == null)
		{
			Debug.LogError("The entity that given is null");
			return false;
		}
		PeTrans peTrans = entity.peTrans;
		if (null == peTrans)
		{
			Debug.LogError("[SceneEntityCreator]No viewCmpt in doodad:" + sp.Prototype);
			return false;
		}
		peTrans.position = sp.spawnPos;
		peTrans.rotation = sp.Rotation;
		peTrans.scale = sp.Scale;
		SceneDoodadLodCmpt cmpt = entity.GetCmpt<SceneDoodadLodCmpt>();
		if (cmpt != null)
		{
			cmpt.IsShown = sp.Visible;
			cmpt.IsDamagable = sp.IsTarget;
		}
		return true;
	}
}
