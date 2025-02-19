using ItemAsset;
using UnityEngine;

public class DragItemLogic : MonoBehaviour
{
	public int id;

	public Drag itemDrag;

	public NetworkInterface mNetlayer;

	public void SetItemDrag(Drag itemDrag)
	{
		this.itemDrag = itemDrag;
	}

	public void InitNetlayer(NetworkInterface netlayer)
	{
		mNetlayer = netlayer;
	}

	public virtual void OnActivate()
	{
	}

	public virtual void OnDeactivate()
	{
	}

	public virtual void OnConstruct()
	{
	}

	public virtual void OnDestruct()
	{
	}
}
