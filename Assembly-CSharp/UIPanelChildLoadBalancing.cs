using System.Collections.Generic;
using UnityEngine;

public class UIPanelChildLoadBalancing : MonoBehaviour
{
	[Header("子UIPanel最大数量")]
	[SerializeField]
	private int m_ChildPanelMaxCount = 20;

	[SerializeField]
	[Header("每个子UIPanel最大的子数量")]
	private int m_ChildPanelChildMaxCount = 10;

	[Header("每个子UIPanel的子数量正常饱和度")]
	[SerializeField]
	private float m_ChildPanelChildSaturability = 0.8f;

	[SerializeField]
	[Header("检测频率")]
	private float m_CheckTime = 0.3f;

	[Header("优先使用新UIPanel")]
	[SerializeField]
	private bool m_FirstUseNewPanel = true;

	private List<UIPanel> m_Panels = new List<UIPanel>();

	private float m_StartTime;

	private int m_SaturabilityCount;

	private void Awake()
	{
		m_StartTime = Time.realtimeSinceStartup;
		m_SaturabilityCount = Mathf.CeilToInt((float)m_ChildPanelChildMaxCount * m_ChildPanelChildSaturability);
	}

	private void Update()
	{
		if (Time.realtimeSinceStartup - m_StartTime > m_CheckTime)
		{
			CheckChildInfo();
			m_StartTime = Time.realtimeSinceStartup;
		}
	}

	private void CheckChildInfo()
	{
		List<Transform> list = new List<Transform>();
		if (base.transform.childCount > 0)
		{
			for (int i = 0; i < base.transform.childCount; i++)
			{
				Transform child = base.transform.GetChild(i);
				if (null == child.GetComponent<UIPanel>() && null != child.GetComponentInChildren<UIWidget>())
				{
					list.Add(child);
				}
			}
		}
		if (list.Count <= 0)
		{
			return;
		}
		for (int j = 0; j < list.Count; j++)
		{
			UIPanel uIPanel = null;
			if (m_Panels.Count < m_ChildPanelMaxCount)
			{
				if (m_FirstUseNewPanel)
				{
					uIPanel = AddNewPanel();
				}
				else
				{
					uIPanel = FindMinChildPanel();
					if (uIPanel.transform.childCount >= m_SaturabilityCount)
					{
						uIPanel = AddNewPanel();
					}
				}
			}
			else
			{
				uIPanel = FindMinChildPanel();
			}
			if (null != uIPanel)
			{
				list[j].gameObject.name = list[j].gameObject.name + "_" + uIPanel.transform.childCount;
				list[j].parent = uIPanel.transform;
				if (uIPanel.transform.childCount == m_SaturabilityCount)
				{
					Debug.LogFormat("<color=blue>{0} 已经进入饱和状态</color>", base.gameObject.name);
				}
			}
		}
	}

	private UIPanel AddNewPanel()
	{
		GameObject gameObject = new GameObject("UIPanel_" + m_Panels.Count);
		gameObject.transform.parent = base.transform;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localScale = Vector3.one;
		gameObject.transform.localRotation = Quaternion.identity;
		UIPanel uIPanel = gameObject.AddComponent<UIPanel>();
		m_Panels.Add(uIPanel);
		return uIPanel;
	}

	private UIPanel FindMinChildPanel()
	{
		UIPanel result = null;
		int num = int.MaxValue;
		if (m_Panels.Count > 0)
		{
			for (int i = 0; i < m_Panels.Count; i++)
			{
				if (m_Panels[i].transform.childCount < num)
				{
					num = m_Panels[i].transform.childCount;
					result = m_Panels[i];
				}
			}
		}
		return result;
	}
}
