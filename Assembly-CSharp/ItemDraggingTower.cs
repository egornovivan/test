using Pathea;
using PeMap;
using UnityEngine;

public class ItemDraggingTower : ItemDraggingBase
{
	private bool bPut;

	public GameObject headTips;

	public bool needVoxel;

	public override bool OnPutDown()
	{
		if (GameConfig.IsMultiClient)
		{
			if (!PeGameMgr.IsMultiCoop && VArtifactUtil.IsInTownBallArea(base.transform.position))
			{
				new PeTipMsg(PELocalization.GetString(8000864), PeTipMsg.EMsgLevel.Warning);
				return true;
			}
			if (null != PlayerNetwork.mainPlayer)
			{
				PlayerNetwork.mainPlayer.RequestDragTower(itemDragging.itemObj.instanceId, base.transform.position, base.transform.rotation);
			}
		}
		else
		{
			DragTowerAgent dragTowerAgent = new DragTowerAgent(itemDragging, base.transform.position, base.transform.rotation);
			dragTowerAgent.Create();
			SceneMan.AddSceneObj(dragTowerAgent);
			TowerMark towerMark = new TowerMark();
			towerMark.position = base.transform.position;
			towerMark.ID = base.itemObjectId;
			towerMark.text = itemDragging.itemObj.protoData.GetName();
			towerMark.campId = Mathf.RoundToInt(PeSingleton<MainPlayer>.Instance.entity.GetAttribute(AttribType.CampID));
			PeSingleton<LabelMgr>.Instance.Add(towerMark);
			PeSingleton<TowerMark.Mgr>.Instance.Add(towerMark);
			RemoveFromBag();
		}
		return base.OnPutDown();
	}

	public override bool OnDragging(Ray ray)
	{
		bool flag = base.OnDragging(ray);
		base.rootGameObject.transform.position = BuildBlockManager.BestMatchPosition(base.rootGameObject.transform.position);
		CheckTreeAndSkEntity();
		UpdateHeadTips();
		return flag && !mHasTree;
	}

	public override bool OnCheckPutDown()
	{
		return true;
	}

	private void UpdateHeadTips()
	{
		bool flag = CheckOnBuildTerrain();
		if (headTips != null)
		{
			if (needVoxel && !flag)
			{
				headTips.SetActive(value: true);
			}
			else
			{
				headTips.SetActive(value: false);
			}
		}
	}

	private bool CheckOnBuildTerrain()
	{
		float s_Scale = BSBlock45Data.s_Scale;
		for (int i = -1; i <= 0; i++)
		{
			for (int j = -1; j <= 0; j++)
			{
				Vector3 vector = base.transform.TransformPoint((float)i * s_Scale, 0f - s_Scale, (float)j * s_Scale);
				IntVector3 intVector = new IntVector3(Mathf.FloorToInt(vector.x * (float)BSBlock45Data.s_ScaleInverted), Mathf.FloorToInt(vector.y * (float)BSBlock45Data.s_ScaleInverted), Mathf.FloorToInt(vector.z * (float)BSBlock45Data.s_ScaleInverted));
				if (Block45Man.self.DataSource.SafeRead(intVector.x, intVector.y, intVector.z).blockType >> 2 == 0)
				{
					return false;
				}
			}
		}
		return true;
	}
}
