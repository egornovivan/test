using System.Collections.Generic;
using UnityEngine;

public class VCEUIAssetList : MonoBehaviour
{
	public UIPanel m_Panel;

	public float m_OriginY;

	public float m_YReserve = 487f;

	public float ClipCenterX = 80f;

	public float ClipSizeX = 250f;

	public GameObject m_ItemGroup;

	public GameObject m_ItemRes;

	protected List<GameObject> m_AssetItems;

	public virtual void Init()
	{
		ClearItems();
		m_AssetItems = new List<GameObject>();
	}

	public void OnEnable()
	{
		RepositionList();
	}

	protected void ClearItems()
	{
		if (m_AssetItems == null)
		{
			return;
		}
		foreach (GameObject assetItem in m_AssetItems)
		{
			assetItem.SetActive(value: false);
			assetItem.transform.parent = null;
			Object.Destroy(assetItem);
		}
		m_AssetItems.Clear();
	}

	protected void Update()
	{
		if (m_YReserve != 0f)
		{
			float num = (float)Screen.height - m_YReserve;
			m_Panel.clipRange = new Vector4(ClipCenterX, (0f - num) * 0.5f - m_Panel.transform.localPosition.y + m_OriginY, ClipSizeX, num);
		}
		else
		{
			Vector4 clipRange = m_Panel.clipRange;
			m_Panel.clipRange = new Vector4(ClipCenterX, clipRange.y, ClipSizeX, clipRange.w);
		}
	}

	public void RepositionGrid()
	{
		Invoke("_RepositionGrid", 0f);
	}

	private void _RepositionGrid()
	{
		m_ItemGroup.GetComponent<UIGrid>().Reposition();
	}

	public void RepositionList()
	{
		Invoke("_RepositionList", 0.05f);
	}

	private void _RepositionList()
	{
		Vector3 localPosition = m_Panel.transform.localPosition;
		localPosition.y = m_OriginY;
		m_Panel.transform.localPosition = localPosition;
	}
}
