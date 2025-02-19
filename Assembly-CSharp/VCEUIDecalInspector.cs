using UnityEngine;

public class VCEUIDecalInspector : VCEUIComponentInspector
{
	public VCEUIVector3Input m_PositionInput;

	public VCEUIVector3Input m_RotationInput;

	public VCEUIVector3Input m_ScaleInput;

	public UICheckbox m_MirroredCheck;

	public VCEUIColorPick m_ColorPicker;

	public UICheckbox m_Shader0Check;

	public UICheckbox m_Shader1Check;

	private VCDecalData m_Data;

	public override VCComponentData Get()
	{
		VCDecalData vCDecalData = m_Data.Copy() as VCDecalData;
		vCDecalData.m_Position = m_PositionInput.Vector;
		vCDecalData.m_Rotation = VCEMath.NormalizeEulerAngle(m_RotationInput.Vector);
		Vector3 vector = m_ScaleInput.Vector;
		vCDecalData.m_Size = vector.x;
		vCDecalData.m_Depth = vector.z;
		vector.y = vector.x;
		vCDecalData.m_Mirrored = m_MirroredCheck.isChecked;
		vCDecalData.m_Color = m_ColorPicker.FinalColor;
		if (m_Shader0Check.isChecked)
		{
			vCDecalData.m_ShaderIndex = 0;
		}
		else if (m_Shader1Check.isChecked)
		{
			vCDecalData.m_ShaderIndex = 1;
		}
		else
		{
			vCDecalData.m_ShaderIndex = 0;
		}
		vCDecalData.Validate();
		m_PositionInput.Vector = vCDecalData.m_Position;
		m_RotationInput.Vector = vCDecalData.m_Rotation;
		m_ScaleInput.Vector = vector;
		return vCDecalData;
	}

	public override void Set(VCComponentData data)
	{
		data.Validate();
		m_Data = data.Copy() as VCDecalData;
		m_PositionInput.Vector = m_Data.m_Position;
		m_RotationInput.Vector = m_Data.m_Rotation;
		m_ScaleInput.Vector = new Vector3(m_Data.m_Size, m_Data.m_Size, m_Data.m_Depth);
		m_MirroredCheck.isChecked = m_Data.m_Mirrored;
		m_ColorPicker.FinalColor = m_Data.m_Color;
		if (m_Data.m_ShaderIndex == 0)
		{
			m_Shader0Check.isChecked = true;
			m_Shader1Check.isChecked = false;
		}
		else if (m_Data.m_ShaderIndex == 1)
		{
			m_Shader1Check.isChecked = true;
			m_Shader0Check.isChecked = false;
		}
		else
		{
			m_Shader0Check.isChecked = true;
			m_Shader1Check.isChecked = false;
		}
	}

	public void OnApplyClick()
	{
		m_SelectBrush.ApplyInspectorChange();
		m_Data = Get().Copy() as VCDecalData;
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
		if (!VCUtils.VectorApproximate(m_ScaleInput.Vector, new Vector3(m_Data.m_Size, m_Data.m_Size, m_Data.m_Depth), m_ScaleInput.m_Format))
		{
			return true;
		}
		if (m_MirroredCheck.isChecked != m_Data.m_Mirrored)
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
		if (m_Shader0Check.isChecked && m_Data.m_ShaderIndex != 0)
		{
			return true;
		}
		if (!m_Shader0Check.isChecked && m_Data.m_ShaderIndex == 0)
		{
			return true;
		}
		if (m_Shader1Check.isChecked && m_Data.m_ShaderIndex != 1)
		{
			return true;
		}
		if (!m_Shader1Check.isChecked && m_Data.m_ShaderIndex == 1)
		{
			return true;
		}
		return false;
	}
}
