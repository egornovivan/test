using System;
using System.Collections;
using Pathea;
using PETools;
using SkillSystem;
using UnityEngine;

public class SiegeAgent
{
	public class AgentInfo : MonsterEntityCreator.AgentInfo
	{
		private SiegeAgent _agent;

		public AgentInfo(EntityMonsterBeacon bcn, SiegeAgent agent)
			: base(bcn)
		{
			_agent = agent;
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
			_agent.OnSuceededToCreate();
		}

		public override void OnFailedToCreate(SceneEntityPosAgent agent)
		{
			base.OnFailedToCreate(agent);
			_agent.OnFailedToCreate();
		}
	}

	public static Action<SiegeAgent> DeathEvent;

	private float _hpMax;

	private float _atkMax;

	private float _hp;

	private bool _death;

	private MonsterSiege _siege;

	private SceneEntityPosAgent _agent;

	private PeEntity _target;

	public float hpPercent
	{
		get
		{
			return _hp / _hpMax;
		}
		set
		{
			_hp = _hpMax * Mathf.Clamp01(value);
		}
	}

	public Vector3 position
	{
		get
		{
			if (_agent.entity != null)
			{
				return _agent.entity.position;
			}
			return _agent.Pos;
		}
	}

	public bool death => _death;

	public bool hasView => _agent.entity != null && _agent.entity.hasView;

	public SiegeAgent(MonsterSiege siege, SceneEntityPosAgent agent, float hp, float atk)
	{
		_siege = siege;
		_agent = agent;
		_hpMax = hp;
		_atkMax = atk;
		_hp = _hpMax;
		siege.StartCoroutine(Move());
		siege.StartCoroutine(Attack());
	}

	public void ApplyDamage(float dmgValue)
	{
		_hp = Mathf.Clamp(_hp - dmgValue, 0f, _hpMax);
		if (_agent.entity != null)
		{
			_agent.entity.HPPercent = hpPercent;
		}
		else if (!_death && _hp <= float.Epsilon)
		{
			OnDeath(null, null);
		}
	}

	public void Clear()
	{
		if (_agent != null && _agent.entity != null && _agent.entity.aliveEntity != null)
		{
			_agent.entity.aliveEntity.deathEvent -= OnDeath;
			_agent.entity.aliveEntity.onHpChange -= OnHpChange;
		}
		_siege.StopCoroutine(Move());
		_siege.StopCoroutine(Attack());
	}

	private IEnumerator Move()
	{
		float startTime = Time.time;
		Vector3 startPos = _agent.Pos;
		while (!_death)
		{
			if (_agent.entity != null && _agent.entity is EntityGrp)
			{
				_hp = 0f;
				if (!_death)
				{
					OnDeath(null, null);
				}
				break;
			}
			Vector3 pos = _siege.assemblyPosition;
			float radius = _siege.assemblyRadius;
			if (_target != null && PEUtil.SqrMagnitude(_target.position, pos) > radius * radius)
			{
				_target = null;
			}
			if (_target == null || _target.IsDeath())
			{
				startTime = Time.time;
				startPos = _agent.Pos;
				_target = _siege.GetClosestEntity(_agent);
			}
			if (_agent.entity == null && _target != null && _target.hasView && _agent.Pos.y <= Mathf.Epsilon && PEUtil.SqrMagnitude(_agent.Pos, _target.position, is3D: false) > 25f)
			{
				Vector3 moveDir = _target.position - _agent.Pos;
				moveDir.y = 0f;
				_agent.Pos += moveDir.normalized * moveDir.magnitude * 0.5f;
			}
			if (_target != null && _target.hasView)
			{
				if (_agent.entity != null)
				{
					if (!_agent.entity.hasView)
					{
						Vector3 fixedPos = PEUtil.GetRandomPositionOnGround(_target.position, 3f, 8f, isResult: false);
						if (fixedPos != Vector3.zero)
						{
							_agent.entity.position = fixedPos;
						}
					}
				}
				else if (_agent.Step == SceneEntityPosAgent.EStep.Created && !death)
				{
					ApplyDamage(_hpMax + 100000f);
				}
				else if (_agent.Pos.y > Mathf.Epsilon)
				{
					Vector3 fixedPos2 = PEUtil.GetRandomPositionOnGround(_target.position, 3f, 8f, isResult: false);
					if (fixedPos2 != Vector3.zero)
					{
						_agent.Pos = fixedPos2;
						SceneMan.SetDirty(_agent);
					}
				}
			}
			yield return new WaitForSeconds(1f);
		}
	}

	private bool CanFixedPos()
	{
		if (_agent.entity != null)
		{
			if (_agent.entity.bounds.size == Vector3.zero)
			{
				return true;
			}
		}
		else if (_agent.Pos.y > Mathf.Epsilon)
		{
			return true;
		}
		return false;
	}

	private IEnumerator Attack()
	{
		while (!_death)
		{
			if (_agent.entity == null && _target != null && !_target.IsDeath())
			{
				AttackEntity(_target);
			}
			yield return new WaitForSeconds(UnityEngine.Random.Range(5f, 10f));
		}
	}

	private void AttackEntity(PeEntity entity)
	{
		float attribute = entity.GetAttribute(AttribType.Hp);
		float attribute2 = entity.GetAttribute(AttribType.HpMax);
		attribute = Mathf.Clamp(attribute - _atkMax, 0f, attribute2);
		entity.SetAttribute(AttribType.Hp, attribute, offEvent: false);
	}

	private void OnSuceededToCreate()
	{
		if (_agent.entity != null)
		{
			_agent.entity.HPPercent = hpPercent;
			if (_agent.entity.aliveEntity != null)
			{
				_agent.entity.aliveEntity.deathEvent += OnDeath;
				_agent.entity.aliveEntity.onHpChange += OnHpChange;
			}
		}
	}

	private void OnFailedToCreate()
	{
	}

	private void OnDeath(SkEntity e1, SkEntity e2)
	{
		_death = true;
		Clear();
		SceneMan.RemoveSceneObj(_agent);
		if (DeathEvent != null)
		{
			DeathEvent(this);
		}
	}

	private void OnHpChange(SkEntity skEntity, float damage)
	{
		if (_agent.entity != null)
		{
			hpPercent = _agent.entity.HPPercent;
		}
	}
}
