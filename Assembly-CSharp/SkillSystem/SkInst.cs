using System;
using System.Collections;
using System.Collections.Generic;
using Pathea;
using UnityEngine;

namespace SkillSystem;

public class SkInst : SkRuntimeInfo
{
	public const int StepPrepare = 0;

	public const int StepEnding = -1;

	public const int StepCooling = -2;

	public const int StepEnded = -99;

	public const int StepNotStart = -99;

	public const int CoolNotStart = 0;

	public const int CoolThis = 1;

	public const int CoolShare = 2;

	public static System.Random s_rand = new System.Random(0);

	public static IExpCompiler s_ExpCompiler = new SkExpEvaluator();

	internal ISkPara _para;

	internal SkEntity _caster;

	internal SkEntity _target;

	internal SkEntity _tmpTar;

	internal SkData _skData;

	internal int _step = -99;

	internal int _coolStat;

	internal List<SkEntity> _hitInGuide = new List<SkEntity>();

	internal float _startTime = -1f;

	internal CoroutineStoppable _coroutine;

	private bool _bSkipPreTime;

	private bool _bSkipMainTime;

	private bool _bSkipPostTime;

	internal bool _bExecExtEnable;

	internal List<SkTriggerEvent> _eventsActFromCol = new List<SkTriggerEvent>();

	internal List<SkTriggerEvent> _eventsActInGuide = new List<SkTriggerEvent>();

	internal PECapsuleHitResult _colInfo;

	public float _forceMagnitude = -1f;

	public Vector3 _forceDirection = Vector3.zero;

	public bool LastHit => _hitInGuide.Count > 0;

	public int GuideCnt => _step;

	public int SkillID => _skData._id;

	public bool IsActive => _step >= -1;

	public bool SkipWaitAll
	{
		set
		{
			_bSkipPreTime = (_bSkipMainTime = (_bSkipPostTime = value));
		}
	}

	public bool SkipWaitPre
	{
		get
		{
			return _bSkipPreTime;
		}
		set
		{
			_bSkipPreTime = value;
		}
	}

	public bool SkipWaitMain
	{
		get
		{
			return _bSkipMainTime;
		}
		set
		{
			_bSkipMainTime = value;
		}
	}

	public bool SkipWaitPost
	{
		get
		{
			return _bSkipPostTime;
		}
		set
		{
			_bSkipPostTime = value;
		}
	}

	public override ISkPara Para => _para;

	public override SkEntity Caster => _caster;

	public override SkEntity Target => (!(_target == null)) ? _target : _tmpTar;

	private SkInst()
	{
	}

	private bool CheckSkipPreTime()
	{
		return _bSkipPreTime;
	}

	private bool CheckSkipMainTime()
	{
		return _bSkipMainTime;
	}

	private bool CheckSkipPostTime()
	{
		return _bSkipPostTime;
	}

	private void StopImm()
	{
		_coolStat = 0;
		_step = -99;
		PeSingleton<SkInstPool>.Instance._skInsts.Remove(this);
	}

	public void Start()
	{
		if (_coroutine == null)
		{
			_coroutine = new CoroutineStoppable(_caster, Exec());
		}
	}

	public void Stop()
	{
		if (_step != -2 && _step != -99)
		{
			_coroutine.stop = true;
			_bSkipPreTime = true;
			_bSkipMainTime = true;
			_bSkipPostTime = true;
			if (_caster != null)
			{
				_caster.UnRegisterInstFromExt(this);
			}
			if (_caster != null && _caster.isActiveAndEnabled)
			{
				_caster.StartCoroutine(CoolDown());
			}
			else
			{
				StopImm();
			}
		}
	}

	private bool ClassifyTriggerEvents(int idx)
	{
		List<SkTriggerEvent> events = _skData.GetEvents(idx);
		_eventsActFromCol.Clear();
		_eventsActInGuide.Clear();
		foreach (SkTriggerEvent item in events)
		{
			if (item._cond._type == SkCond.CondType.TypeRTCol)
			{
				_eventsActFromCol.Add(item);
			}
			else if (item._cond._type == SkCond.CondType.TypeNormal)
			{
				_eventsActInGuide.Add(item);
			}
		}
		return _eventsActInGuide.Count < _skData.GetEvents(idx).Count;
	}

	private void ExecEventsInGuide()
	{
		foreach (SkTriggerEvent item in _eventsActInGuide)
		{
			item.Exec(this);
		}
	}

