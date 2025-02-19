using UnityEngine;

namespace WhiteCat.Internal;

[ExecuteInEditMode]
public abstract class TweenBase : BaseBehaviour
{
	[Interpolator]
	[SerializeField]
	private TweenInterpolator _interpolator;

	public TweenInterpolator interpolator
	{
		get
		{
			return _interpolator;
		}
		set
		{
			if (_interpolator != value)
			{
				if ((bool)_interpolator && base.enabled)
				{
					_interpolator.onTween -= OnTween;
					_interpolator.onRecord -= OnRecord;
					_interpolator.onRestore -= OnRestore;
				}
				_interpolator = value;
				if ((bool)_interpolator && base.enabled)
				{
					_interpolator.onTween += OnTween;
					_interpolator.onRecord += OnRecord;
					_interpolator.onRestore += OnRestore;
				}
			}
		}
	}

	public abstract void OnTween(float factor);

	public abstract void OnRecord();

	public abstract void OnRestore();

	protected virtual void OnEnable()
	{
		if (!_interpolator)
		{
			_interpolator = GetComponent<TweenInterpolator>();
		}
		if ((bool)_interpolator)
		{
			_interpolator.onTween += OnTween;
			_interpolator.onRecord += OnRecord;
			_interpolator.onRestore += OnRestore;
		}
	}

	protected virtual void OnDisable()
	{
		if ((bool)_interpolator)
		{
			_interpolator.onTween -= OnTween;
			_interpolator.onRecord -= OnRecord;
			_interpolator.onRestore -= OnRestore;
		}
	}
}
