using UnityEngine;

public class RadoRender : MonoBehaviour
{
	private void Awake()
	{
		if (!MissionManager.Instance.m_PlayerMission.HasMission(550))
		{
			Object.Destroy(base.gameObject);
		}
	}
}
