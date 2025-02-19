using System.Collections.Generic;
using ItemAsset;
using UnityEngine;

public class SceneDropItem : SceneObject
{
	protected ItemObject _item;

	public ItemObject Item => _item;

	public SceneDropItem()
	{
		_type = ESceneObjType.DROPITEM;
	}

	public void SetItem(ItemObject item)
	{
		_item = item;
	}

	private static SceneObject CreateDropItem(int worldId, int objId, Vector3 pos, Vector3 scale, Quaternion rot)
	{
		ItemObject itemByID = ItemManager.GetItemByID(objId);
		if (itemByID == null)
		{
			if (LogFilter.logDebug)
			{
				Debug.LogError("Invalid item");
			}
			return null;
		}
		SceneDropItem sceneDropItem = SceneObjMgr.Create<SceneDropItem>();
		sceneDropItem.Init(itemByID.instanceId, itemByID.protoId, pos, scale, rot, worldId);
		sceneDropItem.SetItem(itemByID);
		GameWorld.AddSceneObj(sceneDropItem, worldId);
		SceneObjMgr.AddItem(itemByID.instanceId);
		SceneObjMgr.Save(sceneDropItem);
		return sceneDropItem;
	}

	public static void CreateDropItems(int worldId, List<ItemObject> objs, Vector3 pos, Vector3 scale, Quaternion rot)
	{
		List<SceneObject> list = new List<SceneObject>();
		foreach (ItemObject obj in objs)
		{
			list.Add(CreateDropItem(worldId, obj.instanceId, pos, scale, rot));
		}
		Player randomPlayer = Player.GetRandomPlayer();
		if (randomPlayer != null)
		{
			randomPlayer.SyncSceneObjects(list);
		}
	}
}
