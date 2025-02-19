using System.Linq;
using ItemAsset;
using Pathea;
using UnityEngine;

public class ItemDropMousePickRandomItem : ItemDropMousePick
{
	public Vector3 genPos;

	public int[] GetAllItem()
	{
		int[] array = new int[_itemList.Count * 2];
		for (int i = 0; i < _itemList.Count; i++)
		{
			array[i * 2] = _itemList[i].protoId;
			array[i * 2 + 1] = _itemList[i].GetCount();
		}
		return array;
	}

	public override void Fetch(int index)
	{
		if (CanFetch(index))
		{
			ItemSample itemSample = _itemList[index];
			if (!PeGameMgr.IsMulti)
			{
				base.playerPkg.Add(itemSample.protoId, itemSample.stackCount);
				_itemList.Remove(itemSample);
				CheckDestroyObj();
				return;
			}
			int protoId = itemSample.protoId;
			int stackCount = itemSample.stackCount;
			PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_RandomItemFetch, genPos, index, protoId, stackCount);
		}
	}

	public override void FetchAll()
	{
		if (!CanFetchAll())
		{
			return;
		}
		if (!PeGameMgr.IsMulti)
		{
			foreach (ItemSample item in _itemList)
			{
				base.playerPkg.Add(item.protoId, item.stackCount);
			}
			_itemList.Clear();
			CheckDestroyObj();
		}
		else
		{
			PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_RandomItemFetchAll, genPos);
		}
	}

	public void Remove(int index, int protoId, int count)
	{
		if (_itemList.Count > index && _itemList[index].protoId == protoId && _itemList[index].stackCount == count)
		{
			_itemList.RemoveAt(index);
			return;
		}
		for (int i = 0; i < _itemList.Count(); i++)
		{
			if (_itemList[i].protoId == protoId && _itemList[i].stackCount == count)
			{
				_itemList.RemoveAt(i);
				_itemList[i].protoId = -1;
				_itemList[i].stackCount = 0;
			}
		}
	}

	public void RemoveAll()
	{
		_itemList.Clear();
	}

	public void CheckDestroyObj()
	{
		if (_itemList.Count <= 0 && base.gameObject != null)
		{
			RandomItemObj randomItemObj = RandomItemMgr.Instance.GetRandomItemObj(genPos);
			if (randomItemObj != null)
			{
				RandomItemMgr.Instance.RemoveRandomItemObj(randomItemObj);
				SceneMan.RemoveSceneObj(randomItemObj);
			}
			Object.Destroy(base.gameObject);
		}
	}
}
