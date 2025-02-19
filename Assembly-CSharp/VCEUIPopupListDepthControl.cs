using UnityEngine;

public class VCEUIPopupListDepthControl : MonoBehaviour
{
	private UIPopupList m_PopupList;

	public float m_OpenZ = -50f;

	public float m_CloseZ;

	private void Start()
	{
		m_PopupList = GetComponent<UIPopupList>();
	}

	private void Update()
	{
		if (!(m_PopupList == null))
		{
			Vector3 localPosition = base.transform.localPosition;
			if (m_PopupList.isOpen)
			{
				localPosition.z = m_OpenZ;
				Vector3 localPosition2 = m_PopupList.ChildPopupMenu.transform.localPosition;
				localPosition2.z = m_OpenZ;
				m_PopupList.ChildPopupMenu.transform.localPosition = localPosition2;
			}
			else
			{
				localPosition.z = m_CloseZ;
			}
			base.transform.localPosition = localPosition;
		}
	}
}
