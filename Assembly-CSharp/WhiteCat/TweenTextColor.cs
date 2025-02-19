using UnityEngine;
using UnityEngine.UI;
using WhiteCat.BitwiseOperationExtension;
using WhiteCat.Internal;

namespace WhiteCat;

[RequireComponent(typeof(Text))]
[AddComponentMenu("White Cat/Tween/UI/Text Color")]
public class TweenTextColor : TweenBase
{
	public Color from = Color.white;

	public Color to = Color.white;

	public int mask = -1;

	private Color _temp;

	private Text _text;

	private Color _original;

	private Text text => (!_text) ? (_text = GetComponent<Text>()) : _text;

	public override void OnTween(float factor)
	{
		_temp = text.color;
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
		text.color = _temp;
	}

	public override void OnRecord()
	{
		_original = text.color;
	}

	public override void OnRestore()
	{
		text.color = _original;
	}

	[ContextMenu("Set 'From' to current")]
	public void SetFromToCurrent()
	{
		from = text.color;
	}

	[ContextMenu("Set 'To' to current")]
	public void SetToToCurrent()
	{
		to = text.color;
	}

	[ContextMenu("Set current to 'From'")]
	public void SetCurrentToFrom()
	{
		text.color = from;
	}

	[ContextMenu("Set current to 'To'")]
	public void SetCurrentToTo()
	{
		text.color = to;
	}
}
