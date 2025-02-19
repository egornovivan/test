using System;
using System.Collections.Generic;
using CustomData;
using Pathea;
using UnityEngine;

public class UITalkwithctr : MonoBehaviour
{
	private static UITalkwithctr mInstance;

	public UIInput mMsgText;

	public UITalkBoxCtrl mTalkBoxControl;

	public UIAnchor mTalkBoxAnchor;

	private List<string> mTalkNameColorList = new List<string>();

	public static UITalkwithctr Instance => mInstance;

	public bool isShow => base.gameObject.activeInHierarchy;

	private void Awake()
	{
		if (PeGameMgr.IsMulti)
		{
			base.gameObject.SetActive(value: true);
			mTalkBoxAnchor.depthOffset = 1.2f;
			UIEventListener uIEventListener = UIEventListener.Get(mMsgText.gameObject);
			uIEventListener.onSelect = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onSelect, (UIEventListener.BoolDelegate)delegate(GameObject go, bool isSelect)
			{
				mTalkBoxAnchor.depthOffset = ((!isSelect) ? 1.2f : 0.2f);
			});
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
		InitTalkNameColorList();
		mInstance = this;
	}

	public void ShowMenu()
	{
		if (PeGameMgr.IsMulti)
		{
			base.gameObject.SetActive(value: true);
			mMsgText.selected = true;
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
	}

	public void AddTalk(string name, string content, string colorStr)
	{
		if (!(mTalkBoxControl == null))
		{
			mTalkBoxControl.AddMsg(name, content, colorStr);
		}
	}

	private void SendMsg()
	{
		if (!(mMsgText.text == string.Empty))
		{
			string text = mMsgText.text;
			text += ((!SystemSettingData.Instance.IsChinese) ? "[lang:other]" : "[lang:cn]");
			PlayerNetwork.mainPlayer.RequestSendMsg(EMsgType.ToAll, text);
			mMsgText.text = string.Empty;
			Invoke("GetInputCursor", 0.1f);
		}
	}

	private void GetInputCursor()
	{
		mMsgText.selected = true;
	}

	private void Recive()
	{
	}

	private void BtnInputOnClick()
	{
		SendMsg();
	}

	private void OnSubmit(string inputString)
	{
		SendMsg();
	}

	private void Update()
	{
		if (mMsgText.selected)
		{
			GameUI.Instance.IsInput = true;
		}
		else
		{
			GameUI.Instance.IsInput = false;
		}
	}

	private void InitTalkNameColorList()
	{
		mTalkNameColorList.Clear();
		mTalkNameColorList.Add("FFB6C1");
		mTalkNameColorList.Add("DC143C");
		mTalkNameColorList.Add("DB7093");
		mTalkNameColorList.Add("FF69B4");
		mTalkNameColorList.Add("FF1493");
		mTalkNameColorList.Add("DA70D6");
		mTalkNameColorList.Add("FF00FF");
		mTalkNameColorList.Add("8B008B");
		mTalkNameColorList.Add("8A2BE2");
		mTalkNameColorList.Add("7B68EE");
		mTalkNameColorList.Add("0000FF");
		mTalkNameColorList.Add("00008B");
		mTalkNameColorList.Add("778899");
		mTalkNameColorList.Add("00BFFF");
		mTalkNameColorList.Add("5F9EA0");
		mTalkNameColorList.Add("00FFFF");
		mTalkNameColorList.Add("008B8B");
		mTalkNameColorList.Add("F5FFFA");
		mTalkNameColorList.Add("228B22");
		mTalkNameColorList.Add("FFFF00");
		mTalkNameColorList.Add("FFD700");
		mTalkNameColorList.Add("FF8C00");
		mTalkNameColorList.Add("BC8F8F");
		mTalkNameColorList.Add("A52A2A");
		mTalkNameColorList.Add("B0E0E6");
		mTalkNameColorList.Add("00CED1");
		mTalkNameColorList.Add("40E0D0");
		mTalkNameColorList.Add("2E8B57");
		mTalkNameColorList.Add("8FBC8F");
		mTalkNameColorList.Add("ADFF2F");
		mTalkNameColorList.Add("FAFAD2");
		mTalkNameColorList.Add("BDB76B");
	}
}
