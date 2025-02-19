using System.Collections.Generic;
using ItemAsset;
using Pathea;
using SkillSystem;
using uLink;
using UnityEngine;

namespace SkillAsset;

public abstract class SkillRunner : CommonInterface
{
	private SkEntity mSkEntity;

	public List<MergeSkillInstance> m_mergeSkillInsts = new List<MergeSkillInstance>();

	public List<EffSkillInstance> m_effSkillInsts = new List<EffSkillInstance>();

	public List<EffSkillInstance> m_effShareSkillInsts = new List<EffSkillInstance>();

	public LifeFormController mLifeFormController;

	public EffSkillBuffManager m_effSkillBuffManager = new EffSkillBuffManager();

	protected SkEntity _SkEntity
	{
		get
		{
			if (null == mSkEntity)
			{
				mSkEntity = GetComponent<SkEntity>();
			}
			return mSkEntity;
		}
	}

	public PackBase BuffAttribs
	{
		get
		{
			if (null != _SkEntity)
			{
				return null;
			}
			return _SkEntity.attribs.pack;
		}
		set
		{
			if (null != _SkEntity)
			{
				_SkEntity.attribs.pack = value;
			}
		}
	}

	public virtual float GetAttribute(AttribType type, bool isBase = false)
	{
		if (null != _SkEntity)
		{
			if (isBase)
			{
				return _SkEntity.attribs.raws[(int)type];
			}
			return _SkEntity.attribs.sums[(int)type];
		}
		return 0f;
	}

	public virtual void SetAttribute(AttribType type, float value, bool isBase = true)
	{
		if (null != _SkEntity)
		{
			if (isBase)
			{
				_SkEntity.attribs.raws[(int)type] = value;
			}
			_SkEntity.attribs.sums[(int)type] = value;
		}
	}

	public override ESkillTargetType GetTargetType()
	{
		return ESkillTargetType.TYPE_SkillRunner;
	}

	public override Vector3 GetPosition()
	{
		if (this == null)
		{
			return Vector3.zero;
		}
		return base.transform.position;
	}

	internal abstract byte GetBuilderId();

	internal abstract float GetAtkDist(ISkillTarget target);

	internal abstract ItemPackage GetItemPackage();

	internal abstract bool IsEnemy(ISkillTarget target);

	internal abstract ISkillTarget GetTargetInDist(float dist, int targetMask);

	internal abstract List<ISkillTarget> GetTargetlistInScope(EffScope scope, int targetMask, ISkillTarget target);

	internal abstract void ApplyDistRepel(SkillRunner caster, float distRepel);

	internal abstract void ApplyHpChange(SkillRunner caster, float hpChange, float damagePercent, int type);

	internal abstract void ApplyComfortChange(float comfortChange);

	internal abstract void ApplySatiationChange(float satiationChange);

	internal abstract void ApplyThirstLvChange(float thirstLvChange);

	internal virtual void ApplyPropertyChange(Dictionary<int, float> propertyChanges)
	{
		if (null != mLifeFormController)
		{
			mLifeFormController.ApplyPropertyChange(propertyChanges);
		}
	}

	internal virtual float ApplyEnergyShieldAttack(Projectile proj)
	{
		return 1f;
	}

	internal virtual void ApplyDurChange(SkillRunner caster, float durChange, int type)
	{
	}

	internal virtual void ApplyBuffContinuous(SkillRunner caster, short buffSp)
	{
	}

	internal virtual Transform GetCastTransform(EffItemCast cast)
	{
		return null;
	}

	internal virtual void ApplyLearnSkill(List<int> skillIdList)
	{
	}

	internal virtual void ApplyMetalScan(List<int> metalID)
	{
	}

	internal virtual float GetResRadius()
	{
		return 0.1f;
	}

	internal abstract void ApplyAnim(List<string> animName);

	internal virtual void ApplyAnim(string animName)
	{
	}

