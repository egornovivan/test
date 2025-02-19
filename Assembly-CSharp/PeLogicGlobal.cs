using System.Collections;
using Pathea;
using Pathea.PeEntityExt;
using SkillSystem;
using UnityEngine;

public class PeLogicGlobal : Singleton<PeLogicGlobal>
{
	private IEnumerator DestroyEnumerator(SkEntity entity, float delayTime, float fadeoutTime = 3f)
	{
		yield return new WaitForSeconds(delayTime);
		if (entity != null)
		{
			ViewCmpt view = entity.GetComponent<ViewCmpt>();
			if (view != null && view is BiologyViewCmpt)
			{
				(view as BiologyViewCmpt).Fadeout(fadeoutTime);
				yield return new WaitForSeconds(fadeoutTime);
			}
			if (entity != null)
			{
				Object.Destroy(entity.gameObject);
				Singleton<PeEventGlobal>.Instance.DestroyEvent.Invoke(entity);
			}
		}
	}

	private IEnumerator NpcRevive(SkEntity entity, float delayTime)
	{
		if (PeGameMgr.IsMulti)
		{
			yield break;
		}
		PeEntity peentity = entity.GetComponent<PeEntity>();
		if (peentity == null)
		{
			yield break;
		}
		EntityInfoCmpt InfoCmpt = peentity.enityInfoCmpt;
		InfoCmpt.SetDelaytime(Time.time, delayTime);
		PESkEntity skentity = peentity.peSkEntity;
		yield return new WaitForSeconds(delayTime);
		if (!(entity != null) || !(skentity != null) || !skentity.isDead)
		{
			yield break;
		}
		MotionMgrCmpt motion = entity.GetComponent<MotionMgrCmpt>();
		if (!(motion != null))
		{
			yield break;
		}
		while (true)
		{
			if (null == peentity || null == motion || peentity.NpcCmpt.ReviveTime < 0)
			{
				yield break;
			}
			PEActionParamB param = PEActionParamB.param;
			param.b = false;
			if (motion.DoAction(PEActionType.Revive, param))
			{
				break;
			}
			yield return new WaitForSeconds(1f);
		}
		entity.SetAttribute(1, entity.GetAttribute(0) * 0.8f);
	}

	private IEnumerator ServantRevive(PeEntity entity, float delayTime)
	{
		yield return new WaitForSeconds(delayTime);
		if (!PeGameMgr.IsMulti)
		{
			if (!(entity != null) || !(entity.UseItem != null) || !entity.UseItem.ReviveServent(usePlayer: false) || !(entity.motionMgr != null))
			{
				yield break;
			}
			while (!(null == entity) && !(null == entity.motionMgr))
			{
				PEActionParamB param = PEActionParamB.param;
				param.b = false;
				if (null == entity || entity.motionMgr.DoAction(PEActionType.Revive, param))
				{
					break;
				}
				yield return new WaitForSeconds(1f);
			}
		}
		else
		{
			if (!(null != entity) || !(null != entity.netCmpt))
			{
				yield break;
			}
			AiAdNpcNetwork npc = (AiAdNpcNetwork)entity.netCmpt.network;
			if (null != npc && npc.LordPlayerId == PlayerNetwork.mainPlayerId)
			{
				Vector3 pos = entity.position;
				if (null != entity.NpcCmpt && !entity.NpcCmpt.CanRecive)
				{
					pos = PlayerNetwork.mainPlayer.PlayerPos + Vector3.one;
				}
				PlayerNetwork.RequestServantAutoRevive(entity.Id, pos);
			}
		}
	}

