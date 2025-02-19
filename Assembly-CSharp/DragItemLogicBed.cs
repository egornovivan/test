using PETools;
using UnityEngine;

public class DragItemLogicBed : DragItemLogic
{
	private ItemScript_Bed mBedView;

	public override void OnConstruct()
	{
		base.OnConstruct();
		if (itemDrag != null)
		{
			GameObject gameObject = itemDrag.CreateViewGameObject(null);
			if (gameObject != null)
			{
				gameObject.transform.parent = base.transform;
				PEUtil.ResetTransform(gameObject.transform);
				mBedView = gameObject.GetComponent<ItemScript_Bed>();
			}
		}
		if (mBedView != null)
		{
			mBedView.InitNetlayer(mNetlayer);
			mBedView.SetItemObject(itemDrag.itemObj);
			mBedView.id = id;
			mBedView.OnConstruct();
		}
	}

	public override void OnDestruct()
	{
		base.OnDestruct();
		if (mBedView != null)
		{
			mBedView.OnDestruct();
		}
	}

	public override void OnActivate()
	{
		base.OnActivate();
		if (mBedView != null)
		{
			mBedView.OnActivate();
		}
	}

	public override void OnDeactivate()
	{
		base.OnDeactivate();
		if (mBedView != null)
		{
			mBedView.OnDeactivate();
		}
	}
}
