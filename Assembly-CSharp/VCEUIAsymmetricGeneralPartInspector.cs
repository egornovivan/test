using WhiteCat;

public class VCEUIAsymmetricGeneralPartInspector : VCEUIComponentInspector
{
	public VCEUIVector3Input m_PositionInput;

	public VCEUIVector3Input m_RotationInput;

	public UICheckbox m_Phase0Check;

	public UICheckbox m_Phase1Check;

	public UICheckbox m_VisibleCheck;

	public UIToggleGroup m_Group;

	private VCAsymmetricGeneralPartData m_Data;

	public override VCComponentData Get()
	{
		VCAsymmetricGeneralPartData vCAsymmetricGeneralPartData = m_Data.Copy() as VCAsymmetricGeneralPartData;
		vCAsymmetricGeneralPartData.m_Position = m_PositionInput.Vector;
		vCAsymmetricGeneralPartData.m_Rotation = VCEMath.NormalizeEulerAngle(m_RotationInput.Vector);
		vCAsymmetricGeneralPartData.m_Visible = m_VisibleCheck.isChecked;
		vCAsymmetricGeneralPartData.m_Positive = m_Phase0Check.isChecked;
		if ((bool)m_Group)
		{
			vCAsymmetricGeneralPartData.m_ExtendData = m_Group.selected;
		}
		vCAsymmetricGeneralPartData.Validate();
		m_PositionInput.Vector = vCAsymmetricGeneralPartData.m_Position;
		m_RotationInput.Vector = vCAsymmetricGeneralPartData.m_Rotation;
		m_VisibleCheck.isChecked = vCAsymmetricGeneralPartData.m_Visible;
		m_Phase0Check.isChecked = vCAsymmetricGeneralPartData.m_Positive;
		m_Phase1Check.isChecked = !vCAsymmetricGeneralPartData.m_Positive;
		return vCAsymmetricGeneralPartData;
	}

	public override void Set(VCComponentData data)
	{
		data.Validate();
		m_Data = data.Copy() as VCAsymmetricGeneralPartData;
		m_PositionInput.Vector = m_Data.m_Position;
		m_RotationInput.Vector = m_Data.m_Rotation;
		m_VisibleCheck.isChecked = m_Data.m_Visible;
		m_Phase0Check.isChecked = m_Data.m_Positive;
		m_Phase1Check.isChecked = !m_Data.m_Positive;
		if ((bool)m_Group)
		{
			m_Group.selected = data.m_ExtendData;
		}
	}

	public void OnApplyClick()
	{
		m_SelectBrush.ApplyInspectorChange();
		m_Data = Get().Copy() as VCAsymmetricGeneralPartData;
	}

	protected override bool Changed()
	{
		if (!VCUtils.VectorApproximate(m_PositionInput.Vector, m_Data.m_Position, m_PositionInput.m_Format))
		{
			return true;
		}
		if (!VCUtils.VectorApproximate(m_RotationInput.Vector, m_Data.m_Rotation, m_RotationInput.m_Format))
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
		if ((bool)m_Group && m_Data.m_ExtendData != m_Group.selected)
		{
			return true;
		}
		return false;
	}
}
