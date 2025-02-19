using UnityEngine;

public class CSUI_ProcessLabel : MonoBehaviour
{
	public string m_Text = "askdfhkjsadhfkjashfdjkashdfjkh";

	public UILabel m_Label;

	private int m_CurIndex;

	public float m_Speed = 0.1f;

	private float m_startTime;

	private void Start()
	{
		m_startTime = Time.time;
	}

	private void Update()
	{
		if (Time.time - m_startTime > 0.1f && m_CurIndex < m_Text.Length)
		{
			m_CurIndex++;
			m_startTime = Time.time;
		}
		m_Label.text = m_Text.Substring(0, m_CurIndex);
	}
}
