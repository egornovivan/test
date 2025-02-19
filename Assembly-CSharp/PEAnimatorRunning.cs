using UnityEngine;

public class PEAnimatorRunning : StateMachineBehaviour
{
	public string running;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		animator.SetBool(running, value: true);
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		animator.SetBool(running, value: false);
	}
}
