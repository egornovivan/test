using RootMotion.FinalIK;
using UnityEngine;

namespace PEIK;

public abstract class Interaction
{
	protected InteractionSystem m_CasterInteractionSystem;

	protected InteractionSystem m_TargetInteractionSystem;

	protected InteractionObject m_CasterInteractionObj;

	protected InteractionObject m_TargetInteractionObj;

	protected abstract string casterObjName { get; }

	protected abstract string targetObjName { get; }

	protected abstract FullBodyBipedEffector[] casterEffectors { get; }

	protected abstract FullBodyBipedEffector[] targetEffectors { get; }

	public void Init(Transform casterRoot, Transform targetRoot)
	{
		m_CasterInteractionSystem = casterRoot.GetComponentInChildren<InteractionSystem>();
		m_TargetInteractionSystem = targetRoot.GetComponentInChildren<InteractionSystem>();
		if (!string.IsNullOrEmpty(casterObjName))
		{
			InteractionObject[] componentsInChildren = casterRoot.GetComponentsInChildren<InteractionObject>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (componentsInChildren[i].name == casterObjName)
				{
					m_CasterInteractionObj = componentsInChildren[i];
					break;
				}
			}
		}
		if (string.IsNullOrEmpty(targetObjName))
		{
			return;
		}
		InteractionObject[] componentsInChildren2 = targetRoot.GetComponentsInChildren<InteractionObject>(includeInactive: true);
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			if (componentsInChildren2[j].name == targetObjName)
			{
				m_TargetInteractionObj = componentsInChildren2[j];
				break;
			}
		}
	}

	public void StartInteraction()
	{
		DoStart();
	}

	public void StartInteraction(MonoBehaviour mono, float delayTime)
	{
		mono.Invoke("DoStart", delayTime);
	}

	private void DoStart()
	{
		if (null != m_CasterInteractionSystem && null != m_TargetInteractionObj)
		{
			for (int i = 0; i < casterEffectors.Length; i++)
			{
				m_CasterInteractionSystem.StartInteraction(casterEffectors[i], m_TargetInteractionObj, interrupt: false);
			}
		}
		if (null != m_TargetInteractionSystem && null != m_CasterInteractionObj)
		{
			for (int j = 0; j < targetEffectors.Length; j++)
			{
				m_TargetInteractionSystem.StartInteraction(targetEffectors[j], m_CasterInteractionObj, interrupt: false);
			}
		}
	}

	public void EndInteraction(bool immediately = false)
	{
		if (null != m_CasterInteractionSystem)
		{
			for (int i = 0; i < casterEffectors.Length; i++)
			{
				if (immediately || !m_CasterInteractionSystem.IsPaused(casterEffectors[i]))
				{
					m_CasterInteractionSystem.StopInteraction(casterEffectors[i]);
				}
				else
				{
					m_CasterInteractionSystem.ResumeInteraction(casterEffectors[i]);
				}
			}
		}
		if (!(null != m_TargetInteractionSystem))
		{
			return;
		}
		for (int j = 0; j < targetEffectors.Length; j++)
		{
			if (immediately)
			{
				m_TargetInteractionSystem.StopInteraction(targetEffectors[j]);
			}
			else
			{
				m_TargetInteractionSystem.ResumeInteraction(targetEffectors[j]);
			}
		}
	}
}
