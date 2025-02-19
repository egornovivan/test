using System;
using System.Collections.Generic;
using UnityEngine;

public class UINpcSpeechBox : UIBaseWidget
{
	[SerializeField]
	private UILabel contentLabel;

	[SerializeField]
	private UITable chooseTable;

	[SerializeField]
	private UILabel npcNameLabel;

	[SerializeField]
	private UISprite npcHeadSprite;

	[SerializeField]
	private UITexture npcHeadTex;

	[SerializeField]
	private UINpcQuestItem questItemPrefab;

	private List<UINpcQuestItem> m_QuestItems = new List<UINpcQuestItem>();

	public Action<UINpcQuestItem> onSetItemContent;

	public Action<UINpcQuestItem> onQuestItemClick;

	public Action onUIClick;

	public bool IsChoice => !contentLabel.gameObject.activeSelf;

	public void SetContent(string content)
	{
		if (!contentLabel.gameObject.activeSelf)
		{
			contentLabel.gameObject.SetActive(value: true);
		}
		contentLabel.text = content;
		if (chooseTable.gameObject.activeSelf)
		{
			chooseTable.gameObject.SetActive(value: false);
		}
	}

	public void SetContent(int choose_num)
	{
		int num = ((choose_num >= 0) ? choose_num : 0);
		if (!chooseTable.gameObject.activeSelf)
		{
			chooseTable.gameObject.SetActive(value: true);
		}
		if (contentLabel.gameObject.activeSelf)
		{
			contentLabel.gameObject.SetActive(value: false);
		}
		if (num > m_QuestItems.Count)
		{
			for (int i = 0; i < m_QuestItems.Count; i++)
			{
				m_QuestItems[i].index = i;
				if (onSetItemContent != null)
				{
					onSetItemContent(m_QuestItems[i]);
				}
			}
			int num2 = num;
			for (int j = m_QuestItems.Count; j < num2; j++)
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
			for (int k = 0; k < num; k++)
			{
				m_QuestItems[k].index = k;
				if (onSetItemContent != null)
				{
					onSetItemContent(m_QuestItems[k]);
				}
			}
			for (int num3 = m_QuestItems.Count - 1; num3 >= num; num3--)
			{
				UINpcQuestItem uINpcQuestItem2 = m_QuestItems[num3];
				uINpcQuestItem2.onClick = (Action<UINpcQuestItem>)Delegate.Remove(uINpcQuestItem2.onClick, new Action<UINpcQuestItem>(OnQuestItemClick));
				UnityEngine.Object.Destroy(m_QuestItems[num3].gameObject);
				m_QuestItems[num3].transform.parent = null;
				m_QuestItems.RemoveAt(num3);
			}
		}
		chooseTable.repositionNow = true;
	}

	public void SetNpcInfo(string _name, string _icon)
	{
		npcNameLabel.text = _name;
		npcHeadTex.enabled = false;
		npcHeadSprite.enabled = true;
		npcHeadSprite.spriteName = _icon;
	}

	public void SetNpcInfo(string _name, Texture _icon)
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
		uINpcQuestItem.transform.parent = chooseTable.transform;
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

	public void OnUIClick()
	{
		if (onUIClick != null)
		{
			onUIClick();
		}
	}

	public override void Show()
	{
		base.Show();
	}

	protected override void OnHide()
	{
		base.OnHide();
	}

	protected override void OnClose()
	{
		if (onUIClick != null)
		{
			onUIClick();
		}
	}
}
