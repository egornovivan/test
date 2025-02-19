using System;
using UnityEngine;
using UnityEngine.Events;

namespace WhiteCat;

[AddComponentMenu("White Cat/Tween/Interpolator")]
public class TweenInterpolator : BaseBehaviour
{
	public TweenMethod method;

	[SerializeField]
	private AnimationCurve _customCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[GetSet("delay")]
	[SerializeField]
	private float _delay;

	[SerializeField]
	[GetSet("duration")]
	private float _duration = 1f;

	[SerializeField]
	[GetSet("speed")]
	private float _speed = 1f;

	public WrapMode wrapMode;

	public TimeLine timeLine;

	public UnityEvent onArriveAtBeginning = new UnityEvent();

	public UnityEvent onArriveAtEnding = new UnityEvent();

	private float _normalizedTime;

	private static Func<float, float>[] _interpolates = new Func<float, float>[16]
	{
		Interpolation.Linear,
		Interpolation.Square,
		Interpolation.Random,
		Interpolation.EaseIn,
		Interpolation.EaseOut,
		Interpolation.EaseInEaseOut,
		Interpolation.EaseInStrong,
		Interpolation.EaseOutStrong,
		Interpolation.EaseInEaseOutStrong,
		Interpolation.BackInEaseOut,
		Interpolation.EaseInBackOut,
		Interpolation.BackInBackOut,
		Interpolation.Triangle,
		Interpolation.Parabolic,
		Interpolation.Bell,
		Interpolation.Sine
	};

	private float _time;

	private float _deltaNormalizedTime;

	private float _normalizedTimeTarget;

	public float delay
	{
		get
		{
			return _delay;
		}
		set
		{
			_delay = ((!(value < 0f)) ? value : 0f);
		}
	}

	public float duration
	{
		get
		{
			return _duration;
		}
		set
		{
			_duration = ((!(value > 0.01f)) ? 0.01f : value);
		}
	}

	public float speed
	{
		get
		{
			return _speed;
		}
		set
		{
			_speed = Mathf.Clamp(value, -100f, 100f);
		}
	}

	public bool isPlaying
	{
		get
		{
			return base.enabled;
		}
		set
		{
			base.enabled = value;
		}
	}

	public float normalizedTime
	{
		get
		{
			return _normalizedTime;
		}
		set
		{
			_normalizedTime = Mathf.Clamp01(value);
			if (this.onTween != null)
			{
				this.onTween(Interpolate(_normalizedTime));
			}
		}
	}

	public event Action<float> onTween;

	public event Action onRecord;

	public event Action onRestore;

	public void Record()
	{
		if (this.onRecord != null)
		{
			this.onRecord();
		}
	}

	public void Restore()
	{
		if (this.onRestore != null)
		{
			this.onRestore();
		}
	}

	public void ReverseSpeed()
	{
		_speed = 0f - _speed;
	}

	public void Replay()
	{
		_time = 0f;
		_normalizedTime = 0f;
		isPlaying = true;
	}

	public float Interpolate(float t)
	{
		if (method == TweenMethod.CustomCurve)
		{
			return _customCurve.Evaluate(t);
		}
		return _interpolates[(int)method](t);
	}

	public static TweenInterpolator Create(GameObject gameObject, bool isPlaying = true, float delay = 0f, float duration = 1f, float speed = 1f, TweenMethod method = TweenMethod.Linear, WrapMode wrapMode = WrapMode.Once, TimeLine timeLine = TimeLine.Normal, Action<float> onUpdate = null, UnityAction onArriveAtEnding = null, UnityAction onArriveAtBeginning = null)
	{
		TweenInterpolator tweenInterpolator = gameObject.AddComponent<TweenInterpolator>();
		tweenInterpolator.isPlaying = isPlaying;
		tweenInterpolator.delay = delay;
		tweenInterpolator.duration = duration;
		tweenInterpolator.speed = speed;
		tweenInterpolator.method = method;
		tweenInterpolator.wrapMode = wrapMode;
		tweenInterpolator.timeLine = timeLine;
		if (onUpdate != null)
		{
			tweenInterpolator.onTween = (Action<float>)Delegate.Combine(tweenInterpolator.onTween, onUpdate);
		}
		if (onArriveAtEnding != null)
		{
			tweenInterpolator.onArriveAtEnding.AddListener(onArriveAtEnding);
		}
		if (onArriveAtBeginning != null)
		{
			tweenInterpolator.onArriveAtBeginning.AddListener(onArriveAtBeginning);
		}
		return tweenInterpolator;
	}

