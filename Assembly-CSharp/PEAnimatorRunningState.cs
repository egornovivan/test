using UnityEngine;

public class PEAnimatorRunningState : StateMachineBehaviour
{
	public string running;

	public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
	{
		base.OnStateMachineEnter(animator, stateMachinePathHash);
		animator.SetBool(running, value: true);
	}

	public override void OnStateMachineExit(Animator animator, int stateMachinePathHash)
	{
		base.OnStateMachineExit(animator, stateMachinePathHash);
		animator.SetBool(running, value: false);
	}
}
