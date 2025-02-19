using UnityEngine;

public class UIWorkShopPage2AdaptiveCtrl : MonoBehaviour
{
	public struct PosStruct
	{
		public Vector3 TopGoPos;

		public Vector3 BottomGoPos;

		public Vector3 RightGoPos;

		public Vector3 GridPos;

		public Vector3 GridBgPos;

		public Vector3 GridBgSize;
	}

	public Transform TopGo;

	public Transform BottomGo;

	public Transform RightGo;

	public Transform GridBg0;

	public Transform GridBg1;

	public UIGrid Grid;

	private PosStruct m_BasePosInfo;

	private int m_BaseColumnCount = 4;

	private bool m_InitData;

	private void InitBaseData()
	{
		m_BasePosInfo = default(PosStruct);
		m_BasePosInfo.TopGoPos = TopGo.localPosition;
		m_BasePosInfo.BottomGoPos = BottomGo.localPosition;
		m_BasePosInfo.RightGoPos = RightGo.localPosition;
		m_BasePosInfo.GridPos = Grid.transform.localPosition;
		m_BasePosInfo.GridBgSize = GridBg0.localScale;
		m_BasePosInfo.GridBgPos = GridBg0.localPosition;
		m_InitData = true;
	}

	public void UpdateSizeByScreen(int columnCount, int gridWidth)
	{
		if (m_BaseColumnCount != columnCount)
		{
			if (!m_InitData)
			{
				InitBaseData();
			}
			Vector3 vector = new Vector3((columnCount - m_BaseColumnCount) * gridWidth, 0f, 0f);
			Vector3 vector2 = new Vector3(vector.x * 0.5f, 0f, 0f);
			TopGo.localPosition = m_BasePosInfo.TopGoPos - vector2;
			BottomGo.localPosition = m_BasePosInfo.BottomGoPos - vector2;
			RightGo.localPosition = m_BasePosInfo.RightGoPos + vector2;
			GridBg0.localScale = m_BasePosInfo.GridBgSize + vector;
			GridBg0.localPosition = m_BasePosInfo.GridBgPos;
			GridBg1.localPosition = GridBg0.localPosition;
			GridBg1.localScale = GridBg0.localScale;
			Grid.transform.localPosition = m_BasePosInfo.GridPos - vector2;
		}
	}
}
