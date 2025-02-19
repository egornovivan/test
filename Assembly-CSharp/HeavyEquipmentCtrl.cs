using Pathea;

public class HeavyEquipmentCtrl
{
	public Motion_Move_Human moveCmpt;

	public HumanPhyCtrl phyCtrl;

	public IKCmpt ikCmpt;

	public MotionMgrCmpt motionMgr;

	private IHeavyEquipment m_HeavyEquipment;

	private bool m_InWater;

	public IHeavyEquipment heavyEquipment
	{
		get
		{
			if (m_HeavyEquipment == null || m_HeavyEquipment.Equals(null))
			{
				return null;
			}
			return m_HeavyEquipment;
		}
		set
		{
			m_HeavyEquipment = value;
			if (null != moveCmpt)
			{
				moveCmpt.style = ((m_HeavyEquipment == null) ? moveCmpt.baseMoveStyle : m_HeavyEquipment.baseMoveStyle);
			}
			if (null != ikCmpt)
			{
				ikCmpt.SetSpineEffectDeactiveState(GetType(), null != m_HeavyEquipment);
			}
			if (null != motionMgr)
			{
				motionMgr.SetMaskState(PEActionMask.HeavyEquipment, null != m_HeavyEquipment);
			}
		}
	}

	public void Update()
	{
		if (heavyEquipment != null && null != phyCtrl && m_InWater != phyCtrl.spineInWater)
		{
			m_InWater = phyCtrl.spineInWater;
			moveCmpt.style = ((!phyCtrl.spineInWater) ? heavyEquipment.baseMoveStyle : moveCmpt.baseMoveStyle);
			heavyEquipment.HidEquipmentByUnderWater(phyCtrl.spineInWater);
		}
	}
}
