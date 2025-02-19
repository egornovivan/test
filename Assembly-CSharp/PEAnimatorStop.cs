using UnityEngine;

public class PEAnimatorStop : PEAnimatorState
{
	private bool m_Interrupt;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		m_Interrupt = false;
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
		if (!(animator.deltaPosition.sqrMagnitude < 0.010000001f) && !(base.Entity.Rigid == null))
		{
			float maxRadius = base.Entity.maxRadius;
			Vector3 point = base.Entity.Rigid.worldCenterOfMass - Vector3.up * base.Entity.maxHeight * 0.5f;
			Vector3 point2 = base.Entity.Rigid.worldCenterOfMass + Vector3.up * base.Entity.maxHeight * 0.5f;
			Vector3 deltaPosition = animator.deltaPosition;
			int layerMask = 2177024;
			if (Physics.CapsuleCast(point, point2, maxRadius, deltaPosition, out var hitInfo, deltaPosition.magnitude * 5f, layerMask) && !m_Interrupt && Vector3.Angle(hitInfo.normal, Vector3.up) > 45f)
			{
				m_Interrupt = true;
				animator.SetBool("Interrupt", value: true);
			}
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		m_Interrupt = false;
	}
}
