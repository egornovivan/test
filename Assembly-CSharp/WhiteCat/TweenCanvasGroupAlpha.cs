using UnityEngine;
using WhiteCat.Internal;

namespace WhiteCat;

[RequireComponent(typeof(CanvasGroup))]
[AddComponentMenu("White Cat/Tween/UI/Canvas Group Alpha")]
public class TweenCanvasGroupAlpha : TweenBase
{
	[Range(0f, 1f)]
	public float from = 1f;

	[Range(0f, 1f)]
	public float to = 1f;

	private float _original;

	private CanvasGroup _group;

	private CanvasGroup group => (!_group) ? (_group = GetComponent<CanvasGroup>()) : _group;

	public override void OnTween(float factor)
	{
		group.alpha = from + (to - from) * factor;
	}

	public override void OnRecord()
	{
		_original = group.alpha;
	}

	public override void OnRestore()
	{
		group.alpha = _original;
	}

	[ContextMenu("Set 'From' to current")]
	public void SetFromToCurrent()
	{
		from = group.alpha;
	}

	[ContextMenu("Set 'To' to current")]
	public void SetToToCurrent()
	{
		to = group.alpha;
	}

	[ContextMenu("Set current to 'From'")]
	public void SetCurrentToFrom()
	{
		group.alpha = from;
	}

	[ContextMenu("Set current to 'To'")]
	public void SetCurrentToTo()
	{
		group.alpha = to;
	}
}
