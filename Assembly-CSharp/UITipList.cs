using System.Collections.Generic;
using UnityEngine;

public class UITipList : MonoBehaviour
{
	public enum EDirection
	{
		Up,
		Down
	}

	public delegate void DNotify();

	public EDirection direction;

	public int yPadding;

	[SerializeField]
	private UITipMsg msgPrefab;

	private List<UITipMsg> m_MsgList = new List<UITipMsg>();

	private List<UITipMsg> m_WaitList = new List<UITipMsg>();

	public AnimationCurve curve;

	public float duration = 1f;

	public int MaxMsgCount = 5;

	public bool Play = true;

	private List<float> m_DefaultHeight = new List<float>();

	private float m_CurDura;

	private float m_MotionOffset;

	private bool m_PlayEnd = true;

	public List<UITipMsg> MsgList => m_MsgList;

	public event DNotify onAddShowingMsg;

	public void AddMsg(PeTipMsg peTipMsg)
	{
		UITipMsg uITipMsg = Object.Instantiate(msgPrefab);
		uITipMsg.transform.parent = base.transform;
		uITipMsg.transform.localScale = Vector3.one;
		uITipMsg.transform.localPosition = Vector3.zero;
		uITipMsg.content.text = peTipMsg.GetContent();
		uITipMsg.content.color = peTipMsg.GetColor();
		uITipMsg.musicID = peTipMsg.GetMusicID();
		switch (peTipMsg.GetEStyle())
		{
		case PeTipMsg.EStyle.Text:
			uITipMsg.tex.mainTexture = null;
			uITipMsg.icon.spriteName = string.Empty;
			break;
		case PeTipMsg.EStyle.Icon:
			uITipMsg.icon.spriteName = peTipMsg.GetIconName();
			uITipMsg.tex.mainTexture = null;
			break;
		case PeTipMsg.EStyle.Texture:
			uITipMsg.icon.spriteName = string.Empty;
			uITipMsg.tex.mainTexture = peTipMsg.GetIconTex();
			break;
		}
		uITipMsg.SetStyle(peTipMsg.GetEStyle());
		uITipMsg.gameObject.SetActive(value: false);
		m_WaitList.Add(uITipMsg);
		if (GameUI.Instance.mTipRecordsMgr != null)
		{
			GameUI.Instance.mTipRecordsMgr.AddMsg(peTipMsg);
		}
	}

	public void ClearMsg()
	{
		foreach (UITipMsg msg in m_MsgList)
		{
			Object.Destroy(msg.gameObject);
		}
		foreach (UITipMsg wait in m_WaitList)
		{
			Object.Destroy(wait.gameObject);
		}
		m_MsgList.Clear();
		m_WaitList.Clear();
	}

	private float EvaluateOffset(float t)
	{
		return curve.Evaluate(t) * m_MotionOffset;
	}

	private void OnGUI()
	{
	}

	private void Awake()
	{
	}

