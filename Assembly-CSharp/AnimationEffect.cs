using System;
using UnityEngine;

[Serializable]
public class AnimationEffect
{
	[HideInInspector]
	public bool LastIsPlaying;

	public string AnimationName;

	public GameObject EffectGroupRes;
}
