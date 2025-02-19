using System.Collections.Generic;
using Pathea;
using UnityEngine;

public class IsoUploadInfoWndCtrl : MonoBehaviour
{
	public enum UploadState
	{
		None,
		UploadPreViewFile,
		UploadIsoFile,
		SharingIso,
		OthePlayerDownload,
		ExportComplated,
		ExportFailed,
		NotEnoughMaterials
	}

	[SerializeField]
	private UIGrid m_Grid;

	[SerializeField]
	private IsoUploadItem_N m_ItemPrefab;

	[SerializeField]
	private UIScrollBar m_VScrollBar;

	[SerializeField]
	private UIButton m_HideAndShowBtn;

	[SerializeField]
	private TweenPosition m_Tween;

	private Dictionary<int, IsoUploadItem_N> m_ItemDic = new Dictionary<int, IsoUploadItem_N>();

	private bool m_ToShow;

	public static IsoUploadInfoWndCtrl Instance { get; private set; }

	private void Awake()
	{
		if (PeGameMgr.IsMulti)
		{
			Instance = this;
			Init();
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void Init()
	{
		UIEventListener.Get(m_HideAndShowBtn.gameObject).onClick = HideAndShowBtnClick;
		m_Tween.onFinished = TweenFinishEvent;
		m_ToShow = false;
		UpdateHideAndShowBtnDirection();
	}

	private void HideAndShowBtnClick(GameObject go)
	{
		if (null != m_Tween)
		{
			m_Tween.Play(!m_ToShow);
			m_HideAndShowBtn.isEnabled = false;
		}
	}

	private void TweenFinishEvent(UITweener tween)
	{
		m_ToShow = !m_ToShow;
		UpdateHideAndShowBtnDirection();
		m_HideAndShowBtn.isEnabled = true;
	}

	private void UpdateHideAndShowBtnDirection()
	{
		m_HideAndShowBtn.transform.rotation = Quaternion.Euler((!m_ToShow) ? new Vector3(0f, 0f, 180f) : Vector3.zero);
	}

	private IsoUploadItem_N GetNewItem()
	{
		GameObject gameObject = Object.Instantiate(m_ItemPrefab.gameObject);
		gameObject.transform.parent = m_Grid.transform;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localScale = Vector3.one;
		gameObject.transform.localRotation = Quaternion.identity;
		return gameObject.GetComponent<IsoUploadItem_N>();
	}

	private void Reposition()
	{
		m_Grid.Reposition();
		if (m_VScrollBar.foreground.gameObject.activeSelf)
		{
			m_VScrollBar.scrollValue = 1f;
		}
		else
		{
			m_VScrollBar.scrollValue = 0f;
		}
	}

	private void DeleteItem(int id)
	{
		if (m_ItemDic.ContainsKey(id))
		{
			IsoUploadItem_N isoUploadItem_N = m_ItemDic[id];
			m_ItemDic.Remove(id);
			Object.Destroy(isoUploadItem_N.gameObject);
			Invoke("Reposition", 0.1f);
		}
	}

	public void UpdateIsoState(int id, string isoName, int step)
	{
		UploadState step2 = ((step >= 0 && step <= 7) ? ((UploadState)step) : UploadState.None);
		if (m_ItemDic.ContainsKey(id))
		{
			m_ItemDic[id].UpdateStep(step2);
			return;
		}
		IsoUploadItem_N newItem = GetNewItem();
		newItem.DelEvent = DeleteItem;
		newItem.UpdateInfo(id, isoName, step2);
		m_ItemDic.Add(id, newItem);
		Reposition();
	}

	public void ClearAll()
	{
		if (m_ItemDic.Count <= 0)
		{
			return;
		}
		foreach (KeyValuePair<int, IsoUploadItem_N> item in m_ItemDic)
		{
			if (null != item.Value)
			{
				Object.Destroy(item.Value.gameObject);
			}
		}
		m_ItemDic.Clear();
	}
}
