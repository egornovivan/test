using System;
using System.Collections.Generic;
using Pathea;
using Pathea.Operate;
using UnityEngine;
using WhiteCat;

public class UIItemOp : UIBaseWnd
{
	private const float mGetItemTime = 2f;

	public UISlicedSprite mBg;

	public ItemOpBtn_N mCloseBtn;

	public ItemOpBtn_N mPrefab;

	public GameObject mRefillWnd;

	public UILabel mNumText;

	public UILabel mNum;

	public GameObject mSleepWnd;

	public UIScrollBar mSleepSlider;

	public UILabel mSleepTime;

	public UILabel mMaxSleepTime;

	public UILabel mMinSleepTime;

	public GameObject mMainWnd;

	public UISlider mSlider;

	private bool mAddBtnPress;

	private bool mSubBtnPress;

	private float mCurrentNum;

	private float mOpDurNum;

	private float mOpStarTime;

	private bool mIsAmmoTower;

	private int m_AmmoCount;

	private int m_AmmoMaxCount;

	private int m_AmmoMaxHave;

	[SerializeField]
	private SleepingUI sleepingUI;

	private UTimer mTimer;

	private List<ItemOpBtn_N> mBottons = new List<ItemOpBtn_N>();

	private MonoBehaviour m_Operater;

	private PeEntity mEntity;

	private CmdList mCmdList;

	private Action mClose;

	private Action mOpen;

	private Action mDoGet;

	private Func<float> m_SpeepEvent;

	private PESleep m_PeSleep;

	public int AmmoNum => (int)mCurrentNum;

	private void Update()
	{
		if (null == m_Operater && m_SpeepEvent == null)
		{
			Hide();
			return;
		}
		if (mSlider.gameObject.activeSelf)
		{
			if (mTimer == null)
			{
				return;
			}
			mTimer.Update(Time.deltaTime);
			if (mTimer.Second > 2.0)
			{
				mTimer.Second = 2.0;
				if (mDoGet != null)
				{
					mDoGet();
					mSlider.gameObject.SetActive(value: false);
					base.gameObject.SetActive(value: false);
				}
			}
			mSlider.sliderValue = (float)mTimer.Second / 2f;
		}
		mSleepTime.text = string.Format(PELocalization.GetString(8000251), GetCurSleepTime());
		if (mAddBtnPress)
		{
			float num = Time.time - mOpStarTime;
			if (num < 0.2f)
			{
				mOpDurNum = 1f;
			}
			else if (num < 1f)
			{
				mOpDurNum += 2f * Time.deltaTime;
			}
			else if (num < 2f)
			{
				mOpDurNum += 4f * Time.deltaTime;
			}
			else if (num < 3f)
			{
				mOpDurNum += 7f * Time.deltaTime;
			}
			else if (num < 4f)
			{
				mOpDurNum += 11f * Time.deltaTime;
			}
			else if (num < 5f)
			{
				mOpDurNum += 16f * Time.deltaTime;
			}
			else
			{
				mOpDurNum += 20f * Time.deltaTime;
			}
			int num2 = (int)(mOpDurNum + mCurrentNum);
			int num3 = m_AmmoMaxCount - m_AmmoCount;
			mCurrentNum = Mathf.Clamp((num2 <= num3) ? num2 : num3, 0, m_AmmoMaxHave);
			mNum.text = mCurrentNum.ToString();
		}
		else if (mSubBtnPress)
		{
			float num4 = Time.time - mOpStarTime;
			if (num4 < 0.5f)
			{
				mOpDurNum = -1f;
			}
			else if (num4 < 1f)
			{
				mOpDurNum -= 2f * Time.deltaTime;
			}
			else if (num4 < 2f)
			{
				mOpDurNum -= 4f * Time.deltaTime;
			}
			else if (num4 < 3f)
			{
				mOpDurNum -= 7f * Time.deltaTime;
			}
			else if (num4 < 4f)
			{
				mOpDurNum -= 11f * Time.deltaTime;
			}
			else if (num4 < 5f)
			{
				mOpDurNum -= 16f * Time.deltaTime;
			}
			else
			{
				mOpDurNum -= 20f * Time.deltaTime;
			}
			mCurrentNum = int.Parse(mNum.text);
			int num5 = (int)(mOpDurNum + mCurrentNum);
			mCurrentNum = ((num5 >= 0) ? num5 : 0);
			mNum.text = mCurrentNum.ToString();
		}
		if (mSleepWnd.activeSelf && (null == mEntity || null == m_PeSleep || Vector3.Distance(mEntity.position, m_PeSleep.transform.position) > 30f))
		{
			OnCancelBtn();
		}
	}

