using UnityEngine;

public class UIScrollBoxContrl : MonoBehaviour
{
	public UISprite m_BackgroundSprite;

	public UIPanel m_DraggablePanel;

	public UIScrollBar m_VertScrollBar;

	public UIScrollBar m_HorzScrollBar;

	public BoxCollider m_DragContent;

	public int m_Width;

	public int m_Height;

	public bool m_UpdateSize;

	private Vector3 m_TmpDragScale;

	private UIDraggablePanel m_Drag;

	private void Start()
	{
		Reposition();
		if ((bool)m_DraggablePanel)
		{
			m_Drag = m_DraggablePanel.GetComponent<UIDraggablePanel>();
			m_TmpDragScale = m_Drag.scale;
			m_DraggablePanel.transform.localPosition = Vector3.zero;
		}
	}

	private void Update()
	{
		if (m_UpdateSize)
		{
			Reposition();
		}
	}

	private void LateUpdate()
	{
		if (m_DraggablePanel != null && m_Drag != null)
		{
			m_Drag.scale = m_TmpDragScale;
			if (m_VertScrollBar != null && !m_VertScrollBar.background.gameObject.activeSelf)
			{
				Vector3 localPosition = m_DraggablePanel.transform.localPosition;
				localPosition.y = 0f;
				m_DraggablePanel.transform.localPosition = localPosition;
				m_Drag.scale.y = 0f;
			}
			if (m_HorzScrollBar != null && !m_HorzScrollBar.background.gameObject.activeSelf)
			{
				Vector3 localPosition2 = m_DraggablePanel.transform.localPosition;
				localPosition2.x = 0f;
				m_DraggablePanel.transform.localPosition = localPosition2;
				m_Drag.scale.x = 0f;
			}
		}
	}

	public void Reposition()
	{
		if (m_BackgroundSprite != null)
		{
			m_BackgroundSprite.transform.localScale = new Vector3(m_Width + 20, m_Height + 20, 1f);
		}
		if (m_DraggablePanel != null)
		{
			m_DraggablePanel.clipRange = new Vector4((float)m_Width * 0.5f + 10f - m_DraggablePanel.transform.localPosition.x, (float)(-m_Height) * 0.5f - 10f - m_DraggablePanel.transform.localPosition.y, m_Width + 5, m_Height + 5);
		}
		if (m_VertScrollBar != null)
		{
			m_VertScrollBar.transform.localPosition = new Vector3(m_Width + 5, -20f, 0f);
			Vector3 localScale = m_VertScrollBar.background.transform.localScale;
			m_VertScrollBar.background.transform.localScale = new Vector3(localScale.x, m_Height - 20, 1f);
		}
		if (m_HorzScrollBar != null)
		{
			m_HorzScrollBar.transform.localPosition = new Vector3(20f, -m_Height - 25, 0f);
			Vector3 localScale2 = m_HorzScrollBar.background.transform.localScale;
			m_HorzScrollBar.background.transform.localScale = new Vector3(m_Width - 20, localScale2.y, 1f);
		}
	}
}
