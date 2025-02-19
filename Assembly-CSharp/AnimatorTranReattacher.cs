using System;
using Pathea;
using PETools;
using UnityEngine;

public class AnimatorTranReattacher : StateMachineBehaviour
{
	[Serializable]
	public class ReattachReq
	{
		public string objName;

		public string targetBoneName;

		public float reattachTime;

		[HideInInspector]
		public Transform objTran;

		[HideInInspector]
		public bool active;
	}

	[SerializeField]
	private ReattachReq[] m_Reqs;

	private BiologyViewCmpt m_View;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (null == m_View)
		{
			m_View = animator.GetComponentInParent<BiologyViewCmpt>();
		}
		for (int i = 0; i < m_Reqs.Length; i++)
		{
			ReattachReq reattachReq = m_Reqs[i];
			reattachReq.active = true;
			if (null == reattachReq.objTran)
			{
				reattachReq.objTran = PEUtil.GetChild(animator.transform, reattachReq.objName);
			}
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (null == m_View)
		{
			return;
		}
		for (int i = 0; i < m_Reqs.Length; i++)
		{
			ReattachReq reattachReq = m_Reqs[i];
			if (reattachReq.active)
			{
				Reattach(reattachReq);
			}
		}
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (null == m_View)
		{
			return;
		}
		for (int i = 0; i < m_Reqs.Length; i++)
		{
			ReattachReq reattachReq = m_Reqs[i];
			if (reattachReq.active && stateInfo.normalizedTime > reattachReq.reattachTime)
			{
				Reattach(reattachReq);
			}
		}
	}

	private void Reattach(ReattachReq req)
	{
		req.active = false;
		if (!(null == req.objTran))
		{
			m_View.Reattach(req.objTran.gameObject, req.targetBoneName);
		}
	}
}
