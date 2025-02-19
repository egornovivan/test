using Pathea;
using UnityEngine;

public class TRStun : Trajectory
{
	private MotionMgrCmpt m_MotionMgr;

	public override void SetData(Transform caster, Transform emitter, Transform target, Vector3 targetPosition, int index)
	{
		base.SetData(caster, emitter, target, targetPosition, index);
		Stun();
	}

	public override Vector3 Track(float deltaTime)
	{
		Vector3 result = Vector3.zero;
		if (null != m_Target)
		{
			result = GetTargetCenter(m_Target) - base.transform.position;
		}
		return result;
	}

	private void Stun()
	{
		if (null != m_Target)
		{
			PeEntity componentInParent = m_Target.GetComponentInParent<PeEntity>();
			if (null != componentInParent && componentInParent.proto != EEntityProto.Monster)
			{
				m_MotionMgr = componentInParent.GetComponent<MotionMgrCmpt>();
				if (null != m_MotionMgr)
				{
					m_MotionMgr.DoAction(PEActionType.Stuned);
					return;
				}
			}
		}
		Object.Destroy(base.gameObject);
	}

	private void OnDestroy()
	{
		if (null != m_MotionMgr)
		{
			m_MotionMgr.EndImmediately(PEActionType.Stuned);
		}
	}
}
