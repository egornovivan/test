using UnityEngine;

namespace RootMotion.FinalIK.Demos;

public class CharacterAnimationSimpleLegacy : CharacterAnimationThirdPerson
{
	[SerializeField]
	private Animation animation;

	[SerializeField]
	private float pivotOffset;

	[SerializeField]
	private string idleName;

	[SerializeField]
	private string moveName;

	[SerializeField]
	private float idleAnimationSpeed = 0.3f;

	[SerializeField]
	private float moveAnimationSpeed = 0.75f;

	[SerializeField]
	private AnimationCurve moveSpeed;

	protected override void Start()
	{
		base.Start();
		animation[idleName].speed = idleAnimationSpeed;
		animation[moveName].speed = moveAnimationSpeed;
	}

	public override Vector3 GetPivotPoint()
	{
		return base.transform.position + base.transform.forward * pivotOffset;
	}

	public override void UpdateState(CharacterThirdPerson.AnimState state)
	{
		if (Time.deltaTime != 0f)
		{
			base.animationGrounded = true;
			if (state.moveDirection.z > 0.4f)
			{
				animation.CrossFade(moveName, 0.1f);
			}
			else
			{
				animation.CrossFade(idleName);
			}
			character.Move(character.transform.forward * Time.deltaTime * moveSpeed.Evaluate(state.moveDirection.z));
		}
	}
}
