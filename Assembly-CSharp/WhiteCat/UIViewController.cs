using System;
using UnityEngine;

namespace WhiteCat;

public class UIViewController : MonoBehaviour
{
	[SerializeField]
	private float _startDistance = 2.5f;

	[SerializeField]
	private float _startYaw = 22.5f;

	[SerializeField]
	private float _startPitch = 5f;

	[SerializeField]
	private Vector3 _targetOffset = new Vector3(0f, 0.8f, 0f);

	[SerializeField]
	private float _mouseMoveToAngle = 0.25f;

	[SerializeField]
	private float _mouseScrollToDistance = 0.5f;

	[SerializeField]
	private float _halfPitchRange = 45f;

	[SerializeField]
	private float _minDistance = 0.5f;

	[SerializeField]
	private float _maxDistance = 5f;

	[SerializeField]
	private float _rootDistance = 5f;

	[SerializeField]
	private Transform _leftBottom;

	[SerializeField]
	private Transform _rightTop;

	private PeViewController _peViewController;

	private Transform _target;

	private Transform _viewRoot;

	private Transform _viewCamera;

	private Camera _uiCamera;

	private Vector2 _lbPoint;

	private Vector2 _rtPoint;

	private Vector2 _cursor;

	private bool _rightButtonPressing;

	private float _distance;

	[SerializeField]
	private float _yaw;

	[SerializeField]
	private float _pitch;

	public void Init(PeViewController viewController)
	{
		_peViewController = viewController;
		_viewRoot = viewController.transform;
		_viewCamera = viewController.viewCam.transform;
	}

	public Vector2 GetViewSize()
	{
		if (_uiCamera == null)
		{
			_uiCamera = GetComponentInParent<Camera>();
		}
		_lbPoint = _uiCamera.WorldToScreenPoint(_leftBottom.position);
		_rtPoint = _uiCamera.WorldToScreenPoint(_rightTop.position);
		return new Vector2(Mathf.Abs(_lbPoint.x) + Mathf.Abs(_lbPoint.x), Mathf.Abs(_lbPoint.y) + Mathf.Abs(_lbPoint.y));
	}

	public void SetCameraToBastView(Vector3 centerPos, float bestDistance, float bestYaw, float scaleFactor)
	{
		if (!(_peViewController == null) && !(_peViewController.target == null))
		{
			_target = _peViewController.target;
			float num = bestDistance * scaleFactor;
			_minDistance = bestDistance - num;
			_maxDistance = bestDistance + num;
			_rootDistance = bestDistance;
			_targetOffset = centerPos - _target.position;
			_distance = _rootDistance;
			_yaw = bestYaw;
			_pitch = 0f;
		}
	}

	private void OnEnable()
	{
		_distance = _startDistance;
		_yaw = _startYaw;
		_pitch = _startPitch;
	}

	private void Update()
	{
		if (_peViewController == null || _peViewController.target == null)
		{
			return;
		}
		_target = _peViewController.target;
		if (!_peViewController.transform.IsChildOf(_target.parent))
		{
			_peViewController.transform.SetParent(_target.parent, worldPositionStays: true);
		}
		if (_uiCamera == null)
		{
			_uiCamera = GetComponentInParent<Camera>();
		}
		_lbPoint = _uiCamera.WorldToScreenPoint(_leftBottom.position);
		_rtPoint = _uiCamera.WorldToScreenPoint(_rightTop.position);
		_cursor = Input.mousePosition;
		bool flag = _cursor.x > _lbPoint.x && _cursor.x < _rtPoint.x && _cursor.y > _lbPoint.y && _cursor.y < _rtPoint.y;
		if (_rightButtonPressing)
		{
			if (!Input.GetMouseButton(1))
			{
				_rightButtonPressing = false;
			}
			_yaw += Input.GetAxis("Mouse X") * _mouseMoveToAngle;
			_pitch = Mathf.Clamp(_pitch - Input.GetAxis("Mouse Y") * _mouseMoveToAngle, 0f - _halfPitchRange, _halfPitchRange);
		}
		else if (flag && Input.GetMouseButtonDown(1))
		{
			_rightButtonPressing = true;
		}
		if (flag)
		{
			_distance = Mathf.Clamp(_distance - Input.GetAxis("Mouse ScrollWheel") * _mouseScrollToDistance, _minDistance, _maxDistance);
		}
		Vector3 vector = new Vector3(0f, _rootDistance * Mathf.Sin(_pitch * ((float)Math.PI / 180f)), 0f);
		float num = Mathf.Sqrt(_rootDistance * _rootDistance - vector.y * vector.y);
		vector.x = num * Mathf.Sin(_yaw * ((float)Math.PI / 180f));
		vector.z = num * Mathf.Cos(_yaw * ((float)Math.PI / 180f));
		_viewRoot.position = _target.TransformPoint(_targetOffset + vector);
		_viewRoot.LookAt(_target.TransformPoint(_targetOffset));
		vector.x = 0f;
		vector.y = 0f;
		vector.z = _rootDistance - _distance;
		_viewCamera.localPosition = vector;
	}
}
