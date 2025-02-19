using System.Collections.Generic;
using UnityEngine;

public class UITipRecordsMgr : UIBaseWnd
{
	[SerializeField]
	private GameObject msgPrefab;

	[SerializeField]
	private UITable mTable;

	[SerializeField]
	private GoPool itemGoPool;

	[SerializeField]
	private UIScrollBar mScrollBar;

	public int mTipRecordsCountMax = 50;

	private List<GameObject> m_WaitList = new List<GameObject>();

	public void AddMsg(PeTipMsg peTipMsg)
	{
		GameObject gameObject = CreateGo();
		UITipMsg component = gameObject.GetComponent<UITipMsg>();
		component.content.text = peTipMsg.GetContent();
		component.content.color = peTipMsg.GetColor();
		component.musicID = peTipMsg.GetMusicID();
		switch (peTipMsg.GetEStyle())
		{
		case PeTipMsg.EStyle.Text:
			component.tex.mainTexture = null;
			component.icon.spriteName = string.Empty;
			break;
		case PeTipMsg.EStyle.Icon:
			component.icon.spriteName = peTipMsg.GetIconName();
			component.tex.mainTexture = null;
			break;
		case PeTipMsg.EStyle.Texture:
			component.icon.spriteName = string.Empty;
			component.tex.mainTexture = peTipMsg.GetIconTex();
			break;
		}
		component.SetStyle(peTipMsg.GetEStyle());
		component.gameObject.SetActive(value: true);
		m_WaitList.Add(gameObject);
		CheckTipsCount();
		Reposition();
		if (m_WaitList.Count > 7 && mScrollBar != null)
		{
			mScrollBar.scrollValue = 1f;
		}
	}

	public override void Show()
	{
		base.Show();
		Reposition();
	}

	private void Reposition()
	{
		mTable.Reposition();
	}

	private GameObject CreateGo()
	{
		if (itemGoPool != null)
		{
			return itemGoPool.GetGo(mTable.transform, show: false);
		}
		GameObject gameObject = Object.Instantiate(msgPrefab);
		gameObject.transform.parent = mTable.transform;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Quaternion.identity;
		gameObject.transform.localScale = Vector3.one;
		return gameObject;
	}

	private void DestroyGo(GameObject go)
	{
		if ((bool)go)
		{
			if (itemGoPool != null)
			{
				itemGoPool.GiveBackGo(go, hide: true);
			}
			else
			{
				Object.Destroy(go);
			}
		}
	}

	private void CheckTipsCount()
	{
		if (m_WaitList.Count > mTipRecordsCountMax)
		{
			DestroyGo(m_WaitList[0]);
			m_WaitList.RemoveAt(0);
		}
	}

	private void OnClearBtn()
	{
		for (int i = 0; i < m_WaitList.Count; i++)
		{
			DestroyGo(m_WaitList[i]);
		}
		m_WaitList.Clear();
		if (mScrollBar != null)
		{
			mScrollBar.scrollValue = 0f;
		}
	}

	public override void ChangeWindowShowState()
	{
		base.ChangeWindowShowState();
		if (mTable != null)
		{
			mTable.Reposition();
		}
		if (m_WaitList.Count > 7 && mScrollBar != null)
		{
			mScrollBar.scrollValue = 1f;
		}
	}
}
