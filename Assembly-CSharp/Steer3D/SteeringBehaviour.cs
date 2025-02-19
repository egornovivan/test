using System;
using UnityEngine;

namespace Steer3D;

[RequireComponent(typeof(SteerAgent))]
public abstract class SteeringBehaviour : MonoBehaviour
{
	public bool active = true;

	protected SteerAgent agent;

	public BehaviourType type => (BehaviourType)(int)Enum.Parse(typeof(BehaviourType), GetType().Name);

	protected Vector3 position => agent.position;

	public abstract bool idle { get; }

	protected virtual void Awake()
	{
		agent = GetComponent<SteerAgent>();
		agent.RegisterBehaviour(this);
	}

	protected virtual void OnDestroy()
	{
		agent.DeregisterBehaviour(this);
	}

	public abstract void Behave();
}
