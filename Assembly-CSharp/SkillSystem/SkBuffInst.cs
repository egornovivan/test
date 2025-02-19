using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkillSystem;

public class SkBuffInst : SkRuntimeInfo
{
	internal SkBuff _buff;

	internal ISkPara _para;

	internal ISkAttribs _paraCaster;

	internal ISkAttribs _paraTarget;

	internal int _n;

	internal float _startTime;

	public override ISkPara Para => _para;

	public override SkEntity Caster => (_paraCaster == null) ? null : (_paraCaster.entity as SkEntity);

	public override SkEntity Target => (_paraTarget == null) ? null : (_paraTarget.entity as SkEntity);

	public static SkBuffInst Create(int buffId, ISkAttribs paraCaster = null, ISkAttribs paraTarget = null)
	{
		if (SkBuff.s_SkBuffTbl.TryGetValue(buffId, out var value))
		{
			SkBuffInst skBuffInst = new SkBuffInst();
			skBuffInst._buff = value;
			SkEntity skEntity = ((SkRuntimeInfo.Current == null) ? null : SkRuntimeInfo.Current.Caster);
			ISkAttribs paraCaster2;
			if (paraCaster == null && skEntity != null)
			{
				ISkAttribs attribs = skEntity.attribs;
				paraCaster2 = attribs;
			}
			else
			{
				paraCaster2 = paraCaster;
			}
			skBuffInst._paraCaster = paraCaster2;
			skBuffInst._paraTarget = paraTarget;
			skBuffInst._startTime = Time.time;
			return skBuffInst;
		}
		return null;
	}

	public static SkBuffInst Mount(SkPackage buffPak, SkBuffInst inst)
	{
		if (inst == null)
		{
			return null;
		}
		int id = inst._buff._id;
		int type = inst._buff._type;
		int prior = inst._buff._priority;
		int stackLimit = inst._buff._stackLimit;
		List<SkBuffInst> list = buffPak._buffs.FindAll((SkBuffInst it) => it._buff._id == id);
		if (list.Count == 0)
		{
			if (buffPak._buffs.Exists((SkBuffInst it) => it._buff._type == type && it._buff._priority > prior))
			{
				return null;
			}
			Unmount(buffPak, (SkBuffInst it) => it._buff._type == type && it._buff._priority < prior);
		}
		else if (list.Count >= stackLimit)
		{
			Unmount(buffPak, list[0]);
		}
		buffPak._buffs.Add(inst);
		return inst;
	}

	public static void Unmount(SkPackage buffPak, SkBuffInst inst, bool force = true)
	{
		if (inst.OnDiscard(buffPak._parentAttribs) || force)
		{
			buffPak._buffs.Remove(inst);
		}
	}

	public static void Unmount(SkPackage buffPak, Predicate<SkBuffInst> match)
	{
		for (int num = buffPak._buffs.Count - 1; num >= 0; num--)
		{
			if (match(buffPak._buffs[num]))
			{
				Unmount(buffPak, buffPak._buffs[num]);
			}
		}
	}

	public static SkBuffInst GetBuff(SkPackage buffPak, Func<SkBuffInst, bool> match)
	{
		return buffPak._buffs.Find((SkBuffInst inst) => match(inst));
	}

	public bool Exec(ISkAttribs dst)
	{
		bool result = true;
		SkRuntimeInfo.Current = this;
		if (_n == 0)
		{
			if (_buff._effBeg != null)
			{
				_buff._effBeg.Apply(Target, this);
			}
			if (_buff._eff != null)
			{
				_buff._eff.Apply(Target, this);
			}
			if (_buff._mods != null)
			{
				_buff._mods.Exec(dst, _paraCaster, _paraTarget, _para as ISkAttribsModPara);
			}
			_n++;
		}
		float num = Time.time - _startTime;
		if (_buff._interval > float.Epsilon && num > (float)_n * _buff._interval)
		{
			if (_buff._eff != null)
			{
				_buff._eff.Apply(Target, this);
			}
			if (_buff._mods != null)
			{
				_buff._mods.Exec(dst, _paraCaster, _paraTarget, _para as ISkAttribsModPara);
			}
			_n++;
		}
		if (_buff._lifeTime > -1E-45f && num > _buff._lifeTime)
		{
			result = false;
		}
		SkRuntimeInfo.Current = null;
		return result;
	}

	public void TryExecTmp(ISkAttribs dst, int idxToMod)
	{
		if (_buff._mods != null)
		{
			_buff._mods.TryExecTmp(dst, _paraCaster, _paraTarget, _para as ISkAttribsModPara, idxToMod, _n);
		}
	}

	public bool OnDiscard(ISkAttribs dst)
	{
		if (_buff._mods != null)
		{
			_buff._mods.ReqExecTmp(dst);
		}
		if (_buff._effEnd != null)
		{
			_buff._effEnd.Apply(Target, this);
		}
		return true;
	}

	public bool MatchID(int id)
	{
		return _buff._id == id;
	}
}
