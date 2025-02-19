using UnityEngine;

public class VCEUISizeControl : MonoBehaviour
{
	public float m_YReserve;

	private void Start()
	{
		Vector3 localScale = base.transform.localScale;
		localScale.y = (float)Screen.height - m_YReserve;
		base.transform.localScale = localScale;
	}

	private void Update()
	{
		Vector3 localScale = base.transform.localScale;
		localScale.y = (float)Screen.height - m_YReserve;
		base.transform.localScale = localScale;
	}
}
