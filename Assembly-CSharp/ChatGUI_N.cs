using System.Collections.Generic;
using CustomData;
using UnityEngine;

public class ChatGUI_N : UIStaticWnd
{
	public ChartItem_N mPrefab;

	public UIInput mContentAll;

	public UITable mListTable;

	public UIScrollBar mScrollBar;

	public TweenColor mOpBtnCol;

	private int mNumCount;

	private int MaxListLenth = 50;

	private List<string>[] mChatStr = new List<string>[5];

	private int mChatType;

	private List<ChartItem_N> mChatItem = new List<ChartItem_N>();

	protected override void InitWindow()
	{
		base.InitWindow();
		mNumCount = 0;
		for (int i = 0; i < mChatStr.Length; i++)
		{
			mChatStr[i] = new List<string>();
		}
	}

	public override void Show()
	{
		if (Application.isEditor || GameConfig.IsMultiMode)
		{
			base.Show();
			mOpBtnCol.Reset();
			mOpBtnCol.enabled = false;
			mListTable.Reposition();
		}
	}

	private void OnAllBtn()
	{
		mChatType = 0;
		ResetList(mChatType);
	}

	private void OnGroupBtn()
	{
		mChatType = 1;
		ResetList(mChatType);
	}

	private void OnSomeOneBtn()
	{
		mChatType = 3;
		ResetList(mChatType);
	}

	private void OnSystemBtn()
	{
		mChatType = 2;
		ResetList(mChatType);
	}

	private void OnSendBtn()
	{
		SendMessage();
	}

	private void OnSubmit()
	{
		SendMessage();
	}

	private void OnTalkToSomeone()
	{
	}

	private void OnTalkToAll()
	{
	}

	public void AddChat(PromptData.PromptType promptType, string name, int num = 0, int meat = 0, int tdTime = 0, int tdNum = 0, string extStr = "", int type = 2)
	{
		string promptContent = PromptRepository.GetPromptContent((int)promptType);
	}

	public void AddChat(string name, string content, int type = 0)
	{
		switch (type)
		{
		case 0:
			name = "[DE625B][" + name + "]:[-]";
			break;
		case 1:
			name = "[7F63FF][" + name + "]:[-]";
			content = "[7F63FF]" + content + "[-]";
			break;
		case 2:
			name = "[8DF3FE][" + name + "]:[-]";
			content = "[8DF3FE]" + content + "[-]";
			break;
		case 3:
			name = "[FEFF93][" + name + "]:[-]";
			content = "[FEFF93]" + content + "[-]";
			break;
		case 4:
			name = "[FF0000][" + name + "]:[-]";
			content = "[FF0000]" + content + "[-]";
			break;
		}
		AddChatItem(name, content, 0);
		if (type != 0)
		{
			AddChatItem(name, content, type);
		}
		if (!IsOpen())
		{
			mOpBtnCol.Play(forward: true);
		}
	}

	private void AddChatItem(string name, string chatString, int type)
	{
		mChatStr[type].Add(name + "&" + chatString);
		if (mChatType == type)
		{
			ChartItem_N chartItem_N = Object.Instantiate(mPrefab);
			chartItem_N.gameObject.name = "ChatItem" + mNumCount;
			chartItem_N.transform.parent = mListTable.transform;
			chartItem_N.transform.localPosition = Vector3.zero;
			chartItem_N.transform.localRotation = Quaternion.identity;
			chartItem_N.transform.localScale = Vector3.one;
			chartItem_N.SetText(name, chatString);
			mChatItem.Add(chartItem_N);
			if (mChatItem.Count > MaxListLenth)
			{
				mChatItem[0].transform.parent = null;
				Object.Destroy(mChatItem[0].gameObject);
				mChatItem.RemoveAt(0);
				mChatStr[type].RemoveAt(0);
			}
			mListTable.Reposition();
			mScrollBar.scrollValue = 1f;
		}
		mNumCount++;
		if (mNumCount > 200000000)
		{
			mNumCount = 0;
			for (int num = mChatItem.Count - 1; num >= 0; num--)
			{
				mChatItem[num].transform.parent = null;
				Object.Destroy(mChatItem[num].gameObject);
				mChatItem.RemoveAt(num);
			}
			ResetList(mChatType);
		}
	}

	private void ResetList(int type)
	{
		if (mChatStr[type].Count >= mChatItem.Count)
		{
			for (int i = 0; i < mChatItem.Count; i++)
			{
				string[] array = mChatStr[type][i].Split('&');
				mChatItem[i].SetText(array[0], array[1]);
			}
			for (int j = mChatItem.Count; j < mChatStr[type].Count; j++)
			{
				ChartItem_N chartItem_N = Object.Instantiate(mPrefab);
				chartItem_N.gameObject.name = "ChatItem" + mNumCount;
				chartItem_N.transform.parent = mListTable.transform;
				chartItem_N.transform.localPosition = Vector3.zero;
				chartItem_N.transform.localRotation = Quaternion.identity;
				chartItem_N.transform.localScale = Vector3.one;
				string[] array2 = mChatStr[type][j].Split('&');
				chartItem_N.SetText(array2[0], array2[1]);
				mChatItem.Add(chartItem_N);
			}
		}
		else
		{
			for (int k = 0; k < mChatStr[type].Count; k++)
			{
				string[] array3 = mChatStr[type][k].Split('&');
				mChatItem[k].SetText(array3[0], array3[1]);
			}
			for (int num = mChatItem.Count - 1; num >= mChatStr[type].Count; num--)
			{
				mChatItem[num].transform.parent = null;
				Object.Destroy(mChatItem[num].gameObject);
				mChatItem.RemoveAt(num);
			}
		}
		mListTable.Reposition();
		mScrollBar.scrollValue = 1f;
	}

	private void SendMessage()
	{
		if (!(mContentAll.text == string.Empty) && !(null == PlayerNetwork.mainPlayer))
		{
			string text = mContentAll.text;
			PlayerNetwork.mainPlayer.RequestSendMsg(EMsgType.ToAll, text);
			mContentAll.text = string.Empty;
		}
	}
}
