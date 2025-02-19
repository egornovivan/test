using System;
using System.Collections;
using UnityEngine;

namespace SkillSystem;

public class WaitTimeSkippable : IEnumerator
{
	private float _statTime;

	private float _timeToWait;

	private Func<bool> _deleCheckSkip;

	public object Current => null;

	public WaitTimeSkippable(float timeToWait, Func<bool> checkToSkip)
	{
		_statTime = Time.time;
		_timeToWait = timeToWait;
		_deleCheckSkip = checkToSkip;
	}

	public bool MoveNext()
	{
		return Time.time - _statTime < _timeToWait && !_deleCheckSkip();
	}

	public void Reset()
	{
		_statTime = Time.time;
	}
}
