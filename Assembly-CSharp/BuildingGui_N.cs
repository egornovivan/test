using System;
using System.Collections.Generic;
using System.IO;
using ItemAsset;
using Pathea;
using UnityEngine;

public class BuildingGui_N : MonoBehaviour
{
	public enum OpType
	{
		BuildBlock,
		ItemSetting,
		NpcSetting,
		File
	}

	private class CreateReq
	{
		public bool mIsLoad;

		public OpType mType;

		public int mItemId;

		public AssetReq mReq;
	}

	private const int mVersion = 2;

	private static BuildingGui_N mInstance;

	private OpType mOpType;

	public UICheckbox mItemTab;

	public UICheckbox mNpcTab;

	public GameObject mItemOpWnd;

	public GameObject mNpcOpWnd;

	public GameObject mFileWnd;

	public GameObject mTranWnd;

	public UIInput mPosX;

	public UIInput mPosY;

	public UIInput mPosZ;

	public UIInput mRotX;

	public UIInput mRotY;

	public UIInput mRotZ;

	public SelItemGrid_N mItemPerfab;

	public UIGrid mItemGrid;

	private SelItemGrid_N mCurrentSelItem;

	private List<BuildOpItem> mItemList = new List<BuildOpItem>();

	private BuildOpItem mPutOutItem;

	private AssetReq mCurrentReq;

	private List<CreateReq> mCreateReqList = new List<CreateReq>();

	public GameObject mMalePerfab;

	public UIGrid mNpcGrid;

	private List<BuildOpItem> mNpcList = new List<BuildOpItem>();

	private BuildOpItem mSelectedItem;

	public UIInput mFileName;

	public UIGrid mFileGrid;

	public FileNameSelItem_N mFileNamePerfab;

	private List<FileNameSelItem_N> mFileList = new List<FileNameSelItem_N>();

	public static BuildingGui_N Instance => mInstance;

	private void Awake()
	{
		mInstance = this;
	}

	private void Start()
	{
		PeSingleton<ItemProto.Mgr>.Instance.Foreach(delegate(ItemProto item)
		{
			if (item.IsBlock())
			{
				SelItemGrid_N selItemGrid_N = UnityEngine.Object.Instantiate(mItemPerfab);
				selItemGrid_N.transform.parent = mItemGrid.transform;
				selItemGrid_N.transform.localScale = Vector3.one;
				selItemGrid_N.SetItem(item);
			}
		});
		mItemGrid.Reposition();
		SelItemGrid_N selItemGrid_N2 = UnityEngine.Object.Instantiate(mItemPerfab);
		selItemGrid_N2.transform.parent = mNpcGrid.transform;
		selItemGrid_N2.transform.localScale = Vector3.one;
		selItemGrid_N2.SetNpc("Model/PlayerModel/Male", "head_m02");
		mNpcGrid.Reposition();
		mPosX.text = "0";
		mPosY.text = "0";
		mPosZ.text = "0";
		mRotX.text = "0";
		mRotY.text = "0";
		mRotZ.text = "0";
	}

