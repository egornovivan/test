using System;
using System.Reflection;
using UnityEngine;

public class ScriptableWindzoneInterface : MonoBehaviour
{
	private Component m_WindzoneComponent;

	private Type m_WindzoneType;

	private object[] m_WindZoneArgs = new object[1];

	public float Radius
	{
		get
		{
			return (float)GetWindZoneValue("get_radius");
		}
		set
		{
			m_WindZoneArgs[0] = value;
			SetWindZoneValue("set_radius", m_WindZoneArgs);
		}
	}

	public float WindMain
	{
		get
		{
			return (float)GetWindZoneValue("get_windMain");
		}
		set
		{
			m_WindZoneArgs[0] = value;
			SetWindZoneValue("set_windMain", m_WindZoneArgs);
		}
	}

	public float WindTurbulence
	{
		get
		{
			return (float)GetWindZoneValue("get_windTurbulence");
		}
		set
		{
			m_WindZoneArgs[0] = value;
			SetWindZoneValue("set_windTurbulence", m_WindZoneArgs);
		}
	}

	public float WindPulseMagnitude
	{
		get
		{
			return (float)GetWindZoneValue("get_windPulseMagnitude");
		}
		set
		{
			m_WindZoneArgs[0] = value;
			SetWindZoneValue("set_windPulseMagnitude", m_WindZoneArgs);
		}
	}

	public float WindPulseFrequency
	{
		get
		{
			return (float)GetWindZoneValue("get_windPulseFrequency");
		}
		set
		{
			m_WindZoneArgs[0] = value;
			SetWindZoneValue("set_windPulseFrequency", m_WindZoneArgs);
		}
	}

	public void Start()
	{
		m_WindzoneComponent = GetComponent("WindZone");
		if (m_WindzoneComponent == null)
		{
			Debug.LogError("Could not find a wind zone to link to: " + this);
			base.enabled = false;
		}
		else
		{
			m_WindzoneType = m_WindzoneComponent.GetType();
		}
	}

	private void SetWindZoneValue(string MemberName, object[] args)
	{
		if (m_WindzoneType != null && !(m_WindzoneComponent == null) && args != null && MemberName != null)
		{
			m_WindzoneType.InvokeMember(MemberName, BindingFlags.Instance | BindingFlags.InvokeMethod, null, m_WindzoneComponent, args);
		}
	}

	private object GetWindZoneValue(string MemberName)
	{
		return m_WindzoneType.InvokeMember(MemberName, BindingFlags.Instance | BindingFlags.InvokeMethod, null, m_WindzoneComponent, null);
	}
}
