using System;
using System.Collections;
using System.Collections.Generic;
using ItemAsset;
using Mono.Data.SqliteClient;
using UnityEngine;

namespace SkillAsset;

public class EffSkill
{
	internal int m_id;

	internal string[] m_name = new string[2];

	internal string[] m_desc = new string[2];

	internal short m_type;

	internal string m_iconImgPath;

	internal bool m_interruptable;

	internal int m_targetMask;

	internal float m_distOfSkill;

	internal List<EffScope> m_scopeOfSkill;

	internal EffCoolDownInfo m_cdInfo;

	internal EffPrepareInfo m_prepInfo;

	internal EffItemCast m_itemCast;

	internal EffGuidanceInfo m_guidInfo;

	internal EffItemGot m_itemsGot;

	internal List<int> m_skillIdsGot;

	internal List<int> m_metalScanID;

	internal string m_endAction;

	internal float m_endAniTime;

	public static List<string> s_tblEffSkillsColName_CN;

	public static List<string> s_tblEffSkillsColName_EN;

	public static List<EffSkill> s_tblEffSkills;

	public bool CheckTargetsValid(SkillRunner caster, ISkillTarget target)
	{
		if (m_scopeOfSkill != null)
		{
			return true;
		}
		if (m_cdInfo.m_type > 4 || m_cdInfo.m_type >= 1)
		{
		}
		Projectile component = caster.GetComponent<Projectile>();
		if (component != null)
		{
			return true;
		}
		if (target != null)
		{
			float num = ((!(m_distOfSkill < 0f)) ? m_distOfSkill : caster.GetAtkDist(target));
			if (((target.GetPosition() - caster.GetPosition()).sqrMagnitude <= num * num || num < float.Epsilon || num > -1E-45f) && (m_targetMask & ESkillTarget.Type2Mask(target.GetTargetType())) != 0)
			{
				return true;
			}
		}
		return false;
	}

	public List<ISkillTarget> GetTargetList(SkillRunner caster, ISkillTarget target)
	{
		List<ISkillTarget> list = new List<ISkillTarget>();
		if (m_scopeOfSkill == null)
		{
			float num = ((!(m_distOfSkill < 0f)) ? m_distOfSkill : caster.GetAtkDist(target));
			if (target == null || ((target.GetPosition() - caster.GetPosition()).sqrMagnitude > num * num && num > float.Epsilon))
			{
				if (target != null && (target = caster.GetTargetInDist(num, m_targetMask)) != null)
				{
					list.Add(target);
				}
			}
			else
			{
				switch (target.GetTargetType())
				{
				default:
					if ((m_targetMask & ESkillTarget.Type2Mask(target.GetTargetType())) != 0)
					{
						list.Add(target);
					}
					break;
				case ESkillTargetType.TYPE_Building:
					if ((m_targetMask & 0x10) != 0 && !caster.IsEnemy(target))
					{
						list.Add(target);
					}
					else if ((m_targetMask & 2) != 0 && caster.IsEnemy(target))
					{
						list.Add(target);
					}
					break;
				case ESkillTargetType.TYPE_SkillRunner:
					if ((m_targetMask & 8) != 0 && !caster.IsEnemy(target) && target != caster)
					{
						list.Add(target);
					}
					else if ((m_targetMask & 1) != 0 && caster.IsEnemy(target))
					{
						list.Add(target);
					}
					else if ((m_targetMask & 4) != 0 && !caster.IsEnemy(target))
					{
						list.Add(target);
					}
					break;
				}
			}
		}
		else
		{
			for (int i = 0; i < m_scopeOfSkill.Count; i++)
			{
				List<ISkillTarget> targetlistInScope = caster.GetTargetlistInScope(m_scopeOfSkill[i], m_targetMask, target);
				if (targetlistInScope != null)
				{
					list.AddRange(targetlistInScope);
				}
			}
		}
		return list;
	}

	public bool StopGuidOrNot(List<ISkillTarget> targetList)
	{
		foreach (ISkillTarget target in targetList)
		{
			if (!(target is INaturalResTarget naturalResTarget) || !naturalResTarget.IsDestroyed())
			{
				return false;
			}
		}
		return true;
	}

