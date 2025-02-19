using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class IKMapping
{
	[Serializable]
	public class BoneMap
	{
		public Transform transform;

		public IKSolver.Node node;

		public Vector3 defaultLocalPosition;

		public Quaternion defaultLocalRotation;

		public Vector3 localSwingAxis;

		public Vector3 localTwistAxis;

		public Vector3 planePosition;

		public Vector3 ikPosition;

		public Quaternion defaultLocalTargetRotation;

		private Quaternion maintainRotation;

		public float length;

		public Quaternion animatedRotation;

		private IKSolver.Node planeNode1;

		private IKSolver.Node planeNode2;

		private IKSolver.Node planeNode3;

		public Vector3 swingDirection => transform.rotation * localSwingAxis;

		public bool isNodeBone => node != null;

		private Quaternion targetRotation
		{
			get
			{
				if (planeNode1.solverPosition == planeNode3.solverPosition)
				{
					return Quaternion.identity;
				}
				return Quaternion.LookRotation(planeNode2.solverPosition - planeNode1.solverPosition, planeNode3.solverPosition - planeNode1.solverPosition);
			}
		}

		private Quaternion lastAnimatedTargetRotation
		{
			get
			{
				if (planeNode1.transform.position == planeNode3.transform.position)
				{
					return Quaternion.identity;
				}
				return Quaternion.LookRotation(planeNode2.transform.position - planeNode1.transform.position, planeNode3.transform.position - planeNode1.transform.position);
			}
		}

		public void Initiate(Transform transform, IKSolver solver)
		{
			this.transform = transform;
			IKSolver.Point point = solver.GetPoint(transform);
			if (point != null)
			{
				node = point as IKSolver.Node;
			}
		}

		public void StoreDefaultLocalState()
		{
			defaultLocalPosition = transform.localPosition;
			defaultLocalRotation = transform.localRotation;
		}

		public void FixTransform(bool position)
		{
			if (position)
			{
				transform.localPosition = defaultLocalPosition;
			}
			transform.localRotation = defaultLocalRotation;
		}

		public void SetLength(BoneMap nextBone)
		{
			length = Vector3.Distance(transform.position, nextBone.transform.position);
		}

		public void SetLocalSwingAxis(BoneMap swingTarget)
		{
			SetLocalSwingAxis(swingTarget, this);
		}

		public void SetLocalSwingAxis(BoneMap bone1, BoneMap bone2)
		{
			localSwingAxis = Quaternion.Inverse(transform.rotation) * (bone1.transform.position - bone2.transform.position);
		}

		public void SetLocalTwistAxis(Vector3 twistDirection, Vector3 normalDirection)
		{
			Vector3.OrthoNormalize(ref normalDirection, ref twistDirection);
			localTwistAxis = Quaternion.Inverse(transform.rotation) * twistDirection;
		}

		public void SetPlane(IKSolver.Node planeNode1, IKSolver.Node planeNode2, IKSolver.Node planeNode3)
		{
			this.planeNode1 = planeNode1;
			this.planeNode2 = planeNode2;
			this.planeNode3 = planeNode3;
			UpdatePlane(rotation: true, position: true);
		}

		public void UpdatePlane(bool rotation, bool position)
		{
			Quaternion rotation2 = lastAnimatedTargetRotation;
			if (rotation)
			{
				defaultLocalTargetRotation = QuaTools.RotationToLocalSpace(transform.rotation, rotation2);
			}
			if (position)
			{
				planePosition = Quaternion.Inverse(rotation2) * (transform.position - planeNode1.transform.position);
			}
		}

		public void SetIKPosition()
		{
			ikPosition = transform.position;
		}

		public void MaintainRotation()
		{
			maintainRotation = transform.rotation;
		}

		public void SetToIKPosition()
		{
			transform.position = ikPosition;
		}

		public void FixToNode(float weight, IKSolver.Node fixNode = null)
		{
			if (fixNode == null)
			{
				fixNode = node;
			}
			if (weight >= 1f)
			{
				transform.position = fixNode.solverPosition;
			}
			else
			{
				transform.position = Vector3.Lerp(transform.position, fixNode.solverPosition, weight);
			}
		}

		public Vector3 GetPlanePosition()
		{
			return planeNode1.solverPosition + targetRotation * planePosition;
		}

		public void PositionToPlane()
		{
			transform.position = GetPlanePosition();
		}

		public void RotateToPlane(float weight)
		{
			Quaternion quaternion = targetRotation * defaultLocalTargetRotation;
			if (weight >= 1f)
			{
				transform.rotation = quaternion;
			}
			else
			{
				transform.rotation = Quaternion.Lerp(transform.rotation, quaternion, weight);
			}
		}

		public void Swing(Vector3 swingTarget, float weight)
		{
			Swing(swingTarget, transform.position, weight);
		}

		public void Swing(Vector3 pos1, Vector3 pos2, float weight)
		{
			Quaternion quaternion = Quaternion.FromToRotation(transform.rotation * localSwingAxis, pos1 - pos2) * transform.rotation;
			if (weight >= 1f)
			{
				transform.rotation = quaternion;
			}
			else
			{
				transform.rotation = Quaternion.Lerp(transform.rotation, quaternion, weight);
			}
		}

		public void Twist(Vector3 twistDirection, Vector3 normalDirection, float weight)
		{
			Vector3.OrthoNormalize(ref normalDirection, ref twistDirection);
			Quaternion quaternion = Quaternion.FromToRotation(transform.rotation * localTwistAxis, twistDirection) * transform.rotation;
			if (weight >= 1f)
			{
				transform.rotation = quaternion;
			}
			else
			{
				transform.rotation = Quaternion.Lerp(transform.rotation, quaternion, weight);
			}
		}

		public void RotateToMaintain(float weight)
		{
			if (!(weight <= 0f))
			{
				transform.rotation = Quaternion.Lerp(transform.rotation, maintainRotation, weight);
			}
		}

		public void RotateToEffector(float weight)
		{
			if (!isNodeBone)
			{
				return;
			}
			float num = weight * node.effectorRotationWeight;
			if (!(num <= 0f))
			{
				if (num >= 1f)
				{
					transform.rotation = node.solverRotation;
				}
				else
				{
					transform.rotation = Quaternion.Lerp(transform.rotation, node.solverRotation, num);
				}
			}
		}
	}

	protected IKSolver solver;

	public virtual bool IsValid(IKSolver solver, Warning.Logger logger = null)
	{
		return true;
	}

	protected virtual void OnInitiate()
	{
	}

	public void Initiate(IKSolver solver)
	{
		this.solver = solver;
		OnInitiate();
	}

	protected bool BoneIsValid(Transform bone, IKSolver solver, Warning.Logger logger = null)
	{
		if (bone == null)
		{
			logger?.Invoke("IKMappingLimb contains a null reference.");
			return false;
		}
		if (solver.GetPoint(bone) == null)
		{
			logger?.Invoke("IKMappingLimb is referencing to a bone '" + bone.name + "' that does not excist in the Node Chain.");
			return false;
		}
		return true;
	}
}
