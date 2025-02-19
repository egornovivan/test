using System;
using System.Collections;
using Pathea;
using SkillSystem;
using UnityEngine;

public class MonsterEntityCreator
{
	public class AgentInfo : SceneEntityPosAgent.AgentInfo
	{
		public new static AgentInfo s_defAgentInfo = new AgentInfo(-1f);

		public float _bsRate = -1f;

		public EntityGrp _grp;

		public EntityMonsterBeacon _bcn;

		public int _colorType = -1;

		public int _playerId = -1;

		public int _buffId;

		public AgentInfo(float bsRate = -1f)
		{
			_bsRate = bsRate;
		}

		public AgentInfo(EntityGrp grp, int colorType = -1, int playerId = -1, int buffId = 0)
		{
			_grp = grp;
			_colorType = colorType;
			_playerId = playerId;
			_buffId = buffId;
		}

		public AgentInfo(EntityMonsterBeacon bcn)
		{
			_bcn = bcn;
			_colorType = _bcn.CampColor;
		}

		public AgentInfo(int colorType, int playerId, int buffId = 0)
		{
			_colorType = colorType;
			_playerId = playerId;
			_buffId = buffId;
		}

		public override void OnSuceededToCreate(SceneEntityPosAgent agent)
		{
			LodCmpt lodCmpt = agent.entity.lodCmpt;
			if (lodCmpt != null)
			{
				lodCmpt.onDestruct = (Action<PeEntity>)Delegate.Combine(lodCmpt.onDestruct, (Action<PeEntity>)delegate
				{
					agent.DestroyEntity();
				});
			}
			if (_bsRate >= 0f && agent.entity.aliveEntity != null)
			{
				agent.entity.aliveEntity.deathEvent += delegate
				{
					SceneMan.self.StartCoroutine(DelayBossReborn(agent));
				};
			}
			if (_bcn != null)
			{
				SceneMan.RemoveSceneObj(agent);
			}
		}

		private IEnumerator DelayBossReborn(SceneEntityPosAgent agent)
		{
			SceneMan.RemoveSceneObj(agent);
			yield return new WaitForSeconds(240f);
			SceneMan.AddSceneObj(agent);
		}
	}

	public static event Action<PeEntity> commonCreateEvent;

	public static event Action<SkEntity, SkEntity> commonDeathEvent;

	public static void Init()
	{
		MonsterEntityCreator.commonCreateEvent = null;
		MonsterEntityCreator.commonDeathEvent = null;
		MonsterEntityCreator.commonDeathEvent = (Action<SkEntity, SkEntity>)Delegate.Combine(MonsterEntityCreator.commonDeathEvent, new Action<SkEntity, SkEntity>(RepProcessor.OnMonsterDeath));
	}

	public static SceneEntityPosAgent CreateAgent(Vector3 pos, int protoId, Vector3 scl, Quaternion rot)
	{
		SceneEntityPosAgent sceneEntityPosAgent = new SceneEntityPosAgent(pos, scl, rot, EntityType.EntityType_Monster, protoId);
		sceneEntityPosAgent.spInfo = AgentInfo.s_defAgentInfo;
		return sceneEntityPosAgent;
	}

	public static SceneEntityPosAgent CreateAgent(Vector3 pos, int protoId = -1)
	{
		SceneEntityPosAgent sceneEntityPosAgent = new SceneEntityPosAgent(pos, Vector3.one, Quaternion.identity, EntityType.EntityType_Monster, protoId);
		sceneEntityPosAgent.spInfo = AgentInfo.s_defAgentInfo;
		return sceneEntityPosAgent;
	}

	public static SceneEntityPosAgent CreateAdAgent(Vector3 pos, int protoId, int colorType, int playerId, int buffId = 0, bool bride = true)
	{
		SceneEntityPosAgent sceneEntityPosAgent = new SceneEntityPosAgent(pos, Vector3.one, Quaternion.identity, EntityType.EntityType_Monster, protoId);
		sceneEntityPosAgent.spInfo = new AgentInfo(colorType, playerId, buffId);
		sceneEntityPosAgent.canRide = bride;
		return sceneEntityPosAgent;
	}

	public static PeEntity CreateMonster(int protoId, Vector3 pos)
	{
		SceneEntityPosAgent sceneEntityPosAgent = CreateAgent(pos, protoId);
		CreateMonster(sceneEntityPosAgent);
		return sceneEntityPosAgent.entity;
	}

	public static PeEntity CreateAdMonster(int protoId, Vector3 pos, int colorType, int playerId, int buffId = 0, bool bride = true)
	{
		SceneEntityPosAgent sceneEntityPosAgent = CreateAdAgent(pos, protoId, colorType, playerId, buffId, bride);
		CreateMonster(sceneEntityPosAgent);
		return sceneEntityPosAgent.entity;
	}

	public static PeEntity CreateMonster(int protoId, Vector3 pos, Vector3 scl, Quaternion rot)
	{
		SceneEntityPosAgent sceneEntityPosAgent = CreateAgent(pos, protoId, scl, rot);
		CreateMonster(sceneEntityPosAgent);
		return sceneEntityPosAgent.entity;
	}

