using System;
using Pathea;
using SkillSystem;
using UnityEngine;

namespace PeCustom;

public class SceneStaticAgent : PeCustomScene.SceneElement, ISceneObjAgent
{
	private SpawnPoint mPoint;

	public int Id { get; set; }

	public int ScenarioId { get; set; }

	public GameObject Go => (!(entity == null)) ? entity.gameObject : null;

	public Vector3 Pos => mPoint.spawnPos;

	public IBoundInScene Bound => null;

	public bool NeedToActivate => false;

	public bool TstYOnActivate => false;

	public PeEntity entity { get; set; }

	public int protoId => mPoint.Prototype;

	public Quaternion Rot => mPoint.Rotation;

	public Vector3 Scale => mPoint.Scale;

	public bool IsTarget => mPoint.IsTarget;

	public bool Visible => mPoint.Visible;

	public SpawnPoint spawnPoint => mPoint;

	public bool IsSave { get; private set; }

	public SceneStaticAgent(DoodadSpawnPoint sp, bool is_saved)
	{
		mPoint = sp;
		IsSave = is_saved;
	}

	public void OnActivate()
	{
	}

	public void OnConstruct()
	{
		if (!spawnPoint.isDead && entity == null && (spawnPoint.Enable || spawnPoint.EntityID != -1))
		{
			CreateEntityStatic();
		}
	}

	public void OnDeactivate()
	{
	}

	public void OnDestruct()
	{
	}

	private void CreateEntityStatic()
	{
		base.scene.Notify(ESceneNoification.CreateDoodad, this, IsSave);
		if (!(entity != null))
		{
			return;
		}
		entity.scenarioId = ScenarioId;
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
	}

	public bool ForceCreateEntity()
	{
		if (spawnPoint.isDead)
		{
			return false;
		}
		CreateEntityStatic();
		return true;
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
			base.scene.Notify(ESceneNoification.DoodadDead, this);
			SceneMan.RemoveSceneObj(this);
			Debug.Log("Doodad Entity id [" + cmpt.GetId() + "] is dead");
		}
	}
}
