using Pathea.Effect;
using UnityEngine;

public class PEAnimatorEffectState : PEAnimatorState
{
	public int effectId;

	private GameObject m_EffectObject;

	private EffectBuilder.EffectRequest m_Request;

	private void OnEffectSpawned(GameObject obj)
	{
		m_EffectObject = obj;
	}

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (effectId > 0)
		{
			m_Request = Singleton<EffectBuilder>.Instance.Register(effectId, null, animator.transform);
			m_Request.SpawnEvent += OnEffectSpawned;
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		if (m_EffectObject != null)
		{
			Object.Destroy(m_EffectObject);
		}
		if (m_Request != null)
		{
			m_Request.SpawnEvent -= OnEffectSpawned;
		}
	}
}
