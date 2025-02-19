using UnityEngine;

namespace Railway;

public class RailwaySeat : MonoBehaviour
{
	[SerializeField]
	private string[] m_Pose;

	private IPassenger mPassenger;

	public IPassenger passenger => mPassenger;

	public bool SetPassenger(IPassenger passenger)
	{
		if (passenger == null || mPassenger != null)
		{
			return false;
		}
		mPassenger = passenger;
		mPassenger.GetOn(m_Pose[Random.Range(0, m_Pose.Length - 1)]);
		return true;
	}

	public bool ResetPassenger(Vector3 pos)
	{
		if (mPassenger == null)
		{
			return false;
		}
		mPassenger.GetOff(pos);
		mPassenger = null;
		return true;
	}

	private void Update()
	{
		if (mPassenger != null && !mPassenger.Equals(null))
		{
			mPassenger.UpdateTrans(base.transform);
		}
	}
}
