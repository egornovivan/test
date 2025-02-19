using System.Collections;
using UnityEngine;

public class BTCoroutine : IEnumerator
{
	private IEnumerator m_Enumerator;

	private MonoBehaviour m_Behaviour;

	private Coroutine m_Coroutine;

	public bool IsStart => m_Coroutine != null;

	public object Current => m_Enumerator.Current;

	public BTCoroutine(MonoBehaviour behaviour, IEnumerator enumerator)
	{
		m_Behaviour = behaviour;
		m_Enumerator = enumerator;
	}

	public void Start()
	{
		if (m_Behaviour != null && m_Enumerator != null && m_Coroutine == null)
		{
			m_Coroutine = m_Behaviour.StartCoroutine(m_Enumerator);
		}
	}

	public void Stop()
	{
		if (m_Behaviour != null && m_Enumerator != null && m_Coroutine != null)
		{
			m_Coroutine = null;
			m_Behaviour.StopCoroutine(m_Enumerator);
		}
	}

	public bool MoveNext()
	{
		return m_Enumerator.MoveNext();
	}

	public void Reset()
	{
		m_Enumerator.Reset();
	}
}