	private void OverBeginning()
	{
		switch (wrapMode)
		{
		default:
			normalizedTime = 0f;
			isPlaying = false;
			if (onArriveAtBeginning != null && Application.isPlaying)
			{
				onArriveAtBeginning.Invoke();
			}
			break;
		case WrapMode.Loop:
			do
			{
				normalizedTime = 0f;
				if (onArriveAtBeginning != null && Application.isPlaying)
				{
					onArriveAtBeginning.Invoke();
				}
				_normalizedTimeTarget += 1f;
			}
			while (_normalizedTimeTarget <= 0f);
			normalizedTime = _normalizedTimeTarget;
			break;
		case WrapMode.PingPong:
			do
			{
				if (speed > 0f)
				{
					normalizedTime = 1f;
					if (onArriveAtEnding != null && Application.isPlaying)
					{
						onArriveAtEnding.Invoke();
					}
				}
				else
				{
					normalizedTime = 0f;
					if (onArriveAtBeginning != null && Application.isPlaying)
					{
						onArriveAtBeginning.Invoke();
					}
				}
				speed = 0f - speed;
				_normalizedTimeTarget += 1f;
			}
			while (_normalizedTimeTarget <= 0f);
			normalizedTime = ((!(speed < 0f)) ? (1f - _normalizedTimeTarget) : _normalizedTimeTarget);
			break;
		case WrapMode.ClampForever:
			_normalizedTimeTarget = _normalizedTime;
			normalizedTime = 0f;
			if (onArriveAtBeginning != null && Application.isPlaying && _normalizedTimeTarget > 0f)
			{
				onArriveAtBeginning.Invoke();
			}
			break;
		}
	}

	private void OverEnding()
	{
		switch (wrapMode)
		{
		default:
			normalizedTime = 1f;
			isPlaying = false;
			if (onArriveAtEnding != null && Application.isPlaying)
			{
				onArriveAtEnding.Invoke();
			}
			break;
		case WrapMode.Loop:
			do
			{
				normalizedTime = 1f;
				if (onArriveAtEnding != null && Application.isPlaying)
				{
					onArriveAtEnding.Invoke();
				}
				_normalizedTimeTarget -= 1f;
			}
			while (_normalizedTimeTarget >= 1f);
			normalizedTime = _normalizedTimeTarget;
			break;
		case WrapMode.PingPong:
			do
			{
				if (speed > 0f)
				{
					normalizedTime = 1f;
					if (onArriveAtEnding != null && Application.isPlaying)
					{
						onArriveAtEnding.Invoke();
					}
				}
				else
				{
					normalizedTime = 0f;
					if (onArriveAtBeginning != null && Application.isPlaying)
					{
						onArriveAtBeginning.Invoke();
					}
				}
				speed = 0f - speed;
				_normalizedTimeTarget -= 1f;
			}
			while (_normalizedTimeTarget >= 1f);
			normalizedTime = ((!(speed > 0f)) ? (1f - _normalizedTimeTarget) : _normalizedTimeTarget);
			break;
		case WrapMode.ClampForever:
			_normalizedTimeTarget = _normalizedTime;
			normalizedTime = 1f;
			if (onArriveAtEnding != null && Application.isPlaying && _normalizedTimeTarget < 1f)
			{
				onArriveAtEnding.Invoke();
			}
			break;
		}
	}

	private void UpdateTween(float deltaTime)
	{
		_time += deltaTime;
		if (_time < _delay)
		{
			return;
		}
		_deltaNormalizedTime = deltaTime * speed / _duration;
		if (_deltaNormalizedTime == 0f)
		{
			normalizedTime = _normalizedTime;
			return;
		}
		_normalizedTimeTarget = _normalizedTime + _deltaNormalizedTime;
		if (_normalizedTimeTarget >= 1f)
		{
			OverEnding();
		}
		else if (_normalizedTimeTarget <= 0f)
		{
			OverBeginning();
		}
		else
		{
			normalizedTime = _normalizedTimeTarget;
		}
	}

	private void Update()
	{
		UpdateTween((timeLine != 0) ? Time.unscaledDeltaTime : Time.deltaTime);
	}
}
