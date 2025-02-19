using System;
using UnityEngine;

public class VCEUIMirrorWnd : MonoBehaviour
{
	public GameObject m_Window;

	public UICheckbox m_SwitchX;

	public UICheckbox m_SwitchY;

	public UICheckbox m_SwitchZ;

	public UICheckbox m_SwitchXY;

	public UICheckbox m_SwitchYZ;

	public UICheckbox m_SwitchZX;

	public UICheckbox m_SwitchXYZ;

	public GameObject m_ConnXY;

	public GameObject m_ConnYZ;

	public GameObject m_ConnZX;

	public UISprite m_TriangleX;

	public UISprite m_TriangleY;

	public UISprite m_TriangleZ;

	public GameObject m_DisableX;

	public GameObject m_DisableY;

	public GameObject m_DisableZ;

	public GameObject m_ValGroupX;

	public GameObject m_ValGroupY;

	public GameObject m_ValGroupZ;

	public UIInput m_InputX;

	public UIInput m_InputY;

	public UIInput m_InputZ;

	public UILabel m_ImageCntLabel;

	public GameObject m_BuffGroup;

	public UILabel m_DescLabel;

	public UITweener m_GlowIcon;

	private bool m_lastXSelected;

	private bool m_lastYSelected;

	private bool m_lastZSelected;

	private string m_revertXStr = string.Empty;

	private string m_revertYStr = string.Empty;

	private string m_revertZStr = string.Empty;

	private Vector3 m_Vector;

	private int m_ModifierPressed;

	private Vector3 m_ModifierIncreasement = Vector3.zero;

	public Bounds ValueBound
	{
		get
		{
			Bounds result = default(Bounds);
			result.SetMinMax(new Vector3(0.45f, 0.45f, 0.45f), VCEditor.s_Scene.m_Setting.m_EditorSize.ToVector3() - Vector3.one * 0.45f);
			return result;
		}
	}

	public bool WindowVisible()
	{
		return m_Window.activeInHierarchy;
	}

	public void ShowWindow()
	{
		m_Vector = new Vector3(VCEditor.s_Mirror.m_PosX, VCEditor.s_Mirror.m_PosY, VCEditor.s_Mirror.m_PosZ);
		m_SwitchX.isChecked = false;
		m_SwitchY.isChecked = false;
		m_SwitchZ.isChecked = false;
		m_SwitchXY.isChecked = false;
		m_SwitchYZ.isChecked = false;
		m_SwitchZX.isChecked = false;
		m_SwitchXYZ.isChecked = false;
		if (VCEditor.s_Mirror.m_XPlane)
		{
			m_SwitchX.isChecked = true;
		}
		if (VCEditor.s_Mirror.m_YPlane)
		{
			m_SwitchY.isChecked = true;
		}
		if (VCEditor.s_Mirror.m_ZPlane)
		{
			m_SwitchZ.isChecked = true;
		}
		if (VCEditor.s_Mirror.m_XYAxis)
		{
			m_SwitchXY.isChecked = true;
		}
		if (VCEditor.s_Mirror.m_YZAxis)
		{
			m_SwitchYZ.isChecked = true;
		}
		if (VCEditor.s_Mirror.m_ZXAxis)
		{
			m_SwitchZX.isChecked = true;
		}
		if (VCEditor.s_Mirror.m_Point)
		{
			m_SwitchXYZ.isChecked = true;
		}
		ApplyVectorToInput();
		m_Window.SetActive(value: true);
	}

	public void HideWindow()
	{
		m_Window.SetActive(value: false);
	}

	public void UpdateWindow()
	{
		if (VCEditor.s_Mirror.m_Mask == 0)
		{
			HideWindow();
		}
	}

	public void Reset()
	{
	}

	private void Start()
	{
	}

	private void OnDestroy()
	{
	}

	private void Update()
	{
		UpdateGroups();
		UpdateInputFields();
		UpdateModifier();
		UpdateMirrorSetting();
		UpdateBuffUI();
		UpdateWindow();
	}