	public void ExecEventsFromCol(PECapsuleHitResult colInfo)
	{
		if (!_bExecExtEnable)
		{
			return;
		}
		SkEntity skEntity = (SkEntity)colInfo.hitTrans;
		if (!(skEntity != null) || _hitInGuide.Contains(skEntity) || (!(_target == null) && !(_target == skEntity)))
		{
			return;
		}
		_colInfo = colInfo;
		foreach (SkTriggerEvent item in _eventsActFromCol)
		{
			if (item.Exec(this))
			{
				_hitInGuide.Add(skEntity);
				break;
			}
		}
	}

	private IEnumerator CoolDown()
	{
		_step = -2;
		_coolStat = 0;
		float elapseTime = Time.time - _startTime;
		float coolingTime = _skData._coolingTime - elapseTime;
		float coolingTimeShared = _skData._coolingTimeShared - elapseTime;
		if (coolingTime > float.Epsilon)
		{
			_coolStat |= 1;
		}
		if (coolingTimeShared > float.Epsilon)
		{
			_coolStat |= 2;
		}
		bool bShareMin = coolingTimeShared <= coolingTime;
		float min = ((!bShareMin) ? coolingTime : coolingTimeShared);
		float max = ((!bShareMin) ? coolingTimeShared : coolingTime);
		if (min > float.Epsilon)
		{
			yield return new WaitForSeconds(min);
		}
		_coolStat &= ~((!bShareMin) ? 1 : 2);
		if (max > float.Epsilon)
		{
			float left = ((!(min > float.Epsilon)) ? max : (max - min));
			if (left > float.Epsilon)
			{
				yield return new WaitForSeconds(left);
			}
		}
		StopImm();
	}

	private IEnumerator Exec()
	{
		_startTime = Time.time;
		_step = 0;
		if (_skData._pretimeOfPrepare > float.Epsilon)
		{
			yield return new WaitForSeconds(_skData._pretimeOfPrepare);
		}
		if (_skData._effPrepare != null)
		{
			_skData._effPrepare.Apply(_caster, this);
		}
		if (_skData._postimeOfPrepare > float.Epsilon)
		{
			yield return new WaitForSeconds(_skData._postimeOfPrepare);
		}
		if (_skData._effMainOneTime != null)
		{
			_skData._effMainOneTime.Apply(_caster, this);
		}
		do
		{
			bool bExtEvents = ClassifyTriggerEvents(_step);
			_bExecExtEnable = false;
			if (bExtEvents)
			{
				_caster.RegisterInstFromExt(this);
			}
			_step++;
			_bSkipPreTime = false;
			_bSkipMainTime = false;
			_bSkipPostTime = false;
			_skData.TryApplyEachEffOfMain(_step - 1, _caster, this);
			float fPretime = _skData.GetPretimeOfMain(_step - 1);
			if (fPretime > float.Epsilon)
			{
				yield return _caster.StartCoroutine(new WaitTimeSkippable(fPretime, CheckSkipPreTime));
			}
			_hitInGuide.Clear();
			_bExecExtEnable = true;
			ExecEventsInGuide();
			float fMaintime = _skData.GetTimeOfMain(_step - 1);
			if (fMaintime > float.Epsilon)
			{
				yield return _caster.StartCoroutine(new WaitTimeSkippable(fMaintime, CheckSkipMainTime));
			}
			_bExecExtEnable = false;
			float fPostime = _skData.GetPostimeOfMain(_step - 1);
			if (fPostime > float.Epsilon)
			{
				yield return _caster.StartCoroutine(new WaitTimeSkippable(fPostime, CheckSkipPostTime));
			}
			if (bExtEvents)
			{
				_caster.UnRegisterInstFromExt(this);
			}
		}
		while (_skData._condToLoop.Tst(this));
		_step = -1;
		if (_skData._pretimeOfEnding > float.Epsilon)
		{
			yield return new WaitForSeconds(_skData._pretimeOfEnding);
		}
		if (_skData._effEnding != null)
		{
			_skData._effEnding.Apply(_caster, this);
		}
		if (_skData._postimeOfEnding > float.Epsilon)
		{
			yield return new WaitForSeconds(_skData._postimeOfEnding);
		}
		yield return _caster.StartCoroutine(CoolDown());
	}

	public int GetAtkDir()
	{
		SkEntity skEntity = ((!(_target != null)) ? _tmpTar : _target);
		if (skEntity == null || skEntity == null)
		{
			Debug.LogError("[SkInst]:Error in GetAtkDir");
			return 0;
		}
		if (_colInfo == null)
		{
			return 0;
		}
		Transform transform = skEntity.GetTransform();
		Vector3 lhs = -_colInfo.hitDir;
		Vector3 forward = transform.forward;
		float num = Vector3.Dot(lhs, forward);
		if (num > 0.866f)
		{
			return 2;
		}
		if (num < -0.866f)
		{
			return 3;
		}
		Vector3 up = transform.up;
		float num2 = Vector3.Dot(lhs, up);
		if (num2 > 0.707f)
		{
			return 4;
		}
		if (num2 < -0.707f)
		{
			return 5;
		}
		Vector3 rhs = Vector3.Cross(up, forward);
		float num3 = Vector3.Dot(lhs, rhs);
		return (!(num3 < 0f)) ? 1 : 0;
	}

