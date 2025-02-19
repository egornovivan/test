using UnityEngine;

public class ParticipantItem : MonoBehaviour
{
	[SerializeField]
	private UILabel m_LeftLabel;

	[SerializeField]
	private UILabel m_CenterLabel;

	[SerializeField]
	private UILabel m_RightLabel;

	private void Awake()
	{
		ResetItem();
	}

	public void UpdateLeft(string info)
	{
		ResetItem();
		m_LeftLabel.text = info;
		Object.Destroy(m_CenterLabel.gameObject);
		Object.Destroy(m_RightLabel.gameObject);
	}

	public void UpdateCenter(string info)
	{
		ResetItem();
		m_CenterLabel.text = info;
		Object.Destroy(m_LeftLabel.gameObject);
		Object.Destroy(m_RightLabel.gameObject);
	}

	public void UpdateLeftAndRight(string leftInfo, string rightInfo)
	{
		ResetItem();
		m_LeftLabel.text = leftInfo;
		m_RightLabel.text = rightInfo;
		Object.Destroy(m_CenterLabel.gameObject);
	}

	private void ResetItem()
	{
		m_LeftLabel.text = string.Empty;
		m_CenterLabel.text = string.Empty;
		m_RightLabel.text = string.Empty;
	}
}
