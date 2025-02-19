using System;
using System.Collections.Generic;
using ItemAsset;
using Pathea;
using UnityEngine;

public class CSUI_PrcoessMgr : MonoBehaviour
{
	public delegate void ProcesssClickEvent(object sender, int processId);

	public delegate void ItemRemove(object sender, int protoId);

	[SerializeField]
	public UIGrid mProcessGrid;

	[SerializeField]
	public GameObject m_ProcessGridPrefab;

	[SerializeField]
	private UICheckbox mProcessSeclect;

	private List<CSUI_PrcoessGrid> m_PrcoessItemList = new List<CSUI_PrcoessGrid>();

	public UILabel mProcessNumLb;

	public UILabel mNpcNumLb;

	public UILabel mTimeLb;

	public UILabel mRunCountLb;

	private List<ProcessInfo> m_ProcessInfoList = new List<ProcessInfo>();

	private int m_RunCount;

	private int mProcessId;

	public bool IsChecked;

	private float m_Times;

	public int RunCount => m_RunCount;

	public int ProcessId
	{
		get
		{
			return mProcessId;
		}
		set
		{
			mProcessId = value;
		}
	}

	public float Times
	{
		get
		{
			return m_Times;
		}
		set
		{
			m_Times = value;
		}
	}

	public event ProcesssClickEvent e_ProcesssClickEvent;

	public event ItemRemove e_ItemRemove;

	private void Awake()
	{
		InitWnd();
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void InitWnd()
	{
		GetComponent<UICheckbox>().radioButtonRoot = base.transform.parent;
		mProcessId = Convert.ToInt32(mProcessNumLb.text);
		for (int i = 0; i < 12; i++)
		{
			ProcessInfo info = new ProcessInfo();
			AddProcessGrid(info);
		}
		if (IsChecked)
		{
			mProcessSeclect.isChecked = IsChecked;
		}
	}

	public void SetNpcNum(int npcNum)
	{
		mNpcNumLb.text = "NPC:" + npcNum;
	}

	public void SetTime(float times)
	{
		int num = (int)times;
		m_Times = times;
		int num2 = num / 60 / 60 % 24;
		int num3 = num / 60 % 60;
		int num4 = num % 60;
		mTimeLb.text = num2 + ":" + num3 + ":" + num4;
	}

	public void SetRunCount(int runCount)
	{
		m_RunCount = runCount;
		mRunCountLb.text = PELocalization.GetString(8000593) + runCount;
	}

	private void SetProcessNum(int Num)
	{
		ProcessId = Num;
	}

	public void AddGerid(GridInfo Info)
	{
		if (m_ProcessInfoList.Count <= 12)
		{
			ProcessInfo processInfo = new ProcessInfo();
			processInfo.IconName = Info.IconName[0];
			processInfo.m_NeedNum = (int)Info.CurrentNum;
			processInfo.ItemId = Info.mProtoId;
			m_ProcessInfoList.Add(processInfo);
			ReflashItem();
		}
	}

	public void UpdateGridInfo(List<ProcessInfo> infoList)
	{
		foreach (ProcessInfo info in infoList)
		{
			SetProcessNum(info.ProcessNum);
			m_PrcoessItemList[info.ItemId].UpdateGridInfo(info);
		}
	}

	public void RemoveGrid(int itemid)
	{
		foreach (CSUI_PrcoessGrid prcoessItem in m_PrcoessItemList)
		{
			if (prcoessItem.ItemID == itemid)
			{
				prcoessItem.ClearInfo();
			}
		}
	}

	public void ClearGrideInfo()
	{
		foreach (CSUI_PrcoessGrid prcoessItem in m_PrcoessItemList)
		{
			prcoessItem.ClearInfo();
		}
	}

	public void AddProcessGrid(int protoId, int needNum)
	{
		if (m_ProcessInfoList.Count <= 10)
		{
			ProcessInfo processInfo = new ProcessInfo();
			ItemProto itemProto = PeSingleton<ItemProto.Mgr>.Instance.Get(protoId);
			if (itemProto != null)
			{
				processInfo.IconName = itemProto.icon[0];
				processInfo.m_NeedNum = needNum;
				processInfo.ItemId = protoId;
				m_ProcessInfoList.Add(processInfo);
				ReflashItem();
			}
		}
	}

	public void AddProcessGrid(ProcessInfo Info)
	{
		if (Info != null && m_ProcessInfoList.Count < 12)
		{
			m_ProcessInfoList.Add(Info);
			ReflashItem();
		}
	}

	public void RemoveProcessGrid(ProcessInfo Info)
	{
		if (Info != null && m_ProcessInfoList.Remove(Info))
		{
			ReflashItem();
		}
	}

	public void ClearProcessGrid()
	{
		m_ProcessInfoList.Clear();
		ReflashItem();
	}

	public void SelectProcess()
	{
		OnProcessActivate(active: true);
	}

	private void ReflashItem()
	{
		ClearGrid();
		foreach (ProcessInfo processInfo in m_ProcessInfoList)
		{
			AddProcessItem(processInfo);
		}
	}

	private void AddProcessItem(ProcessInfo Info)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(m_ProcessGridPrefab);
		gameObject.transform.parent = mProcessGrid.transform;
		gameObject.transform.localScale = Vector3.one;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.SetActive(value: true);
		CSUI_PrcoessGrid component = gameObject.GetComponent<CSUI_PrcoessGrid>();
		component.mProcessInfo = Info;
		component.e_OnDeleteClick += OnDelete;
		component.e_OnSelectClick += OnSelect;
		m_PrcoessItemList.Add(component);
		mProcessGrid.repositionNow = true;
	}

	private void ClearGrid()
	{
		foreach (CSUI_PrcoessGrid prcoessItem in m_PrcoessItemList)
		{
			if (prcoessItem != null)
			{
				UnityEngine.Object.Destroy(prcoessItem.gameObject);
				prcoessItem.gameObject.transform.parent = null;
			}
		}
		m_PrcoessItemList.Clear();
	}

	private void OnDelete(object sender, int ItemId, int ProtoId)
	{
		CSUI_PrcoessGrid cSUI_PrcoessGrid = sender as CSUI_PrcoessGrid;
		if (cSUI_PrcoessGrid != null && this.e_ItemRemove != null)
		{
			this.e_ItemRemove(this, ProtoId);
		}
	}

	private void OnSelect(object sender)
	{
		CSUI_PrcoessGrid cSUI_PrcoessGrid = sender as CSUI_PrcoessGrid;
		if (!(cSUI_PrcoessGrid != null))
		{
		}
	}

	private void OnProcesssBtn()
	{
	}

	private void OnProcessActivate(bool active)
	{
		IsChecked = active;
		if (this.e_ProcesssClickEvent != null && active)
		{
			this.e_ProcesssClickEvent(this, mProcessId);
		}
		mProcessSeclect.isChecked = active;
		foreach (CSUI_PrcoessGrid prcoessItem in m_PrcoessItemList)
		{
			prcoessItem.SetGridBox(active);
		}
	}
}
