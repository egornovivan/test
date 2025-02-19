using Pathea;
using UnityEngine;

public class ItemDraggingWareHouse : ItemDraggingBase
{
	public int doodadID = 143;

	public override bool OnPutDown()
	{
		if (!PeGameMgr.IsMulti)
		{
			DoodadEntityCreator.CreateRandTerDoodad(doodadID, base.transform.position, Vector3.one, base.transform.rotation);
			RemoveFromBag();
		}
		else
		{
			IntVector3 intVector = new IntVector3(base.transform.position + 0.1f * Vector3.down);
			if (VArtifactUtil.IsInTownBallArea(intVector))
			{
				new PeTipMsg(PELocalization.GetString(8000864), PeTipMsg.EMsgLevel.Warning);
				return true;
			}
			if (null != PlayerNetwork.mainPlayer)
			{
				byte type = VFVoxelTerrain.self.Voxels.SafeRead(intVector.x, intVector.y, intVector.z).Type;
				PlayerNetwork.mainPlayer.RequestDragOut(itemDragging.itemObj.instanceId, base.transform.position, base.transform.localScale, base.transform.rotation, type);
			}
		}
		return base.OnPutDown();
	}
}
