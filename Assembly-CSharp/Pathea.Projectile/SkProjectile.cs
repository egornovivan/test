using System;
using System.Collections;
using System.Collections.Generic;
using Pathea.Effect;
using PETools;
using SkillSystem;
using UnityEngine;
using WhiteCat;

namespace Pathea.Projectile;

public class SkProjectile : SkEntity, IDigTerrain
{
	public static List<SkProjectile> s_Projectiles = new List<SkProjectile>();

	public static Action<SkEntity> onHitSkEntity;

	[SerializeField]
	private float m_LifeTime;

	[SerializeField]
	private float m_DelayTime;

	[SerializeField]
	private int m_Atk;

	[SerializeField]
	private int m_SkillID;

	[SerializeField]
	private int m_SkillTerrainID;

	[SerializeField]
	private int m_SoundID;

	[SerializeField]
	private int m_EffectID;

	[SerializeField]
	private int m_EffectRot;

	[SerializeField]
	private float m_Interval;

	[SerializeField]
	private float m_ResRange;

	[SerializeField]
	private bool m_Trigger;

	[SerializeField]
	private bool m_Explode;

	[SerializeField]
	private float m_ExplodeRadius;

	[SerializeField]
	private Transform bufferEffect;

	[SerializeField]
	private float bufferEffectTime;

	[SerializeField]
	private bool m_DeletWhenCasterDead;

	[SerializeField]
	private float m_DeleteDelayTime = 0.5f;

	[SerializeField]
	private bool m_DeletByHitLayer;

	[SerializeField]
	private LayerMask m_DeletLayer;

	[HideInInspector]
	[SerializeField]
	private bool m_Inited;

	private bool m_Valid;

	private float m_StartTime;

	private Bounds m_Bounds;

	internal Transform m_Caster;

	internal Transform m_Emitter;

	internal Transform m_Target;

	internal Vector3 m_TargetPosition;

	internal PeEntity m_Entity;

	private bool m_BoundsUpdated;

	[SerializeField]
	[HideInInspector]
	private Trajectory m_Trajectory;

	private List<SkEntity> m_Entities = new List<SkEntity>();

	private List<SkEntity> m_DamageEntities = new List<SkEntity>();

	private List<Collider> m_Colliders = new List<Collider>();

	[HideInInspector]
	[SerializeField]
	private Collider[] m_Triggers;

	private Collider m_SelfCollider;

	public SkEntity parentSkEntity { get; set; }

	public Bounds TriggerBounds
	{
		get
		{
			if (!m_BoundsUpdated)
			{
				CalculateTriggerBounds();
			}
			return m_Bounds;
		}
	}

	public IntVector4 digPosType => new IntVector4(base.transform.position, 0);

	public event Action<SkEntity> onCastSkill;

	public override Collider GetCollider(string name)
	{
		Transform child = PEUtil.GetChild(base.transform, name);
		return (!(child != null)) ? null : child.GetComponent<Collider>();
	}

	public GameObject Caster()
	{
		if (m_Caster == null)
		{
			return null;
		}
		return m_Caster.gameObject;
	}

	public virtual void SetData(ProjectileData data, Transform caster, Transform emitter, Transform target, Vector3 targetPosition, int index = 0)
	{
		m_Caster = caster;
		m_Emitter = emitter;
		m_Target = target;
		m_TargetPosition = targetPosition;
		m_Valid = true;
		float num = 0f;
		SkEntity componentInParent = m_Caster.GetComponentInParent<SkEntity>();
		if (componentInParent != null)
		{
			SetNet(componentInParent._net, isSwitch: false);
			num = componentInParent.GetAttribute(25);
		}
		parentSkEntity = componentInParent;
		SkProjectile skProjectile = componentInParent as SkProjectile;
		if (skProjectile != null)
		{
			parentSkEntity = skProjectile.GetSkEntityCaster();
			if (parentSkEntity != null)
			{
				m_Caster = parentSkEntity.transform;
			}
		}
		SetAttribute(25, (float)m_Atk + num);
		if (m_Trajectory != null)
		{
			m_Trajectory.SetData(m_Caster, m_Emitter, m_Target, m_TargetPosition, index);
		}
		if (null != m_Emitter && !m_Explode)
		{
			CheckInTrigger(m_Emitter.position);
		}
	}

	public SkEntity GetSkEntityCaster()
	{
		if (m_Caster != null)
		{
			SkEntity component = m_Caster.GetComponent<SkEntity>();
			if (component is SkProjectile)
			{
				return (component as SkProjectile).GetSkEntityCaster();
			}
			return component;
		}
		return null;
	}

