using System;
using System.Collections.Generic;
using Mono.Data.SqliteClient;
using UnityEngine;

namespace SkillSystem;

public class SkTriggerEvent
{
	internal int _id;

	internal SkCond _cond;

	internal float _force;

	internal SkAttribsModifier _modsCaster;

	internal SkAttribsModifier _modsTarget;

	internal SkEffect _effOnHitCaster;

	internal SkEffect _effOnHitTarget;

	internal static Dictionary<int, SkTriggerEvent> s_SkTriggerEventTbl;

	public static void LoadData()
	{
		if (s_SkTriggerEventTbl != null)
		{
			return;
		}
		SqliteDataReader sqliteDataReader = LocalDatabase.Instance.ReadFullTable("skTriggerEvent");
		s_SkTriggerEventTbl = new Dictionary<int, SkTriggerEvent>();
		while (sqliteDataReader.Read())
		{
			SkTriggerEvent skTriggerEvent = new SkTriggerEvent();
			skTriggerEvent._id = Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_id")));
			skTriggerEvent._cond = SkCond.Create(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_cond")));
			skTriggerEvent._force = Convert.ToSingle(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_force")));
			skTriggerEvent._modsCaster = SkAttribsModifier.Create(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_modsCaster")));
			skTriggerEvent._modsTarget = SkAttribsModifier.Create(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_modsTarget")));
			SkEffect.s_SkEffectTbl.TryGetValue(Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_effOnHitCaster"))), out skTriggerEvent._effOnHitCaster);
			SkEffect.s_SkEffectTbl.TryGetValue(Convert.ToInt32(sqliteDataReader.GetString(sqliteDataReader.GetOrdinal("_effOnHitTarget"))), out skTriggerEvent._effOnHitTarget);
			try
			{
				s_SkTriggerEventTbl.Add(skTriggerEvent._id, skTriggerEvent);
			}
			catch (Exception ex)
			{
				Debug.LogError("Exception on skTriggerEvent " + skTriggerEvent._id + " " + ex);
			}
		}
	}

	public bool Exec(SkInst inst)
	{
		bool result = false;
		SkRuntimeInfo.Current = inst;
		if (_cond.Tst(inst))
		{
			inst._forceMagnitude = _force;
			if (inst._target != null)
			{
				if (_modsCaster != null)
				{
					_modsCaster.Exec(inst._caster.attribs, inst._caster.attribs, inst._target.attribs, inst._para as ISkAttribsModPara);
				}
				if (_modsTarget != null)
				{
					_modsTarget.Exec(inst._target.attribs, inst._caster.attribs, inst._target.attribs, inst._para as ISkAttribsModPara);
				}
				if (_force > 0f)
				{
					inst._forceDirection = inst.GetForceVec();
				}
				if (_effOnHitCaster != null)
				{
					_effOnHitCaster.Apply(inst._caster, inst);
				}
				if (_effOnHitTarget != null)
				{
					_effOnHitTarget.Apply(inst._target, inst);
				}
			}
			else
			{
				inst._tmpTar = null;
				if (_effOnHitCaster != null)
				{
					_effOnHitCaster.Apply(inst._caster, inst);
				}
				if (_cond._retTars != null && _cond._retTars.Count > 0)
				{
					foreach (SkEntity retTar in _cond._retTars)
					{
						SkEntity skEntity = (inst._tmpTar = retTar);
						if (_modsCaster != null)
						{
							_modsCaster.Exec(inst._caster.attribs, inst._caster.attribs, skEntity.attribs, inst._para as ISkAttribsModPara);
						}
						if (_modsTarget != null)
						{
							_modsTarget.Exec(skEntity.attribs, inst._caster.attribs, skEntity.attribs, inst._para as ISkAttribsModPara);
						}
						if (_effOnHitTarget != null)
						{
							if (_force > 0f)
							{
								inst._forceDirection = inst.GetForceVec();
							}
							_effOnHitTarget.Apply(skEntity, inst);
						}
					}
				}
				else if (_modsCaster != null)
				{
					_modsCaster.Exec(inst._caster.attribs, inst._caster.attribs, null, inst._para as ISkAttribsModPara);
				}
			}
			result = true;
		}
		SkRuntimeInfo.Current = null;
		return result;
	}
}
