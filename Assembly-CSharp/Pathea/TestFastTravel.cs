using UnityEngine;

namespace Pathea;

public class TestFastTravel : MonoBehaviour
{
	public bool m_Moved = true;

	private void Update()
	{
		if (!m_Moved)
		{
			PeSingleton<FastTravelMgr>.Instance.TravelTo(base.transform.position);
			m_Moved = true;
		}
	}
}
