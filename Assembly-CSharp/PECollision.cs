using System;
using UnityEngine;

public class PECollision : MonoBehaviour
{
	public Action<Collider, Collision> enter;

	public Action<Collider, Collision> exit;

	private Collider m_Collider;

	public static void Attach(GameObject obj, Action<Collider, Collision> _enter, Action<Collider, Collision> _exit)
	{
		if (!(obj == null))
		{
			PECollision pECollision = obj.GetComponent<PECollision>();
			if (pECollision == null)
			{
				pECollision = obj.AddComponent<PECollision>();
			}
			PECollision pECollision2 = pECollision;
			pECollision2.enter = (Action<Collider, Collision>)Delegate.Combine(pECollision2.enter, _enter);
			PECollision pECollision3 = pECollision;
			pECollision3.exit = (Action<Collider, Collision>)Delegate.Combine(pECollision3.exit, _exit);
		}
	}

	public static void Dettach(GameObject obj, Action<Collider, Collision> _enter, Action<Collider, Collision> _exit)
	{
		if (!(obj == null))
		{
			PECollision component = obj.GetComponent<PECollision>();
			if (component != null)
			{
				component.enter = (Action<Collider, Collision>)Delegate.Remove(component.enter, _enter);
				component.exit = (Action<Collider, Collision>)Delegate.Remove(component.exit, _exit);
			}
			UnityEngine.Object.Destroy(component);
		}
	}

	private void Awake()
	{
		m_Collider = GetComponent<Collider>();
	}

	private void OnCollisionEnter(Collision info)
	{
		if (enter != null)
		{
			enter(m_Collider, info);
		}
	}

	private void OnCollisionExit(Collision info)
	{
		if (exit != null)
		{
			exit(m_Collider, info);
		}
	}
}
