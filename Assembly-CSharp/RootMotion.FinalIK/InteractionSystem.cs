using System;
using System.Collections.Generic;
using UnityEngine;

namespace RootMotion.FinalIK;

[AddComponentMenu("Scripts/RootMotion.FinalIK/Interaction System/Interaction System")]
public class InteractionSystem : MonoBehaviour
{
	public delegate void InteractionDelegate(FullBodyBipedEffector effectorType, InteractionObject interactionObject);

	public delegate void InteractionEventDelegate(FullBodyBipedEffector effectorType, InteractionObject interactionObject, InteractionObject.InteractionEvent interactionEvent);

	public string targetTag = string.Empty;

	public float fadeInTime = 0.3f;

	public float speed = 1f;

	public float resetToDefaultsSpeed = 1f;

	private List<InteractionTrigger> inContact = new List<InteractionTrigger>();

	private List<int> bestRangeIndexes = new List<int>();

	public InteractionDelegate OnInteractionStart;

	public InteractionDelegate OnInteractionPause;

	public InteractionDelegate OnInteractionPickUp;

	public InteractionDelegate OnInteractionResume;

	public InteractionDelegate OnInteractionStop;

	public InteractionEventDelegate OnInteractionEvent;

	[SerializeField]
	private FullBodyBipedIK fullBody;

	public InteractionLookAt lookAt = new InteractionLookAt();

	private InteractionEffector[] interactionEffectors = new InteractionEffector[9]
	{
		new InteractionEffector(FullBodyBipedEffector.Body),
		new InteractionEffector(FullBodyBipedEffector.LeftFoot),
		new InteractionEffector(FullBodyBipedEffector.LeftHand),
		new InteractionEffector(FullBodyBipedEffector.LeftShoulder),
		new InteractionEffector(FullBodyBipedEffector.LeftThigh),
		new InteractionEffector(FullBodyBipedEffector.RightFoot),
		new InteractionEffector(FullBodyBipedEffector.RightHand),
		new InteractionEffector(FullBodyBipedEffector.RightShoulder),
		new InteractionEffector(FullBodyBipedEffector.RightThigh)
	};

	private bool initiated;

	public bool inInteraction
	{
		get
		{
			if (!IsValid(log: true))
			{
				return false;
			}
			for (int i = 0; i < interactionEffectors.Length; i++)
			{
				if (interactionEffectors[i].inInteraction && !interactionEffectors[i].isPaused)
				{
					return true;
				}
			}
			return false;
		}
	}

	public FullBodyBipedIK ik => fullBody;

	public List<InteractionTrigger> triggersInRange { get; private set; }

	public bool IsInInteraction(FullBodyBipedEffector effectorType)
	{
		if (!IsValid(log: true))
		{
			return false;
		}
		for (int i = 0; i < interactionEffectors.Length; i++)
		{
			if (interactionEffectors[i].effectorType == effectorType)
			{
				return interactionEffectors[i].inInteraction && !interactionEffectors[i].isPaused;
			}
		}
		return false;
	}

	public bool IsPaused(FullBodyBipedEffector effectorType)
	{
		if (!IsValid(log: true))
		{
			return false;
		}
		for (int i = 0; i < interactionEffectors.Length; i++)
		{
			if (interactionEffectors[i].effectorType == effectorType)
			{
				return interactionEffectors[i].inInteraction && interactionEffectors[i].isPaused;
			}
		}
		return false;
	}