	private void Update()
	{
		OpType opType = mOpType;
		if (opType != OpType.ItemSetting && opType != OpType.NpcSetting)
		{
			return;
		}
		if (null != mSelectedItem && !UICamera.inputHasFocus)
		{
			if (mPosX.text == string.Empty)
			{
				mPosX.text = "0";
			}
			if (mPosY.text == string.Empty)
			{
				mPosY.text = "0";
			}
			if (mPosZ.text == string.Empty)
			{
				mPosZ.text = "0";
			}
			if (mRotX.text == string.Empty)
			{
				mRotX.text = "0";
			}
			if (mRotY.text == string.Empty)
			{
				mRotY.text = "0";
			}
			if (mRotZ.text == string.Empty)
			{
				mRotZ.text = "0";
			}
			mSelectedItem.transform.position = new Vector3(Convert.ToSingle(mPosX.text), Convert.ToSingle(mPosY.text), Convert.ToSingle(mPosZ.text));
			mSelectedItem.transform.rotation = Quaternion.Euler(new Vector3(Convert.ToSingle(mRotX.text), Convert.ToSingle(mRotY.text), Convert.ToSingle(mRotZ.text)));
		}
		if (null != mPutOutItem)
		{
			if (Input.GetMouseButtonUp(0))
			{
				PutItemDown();
			}
			else
			{
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				if (Physics.Raycast(ray, out var hitInfo, 500f))
				{
					mPutOutItem.transform.position = hitInfo.point;
				}
			}
		}
		if (null != mSelectedItem)
		{
			if (Input.GetKeyDown(KeyCode.Delete))
			{
				mNpcList.Remove(mSelectedItem);
				mItemList.Remove(mSelectedItem);
				UnityEngine.Object.Destroy(mSelectedItem.gameObject);
				mSelectedItem = null;
			}
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				mSelectedItem.SetActive(active: false);
				mSelectedItem = null;
			}
		}
	}

	private void Save(string name)
	{
		if (name == string.Empty)
		{
			Debug.LogError("Inputname is null");
			return;
		}
		string text = GameConfig.GetUserDataPath() + "/PlanetExplorers/Building/";
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		text = text + name + ".txt";
		using (FileStream fileStream = new FileStream(text, FileMode.Create, FileAccess.Write))
		{
			BinaryWriter binaryWriter = new BinaryWriter(fileStream);
			if (binaryWriter == null)
			{
				Debug.LogError("On WriteRecord FileStream is null!");
				return;
			}
			if (Block45Man.self != null)
			{
				byte[] array = Block45Man.self.DataSource.ExportData();
				if (array != null)
				{
					binaryWriter.Write(array);
				}
				else
				{
					binaryWriter.Write(0);
				}
			}
			else
			{
				binaryWriter.Write(0);
			}
			binaryWriter.Close();
			fileStream.Close();
		}
		text = GameConfig.GetUserDataPath() + "/PlanetExplorers/Building/" + name + "SubInfo.txt";
		using (FileStream fileStream2 = new FileStream(text, FileMode.Create, FileAccess.Write))
		{
			BinaryWriter binaryWriter2 = new BinaryWriter(fileStream2);
			if (binaryWriter2 == null)
			{
				Debug.LogError("On WriteRecord FileStream is null!");
				return;
			}
			binaryWriter2.Write(2);
			binaryWriter2.Write(mNpcList.Count);
			for (int i = 0; i < mNpcList.Count; i++)
			{
				binaryWriter2.Write(mNpcList[i].transform.position.x);
				binaryWriter2.Write(mNpcList[i].transform.position.y);
				binaryWriter2.Write(mNpcList[i].transform.position.z);
			}
			binaryWriter2.Write(mItemList.Count);
			for (int j = 0; j < mItemList.Count; j++)
			{
				binaryWriter2.Write(mItemList[j].transform.position.x);
				binaryWriter2.Write(mItemList[j].transform.position.y);
				binaryWriter2.Write(mItemList[j].transform.position.z);
				binaryWriter2.Write(mItemList[j].transform.rotation.eulerAngles.x);
				binaryWriter2.Write(mItemList[j].transform.rotation.eulerAngles.y);
				binaryWriter2.Write(mItemList[j].transform.rotation.eulerAngles.z);
				binaryWriter2.Write(mItemList[j].mItemID);
			}
			binaryWriter2.Close();
			fileStream2.Close();
		}
		ResetFileList();
	}

	private void OnSaveBtn()
	{
		Save(mFileName.text);
	}

	private void OnLoadBtn()
	{
		if (mFileName.text == string.Empty)
		{
			return;
		}
		string text = GameConfig.GetUserDataPath() + "/PlanetExplorers/Building/";
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		for (int i = 0; i < mItemList.Count; i++)
		{
			UnityEngine.Object.Destroy(mItemList[i].gameObject);
		}
		mItemList.Clear();
		for (int j = 0; j < mNpcList.Count; j++)
		{
			UnityEngine.Object.Destroy(mNpcList[j].gameObject);
		}
		mNpcList.Clear();
		if (Block45Man.self.DataSource != null)
		{
			Block45Man.self.DataSource.Clear();
		}
		if (File.Exists(text + mFileName.text + ".txt"))
		{
			using FileStream input = new FileStream(text + mFileName.text + ".txt", FileMode.Open, FileAccess.Read);
			BinaryReader binaryReader = new BinaryReader(input);
			int num = binaryReader.ReadInt32();
			int num2 = num;
			if (num2 == 2)
			{
				int num3 = binaryReader.ReadInt32();
				for (int k = 0; k < num3; k++)
				{
					IntVector3 intVector = new IntVector3(binaryReader.ReadInt32(), binaryReader.ReadInt32(), binaryReader.ReadInt32());
					Block45Man.self.DataSource.SafeWrite(new B45Block(binaryReader.ReadByte(), binaryReader.ReadByte()), intVector.x, intVector.y, intVector.z);
				}
			}
			binaryReader.Close();
		}
		if (!File.Exists(text + mFileName.text + "SubInfo.txt"))
		{
			return;
		}
		using FileStream input2 = new FileStream(text + mFileName.text + "SubInfo.txt", FileMode.Open, FileAccess.Read);
		BinaryReader binaryReader2 = new BinaryReader(input2);
		int num4 = binaryReader2.ReadInt32();
		int num5 = binaryReader2.ReadInt32();
		switch (num4)
		{
		case 1:
		{
			for (int n = 0; n < num5; n++)
			{
				Vector3 position3 = new Vector3(binaryReader2.ReadSingle(), binaryReader2.ReadSingle(), binaryReader2.ReadSingle());
				CreateReq createReq3 = new CreateReq();
				createReq3.mIsLoad = true;
				createReq3.mType = OpType.NpcSetting;
				GameObject gameObject3 = UnityEngine.Object.Instantiate(Resources.Load("Model/PlayerModel/Male")) as GameObject;
				gameObject3.transform.position = position3;
				CreateMode(gameObject3, createReq3);
			}
			break;
		}
		case 2:
		{
			for (int l = 0; l < num5; l++)
			{
				Vector3 position = new Vector3(binaryReader2.ReadSingle(), binaryReader2.ReadSingle(), binaryReader2.ReadSingle());
				CreateReq createReq = new CreateReq();
				createReq.mIsLoad = true;
				createReq.mType = OpType.NpcSetting;
				GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load("Model/PlayerModel/Male")) as GameObject;
				gameObject.transform.position = position;
				CreateMode(gameObject, createReq);
			}
			num5 = binaryReader2.ReadInt32();
			for (int m = 0; m < num5; m++)
			{
				Vector3 position2 = new Vector3(binaryReader2.ReadSingle(), binaryReader2.ReadSingle(), binaryReader2.ReadSingle());
				Quaternion localRotation = Quaternion.Euler(new Vector3(binaryReader2.ReadSingle(), binaryReader2.ReadSingle(), binaryReader2.ReadSingle()));
				int num6 = binaryReader2.ReadInt32();
				ItemProto itemData = ItemProto.GetItemData(num6);
				if (itemData != null && itemData.IsBlock())
				{
					CreateReq createReq2 = new CreateReq();
					createReq2.mIsLoad = true;
					createReq2.mType = OpType.ItemSetting;
					createReq2.mItemId = num6;
					GameObject gameObject2 = UnityEngine.Object.Instantiate(Resources.Load(ItemProto.GetItemData(num6).resourcePath)) as GameObject;
					gameObject2.transform.position = position2;
					gameObject2.transform.localRotation = localRotation;
					CreateMode(gameObject2, createReq2);
				}
			}
			break;
		}
		}
		binaryReader2.Close();
	}

	private void OnBlockBtn(bool selected)
	{
		if (selected)
		{
			mTranWnd.SetActive(value: false);
			mOpType = OpType.BuildBlock;
		}
	}

	private void OnItemSetting(bool selected)
	{
		mItemOpWnd.SetActive(selected);
		if (selected)
		{
			mOpType = OpType.ItemSetting;
			mItemGrid.Reposition();
			mTranWnd.SetActive(value: true);
		}
	}

	private void OnNpcSetting(bool selected)
	{
		mNpcOpWnd.SetActive(selected);
		if (selected)
		{
			mOpType = OpType.NpcSetting;
			mTranWnd.SetActive(value: true);
		}
	}

	private void OnFileBtn(bool selected)
	{
		mFileWnd.SetActive(selected);
		if (selected)
		{
			mOpType = OpType.File;
			mTranWnd.SetActive(value: false);
			ResetFileList();
		}
	}

	private void ResetFileList()
	{
		foreach (FileNameSelItem_N mFile in mFileList)
		{
			mFile.transform.parent = null;
			UnityEngine.Object.Destroy(mFile.gameObject);
		}
		mFileList.Clear();
		string path = GameConfig.GetUserDataPath() + "/PlanetExplorers/Building/";
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
		string[] files = Directory.GetFiles(path);
		string[] array = files;
		foreach (string text in array)
		{
			if (!text.Contains("SubInfo"))
			{
				FileNameSelItem_N fileNameSelItem_N = UnityEngine.Object.Instantiate(mFileNamePerfab);
				fileNameSelItem_N.transform.parent = mFileGrid.transform;
				fileNameSelItem_N.transform.localPosition = Vector3.zero;
				fileNameSelItem_N.transform.localScale = Vector3.one;
				fileNameSelItem_N.SetText(Path.GetFileNameWithoutExtension(text), base.gameObject);
				mFileList.Add(fileNameSelItem_N);
			}
		}
		mFileGrid.Reposition();
	}

	public void OnItemPutOut(SelItemGrid_N selItem)
	{
		if (mCurrentSelItem != selItem)
		{
			mCurrentSelItem = selItem;
			CreateReq createReq = new CreateReq();
			createReq.mType = OpType.ItemSetting;
			createReq.mItemId = mCurrentSelItem.mItemData.id;
			CreateMode(UnityEngine.Object.Instantiate(Resources.Load(mCurrentSelItem.mItemData.resourcePath)) as GameObject, createReq);
		}
	}

	public void OnNpcPutOut(SelItemGrid_N selItem)
	{
		if (mCurrentSelItem != selItem)
		{
			mCurrentSelItem = selItem;
			CreateReq createReq = new CreateReq();
			createReq.mType = OpType.NpcSetting;
			CreateMode(UnityEngine.Object.Instantiate(Resources.Load(mCurrentSelItem.mNpcPath)) as GameObject, createReq);
		}
	}

	public void OnBuildOpItemSel(BuildOpItem item)
	{
		switch (item.mType)
		{
		case OpType.ItemSetting:
			mItemTab.isChecked = true;
			break;
		case OpType.NpcSetting:
			mNpcTab.isChecked = true;
			break;
		}
		if (item != mSelectedItem)
		{
			if ((bool)mSelectedItem)
			{
				mSelectedItem.SetActive(active: false);
			}
			mSelectedItem = item;
			mSelectedItem.SetActive(active: true);
			mPosX.text = mSelectedItem.transform.position.x.ToString();
			if (mPosX.text.Length > 6)
			{
				mPosX.text = mPosX.text.Substring(0, 6);
			}
			mPosY.text = mSelectedItem.transform.position.y.ToString();
			if (mPosY.text.Length > 6)
			{
				mPosY.text = mPosY.text.Substring(0, 6);
			}
			mPosZ.text = mSelectedItem.transform.position.z.ToString();
			if (mPosZ.text.Length > 6)
			{
				mPosZ.text = mPosZ.text.Substring(0, 6);
			}
			mRotX.text = mSelectedItem.transform.eulerAngles.x.ToString();
			if (mRotX.text.Length > 6)
			{
				mRotX.text = mRotX.text.Substring(0, 6);
			}
			mRotY.text = mSelectedItem.transform.eulerAngles.y.ToString();
			if (mRotX.text.Length > 6)
			{
				mRotX.text = mRotX.text.Substring(0, 6);
			}
			mRotZ.text = mSelectedItem.transform.eulerAngles.z.ToString();
			if (mRotX.text.Length > 6)
			{
				mRotX.text = mRotX.text.Substring(0, 6);
			}
		}
		else if ((bool)mSelectedItem)
		{
			mSelectedItem.SetActive(active: false);
			mSelectedItem = null;
		}
	}

	private void OnFileSelected(string fileName)
	{
		mFileName.text = fileName;
	}

	private void CancelOp()
	{
		if ((bool)mSelectedItem)
		{
			mSelectedItem.SetActive(active: false);
		}
		mSelectedItem = null;
		mPutOutItem = null;
		mCurrentReq = null;
		mCurrentSelItem = null;
	}

	private void PutItemDown()
	{
		if ((bool)mPutOutItem)
		{
			mPutOutItem.GetComponent<Collider>().enabled = true;
		}
		mPutOutItem = null;
		mCurrentSelItem = null;
	}

	private void CreateMode(GameObject go, CreateReq req)
	{
		BoxCollider boxCollider = go.AddComponent<BoxCollider>();
		Bounds bounds = boxCollider.bounds;
		Collider[] componentsInChildren = go.GetComponentsInChildren<Collider>();
		Collider[] array = componentsInChildren;
		foreach (Collider collider in array)
		{
			bounds.Encapsulate(collider.bounds);
			if (!req.mIsLoad)
			{
				collider.enabled = false;
			}
		}
		boxCollider.center = bounds.center - go.transform.position;
		boxCollider.size = bounds.size;
		BuildOpItem buildOpItem = go.AddComponent<BuildOpItem>();
		switch (req.mType)
		{
		case OpType.ItemSetting:
			buildOpItem.mType = OpType.ItemSetting;
			buildOpItem.mItemID = req.mItemId;
			mItemList.Add(buildOpItem);
			break;
		case OpType.NpcSetting:
			buildOpItem.mType = OpType.NpcSetting;
			mNpcList.Add(buildOpItem);
			boxCollider.center += 1f * Vector3.up;
			boxCollider.size += 0.5f * Vector3.up;
			break;
		}
		if (!req.mIsLoad)
		{
			mPutOutItem = buildOpItem;
		}
	}

	public void OnSpawned(GameObject go, AssetReq req)
	{
		foreach (CreateReq mCreateReq in mCreateReqList)
		{
			if (mCreateReq.mReq == req)
			{
				CreateMode(go, mCreateReq);
				return;
			}
		}
		UnityEngine.Object.Destroy(go);
	}
}
