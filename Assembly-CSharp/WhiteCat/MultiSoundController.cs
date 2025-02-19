using System;
using Pathea;
using UnityEngine;

namespace WhiteCat;

public class MultiSoundController : StatesBehaviour
{
	[SerializeField]
	private AudioState _turnon;

	[SerializeField]
	private AudioState _standby;

	[SerializeField]
	private AudioState _speedup;

	[SerializeField]
	private AudioState _running;

	[SerializeField]
	private AudioState _slowdown;

	private float _input;

	private bool _acc;

	private bool _hasDriver;

	public void UpdateSound(float input, bool hasDriverAndEnergy)
	{
		if (Mathf.Sign(input) != Mathf.Sign(_input))
		{
			_acc = false;
			_input = input;
		}
		else if (_acc)
		{
			_acc = Mathf.Abs(input) - Mathf.Abs(_input) > -0.1f;
			if (!_acc)
			{
				_input = input;
			}
		}
		else
		{
			_acc = Mathf.Abs(input) - Mathf.Abs(_input) > 0.1f;
			if (_acc)
			{
				_input = input;
			}
		}
		if (_hasDriver != hasDriverAndEnergy)
		{
			_hasDriver = hasDriverAndEnergy;
			if (hasDriverAndEnergy)
			{
				_turnon.source.volume = 0f;
				base.state = _turnon;
			}
			else
			{
				base.state = _standby;
			}
		}
	}

	private void Awake()
	{
		_turnon.Init();
		_standby.Init();
		_speedup.Init();
		_running.Init();
		_slowdown.Init();
		PeGameMgr.PasueEvent = (GamePasue)Delegate.Combine(PeGameMgr.PasueEvent, new GamePasue(OnGamePause));
		_turnon.onUpdate = delegate
		{
			_turnon.source.volume = _turnon.source.time / _turnon.source.clip.length * SystemSettingData.Instance.AbsEffectVolume * _turnon.maxVolume;
			if (_turnon.source.clip.length - _turnon.source.time < Time.fixedDeltaTime || !_turnon.source.isPlaying)
			{
				base.state = _standby;
			}
		};
		_standby.onUpdate = delegate(float t)
		{
			if (_hasDriver)
			{
				_standby.source.volume += t * SystemSettingData.Instance.AbsEffectVolume * _standby.maxVolume;
			}
			else
			{
				_standby.source.volume -= t * SystemSettingData.Instance.AbsEffectVolume * _standby.maxVolume;
			}
			_standby.source.volume = Mathf.Clamp(_standby.source.volume, 0f, SystemSettingData.Instance.AbsEffectVolume * _standby.maxVolume);
			if (_hasDriver && _acc)
			{
				base.state = _speedup;
			}
		};
		_speedup.onUpdate = delegate(float t)
		{
			_speedup.source.volume = SystemSettingData.Instance.AbsEffectVolume * _speedup.maxVolume;
			if (_acc)
			{
				if (_speedup.source.clip.length - _speedup.source.time < Time.fixedDeltaTime || !_speedup.source.isPlaying)
				{
					base.state = _running;
				}
			}
			else
			{
				t = 1f - _speedup.source.time / _speedup.source.clip.length;
				base.state = _slowdown;
				_slowdown.source.timeSamples = Mathf.Clamp((int)(t * (float)_slowdown.source.clip.samples), 0, _slowdown.source.clip.samples - 1);
			}
		};
		_running.onUpdate = delegate
		{
			_running.source.volume = SystemSettingData.Instance.AbsEffectVolume * _running.maxVolume;
			if (!_acc)
			{
				base.state = _slowdown;
			}
		};
		_slowdown.onUpdate = delegate(float t)
		{
			_slowdown.source.volume = SystemSettingData.Instance.AbsEffectVolume * _slowdown.maxVolume;
			if (!_acc)
			{
				if (_slowdown.source.clip.length - _slowdown.source.time < Time.fixedDeltaTime || !_slowdown.source.isPlaying)
				{
					base.state = _standby;
				}
			}
			else
			{
				t = 1f - _slowdown.source.time / _slowdown.source.clip.length;
				base.state = _speedup;
				_speedup.source.timeSamples = Mathf.Clamp((int)(t * (float)_speedup.source.clip.samples), 0, _speedup.source.clip.samples - 1);
			}
		};
		base.enabled = true;
	}

	private void OnGamePause(bool pause)
	{
		if (pause)
		{
			_turnon.source.Pause();
			_standby.source.Pause();
			_speedup.source.Pause();
			_running.source.Pause();
			_slowdown.source.Pause();
		}
		else
		{
			_turnon.source.UnPause();
			_standby.source.UnPause();
			_speedup.source.UnPause();
			_running.source.UnPause();
			_slowdown.source.UnPause();
		}
	}

	private void OnDestroy()
	{
		PeGameMgr.PasueEvent = (GamePasue)Delegate.Remove(PeGameMgr.PasueEvent, new GamePasue(OnGamePause));
	}

	private void FixedUpdate()
	{
		UpdateState(Time.deltaTime);
	}
}
