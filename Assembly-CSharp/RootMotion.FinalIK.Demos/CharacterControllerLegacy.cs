using UnityEngine;

namespace RootMotion.FinalIK.Demos;

public class CharacterControllerLegacy : CharacterControllerDefault
{
	public AnimationCurve animationSpeedRelativeToVelocity;

	protected override void Update()
	{
		base.Update();
		GetComponent<Animation>().CrossFade(state.clipName, 0.3f, PlayMode.StopSameLayer);
		GetComponent<Animation>()[state.clipName].speed = animationSpeedRelativeToVelocity.Evaluate(moveVector.magnitude) * state.animationSpeed;
	}
}
