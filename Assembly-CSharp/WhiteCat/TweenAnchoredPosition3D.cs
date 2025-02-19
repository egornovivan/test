using UnityEngine;
using WhiteCat.BitwiseOperationExtension;
using WhiteCat.Internal;

namespace WhiteCat;

[AddComponentMenu("White Cat/Tween/Rect Transform/Anchored Position 3D")]
[RequireComponent(typeof(RectTransform))]
public class TweenAnchoredPosition3D : TweenBase
{
	public Vector3 from;

	public Vector3 to;

	public int mask = -1;

	private Vector3 _temp;

	private Vector3 _original;

	public override void OnTween(float factor)
	{
		_temp = base.rectTransform.anchoredPosition3D;
		if (mask.GetBit(0))
		{
			_temp.x = from.x + (to.x - from.x) * factor;
		}
		if (mask.GetBit(1))
		{
			_temp.y = from.y + (to.y - from.y) * factor;
		}
		if (mask.GetBit(2))
		{
			_temp.z = from.z + (to.z - from.z) * factor;
		}
		base.rectTransform.anchoredPosition3D = _temp;
	}

	public override void OnRecord()
	{
		_original = base.rectTransform.anchoredPosition3D;
	}

	public override void OnRestore()
	{
		base.rectTransform.anchoredPosition3D = _original;
	}

	[ContextMenu("Set 'From' to current")]
	public void SetFromToCurrent()
	{
		from = base.rectTransform.anchoredPosition3D;
	}

	[ContextMenu("Set 'To' to current")]
	public void SetToToCurrent()
	{
		to = base.rectTransform.anchoredPosition3D;
	}

	[ContextMenu("Set current to 'From'")]
	public void SetCurrentToFrom()
	{
		base.rectTransform.anchoredPosition3D = from;
	}

	[ContextMenu("Set current to 'To'")]
	public void SetCurrentToTo()
	{
		base.rectTransform.anchoredPosition3D = to;
	}
}
