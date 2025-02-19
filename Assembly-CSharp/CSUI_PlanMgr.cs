using System.Collections.Generic;
using ItemAsset;
using Pathea;
using UnityEngine;

public class CSUI_PlanMgr : MonoBehaviour
{
	private static CSUI_PlanMgr mInstance;

	[SerializeField]
	public UIGrid mOrePageGrid;

	[SerializeField]
	public UIGrid mHerbPageGrid;

	[SerializeField]
	public UIGrid mOtherPageGrid;

	[SerializeField]
	public GameObject m_GridPrefab;

	[SerializeField]
	public GameObject m_AdjustObj;

	private List<CSUI_PageGrid> m_OreList = new List<CSUI_PageGrid>();

	private List<CSUI_PageGrid> m_HerbList = new List<CSUI_PageGrid>();

	private List<CSUI_PageGrid> m_OtherList = new List<CSUI_PageGrid>();

	private PageType mPageType;

	public Dictionary<PageType, List<GridInfo>> mPageList = new Dictionary<PageType, List<GridInfo>>();

	private List<GridInfo> curPage = new List<GridInfo>();

	private GridList m_PlanList;

	private PageType PageTye;

	private CSUI_PageGrid m_BackupGird;

	[SerializeField]
	private GameObject m_OrePage;

	[SerializeField]
	private GameObject m_HerbPage;

	[SerializeField]
	private GameObject m_OtherPage;

	public static CSUI_PlanMgr Instance => mInstance;

	public GridList PlanList
	{
		get
		{
			return m_PlanList;
		}
		set
		{
			m_PlanList = value;
			InitPlan();
		}
	}

