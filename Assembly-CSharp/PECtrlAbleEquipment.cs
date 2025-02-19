using ItemAsset;
using Pathea;

public class PECtrlAbleEquipment : PEEquipment
{
	protected Motion_Equip m_MotionEquip;

	protected MotionMgrCmpt m_MotionMgr;

	public PEActionType[] m_RemoveEndAction;

	private Durability m_Durability;

	public float durability => (m_Durability == null) ? 100f : m_Durability.floatValue.current;

	public override void InitEquipment(PeEntity entity, ItemObject itemObj)
	{
		base.InitEquipment(entity, itemObj);
		m_MotionEquip = m_Entity.GetCmpt<Motion_Equip>();
		m_MotionMgr = m_Entity.GetCmpt<MotionMgrCmpt>();
		m_Durability = itemObj.GetCmpt<Durability>();
	}

	public override bool CanTakeOff()
	{
		if (null != m_MotionMgr)
		{
			PEActionType[] removeEndAction = m_RemoveEndAction;
			foreach (PEActionType type in removeEndAction)
			{
				if (m_MotionMgr.IsActionRunning(type))
				{
					return false;
				}
			}
		}
		return true;
	}
}