	protected Transform GetCasterTrans()
	{
		SkEntity skEntityCaster = GetSkEntityCaster();
		if (null != skEntityCaster)
		{
			return skEntityCaster.transform;
		}
		return null;
	}

	private void CalculateTriggerBounds()
	{
		m_Bounds.center = Vector3.zero;
		m_Bounds.size = Vector3.zero;
		for (int i = 0; i < m_Triggers.Length; i++)
		{
			Collider collider = m_Triggers[i];
			if (collider != null)
			{
				if (m_Bounds.center != Vector3.zero)
				{
					m_Bounds.Encapsulate(collider.bounds);
					continue;
				}
				m_Bounds.center = collider.bounds.center;
				m_Bounds.size = collider.bounds.size;
			}
		}
		m_BoundsUpdated = true;
	}

	private PECapsuleHitResult GetHitResult(Transform self, Collider other)
	{
		PECapsuleHitResult result = null;
		PEDefenceTrigger component = other.transform.GetComponent<PEDefenceTrigger>();
		if (null == m_SelfCollider)
		{
			m_SelfCollider = GetComponentInChildren<Collider>();
		}
		if (null != component && (null == m_SelfCollider || !(m_SelfCollider is MeshCollider)))
		{
			float magnitude = (TriggerBounds.size / 2f).magnitude;
			if (magnitude <= Mathf.Epsilon)
			{
				magnitude = 0.1f;
			}
			if (!component.active || !component.GetClosest(self.position, (TriggerBounds.size / 2f).magnitude, out result))
			{
				return null;
			}
		}
		else
		{
			result = new PECapsuleHitResult();
			result.hitTrans = other.transform;
			result.selfTrans = self;
			result.hitPos = other.ClosestPointOnBounds(self.position);
			Vector3 vector = result.hitPos - self.position;
			if (vector == Vector3.zero)
			{
				vector = other.transform.position - self.position;
			}
			result.distance = vector.magnitude;
			result.hitDir = vector.normalized;
			SkEntity componentInParent = other.transform.GetComponentInParent<SkEntity>();
			if (null != componentInParent)
			{
				if (componentInParent is VFVoxelTerrain)
				{
					result.hitDefenceType = DefenceType.Carrier;
				}
				else if (componentInParent is CreationSkEntity)
				{
					result.hitDefenceType = DefenceType.Carrier;
				}
			}
			result.damageScale = 1f;
		}
		result.selfAttackForm = AttackForm.Bullet;
		return result;
	}

	private void OnColliderEnter(Collider self, Collider other)
	{
		if (((1 << other.gameObject.layer) & GameConfig.ProjectileDamageLayer) == 0)
		{
			return;
		}
		if ((m_Caster == null || !other.transform.IsChildOf(m_Caster)) && !other.transform.IsChildOf(base.transform) && self.transform.IsChildOf(base.transform) && ((1 << other.gameObject.layer) & GameConfig.ProjectileDamageLayer) != 0)
		{
			PECapsuleHitResult hitResult = GetHitResult(self.transform, other);
			if (hitResult != null)
			{
				CollisionCheck(hitResult);
			}
		}
		OnTriggerEnter(other);
	}

	protected virtual void CastSkill(SkEntity skEntity)
	{
		if (PEUtil.IsVoxelOrBlock45(skEntity))
		{
			if (m_SkillTerrainID > 0)
			{
				StartSkill(skEntity, m_SkillTerrainID);
			}
		}
		else if (m_SkillID > 0)
		{
			StartSkill(skEntity, m_SkillID);
		}
		if (this.onCastSkill != null)
		{
			this.onCastSkill(skEntity);
		}
		if (null != skEntity && onHitSkEntity != null)
		{
			onHitSkEntity(skEntity);
		}
	}

	protected virtual void CastSkill(SkEntity skEntity, PECapsuleHitResult hitResult)
	{
		if (PEUtil.IsVoxelOrBlock45(skEntity))
		{
			if (m_SkillTerrainID > 0)
			{
				SkInst skInst = StartSkill(skEntity, m_SkillTerrainID, null, bStartImm: false);
				if (skInst != null)
				{
					skInst._colInfo = hitResult;
					skInst.Start();
				}
			}
		}
		else if (m_SkillID > 0)
		{
			SkInst skInst2 = StartSkill(skEntity, m_SkillID, null, bStartImm: false);
			if (skInst2 != null)
			{
				skInst2._colInfo = hitResult;
				skInst2.Start();
			}
		}
		if (this.onCastSkill != null)
		{
			this.onCastSkill(skEntity);
		}
		if (null != skEntity && onHitSkEntity != null)
		{
			onHitSkEntity(skEntity);
		}
	}

