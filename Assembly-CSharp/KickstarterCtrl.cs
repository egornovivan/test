using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml;
using UnityEngine;

public class KickstarterCtrl : UIBaseWnd
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private struct TeamInfo
	{
		public string TitleStr { get; private set; }

		public List<string> PeoplesStr { get; private set; }

		public TeamInfo(int titleID, string peoplesStr)
		{
			TitleStr = PELocalization.GetString(titleID);
			PeoplesStr = new List<string>();
			string[] array = peoplesStr.Split(new char[1] { '\n' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
			string empty = string.Empty;
			for (int i = 0; i < array.Length; i++)
			{
				empty = array[i];
				empty = empty.Trim();
				if (!string.IsNullOrEmpty(empty))
				{
					PeoplesStr.Add(empty);
				}
			}
		}
	}

	[SerializeField]
	private UILabel m_TitleLabel;

	[SerializeField]
	private UIPanel m_ClipPanel;

	[SerializeField]
	private BoxCollider m_DragCollider;

	[SerializeField]
	private UIScrollBar m_VScrollBars;

	[SerializeField]
	private Transform m_ItemParent;

	[SerializeField]
	private ManyPeopleItem m_KickstarterItemPrefab;

	[SerializeField]
	private int m_ColumnCount;

	[SerializeField]
	private int m_PaddingY;

	[SerializeField]
	private Transform m_TopPos;

	[SerializeField]
	private Transform m_BottomPos;

	[SerializeField]
	private float m_StartWaitTime = 2f;

	[SerializeField]
	private float m_MoveSpeed = 2f;

	private List<TeamInfo> m_TeamsInfo;

	private int m_CurShowIndex;

	private bool m_FirstShow;

	private bool m_IsPress;

	private bool m_IsScroll;

	private float m_ContentHeight;

	protected override void InitWindow()
	{
		base.InitWindow();
		Init();
	}

	public override void Show()
	{
		base.Show();
		if ((null != m_TeamsInfo) & (m_TeamsInfo.Count > 0))
		{
			if (!m_FirstShow)
			{
				ChangeCurShowIndex();
			}
			else
			{
				m_CurShowIndex = 0;
			}
			ShowCurContent();
			StartCoroutine("AutoScrollIterator");
		}
		m_FirstShow = false;
	}

	protected override void OnHide()
	{
		StopCoroutine("AutoScrollIterator");
		DestoryItems();
		base.OnHide();
	}

	private void Update()
	{
		if (null != UICamera.hoveredObject && UICamera.hoveredObject == m_DragCollider.gameObject)
		{
			float axis = Input.GetAxis("Mouse ScrollWheel");
			m_IsScroll = axis != 0f;
		}
	}

	private void DestoryItems()
	{
		int childCount = m_ItemParent.childCount;
		for (int i = 0; i < childCount; i++)
		{
			UnityEngine.Object.Destroy(m_ItemParent.GetChild(i).gameObject);
		}
	}

	private void Init()
	{
		m_TeamsInfo = new List<TeamInfo>();
		m_CurShowIndex = 0;
		m_FirstShow = true;
		m_IsPress = false;
		m_IsScroll = false;
		FillTeamsInfo();
		UIEventListener.Get(m_DragCollider.gameObject).onPress = delegate(GameObject go, bool isPress)
		{
			m_IsPress = isPress;
		};
		UIEventListener.Get(m_VScrollBars.foreground.gameObject).onPress = delegate(GameObject go, bool isPress)
		{
			m_IsPress = isPress;
		};
	}

	private void ChangeCurShowIndex()
	{
		if (m_TeamsInfo != null && m_TeamsInfo.Count > 0)
		{
			m_CurShowIndex++;
			if (m_CurShowIndex < 0 || m_CurShowIndex >= m_TeamsInfo.Count)
			{
				m_CurShowIndex = 0;
			}
		}
	}

	private void ShowCurContent()
	{
		if (m_TeamsInfo == null || m_TeamsInfo.Count <= 0 || m_CurShowIndex < 0 || m_CurShowIndex >= m_TeamsInfo.Count)
		{
			return;
		}
		TeamInfo teamInfo = m_TeamsInfo[m_CurShowIndex];
		m_TitleLabel.text = teamInfo.TitleStr;
		m_TitleLabel.MakePixelPerfect();
		List<string> peoplesStr = teamInfo.PeoplesStr;
		if (peoplesStr != null && peoplesStr.Count > 0)
		{
			for (int i = 0; i < peoplesStr.Count; i += m_ColumnCount)
			{
				ManyPeopleItem newKickstarterItem = GetNewKickstarterItem();
				newKickstarterItem.UpdateNames(peoplesStr.GetRange(i, (i + m_ColumnCount < peoplesStr.Count) ? m_ColumnCount : (peoplesStr.Count - i)));
			}
			RepositionVertical();
		}
	}

	private void FillTeamsInfo()
	{
		TextAsset textAsset = Resources.Load("Credits/KickstarterInGame", typeof(TextAsset)) as TextAsset;
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.LoadXml(textAsset.text);
		XmlNodeList xmlNodeList = xmlDocument.SelectNodes("Root/Team");
		if (xmlNodeList != null && xmlNodeList.Count > 0)
		{
			for (int i = 0; i < xmlNodeList.Count; i++)
			{
				XmlNode xmlNode = xmlNodeList[i];
				XmlNode xmlNode2 = xmlNode.SelectSingleNode("Title");
				XmlNode xmlNode3 = xmlNode.SelectSingleNode("Peoples");
				m_TeamsInfo.Add(new TeamInfo((xmlNode2 != null) ? int.Parse(xmlNode2.InnerText) : 0, (xmlNode3 != null) ? xmlNode3.InnerText : string.Empty));
			}
		}
	}

	private ManyPeopleItem GetNewKickstarterItem()
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(m_KickstarterItemPrefab.gameObject);
		gameObject.transform.parent = m_ItemParent.transform;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localScale = Vector3.one;
		return gameObject.GetComponent<ManyPeopleItem>();
	}

	private void RepositionVertical()
	{
		int childCount = m_ItemParent.childCount;
		Vector3 zero = Vector3.zero;
		Vector3 vector = new Vector3(0f, m_PaddingY, 0f);
		ManyPeopleItem manyPeopleItem = null;
		for (int i = 0; i < childCount; i++)
		{
			Transform child = m_ItemParent.GetChild(i);
			manyPeopleItem = child.GetComponent<ManyPeopleItem>();
			child.localPosition = zero - vector;
			Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds(child);
			zero.y = child.localPosition.y - bounds.size.y;
		}
		m_ContentHeight = Math.Abs(zero.y);
		ResetClipPanel();
		m_ItemParent.transform.localPosition = m_TopPos.localPosition;
	}

	private void ResetClipPanel()
	{
		m_VScrollBars.scrollValue = 0f;
		Vector3 localPosition = m_ClipPanel.transform.localPosition;
		localPosition.y = 0f;
		m_ClipPanel.transform.localPosition = localPosition;
		m_ClipPanel.clipRange = new Vector4(0f, 0f, 800f, 430f);
		m_ClipPanel.clipSoftness = new Vector2(10f, 20f);
	}

	private IEnumerator AutoScrollIterator()
	{
		float startTime = Time.realtimeSinceStartup;
		while (Time.realtimeSinceStartup - startTime < m_StartWaitTime)
		{
			yield return null;
		}
		Vector3 Offset = new Vector3(0f, m_MoveSpeed, 0f);
		while (true)
		{
			if (!m_IsScroll && !m_IsPress)
			{
				m_ItemParent.transform.localPosition += Offset;
				if (m_ClipPanel.transform.localPosition.y + m_ItemParent.transform.localPosition.y - m_ContentHeight > m_TopPos.localPosition.y)
				{
					ResetClipPanel();
					Vector3 curPos = m_ItemParent.transform.localPosition;
					curPos.y = m_BottomPos.localPosition.y;
					m_ItemParent.transform.localPosition = curPos;
				}
			}
			yield return null;
		}
	}
}
