using Pathea;
using UnityEngine;

public class PEMount : MonoBehaviour
{
	private Transform m_Target;

	public Transform Target
	{
		set
		{
			if (!(m_Target != value))
			{
				return;
			}
			if (m_Target != null)
			{
				Motion_Move componentInChildren = m_Target.GetComponentInChildren<Motion_Move>();
				if (componentInChildren != null)
				{
					componentInChildren.enabled = false;
				}
			}
			m_Target = value;
			if (m_Target != null)
			{
				Motion_Move componentInChildren2 = m_Target.GetComponentInChildren<Motion_Move>();
				if (componentInChildren2 != null)
				{
					componentInChildren2.enabled = false;
				}
			}
		}
	}

	private void Update()
	{
		if (m_Target != null)
		{
			m_Target.position = base.transform.position;
			m_Target.rotation = base.transform.rotation;
		}
	}
}
