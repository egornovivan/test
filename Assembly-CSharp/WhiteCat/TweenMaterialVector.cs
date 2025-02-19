using UnityEngine;
using WhiteCat.BitwiseOperationExtension;
using WhiteCat.Internal;

namespace WhiteCat;

[AddComponentMenu("White Cat/Tween/Material/Vector")]
public class TweenMaterialVector : TweenMaterialProperty
{
	public Vector4 from;

	public Vector4 to;

	public int mask = -1;

	private Vector4 _temp;

	private Material _material;

	private Vector4 _original;

	public override void OnTween(float factor)
	{
		if ((bool)(_material = base.material))
		{
			_temp = _material.GetVector(base.propertyID);
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
			if (mask.GetBit(3))
			{
				_temp.w = from.w + (to.w - from.w) * factor;
			}
			_material.SetVector(base.propertyID, _temp);
		}
	}

	public override void OnRecord()
	{
		if ((bool)(_material = base.material))
		{
			_original = _material.GetVector(base.propertyID);
		}
	}

	public override void OnRestore()
	{
		if ((bool)(_material = base.material))
		{
			_material.SetVector(base.propertyID, _original);
		}
	}

	[ContextMenu("Set 'From' to current")]
	public void SetFromToCurrent()
	{
		if ((bool)(_material = base.material))
		{
			from = _material.GetVector(base.propertyID);
		}
	}

	[ContextMenu("Set 'To' to current")]
	public void SetToToCurrent()
	{
		if ((bool)(_material = base.material))
		{
			to = _material.GetVector(base.propertyID);
		}
	}

	[ContextMenu("Set current to 'From'")]
	public void SetCurrentToFrom()
	{
		if ((bool)(_material = base.material))
		{
			_material.SetVector(base.propertyID, from);
		}
	}

	[ContextMenu("Set current to 'To'")]
	public void SetCurrentToTo()
	{
		if ((bool)(_material = base.material))
		{
			_material.SetVector(base.propertyID, to);
		}
	}
}
