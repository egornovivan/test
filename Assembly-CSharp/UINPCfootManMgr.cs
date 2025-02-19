using System.Collections;
using System.Collections.Generic;
using Pathea;
using Pathea.PeEntityExt;
using UnityEngine;

public class UINPCfootManMgr : MonoBehaviour
{
	public class FootmanInfo
	{
		public int Num;

		public int Followerid;

		public Texture mTexture;

		public float Hppercent;

		public NpcCmpt mNpCmpt;
	}

	private static UINPCfootManMgr mInstance;

	[SerializeField]
	private Transform Centent;

	[SerializeField]
	private GameObject UIfootManItemPrefab;

	[HideInInspector]
	public UIfootManItem mUIfootManItem;

	[SerializeField]
	private UIGrid mGird;

	private List<FootmanInfo> mInfoList = new List<FootmanInfo>();

	public List<UIfootManItem> mItemList = new List<UIfootManItem>();

	private PeEntity mEntity;

	public Vector3 newPos;

	private List<SkAliveEntity> mAlive = new List<SkAliveEntity>();

	private int countFollower;

	private float m_deathTime;

	private float m_DelayTime = 100000f;

	public bool m_dead;

	private SkAliveEntity m_deadNpc;

	public static UINPCfootManMgr Instance => mInstance;

	public PeEntity Entity
	{
		get
		{
			if (mEntity == null)
			{
				mEntity = GetComponent<PeEntity>();
			}
			return mEntity;
		}
	}

	private void Awake()
	{
		mInstance = this;
		AddFootManItem(ServantLeaderCmpt.mMaxFollower);
		StartCoroutine(NewGetFollowerAlive());
	}

	private IEnumerator NewGetFollowerAlive()
	{
		yield return new WaitForSeconds(5f);
		GetFollowerAlive();
	}

	private void IntServant()
	{
	}

	private void Start()
	{
		if (!(ServantLeaderCmpt.Instance == null))
		{
		}
	}

	private void Update()
	{
		Centent.localPosition = new Vector3(0f - UIMinMapCtrl.Instance.GetMinMapWidth() - 20f, -5f, -22f);
		GameUI.Instance.mRevive.mUpdate();
	}

	public void GetFollowerAlive()
	{
		if (ServantLeaderCmpt.Instance == null)
		{
			return;
		}
		mInfoList.Clear();
		mAlive.Clear();
		NpcCmpt[] mFollowers = ServantLeaderCmpt.Instance.mFollowers;
		foreach (NpcCmpt npcCmpt in mFollowers)
		{
			if (npcCmpt != null && npcCmpt.Alive != null)
			{
				mAlive.Add(npcCmpt.Alive);
				FootmanInfo footmanInfo = new FootmanInfo();
				footmanInfo.mNpCmpt = npcCmpt;
				footmanInfo.mTexture = npcCmpt.Follwerentity.ExtGetFaceTex();
				mInfoList.Add(footmanInfo);
			}
		}
		for (int j = 0; j < mItemList.Count; j++)
		{
			mItemList[j].gameObject.SetActive(value: true);
			if (j < mInfoList.Count)
			{
				mItemList[j].FootmanInfo = mInfoList[j];
			}
			else
			{
				mItemList[j].FootmanInfo = null;
			}
		}
		for (int k = 0; k < mItemList.Count; k++)
		{
			if (k < mAlive.Count)
			{
				mItemList[k].SkEntity = mAlive[k];
			}
			else
			{
				mItemList[k].SkEntity = null;
			}
		}
		mGird.Reposition();
	}

	public void AddFootManInfo(FootmanInfo Info)
	{
		mInfoList.Add(Info);
	}

	private void AddFootManItem(int _count)
	{
		for (int i = 0; i < _count; i++)
		{
			GameObject gameObject = Object.Instantiate(UIfootManItemPrefab);
			gameObject.transform.parent = mGird.transform;
			gameObject.transform.localScale = Vector3.one;
			gameObject.transform.localPosition = Vector3.zero;
			UIfootManItem component = gameObject.GetComponent<UIfootManItem>();
			component.mIndex = i;
			gameObject.SetActive(value: false);
			mItemList.Add(component);
		}
		mGird.repositionNow = true;
	}

	private void ChoseOnClick(object sender, ENpcBattle type)
	{
		UIfootManItem uIfootManItem = sender as UIfootManItem;
		if (!(null != uIfootManItem))
		{
		}
	}

	private void Clear()
	{
		foreach (UIfootManItem mItem in mItemList)
		{
			if (mItem != null)
			{
				Object.Destroy(mItem.gameObject);
				mItem.gameObject.transform.parent = null;
			}
		}
		mItemList.Clear();
	}
}
