using System.Collections.Generic;
using System.IO;
using Pathea;
using UnityEngine;

public class UIMapSelectWnd : UIBaseWnd
{
	public enum ItemType
	{
		it_dir,
		it_map
	}

	private class ItemInfo
	{
		public ItemType type;

		public string text;

		public Texture MapTexture;

		public Vector3 size = Vector3.zero;

		public List<string> roles = new List<string>();

		public int dataIndex;
	}

	public UIHostCreateCtrl mHostCreatCtrl;

	[SerializeField]
	private Transform Centent;

	[SerializeField]
	private GameObject UIAMapSelectItemPrefab;

	[HideInInspector]
	public UIMapSelectItem mUIMapSelectItem;

	[SerializeField]
	private UIGrid mGird;

	[SerializeField]
	private UILabel mLbPathvalve;

	[SerializeField]
	private GameObject mBackBtn;

	[SerializeField]
	private GameObject mMassageLb;

	[SerializeField]
	private UILabel mLbName;

	[SerializeField]
	private UILabel mLbExtension;

	[SerializeField]
	private UILabel mLbLastWriteTime;

	[SerializeField]
	private UILabel mLbMapSize;

	[SerializeField]
	private UITexture mMapTexture;

	[SerializeField]
	private UIPopupList mPopPlayer;

	public bool ismulti;

	private List<UIMapSelectItem> mItemList = new List<UIMapSelectItem>();

	private List<CustomGameData> mCustomGameData = new List<CustomGameData>();

	private UIMapSelectItem mSelectMapItem;

	private List<ItemInfo> mInfoList = new List<ItemInfo>();

	private string OnClickName;

	private int Size_x;

	private int Size_z;

	public string LbPathText
	{
		get
		{
			return mLbPathvalve.text;
		}
		set
		{
			mLbPathvalve.text = value;
		}
	}

	private GameObject AddUIPrefab(GameObject prefab, Transform parentTs)
	{
		GameObject gameObject = Object.Instantiate(prefab);
		gameObject.transform.parent = parentTs;
		gameObject.layer = parentTs.gameObject.layer;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localScale = Vector3.one;
		return gameObject;
	}

	private void Awake()
	{
		GameObject gameObject = AddUIPrefab(UIAMapSelectItemPrefab, Centent);
		mUIMapSelectItem = gameObject.GetComponent<UIMapSelectItem>();
		gameObject.SetActive(value: false);
		mBackBtn.SetActive(value: false);
		mLbPathvalve.text = GameConfig.CustomDataDir;
		mMapTexture.gameObject.SetActive(value: false);
	}

	private void Start()
	{
	}

	private void Reflsh()
	{
		Clear();
		foreach (ItemInfo mInfo in mInfoList)
		{
			AddMapItem(mInfo);
		}
		mGird.repositionNow = true;
	}

	private void AddMapItem(ItemInfo info)
	{
		GameObject gameObject = Object.Instantiate(UIAMapSelectItemPrefab);
		gameObject.transform.parent = mGird.transform;
		gameObject.transform.localScale = Vector3.one;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.SetActive(value: true);
		UIMapSelectItem component = gameObject.GetComponent<UIMapSelectItem>();
		component.Text = info.text;
		component.Type = info.type;
		component.mTexture = info.MapTexture;
		component.mSize = info.size;
		component.index = info.dataIndex;
		component.e_ItemOnDbClick += ItemOnDbClick;
		component.e_ItemOnClick += ItemOnClick;
		mItemList.Add(component);
	}

	private void Clear()
	{
		foreach (UIMapSelectItem mItem in mItemList)
		{
			if (mItem != null)
			{
				Object.Destroy(mItem.gameObject);
				mItem.gameObject.transform.parent = null;
			}
		}
		mItemList.Clear();
	}

