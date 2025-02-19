using UnityEngine;
using WhiteCat.BitwiseOperationExtension;
using WhiteCat.Internal;

namespace WhiteCat;

[AddComponentMenu("White Cat/Tween/Transform/Euler Angles")]
public class TweenEulerAngles : TweenBase
{
	public Space relativeTo = Space.Self;

	public Vector3 from;

	public Vector3 to;

	public int mask = -1;

	private Vector3 _temp;

	private Quaternion _original;

	public override void OnTween(float factor)
	{
		switch (relativeTo)
		{
		default:
			_temp = base.transform.localEulerAngles;
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
			base.transform.localEulerAngles = _temp;
			break;
		case Space.World:
			_temp = base.transform.eulerAngles;
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
			base.transform.eulerAngles = _temp;
			break;
		}
	}

	public override void OnRecord()
	{
		_original = base.transform.localRotation;
	}

	public override void OnRestore()
	{
		base.transform.localRotation = _original;
	}

	[ContextMenu("Set 'From' to current")]
	public void SetFromToCurrent()
	{
		from = ((relativeTo != Space.Self) ? base.transform.eulerAngles : base.transform.localEulerAngles);
	}

	[ContextMenu("Set 'To' to current")]
	public void SetToToCurrent()
	{
		to = ((relativeTo != Space.Self) ? base.transform.eulerAngles : base.transform.localEulerAngles);
	}

	[ContextMenu("Set current to 'From'")]
	public void SetCurrentToFrom()
	{
		if (relativeTo == Space.Self)
		{
			base.transform.localEulerAngles = from;
		}
		else
		{
			base.transform.eulerAngles = from;
		}
	}

	[ContextMenu("Set current to 'To'")]
	public void SetCurrentToTo()
	{
		if (relativeTo == Space.Self)
		{
			base.transform.localEulerAngles = to;
		}
		else
		{
			base.transform.eulerAngles = to;
		}
	}
}
