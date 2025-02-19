using System.Collections.Generic;
using UnityEngine;

public class MessageBox_N : UIStaticWnd
{
	public class Message
	{
		public MsgBoxType mType;

		public MsgInfoType mInfotype;

		public string mContent;

		public CallBackFunc mYesFunc;

		public CallBackFunc mNoFunc;

		public CallBackFunc mOkFunc;

		public CallBackFunc mTimeOutFunc;

		public UTimer mTimer;
	}

	public delegate void CallBackFunc();

	private static MessageBox_N mInstance;

	public UILabel mContent;

	public N_ImageButton mOkBtn;

	public N_ImageButton mYseBtn;

	public N_ImageButton mNoBtn;

	private List<Message> mMsgs = new List<Message>();

	public static MessageBox_N Instance => mInstance;

	public static bool IsShowing => null != mInstance && mInstance.mMsgs.Count > 0;

	private void Awake()
	{
		mInstance = this;
		mMsgs.Clear();
	}

	private void OnNoOrOkBtn()
	{
		if (mMsgs.Count > 0)
		{
			if (mMsgs[0].mNoFunc != null)
			{
				mMsgs[0].mNoFunc();
			}
			if (mMsgs[0].mOkFunc != null)
			{
				mMsgs[0].mOkFunc();
			}
			mMsgs.RemoveAt(0);
		}
		ResetMsgBox();
	}

	private void OnYesBtn()
	{
		if (mMsgs.Count > 0)
		{
			if (mMsgs[0].mYesFunc != null)
			{
				mMsgs[0].mYesFunc();
			}
			mMsgs.RemoveAt(0);
		}
		ResetMsgBox();
	}

	public static Message ShowOkBox(string text, CallBackFunc func = null)
	{
		if ((bool)mInstance)
		{
			Message message = new Message();
			message.mType = MsgBoxType.Msg_OK;
			message.mInfotype = MsgInfoType.NoticeOnly;
			message.mContent = text;
			message.mOkFunc = func;
			mInstance.mMsgs.Add(message);
			mInstance.ResetMsgBox();
			return message;
		}
		return null;
	}

	public static Message ShowYNBox(string text, CallBackFunc yesFunc = null, CallBackFunc noFunc = null)
	{
		if ((bool)mInstance)
		{
			Message message = new Message();
			message.mType = MsgBoxType.Msg_YN;
			message.mInfotype = MsgInfoType.NoticeOnly;
			message.mContent = text;
			message.mYesFunc = yesFunc;
			message.mNoFunc = noFunc;
			mInstance.mMsgs.Add(message);
			mInstance.ResetMsgBox();
			return message;
		}
		return null;
	}

	public static Message ShowMaskBox(MsgInfoType type, string text, float waitTime = 600f, CallBackFunc timeOutFunc = null)
	{
		if ((bool)mInstance)
		{
			for (int i = 0; i < mInstance.mMsgs.Count; i++)
			{
				if (mInstance.mMsgs[i].mInfotype == type)
				{
					return null;
				}
			}
			Message message = new Message();
			message.mType = MsgBoxType.Msg_Mask;
			message.mInfotype = type;
			message.mContent = text;
			message.mTimeOutFunc = timeOutFunc;
			message.mTimer = new UTimer();
			message.mTimer.Second = waitTime;
			message.mTimer.ElapseSpeed = -1f;
			mInstance.mMsgs.Add(message);
			mInstance.ResetMsgBox();
			return message;
		}
		return null;
	}

	public static void CancelMessage(Message msg)
	{
		if (null != mInstance)
		{
			mInstance.CancelMsg(msg);
		}
	}

	public static void CancelMask(MsgInfoType info)
	{
		if ((bool)mInstance)
		{
			mInstance.CancelMaskP(info);
		}
	}

	public void CancelMaskP(MsgInfoType info)
	{
		for (int i = 0; i < mMsgs.Count; i++)
		{
			if (mMsgs[i].mInfotype == info)
			{
				if (mMsgs[i].mNoFunc != null)
				{
					mMsgs[i].mNoFunc();
				}
				if (mMsgs[i].mOkFunc != null)
				{
					mMsgs[i].mOkFunc();
				}
				mMsgs.RemoveAt(i);
				if (i == 0)
				{
					ResetMsgBox();
				}
				break;
			}
		}
	}

	public void CancelMsg(Message msg)
	{
		if (msg == null)
		{
			return;
		}
		for (int i = 0; i < mMsgs.Count; i++)
		{
			if (mMsgs[i] == msg)
			{
				mMsgs.RemoveAt(i);
				if (i == 0)
				{
					ResetMsgBox();
				}
				break;
			}
		}
	}

	public MsgInfoType GetCurrentInfoTypeP()
	{
		if (mMsgs.Count > 0)
		{
			return mMsgs[0].mInfotype;
		}
		return MsgInfoType.Null;
	}

	private void ResetMsgBox()
	{
		if (mMsgs.Count > 0)
		{
			Show();
			switch (mMsgs[0].mType)
			{
			case MsgBoxType.Msg_OK:
				mYseBtn.gameObject.SetActive(value: false);
				mNoBtn.gameObject.SetActive(value: false);
				mOkBtn.gameObject.SetActive(value: true);
				break;
			case MsgBoxType.Msg_YN:
				mYseBtn.gameObject.SetActive(value: true);
				mNoBtn.gameObject.SetActive(value: true);
				mOkBtn.gameObject.SetActive(value: false);
				break;
			case MsgBoxType.Msg_Mask:
				mYseBtn.gameObject.SetActive(value: false);
				mNoBtn.gameObject.SetActive(value: false);
				mOkBtn.gameObject.SetActive(value: false);
				break;
			}
			mContent.text = mMsgs[0].mContent;
			GlobalEvent.NoticeMouseUnlock();
		}
		else
		{
			Hide();
		}
	}

	private void Update()
	{
		if (mMsgs.Count <= 0)
		{
			return;
		}
		if (mMsgs[0] == null)
		{
			mMsgs.RemoveAt(0);
		}
		else if (mMsgs[0].mType == MsgBoxType.Msg_OK && Input.GetKeyDown(KeyCode.Return))
		{
			OnNoOrOkBtn();
		}
		else
		{
			if (mMsgs[0].mType != MsgBoxType.Msg_Mask || mMsgs[0].mTimer == null)
			{
				return;
			}
			uint num = (uint)mMsgs[0].mTimer.Second;
			mMsgs[0].mTimer.Update(Time.deltaTime);
			uint num2 = (uint)mMsgs[0].mTimer.Second;
			if (num != num2)
			{
				mContent.text += ".";
				if (mContent.text.Length - mMsgs[0].mContent.Length > 6)
				{
					mContent.text = mMsgs[0].mContent;
				}
			}
			if (mMsgs[0].mTimer.Second < 0.0)
			{
				mMsgs[0].mTimer = null;
				if (mMsgs[0].mTimeOutFunc != null)
				{
					mMsgs[0].mTimeOutFunc();
				}
				CancelMaskP(mMsgs[0].mInfotype);
			}
		}
	}
}
