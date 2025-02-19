using UnityEngine;

public class PEAnimatorAudio_Monster : PEAnimatorAudio
{
	public int id;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		AudioManager.instance.Create(animator.transform.position, id);
	}
}
