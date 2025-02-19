using PETools;
using UnityEngine;

public class torch : MonoBehaviour
{
	[SerializeField]
	private GameObject m_fire;

	private bool mBurningEnv = true;

	public void SetBurning(bool v)
	{
		if (!(m_fire == null))
		{
			m_fire.SetActive(v && mBurningEnv);
		}
	}

	public bool IsBurning()
	{
		if (m_fire == null)
		{
			return false;
		}
		return m_fire.activeSelf;
	}

	private void Start()
	{
		SetBurning(v: true);
	}

	private bool CheckBurningEnv()
	{
		return !(PE.PointInWater(base.transform.position) > 0.5f);
	}

	private void Update()
	{
		mBurningEnv = CheckBurningEnv();
		if (IsBurning() && !mBurningEnv)
		{
			SetBurning(v: false);
		}
	}
}