	private bool GetRaycastInfo(Vector3 position, Vector3 velcity, out RaycastHit hitInfo, out Transform hitTrans, out PECapsuleHitResult hitResult, out bool useHitReslut, int layer)
	{
		hitResult = null;
		useHitReslut = false;
		hitInfo = default(RaycastHit);
		hitTrans = null;
		if (velcity.sqrMagnitude > 0.0025000002f)
		{
			Ray ray = new Ray(position, velcity);
			RaycastHit[] hits = Physics.RaycastAll(ray, velcity.magnitude, layer);
			hits = PEUtil.SortHitInfo(hits, ignoreTrigger: false);
			for (int i = 0; i < hits.Length; i++)
			{
				RaycastHit raycastHit = hits[i];
				if (raycastHit.transform == null || raycastHit.transform.IsChildOf(base.transform) || (m_Caster != null && raycastHit.transform.IsChildOf(m_Caster)))
				{
					continue;
				}
				if (raycastHit.collider.gameObject.tag == "EnergyShield")
				{
					EnergySheildHandler component = raycastHit.collider.GetComponent<EnergySheildHandler>();
					if (component != null)
					{
						component.Impact(raycastHit.point);
					}
					continue;
				}
				hitInfo = raycastHit;
				hitTrans = raycastHit.transform;
				PEDefenceTrigger component2 = raycastHit.transform.GetComponent<PEDefenceTrigger>();
				if (null != component2)
				{
					if (!component2.RayCast(ray, 100f, out hitResult))
					{
						continue;
					}
					hitInfo.point = hitResult.hitPos;
					hitTrans = hitResult.hitTrans;
					useHitReslut = true;
				}
				else if (raycastHit.collider.isTrigger)
				{
					return false;
				}
				return true;
			}
		}
		return false;
	}

	private void CheckInTrigger(Vector3 emitter)
	{
		Collider[] array = Physics.OverlapSphere(base.transform.position, 0.1f, GameConfig.ProjectileDamageLayer);
		foreach (Collider collider in array)
		{
			if (collider.transform == null || collider.transform.IsChildOf(base.transform) || (m_Caster != null && collider.transform.IsChildOf(m_Caster)))
			{
				continue;
			}
			if (collider.gameObject.tag == "EnergyShield")
			{
				EnergySheildHandler component = collider.GetComponent<EnergySheildHandler>();
				if (component != null)
				{
					component.Impact(emitter);
				}
				continue;
			}
			PEDefenceTrigger component2 = collider.transform.GetComponent<PEDefenceTrigger>();
			if (null != component2)
			{
				if (component2.GetClosest(emitter, (TriggerBounds.size / 2f).magnitude, out var result))
				{
					Hit(result.hitPos, -result.hitDir, result.hitTrans);
					if (!m_Trigger)
					{
						m_Valid = false;
					}
					break;
				}
				continue;
			}
			if (!collider.isTrigger)
			{
				Hit(emitter, Vector3.Normalize(emitter - collider.transform.position), collider.transform);
				if (!m_Trigger)
				{
					m_Valid = false;
				}
			}
			break;
		}
	}

	private void PlayEffectHit(Vector3 hitPos, Vector3 hitNormal, bool useNormal = true)
	{
		if (m_EffectID > 0)
		{
			Quaternion rotation = Quaternion.identity;
			if (m_EffectRot == 1)
			{
				rotation = base.transform.rotation;
			}
			else if (useNormal && m_EffectRot == 2)
			{
				rotation = Quaternion.LookRotation(hitNormal);
			}
			Singleton<EffectBuilder>.Instance.Register(m_EffectID, null, hitPos, rotation);
		}
		if (m_SoundID > 0)
		{
			AudioManager.instance.Create(hitPos, m_SoundID);
		}
	}

	private void PlayEffect()
	{
		PlayEffectHit(base.transform.position, Vector3.zero, useNormal: false);
	}

