using UnityEngine;

public class McTalkControl : MonoBehaviour
{
	private Transform[] trans;

	private void Start()
	{
		trans = GetComponentsInChildren<Transform>(includeInactive: true);
	}

	private void Update()
	{
		if (!(MissionManager.Instance == null))
		{
			if (MissionManager.Instance.HadCompleteMission(948))
			{
				trans[3].gameObject.SetActive(value: true);
				trans[4].gameObject.SetActive(value: true);
			}
			if (MissionManager.Instance.HadCompleteMission(959))
			{
				trans[5].gameObject.SetActive(value: true);
				trans[6].gameObject.SetActive(value: true);
			}
			if (MissionManager.Instance.HadCompleteMission(962))
			{
				trans[7].gameObject.SetActive(value: true);
				trans[8].gameObject.SetActive(value: true);
				trans[9].gameObject.SetActive(value: true);
				trans[10].gameObject.SetActive(value: true);
			}
			if (MissionManager.Instance.HadCompleteMission(969))
			{
				trans[11].gameObject.SetActive(value: true);
				trans[12].gameObject.SetActive(value: true);
			}
			Object.Destroy(this);
		}
	}
}
