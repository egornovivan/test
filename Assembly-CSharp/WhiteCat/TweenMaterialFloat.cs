using UnityEngine;
using WhiteCat.Internal;

namespace WhiteCat;

[AddComponentMenu("White Cat/Tween/Material/Float")]
public class TweenMaterialFloat : TweenMaterialProperty
{
	public float from;

	public float to;

	private Material _material;

	private float _original;

	public override void OnTween(float factor)
	{
		if ((bool)(_material = base.material))
		{
			_material.SetFloat(base.propertyID, from + (to - from) * factor);
		}
	}

	public override void OnRecord()
	{
		if ((bool)(_material = base.material))
		{
			_original = _material.GetFloat(base.propertyID);
		}
	}

	public override void OnRestore()
	{
		if ((bool)(_material = base.material))
		{
			_material.SetFloat(base.propertyID, _original);
		}
	}

	[ContextMenu("Set 'From' to current")]
	public void SetFromToCurrent()
	{
		if ((bool)(_material = base.material))
		{
			from = _material.GetFloat(base.propertyID);
		}
	}

	[ContextMenu("Set 'To' to current")]
	public void SetToToCurrent()
	{
		if ((bool)(_material = base.material))
		{
			to = _material.GetFloat(base.propertyID);
		}
	}

	[ContextMenu("Set current to 'From'")]
	public void SetCurrentToFrom()
	{
		if ((bool)(_material = base.material))
		{
			_material.SetFloat(base.propertyID, from);
		}
	}

	[ContextMenu("Set current to 'To'")]
	public void SetCurrentToTo()
	{
		if ((bool)(_material = base.material))
		{
			_material.SetFloat(base.propertyID, to);
		}
	}
}
