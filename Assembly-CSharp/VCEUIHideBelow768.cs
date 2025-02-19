using UnityEngine;

public class VCEUIHideBelow768 : MonoBehaviour
{
	public float m_minScreenHeight = 768f;

	private void Start()
	{
		if ((float)Screen.height < m_minScreenHeight)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void Update()
	{
		if ((float)Screen.height < m_minScreenHeight)
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