	public IEnumerator Exec(SkillRunner caster, ISkillTarget target, EffSkillInstance inst)
	{
		caster.m_effSkillInsts.Add(inst);
		float timeStart = Time.time;
		inst.m_section = EffSkillInstance.EffSection.Start;
		if (m_prepInfo != null)
		{
			m_prepInfo.Prepare(caster, target);
			if (m_prepInfo.m_DelayR > 0f)
			{
				yield return new WaitForSeconds(m_prepInfo.m_DelayR);
			}
			if (m_prepInfo.m_ReadySound != null && m_prepInfo.m_ReadySound.Count > 0)
			{
				int index = UnityEngine.Random.Range(0, 100) % m_prepInfo.m_ReadySound.Count;
				caster.ApplySound(m_prepInfo.m_ReadySound[index]);
			}
			if (m_prepInfo.m_timeCost > float.Epsilon)
			{
				yield return new WaitForSeconds(m_prepInfo.m_timeCost - m_prepInfo.m_DelayR);
			}
		}
		inst.m_section = EffSkillInstance.EffSection.Running;
		inst.mSkillCostTimeAdd = false;
		if (m_guidInfo.m_timeInterval > float.Epsilon)
		{
			int skillCostTime = (int)(m_guidInfo.m_timeCost / m_guidInfo.m_timeInterval);
			float timeInterval = m_guidInfo.m_timeInterval;
			for (int i = 0; i < skillCostTime; i++)
			{
				bool finish = false;
				if (m_guidInfo.m_animNameList != null)
				{
					caster.ApplyAnim(m_guidInfo.m_animNameList);
				}
				if (m_guidInfo.m_DelayTime <= m_guidInfo.m_SoundDelayTime)
				{
					if (m_guidInfo.m_DelayTime > 0f && m_guidInfo.m_DelayTime <= m_guidInfo.m_timeInterval)
					{
						yield return new WaitForSeconds(m_guidInfo.m_DelayTime);
					}
					if (m_guidInfo.m_GuidanceSound != null && m_guidInfo.m_GuidanceSound.Count > 0)
					{
						int index2 = UnityEngine.Random.Range(0, 100) % m_guidInfo.m_GuidanceSound.Count;
						caster.ApplySound(m_guidInfo.m_GuidanceSound[index2]);
					}
					List<ISkillTarget> targetList = GetTargetList(caster, target);
					if (m_guidInfo.m_effIdList != null)
					{
						caster.ApplyEffect(m_guidInfo.m_effIdList, target);
					}
					if (m_guidInfo.m_TargetEffIDList != null)
					{
						for (int j = 0; j < targetList.Count; j++)
						{
							for (int k = 0; k < m_guidInfo.m_TargetEffIDList.Count; k++)
							{
								if (m_guidInfo.m_TargetEffIDList[k] != 0)
								{
									EffectManager.Instance.Instantiate(m_guidInfo.m_TargetEffIDList[k], targetList[j].GetPosition(), Quaternion.identity);
								}
							}
						}
					}
					if (m_guidInfo.m_SoundDelayTime > m_guidInfo.m_DelayTime)
					{
						yield return new WaitForSeconds(m_guidInfo.m_SoundDelayTime - m_guidInfo.m_DelayTime);
					}
					if (m_itemCast != null && m_itemCast.m_itemId > 0)
					{
						m_itemCast.Cast(caster, target);
					}
					finish = m_guidInfo.TakeEffect(caster, targetList, m_id);
					if (timeInterval > m_guidInfo.m_SoundDelayTime)
					{
						yield return new WaitForSeconds(timeInterval - m_guidInfo.m_SoundDelayTime);
					}
				}
				else
				{
					if (m_guidInfo.m_effIdList != null)
					{
						caster.ApplyEffect(m_guidInfo.m_effIdList, target);
					}
					List<ISkillTarget> targetList2 = GetTargetList(caster, target);
					if (m_guidInfo.m_SoundDelayTime > 0f)
					{
						yield return new WaitForSeconds(m_guidInfo.m_SoundDelayTime);
					}
					if (m_guidInfo.m_GuidanceSound != null && m_guidInfo.m_GuidanceSound.Count > 0)
					{
						int index3 = UnityEngine.Random.Range(0, 100) % m_guidInfo.m_GuidanceSound.Count;
						caster.ApplySound(m_guidInfo.m_GuidanceSound[index3]);
					}
					if (m_guidInfo.m_DelayTime > 0f && m_guidInfo.m_DelayTime <= m_guidInfo.m_timeInterval)
					{
						yield return new WaitForSeconds(m_guidInfo.m_DelayTime - m_guidInfo.m_SoundDelayTime);
					}
					if (m_itemCast != null && m_itemCast.m_itemId > 0)
					{
						m_itemCast.Cast(caster, target);
					}
					finish = m_guidInfo.TakeEffect(caster, targetList2, m_id);
					if (timeInterval > m_guidInfo.m_DelayTime)
					{
						yield return new WaitForSeconds(timeInterval - m_guidInfo.m_DelayTime);
					}
				}
				if (inst.mSkillCostTimeAdd)
				{
					skillCostTime = i + 2;
					inst.mSkillCostTimeAdd = false;
				}
				if (finish && inst.mNextTarget == null)
				{
					break;
				}
				if (inst.mNextTarget != null)
				{
					if (!(inst.mNextTarget is VFTerrainTarget terrainTaget))
					{
						inst.mNextTarget = null;
						break;
					}
					if (VFVoxelTerrain.self.Voxels.SafeRead(terrainTaget.m_intPos.x, terrainTaget.m_intPos.y, terrainTaget.m_intPos.z).Volume <= 127)
					{
						inst.mNextTarget = null;
						break;
					}
					target = inst.mNextTarget;
					inst.mNextTarget = null;
				}
			}
		}
		else
		{
			if (m_itemCast != null && m_itemCast.m_itemId > 0)
			{
				m_itemCast.Cast(caster, target);
			}
			List<ISkillTarget> targetList3 = GetTargetList(caster, target);
			m_guidInfo.TakeEffect(caster, targetList3, m_id);
			if (m_guidInfo.m_GuidanceSound != null && m_guidInfo.m_GuidanceSound.Count > 0)
			{
				int index4 = UnityEngine.Random.Range(0, 100) % m_guidInfo.m_GuidanceSound.Count;
				caster.ApplySound(m_guidInfo.m_GuidanceSound[index4]);
			}
			if (m_guidInfo.m_effIdList != null)
			{
				caster.ApplyEffect(m_guidInfo.m_effIdList, target);
			}
		}
		if (!GameConfig.IsMultiMode)
		{
			if (m_itemsGot != null)
			{
				ItemPackage pack = caster.GetItemPackage();
				if (pack != null)
				{
					m_itemsGot.PutIntoPack(pack);
				}
			}
			if (m_skillIdsGot != null)
			{
				caster.ApplyLearnSkill(m_skillIdsGot);
			}
			if (m_metalScanID != null)
			{
				caster.ApplyMetalScan(m_metalScanID);
			}
		}
		if (m_endAction != "0")
		{
			caster.ApplyAnim(new List<string> { m_endAction });
		}
		if (m_endAniTime > float.Epsilon)
		{
			yield return new WaitForSeconds(m_endAniTime);
		}
		inst.m_section = EffSkillInstance.EffSection.Completed;
		if (Time.time < timeStart + m_cdInfo.m_timeCost)
		{
			yield return new WaitForSeconds(timeStart + m_cdInfo.m_timeCost - Time.time);
		}
		caster.m_effSkillInsts.Remove(inst);
	}

