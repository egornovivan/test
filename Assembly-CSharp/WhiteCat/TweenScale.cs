using UnityEngine;
using WhiteCat.BitwiseOperationExtension;
using WhiteCat.Internal;

namespace WhiteCat;

[AddComponentMenu("White Cat/Tween/Transform/Scale")]
public class TweenScale : TweenBase
{
	public Vector3 from = Vector3.one;

	public Vector3 to = Vector3.one;

	public int mask = -1;

	private Vector3 _temp;

	private Vector3 _original;

	public override void OnTween(float factor)
	{
		_temp = base.transform.localScale;
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
		base.transform.localScale = _temp;
	}

	public override void OnRecord()
	{
		_original = base.transform.localScale;
	}

	public override void OnRestore()
	{
		base.transform.localScale = _original;
	}

	[ContextMenu("Set 'From' to current")]
	public void SetFromToCurrent()
	{
		from = base.transform.localScale;
	}

	[ContextMenu("Set 'To' to current")]
	public void SetToToCurrent()
	{
		to = base.transform.localScale;
	}

	[ContextMenu("Set current to 'From'")]
	public void SetCurrentToFrom()
	{
		base.transform.localScale = from;
	}

	[ContextMenu("Set current to 'To'")]
	public void SetCurrentToTo()
	{
		base.transform.localScale = to;
	}
}
