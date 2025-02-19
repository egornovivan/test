using System;
using UnityEngine;

namespace WhiteCat;

[Serializable]
public class AudioState : State
{
	public AudioSource source;

	[Range(0f, 1f)]
	public float maxVolume = 1f;

	public void Init()
	{
		source.enabled = true;
		source.playOnAwake = false;
		source.velocityUpdateMode = AudioVelocityUpdateMode.Fixed;
		source.Stop();
		onEnter = delegate
		{
			source.timeSamples = 0;
			source.Play();
		};
		onExit = delegate
		{
			source.Stop();
		};
	}
}
