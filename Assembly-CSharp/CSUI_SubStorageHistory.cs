using System.Collections.Generic;
using UnityEngine;

public class CSUI_SubStorageHistory : MonoBehaviour
{
	public delegate void RepositionDel();

	public int Day;

	[SerializeField]
	private UITable m_TableUI;

	[SerializeField]
	private UILabel m_HistoryLbPrefab;

	private List<GameObject> m_HistoryObjs = new List<GameObject>();

	public RepositionDel onReposition;

	private int repositionNow_cnt;

	public bool IsEmpty => m_HistoryObjs.Count == 0;

	public void AddHistory(string history)
	{
		UILabel uILabel = Object.Instantiate(m_HistoryLbPrefab);
		uILabel.transform.parent = m_TableUI.transform;
		uILabel.transform.localPosition = Vector3.zero;
		uILabel.transform.localRotation = Quaternion.identity;
		uILabel.MakePixelPerfect();
		uILabel.text = history;
		m_HistoryObjs.Add(uILabel.gameObject);
		repositionNow_cnt = 2;
	}

	public void PopImmediate()
	{
		if (m_HistoryObjs.Count != 0)
		{
			Object.DestroyImmediate(m_HistoryObjs[0]);
			m_HistoryObjs.RemoveAt(0);
			m_TableUI.repositionNow = true;
		}
	}

	private void OnReposition()
	{
		Transform transform = m_TableUI.transform;
		transform.localPosition = new Vector3(transform.localPosition.x, 0f - m_TableUI.mVariableHeight, transform.position.y);
		if (onReposition != null)
		{
			onReposition();
		}
	}

	private void Awake()
	{
		m_TableUI.onReposition = OnReposition;
	}

	private void OnDestroy()
	{
		if (onReposition != null)
		{
			onReposition();
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (repositionNow_cnt > 0)
		{
			m_TableUI.repositionNow = true;
			repositionNow_cnt--;
		}
	}

	private void LateUpdate()
	{
	}
}
