using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class BSGizmoTriggerEvent : MonoBehaviour
{
	private Dictionary<int, Rigidbody> m_Rigidbodys = new Dictionary<int, Rigidbody>();

	private BoxCollider m_Collider;

	public BoxCollider boxCollider => m_Collider;

	public bool RayCast => m_Rigidbodys.Count != 0;

	private void Awake()
	{
		m_Collider = base.gameObject.GetComponent<BoxCollider>();
	}

	private void OnDisable()
	{
		m_Rigidbodys.Clear();
	}

	private void OnTriggerEnter(Collider other)
	{
	}

	private void OnTriggerStay(Collider other)
	{
		Rigidbody component = other.gameObject.GetComponent<Rigidbody>();
		if (component != null)
		{
			int instanceID = other.gameObject.GetInstanceID();
			if (!m_Rigidbodys.ContainsKey(instanceID))
			{
			}
			m_Rigidbodys[instanceID] = component;
			return;
		}
		component = other.gameObject.GetComponentInParent<Rigidbody>();
		if (component != null)
		{
			int instanceID2 = other.gameObject.GetInstanceID();
			if (!m_Rigidbodys.ContainsKey(instanceID2))
			{
			}
			m_Rigidbodys[instanceID2] = component;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (!m_Rigidbodys.Remove(other.gameObject.GetInstanceID()))
		{
		}
	}
}
