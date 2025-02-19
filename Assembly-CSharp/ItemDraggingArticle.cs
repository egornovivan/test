using ItemAsset;
using Pathea;
using UnityEngine;

public class ItemDraggingArticle : ItemDraggingBase
{
	public override bool OnPutDown()
	{
		if (!CheckMonsterBeaconEnable())
		{
			return true;
		}
		if (NetworkInterface.IsClient)
		{
			if (null != PlayerNetwork.mainPlayer)
			{
				IntVector3 intVector = new IntVector3(base.transform.position + 0.1f * Vector3.down);
				byte type = VFVoxelTerrain.self.Voxels.SafeRead(intVector.x, intVector.y, intVector.z).Type;
				PlayerNetwork.mainPlayer.RequestDragOut(itemDragging.itemObj.instanceId, base.transform.position, base.transform.localScale, base.transform.rotation, type);
			}
		}
		else
		{
			PutDown();
		}
		return base.OnPutDown();
	}

	protected void PutDown(bool isCreation = false)
	{
		Drag cmpt = itemDragging;
		if (itemDragging.itemObj.stackCount > 1)
		{
			ItemObject itemObject = PeSingleton<ItemMgr>.Instance.CreateItem(itemDragging.itemObj.protoId);
			cmpt = itemObject.GetCmpt<Drag>();
		}
		DragArticleAgent.Create(cmpt, base.transform.position, base.transform.localScale, base.transform.rotation, 0, null, isCreation);
		RemoveFromBag();
	}

	private bool CheckMonsterBeaconEnable()
	{
		ItemScript_MonsterBeacon component = GetComponent<ItemScript_MonsterBeacon>();
		if (null != component && EntityMonsterBeacon.IsRunning())
		{
			PeTipMsg.Register(PELocalization.GetString(82201076), PeTipMsg.EMsgLevel.Warning);
			return false;
		}
		return true;
	}
}
