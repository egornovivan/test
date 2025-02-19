using UnityEngine;

namespace RootMotion.FinalIK.Demos;

public class CharacterAnimationThirdPerson : CharacterAnimationBase
{
	public virtual void UpdateState(CharacterThirdPerson.AnimState _state)
	{
	}

	public override Vector3 GetPivotPoint()
	{
		return base.transform.position;
	}
}
