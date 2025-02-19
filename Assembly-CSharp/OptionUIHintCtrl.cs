using System.Collections.Generic;
using UnityEngine;

public class OptionUIHintCtrl : MonoBehaviour
{
	public delegate void DNotify();

	[SerializeField]
	private OptionUIHintItem mPrefab;

	[SerializeField]
	private Transform mParent;

	public float duration = 1f;

	private float m_CurDura;

	public AnimationCurve curve;

	private float m_MotionOffset;

	private bool m_PlayEnd = true;

	public int yPadding;

	private List<float> m_DefaultHeight = new List<float>();

	private List<OptionUIHintItem> m_MsgList = new List<OptionUIHintItem>(10);

	private List<OptionUIHintItem> m_WaitList = new List<OptionUIHintItem>(10);

	public List<OptionUIHintItem> MsgList => m_MsgList;

	public event DNotify onAddShowingMsg;

	public void AddOneHint(string _content)
	{
		OptionUIHintItem optionUIHintItem = GoCreat();
		optionUIHintItem.SetHintInfo(_content);
		m_WaitList.Add(optionUIHintItem);
	}

	private OptionUIHintItem GoCreat()
	{
		OptionUIHintItem optionUIHintItem = Object.Instantiate(mPrefab);
		optionUIHintItem.transform.parent = mParent;
		optionUIHintItem.transform.localPosition = Vector3.zero;
		optionUIHintItem.transform.localRotation = Quaternion.identity;
		optionUIHintItem.transform.localScale = Vector3.one;
		optionUIHintItem.gameObject.SetActive(value: false);
		return optionUIHintItem;
	}

	private void Update()
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
			OptionUIHintItem ui_msg = m_WaitList[0];
			m_WaitList.RemoveAt(0);
			_addMsgDirDown(ui_msg);
		}
		if (m_MsgList.Count > 5)
		{
			Object.Destroy(m_MsgList[m_MsgList.Count - 1].gameObject);
			m_MsgList.RemoveAt(m_MsgList.Count - 1);
			m_DefaultHeight.RemoveAt(m_MsgList.Count - 1);
		}
	}

	private float EvaluateOffset(float t)
	{
		return curve.Evaluate(t) * m_MotionOffset;
	}

	private void _addMsgDirDown(OptionUIHintItem ui_msg)
	{
		ui_msg.gameObject.SetActive(value: true);
		Bounds bounds = ui_msg.GetBounds();
		if (m_MsgList.Count == 0)
		{
			ui_msg.transform.localPosition = new Vector3(0f, bounds.size.y, 0f);
		}
		else
		{
			OptionUIHintItem optionUIHintItem = m_MsgList[0];
			ui_msg.transform.localPosition = new Vector3(0f, optionUIHintItem.transform.localPosition.y + bounds.size.y + (float)yPadding, 0f);
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

	public void ClearMsg()
	{
		foreach (OptionUIHintItem msg in m_MsgList)
		{
			Object.Destroy(msg.gameObject);
		}
		foreach (OptionUIHintItem wait in m_WaitList)
		{
			Object.Destroy(wait.gameObject);
		}
		m_MsgList.Clear();
		m_WaitList.Clear();
	}
}
