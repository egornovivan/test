using System.Collections.Generic;
using System.IO;
using ItemAsset;
using Pathea;
using Pathea.Maths;
using PETools;
using UnityEngine;

public class ItemDraggingBase : MonoBehaviour
{
	public float DraggingDistance = 10f;

	public Drag itemDragging;

	public ItemDraggingBounds itemBounds;

	public float subTerrainClearRadius = 1f;

	protected ItemDraggingBounds mAdsorbItemBounds;

	protected ItemDraggingBounds mOverlapedItemBounds;

	protected List<ItemDraggingBounds> mFindBounds;

	protected static readonly int layerMask = 595968;

	protected static readonly int treeAndEntityLayer = 2098432;

	protected static readonly float treeCheckHeight = 1f;

	private PeTrans mTrans;

	protected bool mOverlaped;

	protected bool mTooFar;

	protected bool mRayHitTerrain;

	protected bool mHasTree;

	protected RaycastHit fhitInfo;

	protected Vector3 mHitPos;

	protected float mMinDis;

	protected virtual bool canPutUp => false;

	public int itemObjectId
	{
		get
		{
			if (itemDragging == null)
			{
				return -1;
			}
			return itemDragging.itemObj.instanceId;
		}
	}

	public GameObject rootGameObject => base.gameObject;

	protected Vector3 playerPos
	{
		get
		{
			if (null == mTrans && null != PeSingleton<PeCreature>.Instance.mainPlayer)
			{
				mTrans = PeSingleton<PeCreature>.Instance.mainPlayer.peTrans;
			}
			if (null == mTrans)
			{
				return Vector3.zero;
			}
			return mTrans.position;
		}
	}

	protected void SetPos(Vector3 pos)
	{
		rootGameObject.transform.position = pos;
	}

	private void OnDestroy()
	{
		CloseItemBounds();
	}

	public virtual void OnDragOut()
	{
		base.transform.rotation = Quaternion.identity;
		Rigidbody componentInChildren = rootGameObject.GetComponentInChildren<Rigidbody>();
		if (componentInChildren != null)
		{
			componentInChildren.detectCollisions = false;
		}
		Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
		Collider[] array = componentsInChildren;
		foreach (Collider collider in array)
		{
			collider.enabled = false;
			if (collider is MeshCollider)
			{
				collider.isTrigger = true;
			}
		}
		if (null == itemBounds)
		{
			itemBounds = GetComponent<ItemDraggingBounds>();
		}
		if (null != itemBounds)
		{
			itemBounds.showBounds = true;
		}
	}

	public virtual bool OnDragging(Ray cameraRay)
	{
		UpdatePosByRay(cameraRay);
		FixPosByItemBounds(cameraRay);
		CheckTreeAndSkEntity();
		if (mRayHitTerrain && !rootGameObject.activeSelf)
		{
			rootGameObject.SetActive(value: true);
		}
		if (null != itemBounds)
		{
			itemBounds.activeState = itemBounds.activeState && !mTooFar;
		}
		return mRayHitTerrain && !mTooFar && !mOverlaped && !mHasTree;
	}

	protected virtual void UpdatePosByRay(Ray cameraRay)
	{
		mRayHitTerrain = Phy.Raycast(cameraRay, out fhitInfo, 100f, layerMask, base.transform);
		if (mRayHitTerrain)
		{
			mHitPos = fhitInfo.point;
			mMinDis = fhitInfo.distance;
			float num = Vector3.Distance(playerPos, mHitPos);
			mTooFar = DraggingDistance < num;
			SetPos(mHitPos);
		}
	}

