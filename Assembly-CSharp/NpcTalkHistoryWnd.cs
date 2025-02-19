using System;
using System.Collections.Generic;
using Pathea;
using UnityEngine;

public class NpcTalkHistoryWnd : UIBaseWnd
{
	[SerializeField]
	private GameObject m_MsgPrefab;

	[SerializeField]
	private UITable m_Table;

	[SerializeField]
	private GoPool m_ItemGoPool;

	[SerializeField]
	private UIScrollBar m_ScrollBar;

	[SerializeField]
	private string m_MsgFormat = "[45D0FF]{0} : [-]\n[ffffff]{1}[-]";

	private List<GameObject> m_MsgGoList = new List<GameObject>();

	protected override void InitWindow()
	{
		if (PeSingleton<NPCTalkHistroy>.Instance != null)
		{
			FirstShowAllData();
			NPCTalkHistroy instance = PeSingleton<NPCTalkHistroy>.Instance;
			instance.onAddHistroy = (Action<NPCTalkHistroy.Histroy>)Delegate.Combine(instance.onAddHistroy, new Action<NPCTalkHistroy.Histroy>(AddHistroy));
			NPCTalkHistroy instance2 = PeSingleton<NPCTalkHistroy>.Instance;
			instance2.onRemoveHistroy = (Action)Delegate.Combine(instance2.onRemoveHistroy, new Action(RemoveHistroy));
		}
		base.InitWindow();
	}

	public override void Show()
	{
		base.Show();
		Reposition();
	}

	private void FirstShowAllData()
	{
		if (!mInit && PeSingleton<NPCTalkHistroy>.Instance != null && PeSingleton<NPCTalkHistroy>.Instance.histroies.Count > 0)
		{
			for (int i = 0; i < PeSingleton<NPCTalkHistroy>.Instance.histroies.Count; i++)
			{
				AddHistroy(PeSingleton<NPCTalkHistroy>.Instance.histroies[i]);
			}
			Reposition();
		}
	}

	private void AddHistroy(NPCTalkHistroy.Histroy histroyMsg)
	{
		GameObject gameObject = CreateGo();
		UILabel component = gameObject.GetComponent<UILabel>();
		if (null != component && histroyMsg.npcName != null && histroyMsg.countent != null)
		{
			string arg = histroyMsg.countent.Replace("\n", string.Empty);
			component.text = string.Format(m_MsgFormat, histroyMsg.npcName, arg);
			component.MakePixelPerfect();
		}
		gameObject.SetActive(value: true);
		m_MsgGoList.Add(gameObject);
		Reposition();
	}

	private void RemoveHistroy()
	{
		if (m_MsgGoList.Count > 0)
		{
			DestroyGo(m_MsgGoList[0]);
			m_MsgGoList.RemoveAt(0);
			Reposition();
		}
	}

	private void Reposition()
	{
		m_Table.Reposition();
		Invoke("RepostionScroll", 0.2f);
	}

	private void RepostionScroll()
	{
		if (m_ScrollBar.foreground.gameObject.activeSelf)
		{
			m_ScrollBar.scrollValue = 1f;
		}
		else
		{
			m_ScrollBar.scrollValue = 0f;
		}
	}

	private GameObject CreateGo()
	{
		if (m_ItemGoPool != null)
		{
			return m_ItemGoPool.GetGo(m_Table.transform, show: false);
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(m_MsgPrefab);
		gameObject.transform.parent = m_Table.transform;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Quaternion.identity;
		gameObject.transform.localScale = Vector3.one;
		return gameObject;
	}

	private void DestroyGo(GameObject go)
	{
		if ((bool)go)
		{
			if (m_ItemGoPool != null)
			{
				m_ItemGoPool.GiveBackGo(go, hide: true);
			}
			else
			{
				UnityEngine.Object.Destroy(go);
			}
		}
	}

	private void OnClearBtn()
	{
		for (int i = 0; i < m_MsgGoList.Count; i++)
		{
			DestroyGo(m_MsgGoList[i]);
		}
		m_MsgGoList.Clear();
		m_ScrollBar.scrollValue = 0f;
		if (PeSingleton<NPCTalkHistroy>.Instance != null)
		{
			PeSingleton<NPCTalkHistroy>.Instance.Clear();
		}
	}
}
