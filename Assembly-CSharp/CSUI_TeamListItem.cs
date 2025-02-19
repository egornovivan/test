using UnityEngine;

public class CSUI_TeamListItem : MonoBehaviour
{
	public delegate void OnCheckItem(int _index, MyItemType _type);

	public delegate void OnCheckItemPlayer(PlayerNetwork pnet);

	public delegate void OnAgreementBtn(bool _isAgree, PlayerNetwork _mPnet);

	public UILabel m_team;

	public UILabel m_name;

	public UILabel m_killAndDeath;

	public UILabel m_score;

	public UISprite m_captainSpr;

	public UISprite m_AgreeSpr;

	public UISprite m_DisAgreeSpr;

	public BoxCollider mBoxCollider;

	public UISlicedSprite mCkSelectedBg;

	private bool IsSelected;

	[HideInInspector]
	public int mIndex;

	[HideInInspector]
	public MyItemType mType;

	[HideInInspector]
	public PlayerNetwork mPnet;

	public event OnCheckItem ItemChecked;

	public event OnCheckItemPlayer ItemCheckedPlayer;

	public event OnAgreementBtn OnAgreementBtnEvent;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void SetActive(bool isActive)
	{
		mBoxCollider.enabled = isActive;
	}

	public void SetInfo(int _team, int _kill, int _death, float _score)
	{
		SetActive(isActive: true);
		if (m_team != null)
		{
			m_team.text = "Team" + _team;
		}
		if (m_killAndDeath != null)
		{
			m_killAndDeath.text = _kill + "/" + _death;
		}
		if (m_score != null)
		{
			m_score.text = _score.ToString();
		}
	}

	public void ClearInfo()
	{
		SetActive(isActive: false);
		if (m_team != null)
		{
			m_team.text = string.Empty;
		}
		if (m_name != null)
		{
			m_name.text = string.Empty;
		}
		if (m_killAndDeath != null)
		{
			m_killAndDeath.text = string.Empty;
		}
		if (m_score != null)
		{
			m_score.text = string.Empty;
		}
		if (m_captainSpr != null)
		{
			m_captainSpr.enabled = false;
		}
		if (m_AgreeSpr != null)
		{
			m_AgreeSpr.gameObject.SetActive(value: false);
		}
		if (m_DisAgreeSpr != null)
		{
			m_DisAgreeSpr.gameObject.SetActive(value: false);
		}
		mPnet = null;
	}

	public void SetInfo(int _team, string _name, int _kill, int _death, float _score, PlayerNetwork _pnet)
	{
		SetActive(isActive: true);
		if (m_team != null)
		{
			m_team.text = "Team" + _team;
		}
		if (m_name != null)
		{
			m_name.text = _name;
		}
		if (m_killAndDeath != null)
		{
			m_killAndDeath.text = _kill + "/" + _death;
		}
		if (m_score != null)
		{
			m_score.text = _score.ToString();
		}
		if (_pnet != null)
		{
			mPnet = _pnet;
		}
	}

	public void SetInfo(string _name, int _kill, int _death, float _score, PlayerNetwork _pnet)
	{
		if (PlayerNetwork.mainPlayer.TeamId == -1 || null == _pnet)
		{
			return;
		}
		SetActive(isActive: true);
		if (m_name != null)
		{
			m_name.text = _name;
		}
		if (m_killAndDeath != null)
		{
			m_killAndDeath.text = _kill + "/" + _death;
		}
		if (m_score != null)
		{
			m_score.text = _score.ToString();
		}
		TeamData teamInfo = GroupNetwork.GetTeamInfo(PlayerNetwork.mainPlayer.TeamId);
		mPnet = _pnet;
		if (_pnet.Id == teamInfo.LeaderId)
		{
			if (m_captainSpr != null)
			{
				m_captainSpr.enabled = true;
			}
		}
		else if (_pnet.Id != teamInfo.LeaderId && m_captainSpr != null)
		{
			m_captainSpr.enabled = false;
		}
		if (PlayerNetwork.mainPlayer.Id == teamInfo.LeaderId && GroupNetwork.IsJoinRequest(_pnet))
		{
			if (m_AgreeSpr != null)
			{
				m_AgreeSpr.gameObject.SetActive(value: true);
			}
			if (m_DisAgreeSpr != null)
			{
				m_DisAgreeSpr.gameObject.SetActive(value: true);
			}
		}
		else
		{
			if (m_AgreeSpr != null)
			{
				m_AgreeSpr.gameObject.SetActive(value: false);
			}
			if (m_DisAgreeSpr != null)
			{
				m_DisAgreeSpr.gameObject.SetActive(value: false);
			}
		}
	}

	public void OnActivate(bool active)
	{
	}

	public void SetSelected(bool _isSelected)
	{
		IsSelected = _isSelected;
		if (mCkSelectedBg.enabled != _isSelected)
		{
			mCkSelectedBg.enabled = _isSelected;
		}
	}

	private void OnChecked()
	{
		if (Input.GetMouseButtonUp(0) && !IsSelected)
		{
			if (this.ItemChecked != null)
			{
				this.ItemChecked(mIndex, mType);
			}
			if (this.ItemCheckedPlayer != null)
			{
				this.ItemCheckedPlayer(mPnet);
			}
		}
	}

	private void OnAgreeBtn()
	{
		if (this.OnAgreementBtnEvent != null && mPnet != null)
		{
			this.OnAgreementBtnEvent(_isAgree: true, mPnet);
		}
		ClearInfo();
	}

	private void OnDisAgreeBtn()
	{
		if (this.OnAgreementBtnEvent != null && mPnet != null)
		{
			this.OnAgreementBtnEvent(_isAgree: false, mPnet);
		}
		ClearInfo();
	}
}
