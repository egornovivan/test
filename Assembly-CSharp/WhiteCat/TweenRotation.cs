using UnityEngine;
using WhiteCat.Internal;

namespace WhiteCat;

[AddComponentMenu("White Cat/Tween/Transform/Rotation")]
public class TweenRotation : TweenBase
{
	public Space relativeTo = Space.Self;

	[EulerAngles]
	public Quaternion from = Quaternion.identity;

	[EulerAngles]
	public Quaternion to = Quaternion.identity;

	private Quaternion _original;

	public override void OnTween(float factor)
	{
		switch (relativeTo)
		{
		default:
			base.transform.localRotation = Quaternion.Slerp(from, to, factor);
			break;
		case Space.World:
			base.transform.rotation = Quaternion.Slerp(from, to, factor);
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
		from = ((relativeTo != Space.Self) ? base.transform.rotation : base.transform.localRotation);
	}

	[ContextMenu("Set 'To' to current")]
	public void SetToToCurrent()
	{
		to = ((relativeTo != Space.Self) ? base.transform.rotation : base.transform.localRotation);
	}

	[ContextMenu("Set current to 'From'")]
	public void SetCurrentToFrom()
	{
		if (relativeTo == Space.Self)
		{
			base.transform.localRotation = from;
		}
		else
		{
			base.transform.rotation = from;
		}
	}

	[ContextMenu("Set current to 'To'")]
	public void SetCurrentToTo()
	{
		if (relativeTo == Space.Self)
		{
			base.transform.localRotation = to;
		}
		else
		{
			base.transform.rotation = to;
		}
	}
}
