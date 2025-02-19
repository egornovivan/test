using ItemAsset;
using UnityEngine;

public class NpcStorage
{
	private const int NpcStorageIDBegin = 100000;

	private static CSUI_StorageMain storageMain;

	private ItemPackage mPackage;

	private int mId;

	public ItemPackage Package
	{
		get
		{
			if (mPackage == null)
			{
				CSEntityAttr attr = default(CSEntityAttr);
				attr.m_InstanceId = mId;
				attr.m_Type = 2;
				CSEntity outEnti = null;
				if (CSMain.GetCreator(10000).CreateEntity(attr, out outEnti) == 4)
				{
					if (outEnti is CSStorage cSStorage)
					{
						mPackage = cSStorage.m_Package;
					}
					else
					{
						Debug.LogError("Get CSStorage failed by " + mId);
					}
				}
			}
			return mPackage;
		}
		set
		{
			mPackage = value;
		}
	}

	private static CSUI_StorageMain StorageMain
	{
		get
		{
			if (null == storageMain)
			{
				GameObject gameObject = Object.Instantiate(Resources.Load<GameObject>("Prefabs/CS_StorageMain"));
				GameObject gameObject2 = GameObject.Find("GameUIRoot/UICamera/Game");
				if (null == gameObject2)
				{
					Debug.LogError("cant find GameUIRoot/UICamera/Game");
					return null;
				}
				gameObject.transform.parent = UINpcStorageCtrl.Instance.mContent.transform;
				gameObject.transform.localPosition = new Vector3(-125f, 82f, 0f);
				gameObject.transform.localScale = Vector3.one;
				storageMain = gameObject.GetComponent<CSUI_StorageMain>();
			}
			return storageMain;
		}
	}

	public void Init(int id)
	{
		mId = 100000 + id;
		UINpcStorageCtrl.Instance.btnClose += btnClose;
		UINpcStorageCtrl.Instance.btnPageItem += btnPageItem;
		UINpcStorageCtrl.Instance.btnPageEquipment += btnPageEquipment;
		UINpcStorageCtrl.Instance.btnPageResource += btnPageResource;
	}

	public void Reset()
	{
		if (!(null == StorageMain))
		{
			StorageMain.RestItems();
		}
	}

	public void Open()
	{
	}

	private void btnClose()
	{
	}

	private void btnPageItem()
	{
		storageMain.SetType(0);
		GameUI.Instance.mItemPackageCtrl.ResetItem(0, 0);
	}

	private void btnPageEquipment()
	{
		storageMain.SetType(1);
		GameUI.Instance.mItemPackageCtrl.ResetItem(1, 0);
	}

	private void btnPageResource()
	{
		storageMain.SetType(2);
		GameUI.Instance.mItemPackageCtrl.ResetItem(2, 0);
	}
}
