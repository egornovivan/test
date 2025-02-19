using System;
using System.Collections.Generic;
using System.IO;
using ItemAsset;
using ItemAsset.PackageHelper;
using Pathea;
using UnityEngine;

public class WareHouseObject : MousePickableChildCollider, ISaveDataInScene
{
	private const int PakCapacity = 30;

	public const float MaxOperateDistance = 10f;

	private PeEntity _entity;

	public int _id;

	private ItemPackage _itemPak;

	public MapObjNetwork _objNet;

	public ItemPackage ItemPak => _itemPak;

	protected override void OnStart()
	{
		base.OnStart();
		WareHouseManager.AddWareHouseObjectList(this);
		if (_itemPak == null && PeGameMgr.IsSingleStory)
		{
			_itemPak = DescToItemPack(WareHouseManager.GetWareHouseData(_id).m_itemsDesc);
		}
		else if (_itemPak == null && PeGameMgr.playerType == PeGameMgr.EPlayerType.Tutorial)
		{
			_itemPak = DescToItemPack(WareHouseManager.GetWareHouseData(_id).m_itemsDesc);
		}
		else if (PeGameMgr.IsMulti)
		{
			GlobalBehaviour.RegisterEvent(RequestCreate);
		}
		if (_itemPak == null && PeGameMgr.IsSingle)
		{
			_itemPak = new ItemPackage(30);
		}
		operateDistance = 10f;
		_entity = base.gameObject.GetComponentInParent<PeEntity>();
		if (_entity != null)
		{
			MapObjNetwork net = MapObjNetwork.GetNet(_entity.Id);
			if (net != null)
			{
				net.wareHouseObj = this;
				_id = net.AssetId;
				InitForNet(net);
			}
		}
	}

	private bool RequestCreate()
	{
		if (PlayerNetwork.mainPlayer == null)
		{
			return false;
		}
		if (MapObjNetwork.HadCreate(_id, 3))
		{
			MapObjNetwork net = MapObjNetwork.GetNet(_id, 3);
			if (net != null)
			{
				net.RequestItemList();
			}
		}
		else
		{
			PlayerNetwork.mainPlayer.CreateSceneBox(_id);
		}
		return true;
	}

	public static bool MatchID(WareHouseObject iter, int id)
	{
		if (iter == null)
		{
			return false;
		}
		return iter._id == id;
	}

	protected override void CheckOperate()
	{
		if (!PeInput.Get(PeInput.LogicFunction.OpenItemMenu) && !PeInput.Get(PeInput.LogicFunction.InteractWithItem))
		{
			return;
		}
		if (MissionManager.Instance != null && MissionManager.Instance.HasMission(8) && _id == 1)
		{
			if (PeGameMgr.IsMulti)
			{
				MissionManager.Instance.RequestCompleteMission(8);
			}
			else
			{
				MissionManager.Instance.CompleteMission(8);
			}
		}
		GameUI.Instance.mWarehouse.ResetItemPacket(_itemPak, base.transform, this);
		GameUI.Instance.mWarehouse.Show();
	}

	public void ImportData(byte[] data)
	{
		if (!PeGameMgr.IsMulti)
		{
			_itemPak = new ItemPackage(30);
			if (data != null)
			{
				_itemPak.Import(data);
			}
		}
	}

	public byte[] ExportData()
	{
		if (PeGameMgr.IsMulti)
		{
			return null;
		}
		if (_itemPak != null)
		{
			using (MemoryStream memoryStream = new MemoryStream(1000))
			{
				using (BinaryWriter w = new BinaryWriter(memoryStream))
				{
					_itemPak.Export(w);
				}
				return memoryStream.ToArray();
			}
		}
		return null;
	}

	public static ItemPackage DescToItemPack(string desc)
	{
		ItemPackage itemPackage = new ItemPackage(30);
		if (desc != "0")
		{
			string[] array = desc.Split(';');
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = array[i].Split(',');
				if (array2.Length == 2)
				{
					int prototypeId = Convert.ToInt32(array2[0]);
					int count = Convert.ToInt32(array2[1]);
					itemPackage.Add(prototypeId, count);
				}
			}
		}
		return itemPackage;
	}

	public void InitForNet(MapObjNetwork net)
	{
		if (_itemPak == null)
		{
			_itemPak = new ItemPackage(30);
		}
		_objNet = net;
	}

	public void ResetItemByIdList(List<int> itemIdList)
	{
		if (itemIdList.Count != 120)
		{
			Debug.LogErrorFormat("WareHouseObject.ResetItemByIdList() Error: itemIdList.Count !={0},itemIdList.Count:{1}", 120, itemIdList.Count);
		}
		ItemPak.Clear();
		if (itemIdList != null)
		{
			for (int i = 0; i < itemIdList.Count; i++)
			{
				ItemObject itemObject = ((itemIdList[i] != -1) ? PeSingleton<ItemMgr>.Instance.Get(itemIdList[i]) : null);
				if (itemObject != null)
				{
					ItemPak.PutItem(itemObject, i % 30, (ItemPackage.ESlotType)(i / 30));
				}
			}
		}
		GameUI.Instance.mWarehouse.ResetItem();
	}

	public void RemoveItemById(int itemId)
	{
		ItemPak.RemoveItemById(itemId);
		GameUI.Instance.mWarehouse.ResetItem();
	}
}
