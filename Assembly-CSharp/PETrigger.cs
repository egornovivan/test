using System;
using UnityEngine;

public class PETrigger : MonoBehaviour
{
	private bool m_AddRigidBody;

	private Rigidbody m_Rigidbody;

	private event TriggerDelegate TriggerEnterEvent;

	private event TriggerDelegate TriggerStayEvent;

	private event TriggerDelegate TriggerExitEvent;

	public static void AttachTriggerEvent(GameObject obj, TriggerDelegate enter, TriggerDelegate stay, TriggerDelegate exit)
	{
		if (obj != null)
		{
			Collider[] componentsInChildren = obj.GetComponentsInChildren<Collider>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				AttachTriggerEvent(componentsInChildren[i], enter, stay, exit);
			}
		}
	}

	public static void AttachTriggerEvent(Collider collider, TriggerDelegate enter, TriggerDelegate stay, TriggerDelegate exit)
	{
		if (collider != null && collider.isTrigger)
		{
			PETrigger pETrigger = collider.gameObject.GetComponent<PETrigger>();
			if (pETrigger == null)
			{
				pETrigger = collider.gameObject.AddComponent<PETrigger>();
			}
			if (pETrigger != null)
			{
				pETrigger.AttachTrigger(enter, stay, exit);
			}
		}
	}

	public static void DetachTriggerEvent(GameObject obj, TriggerDelegate enter, TriggerDelegate stay, TriggerDelegate exit)
	{
		if (obj != null)
		{
			Collider[] componentsInChildren = obj.GetComponentsInChildren<Collider>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				DetachTriggerEvent(componentsInChildren[i], enter, stay, exit);
			}
		}
	}

	public static void DetachTriggerEvent(Collider collider, TriggerDelegate enter, TriggerDelegate stay, TriggerDelegate exit)
	{
		if (collider == null && collider.isTrigger)
		{
			PETrigger component = collider.gameObject.GetComponent<PETrigger>();
			if (component != null)
			{
				component.DetachTrigger(enter, stay, exit);
			}
			UnityEngine.Object.Destroy(component);
		}
	}

	public void AttachTrigger(TriggerDelegate enter, TriggerDelegate stay, TriggerDelegate exit)
	{
		m_Rigidbody = GetComponent<Rigidbody>();
		if (m_Rigidbody == null)
		{
			m_AddRigidBody = true;
			m_Rigidbody = base.gameObject.AddComponent<Rigidbody>();
			m_Rigidbody.useGravity = false;
			m_Rigidbody.isKinematic = true;
		}
		this.TriggerEnterEvent = (TriggerDelegate)Delegate.Combine(this.TriggerEnterEvent, enter);
		this.TriggerStayEvent = (TriggerDelegate)Delegate.Combine(this.TriggerStayEvent, stay);
		this.TriggerExitEvent = (TriggerDelegate)Delegate.Combine(this.TriggerExitEvent, exit);
	}

	public void DetachTrigger(TriggerDelegate enter, TriggerDelegate stay, TriggerDelegate exit)
	{
		if (m_AddRigidBody && m_Rigidbody != null)
		{
			m_AddRigidBody = false;
			UnityEngine.Object.Destroy(m_Rigidbody);
		}
		this.TriggerEnterEvent = (TriggerDelegate)Delegate.Remove(this.TriggerEnterEvent, enter);
		this.TriggerStayEvent = (TriggerDelegate)Delegate.Remove(this.TriggerStayEvent, stay);
		this.TriggerExitEvent = (TriggerDelegate)Delegate.Remove(this.TriggerExitEvent, exit);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (this.TriggerEnterEvent != null)
		{
			this.TriggerEnterEvent(GetComponent<Collider>(), other);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (this.TriggerExitEvent != null)
		{
			this.TriggerExitEvent(GetComponent<Collider>(), other);
		}
	}
}