	public static PeEntity CreateDungeonMonster(int protoId, Vector3 pos, int dungeonId, int buffId = 0, int colorType = -1, int playerId = -1)
	{
		if (PeGameMgr.IsSingle)
		{
			PeEntity peEntity = CreateAdMonster(protoId, pos, colorType, playerId, buffId, bride: false);
			if (peEntity != null)
			{
				peEntity.SetAttribute(AttribType.CampID, 26f);
				peEntity.SetAttribute(AttribType.DamageID, 26f);
			}
			else
			{
				Debug.LogError("createDungeonMonsterFailed!");
			}
			return peEntity;
		}
		SceneEntityPosAgent sceneEntityPosAgent = CreateAdAgent(pos, protoId, colorType, playerId, buffId, bride: false);
		sceneEntityPosAgent.entity = null;
		float exScale = -1f;
		if (!ParseAgentInfo(sceneEntityPosAgent, out var grp, out var bcn, ref exScale, ref colorType, ref playerId, ref buffId))
		{
			return sceneEntityPosAgent.entity;
		}
		NetworkManager.WaitCoroutine(PlayerNetwork.RequestCreateAi(sceneEntityPosAgent.protoId, sceneEntityPosAgent.Pos, (!(null == grp)) ? grp.Id : (-1), (!(null == bcn)) ? bcn.Id : (-1), dungeonId, colorType, playerId, buffId));
		return sceneEntityPosAgent.entity;
	}

	public static void AttachMonsterDeathEvent(PeEntity entity)
	{
		SkAliveEntity cmpt = entity.GetCmpt<SkAliveEntity>();
		if (!(cmpt != null))
		{
			return;
		}
		cmpt.deathEvent += cmpt.OnDeathProcessBuff;
		cmpt.deathEvent += delegate(SkEntity a, SkEntity b)
		{
			if (MonsterEntityCreator.commonDeathEvent != null)
			{
				MonsterEntityCreator.commonDeathEvent(a, b);
			}
		};
	}

	public static void CreateMonster(SceneEntityPosAgent agent)
	{
		agent.entity = null;
		float exScale = -1f;
		int colorType = -1;
		int playerId = -1;
		int buffId = 0;
		if (!ParseAgentInfo(agent, out var grp, out var bcn, ref exScale, ref colorType, ref playerId, ref buffId))
		{
			return;
		}
		if (NetworkInterface.IsClient)
		{
			NetworkManager.WaitCoroutine(PlayerNetwork.RequestCreateAi(agent.protoId, agent.Pos, (!(null == grp)) ? grp.Id : (-1), (!(null == bcn)) ? bcn.Id : (-1), -1, colorType, playerId, buffId));
			return;
		}
		if ((agent.protoId & 0x40000000) != 0)
		{
			agent.entity = EntityGrp.CreateMonsterGroup(agent.protoId & -1073741825, agent.Pos, colorType, playerId, -1, buffId);
			return;
		}
		Vector3 vector = new Vector3(agent.Pos.x, agent.Pos.y, agent.Pos.z);
		MonsterProtoDb.Item item = MonsterProtoDb.Get(agent.protoId);
		if (item != null && bcn == null && !agent.FixPos)
		{
			float hOffset = item.hOffset;
			if (hOffset < 0f)
			{
				float num = VFVoxelWater.self.UpToWaterSurface(agent.Pos.x, agent.Pos.y, agent.Pos.z);
				if (num <= 0f)
				{
					Debug.LogError("[SceneEntityCreator]Failed to create water monster becasue not in water:" + agent.protoId);
					return;
				}
				vector.y += num + hOffset;
			}
			else if (hOffset > 2f)
			{
				float num2 = VFVoxelWater.self.UpToWaterSurface(agent.Pos.x, agent.Pos.y, agent.Pos.z);
				if (num2 <= 0f)
				{
					float num3 = 128f;
					if (Physics.Raycast(vector + num3 * Vector3.up, Vector3.down, out var hitInfo, num3, SceneMan.DependenceLayer))
					{
						vector.y = hitInfo.point.y;
					}
					vector.y += hOffset;
				}
				else
				{
					vector.y += num2 + hOffset;
				}
			}
			else
			{
				float num4 = VFVoxelWater.self.UpToWaterSurface(agent.Pos.x, agent.Pos.y, agent.Pos.z);
				if (!(num4 <= 0f))
				{
					Debug.LogError("[SceneEntityCreator]Failed to create land monster becasue in water:" + agent.protoId);
					return;
				}
				vector.y += hOffset;
			}
		}
		int id = PeSingleton<WorldInfoMgr>.Instance.FetchNonRecordAutoId();
		agent.entity = PeSingleton<PeEntityCreator>.Instance.CreateMonster(id, agent.protoId, vector, agent.Rot, agent.Scl, exScale, colorType, buffId);
		if (null == agent.entity)
		{
			Debug.LogError("[SceneEntityCreator]Failed to create monster:" + agent.protoId);
			return;
		}
		agent.entity.monster.Ride(agent.canRide);
		if (playerId >= 0)
		{
			agent.entity.SetAttribute(AttribType.DefaultPlayerID, playerId);
		}
		if (MonsterEntityCreator.commonCreateEvent != null)
		{
			MonsterEntityCreator.commonCreateEvent(agent.entity);
		}
		if (grp != null)
		{
			grp.OnMemberCreated(agent.entity);
		}
		if (bcn != null)
		{
			bcn.OnMonsterCreated(agent.entity);
		}
	}

