using System;
using System.Collections;
using Pathea;
using UnityEngine;

public class UIfootManItem : MonoBehaviour
{
	private const int m_RevivePrtroId = 937;

	private UINPCfootManMgr.FootmanInfo mFootmanInfo;

	private PlayerPackageCmpt m_PlayerPackage;

	private SkAliveEntity mSkEntity;

	[SerializeField]
	private GameObject mAttackBtn;

	[SerializeField]
	private GameObject mDefenceBtn;

	[SerializeField]
	private GameObject mRestBtn;

	[SerializeField]
	private GameObject mStayBtn;

	[SerializeField]
	private GameObject mChoseList;

	[SerializeField]
	private UIFilledSprite mNpcBlood;

	[SerializeField]
	private UITexture mNPCTexture;

	[SerializeField]
	private GameObject mNpcDead;

	[SerializeField]
	private UICheckbox mAttCk;

	[SerializeField]
	private UICheckbox mDefCk;

	[SerializeField]
	private UICheckbox mResCk;

	[SerializeField]
	private UICheckbox mStayCk;

	[SerializeField]
	private BoxCollider mAttCol;

	[SerializeField]
	private BoxCollider mDefCol;

	[SerializeField]
	private BoxCollider mResCol;

	[SerializeField]
	private BoxCollider mStayCol;

	[SerializeField]
	private UISprite m_WorkSpr;

	public UICheckbox mBtnCk;

	public int FollowerId;

	[HideInInspector]
	public int mIndex;

	private bool IsDead;

	private bool init;

	private NpcCmpt m_Cmpt;

	private ShowToolTipItem_N m_ShowToolTipItem;

	private ENpcBattle mBattlType = ENpcBattle.Evasion;

	private ENpcBattle mOldBattlType = ENpcBattle.Evasion;

	private float mReviveTimer;

	private float mTimer;

	public UINPCfootManMgr.FootmanInfo FootmanInfo
	{
		get
		{
			return mFootmanInfo;
		}
		set
		{
			mFootmanInfo = value;
			if (mFootmanInfo == null)
			{
				npcCmpt = null;
				base.gameObject.SetActive(value: false);
			}
			else
			{
				base.gameObject.SetActive(value: true);
			}
			if (mFootmanInfo == null)
			{
				return;
			}
			if (npcCmpt != null && npcCmpt != mFootmanInfo.mNpCmpt)
			{
				npcCmpt = mFootmanInfo.mNpCmpt;
				if (npcCmpt.FollowerCurReviveTime == 0f)
				{
					InitReviveTime();
				}
			}
			else if (npcCmpt == null)
			{
				npcCmpt = mFootmanInfo.mNpCmpt;
				if (npcCmpt.FollowerCurReviveTime == 0f)
				{
					InitReviveTime();
				}
			}
			SetNpcHeadTextre(mFootmanInfo.mTexture);
		}
	}

	public SkAliveEntity SkEntity
	{
		get
		{
			return mSkEntity;
		}
		set
		{
			mSkEntity = value;
			if (mSkEntity != null)
			{
				FollowerId = mSkEntity.Entity.Id;
			}
		}
	}

	public NpcCmpt npcCmpt
	{
		get
		{
			return m_Cmpt;
		}
		set
		{
			m_Cmpt = value;
			if (null != m_Cmpt)
			{
				UpdateShowToolTip();
				UpdateWorkState();
				NpcCmpt cmpt = m_Cmpt;
				cmpt.FollowerWorkStateChangeEvent = (Action)Delegate.Remove(cmpt.FollowerWorkStateChangeEvent, new Action(UpdateWorkState));
				NpcCmpt cmpt2 = m_Cmpt;
				cmpt2.FollowerWorkStateChangeEvent = (Action)Delegate.Combine(cmpt2.FollowerWorkStateChangeEvent, new Action(UpdateWorkState));
			}
		}
	}

	public float ReviveTimer
	{
		get
		{
			return (!(m_Cmpt != null)) ? 100000f : m_Cmpt.FollowerCurReviveTime;
		}
		set
		{
			if (m_Cmpt != null)
			{
				m_Cmpt.FollowerCurReviveTime = value;
			}
		}
	}

	private float TotalRestTime => (!(m_Cmpt != null)) ? 100000f : m_Cmpt.FollowerReviceTime;

	private void Start()
	{
		UpdateWorkState();
	}

	public void InitReviveTime()
	{
		ReviveTimer = TotalRestTime;
	}

	private void Update()
	{
		UpdateHpPersent();
		if (m_Cmpt != null)
		{
			mBattlType = m_Cmpt.Battle;
		}
		if (mOldBattlType != mBattlType)
		{
			ShowBattle();
		}
		if (mSkEntity != null)
		{
			ShowNpcDead(mSkEntity.isDead);
		}
		if (mSkEntity != null && mSkEntity.isDead)
		{
			mTimer += Time.deltaTime;
			if (mTimer >= 1f)
			{
				mTimer = 0f;
				ReviveTimer -= 1f;
			}
		}
	}

	private void LateUpdate()
	{
		if (mChoseList.gameObject.activeSelf && (Input.GetMouseButton(0) || Input.GetMouseButton(1)) && !init)
		{
			init = true;
			Ray ray = UICamera.mainCamera.ScreenPointToRay(Input.mousePosition);
			if (mAttCol.Raycast(ray, out var hitInfo, 300f) || mDefCol.Raycast(ray, out hitInfo, 300f) || mResCol.Raycast(ray, out hitInfo, 300f) || mStayCol.Raycast(ray, out hitInfo, 300f))
			{
				StartCoroutine(BtnCkStateChange(0.2f));
			}
			else
			{
				StartCoroutine(BtnCkStateChange(0.1f));
			}
		}
	}

