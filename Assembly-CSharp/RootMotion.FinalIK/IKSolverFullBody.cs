using System;
using UnityEngine;

namespace RootMotion.FinalIK;

[Serializable]
public class IKSolverFullBody : IKSolver
{
	[Range(0f, 10f)]
	public int iterations = 4;

	public FBIKChain[] chain = new FBIKChain[0];

	public IKEffector[] effectors = new IKEffector[0];

	public IKMappingSpine spineMapping = new IKMappingSpine();

	public IKMappingBone[] boneMappings = new IKMappingBone[0];

	public IKMappingLimb[] limbMappings = new IKMappingLimb[0];

	public UpdateDelegate OnPreRead;

	public UpdateDelegate OnPreSolve;

	public IterationDelegate OnPreIteration;

	public IterationDelegate OnPostIteration;

	public UpdateDelegate OnPreBend;

	public UpdateDelegate OnPostSolve;

	public IKEffector GetEffector(Transform t)
	{
		for (int i = 0; i < effectors.Length; i++)
		{
			if (effectors[i].bone == t)
			{
				return effectors[i];
			}
		}
		return null;
	}

	public FBIKChain GetChain(Transform transform)
	{
		for (int i = 0; i < chain.Length; i++)
		{
			for (int j = 0; j < chain[i].nodes.Length; j++)
			{
				if (chain[i].nodes[j].transform == transform)
				{
					return chain[i];
				}
			}
		}
		return null;
	}

	public override Point[] GetPoints()
	{
		int num = 0;
		for (int i = 0; i < chain.Length; i++)
		{
			num += chain[i].nodes.Length;
		}
		Point[] array = new Point[num];
		int num2 = 0;
		for (int j = 0; j < chain.Length; j++)
		{
			for (int k = 0; k < chain[j].nodes.Length; k++)
			{
				array[num2] = chain[j].nodes[k];
			}
		}
		return array;
	}

	public override Point GetPoint(Transform transform)
	{
		for (int i = 0; i < chain.Length; i++)
		{
			for (int j = 0; j < chain[i].nodes.Length; j++)
			{
				if (chain[i].nodes[j].transform == transform)
				{
					return chain[i].nodes[j];
				}
			}
		}
		return null;
	}

	public override bool IsValid(bool log)
	{
		if (chain == null)
		{
			if (log)
			{
				LogWarning("FBIK chain is null, can't initiate solver.");
			}
			return false;
		}
		if (chain.Length == 0)
		{
			if (log)
			{
				LogWarning("FBIK chain length is 0, can't initiate solver.");
			}
			return false;
		}
		for (int i = 0; i < chain.Length; i++)
		{
			if (log)
			{
				if (!chain[i].IsValid(base.LogWarning))
				{
					return false;
				}
			}
			else if (!chain[i].IsValid())
			{
				return false;
			}
		}
		IKEffector[] array = effectors;
		foreach (IKEffector iKEffector in array)
		{
			if (!iKEffector.IsValid(this, base.LogWarning))
			{
				return false;
			}
		}
		if (log)
		{
			if (!spineMapping.IsValid(this, base.LogWarning))
			{
				return false;
			}
			IKMappingLimb[] array2 = limbMappings;
			foreach (IKMappingLimb iKMappingLimb in array2)
			{
				if (!iKMappingLimb.IsValid(this, base.LogWarning))
				{
					return false;
				}
			}
			IKMappingBone[] array3 = boneMappings;
			foreach (IKMappingBone iKMappingBone in array3)
			{
				if (!iKMappingBone.IsValid(this, base.LogWarning))
				{
					return false;
				}
			}
		}
		else
		{
			if (!spineMapping.IsValid(this))
			{
				return false;
			}
			IKMappingLimb[] array4 = limbMappings;
			foreach (IKMappingLimb iKMappingLimb2 in array4)
			{
				if (!iKMappingLimb2.IsValid(this))
				{
					return false;
				}
			}
			IKMappingBone[] array5 = boneMappings;
			foreach (IKMappingBone iKMappingBone2 in array5)
			{
				if (!iKMappingBone2.IsValid(this))
				{
					return false;
				}
			}
		}
		return true;
	}

	public override void StoreDefaultLocalState()
	{
		spineMapping.StoreDefaultLocalState();
		for (int i = 0; i < limbMappings.Length; i++)
		{
			limbMappings[i].StoreDefaultLocalState();
		}
		for (int j = 0; j < boneMappings.Length; j++)
		{
			boneMappings[j].StoreDefaultLocalState();
		}
	}

