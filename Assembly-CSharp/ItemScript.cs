using ItemAsset;
using UnityEngine;

public class ItemScript : MonoBehaviour
{
	protected ItemObject mItemObj;

	protected NetworkInterface mNetlayer;

	public int id;

	public NetworkInterface netLayer => mNetlayer;

	public int itemObjectId
	{
		get
		{
			if (mItemObj != null)
			{
				return mItemObj.instanceId;
			}
			return -1;
		}
	}

	public ItemObject MItemObj => mItemObj;

	public GameObject rootGameObject => base.gameObject;

	public virtual void SetItemObject(ItemObject itemObj)
	{
		mItemObj = itemObj;
	}

	public void InitNetlayer(NetworkInterface netlayer)
	{
		mNetlayer = netlayer;
	}

	private void EnableCollider()
	{
		Collider[] componentsInChildren = rootGameObject.GetComponentsInChildren<Collider>();
		Collider[] array = componentsInChildren;
		foreach (Collider collider in array)
		{
			collider.enabled = true;
			if (collider is MeshCollider)
			{
				collider.isTrigger = false;
			}
		}
	}

	private void DisableCollider()
	{
		Collider[] componentsInChildren = rootGameObject.GetComponentsInChildren<Collider>();
		Collider[] array = componentsInChildren;
		foreach (Collider collider in array)
		{
			collider.enabled = false;
		}
	}

	public virtual void OnDeactivate()
	{
	}

	public virtual void OnActivate()
	{
	}

	public virtual void OnDestruct()
	{
	}

	public virtual void OnConstruct()
	{
	}
}