	public Vector3 GetForceVec()
	{
		return (_colInfo == null) ? Vector3.zero : _colInfo.hitDir;
	}

	public Vector3 GetCollisionContactPoint()
	{
		return (_colInfo == null) ? _caster.GetTransform().position : _colInfo.hitPos;
	}

	public static SkInst StartSkill(SkEntity caster, SkEntity target, int skId, ISkPara para = null, bool bStartImm = true)
	{
		SkData value = null;
		if (!SkData.s_SkillTbl.TryGetValue(skId, out value))
		{
			Debug.LogError("[SkInst]:Invalid skill id:" + skId);
			return null;
		}
		SkCoolInfo skillCoolingInfo = GetSkillCoolingInfo(caster, value);
		if (skillCoolingInfo != null && skillCoolingInfo._fLeftTime > 0f)
		{
			return null;
		}
		SkInst skInst = new SkInst();
		PeSingleton<SkInstPool>.Instance._skInsts.Add(skInst);
		skInst._skData = value;
		skInst._para = para;
		skInst._caster = caster;
		skInst._target = target;
		skInst._coroutine = ((!bStartImm) ? null : new CoroutineStoppable(caster, skInst.Exec()));
		return skInst;
	}

	public static int StopSkill(Func<SkInst, bool> match)
	{
		List<SkInst> skInsts = PeSingleton<SkInstPool>.Instance._skInsts;
		int count = skInsts.Count;
		for (int num = count - 1; num >= 0; num--)
		{
			SkInst skInst = skInsts[num];
			if (match(skInst))
			{
				skInst.Stop();
			}
		}
		return skInsts.Count - count;
	}

	public static SkInst GetSkill(Func<SkInst, bool> match)
	{
		List<SkInst> skInsts = PeSingleton<SkInstPool>.Instance._skInsts;
		int count = skInsts.Count;
		for (int num = count - 1; num >= 0; num--)
		{
			SkInst skInst = skInsts[num];
			if (match(skInst))
			{
				return skInst;
			}
		}
		return null;
	}

	public static bool IsSkillRunnable(SkEntity caster, int skId)
	{
		SkData value = null;
		if (!SkData.s_SkillTbl.TryGetValue(skId, out value))
		{
			return false;
		}
		SkCoolInfo skillCoolingInfo = GetSkillCoolingInfo(caster, value);
		if (skillCoolingInfo != null && skillCoolingInfo._fLeftTime > 0f)
		{
			return false;
		}
		return true;
	}

	public static float GetSkillCoolingPercent(SkEntity caster, int skId)
	{
		SkData value = null;
		if (!SkData.s_SkillTbl.TryGetValue(skId, out value))
		{
			return -1f;
		}
		SkCoolInfo skillCoolingInfo = GetSkillCoolingInfo(caster, value);
		if (skillCoolingInfo != null && skillCoolingInfo._fLeftTime > 0f)
		{
			return skillCoolingInfo._fLeftTime / skillCoolingInfo._fMaxTime;
		}
		return 0f;
	}

	public static SkCoolInfo GetSkillCoolingInfo(SkEntity caster, SkData skData)
	{
		List<SkInst> skInsts = PeSingleton<SkInstPool>.Instance._skInsts;
		foreach (SkInst item in skInsts)
		{
			if (item._caster == caster)
			{
				if (item._skData._id == skData._id && item._skData._coolingTime > float.Epsilon)
				{
					SkCoolInfo skCoolInfo = new SkCoolInfo();
					skCoolInfo._bShare = false;
					skCoolInfo._fMaxTime = item._skData._coolingTime;
					skCoolInfo._fLeftTime = item._skData._coolingTime - (Time.time - item._startTime);
					return skCoolInfo;
				}
				if (item._skData._coolingTimeType == skData._coolingTimeType && item._skData._coolingTimeShared > float.Epsilon)
				{
					SkCoolInfo skCoolInfo2 = new SkCoolInfo();
					skCoolInfo2._bShare = true;
					skCoolInfo2._fMaxTime = item._skData._coolingTimeShared;
					skCoolInfo2._fLeftTime = item._skData._coolingTimeShared - (Time.time - item._startTime);
					return skCoolInfo2;
				}
			}
		}
		return null;
	}
}
