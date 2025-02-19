using UnityEngine;

namespace WhiteCat;

[ExecuteInEditMode]
[AddComponentMenu("White Cat/Path/Path Driver")]
public class PathDriver : BaseBehaviour
{
	[GetSet("path")]
	[SerializeField]
	private Path _path;

	[GetSet("location")]
	[SerializeField]
	private float _location;

	public RotationMode rotationMode;

	public bool reverseForward;

	[Direction(1f)]
	public Vector3 constantUp = Vector3.up;

	private int _splineIndex = -1;

	private float _splineTime;

	public Path path
	{
		get
		{
			return _path;
		}
		set
		{
			if (value != _path && (!value || !(value.gameObject == base.gameObject)))
			{
				if ((bool)_path)
				{
					_path.onChange -= Sync;
				}
				_path = value;
				if ((bool)_path)
				{
					_path.onChange += Sync;
				}
			}
		}
	}

	public float location
	{
		get
		{
			return _location;
		}
		set
		{
			_location = value;
			if ((bool)_path)
			{
				Sync();
			}
		}
	}

	public float normalizedLocation
	{
		get
		{
			return _location / path.pathTotalLength;
		}
		set
		{
			_location = value * path.pathTotalLength;
			if ((bool)_path)
			{
				Sync();
			}
		}
	}

	private void OnDestroy()
	{
		if ((bool)_path)
		{
			_path.onChange -= Sync;
		}
	}

	private void Sync()
	{
		if (!_path.isCircular)
		{
			_location = Mathf.Clamp(_location, 0f, _path.pathTotalLength);
		}
		_path.GetPathPositionAtPathLength(_location, ref _splineIndex, ref _splineTime);
		base.transform.position = _path.GetSplinePoint(_splineIndex, _splineTime);
		switch (rotationMode)
		{
		case RotationMode.ConstantUp:
			base.transform.rotation = _path.GetSplineRotation(_splineIndex, _splineTime, constantUp, reverseForward);
			break;
		case RotationMode.SlerpNodes:
			base.transform.rotation = _path.GetSplineRotation(_splineIndex, _splineTime, reverseForward);
			break;
		case RotationMode.MinimizeDelta:
			base.transform.rotation = _path.GetSplineRotation(_splineIndex, _splineTime, base.transform.up, reverseForward);
			break;
		}
	}
}
