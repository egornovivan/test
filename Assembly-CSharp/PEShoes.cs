using ItemAsset;
using Pathea;
using UnityEngine;

public class PEShoes : PEEquipmentLogic
{
	[SerializeField]
	private float m_SpeedScale = 3f;

	public override void InitEquipment(PeEntity entity, ItemObject itemObj)
	{
		base.InitEquipment(entity, itemObj);
		ResetSpeed(enable: true);
	}

	public override void RemoveEquipment()
	{
		base.RemoveEquipment();
		ResetSpeed(enable: false);
	}

	public override void OnModelRebuild()
	{
		base.OnModelRebuild();
		ResetSpeed(enable: true);
	}

	private void ResetSpeed(bool enable)
	{
		if (null != m_Entity && null != m_Entity.biologyViewCmpt && null != m_Entity.biologyViewCmpt.monoPhyCtrl)
		{
			m_Entity.biologyViewCmpt.monoPhyCtrl.mSpeedTimes = ((!enable) ? 1f : m_SpeedScale);
		}
	}
}
