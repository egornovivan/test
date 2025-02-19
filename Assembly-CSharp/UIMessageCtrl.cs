using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIMessageCtrl : UIBaseWidget
{
	private Queue<UIListItem> m_MsgListItemPool = new Queue<UIListItem>();

	private List<UIListItem> m_CurListItems = new List<UIListItem>();

	[SerializeField]
	private UIListItem m_ListItemPrefab;

	[SerializeField]
	private UIGrid m_ListGrid;

	[SerializeField]
	private UIScrollBar m_ListScrollBar;

	[SerializeField]
	private UILabel m_FormLabel;

	[SerializeField]
	private UILabel m_ToLabel;

	[SerializeField]
	private UILabel m_TopicLabel;

	[SerializeField]
	private UILabel m_TitleLabel;

	[SerializeField]
	private UILabel m_ContentLabel;

	[SerializeField]
	private UILabel m_EndLabel;

	[SerializeField]
	private UITable m_ContentTable;

	[SerializeField]
	private UIScrollBar m_ContentScrollBar;

	[SerializeField]
	private UILabel m_DateLabel;

	[SerializeField]
	private UIInput m_InfoInput;

	[SerializeField]
	private N_ImageButton m_SeedBtn;

	private UIListItem m_BackupSelectItem;

	public Action<string> SeedMsgEvent;

	private void Awake()
	{
		UIEventListener uIEventListener = UIEventListener.Get(m_SeedBtn.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
		{
			SeedBtnEvent();
		});
	}

	public override void OnCreate()
	{
		m_MsgListItemPool.Clear();
		m_CurListItems.Clear();
		MessageData.AddMsgEvent = (Action<int>)Delegate.Combine(MessageData.AddMsgEvent, new Action<int>(AddMsgByData));
	}

	protected override void InitWindow()
	{
		base.InitWindow();
	}

	public override void Show()
	{
		base.Show();
		ResetWnd();
		UpdateMsgList();
	}

	protected override void OnHide()
	{
		base.OnHide();
		RecycleMsgListItem();
	}

	protected override void OnClose()
	{
		base.OnClose();
		RecycleMsgListItem();
	}

	private void AddMsgByData(int msgID)
	{
		if (null != this && base.gameObject.activeInHierarchy)
		{
			AddNewListItem(msgID);
			m_ListGrid.repositionNow = true;
		}
	}

	private void UpdateMsgList()
	{
		if (MessageData.ActiveMsgDataIDs == null || MessageData.ActiveMsgDataIDs.Count <= 0)
		{
			return;
		}
		int i;
		for (i = 0; i < MessageData.ActiveMsgDataIDs.Count; i++)
		{
			if (!m_CurListItems.Any((UIListItem a) => a.ID == MessageData.ActiveMsgDataIDs[i]))
			{
				AddNewListItem(MessageData.ActiveMsgDataIDs[i]);
			}
		}
		m_ListGrid.repositionNow = true;
		if (m_CurListItems != null && m_CurListItems.Count > 0)
		{
			m_CurListItems[0].Select();
		}
	}

	private void AddNewListItem(int msgID)
	{
		UIListItem newMsgListItem = GetNewMsgListItem();
		newMsgListItem.UpdateInfo(msgID, MessageData.AllMsgDataDic[msgID].Topic);
		newMsgListItem.SelectEvent = MegListItemSelectEvent;
		m_CurListItems.Add(newMsgListItem);
	}

	private void MegListItemSelectEvent(UIListItem item)
	{
		if (!(item == m_BackupSelectItem))
		{
			if (null != m_BackupSelectItem)
			{
				m_BackupSelectItem.CancelSelect();
			}
			m_BackupSelectItem = item;
			if (MessageData.ActiveMsgDataIDs.Contains(item.ID))
			{
				MessageData messageData = MessageData.AllMsgDataDic[item.ID];
				UpdateForm(messageData.Form);
				UpdateTo(messageData.To);
				UpdateTopic(messageData.Topic);
				UpdateContent(messageData.Title, messageData.Content, messageData.End);
				UpdateDate(messageData.Date);
			}
			else
			{
				ResetAllLabel();
			}
		}
	}

	private void ResetAllLabel()
	{
		UpdateForm(string.Empty);
		UpdateTo(string.Empty);
		UpdateTopic(string.Empty);
		UpdateContent(string.Empty, string.Empty, string.Empty);
		UpdateDate(string.Empty);
	}

	private void UpdateForm(string from)
	{
		m_FormLabel.text = from;
	}

	private void UpdateTo(string to)
	{
		m_ToLabel.text = to;
	}

	private void UpdateTopic(string topic)
	{
		m_TopicLabel.text = topic;
	}

	private void UpdateContent(string title, string content, string end)
	{
		UpdateContentTitle(title);
		if (content == "0" || string.IsNullOrEmpty(content))
		{
			m_ContentLabel.gameObject.SetActive(value: false);
		}
		else
		{
			m_ContentLabel.gameObject.SetActive(value: true);
			content = content.Replace("<\\n>", "\n");
			m_ContentLabel.text = content;
		}
		UpdateContentEnd(end);
		m_ContentTable.repositionNow = true;
		m_ContentScrollBar.scrollValue = 0f;
	}

	private void UpdateContentTitle(string title)
	{
		if (title == "0" || string.IsNullOrEmpty(title))
		{
			m_TitleLabel.gameObject.SetActive(value: false);
			return;
		}
		m_TitleLabel.gameObject.SetActive(value: true);
		title = title.Replace("<\\n>", "\n");
		m_TitleLabel.text = title;
	}

	private void UpdateContentEnd(string end)
	{
		if (end == "0" || string.IsNullOrEmpty(end))
		{
			m_EndLabel.gameObject.SetActive(value: false);
			return;
		}
		m_EndLabel.gameObject.SetActive(value: true);
		if (end.Contains("<\\right>"))
		{
			m_EndLabel.pivot = UIWidget.Pivot.TopRight;
			m_EndLabel.transform.localPosition = new Vector3(550f, 0f, 0f);
			end = end.Replace("<\\right>", string.Empty);
		}
		else
		{
			m_EndLabel.pivot = UIWidget.Pivot.TopLeft;
			m_EndLabel.transform.localPosition = new Vector3(0f, 0f, 0f);
			if (end.Contains("<\\left>"))
			{
				end = end.Replace("<\\left>", string.Empty);
			}
		}
		end = end.Replace("<\\n>", "\n");
		m_EndLabel.text = end;
	}

	private void UpdateDate(string date)
	{
		m_DateLabel.text = date;
	}

	private UIListItem GetNewMsgListItem()
	{
		UIListItem uIListItem = null;
		if (m_MsgListItemPool.Count > 0)
		{
			uIListItem = m_MsgListItemPool.Dequeue();
			uIListItem.ResetItem();
			uIListItem.gameObject.SetActive(value: true);
		}
		else
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(m_ListItemPrefab.gameObject);
			gameObject.transform.parent = m_ListGrid.gameObject.transform;
			gameObject.transform.localRotation = Quaternion.identity;
			gameObject.transform.localScale = Vector3.one;
			gameObject.transform.localPosition = new Vector3(0f, 0f, 0f);
			uIListItem = gameObject.GetComponent<UIListItem>();
		}
		return uIListItem;
	}

	private void ResetWnd()
	{
		m_ListScrollBar.scrollValue = 0f;
		m_ContentScrollBar.scrollValue = 0f;
		ResetAllLabel();
	}

	private void RecycleMsgListItem()
	{
		if (m_CurListItems != null && m_CurListItems.Count > 0)
		{
			for (int i = 0; i < m_CurListItems.Count; i++)
			{
				m_CurListItems[i].gameObject.SetActive(value: false);
				m_MsgListItemPool.Enqueue(m_CurListItems[i]);
			}
			m_CurListItems.Clear();
		}
	}

	private void SeedBtnEvent()
	{
		string text = m_InfoInput.text;
		text = text.Trim();
		if (!string.IsNullOrEmpty(text))
		{
			if (SeedMsgEvent != null)
			{
				SeedMsgEvent(text);
			}
			if (text == $"OpenRadio{DateTime.Now.Month}{DateTime.Now.Day}" && null != GameUI.Instance && null != GameUI.Instance.mPhoneWnd)
			{
				GameUI.Instance.mPhoneWnd.OpenRadio = true;
			}
			m_InfoInput.text = string.Empty;
		}
	}

	private void TestInfo()
	{
		if (Application.isEditor)
		{
			MessageData.AllMsgDataDic.Add(1000, new MessageData(1000, new int[1] { 501 }, "TestTopic0", "star", "sun", "2016.06.12", "This is a title", "This is a Content", "<\\right>This is a end!<\\n> This is a second line"));
			MessageData.AllMsgDataDic.Add(1001, new MessageData(1001, new int[1] { 502 }, "TestTopic1", "star", "sun", "2016.06.12", "This is a title", "This is a Content", "This is a end! <\\n>Is a Two Lines"));
			MessageData.ActiveMsgDataIDs.Add(1000);
			MessageData.ActiveMsgDataIDs.Add(1001);
		}
	}
}