	private void Awake()
	{
		mInstance = this;
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	private void InitPlan()
	{
		mPageList[PageType.Ore] = new List<GridInfo>();
		mPageList[PageType.Ore] = m_PlanList.OreList;
		mPageList[PageType.Herb] = new List<GridInfo>();
		mPageList[PageType.Herb] = m_PlanList.HerbList;
		mPageList[PageType.Other] = new List<GridInfo>();
		mPageList[PageType.Other] = m_PlanList.OtherList;
		Relash(PageType.Ore);
	}

	public void AddOrePage(int protoId, int MaxNum)
	{
		PageTye = PageType.Ore;
		AddPageInfo(protoId, MaxNum);
	}

	public void AddHerbPage(int protoId, int MaxNum)
	{
		PageTye = PageType.Herb;
		AddPageInfo(protoId, MaxNum);
	}

	public void AddOtherPage(int protoId, int MaxNum)
	{
		PageTye = PageType.Other;
		AddPageInfo(protoId, MaxNum);
	}

	public void RemoveOrePage(int protoId)
	{
		PageTye = PageType.Ore;
		RemovePageInfo(protoId);
	}

	public void RemoveHerbPage(int protoId)
	{
		PageTye = PageType.Herb;
		RemovePageInfo(protoId);
	}

	public void RemoveOtherPage(int protoId)
	{
		PageTye = PageType.Other;
		RemovePageInfo(protoId);
	}

	public void ClearOrePage()
	{
		mPageList[PageType.Ore].Clear();
		Relash(PageType.Ore);
	}

	public void ClearHerbPage()
	{
		mPageList[PageType.Herb].Clear();
		Relash(PageType.Herb);
	}

	public void ClearOtherPage()
	{
		mPageList[PageType.Other].Clear();
		Relash(PageType.Other);
	}

	private void AddPageInfo(int protoId, int MaxNum)
	{
		GridInfo gridInfo = new GridInfo();
		ItemProto itemProto = PeSingleton<ItemProto.Mgr>.Instance.Get(protoId);
		if (itemProto != null)
		{
			gridInfo.IconName = itemProto.icon;
			gridInfo.mProtoId = protoId;
			gridInfo.MaxNum = MaxNum;
		}
		if (mPageList.Count == 0)
		{
			mPageList[PageType.Ore] = new List<GridInfo>();
			mPageList[PageType.Herb] = new List<GridInfo>();
			mPageList[PageType.Other] = new List<GridInfo>();
		}
		mPageList[PageTye].Add(gridInfo);
		Relash(PageTye);
	}

	private bool RemovePageInfo(int protoId)
	{
		GridInfo gridInfo = mPageList[PageTye].Find((GridInfo item) => (item.mProtoId == protoId) ? true : false);
		if (gridInfo != null)
		{
			return RemovePage(gridInfo);
		}
		return false;
	}

	private bool RemovePage(GridInfo Info)
	{
		if (mPageList[PageTye].Contains(Info))
		{
			mPageList[PageTye].Remove(Info);
			Relash(PageTye);
			return true;
		}
		return false;
	}

	private void Relash(PageType type)
	{
		switch (type)
		{
		case PageType.Ore:
			ClearOreGrid();
			break;
		case PageType.Herb:
			ClearHerbGrid();
			break;
		case PageType.Other:
			ClearOtherGrid();
			break;
		}
		foreach (KeyValuePair<PageType, List<GridInfo>> mPage in mPageList)
		{
			if (mPage.Key == type)
			{
				curPage = mPage.Value;
				break;
			}
		}
		foreach (GridInfo item in curPage)
		{
			switch (type)
			{
			case PageType.Ore:
				AddOreGrid(item);
				break;
			case PageType.Herb:
				AddHerbGrid(item);
				break;
			case PageType.Other:
				AddOtherGrid(item);
				break;
			}
		}
	}

	private void ClearOreGrid()
	{
		foreach (CSUI_PageGrid ore in m_OreList)
		{
			if (ore != null)
			{
				Object.Destroy(ore.gameObject);
				ore.gameObject.transform.parent = null;
			}
		}
		m_OreList.Clear();
	}

	private void ClearHerbGrid()
	{
		foreach (CSUI_PageGrid herb in m_HerbList)
		{
			if (herb != null)
			{
				Object.Destroy(herb.gameObject);
				herb.gameObject.transform.parent = null;
			}
		}
		m_HerbList.Clear();
	}

	private void ClearOtherGrid()
	{
		foreach (CSUI_PageGrid other in m_OtherList)
		{
			if (other != null)
			{
				Object.Destroy(other.gameObject);
				other.gameObject.transform.parent = null;
			}
		}
		m_OtherList.Clear();
	}

	private void AddOreGrid(GridInfo Info)
	{
		GameObject gameObject = Object.Instantiate(m_GridPrefab);
		gameObject.transform.parent = mOrePageGrid.transform;
		gameObject.transform.localScale = Vector3.one;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.SetActive(value: true);
		CSUI_PageGrid component = gameObject.GetComponent<CSUI_PageGrid>();
		component.mGridInfo = Info;
		component.Type = ListItemType.mItem;
		component.e_ItemClick += ItemClick;
		m_OreList.Add(component);
		mOrePageGrid.repositionNow = true;
	}

	private void AddHerbGrid(GridInfo Info)
	{
		GameObject gameObject = Object.Instantiate(m_GridPrefab);
		gameObject.transform.parent = mHerbPageGrid.transform;
		gameObject.transform.localScale = Vector3.one;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.SetActive(value: true);
		CSUI_PageGrid component = gameObject.GetComponent<CSUI_PageGrid>();
		component.mGridInfo = Info;
		component.e_ItemClick += ItemClick;
		m_HerbList.Add(component);
		mHerbPageGrid.repositionNow = true;
	}

	private void AddOtherGrid(GridInfo Info)
	{
		GameObject gameObject = Object.Instantiate(m_GridPrefab);
		gameObject.transform.parent = mOtherPageGrid.transform;
		gameObject.transform.localScale = Vector3.one;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.SetActive(value: true);
		CSUI_PageGrid component = gameObject.GetComponent<CSUI_PageGrid>();
		component.mGridInfo = Info;
		component.e_ItemClick += ItemClick;
		m_OtherList.Add(component);
		mOtherPageGrid.repositionNow = true;
	}

	private void ItemClick(object sender)
	{
		CSUI_PageGrid cSUI_PageGrid = sender as CSUI_PageGrid;
		if (cSUI_PageGrid != null && cSUI_PageGrid != m_BackupGird)
		{
			if (null != m_BackupGird)
			{
				m_BackupGird.ShowGridSeclect(show: false);
			}
			m_BackupGird = cSUI_PageGrid;
			m_AdjustObj.SetActive(value: true);
			if (CSUI_MainWndCtrl.Instance != null)
			{
				CSUI_MainWndCtrl.Instance.CollectUI.MaxNum = cSUI_PageGrid.MaxNum;
				CSUI_MainWndCtrl.Instance.CollectUI.CurProtoID = cSUI_PageGrid.mGridInfo.mProtoId;
			}
		}
	}

	private void PageOREOnActive(bool active)
	{
		m_OrePage.SetActive(active);
		if (active)
		{
			mPageType = PageType.Ore;
			Relash(mPageType);
		}
	}

	private void PageHerbOnActive(bool active)
	{
		m_HerbPage.SetActive(active);
		if (active)
		{
			mPageType = PageType.Herb;
			Relash(mPageType);
		}
	}

	private void PageOtherOnActive(bool active)
	{
		m_OtherPage.SetActive(active);
		if (active)
		{
			mPageType = PageType.Other;
			Relash(mPageType);
		}
	}
}
