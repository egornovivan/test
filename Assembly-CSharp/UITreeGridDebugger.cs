using UnityEngine;

public class UITreeGridDebugger : MonoBehaviour
{
	public string m_NodeRes;

	public UITreeGrid m_Target;

	public Transform m_ChildrenGroup;

	public bool m_AddChild;

	private void Start()
	{
	}

	private void Update()
	{
		if (m_AddChild)
		{
			m_AddChild = false;
			AddChild();
		}
	}

	private void AddChild()
	{
		GameObject gameObject = Object.Instantiate(Resources.Load(m_NodeRes) as GameObject);
		gameObject.transform.parent = m_ChildrenGroup;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Quaternion.identity;
		gameObject.transform.localScale = Vector3.one;
		m_Target.m_Children.Add(gameObject.GetComponent<UITreeGrid>());
		m_Target.transform.root.GetComponentInChildren<UITreeGrid>().Reposition();
	}
}
