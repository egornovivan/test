using System;
using System.Collections.Generic;
using ItemAsset;
using Pathea;
using UnityEngine;

public class CSUI_CollectWnd : MonoBehaviour
{
	public delegate void UpdateCollectEvent(object sender);

	public delegate void InitCollectEvent(object sender);

	public delegate void BaseFristClick(object sender, bool active);

	public delegate void AutoClick(object sender, bool active);

	public delegate void StarrClick(object sender, int Index);

	public delegate void StopClick(object sender, int Index);

	public delegate void AddtoClick(object sender, int index, int protoId, int curCout);

	public delegate void RemoveEvent(object sender, int index, int protoId);

	public delegate void ProcessChoseEvent(object sender, int index);

	public delegate void SetRunCount(object sender, int index, int runCount);

	[SerializeField]
	public CSUI_PrcoessMgr[] m_Processes;

	public Dictionary<int, List<ProcessInfo>> mProlists = new Dictionary<int, List<ProcessInfo>>();

	public bool isInited;

	[SerializeField]
	private UIInput mNumInput;

	[SerializeField]
	private UIInput mRunCountInput;

	[SerializeField]
	private N_ImageButton mStartBtn;

	[SerializeField]
	private N_ImageButton mStopBtn;

	[SerializeField]
	private N_ImageButton mAddBtn;

	public int CurProtoID;

	private int m_Index;

	private bool mAddNumBtnPress;

	private bool mSubNumBtnPress;

	private float mNumOpStarTime;

	private int mCurrentNum;

	private int mNewNum = 1;

	private float mOpDurNum;

	private int m_MaxNum = 10;

	private bool mAddRunCountBtnPress;

	private bool mSubRunCountBtnPress;

	private float mRunCountOpStartTime;

	private int mCurrentRunCount;

	private int mNewRunCount = 1;

	private float mOpDurRunCount;

	private int m_MaxRunCount = 10;

	private int m_BackRunCount;

	public int MaxNum
	{
		get
		{
			return m_MaxNum;
		}
		set
		{
			m_MaxNum = value;
			mNewNum = 1;
		}
	}

	public int MaxRunCount
	{
		get
		{
			return m_MaxRunCount;
		}
		set
		{
			m_MaxRunCount = value;
			mNewRunCount = 0;
		}
	}

	public event UpdateCollectEvent e_UpdateCollect;

	public event InitCollectEvent e_InitCollectEvent;

	public event BaseFristClick e_BaseFristClick;

	public event AutoClick e_AutoClick;

	public event StarrClick e_StartClick;

	public event StopClick e_StopClick;

	public event AddtoClick e_AddtoClick;

	public event RemoveEvent e_RemoveEvent;

	public event ProcessChoseEvent e_ProcessChoseEvent;

	public event SetRunCount e_SetRunCountEvent;