	private void GetDirectory(string path)
	{
		mInfoList.Clear();
		string[] directories = Directory.GetDirectories(path);
		for (int i = 0; i < directories.Length; i++)
		{
			FileInfo fileInfo = new FileInfo(directories[i]);
			ItemInfo itemInfo = new ItemInfo();
			itemInfo.text = fileInfo.DirectoryName;
			itemInfo.type = ItemType.it_dir;
			itemInfo.text = fileInfo.Name;
			mInfoList.Add(itemInfo);
		}
		string[] files = Directory.GetFiles(path);
		for (int j = 0; j < files.Length; j++)
		{
			FileInfo fileInfo2 = new FileInfo(files[j]);
			ItemInfo itemInfo2 = new ItemInfo();
			itemInfo2.text = fileInfo2.DirectoryName;
			itemInfo2.type = ItemType.it_map;
			itemInfo2.text = fileInfo2.Name;
			mInfoList.Add(itemInfo2);
		}
	}

	private void EnableBack(string Path)
	{
		mBackBtn.SetActive(value: false);
	}

	private void ItemOnDbClick(object sender)
	{
		UIMapSelectItem uIMapSelectItem = sender as UIMapSelectItem;
		if (uIMapSelectItem != null && uIMapSelectItem.Type == ItemType.it_dir)
		{
			mLbPathvalve.text += "\\";
			mLbPathvalve.text += uIMapSelectItem.Text.ToString();
			GetDirectory(mLbPathvalve.text);
			EnableBack(mLbPathvalve.text);
			Reflsh();
			Debug.Log("ItemOnClick: -------------" + uIMapSelectItem.Text.ToString());
		}
	}

	private void ItemOnClick(object sender)
	{
		UIMapSelectItem uIMapSelectItem = sender as UIMapSelectItem;
		if (!(uIMapSelectItem != null))
		{
			return;
		}
		mSelectMapItem = uIMapSelectItem;
		if (uIMapSelectItem.Type == ItemType.it_map)
		{
			mMapTexture.gameObject.SetActive(value: true);
			mLbName.text = uIMapSelectItem.Text.ToString();
			OnClickName = mLbName.text;
			Size_x = (int)uIMapSelectItem.mSize.x;
			Size_z = (int)uIMapSelectItem.mSize.z;
			mLbMapSize.text = Size_x + "X" + Size_z;
			mMapTexture.mainTexture = uIMapSelectItem.mTexture;
			PlayerDesc[] humanDescs = mCustomGameData[uIMapSelectItem.index].humanDescs;
			if (humanDescs.Length != 0)
			{
				mPopPlayer.items.Clear();
				PlayerDesc[] array = humanDescs;
				foreach (PlayerDesc playerDesc in array)
				{
					mPopPlayer.items.Add(playerDesc.Name);
				}
				mPopPlayer.selection = mPopPlayer.items[0];
				mCustomGameData[uIMapSelectItem.index].DeterminePlayer(0);
				mMassageLb.SetActive(value: true);
			}
		}
		if (ismulti && mHostCreatCtrl != null)
		{
			mHostCreatCtrl.mMapName.text = uIMapSelectItem.Text.ToString();
		}
	}

	private void btnBackOnClick()
	{
		DirectoryInfo directoryInfo = new DirectoryInfo(mLbPathvalve.text);
		DirectoryInfo parent = directoryInfo.Parent;
		mLbPathvalve.text = parent.FullName;
		EnableBack(mLbPathvalve.text);
		GetDirectory(mLbPathvalve.text);
		mMassageLb.SetActive(value: false);
		Reflsh();
	}

	protected override void OnClose()
	{
		Clear();
		mInfoList.Clear();
		base.OnClose();
	}

	private void OnCancelBtn()
	{
		Object.Destroy(base.gameObject);
		PeSingleton<PeFlowMgr>.Instance.LoadScene(PeFlowMgr.EPeScene.RoleScene);
	}

	private void OnStartBtn()
	{
		if (OnClickName != null)
		{
			PeGameMgr.playerType = PeGameMgr.EPlayerType.Single;
			PeGameMgr.loadArchive = ArchiveMgr.ESave.MaxUser;
			PeGameMgr.sceneMode = PeGameMgr.ESceneMode.Custom;
			PeGameMgr.gameName = OnClickName;
			PeSingleton<PeFlowMgr>.Instance.LoadScene(PeFlowMgr.EPeScene.GameScene);
		}
	}

	private void OnSelectionChange(string select_item)
	{
		if (!(mSelectMapItem == null))
		{
			int index = mPopPlayer.items.FindIndex((string item0) => item0 == select_item);
			mCustomGameData[mSelectMapItem.index].DeterminePlayer(index);
		}
	}
}
