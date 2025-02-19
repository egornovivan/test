using UnityEngine;

public class VCEUIBrushItem : MonoBehaviour
{
	public VCEUIBrushGroup m_Group;

	public bool m_IsGeneralBrush;

	public GameObject m_BrushPrefab;

	public GameObject m_BrushInstance;

	private void Start()
	{
	}

	private void Update()
	{
		if (m_BrushPrefab == null || !VCEditor.DocumentOpen())
		{
			return;
		}
		UICheckbox component = GetComponent<UICheckbox>();
		if (!(component != null))
		{
			return;
		}
		if (component.isChecked)
		{
			if (m_BrushInstance == null)
			{
				m_BrushInstance = Object.Instantiate(m_BrushPrefab);
				m_BrushInstance.transform.parent = VCEditor.Instance.m_BrushGroup.transform;
				m_BrushInstance.transform.localPosition = Vector3.zero;
				m_BrushInstance.transform.localRotation = Quaternion.identity;
				m_BrushInstance.transform.localScale = Vector3.one;
				m_BrushInstance.SetActive(value: true);
			}
		}
		else if (m_BrushInstance != null)
		{
			Object.Destroy(m_BrushInstance);
			m_BrushInstance = null;
		}
	}

	private void OnEnable()
	{
		UICheckbox component = GetComponent<UICheckbox>();
		if (component != null)
		{
			component.isChecked = component.startsChecked;
		}
	}

	private void OnDisable()
	{
		UICheckbox component = GetComponent<UICheckbox>();
		if (component != null)
		{
			component.isChecked = component.startsChecked;
		}
		if (m_BrushInstance != null)
		{
			Object.Destroy(m_BrushInstance);
			m_BrushInstance = null;
		}
	}

	private void OnBrushSelect(bool isChecked)
	{
		if (isChecked && m_IsGeneralBrush)
		{
			m_Group.m_LastGeneralBrush = this;
		}
	}
}
