using System;
using UnityEngine;
using WhiteCat;

public class VCEUIObjectPivotInspector : VCEUIComponentInspector
{
	public VCEUIVector3Input m_PositionInput;

	public VCEUIVector3Input m_RotationInput;

	public VCEUIVector3Input m_ScaleInput;

	public UICheckbox m_VisibleCheck;

	public UILabel m_AnlgeLable;

	public UISlider m_AnlgeSlider;

	private VCObjectPivotData m_Data;

	private VCPPivot pivot;

	private float originValue01;

	public int Float01ToIntAngle(float value01)
	{
		return Mathf.RoundToInt((value01 - 0.5f) * 144f) * 5;
	}

	public override VCComponentData Get()
	{
		VCObjectPivotData vCObjectPivotData = m_Data.Copy() as VCObjectPivotData;
		vCObjectPivotData.m_Position = m_PositionInput.Vector;
		vCObjectPivotData.m_Rotation = VCEMath.NormalizeEulerAngle(m_RotationInput.Vector);
		vCObjectPivotData.m_Scale = m_ScaleInput.Vector;
		vCObjectPivotData.m_Visible = m_VisibleCheck.isChecked;
		int pivotAng = Float01ToIntAngle(m_AnlgeSlider.sliderValue);
		vCObjectPivotData.m_PivotAng = pivotAng;
		vCObjectPivotData.Validate();
		m_PositionInput.Vector = vCObjectPivotData.m_Position;
		m_RotationInput.Vector = vCObjectPivotData.m_Rotation;
		m_ScaleInput.Vector = vCObjectPivotData.m_Scale;
		m_VisibleCheck.isChecked = vCObjectPivotData.m_Visible;
		pivotAng = vCObjectPivotData.m_PivotAng;
		m_AnlgeLable.text = pivotAng.ToString();
		m_AnlgeSlider.sliderValue = Convert.ToSingle(pivotAng) / 720f + 0.5f;
		return vCObjectPivotData;
	}

	public override void Set(VCComponentData data)
	{
		data.Validate();
		m_Data = data.Copy() as VCObjectPivotData;
		m_PositionInput.Vector = m_Data.m_Position;
		m_RotationInput.Vector = m_Data.m_Rotation;
		m_ScaleInput.Vector = m_Data.m_Scale;
		m_VisibleCheck.isChecked = m_Data.m_Visible;
		m_AnlgeLable.text = m_Data.m_PivotAng.ToString();
		m_AnlgeSlider.sliderValue = Convert.ToSingle(m_Data.m_PivotAng) / 720f + 0.5f;
	}

	public void OnApplyClick()
	{
		m_SelectBrush.ApplyInspectorChange();
		m_Data = Get().Copy() as VCObjectPivotData;
		originValue01 = m_AnlgeSlider.sliderValue;
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
		if (m_VisibleCheck.isChecked != m_Data.m_Visible)
		{
			return true;
		}
		if (Float01ToIntAngle(m_AnlgeSlider.sliderValue) != m_Data.m_PivotAng)
		{
			return true;
		}
		return false;
	}

	private void Start()
	{
		pivot = null;
		if (!VCEditor.s_Active)
		{
			return;
		}
		VCESelectComponent selectComponentBrush = VCEditor.SelectComponentBrush;
		if (!(selectComponentBrush != null) || selectComponentBrush.m_Selection == null || selectComponentBrush.m_Selection.Count != 1)
		{
			return;
		}
		VCEComponentTool component = selectComponentBrush.m_Selection[0].m_Component;
		if (component != null)
		{
			pivot = component.GetComponent<VCPPivot>();
			if (pivot != null)
			{
				UISlider anlgeSlider = m_AnlgeSlider;
				anlgeSlider.onValueChange = (UISlider.OnValueChange)Delegate.Combine(anlgeSlider.onValueChange, new UISlider.OnValueChange(ChangeAngle));
				originValue01 = m_AnlgeSlider.sliderValue;
			}
		}
	}

	private void ChangeAngle(float value01)
	{
		pivot.Angle = Float01ToIntAngle(value01);
		m_AnlgeLable.text = ((int)pivot.Angle).ToString();
	}

	private void OnDisable()
	{
		if (pivot != null)
		{
			UISlider anlgeSlider = m_AnlgeSlider;
			anlgeSlider.onValueChange = (UISlider.OnValueChange)Delegate.Remove(anlgeSlider.onValueChange, new UISlider.OnValueChange(ChangeAngle));
			ChangeAngle(originValue01);
		}
		pivot = null;
	}
}
