using WhiteCat;

public class VCEUIFreePartInspector : VCEUIComponentInspector
{
	public VCEUIVector3Input m_PositionInput;

	public VCEUIVector3Input m_RotationInput;

	public VCEUIVector3Input m_ScaleInput;

	public UICheckbox m_VisibleCheck;

	public UIToggleGroup m_Group;

	private VCFreePartData m_Data;

	public override VCComponentData Get()
	{
		VCFreePartData vCFreePartData = m_Data.Copy() as VCFreePartData;
		vCFreePartData.m_Position = m_PositionInput.Vector;
		if ((bool)m_RotationInput)
		{
			vCFreePartData.m_Rotation = VCEMath.NormalizeEulerAngle(m_RotationInput.Vector);
		}
		vCFreePartData.m_Scale = m_ScaleInput.Vector;
		vCFreePartData.m_Visible = m_VisibleCheck.isChecked;
		if ((bool)m_Group)
		{
			vCFreePartData.m_ExtendData = m_Group.selected;
		}
		vCFreePartData.Validate();
		m_PositionInput.Vector = vCFreePartData.m_Position;
		if ((bool)m_RotationInput)
		{
			m_RotationInput.Vector = vCFreePartData.m_Rotation;
		}
		m_ScaleInput.Vector = vCFreePartData.m_Scale;
		m_VisibleCheck.isChecked = vCFreePartData.m_Visible;
		return vCFreePartData;
	}

	public override void Set(VCComponentData data)
	{
		data.Validate();
		m_Data = data.Copy() as VCFreePartData;
		m_PositionInput.Vector = m_Data.m_Position;
		if ((bool)m_RotationInput)
		{
			m_RotationInput.Vector = m_Data.m_Rotation;
		}
		m_ScaleInput.Vector = m_Data.m_Scale;
		m_VisibleCheck.isChecked = m_Data.m_Visible;
		if ((bool)m_Group)
		{
			m_Group.selected = data.m_ExtendData;
		}
	}

	public void OnApplyClick()
	{
		m_SelectBrush.ApplyInspectorChange();
		m_Data = Get().Copy() as VCFreePartData;
	}

	protected override bool Changed()
	{
		if (!VCUtils.VectorApproximate(m_PositionInput.Vector, m_Data.m_Position, m_PositionInput.m_Format))
		{
			return true;
		}
		if ((bool)m_RotationInput && !VCUtils.VectorApproximate(m_RotationInput.Vector, m_Data.m_Rotation, m_RotationInput.m_Format))
		{
			return true;
		}
		if (!VCUtils.VectorApproximate(m_ScaleInput.Vector, m_Data.m_Scale, m_ScaleInput.m_Format))
		{
			return true;
		}
		if (m_VisibleCheck.isChecked != m_Data.m_Visible)
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
