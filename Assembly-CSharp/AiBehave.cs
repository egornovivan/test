using System.Collections;
using Behave.Runtime;
using UnityEngine;

public class AiBehave : AiBehaveTree, IAgent
{
	public string treeName;

	private bool m_Pause;

	private bool m_isActive;

	private Behave.Runtime.Tree m_tree;

	private BehaveResult m_result;

	public virtual bool isActive
	{
		get
		{
			return m_isActive && !m_Pause;
		}
		set
		{
			if (value || m_result != 0)
			{
				m_isActive = value;
			}
		}
	}

	public virtual bool isPause
	{
		set
		{
			m_Pause = value;
		}
	}

	public bool running => m_result == BehaveResult.Running;

	public override void Reset()
	{
		base.Reset();
		if (m_tree != null)
		{
			m_tree.Reset();
		}
	}

	private void Start()
	{
		m_tree = null;
		m_isActive = true;
		m_result = BehaveResult.Failure;
		base.gameObject.layer = 0;
	}

	private void OnDestroy()
	{
	}

	private void OnEnable()
	{
		ActiveBehaveTree(value: true);
	}

	private void OnDisable()
	{
		ActiveBehaveTree(value: false);
	}

	private void ActiveBehaveTree(bool value)
	{
		if (value)
		{
			StopAllCoroutines();
			StartCoroutine(Logic());
			return;
		}
		if (m_tree != null)
		{
			m_tree.Reset();
		}
		StopAllCoroutines();
	}

	private IEnumerator Logic()
	{
		while (Application.isPlaying && m_tree != null)
		{
			if (isActive)
			{
				m_result = AiUpdate();
			}
			yield return new WaitForSeconds(1f / m_tree.Frequency);
		}
	}

	private BehaveResult AiUpdate()
	{
		return m_tree.Tick();
	}

	public BehaveResult Tick(Behave.Runtime.Tree sender)
	{
		return BehaveResult.Success;
	}

	public void Reset(Behave.Runtime.Tree sender)
	{
	}

	public int SelectTopPriority(Behave.Runtime.Tree sender, params int[] IDs)
	{
		return IDs[0];
	}
}
