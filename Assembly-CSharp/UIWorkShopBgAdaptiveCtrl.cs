using UnityEngine;

public class UIWorkShopBgAdaptiveCtrl : MonoBehaviour
{
	public struct PosStruct
	{
		public Vector3 Bg0GoSize;

		public Vector3 RightGoPos;

		public Vector3 LeftGoPos;

		public Vector3 LeftGoSize;

		public Vector3 LeftBgPos;

		public Vector3 LeftBgSize;

		public Vector3 BtnGoPos;
	}

	public Transform Bg0Go;

	public Transform Bg1Go;

	public Transform Bg2Go;

	public Transform RightGo;

	public Transform LeftGo;

	public Transform LeftBG;

	public Transform BtnGo;

	private int m_BaseColumnCount = 3;

	private PosStruct m_BasePosInfo;

	private bool m_InitData;

	private void InitBaseData()
	{
		m_BasePosInfo = default(PosStruct);
		m_BasePosInfo.Bg0GoSize = Bg0Go.localScale;
		m_BasePosInfo.RightGoPos = RightGo.localPosition;
		m_BasePosInfo.LeftGoPos = LeftGo.localPosition;
		m_BasePosInfo.LeftGoSize = LeftGo.localScale;
		m_BasePosInfo.LeftBgSize = LeftBG.localScale;
		m_BasePosInfo.LeftBgPos = LeftBG.localPosition;
		m_BasePosInfo.BtnGoPos = BtnGo.localPosition;
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
			Bg0Go.localScale = m_BasePosInfo.Bg0GoSize + vector;
			Bg0Go.localPosition = Vector3.zero;
			Bg1Go.localScale = Bg0Go.localScale;
			Bg1Go.localPosition = Vector3.zero;
			Bg2Go.localScale = Bg0Go.localScale;
			Bg2Go.localPosition = Vector3.zero;
			LeftBG.localScale = m_BasePosInfo.LeftBgSize + vector;
			LeftBG.localPosition = m_BasePosInfo.LeftBgPos;
			RightGo.localPosition = m_BasePosInfo.RightGoPos + vector2;
			LeftGo.localPosition = m_BasePosInfo.LeftGoPos - vector2;
			BtnGo.localPosition = m_BasePosInfo.BtnGoPos + vector2;
		}
	}
}