	private void GetVectorFromInput()
	{
		if (!m_InputX.selected)
		{
			try
			{
				m_Vector.x = Convert.ToSingle(m_InputX.text);
			}
			catch (Exception)
			{
				m_Vector.x = 0f;
			}
		}
		if (!m_InputY.selected)
		{
			try
			{
				m_Vector.y = Convert.ToSingle(m_InputY.text);
			}
			catch (Exception)
			{
				m_Vector.y = 0f;
			}
		}
		if (!m_InputZ.selected)
		{
			try
			{
				m_Vector.z = Convert.ToSingle(m_InputZ.text);
			}
			catch (Exception)
			{
				m_Vector.z = 0f;
			}
		}
		ValidateVector();
	}

	private void ApplyVectorToInput()
	{
		ValidateVector();
		m_InputX.text = m_Vector.x.ToString("0.0");
		m_InputY.text = m_Vector.y.ToString("0.0");
		m_InputZ.text = m_Vector.z.ToString("0.0");
	}

	private void ValidateVector()
	{
		m_Vector = VCUtils.ClampInBound(m_Vector, ValueBound);
		m_Vector *= 2f;
		m_Vector.x = Mathf.Round(m_Vector.x);
		m_Vector.y = Mathf.Round(m_Vector.y);
		m_Vector.z = Mathf.Round(m_Vector.z);
		m_Vector *= 0.5f;
	}

	private void UpdateInputFields()
	{
		if (m_InputX.selected && !m_lastXSelected)
		{
			m_revertXStr = m_InputX.text;
			m_InputX.text = string.Empty;
			m_lastXSelected = m_InputX.selected;
		}
		else if (!m_InputX.selected && m_lastXSelected)
		{
			if (m_InputX.text.Trim().Length == 0)
			{
				m_InputX.text = m_revertXStr;
			}
			m_revertXStr = string.Empty;
			m_lastXSelected = m_InputX.selected;
			GetVectorFromInput();
			m_InputX.text = m_Vector.x.ToString("0.0");
		}
		if (m_InputY.selected && !m_lastYSelected)
		{
			m_revertYStr = m_InputY.text;
			m_InputY.text = string.Empty;
			m_lastYSelected = m_InputY.selected;
		}
		else if (!m_InputY.selected && m_lastYSelected)
		{
			if (m_InputY.text.Trim().Length == 0)
			{
				m_InputY.text = m_revertYStr;
			}
			m_revertYStr = string.Empty;
			m_lastYSelected = m_InputY.selected;
			GetVectorFromInput();
			m_InputY.text = m_Vector.y.ToString("0.0");
		}
		if (m_InputZ.selected && !m_lastZSelected)
		{
			m_revertZStr = m_InputZ.text;
			m_InputZ.text = string.Empty;
			m_lastZSelected = m_InputZ.selected;
		}
		else if (!m_InputZ.selected && m_lastZSelected)
		{
			if (m_InputZ.text.Trim().Length == 0)
			{
				m_InputZ.text = m_revertZStr;
			}
			m_revertZStr = string.Empty;
			m_lastZSelected = m_InputZ.selected;
			GetVectorFromInput();
			m_InputZ.text = m_Vector.z.ToString("0.0");
		}
	}

	private void UpdateModifier()
	{
		if (m_ModifierPressed <= 0)
		{
			return;
		}
		if (m_ModifierPressed < 25)
		{
			if (m_ModifierPressed == 1)
			{
				DoModifyVector();
			}
		}
		else if (m_ModifierPressed < 120)
		{
			if (m_ModifierPressed % 5 == 0)
			{
				DoModifyVector();
			}
		}
		else
		{
			DoModifyVector();
		}
		m_ModifierPressed++;
	}

	private void DoModifyVector()
	{
		m_Vector += m_ModifierIncreasement * 0.5f;
		ApplyVectorToInput();
	}