	internal virtual void ApplyEffect(List<int> effId, ISkillTarget target)
	{
		if (effId == null)
		{
			return;
		}
		for (int i = 0; i < effId.Count; i++)
		{
			if (effId[i] != 0)
			{
				EffectManager.Instance.Instantiate(effId[i], base.transform);
			}
		}
	}

	internal virtual void ApplySound(int soundID)
	{
	}

	public MergeSkillInstance RunMerge(int skillId, int productNum)
	{
		return null;
	}

	public void CancelMerge()
	{
	}

	public bool CheckRunEffEnabl(int skillId, ISkillTarget target)
	{
		EffSkillInstance inst = new EffSkillInstance();
		inst.m_data = EffSkill.s_tblEffSkills.Find((EffSkill iterSkill1) => EffSkill.MatchId(iterSkill1, skillId));
		if (inst.m_data == null)
		{
			return false;
		}
		if (m_effShareSkillInsts.Find((EffSkillInstance iterSkill0) => EffSkillInstance.MatchType(iterSkill0, inst.m_data.m_cdInfo.m_type)) != null)
		{
			return false;
		}
		if (m_effSkillInsts.Find((EffSkillInstance iterSkill0) => EffSkillInstance.MatchId(iterSkill0, skillId)) != null)
		{
			return false;
		}
		if (!inst.m_data.CheckTargetsValid(this, target))
		{
			return false;
		}
		return true;
	}

	public EffSkillInstance RunEff(int skillId, ISkillTarget target)
	{
		EffSkillInstance inst = new EffSkillInstance();
		inst.m_data = EffSkill.s_tblEffSkills.Find((EffSkill iterSkill1) => EffSkill.MatchId(iterSkill1, skillId));
		if (inst.m_data == null)
		{
			return null;
		}
		if (m_effShareSkillInsts.Find((EffSkillInstance iterSkill0) => EffSkillInstance.MatchType(iterSkill0, inst.m_data.m_cdInfo.m_type)) != null)
		{
			return null;
		}
		if (m_effSkillInsts.Find((EffSkillInstance iterSkill0) => EffSkillInstance.MatchId(iterSkill0, skillId)) != null)
		{
			return null;
		}
		if (!inst.m_data.CheckTargetsValid(this, target))
		{
			return null;
		}
		if (!GameConfig.IsMultiMode || IsController)
		{
			if (GameConfig.IsMultiMode)
			{
				if (target is CommonInterface)
				{
					CommonInterface commonInterface = target as CommonInterface;
					if (null != commonInterface && null != commonInterface.OwnerView)
					{
						RPCServer(EPacketType.PT_InGame_SkillCast, skillId, commonInterface.OwnerView.viewID);
					}
					else
					{
						RPCServer(EPacketType.PT_InGame_SkillCast, skillId, uLink.NetworkViewID.unassigned);
					}
				}
				else if (target is DefaultPosTarget)
				{
					RPCServer(EPacketType.PT_InGame_SkillShoot, skillId, target.GetPosition());
				}
				else
				{
					RPCServer(EPacketType.PT_InGame_SkillCast, skillId, uLink.NetworkViewID.unassigned);
				}
			}
			inst.m_timeStartPrep = Time.time;
			inst.m_runner = new CoroutineStoppable(this, inst.m_data.Exec(this, target, inst));
			inst.m_sharedRunner = new CoroutineStoppable(this, inst.m_data.SharingCooling(this, inst));
		}
		return inst;
	}

	public EffSkillInstance RunEffOnProxy(int skillId, ISkillTarget target)
	{
		EffSkillInstance inst = new EffSkillInstance();
		inst.m_data = EffSkill.s_tblEffSkills.Find((EffSkill iterSkill1) => EffSkill.MatchId(iterSkill1, skillId));
		if (inst.m_data == null)
		{
			return null;
		}
		if (m_effShareSkillInsts.Find((EffSkillInstance iterSkill0) => EffSkillInstance.MatchType(iterSkill0, inst.m_data.m_cdInfo.m_type)) != null)
		{
			return null;
		}
		if (m_effSkillInsts.Find((EffSkillInstance iterSkill0) => EffSkillInstance.MatchId(iterSkill0, skillId)) != null)
		{
			return null;
		}
		inst.m_timeStartPrep = Time.time;
		inst.m_runner = new CoroutineStoppable(this, inst.m_data.ExecProxy(this, target, inst));
		inst.m_sharedRunner = new CoroutineStoppable(this, inst.m_data.SharingCooling(this, inst));
		return inst;
	}