	private void Update()
	{
		if (!Play)
		{
			return;
		}
		if (direction == EDirection.Up)
		{
			if (duration + 0.1f > m_CurDura)
			{
				m_CurDura += Time.deltaTime;
				float num = EvaluateOffset(Mathf.Clamp(m_CurDura / duration, 0f, 1f));
				for (int i = 0; i < m_MsgList.Count; i++)
				{
					Vector3 localPosition = m_MsgList[i].transform.localPosition;
					m_MsgList[i].transform.localPosition = new Vector3(localPosition.x, Mathf.Round(m_DefaultHeight[i] + num), localPosition.z);
				}
				m_PlayEnd = true;
				return;
			}
			for (int num2 = m_MsgList.Count - 1; num2 >= 0; num2--)
			{
				m_DefaultHeight[num2] = m_MsgList[num2].transform.localPosition.y;
			}
			if (m_PlayEnd && m_WaitList.Count == 0)
			{
				m_PlayEnd = false;
			}
			if (m_WaitList.Count != 0)
			{
				UITipMsg uITipMsg = m_WaitList[0];
				m_WaitList.RemoveAt(0);
				_addMsgDirUp(uITipMsg);
				PlayAudio(uITipMsg.musicID);
			}
			if (m_MsgList.Count > 5)
			{
				Object.Destroy(m_MsgList[0].gameObject);
				m_MsgList.RemoveAt(0);
				m_DefaultHeight.RemoveAt(0);
			}
		}
		else
		{
			if (direction != EDirection.Down)
			{
				return;
			}
			if (duration + 0.1f > m_CurDura)
			{
				m_CurDura += Time.deltaTime;
				float num3 = EvaluateOffset(Mathf.Clamp(m_CurDura / duration, 0f, 1f));
				for (int j = 0; j < m_MsgList.Count; j++)
				{
					Vector3 localPosition2 = m_MsgList[j].transform.localPosition;
					m_MsgList[j].transform.localPosition = new Vector3(localPosition2.x, Mathf.Round(m_DefaultHeight[j] + num3), localPosition2.z);
				}
				m_PlayEnd = true;
				return;
			}
			for (int num4 = m_MsgList.Count - 1; num4 >= 0; num4--)
			{
				m_DefaultHeight[num4] = m_MsgList[num4].transform.localPosition.y;
			}
			if (m_PlayEnd && m_WaitList.Count == 0)
			{
				m_PlayEnd = false;
			}
			if (m_WaitList.Count != 0)
			{
				UITipMsg uITipMsg2 = m_WaitList[0];
				m_WaitList.RemoveAt(0);
				_addMsgDirDown(uITipMsg2);
				PlayAudio(uITipMsg2.musicID);
			}
			if (m_MsgList.Count > 5)
			{
				Object.Destroy(m_MsgList[m_MsgList.Count - 1].gameObject);
				m_MsgList.RemoveAt(m_MsgList.Count - 1);
				m_DefaultHeight.RemoveAt(m_MsgList.Count - 1);
			}
		}
	}

	private void _addMsgDirUp(UITipMsg ui_msg)
	{
		ui_msg.gameObject.SetActive(value: true);
		Bounds bounds = ui_msg.GetBounds();
		if (m_MsgList.Count == 0)
		{
			ui_msg.transform.localPosition = new Vector3(0f, 0f, 0f);
		}
		else
		{
			UITipMsg uITipMsg = m_MsgList[m_MsgList.Count - 1];
			Bounds bounds2 = uITipMsg.GetBounds();
			ui_msg.transform.localPosition = new Vector3(0f, uITipMsg.transform.localPosition.y - bounds2.size.y - (float)yPadding, 0f);
		}
		m_MotionOffset = bounds.size.y + (float)yPadding;
		m_DefaultHeight.Add(ui_msg.transform.localPosition.y);
		m_CurDura = 0f;
		m_MsgList.Add(ui_msg);
		if (this.onAddShowingMsg != null)
		{
			this.onAddShowingMsg();
		}
	}

	private void _addMsgDirDown(UITipMsg ui_msg)
	{
		ui_msg.gameObject.SetActive(value: true);
		Bounds bounds = ui_msg.GetBounds();
		if (m_MsgList.Count == 0)
		{
			ui_msg.transform.localPosition = new Vector3(0f, bounds.size.y, 0f);
		}
		else
		{
			UITipMsg uITipMsg = m_MsgList[0];
			ui_msg.transform.localPosition = new Vector3(0f, uITipMsg.transform.localPosition.y + bounds.size.y + (float)yPadding, 0f);
		}
		m_MotionOffset = 0f - bounds.size.y - (float)yPadding;
		m_MsgList.Insert(0, ui_msg);
		m_DefaultHeight.Insert(0, ui_msg.transform.localPosition.y);
		m_CurDura = 0f;
		if (this.onAddShowingMsg != null)
		{
			this.onAddShowingMsg();
		}
	}

	private void PlayAudio(int musicID)
	{
		if (musicID != -1)
		{
			AudioManager.instance.Create(Vector3.zero, musicID);
		}
	}
}
