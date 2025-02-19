using System;
using System.Collections.Generic;
using UnityEngine;

public class UINpcDialogWnd : UIBaseWnd
{
	[SerializeField]
	private UILabel npcNameLabel;

	[SerializeField]
	private UISprite npcHeadSprite;

	[SerializeField]
	private UITexture npcHeadTex;

	[SerializeField]
	private UITable questTable;

	[SerializeField]
	private UINpcQuestItem questItemPrefab;

	[SerializeField]
	private Transform defaultQuest;

	private List<UINpcQuestItem> m_QuestItems = new List<UINpcQuestItem>();

	public Action<UINpcQuestItem> onSetItemContent;

	public Action<UINpcQuestItem> onQuestItemClick;

	public Action onBeforeShow;

	public void UpdateTable(int count)
	{
		count = ((count >= 0) ? count : 0);
		if (count > m_QuestItems.Count)
		{
			for (int i = 0; i < m_QuestItems.Count; i++)
			{
				m_QuestItems[i].index = i;
				if (onSetItemContent != null)
				{
					onSetItemContent(m_QuestItems[i]);
				}
			}
			int num = count;
			for (int j = m_QuestItems.Count; j < num; j++)
			{
				UINpcQuestItem uINpcQuestItem = CreateQuestItem();
				uINpcQuestItem.index = j;
				m_QuestItems.Add(uINpcQuestItem);
				uINpcQuestItem.onClick = (Action<UINpcQuestItem>)Delegate.Combine(uINpcQuestItem.onClick, new Action<UINpcQuestItem>(OnQuestItemClick));
				if (onSetItemContent != null)
				{
					onSetItemContent(m_QuestItems[j]);
				}
			}
		}
		else
		{
			for (int k = 0; k < count; k++)
			{
				m_QuestItems[k].index = k;
				if (onSetItemContent != null)
				{
					onSetItemContent(m_QuestItems[k]);
				}
			}
			for (int num2 = m_QuestItems.Count - 1; num2 >= count; num2--)
			{
				UINpcQuestItem uINpcQuestItem2 = m_QuestItems[num2];
				uINpcQuestItem2.onClick = (Action<UINpcQuestItem>)Delegate.Remove(uINpcQuestItem2.onClick, new Action<UINpcQuestItem>(OnQuestItemClick));
				UnityEngine.Object.Destroy(m_QuestItems[num2].gameObject);
				m_QuestItems[num2].transform.parent = null;
				m_QuestItems.RemoveAt(num2);
			}
		}
		defaultQuest.transform.SetAsLastSibling();
		questTable.repositionNow = true;
	}

	public void SetNPCInfo(string _name, string _icon)
	{
		npcNameLabel.text = _name;
		npcHeadTex.enabled = false;
		npcHeadSprite.enabled = true;
		npcHeadSprite.spriteName = _icon;
	}

	public void SetNPCInfo(string _name, Texture _icon)
	{
		npcNameLabel.text = _name;
		npcHeadTex.enabled = true;
		npcHeadSprite.enabled = false;
		npcHeadTex.mainTexture = _icon;
	}

	private UINpcQuestItem CreateQuestItem()
	{
		UINpcQuestItem uINpcQuestItem = UnityEngine.Object.Instantiate(questItemPrefab);
		Transform transform = uINpcQuestItem.transform;
		uINpcQuestItem.transform.parent = questTable.transform;
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		transform.localScale = Vector3.one;
		transform.gameObject.SetActive(value: true);
		return uINpcQuestItem;
	}

	private void OnQuestItemClick(UINpcQuestItem item)
	{
		if (onQuestItemClick != null)
		{
			onQuestItemClick(item);
		}
	}

	private void OnDefaultQuestClick()
	{
		Hide();
	}

	public override void Show()
	{
		if (onBeforeShow != null)
		{
			onBeforeShow();
		}
		base.Show();
	}

	protected override void OnHide()
	{
		base.OnHide();
	}

	protected override void OnClose()
	{
		base.OnClose();
	}
}