	public IEnumerator ExecProxy(SkillRunner caster, ISkillTarget target, EffSkillInstance inst)
	{
		caster.m_effSkillInsts.Add(inst);
		float timeStart = Time.time;
		inst.m_section = EffSkillInstance.EffSection.Start;
		if (m_prepInfo != null)
		{
			m_prepInfo.Prepare(caster, target);
			if (m_prepInfo.m_DelayR > 0f)
			{
				yield return new WaitForSeconds(m_prepInfo.m_DelayR);
			}
			if (m_prepInfo.m_ReadySound != null && m_prepInfo.m_ReadySound.Count > 0)
			{
				int index = UnityEngine.Random.Range(0, 100) % m_prepInfo.m_ReadySound.Count;
				caster.ApplySound(m_prepInfo.m_ReadySound[index]);
			}
			if (m_prepInfo.m_timeCost > float.Epsilon)
			{
				yield return new WaitForSeconds(m_prepInfo.m_timeCost - m_prepInfo.m_DelayR);
			}
		}
		inst.m_section = EffSkillInstance.EffSection.Running;
		inst.mSkillCostTimeAdd = false;
		bool bAffectCaster = (m_targetMask & ESkillTarget.Type2Mask(ESkillTargetType.TYPE_SkillRunner)) == 0;
		if (m_guidInfo.m_timeInterval > float.Epsilon)
		{
			int skillCostTime = (int)(m_guidInfo.m_timeCost / m_guidInfo.m_timeInterval);
			float timeInterval = m_guidInfo.m_timeInterval;
			for (int i = 0; i < skillCostTime; i++)
			{
				bool finish = false;
				if (m_guidInfo.m_animNameList != null)
				{
					caster.ApplyAnim(m_guidInfo.m_animNameList);
				}
				if (m_guidInfo.m_DelayTime <= m_guidInfo.m_SoundDelayTime)
				{
					if (m_guidInfo.m_DelayTime > 0f && m_guidInfo.m_DelayTime <= m_guidInfo.m_timeInterval)
					{
						yield return new WaitForSeconds(m_guidInfo.m_DelayTime);
					}
					if (m_guidInfo.m_GuidanceSound != null && m_guidInfo.m_GuidanceSound.Count > 0)
					{
						int index2 = UnityEngine.Random.Range(0, 100) % m_guidInfo.m_GuidanceSound.Count;
						caster.ApplySound(m_guidInfo.m_GuidanceSound[index2]);
					}
					List<ISkillTarget> targetList = GetTargetList(caster, target);
					if (m_guidInfo.m_effIdList != null)
					{
						caster.ApplyEffect(m_guidInfo.m_effIdList, target);
					}
					if (m_guidInfo.m_TargetEffIDList != null)
					{
						for (int j = 0; j < targetList.Count; j++)
						{
							for (int k = 0; k < m_guidInfo.m_TargetEffIDList.Count; k++)
							{
								if (m_guidInfo.m_TargetEffIDList[k] != 0)
								{
									EffectManager.Instance.Instantiate(m_guidInfo.m_TargetEffIDList[k], targetList[j].GetPosition(), Quaternion.identity);
								}
							}
						}
					}
					if (m_guidInfo.m_SoundDelayTime > m_guidInfo.m_DelayTime)
					{
						yield return new WaitForSeconds(m_guidInfo.m_SoundDelayTime - m_guidInfo.m_DelayTime);
					}
					if (m_itemCast != null && m_itemCast.m_itemId > 0)
					{
						m_itemCast.Cast(caster, target);
					}
					finish = m_guidInfo.TakeEffectProxy(caster, targetList, m_id, bAffectCaster);
					if (timeInterval > m_guidInfo.m_SoundDelayTime)
					{
						yield return new WaitForSeconds(timeInterval - m_guidInfo.m_SoundDelayTime);
					}
				}
				else
				{
					if (m_guidInfo.m_effIdList != null)
					{
						caster.ApplyEffect(m_guidInfo.m_effIdList, target);
					}
					List<ISkillTarget> targetList2 = GetTargetList(caster, target);
					if (m_guidInfo.m_SoundDelayTime > 0f)
					{
						yield return new WaitForSeconds(m_guidInfo.m_SoundDelayTime);
					}
					if (m_guidInfo.m_GuidanceSound != null && m_guidInfo.m_GuidanceSound.Count > 0)
					{
						int index3 = UnityEngine.Random.Range(0, 100) % m_guidInfo.m_GuidanceSound.Count;
						caster.ApplySound(m_guidInfo.m_GuidanceSound[index3]);
					}
					if (m_guidInfo.m_DelayTime > 0f && m_guidInfo.m_DelayTime <= m_guidInfo.m_timeInterval)
					{
						yield return new WaitForSeconds(m_guidInfo.m_DelayTime - m_guidInfo.m_SoundDelayTime);
					}
					if (m_itemCast != null && m_itemCast.m_itemId > 0)
					{
						m_itemCast.Cast(caster, target);
					}
					finish = m_guidInfo.TakeEffectProxy(caster, targetList2, m_id, bAffectCaster);
					if (timeInterval > m_guidInfo.m_DelayTime)
					{
						yield return new WaitForSeconds(timeInterval - m_guidInfo.m_DelayTime);
					}
				}
				if (inst.mSkillCostTimeAdd)
				{
					skillCostTime = i + 2;
					inst.mSkillCostTimeAdd = false;
				}
				if (finish)
				{
					if (inst.mNextTarget == null)
					{
						break;
					}
					if (!(inst.mNextTarget is VFTerrainTarget terrainTaget))
					{
						inst.mNextTarget = null;
						break;
					}
					if (VFVoxelTerrain.self.Voxels.SafeRead(terrainTaget.m_intPos.x, terrainTaget.m_intPos.y, terrainTaget.m_intPos.z).Volume <= 127)
					{
						inst.mNextTarget = null;
						break;
					}
					target = inst.mNextTarget;
					inst.mNextTarget = null;
				}
			}
		}
		else
		{
			if (m_itemCast != null && m_itemCast.m_itemId > 0)
			{
				m_itemCast.Cast(caster, target);
			}
			List<ISkillTarget> targetList3 = GetTargetList(caster, target);
			m_guidInfo.TakeEffectProxy(caster, targetList3, m_id, bAffectCaster);
			if (m_guidInfo.m_GuidanceSound != null && m_guidInfo.m_GuidanceSound.Count > 0)
			{
				int index4 = UnityEngine.Random.Range(0, 100) % m_guidInfo.m_GuidanceSound.Count;
				caster.ApplySound(m_guidInfo.m_GuidanceSound[index4]);
			}
			if (m_guidInfo.m_effIdList != null)
			{
				caster.ApplyEffect(m_guidInfo.m_effIdList, target);
			}
		}
		if (m_endAction != "0")
		{
			caster.ApplyAnim(new List<string> { m_endAction });
		}
		if (m_endAniTime > float.Epsilon)
		{
			yield return new WaitForSeconds(m_endAniTime);
		}
		inst.m_section = EffSkillInstance.EffSection.Completed;
		if (Time.time < timeStart + m_cdInfo.m_timeCost)
		{
			yield return new WaitForSeconds(timeStart + m_cdInfo.m_timeCost - Time.time);
		}
		caster.m_effSkillInsts.Remove(inst);
	}

