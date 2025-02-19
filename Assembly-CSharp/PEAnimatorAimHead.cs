using UnityEngine;

public class PEAnimatorAimHead : PEAnimatorState
{
	public AnimationCurve weightCurve;

	private IKAimCtrl m_AimCtrl;

	internal override void Init(Animator animator)
	{
		base.Init(animator);
		if (m_AimCtrl == null && base.Entity != null)
		{
			m_AimCtrl = base.Entity.GetComponentInChildren<IKAimCtrl>();
		}
	}

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (m_AimCtrl != null && base.Entity.attackEnemy != null)
		{
			m_AimCtrl.SetActive(active: true);
			m_AimCtrl.SetTarget(base.Entity.attackEnemy.CenterBone);
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		if (m_AimCtrl != null)
		{
			m_AimCtrl.SetActive(active: false);
			m_AimCtrl.SetTarget(null);
		}
	}
}
