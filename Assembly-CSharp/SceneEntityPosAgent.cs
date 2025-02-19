using System;
using System.Collections;
using Pathea;
using UnityEngine;

public class SceneEntityPosAgent : ISceneObjAgent
{
	public class AgentInfo
	{
		public static AgentInfo s_defAgentInfo = new AgentInfo();

		public virtual void OnSuceededToCreate(SceneEntityPosAgent agent)
		{
			LodCmpt lodCmpt = agent.entity.lodCmpt;
			if (lodCmpt != null)
			{
				lodCmpt.onDestruct = (Action<PeEntity>)Delegate.Combine(lodCmpt.onDestruct, (Action<PeEntity>)delegate
				{
					agent.DestroyEntity();
				});
			}
		}

		public virtual void OnFailedToCreate(SceneEntityPosAgent agent)
		{
			SceneMan.RemoveSceneObj(agent);
		}
	}

	public enum EStep
	{
		NotCreated,
		InCreating,
		Created,
		Destroyed
	}

	private const int MaxTimes = 100;

	private const float OneWait = 0.05f;

	public const float PosYAssignedMin = float.Epsilon;

	public const float PosYTBD = 0f;

	private EStep _step;

	private Vector3 _pos;

	private Vector3 _scl;

	private Quaternion _rot;

	private EntityType _type;

	private bool _canRide = true;

	public bool FixPos { get; set; }

	public int protoId { get; set; }

	public PeEntity entity { get; set; }

	public AgentInfo spInfo { get; set; }

	public int Id { get; set; }

	public int ScenarioId { get; set; }

	public GameObject Go => (!(entity == null)) ? entity.gameObject : null;

	public Vector3 Pos
	{
		get
		{
			return _pos;
		}
		set
		{
			_pos = value;
		}
	}

	public bool canRide
	{
		get
		{
			return _canRide;
		}
		set
		{
			_canRide = value;
		}
	}

	public IBoundInScene Bound => null;

	public bool NeedToActivate => _type != EntityType.EntityType_Doodad;

	public bool TstYOnActivate => _type != EntityType.EntityType_Doodad && _pos.y >= float.Epsilon;

	public bool IsIdle => _step != EStep.InCreating;

	public EStep Step => _step;

	public Vector3 Scl => _scl;

	public Quaternion Rot => _rot;

	public SceneEntityPosAgent(Vector3 pos, EntityType type, int protoid = -1)
	{
		_pos = pos;
		_scl = Vector3.one;
		_rot = Quaternion.identity;
		_type = type;
		protoId = protoid;
		spInfo = AgentInfo.s_defAgentInfo;
	}

	public SceneEntityPosAgent(Vector3 pos, Vector3 scl, Quaternion rot, EntityType type, int protoid = -1)
	{
		_pos = pos;
		_scl = scl;
		_rot = rot;
		_type = type;
		protoId = protoid;
		spInfo = AgentInfo.s_defAgentInfo;
	}

	public void OnConstruct()
	{
		if (!NeedToActivate && entity == null && _step != EStep.InCreating)
		{
			CreateEntityStatic();
		}
	}

	public void OnActivate()
	{
		if (NeedToActivate && entity == null && _step != EStep.InCreating)
		{
			SceneMan.self.StartCoroutine(TryCreateEntityUnstatic());
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
		_step = EStep.InCreating;
		DoodadEntityCreator.CreateDoodad(this);
		if (entity == null)
		{
			Debug.LogWarning("[EntityPosAgent]:Failed to create entity " + _type.ToString() + protoId);
		}
		SceneMan.RemoveSceneObj(this);
		_step = EStep.Created;
	}

	private bool CheckSetAvailablePos()
	{
		if (!NeedToActivate || FixPos)
		{
			return true;
		}
		RaycastHit[] array = SceneMan.DependenceHitTst(this);
		if (array != null && array.Length > 0)
		{
			if (array.Length > 1)
			{
				PeEntity mainPlayer = PeSingleton<PeCreature>.Instance.mainPlayer;
				if (null != mainPlayer)
				{
					float[] array2 = new float[array.Length];
					float num = 0f;
					int num2 = -1;
					for (int i = 0; i < array.Length; i++)
					{
						array2[i] = Mathf.Abs(array[i].point.y - mainPlayer.position.y);
						if (array2[i] > float.Epsilon)
						{
							array2[i] = 1f / array2[i];
							num += array2[i];
							continue;
						}
						num2 = i;
						break;
					}
					if (num2 < 0)
					{
						float num3 = UnityEngine.Random.Range(0f, num);
						float num4 = 0f;
						for (int j = 0; j < array2.Length; j++)
						{
							if (num3 < array2[j] + num4)
							{
								num2 = j;
								break;
							}
							num4 += array2[j];
						}
					}
					_pos.y = array[num2].point.y + 1f;
					array2 = null;
				}
				else
				{
					_pos.y = Mathf.Max(array[0].point.y, array[1].point.y) + 1f;
				}
			}
			else
			{
				_pos.y = array[0].point.y + 1f;
			}
			return true;
		}
		return false;
	}

	private IEnumerator TryCreateEntityUnstatic()
	{
		_step = EStep.InCreating;
		int n = 0;
		while (n++ < 100)
		{
			if (CheckSetAvailablePos())
			{
				switch (_type)
				{
				case EntityType.EntityType_Npc:
					NpcEntityCreator.CreateNpc(this);
					break;
				case EntityType.EntityType_Monster:
					MonsterEntityCreator.CreateMonster(this);
					break;
				}
				if (entity != null)
				{
					spInfo.OnSuceededToCreate(this);
				}
				else
				{
					spInfo.OnFailedToCreate(this);
				}
				break;
			}
			yield return new WaitForSeconds(0.05f);
		}
		if (n >= 100)
		{
			Debug.LogWarning("[EntityPosAgent]:Failed to create entity " + _type.ToString() + protoId + " during " + (float)n * 0.05f);
		}
		_step = EStep.Created;
	}

	public void DestroyEntity()
	{
		if (entity != null)
		{
			PeSingleton<PeCreature>.Instance.Destory(entity.Id);
			entity = null;
		}
	}
}
