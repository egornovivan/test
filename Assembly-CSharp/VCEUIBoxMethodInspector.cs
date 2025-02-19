using UnityEngine;

public class VCEUIBoxMethodInspector : VCEUIInspector
{
	public VCESelectMethod_Box m_SelectMethod;

	public UILabel m_StatusLabel;

	public UILabel m_DepthLabel;

	public UISlider m_FeatherSlider;

	public UILabel m_FeatherValueLabel;

	public UICheckbox m_FeatherDepthCheck;

	public UICheckbox m_MaterialSelectCheck;

	private void Start()
	{
		m_FeatherSlider.sliderValue = (float)VCESelectMethod_Box.s_RecentFeatherLength / 20f;
		m_FeatherValueLabel.text = VCESelectMethod_Box.s_RecentFeatherLength.ToString();
		m_FeatherDepthCheck.isChecked = !VCESelectMethod_Box.s_RecentPlaneFeather;
		m_MaterialSelectCheck.isChecked = VCESelectMethod_Box.s_RecentMaterialSelect;
		m_DepthLabel.text = "Select depth".ToLocalizationString() + ": " + VCESelectMethod_Box.s_RecentDepth + " (+/-)";
	}

	private void Update()
	{
		if (!(m_SelectMethod == null))
		{
			IntVector3 intVector = m_SelectMethod.SelectingSize();
			if (intVector.x * intVector.y > 0)
			{
				m_StatusLabel.gameObject.SetActive(value: true);
			}
			else
			{
				m_StatusLabel.gameObject.SetActive(value: false);
			}
			m_StatusLabel.text = intVector.x + " x " + intVector.y;
			m_DepthLabel.text = "Select depth".ToLocalizationString() + ": " + intVector.z + " (+/-)";
			m_FeatherValueLabel.text = Mathf.RoundToInt(m_FeatherSlider.sliderValue * 20f).ToString();
			m_SelectMethod.m_FeatherLength = Mathf.RoundToInt(m_FeatherSlider.sliderValue * 20f);
			if (m_SelectMethod.m_FeatherLength < 1)
			{
				m_FeatherDepthCheck.gameObject.SetActive(value: false);
			}
			else
			{
				m_FeatherDepthCheck.gameObject.SetActive(value: true);
			}
			m_SelectMethod.m_PlaneFeather = !m_FeatherDepthCheck.isChecked;
			m_SelectMethod.m_MaterialSelectChange = m_SelectMethod.m_MaterialSelect ^ m_MaterialSelectCheck.isChecked;
			m_SelectMethod.m_MaterialSelect = m_MaterialSelectCheck.isChecked;
		}
	}
}
