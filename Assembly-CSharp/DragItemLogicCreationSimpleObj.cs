using UnityEngine;
using WhiteCat;

public class DragItemLogicCreationSimpleObj : DragItemLogic
{
	public override void OnConstruct()
	{
		ItemScript[] componentsInChildren = GetComponentsInChildren<ItemScript>(includeInactive: true);
		ItemScript[] array = componentsInChildren;
		foreach (ItemScript itemScript in array)
		{
			if (itemScript != null)
			{
				itemScript.InitNetlayer(mNetlayer);
				itemScript.SetItemObject(itemDrag.itemObj);
				itemScript.id = id;
				itemScript.OnConstruct();
			}
		}
	}

	public override void OnDestruct()
	{
		base.OnDestruct();
		ItemScript[] componentsInChildren = GetComponentsInChildren<ItemScript>(includeInactive: true);
		ItemScript[] array = componentsInChildren;
		foreach (ItemScript itemScript in array)
		{
			if (itemScript != null)
			{
				itemScript.OnDestruct();
			}
		}
	}

	public override void OnActivate()
	{
		base.OnActivate();
		ItemScript[] componentsInChildren = GetComponentsInChildren<ItemScript>(includeInactive: true);
		ItemScript[] array = componentsInChildren;
		foreach (ItemScript itemScript in array)
		{
			if (itemScript != null)
			{
				itemScript.OnActivate();
			}
		}
		GetComponent<Rigidbody>().constraints &= (RigidbodyConstraints)(-5);
		CreationController component = GetComponent<CreationController>();
		bool collidable = (component.visible = true);
		component.collidable = collidable;
	}

	public override void OnDeactivate()
	{
		base.OnDeactivate();
		ItemScript[] componentsInChildren = GetComponentsInChildren<ItemScript>(includeInactive: true);
		ItemScript[] array = componentsInChildren;
		foreach (ItemScript itemScript in array)
		{
			if (itemScript != null)
			{
				itemScript.OnDeactivate();
			}
		}
		GetComponent<Rigidbody>().constraints |= RigidbodyConstraints.FreezePositionY;
		CreationController component = GetComponent<CreationController>();
		bool collidable = (component.visible = false);
		component.collidable = collidable;
	}
}
