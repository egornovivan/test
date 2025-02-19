using System.Collections.Generic;
using ItemAsset;
using Pathea;
using UnityEngine;

public class CSUI_Transaction : MonoBehaviour
{
	public class CampInfo
	{
		public string Name;
	}

	public class ExchangeInfo
	{
		public Texture mTexture;

		public string mIcon;

		public int mMaxNum;

		public int mCurentNum;

		public int mProtoId;
	}

	public class GetInfo
	{
		public string mIcon;

		public int mMaxNum;

		public int mProtoId;
	}

	public delegate void ExchangeClick(object sender);

	public delegate void UpdateTrade(object sender);

	public delegate void CampClick(CSUI_CampItem CampItem);

	[SerializeField]
	public UIGrid mMidGrid;

	[SerializeField]
	public GameObject m_ExchangeItemPrefab;

	[SerializeField]
	public UIGrid mGetGrid;

	[SerializeField]
	public GameObject m_GetItemPrefab;

	[SerializeField]
	public UIGrid mCampGrid;

	[SerializeField]
	public GameObject m_CampItemPrefab;

	[SerializeField]
	public GameObject m_TradeSuccessful;

	[SerializeField]
	public GameObject m_UpdateTime;

	[SerializeField]
	public GameObject m_NPCInfo;

	[SerializeField]
	private UITexture mNPCTexture;

	[SerializeField]
	private UISprite mNPCHead;

	[SerializeField]
	private UILabel mNpcNameLb;

	[SerializeField]
	private UILabel mNpcTalkLb;

	[SerializeField]
	private UILabel mUpdateTiemLb;

	[SerializeField]
	private N_ImageButton mExhangeBtn;

	private List<CSUI_CampItem> m_CampItem = new List<CSUI_CampItem>();

	private List<CSUI_EXchangeItem> m_EXchangeItem = new List<CSUI_EXchangeItem>();

	private List<CSUI_GetItem> m_GetItem = new List<CSUI_GetItem>();

	public List<CampInfo> m_CampList = new List<CampInfo>();

	public List<ExchangeInfo> m_ExchangeList = new List<ExchangeInfo>();

	public List<GetInfo> m_GetList = new List<GetInfo>();

	private PlayerPackageCmpt playerPackageCmpt;

	private PeEntity player;

	private List<CSUI_ItemInfo> InfoList = new List<CSUI_ItemInfo>();

	private List<CSUI_ItemInfo> InfoList2 = new List<CSUI_ItemInfo>();

	public Texture NPCTexture
	{
		set
		{
			mNPCTexture.mainTexture = value;
		}
	}

	public string NpcName
	{
		set
		{
			mNpcNameLb.text = value;
		}
	}

	public event ExchangeClick e_ExchangeClick;

	public event UpdateTrade e_UpdateTrade;

	public event CampClick e_CampClick;

	private void Awake()
	{
		UpdateEvent();
	}

