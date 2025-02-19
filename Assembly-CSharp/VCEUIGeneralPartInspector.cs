using WhiteCat;

public class VCEUIGeneralPartInspector : VCEUIComponentInspector
{
	public VCEUIVector3Input m_PositionInput;

	public VCEUIVector3Input m_RotationInput;

	public UICheckbox m_VisibleCheck;

	private int m_ArmorPartIndex;

	private VCGeneralPartData m_Data;

	public override VCComponentData Get()
	{
		VCGeneralPartData vCGeneralPartData = m_Data.Copy() as VCGeneralPartData;
		vCGeneralPartData.m_Position = m_PositionInput.Vector;
		vCGeneralPartData.m_Rotation = VCEMath.NormalizeEulerAngle(m_RotationInput.Vector);
		vCGeneralPartData.m_Visible = !m_VisibleCheck || m_VisibleCheck.isChecked;
		vCGeneralPartData.Validate();
		vCGeneralPartData.m_ExtendData = VCEditor.Instance.m_UI.bonePanel.ArmorPartIndex;
		m_PositionInput.Vector = vCGeneralPartData.m_Position;
		m_RotationInput.Vector = vCGeneralPartData.m_Rotation;
		if ((bool)m_VisibleCheck)
		{
			m_VisibleCheck.isChecked = vCGeneralPartData.m_Visible;
		}
		m_ArmorPartIndex = vCGeneralPartData.m_ExtendData;
		return vCGeneralPartData;
	}

	public override void Set(VCComponentData data)
	{
		data.Validate();
		m_Data = data.Copy() as VCGeneralPartData;
		m_PositionInput.Vector = m_Data.m_Position;
		m_RotationInput.Vector = m_Data.m_Rotation;
		m_ArmorPartIndex = m_Data.m_ExtendData;
		if ((bool)m_VisibleCheck)
		{
			m_VisibleCheck.isChecked = m_Data.m_Visible;
		}
		VCPArmorPivot vCPArmorPivotByIndex = m_SelectBrush.GetVCPArmorPivotByIndex(m_Data.m_ExtendData);
		if ((bool)vCPArmorPivotByIndex)
		{
			VCEditor.Instance.m_UI.bonePanel.Show(vCPArmorPivotByIndex);
		}
	}

	public void OnApplyClick()
	{
		m_SelectBrush.ApplyInspectorChange();
		m_Data = Get().Copy() as VCGeneralPartData;
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
		if ((bool)VCEditor.Instance && (bool)VCEditor.Instance.m_UI && (bool)VCEditor.Instance.m_UI.bonePanel && m_ArmorPartIndex != VCEditor.Instance.m_UI.bonePanel.ArmorPartIndex)
		{
			return true;
		}
		return false;
	}
}
