using UnityEngine;

public class VCEUIPosControl : MonoBehaviour
{
	public float m_BottomDist;

	private void Start()
	{
		Vector3 localPosition = base.transform.localPosition;
		localPosition.y = m_BottomDist - (float)Screen.height;
		base.transform.localPosition = localPosition;
	}

	private void Update()
	{
		Vector3 localPosition = base.transform.localPosition;
		localPosition.y = m_BottomDist - (float)Screen.height;
		base.transform.localPosition = localPosition;
	}
}
