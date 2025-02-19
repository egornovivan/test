using System;
using System.Collections.Generic;
using AiAsset;
using ItemAsset;
using Pathea;
using SkillAsset;
using uLink;
using UnityEngine;

public class Projectile : SkillRunner
{
	public static float MindamagePercent;

	public static float DamageRadius = 0.3f;

	public SkillRunner emitRunner;

	public Transform emitTransform;

	public GameObject bufferEffect;

	public float bufferEffectLifetime = 2f;

	public float existTime;

	public int damageSkillID;

	public int hitEffectID;

	public int soundID;

	public int minDampRadius;

	public int maxDampRadius;

	public bool destruct;

	private float energyShieldPercent = 1f;

	private Vector3 spawnPosition = Vector3.zero;

	private List<EnergyShieldCtrl> mShieldList = new List<EnergyShieldCtrl>();

	private bool mValid;

	private Trajectory mTrajectory;

	public virtual byte effectType => 0;

	public override bool IsController
	{
		get
		{
			CommonInterface commonInterface = emitRunner;
			if (null != commonInterface)
			{
				return commonInterface.IsController;
			}
			return base.IsController;
		}
	}

	internal override uLink.NetworkView OwnerView
	{
		get
		{
			CommonInterface commonInterface = emitRunner;
			if (null != commonInterface)
			{
				return commonInterface.OwnerView;
			}
			return base.OwnerView;
		}
	}

	public void SetupTrajectory(SkillRunner caster, Transform emit, ISkillTarget target)
	{
	}

	public void Start()
	{
		mValid = true;
		spawnPosition = base.transform.position;
		Invoke("Destruct", existTime);
	}

	public void Update()
	{
	}