	private void UpdateGroups()
	{
		m_ValGroupX.SetActive(m_SwitchX.isChecked);
		m_ValGroupY.SetActive(m_SwitchY.isChecked);
		m_ValGroupZ.SetActive(m_SwitchZ.isChecked);
		m_DisableX.SetActive(!m_SwitchX.isChecked);
		m_DisableY.SetActive(!m_SwitchY.isChecked);
		m_DisableZ.SetActive(!m_SwitchZ.isChecked);
		m_ConnXY.SetActive(m_SwitchXY.isChecked);
		m_ConnYZ.SetActive(m_SwitchYZ.isChecked);
		m_ConnZX.SetActive(m_SwitchZX.isChecked);
		Color color = ((!m_SwitchXYZ.isChecked) ? Color.white : new Color(0f, 0.5f, 1f, 1f));
		m_TriangleX.color = color;
		m_TriangleY.color = color;
		m_TriangleZ.color = color;
	}

	private void UpdateMirrorSetting()
	{
		if (WindowVisible())
		{
			VCEditor.s_Mirror.m_PosX = m_Vector.x;
			VCEditor.s_Mirror.m_PosY = m_Vector.y;
			VCEditor.s_Mirror.m_PosZ = m_Vector.z;
			VCEditor.s_Mirror.m_XPlane = m_SwitchX.isChecked;
			VCEditor.s_Mirror.m_YPlane = m_SwitchY.isChecked;
			VCEditor.s_Mirror.m_ZPlane = m_SwitchZ.isChecked;
			VCEditor.s_Mirror.m_XYAxis = m_SwitchXY.isChecked;
			VCEditor.s_Mirror.m_YZAxis = m_SwitchYZ.isChecked;
			VCEditor.s_Mirror.m_ZXAxis = m_SwitchZX.isChecked;
			VCEditor.s_Mirror.m_Point = m_SwitchXYZ.isChecked;
			VCEditor.s_Mirror.Validate();
			if (VCEditor.s_Mirror.Enabled)
			{
				m_ImageCntLabel.text = Mathf.Pow(2f, VCEditor.s_Mirror.MirrorCount).ToString("0") + "x";
			}
			else
			{
				m_ImageCntLabel.text = "No mirror".ToLocalizationString();
			}
		}
	}

	private void UpdateBuffUI()
	{
		if (VCEditor.s_Mirror.Enabled)
		{
			m_BuffGroup.SetActive(value: true);
			if (VCEditor.s_Mirror.Enabled_Masked)
			{
				m_DescLabel.text = string.Empty;
				if (VCEditor.s_Mirror.XPlane_Masked)
				{
					UILabel descLabel = m_DescLabel;
					descLabel.text = descLabel.text + "X-" + "Plane".ToLocalizationString() + " ";
				}
				if (VCEditor.s_Mirror.YPlane_Masked)
				{
					UILabel descLabel2 = m_DescLabel;
					descLabel2.text = descLabel2.text + "Y-" + "Plane".ToLocalizationString() + " ";
				}
				if (VCEditor.s_Mirror.ZPlane_Masked)
				{
					UILabel descLabel3 = m_DescLabel;
					descLabel3.text = descLabel3.text + "Z-" + "Plane".ToLocalizationString() + " ";
				}
				if (VCEditor.s_Mirror.XYAxis_Masked)
				{
					UILabel descLabel4 = m_DescLabel;
					descLabel4.text = descLabel4.text + "XY-" + "Axis".ToLocalizationString() + " ";
				}
				if (VCEditor.s_Mirror.YZAxis_Masked)
				{
					UILabel descLabel5 = m_DescLabel;
					descLabel5.text = descLabel5.text + "YZ-" + "Axis".ToLocalizationString() + " ";
				}
				if (VCEditor.s_Mirror.ZXAxis_Masked)
				{
					UILabel descLabel6 = m_DescLabel;
					descLabel6.text = descLabel6.text + "ZX-" + "Axis".ToLocalizationString() + " ";
				}
				if (VCEditor.s_Mirror.Point_Masked)
				{
					UILabel descLabel7 = m_DescLabel;
					descLabel7.text = descLabel7.text + "Point".ToLocalizationString() + " ";
				}
				UILabel descLabel8 = m_DescLabel;
				descLabel8.text = descLabel8.text + "Symmetry".ToLocalizationString() + " ";
				UILabel descLabel9 = m_DescLabel;
				descLabel9.text = descLabel9.text + "(" + Mathf.Pow(2f, VCEditor.s_Mirror.MirrorCount_Masked).ToString("0") + "x)";
			}
			else
			{
				m_DescLabel.text = "[C0C040]< " + "The mirror has been temporarily disabled".ToLocalizationString() + ".>[-]";
			}
		}
		else
		{
			m_BuffGroup.SetActive(value: false);
		}
	}

