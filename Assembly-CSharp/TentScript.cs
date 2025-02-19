using UnityEngine;

public class TentScript : MonoBehaviour
{
	public Vector3 m_Pos;

	public string m_NpcName = string.Empty;

	private void Start()
	{
		m_Pos = base.transform.position + Vector3.up * 0.5f;
		if (!(StroyManager.Instance == null) && !StroyManager.Instance.m_TentList.ContainsKey(m_Pos))
		{
			StroyManager.Instance.m_TentList.Add(m_Pos, this);
		}
	}

	public void CmdToSleep()
	{
	}

	private void OnSleepToDest()
	{
	}
}
