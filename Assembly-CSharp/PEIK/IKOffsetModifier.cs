using System;
using RootMotion.FinalIK;
using UnityEngine;

namespace PEIK;

[RequireComponent(typeof(FullBodyBipedIK))]
public abstract class IKOffsetModifier : MonoBehaviour
{
	public float m_Weight = 1f;

	[SerializeField]
	[HideInInspector]
	protected FullBodyBipedIK m_FBBIK;

	private float m_LastTime;

	protected float deltaTime => Time.deltaTime;

	protected abstract void OnModifyOffset();

	protected abstract void OnInit();

	protected virtual void Awake()
	{
		m_FBBIK = GetComponent<FullBodyBipedIK>();
		IKSolverFullBodyBiped solver = m_FBBIK.solver;
		solver.OnPreUpdate = (RootMotion.FinalIK.IKSolver.UpdateDelegate)Delegate.Combine(solver.OnPreUpdate, new RootMotion.FinalIK.IKSolver.UpdateDelegate(ModifyOffset));
		m_LastTime = Time.time;
		OnInit();
	}

	private void ModifyOffset()
	{
		if (base.enabled && !(m_Weight <= 0f) && !(deltaTime <= 0f) && !(m_FBBIK == null))
		{
			m_Weight = Mathf.Clamp(m_Weight, 0f, 1f);
			OnModifyOffset();
			m_LastTime = Time.time;
		}
	}

	private void OnDestroy()
	{
		if (m_FBBIK != null)
		{
			IKSolverFullBodyBiped solver = m_FBBIK.solver;
			solver.OnPreUpdate = (RootMotion.FinalIK.IKSolver.UpdateDelegate)Delegate.Remove(solver.OnPreUpdate, new RootMotion.FinalIK.IKSolver.UpdateDelegate(ModifyOffset));
		}
	}
}
