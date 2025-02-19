using System;
using UnityEngine;

public class VCEUIVector3Input : MonoBehaviour
{
	public UIInput m_XInput;

	public UIInput m_YInput;

	public UIInput m_ZInput;

	public bool m_KeepUniform;

	public string m_Format = "0.00";

	public Vector3 m_Vector = Vector3.zero;

	public GameObject m_EventReceiver;

	public string m_OnChangeFuncName = "OnVectorChange";

	private Vector3 m_lastVector;

	private bool m_lastXSelected;

	private bool m_lastYSelected;

	private bool m_lastZSelected;

	private string m_revertXStr = string.Empty;

	private string m_revertYStr = string.Empty;

	private string m_revertZStr = string.Empty;

	public Vector3 Vector
	{
		get
		{
			return m_Vector;
		}
		set
		{
			if (!m_XInput.selected)
			{
				m_XInput.text = value.x.ToString(m_Format);
			}
			if (!m_YInput.selected)
			{
				m_YInput.text = value.y.ToString(m_Format);
			}
			if (!m_ZInput.selected)
			{
				m_ZInput.text = value.z.ToString(m_Format);
			}
			GetVectorFromInput();
		}
	}

	private void GetVectorFromInput()
	{
		if (!m_XInput.selected)
		{
			try
			{
				m_Vector.x = Convert.ToSingle(m_XInput.text);
			}
			catch (Exception)
			{
				m_Vector.x = 0f;
			}
		}
		if (!m_YInput.selected)
		{
			try
			{
				m_Vector.y = Convert.ToSingle(m_YInput.text);
			}
			catch (Exception)
			{
				m_Vector.y = 0f;
			}
		}
		if (!m_ZInput.selected)
		{
			try
			{
				m_Vector.z = Convert.ToSingle(m_ZInput.text);
			}
			catch (Exception)
			{
				m_Vector.z = 0f;
			}
		}
	}

	private void Start()
	{
		m_lastVector = m_Vector;
	}

	private void Update()
	{
		if (m_XInput.selected && !m_lastXSelected)
		{
			m_revertXStr = m_XInput.text;
			m_XInput.text = string.Empty;
			m_lastXSelected = m_XInput.selected;
		}
		else if (!m_XInput.selected && m_lastXSelected)
		{
			if (m_XInput.text.Trim().Length == 0)
			{
				m_XInput.text = m_revertXStr;
			}
			m_revertXStr = string.Empty;
			m_lastXSelected = m_XInput.selected;
			GetVectorFromInput();
			if (m_KeepUniform)
			{
				m_Vector.y = (m_Vector.z = m_Vector.x);
			}
		}
		if (m_YInput.selected && !m_lastYSelected)
		{
			m_revertYStr = m_YInput.text;
			m_YInput.text = string.Empty;
			m_lastYSelected = m_YInput.selected;
		}
		else if (!m_YInput.selected && m_lastYSelected)
		{
			if (m_YInput.text.Trim().Length == 0)
			{
				m_YInput.text = m_revertYStr;
			}
			m_revertYStr = string.Empty;
			m_lastYSelected = m_YInput.selected;
			GetVectorFromInput();
			if (m_KeepUniform)
			{
				m_Vector.x = (m_Vector.z = m_Vector.y);
			}
		}
		if (m_ZInput.selected && !m_lastZSelected)
		{
			m_revertZStr = m_ZInput.text;
			m_ZInput.text = string.Empty;
			m_lastZSelected = m_ZInput.selected;
		}
		else if (!m_ZInput.selected && m_lastZSelected)
		{
			if (m_ZInput.text.Trim().Length == 0)
			{
				m_ZInput.text = m_revertZStr;
			}
			m_revertZStr = string.Empty;
			m_lastZSelected = m_ZInput.selected;
			GetVectorFromInput();
			if (m_KeepUniform)
			{
				m_Vector.x = (m_Vector.y = m_Vector.z);
			}
		}
		if (m_Vector != m_lastVector)
		{
			if (m_EventReceiver == null)
			{
				m_EventReceiver = base.gameObject;
			}
			m_EventReceiver.SendMessage(m_OnChangeFuncName, m_Vector, SendMessageOptions.DontRequireReceiver);
			m_lastVector = m_Vector;
		}
		Vector = m_Vector;
	}
}
