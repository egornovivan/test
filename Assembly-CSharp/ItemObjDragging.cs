using ItemAsset;
using UnityEngine;

public class ItemObjDragging : DraggingMgr.IDragable
{
	protected ItemDraggingBase dragBase;

	private Drag mDrag;

	public ItemDraggingBase DragBase => dragBase;

	public ItemObjDragging(Drag drag)
	{
		mDrag = drag;
	}

	void DraggingMgr.IDragable.OnDragOut()
	{
		dragBase = CreateGameObj();
		if (!(null == dragBase))
		{
			dragBase.OnDragOut();
		}
	}

	bool DraggingMgr.IDragable.OnDragging(Ray cameraRay)
	{
		if (null == dragBase)
		{
			return false;
		}
		return dragBase.OnDragging(cameraRay);
	}

	bool DraggingMgr.IDragable.OnCheckPutDown()
	{
		if (null == dragBase)
		{
			return false;
		}
		return dragBase.OnCheckPutDown();
	}

	void DraggingMgr.IDragable.OnPutDown()
	{
		if (!(null == dragBase) && dragBase.OnPutDown())
		{
			Destroy();
		}
	}

	void DraggingMgr.IDragable.OnCancel()
	{
		if (!(null == dragBase))
		{
			dragBase.OnCancel();
			Destroy();
		}
	}

	void DraggingMgr.IDragable.OnRotate()
	{
		if (!(null == dragBase))
		{
			dragBase.OnRotate();
		}
	}

	public Drag GetItemDrag()
	{
		return mDrag;
	}

	public int GetItemProtoId()
	{
		if (mDrag.itemObj.protoId > 100000000)
		{
			if (StroyManager.Instance != null)
			{
				return StroyManager.Instance.ItemClassIdtoProtoId(mDrag.itemObj.protoData.itemClassId);
			}
			return mDrag.itemObj.protoId;
		}
		return mDrag.itemObj.protoId;
	}

	public int GetItemInstanceID()
	{
		return mDrag.itemObj.instanceId;
	}

	public Vector3 GetPos()
	{
		if (null == dragBase)
		{
			return Vector3.zero;
		}
		return dragBase.transform.position;
	}

	private ItemDraggingBase CreateGameObj()
	{
		Drag itemDrag = GetItemDrag();
		if (itemDrag == null)
		{
			return null;
		}
		GameObject gameObject = itemDrag.CreateDraggingGameObject(null);
		if (null == gameObject)
		{
			return null;
		}
		ItemDraggingBase itemDraggingBase = gameObject.GetComponent<ItemDraggingBase>();
		if (null == itemDraggingBase)
		{
			itemDraggingBase = gameObject.AddComponent<ItemDraggingArticle>();
		}
		itemDraggingBase.itemDragging = itemDrag;
		gameObject.SetActive(value: true);
		return itemDraggingBase;
	}

	private void Destroy()
	{
		if (null != dragBase)
		{
			Object.Destroy(dragBase.gameObject);
		}
	}
}