	private void OnEntityDeath(SkEntity entity, SkEntity caster)
	{
		CommonCmpt component = entity.GetComponent<CommonCmpt>();
		if (!(component != null))
		{
			return;
		}
		if (component.entityProto.proto == EEntityProto.Doodad)
		{
			DestroyEntity(entity, 30f);
		}
		if (component.entityProto.proto == EEntityProto.Monster)
		{
			MonsterHandbookData.AddMhByKilledMonsterID(component.entityProto.protoId);
			if (component.GetComponent<TowerCmpt>() == null)
			{
				float num = 10f;
				PeEntity component2 = entity.GetComponent<PeEntity>();
				if (component2 != null && StroyManager.Instance != null && StroyManager.Instance.m_RecordKillMons.Count != 0)
				{
					foreach (KillMons value in StroyManager.Instance.m_RecordKillMons.Values)
					{
						if (value.type == KillMons.Type.fixedId && PeSingleton<SceneEntityCreatorArchiver>.Instance.GetEntityByFixedSpId(value.monId) == component2)
						{
							num = ((value.reviveTime != 0) ? ((float)value.reviveTime) : num);
							break;
						}
						if (value.type == KillMons.Type.protoTypeId && Vector3.Distance(component2.position, value.center) <= value.radius && (value.monId == -999 || component.entityProto.protoId == value.monId))
						{
							num = ((value.reviveTime != 0) ? ((float)value.reviveTime) : num);
							break;
						}
					}
				}
				DestroyEntity(entity, num);
			}
		}
		NpcCmpt component3 = entity.GetComponent<NpcCmpt>();
		if (component.entityProto.proto == EEntityProto.Npc)
		{
			if (GameUI.Instance != null && GameUI.Instance.mNpcWnd.IsOpen() && GameUI.Instance.mNpcWnd.m_CurSelNpc.commonCmpt == component)
			{
				GameUI.Instance.mNpcWnd.Hide();
			}
			if (component3 != null && component3.Type != ENpcType.Follower && component3.ReviveTime > 0)
			{
				ReviveEntity(entity, 10f);
			}
			if (component3.ReviveTime <= 0)
			{
				PeEntity component4 = component3.GetComponent<PeEntity>();
				if (MissionManager.Instance != null && MissionManager.Instance.m_PlayerMission != null)
				{
					MissionManager.Instance.m_PlayerMission.SetMissionState(component4, NpcMissionState.Max);
				}
				if (component4.GetUserData() is NpcMissionData npcMissionData)
				{
					npcMissionData.m_MissionList.Clear();
				}
			}
		}
		else
		{
			if (component.entityProto.proto != EEntityProto.RandomNpc || !(component3 != null) || component3.IsServant || component3.ReviveTime <= 0)
			{
				return;
			}
			if (PeGameMgr.IsMultiStory)
			{
				if (entity._net is AiAdNpcNetwork)
				{
					int externId = ((AiAdNpcNetwork)entity._net).ExternId;
					RandomNpcDb.Item item = RandomNpcDb.Get(externId);
					if (item != null && item.reviveTime != -1)
					{
						ReviveEntity(entity, item.reviveTime);
					}
				}
			}
			else
			{
				ReviveEntity(entity, component3.ReviveTime);
			}
		}
	}

	private void OnEntityPickup(SkEntity entity)
	{
		CommonCmpt component = entity.GetComponent<CommonCmpt>();
		if (!(component != null))
		{
			return;
		}
		if (component.entityProto.proto == EEntityProto.RandomNpc || component.entityProto.proto == EEntityProto.Npc)
		{
			if (!ServantLeaderCmpt.Instance.ContainsServant(component.GetComponent<NpcCmpt>()))
			{
				DestroyEntity(entity);
			}
			return;
		}
		DestroyEntity(entity);
		PeEntity component2 = entity.GetComponent<PeEntity>();
		if (component2 != null)
		{
			LootItemDropPeEntity.RemovePeEntity(component2);
		}
	}

	private void OnEntityRevive(SkEntity entity)
	{
		CommonCmpt component = entity.GetComponent<CommonCmpt>();
		if (component != null && (component.entityProto.proto == EEntityProto.Npc || component.entityProto.proto == EEntityProto.RandomNpc))
		{
			ReviveEntity(entity);
		}
	}

	private void OnEntityDestroy(SkEntity entity)
	{
		PeEntity component = entity.GetComponent<PeEntity>();
		if (component != null)
		{
			LootItemDropPeEntity.RemovePeEntity(component);
			PeSingleton<PeCreature>.Instance.Destory(component.Id);
		}
	}

	public void Init()
	{
		Singleton<PeEventGlobal>.Instance.DeathEvent.AddListener(OnEntityDeath);
		Singleton<PeEventGlobal>.Instance.PickupEvent.AddListener(OnEntityPickup);
		Singleton<PeEventGlobal>.Instance.ReviveEvent.AddListener(OnEntityRevive);
		Singleton<PeEventGlobal>.Instance.DestroyEvent.AddListener(OnEntityDestroy);
	}

	public void DestroyEntity(SkEntity entity, float delayTime = 0f, float fadeoutTime = 3f)
	{
		if (delayTime >= 0f)
		{
			StartCoroutine(DestroyEnumerator(entity, delayTime, fadeoutTime));
		}
	}

	public void ReviveEntity(SkEntity entity, float delayTime = 0f)
	{
		StartCoroutine(NpcRevive(entity, delayTime));
	}

	public void ServantReviveAtuo(PeEntity entity, float delayTime = 0f)
	{
		StartCoroutine(ServantRevive(entity, delayTime));
	}
}