	protected virtual void Hit(Vector3 pos, Vector3 normal, Transform hitTrans)
	{
		PlayEffectHit(pos, normal);
		if (!m_Trigger)
		{
			Delete();
		}
		if (!(hitTrans != null))
		{
			return;
		}
		SkEntity componentInParent = hitTrans.GetComponentInParent<SkEntity>();
		if (!(null == componentInParent))
		{
			PeEntity component = componentInParent.GetComponent<PeEntity>();
			if (null != componentInParent && (null == component || component.canInjured))
			{
				PECapsuleHitResult pECapsuleHitResult = new PECapsuleHitResult();
				pECapsuleHitResult.selfTrans = base.transform;
				pECapsuleHitResult.hitPos = pos;
				pECapsuleHitResult.hitDir = -normal;
				pECapsuleHitResult.hitTrans = hitTrans;
				pECapsuleHitResult.damageScale = 1f;
				CastSkill(componentInParent, pECapsuleHitResult);
			}
		}
	}

	protected virtual void Hit(PECapsuleHitResult hitResult, SkEntity skEntity = null)
	{
		PlayEffectHit(hitResult.hitPos, -hitResult.hitDir);
		if (!m_Trigger)
		{
			Delete();
		}
		if (!(hitResult.hitTrans != null))
		{
			return;
		}
		if (null == skEntity)
		{
			skEntity = hitResult.hitTrans.GetComponentInParent<SkEntity>();
		}
		if (!(null == skEntity))
		{
			PeEntity component = skEntity.GetComponent<PeEntity>();
			if (null == component || component.canInjured)
			{
				CastSkill(skEntity, hitResult);
			}
		}
	}

