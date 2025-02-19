using UnityEngine;

public class AiBehaveTree : MonoBehaviour
{
	private bool m_valid = true;

	public virtual bool valid
	{
		get
		{
			return m_valid;
		}
		set
		{
			if (!value && m_valid)
			{
				Reset();
			}
			m_valid = value;
		}
	}

	public virtual bool isMember => false;

	public virtual bool isSingle => false;

	public virtual bool isGroup => false;

	public virtual void Reset()
	{
	}
}