	private void UpdateAmmoLabel(int ammoCount, int ammoMaxCount)
	{
		mNumText.text = ammoCount + "/" + ammoMaxCount;
	}

	private void ClearOpBtn()
	{
		for (int num = mBottons.Count - 1; num >= 0; num--)
		{
			UnityEngine.Object.Destroy(mBottons[num].gameObject);
		}
		mBottons.Clear();
	}

	private void AddOpBtn(string buttonName)
	{
		ItemOpBtn_N itemOpBtn_N = UnityEngine.Object.Instantiate(mPrefab);
		itemOpBtn_N.gameObject.name = buttonName;
		itemOpBtn_N.InitButton(buttonName, base.gameObject);
		itemOpBtn_N.transform.parent = mMainWnd.transform;
		itemOpBtn_N.transform.localRotation = Quaternion.identity;
		itemOpBtn_N.transform.localScale = Vector3.one;
		mBottons.Add(itemOpBtn_N);
	}

	private void ResetWnd()
	{
		if (mIsAmmoTower)
		{
			mBg.transform.localScale = new Vector3(172f, 50f + (float)(mBottons.Count + 1) * 40f + 50f, 1f);
			mRefillWnd.SetActive(value: true);
			mOpDurNum = 0f;
			mCurrentNum = 0f;
			Vector3 localPosition = mRefillWnd.transform.localPosition;
			localPosition.y = mBg.transform.localScale.y / 2f - 52f;
			localPosition.z = -2f;
			mRefillWnd.transform.localPosition = localPosition;
		}
		else
		{
			mBg.transform.localScale = new Vector3(172f, 50f + (float)(mBottons.Count + 1) * 40f, 1f);
			mRefillWnd.SetActive(value: false);
		}
		float num = mBottons.Count * 14;
		for (int i = 0; i < mBottons.Count; i++)
		{
			if (mIsAmmoTower)
			{
				mBottons[i].transform.localPosition = new Vector3(0f, (float)(-i) * 40f + num - 32f, 0f);
			}
			else
			{
				mBottons[i].transform.localPosition = new Vector3(0f, (float)(-i) * 40f + num, 0f);
			}
		}
		if (mIsAmmoTower)
		{
			mCloseBtn.transform.localPosition = new Vector3(0f, (float)(-mBottons.Count) * 40f + num - 32f, 0f);
		}
		else
		{
			mCloseBtn.transform.localPosition = new Vector3(0f, (float)(-mBottons.Count) * 40f + num, 0f);
		}
	}

	private void ResetDefaultState()
	{
		m_Operater = null;
		mEntity = null;
		mCmdList = null;
		mOpen = null;
		mDoGet = null;
		m_SpeepEvent = null;
		m_PeSleep = null;
		mSleepWnd.SetActive(value: false);
		mMainWnd.SetActive(value: true);
		mSlider.gameObject.SetActive(value: false);
	}

	private void OnOkBtn()
	{
		if (!(m_PeSleep == null) && !(mEntity == null) && m_PeSleep.CanOperateMask(EOperationMask.Sleep))
		{
			MotionMgrCmpt cmpt = mEntity.GetCmpt<MotionMgrCmpt>();
			if (!(null != cmpt) || (!cmpt.IsActionRunning(PEActionType.Sleep) && cmpt.CanDoAction(PEActionType.Sleep)))
			{
				SleepController.StartSleep(m_PeSleep, mEntity, GetCurSleepTime());
				ShowSleepWnd(show: false);
			}
		}
	}

	private void OnCancelBtn()
	{
		ShowSleepWnd(show: false);
		base.gameObject.SetActive(value: false);
	}

	private void OnMinBtn()
	{
		mCurrentNum = 0f;
		mOpDurNum = 0f;
		mNum.text = "0";
	}

	private void OnMaxBtn()
	{
		int num = Mathf.Clamp(m_AmmoMaxCount - m_AmmoCount, 0, m_AmmoMaxHave);
		mCurrentNum = num;
		mNum.text = num.ToString();
	}

	private void OnAddBtnPress()
	{
		mAddBtnPress = true;
		mOpStarTime = Time.time;
		mOpDurNum = 0f;
	}

	private void OnAddBtnRelease()
	{
		mAddBtnPress = false;
	}

