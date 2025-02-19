using System;
using UnityEngine;

namespace NovaEnv;

[Serializable]
public class ThunderPrototype
{
	public AnimationCurve LightIntensityChange;

	public float Delay = 0.7f;

	public AudioClip Sound;
}
