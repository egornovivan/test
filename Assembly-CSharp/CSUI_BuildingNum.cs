using UnityEngine;

public class CSUI_BuildingNum : MonoBehaviour
{
	[SerializeField]
	private UILabel m_Label;

	public string m_Description;

	public int m_Count;

	public int m_LimitCnt;

	private void Start()
	{
	}

	private void Update()
	{
		if (m_LimitCnt != 0)
		{
			m_Label.text = m_Description + "  " + m_Count + " / " + m_LimitCnt;
		}
		else
		{
			m_Label.text = "[858585]" + m_Description + "  " + m_Count + " / " + m_LimitCnt + "[-]";
		}
	}
}
