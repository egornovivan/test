public class VCEUIAsymmetricFixedPartInspector : VCEUIComponentInspector
{
	public VCEUIVector3Input m_PositionInput;

	public UICheckbox m_Phase0Check;

	public UICheckbox m_Phase1Check;

	public UICheckbox m_VisibleCheck;

	private VCAsymmetricFixedPartData m_Data;

	public override VCComponentData Get()
	{
		VCAsymmetricFixedPartData vCAsymmetricFixedPartData = m_Data.Copy() as VCAsymmetricFixedPartData;
		vCAsymmetricFixedPartData.m_Position = m_PositionInput.Vector;
		vCAsymmetricFixedPartData.m_Visible = m_VisibleCheck.isChecked;
		vCAsymmetricFixedPartData.m_Positive = m_Phase0Check.isChecked;
		vCAsymmetricFixedPartData.Validate();
		m_PositionInput.Vector = vCAsymmetricFixedPartData.m_Position;
		m_VisibleCheck.isChecked = vCAsymmetricFixedPartData.m_Visible;
		m_Phase0Check.isChecked = vCAsymmetricFixedPartData.m_Positive;
		m_Phase1Check.isChecked = !vCAsymmetricFixedPartData.m_Positive;
		return vCAsymmetricFixedPartData;
	}

	public override void Set(VCComponentData data)
	{
		data.Validate();
		m_Data = data.Copy() as VCAsymmetricFixedPartData;
		m_PositionInput.Vector = m_Data.m_Position;
		m_VisibleCheck.isChecked = m_Data.m_Visible;
		m_Phase0Check.isChecked = m_Data.m_Positive;
		m_Phase1Check.isChecked = !m_Data.m_Positive;
	}

	public void OnApplyClick()
	{
		m_SelectBrush.ApplyInspectorChange();
		m_Data = Get().Copy() as VCAsymmetricFixedPartData;
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
		if (m_Phase0Check.isChecked != m_Data.m_Positive)
		{
			return true;
		}
		if (m_Phase1Check.isChecked == m_Data.m_Positive)
		{
			return true;
		}
		return false;
	}
}