	public IEnumerator SkipExec(SkillRunner caster, EffSkillInstance inst)
	{
		caster.m_effSkillInsts.Add(inst);
		float timeStart = Time.time;
		if (Time.time < timeStart + m_cdInfo.m_timeCost)
		{
			yield return new WaitForSeconds(timeStart + m_cdInfo.m_timeCost - Time.time);
		}
		caster.m_effSkillInsts.Remove(inst);
	}

	public IEnumerator SharingCooling(SkillRunner caster, EffSkillInstance inst)
	{
		if (!(m_cdInfo.m_timeShared <= 0f))
		{
			caster.m_effShareSkillInsts.Add(inst);
			yield return new WaitForSeconds(m_cdInfo.m_timeShared);
			caster.m_effShareSkillInsts.Remove(inst);
		}
	}

	public static void LoadData()
	{
		if (s_tblEffSkills == null)
		{
			if (EffSkillBuff.s_tblEffSkillBuffs == null)
			{
				EffSkillBuff.LoadData();
			}
			SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("skill");
			int fieldCount = sqliteDataReader.FieldCount;
			s_tblEffSkillsColName_CN = new List<string>(fieldCount);
			sqliteDataReader.Read();
			for (int i = 0; i < fieldCount; i++)
			{
				s_tblEffSkillsColName_CN.Add(sqliteDataReader.GetString(i));
			}
			s_tblEffSkillsColName_EN = new List<string>(fieldCount);
			sqliteDataReader.Read();
			for (int j = 0; j < fieldCount; j++)
			{
				s_tblEffSkillsColName_EN.Add(sqliteDataReader.GetString(j));
			}
			s_tblEffSkills = new List<EffSkill>();
			while (sqliteDataReader.Read())
			{
				EffSkill effSkill = new EffSkill();
				effSkill.m_id = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_id")));
				effSkill.m_name[0] = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_name"));
				effSkill.m_name[1] = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_engname"));
				effSkill.m_type = Convert.ToInt16(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_type")));
				effSkill.m_iconImgPath = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_icon"));
				effSkill.m_desc[0] = PELocalization.GetString(Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_desc"))));
				effSkill.m_desc[1] = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_engdesc"));
				effSkill.m_interruptable = ((Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_interruptable"))) != 0) ? true : false);
				effSkill.m_targetMask = ToBitMask(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_target")));
				effSkill.m_distOfSkill = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_distCast")));
				effSkill.m_scopeOfSkill = EffScope.Create(sqliteDataReader);
				effSkill.m_cdInfo = EffCoolDownInfo.Create(sqliteDataReader);
				effSkill.m_prepInfo = EffPrepareInfo.Create(sqliteDataReader);
				effSkill.m_itemCast = EffItemCast.Create(sqliteDataReader, effSkill.m_id);
				effSkill.m_guidInfo = EffGuidanceInfo.Create(sqliteDataReader);
				effSkill.m_itemsGot = EffItemGot.Create(sqliteDataReader);
				effSkill.m_skillIdsGot = ToListInt32P(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_learnSkill")));
				effSkill.m_metalScanID = ToListInt32P(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_mineupdate")));
				effSkill.m_endAction = sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_endAction"));
				effSkill.m_endAniTime = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_endTime")));
				s_tblEffSkills.Add(effSkill);
			}
		}
	}

	public static void AddWeaponSkill(int id, string name, int buffID)
	{
		EffSkill effSkill = new EffSkill();
		effSkill.m_id = id;
		effSkill.m_name[0] = name;
		effSkill.m_name[1] = name;
		effSkill.m_type = 4;
		effSkill.m_iconImgPath = string.Empty;
		effSkill.m_desc[0] = "0";
		effSkill.m_desc[1] = "0";
		effSkill.m_interruptable = false;
		effSkill.m_targetMask = ToBitMask("3");
		effSkill.m_distOfSkill = 0f;
		effSkill.m_scopeOfSkill = null;
		effSkill.m_cdInfo = new EffCoolDownInfo();
		effSkill.m_prepInfo = null;
		effSkill.m_itemCast = null;
		effSkill.m_guidInfo = new EffGuidanceInfo(buffID);
		effSkill.m_itemsGot = null;
		effSkill.m_skillIdsGot = null;
		effSkill.m_endAction = "0";
		s_tblEffSkills.Add(effSkill);
	}

	public static bool MatchId(EffSkill iter, int id)
	{
		return iter.m_id == id;
	}

	public static int ToBitMask(string desc)
	{
		string[] array = desc.Split(',');
		int num = 0;
		for (int i = 0; i < array.Length; i++)
		{
			byte b = Convert.ToByte(array[i]);
			if (b > 0)
			{
				num |= 1 << ((b - 1) & 0x1F);
			}
		}
		return num;
	}

	public static List<byte> ToListByteP(string desc)
	{
		string[] array = desc.Split(',');
		byte b = Convert.ToByte(array[0]);
		if (b <= 0)
		{
			return null;
		}
		List<byte> list = new List<byte>();
		list.Add(b);
		for (int i = 1; i < array.Length; i++)
		{
			list.Add(Convert.ToByte(array[i]));
		}
		return list;
	}

	public static List<int> ToListInt32P(string desc)
	{
		string[] array = desc.Split(',');
		int num = Convert.ToInt32(array[0]);
		if (num <= 0)
		{
			return null;
		}
		List<int> list = new List<int>();
		list.Add(num);
		for (int i = 1; i < array.Length; i++)
		{
			list.Add(Convert.ToInt32(array[i]));
		}
		return list;
	}

	public static List<string> ToListString(string desc)
	{
		string[] array = desc.Split(',');
		if (array[0].CompareTo("0") == 0)
		{
			return null;
		}
		return new List<string>(array);
	}
}
