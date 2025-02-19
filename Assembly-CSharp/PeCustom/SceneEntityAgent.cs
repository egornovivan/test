using System;
using System.Collections;
using Pathea;
using Pathea.PeEntityExtTrans;
using SkillSystem;
using UnityEngine;

namespace PeCustom;

public class SceneEntityAgent : PeCustomScene.SceneElement, ISceneObjAgent
{
	private const int MaxTimes = 100;

	private const float OneWait = 0.05f;

	private bool _firstConstruct = true;

	private bool mIsProcessing;

	private SpawnPoint mPoint;

	private MonsterSpawnArea mArea;

	private bool mIsSave;

	private EntityType mType;

	public int Id { get; set; }

	public int ScenarioId { get; set; }

	public GameObject Go => null;

	public Vector3 Pos => spawnPoint.spawnPos;

	public IBoundInScene Bound => null;

	public bool NeedToActivate => true;

	public bool TstYOnActivate => true;

	public int protoId => mPoint.Prototype;

	public Quaternion Rot => mPoint.Rotation;

	public Vector3 Scale => mPoint.Scale;

	public PeEntity entity { get; set; }

	public EntityGrp entityGp { get; set; }

	public MonsterSpawnPoint[] groupPoints { get; private set; }

	public SpawnPoint spawnPoint => mPoint;

	public MonsterSpawnPoint mstPoint => mPoint as MonsterSpawnPoint;

	public MonsterSpawnArea spawnArea => mArea;

	public SceneEntityAgent(MonsterSpawnPoint _point, bool is_saved = false, MonsterSpawnArea _area = null, MonsterSpawnPoint[] _groupPoints = null)
	{
		mPoint = _point;
		mIsSave = is_saved;
		mArea = _area;
		groupPoints = _groupPoints;
		mType = EntityType.EntityType_Monster;
	}

	public SceneEntityAgent(NPCSpawnPoint _point)
	{
		mPoint = _point;
		mArea = null;
		groupPoints = null;
		mType = EntityType.EntityType_Npc;
	}

	public void OnActivate()
	{
		if (entity == null && !mIsProcessing)
		{
			MonsterSpawnPoint monsterSpawnPoint = mstPoint;
			if ((monsterSpawnPoint == null || !monsterSpawnPoint.isDead) && (spawnPoint.Enable || spawnPoint.EntityID != -1))
			{
				SceneMan.self.StartCoroutine(CreateEntity());
			}
		}
		else if (entity != null && !mIsProcessing && entity.peTrans != null)
		{
			Vector3 outPutPos = entity.peTrans.position;
			if (SceneAgentsContoller.CheckPos(out outPutPos, outPutPos, spawnPoint, spawnArea))
			{
				entity.ExtSetPos(outPutPos);
			}
			else
			{
				Debug.LogWarning("The Entity id [" + entity.Id + "] position is wrong");
			}
		}
	}

	public void OnConstruct()
	{
		if (_firstConstruct)
		{
			_firstConstruct = false;
			Vector3 outPutPos = spawnPoint.spawnPos;
			if (SceneAgentsContoller.CheckPos(out outPutPos, outPutPos, spawnPoint, spawnArea))
			{
				spawnPoint.spawnPos = outPutPos;
			}
		}
	}

	public void OnDeactivate()
	{
	}

	public void OnDestruct()
	{
	}

	private IEnumerator CreateEntity()
	{
		mIsProcessing = true;
		int n = 0;
		while (n++ < 100)
		{
			switch (mType)
			{
			case EntityType.EntityType_Npc:
				base.scene.Notify(ESceneNoification.CreateNpc, this);
				break;
			case EntityType.EntityType_Monster:
				base.scene.Notify(ESceneNoification.CreateMonster, this, mIsSave);
				break;
			}
			if (entity != null)
			{
				entity.scenarioId = ScenarioId;
				SkAliveEntity skAlive = entity.GetCmpt<SkAliveEntity>();
				if (skAlive != null)
				{
					skAlive.deathEvent += OnEntityDeath;
				}
				LodCmpt entityLodCmpt = entity.lodCmpt;
				if (entityLodCmpt != null)
				{
					entityLodCmpt.onDestruct = (Action<PeEntity>)Delegate.Combine(entityLodCmpt.onDestruct, (Action<PeEntity>)delegate
					{
						DestroyEntity();
					});
				}
				break;
			}
			yield return new WaitForSeconds(0.05f);
		}
		mIsProcessing = false;
	}

	public bool ForceCreateEntity()
	{
		if (spawnPoint.isDead)
		{
			return false;
		}
		switch (mType)
		{
		case EntityType.EntityType_Npc:
			base.scene.Notify(ESceneNoification.CreateNpc, this, false);
			break;
		case EntityType.EntityType_Monster:
			base.scene.Notify(ESceneNoification.CreateMonster, this, mIsSave, false);
			break;
		}
		if (entity != null)
		{
			SkAliveEntity cmpt = entity.GetCmpt<SkAliveEntity>();
			if (cmpt != null)
			{
				cmpt.deathEvent += OnEntityDeath;
			}
			LodCmpt lodCmpt = entity.lodCmpt;
			if (lodCmpt != null)
			{
				lodCmpt.onDestruct = (Action<PeEntity>)Delegate.Combine(lodCmpt.onDestruct, (Action<PeEntity>)delegate
				{
					DestroyEntity();
				});
			}
			return true;
		}
		return false;
	}

	public void DestroyEntity()
	{
		if (entity != null)
		{
			base.scene.Notify(ESceneNoification.EntityDestroy, mPoint, entity);
			entity = null;
		}
	}

	private void OnEntityDeath(SkEntity cur, SkEntity caster)
	{
		SkAliveEntity cmpt = entity.GetCmpt<SkAliveEntity>();
		if (cur == cmpt)
		{
			base.scene.Notify(ESceneNoification.MonsterDead, this);
			SceneMan.RemoveSceneObj(this);
		}
	}

	public void Respawn()
	{
		if (!mIsProcessing && (entity == null || (entity.GetCmpt<SkAliveEntity>() != null && entity.GetCmpt<SkAliveEntity>().isDead)))
		{
			entity = null;
			SceneMan.RemoveSceneObj(this);
			SceneMan.AddSceneObj(this);
		}
	}
}