	private void OnSubstructBtnPress()
	{
		mSubBtnPress = true;
		mOpStarTime = Time.time;
		mOpDurNum = 0f;
	}

	private void OnSubstructBtnRelease()
	{
		mSubBtnPress = false;
	}

	private int GetCurSleepTime()
	{
		return (int)(25f * mSleepSlider.scrollValue + 1f);
	}

	private void InitSleepTime()
	{
		mMaxSleepTime.text = 26.ToString();
		mMinSleepTime.text = 1.ToString();
		mSleepSlider.scrollValue = 0.61538464f;
		mSleepTime.text = string.Format(PELocalization.GetString(8000251), GetCurSleepTime());
	}

	protected override void InitWindow()
	{
		if (!mInit)
		{
			base.InitWindow();
			mTimer = new UTimer();
			mTimer.ElapseSpeed = 1f;
			mTimer.Second = 0.0;
			mInit = true;
		}
	}

	public override void Show()
	{
		ResetDefaultState();
		base.Show();
	}

	protected override void OnClose()
	{
		base.OnClose();
		if (mClose != null)
		{
			mClose();
		}
	}

	protected override void OnHide()
	{
		ResetDefaultState();
		base.OnHide();
	}

	public void ShowSleepingUI(Func<float> time01)
	{
		InitWindow();
		isShow = true;
		m_SpeepEvent = time01;
		sleepingUI.Show(time01);
	}

	public void HideSleepingUI()
	{
		isShow = false;
		m_SpeepEvent = null;
		sleepingUI.Hide();
	}

	public void SetRefill(int currentCount, int maxCount, int maxHave)
	{
		UpdateAmmoCount(currentCount, maxCount, maxHave);
		UpdateAmmoLabel(m_AmmoCount, m_AmmoMaxCount);
		mNum.text = "0";
		mCurrentNum = 0f;
		mOpDurNum = 0f;
		mIsAmmoTower = true;
		ResetWnd();
	}

	public void UpdateAmmoCount(int currentCount, int maxCount, int maxHave)
	{
		m_AmmoCount = currentCount;
		m_AmmoMaxCount = maxCount;
		m_AmmoMaxHave = maxHave;
		UpdateAmmoLabel(m_AmmoCount, m_AmmoMaxCount);
	}

	public void ListenEvent(Action close, Action open)
	{
		if (mClose != null)
		{
			mClose();
		}
		mClose = close;
		mOpen = open;
	}

	private void SetOperater(MonoBehaviour mono)
	{
		m_Operater = mono;
	}

	public void SetCmdList(MonoBehaviour mono, CmdList cmdList)
	{
		Show();
		SetOperater(mono);
		if (mOpen != null)
		{
			mOpen();
		}
		mIsAmmoTower = false;
		ClearOpBtn();
		mCmdList = cmdList;
		foreach (string item in (IEnumerable<string>)cmdList)
		{
			AddOpBtn(item);
		}
		ResetWnd();
	}

	public void ShowSleepWnd(bool show, MonoBehaviour operater = null, PESleep peSleep = null, PeEntity character = null, Action<float> sleep = null)
	{
		if (!isShow)
		{
			Show();
		}
		if (show && operater != null)
		{
			if (!mInit)
			{
				InitWindow();
			}
			mSleepWnd.SetActive(value: true);
			mMainWnd.SetActive(value: false);
			InitSleepTime();
			m_PeSleep = peSleep;
			mEntity = character;
			SetOperater(operater);
		}
		else
		{
			mSleepWnd.SetActive(value: false);
		}
	}

	public void SleepImmediately(PESleep peSleep, PeEntity character)
	{
		if (!mInit)
		{
			InitWindow();
		}
		mMainWnd.SetActive(value: false);
		SleepController.StartSleep(peSleep, PeSingleton<MainPlayer>.Instance.entity, 16f);
	}

	public void GetItem(Action doGet, MonoBehaviour mono)
	{
		if (doGet != null)
		{
			if (!isShow)
			{
				Show();
			}
			m_Operater = mono;
			mDoGet = doGet;
			mMainWnd.SetActive(value: false);
			mSlider.gameObject.SetActive(value: true);
			mTimer.Second = 0.0;
		}
	}

	public void CallFunction(string funcName)
	{
		if (mCmdList != null && !(null == m_Operater))
		{
			mCmdList.ExecuteCmd(funcName);
		}
	}
}
