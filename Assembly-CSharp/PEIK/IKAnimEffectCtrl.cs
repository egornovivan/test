using UnityEngine;

namespace PEIK;

public class IKAnimEffectCtrl : IKOffsetModifier
{
	public IKMoveEffect m_MoveEffect;

	public IKHitReaction m_HitReaction;

	public bool moveEffectRunning => base.isActiveAndEnabled && m_Weight > 0f && m_MoveEffect.isRunning;

	public void StartMove(Vector3 velocity)
	{
		if (null != m_FBBIK)
		{
			m_MoveEffect.StartMove(m_FBBIK.solver, velocity);
		}
	}

	public void StopMove(Vector3 velocity)
	{
		if (null != m_FBBIK)
		{
			m_MoveEffect.StopMove(m_FBBIK.solver, velocity);
		}
	}

	public void StopMove(Vector3 velocity, bool rightFoot)
	{
		if (null != m_FBBIK)
		{
			m_MoveEffect.StopMove(m_FBBIK.solver, velocity, rightFoot);
		}
	}

	public void EndMoveEffect()
	{
		m_MoveEffect.EndEffect();
	}

	public void OnHit(Transform trans, Vector3 dir, float weight, float effectTime)
	{
		if (null != m_FBBIK)
		{
			m_HitReaction.OnHit(m_FBBIK.solver, trans, dir, weight, effectTime);
		}
	}

	protected override void OnInit()
	{
	}

	protected override void OnModifyOffset()
	{
		if (null != m_FBBIK)
		{
			m_MoveEffect.OnModifyOffset(m_FBBIK.solver, m_Weight, base.deltaTime);
			m_HitReaction.OnModifyOffset(m_FBBIK.solver, m_Weight, base.deltaTime);
		}
	}
}