	private void OnCreateMirrorClick()
	{
	}

	private void OnXClick(bool active)
	{
		if (!active)
		{
			m_SwitchXY.isChecked = false;
			m_SwitchZX.isChecked = false;
			m_SwitchXYZ.isChecked = false;
		}
		ApplyVectorToInput();
		m_GlowIcon.Reset();
		m_GlowIcon.Play(forward: true);
	}

	private void OnYClick(bool active)
	{
		if (!active)
		{
			m_SwitchXY.isChecked = false;
			m_SwitchYZ.isChecked = false;
			m_SwitchXYZ.isChecked = false;
		}
		ApplyVectorToInput();
		m_GlowIcon.Reset();
		m_GlowIcon.Play(forward: true);
	}

	private void OnZClick(bool active)
	{
		if (!active)
		{
			m_SwitchYZ.isChecked = false;
			m_SwitchZX.isChecked = false;
			m_SwitchXYZ.isChecked = false;
		}
		ApplyVectorToInput();
		m_GlowIcon.Reset();
		m_GlowIcon.Play(forward: true);
	}

	private void OnXYClick(bool active)
	{
		if (active)
		{
			m_SwitchX.isChecked = true;
			m_SwitchY.isChecked = true;
			m_SwitchYZ.isChecked = false;
			m_SwitchZX.isChecked = false;
			m_SwitchXYZ.isChecked = false;
		}
		m_GlowIcon.Reset();
		m_GlowIcon.Play(forward: true);
	}

	private void OnYZClick(bool active)
	{
		if (active)
		{
			m_SwitchY.isChecked = true;
			m_SwitchZ.isChecked = true;
			m_SwitchXY.isChecked = false;
			m_SwitchZX.isChecked = false;
			m_SwitchXYZ.isChecked = false;
		}
		m_GlowIcon.Reset();
		m_GlowIcon.Play(forward: true);
	}

	private void OnZXClick(bool active)
	{
		if (active)
		{
			m_SwitchZ.isChecked = true;
			m_SwitchX.isChecked = true;
			m_SwitchXY.isChecked = false;
			m_SwitchYZ.isChecked = false;
			m_SwitchXYZ.isChecked = false;
		}
		m_GlowIcon.Reset();
		m_GlowIcon.Play(forward: true);
	}

	private void OnXYZClick(bool active)
	{
		if (active)
		{
			m_SwitchX.isChecked = true;
			m_SwitchY.isChecked = true;
			m_SwitchZ.isChecked = true;
			m_SwitchXY.isChecked = false;
			m_SwitchYZ.isChecked = false;
			m_SwitchZX.isChecked = false;
		}
		m_GlowIcon.Reset();
		m_GlowIcon.Play(forward: true);
	}

	private void OnHideClick()
	{
		HideWindow();
	}

	private void OnModifierRelease()
	{
		m_ModifierPressed = 0;
		m_ModifierIncreasement = Vector3.zero;
	}

	private void OnXPlusPress()
	{
		m_ModifierPressed = 1;
		m_ModifierIncreasement = Vector3.right;
	}

	private void OnXMinusPress()
	{
		m_ModifierPressed = 1;
		m_ModifierIncreasement = Vector3.left;
	}

	private void OnYPlusPress()
	{
		m_ModifierPressed = 1;
		m_ModifierIncreasement = Vector3.up;
	}

	private void OnYMinusPress()
	{
		m_ModifierPressed = 1;
		m_ModifierIncreasement = Vector3.down;
	}

	private void OnZPlusPress()
	{
		m_ModifierPressed = 1;
		m_ModifierIncreasement = Vector3.forward;
	}

	private void OnZMinusPress()
	{
		m_ModifierPressed = 1;
		m_ModifierIncreasement = Vector3.back;
	}

	private void OnMirrorBuffClick()
	{
		if (!WindowVisible())
		{
			ShowWindow();
		}
	}
}
