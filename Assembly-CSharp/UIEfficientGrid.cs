using System.Collections.Generic;
using UnityEngine;

public class UIEfficientGrid : MonoBehaviour
{
	public enum Arrangement
	{
		Horizontal,
		Vertical
	}

	public delegate void DSetContent(int index, GameObject go);

	public delegate void DClearContent(GameObject go);

	public GoPool itemGoPool;

	public GameObject itemPrefab;

	public UIPanel panel;

	public Arrangement arrangement;

	public float cellWidth = 58f;

	public float cellHeight = 200f;

	public bool repositionPosNow;

	public bool repositionVisibleNow;

	private List<GameObject> m_Items = new List<GameObject>();

	private float _oldClipRangeX = -10000f;

	private float _oldClipRangeY = -10000f;

	public List<GameObject> Gos => m_Items;

	public void UpdateList(int count, DSetContent setContent, DClearContent clearContent)
	{
		if (count > m_Items.Count)
		{
			for (int i = 0; i < m_Items.Count; i++)
			{
				Transform transform = m_Items[i].transform;
				transform.localPosition = new Vector3(0f, (float)(-i) * cellHeight, 0f);
				setContent?.Invoke(i, m_Items[i]);
			}
			for (int j = m_Items.Count; j < count; j++)
			{
				GameObject gameObject = CreateGo();
				gameObject.transform.localPosition = new Vector3(0f, (float)(-j) * cellHeight, 0f);
				setContent?.Invoke(j, gameObject);
				m_Items.Add(gameObject);
			}
		}
		else
		{
			for (int k = 0; k < count; k++)
			{
				Transform transform2 = m_Items[k].transform;
				transform2.localPosition = new Vector3(0f, (float)(-k) * cellHeight, 0f);
				setContent?.Invoke(k, m_Items[k]);
			}
			for (int num = m_Items.Count - 1; num >= count; num--)
			{
				clearContent?.Invoke(m_Items[num]);
				DestroyGo(m_Items[num]);
			}
			m_Items.RemoveRange(count, m_Items.Count - count);
		}
		repositionPosNow = true;
		repositionVisibleNow = true;
		UIDraggablePanel uIDraggablePanel = NGUITools.FindInParents<UIDraggablePanel>(base.gameObject);
		if (uIDraggablePanel != null)
		{
			uIDraggablePanel.UpdateScrollbars(recalculateBounds: true);
		}
	}

	public void UpdateList(int count, IListReceiver receiver)
	{
		if (receiver == null)
		{
			Debug.LogError("The receiver is null");
			return;
		}
		if (count > m_Items.Count)
		{
			for (int i = 0; i < m_Items.Count; i++)
			{
				Transform transform = m_Items[i].transform;
				transform.localPosition = new Vector3(0f, (float)(-i) * cellHeight, 0f);
				receiver.SetContent(i, m_Items[i]);
			}
			for (int j = m_Items.Count; j < count; j++)
			{
				GameObject gameObject = CreateGo();
				gameObject.transform.localPosition = new Vector3(0f, (float)(-j) * cellHeight, 0f);
				receiver.SetContent(j, gameObject);
				m_Items.Add(gameObject);
			}
		}
		else
		{
			for (int k = 0; k < count; k++)
			{
				Transform transform2 = m_Items[k].transform;
				transform2.localPosition = new Vector3(0f, (float)(-k) * cellHeight, 0f);
				receiver.SetContent(k, m_Items[k]);
			}
			for (int num = m_Items.Count - 1; num >= count; num--)
			{
				receiver.ClearContent(m_Items[num]);
				DestroyGo(m_Items[num]);
			}
			m_Items.RemoveRange(count, m_Items.Count - count);
		}
		repositionPosNow = true;
		repositionVisibleNow = true;
		UIDraggablePanel uIDraggablePanel = NGUITools.FindInParents<UIDraggablePanel>(base.gameObject);
		if (uIDraggablePanel != null)
		{
			uIDraggablePanel.UpdateScrollbars(recalculateBounds: true);
		}
	}

	public void Reposition()
	{
		if (arrangement == Arrangement.Vertical)
		{
			for (int i = 0; i < m_Items.Count; i++)
			{
				m_Items[i].transform.localPosition = new Vector3(0f, (float)(-i) * cellHeight, 0f);
			}
		}
		else if (arrangement == Arrangement.Horizontal)
		{
			for (int j = 0; j < m_Items.Count; j++)
			{
				m_Items[j].transform.localPosition = new Vector3((float)j * cellWidth, 0f, 0f);
			}
		}
	}

	public void RepositionVisible()
	{
		Vector4 clipRange = panel.clipRange;
		float num = clipRange.y + clipRange.w / 2f + cellHeight * 5f;
		float num2 = clipRange.y - cellHeight * 5f;
		float num3 = clipRange.x - clipRange.w / 2f - cellHeight * 5f;
		float num4 = clipRange.x + clipRange.w / 2f + cellHeight * 5f;
		for (int i = 1; i < m_Items.Count - 1; i++)
		{
			GameObject gameObject = m_Items[i];
			Vector3 localPosition = GetLocalPosition(gameObject.transform);
			if (localPosition.y > num || localPosition.y < num2 || localPosition.x < num3 || localPosition.x > num4)
			{
				if (gameObject.activeInHierarchy)
				{
					gameObject.SetActive(value: false);
				}
			}
			else if (!gameObject.activeInHierarchy)
			{
				gameObject.SetActive(value: true);
			}
		}
		_oldClipRangeX = clipRange.x;
		_oldClipRangeY = clipRange.y;
	}

	private GameObject CreateGo()
	{
		if (itemGoPool != null)
		{
			return itemGoPool.GetGo(base.transform, show: false);
		}
		GameObject gameObject = Object.Instantiate(itemPrefab);
		gameObject.transform.parent = base.transform;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Quaternion.identity;
		gameObject.transform.localScale = Vector3.one;
		return gameObject;
	}

	private void DestroyGo(GameObject go)
	{
		if ((bool)go)
		{
			if (itemGoPool != null)
			{
				itemGoPool.GiveBackGo(go, hide: true);
			}
			else
			{
				Object.Destroy(go);
			}
		}
	}

	private void Awake()
	{
	}

	private void Update()
	{
		if (panel != null)
		{
			Vector4 clipRange = panel.clipRange;
			if (clipRange.y != _oldClipRangeY || clipRange.x != _oldClipRangeX)
			{
				repositionVisibleNow = true;
				_oldClipRangeY = clipRange.y;
				_oldClipRangeX = clipRange.x;
			}
		}
	}

	private void LateUpdate()
	{
		if (repositionPosNow)
		{
			Reposition();
			repositionPosNow = false;
		}
		if (repositionVisibleNow)
		{
			RepositionVisible();
			repositionVisibleNow = false;
		}
		if (m_Items.Count != 0)
		{
			m_Items[0].SetActive(value: true);
			m_Items[m_Items.Count - 1].SetActive(value: true);
		}
	}

	private Vector3 GetLocalPosition(Transform trans)
	{
		Vector3 localPosition = trans.localPosition;
		Transform parent = trans.parent;
		while (parent != panel.transform)
		{
			if (parent == null)
			{
				Debug.LogError("This item is not ");
				return trans.localPosition;
			}
			localPosition += parent.localPosition;
			parent = parent.parent;
		}
		return localPosition;
	}
}