	private void Delete()
	{
		StopAllCoroutines();
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			ParticleSystem component = renderer.GetComponent<ParticleSystem>();
			if (null != component)
			{
				component.enableEmission = false;
			}
			if (!(renderer is LineRenderer) && !(renderer is TrailRenderer) && !(renderer is ParticleSystemRenderer))
			{
				renderer.enabled = false;
			}
		}
		UnityEngine.Object.Destroy(base.gameObject, m_DeleteDelayTime);
		m_Valid = false;
		if (null != m_Trajectory)
		{
			m_Trajectory.isActive = false;
		}
	}

	protected virtual void OnLifeTimeEnd()
	{
		if (m_Explode)
		{
			Explode();
		}
	}

	private IEnumerator DeleteEnumerator(float delayTime)
	{
		yield return new WaitForSeconds(delayTime);
		OnLifeTimeEnd();
		Delete();
	}

	private IEnumerator TriggerDamage()
	{
		while (m_Trigger && !m_Explode)
		{
			for (int i = 0; i < m_Entities.Count; i++)
			{
				if (null != m_Entities[i])
				{
					PeEntity entity = m_Entities[i].GetComponent<PeEntity>();
					if (null == entity || entity.canInjured)
					{
						CastSkill(m_Entities[i]);
					}
				}
			}
			if (m_Colliders.Count > 0)
			{
				PlayEffect();
			}
			yield return new WaitForSeconds(m_Interval);
		}
	}

	private void Explode()
	{
		if (!m_Valid)
		{
			return;
		}
		for (int i = 0; i < m_Entities.Count; i++)
		{
			if (null != m_Entities[i])
			{
				PeEntity component = m_Entities[i].GetComponent<PeEntity>();
				if (null == component || component.canInjured)
				{
					CastSkill(m_Entities[i]);
				}
			}
		}
		if (m_Colliders.Count > 0)
		{
			PlayEffect();
		}
		m_Valid = false;
	}

	public void Awake()
	{
		Init(null, null, 97);
		m_StartTime = Time.time;
		if (!m_Inited)
		{
			m_Entity = GetComponent<PeEntity>();
			if (m_Entity == null)
			{
				m_Entity = base.gameObject.AddComponent<PeEntity>();
			}
			m_Triggers = GetComponentsInChildren<Collider>();
			m_Trajectory = GetComponent<Trajectory>();
		}
		if (m_Trigger && m_Interval > float.Epsilon)
		{
			StartCoroutine(TriggerDamage());
		}
		SetAttribute(22, m_ResRange);
		PETrigger.AttachTriggerEvent(base.gameObject, OnColliderEnter, null, null);
		if (bufferEffect != null)
		{
			PEFollow.Follow(bufferEffect, base.transform);
		}
		StartCoroutine(DeleteEnumerator(m_LifeTime));
		s_Projectiles.Add(this);
	}

	public void Update()
	{
		if (!m_Valid || Time.time - m_StartTime <= m_DelayTime)
		{
			return;
		}
		m_BoundsUpdated = false;
		CheckCasterAlive();
		if (m_Trajectory != null)
		{
			Vector3 vector = m_Trajectory.Track(Time.deltaTime);
			Quaternion rotation = m_Trajectory.Rotate(Time.deltaTime);
			if (GetRaycastInfo(base.transform.position, vector, out var hitInfo, out var hitTrans, out var hitResult, out var useHitReslut, GameConfig.ProjectileDamageLayer))
			{
				if (!m_Explode)
				{
					if (!m_Trajectory.rayCast)
					{
						vector = hitInfo.point - base.transform.position;
						if (useHitReslut && hitResult.hitTrans != null)
						{
							Hit(hitResult);
						}
						else
						{
							Hit(hitInfo.point, hitInfo.normal, hitTrans);
						}
					}
					if (!m_Trigger)
					{
						m_Valid = false;
					}
				}
				else if (m_ExplodeRadius < float.Epsilon)
				{
					Explode();
					Delete();
				}
			}
			base.transform.position += vector;
			base.transform.rotation = rotation;
			m_Trajectory.moveVector = vector;
		}
		else
		{
			if (!m_Explode || !(m_ExplodeRadius > float.Epsilon))
			{
				return;
			}
			Collider[] array = Physics.OverlapSphere(base.transform.position, m_ExplodeRadius);
			for (int i = 0; i < array.Length; i++)
			{
				CommonCmpt componentInParent = array[i].GetComponentInParent<CommonCmpt>();
				if (componentInParent != null && componentInParent.isPlayerOrNpc)
				{
					Explode();
					Delete();
					break;
				}
			}
		}
	}

	public virtual void OnDestroy()
	{
		if (bufferEffect != null)
		{
			ShiftEffect();
		}
		PETrigger.DetachTriggerEvent(base.gameObject, OnColliderEnter, null, null);
		s_Projectiles.Remove(this);
	}

	public void OnTriggerEnter(Collider other)
	{
		if (m_DeletByHitLayer && (m_DeletLayer.value & (1 << other.gameObject.layer)) != 0)
		{
			Delete();
		}
		if (!m_Valid || ((1 << other.gameObject.layer) & GameConfig.ProjectileDamageLayer) == 0 || other.gameObject.GetComponentInParent<SkProjectile>() != null || other.transform.IsChildOf(base.transform) || (m_Caster != null && other.transform.IsChildOf(m_Caster)))
		{
			return;
		}
		if (!m_Colliders.Contains(other))
		{
			m_Colliders.Add(other);
		}
		SkEntity componentInParent = other.GetComponentInParent<SkEntity>();
		if (!(componentInParent != null))
		{
			return;
		}
		if (!m_Explode && !m_DamageEntities.Contains(componentInParent))
		{
			PECapsuleHitResult hitResult = GetHitResult(base.transform, other);
			if (hitResult != null)
			{
				if (other.gameObject.tag == "EnergyShield")
				{
					EnergySheildHandler component = other.GetComponent<EnergySheildHandler>();
					if (component != null)
					{
						component.Impact(hitResult.hitPos);
					}
					return;
				}
				if (!m_DamageEntities.Contains(componentInParent))
				{
					Hit(hitResult);
					m_DamageEntities.Add(componentInParent);
				}
			}
		}
		if (!m_Entities.Contains(componentInParent))
		{
			m_Entities.Add(componentInParent);
		}
	}

	public void OnTriggerExit(Collider other)
	{
		if (!other.transform.IsChildOf(base.transform) && (!(m_Caster != null) || !other.transform.IsChildOf(m_Caster)))
		{
			if (m_Colliders.Contains(other))
			{
				m_Colliders.Remove(other);
			}
			SkEntity componentInParent = other.GetComponentInParent<SkEntity>();
			if (componentInParent != null && m_Entities.Contains(componentInParent))
			{
				m_Entities.Remove(componentInParent);
			}
		}
	}

	public override void ApplyEmission(int emitId, SkRuntimeInfo info)
	{
		base.ApplyEmission(emitId, info);
		Singleton<ProjectileBuilder>.Instance.Register(emitId, base.transform, info);
	}

	private void ShiftEffect()
	{
		ParticleSystem[] componentsInChildren = bufferEffect.GetComponentsInChildren<ParticleSystem>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enableEmission = false;
		}
		bufferEffect.gameObject.AddComponent<DestroyTimer>().m_LifeTime = bufferEffectTime;
	}

	private void CheckCasterAlive()
	{
		if (m_DeletWhenCasterDead)
		{
			PESkEntity pESkEntity = GetSkEntityCaster() as PESkEntity;
			if (null != pESkEntity && pESkEntity.isDead)
			{
				Delete();
			}
		}
	}
}
