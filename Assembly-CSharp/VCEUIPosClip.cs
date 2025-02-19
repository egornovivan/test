using UnityEngine;

public class VCEUIPosClip : MonoBehaviour
{
	public bool m_Left = true;

	public float m_XMin;

	public float m_XMax;

	public float m_YMin;

	public float m_YMax;

	private void Start()
	{
	}

	private void LateUpdate()
	{
		if (m_Left)
		{
			Vector3 localPosition = base.transform.localPosition;
			localPosition.x = Mathf.Clamp(localPosition.x, m_XMin, (float)Screen.width - m_XMax);
			localPosition.y = Mathf.Clamp(localPosition.y, m_YMax - (float)Screen.height, 0f - m_YMin);
			base.transform.localPosition = localPosition;
		}
		else
		{
			Vector3 localPosition2 = base.transform.localPosition;
			localPosition2.x = Mathf.Clamp(localPosition2.x, 0f - ((float)Screen.width - m_XMin), 0f - m_XMax);
			localPosition2.y = Mathf.Clamp(localPosition2.y, m_YMax - (float)Screen.height, 0f - m_YMin);
			base.transform.localPosition = localPosition2;
		}
	}
}