	private static bool ParseAgentInfo(SceneEntityPosAgent agent, out EntityGrp grp, out EntityMonsterBeacon bcn, ref float exScale, ref int colorType, ref int playerId, ref int buffId)
	{
		AgentInfo agentInfo = agent.spInfo as AgentInfo;
		grp = agentInfo?._grp;
		bcn = agentInfo?._bcn;
		colorType = agentInfo?._colorType ?? (-1);
		playerId = agentInfo?._playerId ?? (-1);
		buffId = agentInfo?._buffId ?? 0;
		if (bcn != null)
		{
			agent.protoId = GetMonsterProtoIDForBeacon(agent.protoId, agent.Pos, ref exScale);
			if ((agent.protoId & 0xC000000) != 0)
			{
				bcn.AddAirborneReq(agent);
				return false;
			}
		}
		if (0 > agent.protoId)
		{
			agent.protoId = ((agentInfo != null && !(agentInfo._bsRate < 0f)) ? GetBossMonsterProtoID(agent.Pos, agentInfo._bsRate, ref exScale) : GetMonsterProtoID(agent.Pos, ref exScale));
		}
		return agent.protoId >= 0;
	}

	private static int GetMonsterProtoID(Vector3 pos, ref float fScale)
	{
		int result = 0;
		int num = 0;
		num = (int)AiUtil.GetPointType(pos);
		if (PeGameMgr.IsStory)
		{
			int aiSpawnMapId = PeSingleton<PeMappingMgr>.Instance.GetAiSpawnMapId(new Vector2(pos.x, pos.z));
			result = AISpeciesData.GetRandomAI(AISpawnDataStory.GetAiSpawnData(aiSpawnMapId, num));
		}
		else if (PeGameMgr.IsAdventure)
		{
			int mapID = AiUtil.GetMapID(pos);
			int areaID = AiUtil.GetAreaID(pos);
			result = AISpawnDataAdvSingle.GetPathIDScale(mapID, areaID, num, ref fScale);
		}
		return result;
	}

	private static int GetBossMonsterProtoID(Vector3 pos, float rndVal, ref float fScale)
	{
		int num = 0;
		int num2 = 0;
		num2 = (int)AiUtil.GetPointType(pos);
		int mapID = AiUtil.GetMapID(pos);
		int areaID = AiUtil.GetAreaID(pos);
		return AISpawnDataAdvSingle.GetBossPathIDScale(mapID, areaID, num2, rndVal, ref fScale);
	}

	private static int GetMonsterProtoIDForBeacon(int bcnProtoId, Vector3 pos, ref float fScale)
	{
		if (!EntityMonsterBeacon.IsBcnMonsterProtoId(bcnProtoId))
		{
			return bcnProtoId;
		}
		EntityMonsterBeacon.DecodeBcnMonsterProtoId(bcnProtoId, out var spType, out var dif, out var spawnType);
		int terType = 0;
		int num = -1;
		AISpawnTDWavesData.TDMonsterData tDMonsterData = null;
		if (spType < 500)
		{
			terType = AiUtil.GetPointType(pos) switch
			{
				PointType.PT_Water => 1, 
				PointType.PT_Slope => 2, 
				PointType.PT_Cave => 3, 
				_ => 0, 
			};
			num = ((!PeGameMgr.IsStory) ? AiUtil.GetMapID(pos) : PeSingleton<PeMappingMgr>.Instance.GetAiSpawnMapId(new Vector2(pos.x, pos.z)));
			if (PeGameMgr.IsAdventure && num == 5)
			{
				num = 2;
			}
		}
		int opPlayerId = -1;
		if (MainPlayerCmpt.gMainPlayer != null)
		{
			SkAliveEntity aliveEntity = MainPlayerCmpt.gMainPlayer.Entity.aliveEntity;
			if (aliveEntity != null)
			{
				opPlayerId = (int)aliveEntity.GetAttribute(AttribType.DefaultPlayerID);
			}
		}
		tDMonsterData = AISpawnTDWavesData.GetMonsterProtoId(!PeGameMgr.IsStory, spType, dif, spawnType, num, terType, opPlayerId);
		if (tDMonsterData != null)
		{
			if (tDMonsterData.IsAirbornePuja)
			{
				tDMonsterData.ProtoId |= 134217728;
			}
			if (tDMonsterData.IsAirbornePaja)
			{
				tDMonsterData.ProtoId |= 67108864;
			}
			if (tDMonsterData.IsGrp)
			{
				tDMonsterData.ProtoId |= 1073741824;
			}
			return tDMonsterData.ProtoId;
		}
		return -1;
	}
}
