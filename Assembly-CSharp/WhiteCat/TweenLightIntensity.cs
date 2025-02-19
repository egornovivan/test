using UnityEngine;
using WhiteCat.Internal;

namespace WhiteCat;

[RequireComponent(typeof(Light))]
[AddComponentMenu("White Cat/Tween/Light/Intensity")]
public class TweenLightIntensity : TweenBase
{
	public float from = 0.5f;

	public float to = 0.5f;

	private Light _light;

	private float _original;

	private Light light => (!_light) ? (_light = GetComponent<Light>()) : _light;

	public override void OnTween(float factor)
	{
		light.intensity = from + (to - from) * factor;
	}

	public override void OnRecord()
	{
		_original = light.intensity;
	}

	public override void OnRestore()
	{
		light.intensity = _original;
	}

	[ContextMenu("Set 'From' to current")]
	public void SetFromToCurrent()
	{
		from = light.intensity;
	}

	[ContextMenu("Set 'To' to current")]
	public void SetToToCurrent()
	{
		to = light.intensity;
	}

	[ContextMenu("Set current to 'From'")]
	public void SetCurrentToFrom()
	{
		light.intensity = from;
	}

	[ContextMenu("Set current to 'To'")]
	public void SetCurrentToTo()
	{
		light.intensity = to;
	}
}
