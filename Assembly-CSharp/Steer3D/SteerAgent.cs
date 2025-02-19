using System;
using System.Collections.Generic;
using UnityEngine;

namespace Steer3D;

public class SteerAgent : MonoBehaviour
{
	public Dimension dimension = Dimension.TwoD;

	public float maxSpeed = 5f;

	public float inertia = 1f;

	public MonoBehaviour motor;

	private Vector3 steerVector = Vector3.zero;

	private Vector3 force = Vector3.zero;

	private static float steerRadius = 0.2f;

	public bool manualUpdate;

	private Dictionary<BehaviourType, SteeringBehaviour> behaviours = new Dictionary<BehaviourType, SteeringBehaviour>();

	public Vector3 velocity
	{
		get
		{
			float magnitude = steerVector.magnitude;
			if (magnitude < steerRadius * 1.00001f)
			{
				return Vector3.zero;
			}
			Vector3 result = steerVector / magnitude * (magnitude - steerRadius);
			if (result.sqrMagnitude < 1E-07f)
			{
				return Vector3.zero;
			}
			return result;
		}
		set
		{
			float magnitude = value.magnitude;
			if (magnitude < 0.0001f)
			{
				steerVector = steerVector.normalized * steerRadius;
			}
			else
			{
				steerVector = value.normalized * (magnitude + steerRadius);
			}
		}
	}

	public Vector3 forward => steerVector.normalized;

	public Vector3 position => base.transform.position;

	public bool idle
	{
		get
		{
			if (behaviours != null)
			{
				foreach (KeyValuePair<BehaviourType, SteeringBehaviour> behaviour in behaviours)
				{
					if (behaviour.Value == null || behaviour.Value.idle)
					{
						continue;
					}
					return false;
				}
			}
			return true;
		}
	}

	private Vector3 _velocityToSteerVec(Vector3 vel)
	{
		float magnitude = vel.magnitude;
		if (magnitude < 0.0001f)
		{
			return steerVector.normalized * steerRadius;
		}
		return vel.normalized * (magnitude + steerRadius);
	}

	private void ResetUpdateState()
	{
		force = Vector3.zero;
	}

	public void AddDesiredVelocity(Vector3 desired_vel, float multiplier, float spherical = 0.5f)
	{
		Vector3 vector = _velocityToSteerVec(desired_vel);
		Vector3 vector2 = vector - steerVector;
		Vector3 vector3 = Vector3.Slerp(steerVector, vector, Mathf.Clamp(1f - spherical, 0.02f, 1f));
		Vector3 vector4 = vector3 - steerVector;
		float magnitude = vector2.magnitude;
		float magnitude2 = vector4.magnitude;
		if (magnitude > 0.0001f && magnitude2 > 0.0001f)
		{
			vector4 *= magnitude / magnitude2;
			force += vector4 * multiplier * 5f;
		}
	}

	private void UpdateSteer()
	{
		inertia = Mathf.Max(0.001f, inertia);
		steerVector += force / inertia * Time.deltaTime;
		if (dimension == Dimension.TwoD)
		{
			steerVector.y = 0f;
		}
		if (steerVector.magnitude < steerRadius)
		{
			steerVector = steerVector.normalized * steerRadius;
		}
	}

	private void MotorMessage()
	{
		if (motor != null)
		{
			motor.SendMessage("OnSteer");
		}
	}

	private void Start()
	{
		steerVector = base.transform.forward;
		if (dimension == Dimension.TwoD)
		{
			steerVector.y = 0f;
			steerVector.Normalize();
			if (steerVector.magnitude < 0.001f)
			{
				steerVector = Vector3.forward;
			}
		}
		steerVector *= steerRadius;
	}

	private void Update()
	{
		if (!manualUpdate)
		{
			ManualUpdate();
		}
	}

	public void ManualUpdate()
	{
		UpdateBehaviours();
		UpdateSteer();
		MotorMessage();
		ResetUpdateState();
	}

	public Seek AlterSeekBehaviour(Vector3 target, float slowingRadius, float arriveRadius, float weight = 1f)
	{
		Seek seek = InvokeBehaviour<Seek>();
		seek.target = target;
		seek.targetTrans = null;
		seek.weight = weight;
		seek.slowingRadius = slowingRadius;
		seek.arriveRadius = arriveRadius;
		return seek;
	}

	public Seek AlterSeekBehaviour(Transform target, float slowingRadius, float arriveRadius, float weight = 1f)
	{
		Seek seek = InvokeBehaviour<Seek>();
		seek.targetTrans = target;
		seek.weight = weight;
		seek.slowingRadius = slowingRadius;
		seek.arriveRadius = arriveRadius;
		return seek;
	}

	public Flee AlterFleeBehaviour(Vector3 target, float affectRadius, float fleeRadius, float forbiddenRadius, float weight = 1f)
	{
		Flee flee = InvokeBehaviour<Flee>();
		flee.target = target;
		flee.weight = weight;
		flee.affectRadius = affectRadius;
		flee.fleeRadius = fleeRadius;
		flee.forbiddenRadius = forbiddenRadius;
		return flee;
	}

