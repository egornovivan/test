using UnityEngine;

[RequireComponent(typeof(torch))]
public class ItemScript_Torch : ItemScript_ItemList
{
	public Collider[] m_Colliders;

	private torch mTorch;

	private torch torch
	{
		get
		{
			if (mTorch == null)
			{
				mTorch = GetComponent<torch>();
			}
			return mTorch;
		}
	}

	public override void OnConstruct()
	{
		base.OnConstruct();
		base.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
		torch.SetBurning(v: true);
		ActiveCollider();
	}

	public override void OnDestruct()
	{
		base.OnDestruct();
		torch.SetBurning(v: false);
	}

	private void ActiveCollider()
	{
		for (int i = 0; i < m_Colliders.Length; i++)
		{
			if (null != m_Colliders[i])
			{
				m_Colliders[i].enabled = true;
			}
		}
	}
}
