public class VCEUIFixedHandPartInspector : VCEUIComponentInspector
{
	public VCEUIVector3Input m_PositionInput;

	public VCEUIVector3Input m_RotationInput;

	public UICheckbox m_VisibleCheck;

	public UICheckbox m_Phase0Check;

	public UICheckbox m_Phase1Check;

	private VCFixedHandPartData m_Data;

	public override VCComponentData Get()
	{
		VCFixedHandPartData vCFixedHandPartData = m_Data.Copy() as VCFixedHandPartData;
		vCFixedHandPartData.m_Position = m_PositionInput.Vector;
		vCFixedHandPartData.m_Rotation = VCEMath.NormalizeEulerAngle(m_RotationInput.Vector);
		vCFixedHandPartData.m_Visible = !m_VisibleCheck || m_VisibleCheck.isChecked;
		vCFixedHandPartData.m_LeftHand = m_Phase0Check.isChecked;
		vCFixedHandPartData.Validate();
		m_PositionInput.Vector = vCFixedHandPartData.m_Position;
		m_RotationInput.Vector = vCFixedHandPartData.m_Rotation;
		if ((bool)m_VisibleCheck)
		{
			m_VisibleCheck.isChecked = vCFixedHandPartData.m_Visible;
		}
		return vCFixedHandPartData;
	}

	public override void Set(VCComponentData data)
	{
		data.Validate();
		m_Data = data.Copy() as VCFixedHandPartData;
		m_PositionInput.Vector = m_Data.m_Position;
		m_RotationInput.Vector = m_Data.m_Rotation;
		if ((bool)m_VisibleCheck)
		{
			m_VisibleCheck.isChecked = m_Data.m_Visible;
		}
		m_Phase0Check.isChecked = m_Data.m_LeftHand;
		m_Phase1Check.isChecked = !m_Data.m_LeftHand;
	}

	public void OnApplyClick()
	{
		m_SelectBrush.ApplyInspectorChange();
		m_Data = Get().Copy() as VCFixedHandPartData;
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
		if ((bool)m_VisibleCheck && m_VisibleCheck.isChecked != m_Data.m_Visible)
		{
			return true;
		}
		if (m_Phase0Check.isChecked != m_Data.m_LeftHand)
		{
			return true;
		}
		if (m_Phase1Check.isChecked == m_Data.m_LeftHand)
		{
			return true;
		}
		return false;
	}
}
