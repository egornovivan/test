using System.Collections.Generic;
using UnityEngine;
using WhiteCat;

public class VCEUIPartDescList : MonoBehaviour
{
	public UITable m_Table;

	public VCEUIPartDescItem m_Res;

	public List<VCEUIPartDescItem> m_Items;

	private int m_DirtyCounter;

	public Transform m_BGTrans;

	public void SetDirty()
	{
		m_Table.Reposition();
		m_DirtyCounter = 2;
	}

	public void SyncList(List<VCPart> list_target)
	{
		if (list_target == null)
		{
			Clear();
			return;
		}
		List<VCEUIPartDescItem> list = new List<VCEUIPartDescItem>();
		foreach (VCEUIPartDescItem item in m_Items)
		{
			if (!list_target.Remove(item.m_PartProp))
			{
				list.Add(item);
			}
		}
		foreach (VCEUIPartDescItem item2 in list)
		{
			Remove(item2);
		}
		foreach (VCPart item3 in list_target)
		{
			Add(item3);
		}
	}

	public void Add(VCPart part_prop)
	{
		if (!Exists(part_prop))
		{
			VCEUIPartDescItem vCEUIPartDescItem = Object.Instantiate(m_Res);
			vCEUIPartDescItem.transform.parent = base.transform;
			vCEUIPartDescItem.transform.localScale = Vector3.one;
			vCEUIPartDescItem.gameObject.name = part_prop.gameObject.name;
			vCEUIPartDescItem.Set(part_prop);
			m_Items.Add(vCEUIPartDescItem);
			SetDirty();
		}
	}

	public void Clear()
	{
		foreach (VCEUIPartDescItem item in m_Items)
		{
			item.gameObject.SetActive(value: false);
			item.transform.parent = null;
			Object.Destroy(item.gameObject);
		}
		m_Items.Clear();
		SetDirty();
	}

	public bool Exists(VCPart part_prop)
	{
		foreach (VCEUIPartDescItem item in m_Items)
		{
			if (item.m_PartProp == part_prop)
			{
				return true;
			}
		}
		return false;
	}

	public bool Remove(VCPart part_prop)
	{
		VCEUIPartDescItem vCEUIPartDescItem = null;
		foreach (VCEUIPartDescItem item in m_Items)
		{
			if (item.m_PartProp == part_prop)
			{
				vCEUIPartDescItem = item;
				item.gameObject.SetActive(value: false);
				item.transform.parent = null;
				Object.Destroy(item.gameObject);
			}
		}
		if (vCEUIPartDescItem != null)
		{
			m_Items.Remove(vCEUIPartDescItem);
			SetDirty();
			return true;
		}
		return false;
	}

	public bool Remove(VCEUIPartDescItem item)
	{
		item.gameObject.SetActive(value: false);
		item.transform.parent = null;
		Object.Destroy(item.gameObject);
		bool result = m_Items.Remove(item);
		SetDirty();
		return result;
	}

	private void Awake()
	{
		m_Items = new List<VCEUIPartDescItem>();
	}

	private void Update()
	{
		if (m_DirtyCounter > 0)
		{
			m_Table.Reposition();
			m_DirtyCounter--;
		}
		if (m_Items.Count > 0)
		{
			m_BGTrans.gameObject.SetActive(value: true);
			Vector3 localScale = m_BGTrans.localScale;
			localScale.y = m_Table.mVariableHeight + 8f;
			m_BGTrans.localScale = localScale;
		}
		else
		{
			m_BGTrans.gameObject.SetActive(value: false);
		}
	}
}
