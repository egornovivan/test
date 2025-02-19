using System.Collections.Generic;
using UnityEngine;

public class CSUI_WorkWnd : MonoBehaviour
{
	[HideInInspector]
	public CSEntity m_Entity;

	public GameObject mWorkerItemPrefab;

	public UIGrid mWorkerGrid;

	private List<CSUI_WorkItem> mUIWokerList;

	private List<CSPersonnel> mPersonnelList;

	private int m_EntityType = -1;

	private void Awake()
	{
		mUIWokerList = new List<CSUI_WorkItem>();
		mPersonnelList = new List<CSPersonnel>();
	}

	private void Start()
	{
		AddCSUI_WorkeItem(8);
	}

	private void AddCSUI_WorkeItem(int count)
	{
		for (int i = 0; i < count; i++)
		{
			GameObject gameObject = Object.Instantiate(mWorkerItemPrefab);
			gameObject.transform.parent = mWorkerGrid.transform;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localScale = Vector3.one;
			CSUI_WorkItem component = gameObject.GetComponent<CSUI_WorkItem>();
			mUIWokerList.Add(component);
			gameObject.SetActive(value: false);
		}
	}

	private void Update()
	{
		if (m_Entity != null && ChangeEntityWorkers())
		{
			RefalshWorkersGrid();
		}
	}

	private bool ChangeEntityWorkers()
	{
		if (m_EntityType != m_Entity.m_Type)
		{
			m_EntityType = m_Entity.m_Type;
			return true;
		}
		if (!(m_Entity is CSCommon cSCommon))
		{
			return false;
		}
		if (cSCommon.WorkerMaxCount != mPersonnelList.Count)
		{
			return true;
		}
		for (int i = 0; i < cSCommon.WorkerMaxCount; i++)
		{
			if (cSCommon.Worker(i) != mPersonnelList[i])
			{
				return true;
			}
		}
		return false;
	}

	private void RefalshWorkersGrid()
	{
		if (!(m_Entity is CSCommon cSCommon))
		{
			return;
		}
		if (cSCommon.WorkerCount > mUIWokerList.Count)
		{
			AddCSUI_WorkeItem(cSCommon.WorkerCount - mUIWokerList.Count);
		}
		int num = 0;
		for (int i = 0; i < cSCommon.WorkerMaxCount; i++)
		{
			if (cSCommon.Worker(i) != null)
			{
				mUIWokerList[num].SetWorker(cSCommon.Worker(i));
				mUIWokerList[num].gameObject.SetActive(value: true);
				num++;
			}
		}
		for (int j = num; j < mUIWokerList.Count; j++)
		{
			mUIWokerList[j].gameObject.SetActive(value: false);
		}
		mPersonnelList.Clear();
		for (int k = 0; k < cSCommon.WorkerMaxCount; k++)
		{
			mPersonnelList.Add(cSCommon.Worker(k));
		}
		mWorkerGrid.repositionNow = true;
	}
}
