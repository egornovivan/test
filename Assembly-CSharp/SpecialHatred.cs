using System.Collections.Generic;
using Pathea;
using SkillSystem;
using UnityEngine;

public class SpecialHatred
{
	private enum hatredType
	{
		recovery,
		ignore,
		highHatred
	}

	private const int allMonsters = 8888;

	private const int fixMonsBase = 8000;

	private const int allNpcs = 30000;

	private const int mainPlayer = 20000;

	private const int npcBase = 9000;

	private static Dictionary<int, Dictionary<int, int>> dstEntity_hatredData = new Dictionary<int, Dictionary<int, int>>();

	private static List<int> entityHarmRecord = new List<int>();

	public static void ClearRecord()
	{
		dstEntity_hatredData.Clear();
		entityHarmRecord.Clear();
	}

	public static void MonsterHatredAdd(List<int> tmp)
	{
		if (tmp.Count != 5)
		{
			Debug.LogError("MonsterHatredAdd's format is wrong!");
			return;
		}
		List<PeEntity> list;
		int key;
		if (tmp[1] == 1)
		{
			list = new List<PeEntity>(PeSingleton<EntityMgr>.Instance.All);
			list = list.FindAll(delegate(PeEntity e)
			{
				if (e == null)
				{
					return false;
				}
				return (e.proto == EEntityProto.Monster && e.entityProto.protoId == tmp[2]) ? true : false;
			});
			key = tmp[2];
		}
		else
		{
			list = new List<PeEntity>();
			PeEntity entityByFixedSpId = PeSingleton<SceneEntityCreatorArchiver>.Instance.GetEntityByFixedSpId(tmp[2]);
			if (entityByFixedSpId != null)
			{
				list.Add(entityByFixedSpId);
			}
			key = tmp[2] + 8000;
		}
		int src;
		List<PeEntity> list2;
		if (tmp[3] == 1)
		{
			src = tmp[4];
			list2 = new List<PeEntity>(PeSingleton<EntityMgr>.Instance.All);
			list2 = list2.FindAll(delegate(PeEntity n)
			{
				if (n == null)
				{
					return false;
				}
				return (n.proto == EEntityProto.Monster && n.entityProto.protoId == src) ? true : false;
			});
		}
		else if (tmp[3] == 2)
		{
			src = tmp[4] + 8000;
			list2 = new List<PeEntity>();
			list2.Add(PeSingleton<SceneEntityCreatorArchiver>.Instance.GetEntityByFixedSpId(tmp[4]));
		}
		else if (tmp[3] == 3)
		{
			src = 8888;
			list2 = new List<PeEntity>(PeSingleton<EntityMgr>.Instance.All);
			list2 = list2.FindAll(delegate(PeEntity n)
			{
				if (n == null)
				{
					return false;
				}
				return n.proto == EEntityProto.Monster;
			});
		}
		else
		{
			src = tmp[4];
			if (tmp[4] == 30000)
			{
				list2 = new List<PeEntity>(PeSingleton<EntityMgr>.Instance.All);
				list2 = list2.FindAll(delegate(PeEntity n)
				{
					if (n == null)
					{
						return false;
					}
					return (n.proto == EEntityProto.Npc || n.proto == EEntityProto.RandomNpc) ? true : false;
				});
			}
			else
			{
				list2 = new List<PeEntity>();
				if (tmp[4] == 25001 && ServantLeaderCmpt.Instance.GetServant(0) != null)
				{
					list2.Add(ServantLeaderCmpt.Instance.GetServant(0).Entity);
				}
				else if (tmp[4] == 25002 && ServantLeaderCmpt.Instance.GetServant(1) != null)
				{
					list2.Add(ServantLeaderCmpt.Instance.GetServant(1).Entity);
				}
				else if (tmp[4] == 25010)
				{
					if (ServantLeaderCmpt.Instance.GetServant(0) != null)
					{
						list2.Add(ServantLeaderCmpt.Instance.GetServant(0).Entity);
					}
					if (ServantLeaderCmpt.Instance.GetServant(1) != null)
					{
						list2.Add(ServantLeaderCmpt.Instance.GetServant(1).Entity);
					}
				}
				else
				{
					list2.Add(PeSingleton<EntityMgr>.Instance.Get(tmp[4]));
				}
			}
		}
		if (tmp[0] == 0)
		{
			foreach (PeEntity item in list)
			{
				SkEntity aliveEntity = item.aliveEntity;
				if (!(aliveEntity == null) && aliveEntity.SpecialHatredData.ContainsKey(src))
				{
					aliveEntity.SpecialHatredData.Remove(src);
				}
			}
			if (dstEntity_hatredData.ContainsKey(key))
			{
				if (dstEntity_hatredData[key].ContainsKey(src))
				{
					dstEntity_hatredData[key].Remove(src);
				}
				if (dstEntity_hatredData[key].Count == 0)
				{
					dstEntity_hatredData.Remove(key);
				}
			}
			if (dstEntity_hatredData.Count == 0)
			{
				MonsterEntityCreator.commonCreateEvent -= SetSpecialHatred;
			}
			return;
		}
		if (tmp[0] == 3)
		{
			foreach (PeEntity item2 in list2)
			{
				if (item2 == null)
				{
					continue;
				}
				SkEntity aliveEntity2 = item2.aliveEntity;
				if (!(aliveEntity2 == null))
				{
					if (aliveEntity2.SpecialHatredData.ContainsKey(key))
					{
						aliveEntity2.SpecialHatredData[key] = tmp[0];
					}
					else
					{
						aliveEntity2.SpecialHatredData.Add(key, tmp[0]);
					}
				}
			}
			if (src > 9000)
			{
				return;
			}
			if (dstEntity_hatredData.ContainsKey(src))
			{
				if (!dstEntity_hatredData[src].ContainsKey(key))
				{
					dstEntity_hatredData[src].Add(key, tmp[0]);
				}
				else
				{
					dstEntity_hatredData[src][key] = tmp[0];
				}
				return;
			}
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			dictionary.Add(key, tmp[0]);
			if (dstEntity_hatredData.Count == 0)
			{
				MonsterEntityCreator.commonCreateEvent += SetSpecialHatred;
			}
			dstEntity_hatredData.Add(src, dictionary);
			return;
		}
		foreach (PeEntity item3 in list)
		{
			if (item3 == null)
			{
				continue;
			}
			SkEntity aliveEntity3 = item3.aliveEntity;
			if (!(aliveEntity3 == null))
			{
				if (aliveEntity3.SpecialHatredData.ContainsKey(src))
				{
					aliveEntity3.SpecialHatredData[src] = tmp[0];
				}
				else
				{
					aliveEntity3.SpecialHatredData.Add(src, tmp[0]);
				}
			}
		}
		if (dstEntity_hatredData.ContainsKey(key))
		{
			if (!dstEntity_hatredData[key].ContainsKey(src))
			{
				dstEntity_hatredData[key].Add(src, tmp[0]);
			}
			else
			{
				dstEntity_hatredData[key][src] = tmp[0];
			}
			return;
		}
		Dictionary<int, int> dictionary2 = new Dictionary<int, int>();
		dictionary2.Add(src, tmp[0]);
		if (dstEntity_hatredData.Count == 0)
		{
			MonsterEntityCreator.commonCreateEvent += SetSpecialHatred;
		}
		dstEntity_hatredData.Add(key, dictionary2);
	}