	public bool IsPaused()
	{
		if (!IsValid(log: true))
		{
			return false;
		}
		for (int i = 0; i < interactionEffectors.Length; i++)
		{
			if (interactionEffectors[i].inInteraction && interactionEffectors[i].isPaused)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsInSync()
	{
		if (!IsValid(log: true))
		{
			return false;
		}
		for (int i = 0; i < interactionEffectors.Length; i++)
		{
			if (!interactionEffectors[i].isPaused)
			{
				continue;
			}
			for (int j = 0; j < interactionEffectors.Length; j++)
			{
				if (j != i && interactionEffectors[j].inInteraction && !interactionEffectors[j].isPaused)
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool StartInteraction(FullBodyBipedEffector effectorType, InteractionObject interactionObject, bool interrupt)
	{
		if (!IsValid(log: true))
		{
			return false;
		}
		if (interactionObject == null)
		{
			return false;
		}
		for (int i = 0; i < interactionEffectors.Length; i++)
		{
			if (interactionEffectors[i].effectorType == effectorType)
			{
				return interactionEffectors[i].Start(interactionObject, targetTag, fadeInTime, interrupt);
			}
		}
		return false;
	}

	public bool PauseInteraction(FullBodyBipedEffector effectorType)
	{
		if (!IsValid(log: true))
		{
			return false;
		}
		for (int i = 0; i < interactionEffectors.Length; i++)
		{
			if (interactionEffectors[i].effectorType == effectorType)
			{
				return interactionEffectors[i].Pause();
			}
		}
		return false;
	}

	public bool ResumeInteraction(FullBodyBipedEffector effectorType)
	{
		if (!IsValid(log: true))
		{
			return false;
		}
		for (int i = 0; i < interactionEffectors.Length; i++)
		{
			if (interactionEffectors[i].effectorType == effectorType)
			{
				return interactionEffectors[i].Resume();
			}
		}
		return false;
	}

	public bool StopInteraction(FullBodyBipedEffector effectorType)
	{
		if (!IsValid(log: true))
		{
			return false;
		}
		for (int i = 0; i < interactionEffectors.Length; i++)
		{
			if (interactionEffectors[i].effectorType == effectorType)
			{
				return interactionEffectors[i].Stop();
			}
		}
		return false;
	}

	public void PauseAll()
	{
		if (IsValid(log: true))
		{
			for (int i = 0; i < interactionEffectors.Length; i++)
			{
				interactionEffectors[i].Pause();
			}
		}
	}

	public void ResumeAll()
	{
		if (IsValid(log: true))
		{
			for (int i = 0; i < interactionEffectors.Length; i++)
			{
				interactionEffectors[i].Resume();
			}
		}
	}

	public void StopAll()
	{
		for (int i = 0; i < interactionEffectors.Length; i++)
		{
			interactionEffectors[i].Stop();
		}
	}

	public InteractionObject GetInteractionObject(FullBodyBipedEffector effectorType)
	{
		if (!IsValid(log: true))
		{
			return null;
		}
		for (int i = 0; i < interactionEffectors.Length; i++)
		{
			if (interactionEffectors[i].effectorType == effectorType)
			{
				return interactionEffectors[i].interactionObject;
			}
		}
		return null;
	}

	public float GetProgress(FullBodyBipedEffector effectorType)
	{
		if (!IsValid(log: true))
		{
			return 0f;
		}
		for (int i = 0; i < interactionEffectors.Length; i++)
		{
			if (interactionEffectors[i].effectorType == effectorType)
			{
				return interactionEffectors[i].progress;
			}
		}
		return 0f;
	}

	public float GetMinActiveProgress()
	{
		if (!IsValid(log: true))
		{
			return 0f;
		}
		float num = 1f;
		for (int i = 0; i < interactionEffectors.Length; i++)
		{
			if (interactionEffectors[i].inInteraction)
			{
				float progress = interactionEffectors[i].progress;
				if (progress > 0f && progress < num)
				{
					num = progress;
				}
			}
		}
		return num;
	}

	public bool TriggerInteraction(int index, bool interrupt)
	{
		if (!IsValid(log: true))
		{
			return false;
		}
		if (!TriggerIndexIsValid(index))
		{
			return false;
		}
		bool result = true;
		InteractionTrigger.Range range = triggersInRange[index].ranges[bestRangeIndexes[index]];
		for (int i = 0; i < range.interactions.Length; i++)
		{
			for (int j = 0; j < range.interactions[i].effectors.Length; j++)
			{
				if (!StartInteraction(range.interactions[i].effectors[j], range.interactions[i].interactionObject, interrupt))
				{
					result = false;
				}
			}
		}
		return result;
	}

	public bool TriggerEffectorsReady(int index)
	{
		if (!IsValid(log: true))
		{
			return false;
		}
		if (!TriggerIndexIsValid(index))
		{
			return false;
		}
		for (int i = 0; i < triggersInRange[index].ranges.Length; i++)
		{
			InteractionTrigger.Range range = triggersInRange[index].ranges[i];
			for (int j = 0; j < range.interactions.Length; j++)
			{
				for (int k = 0; k < range.interactions[j].effectors.Length; k++)
				{
					if (IsInInteraction(range.interactions[j].effectors[k]))
					{
						return false;
					}
				}
			}
			for (int l = 0; l < range.interactions.Length; l++)
			{
				for (int m = 0; m < range.interactions[l].effectors.Length; m++)
				{
					if (!IsPaused(range.interactions[l].effectors[m]))
					{
						continue;
					}
					for (int n = 0; n < range.interactions[l].effectors.Length; n++)
					{
						if (n != m && !IsPaused(range.interactions[l].effectors[n]))
						{
							return false;
						}
					}
				}
			}
		}
		return true;
	}

	public InteractionTrigger.Range GetTriggerRange(int index)
	{
		if (!IsValid(log: true))
		{
			return null;
		}
		if (index < 0 || index >= bestRangeIndexes.Count)
		{
			Warning.Log("Index out of range.", base.transform);
			return null;
		}
		return triggersInRange[index].ranges[bestRangeIndexes[index]];
	}

	public int GetClosestTriggerIndex()
	{
		if (!IsValid(log: true))
		{
			return -1;
		}
		if (triggersInRange.Count == 0)
		{
			return -1;
		}
		if (triggersInRange.Count == 1)
		{
			return 0;
		}
		int result = -1;
		float num = float.PositiveInfinity;
		for (int i = 0; i < triggersInRange.Count; i++)
		{
			if (triggersInRange[i] != null)
			{
				float num2 = Vector3.SqrMagnitude(triggersInRange[i].transform.position - base.transform.position);
				if (num2 < num)
				{
					result = i;
					num = num2;
				}
			}
		}
		return result;
	}

	protected virtual void Start()
	{
		if (fullBody == null)
		{
			fullBody = GetComponent<FullBodyBipedIK>();
		}
		if (fullBody == null)
		{
			Warning.Log("InteractionSystem can not find a FullBodyBipedIK component", base.transform);
			return;
		}
		IKSolverFullBodyBiped solver = fullBody.solver;
		solver.OnPreUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(solver.OnPreUpdate, new IKSolver.UpdateDelegate(OnPreFBBIK));
		IKSolverFullBodyBiped solver2 = fullBody.solver;
		solver2.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(solver2.OnPostUpdate, new IKSolver.UpdateDelegate(OnPostFBBIK));
		OnInteractionStart = (InteractionDelegate)Delegate.Combine(OnInteractionStart, new InteractionDelegate(LookAtInteraction));
		InteractionEffector[] array = interactionEffectors;
		foreach (InteractionEffector interactionEffector in array)
		{
			interactionEffector.Initiate(this, fullBody.solver);
		}
		triggersInRange = new List<InteractionTrigger>();
		initiated = true;
	}

	private void LookAtInteraction(FullBodyBipedEffector effector, InteractionObject interactionObject)
	{
		lookAt.Look(interactionObject.lookAtTarget, Time.time + interactionObject.length * 0.5f);
	}

	public void OnTriggerEnter(Collider c)
	{
		if (!(fullBody == null))
		{
			InteractionTrigger component = c.GetComponent<InteractionTrigger>();
			if (!inContact.Contains(component))
			{
				inContact.Add(component);
			}
		}
	}

	public void OnTriggerExit(Collider c)
	{
		if (!(fullBody == null))
		{
			InteractionTrigger component = c.GetComponent<InteractionTrigger>();
			inContact.Remove(component);
		}
	}

	private bool ContactIsInRange(int index, out int bestRangeIndex)
	{
		bestRangeIndex = -1;
		if (!IsValid(log: true))
		{
			return false;
		}
		if (index < 0 || index >= inContact.Count)
		{
			Warning.Log("Index out of range.", base.transform);
			return false;
		}
		if (inContact[index] == null)
		{
			Warning.Log("The InteractionTrigger in the list 'inContact' has been destroyed", base.transform);
			return false;
		}
		bestRangeIndex = inContact[index].GetBestRangeIndex(base.transform);
		if (bestRangeIndex == -1)
		{
			return false;
		}
		return true;
	}

	private void OnDrawGizmosSelected()
	{
		if (fullBody == null)
		{
			fullBody = GetComponent<FullBodyBipedIK>();
		}
	}

	private void Update()
	{
		if (fullBody == null)
		{
			return;
		}
		triggersInRange.Clear();
		bestRangeIndexes.Clear();
		for (int i = 0; i < inContact.Count; i++)
		{
			int bestRangeIndex = -1;
			if (inContact[i] != null && ContactIsInRange(i, out bestRangeIndex))
			{
				triggersInRange.Add(inContact[i]);
				bestRangeIndexes.Add(bestRangeIndex);
			}
		}
		lookAt.Update();
	}

	private void LateUpdate()
	{
		if (!(fullBody == null))
		{
			for (int i = 0; i < interactionEffectors.Length; i++)
			{
				interactionEffectors[i].Update(base.transform, fullBody.solver, speed);
			}
			for (int j = 0; j < interactionEffectors.Length; j++)
			{
				interactionEffectors[j].ResetToDefaults(fullBody.solver, resetToDefaultsSpeed);
			}
		}
	}

	private void OnPreFBBIK()
	{
		if (base.enabled && !(fullBody == null))
		{
			lookAt.SolveSpine();
		}
	}

	private void OnPostFBBIK()
	{
		if (base.enabled && !(fullBody == null))
		{
			for (int i = 0; i < interactionEffectors.Length; i++)
			{
				interactionEffectors[i].OnPostFBBIK(fullBody.solver);
			}
			lookAt.SolveHead();
		}
	}

	private void OnDestroy()
	{
		if (!(fullBody == null))
		{
			IKSolverFullBodyBiped solver = fullBody.solver;
			solver.OnPreUpdate = (IKSolver.UpdateDelegate)Delegate.Remove(solver.OnPreUpdate, new IKSolver.UpdateDelegate(OnPreFBBIK));
			IKSolverFullBodyBiped solver2 = fullBody.solver;
			solver2.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Remove(solver2.OnPostUpdate, new IKSolver.UpdateDelegate(OnPostFBBIK));
			OnInteractionStart = (InteractionDelegate)Delegate.Remove(OnInteractionStart, new InteractionDelegate(LookAtInteraction));
		}
	}

	private bool IsValid(bool log)
	{
		if (fullBody == null)
		{
			if (log)
			{
				Warning.Log("FBBIK is null. Will not update the InteractionSystem", base.transform);
			}
			return false;
		}
		if (!initiated)
		{
			if (log)
			{
				Warning.Log("The InteractionSystem has not been initiated yet.", base.transform);
			}
			return false;
		}
		return true;
	}

	private bool TriggerIndexIsValid(int index)
	{
		if (index < 0 || index >= triggersInRange.Count)
		{
			Warning.Log("Index out of range.", base.transform);
			return false;
		}
		if (triggersInRange[index] == null)
		{
			Warning.Log("The InteractionTrigger in the list 'inContact' has been destroyed", base.transform);
			return false;
		}
		if (triggersInRange[index].target == null)
		{
			Warning.Log("InteractionTrigger has no target Transform", base.transform);
			return false;
		}
		return true;
	}

	[ContextMenu("User Manual")]
	private void OpenUserManual()
	{
		Application.OpenURL("http://www.root-motion.com/finalikdox/html/page10.html");
	}

	[ContextMenu("Scrpt Reference")]
	private void OpenScriptReference()
	{
		Application.OpenURL("http://www.root-motion.com/finalikdox/html/class_root_motion_1_1_final_i_k_1_1_interaction_system.html");
	}
}
