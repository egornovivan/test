using UnityEngine;
using WhiteCat.BitwiseOperationExtension;
using WhiteCat.Internal;

namespace WhiteCat;

[RequireComponent(typeof(RectTransform))]
[AddComponentMenu("White Cat/Tween/Rect Transform/Anchored Position")]
public class TweenAnchoredPosition : TweenBase
{
	public Vector2 from;

	public Vector2 to;

	public int mask = -1;

	private Vector2 _temp;

	private Vector2 _original;

	public override void OnTween(float factor)
	{
		_temp = base.rectTransform.anchoredPosition;
		if (mask.GetBit(0))
		{
			_temp.x = from.x + (to.x - from.x) * factor;
		}
		if (mask.GetBit(1))
		{
			_temp.y = from.y + (to.y - from.y) * factor;
		}
		base.rectTransform.anchoredPosition = _temp;
	}

	public override void OnRecord()
	{
		_original = base.rectTransform.anchoredPosition;
	}

	public override void OnRestore()
	{
		base.rectTransform.anchoredPosition = _original;
	}

	[ContextMenu("Set 'From' to current")]
	public void SetFromToCurrent()
	{
		from = base.rectTransform.anchoredPosition;
	}

	[ContextMenu("Set 'To' to current")]
	public void SetToToCurrent()
	{
		to = base.rectTransform.anchoredPosition;
	}

	[ContextMenu("Set current to 'From'")]
	public void SetCurrentToFrom()
	{
		base.rectTransform.anchoredPosition = from;
	}

	[ContextMenu("Set current to 'To'")]
	public void SetCurrentToTo()
	{
		base.rectTransform.anchoredPosition = to;
	}
}
