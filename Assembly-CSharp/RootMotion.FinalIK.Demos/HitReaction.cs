using System;
using UnityEngine;

namespace RootMotion.FinalIK.Demos;

public class HitReaction : OffsetModifier
{
	[Serializable]
	public abstract class HitPoint
	{
		public string name;

		public Collider collider;

		[SerializeField]
		private float crossFadeTime = 0.1f;

		private float length;

		private float crossFadeSpeed;

		private float lastTime;

		protected float crossFader { get; private set; }

		protected float timer { get; private set; }

		protected Vector3 force { get; private set; }

		protected Vector3 point { get; private set; }

		public void Hit(Vector3 force, Vector3 point)
		{
			if (length == 0f)
			{
				length = GetLength();
			}
			if (length <= 0f)
			{
				Debug.LogError("Hit Point WeightCurve length is zero.");
				return;
			}
			if (timer < 1f)
			{
				crossFader = 0f;
			}
			crossFadeSpeed = ((!(crossFadeTime > 0f)) ? 0f : (1f / crossFadeTime));
			CrossFadeStart();
			timer = 0f;
			this.force = force;
			this.point = point;
		}

		public void Apply(IKSolverFullBodyBiped solver, float weight)
		{
			float num = Time.time - lastTime;
			lastTime = Time.time;
			if (!(timer >= length))
			{
				timer = Mathf.Clamp(timer + num, 0f, length);
				if (crossFadeSpeed > 0f)
				{
					crossFader = Mathf.Clamp(crossFader + num * crossFadeSpeed, 0f, 1f);
				}
				else
				{
					crossFader = 1f;
				}
				OnApply(solver, weight);
			}
		}

		protected abstract float GetLength();

		protected abstract void CrossFadeStart();

		protected abstract void OnApply(IKSolverFullBodyBiped solver, float weight);
	}

	[Serializable]
	public class HitPointEffector : HitPoint
	{
		[Serializable]
		public class EffectorLink
		{
			public FullBodyBipedEffector effector;

			public float weight;

			private Vector3 lastValue;

			private Vector3 current;

			public void Apply(IKSolverFullBodyBiped solver, Vector3 offset, float crossFader)
			{
				current = Vector3.Lerp(lastValue, offset * weight, crossFader);
				solver.GetEffector(effector).positionOffset += current;
			}

			public void CrossFadeStart()
			{
				lastValue = current;
			}
		}

		public AnimationCurve offsetInForceDirection;

		public AnimationCurve offsetInUpDirection;

		public EffectorLink[] effectorLinks;

		protected override float GetLength()
		{
			float num = ((offsetInForceDirection.keys.Length <= 0) ? 0f : offsetInForceDirection.keys[offsetInForceDirection.length - 1].time);
			float min = ((offsetInUpDirection.keys.Length <= 0) ? 0f : offsetInUpDirection.keys[offsetInUpDirection.length - 1].time);
			return Mathf.Clamp(num, min, num);
		}

		protected override void CrossFadeStart()
		{
			EffectorLink[] array = effectorLinks;
			foreach (EffectorLink effectorLink in array)
			{
				effectorLink.CrossFadeStart();
			}
		}

		protected override void OnApply(IKSolverFullBodyBiped solver, float weight)
		{
			Vector3 vector = solver.GetRoot().up * base.force.magnitude;
			Vector3 offset = offsetInForceDirection.Evaluate(base.timer) * base.force + offsetInUpDirection.Evaluate(base.timer) * vector;
			offset *= weight;
			EffectorLink[] array = effectorLinks;
			foreach (EffectorLink effectorLink in array)
			{
				effectorLink.Apply(solver, offset, base.crossFader);
			}
		}
	}

	[Serializable]
	public class HitPointBone : HitPoint
	{
		[Serializable]
		public class BoneLink
		{
			public Transform bone;

			[Range(0f, 1f)]
			public float weight;

			private Quaternion lastValue;

			private Quaternion current;

			public void Apply(IKSolverFullBodyBiped solver, Quaternion offset, float crossFader)
			{
				current = Quaternion.Lerp(lastValue, Quaternion.Lerp(Quaternion.identity, offset, weight), crossFader);
				bone.rotation = current * bone.rotation;
			}

			public void CrossFadeStart()
			{
				lastValue = current;
			}
		}

		public AnimationCurve aroundCenterOfMass;

		public BoneLink[] boneLinks;

		private Rigidbody rigidbody;

		protected override float GetLength()
		{
			return (aroundCenterOfMass.keys.Length <= 0) ? 0f : aroundCenterOfMass.keys[aroundCenterOfMass.length - 1].time;
		}

		protected override void CrossFadeStart()
		{
			BoneLink[] array = boneLinks;
			foreach (BoneLink boneLink in array)
			{
				boneLink.CrossFadeStart();
			}
		}

		protected override void OnApply(IKSolverFullBodyBiped solver, float weight)
		{
			if (rigidbody == null)
			{
				rigidbody = collider.GetComponent<Rigidbody>();
			}
			if (rigidbody != null)
			{
				Vector3 axis = Vector3.Cross(base.force, base.point - rigidbody.worldCenterOfMass);
				float angle = aroundCenterOfMass.Evaluate(base.timer) * weight;
				Quaternion offset = Quaternion.AngleAxis(angle, axis);
				BoneLink[] array = boneLinks;
				foreach (BoneLink boneLink in array)
				{
					boneLink.Apply(solver, offset, base.crossFader);
				}
			}
		}
	}

	public HitPointEffector[] effectorHitPoints;

	public HitPointBone[] boneHitPoints;

	protected override void OnModifyOffset()
	{
		HitPointEffector[] array = effectorHitPoints;
		foreach (HitPointEffector hitPointEffector in array)
		{
			hitPointEffector.Apply(ik.solver, weight);
		}
		HitPointBone[] array2 = boneHitPoints;
		foreach (HitPointBone hitPointBone in array2)
		{
			hitPointBone.Apply(ik.solver, weight);
		}
	}

	public bool Hit(Collider collider, Vector3 force, Vector3 point)
	{
		if (ik == null)
		{
			Debug.LogError("No IK assigned in HitReaction");
			return false;
		}
		HitPointEffector[] array = effectorHitPoints;
		foreach (HitPointEffector hitPointEffector in array)
		{
			if (hitPointEffector.collider == collider)
			{
				hitPointEffector.Hit(force, point);
				return true;
			}
		}
		HitPointBone[] array2 = boneHitPoints;
		foreach (HitPointBone hitPointBone in array2)
		{
			if (hitPointBone.collider == collider)
			{
				hitPointBone.Hit(force, point);
				return true;
			}
		}
		return false;
	}
}
