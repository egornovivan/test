using UnityEngine;
using WhiteCat.Internal;

namespace WhiteCat;

[AddComponentMenu("White Cat/Tween/Path/Driver")]
[RequireComponent(typeof(PathDriver))]
public class TweenPathDriver : TweenBase
{
	[SerializeField]
	[GetSet("from")]
	private float _from;

	[GetSet("to")]
	[SerializeField]
	private float _to;

	private float _original;

	private PathDriver _driver;

	private PathDriver driver => (!_driver) ? (_driver = GetComponent<PathDriver>()) : _driver;

	public float from
	{
		get
		{
			return _from;
		}
		set
		{
			if ((bool)driver.path && !driver.path.isCircular)
			{
				_from = Mathf.Clamp(value, 0f, driver.path.pathTotalLength);
			}
			else
			{
				_from = value;
			}
		}
	}

	public float to
	{
		get
		{
			return _to;
		}
		set
		{
			if ((bool)driver.path && !driver.path.isCircular)
			{
				_to = Mathf.Clamp(value, 0f, driver.path.pathTotalLength);
			}
			else
			{
				_to = value;
			}
		}
	}

	public override void OnTween(float factor)
	{
		driver.location = (to - from) * factor + from;
	}

	public override void OnRecord()
	{
		_original = driver.location;
	}

	public override void OnRestore()
	{
		driver.location = _original;
	}

	[ContextMenu("Set 'From' to current")]
	public void SetFromToCurrent()
	{
		from = driver.location;
	}

	[ContextMenu("Set 'To' to current")]
	public void SetToToCurrent()
	{
		to = driver.location;
	}

	[ContextMenu("Set current to 'From'")]
	public void SetCurrentToFrom()
	{
		driver.location = from;
	}

	[ContextMenu("Set current to 'To'")]
	public void SetCurrentToTo()
	{
		driver.location = to;
	}
}
