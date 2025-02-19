using UnityEngine;
using WhiteCat.BitwiseOperationExtension;
using WhiteCat.Internal;

namespace WhiteCat;

[RequireComponent(typeof(RectTransform))]
[AddComponentMenu("White Cat/Tween/Rect Transform/Offset Min")]
public class TweenOffsetMin : TweenBase
{
	public Vector2 from;

	public Vector2 to;

	public int mask = -1;

	private Vector2 _temp;

	private Vector2 _original;

	public override void OnTween(float factor)
	{
		_temp = base.rectTransform.offsetMin;
		if (mask.GetBit(0))
		{
			_temp.x = from.x + (to.x - from.x) * factor;
		}
		if (mask.GetBit(1))
		{
			_temp.y = from.y + (to.y - from.y) * factor;
		}
		base.rectTransform.offsetMin = _temp;
	}

	public override void OnRecord()
	{
		_original = base.rectTransform.offsetMin;
	}

	public override void OnRestore()
	{
		base.rectTransform.offsetMin = _original;
	}

	[ContextMenu("Set 'From' to current")]
	public void SetFromToCurrent()
	{
		from = base.rectTransform.offsetMin;
	}

	[ContextMenu("Set 'To' to current")]
	public void SetToToCurrent()
	{
		to = base.rectTransform.offsetMin;
	}

	[ContextMenu("Set current to 'From'")]
	public void SetCurrentToFrom()
	{
		base.rectTransform.offsetMin = from;
	}

	[ContextMenu("Set current to 'To'")]
	public void SetCurrentToTo()
	{
		base.rectTransform.offsetMin = to;
	}
}
