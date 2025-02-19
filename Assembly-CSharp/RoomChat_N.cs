using System;
using System.Collections.Generic;
using UnityEngine;

public class RoomChat_N : MonoBehaviour
{
	public const string LANGE_CN = "[lang:cn]";

	public const string LANGE_OTHER = "[lang:other]";

	public CommonChatItem_N m_ChatItemPrefab;

	public UITable ChatListTable;

	public UIPanel ClipPanel;

	public UIScrollBar ChatScrollBar;

	public UIInput ChatInput;

	public UIButton SendBtn;

	public BoxCollider DragContentCollider;

	public Action<string> SendMsgEvent;

	public int LineWidth = 584;

	private int mMaxMsgCount = 300;

	private int mDeleteMsgCount = 100;

	private List<CommonChatItem_N> m_CurChatItems = new List<CommonChatItem_N>();

	private Queue<CommonChatItem_N> m_ChatItemsPool = new Queue<CommonChatItem_N>();

	private int m_RepositionCount;

	private void OnEnable()
	{
		UIEventListener.Get(SendBtn.gameObject).onClick = delegate
		{
			SeedMsg();
		};
	}

	private void OnDisable()
	{
		RecoveryItems(m_CurChatItems.Count);
	}

	private void Update()
	{
		if (m_RepositionCount > 0)
		{
			ChatListTable.Reposition();
			ChatScrollBar.scrollValue = 1f;
			m_RepositionCount--;
		}
	}

	public void AddMsg(string userName, string strMsg, string strColor)
	{
		bool flag = strMsg.Contains("[lang:cn]");
		strMsg = strMsg.Replace((!flag) ? "[lang:other]" : "[lang:cn]", string.Empty);
		string info = "[" + strColor + "]" + userName + "[-]:" + strMsg;
		CommonChatItem_N newChatItem = GetNewChatItem();
		if (null != newChatItem)
		{
			newChatItem.SetLineWidth(LineWidth);
			newChatItem.UpdateText(flag, info);
			m_CurChatItems.Add(newChatItem);
		}
		if (m_CurChatItems.Count > mMaxMsgCount)
		{
			RecoveryItems(mDeleteMsgCount);
		}
		m_RepositionCount = 3;
	}

	private void GetInputFocus()
	{
		ChatInput.selected = true;
	}

	private void SeedMsg()
	{
		if (SendMsgEvent != null)
		{
			string text = ChatInput.text;
			ChatInput.text = string.Empty;
			text = text.Trim();
			if (!string.IsNullOrEmpty(text))
			{
				text += ((!SystemSettingData.Instance.IsChinese) ? "[lang:other]" : "[lang:cn]");
				SendMsgEvent(text);
				Invoke("GetInputFocus", 0.1f);
			}
		}
	}

	private CommonChatItem_N GetNewChatItem()
	{
		CommonChatItem_N commonChatItem_N;
		if (m_ChatItemsPool.Count > 0)
		{
			commonChatItem_N = m_ChatItemsPool.Dequeue();
			commonChatItem_N.gameObject.SetActive(value: true);
		}
		else
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(m_ChatItemPrefab.gameObject);
			gameObject.transform.parent = ChatListTable.transform;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localScale = Vector3.one;
			gameObject.transform.localRotation = Quaternion.identity;
			commonChatItem_N = gameObject.GetComponent<CommonChatItem_N>();
		}
		return commonChatItem_N;
	}

	private void RecoveryItems(int count)
	{
		if (m_CurChatItems != null && m_CurChatItems.Count >= count)
		{
			for (int i = 0; i < count; i++)
			{
				CommonChatItem_N commonChatItem_N = m_CurChatItems[i];
				commonChatItem_N.ResetItem();
				commonChatItem_N.gameObject.SetActive(value: false);
				m_ChatItemsPool.Enqueue(commonChatItem_N);
			}
			m_CurChatItems.RemoveRange(0, count);
		}
	}
}
