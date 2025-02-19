public class VCEUIQuadphaseFixedPartInspector : VCEUIComponentInspector
{
	public VCEUIVector3Input m_PositionInput;

	public UICheckbox m_Phase0Check;

	public UICheckbox m_Phase1Check;

	public UICheckbox m_SmallBigCheck;

	public UICheckbox m_MotorCheck;

	public UICheckbox m_VisibleCheck;

	private VCQuadphaseFixedPartData m_Data;

	public override VCComponentData Get()
	{
		VCQuadphaseFixedPartData vCQuadphaseFixedPartData = m_Data.Copy() as VCQuadphaseFixedPartData;
		vCQuadphaseFixedPartData.m_Position = m_PositionInput.Vector;
		vCQuadphaseFixedPartData.m_Visible = m_VisibleCheck.isChecked;
		vCQuadphaseFixedPartData.m_Phase = (m_SmallBigCheck.isChecked ? 2 : 0) + ((!m_Phase0Check.isChecked) ? 1 : 0) + ((!m_MotorCheck.isChecked) ? 4 : 0);
		vCQuadphaseFixedPartData.Validate();
		m_PositionInput.Vector = vCQuadphaseFixedPartData.m_Position;
		m_VisibleCheck.isChecked = vCQuadphaseFixedPartData.m_Visible;
		return vCQuadphaseFixedPartData;
	}

	public override void Set(VCComponentData data)
	{
		data.Validate();
		m_Data = data.Copy() as VCQuadphaseFixedPartData;
		m_PositionInput.Vector = m_Data.m_Position;
		m_VisibleCheck.isChecked = m_Data.m_Visible;
		m_Phase0Check.isChecked = (m_Data.m_Phase & 1) == 0;
		m_Phase1Check.isChecked = (m_Data.m_Phase & 1) == 1;
		m_SmallBigCheck.isChecked = (m_Data.m_Phase & 2) == 2;
		m_MotorCheck.isChecked = (m_Data.m_Phase & 4) != 4;
	}

	public void OnApplyClick()
	{
		m_SelectBrush.ApplyInspectorChange();
		m_Data = Get().Copy() as VCQuadphaseFixedPartData;
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
		if (m_Phase0Check.isChecked != ((m_Data.m_Phase & 1) == 0))
		{
			return true;
		}
		if (m_Phase1Check.isChecked != ((m_Data.m_Phase & 1) == 1))
		{
			return true;
		}
		if (m_SmallBigCheck.isChecked != ((m_Data.m_Phase & 2) == 2))
		{
			return true;
		}
		if (m_MotorCheck.isChecked != ((m_Data.m_Phase & 4) != 4))
		{
			return true;
		}
		return false;
	}
}
