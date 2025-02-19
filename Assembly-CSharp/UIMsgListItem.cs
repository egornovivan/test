using System;
using UnityEngine;

public class UIMsgListItem : MonoBehaviour
{
	[SerializeField]
	private UILabel m_TopicLabel;

	[SerializeField]
	private GameObject m_SelectGo;

	[SerializeField]
	private GameObject m_HoverGo;

	private int m_MsgID;

	private bool m_Selected;

	public Action<UIMsgListItem> SelectEvent;

	public int MsgID => m_MsgID;

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

	private void UpdateTotpic(string topic)
	{
		m_TopicLabel.text = topic;
	}

	private void Select()
	{
		m_SelectGo.SetActive(value: true);
		Hover(isHover: false);
		if (SelectEvent != null)
		{
			SelectEvent(this);
		}
		m_Selected = true;
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

	public void ResetItem()
	{
		CancelSelect();
		Hover(isHover: false);
		m_TopicLabel.text = string.Empty;
	}

	public void UpdateInfo(int msgID, string topic)
	{
		m_MsgID = msgID;
		UpdateTotpic(topic);
	}

	public void CancelSelect()
	{
		m_SelectGo.SetActive(value: false);
		m_Selected = false;
	}
}