	private void UpdateHpPersent()
	{
		if (mSkEntity != null)
		{
			mNpcBlood.fillAmount = mSkEntity.HPPercent;
		}
	}

	private void ShowNpcDead(bool Show)
	{
		mNpcDead.SetActive(Show);
		mNPCTexture.gameObject.SetActive(!Show);
		IsDead = Show;
	}

	private void ChangeBattle(ENpcBattle type)
	{
		if (!(m_Cmpt == null))
		{
			m_Cmpt.Battle = type;
			UpdateShowToolTip();
		}
	}

	private void UpdateShowToolTip()
	{
		if (null == m_ShowToolTipItem)
		{
			m_ShowToolTipItem = mBtnCk.gameObject.AddComponent<ShowToolTipItem_N>();
		}
		int num = 0;
		switch (m_Cmpt.Battle)
		{
		case ENpcBattle.Attack:
			num = 10077;
			break;
		case ENpcBattle.Defence:
			num = 10078;
			break;
		case ENpcBattle.Passive:
			num = 10079;
			break;
		case ENpcBattle.Stay:
			num = 10076;
			break;
		}
		if (num != 0)
		{
			m_ShowToolTipItem.mStrID = num;
		}
	}

	private void ShowBattle()
	{
		mOldBattlType = mBattlType;
		switch (m_Cmpt.Battle)
		{
		case ENpcBattle.Attack:
			mAttackBtn.SetActive(value: true);
			mDefenceBtn.SetActive(value: false);
			mRestBtn.SetActive(value: false);
			mStayBtn.SetActive(value: false);
			mAttCk.isChecked = true;
			mDefCk.isChecked = false;
			mResCk.isChecked = false;
			mStayCk.isChecked = false;
			break;
		case ENpcBattle.Defence:
			mAttackBtn.SetActive(value: false);
			mDefenceBtn.SetActive(value: true);
			mRestBtn.SetActive(value: false);
			mStayBtn.SetActive(value: false);
			mAttCk.isChecked = false;
			mDefCk.isChecked = true;
			mResCk.isChecked = false;
			mStayCk.isChecked = false;
			break;
		case ENpcBattle.Passive:
			mAttackBtn.SetActive(value: false);
			mDefenceBtn.SetActive(value: false);
			mRestBtn.SetActive(value: true);
			mStayBtn.SetActive(value: false);
			mAttCk.isChecked = false;
			mDefCk.isChecked = false;
			mResCk.isChecked = true;
			mStayCk.isChecked = false;
			break;
		case ENpcBattle.Stay:
			mAttackBtn.SetActive(value: false);
			mDefenceBtn.SetActive(value: false);
			mRestBtn.SetActive(value: false);
			mStayBtn.SetActive(value: true);
			mAttCk.isChecked = false;
			mDefCk.isChecked = false;
			mResCk.isChecked = false;
			mStayCk.isChecked = true;
			break;
		case ENpcBattle.Evasion:
			break;
		}
	}

	private IEnumerator BtnCkStateChange(float _waittime)
	{
		yield return new WaitForSeconds(_waittime);
		mChoseList.SetActive(value: false);
		mBtnCk.isChecked = false;
		init = false;
	}

	public void SetNPCHpPercent(float HpPercent)
	{
		mNpcBlood.fillAmount = HpPercent;
	}

	public void SetNpcHeadTextre(Texture tex)
	{
		mNPCTexture.mainTexture = tex;
	}

	private void OnAttackChosebtn(bool active)
	{
		if (active)
		{
			ChangeBattle(ENpcBattle.Attack);
		}
	}

	private void OnDefenceChoseBtn(bool active)
	{
		if (active)
		{
			ChangeBattle(ENpcBattle.Defence);
		}
	}

	private void OnRestChoseBtn(bool active)
	{
		if (active)
		{
			ChangeBattle(ENpcBattle.Passive);
		}
	}

	private void OnStayChooseBtn(bool active)
	{
		if (active)
		{
			ChangeBattle(ENpcBattle.Stay);
		}
	}

	private void OnServantRevive()
	{
		if (IsDead)
		{
			if (m_Cmpt.CanRecive)
			{
				GameUI.Instance.mRevive.ShowServantRevive(m_Cmpt);
			}
			else
			{
				MessageBox_N.ShowOkBox(PELocalization.GetString(8000188));
			}
		}
	}

	public bool HasReviveMedicine()
	{
		if (m_PlayerPackage == null)
		{
			if (GameUI.Instance != null && GameUI.Instance.mMainPlayer != null)
			{
				m_PlayerPackage = GameUI.Instance.mMainPlayer.GetCmpt<PlayerPackageCmpt>();
			}
			if (m_PlayerPackage == null)
			{
				return false;
			}
		}
		int itemCount = m_PlayerPackage.GetItemCount(937);
		return itemCount > 0;
	}

	private void OnServantWndShow()
	{
		GameUI.Instance.mServantWndCtrl.mCurrentIndex = (UIServantWnd.ServantIndex)mIndex;
		GameUI.Instance.mServantWndCtrl.Show();
	}

	private void ShowOptions(bool active)
	{
		if (!mChoseList.activeSelf && active)
		{
			mChoseList.SetActive(active);
		}
	}

	private void UpdateWorkState()
	{
		m_WorkSpr.enabled = !(null == m_Cmpt) && m_Cmpt.FollowerWork;
	}
}
