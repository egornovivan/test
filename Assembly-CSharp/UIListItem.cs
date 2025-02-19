using System;
using UnityEngine;

public class UIListItem : MonoBehaviour
{
	[SerializeField]
	private UILabel m_TopicLabel;

	[SerializeField]
	private GameObject m_SelectGo;

	[SerializeField]
	private GameObject m_HoverGo;

	[SerializeField]
	private BoxCollider m_Collider;

	[SerializeField]
	private Color32 m_ErrorCol = new Color32(246, 13, 13, byte.MaxValue);

	private int m_ID;

	private bool m_Selected;

	private UIPanel m_CurPanel;

	public Action<UIListItem> SelectEvent;

	public int ID => m_ID;

	private void Awake()
	{
		ResetItem();
		UIEventListener uIEventListener = UIEventListener.Get(base.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
		{
			Select();
		});
		UIEventListener uIEventListener2 = UIEventListener.Get(base.gameObject);
		uIEventListener2.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener2.onHover, (UIEventListener.BoolDelegate)delegate(GameObject go, bool isHover)
		{
			Hover(isHover);
		});
	}

	private void Update()
	{
		if (null != m_CurPanel)
		{
			m_Collider.enabled = m_CurPanel.IsVisible(base.transform.position);
		}
	}

	private void UpdateTotpic(string topic)
	{
		m_TopicLabel.text = topic;
	}

	private void Hover(bool isHover)
	{
		if (m_Selected)
		{
			m_HoverGo.SetActive(value: false);
		}
		else
		{
			m_HoverGo.SetActive(isHover);
		}
	}

	private void GetCompt()
	{
		UIDraggablePanel uIDraggablePanel = NGUITools.FindInParents<UIDraggablePanel>(base.gameObject);
		if (null != uIDraggablePanel)
		{
			m_CurPanel = uIDraggablePanel.GetComponent<UIPanel>();
		}
	}

	public void ResetItem()
	{
		CancelSelect();
		Hover(isHover: false);
		m_TopicLabel.text = string.Empty;
	}

	public void UpdateInfo(int msgID, string topic)
	{
		m_ID = msgID;
		UpdateTotpic(topic);
		GetCompt();
	}

	public void UpdateInfo(int msgID, string topic, bool isPlayError)
	{
		m_ID = msgID;
		UpdateTotpic(topic);
		SetIsPlayError(isPlayError);
		GetCompt();
	}

	public void CancelSelect()
	{
		m_SelectGo.SetActive(value: false);
		m_Selected = false;
	}

	public void Select()
	{
		m_SelectGo.SetActive(value: true);
		Hover(isHover: false);
		if (SelectEvent != null)
		{
			SelectEvent(this);
		}
		m_Selected = true;
	}

	public void SetIsPlayError(bool isPlayError)
	{
		m_TopicLabel.color = ((!isPlayError) ? new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue) : m_ErrorCol);
	}
}