	private void Awake()
	{
		InitWnd();
		UIEventListener uIEventListener = UIEventListener.Get(mRunCountInput.gameObject);
		uIEventListener.onSelect = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onSelect, (UIEventListener.BoolDelegate)delegate(GameObject go, bool isSelect)
		{
			if (!isSelect)
			{
				if (mRunCountInput.text == string.Empty)
				{
					mNewRunCount = 1;
				}
				else
				{
					mNewRunCount = Mathf.Clamp(int.Parse(mRunCountInput.text), 1, m_MaxRunCount);
				}
				OnSetRunCount();
			}
		});
		UIEventListener uIEventListener2 = UIEventListener.Get(mNumInput.gameObject);
		uIEventListener2.onSelect = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener2.onSelect, (UIEventListener.BoolDelegate)delegate(GameObject go, bool isSelect)
		{
			if (!isSelect)
			{
				if (mNumInput.text == string.Empty)
				{
					mNewNum = 1;
				}
				else
				{
					mNewNum = Mathf.Clamp(int.Parse(mNumInput.text), 1, m_MaxNum);
				}
			}
		});
	}

	private void Start()
	{
		InitEnvent();
		UpdateCollect();
	}

	public void InitWnd()
	{
		if (!isInited)
		{
			isInited = true;
		}
	}

	public void Init()
	{
		if (!isInited)
		{
			InitProcess();
			isInited = true;
		}
	}

	private void PlanInit()
	{
	}

	private void Update()
	{
		UpdataInput();
		ButtonCtr();
	}

	public void SetEnity(CSEntity enity)
	{
		if (enity == null)
		{
			Debug.LogWarning("Reference Entity is null.");
		}
		else
		{
			CSUI_MainWndCtrl.Instance.mSelectedEnntity = enity;
		}
	}

	public void AddMainPlan(GridList Plan)
	{
		if (!(CSUI_PlanMgr.Instance == null))
		{
			CSUI_PlanMgr.Instance.PlanList = Plan;
		}
	}

	public void AddOreList(List<ItemIdCount> list)
	{
		if (list == null)
		{
			return;
		}
		foreach (ItemIdCount item in list)
		{
			AddOrePage(item.protoId, item.count);
		}
	}

	public void AddHerbList(List<ItemIdCount> list)
	{
		if (list == null)
		{
			return;
		}
		foreach (ItemIdCount item in list)
		{
			AddHerbPage(item.protoId, item.count);
		}
	}

	public void AddOtherList(List<ItemIdCount> list)
	{
		if (list == null)
		{
			return;
		}
		foreach (ItemIdCount item in list)
		{
			AddOtherPage(item.protoId, item.count);
		}
	}

	public void ClearOreList()
	{
		if (!(CSUI_PlanMgr.Instance == null))
		{
			CSUI_PlanMgr.Instance.ClearOrePage();
		}
	}

	public void ClearHerbList()
	{
		if (!(CSUI_PlanMgr.Instance == null))
		{
			CSUI_PlanMgr.Instance.ClearHerbPage();
		}
	}

	public void ClearOtherList()
	{
		if (!(CSUI_PlanMgr.Instance == null))
		{
			CSUI_PlanMgr.Instance.ClearOtherPage();
		}
	}

	public bool AddOrePage(int protoId, int maxNum)
	{
		if (CSUI_PlanMgr.Instance == null)
		{
			return false;
		}
		CSUI_PlanMgr.Instance.AddOrePage(protoId, maxNum);
		return true;
	}

	public bool AddHerbPage(int protoId, int maxNum)
	{
		if (CSUI_PlanMgr.Instance == null)
		{
			return false;
		}
		CSUI_PlanMgr.Instance.AddHerbPage(protoId, maxNum);
		return true;
	}

	public bool AddOtherPage(int protoId, int maxNum)
	{
		if (CSUI_PlanMgr.Instance == null)
		{
			return false;
		}
		CSUI_PlanMgr.Instance.AddOtherPage(protoId, maxNum);
		return true;
	}

	public bool RemoveOrePage(int protoId)
	{
		if (CSUI_PlanMgr.Instance == null)
		{
			return false;
		}
		CSUI_PlanMgr.Instance.RemoveOrePage(protoId);
		return true;
	}

	public bool RemoveHerbPage(int protoId)
	{
		if (CSUI_PlanMgr.Instance == null)
		{
			return false;
		}
		CSUI_PlanMgr.Instance.RemoveHerbPage(protoId);
		return true;
	}

	public bool RemoveOtherPage(int protoId)
	{
		if (CSUI_PlanMgr.Instance == null)
		{
			return false;
		}
		CSUI_PlanMgr.Instance.RemoveOtherPage(protoId);
		return true;
	}

	public void ClearPlan()
	{
		if (!(CSUI_PlanMgr.Instance == null) && CSUI_PlanMgr.Instance.PlanList != null)
		{
			CSUI_PlanMgr.Instance.PlanList.ClearList();
		}
	}

	public void UpdateProcess(int index, List<ItemIdCount> list, int NpcNum, float times, int runCount)
	{
		ClearProcess(index);
		foreach (ItemIdCount item in list)
		{
			AddProcess(index, item.protoId, item.count);
		}
		m_Processes[index].SetTime(times);
		m_Processes[index].SetNpcNum(NpcNum);
		m_Processes[index].SetRunCount(runCount);
	}

	public void UpdateTimes(int index, float times)
	{
		m_Processes[index].SetTime(times);
	}

	public void UpdateNpcNum(int index, int NpcNum)
	{
		m_Processes[index].SetNpcNum(NpcNum);
	}

	public void UpdateRepeat(int index, int repeat)
	{
		m_Processes[index].SetRunCount(repeat);
	}

	public void AddProcess(int index, int ProtoID, int NeedNum)
	{
		ProcessInfo processInfo = new ProcessInfo();
		ItemProto itemProto = PeSingleton<ItemProto.Mgr>.Instance.Get(ProtoID);
		if (itemProto != null)
		{
			processInfo.IconName = itemProto.icon[0];
			processInfo.ProtoId = ProtoID;
			processInfo.m_NeedNum = NeedNum;
			processInfo.ProcessNum = index;
			processInfo.ItemId = mProlists[index].Count;
			AddProcessInfo(index, processInfo);
		}
	}

	public void SelectProcessByIndex(int index)
	{
		if (index >= 0 && index < m_Processes.Length)
		{
			m_Index = index;
			m_Processes[index].SelectProcess();
		}
	}

	private void AddProcessInfo(int index, ProcessInfo Info)
	{
		mProlists[index].Add(Info);
		UpdateProcesses(index);
	}

	public void ClearProcess()
	{
		for (int i = 0; i < m_Processes.Length; i++)
		{
			m_Processes[i].ClearGrideInfo();
			m_Processes[i].SetTime(0f);
			m_Processes[i].SetNpcNum(0);
			m_Processes[i].SetRunCount(1);
			mProlists[i].Clear();
			UpdateProcesses(i);
		}
	}

	public bool ClearProcess(int index)
	{
		if (index < 0 || index > m_Processes.Length)
		{
			return false;
		}
		m_Processes[index].ClearGrideInfo();
		m_Processes[index].SetTime(0f);
		m_Processes[index].SetNpcNum(0);
		m_Processes[index].SetRunCount(1);
		if (mProlists.ContainsKey(index))
		{
			mProlists[index].Clear();
		}
		return true;
	}

	private void UpdateProcesses(int index)
	{
		m_Processes[index].UpdateGridInfo(mProlists[index]);
	}

	public void ShowStartBtn(bool show)
	{
		mStartBtn.gameObject.SetActive(show);
		mStopBtn.gameObject.SetActive(!show);
	}

	private void InitProcess()
	{
		for (int i = 0; i < m_Processes.Length; i++)
		{
			m_Processes[i].e_ProcesssClickEvent += OnProcessClick;
			m_Processes[i].e_ItemRemove += OnRemoveItem;
			mProlists[i] = new List<ProcessInfo>();
			m_Processes[i].InitWnd();
		}
	}

	private void UpdataInput()
	{
		if (mAddNumBtnPress)
		{
			SetNumberByTime(Time.time - mNumOpStarTime, ref mOpDurNum);
			if (mOpDurNum + (float)mCurrentNum <= (float)MaxNum)
			{
				mNewNum = (int)(mOpDurNum + (float)mCurrentNum);
			}
			else
			{
				mNewNum = MaxNum;
			}
		}
		else if (mSubNumBtnPress)
		{
			SetNumberByTime(Time.time - mNumOpStarTime, ref mOpDurNum);
			if ((float)mCurrentNum - mOpDurNum >= 1f)
			{
				mNewNum = (int)((float)mCurrentNum - mOpDurNum);
			}
			else
			{
				mNewNum = 1;
			}
		}
		else if (mCurrentNum != mNewNum)
		{
			mCurrentNum = mNewNum;
		}
		if (!mNumInput.selected)
		{
			mNumInput.text = mNewNum.ToString();
		}
		if (mAddRunCountBtnPress)
		{
			SetNumberByTime(Time.time - mRunCountOpStartTime, ref mOpDurRunCount);
			if (mOpDurRunCount + (float)mCurrentRunCount <= (float)MaxRunCount)
			{
				mNewRunCount = (int)(mOpDurRunCount + (float)mCurrentRunCount);
			}
			else
			{
				mNewRunCount = MaxRunCount;
			}
		}
		else if (mSubRunCountBtnPress)
		{
			SetNumberByTime(Time.time - mRunCountOpStartTime, ref mOpDurRunCount);
			if ((float)mCurrentRunCount - mOpDurRunCount >= 1f)
			{
				mNewRunCount = (int)((float)mCurrentRunCount - mOpDurRunCount);
			}
			else
			{
				mNewRunCount = 1;
			}
		}
		else if (mNewRunCount != mCurrentRunCount)
		{
			mCurrentRunCount = mNewRunCount;
		}
		if (!mRunCountInput.selected)
		{
			mRunCountInput.text = mNewRunCount.ToString();
		}
	}

	private void SetNumberByTime(float time, ref float number)
	{
		if (time < 0.3f)
		{
			number = 1f;
		}
		else if (time < 1f)
		{
			number += 2f * Time.deltaTime;
		}
		else if (time < 2f)
		{
			number += 4f * Time.deltaTime;
		}
		else if (time < 3f)
		{
			number += 7f * Time.deltaTime;
		}
		else if (time < 4f)
		{
			number += 11f * Time.deltaTime;
		}
		else if (time < 5f)
		{
			number += 16f * Time.deltaTime;
		}
		else
		{
			number += 20f * Time.deltaTime;
		}
	}

	private void OnNumAddBtnPress()
	{
		mAddNumBtnPress = true;
		mNumOpStarTime = Time.time;
	}

	private void OnNumAddBtnRelease()
	{
		mAddNumBtnPress = false;
	}

	private void OnNumSubstructBtnPress()
	{
		mSubNumBtnPress = true;
		mNumOpStarTime = Time.time;
	}

	private void OnNumSubstructBtnRelease()
	{
		mSubNumBtnPress = false;
	}

	private void OnNumMaxBtn()
	{
		mNewNum = m_MaxNum;
	}

	private void OnNumMinBtn()
	{
		mNewNum = 1;
	}

	private void OnRunCountAddBtnPress()
	{
		mAddRunCountBtnPress = true;
		mRunCountOpStartTime = Time.time;
	}

	private void OnRunCountAddBtnRelease()
	{
		mAddRunCountBtnPress = false;
		OnSetRunCount();
	}

	private void OnRunCountSubstructBtnPress()
	{
		mSubRunCountBtnPress = true;
		mRunCountOpStartTime = Time.time;
	}

	private void OnRunCountSubstructBtnRelease()
	{
		mSubRunCountBtnPress = false;
		OnSetRunCount();
	}

	private void OnRunCountMaxBtn()
	{
		mNewRunCount = m_MaxRunCount;
		OnSetRunCount();
	}

	private void OnRunCountMinBtn()
	{
		mNewRunCount = 1;
		OnSetRunCount();
	}

	private bool ProcessChecked()
	{
		for (int i = 0; i < m_Processes.Length; i++)
		{
			if (m_Processes[i].IsChecked)
			{
				return true;
			}
		}
		return false;
	}

	private void ButtonCtr()
	{
		if (mCurrentNum > 0 && ProcessChecked())
		{
			mAddBtn.disable = false;
		}
		else
		{
			mAddBtn.disable = true;
		}
	}

	private void OnBaseFistBtn(bool active)
	{
		if (this.e_BaseFristClick != null)
		{
			this.e_BaseFristClick(this, active);
		}
	}

	private void OnAutoBtn(bool active)
	{
		if (this.e_AutoClick != null)
		{
			this.e_AutoClick(this, active);
		}
	}

	private void OnAddToBtn()
	{
		if (this.e_AddtoClick != null && mCurrentNum >= 0)
		{
			this.e_AddtoClick(this, m_Index, CurProtoID, Mathf.Clamp(mCurrentNum, 1, m_MaxNum));
		}
	}

	private void OnStartBtn()
	{
		if (this.e_StartClick != null)
		{
			this.e_StartClick(this, m_Index);
		}
	}

	private void OnSetRunCount()
	{
		mCurrentRunCount = mNewRunCount;
		if (this.e_SetRunCountEvent != null && m_BackRunCount != mCurrentRunCount)
		{
			m_BackRunCount = mCurrentRunCount;
			this.e_SetRunCountEvent(this, m_Index, mCurrentRunCount);
		}
	}

	private void OnStopBtn()
	{
		if (this.e_StopClick != null)
		{
			this.e_StopClick(this, m_Index);
		}
	}

	private void InitEnvent()
	{
		if (this.e_InitCollectEvent != null)
		{
			this.e_InitCollectEvent(this);
		}
	}

	public void UpdateCollect()
	{
		if (this.e_UpdateCollect != null)
		{
			this.e_UpdateCollect(this);
		}
	}

	private void OnProcessClick(object sender, int processId)
	{
		CSUI_PrcoessMgr cSUI_PrcoessMgr = sender as CSUI_PrcoessMgr;
		if (cSUI_PrcoessMgr != null)
		{
			m_Index = processId;
			if (this.e_ProcessChoseEvent != null)
			{
				this.e_ProcessChoseEvent(this, m_Index);
			}
			m_BackRunCount = -1;
			mNewRunCount = Mathf.Clamp(cSUI_PrcoessMgr.RunCount, 0, m_MaxRunCount);
		}
	}

	private void OnRemoveItem(object sender, int processId)
	{
		if (this.e_RemoveEvent != null)
		{
			this.e_RemoveEvent(this, m_Index, processId);
		}
	}
}
