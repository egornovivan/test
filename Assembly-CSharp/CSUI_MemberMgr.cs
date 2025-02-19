using System.Collections.Generic;
using UnityEngine;

public class CSUI_MemberMgr : MonoBehaviour
{
	[SerializeField]
	private UIGrid mMemberPageGrid;

	[SerializeField]
	private GameObject mMemberPrefab;

	[SerializeField]
	public Dictionary<MyMemberType, List<MyMemberInf>> mMemberDataList = new Dictionary<MyMemberType, List<MyMemberInf>>();

	private List<CSUI_MemberItemLocal> mMemberObjList = new List<CSUI_MemberItemLocal>();

	private List<MyMemberInf> curMemList = new List<MyMemberInf>();

	private MyMemberType mPageType;

	private static CSUI_MemberMgr mInstance;

	private MyMemberList passList;

	private List<CSPersonnel> mRandomNpcs;

	private List<CSPersonnel> mInstructorNpcs;

	private List<CSPersonnel> mOtherNpcs;

	public static CSUI_MemberMgr Instance => mInstance;

	public MyMemberList PassList
	{
		get
		{
			return passList;
		}
		set
		{
			passList = value;
			InitList();
		}
	}

	public List<CSPersonnel> RandomNpcs
	{
		get
		{
			return mRandomNpcs;
		}
		set
		{
			mRandomNpcs = value;
			SortNpcs();
		}
	}

	private void SortNpcs()
	{
		if (mRandomNpcs.Count > 0)
		{
			mInstructorNpcs.Clear();
			mOtherNpcs.Clear();
			for (int i = 0; i < mRandomNpcs.Count; i++)
			{
			}
		}
	}

	private void InitList()
	{
		mMemberDataList[MyMemberType.Trainee] = passList.traineeList;
		mMemberDataList[MyMemberType.Instructor] = passList.instructorList;
		Refresh(MyMemberType.Trainee);
	}

	private void Refresh(MyMemberType type)
	{
		ClearMemberList();
		foreach (KeyValuePair<MyMemberType, List<MyMemberInf>> mMemberData in mMemberDataList)
		{
			if (mMemberData.Key == type)
			{
				curMemList = mMemberData.Value;
			}
		}
		foreach (MyMemberInf curMem in curMemList)
		{
			CreatMemberItem(curMem);
		}
	}

	private void CreatMemberItem(MyMemberInf Info)
	{
		GameObject gameObject = Object.Instantiate(mMemberPrefab);
		gameObject.transform.parent = mMemberPageGrid.transform;
		gameObject.transform.localScale = Vector3.one;
		gameObject.transform.localPosition = Vector3.zero;
		if (!gameObject.activeSelf)
		{
			gameObject.SetActive(value: true);
		}
		CSUI_MemberItemLocal component = gameObject.GetComponent<CSUI_MemberItemLocal>();
		component.MemberInfoLocal = Info;
		mMemberObjList.Add(component);
		mMemberPageGrid.repositionNow = true;
	}

	private void ClearMemberList()
	{
		foreach (CSUI_MemberItemLocal mMemberObj in mMemberObjList)
		{
			if (mMemberObj != null)
			{
				Object.Destroy(mMemberObj.gameObject);
				mMemberObj.gameObject.transform.parent = null;
			}
		}
		mMemberObjList.Clear();
	}

	private void PageTraineeOnActive(bool active)
	{
		if (active)
		{
			mPageType = MyMemberType.Trainee;
			Refresh(mPageType);
		}
	}

	private void PageInstructorOnActive(bool active)
	{
		if (active)
		{
			mPageType = MyMemberType.Instructor;
			Refresh(mPageType);
		}
	}

	public void Test()
	{
		if (!(CSUI_MainWndCtrl.Instance == null))
		{
		}
	}

	private void Awake()
	{
		mInstance = this;
	}

	private void Update()
	{
	}
}