	public static void NpcHatredAdd(List<int> tmp)
	{
		if (tmp.Count != 4)
		{
			return;
		}
		List<PeEntity> list = new List<PeEntity>();
		if (tmp[1] == 20000)
		{
			list.Add(PeSingleton<PeCreature>.Instance.mainPlayer);
		}
		else if (tmp[1] == 30000)
		{
			list = new List<PeEntity>(PeSingleton<EntityMgr>.Instance.All);
			list = list.FindAll(delegate(PeEntity e)
			{
				if (e == null)
				{
					return false;
				}
				return (e.proto == EEntityProto.Npc || e.proto == EEntityProto.RandomNpc) ? true : false;
			});
			list.Add(PeSingleton<PeCreature>.Instance.mainPlayer);
		}
		else if (tmp[1] == 25001 && ServantLeaderCmpt.Instance.GetServant(0) != null)
		{
			list.Add(ServantLeaderCmpt.Instance.GetServant(0).GetComponent<PeEntity>());
		}
		else if (tmp[1] == 25002 && ServantLeaderCmpt.Instance.GetServant(1) != null)
		{
			list.Add(ServantLeaderCmpt.Instance.GetServant(1).GetComponent<PeEntity>());
		}
		else if (tmp[1] == 25010)
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
			list.Add(PeSingleton<EntityMgr>.Instance.Get(tmp[1]));
		}
		int src;
		List<PeEntity> list2;
		if (tmp[2] == 1)
		{
			src = tmp[3];
			list2 = new List<PeEntity>(PeSingleton<EntityMgr>.Instance.All);
			list2 = list2.FindAll(delegate(PeEntity n)
			{
				if (n == null)
				{
					return false;
				}
				return (n.proto == EEntityProto.Monster && n.entityProto.protoId == src) ? true : false;
			});
		}
		else if (tmp[2] == 2)
		{
			src = tmp[3] + 8000;
			list2 = new List<PeEntity>();
			list2.Add(PeSingleton<SceneEntityCreatorArchiver>.Instance.GetEntityByFixedSpId(tmp[3]));
		}
		else if (tmp[2] == 3)
		{
			src = 8888;
			list2 = new List<PeEntity>(PeSingleton<EntityMgr>.Instance.All);
			list2 = list2.FindAll(delegate(PeEntity n)
			{
				if (n == null)
				{
					return false;
				}
				return n.proto == EEntityProto.Monster;
			});
		}
		else
		{
			src = tmp[3];
			if (tmp[3] == 30000)
			{
				list2 = new List<PeEntity>(PeSingleton<EntityMgr>.Instance.All);
				list2 = list2.FindAll(delegate(PeEntity n)
				{
					if (n == null)
					{
						return false;
					}
					return (n.proto == EEntityProto.Npc || n.proto == EEntityProto.RandomNpc) ? true : false;
				});
			}
			else
			{
				list2 = new List<PeEntity>();
				list2.Add(PeSingleton<EntityMgr>.Instance.Get(tmp[3]));
			}
		}
		if (tmp[0] == 0)
		{
			foreach (PeEntity item in list2)
			{
				if (!(item == null) && item.target != null)
				{
					item.target.ClearEnemy();
				}
			}
			{
				foreach (PeEntity item2 in list)
				{
					if (!(item2 == null))
					{
						SkEntity aliveEntity = item2.aliveEntity;
						if (!(aliveEntity == null) && aliveEntity.SpecialHatredData.ContainsKey(src))
						{
							aliveEntity.SpecialHatredData.Remove(src);
						}
					}
				}
				return;
			}
		}
		if (tmp[0] == 3)
		{
			foreach (PeEntity item3 in list2)
			{
				if (item3 == null)
				{
					continue;
				}
				SkEntity aliveEntity2 = item3.aliveEntity;
				if (!(aliveEntity2 == null))
				{
					if (aliveEntity2.SpecialHatredData.ContainsKey(tmp[1]))
					{
						aliveEntity2.SpecialHatredData[tmp[1]] = tmp[0];
					}
					else
					{
						aliveEntity2.SpecialHatredData.Add(tmp[1], tmp[0]);
					}
				}
			}
			if (src > 9000)
			{
				return;
			}
			if (dstEntity_hatredData.ContainsKey(src))
			{
				if (!dstEntity_hatredData[src].ContainsKey(tmp[1]))
				{
					dstEntity_hatredData[src].Add(tmp[1], tmp[0]);
				}
				else
				{
					dstEntity_hatredData[src][tmp[1]] = tmp[0];
				}
				return;
			}
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			dictionary.Add(tmp[1], tmp[0]);
			if (dstEntity_hatredData.Count == 0)
			{
				MonsterEntityCreator.commonCreateEvent += SetSpecialHatred;
			}
			dstEntity_hatredData.Add(src, dictionary);
			return;
		}
		foreach (PeEntity item4 in list)
		{
			if (item4 == null)
			{
				continue;
			}
			SkEntity aliveEntity3 = item4.aliveEntity;
			if (!(aliveEntity3 == null))
			{
				if (aliveEntity3.SpecialHatredData.ContainsKey(src))
				{
					aliveEntity3.SpecialHatredData[src] = tmp[0];
				}
				else
				{
					aliveEntity3.SpecialHatredData.Add(src, tmp[0]);
				}
			}
		}
		if (tmp[0] != 1)
		{
			return;
		}
		foreach (PeEntity item5 in list2)
		{
			if (!(item5 == null) && item5.target != null)
			{
				item5.target.ClearEnemy();
			}
		}
	}

	public static void HarmAdd(List<int> tmp)
	{
		if (tmp.Count != 3)
		{
			return;
		}
		int num = 0;
		List<PeEntity> list;
		switch (tmp[1])
		{
		case 1:
			list = new List<PeEntity>(PeSingleton<EntityMgr>.Instance.All);
			list = list.FindAll(delegate(PeEntity e)
			{
				if (e == null)
				{
					return false;
				}
				return (e.proto == EEntityProto.Monster && e.entityProto.protoId == tmp[2]) ? true : false;
			});
			num = tmp[2];
			break;
		case 2:
		{
			list = new List<PeEntity>();
			PeEntity entityByFixedSpId = PeSingleton<SceneEntityCreatorArchiver>.Instance.GetEntityByFixedSpId(tmp[2]);
			if (entityByFixedSpId != null)
			{
				list.Add(PeSingleton<SceneEntityCreatorArchiver>.Instance.GetEntityByFixedSpId(tmp[2]));
			}
			num = tmp[2] + 8000;
			break;
		}
		case 3:
			if (tmp[2] == 20000)
			{
				list = new List<PeEntity>();
				list.Add(PeSingleton<PeCreature>.Instance.mainPlayer);
			}
			else if (tmp[2] == 30000)
			{
				list = new List<PeEntity>(PeSingleton<EntityMgr>.Instance.All);
				list = list.FindAll(delegate(PeEntity e)
				{
					if (e == null)
					{
						return false;
					}
					return (e.proto == EEntityProto.Npc || e.proto == EEntityProto.RandomNpc) ? true : false;
				});
				list.Add(PeSingleton<PeCreature>.Instance.mainPlayer);
			}
			else
			{
				list = new List<PeEntity>();
				list.Add(PeSingleton<EntityMgr>.Instance.Get(tmp[2]));
			}
			break;
		default:
			list = new List<PeEntity>();
			break;
		}
		if (tmp[0] == 1)
		{
			foreach (PeEntity item in list)
			{
				SkEntity aliveEntity = item.aliveEntity;
				if (aliveEntity != null)
				{
					SkEntity.MountBuff(aliveEntity, 30200102, new List<int>(), new List<float>());
				}
			}
			if (!entityHarmRecord.Contains(num) && num != 0)
			{
				if (entityHarmRecord.Count == 0)
				{
					MonsterEntityCreator.commonCreateEvent += SetHarm;
				}
				entityHarmRecord.Add(num);
			}
		}
		else
		{
			if (tmp[0] != 0)
			{
				return;
			}
			foreach (PeEntity item2 in list)
			{
				SkEntity aliveEntity2 = item2.aliveEntity;
				if (aliveEntity2 != null)
				{
					aliveEntity2.CancelBuffById(30200102);
				}
			}
			if (entityHarmRecord.Contains(num) && num != 0)
			{
				entityHarmRecord.Remove(num);
				if (entityHarmRecord.Count == 0)
				{
					MonsterEntityCreator.commonCreateEvent -= SetHarm;
				}
			}
		}
	}

	public static void IsHaveEnnemy(PeEntity me, ref List<PeEntity> result)
	{
		if (me == null || me.aliveEntity == null || me.aliveEntity.SpecialHatredData.Count == 0)
		{
			return;
		}
		foreach (KeyValuePair<int, int> specialHatredDatum in me.aliveEntity.SpecialHatredData)
		{
			if (specialHatredDatum.Value != 3)
			{
				continue;
			}
			if (specialHatredDatum.Key == 25001 && ServantLeaderCmpt.Instance.GetServant(0) != null)
			{
				result.Add(ServantLeaderCmpt.Instance.GetServant(0).Entity);
			}
			else if (specialHatredDatum.Key == 25002 && ServantLeaderCmpt.Instance.GetServant(1) != null)
			{
				result.Add(ServantLeaderCmpt.Instance.GetServant(1).Entity);
			}
			else if (specialHatredDatum.Key == 25010)
			{
				if (ServantLeaderCmpt.Instance.GetServant(0) != null)
				{
					result.Add(ServantLeaderCmpt.Instance.GetServant(0).Entity);
				}
				if (ServantLeaderCmpt.Instance.GetServant(1) != null)
				{
					result.Add(ServantLeaderCmpt.Instance.GetServant(1).Entity);
				}
			}
			else if (specialHatredDatum.Key == 20000)
			{
				result.Add(PeSingleton<PeCreature>.Instance.mainPlayer);
			}
			else if (specialHatredDatum.Key / 9000 == 1)
			{
				PeEntity peEntity = PeSingleton<EntityMgr>.Instance.Get(specialHatredDatum.Key);
				if (peEntity != null)
				{
					result.Add(peEntity);
				}
			}
			else if (specialHatredDatum.Key / 8000 == 1)
			{
				PeEntity entityByFixedSpId = PeSingleton<SceneEntityCreatorArchiver>.Instance.GetEntityByFixedSpId(specialHatredDatum.Key % 8000);
				if (entityByFixedSpId != null)
				{
					result.Add(entityByFixedSpId);
				}
			}
		}
	}

	public static bool IsHaveSpecialHatred(PeEntity me, PeEntity ennemy, out int ignoreOrHighHatred)
	{
		ignoreOrHighHatred = 0;
		if (ennemy == null || ennemy.aliveEntity == null)
		{
			return false;
		}
		if (ennemy.aliveEntity.SpecialHatredData.Count == 0)
		{
			return false;
		}
		List<int> list = new List<int>(ennemy.aliveEntity.SpecialHatredData.Keys);
		if (me.entityProto.proto == EEntityProto.Monster)
		{
			if (list.Contains(me.entityProto.protoId))
			{
				ignoreOrHighHatred = ennemy.aliveEntity.SpecialHatredData[me.entityProto.protoId];
				return true;
			}
			if (list.Contains(8888))
			{
				ignoreOrHighHatred = ennemy.aliveEntity.SpecialHatredData[8888];
				return true;
			}
			foreach (int item in list)
			{
				if (item / 1000 != 8 || item == 8888 || ennemy.aliveEntity.SpecialHatredData[item] == 3)
				{
					continue;
				}
				PeEntity entityByFixedSpId = PeSingleton<SceneEntityCreatorArchiver>.Instance.GetEntityByFixedSpId(item % 8000);
				if (!(entityByFixedSpId != null))
				{
					continue;
				}
				if (entityByFixedSpId is EntityGrp)
				{
					EntityGrp entityGrp = entityByFixedSpId as EntityGrp;
					foreach (ISceneObjAgent memberAgent in entityGrp.memberAgents)
					{
						if (!(memberAgent is SceneEntityPosAgent) || !((memberAgent as SceneEntityPosAgent).entity == me))
						{
							continue;
						}
						ignoreOrHighHatred = ennemy.aliveEntity.SpecialHatredData[item];
						return true;
					}
				}
				else if (entityByFixedSpId == me)
				{
					ignoreOrHighHatred = ennemy.aliveEntity.SpecialHatredData[item];
					return true;
				}
			}
		}
		else if (me.entityProto.proto == EEntityProto.Npc || me.entityProto.proto == EEntityProto.RandomNpc)
		{
			if (list.Contains(me.Id))
			{
				ignoreOrHighHatred = ennemy.aliveEntity.SpecialHatredData[me.Id];
				return true;
			}
			if (list.Contains(30000))
			{
				ignoreOrHighHatred = ennemy.aliveEntity.SpecialHatredData[30000];
				return true;
			}
			for (int i = 0; i < ServantLeaderCmpt.mMaxFollower; i++)
			{
				NpcCmpt servant = ServantLeaderCmpt.Instance.GetServant(i);
				if (!(servant != null))
				{
					continue;
				}
				PeEntity entity = servant.Entity;
				if (entity != null && entity == me)
				{
					if (list.Contains(25001 + i))
					{
						ignoreOrHighHatred = ennemy.aliveEntity.SpecialHatredData[25001 + i];
						return true;
					}
					if (list.Contains(25010))
					{
						ignoreOrHighHatred = ennemy.aliveEntity.SpecialHatredData[25010];
						return true;
					}
				}
			}
		}
		ignoreOrHighHatred = 0;
		return false;
	}

	private static void SetHarm(PeEntity entity)
	{
		SkEntity aliveEntity = entity.aliveEntity;
		if (aliveEntity == null)
		{
			return;
		}
		int protoId = entity.entityProto.protoId;
		int num = 0;
		foreach (int key in AISpawnPoint.s_spawnPointData.Keys)
		{
			if (PeSingleton<SceneEntityCreatorArchiver>.Instance.GetEntityByFixedSpId(key) == entity)
			{
				num = key;
				break;
			}
		}
		if (entityHarmRecord.Contains(protoId))
		{
			SkEntity.MountBuff(aliveEntity, 30200102, new List<int>(), new List<float>());
		}
		if (num != 0 && entityHarmRecord.Contains(8000 + num))
		{
			SkEntity.MountBuff(aliveEntity, 30200102, new List<int>(), new List<float>());
		}
	}

	private static void SetSpecialHatred(PeEntity entity)
	{
		if (dstEntity_hatredData.Count == 0)
		{
			return;
		}
		SkEntity aliveEntity = entity.aliveEntity;
		if (aliveEntity == null)
		{
			return;
		}
		int protoId = entity.entityProto.protoId;
		int num = 0;
		foreach (int key in AISpawnPoint.s_spawnPointData.Keys)
		{
			if (PeSingleton<SceneEntityCreatorArchiver>.Instance.GetEntityByFixedSpId(key) == entity)
			{
				num = key;
			}
		}
		aliveEntity.SpecialHatredData = new Dictionary<int, int>();
		if (dstEntity_hatredData.ContainsKey(protoId))
		{
			foreach (KeyValuePair<int, int> item in dstEntity_hatredData[protoId])
			{
				aliveEntity.SpecialHatredData.Add(item.Key, item.Value);
			}
		}
		if (num == 0 || !dstEntity_hatredData.ContainsKey(8000 + num))
		{
			return;
		}
		foreach (KeyValuePair<int, int> item2 in dstEntity_hatredData[8000 + num])
		{
			aliveEntity.SpecialHatredData.Add(item2.Key, item2.Value);
		}
	}
}
