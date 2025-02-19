using UnityEngine;

public class VCEUITabControl : MonoBehaviour
{
	public UICheckbox m_Tab;

	public GameObject m_Page;

	private void Start()
	{
	}

	private void Update()
	{
		m_Page.SetActive(m_Tab.isChecked);
	}
}
