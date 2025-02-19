using UnityEngine;

public class OpenL1 : MonoBehaviour
{
	private void Start()
	{
		if (!(MissionManager.Instance == null) && MissionManager.Instance.m_PlayerMission.HadCompleteMission(606))
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
