using UnityEngine;

namespace WhiteCat;

public class UIBoneFocus : MonoBehaviour
{
	private const float panelWidth = 310f;

	private const float panelHeight = 480f;

	private const float sqrRadius = 361f;

	private const float minX = 19f;

	private const float minY = 19f;

	private const float maxX = 291f;

	private const float maxY = 461f;

	[SerializeField]
	private UIWidget _sprite;

	[SerializeField]
	private Vector3 _boneOffset = new Vector3(0.1f, 0f, 0f);

	[SerializeField]
	private TweenInterpolator _interpolator;

	[SerializeField]
	private TweenInterpolator _highlightAnim;

	private bool _visible;

	private bool _highlight;

	private bool _pressing;

	private static Color normalColor = new Color(1f, 1f, 1f, 0.9f);

	private static Color highlightColor = new Color(0.25f, 1f, 1f, 1f);

	private static Color pressingColor = new Color(0.25f, 0.75f, 0.85f, 1f);

	public bool highlight
	{
		get
		{
			return _highlight;
		}
		set
		{
			_highlight = value;
			if (value)
			{
				_highlightAnim.speed = 1f;
				_highlightAnim.isPlaying = true;
				_sprite.color = highlightColor;
			}
			else
			{
				_highlightAnim.speed = -1f;
				_highlightAnim.isPlaying = true;
				_sprite.color = normalColor;
			}
		}
	}

	public bool pressing
	{
		get
		{
			return _pressing;
		}
		set
		{
			_pressing = value;
			if (value)
			{
				base.transform.localScale = new Vector3(0.95f, 0.95f, 1f);
				_sprite.color = pressingColor;
			}
			else
			{
				base.transform.localScale = new Vector3(1f, 1f, 1f);
				_sprite.color = highlightColor;
			}
		}
	}

	private void Awake()
	{
		_sprite.color = normalColor;
	}

	private void OnEnable()
	{
		_visible = true;
		_interpolator.speed = 1f;
		_interpolator.isPlaying = true;
	}

	private void OnDisable()
	{
		_visible = false;
		_interpolator.speed = -1f;
		_interpolator.isPlaying = true;
	}

	public bool isHover(Vector2 mouseLocalPosition)
	{
		if (_visible)
		{
			Vector3 localPosition = base.transform.localPosition;
			localPosition.x -= mouseLocalPosition.x;
			localPosition.y -= mouseLocalPosition.y;
			return localPosition.x * localPosition.x + localPosition.y * localPosition.y < 361f;
		}
		return false;
	}

	public void UpdatePosition(Transform bone, Camera viewCamera)
	{
		Vector3 localPosition = viewCamera.WorldToViewportPoint(bone.TransformPoint(_boneOffset));
		localPosition.x = (int)(localPosition.x * 310f);
		localPosition.y = (int)(localPosition.y * 480f);
		localPosition.z = 0f;
		base.transform.localPosition = localPosition;
		if (_visible != (localPosition.x > 19f && localPosition.y > 19f && localPosition.x < 291f && localPosition.y < 461f))
		{
			_visible = !_visible;
			if (_visible)
			{
				_interpolator.speed = 1f;
				_interpolator.isPlaying = true;
			}
			else
			{
				_interpolator.speed = -1f;
				_interpolator.isPlaying = true;
			}
		}
	}
}
