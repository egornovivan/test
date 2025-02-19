using UnityEngine;

public class CommonChatItem_N : MonoBehaviour
{
	[SerializeField]
	private UILabel m_ContentLb;

	[SerializeField]
	private UIFont m_ChineseFont;

	[SerializeField]
	private UIFont m_OtherFont;

	public void UpdateText(bool isChinese, string info)
	{
		m_ContentLb.font = ((!isChinese) ? m_OtherFont : m_ChineseFont);
		m_ContentLb.text = info;
		m_ContentLb.MakePixelPerfect();
	}

	public void SetLineWidth(int width)
	{
		m_ContentLb.lineWidth = width;
		m_ContentLb.MakePixelPerfect();
	}

	public void ResetItem()
	{
		m_ContentLb.text = string.Empty;
	}
}
