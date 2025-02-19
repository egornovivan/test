using UnityEngine;

public class VCEUIObjectLightlInspector : VCEUIComponentInspector
{
	public VCEUIVector3Input m_PositionInput;

	public VCEUIVector3Input m_RotationInput;

	public VCEUIVector3Input m_ScaleInput;

	public VCEUIColorPick m_ColorPicker;

	public UICheckbox m_VisibleCheck;

	private VCObjectLightData m_Data;

	public override VCComponentData Get()
	{
		VCObjectLightData vCObjectLightData = m_Data.Copy() as VCObjectLightData;
		vCObjectLightData.m_Position = m_PositionInput.Vector;
		vCObjectLightData.m_Rotation = VCEMath.NormalizeEulerAngle(m_RotationInput.Vector);
		vCObjectLightData.m_Scale = m_ScaleInput.Vector;
		vCObjectLightData.m_Color = m_ColorPicker.FinalColor;
		vCObjectLightData.m_Visible = m_VisibleCheck.isChecked;
		vCObjectLightData.Validate();
		m_PositionInput.Vector = vCObjectLightData.m_Position;
		m_RotationInput.Vector = vCObjectLightData.m_Rotation;
		m_ScaleInput.Vector = vCObjectLightData.m_Scale;
		m_ColorPicker.FinalColor = vCObjectLightData.m_Color;
		m_VisibleCheck.isChecked = vCObjectLightData.m_Visible;
		return vCObjectLightData;
	}

	public override void Set(VCComponentData data)
	{
		data.Validate();
		m_Data = data.Copy() as VCObjectLightData;
		m_PositionInput.Vector = m_Data.m_Position;
		m_RotationInput.Vector = m_Data.m_Rotation;
		m_ScaleInput.Vector = m_Data.m_Scale;
		m_ColorPicker.FinalColor = m_Data.m_Color;
		m_VisibleCheck.isChecked = m_Data.m_Visible;
	}

	public void OnApplyClick()
	{
		m_SelectBrush.ApplyInspectorChange();
		m_Data = Get().Copy() as VCObjectLightData;
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
		if (!VCUtils.VectorApproximate(m_ScaleInput.Vector, m_Data.m_Scale, m_ScaleInput.m_Format))
		{
			return true;
		}
		Color32 color = m_ColorPicker.FinalColor;
		Color32 color2 = m_Data.m_Color;
		if (color.r != color2.r)
		{
			return true;
		}
		if (color.g != color2.g)
		{
			return true;
		}
		if (color.b != color2.b)
		{
			return true;
		}
		if (m_VisibleCheck.isChecked != m_Data.m_Visible)
		{
			return true;
		}
		return false;
	}
}
