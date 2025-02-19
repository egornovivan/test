using UnityEngine;

public class UIMouseMessage : MonoBehaviour
{
	public GameObject Target;

	public string MouseEnterFunc;

	public string MouseExitFunc;

	private bool m_PrevRayCast;

	private BoxCollider m_BoxCollider;

	private void Awake()
	{
		m_BoxCollider = base.gameObject.GetComponent<BoxCollider>();
	}

	private void Update()
	{
		if (Target == null || m_BoxCollider == null || UICamera.mainCamera == null)
		{
			return;
		}
		Ray ray = UICamera.mainCamera.ScreenPointToRay(Input.mousePosition);
		RaycastHit hitInfo;
		bool flag = m_BoxCollider.Raycast(ray, out hitInfo, 512f);
		if (m_PrevRayCast && !flag)
		{
			if (MouseExitFunc != string.Empty)
			{
				Target.SendMessage(MouseExitFunc);
			}
		}
		else if (!m_PrevRayCast && flag && MouseEnterFunc != string.Empty)
		{
			Target.SendMessage(MouseEnterFunc);
		}
		m_PrevRayCast = flag;
	}
}
