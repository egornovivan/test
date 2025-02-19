using UnityEngine;
using WhiteCat.BitwiseOperationExtension;
using WhiteCat.Internal;

namespace WhiteCat;

[AddComponentMenu("White Cat/Tween/Rect Transform/Size Delta")]
[RequireComponent(typeof(RectTransform))]
public class TweenSizeDelta : TweenBase
{
	public Vector2 from;

	public Vector2 to;

	public int mask = -1;

	private Vector2 _temp;

	private Vector2 _original;

	public override void OnTween(float factor)
	{
		_temp = base.rectTransform.sizeDelta;
		if (mask.GetBit(0))
		{
			_temp.x = from.x + (to.x - from.x) * factor;
		}
		if (mask.GetBit(1))
		{
			_temp.y = from.y + (to.y - from.y) * factor;
		}
		base.rectTransform.sizeDelta = _temp;
	}

	public override void OnRecord()
	{
		_original = base.rectTransform.sizeDelta;
	}

	public override void OnRestore()
	{
		base.rectTransform.sizeDelta = _original;
	}

	[ContextMenu("Set 'From' to current")]
	public void SetFromToCurrent()
	{
		from = base.rectTransform.sizeDelta;
	}

	[ContextMenu("Set 'To' to current")]
	public void SetToToCurrent()
	{
		to = base.rectTransform.sizeDelta;
	}

	[ContextMenu("Set current to 'From'")]
	public void SetCurrentToFrom()
	{
		base.rectTransform.sizeDelta = from;
	}

	[ContextMenu("Set current to 'To'")]
	public void SetCurrentToTo()
	{
		base.rectTransform.sizeDelta = to;
	}
}
