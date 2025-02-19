using ItemAsset;
using Pathea;
using UnityEngine;

public class PEFlashLight : PECtrlAbleEquipment
{
	public Transform aimTrans;

	public override void InitEquipment(PeEntity entity, ItemObject itemObj)
	{
		base.InitEquipment(entity, itemObj);
		m_View.AttachObject(base.gameObject, "mountOff");
	}
}
