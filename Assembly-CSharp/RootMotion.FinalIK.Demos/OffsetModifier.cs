using System;
using System.Collections;
using UnityEngine;

namespace RootMotion.FinalIK.Demos;

public abstract class OffsetModifier : MonoBehaviour
{
	[Serializable]
	public class OffsetLimits
	{
		public FullBodyBipedEffector effector;

		public float spring;

		public bool x;

		public bool y;

		public bool z;

		public float minX;

		public float maxX;

		public float minY;

		public float maxY;

		public float minZ;

		public float maxZ;

		public void Apply(IKEffector e, Quaternion rootRotation)
		{
			Vector3 vector = Quaternion.Inverse(rootRotation) * e.positionOffset;
			if (spring <= 0f)
			{
				if (x)
				{
					vector.x = Mathf.Clamp(vector.x, minX, maxX);
				}
				if (y)
				{
					vector.y = Mathf.Clamp(vector.y, minY, maxY);
				}
				if (z)
				{
					vector.z = Mathf.Clamp(vector.z, minZ, maxZ);
				}
			}
			else
			{
				if (x)
				{
					vector.x = SpringAxis(vector.x, minX, maxX);
				}
				if (y)
				{
					vector.y = SpringAxis(vector.y, minY, maxY);
				}
				if (z)
				{
					vector.z = SpringAxis(vector.z, minZ, maxZ);
				}
			}
			e.positionOffset = rootRotation * vector;
		}

		private float SpringAxis(float value, float min, float max)
		{
			if (value > min && value < max)
			{
				return value;
			}
			if (value < min)
			{
				return Spring(value, min, negative: true);
			}
			return Spring(value, max, negative: false);
		}

		private float Spring(float value, float limit, bool negative)
		{
			float num = value - limit;
			float num2 = num * spring;
			if (negative)
			{
				return value + Mathf.Clamp(0f - num2, 0f, 0f - num);
			}
			return value - Mathf.Clamp(num2, 0f, num);
		}
	}

	public float weight = 1f;

	[SerializeField]
	protected FullBodyBipedIK ik;

	private float lastTime;

	protected float deltaTime => Time.time - lastTime;

	protected abstract void OnModifyOffset();

	protected virtual void Start()
	{
		StartCoroutine(Initiate());
	}

	private IEnumerator Initiate()
	{
		while (ik == null)
		{
			yield return null;
		}
		IKSolverFullBodyBiped solver = ik.solver;
		solver.OnPreUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(solver.OnPreUpdate, new IKSolver.UpdateDelegate(ModifyOffset));
		lastTime = Time.time;
	}

	private void ModifyOffset()
	{
		if (base.enabled && !(weight <= 0f) && !(deltaTime <= 0f) && !(ik == null))
		{
			weight = Mathf.Clamp(weight, 0f, 1f);
			OnModifyOffset();
			lastTime = Time.time;
		}
	}

	protected void ApplyLimits(OffsetLimits[] limits)
	{
		foreach (OffsetLimits offsetLimits in limits)
		{
			offsetLimits.Apply(ik.solver.GetEffector(offsetLimits.effector), base.transform.rotation);
		}
	}

	private void OnDestroy()
	{
		if (ik != null)
		{
			IKSolverFullBodyBiped solver = ik.solver;
			solver.OnPreUpdate = (IKSolver.UpdateDelegate)Delegate.Remove(solver.OnPreUpdate, new IKSolver.UpdateDelegate(ModifyOffset));
		}
	}
}
