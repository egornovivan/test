using UnityEngine;

public class TRActiveAndScaleStraight : TRStraight
{
	[SerializeField]
	private ActiveAndScale m_ActiveAndScale;

	public override void SetData(Transform caster, Transform emitter, Transform target, Vector3 targetPosition, int index)
	{
		base.SetData(caster, emitter, target, targetPosition, index);
		m_ActiveAndScale.Init(this);
	}

	public override Vector3 Track(float deltaTime)
	{
		m_ActiveAndScale.UpdateAttackState(deltaTime);
		return base.Track(deltaTime);
	}
}
