using System;
using UnityEngine;

public class UITowerInfo : UIStaticWnd
{
	private static UITowerInfo mInstance;

	[SerializeField]
	private UILabel mLbPrepTime;

	[SerializeField]
	private UILabel mLbMonsterNum;

	[SerializeField]
	private UILabel mLbRemainTime;

	[SerializeField]
	private UILabel mLbWavesRemaining;

	[SerializeField]
	private Color32 LbPrepTimeColor;

	[SerializeField]
	private Color32 LbMonsterNumColor;

	[SerializeField]
	private Color32 LbRemainTimeColor;

	[SerializeField]
	private Color32 LbWavesRemainingColor;

	[SerializeField]
	private string m_TitleSpece;

	private string m_PrepTimeTitleStr;

	private string m_MonsterNumTitleStr;

	private string m_RemainTimeStr;

	private string m_WavesRemainingTitleStr;

	private string m_PrepTimeColorStr;

	private string m_MonsterNumColorStr;

	private string m_RemainTimeColorStr;

	private string m_WavesRemainingColorStr;

	private string m_ColorEnd;

	private int m_PrepTimeTitleID;

	private UTimer m_UTimer;

	private TowerInfoUIData m_info;

	public static UITowerInfo Instance => mInstance;

	public event OnGuiBtnClicked e_BtnReady;

	private void Awake()
	{
		m_PrepTimeColorStr = ConvertColor32ToHexStr(LbPrepTimeColor);
		m_MonsterNumColorStr = ConvertColor32ToHexStr(LbMonsterNumColor);
		m_RemainTimeColorStr = ConvertColor32ToHexStr(LbRemainTimeColor);
		m_WavesRemainingColorStr = ConvertColor32ToHexStr(LbWavesRemainingColor);
		m_ColorEnd = "[-]";
		m_PrepTimeTitleID = 8000602;
		m_PrepTimeTitleStr = PELocalization.GetString(m_PrepTimeTitleID) + m_TitleSpece + m_PrepTimeColorStr;
		m_MonsterNumTitleStr = PELocalization.GetString(8000601) + m_TitleSpece + m_MonsterNumColorStr;
		m_RemainTimeStr = PELocalization.GetString(8000607) + m_TitleSpece + m_RemainTimeColorStr;
		m_WavesRemainingTitleStr = PELocalization.GetString(8000608) + m_TitleSpece + m_WavesRemainingColorStr;
	}

	private string ConvertColor32ToHexStr(Color32 color)
	{
		string text = "[";
		string empty = string.Empty;
		empty = Convert.ToString(color.r, 16);
		if (empty.Length == 1)
		{
			empty = "0" + empty;
		}
		text += empty;
		empty = Convert.ToString(color.g, 16);
		if (empty.Length == 1)
		{
			empty = "0" + empty;
		}
		text += empty;
		empty = Convert.ToString(color.b, 16);
		if (empty.Length == 1)
		{
			empty = "0" + empty;
		}
		text += empty;
		return text + "]";
	}

	public void SetInfo(TowerInfoUIData info)
	{
		m_info = info;
	}

	public override void OnCreate()
	{
		base.OnCreate();
		mInstance = this;
		m_UTimer = new UTimer();
	}

	public void SetPrepTime(float preTime)
	{
		if (preTime >= 0f)
		{
			m_UTimer.Second = preTime;
			mLbPrepTime.text = m_PrepTimeTitleStr + m_UTimer.FormatString("mm:ss") + m_ColorEnd;
		}
		else
		{
			mLbPrepTime.text = m_PrepTimeTitleStr + "--:--" + m_ColorEnd;
		}
	}

	public void SetWavesRemaining(int curWavesRemaining, int totalWaves)
	{
		int num = ((curWavesRemaining != totalWaves) ? 8000606 : 8000602);
		if (m_PrepTimeTitleID != num)
		{
			m_PrepTimeTitleID = num;
			m_PrepTimeTitleStr = PELocalization.GetString(m_PrepTimeTitleID) + m_TitleSpece + m_PrepTimeColorStr;
		}
		if (curWavesRemaining >= 0)
		{
			mLbWavesRemaining.text = m_WavesRemainingTitleStr + curWavesRemaining + m_ColorEnd;
		}
	}

	private void SetMonsterNum(int curCount, int maxCount)
	{
		if (maxCount > 0)
		{
			mLbMonsterNum.text = m_MonsterNumTitleStr + curCount + "/" + maxCount + m_ColorEnd;
		}
		else
		{
			mLbMonsterNum.text = m_MonsterNumTitleStr + curCount + m_ColorEnd;
		}
	}

	private void SetRemainTime(float remainTime)
	{
		if (remainTime > 0f)
		{
			m_UTimer.Second = remainTime;
			mLbRemainTime.text = m_RemainTimeStr + m_UTimer.FormatString("mm:ss") + m_ColorEnd;
		}
		else
		{
			mLbRemainTime.text = m_RemainTimeStr + "--:--" + m_ColorEnd;
		}
	}

	private void BtnReady_OnClick()
	{
		if (this.e_BtnReady != null)
		{
			this.e_BtnReady();
		}
	}

	private void Update()
	{
		if (m_info != null)
		{
			SetMonsterNum(m_info.curCount(), m_info.MaxCount);
			SetRemainTime(m_info.RemainTime);
			SetPrepTime(m_info.PreTime);
			SetWavesRemaining(m_info.CurWavesRemaining, m_info.TotalWaves);
		}
	}
}