	public virtual void DestroyProjectile()
	{
		if (null != bufferEffect)
		{
			ShiftEffect(bufferEffect);
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public void Init(byte index, SkillRunner emitRunner, ISkillTarget target, Transform emitTransform, float rand1)
	{
		this.emitRunner = emitRunner;
		this.emitTransform = emitTransform;
		SetupTrajectory(emitRunner, emitTransform, target);
	}

	public void Init(byte index, SkillRunner emitRunner, ISkillTarget target, Transform emitTransform, int damageSkillID, float rand1)
	{
		this.emitRunner = emitRunner;
		this.emitTransform = emitTransform;
		this.damageSkillID = damageSkillID;
		SetupTrajectory(emitRunner, emitTransform, target);
	}

	private void Destruct()
	{
		if (destruct)
		{
			RunEff(damageSkillID, null);
			PlayHitEffect(base.transform.position, base.transform.rotation);
		}
		DestroyProjectile();
	}

	public bool IsIgnoreRaycastHit(RaycastHit hitInfo)
	{
		return IsIgnoreCollider(hitInfo.collider);
	}

	private void CheckEnergyShield(Collider collider)
	{
		EnergyShieldCtrl component = collider.GetComponent<EnergyShieldCtrl>();
		if (component != null)
		{
			component.HitShield(this);
		}
	}

	public static Vector3 GetPredictPosition(ISkillTarget target, Vector3 startPos, float speed)
	{
		SkillRunner skillRunner = target as SkillRunner;
		if (skillRunner != null && skillRunner.GetComponent<Rigidbody>() != null)
		{
			Vector3 from = startPos - GetShootPosition(target);
			float num = Mathf.Sqrt(speed * speed - skillRunner.GetComponent<Rigidbody>().velocity.sqrMagnitude);
			float num2 = Mathf.Cos(Vector3.Angle(from, skillRunner.GetComponent<Rigidbody>().velocity) / 180f * (float)Math.PI);
			float num3 = from.sqrMagnitude * skillRunner.GetComponent<Rigidbody>().velocity.sqrMagnitude * num2 * num2;
			float num4 = ((!(Vector3.Angle(from, skillRunner.GetComponent<Rigidbody>().velocity) <= 90f)) ? ((Mathf.Sqrt(from.sqrMagnitude + num3 / num / num) + Mathf.Sqrt(num3 / num / num)) / num) : ((Mathf.Sqrt(from.sqrMagnitude + num3 / num / num) - Mathf.Sqrt(num3 / num / num)) / num));
			return GetShootPosition(target) + skillRunner.GetComponent<Rigidbody>().velocity * num4;
		}
		return GetShootPosition(target);
	}

	public static Vector3 GetShootPosition(ISkillTarget target)
	{
		SkillRunner skillRunner = target as SkillRunner;
		if (skillRunner != null && skillRunner.GetComponent<Collider>() != null)
		{
			AiObject aiObject = skillRunner as AiObject;
			if (aiObject != null)
			{
				if (aiObject.model != null)
				{
					Rigidbody[] componentsInChildren = aiObject.model.GetComponentsInChildren<Rigidbody>();
					if (componentsInChildren != null && componentsInChildren.Length > 0)
					{
						return componentsInChildren[UnityEngine.Random.Range(0, componentsInChildren.Length)].worldCenterOfMass;
					}
				}
				return aiObject.center;
			}
			return AiUtil.GetColliderCenter(skillRunner.GetComponent<Collider>());
		}
		return Vector3.zero;
	}

	public static Vector3 GetTargetCenter(ISkillTarget target)
	{
		SkillRunner skillRunner = target as SkillRunner;
		if (skillRunner != null && skillRunner.GetComponent<Collider>() != null)
		{
			AiObject aiObject = skillRunner as AiObject;
			if (aiObject != null)
			{
				return aiObject.center;
			}
			return AiUtil.GetColliderCenter(skillRunner.GetComponent<Collider>());
		}
		return Vector3.zero;
	}

	public void CheckMovementCollision()
	{
	}

	public void CollisionHitInfo(RaycastHit hitInfo)
	{
		if (mValid)
		{
			CastDamageSkill(hitInfo.collider);
			PlayEffect(hitInfo);
			PlaySound(hitInfo.point);
			DestroyProjectile();
			mValid = false;
		}
	}

	public void TriggerCollider(Collider col)
	{
		if (mValid)
		{
			CastDamageSkill(col);
			PlayHitEffect(col.ClosestPointOnBounds(base.transform.position), Quaternion.identity);
			PlaySound(col.ClosestPointOnBounds(base.transform.position));
			DestroyProjectile();
			mValid = false;
		}
	}

	public void TriggerColliderInterval(Collider col)
	{
		CastDamageSkill(col);
		PlayHitEffect(col.ClosestPointOnBounds(base.transform.position), Quaternion.identity);
	}

	private void PlaySound(Vector3 pos)
	{
		if (soundID > 0)
		{
			AudioManager.instance.Create(pos, soundID);
		}
	}

	public bool IsIgnoreCollider(Collider collider)
	{
		if (collider.transform.IsChildOf(base.transform))
		{
			return true;
		}
		if (collider.transform.tag == "WorldCollider")
		{
			return true;
		}
		if (collider.isTrigger)
		{
			CheckEnergyShield(collider);
			return true;
		}
		if (emitRunner != null)
		{
			Transform transform = emitRunner.transform;
			CreationSkillRunner componentOrOnParent = VCUtils.GetComponentOrOnParent<CreationSkillRunner>(transform.gameObject);
			if (componentOrOnParent != null)
			{
				transform = componentOrOnParent.transform;
			}
			if (collider.transform.IsChildOf(transform))
			{
				return true;
			}
		}
		return false;
	}

	public void CastDamageSkill(Collider other)
	{
		if (emitRunner == null || other == null)
		{
			return;
		}
		VFVoxelChunkGo component = other.GetComponent<VFVoxelChunkGo>();
		B45ChunkGo component2 = other.GetComponent<B45ChunkGo>();
		if (component != null || null != component2)
		{
			EffSkill effSkill = EffSkill.s_tblEffSkills.Find((EffSkill iterSkill1) => EffSkill.MatchId(iterSkill1, damageSkillID));
			if (effSkill.m_scopeOfSkill != null)
			{
				RunEff(damageSkillID, null);
			}
			else
			{
				RunEff(damageSkillID, new DefaultPosTarget(base.transform.position));
			}
			return;
		}
		if (other != null)
		{
			int harm = AiUtil.GetHarm(emitRunner.gameObject);
			if (GameConfig.IsMultiMode)
			{
				SkillRunner componentOrOnParent = VCUtils.GetComponentOrOnParent<SkillRunner>(other.gameObject);
				if (null == componentOrOnParent)
				{
					return;
				}
				int harm2 = AiUtil.GetHarm(componentOrOnParent.gameObject);
				if (AiHarmData.GetHarmValue(harm, harm2) == 0)
				{
					return;
				}
			}
			else
			{
				int harm3 = AiUtil.GetHarm(other.gameObject);
				if (AiHarmData.GetHarmValue(harm, harm3) == 0)
				{
					return;
				}
			}
		}
		EffSkill effSkill2 = EffSkill.s_tblEffSkills.Find((EffSkill iterSkill1) => EffSkill.MatchId(iterSkill1, damageSkillID));
		if (effSkill2 == null)
		{
			return;
		}
		if (effSkill2.m_scopeOfSkill != null)
		{
			RunEff(damageSkillID, null);
			return;
		}
		SkillRunner componentOrOnParent2 = VCUtils.GetComponentOrOnParent<SkillRunner>(other.gameObject);
		if (componentOrOnParent2 != null)
		{
			RunEff(damageSkillID, componentOrOnParent2);
		}
	}

	public void PlayEffect(RaycastHit hitInfo)
	{
		Vector3 position = base.transform.position;
		Quaternion rotation = base.transform.rotation;
		position = ((effectType << 4 >> 7 == 1) ? hitInfo.collider.transform.position : ((effectType << 4 >> 6 != 1) ? hitInfo.point : base.transform.position));
		rotation = ((effectType >> 7 == 1) ? Quaternion.FromToRotation(Vector3.forward, hitInfo.point - base.transform.position) : ((effectType >> 6 == 1) ? base.transform.rotation : ((effectType >> 5 != 1) ? Quaternion.identity : Quaternion.AngleAxis(UnityEngine.Random.Range(0, 360), new Vector3(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value)))));
		EffectManager.Instance.Instantiate(hitEffectID, position, rotation);
	}

	public void PlayHitEffect(Vector3 position, Quaternion rot)
	{
		EffectManager.Instance.Instantiate(hitEffectID, position, rot);
	}

	public void ShiftEffect(GameObject eff)
	{
		eff.transform.parent = base.transform.parent;
		if (null != eff.GetComponent<ParticleSystem>())
		{
			eff.GetComponent<ParticleSystem>().enableEmission = false;
		}
		eff.AddComponent<DestroyTimer>();
		eff.GetComponent<DestroyTimer>().m_LifeTime = bufferEffectLifetime;
	}

	public bool ShieldHasBeenHitted(EnergyShieldCtrl esc)
	{
		return mShieldList.Contains(esc);
	}

	public void ApplyDamageReduce(float reducePercent, EnergyShieldCtrl esc)
	{
		energyShieldPercent *= reducePercent;
		mShieldList.Add(esc);
	}

	public float GetFinalAttack()
	{
		EffSkill effSkill = EffSkill.s_tblEffSkills.Find((EffSkill iterSkill1) => EffSkill.MatchId(iterSkill1, damageSkillID));
		return GetAttribute(AttribType.Atk) * effSkill.m_guidInfo.m_hpChangePercent + effSkill.m_guidInfo.m_hpChangeOnce;
	}

	public override float GetAttribute(AttribType type, bool isBase = false)
	{
		if (type == AttribType.Atk)
		{
			return GetBaseAtk();
		}
		return base.GetAttribute(type, isBase);
	}

	internal float GetBaseAtk()
	{
		if (emitRunner == null)
		{
			return 0f;
		}
		float attribute = emitRunner.GetAttribute(AttribType.Atk);
		float num = 1f;
		if ((float)minDampRadius > float.Epsilon && (float)maxDampRadius > float.Epsilon && maxDampRadius > minDampRadius)
		{
			float num2 = Mathf.Max(0f, Vector3.Distance(spawnPosition, base.transform.position) - (float)minDampRadius);
			num = Mathf.Lerp(1f, MindamagePercent, num2 / (float)(maxDampRadius - minDampRadius));
		}
		return attribute * energyShieldPercent * num;
	}

	internal override List<ISkillTarget> GetTargetlistInScope(EffScope scope, int targetMask, ISkillTarget target)
	{
		List<ISkillTarget> list = new List<ISkillTarget>();
		if (emitRunner == null)
		{
			return list;
		}
		Collider[] array = Physics.OverlapSphere(base.transform.position, scope.m_radius);
		List<Transform> list2 = new List<Transform>();
		Collider[] array2 = array;
		foreach (Collider collider in array2)
		{
			if (!collider.isTrigger)
			{
				if (!list2.Contains(collider.transform) && !collider.gameObject.Equals(base.gameObject) && !collider.gameObject.Equals(emitRunner.gameObject))
				{
					list2.Add(collider.transform);
				}
				Transform transform = collider.transform;
				CreationSkillRunner componentOrOnParent = VCUtils.GetComponentOrOnParent<CreationSkillRunner>(transform.gameObject);
				if (componentOrOnParent != null && !list2.Contains(componentOrOnParent.transform))
				{
					list2.Add(componentOrOnParent.transform);
				}
			}
		}
		foreach (Transform item in list2)
		{
			if (item == null || emitTransform == null)
			{
				continue;
			}
			float f = AiMath.Dot(emitTransform, item);
			float num = Mathf.Acos(f) * 57.29578f;
			int harm = AiUtil.GetHarm(emitRunner.gameObject);
			if (GameConfig.IsMultiMode)
			{
				SkillRunner component = item.GetComponent<SkillRunner>();
				if (null == component)
				{
					continue;
				}
				int harm2 = AiUtil.GetHarm(component.gameObject);
				if (AiHarmData.GetHarmValue(harm, harm2) == 0)
				{
					continue;
				}
			}
			else
			{
				int harm3 = AiUtil.GetHarm(item.gameObject);
				if (AiHarmData.GetHarmValue(harm, harm3) == 0)
				{
					continue;
				}
			}
			Ray ray = new Ray(base.transform.position, item.position - base.transform.position);
			float maxDistance = Vector3.Distance(base.transform.position, item.position);
			if (!Physics.Raycast(ray, maxDistance, (int)AiUtil.groundedLayer | (int)AiUtil.obstructLayer) && !(num > scope.m_degEnd) && !(num < scope.m_degStart))
			{
				SkillRunner skillRunner = item.GetComponent<SkillRunner>();
				if (skillRunner == null)
				{
					skillRunner = VCUtils.GetComponentOrOnParent<SkillRunner>(item.gameObject);
				}
				if (skillRunner != null && !list.Contains(skillRunner))
				{
					list.Add(skillRunner);
				}
			}
		}
		return list;
	}

	internal override byte GetBuilderId()
	{
		return 0;
	}

	internal override float GetAtkDist(ISkillTarget target)
	{
		return 0f;
	}

	internal override ItemPackage GetItemPackage()
	{
		return null;
	}

	internal override bool IsEnemy(ISkillTarget target)
	{
		return true;
	}

	internal override ISkillTarget GetTargetInDist(float dist, int targetMask)
	{
		return null;
	}

	internal override void ApplyEffect(List<int> effId, ISkillTarget target)
	{
		base.ApplyEffect(effId, target);
	}

	internal override void ApplyDistRepel(SkillRunner caster, float distRepel)
	{
	}

	internal override void ApplyHpChange(SkillRunner caster, float hpChange, float damagePercent, int type)
	{
	}

	internal override void ApplyComfortChange(float comfortChange)
	{
	}

	internal override void ApplySatiationChange(float satiationChange)
	{
	}

	internal override void ApplyThirstLvChange(float thirstLvChange)
	{
	}

	internal override void ApplyAnim(List<string> animName)
	{
	}
}