	protected virtual void FixPosByItemBounds(Ray cameraRay)
	{
		if (null == itemBounds)
		{
			mOverlaped = false;
			return;
		}
		CloseItemBounds();
		if (mFindBounds == null)
		{
			mFindBounds = new List<ItemDraggingBounds>();
		}
		List<ISceneObjAgent> activeSceneObjs = SceneMan.GetActiveSceneObjs(typeof(DragItemAgent), includChild: true);
		bool flag = false;
		Bounds worldBounds = itemBounds.worldBounds;
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
				if (worldBounds2.IntersectRay(cameraRay, out var distance) && distance <= mMinDis + 0.05f && Vector3.Distance(playerPos, cameraRay.origin + distance * cameraRay.direction.normalized) < DraggingDistance)
				{
					mTooFar = false;
					mMinDis = distance;
					mAdsorbItemBounds = componentInChildren;
					mHitPos = cameraRay.origin + distance * cameraRay.direction.normalized;
					flag = true;
				}
				if (!flag && mRayHitTerrain && worldBounds2.Intersects(worldBounds))
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
				rootGameObject.transform.position = mHitPos;
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
			Vector3 vector = mHitPos;
			if (flag2)
			{
				vector = mHitPos + (worldBounds2.min.x - worldBounds.max.x) * Vector3.right;
			}
			else if (flag3)
			{
				vector = mHitPos + (worldBounds2.max.x - worldBounds.min.x) * Vector3.right;
			}
			else if (flag4)
			{
				vector = mHitPos + (worldBounds2.min.z - worldBounds.max.z) * Vector3.forward;
			}
			else if (flag5)
			{
				vector = mHitPos + (worldBounds2.max.z - worldBounds.min.z) * Vector3.forward;
			}
			else if (flag6)
			{
				vector = mHitPos + (worldBounds2.max.y - worldBounds.min.y) * Vector3.up;
				if (!canPutUp)
				{
					rootGameObject.transform.position = vector;
					mOverlaped = true;
					itemBounds.activeState = false;
					mFindBounds.Clear();
					return;
				}
			}
			else if (flag7)
			{
				vector = mHitPos + (worldBounds2.min.y - worldBounds.max.y) * Vector3.up;
				rootGameObject.transform.position = vector;
				mOverlaped = true;
				itemBounds.activeState = false;
				mFindBounds.Clear();
				return;
			}
			if (!flag6)
			{
				if (!Physics.Raycast(vector + Vector3.up, Vector3.down, out fhitInfo, 10f, layerMask) || !(mAdsorbItemBounds.worldBounds.min.y - fhitInfo.point.y < itemBounds.selfBounds.max.y))
				{
					rootGameObject.transform.position = vector;
					mOverlaped = true;
					itemBounds.activeState = false;
					mFindBounds.Clear();
					return;
				}
				vector.y = fhitInfo.point.y;
			}
			rootGameObject.transform.position = vector;
			worldBounds = itemBounds.worldBounds;
			for (int j = 0; j < mFindBounds.Count; j++)
			{
				if (worldBounds.Intersects(mFindBounds[j].worldBounds))
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
		mFindBounds.Clear();
		mOverlaped = false;
		itemBounds.activeState = flag || mRayHitTerrain;
	}

	public virtual bool OnCheckPutDown()
	{
		return true;
	}

	public virtual bool OnPutDown()
	{
		CloseItemBounds();
		ClearSubterrain();
		PlaySound();
		return true;
	}

	protected void CloseItemBounds()
	{
		if (null != mAdsorbItemBounds)
		{
			mAdsorbItemBounds.showBounds = false;
			mAdsorbItemBounds = null;
		}
		if (null != mOverlapedItemBounds)
		{
			mOverlapedItemBounds.showBounds = false;
			mOverlapedItemBounds = null;
		}
	}

	protected void ClearSubterrain()
	{
		if (null == itemBounds)
		{
			return;
		}
		if (PeGameMgr.IsMulti)
		{
			List<Vector3> grassPos = new List<Vector3>();
			Bounds worldBounds = itemBounds.worldBounds;
			Vector3 vector = worldBounds.min - subTerrainClearRadius * Vector3.one;
			Vector3 vector2 = worldBounds.max + subTerrainClearRadius * Vector3.one;
			for (float num = vector.x; num <= vector2.x; num += 1f)
			{
				for (float num2 = vector.y; num2 <= vector2.y; num2 += 1f)
				{
					for (float num3 = vector.z; num3 <= vector2.z; num3 += 1f)
					{
						Vector3 voxelPos = new Vector3(Mathf.RoundToInt(num), Mathf.RoundToInt(num2), Mathf.RoundToInt(num3));
						if (PeGrassSystem.DeleteAtPos(voxelPos, out var pos))
						{
							grassPos.Add(pos);
						}
					}
				}
			}
			if (grassPos.Count == 0)
			{
				return;
			}
			byte[] array = Serialize.Export(delegate(BinaryWriter w)
			{
				w.Write(grassPos.Count);
				for (int i = 0; i < grassPos.Count; i++)
				{
					BufferHelper.Serialize(w, grassPos[i]);
				}
			});
			if (null != PlayerNetwork.mainPlayer)
			{
				PlayerNetwork.mainPlayer.RPCServer(EPacketType.PT_InGame_ClearGrass, array);
			}
			return;
		}
		Bounds worldBounds2 = itemBounds.worldBounds;
		Vector3 vector3 = worldBounds2.min - subTerrainClearRadius * Vector3.one;
		Vector3 vector4 = worldBounds2.max + subTerrainClearRadius * Vector3.one;
		for (float num4 = vector3.x; num4 <= vector4.x; num4 += 1f)
		{
			for (float num5 = vector3.y; num5 <= vector4.y; num5 += 1f)
			{
				for (float num6 = vector3.z; num6 <= vector4.z; num6 += 1f)
				{
					PeGrassSystem.DeleteAtPos(new Vector3(Mathf.RoundToInt(num4), Mathf.RoundToInt(num5), Mathf.RoundToInt(num6)));
				}
			}
		}
	}

	protected void PlaySound()
	{
		if (itemDragging.itemObj.protoData.placeSoundID != 0)
		{
			AudioManager.instance.Create(base.transform.position, itemDragging.itemObj.protoData.placeSoundID);
		}
	}

	protected void CheckTreeAndSkEntity()
	{
		if (null == itemBounds)
		{
			mHasTree = false;
			return;
		}
		Bounds worldBounds = itemBounds.worldBounds;
		Vector3 min = worldBounds.min;
		Vector3 min2 = worldBounds.min;
		min2.z = worldBounds.max.z;
		float num = worldBounds.max.y - worldBounds.min.y;
		int num2 = Mathf.FloorToInt(num / treeCheckHeight) + 2;
		float maxDistance = worldBounds.max.x - worldBounds.min.x;
		for (int i = 0; i < num2; i++)
		{
			if (Physics.CheckCapsule(min, min2, 0.01f, treeAndEntityLayer) || Physics.CapsuleCast(min, min2, 0.01f, Vector3.right, maxDistance, treeAndEntityLayer))
			{
				mHasTree = true;
				itemBounds.activeState = false;
				return;
			}
			min.y += treeCheckHeight;
			min2.y += treeCheckHeight;
		}
		mHasTree = false;
	}

	public virtual void OnCancel()
	{
	}

	public virtual void OnRotate()
	{
		base.transform.Rotate(new Vector3(0f, 90f, 0f));
	}

	protected void RemoveFromBag()
	{
		PackageCmpt packageCmpt = PeSingleton<PeCreature>.Instance.mainPlayer.packageCmpt;
		if (itemDragging.itemObj.stackCount > 1)
		{
			packageCmpt.DestroyItem(itemDragging.itemObj, 1);
		}
		else
		{
			packageCmpt.Remove(itemDragging.itemObj);
		}
		if (PlayerPackageCmpt.LockStackCount && !ItemMgr.IsCreationItem(itemDragging.itemObj.protoId))
		{
			PlayerPackageCmpt playerPackageCmpt = packageCmpt as PlayerPackageCmpt;
			if (playerPackageCmpt != null)
			{
				playerPackageCmpt.package.Add(itemDragging.itemObj.protoId, 1);
			}
		}
	}
}
