using UnityEngine;

public class GLAirbrushCursor : GLBehaviour
{
	public VCEFreeAirbrush m_Airbrush;

	public void Update()
	{
	}

	private void OnGUI()
	{
		GUI.skin = VCEditor.Instance.m_GUISkin;
		if (m_Airbrush.DisplayCursor)
		{
			GUI.color = m_Airbrush.m_UIColor;
			GUI.Label(new Rect(Input.mousePosition.x - 16f, (float)Screen.height - Input.mousePosition.y - 16f, 32f, 32f), string.Empty, (!m_Airbrush.m_Eraser) ? "AirbrushPaint" : "AirbrushEraser");
		}
	}

	public override void OnGL()
	{
	}
}
