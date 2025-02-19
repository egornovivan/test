using ItemAsset;
using Pathea;
using UnityEngine;

public class PEEquipmentLogic : MonoBehaviour
{
	protected ItemObject m_ItemObj;

	protected PeEntity m_Entity;

	protected Motion_Equip m_Equip;

	public ItemObject itemObject => m_ItemObj;

	public virtual void InitEquipment(PeEntity entity, ItemObject itemObj)
	{
		m_ItemObj = itemObj;
		m_Entity = entity;
		m_Equip = m_Entity.GetCmpt<Motion_Equip>();
	}

	public virtual void RemoveEquipment()
	{
		Object.Destroy(base.gameObject);
	}

	public virtual void OnModelRebuild()
	{
	}
}
