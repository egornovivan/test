using UnityEngine;

namespace WhiteCat;

public class ArmorToolTip : MonoBehaviour
{
	[SerializeField]
	private UILabel _label;

	[SerializeField]
	private Transform _background;

	[SerializeField]
	private Vector2 _offset;

	[SerializeField]
	private float _widthExtend = 16f;

	[SerializeField]
	private float _showDelay = 1.25f;

	[SerializeField]
	private float _ignoreInterval = 1.25f;

	private Collider _lastCollider;

	private float _time = -1f;

	private float _lastHideTime;

	private void UpdateText()
	{
		if ((bool)_lastCollider)
		{
			UITip component = _lastCollider.GetComponent<UITip>();
			if ((bool)component)
			{
				base.gameObject.SetActive(value: true);
				_label.text = component.text;
				Vector2 vector = _label.font.size * _label.font.CalculatePrintedSize(component.text, _label.supportEncoding, _label.symbolStyle);
				Vector3 localScale = _background.localScale;
				localScale.x = vector.x + _widthExtend;
				_background.localScale = localScale;
			}
			else
			{
				base.gameObject.SetActive(value: false);
				_lastHideTime = Time.unscaledTime;
			}
		}
		else
		{
			base.gameObject.SetActive(value: false);
			_lastHideTime = Time.unscaledTime;
		}
	}

	public void UpdateToolTip(Vector2 mousePosition)
	{
		Collider collider = UICamera.lastHit.collider;
		if (collider != _lastCollider)
		{
			_lastCollider = collider;
			if ((bool)collider)
			{
				if (base.gameObject.activeSelf || Time.unscaledTime - _lastHideTime < _ignoreInterval)
				{
					UpdateText();
				}
				else if ((bool)collider.GetComponent<UITip>())
				{
					_time = _showDelay;
				}
			}
			else
			{
				UpdateText();
			}
		}
		if (_time > 0f)
		{
			_time -= Time.unscaledDeltaTime;
			if (_time <= 0f)
			{
				UpdateText();
			}
		}
		mousePosition.x = (int)(mousePosition.x + _offset.x);
		mousePosition.y = (int)(mousePosition.y + _offset.y);
		base.transform.localPosition = mousePosition;
	}
}
