using UnityEngine;
using WhiteCat.BitwiseOperationExtension;
using WhiteCat.Internal;

namespace WhiteCat;

[AddComponentMenu("White Cat/Tween/Rect Transform/Anchor Min")]
[RequireComponent(typeof(RectTransform))]
public class TweenAnchorMin : TweenBase
{
	public Vector2 from;

	public Vector2 to;

	public int mask = -1;

	private Vector2 _temp;

	private Vector2 _original;

	public override void OnTween(float factor)
	{
		_temp = base.rectTransform.anchorMin;
		if (mask.GetBit(0))
		{
			_temp.x = from.x + (to.x - from.x) * factor;
		}
		if (mask.GetBit(1))
		{
			_temp.y = from.y + (to.y - from.y) * factor;
		}
		base.rectTransform.anchorMin = _temp;
	}

	public override void OnRecord()
	{
		_original = base.rectTransform.anchorMin;
	}

	public override void OnRestore()
	{
		base.rectTransform.anchorMin = _original;
	}

	[ContextMenu("Set 'From' to current")]
	public void SetFromToCurrent()
	{
		from = base.rectTransform.anchorMin;
	}

	[ContextMenu("Set 'To' to current")]
	public void SetToToCurrent()
	{
		to = base.rectTransform.anchorMin;
	}

	[ContextMenu("Set current to 'From'")]
	public void SetCurrentToFrom()
	{
		base.rectTransform.anchorMin = from;
	}

	[ContextMenu("Set current to 'To'")]
	public void SetCurrentToTo()
	{
		base.rectTransform.anchorMin = to;
	}
}
