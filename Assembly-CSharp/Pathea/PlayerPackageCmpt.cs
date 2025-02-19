using System.Collections.Generic;
using System.IO;
using ItemAsset;
using PeEvent;
using PETools;
using UnityEngine;

namespace Pathea;

public class PlayerPackageCmpt : PackageCmpt
{
	public class GetItemEventArg : EventArg
	{
		public int protoId;

		public int count;
	}

	public const int PkgCapacity = 420;

	public static bool LockStackCount;

	private PlayerPackage mPackage;

	private ShortCutSlotList mShotCutSlotList;

	private bool mUpdateShortCut;

	private Event<GetItemEventArg> mEventor = new Event<GetItemEventArg>();

	public PlayerPackage package
	{
		get
		{
			if (mPackage == null)
			{
				bool createMisPkg = ((!PeGameMgr.IsMulti || PlayerNetwork.mainPlayerId == base.Entity.Id) ? true : false);
				mPackage = new PlayerPackage(420, createMisPkg);
			}
			return mPackage;
		}
	}

	public ShortCutSlotList shortCutSlotList
	{
		get
		{
			if (mShotCutSlotList == null)
			{
				EquipmentCmpt component = base.Entity.GetComponent<EquipmentCmpt>();
				mShotCutSlotList = new ShortCutSlotList(27, package._playerPak, component);
				package._playerPak.changeEventor.Subscribe(OnItemPakChange);
				if (null != component)
				{
					component.changeEventor.Subscribe(OnItemPakChange);
				}
			}
			return mShotCutSlotList;
		}
	}

	public Event<GetItemEventArg> getItemEventor => mEventor;

	public override void OnUpdate()
	{
		base.OnUpdate();
		package.UpdateNewFlag(Time.deltaTime);
		if (mUpdateShortCut)
		{
			mUpdateShortCut = false;
			shortCutSlotList.UpdateShortCut();
		}
	}

	public override void Deserialize(BinaryReader r)
	{
		base.Deserialize(r);
		byte[] buffer = PETools.Serialize.ReadBytes(r);
		package.Import(buffer);
		if (PeSingleton<ArchiveMgr>.Instance.GetCurArvhiveVersion() > 2)
		{
			buffer = PETools.Serialize.ReadBytes(r);
			PlayerPackage._missionPak.Import(buffer);
		}
		buffer = PETools.Serialize.ReadBytes(r);
		shortCutSlotList.Import(buffer);
	}

	public override void Serialize(BinaryWriter w)
	{
		base.Serialize(w);
		PETools.Serialize.WriteData(package.Export, w);
		PETools.Serialize.WriteData(PlayerPackage._missionPak.Export, w);
		PETools.Serialize.WriteData(shortCutSlotList.Export, w);
	}

	private void OnItemPakChange()
	{
		mUpdateShortCut = true;
	}

	private void OnItemPakChange(object sender, ItemPackage.EventArg arg)
	{
		OnItemPakChange();
	}

	private void OnItemPakChange(object sender, EquipmentCmpt.EventArg arg)
	{
		OnItemPakChange();
	}

	public void NetOnItemUpdate(ItemObject itemObj = null)
	{
		if (itemObj == null)
		{
			OnItemPakChange();
		}
		else
		{
			shortCutSlotList.UpdateShortCut(itemObj);
		}
	}

	public bool PutItemToShortCutList(int pkgIndex, int shortCutListIndex, bool isMission = false)
	{
		ItemObject item = package.GetItem(pkgIndex, isMission);
		if (item == null)
		{
			return false;
		}
		shortCutSlotList.PutItemObj(item, shortCutListIndex);
		return true;
	}

	private void SendGetItemEvent(ItemObject item)
	{
		SendGetItemEvent(item.protoId, item.stackCount);
	}

	private void SendGetItemEvent(int protoId, int count)
	{
		getItemEventor.Dispatch(new GetItemEventArg
		{
			protoId = protoId,
			count = count
		});
	}

	public override bool Add(ItemObject item, bool isNew = false)
	{
		if (package.CanAdd(item))
		{
			package.AddItem(item, isNew);
			SendGetItemEvent(item);
			CheckMainPlayerGetItem(item.protoId);
			return true;
		}
		return false;
	}

	public override bool Remove(ItemObject item)
	{
		return package.RemoveItem(item);
	}

	public override bool Contain(ItemObject item)
	{
		return false;
	}

	public override bool CanAddItemList(List<ItemObject> items)
	{
		return package.CanAddItemList(items);
	}

	public override bool AddItemList(List<ItemObject> items)
	{
		foreach (ItemObject item in items)
		{
			SendGetItemEvent(item);
		}
		return package.AddItemList(items, isNew: true);
	}

	public override bool AdditemFromEquip(List<ItemObject> items)
	{
		return package.AddItemList(items);
	}

	public override int GetItemCount(int protoId)
	{
		return package.GetCount(protoId);
	}

	public override bool ContainsItem(int protoId)
	{
		return package.ContainsItem(protoId);
	}

	public override int GetCountByEditorType(int editorType)
	{
		return package.GetCountByEditorType(editorType);
	}

	public override int GetAllItemsCount()
	{
		return package.GetAllItemsCount();
	}

	public override bool Destory(int protoId, int count)
	{
		if (LockStackCount)
		{
			return true;
		}
		return package.Destroy(protoId, count);
	}

	public override bool DestroyItem(int instanceId, int count)
	{
		if (LockStackCount)
		{
			return true;
		}
		return package.DestroyItem(instanceId, count);
	}

	public override bool DestroyItem(ItemObject item, int count)
	{
		if (LockStackCount)
		{
			return true;
		}
		return package.DestroyItem(item, count);
	}

	public override bool Add(int protoId, int count)
	{
		if (package.CanAdd(protoId, count))
		{
			package.Add(protoId, count, newFlag: true);
			SendGetItemEvent(protoId, count);
			CheckMainPlayerGetItem(protoId);
			return true;
		}
		return false;
	}

	public override bool Set(int protoId, int count)
	{
		return package.Set(protoId, count);
	}

	private void CheckMainPlayerGetItem(int itemID)
	{
		if (base.Entity.IsMainPlayer)
		{
			InGameAidData.CheckGetItem(itemID);
		}
	}
}