	public ISkillTarget GetTargetByGameObject(RaycastHit hitinfo, EffSkillInstance inst)
	{
		ISkillTarget skillTarget = null;
		GameObject gameObject = hitinfo.collider.gameObject;
		if (gameObject != null)
		{
			switch (gameObject.layer)
			{
			case 12:
				if (gameObject.GetComponent<VFVoxelChunkGo>() != null)
				{
					Vector3 point = hitinfo.point;
					Vector3 vector = point / 1f;
					vector += -Vector3.up * 0.01f;
					IntVector3 intVector = new IntVector3(Mathf.FloorToInt(vector.x + 1f), Mathf.FloorToInt(vector.y + 1f), Mathf.FloorToInt(vector.z + 1f));
					VFVoxel voxel = VFVoxelTerrain.self.Voxels.SafeRead(intVector.x, intVector.y, intVector.z);
					skillTarget = new VFTerrainTarget(hitinfo.point, intVector, ref voxel);
				}
				break;
			case 10:
				skillTarget = gameObject.GetComponent<SkillRunner>();
				break;
			}
		}
		if (!inst.m_data.CheckTargetsValid(this, skillTarget))
		{
			return null;
		}
		return skillTarget;
	}

	private VFVoxel GetRaycastHitVoxel(Vector3 normal, Vector3 point, out IntVector3 voxelPos)
	{
		Vector3 vector = point;
		if (0.05f > Mathf.Abs(normal.normalized.x))
		{
			vector.x = Mathf.RoundToInt(vector.x);
		}
		else
		{
			vector.x = ((!(normal.x > 0f)) ? Mathf.Ceil(vector.x) : Mathf.Floor(vector.x));
		}
		if (0.05f > Mathf.Abs(normal.normalized.y))
		{
			vector.y = Mathf.RoundToInt(vector.y);
		}
		else
		{
			vector.y = ((!(normal.y > 0f)) ? Mathf.Ceil(vector.y) : Mathf.Floor(vector.y));
		}
		if (0.05f > Mathf.Abs(normal.normalized.z))
		{
			vector.z = Mathf.RoundToInt(vector.z);
		}
		else
		{
			vector.z = ((!(normal.z > 0f)) ? Mathf.Ceil(vector.z) : Mathf.Floor(vector.z));
		}
		voxelPos = new Vector3(Mathf.Round(vector.x), Mathf.Round(vector.y), Mathf.Round(vector.z));
		VFVoxel result = VFVoxelTerrain.self.Voxels.SafeRead(voxelPos.x, voxelPos.y, voxelPos.z);
		float num = 0f;
		while (result.Volume == 0)
		{
			num += 0.1f;
			Vector3 vector2 = point - normal * num;
			voxelPos = new Vector3(Mathf.Round(vector2.x), Mathf.Round(vector2.y), Mathf.Round(vector2.z));
			result = VFVoxelTerrain.self.Voxels.SafeRead(voxelPos.x, voxelPos.y, voxelPos.z);
		}
		return result;
	}

	public ISkillTarget GetTerrainTarget(Vector3 normal, Vector3 point)
	{
		IntVector3 voxelPos;
		VFVoxel voxel = GetRaycastHitVoxel(normal, point, out voxelPos);
		return new VFTerrainTarget(point, voxelPos, ref voxel);
	}

