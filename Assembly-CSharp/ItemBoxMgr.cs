using System.Collections.Generic;
using System.IO;
using Pathea;
using UnityEngine;

public class ItemBoxMgr : Pathea.ISerializable
{
	private const string ArchiveKey = "ArchiveKeyItemBox";

	private const int Version = 2;

	private static ItemBoxMgr mInstance;

	private int LastItemID;

	private Dictionary<int, ItemBox> mItemBox;

	private Transform transform;

	public static ItemBoxMgr Instance
	{
		get
		{
			if (mInstance == null)
			{
				mInstance = new ItemBoxMgr();
			}
			return mInstance;
		}
	}

	public List<int> mDropReq => GameUI.Instance.mItemPackageCtrl.mDropItemWnd.DropReqList;

	private ItemBoxMgr()
	{
	}

	void Pathea.ISerializable.Serialize(PeRecordWriter w)
	{
		w.Write(Export());
	}

	private void Init()
	{
		mItemBox = new Dictionary<int, ItemBox>();
		transform = new GameObject("ItemBox").transform;
		PeSingleton<ArchiveMgr>.Instance.Register("ArchiveKeyItemBox", this);
	}

	private ItemBox CreateItemBox()
	{
		GameObject gameObject = Object.Instantiate(Resources.Load("Prefab/Other/ItemBox")) as GameObject;
		return (!(gameObject != null)) ? null : gameObject.GetComponent<ItemBox>();
	}

	public ItemBox AddItemMultiPlay(int id, int opID, Vector3 pos, MapObjNetwork netWork = null)
	{
		ItemBox itemBox = CreateItemBox();
		itemBox.transform.parent = transform;
		itemBox.mID = id;
		itemBox.mPos = pos;
		itemBox.mNetWork = netWork;
		mItemBox[itemBox.mID] = itemBox;
		if (null != PeSingleton<PeCreature>.Instance.mainPlayer)
		{
			SkAliveEntity cmpt = GameUI.Instance.mMainPlayer.GetCmpt<SkAliveEntity>();
			if (cmpt != null && opID == cmpt.GetId())
			{
				itemBox.InsertItem(mDropReq);
				mDropReq.Clear();
			}
		}
		return itemBox;
	}

	public void RemoveItemMultiPlay(int id)
	{
		if (mItemBox.ContainsKey(id))
		{
			Object.Destroy(mItemBox[id].gameObject);
			mItemBox.Remove(id);
		}
	}

	public ItemBox AddItemSinglePlay(Vector3 pos)
	{
		ItemBox itemBox = CreateItemBox();
		itemBox.transform.parent = transform;
		itemBox.mID = ++LastItemID;
		itemBox.mPos = pos;
		mItemBox[itemBox.mID] = itemBox;
		return itemBox;
	}

	public void Remove(ItemBox box)
	{
		if (mItemBox.Remove(box.mID))
		{
			Object.Destroy(box.gameObject);
		}
	}

	private byte[] Export()
	{
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(2);
		binaryWriter.Write(LastItemID);
		binaryWriter.Write(mItemBox.Count);
		foreach (int key in mItemBox.Keys)
		{
			binaryWriter.Write(mItemBox[key].mID);
			binaryWriter.Write(mItemBox[key].mPos.x);
			binaryWriter.Write(mItemBox[key].mPos.y);
			binaryWriter.Write(mItemBox[key].mPos.z);
			binaryWriter.Write(mItemBox[key].ItemList.Count);
			for (int i = 0; i < mItemBox[key].ItemList.Count; i++)
			{
				binaryWriter.Write(mItemBox[key].ItemList[i]);
			}
		}
		binaryWriter.Close();
		memoryStream.Close();
		return memoryStream.ToArray();
	}

	private void Import(byte[] buffer)
	{
		mItemBox.Clear();
		MemoryStream memoryStream = new MemoryStream(buffer);
		BinaryReader binaryReader = new BinaryReader(memoryStream);
		int num = binaryReader.ReadInt32();
		LastItemID = binaryReader.ReadInt32();
		int num2 = binaryReader.ReadInt32();
		switch (num)
		{
		case 1:
		{
			for (int k = 0; k < num2; k++)
			{
				ItemBox itemBox2 = CreateItemBox();
				itemBox2.transform.parent = transform;
				itemBox2.mID = binaryReader.ReadInt32();
				itemBox2.mPos = new Vector3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
				mItemBox.Add(itemBox2.mID, itemBox2);
			}
			break;
		}
		case 2:
		{
			for (int i = 0; i < num2; i++)
			{
				ItemBox itemBox = CreateItemBox();
				itemBox.transform.parent = transform;
				itemBox.mID = binaryReader.ReadInt32();
				itemBox.mPos = new Vector3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), binaryReader.ReadSingle());
				int num3 = binaryReader.ReadInt32();
				for (int j = 0; j < num3; j++)
				{
					itemBox.AddItem(binaryReader.ReadInt32());
				}
				mItemBox.Add(itemBox.mID, itemBox);
			}
			break;
		}
		}
		binaryReader.Close();
		memoryStream.Close();
	}

	public void New()
	{
		Init();
	}

	public void Restore()
	{
		Init();
		byte[] data = PeSingleton<ArchiveMgr>.Instance.GetData("ArchiveKeyItemBox");
		if (data != null)
		{
			Import(data);
		}
	}
}
