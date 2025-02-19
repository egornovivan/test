using System.Collections.Generic;
using UnityEngine;

public class ItemDraggingPlant : ItemDraggingArticle
{
	public override void OnDragOut()
	{
		if (itemDragging != null)
		{
			PlantInfo plantInfoByItemId = PlantInfo.GetPlantInfoByItemId(itemDragging.itemObj.protoId);
			ItemScript_Plant component = base.gameObject.GetComponent<ItemScript_Plant>();
			component.ResetModel(3, plantInfoByItemId);
		}
		base.OnDragOut();
	}

	private bool checkTerrain(Vector3 pos)
	{
		VFVoxel vFVoxel = VFVoxelTerrain.self.Voxels.SafeRead(Mathf.RoundToInt(pos.x), Mathf.FloorToInt(pos.y), Mathf.RoundToInt(pos.z));
		Collider[] array = Physics.OverlapSphere(base.transform.position, 0.2f, -4097);
		Collider[] array2 = array;
		foreach (Collider collider in array2)
		{
			if (!collider.isTrigger)
			{
				return false;
			}
		}
		if (vFVoxel.Type == 19 || vFVoxel.Type == 63)
		{
			return true;
		}
		return false;
	}

	protected override void FixPosByItemBounds(Ray cameraRay)
	{
		if (null == itemBounds)
		{
			mOverlaped = false;
			return;
		}
		if (!mRayHitTerrain)
		{
			itemBounds.activeState = false;
			return;
		}
		mHitPos.x = Mathf.RoundToInt(mHitPos.x);
		mHitPos.y -= 0.1f;
		mHitPos.z = Mathf.RoundToInt(mHitPos.z);
		SetPos(mHitPos);
		CloseItemBounds();
		if (mFindBounds == null)
		{
			mFindBounds = new List<ItemDraggingBounds>();
		}
		List<ISceneObjAgent> activeSceneObjs = SceneMan.GetActiveSceneObjs(typeof(DragItemAgent), includChild: true);
		bool flag = false;
		Bounds worldBounds = itemBounds.worldBounds;
		worldBounds.size -= 0.01f * Vector3.one;
		for (int i = 0; i < activeSceneObjs.Count; i++)
		{
			DragItemAgent dragItemAgent = activeSceneObjs[i] as DragItemAgent;
			if (!(null != dragItemAgent.gameObject))
			{
				continue;
			}
			ItemDraggingBounds componentInChildren = dragItemAgent.gameObject.GetComponentInChildren<ItemDraggingBounds>();
			if (null != componentInChildren)
			{
				Bounds worldBounds2 = componentInChildren.worldBounds;
				mFindBounds.Add(componentInChildren);
				if (worldBounds2.IntersectRay(cameraRay, out var distance) && distance <= mMinDis + 0.05f && Vector3.Distance(base.playerPos, cameraRay.origin + distance * cameraRay.direction.normalized) < DraggingDistance)
				{
					mTooFar = false;
					mMinDis = distance;
					mAdsorbItemBounds = componentInChildren;
					mHitPos = cameraRay.origin + distance * cameraRay.direction.normalized;
					flag = true;
				}
				if (mRayHitTerrain && worldBounds2.Intersects(worldBounds))
				{
					mAdsorbItemBounds = componentInChildren;
				}
			}
		}
		if (null != mAdsorbItemBounds)
		{
			SetPos(mHitPos);
			mFindBounds.Remove(mAdsorbItemBounds);
			mAdsorbItemBounds.showBounds = true;
			mAdsorbItemBounds.activeState = true;
			Bounds worldBounds2 = mAdsorbItemBounds.worldBounds;
			bool flag2 = false;
			bool flag3 = false;
			bool flag4 = false;
			bool flag5 = false;
			bool flag6 = false;
			bool flag7 = false;
			if (flag)
			{
				base.rootGameObject.transform.position = mHitPos;
				worldBounds = itemBounds.worldBounds;
				if (Mathf.Abs(mHitPos.x - worldBounds2.min.x) < 0.01f)
				{
					flag2 = true;
				}
				else if (Mathf.Abs(mHitPos.x - worldBounds2.max.x) < 0.01f)
				{
					flag3 = true;
				}
				else if (Mathf.Abs(mHitPos.z - worldBounds2.max.z) < 0.01f)
				{
					flag5 = true;
				}
				else if (Mathf.Abs(mHitPos.z - worldBounds2.min.z) < 0.01f)
				{
					flag4 = true;
				}
				else if (Mathf.Abs(mHitPos.y - worldBounds2.min.y) < 0.01f)
				{
					flag7 = true;
				}
				else if (Mathf.Abs(mHitPos.y - worldBounds2.max.y) < 0.01f)
				{
					flag6 = true;
				}
			}
			else
			{
				float num = 100f;
				if (Mathf.Abs(worldBounds.max.x - worldBounds2.min.x) < num)
				{
					flag2 = true;
					num = Mathf.Abs(worldBounds.max.x - worldBounds2.min.x);
				}
				if (Mathf.Abs(worldBounds.min.x - worldBounds2.max.x) < num)
				{
					flag2 = false;
					flag3 = true;
					num = Mathf.Abs(worldBounds.min.x - worldBounds2.max.x);
				}
				if (Mathf.Abs(worldBounds.min.z - worldBounds2.max.z) < num)
				{
					flag2 = false;
					flag3 = false;
					flag5 = true;
					num = Mathf.Abs(worldBounds.min.z - worldBounds2.max.z);
				}
				if (Mathf.Abs(worldBounds.max.z - worldBounds2.min.z) < num)
				{
					flag2 = false;
					flag3 = false;
					flag5 = false;
					flag4 = true;
				}
			}
			if (flag2)
			{
				mHitPos += (worldBounds2.min.x - worldBounds.max.x) * Vector3.right;
				mHitPos.x = Mathf.FloorToInt(mHitPos.x);
			}
			else if (flag3)
			{
				mHitPos += (worldBounds2.max.x - worldBounds.min.x) * Vector3.right;
				mHitPos.x = Mathf.CeilToInt(mHitPos.x);
			}
			else if (flag4)
			{
				mHitPos += (worldBounds2.min.z - worldBounds.max.z) * Vector3.forward;
				mHitPos.z = Mathf.FloorToInt(mHitPos.z);
			}
			else if (flag5)
			{
				mHitPos += (worldBounds2.max.z - worldBounds.min.z) * Vector3.forward;
				mHitPos.z = Mathf.CeilToInt(mHitPos.z);
			}
			else if (flag6)
			{
				mHitPos += (worldBounds2.max.y - worldBounds.min.y) * Vector3.up;
			}
			else if (flag7)
			{
				mHitPos += (worldBounds2.min.y - worldBounds.max.y) * Vector3.up;
			}
			mHitPos.x = Mathf.RoundToInt(mHitPos.x);
			mHitPos.y -= 0.1f;
			mHitPos.z = Mathf.RoundToInt(mHitPos.z);
			if (Physics.Raycast(mHitPos + Vector3.up, Vector3.down, out fhitInfo, 10f, ItemDraggingBase.layerMask))
			{
				mHitPos.y = fhitInfo.point.y;
			}
			SetPos(mHitPos);
			if (flag6 || flag7)
			{
				mOverlaped = true;
				itemBounds.activeState = false;
				return;
			}
			worldBounds = itemBounds.worldBounds;
			worldBounds.size -= 0.01f * Vector3.one;
			for (int j = 0; j < mFindBounds.Count; j++)
			{
				if (null != mFindBounds[j] && worldBounds.Intersects(mFindBounds[j].worldBounds))
				{
					mOverlaped = true;
					itemBounds.activeState = false;
					mOverlapedItemBounds = mFindBounds[j];
					mOverlapedItemBounds.showBounds = true;
					mOverlapedItemBounds.activeState = false;
					mFindBounds.Clear();
					return;
				}
			}
		}
		if (!checkTerrain(mHitPos))
		{
			mOverlaped = true;
			itemBounds.activeState = false;
		}
		else
		{
			mFindBounds.Clear();
			mOverlaped = false;
			itemBounds.activeState = true;
		}
	}
}