	public void UpdateEvent()
	{
		if (this.e_UpdateTrade != null)
		{
			this.e_UpdateTrade(this);
		}
		using List<CSUI_CampItem>.Enumerator enumerator = m_CampItem.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			return;
		}
		CSUI_CampItem current = enumerator.Current;
		if (!(current == null))
		{
			current.SetChoeBg(Show: true);
			if (this.e_CampClick != null)
			{
				this.e_CampClick(current);
			}
			mExhangeBtn.disable = false;
		}
	}

	private void Start()
	{
		using List<CSUI_CampItem>.Enumerator enumerator = m_CampItem.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			return;
		}
		CSUI_CampItem current = enumerator.Current;
		if (!(current == null))
		{
			current.SetChoeBg(Show: true);
			if (this.e_CampClick != null)
			{
				this.e_CampClick(current);
			}
		}
	}

	private void Update()
	{
		UpdatePackageNum();
	}

	private void UpdatePackageNum()
	{
		if (!(PeSingleton<PeCreature>.Instance.mainPlayer != null))
		{
			return;
		}
		if (playerPackageCmpt == null || player == null || player != PeSingleton<PeCreature>.Instance.mainPlayer)
		{
			player = PeSingleton<PeCreature>.Instance.mainPlayer;
			playerPackageCmpt = player.GetCmpt<PlayerPackageCmpt>();
		}
		foreach (CSUI_EXchangeItem item in m_EXchangeItem)
		{
			if (item.ProtoId > 0)
			{
				item.PackageNum = playerPackageCmpt.package.GetCount(item.ProtoId);
			}
		}
		foreach (CSUI_GetItem item2 in m_GetItem)
		{
			if (item2.ProtoId > 0)
			{
				item2.PackageNum = playerPackageCmpt.package.GetCount(item2.ProtoId);
			}
		}
	}

	private void Test()
	{
		for (int i = 0; i < 3; i++)
		{
			CampInfo campInfo = new CampInfo();
			campInfo.Name = "Camp" + i;
			m_CampList.Add(campInfo);
		}
		for (int j = 0; j < 5; j++)
		{
			ExchangeInfo exchangeInfo = new ExchangeInfo();
			exchangeInfo.mMaxNum = 50;
			exchangeInfo.mCurentNum = 50 - j;
			m_ExchangeList.Add(exchangeInfo);
			GetInfo getInfo = new GetInfo();
			getInfo.mMaxNum = j + 1;
			m_GetList.Add(getInfo);
		}
		Reflash();
	}

	private void Test2()
	{
		for (int i = 0; i < 3; i++)
		{
			string camp = "Camp" + i;
			SetCamp(camp);
		}
		for (int j = 1; j < 8; j++)
		{
			SetExchangeInfo(j + 50, 40);
		}
		for (int k = 1; k < 8; k++)
		{
			SetGet(k + 80, 40);
		}
	}

	public List<CSUI_ItemInfo> GetTheExChangeList()
	{
		InfoList.Clear();
		foreach (CSUI_EXchangeItem item in m_EXchangeItem)
		{
			CSUI_ItemInfo cSUI_ItemInfo = new CSUI_ItemInfo();
			cSUI_ItemInfo.protoId = item.ProtoId;
			cSUI_ItemInfo.Number = (int)item.CurrentNum;
			InfoList.Add(cSUI_ItemInfo);
		}
		return InfoList;
	}

	public List<CSUI_ItemInfo> GetThegetList()
	{
		InfoList2.Clear();
		foreach (CSUI_GetItem item in m_GetItem)
		{
			CSUI_ItemInfo cSUI_ItemInfo = new CSUI_ItemInfo();
			cSUI_ItemInfo.protoId = item.ProtoId;
			cSUI_ItemInfo.Number = (int)item.CurrentNum;
			InfoList2.Add(cSUI_ItemInfo);
		}
		return InfoList2;
	}

	public void ClearNpcshow()
	{
		mNPCHead.gameObject.SetActive(value: false);
		mNPCTexture.gameObject.SetActive(value: false);
		mNpcNameLb.text = "Name";
		m_NPCInfo.SetActive(value: false);
		mExhangeBtn.disable = true;
	}

	public void SetCSEntity(List<CSEntity> menList)
	{
		if (menList == null || menList.Count <= 0)
		{
			return;
		}
		CSEntity mSelectedEnntity = menList[0];
		if (menList.Count > 1)
		{
			foreach (CSEntity men in menList)
			{
				if (men.IsRunning)
				{
					mSelectedEnntity = men;
					break;
				}
			}
		}
		CSUI_MainWndCtrl.Instance.mSelectedEnntity = mSelectedEnntity;
	}

	public void SetUpdaTime(float mTimes)
	{
		if (mTimes < 0f)
		{
			m_UpdateTime.SetActive(value: false);
			return;
		}
		int num = (int)mTimes;
		int num2 = num / 60 / 60 % 24;
		int num3 = num / 60 % 60;
		int num4 = num % 60;
		m_UpdateTime.SetActive(value: true);
		mUpdateTiemLb.text = num2 + ":" + num3 + ":" + num4;
	}

	public void SetNpcHeadShow(string Name)
	{
		m_NPCInfo.SetActive(value: true);
		if (Name == string.Empty)
		{
			mNPCHead.spriteName = "null";
			mNPCHead.gameObject.SetActive(value: false);
		}
		else
		{
			mNPCHead.spriteName = Name;
			mNPCHead.gameObject.SetActive(value: true);
		}
	}

	public void SetNpcHeadShow(Texture Tex)
	{
		m_NPCInfo.SetActive(value: true);
		if (Tex != null)
		{
			mNPCTexture.gameObject.SetActive(value: false);
			return;
		}
		mNPCTexture.mainTexture = Tex;
		mNPCTexture.gameObject.SetActive(value: true);
	}

	public void ShowTradeSuccess(bool Show)
	{
		m_TradeSuccessful.SetActive(Show);
	}

	public void NPCTalk(string npctalk)
	{
		mNpcTalkLb.text = npctalk;
	}

	public void SetExchangeInfo(int protoId, int MaxNum)
	{
		ExchangeInfo exchangeInfo = new ExchangeInfo();
		exchangeInfo.mMaxNum = MaxNum;
		ItemProto itemProto = PeSingleton<ItemProto.Mgr>.Instance.Get(protoId);
		if (itemProto != null)
		{
			exchangeInfo.mIcon = itemProto.icon[0];
			exchangeInfo.mProtoId = protoId;
		}
		AddExchange(exchangeInfo);
	}

	public void SetCamp(string CampName)
	{
		CampInfo campInfo = new CampInfo();
		campInfo.Name = CampName;
		AddCamp(campInfo);
	}

	public void SetGet(int protoId, int GetNum)
	{
		GetInfo getInfo = new GetInfo();
		getInfo.mMaxNum = GetNum;
		ItemProto itemProto = PeSingleton<ItemProto.Mgr>.Instance.Get(protoId);
		if (itemProto != null)
		{
			getInfo.mIcon = itemProto.icon[0];
			getInfo.mProtoId = protoId;
		}
		AddGet(getInfo);
	}

	public void ClearExchange()
	{
		m_ExchangeList.Clear();
		ReflashMid();
	}

	public void ClearCamp()
	{
		m_CampList.Clear();
		ReflashCamp();
	}

	public void ClearGet()
	{
		m_GetList.Clear();
		ReflashGet();
	}

	public void AddCamp(CampInfo Info)
	{
		m_CampList.Add(Info);
		ReflashCamp();
	}

	public void AddExchange(ExchangeInfo Info)
	{
		m_ExchangeList.Add(Info);
		ReflashMid();
	}

	public void AddGet(GetInfo Info)
	{
		m_GetList.Add(Info);
		ReflashGet();
	}

	private void Reflash()
	{
		ReflashMid();
		ReflashGet();
		ReflashCamp();
	}

	private void ReflashMid()
	{
		ClearExhangeItem();
		foreach (ExchangeInfo exchange in m_ExchangeList)
		{
			AddMidItem(exchange);
		}
	}

	private void AddMidItem(ExchangeInfo Info)
	{
		GameObject gameObject = Object.Instantiate(m_ExchangeItemPrefab);
		gameObject.transform.parent = mMidGrid.transform;
		gameObject.transform.localScale = Vector3.one;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.SetActive(value: true);
		CSUI_EXchangeItem component = gameObject.GetComponent<CSUI_EXchangeItem>();
		component.SetMaxNum(Info.mMaxNum);
		component.SetIcon(Info.mIcon);
		component.ProtoId = Info.mProtoId;
		component.Type = ListItemType.mItem;
		m_EXchangeItem.Add(component);
		mMidGrid.repositionNow = true;
	}

	private void ClearExhangeItem()
	{
		foreach (CSUI_EXchangeItem item in m_EXchangeItem)
		{
			if (item != null)
			{
				Object.Destroy(item.gameObject);
				item.gameObject.transform.parent = null;
			}
		}
		m_EXchangeItem.Clear();
	}

	private void ClearGetItem()
	{
		foreach (CSUI_GetItem item in m_GetItem)
		{
			if (item != null)
			{
				Object.Destroy(item.gameObject);
				item.gameObject.transform.parent = null;
			}
		}
		m_GetItem.Clear();
	}

	private void ClearCampItem()
	{
		foreach (CSUI_CampItem item in m_CampItem)
		{
			if (item != null)
			{
				Object.Destroy(item.gameObject);
				item.gameObject.transform.parent = null;
			}
		}
		m_CampItem.Clear();
	}

	private void ReflashGet()
	{
		ClearGetItem();
		foreach (GetInfo get in m_GetList)
		{
			AddGetItem(get);
		}
	}

	private void AddGetItem(GetInfo Info)
	{
		GameObject gameObject = Object.Instantiate(m_GetItemPrefab);
		gameObject.transform.parent = mGetGrid.transform;
		gameObject.transform.localScale = Vector3.one;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.SetActive(value: true);
		CSUI_GetItem component = gameObject.GetComponent<CSUI_GetItem>();
		component.SetTexture(Info.mIcon);
		component.SetCurrentNum(Info.mMaxNum);
		component.ProtoId = Info.mProtoId;
		component.Type = ListItemType.mItem;
		m_GetItem.Add(component);
		mGetGrid.repositionNow = true;
	}

	private void ReflashCamp()
	{
		ClearCampItem();
		foreach (CampInfo camp in m_CampList)
		{
			AddCampItem(camp);
		}
	}

	private void AddCampItem(CampInfo Info)
	{
		GameObject gameObject = Object.Instantiate(m_CampItemPrefab);
		gameObject.transform.parent = mCampGrid.transform;
		gameObject.transform.localScale = Vector3.one;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.SetActive(value: true);
		CSUI_CampItem component = gameObject.GetComponent<CSUI_CampItem>();
		component.e_ItemOnClick += CampChose;
		component.SetCampName(Info.Name);
		m_CampItem.Add(component);
		mCampGrid.repositionNow = true;
	}

	private void CampChose(object sender)
	{
		CSUI_CampItem cSUI_CampItem = sender as CSUI_CampItem;
		if (!(cSUI_CampItem != null))
		{
			return;
		}
		foreach (CSUI_CampItem item in m_CampItem)
		{
			item.SetChoeBg(Show: false);
		}
		cSUI_CampItem.SetChoeBg(Show: true);
		if (this.e_CampClick != null)
		{
			this.e_CampClick(cSUI_CampItem);
		}
	}

	private void OnExchangeBtn()
	{
		if (this.e_ExchangeClick != null)
		{
			this.e_ExchangeClick(this);
		}
	}
}