	public override void FixTransforms()
	{
		spineMapping.FixTransforms();
		for (int i = 0; i < limbMappings.Length; i++)
		{
			limbMappings[i].FixTransforms();
		}
		for (int j = 0; j < boneMappings.Length; j++)
		{
			boneMappings[j].FixTransforms();
		}
	}

	protected override void OnInitiate()
	{
		Vector3 position = root.position;
		Quaternion rotation = root.rotation;
		root.position = Vector3.zero;
		root.rotation = Quaternion.identity;
		for (int i = 0; i < chain.Length; i++)
		{
			chain[i].Initiate(this, chain);
		}
		IKEffector[] array = effectors;
		foreach (IKEffector iKEffector in array)
		{
			iKEffector.Initiate(this);
		}
		spineMapping.Initiate(this);
		IKMappingBone[] array2 = boneMappings;
		foreach (IKMappingBone iKMappingBone in array2)
		{
			iKMappingBone.Initiate(this);
		}
		IKMappingLimb[] array3 = limbMappings;
		foreach (IKMappingLimb iKMappingLimb in array3)
		{
			iKMappingLimb.Initiate(this);
		}
		root.position = position;
		root.rotation = rotation;
	}

	protected override void OnUpdate()
	{
		if (IKPositionWeight <= 0f)
		{
			for (int i = 0; i < effectors.Length; i++)
			{
				effectors[i].positionOffset = Vector3.zero;
			}
		}
		else if (chain.Length != 0)
		{
			IKPositionWeight = Mathf.Clamp(IKPositionWeight, 0f, 1f);
			if (OnPreRead != null)
			{
				OnPreRead();
			}
			ReadPose();
			if (OnPreSolve != null)
			{
				OnPreSolve();
			}
			Solve();
			if (OnPostSolve != null)
			{
				OnPostSolve();
			}
			WritePose();
			for (int j = 0; j < effectors.Length; j++)
			{
				effectors[j].OnPostWrite();
			}
		}
	}

	protected virtual void ReadPose()
	{
		for (int i = 0; i < chain.Length; i++)
		{
			if (chain[i].bendConstraint.initiated)
			{
				chain[i].bendConstraint.LimitBend(IKPositionWeight, GetEffector(chain[i].nodes[2].transform).positionWeight);
			}
		}
		for (int j = 0; j < effectors.Length; j++)
		{
			effectors[j].ResetOffset();
		}
		for (int k = 0; k < effectors.Length; k++)
		{
			effectors[k].OnPreSolve(iterations > 0);
		}
		for (int l = 0; l < chain.Length; l++)
		{
			chain[l].ReadPose(chain, iterations > 0);
		}
		if (iterations > 0)
		{
			spineMapping.ReadPose();
			for (int m = 0; m < boneMappings.Length; m++)
			{
				boneMappings[m].ReadPose();
			}
		}
		for (int n = 0; n < limbMappings.Length; n++)
		{
			limbMappings[n].ReadPose();
		}
	}

	protected virtual void Solve()
	{
		if (iterations > 0)
		{
			for (int i = 0; i < iterations; i++)
			{
				if (OnPreIteration != null)
				{
					OnPreIteration(i);
				}
				for (int j = 0; j < effectors.Length; j++)
				{
					if (effectors[j].isEndEffector)
					{
						effectors[j].Update();
					}
				}
				chain[0].Push(chain);
				chain[0].Reach(chain);
				for (int k = 0; k < effectors.Length; k++)
				{
					if (!effectors[k].isEndEffector)
					{
						effectors[k].Update();
					}
				}
				chain[0].SolveTrigonometric(chain);
				chain[0].Stage1(chain);
				for (int l = 0; l < effectors.Length; l++)
				{
					if (!effectors[l].isEndEffector)
					{
						effectors[l].Update();
					}
				}
				chain[0].Stage2(chain[0].nodes[0].solverPosition, iterations, chain);
				if (OnPostIteration != null)
				{
					OnPostIteration(i);
				}
			}
		}
		if (OnPreBend != null)
		{
			OnPreBend();
		}
		for (int m = 0; m < effectors.Length; m++)
		{
			if (effectors[m].isEndEffector)
			{
				effectors[m].Update();
			}
		}
		ApplyBendConstraints();
	}

	protected virtual void ApplyBendConstraints()
	{
		chain[0].SolveTrigonometric(chain, calculateBendDirection: true);
	}

	protected virtual void WritePose()
	{
		if (iterations > 0)
		{
			spineMapping.WritePose();
			for (int i = 0; i < boneMappings.Length; i++)
			{
				boneMappings[i].WritePose();
			}
		}
		for (int j = 0; j < limbMappings.Length; j++)
		{
			limbMappings[j].WritePose(iterations > 0);
		}
	}
}
