using UnityEngine;
using WhiteCat.BitwiseOperationExtension;
using WhiteCat.Internal;

namespace WhiteCat;

[AddComponentMenu("White Cat/Tween/Material/Color")]
public class TweenMaterialColor : TweenMaterialProperty
{
	public Color from = Color.white;

	public Color to = Color.white;

	public int mask = -1;

	private Color _temp;

	private Material _material;

	private Color _original;

	public override void OnTween(float factor)
	{
		if ((bool)(_material = base.material))
		{
			_temp = _material.GetColor(base.propertyID);
			if (mask.GetBit(0))
			{
				_temp.r = from.r + (to.r - from.r) * factor;
			}
			if (mask.GetBit(1))
			{
				_temp.g = from.g + (to.g - from.g) * factor;
			}
			if (mask.GetBit(2))
			{
				_temp.b = from.b + (to.b - from.b) * factor;
			}
			if (mask.GetBit(3))
			{
				_temp.a = from.a + (to.a - from.a) * factor;
			}
			_material.SetColor(base.propertyID, _temp);
		}
	}

	public override void OnRecord()
	{
		if ((bool)(_material = base.material))
		{
			_original = _material.GetColor(base.propertyID);
		}
	}

	public override void OnRestore()
	{
		if ((bool)(_material = base.material))
		{
			_material.SetColor(base.propertyID, _original);
		}
	}

	[ContextMenu("Set 'From' to current")]
	public void SetFromToCurrent()
	{
		if ((bool)(_material = base.material))
		{
			from = _material.GetColor(base.propertyID);
		}
	}

	[ContextMenu("Set 'To' to current")]
	public void SetToToCurrent()
	{
		if ((bool)(_material = base.material))
		{
			to = _material.GetColor(base.propertyID);
		}
	}

	[ContextMenu("Set current to 'From'")]
	public void SetCurrentToFrom()
	{
		if ((bool)(_material = base.material))
		{
			_material.SetColor(base.propertyID, from);
		}
	}

	[ContextMenu("Set current to 'To'")]
	public void SetCurrentToTo()
	{
		if ((bool)(_material = base.material))
		{
			_material.SetColor(base.propertyID, to);
		}
	}
}
