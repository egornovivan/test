using System.Collections;
using Pathea;
using UnityEngine;

public class EntityPosAgent : ISceneObjAgent
{
	private int m_Id;

	private int m_EntityProID;

	private int m_EntityCreatedID;

	private Vector3 m_Pos;

	private EntityType m_EntityType;

	private IntVec2 m_Idx;

	private bool bCreate;

	public bool bMission;

	public int m_MissionStep;

	int ISceneObjAgent.Id { get; set; }

	GameObject ISceneObjAgent.Go => null;

	Vector3 ISceneObjAgent.Pos => position;

	IBoundInScene ISceneObjAgent.Bound => null;

	bool ISceneObjAgent.NeedToActivate => true;

	bool ISceneObjAgent.TstYOnActivate => false;

	public int id
	{
		get
		{
			return m_Id;
		}
		private set
		{
			m_Id = value;
		}
	}

	public int ScenarioId { get; set; }

	public int proid
	{
		get
		{
			return m_EntityProID;
		}
		set
		{
			m_EntityProID = value;
		}
	}

	public int createdid
	{
		get
		{
			return m_EntityCreatedID;
		}
		set
		{
			m_EntityCreatedID = value;
		}
	}

	public EntityType entitytype
	{
		get
		{
			return m_EntityType;
		}
		set
		{
			m_EntityType = value;
		}
	}

	public IntVec2 idx
	{
		get
		{
			return m_Idx;
		}
		set
		{
			m_Idx = value;
		}
	}

	public Vector3 position
	{
		get
		{
			return m_Pos;
		}
		set
		{
			m_Pos = value;
		}
	}

	void ISceneObjAgent.OnConstruct()
	{
	}

	void ISceneObjAgent.OnActivate()
	{
		if (createdid <= 0)
		{
			VFVoxelTerrain.self.StartCoroutine(CreateEntity());
		}
	}

	void ISceneObjAgent.OnDeactivate()
	{
	}

	void ISceneObjAgent.OnDestruct()
	{
		if (createdid != 0)
		{
			PeSingleton<PeCreature>.Instance.Destory(createdid);
			createdid = 0;
		}
		EntityCreateMgr.Instance.RemoveStoryEntityMgr(m_Idx);
	}

	public void DestroyEntity()
	{
		if (createdid != 0)
		{
			PeSingleton<PeCreature>.Instance.Destory(createdid);
			createdid = 0;
		}
	}

	private IEnumerator CreateEntity()
	{
		LayerMask layer = 71680;
		Vector3 pos = new Vector3(m_Pos.x, 600f, m_Pos.z);
		RaycastHit hitInfo;
		while (!Physics.Raycast(pos, Vector3.down, out hitInfo, 600f, layer))
		{
			yield return new WaitForSeconds(0.1f);
		}
		m_Pos.y = Mathf.Floor(hitInfo.point.y) + 1f;
		if (m_EntityType == EntityType.EntityType_Npc)
		{
			EntityCreateMgr.Instance.CreateRandomNpc(m_EntityProID, m_Pos);
			SceneMan.RemoveSceneObj(this);
		}
		else
		{
			EntityCreateMgr.Instance.CreateMonsterInst(m_Pos, proid, this);
		}
	}
}