	public Flee AlterFleeBehaviour(Transform target, float affectRadius, float fleeRadius, float forbiddenRadius, float weight = 1f)
	{
		Flee flee = InvokeBehaviour<Flee>();
		flee.targetTrans = target;
		flee.weight = weight;
		flee.affectRadius = affectRadius;
		flee.fleeRadius = fleeRadius;
		flee.forbiddenRadius = forbiddenRadius;
		return flee;
	}

	public Flees AlterFleesBehaviour()
	{
		return InvokeBehaviour<Flees>();
	}

	public Pursue AlterPursueBehaviour(Vector3 target, float slowingRadius, float arriveRadius, float weight = 1f)
	{
		Pursue pursue = InvokeBehaviour<Pursue>();
		pursue.target = target;
		pursue.weight = weight;
		pursue.slowingRadius = slowingRadius;
		pursue.arriveRadius = arriveRadius;
		return pursue;
	}

	public Pursue AlterPursueBehaviour(Transform target, float slowingRadius, float arriveRadius, float weight = 1f)
	{
		Pursue pursue = InvokeBehaviour<Pursue>();
		pursue.targetTrans = target;
		pursue.weight = weight;
		pursue.slowingRadius = slowingRadius;
		pursue.arriveRadius = arriveRadius;
		return pursue;
	}

	public Evade AlterEvadeBehaviour(Vector3 target, float affectRadius, float fleeRadius, float forbiddenRadius, float weight = 1f)
	{
		Evade evade = InvokeBehaviour<Evade>();
		evade.target = target;
		evade.weight = weight;
		evade.affectRadius = affectRadius;
		evade.fleeRadius = fleeRadius;
		evade.forbiddenRadius = forbiddenRadius;
		return evade;
	}

	public Evade AlterEvadeBehaviour(Transform target, float affectRadius, float fleeRadius, float forbiddenRadius, float weight = 1f)
	{
		Evade evade = InvokeBehaviour<Evade>();
		evade.targetTrans = target;
		evade.weight = weight;
		evade.affectRadius = affectRadius;
		evade.fleeRadius = fleeRadius;
		evade.forbiddenRadius = forbiddenRadius;
		return evade;
	}

	public PathFollow AlterPathFollowBehaviour(Vector3[] path, float slowingRadius, float arriveRadius, float weight = 1f)
	{
		PathFollow pathFollow = InvokeBehaviour<PathFollow>();
		pathFollow.Reset(path);
		pathFollow.weight = weight;
		pathFollow.slowingRadius = slowingRadius;
		pathFollow.arriveRadius = arriveRadius;
		return pathFollow;
	}

	public T InvokeBehaviour<T>() where T : SteeringBehaviour
	{
		BehaviourType key = (BehaviourType)(int)Enum.Parse(typeof(BehaviourType), typeof(T).Name);
		if (behaviours.ContainsKey(key))
		{
			return (T)behaviours[key];
		}
		return base.gameObject.AddComponent<T>();
	}

	public T FindBehaviour<T>() where T : SteeringBehaviour
	{
		BehaviourType key = (BehaviourType)(int)Enum.Parse(typeof(BehaviourType), typeof(T).Name);
		if (behaviours.ContainsKey(key))
		{
			return (T)behaviours[key];
		}
		return (T)null;
	}

	public T SetBehaviourActive<T>(bool active) where T : SteeringBehaviour
	{
		T val = FindBehaviour<T>();
		if (val != null)
		{
			val.active = active;
		}
		return val;
	}

	public bool RemoveBehaviour<T>() where T : SteeringBehaviour
	{
		T val = FindBehaviour<T>();
		if (val != null)
		{
			UnityEngine.Object.Destroy(val);
			DeregisterBehaviour(val);
			return true;
		}
		return false;
	}

	internal void RegisterBehaviour(SteeringBehaviour behaviour)
	{
		if (behaviours == null)
		{
			behaviours = new Dictionary<BehaviourType, SteeringBehaviour>();
		}
		BehaviourType type = behaviour.type;
		if (behaviours.ContainsKey(type))
		{
			UnityEngine.Object.Destroy(behaviour);
			DeregisterBehaviour(behaviour);
		}
		behaviours[type] = behaviour;
	}

	internal void DeregisterBehaviour(SteeringBehaviour behaviour)
	{
		if (behaviours != null)
		{
			BehaviourType type = behaviour.type;
			if (behaviours.ContainsKey(type) && behaviours[type] == behaviour)
			{
				behaviours.Remove(type);
			}
		}
	}

	private void UpdateBehaviours()
	{
		bool flag = true;
		if (behaviours != null)
		{
			foreach (KeyValuePair<BehaviourType, SteeringBehaviour> behaviour in behaviours)
			{
				SteeringBehaviour value = behaviour.Value;
				if (value != null && value.enabled && value.active)
				{
					if (!value.idle)
					{
						flag = false;
					}
					value.Behave();
				}
			}
		}
		if (flag)
		{
			AddDesiredVelocity(Vector3.zero, 1f);
		}
	}
}
