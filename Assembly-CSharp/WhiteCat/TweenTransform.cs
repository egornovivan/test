using UnityEngine;
using WhiteCat.Internal;

namespace WhiteCat;

[AddComponentMenu("White Cat/Tween/Transform/Transform")]
public class TweenTransform : TweenBase
{
	public Transform from;

	public Transform to;

	private Vector3 _originalPosition;

	private Quaternion _originalRotation;

	private Vector3 _originalScale;

	public override void OnTween(float factor)
	{
		if ((bool)from && (bool)to)
		{
			base.transform.position = from.position + (to.position - from.position) * factor;
			base.transform.rotation = Quaternion.Slerp(from.rotation, to.rotation, factor);
			base.transform.localScale = from.localScale + (to.localScale - from.localScale) * factor;
		}
	}

	public override void OnRecord()
	{
		_originalPosition = base.transform.localPosition;
		_originalRotation = base.transform.localRotation;
		_originalScale = base.transform.localScale;
	}

	public override void OnRestore()
	{
		base.transform.localPosition = _originalPosition;
		base.transform.localRotation = _originalRotation;
		base.transform.localScale = _originalScale;
	}

	[ContextMenu("Set 'From' to current")]
	public void SetFromToCurrent()
	{
		if ((bool)from)
		{
			from.position = base.transform.position;
			from.rotation = base.transform.rotation;
			from.localScale = base.transform.localScale;
		}
	}

	[ContextMenu("Set 'To' to current")]
	public void SetToToCurrent()
	{
		if ((bool)to)
		{
			to.position = base.transform.position;
			to.rotation = base.transform.rotation;
			to.localScale = base.transform.localScale;
		}
	}

	[ContextMenu("Set current to 'From'")]
	public void SetCurrentToFrom()
	{
		if ((bool)from)
		{
			base.transform.position = from.position;
			base.transform.rotation = from.rotation;
			base.transform.localScale = from.localScale;
		}
	}

	[ContextMenu("Set current to 'To'")]
	public void SetCurrentToTo()
	{
		if ((bool)to)
		{
			base.transform.position = to.position;
			base.transform.rotation = to.rotation;
			base.transform.localScale = to.localScale;
		}
	}
}
