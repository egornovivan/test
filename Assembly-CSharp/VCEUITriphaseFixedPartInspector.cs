public class VCEUITriphaseFixedPartInspector : VCEUIComponentInspector
{
	public VCEUIVector3Input m_PositionInput;

	public UICheckbox m_Phase0Check;

	public UICheckbox m_Phase1Check;

	public UICheckbox m_Phase2Check;

	public UICheckbox m_VisibleCheck;

	private VCTriphaseFixedPartData m_Data;

	public override VCComponentData Get()
	{
		VCTriphaseFixedPartData vCTriphaseFixedPartData = m_Data.Copy() as VCTriphaseFixedPartData;
		vCTriphaseFixedPartData.m_Position = m_PositionInput.Vector;
		vCTriphaseFixedPartData.m_Visible = m_VisibleCheck.isChecked;
		vCTriphaseFixedPartData.m_Phase = ((!m_Phase0Check.isChecked) ? (m_Phase1Check.isChecked ? 1 : 2) : 0);
		vCTriphaseFixedPartData.Validate();
		m_PositionInput.Vector = vCTriphaseFixedPartData.m_Position;
		m_VisibleCheck.isChecked = vCTriphaseFixedPartData.m_Visible;
		return vCTriphaseFixedPartData;
	}

	public override void Set(VCComponentData data)
	{
		data.Validate();
		m_Data = data.Copy() as VCTriphaseFixedPartData;
		m_PositionInput.Vector = m_Data.m_Position;
		m_VisibleCheck.isChecked = m_Data.m_Visible;
		m_Phase0Check.isChecked = m_Data.m_Phase == 0;
		m_Phase1Check.isChecked = m_Data.m_Phase == 1;
		m_Phase2Check.isChecked = m_Data.m_Phase == 2;
	}

	public void OnApplyClick()
	{
		m_SelectBrush.ApplyInspectorChange();
		m_Data = Get().Copy() as VCTriphaseFixedPartData;
	}

	protected override bool Changed()
	{
		if (!VCUtils.VectorApproximate(m_PositionInput.Vector, m_Data.m_Position, m_PositionInput.m_Format))
		{
			return true;
		}
		if (m_VisibleCheck.isChecked != m_Data.m_Visible)
		{
			return true;
		}
		if (m_Phase0Check.isChecked != (m_Data.m_Phase == 0))
		{
			return true;
		}
		if (m_Phase1Check.isChecked != (m_Data.m_Phase == 1))
		{
			return true;
		}
		if (m_Phase2Check.isChecked != (m_Data.m_Phase == 2))
		{
			return true;
		}
		return false;
	}
}
