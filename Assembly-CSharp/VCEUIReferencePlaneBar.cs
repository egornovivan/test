using UnityEngine;

public class VCEUIReferencePlaneBar : MonoBehaviour
{
	public UISlider m_Slider;

	public UILabel m_Label;

	private void Start()
	{
	}

	private void Update()
	{
		if (VCEditor.DocumentOpen())
		{
			int y = VCEditor.s_Scene.m_Setting.m_EditorSize.y;
			m_Slider.numberOfSteps = y + 1;
			int yRef = Mathf.RoundToInt(m_Slider.sliderValue * (float)y);
			m_Label.text = "Y = " + yRef;
			VCERefPlane.YRef = yRef;
		}
	}

	public void Reset()
	{
		m_Slider.sliderValue = 0f;
	}
}
