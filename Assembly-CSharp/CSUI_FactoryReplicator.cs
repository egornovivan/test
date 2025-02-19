using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSUI_FactoryReplicator : MonoBehaviour
{
	[Serializable]
	public class LeftContent
	{
		public UIPopupList popList;

		public UIEfficientGrid gridList;

		public UIScrollBox scrollBox;

		public UIPanel scrollBoxPanel;

		public List<UICheckbox> menuCBs;
	}

	[Serializable]
	public class MiddleContent
	{
		public UIGraphControl graphCtrl;

		public UIScrollBox graphScrollBox;

		public UIGrid queryItemRoot;

		public GameObject queryItemPrefab;

		public UIScrollBox queryScrollBox;

		public UIButton queryLeftBtn;

		public UIButton queryRightBtn;

		public float duration = 1f;

		public UIInput queryInput;

		public UIButton querySearchBtn;

		public UIButton queryClearBtn;
	}

	[Serializable]
	public class RightContent
	{
		public GameObject gridPrefab;

		public Transform gridRoot;

		public UIInput countInput;

		public UIButton addCountBtn;

		public UIButton subCountBtn;

		public UIButton maxCountBtn;

		public UIButton minCountBtn;

		public UIButton compoundBtn;
	}

	public delegate void DelegateType_0(GameObject go);

	public delegate void DelegateType_1(GameObject go, int index);

	public delegate void DelegateType_2(string selection);

	public delegate void DelegateType_3(object history);

	public delegate int DelegateType_4(int count);

	private const int c_GraphHistoryCont = 20;

	public LeftContent m_MenuContent;

	public DelegateType_1 onMenuBtnClick;

	public DelegateType_2 onMenueSelect;

	public MiddleContent m_MiddleContent;

	public DelegateType_3 onGraphUseHistory;

	public DelegateType_0 onQueryLeftBtnClick;

	public DelegateType_0 onQueryRightBtnClick;

	public DelegateType_0 onQuerySearchBtnClick;

	public DelegateType_0 onQueryClearBtnClick;

	private List<object> m_GraphHistory = new List<object>();

	private int m_HistoryIndex = -1;

	private bool m_QueryScrolling;

	private float m_Factor;

	private float m_prvVal;

	private float m_TweenLen;

	private UIDraggablePanel m_QueryDraggablePanel;

	public RightContent m_RightContent;

	public DelegateType_4 onCountIputChanged;

	public DelegateType_0 onCompoundBtnClick;

	private void OnMenuCBClick(GameObject go)
	{
		int num = m_MenuContent.menuCBs.FindIndex((UICheckbox item0) => item0.gameObject == go);
		if (num == -1)
		{
			Debug.LogError("Cant find the gameobject in menu check box.");
		}
		else if (onMenuBtnClick != null)
		{
			onMenuBtnClick(go, num);
		}
	}

	private void OnMenuSelectionChange(string selection)
	{
		if (onMenueSelect != null)
		{
			onMenueSelect(selection);
		}
	}

	public int GetMenuListItemCount()
	{
		return m_MenuContent.gridList.Gos.Count;
	}

	public GameObject GetMenuListItemGo(int index)
	{
		return m_MenuContent.gridList.Gos[index];
	}

	public bool SetMenuCBChecked(int index, bool check)
	{
		if (index < -1 || index >= m_MenuContent.menuCBs.Count)
		{
			return false;
		}
		m_MenuContent.menuCBs[index].isChecked = true;
		return true;
	}

	public void ClearGraph()
	{
		m_MiddleContent.graphCtrl.ClearGraph();
	}

	public void DrawGraph()
	{
		m_MiddleContent.graphCtrl.DrawGraph();
	}

	public void AddGraphHistory(object history)
	{
		if (m_GraphHistory.Count >= 20)
		{
			m_GraphHistory.RemoveAt(0);
		}
		m_GraphHistory.Add(history);
		m_HistoryIndex = m_GraphHistory.Count - 2;
	}

	public GameObject InstantiateQueryItem(string desc)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(m_MiddleContent.queryItemPrefab);
		gameObject.transform.parent = m_MiddleContent.queryItemRoot.transform;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Quaternion.identity;
		gameObject.transform.localScale = Vector3.one;
		m_MiddleContent.queryItemRoot.repositionNow = true;
		return gameObject;
	}

	public void DestroyQueryItems()
	{
		m_QueryDraggablePanel.ResetPosition();
		Transform transform = m_MiddleContent.queryItemRoot.transform;
		int num = 0;
		while (num < transform.childCount)
		{
			UnityEngine.Object.Destroy(transform.GetChild(num).gameObject);
			transform.GetChild(num).parent = null;
		}
	}

	public string GetQueryString()
	{
		UIInput queryInput = m_MiddleContent.queryInput;
		queryInput.text = queryInput.text.Trim();
		string text = queryInput.text;
		text = text.Replace("*", string.Empty);
		text = text.Replace("$", string.Empty);
		text = text.Replace("(", string.Empty);
		text = text.Replace(")", string.Empty);
		text = text.Replace("@", string.Empty);
		text = text.Replace("^", string.Empty);
		text = text.Replace("[", string.Empty);
		text = text.Replace("]", string.Empty);
		return queryInput.text = text.Replace("\\", string.Empty);
	}

	public bool IsQueryInputValid()
	{
		string text = m_MiddleContent.queryInput.text;
		if (text.Length > 0)
		{
			return true;
		}
		return false;
	}

	private void OnGraphBackBtnClick()
	{
		if (m_HistoryIndex >= m_GraphHistory.Count)
		{
			m_HistoryIndex = m_GraphHistory.Count - 1;
		}
		if (m_HistoryIndex >= 0)
		{
			if (onGraphUseHistory != null)
			{
				onGraphUseHistory(m_GraphHistory[m_HistoryIndex]);
			}
			m_HistoryIndex--;
		}
	}

	private void OnGraphForwadBtnClick()
	{
		if (m_HistoryIndex < 0)
		{
			m_HistoryIndex = 1;
		}
		if (m_HistoryIndex < m_GraphHistory.Count)
		{
			if (onGraphUseHistory != null)
			{
				onGraphUseHistory(m_GraphHistory[m_HistoryIndex]);
			}
			m_HistoryIndex++;
		}
	}

	private void OnQueryLeftBtnClick(GameObject go)
	{
		if (!m_QueryScrolling)
		{
			m_QueryScrolling = true;
			UIPanel draggablePanel = m_MiddleContent.queryScrollBox.m_DraggablePanel;
			if (draggablePanel.clipping == UIDrawCall.Clipping.SoftClip)
			{
				m_TweenLen = m_MiddleContent.queryScrollBox.m_DraggablePanel.clipRange.z - draggablePanel.clipSoftness.x * 0.5f;
			}
			else
			{
				m_TweenLen = m_MiddleContent.queryScrollBox.m_DraggablePanel.clipRange.z;
			}
			m_Factor = 0f;
			m_prvVal = 0f;
			if (onQueryLeftBtnClick == null)
			{
			}
		}
	}

	private void OnQueryRightBtnClick(GameObject go)
	{
		if (!m_QueryScrolling)
		{
			m_QueryScrolling = true;
			UIPanel draggablePanel = m_MiddleContent.queryScrollBox.m_DraggablePanel;
			if (draggablePanel.clipping == UIDrawCall.Clipping.SoftClip)
			{
				m_TweenLen = 0f - (m_MiddleContent.queryScrollBox.m_DraggablePanel.clipRange.z - draggablePanel.clipSoftness.x * 0.5f);
			}
			else
			{
				m_TweenLen = 0f - m_MiddleContent.queryScrollBox.m_DraggablePanel.clipRange.z;
			}
			m_Factor = 0f;
			m_prvVal = 0f;
			if (onQueryRightBtnClick == null)
			{
			}
		}
	}

	private void OnQuerySearchBtnClick(GameObject go)
	{
		if (onQuerySearchBtnClick != null)
		{
			onQuerySearchBtnClick(go);
		}
	}

	private void OnQueryClearBtnClick(GameObject go)
	{
		if (onQueryClearBtnClick != null)
		{
			onQueryClearBtnClick(go);
		}
	}

	public GameObject InstantiateGridItem(string desc)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(m_RightContent.gridPrefab);
		gameObject.transform.parent = m_RightContent.gridRoot;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Quaternion.identity;
		gameObject.transform.localScale = Vector3.one;
		m_MiddleContent.queryItemRoot.repositionNow = true;
		return gameObject;
	}

	private void OnCountInputSelected(GameObject go, bool isSelect)
	{
		if (!isSelect)
		{
			int value = 1;
			if (CSUtils.IsNumber(m_RightContent.countInput.text))
			{
				value = int.Parse(m_RightContent.countInput.text);
			}
			UIGraphControl graphCtrl = m_MiddleContent.graphCtrl;
			if (graphCtrl.rootNode != null)
			{
				value = Mathf.Clamp(value, graphCtrl.rootNode.ms.m_productItemCount, graphCtrl.GetMaxCount());
				UpdateInputCount(value, immediateUpdateInputTxet: false);
			}
		}
	}

	private void UpdateInputCount(int count, bool immediateUpdateInputTxet = true)
	{
		if (onCountIputChanged != null)
		{
			count = onCountIputChanged(count);
		}
		if (immediateUpdateInputTxet)
		{
			m_RightContent.countInput.text = count.ToString();
		}
		else
		{
			StartCoroutine(WiatUpdateInputIterator(count));
		}
	}

	private IEnumerator WiatUpdateInputIterator(int count)
	{
		yield return null;
		m_RightContent.countInput.text = count.ToString();
	}

	private void OnAddBtnClick(GameObject go)
	{
		if (!CSUtils.IsNumber(m_RightContent.countInput.text))
		{
			return;
		}
		int num = int.Parse(m_RightContent.countInput.text);
		UIGraphControl graphCtrl = m_MiddleContent.graphCtrl;
		if (graphCtrl.rootNode != null && num < graphCtrl.GetMaxCount())
		{
			if (num + graphCtrl.rootNode.ms.m_productItemCount <= graphCtrl.GetMaxCount())
			{
				num += graphCtrl.rootNode.ms.m_productItemCount;
			}
			UpdateInputCount(num);
		}
	}

	private void OnSubstractBtnClick(GameObject go)
	{
		if (!CSUtils.IsNumber(m_RightContent.countInput.text))
		{
			return;
		}
		int num = int.Parse(m_RightContent.countInput.text);
		UIGraphControl graphCtrl = m_MiddleContent.graphCtrl;
		if (graphCtrl.rootNode != null)
		{
			if (num > graphCtrl.rootNode.ms.m_productItemCount)
			{
				num -= graphCtrl.rootNode.ms.m_productItemCount;
			}
			UpdateInputCount(num);
		}
	}

	private void OnMinBtnClick(GameObject go)
	{
		UIGraphControl graphCtrl = m_MiddleContent.graphCtrl;
		int minCount = graphCtrl.GetMinCount();
		UpdateInputCount(minCount);
	}

	private void OnMaxBtnClick(GameObject go)
	{
		UIGraphControl graphCtrl = m_MiddleContent.graphCtrl;
		int maxCount = graphCtrl.GetMaxCount();
		UpdateInputCount(maxCount);
	}

	private void OnCompoundBtnClick(GameObject go)
	{
		if (onCompoundBtnClick != null)
		{
			onCompoundBtnClick(go);
		}
	}

	private void Awake()
	{
		UIEventListener uIEventListener = null;
		for (int i = 0; i < m_MenuContent.menuCBs.Count; i++)
		{
			uIEventListener = UIEventListener.Get(m_MenuContent.menuCBs[i].gameObject);
			uIEventListener.onClick = OnMenuCBClick;
		}
		uIEventListener = UIEventListener.Get(m_MiddleContent.queryLeftBtn.gameObject);
		uIEventListener.onClick = OnQueryLeftBtnClick;
		uIEventListener = UIEventListener.Get(m_MiddleContent.queryRightBtn.gameObject);
		uIEventListener.onClick = OnQueryRightBtnClick;
		uIEventListener = UIEventListener.Get(m_MiddleContent.querySearchBtn.gameObject);
		uIEventListener.onClick = OnQuerySearchBtnClick;
		uIEventListener = UIEventListener.Get(m_MiddleContent.queryClearBtn.gameObject);
		uIEventListener.onClick = OnQueryClearBtnClick;
		uIEventListener = UIEventListener.Get(m_RightContent.addCountBtn.gameObject);
		uIEventListener.onClick = OnAddBtnClick;
		uIEventListener = UIEventListener.Get(m_RightContent.minCountBtn.gameObject);
		uIEventListener.onClick = OnMinBtnClick;
		uIEventListener = UIEventListener.Get(m_RightContent.maxCountBtn.gameObject);
		uIEventListener.onClick = OnMaxBtnClick;
		uIEventListener = UIEventListener.Get(m_RightContent.subCountBtn.gameObject);
		uIEventListener.onClick = OnSubstractBtnClick;
		uIEventListener = UIEventListener.Get(m_RightContent.compoundBtn.gameObject);
		uIEventListener.onClick = OnCompoundBtnClick;
		uIEventListener = UIEventListener.Get(m_RightContent.countInput.gameObject);
		uIEventListener.onSelect = OnCountInputSelected;
		m_QueryDraggablePanel = m_MiddleContent.queryScrollBox.m_DraggablePanel.gameObject.GetComponent<UIDraggablePanel>();
	}

	private void Start()
	{
	}

	private void Update()
	{
		UIDraggablePanel queryDraggablePanel = m_QueryDraggablePanel;
		UIPanel draggablePanel = m_MiddleContent.queryScrollBox.m_DraggablePanel;
		Bounds bounds = queryDraggablePanel.bounds;
		Vector2 vector = bounds.min;
		Vector2 vector2 = bounds.max;
		if (draggablePanel.clipping == UIDrawCall.Clipping.SoftClip)
		{
			Vector2 clipSoftness = draggablePanel.clipSoftness;
			vector -= clipSoftness;
			vector2 += clipSoftness;
		}
		if (vector2.x > vector.x)
		{
			Vector4 clipRange = draggablePanel.clipRange;
			float num = clipRange.z * 0.5f;
			float num2 = clipRange.x - num - bounds.min.x;
			float num3 = bounds.max.x - num - clipRange.x;
			m_MiddleContent.queryLeftBtn.isEnabled = num2 > 0f;
			m_MiddleContent.queryRightBtn.isEnabled = num3 > 0f;
		}
		else
		{
			m_MiddleContent.queryLeftBtn.isEnabled = false;
			m_MiddleContent.queryRightBtn.isEnabled = false;
		}
		if (m_QueryScrolling)
		{
			float num4 = Mathf.Abs((!(m_MiddleContent.duration > 0f)) ? 1000f : (1f / m_MiddleContent.duration));
			m_Factor += num4 * Time.deltaTime;
			if (m_Factor > 1f)
			{
				m_Factor = 1f;
				m_QueryScrolling = false;
			}
			float num5 = m_Factor - Mathf.Sin(m_Factor * ((float)Math.PI * 2f)) / ((float)Math.PI * 2f);
			Vector3 vector3 = new Vector3(m_TweenLen * Mathf.Clamp01(num5 - m_prvVal), 0f, 0f);
			queryDraggablePanel.transform.localPosition += vector3;
			Vector4 clipRange2 = draggablePanel.clipRange;
			clipRange2.x -= vector3.x;
			clipRange2.y -= vector3.y;
			draggablePanel.clipRange = clipRange2;
			m_prvVal = num5;
		}
		else
		{
			if (m_Factor != 0f)
			{
				Vector3 localPosition = queryDraggablePanel.transform.localPosition;
				localPosition.x = Mathf.RoundToInt(localPosition.x);
				localPosition.y = Mathf.RoundToInt(localPosition.y);
				localPosition.z = Mathf.RoundToInt(localPosition.z);
				queryDraggablePanel.transform.localPosition = localPosition;
				Vector4 clipRange3 = draggablePanel.clipRange;
				clipRange3.x = Mathf.RoundToInt(clipRange3.x);
				clipRange3.y = Mathf.RoundToInt(clipRange3.y);
				draggablePanel.clipRange = clipRange3;
			}
			m_Factor = 0f;
			m_prvVal = 0f;
			m_TweenLen = 0f;
		}
	}
}
