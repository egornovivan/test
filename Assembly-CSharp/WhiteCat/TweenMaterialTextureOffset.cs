using UnityEngine;
using WhiteCat.BitwiseOperationExtension;
using WhiteCat.Internal;

namespace WhiteCat;

[AddComponentMenu("White Cat/Tween/Material/Texture Offset")]
public class TweenMaterialTextureOffset : TweenMaterialProperty
{
	public Vector2 from;

	public Vector2 to;

	public int mask = -1;

	private Vector2 _temp;

	private Material _material;

	private Vector2 _original;

	public override void OnTween(float factor)
	{
		if ((bool)(_material = base.material))
		{
			_temp = _material.GetTextureOffset(base.propertyName);
			if (mask.GetBit(0))
			{
				_temp.x = from.x + (to.x - from.x) * factor;
			}
			if (mask.GetBit(1))
			{
				_temp.y = from.y + (to.y - from.y) * factor;
			}
			_material.SetTextureOffset(base.propertyName, _temp);
		}
	}

	public override void OnRecord()
	{
		if ((bool)(_material = base.material))
		{
			_original = _material.GetTextureOffset(base.propertyName);
		}
	}

	public override void OnRestore()
	{
		if ((bool)(_material = base.material))
		{
			_material.SetTextureOffset(base.propertyName, _original);
		}
	}

	[ContextMenu("Set 'From' to current")]
	public void SetFromToCurrent()
	{
		if ((bool)(_material = base.material))
		{
			from = _material.GetTextureOffset(base.propertyName);
		}
	}

	[ContextMenu("Set 'To' to current")]
	public void SetToToCurrent()
	{
		if ((bool)(_material = base.material))
		{
			to = _material.GetTextureOffset(base.propertyName);
		}
	}

	[ContextMenu("Set current to 'From'")]
	public void SetCurrentToFrom()
	{
		if ((bool)(_material = base.material))
		{
			_material.SetTextureOffset(base.propertyName, from);
		}
	}

	[ContextMenu("Set current to 'To'")]
	public void SetCurrentToTo()
	{
		if ((bool)(_material = base.material))
		{
			_material.SetTextureOffset(base.propertyName, to);
		}
	}
}
