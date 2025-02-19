using UnityEngine;

public class PEAnimatorVision : StateMachineBehaviour
{
	public float radius;

	private PEHearing[] hears;

	private PEVision[] visions;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (hears == null)
		{
			hears = animator.GetComponentsInChildren<PEHearing>();
		}
		if (visions == null)
		{
			visions = animator.GetComponentsInChildren<PEVision>();
		}
		for (int i = 0; i < hears.Length; i++)
		{
			if (hears[i] != null)
			{
				hears[i].AddBuff(radius, stateInfo.length);
			}
		}
		for (int j = 0; j < visions.Length; j++)
		{
			if (visions[j] != null)
			{
				visions[j].AddBuff(radius, stateInfo.length);
			}
		}
	}
}