	public EffSkillInstance SkipEff(int skillId)
	{
		EffSkillInstance effSkillInstance = new EffSkillInstance();
		effSkillInstance.m_data = EffSkill.s_tblEffSkills.Find((EffSkill iterSkill1) => EffSkill.MatchId(iterSkill1, skillId));
		if (effSkillInstance.m_data == null)
		{
			return null;
		}
		effSkillInstance.m_runner = new CoroutineStoppable(this, effSkillInstance.m_data.SkipExec(this, effSkillInstance));
		effSkillInstance.m_sharedRunner = new CoroutineStoppable(this, effSkillInstance.m_data.SharingCooling(this, effSkillInstance));
		return effSkillInstance;
	}

	public void DeadClear()
	{
		m_effSkillInsts.Clear();
		m_effShareSkillInsts.Clear();
		m_mergeSkillInsts.Clear();
	}

	public void StopEff()
	{
		for (int num = m_effSkillInsts.Count - 1; num >= 0; num--)
		{
			if (m_effSkillInsts[num].m_data.m_interruptable)
			{
				m_effSkillInsts[num].m_runner.stop = true;
				m_effSkillInsts[num].m_sharedRunner.stop = true;
				m_effSkillInsts.RemoveAt(num);
			}
		}
	}

	public void StopEff(int skillId)
	{
		for (int num = m_effSkillInsts.Count - 1; num >= 0; num--)
		{
			if (m_effSkillInsts[num].m_data.m_id == skillId && m_effSkillInsts[num].m_data.m_interruptable)
			{
				m_effSkillInsts[num].m_runner.stop = true;
				m_effSkillInsts[num].m_sharedRunner.stop = true;
				m_effSkillInsts.RemoveAt(num);
				break;
			}
		}
	}

	public void CancelSkillBuff(int skillId)
	{
		m_effSkillBuffManager.Remove(skillId);
	}

	public bool IsEffRunning()
	{
		return m_effSkillInsts.Count > 0;
	}

	public bool IsEffRunning(int skillId)
	{
		return m_effSkillInsts.Find((EffSkillInstance iterSkill0) => EffSkillInstance.MatchId(iterSkill0, skillId)) != null;
	}

	public bool IsSkillRunning(int skillId)
	{
		EffSkillInstance effSkillInstance = m_effSkillInsts.Find((EffSkillInstance iterSkill0) => EffSkillInstance.MatchId(iterSkill0, skillId));
		return effSkillInstance != null && effSkillInstance.m_section > EffSkillInstance.EffSection.None && effSkillInstance.m_section < EffSkillInstance.EffSection.Completed;
	}

	public EffSkillInstance GetRunningEff(int skillId)
	{
		return m_effSkillInsts.Find((EffSkillInstance iterSkill0) => EffSkillInstance.MatchId(iterSkill0, skillId));
	}

	public bool IsSkillCooling(int skillId)
	{
		EffSkill effSkill = EffSkill.s_tblEffSkills.Find((EffSkill iterSkill1) => EffSkill.MatchId(iterSkill1, skillId));
		if (effSkill == null)
		{
			return false;
		}
		if (m_effSkillInsts.Find((EffSkillInstance iterSkill0) => EffSkillInstance.MatchId(iterSkill0, skillId)) != null)
		{
			return true;
		}
		return false;
	}

	public bool IsSharedCooling(int skillId)
	{
		EffSkill skill = EffSkill.s_tblEffSkills.Find((EffSkill iterSkill1) => EffSkill.MatchId(iterSkill1, skillId));
		if (skill == null)
		{
			return false;
		}
		if (m_effShareSkillInsts.Find((EffSkillInstance iterSkill0) => EffSkillInstance.MatchType(iterSkill0, skill.m_cdInfo.m_type)) != null)
		{
			return true;
		}
		return false;
	}

	public bool IsValidSkill(int skillId)
	{
		return !IsSkillCooling(skillId) && !IsSharedCooling(skillId);
	}

	public bool IsSkillCoolingByType(short type)
	{
		return m_effShareSkillInsts.Find((EffSkillInstance iterSkill0) => EffSkillInstance.MatchType(iterSkill0, type)) != null;
	}

	public bool IsAppendBuff(int buffId)
	{
		return m_effSkillBuffManager.IsAppendBuff(buffId);
	}
}
